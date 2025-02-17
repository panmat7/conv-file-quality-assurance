using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaDraft.Views;

public partial class SettingsView : UserControl
{
    public SettingsView()
    {
        InitializeComponent();
    }

    private void SaveSettingsButton_Click(object sender, RoutedEventArgs e)
    {
        // Use a simple MessageBox or logging instead
        var window = (Window)this.VisualRoot;
        var dialog = new Window
        {
            Title = "Settings Saved",
            Content = new TextBlock { Text = "Your settings have been saved." },
            SizeToContent = SizeToContent.WidthAndHeight
        };
        dialog.ShowDialog(window);
    }
}