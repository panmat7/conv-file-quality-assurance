using System.Collections.Generic;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AvaloniaDraft.ViewModels;

namespace AvaloniaDraft.Views;

public partial class IgnoredFilesView : Window
{
    public string Message { get; set; }
    
    public IgnoredFilesView(int totalFilePairs, List<string> filePaths)
    {
        Message = $"{totalFilePairs} file pairs were created and are ready for verification";
        InitializeComponent();

        DataContext = new IgnoredFilesViewModel(totalFilePairs, filePaths);
    }

    private void OKButton_OnClick_(object? sender, RoutedEventArgs e)
    {
        Close();
    }
}