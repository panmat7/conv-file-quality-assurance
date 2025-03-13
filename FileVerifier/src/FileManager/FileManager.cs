using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.FileManager;

/// <summary>
/// Class <c>FilePair</c> is used to store the path and format of the two compared files
/// </summary>
public class FilePair
{
    public string OriginalFilePath { get; set; }
    public string OriginalFileFormat { get; set; }
    public string NewFilePath { get; set; }
    public string NewFileFormat { get; set; }
    public bool Done { get; set; }
    public bool InProcess { get; set; }

    public FilePair(string oFilePath, string nFilePath)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = "";
        NewFilePath = nFilePath;
        NewFileFormat = "";
        Done = false;
        InProcess = false;
    }

    public FilePair(string oFilePath, string oFileFormat, string nFilePath, string newFileFormat)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = oFileFormat;
        NewFilePath = nFilePath;
        NewFileFormat = newFileFormat;
        Done = false;
        InProcess = false;
    }
    
    public void UpdateDone() => Done = true;
    
    public void StartProcess() => InProcess = true;
    
    public override bool Equals(object? obj)
    {
        if (obj is FilePair fp)
        {
            return (OriginalFilePath == fp.OriginalFilePath) && (OriginalFileFormat == fp.OriginalFileFormat)
                && (NewFilePath == fp.NewFilePath) && (NewFileFormat == fp.NewFileFormat);
        }

        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(OriginalFilePath, OriginalFileFormat, NewFilePath, NewFileFormat);
    }
}

public class IgnoredFile
{
    public string FilePath { get; set; }
    public ReasonForIgnoring Reason { get; set; }

    public IgnoredFile(string filePath, ReasonForIgnoring reason = ReasonForIgnoring.Unknown)
    {
        FilePath = filePath;
        Reason = reason;
    }
    
}

public enum ReasonForIgnoring
{
    Encrypted,
    Filtered,
    UnsupportedFormat,
    Unknown
}

/// <summary>
/// Class <c>FileManager</c> is responsible for file handling and pairing before the verification process
/// </summary>
public sealed class FileManager
{
    private readonly string _oDirectory;
    private readonly string _nDirectory;
    private readonly string _tempODirectory;
    private readonly string _tempNDirectory;
    internal List<IgnoredFile> IgnoredFiles { get; set; }
    private List<FilePair> _filePairs;
    private readonly List<string> _pairlessFiles;
    private readonly IFileSystem _fileSystem;
    
    public List<string> GetPairlessFiles() => _pairlessFiles;
    public List<FilePair> GetFilePairs() => _filePairs;
    
    //Threading
    private int _currentThreads = 0;
    private static readonly object Lock = new object();
    private static readonly object ListLock = new object();
    private readonly List<Thread> _threads = [];
    
    public FileManager(string originalDirectory, string newDirectory, IFileSystem? fileSystem = null)
    {
        Console.WriteLine("Test1");
        _fileSystem = fileSystem ?? new FileSystem();
        _oDirectory = originalDirectory;
        _nDirectory = newDirectory;
        
        IgnoredFiles = [];
        
        _filePairs = [];
        _pairlessFiles = [];
        
        _tempODirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        _tempNDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
        Directory.CreateDirectory(_tempODirectory);
        Directory.CreateDirectory(_tempNDirectory);
        
        ZipHelper.ExtractCompressedFiles(_oDirectory, _tempODirectory, this);
        ZipHelper.ExtractCompressedFiles(_nDirectory, _tempNDirectory, this);
        
        var originalFiles = _fileSystem.Directory.GetFiles(_oDirectory, "*", SearchOption.AllDirectories)
            .Where(f => IgnoredFiles.All(ignored => ignored.FilePath != f) && !ZipHelper.CompressedFilesExtensions.Contains("*" + Path.GetExtension(f))).ToList();
        originalFiles.AddRange(_fileSystem.Directory.GetFiles(_tempODirectory, "*", SearchOption.AllDirectories)
            .Where(f => IgnoredFiles.All(ignored => ignored.FilePath != f) && !ZipHelper.CompressedFilesExtensions.Contains("*" + Path.GetExtension(f))).ToList());
        
        var newFiles = _fileSystem.Directory.GetFiles(_nDirectory, "*", SearchOption.AllDirectories)
            .Where(f => IgnoredFiles.All(ignored => ignored.FilePath != f) && !ZipHelper.CompressedFilesExtensions.Contains("*" + Path.GetExtension(f))).ToList();
        newFiles.AddRange(_fileSystem.Directory.GetFiles(_tempNDirectory, "*", SearchOption.AllDirectories)
            .Where(f => IgnoredFiles.All(ignored => ignored.FilePath != f) && !ZipHelper.CompressedFilesExtensions.Contains("*" + Path.GetExtension(f))).ToList());
        
        //Check for number of files here? Like, we probably don't want to run 1 000 000 files...
        
        //If any file name appears more than once - inform
        if (originalFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() != originalFiles.Count)
            throw new InvalidOperationException("FILENAME DUPLICATES IN ORIGINAL DIRECTORY");
        
        if (newFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() != newFiles.Count)
            throw new InvalidOperationException("FILENAME DUPLICATES IN NEW DIRECTORY");
        
        foreach (var iFile in originalFiles)
        {
            try
            {
                //Creating the file-to-file dictionary, getting first result of outputfiles containing file name 
                var oFile = newFiles.First(f => f.Contains(_fileSystem.Path.GetFileNameWithoutExtension(iFile)));
                _filePairs.Add(new FilePair(iFile, "", oFile, ""));
            }
            catch
            {
                _pairlessFiles.Add(iFile);
            }
        }
        
        //Adding all files that do not have a pair from newfiles to pairless
        _pairlessFiles.AddRange(newFiles.FindAll(f => !_filePairs.Select(fp => fp.NewFilePath).Contains(f)));
        
        // Register cleanup of temporary directories on application exit
        AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupTempDirectories(_tempODirectory, _tempNDirectory);
    }

