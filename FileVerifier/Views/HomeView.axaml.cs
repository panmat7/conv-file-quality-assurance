using System;
using System.IO;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.Views;

public partial class HomeView : UserControl
{
    private string InputPath { get; set; }
    private string OutputPath { get; set; }
    private bool Working { get; set; } = false;

    public HomeView()
    {
        InitializeComponent();
        ConsoleService.Instance.OnMessageLogged += UpdateConsole;
        InputButton.Content = string.IsNullOrEmpty(InputPath) ? "Select" : "Selected";
        OutputButton.Content = string.IsNullOrEmpty(OutputPath) ? "Select" : "Selected";
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

    private void Start_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Working || GlobalVariables.FileManager == null) return;
        
        Working = true;
        
        StartButton.IsEnabled = false;
        LoadButton.IsEnabled = false;
        
        GlobalVariables.FileManager.StartVerification();
        
        StartButton.IsEnabled = true;
        LoadButton.IsEnabled = true;
        
        Working = false;
        
        ConsoleService.Instance.WriteToConsole("Testing start button");
    }

    

    private async void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Working || string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;

        var loadingWindow = new LoadingView();

        try
        {
            loadingWindow.Show();

            await Task.Run(PerformBackgroundWork);

            StartButton.IsEnabled = true;
        }
        finally
        {
            loadingWindow.Close();
        }
    }
    
    private void PerformBackgroundWork()
    {
        GlobalVariables.FileManager = new FileManager.FileManager(InputPath, OutputPath);
        GlobalVariables.FileManager.GetSiegfriedFormats();
        GlobalVariables.FileManager.WritePairs();
    }

    private void UpdateConsole(string message)
    {
        Console.Text = null; // This should probably be some switch statement resetting only when something something
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text += message + Environment.NewLine;
        });
    }
}


    
