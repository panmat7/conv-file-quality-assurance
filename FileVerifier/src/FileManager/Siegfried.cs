using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AvaloniaDraft.FileManager;

/// <summary>
/// The <c>Siegfried</c> class is responsible for handing communication with Siegfried
/// </summary>
public static class Siegfried
{
    //Three classes used to parse siegfried output
    private sealed class SiegfriedOutputJson(string version, List<SiegfriedFile> files)
    {
        [JsonPropertyName("siegfried")] public string Version { get; set; } = version;
        [JsonPropertyName("files")] public List<SiegfriedFile> Files { get; set; } = files;
    }

    private sealed class SiegfriedFile(string name, string errors, List<SiegfriedMatches> matches)
    {
        [JsonPropertyName("filename")] public string Name { get; set; } = name;
        [JsonPropertyName("errors")] public string Errors { get; set; } = errors;
        [JsonPropertyName("matches")] public List<SiegfriedMatches> Matches { get; set; } = matches;
    }

    private sealed class SiegfriedMatches(string ns, string id)
    {
        [JsonPropertyName("ns")] public string ns { get; set; } = ns;
        [JsonPropertyName("id")] public string id { get; set; } = id;
    }
    
    /// <summary>
    /// Method <c>GetFileFormats</c> is to called run siegfried on two directories and assign PRONOM formats to files
    /// </summary>
    /// <param name="originalDir">The directory containing the original files</param>
    /// <param name="newDir">The directory contacting the newly converted files</param>
    /// <param name="files">The list of file pairs from both directories</param>
    public static void GetFileFormats(string originalDir, string newDir, ref List<FilePair> files)
    {
        string terminal = "";
        string arguments = "";
        string windowsTerminal = "powershell.exe";
        string linuxTerminal = "/bin/bash";
        string windowsArguments = $"-ExecutionPolicy Bypass -Command \"sf -json {originalDir}; sf -json {newDir}\"";
        string linuxArguments = $"-c \"sf -json {originalDir}; sf -json {newDir}\"";
        
        if (OperatingSystem.IsWindows())
        {
            terminal = windowsTerminal;
            arguments = windowsArguments;
        }
        else if (OperatingSystem.IsLinux())
        {
            terminal = linuxTerminal;
            arguments = linuxArguments;
        }

        //Using powershell to run siegfried (REQUIRES LOCAL INSTALLATION AND PRESENCE IN PATH)
        var processInfo = new ProcessStartInfo
        {
            FileName = terminal,
            Arguments = arguments,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            RedirectStandardError = true,
        };

        using var process = new Process();
        process.StartInfo = processInfo;
       
        try { process.Start(); }
        catch(Exception ex)
        {
            throw new Exception($"Unable to start powershell.exe and Siegfried: {ex.Message}");
        }
        
        //Currently error-prone - if one of the files is empty the entire json sequence if broken.
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();
        
        if (!string.IsNullOrEmpty(error)) throw new Exception(error);
        
        //Output currently contains two json object separated by a new line, needs to be split 
        var outputSep = output.Split("\n");
        outputSep = outputSep.Where(x => !string.IsNullOrEmpty(x)).ToArray();
        
        //Should always result in array of length 2 - a JSON object per command
        if (outputSep.Length != 2) throw new Exception("Invalid Siegfried output");
        
        var originalOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[0]);
        var newOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[1]);
        
        if(originalOutput == null || newOutput == null) throw new Exception("Invalid Siegfried output");
        
        //Removing files with no matches TODO: Log them
        originalOutput.Files = originalOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        newOutput.Files = newOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        
        //Assign files their format
        foreach (var file in files)
        {
            Console.WriteLine($"{file.OriginalFilePath} - {file.NewFilePath}");
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