    private static void CleanupTempDirectories(params string[] tempDirectories)
    {
        foreach (var tempDir in tempDirectories)
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
    
    /// <summary>
    /// Calls Siegfried to identify format of files in both directories
    /// </summary>
    public void GetSiegfriedFormats()
    {
        Siegfried.GetFileFormats(_oDirectory, _nDirectory, _tempODirectory, _tempNDirectory,  ref _filePairs);
    }
    
    /// <summary>
    /// Starts the verification process. Continues until all files are checked.
    /// </summary>
    public void StartVerification()
    {
        var maxThreads = 8; //Read from options
        
        //Continuing until done with all files
        while (true)
        {
            lock (Lock)
            {
                Console.WriteLine($"Using {_currentThreads} threads of {maxThreads} threads.");
                
                if (_currentThreads < maxThreads)
                {
                    var pair = _filePairs.FirstOrDefault((p) => !p.InProcess && !p.Done);
                    if (pair == null) break; //We are done, everything either in progress or done
                    
                    pair.StartProcess();
                    Console.WriteLine($"Done with {_filePairs.IndexOf(pair)} files out of {_filePairs.Count} files.");
                    
                    var assigned = GetAdditionalThreadCount(pair);
                    if (maxThreads < _currentThreads + (1 + assigned))
                        assigned = maxThreads - _currentThreads - 1; //Making sure we don't create too many threads

                    if (SelectAndStartPipeline(pair, assigned))
                    {
                        Console.WriteLine($"THREAD STARTED");
                        _currentThreads += (1 + assigned); //Main thread + additional assigned
                    }
                    else
                    {
                        //TODO: Log error, not supported conversion
                    }
                    
                }
            }
            
            Thread.Sleep(150);
        }

        AwaitThreads(); //Awaiting all remaining threads
        
        foreach (var file in _filePairs)
        {
            Console.WriteLine($"This file is verified: {file.Done}");
        }
    }
    
    /// <summary>
    /// Selects and starts a verification pipeline based on 
    /// </summary>
    /// <param name="pair">Files to be compared</param>
    /// <param name="assigned">Additional thread budget assigned to the pipeline</param>
    /// <returns>False if no pipeline was found (meaning unsupported verification)</returns>
    private bool SelectAndStartPipeline(FilePair pair, int assigned)
    {
        Action<FilePair, int, Action<int>, Action>? pipeline = null;
        
        //Get the correct pipeline
        if (FormatCodes.PronomCodesPNG.Contains(pair.OriginalFileFormat))
        {
            pipeline = PngPipelines.GetPNGPipelines(pair.NewFileFormat);
        }
        
        if (pipeline == null) return false; //None found
        
        var thread = new Thread(() =>
        {
            try
            {
                pipeline(pair, assigned, ReturnUsedThreadsAndFinishFile, () => pair.UpdateDone());
            }
            finally
            {
                lock (ListLock)
                {
                    _threads.Remove(Thread.CurrentThread); //When finished, remove from list
                }
            }
        });
        
        lock (ListLock) _threads.Add(thread); //Add the thread to list of currently active
        
        thread.Start();
        
        return true;
    }

    /// <summary>
    /// Calculates the number of threads that should be assigned to a file verification process. 
    /// </summary>
    /// <param name="filePair">The pair of files</param>
    /// <returns>The recommended number of additional threads</returns>
    private static int GetAdditionalThreadCount(FilePair filePair)
    {
        //TODO: The actual calculation
        return 0;
    }

    /// <summary>
    /// Safely updates current threads and signals file as done
    /// </summary>
    /// <param name="change">The change in thread count</param>
    private void ReturnUsedThreadsAndFinishFile(int change)
    {
        lock (Lock)
        {
            _currentThreads += change;
        }
    }
    
    /// <summary>
    /// Waits till all threads inside _threads finish
    /// </summary>
    private void AwaitThreads()
    {
        List<Thread> toAwait;
        
        lock (ListLock) toAwait = new List<Thread>(_threads);
        
        foreach (var t in toAwait)
        {
            t.Join();
        }
        
        lock (ListLock) _threads.Clear();
    }
    
    /// <summary>
    /// Writes all files pairs and pairless files to standard output
    /// </summary>
    public void WritePairs()
    {
        Console.WriteLine("PAIRS:");

        var pronomFormat = new Dictionary<string, Tuple<string, int>>();

        foreach (var pair in _filePairs)
        {
            Console.WriteLine($"{pair.OriginalFilePath} ({pair.OriginalFileFormat}) - {pair.NewFilePath} ({pair.NewFileFormat})");

            var extension = pair.OriginalFileFormat;
            var fileExtension = Path.GetExtension(pair.OriginalFilePath);

            if (pronomFormat.ContainsKey(extension))
            {
                var existingTuple = pronomFormat[extension];
                
                pronomFormat[extension] = new Tuple<string, int>(existingTuple.Item1, existingTuple.Item2 + 1);
            }
            else
            {
                pronomFormat.Add(extension, new Tuple<string, int>(fileExtension, 1));
            }
        }

        Console.WriteLine("PAIRLESS:");
        foreach (var file in _pairlessFiles)
        {
            Console.WriteLine($"{file}");
        }
        
        
        foreach (KeyValuePair<string, Tuple<string, int>> kvp in pronomFormat)
        {
            ConsoleService.Instance.WriteToConsole($"{kvp.Key}  -  {kvp.Value.Item1}  -  {kvp.Value.Item2}");
        }
    }

}