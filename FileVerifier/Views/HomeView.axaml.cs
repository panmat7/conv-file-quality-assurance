using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class HomeView : UserControl
{
    private bool AutoStartVerification { get; set; }
    
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
        try
        {
            await StartVerificationProcess();
        }
        catch (Exception err)
        {
            System.Console.WriteLine(err);
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow =
                    new ErrorWindow("An error occured when starting the verification process.");
                errWindow.ShowDialog((VisualRoot as Window)!);
            });
        }
    }

    private async void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        ResetProgress();
        
        if (Working || string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;

        var loadingWindow = new LoadingView();

        loadingWindow.AutoStartChanged += LoadingWindow_AutoStartChanged;
        
        try
        {
            loadingWindow.Show();

            await Task.Run(PerformBackgroundWork);

            StartButton.IsEnabled = true;
        }
        catch (InvalidOperationException err)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow = new ErrorWindow(
                    "Duplicate file names in the input or output folder! Ensure all files have unique names, matching their converted counterpart."
                );
                errWindow.ShowDialog((this.VisualRoot as Window)!);
            });
            return;
        }
        finally
        {
            loadingWindow.Close();

            try
            {
                if (AutoStartVerification)
                {
                    await StartVerificationProcess();
                    AutoStartVerification = false;
                }
                else if (GlobalVariables.FileManager != null)
                {
                    var ignoredFilesWindow = new IgnoredFilesView(
                        GlobalVariables.FileManager.GetFilePairs().Count, GlobalVariables.FileManager.IgnoredFiles);
                    ignoredFilesWindow.Show();
                }
            }
            catch (Exception exception)
            {
                System.Console.WriteLine(exception);
            }
        }
    }
    
    private void LoadingWindow_AutoStartChanged(object? sender, bool e)
    {
        AutoStartVerification = e;
    }

    private Task StartVerificationProcess()
    {
        return Task.Run(async () =>
        {
            try
            {
                if (Working || GlobalVariables.FileManager == null) return;

                Working = true;

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    StartButton.IsEnabled = false;
                    LoadButton.IsEnabled = false;
                    OverwriteConsole(null);
                });

                GlobalVariables.Logger.Start();

                await Task.Run(() =>
                {
                    GlobalVariables.FileManager.StartVerification();
                });

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    LoadButton.IsEnabled = true;
                });

                GlobalVariables.Logger.Finish();
                GlobalVariables.Logger.SaveReport();
                Trace.WriteLine("Finished");

                Working = false;
            }
            catch (Exception err)
            {
                System.Console.WriteLine(err);
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var errWindow =
                        new ErrorWindow("An error occured when starting the verification process.");
                    errWindow.ShowDialog((VisualRoot as Window)!);
                });
            }
        });
    }
    
    private void PerformBackgroundWork()
    {
        try
        {
            GlobalVariables.FileManager = new FileManager.FileManager(InputPath, OutputPath);
            GlobalVariables.FileManager.GetSiegfriedFormats();
            GlobalVariables.FileManager.FilterOutDisabledFileFormats();
            SetFileCount(GlobalVariables.FileManager.GetFilePairs().Count);
        }
        catch (InvalidOperationException err)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow = new ErrorWindow(
                    "Duplicate file names in the input or output folder! Ensure all files have unique names, matching their converted counterpart."
                );
                errWindow.ShowDialog((this.VisualRoot as Window)!);
            });
            return;
        }
        catch
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow =
                    new ErrorWindow("An error occured when forming file pairs.");
                errWindow.ShowDialog((this.VisualRoot as Window)!);
            });
            return;
        }
        GlobalVariables.FileManager.WritePairs();
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


    
