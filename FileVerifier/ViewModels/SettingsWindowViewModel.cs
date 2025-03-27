using AvaloniaDraft.Helpers;
using System.Collections.Generic;
    using System.ComponentModel;
using System.Diagnostics;
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
        private bool _isIgnoreUnsupportedFormatsEnabled;

        private double _sizeComparisonThreshold;
        private double _pbpComparisonThreshold;

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

            _sizeComparisonThreshold = GlobalVariables.Options.SizeComparisonThreshold;
            _pbpComparisonThreshold = GlobalVariables.Options.PbpComparisonThreshold;

            _isSizeEnabled = GlobalVariables.Options.GetMethod(Methods.Size.Name);
            _isResolutionEnabled = GlobalVariables.Options.GetMethod(Methods.Resolution.Name);
            _isFontEnabled = GlobalVariables.Options.GetMethod(Methods.Fonts.Name);
            _isPbPEnabled = GlobalVariables.Options.GetMethod(Methods.PointByPoint.Name);
            _isPageCountEnabled = GlobalVariables.Options.GetMethod(Methods.Pages.Name);
            _isColorEnabled = GlobalVariables.Options.GetMethod(Methods.ColorProfile.Name);
            _isAnimationEnabled = GlobalVariables.Options.GetMethod(Methods.Animations.Name);

            _isIgnoreUnsupportedFormatsEnabled = GlobalVariables.Options.IgnoreUnsupportedFileType;
        }


        public double SizeComparisonThreshold
        {
            get => _sizeComparisonThreshold;
            set
            {
                if (_sizeComparisonThreshold == value) return;
                _sizeComparisonThreshold = value;
                GlobalVariables.Options.SizeComparisonThreshold = value;
                OnPropertyChanged(nameof(SizeComparisonThreshold));
            }
        }

        public double PbpComparisonThreshold
    {
            get => _pbpComparisonThreshold;
            set
            {
                if (_pbpComparisonThreshold == value) return;
                _pbpComparisonThreshold = value;
                GlobalVariables.Options.PbpComparisonThreshold = value;
                OnPropertyChanged(nameof(PbpComparisonThreshold));
            }
        }

        public bool IsIgnoreUnsupportedFormatsEnabled
        {
            get => _isIgnoreUnsupportedFormatsEnabled;
            set
            {
                if (_isIgnoreUnsupportedFormatsEnabled == value) return;
                _isIgnoreUnsupportedFormatsEnabled = value;
                GlobalVariables.Options.IgnoreUnsupportedFileType = value;
                OnPropertyChanged(nameof(IsIgnoreUnsupportedFormatsEnabled));
            }
        }

        public bool IsSizeEnabled
        {
            get => _isSizeEnabled;
            set
            {
                if (_isSizeEnabled == value) return;
                _isSizeEnabled = value;
                GlobalVariables.Options.SetMethod(Methods.Size.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.Resolution.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.Fonts.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.PointByPoint.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.Pages.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.ColorProfile.Name, value);
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
                GlobalVariables.Options.SetMethod(Methods.Animations.Name, value);
                OnPropertyChanged(nameof(IsAnimationEnabled));
            }
        }
        
        public event PropertyChangedEventHandler? PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }