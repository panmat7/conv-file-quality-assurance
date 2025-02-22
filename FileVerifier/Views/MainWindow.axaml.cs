using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class MainWindow : Window
{
    private readonly SettingsViewModel _settingsViewModel = new SettingsViewModel();
    
    public MainWindow()
    {
        InitializeComponent();
        CanResize = true;
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
        this.Center(); // Center the window
    }

    private void SetActiveButton(Button button)
    {
        // Reset all buttons
        HomeButton.Background = new SolidColorBrush(Colors.Transparent);
        SettingsButton.Background = new SolidColorBrush(Colors.Transparent);

        // Set active button style
        button.Background = new SolidColorBrush(Color.Parse("#107F37"));
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        var homeView = new HomeView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = homeView;
        SetActiveButton((Button)sender);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        var settingsView = new SettingsView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = settingsView;
        SetActiveButton((Button)sender);
    }
}