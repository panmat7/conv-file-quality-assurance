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

/// <summary>
/// Used to store metadata of image files.
/// </summary>
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
    
    /// <summary>
    /// Disposes of the Exiftool object
    /// </summary>
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
    
    /// <summary>
    /// Executes an Exiftool command. If the process is not already running in the background, starts a new one.
    /// </summary>
    /// <param name="filenames">Name of the files exiftool is to check.</param>
    /// <param name="group">Whether the group tag is to be used.</param>
    /// <returns>The ExifTool output as a string. Null if an error occured.</returns>
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
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardInputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };

                _exifProcess.StartInfo = psi;
                _exifProcess.ErrorDataReceived += ExifErrorHandler; //Error handler

                _exifProcess.Start();
                _exifProcess.BeginErrorReadLine();
                
                _exifProcess.StandardInput.WriteLine("-execute"); //Doing this to clear the std input
            
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
    
    /// <summary>
    /// Tries to gracefully stop the process. Force terminates if necessary. 
    /// </summary>
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
    
    /// <summary>
    /// The error handler.
    /// </summary>
    /// <param name="sender">Sender object.</param>
    /// <param name="e">Error event data.</param>
    private static void ExifErrorHandler(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            Debug.WriteLine(e.Data);
        }
    }
    
    /// <summary>
    /// Gets ExifTool output data for a set of files and returns it as a dictionary.
    /// </summary>
    /// <param name="filenames">Files to be checked, in form of their absolute paths.</param>
    /// <param name="group">Whether the group tag is to be used.</param>
    /// <returns>Exif data as string-object dictionary. Null if an error occured.</returns>
    public List<Dictionary<string, object>>? GetExifDataDictionary(string[] filenames, bool group = true)
    {
        if (_disposed == 1) return null;

        string? output;

        try { output = RunExiftoolStayingOpen(filenames, group); }
        catch { return null; }

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
    
    /// <summary>
    /// Gets ExifTool output data for a set of files and returns it as ImageMetadata objects.
    /// </summary>
    /// <param name="files">Files to be checked, in form of their absolute paths.</param>
    /// <returns>Exif data as an list of ImageMetadata objects. Null if an error occured.</returns>
    public List<ImageMetadata>? GetExifDataImageMetadata(string[] files)
    {
        if(_disposed == 1) return null;

        string? output;
        
        try { output = RunExiftoolStayingOpen(files); }
        catch { return null; }
        
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
    
    /// <summary>
    /// Tries to find the absolute path to the exiftool executable.
    /// </summary>
    /// <returns></returns>
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