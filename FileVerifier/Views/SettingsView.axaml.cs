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

        GlobalVariables.Options.Profile = SettingsProfile.SelectedIndex switch
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

    private void SaveSettings(object sender, Avalonia.Interactivity.RoutedEventArgs e)
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

        viewModel.IsIgnoreUnsupportedFormatsEnabled = GlobalVariables.Options.IgnoreUnsupportedFileType;

        viewModel.IsPointByPointEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.PointByPoint.Name);
        viewModel.IsAnimationEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Animations.Name);
        viewModel.IsPageCountEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Pages.Name);
        viewModel.IsColorProfileEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.ColorProfile.Name);
        viewModel.IsFontEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Fonts.Name);
        viewModel.IsResolutionEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Resolution.Name);
        viewModel.IsSizeEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);
    }
}