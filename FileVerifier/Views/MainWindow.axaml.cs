using System;
using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class MainWindow : Window
{
    private readonly SettingsViewModel _settingsViewModel = new SettingsViewModel();

    private HomeView HomeView;
    private SettingsView SettingsView;
    private ReportView ReportView;
    private TestAnalysisView TestAnalysisView;

    public MainWindow()
    {
        InitializeComponent();
        SetActiveButton(HomeButton);

        // Listen for changes in SelectedWindowSize
        _settingsViewModel.PropertyChanged += SettingsViewModel_PropertyChanged;
        
        // Set initial window size based on the current selection
        UpdateWindowSize(_settingsViewModel.SelectedWindowSize);
    
        var homeView = new HomeView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = homeView;

        // Add event handler for window resizing
        LayoutUpdated += MainWindow_LayoutUpdated;
    }

    private void MainWindow_LayoutUpdated(object? sender, EventArgs e)
    {
        // Center the window after resizing
        WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    
    private void SettingsViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SettingsViewModel.SelectedWindowSize))
        {
            UpdateWindowSize(_settingsViewModel.SelectedWindowSize);
        }
    }
        
    private void UpdateWindowSize(WindowSizeOption? option)
    {
        if (option == null) return;
        Width = option.Width;
        Height = option.Height;
    }

    private void SetActiveButton(Button button)
    {
        // Reset all buttons
        HomeButton.Background = new SolidColorBrush(Colors.Transparent);
        SettingsButton.Background = new SolidColorBrush(Colors.Transparent);
        ReportButton.Background = new SolidColorBrush(Colors.Transparent);
        TestAnalysisButton.Background = new SolidColorBrush(Colors.Transparent);


        // Set active button style
        button.Background = new SolidColorBrush(Color.Parse("#107F37"));
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        if (HomeView == null) HomeView = new HomeView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = HomeView;
        SetActiveButton((Button)sender);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        if (SettingsView == null) SettingsView = new SettingsView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = SettingsView;
        SetActiveButton((Button)sender);
    }

    private void ReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (ReportView == null) ReportView = new ReportView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = ReportView;
        SetActiveButton((Button)sender);
    }

    private void TestAnalysisButton_Click(object sender, RoutedEventArgs e)
    {
        if (TestAnalysisView == null) TestAnalysisView = new TestAnalysisView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = TestAnalysisView;
        SetActiveButton((Button)sender);
    }
}