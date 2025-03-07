using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;
using System.Diagnostics;
using System.Text.Json;

namespace AvaloniaDraft.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }


    private void SetProfile(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (SettingsProfile == null) return;

        GlobalVariables.Options.profile = SettingsProfile.SelectedIndex switch
        {
            0 => Options.SettingsProfile.Default,
            1 => Options.SettingsProfile.Custom1,
            2 => Options.SettingsProfile.Custom2,
            3 => Options.SettingsProfile.Custom3,
            _ => Options.SettingsProfile.Default,
        };
        GlobalVariables.Options.LoadSettings();
        Synchronize();
    }

    private static void SaveSettings(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        GlobalVariables.Options.SaveSettings();
    }


    private void ResetSettings(object sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        GlobalVariables.Options.SetDefaultSettings();
        Synchronize();
    }


    private void Synchronize()
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel == null) return;

        viewModel.IsIgnoreUnsupportedFormatsEnabled = GlobalVariables.Options.ignoreUnsupportedFileType;

        viewModel.IsPointByPointEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.PointByPoint.Name);
        viewModel.IsAnimationEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Animations.Name);
        viewModel.IsPageCountEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Pages.Name);
        viewModel.IsColorProfileEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.ColorSpace.Name);
        viewModel.IsFontEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Fonts.Name);
        viewModel.IsResolutionEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Resolution.Name);
        viewModel.IsSizeEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
    }


    private void SetIgnoreUnsupportedFormats(object? sender, RoutedEventArgs e)
    {
        var viewModel = DataContext as SettingsViewModel;
        if (viewModel?.IsSizeEnabled != null) GlobalVariables.Options.ignoreUnsupportedFileType = viewModel.IsIgnoreUnsupportedFormatsEnabled;
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