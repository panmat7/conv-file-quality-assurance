using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

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

    public FilePair(string oFilePath, string nFilePath)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = "";
        NewFilePath = nFilePath;
        NewFileFormat = "";
        
    }

    public FilePair(string oFilePath, string oFileFormat, string nFilePath, string newFileFormat)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = oFileFormat;
        NewFilePath = nFilePath;
        NewFileFormat = newFileFormat;
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