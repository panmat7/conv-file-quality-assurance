using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Aspose.Words.Fields;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using AvaloniaDraft.FileManager;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class MainWindow : Window
{
    private string InputPath { get; set; }
    private string OutputPath { get; set; }
    
    public MainWindow()
    {
        InitializeComponent();
        InputButton.Content = string.IsNullOrEmpty(InputPath) ? "Select" : "Selected";
        OutputButton.Content = string.IsNullOrEmpty(OutputPath) ? "Select" : "Selected";
    }
    
    private async void InputButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var result = await StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
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
        if(sender is not Button button) return;
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
        if(sender is not Button button) return;
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

    private void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;
        
        AppendMessageToConsole("INPUT:");
        var files = Directory.GetFiles(InputPath);
        foreach (var file in files) { AppendMessageToConsole(file); }
        
        AppendMessageToConsole("OUTPUT:");
        files = Directory.GetFiles(OutputPath);
        foreach (var file in files) { AppendMessageToConsole(file); }
        
        var f = new FileManager.FileManager(InputPath, OutputPath);
        f.GetSiegfriedFormats();
        f.WritePairs();
    }
    
    private void Load_OnClick(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;
        
        // For Input folder files
        AppendMessageToConsole("INPUT:");
        var files = Directory.GetFiles(InputPath);
        AppendMessageToConsole($"Number of files in Input folder: {files.Length}");
        var inputExtensions = GetFileExtensions(files);
        AppendMessageToConsole("All file extensions in input folder:");
        foreach (var keyValuePair in inputExtensions.OrderBy(x => x.Key))
        {
            AppendMessageToConsole($"{keyValuePair.Key}: {keyValuePair.Value}");
        }
        
        // For Output folder files
        AppendMessageToConsole("OUTPUT:");
        files = Directory.GetFiles(OutputPath);
        AppendMessageToConsole($"Number of files in Output folder: {files.Length}");
        var outputExtensions = GetFileExtensions(files);
        AppendMessageToConsole("All file extensions in output folder:");
        foreach (var keyValuePair in outputExtensions.OrderBy(x => x.Key))
        {
            AppendMessageToConsole($"{keyValuePair.Key}: {keyValuePair.Value}");
        }

        var f = new FileManager.FileManager(InputPath, OutputPath);
        f.GetSiegfriedFormats();
        f.WritePairs();
    }

    private void AppendMessageToConsole(string text)
    {
        if (!string.IsNullOrEmpty(Console.Text))
        {
            Console.Text += "\n";
        }
        
        Console.Text += text;
        Console.CaretIndex = Console.Text.Length;
    }

    private Dictionary<string, int> GetFileExtensions(string[] files)
    {
        Dictionary<string, int> fileCount = new Dictionary<string, int>();
        foreach (var file in files)
        {
            string extension = Path.GetExtension(file).ToLower();
            if (string.IsNullOrEmpty(extension)) 
                extension = "*";
            if (fileCount.ContainsKey(extension))
                fileCount[extension]++;
            else fileCount.Add(extension, 1);
        }
        return fileCount;
    }
}