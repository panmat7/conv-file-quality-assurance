using Avalonia;
using System;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.FileManager;

namespace AvaloniaDraft;


sealed class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        //Ensure proper cleanup after exit
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledExceptionCleanup;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;

        GlobalVariables.Options = new Options.Options();
        GlobalVariables.Options.Initialize();

        GlobalVariables.Logger = new Logger.Logger();
        GlobalVariables.Logger.Initialize();



        /*
        GlobalVariables.Logger.Start();
        var src2 = "C:\\Users\\fredr\\OneDrive\\Skrivebord\\NTNU\\Bachelor\\Testing\\Fonts\\Fonts\\Input\\word.docx";
        var src1 = "C:\\Users\\fredr\\OneDrive\\Skrivebord\\NTNU\\Bachelor\\Testing\\Fonts\\Fonts\\Output\\word.pdf";
        var src3 = "C:\\Users\\fredr\\OneDrive\\Skrivebord\\NTNU\\Bachelor\\Testing\\Fonts\\Fonts\\Input\\pp.pptx";

        FilePair fp1 = new FilePair(src1, src2);

        FilePair fp2 = new FilePair(src2, src3);

        var test1 = Methods.Resolution.Name;
        var test2 = Methods.Fonts.Name;
        var err = new Error("ErrrrrOR", "Wow very bad", ErrorSeverity.Medium);


        GlobalVariables.Logger.AddTestResult(fp1, test1, false, "no comment", null, err);
        GlobalVariables.Logger.AddTestResult(fp1, test2, true, "no comment", 99, err);

        GlobalVariables.Logger.AddTestResult(fp2, test1, true, "no comment", null, err);

        GlobalVariables.Logger.Finish();
        GlobalVariables.Logger.ExportJSON("C:\\Users\\fredr\\OneDrive\\Skrivebord\\NTNU\\Bachelor\\report.json");
        */



        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    private static void OnUnhandledExceptionCleanup(object sender, UnhandledExceptionEventArgs e)
    {
        GlobalVariables.ExifTool.Dispose();
    }

    private static void OnProcessExit(object? sender, EventArgs e)
    {
        GlobalVariables.ExifTool.Dispose();
    }
}