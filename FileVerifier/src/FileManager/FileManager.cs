using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
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

    public FilePair(string oFilePath, string nFilePath)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = "";
        NewFilePath = nFilePath;
        NewFileFormat = "";
        Done = false;
    }

    public FilePair(string oFilePath, string oFileFormat, string nFilePath, string newFileFormat)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = oFileFormat;
        NewFilePath = nFilePath;
        NewFileFormat = newFileFormat;
        Done = false;
    }

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

/// <summary>
/// Class <c>FileManager</c> is responsible for file handling and pairing before the verification process
/// </summary>
public class FileManager
{
    private readonly string oDirectory;
    private readonly string nDirectory;
    private List<FilePair> filePairs;
    private readonly List<string> pairlessFiles;
    private readonly IFileSystem _fileSystem;
    private int filesDone = 0;
    
    //Theading
    private int CurrentThreads = 0;
    private static readonly object _lock = new object();

    public List<FilePair> GetFilePairs() => filePairs;
    public List<string> GetPairlessFiles() => pairlessFiles;
    
    public FileManager(string originalDirectory, string newDirectory, IFileSystem? fileSystem = null)
    {
        _fileSystem = fileSystem ?? new FileSystem();
        oDirectory = originalDirectory;
        nDirectory = newDirectory;
        
        filePairs = new List<FilePair>();
        pairlessFiles = new List<string>();
        
        var originalFiles = _fileSystem.Directory.GetFiles(oDirectory, "*", SearchOption.AllDirectories).ToList();
        var newFiles = _fileSystem.Directory.GetFiles(nDirectory, "*", SearchOption.AllDirectories).ToList();
        
        //Check for number of files here? Like, we probably don't want to run 1 000 000 files...
        
        //If any file name appears more than once - inform
        if (originalFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() != originalFiles.Count)
            throw new Exception("FILENAME DUPLICATES IN ORIGINAL DIRECTORY");
        
        if (newFiles.Select(_fileSystem.Path.GetFileNameWithoutExtension).Distinct().Count() != newFiles.Count)
            throw new Exception("FILENAME DUPLICATES IN NEW DIRECTORY");
        
        foreach (var iFile in originalFiles)
        {
            try
            {
                //Creating the file-to-file dictionary, getting first result of outputfiles containing file name 
                var oFile = newFiles.First(f => f.Contains(_fileSystem.Path.GetFileNameWithoutExtension(iFile)));
                filePairs.Add(new FilePair(iFile, "", oFile, ""));
            }
            catch
            {
                pairlessFiles.Add(iFile);
            }
        }
        
        //Adding all files that do not have a pair from newfiles to pairless
        pairlessFiles.AddRange(newFiles.FindAll(f => !filePairs.Select(fp => fp.NewFilePath).Contains(f)));
    }
    
    /// <summary>
    /// Calls Siegfried to identify format of files in both directories
    /// </summary>
    public void GetSiegfriedFormats()
    {
        Siegfried.GetFileFormats(oDirectory, nDirectory, ref filePairs);
    }

    public void StartVerification()
    {
        var maxThreads = 8; //Read from options
        var fileCount = filePairs.Count;
        
        //Continuing untill done with all files
        while (filesDone < fileCount)
        {
            lock (_lock)
            {
                if (CurrentThreads < maxThreads)
                {
                    var assigned = GetAdditionalThreadCount(filePairs[filesDone]);
                    if (maxThreads < CurrentThreads + (1 + assigned))
                        assigned = maxThreads - CurrentThreads - 1; //Making sure we dont create too many threads

                    if (SelectAndStartPipeline(filePairs[filesDone], assigned))
                    {
                        CurrentThreads += (1 + assigned); //Main thread + additional assigned
                    }
                    else
                    {
                        //TODO: Log error, not supported conversion
                    }
                    
                }
            }
            
            Thread.Sleep(150);
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
        Action<FilePair, int>? pipeline = null;
        
        if (FormatCodes.PronomCodesPNG.Contains(pair.OriginalFileFormat))
        {
            pipeline = VerificationPipelines.GetPNGPipelines(pair.NewFileFormat);
        }

        if (pipeline == null) return false;
        
        pipeline(pair, assigned);
        return true;
    }

    /// <summary>
    /// Calculates the number of threads that should be assigned to a file verification process. 
    /// </summary>
    /// <param name="filePair">The pair of files</param>
    /// <returns>The recommended number of additional threads</returns>
    private int GetAdditionalThreadCount(FilePair filePair)
    {
        //TODO: The actual calculation
        return 0;
    }

    public void TestStartThreads()
    {
        var maxThreads = 8; //Read from options later
        var fileCount = filePairs.Count;
        
        Console.WriteLine($"MAXTHREADS: {maxThreads}");
        Console.WriteLine($"FILES TO DO: {fileCount}");
        
        while (filesDone < fileCount)
        {
            Console.WriteLine($"Files Done: {filesDone}/{fileCount}. Using Threads: {CurrentThreads}. Available Threads: {maxThreads - CurrentThreads}.");

            lock (_lock)
            {
                if (CurrentThreads < maxThreads)
                {
                    var assigned = 0;
                    if (new Random().Next() % 2 == 0)
                    {
                        assigned = new Random().Next(1, 4);
                        if (maxThreads < CurrentThreads + (1 + assigned))
                        {
                            assigned = maxThreads - CurrentThreads - 1;
                        }
                    }
                    
                    CurrentThreads += (1 + assigned);
                    var thread = new Thread(ThreadTest);
                    thread.Start(assigned);
                }
            }
            
            Thread.Sleep(100);
        }
        
        Console.WriteLine($"Done with {filePairs.Count} pairs.");
    }

    private void ThreadTest(object assignedThreads)
    {
        var assigned = (int)assignedThreads;
        
        try
        {
            Console.WriteLine($"Thread #{CurrentThreads} started");
            var random = new Random();
            var randomInterval = random.Next(500, 1750);
            Thread.Sleep(randomInterval);
            Console.WriteLine(
                $"Thread #{CurrentThreads} finished after {randomInterval} ms, returning {assigned} threads");
        }
        finally
        {
            lock (_lock)
            {
                CurrentThreads -= (assigned + 1);
                filesDone++;
            }
        }
    }
    
    /// <summary>
    /// Writes all files pairs and pairless files to standard output
    /// </summary>
    public void WritePairs()
    {
        Console.WriteLine("PAIRS:");
        foreach (var pair in filePairs)
        {
            Console.WriteLine($"{pair.OriginalFilePath} ({pair.OriginalFileFormat}) - {pair.NewFilePath} ({pair.NewFileFormat})");
        }
        Console.WriteLine("PAIRLESS");
        foreach (var file in pairlessFiles)
        {
            Console.WriteLine($"{file}");
        }
    }
}