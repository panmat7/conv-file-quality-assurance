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
using AvaloniaDraft.FileManager;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class HomeView : UserControl
{
    private bool AutoStartVerification { get; set; }
    
    private string InputPath { get; set; }
    private string OutputPath { get; set; }
    private string CheckpointPath { get; set; }
    private string ExtractionPath { get; set; }
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
        ExtractionButton.Content = string.IsNullOrEmpty(InputPath) ? "Select" : "Selected";
        DataContext = new SettingsViewModel();

        LoadPaths();
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
                    GlobalVariables.Paths.OriginalFilesPath = InputPath;
                    break;
                case "OutputButton":
                    OutputPath = folder.TryGetLocalPath() ?? throw new InvalidOperationException();
                    OutputButton.Content = "Selected";
                    GlobalVariables.Paths.NewFilesPath = OutputPath;
                    break;
                case "ExtractionButton":
                    ExtractionPath = folder.TryGetLocalPath() ?? throw new InvalidOperationException();
                    ExtractionButton.Content = "Selected";
                    GlobalVariables.Paths.DataExtractionFilesPath = ExtractionPath;
                    break;
            }
            GlobalVariables.Paths.SavePaths();
        }
        else
        {
            //TODO: Please select folder message
        }
    }


    private async void CheckpointButton_OnClick(object? sender, RoutedEventArgs e)
    {
        var topLevel = TopLevel.GetTopLevel(this);
        if (topLevel == null) return;

        var result = await topLevel.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select Checkpoint",
            AllowMultiple = false,
            FileTypeFilter =
            [
                new FilePickerFileType("JSON file")
                {
                    Patterns = ["*.json"]
                }
            ]
        });

        if (result.Count > 0)
        {
            var file = result[0];

            if (sender is not Button) return;
            CheckpointButton.Content = "Selected";
            CheckpointPath = file.TryGetLocalPath() ?? throw new InvalidOperationException();
            GlobalVariables.Paths.CheckpointPath = CheckpointPath;
            GlobalVariables.Paths.SavePaths();
        }
        else
        {
            //TODO: Please select file message
        }
    }


    private void LoadPaths()
    {
        GlobalVariables.Paths.LoadPaths();
        var oPath = GlobalVariables.Paths.OriginalFilesPath;
        var nPath = GlobalVariables.Paths.NewFilesPath;
        var cpPath = GlobalVariables.Paths.CheckpointPath;
        var dePath = GlobalVariables.Paths.DataExtractionFilesPath;

        if (oPath != null)
        {
            InputPath = oPath;
            InputButton.Content = "Selected";
        }

        if (nPath != null)
        {
            OutputPath = nPath;
            OutputButton.Content = "Selected";
        }

        if (cpPath != null)
        {
            CheckpointPath = cpPath;
            CheckpointButton.Content = "Selected";
        }

        if (dePath != null)
        {
            ExtractionPath = dePath;
            ExtractionButton.Content = "Selected";
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
            case "CheckpointButton":
                if (string.IsNullOrEmpty(CheckpointPath)) return;
                CheckpointButton.Content = CheckpointPath;
                break;
            case "ExtractionButton":
                if (string.IsNullOrEmpty(ExtractionPath)) return;
                ExtractionButton.Content = ExtractionPath;
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
            case "CheckpointButton":
                CheckpointButton.Content = string.IsNullOrEmpty(CheckpointPath) ? "Select" : "Selected";
                break;
            case "ExtractionButton":
                ExtractionButton.Content = string.IsNullOrEmpty(ExtractionPath) ? "Select" : "Selected";
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

            var checkpointChecked = CheckpointCheckbox.IsChecked;
            await Task.Run(() => PerformBackgroundWork(checkpointChecked));

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
        var checkpointChecked = CheckpointCheckbox?.IsChecked ?? false;

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
                    ExtractionStartButton.IsEnabled = false;
                    OverwriteConsole(null);
                });

                GlobalVariables.Logger.Start();

                await Task.Run(() =>
                {
                    GlobalVariables.FileManager.StartVerification();
                });
                
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    var completedWindow = new CompletedView();
                    completedWindow.Show();
                    completedWindow.Closed += (sender, args) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            LoadButton.IsEnabled = true;
                            ExtractionStartButton.IsEnabled = true;
                        });
                    };
                });

                GlobalVariables.Logger.Finish();
                GlobalVariables.Logger.SaveReport();
                AppendConsole("End report written.");

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
    
    private void PerformBackgroundWork(bool? checkpointChecked)
    {
        try
        {
            var checkPointPath = (checkpointChecked ?? false) ? CheckpointPath : null;

            if (!string.IsNullOrEmpty(checkPointPath)) GlobalVariables.Logger.ImportJSON(checkPointPath);
            var filePairs = GlobalVariables.Logger.GetFilePairs();

            GlobalVariables.FileManager = new FileManager.FileManager(InputPath, OutputPath, filePairs);
            GlobalVariables.FileManager.SetSiegfriedFormats();
            GlobalVariables.FileManager.FilterOutDisabledFileFormats();
            SetFileCount(GlobalVariables.FileManager.GetFilePairs().Count);
        }
        catch (InvalidOperationException err)
        {
            Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow = new ErrorWindow(
                    "Duplicate file names in the folder containing converted files! Ensure all files have unique names, matching their original counterpart."
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
        OverwriteConsole("The following pairs were formed:\n" + GlobalVariables.FileManager.GetPairFormats());
    }
    
    private async void ExtractionStartButton_OnClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            await StartExtractionProcess();
        }
        catch(Exception err)
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                var errWindow =
                    new ErrorWindow("An error occured when starting the extraction process.");
                errWindow.ShowDialog((VisualRoot as Window)!);
            });
        }
    }

    private Task StartExtractionProcess()
    {
        return Task.Run(async () =>
        {
            if(Working) return;
            
            Working = true;

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                StartButton.IsEnabled = false;
                LoadButton.IsEnabled = false;
                ExtractionStartButton.IsEnabled = false;
                OverwriteConsole(null);
            });
            
            AppendConsole("Starting extraction...\n\n");

            GlobalVariables.SingleFileManager = new SingleFileManager(ExtractionPath);
            GlobalVariables.SingleFileManager.SetSiegfriedFormats();
            await Task.Run(() =>
            {
                GlobalVariables.SingleFileManager.StartProcessing();
            });

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                var completedWindow = new CompletedView();
                completedWindow.Show();
                completedWindow.Closed += (sender, args) =>
                {
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        LoadButton.IsEnabled = true;
                        ExtractionButton.IsEnabled = true;
                    });
                };
            });
            
            GlobalVariables.SingleFileManager.WriteReport();
            AppendConsole("Extraction report written.");
            
            Working = false;
            GlobalVariables.SingleFileManager = null;
        });
    }

    /// <summary>
    /// Increments the number of files done
    /// </summary>
    /// <param name="count"></param>
    private void SetFileCount(int count)
    {
        lock (_lock)
        {
            FileCount = count;
        }
    }

    /// <summary>
    /// Resets the progress bar to 0
    /// </summary>
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
    
    /// <summary>
    /// Marks a file as done and updates console progress 
    /// </summary>
    private void FileDone()
    {
        lock (_lock)
        {
            FilesDone++;
            var progress = (FilesDone / (double)FileCount) * 100; //Progressing in 1% increments to avoid ui updates
            if ((int)progress > (int)ProgressBar.Value)
            {
                Dispatcher.UIThread.Post(() =>
                {
                    ProgressBar.Value = (FilesDone / (double)FileCount) * 100;
                });
            }
        }
    }

    /// <summary>
    /// Appends the message to console.
    /// </summary>
    /// <param name="message">Message to be appended.</param>
    private void AppendConsole(string message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text += message + Environment.NewLine;
            Console.CaretIndex = Console.Text.Length;
        });
    }

    /// <summary>
    /// Overwrites the console with a message, or resets it.
    /// </summary>
    /// <param name="message">Message to overwrite with, null just remove all content.</param>
    private void OverwriteConsole(string? message)
    {
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text = null;

            if (message != null)
                AppendConsole(message);
        });
    }
}


    
