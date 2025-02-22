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
        SetActiveButton(HomeButton);

        var homeView = new HomeView
        {
            DataContext = _settingsViewModel
        };
        MainContent.Content = homeView;
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