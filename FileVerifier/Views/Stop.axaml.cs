using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDraft.Helpers;

namespace AvaloniaDraft.Views;

public partial class StopWindow : Window
{
    public StopWindow()
    {
        InitializeComponent();
    }

    private void StopComparison(object sender, RoutedEventArgs e)
    {
        GlobalVariables.StopProcessing = true;
        Close();
    }
    
    private void CloseWindow(object sender, RoutedEventArgs e)
    {
        Close();
    }
}