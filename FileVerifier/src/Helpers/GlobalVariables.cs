using System.IO.Abstractions;
using AvaloniaDraft.ComparingMethods.ExifTool;

namespace AvaloniaDraft.Helpers;

public static class GlobalVariables
{
    public static readonly string? ExifPath = ExifTool.GetExifPath();
    public static FileManager.FileManager? FileManager { get; set; } = null!;
    public static readonly ExifTool ExifTool;

    static GlobalVariables()
    {
        ExifTool = new ExifTool();
    }
}