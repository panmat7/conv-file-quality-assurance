using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.OpenGL.Surfaces;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class HomeView : UserControl
{
    private string InputPath { get; set; }
    private string OutputPath { get; set; }
    private bool Working { get; set; } = false;
    
    //Progress bar
    private int FileCount { get; set; } = 0;
    private int FilesDone { get; set; } = 0;
    private readonly object _lock = new object();

    public HomeView()
    {
        InitializeComponent();
        UiControlService.Instance.OnMessageLogged += AppendConsole;
        UiControlService.Instance.OverwriteConsole += OverwriteConsole;
        UiControlService.Instance.UpdateProgressBar += FileDone;
        InputButton.Content = string.IsNullOrEmpty(InputPath) ? "Select" : "Selected";
        OutputButton.Content = string.IsNullOrEmpty(OutputPath) ? "Select" : "Selected";
        DataContext = new SettingsViewModel();
    }

    private async void InputButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var result = await topLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = "Select Folder",
            AllowMultiple = false
        });

        if (result.Count > 0)
        {
            var folder = result[0];

            if (sender is not Button button) return;
            switch (button.Name)
            {
                case "InputButton":
                    InputPath = folder.TryGetLocalPath() ?? throw new InvalidOperationException();
                    InputButton.Content = "Selected";
                    break;
                case "OutputButton":
                    OutputPath = folder.TryGetLocalPath() ?? throw new InvalidOperationException();
                    OutputButton.Content = "Selected";
                    break;
            }
        }
        else
        {
            //TODO: Please select folder message
        }
    }

    private void InputButton_OnPointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.Name)
        {
            case "InputButton":
                if (string.IsNullOrEmpty(InputPath)) return;
                InputButton.Content = InputPath;
                break;
            case "OutputButton":
                if (string.IsNullOrEmpty(OutputPath)) return;
                OutputButton.Content = OutputPath;
                break;
        }
    }

    private void InputButton_OnPointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is not Button button) return;
        switch (button.Name)
        {
            case "InputButton":
                InputButton.Content = string.IsNullOrEmpty(InputPath) ? "Select" : "Selected";
                break;
            case "OutputButton":
                OutputButton.Content = string.IsNullOrEmpty(OutputPath) ? "Select" : "Selected";
                break;
        }
    }

    private async void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Working || GlobalVariables.FileManager == null) return;
        
        Working = true;
        
        StartButton.IsEnabled = false;
        LoadButton.IsEnabled = false;
        
        OverwriteConsole(null);
        
        await Task.Run(() =>
        {
            GlobalVariables.FileManager.StartVerification();
        });
        
        LoadButton.IsEnabled = true;
        
        Working = false;
    }

    

    private void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ResetProgress();
        
        if (Working || string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;
        try
        {
            GlobalVariables.FileManager = new FileManager.FileManager(InputPath, OutputPath);
            SetFileCount(GlobalVariables.FileManager.GetFilePairs().Count);
        }
        catch (InvalidOperationException err)
        {
            var errWindow =
                new ErrorWindow(
                    "Duplicate file names in the input or output folder! Ensure all files have unique names, matching their converted counterpart.");
            errWindow.ShowDialog((this.VisualRoot as Window)!);
            return;
        }
        catch
        {
            var errWindow =
                new ErrorWindow(
                    "An error occured when forming file pairs.");
            errWindow.ShowDialog((this.VisualRoot as Window)!);
            return;
        }
        
        GlobalVariables.FileManager.GetSiegfriedFormats();
        GlobalVariables.FileManager.WritePairs();
        StartButton.IsEnabled = true;
    }

    private void SetFileCount(int count)
    {
        lock (_lock)
        {
            FileCount = count;
        }
    }

    private void ResetProgress()
    {
        lock (_lock)
        {
            FilesDone = 0;
            Dispatcher.UIThread.Post(() =>
            {
                ProgressBar.Value = 0;
            });
        }
    }
    
    private void FileDone()
    {
        lock (_lock)
        {
            FilesDone++;
            
            Dispatcher.UIThread.Post(() =>
            {
                ProgressBar.Value = (FilesDone / (double)FileCount) * 100;
            });
        }
    }

    private void AppendConsole(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text += message + Environment.NewLine;
        });
    }

    private void OverwriteConsole(string? message)
    {
        Console.Text = null;

        if (message != null)
            AppendConsole(message);
    }
}


    
