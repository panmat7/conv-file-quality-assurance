using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Abstractions;
using System.Linq;

namespace AvaloniaDraft.FileManager;

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

    public override bool Equals(object obj)
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

public class FileManager
{
    private readonly string oDirectory;
    private readonly string nDirectory;
    private List<FilePair> filePairs;
    private List<string> pairlessFiles;
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
        
        var originalFiles = _fileSystem.Directory.GetFiles(oDirectory).ToList();
        var newFiles = _fileSystem.Directory.GetFiles(nDirectory).ToList();
        
        //If any file name appears more than once - inform
        if (originalFiles.Select(_fileSystem.Path.GetFileName).Distinct().Count() != originalFiles.Count)
            throw new Exception("FILENAME DUPLICATES IN ORIGINAL DIRECTORY");
        
        if (newFiles.Select(_fileSystem.Path.GetFileName).Distinct().Count() != newFiles.Count)
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
    
    public void GetSiegfriedFormats()
    {
        Siegfried.GetFileFormats(oDirectory, nDirectory, ref filePairs);
    }
    
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