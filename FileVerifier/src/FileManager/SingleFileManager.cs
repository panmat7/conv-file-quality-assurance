using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Text.Json;
using System.Threading;
using AvaloniaDraft.ExtractionPipelines;
using AvaloniaDraft.Helpers;
using DocumentFormat.OpenXml.Wordprocessing;

namespace AvaloniaDraft.FileManager;


public class SingleFile
{
    public string FilePath { get; set; } = string.Empty;
    public string FileFormat { get; set; } = string.Empty;
    public bool Done { get; set; } = false;
    public bool InProcess { get; set; } = false;
    
    public void StartProcess() => InProcess = true;
    public void UpdateDone() => Done = true;
}

/// <summary>
/// Used to write the extraction reports, make to mirror the design of FileManager. Work on the same principles.
/// </summary>
public class SingleFileManager
{
    private readonly string _inputDirectory;
    private List<SingleFile> _files;
    private readonly IFileSystem _fileSystem;
    
    //Threading
    private int _currentThreads = 0;
    private readonly object _lock = new();
    private readonly object _listLock = new();
    private readonly List<Thread> _threads = new();
    
    //Report
    private Dictionary<string, Dictionary<string, string>> _results = new();
    private readonly object _resultsLock = new();

    public SingleFileManager(string inputDirectory, IFileSystem? fileSystem = null)
    {
        _inputDirectory = inputDirectory;
        _files = new List<SingleFile>();
        _fileSystem = fileSystem ?? new FileSystem();

        foreach (var file in _fileSystem.Directory.GetFiles(inputDirectory))
        {
            _files.Add(new SingleFile { FilePath = file });
        }
        
        Siegfried.GetFileFormats(_inputDirectory, ref _files);
    }

    /// <summary>
    /// Starts the extraction process
    /// </summary>
    public void StartProcessing()
    {
        var maxThreads = GlobalVariables.Options.SpecifiedThreadCount ?? 8;
        var _startTime = DateTime.Now;
        
        
        while (true)
        {
            lock (_lock)
            {
                Console.WriteLine($"Using {_currentThreads} threads of {maxThreads} threads.");
                
                if (_currentThreads < maxThreads)
                {
                    var file = _files.FirstOrDefault((p) => !p.InProcess && !p.Done);
                    if (file == null) break; //We are done, everything either in progress or done
                    
                    file.StartProcess();
                    Console.WriteLine($"Done with {_files.IndexOf(file)} files out of {_files.Count} files.");

                    if (SelectAndStartDataExtraction(file))
                    {
                        Console.WriteLine($"THREAD STARTED");
                        _currentThreads += 1; //Main thread + additional assigned
                    }
                    else
                    {
                        UiControlService.Instance.AppendToConsole(
                            $"Data extraction from {file.FileFormat} not supported.");
                    }
                    
                }
            }
            
            Thread.Sleep(150);
        }
    
        AwaitThreads(); //Awaiting all remaining threads
        UiControlService.Instance.AppendToConsole("\n" + $@"Extraction completed in {(DateTime.Now - _startTime):hh\:mm\:ss}." + "\n");
    }

    /// <summary>
    /// Gets the data extraction pipeline and starts it
    /// </summary>
    /// <param name="file">File the pipeline is to be started for</param>
    /// <returns>True if a pipeline was found, false is none were found.</returns>
    private bool SelectAndStartDataExtraction(SingleFile file)
    {
        var pipeline = BaseExtraction.SelectPipeline(file.FileFormat);
        
        if(pipeline == null) return false;

        var thread = new Thread(() =>
        {
            try
            {
                var res = pipeline(file, () => _currentThreads--, file.UpdateDone);
                
                _results[file.FilePath] = res ?? new Dictionary<string, string>{{"Status", "FAILED"}};
            }
            catch
            {
                lock(_resultsLock)
                    _results[file.FilePath] = new Dictionary<string, string>{ {"Status", "FAILED"} };
            }
            finally
            {
                lock (_listLock)
                    _threads.Remove(Thread.CurrentThread);
            }
        });
        
        lock(_listLock) _threads.Add(thread);
        thread.Start();
        
        return true;
    }

    /// <summary>
    /// Awaits all remaining threads
    /// </summary>
    private void AwaitThreads()
    {
        List<Thread> toAwait;
        
        lock(_listLock) toAwait = new List<Thread>(_threads);
        
        foreach (var thread in toAwait) thread.Join();
        
        lock(_listLock) _threads.Clear();
    }

    /// <summary>
    /// Write the extracted data to a file.
    /// </summary>
    public void WriteReport()
    {
        var outputDir = _fileSystem.Directory.GetCurrentDirectory() + @"\extraction-reports";
        _fileSystem.Directory.CreateDirectory(outputDir);
        
        var reportName = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".json";
        string jsonOutput;
        lock (_resultsLock) //Just to get rid of the warning, should not actually need the lock as the process is done
        {
            jsonOutput = JsonSerializer.Serialize(_results);
        }
        
        var filePath = _fileSystem.Path.Combine(outputDir, reportName);

        using var stream = _fileSystem.File.Create(filePath);
        using var writer = new StreamWriter(stream);
        writer.Write(jsonOutput);
    }
}
