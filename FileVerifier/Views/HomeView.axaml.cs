using System;
using System.Diagnostics;
using System.IO;
using Avalonia;
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
    private string InputPath { get; set; }
    private string OutputPath { get; set; }
    private bool Working { get; set; } = false;

    public HomeView()
    {
        InitializeComponent();
        ConsoleService.Instance.OnMessageLogged += UpdateConsole;
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

    

    private void LoadButton_OnClick(object? sender, RoutedEventArgs e)
    {
        if (Working || string.IsNullOrEmpty(InputPath) || string.IsNullOrEmpty(OutputPath)) return;
        GlobalVariables.FileManager = new FileManager.FileManager(InputPath, OutputPath);
        GlobalVariables.FileManager.GetSiegfriedFormats();
        GlobalVariables.FileManager.WritePairs();
        StartButton.IsEnabled = true;
    }
    

    private void UpdateConsole(string message)
    {
        Console.Text = null; // This should probably be some switch statement resetting only when something something
        Dispatcher.UIThread.Post(() =>
        {
            Console.Text += message + Environment.NewLine;
        });
    }





    private void SynchronizeMethods()
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel == null) return;

        viewModel.IsPointByPointEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsAnimationEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsPageCountEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsColorProfileEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsFontEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsResolutionEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
        viewModel.IsSizeEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
    }


    private void SetSize(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsSizeEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.Size.Name, viewModel.IsSizeEnabled);
    }


    private void SetAnimations(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsAnimationEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.Animations.Name, viewModel.IsAnimationEnabled);
    }

    private void SetFonts(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsFontEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.Fonts.Name, viewModel.IsFontEnabled);
    }

    private void SetPointByPoint(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsPointByPointEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.PointByPoint.Name, viewModel.IsPointByPointEnabled);
    }

    private void SetResolution(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsResolutionEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.Resolution.Name, viewModel.IsResolutionEnabled);
    }

    private void SetColorProfile(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsColorProfileEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.ColorSpace.Name, viewModel.IsColorProfileEnabled);
    }


    private void SetPageCount(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsPageCountEnabled != null) GlobalVariables.Options.SetMethod(Helpers.Methods.Pages.Name, viewModel.IsPageCountEnabled);
    }
}


    
