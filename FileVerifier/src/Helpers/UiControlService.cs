using System;
using Avalonia.Threading;

namespace AvaloniaDraft.Helpers;

public class UiControlService
{
    public event Action<string>? OnMessageLogged;
    public event Action? UpdateProgressBar;
    public event Action<string?>? OverwriteConsole;
    
    private static UiControlService? _instance;
    public static UiControlService Instance => _instance ??= new UiControlService();
    
    private UiControlService(){}

    /// <summary>
    /// Appends a message to console.
    /// </summary>
    /// <param name="message">Message to be appended</param>
    public void AppendToConsole(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => OnMessageLogged?.Invoke(message));
    }
    
    /// <summary>
    /// Overwrites the entire console, deleting current content, and prints a new message.
    /// </summary>
    /// <param name="message">Message to be written. Null if the console is to just be cleared.</param>
    public void OverwriteConsoleOutput(string? message)
    {
        Dispatcher.UIThread.InvokeAsync(() => OverwriteConsole?.Invoke(message));
    }

    public void MarkProgress()
    {
        Dispatcher.UIThread.InvokeAsync(() => UpdateProgressBar?.Invoke());
    }
}