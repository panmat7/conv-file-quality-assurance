using AvaloniaDraft.ComparingMethods.ExifTool;
using AvaloniaDraft.ProgramManager;

namespace AvaloniaDraft.Helpers;

public static class GlobalVariables
{
    public static readonly string? ExifPath = ExifTool.GetExifPath();
    public static ProgramManager.ProgramManager? ProgramManager { get; set; } = null!;
    public static SingleFileManager? SingleFileManager { get; set; } = null!;
    public static readonly ExifTool ExifTool;
    public static Options.Options Options { get; set; }
    public static Logger.Logger Logger { get; set; }
    public static object ImageExtractionLock { get; } = new object();
    public static Paths Paths { get; set; } = new Paths();
    public static bool StopProcessing { get; set; } = false;

    static GlobalVariables()
    {
        ExifTool = new ExifTool();
    }
}