using System;
using System.Windows.Input;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Threading;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Avalonia.Input;

namespace ChineseInputSwitcher.ViewModels
{
    public class SettingsViewModel : ReactiveObject, IDisposable, INotifyPropertyChanged
    {
        private readonly AppSettings _settings;
        private readonly AppSettings _originalSettings;
        private readonly LocalizationService? _localizationService;
        
        private int _selectedLanguageIndex = 0;
        private string _imeToggleShortcut = "Alt+Shift+F";
        private string _notificationToggleShortcut = "Ctrl+Alt+Shift+X";
        private string _textToSqlFormatShortcut = "Ctrl+Alt+S";
        private string _textToKeyboardInputShortcut = "Ctrl+Alt+K";
        private bool _isWindowsPlatform;
        private bool _enableTextConversion;
        private bool _enableClipboardToKeyboard;
        private bool _enableNotifications;
        private bool _enableMultiScreenNotifications;
        
        public event EventHandler<bool>? RequestClose;
        public new event PropertyChangedEventHandler PropertyChanged;
        
        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set 
            {
                this.RaiseAndSetIfChanged(ref _selectedLanguageIndex, value);
                
                // 將索引轉換為語言代碼
                string languageCode;
                switch (value)
                {
                    case 0: languageCode = "system"; break;
                    case 1: languageCode = "zh-Hant"; break;
                    case 2: languageCode = "zh-Hans"; break;
                    case 3: languageCode = "en"; break;
                    case 4: languageCode = "ja"; break;
                    default: languageCode = "system"; break;
                }
                
                // 更新設置
                _settings.Language = languageCode;
                
                // 如果提供了本地化服務，就即時切換語言顯示
                _localizationService?.SwitchLanguage(languageCode);
            }
        }
        
        public bool EnableNotifications
        {
            get => _enableNotifications;
            set => this.RaiseAndSetIfChanged(ref _enableNotifications, value);
        }
        
        public bool EnableMultiScreenNotifications
        {
            get => _enableMultiScreenNotifications;
            set => this.RaiseAndSetIfChanged(ref _enableMultiScreenNotifications, value);
        }
        
        public bool EnableOnWindows
        {
            get => _settings.EnableOnWindows;
            set 
            {
                _settings.EnableOnWindows = value;
                this.RaisePropertyChanged(nameof(EnableOnWindows));
            }
        }
        
        public bool EnableOnMacOS
        {
            get => _settings.EnableOnMacOS;
            set 
            {
                _settings.EnableOnMacOS = value;
                this.RaisePropertyChanged(nameof(EnableOnMacOS));
            }
        }
        
        public bool EnableOnLinux
        {
            get => _settings.EnableOnLinux;
            set 
            {
                _settings.EnableOnLinux = value;
                this.RaisePropertyChanged(nameof(EnableOnLinux));
            }
        }
        
        public bool EnableIMEToggleOnWindows
        {
            get => _settings.EnableIMEToggleOnWindows;
            set 
            {
                _settings.EnableIMEToggleOnWindows = value;
                this.RaisePropertyChanged(nameof(EnableIMEToggleOnWindows));
            }
        }
        
        public bool EnableIMEToggleOnMacOS
        {
            get => _settings.EnableIMEToggleOnMacOS;
            set 
            {
                _settings.EnableIMEToggleOnMacOS = value;
                this.RaisePropertyChanged(nameof(EnableIMEToggleOnMacOS));
            }
        }
        
        public bool EnableIMEToggleOnLinux
        {
            get => _settings.EnableIMEToggleOnLinux;
            set 
            {
                _settings.EnableIMEToggleOnLinux = value;
                this.RaisePropertyChanged(nameof(EnableIMEToggleOnLinux));
            }
        }
        
        public bool EnableTextConversion
        {
            get => _enableTextConversion;
            set => this.RaiseAndSetIfChanged(ref _enableTextConversion, value);
        }
        
        public bool EnableClipboardToKeyboard
        {
            get => _enableClipboardToKeyboard;
            set => this.RaiseAndSetIfChanged(ref _enableClipboardToKeyboard, value);
        }
        
        // Properties for shortcut combinations
        public string IMEToggleShortcut
        {
            get => _imeToggleShortcut;
            set => this.RaiseAndSetIfChanged(ref _imeToggleShortcut, value);
        }

        public string NotificationToggleShortcut
        {
            get => _notificationToggleShortcut;
            set => this.RaiseAndSetIfChanged(ref _notificationToggleShortcut, value);
        }

        public string TextToSqlFormatShortcut
        {
            get => _textToSqlFormatShortcut;
            set => this.RaiseAndSetIfChanged(ref _textToSqlFormatShortcut, value);
        }

        public string TextToKeyboardInputShortcut
        {
            get => _textToKeyboardInputShortcut;
            set => this.RaiseAndSetIfChanged(ref _textToKeyboardInputShortcut, value);
        }
        
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand ResetIMEToggleCommand { get; private set; }
        public ICommand ResetNotificationToggleCommand { get; private set; }
        public ICommand ResetShortcutsCommand { get; private set; }
        public ICommand ResetTextToSqlFormatCommand { get; private set; }
        public ICommand ResetTextToKeyboardInputCommand { get; private set; }
        
