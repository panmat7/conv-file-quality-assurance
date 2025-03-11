    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    namespace AvaloniaDraft.ViewModels;

    public sealed class WindowSizeOption
    {
        public string Name { get; set; } = string.Empty;
        public double Width { get; set; }
        public double Height { get; set; }
    }

    public sealed class SettingsViewModel : INotifyPropertyChanged
    {
        private bool _isSizeEnabled;
        private bool _isResolutionEnabled;
        private bool _isFontEnabled;
        private bool _isPbPEnabled;
        private bool _isPageCountEnabled;
        private bool _isColorEnabled;
        private bool _isAnimationEnabled;

        public List<WindowSizeOption?> AvailableWindowSizes { get; } =
        [
            new WindowSizeOption { Name = "Small (800x600)",    Width = 800,  Height = 600 },
            new WindowSizeOption { Name = "Medium (1024x768)",  Width = 1024, Height = 768 },
            new WindowSizeOption { Name = "Large (1366x768)",   Width = 1366, Height = 768 },
            new WindowSizeOption { Name = "X-Large (1440x900)",  Width = 1440, Height = 900 },
            new WindowSizeOption { Name = "Full HD (1920x1080)", Width = 1920, Height = 1080 }
        ];
        
        private WindowSizeOption? _selectedWindowSize;
        public WindowSizeOption? SelectedWindowSize
        {
            get => _selectedWindowSize;
            set
            {
                if (_selectedWindowSize == value) return;
                _selectedWindowSize = value;
                OnPropertyChanged(nameof(SelectedWindowSize));
            }
        }
        
        public SettingsViewModel()
        {
            // Set default to the first option:
            _selectedWindowSize = AvailableWindowSizes.Skip(1).FirstOrDefault();
        }


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