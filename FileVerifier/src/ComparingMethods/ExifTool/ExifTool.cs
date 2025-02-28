using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using AvaloniaDraft.Helpers;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AvaloniaDraft.ComparingMethods.ExifTool;

public class ImageMetadata
{

    public string SourceFile { get; set; } = "";
    public Dictionary<string, object> ExifTool { get; set; } = new();
    public Dictionary<string, object> File { get; set; } = new();
    
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();
}

public sealed class ExifTool : IDisposable
{
    private bool _isLoaded = false;
    private readonly Process _exifProcess = new Process();
    private readonly object _processLock = new();
    private readonly string _terminal = OperatingSystem.IsWindows() ? "cmd" : "/bin/bash"; //Assuming the app will never start on MacOS
    private int _disposed = 0; //Int so that Interlocked Exchange can be used

    public void Dispose()
    {
        //Making sure called only once
        if (Interlocked.Exchange(ref _disposed, 1) == 1) return; 
        
        lock (_processLock)
        {
            _disposed = 1;
            Stop();
            _exifProcess.Dispose();
        }
        
        GC.SuppressFinalize(this); //Prevents destructor from running
    }
    
    ~ExifTool() => Dispose();
    
    private string? RunExiftoolStayingOpen(string[] filenames, bool group = true)
    {
        lock (_processLock)
        {
            //Start it not already started
            if (!_isLoaded)
            {
                var cmds = GlobalVariables.ExifPath + " -stay_open true -@ -";
                
                var psi = new ProcessStartInfo(_terminal, $"/c \"{cmds}\"")
                {
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                _exifProcess.StartInfo = psi;
                _exifProcess.ErrorDataReceived += new DataReceivedEventHandler(ExifErrorHandler); //Error handler

                _exifProcess.Start();
                _exifProcess.BeginErrorReadLine();
            
                _isLoaded = true;
            }

            var outputTask = System.Threading.Tasks.Task.Run(() =>
            {
                var outputBuilder = new StringBuilder();
                
                _exifProcess.StandardInput.WriteLine("-j");
                if(group) _exifProcess.StandardInput.WriteLine("-g");
                _exifProcess.StandardInput.WriteLine(string.Join("\n", filenames));
                _exifProcess.StandardInput.WriteLine("-execute");
                _exifProcess.StandardInput.Flush();

                string? line;
                while ((line = _exifProcess.StandardOutput.ReadLine()) != null)
                {
                    if (line.Contains("{ready}"))
                        break;

                    outputBuilder.AppendLine(line);
                    Debug.WriteLine(outputBuilder.ToString());
                }

                return outputBuilder.ToString();
            });

            //Something went wrong. Shutdown process so that it can be restarted and return null.
            if (!outputTask.Wait(TimeSpan.FromSeconds(5)))
            {
                Stop();
                return null;
            }
            
            return outputTask.Result;
        }
    }

    private void Stop()
    {
        if(!_isLoaded) return;
        
        //Trying to shut down gracefully, when forced terminate the process
        try
        {
            _exifProcess.StandardInput.WriteLine("-stay_open");
            _exifProcess.StandardInput.WriteLine("false");
            _exifProcess.StandardInput.Flush();

            if (!_exifProcess.WaitForExit(TimeSpan.FromSeconds(2)))
            {
                _exifProcess.Kill();
            }
        }
        catch
        {
            _exifProcess.Kill();
        }
        finally
        {
            _isLoaded = false;
        }
    }
    
    private static void ExifErrorHandler(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Debug.WriteLine(e.Data);
        }
    }
    
    public List<Dictionary<string, object>>? GetExifDataDictionary(string[] filenames, bool group = true)
    {
        var output = RunExiftoolStayingOpen(filenames, group);

        if (output == null) return null;
        
        List<Dictionary<string, object>>? metadata;
        try
        {
            metadata =  JsonSerializer.Deserialize<List<Dictionary<string, object>>>(output);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing exiftool output: {e.Message}");
            return null;
        }

        return metadata;

    }
    
    public List<ImageMetadata>? GetExifDataImageMetadata(string[] files)
    {
        var output = RunExiftoolStayingOpen(files);
        
        if (output == null) return null;
        
        List<ImageMetadata>? metadata;
        try
        {
            metadata = JsonConvert.DeserializeObject<List<ImageMetadata>>(output);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing exiftool output: {e.Message}");
            return null;
        }
        
        return metadata;
    }

    public static string? GetExifPath()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                return curDir + @"\FileVerifier\src\ComparingMethods\ExifTool\exiftool.exe";
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        

        return null;
    } 
}