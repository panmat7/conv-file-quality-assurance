using System;
using System.Collections.Generic;
using System.IO;
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
        OriginalFileFormat = Path.GetExtension(oFilePath);
        NewFilePath = nFilePath;
        NewFileFormat = Path.GetExtension(nFilePath);
        
    }

    public FilePair(string oFilePath, string oFileFormat, string nFilePath, string newFileFormat)
    {
        OriginalFilePath = oFilePath;
        OriginalFileFormat = oFileFormat;
        NewFilePath = nFilePath;
        NewFileFormat = newFileFormat;
    }
}

public class FileManager
{
    private List<FilePair> filePairs;
    private List<string> pairlessFiles;

    public FileManager(string originalDirectory, string newDirectory)
    {
        filePairs = new List<FilePair>();
        pairlessFiles = new List<string>();
        
        var originalFiles = Directory.GetFiles(originalDirectory).ToList();
        var newFiles = Directory.GetFiles(newDirectory).ToList();
        
        //If any file name appears more than once - inform
        if (originalFiles.Select(Path.GetFileName).Distinct().Count() != originalFiles.Count)
            throw new Exception("FILENAME DUPLICATES IN ORIGINAL DIRECTORY");
        
        if (newFiles.Select(Path.GetFileName).Distinct().Count() != newFiles.Count)
            throw new Exception("FILENAME DUPLICATES IN NEW DIRECTORY");
        
        foreach (var iFile in originalFiles)
        {
            try
            {
                //Creating the file-to-file dictionary, getting first result of outputfiles containing file name 
                var oFile = newFiles.First(f => f.Contains(Path.GetFileNameWithoutExtension(iFile)));
                filePairs.Add(new FilePair(iFile, "", oFile, ""));
            }
            catch
            {
                pairlessFiles.Add(iFile);
            }
        }
        
        //Adding all files that do not have a pair from newfiles to pairless
        pairlessFiles.AddRange(newFiles.FindAll(f => !filePairs.Select(fp => fp.NewFilePath).Contains(f)));
        
        Siegfried.GetFileFormats(originalDirectory, newDirectory, ref filePairs);
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