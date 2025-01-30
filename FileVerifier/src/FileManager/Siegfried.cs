using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaDraft.FileManager;

public static class Siegfried
{
    private class SiegfriedOutputJson
    {
        public SiegfriedOutputJson(string version, List<SiegfriedFile> files)
        {
            Version = version;
            Files = files;
        }

        [JsonPropertyName("siegfried")] public string Version { get; set; }
        [JsonPropertyName("files")] public List<SiegfriedFile> Files { get; set; }
        
    }

    private class SiegfriedFile
    {
        public SiegfriedFile(string name, string errors, List<SiegfriedMatches> matches)
        {
            Name = name;
            Errors = errors;
            Matches = matches;
        }

        [JsonPropertyName("filename")] public string Name { get; set; }
        [JsonPropertyName("errors")] public string Errors { get; set; }
        [JsonPropertyName("matches")] public List<SiegfriedMatches> Matches { get; set; }
    }

    private class SiegfriedMatches
    {
        public SiegfriedMatches(string ns, string id)
        {
            this.ns = ns;
            this.id = id;
        }

        [JsonPropertyName("ns")] public string ns { get; set; }
        [JsonPropertyName("id")] public string id { get; set; }
    }
    
    public static void GetFileFormats(string originalDir, string newDir, ref List<FilePair> files)
    {
        //Using powershell to run siegfried (REQUIRES LOCAL INSTALLATION AND PRESENCE IN PATH)
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-ExecutionPolicy Bypass -Command \"sf -json {originalDir}; sf -json {newDir}\"",
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            RedirectStandardError = true,
        };

        using var process = new Process();
        process.StartInfo = psi;
        process.Start();
        
        //Currently error-prone - if one of the files is empty the entire json sequence if broken.
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        if (!string.IsNullOrEmpty(error)) throw new Exception(error);
        
        //Output currently contains two json object separated by a new line, needs to be split 
        var outputSep = output.Split("\n");
        
        //Should always result in array of length 3 - two JSON object and an empty string
        if (outputSep.Length != 3) throw new Exception("Invalid Siegfried output");
        
        var originalOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[0]);
        var newOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[1]);
        
        if(originalOutput == null || newOutput == null) throw new Exception("Invalid Siegfried output");
        
        //Removing files with no matches TODO: Log them
        originalOutput.Files = originalOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        newOutput.Files = newOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        
        //Assign files their format
        foreach (var file in files)
        {
            file.OriginalFileFormat = originalOutput.Files.First(
                    f => f.Name == file.OriginalFilePath)
                .Matches[0].id;
            
            file.NewFileFormat = newOutput.Files.First(
                f => f.Name == file.NewFilePath)
                .Matches[0].id;
        }
        
        process.WaitForExit();
    }
}