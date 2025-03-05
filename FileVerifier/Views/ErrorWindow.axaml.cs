using Avalonia.Controls;
using Avalonia.Interactivity;

namespace AvaloniaDraft.Views;

public partial class ErrorWindow : Window
{
    public ErrorWindow(string message)
    {
        InitializeComponent();
        MessageTextBlock.Text = message;
    }

    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}