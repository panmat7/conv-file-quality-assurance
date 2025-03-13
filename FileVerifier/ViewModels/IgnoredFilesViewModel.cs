using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace AvaloniaDraft.ViewModels;

public class IgnoredFilesViewModel : INotifyPropertyChanged
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

    public IgnoredFilesViewModel(int totalFilePairs, List<string> filePaths)
    {
        Message = $"{totalFilePairs} file pairs created and ready for verification";

        if (filePaths.Count != 0)
        {
            foreach (var path in filePaths)
            {
                FilePaths.Add(path);
            }
        }
        else
        {
            FilePaths.Add("None");
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}