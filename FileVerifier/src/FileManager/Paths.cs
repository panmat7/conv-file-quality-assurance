using DocumentFormat.OpenXml.Office2016.Drawing.Command;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvaloniaDraft.FileManager;

/// <summary>
/// Stores the directory paths
/// </summary>
public class Paths
{
    public string? OriginalFilesPath { get; set; }
    public string? NewFilesPath { get; set; }
    public string? CheckpointPath { get; set; }

    private readonly string? JsonPath;

    public Paths() 
    {
        var currentDir = Directory.GetCurrentDirectory();

        while (currentDir != null)
        {
            if (Path.GetFileName(currentDir) == "FileVerifier")
            {
                JsonPath = Path.Join(currentDir, "settings/paths.json");
                return;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }
    }


    /// <summary>
    /// Save to JSON
    /// </summary>
    public void SavePaths()
    {
        if (JsonPath == null || !Path.Exists(Path.GetDirectoryName(JsonPath))) return;

        try
        {
            var jsonString = JsonSerializer.Serialize(this);
            File.WriteAllText(JsonPath, jsonString);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to save paths: {ex}");
        }
    }


    /// <summary>
    /// Load from JSON
    /// </summary>
    public void LoadPaths()
    {
        if (JsonPath == null || !File.Exists(JsonPath)) return;

        try
        {
            var jsonString = File.ReadAllText(JsonPath);

            var p = JsonSerializer.Deserialize<Paths>(jsonString);
            if (p is Paths paths)
            {
                if (Path.Exists(paths.OriginalFilesPath)) this.OriginalFilesPath = paths.OriginalFilesPath;
                if (Path.Exists(paths.NewFilesPath)) this.NewFilesPath = paths.NewFilesPath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to load paths: {ex}");
        }
    }
}
