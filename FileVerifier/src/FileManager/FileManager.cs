using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Abstractions;
using System.Linq;
using System.Threading;
using Avalonia.Threading;
using AvaloniaDraft.ComparingMethods;
using AvaloniaDraft.ComparisonPipelines;
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

    public FilePair() { }

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
    AlreadyChecked,
    Corrupted,
    EncryptedOrCorrupted,
    UnsupportedFormat,
    Unknown,
    None
}

/// <summary>
/// Class <c>FileManager</c> is responsible for file handling and pairing before the verification process and
/// starting the process itself.
/// </summary>
public sealed class FileManager
{
    private readonly string _oDirectory;
    private readonly string _nDirectory;
    private readonly string _tempODirectory;
    private readonly string _tempNDirectory;
    private readonly string _checkpoint;
    internal List<IgnoredFile> IgnoredFiles { get; set; }
    private List<FilePair> _filePairs;
    private readonly List<string> _pairlessFiles;
    private readonly IFileSystem _fileSystem;
    
    public List<string> GetPairlessFiles() => _pairlessFiles;
    public List<FilePair> GetFilePairs() => _filePairs;
    public IFileSystem GetFilesystem() => _fileSystem;
    
    //Threading
    private int _currentThreads = 0;
    private static readonly object Lock = new object();
    private static readonly object ListLock = new object();
    private readonly List<Thread> _threads = [];
    
    //Progress reporting
    private DateTime _startTime;
    
