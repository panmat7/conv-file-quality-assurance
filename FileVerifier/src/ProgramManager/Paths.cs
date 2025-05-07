using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text.Json;

namespace AvaloniaDraft.ProgramManager;

/// <summary>
/// Stores the directory paths
/// </summary>
[ExcludeFromCodeCoverage]
public class Paths
{
    public string? OriginalFilesPath { get; set; }
    public string? NewFilesPath { get; set; }
    public string? CheckpointPath { get; set; }
    public string? DataExtractionFilesPath { get; set; }

    private readonly string? JsonPath; // Path to where Paths is saved as a JSON file

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
                if (Path.Exists(paths.CheckpointPath)) this.CheckpointPath = paths.CheckpointPath;
                if (Path.Exists(paths.DataExtractionFilesPath)) this.DataExtractionFilesPath = paths.DataExtractionFilesPath;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error trying to load paths: {ex}");
        }
    }
}