        public bool IsWindowsPlatform
        {
            get => _isWindowsPlatform;
            set => this.RaiseAndSetIfChanged(ref _isWindowsPlatform, value);
        }
        
        public SettingsViewModel(AppSettings settings, LocalizationService? localizationService = null)
        {
            _settings = settings.Clone();
            _originalSettings = settings;
            _localizationService = localizationService;
            
            // 設置初始語言選擇
            switch (_settings.Language)
            {
                case "system": _selectedLanguageIndex = 0; break;
                case "zh-Hant": _selectedLanguageIndex = 1; break;
                case "zh-Hans": _selectedLanguageIndex = 2; break;
                case "en": _selectedLanguageIndex = 3; break;
                case "ja": _selectedLanguageIndex = 4; break;
                default: _selectedLanguageIndex = 0; break;
            }
            
            InitializeCommands();
            
            // 訂閱語言變更事件
            App.LanguageChanged += OnLanguageChanged;
        }
        
        private void InitializeCommands()
        {
            SaveCommand = new RelayCommand(() => SafeSave());
            CancelCommand = new RelayCommand(() => SafeCancel());
            ResetIMEToggleCommand = new RelayCommand(() => ResetIMEToggle());
            ResetNotificationToggleCommand = new RelayCommand(() => ResetNotificationToggle());
            ResetShortcutsCommand = new RelayCommand(() => ResetAllShortcuts());
            ResetTextToSqlFormatCommand = new RelayCommand(() => ResetTextToSqlFormat());
            ResetTextToKeyboardInputCommand = new RelayCommand(() => ResetTextToKeyboardInput());
        }

        private void ResetIMEToggle()
        {
            try
            {
                _imeToggleShortcut = "Alt+Shift+F";
                this.RaisePropertyChanged(nameof(IMEToggleShortcut));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置IME切換快捷鍵時發生錯誤: {ex.Message}");
            }
        }

        private void ResetNotificationToggle()
        {
            try
            {
                _notificationToggleShortcut = "Ctrl+Alt+Shift+X";
                this.RaisePropertyChanged(nameof(NotificationToggleShortcut));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置通知切換快捷鍵時發生錯誤: {ex.Message}");
            }
        }

        private void ResetAllShortcuts()
        {
            try
            {
                ResetIMEToggle();
                ResetNotificationToggle();
                _textToSqlFormatShortcut = "Ctrl+Alt+S";
                this.RaisePropertyChanged(nameof(TextToSqlFormatShortcut));
                _textToKeyboardInputShortcut = "Ctrl+Alt+K";
                this.RaisePropertyChanged(nameof(TextToKeyboardInputShortcut));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置所有快捷鍵時發生錯誤: {ex.Message}");
            }
        }
        
        private void ResetTextToSqlFormat()
        {
            try
            {
                _textToSqlFormatShortcut = "Ctrl+Alt+S";
                this.RaisePropertyChanged(nameof(TextToSqlFormatShortcut));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置SQL格式轉換快捷鍵時發生錯誤: {ex.Message}");
            }
        }

        private void ResetTextToKeyboardInput()
        {
            try
            {
                _textToKeyboardInputShortcut = "Ctrl+Alt+K";
                this.RaisePropertyChanged(nameof(TextToKeyboardInputShortcut));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置鍵盤輸入快捷鍵時發生錯誤: {ex.Message}");
            }
        }
        
        private class RelayCommand : ICommand
        {
            private readonly Action _execute;
            
            public RelayCommand(Action execute)
            {
                _execute = execute;
            }
            
            public bool CanExecute(object? parameter) => true;
            
            public void Execute(object? parameter)
            {
                try
                {
                    // 在 UI 線程上執行命令
                    if (Dispatcher.UIThread.CheckAccess())
                    {
                        _execute();
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(_execute);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"命令執行出錯: {ex.Message}");
                }
            }
            
            public event EventHandler? CanExecuteChanged;
        }
        
        private void SafeSave()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(SafeSave);
                return;
            }
            
            try
            {
                _originalSettings.CopyFrom(_settings);
                _originalSettings.Save();
                _localizationService?.SwitchLanguage(_settings.Language);
                RequestClose?.Invoke(this, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存設置時出錯: {ex.Message}");
            }
        }
        
        private void SafeCancel()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(SafeCancel);
                return;
            }
            
            try
            {
                RequestClose?.Invoke(this, false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"取消設置時出錯: {ex.Message}");
            }
        }
        
        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            // 確保在 UI 線程上執行
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(() => OnLanguageChanged(sender, e));
                return;
            }
            
            // 通知所有綁定的屬性進行更新
            this.RaisePropertyChanged(nameof(IMEToggleShortcut));
            this.RaisePropertyChanged(nameof(NotificationToggleShortcut));
        }
        
        public void Dispose()
        {
            App.LanguageChanged -= OnLanguageChanged;
        }
        
        public void NotifyWindowClosed()
        {
            RequestClose?.Invoke(this, false);
        }
        
        protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}