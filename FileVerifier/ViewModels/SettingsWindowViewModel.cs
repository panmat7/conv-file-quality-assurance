using System.ComponentModel;

namespace AvaloniaDraft.ViewModels;

public sealed class SettingsViewModel : INotifyPropertyChanged
{
    private bool _isSizeEnabled;
    private bool _isResolutionEnabled;
    private bool _isFontEnabled;
    private bool _isPbPEnabled;
    private bool _isPageCountEnabled;
    private bool _isColorEnabled;
    private bool _isAnimationEnabled;

    public bool IsSizeEnabled
    {
        get => _isSizeEnabled;
        set
        {
            if (_isSizeEnabled == value) return;
            _isSizeEnabled = value;
            OnPropertyChanged(nameof(IsSizeEnabled));
        }
    }
    
    public bool IsResolutionEnabled
    {
        get => _isResolutionEnabled;
        set
        {
            if (_isResolutionEnabled == value) return;
            _isResolutionEnabled = value;
            OnPropertyChanged(nameof(IsResolutionEnabled));
        }
    }
    
    public bool IsFontEnabled
    {
        get => _isFontEnabled;
        set
        {
            if (_isFontEnabled == value) return;
            _isFontEnabled = value;
            OnPropertyChanged(nameof(IsFontEnabled));
        }
    }
    
    public bool IsPointByPointEnabled
    {
        get => _isPbPEnabled;
        set
        {
            if (_isPbPEnabled == value) return;
            _isPbPEnabled = value;
            OnPropertyChanged(nameof(IsPointByPointEnabled));
        }
    }
    
    public bool IsPageCountEnabled
    {
        get => _isPageCountEnabled;
        set
        {
            if (_isPageCountEnabled == value) return;
            _isPageCountEnabled = value;
            OnPropertyChanged(nameof(IsPageCountEnabled));
        }
    }
    
    public bool IsColorProfileEnabled
    {
        get => _isColorEnabled;
        set
        {
            if (_isColorEnabled == value) return;
            _isColorEnabled = value;
            OnPropertyChanged(nameof(IsColorProfileEnabled));
        }
    }
    
    public bool IsAnimationEnabled
    {
        get => _isAnimationEnabled;
        set
        {
            if (_isAnimationEnabled == value) return;
            _isAnimationEnabled = value;
            OnPropertyChanged(nameof(IsAnimationEnabled));
        }
    }
    
    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}