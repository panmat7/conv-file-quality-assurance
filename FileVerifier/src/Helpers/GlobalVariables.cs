using System.IO.Abstractions;
using Avalonia.Logger;
using AvaloniaDraft.ComparingMethods.ExifTool;

namespace AvaloniaDraft.Helpers;

public static class GlobalVariables
{
    public static readonly string? ExifPath = ExifTool.GetExifPath();
    public static FileManager.FileManager? FileManager { get; set; } = null!;
    public static readonly ExifTool ExifTool;
    public static Options.Options Options { get; set; }
    public static Logger Logger { get; set; }

    static GlobalVariables()
    {
        ExifTool = new ExifTool();
    }
}