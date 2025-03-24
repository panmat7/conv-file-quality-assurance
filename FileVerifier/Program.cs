using Avalonia;
using System;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.Logger;
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