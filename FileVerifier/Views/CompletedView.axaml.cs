using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class CompletedView : Window
{
    public CompletedView()
    {
        InitializeComponent();
    }
    
    private void OKButton_OnClick_(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}