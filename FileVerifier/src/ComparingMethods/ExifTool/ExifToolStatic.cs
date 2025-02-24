using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Newtonsoft.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace AvaloniaDraft.ComparingMethods.ExifTool;

public class ImageMetadata
{

    public string SourceFile { get; set; } = "";
    public Dictionary<string, object> ExifTool { get; set; } = new();
    public Dictionary<string, object> File { get; set; } = new();
    
    [JsonExtensionData]
    public IDictionary<string, object> AdditionalProperties { get; set; } = new Dictionary<string, object>();
}

/// <summary>
/// Class <c>ExifTool</c> is responsible for the programs communication with ExifTool
/// </summary>
public static class ExifToolStatic
{
    /// <summary>
    /// Uses ExifTool to extract metadata information about files. 
    /// </summary>
    /// <param name="filenames">List of files that the metadata is to be extracted from</param>
    /// <returns>List of dictionaries contacting all metadata for each file</returns>
    private static string? GetExifData(string[] filenames, string? path = null, bool group = true)
    {
        ProcessStartInfo psi;

        string commandPowershell;
        string commandExifTool;
        
        if (group)
        {
            commandPowershell = $"exiftool -j -quiet -g {string.Join(" ", filenames)}";
            commandExifTool = $"-j -quiet -g {string.Join(" ", filenames)}";
        }
        else
        {
            commandPowershell = $"exiftool -j -quiet {string.Join(" ", filenames)}";
            commandExifTool = $"-j -quiet {string.Join(" ", filenames)}";
        }
        
        if (path == null)
        {
            psi = new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = commandPowershell,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        else
        {
            psi = new ProcessStartInfo
            {
                FileName = path,
                Arguments = commandExifTool,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };
        }
        
        using var process = new Process();
        process.StartInfo = psi;
        process.Start();
        var output = process.StandardOutput.ReadToEnd();
        var error = process.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            Console.WriteLine($"Error starting exiftool process: {error}");
            return null;
        }
        
        process.WaitForExit();

        return output;
    }

    public static List<Dictionary<string, object>>? GetExifDataDictionary(string[] filenames, string? path = null, bool group = true)
    {
        var output = GetExifData(filenames, path, group);

        if (output == null) return null;
        
        List<Dictionary<string, object>>? metadata;
        try
        {
            metadata =  JsonSerializer.Deserialize<List<Dictionary<string, object>>>(output);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing exiftool output: {e.Message}");
            return null;
        }

        return metadata;

    }
    
    public static List<ImageMetadata>? GetExifDataImageMetadata(string[] files, string? path = null)
    {
        var output = GetExifData(files, path);
        
        if (output == null) return null;
        
        List<ImageMetadata>? metadata;
        try
        {
            metadata = JsonConvert.DeserializeObject<List<ImageMetadata>>(output);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing exiftool output: {e.Message}");
            return null;
        }
        
        return metadata;
    }

    public static string? GetExifPath()
    {
        var curDir = Directory.GetCurrentDirectory();

        while (!string.IsNullOrEmpty(curDir))
        {
            if (Path.GetFileName(curDir) == "conv-file-quality-assurance")
            {
                return curDir + @"\FileVerifier\src\ComparingMethods\ExifTool\exiftool.exe";
            }
            
            curDir = Directory.GetParent(curDir)?.FullName;
        }
        

        return null;
    } 
}