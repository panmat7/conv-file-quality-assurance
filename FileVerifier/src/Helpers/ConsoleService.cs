using System;
using Avalonia.Threading;

namespace AvaloniaDraft.Helpers;

public class ConsoleService
{
    public event Action<string>? OnMessageLogged;
    
    private static ConsoleService? _instance;
    public static ConsoleService Instance => _instance ??= new ConsoleService();
    
    private ConsoleService(){}

    public void WriteToConsole(string message)
    {
        Dispatcher.UIThread.InvokeAsync(() => OnMessageLogged?.Invoke(message));
    }
}