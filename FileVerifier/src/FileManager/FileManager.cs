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
    //Key - Original file 
    private List<FilePair> filePairs;
    private List<string> pairlessFiles;

    FileManager(string input, string output)
    {
        filePairs = new List<FilePair>();
        pairlessFiles = new List<string>();
        
        var inputFiles = Directory.GetFiles(input);
        var outputFiles = Directory.GetFiles(output);
        
        //If any file name appears more than once - inform
        if (inputFiles.Select(Path.GetFileName).Distinct().Count() != inputFiles.Length)
            throw new Exception("FILENAME DUPLICATES IN INPUT");
        
        if (outputFiles.Select(Path.GetFileName).Distinct().Count() != outputFiles.Length)
            throw new Exception("FILENAME DUPLICATES IN OUTPUT");
        
        foreach (var iFile in inputFiles)
        {
            try
            {
                //Creating the file-to-file dictionary, getting first result of outputfiles containing file name 
                var oFile = outputFiles.First(f => f.Contains(Path.GetFileName(iFile)));
                filePairs.Add(new FilePair(iFile, Path.GetExtension(iFile), oFile, Path.GetFileName(oFile)));
            }
            catch
            {
                pairlessFiles.Add(iFile);
            }
        }
    }

    void VerifyFormatSiegfried()
    {
        //TODO: Use Siegfried to verify signature
    }
}