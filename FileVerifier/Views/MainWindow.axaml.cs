using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace AvaloniaDraft.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        SetActiveButton(HomeButton);
    }

    private void SetActiveButton(Button button)
    {
        // Reset all buttons
        HomeButton.Background = new SolidColorBrush(Colors.Transparent);
        SettingsButton.Background = new SolidColorBrush(Colors.Transparent);

        // Set active button style
        button.Background = new SolidColorBrush(0xFFCC0000);
    }

    private void HomeButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new HomeView();
        SetActiveButton((Button)sender);
    }

    private void SettingsButton_Click(object sender, RoutedEventArgs e)
    {
        MainContent.Content = new SettingsView();
        SetActiveButton((Button)sender);
    }
}