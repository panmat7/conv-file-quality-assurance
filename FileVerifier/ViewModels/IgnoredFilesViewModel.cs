using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using AvaloniaDraft.FileManager;

namespace AvaloniaDraft.ViewModels;

public sealed class IgnoredFilesViewModel : INotifyPropertyChanged
{
    private string? _message;
    public ObservableCollection<string> FilePaths { get; } = [];

    public string? Message
    {
        get => _message;
        set
        {
            _message = value;
            OnPropertyChanged();
        }
    }

    public IgnoredFilesViewModel(int totalFilePairs, List<IgnoredFile> ignoredFiles)
    {
        Message = $"{totalFilePairs} file pairs were created and are ready for verification";

        if (ignoredFiles.Count != 0)
        {
            foreach (var file in ignoredFiles)
            {
                FilePaths.Add(file.FilePath);
            }
        }
        else
        {
            FilePaths.Add("None");
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}