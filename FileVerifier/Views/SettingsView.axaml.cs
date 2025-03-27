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
            var fileFormat = fileType.Key;
            var formats = fileType.Value;
            if (fileFormat == null || formats == null || formats.Count == 0) continue;

            var fileTypeCheckBox = new CheckBox
            {
                Name = fileFormat,
                Content = fileFormat,
            };
            fileTypeCheckBox.Click += (_, _) =>
            {
                MainFileFormatCheckBoxClick(fileTypeCheckBox.Name, fileTypeCheckBox.IsChecked ?? false);
            };

            // Add an expander for each file type
            var fileTypeExpander = new Expander();
            fileTypeExpander.Header = fileTypeCheckBox;
            fileTypeExpander.HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch;
            mainStackPanel.Children.Add(fileTypeExpander);

            var fileTypeStackPanel = new StackPanel();

            var allChecked = true;

            // Add a checkbox for each PRONOM code
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
                checkBox.Classes.Add(fileFormat);

                checkBox.Click += (_, _) =>
                {
                    GlobalVariables.Options.FileFormatsEnabled[fileFormat][pronomCode] = checkBox.IsChecked ?? false;

                    UpdateFormatCheckBox(checkBox.Classes[0]);
                };

                fileTypeStackPanel.Children.Add(checkBox);
            }

            fileTypeCheckBox.IsChecked = allChecked;
            fileTypeExpander.Content = fileTypeStackPanel;
        }

        FileFormatsExpander.Content = mainStackPanel;
    }

    private void UpdateFormatCheckBox(string fileFormat)
    {
        var mainCheckBox = FileFormatsExpander.GetLogicalDescendants()
                   .OfType<CheckBox>()
                   .FirstOrDefault(cb => cb.Name == fileFormat);

        if (mainCheckBox == null) return;

        var checkBoxes = FileFormatsExpander.GetLogicalDescendants()
                   .OfType<CheckBox>()
                   .Where(cb => cb.Classes.Contains(fileFormat));

        var allChecked = checkBoxes.Any(cb => cb.IsChecked == true);
        mainCheckBox.IsChecked = allChecked;
    }


    private void MainFileFormatCheckBoxClick(string fileFormat, bool check)
    {
        var checkBoxes = FileFormatsExpander.GetLogicalDescendants()
                   .OfType<CheckBox>()
                   .Where(cb => cb.Classes.Contains(fileFormat));
        

        foreach (var checkBox in checkBoxes) checkBox.IsChecked = check;
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

        viewModel.SizeComparisonThreshold = GlobalVariables.Options.SizeComparisonThreshold;
        viewModel.PbpComparisonThreshold = GlobalVariables.Options.PbpComparisonThreshold;

        viewModel.IsPointByPointEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.PointByPoint.Name);
        viewModel.IsAnimationEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Animations.Name);
        viewModel.IsPageCountEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Pages.Name);
        viewModel.IsColorProfileEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.ColorProfile.Name);
        viewModel.IsFontEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Fonts.Name);
        viewModel.IsResolutionEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Resolution.Name);
        viewModel.IsSizeEnabled = GlobalVariables.Options.GetMethod(Helpers.Methods.Size.Name);

        // Synchronize format filtering
        foreach (var format in GlobalVariables.Options.FileFormatsEnabled)
        {
            var fileType = format.Key;
            var pronomCodes = format.Value;

            var checkBoxes = FileFormatsExpander.GetLogicalDescendants()
           .OfType<CheckBox>()
           .Where(cb => cb.Classes.Contains(fileType));

            foreach (var checkBox in checkBoxes)
            {
                var cbCode = checkBox?.Content?.ToString();
                if (checkBox != null && cbCode != null && pronomCodes.ContainsKey(cbCode))
                {
                    checkBox.IsChecked = pronomCodes[cbCode];
                }
            }
            UpdateFormatCheckBox(fileType);
        }
    }
}