    public FileManager(string originalDirectory, string newDirectory, List<FilePair> checkpointFilePairs, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        _oDirectory = originalDirectory;
        _nDirectory = newDirectory;

        IgnoredFiles = [];
        
        _filePairs = new List<FilePair>();
        _pairlessFiles = new List<string>();
        var fileDuplicates = new List<string>();
        
        _tempODirectory = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), _fileSystem.Path.GetRandomFileName());
        _tempNDirectory = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), _fileSystem.Path.GetRandomFileName());
        _fileSystem.Directory.CreateDirectory(_tempODirectory);
        _fileSystem.Directory.CreateDirectory(_tempNDirectory);
        
        ZipHelper.ExtractCompressedFiles(_oDirectory, _tempODirectory, this);
        ZipHelper.ExtractCompressedFiles(_nDirectory, _tempNDirectory, this);

        // Process files to work on
        var originalFiles = ProcessFiles(_oDirectory, _tempODirectory);
        var newFiles = ProcessFiles(_nDirectory, _tempNDirectory);
        
        //If any file name appears more than once - add to duplicate list
        if (originalFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() !=
            originalFiles.Count)
        {
            fileDuplicates.AddRange(
                originalFiles.GroupBy(x => _fileSystem.Path.GetFileNameWithoutExtension(x))
                    .Where(g => g.Count() > 1)
                    .SelectMany(g => g)
            );
        }
        
        if (newFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() != newFiles.Count)
            throw new InvalidOperationException("FILENAME DUPLICATES IN NEW DIRECTORY");

        //Lookup directory
        var newFileLookupDir = newFiles
            .ToDictionary(
                f => _fileSystem.Path.GetFileNameWithoutExtension(f),
                f => f
            );
        
        foreach (var oFile in originalFiles)
        {
            //If file with matching name is found, adding pair
            if (newFileLookupDir.TryGetValue(_fileSystem.Path.GetFileNameWithoutExtension(oFile), out var nFile))
            {
                var pair = new FilePair(iFile, "", oFile, "");

                if (!checkpointFilePairs.Any(fp => fp.OriginalFilePath == pair.OriginalFilePath 
                    && fp.NewFilePath == pair.NewFilePath))
                {
                    _filePairs.Add(pair);
                } 
                else
                {
                    var reason = ReasonForIgnoring.AlreadyChecked;
                    IgnoredFiles.Add(new IgnoredFile(iFile, reason));
                    IgnoredFiles.Add(new IgnoredFile(oFile, reason));
                }
            }
            else
            {
                //Checking if its one of the duplicates, if not - to pairless
                if (fileDuplicates.Contains(oFile)) //This is done to mimic the naming method used by the conversion tool
                {
                    //Constructing the name using the same method as the conversion tool
                    var constructedName = _fileSystem.Path.GetFileNameWithoutExtension(oFile) + "_" +
                                          _fileSystem.Path.GetExtension(oFile).TrimStart('.').ToUpper();
                    
                    //We have a match, create pair, otherwise add to pairless
                    if (newFileLookupDir.TryGetValue(constructedName, out var nFileMatch)) {
                        _filePairs.Add(new FilePair(oFile, "", nFileMatch, ""));
                    } else {
                        _pairlessFiles.Add(oFile);
                    }
                }
                else
                {
                    _pairlessFiles.Add(oFile);
                }
            }
        }
        
        //Adding all files that do not have a pair from newfiles to pairless
        _pairlessFiles.AddRange(newFiles.FindAll(f => !_filePairs.Select(fp => fp.NewFilePath).Contains(f)));
        
        // Register cleanup of temporary directories on application exit
        AppDomain.CurrentDomain.ProcessExit += (s, e) => CleanupTempDirectories(_tempODirectory, _tempNDirectory);
    }

    private List<string> ProcessFiles(string srcPath, string tempPath)
    {
        var files = _fileSystem.Directory.GetFiles(srcPath, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                if (ZipHelper.CompressedFilesExtensions.Contains("*" + _fileSystem.Path.GetExtension(f)))
                    return false;
                var reason = EncryptionChecker.CheckForEncryption(f);
                if (reason == ReasonForIgnoring.None) return true;
                IgnoredFiles.Add(new IgnoredFile(f, reason));
                return false;
            }).ToList();
        
        files.AddRange(_fileSystem.Directory.GetFiles(tempPath, "*", SearchOption.AllDirectories)
            .Where(f =>
            {
                if (ZipHelper.CompressedFilesExtensions.Contains("*" + _fileSystem.Path.GetExtension(f)))
                    return false;
                var reason = EncryptionChecker.CheckForEncryption(f);
                if (reason == ReasonForIgnoring.None) return true;
                IgnoredFiles.Add(new IgnoredFile(f, reason));
                return false;
            }).ToList());

        return files;
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
    /// Filter out file pairs containing file formats that is not to be checked
    /// </summary>
    public void FilterOutDisabledFileFormats()
    {
        var filteredOut = _filePairs.Where(fp => !GlobalVariables.Options.FormatsAreEnabled(fp)).ToList();
        _filePairs = _filePairs.Except(filteredOut).ToList();
        foreach (var fp in filteredOut)
        {
            var reason = ReasonForIgnoring.Filtered;
            IgnoredFiles.Add(new IgnoredFile(fp.OriginalFilePath, reason));
            IgnoredFiles.Add(new IgnoredFile(fp.NewFilePath, reason));
        }
    }

    /// <summary>
    /// Starts the verification process. Continues until all files are checked.
    /// </summary>
    public void StartVerification()
    {
        var maxThreads = GlobalVariables.Options.SpecifiedThreadCount ?? 8;
        _startTime = DateTime.Now;
        var timer = new Timer(WriteProgressToConsole, null, (int)TimeSpan.FromMinutes(5).TotalMilliseconds, 
            (int)TimeSpan.FromMinutes(5).TotalMilliseconds);


        var checkPointInterval = (int)TimeSpan.FromMinutes(15).TotalMilliseconds; // Change later based on settings
        var checkpointTimer = new Timer(_ => GlobalVariables.Logger.SaveReport(true), null,
            checkPointInterval, checkPointInterval);

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
                        UiControlService.Instance.AppendToConsole(
                            $"Convertion between {pair.OriginalFileFormat} and {pair.NewFileFormat} not supported.");
                    }
                    
                }
            }
            
            Thread.Sleep(150);
        }
    
        AwaitThreads(); //Awaiting all remaining threads
        UiControlService.Instance.AppendToConsole("\n" + $@"Verification completed in {(DateTime.Now - _startTime):hh\:mm\:ss}." + "\n");
        timer.Dispose();
        checkpointTimer.Dispose();
    }
    
    /// <summary>
    /// Selects and starts a verification pipeline based on 
    /// </summary>
    /// <param name="pair">Files to be compared</param>
    /// <param name="assigned">Additional thread budget assigned to the pipeline</param>
    /// <returns>False if no pipeline was found (meaning unsupported verification)</returns>
    private bool SelectAndStartPipeline(FilePair pair, int assigned)
    {
        //Get the correct pipeline based on pair formats
        var pipeline = BasePipeline.SelectPipeline(pair);
        
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
        // //Visual comparison may receive additional threads
        // if ((FormatCodes.PronomCodesPDF.Contains(filePair.OriginalFileFormat) ||
        //      FormatCodes.PronomCodesPDFA.Contains(filePair.OriginalFileFormat))
        //     && (FormatCodes.PronomCodesPDF.Contains(filePair.NewFileFormat) ||
        //         FormatCodes.PronomCodesPDFA.Contains(filePair.NewFileFormat)))
        // {
        //     var pageCount = ComperingMethods.GetPageCountExif(filePair.OriginalFilePath, filePair.OriginalFileFormat);
        //
        //     switch (pageCount)
        //     {
        //         case > 500:
        //             return 4;
        //         case > 250:
        //             return 3;
        //         case > 100:
        //             return 2;
        //     }
        // }
        
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
    /// Returns the path to temporary directories as (original, new).
    /// </summary>
    public (string, string) GetTempDirectories()
    {
        return (_tempODirectory, _tempNDirectory);
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
        var pronomFormat = new Dictionary<string, Tuple<string, int>>();

        foreach (var pair in _filePairs)
        {
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
    }
    
    /// <summary>
    /// Writes the current progress to console, used as a callback for a timer
    /// </summary>
    private void WriteProgressToConsole(object? state)
    {
        var filesDone = _filePairs.Count(p => p.Done);
        var time = DateTime.Now - _startTime;
        var estimate = TimeSpan.Zero;

        if (filesDone > 0)
        {
            estimate = (time / filesDone) * (filesDone - _filePairs.Count); 
        }
        
        var msg = $"Files completed: {filesDone}/{_filePairs.Count}, " +
            $@"time elapsed: {time:hh\:mm\:ss}. Estimated time to completion: {estimate:hh\:mm\:ss}";
        UiControlService.Instance.AppendToConsole(msg);
    }
    
    /// <summary>
    /// Returns a string detailing the formats for pairs.
    /// </summary>
    public string GetPairFormats()
    {
        var pairs = new Dictionary<string, int>();

        foreach (var pair in _filePairs)
        {
            try
            {
                var oCode = FormatCodes.AllCodes
                    .First(c => c.PronomCodes.Contains(pair.OriginalFileFormat))
                    .FormatCodes.FirstOrDefault() ?? "unk.";
                var nCode = FormatCodes.AllCodes
                    .First(c => c.PronomCodes.Contains(pair.NewFileFormat))
                    .FormatCodes.FirstOrDefault() ?? "unk.";

                var key = $"{oCode} - {nCode}";
                if (!pairs.TryAdd(key, 1))
                    pairs[key] += 1;
            }
            catch
            {
                var key = "unk.-unk.";
                if (!pairs.TryAdd(key, 1))
                    pairs[key] += 1;
            }
        }
        
        return string.Join("\n", pairs.Select(pair => $"{pair.Key}:  {pair.Value}"));
    }
}