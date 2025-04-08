using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class LoadingView : Window
{
    public event EventHandler<bool>? AutoStartChanged;
    
    public LoadingView()
    {
        InitializeComponent();
    }
    
    private void Checkbox_Changed(object? sender, RoutedEventArgs e)
    {
        AutoStartChanged?.Invoke(this, true);
    }
}