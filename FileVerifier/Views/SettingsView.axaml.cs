using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.LogicalTree;
using AvaloniaDraft.Helpers;
using AvaloniaDraft.ViewModels;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;

namespace AvaloniaDraft.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();

        InitializeFileFormats();
    }


    private void InitializeFileFormats()
    {
        var mainStackPanel = new StackPanel();

        var fileTypes = GlobalVariables.Options.FileFormatsEnabled;
        foreach (var fileType in fileTypes)
        {
            var name = fileType.Key;
            var formats = fileType.Value;
            if (name == null || formats == null || formats.Count == 0) continue;

            var fileTypeCheckBox = new CheckBox();
            mainStackPanel.Children.Add(fileTypeCheckBox);

            // Add an expander for each file type
            var fileTypeExpander = new Expander();
            fileTypeExpander.Header = new TextBlock { Text = name };
            fileTypeExpander.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            mainStackPanel.Children.Add(fileTypeExpander);

            var fileTypeStackPanel = new StackPanel();

            var allChecked = true;

            // Add a checkbox for each pronom code
            foreach (var format in formats)
            {
                var pronomCode = format.Key;
                var isChecked = format.Value;

                if (!isChecked) allChecked = false;

                var checkBox = new CheckBox
                {
                    Content = pronomCode,
                    IsChecked = isChecked,
                };

                checkBox.IsCheckedChanged += (_, _) =>
                {
                    GlobalVariables.Options.FileFormatsEnabled[name][pronomCode] = checkBox.IsChecked ?? false;
                };

                fileTypeStackPanel.Children.Add(checkBox);
            }

            fileTypeCheckBox.IsChecked = allChecked;
            fileTypeExpander.Content = fileTypeStackPanel;
        }

        FileFormatsExpander.Content = mainStackPanel;
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
        
        // Reset file format check boxes
        if (FileFormatsExpander.Content is StackPanel mainStackPanel)
        {
            foreach(Expander expander in mainStackPanel.Children.OfType<Expander>())
            {
                if (expander.Content is not StackPanel fileTypeStackPanel) continue;
                if (expander.Header is not TextBlock headerTextBlock) continue;

                var fileType = headerTextBlock.Text ?? "";
                if (!GlobalVariables.Options.FileFormatsEnabled.ContainsKey(fileType)) continue;
                foreach (CheckBox checkBox in fileTypeStackPanel.Children.OfType<CheckBox>())
                {
                    var formatCode = checkBox?.Content?.ToString() ?? "";
                    if (!GlobalVariables.Options.FileFormatsEnabled[fileType].ContainsKey(formatCode)) continue;

                    if (checkBox.IsChecked == null) continue;
                    checkBox.IsChecked = GlobalVariables.Options.FileFormatsEnabled[fileType][formatCode];
                }
            }
        }
    }
}