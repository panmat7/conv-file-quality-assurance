using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace AvaloniaDraft.ComparingMethods.ExifTool;

/// <summary>
/// Class <c>ExifTool</c> is responsible for the programs communication with ExifTool
/// </summary>
public static class ExifTool
{
    /// <summary>
    /// Uses ExifTool to extract metadata information about files. 
    /// </summary>
    /// <param name="filenames">List of files that the metadata is to be extracted from</param>
    /// <returns>List of dictionaries contacting all metadata for each file</returns>
    public static List<Dictionary<string, object>>? GetExifData(string[] filenames)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"exiftool -j -quiet {string.Join(" ", filenames)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

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

        List<Dictionary<string, object>>? metadata;
        try
        {
            metadata = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(output);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error parsing exiftool output: {e.Message}");
            return null;
        }

        return metadata;
    }
}