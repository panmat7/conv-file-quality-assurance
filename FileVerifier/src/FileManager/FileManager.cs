using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace AvaloniaDraft.FileManager;

public class FileManager
{
    private Dictionary<Tuple<string, string>, Tuple<string, string>> filePairs;
    private List<string> pairlessFiles;

    FileManager(string input, string output)
    {
        filePairs = new Dictionary<Tuple<string, string>, Tuple<string, string>>();
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
                filePairs.Add(new Tuple<string, string>(iFile, Path.GetExtension(iFile)), new Tuple<string, string>(oFile, Path.GetFileName(oFile)));
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