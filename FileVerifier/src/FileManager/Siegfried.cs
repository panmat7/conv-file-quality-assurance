using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using AvaloniaDraft.Helpers;

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
    public static void GetFileFormats(string originalDir, string newDir, string tempOriginalDir, string tempNewDir, ref List<FilePair> files)
    {
        string terminal = "";
        string arguments = "";
        string windowsTerminal = "powershell.exe";
        string linuxTerminal = "/bin/bash";
        string windowsArguments = $"-ExecutionPolicy Bypass -Command \"sf -json {originalDir}; sf -json {newDir}; sf -json {tempOriginalDir}; sf -json {tempNewDir}\"";
        string linuxArguments = $"-c \"sf -json {originalDir}; sf -json {newDir}; sf -json {tempOriginalDir}; sf -json {tempNewDir}\"";

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
            throw new Exception($"Unable to start {terminal} and Siegfried: {ex.Message}");
        }

        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error)) throw new Exception(error);

        // Use a regular expression to split the JSON objects correctly
        var regex = new Regex(@"(?<=\})\s*(?=\{)");
        var outputSep = regex.Split(output);

        if (outputSep.Length != 4) throw new Exception("Invalid Siegfried output");

        var originalOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[0]);
        var newOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[1]);
        var tempOriginalOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[2]);
        var tempNewOutput = JsonSerializer.Deserialize<SiegfriedOutputJson>(outputSep[3]);

        if (originalOutput == null || newOutput == null || tempOriginalOutput == null || tempNewOutput == null)
        {
            throw new Exception("Invalid Siegfried output");
        }

        originalOutput.Files = originalOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        newOutput.Files = newOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        tempOriginalOutput.Files = tempOriginalOutput.Files.Where(f => f.Matches.Count > 0).ToList();
        tempNewOutput.Files = tempNewOutput.Files.Where(f => f.Matches.Count > 0).ToList();

        foreach (var file in files)
        {
            var originalFile = originalOutput.Files.FirstOrDefault(f => f.Name == file.OriginalFilePath) ??
                               tempOriginalOutput.Files.FirstOrDefault(f => f.Name == file.OriginalFilePath);
            var newFile = newOutput.Files.FirstOrDefault(f => f.Name == file.NewFilePath) ??
                          tempNewOutput.Files.FirstOrDefault(f => f.Name == file.NewFilePath);

            if (originalFile != null)
            {
                file.OriginalFileFormat = originalFile.Matches[0].id;
            }

            if (newFile != null)
            {
                file.NewFileFormat = newFile.Matches[0].id;
            }
        }
        process.WaitForExit();
    }
}