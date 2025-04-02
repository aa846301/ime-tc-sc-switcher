using System;
using System.Windows.Input;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Services;
using ReactiveUI;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using Avalonia.Threading;
using System.Runtime.InteropServices;

namespace ChineseInputSwitcher.ViewModels
{
    public class SettingsViewModel : ReactiveObject, IDisposable
    {
        private readonly AppSettings _settings;
        private readonly AppSettings _originalSettings;
        private readonly LocalizationService? _localizationService;
        
        private int _selectedLanguageIndex = 0;
        private string _imeToggleShortcut = "Alt+Shift+F";
        private string _notificationToggleShortcut = "Ctrl+Alt+Shift+X";
        
        public event EventHandler<bool>? RequestClose;
        
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
            get => _settings.EnableNotifications;
            set 
            {
                _settings.EnableNotifications = value;
                this.RaisePropertyChanged(nameof(EnableNotifications));
            }
        }
        
        public bool EnableMultiScreenNotifications
        {
            get => _settings.EnableMultiScreenNotifications;
            set 
            {
                _settings.EnableMultiScreenNotifications = value;
                this.RaisePropertyChanged(nameof(EnableMultiScreenNotifications));
            }
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
        
        public List<string> KeyOptions { get; } = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"
        };
        
        public bool IMEToggleCtrl
        {
            get => _settings.ToggleInputMethod.Ctrl;
            set
            {
                _settings.ToggleInputMethod.Ctrl = value;
                this.RaisePropertyChanged(nameof(IMEToggleCtrl));
            }
        }
        
        public bool IMEToggleAlt
        {
            get => _settings.ToggleInputMethod.Alt;
            set
            {
                _settings.ToggleInputMethod.Alt = value;
                this.RaisePropertyChanged(nameof(IMEToggleAlt));
            }
        }
        
        public bool IMEToggleShift
        {
            get => _settings.ToggleInputMethod.Shift;
            set
            {
                _settings.ToggleInputMethod.Shift = value;
                this.RaisePropertyChanged(nameof(IMEToggleShift));
            }
        }
        
        public bool IMEToggleWin
        {
            get => _settings.ToggleInputMethod.Win;
            set
            {
                _settings.ToggleInputMethod.Win = value;
                this.RaisePropertyChanged(nameof(IMEToggleWin));
            }
        }
        
        public int IMEToggleKeyIndex
        {
            get => GetKeyIndex(_settings.ToggleInputMethod.Key);
            set
            {
                _settings.ToggleInputMethod.Key = GetKeyCode(value);
                this.RaisePropertyChanged(nameof(IMEToggleKeyIndex));
            }
        }
        
        public bool NotificationToggleCtrl
        {
            get => _settings.ToggleNotification.Ctrl;
            set
            {
                _settings.ToggleNotification.Ctrl = value;
                this.RaisePropertyChanged(nameof(NotificationToggleCtrl));
            }
        }
        
        public bool NotificationToggleAlt
        {
            get => _settings.ToggleNotification.Alt;
            set
            {
                _settings.ToggleNotification.Alt = value;
                this.RaisePropertyChanged(nameof(NotificationToggleAlt));
            }
        }
        
        public bool NotificationToggleShift
        {
            get => _settings.ToggleNotification.Shift;
            set
            {
                _settings.ToggleNotification.Shift = value;
                this.RaisePropertyChanged(nameof(NotificationToggleShift));
            }
        }
        
        public bool NotificationToggleWin
        {
            get => _settings.ToggleNotification.Win;
            set
            {
                _settings.ToggleNotification.Win = value;
                this.RaisePropertyChanged(nameof(NotificationToggleWin));
            }
        }
        
        public int NotificationToggleKeyIndex
        {
            get => GetKeyIndex(_settings.ToggleNotification.Key);
            set
            {
                _settings.ToggleNotification.Key = GetKeyCode(value);
                this.RaisePropertyChanged(nameof(NotificationToggleKeyIndex));
            }
        }
        
        public ICommand SaveCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }
        public ICommand ResetIMEToggleCommand { get; private set; }
        public ICommand ResetNotificationToggleCommand { get; private set; }
        public ICommand ResetShortcutsCommand { get; private set; }
        
        public SettingsViewModel(AppSettings settings, LocalizationService? localizationService = null)
        {
            _originalSettings = settings;
            _settings = settings.Clone();
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
            
            // 初始化語言選擇
            InitializeLanguageSelection();
            
            // 初始化快捷鍵設置
            IMEToggleShortcut = FormatHotKey(_settings.ToggleInputMethod);
            NotificationToggleShortcut = FormatHotKey(_settings.ToggleNotification);
            
            // 使用最簡單的無參數命令初始化，避免線程問題
            SaveCommand = new RelayCommand(() => SafeSave());
            CancelCommand = new RelayCommand(() => SafeCancel());
            ResetIMEToggleCommand = new RelayCommand(() => SafeResetIMEToggleShortcut());
            ResetNotificationToggleCommand = new RelayCommand(() => SafeResetNotificationToggleShortcut());
            ResetShortcutsCommand = new RelayCommand(() => SafeResetAllShortcuts());
            
            // 訂閱語言變更事件
            App.LanguageChanged += OnLanguageChanged;
        }
        
        // 添加 RelayCommand 實現
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

        // 添加安全的操作方法
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

        // 更新 OnLanguageChanged 方法
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
            // 可能需要更新其他屬性...
        }

        // 在視圖模型被釋放時取消訂閱事件
        public void Dispose()
        {
            App.LanguageChanged -= OnLanguageChanged;
        }

        // 修改 ResetAllShortcuts 方法，確保在 UI 線程上執行
        private void ResetAllShortcuts()
        {
            // 確保在UI線程上執行
            if (Dispatcher.UIThread.CheckAccess())
            {
                // 已在UI線程上，直接執行
                ResetIMEToggleShortcut();
                ResetNotificationToggleShortcut();
            }
            else
            {
                // 不在UI線程上，需要調度到UI線程
                Dispatcher.UIThread.InvokeAsync(() => {
                    ResetIMEToggleShortcut();
                    ResetNotificationToggleShortcut();
                });
            }
        }

        // 完全重構 ResetIMEToggleShortcut 方法，確保所有操作都在UI線程上執行
        private void ResetIMEToggleShortcut()
        {
            var action = new Action(() => {
                try
                {
                    // 在UI線程內創建臨時HotKeySettings，避免直接修改可能觸發UI更新的屬性
                    var tempSettings = new HotKeySettings
                    {
                        Key = 70, // F key
                        Alt = true,
                        Shift = true,
                        Ctrl = false,
                        Win = false
                    };
                    
                    // 一次性設置所有屬性
                    _settings.ToggleInputMethod = tempSettings;
                    
                    // 更新顯示字符串，確保在最後才觸發UI更新
                    string newShortcut = FormatHotKey(_settings.ToggleInputMethod);
                    IMEToggleShortcut = newShortcut;
                    
                    // 強制通知屬性已變更，但使用批處理方式避免多次觸發UI更新
                    this.RaisePropertyChanged(nameof(IMEToggleAlt));
                    this.RaisePropertyChanged(nameof(IMEToggleShift));
                    this.RaisePropertyChanged(nameof(IMEToggleCtrl));
                    this.RaisePropertyChanged(nameof(IMEToggleWin));
                    this.RaisePropertyChanged(nameof(IMEToggleKeyIndex));
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"重置IME切換快捷鍵時出錯: {ex.Message}");
                }
            });
            
            // 確保在UI線程上執行所有操作
            if (Dispatcher.UIThread.CheckAccess())
            {
                action();
            }
            else
            {
                Dispatcher.UIThread.InvokeAsync(action).Wait(); // 使用Wait確保操作完成
            }
        }
        
        private void ResetNotificationToggleShortcut()
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.InvokeAsync(ResetNotificationToggleShortcut);
                return;
            }
            
            // 重置為 Ctrl+Alt+Shift+X
            _settings.ToggleNotification.Key = 88; // X key
            _settings.ToggleNotification.Ctrl = true;
            _settings.ToggleNotification.Alt = true;
            _settings.ToggleNotification.Shift = true;
            _settings.ToggleNotification.Win = false;
            
            // 更新顯示字符串
            NotificationToggleShortcut = FormatHotKey(_settings.ToggleNotification);
        }
        
        private int GetKeyIndex(int keyCode)
        {
            // 將按鍵代碼轉換為索引
            switch (keyCode)
            {
                // 數字鍵 0-9
                case 48: return 0;  // 0
                case 49: return 1;  // 1
                case 50: return 2;  // 2
                case 51: return 3;  // 3
                case 52: return 4;  // 4
                case 53: return 5;  // 5
                case 54: return 6;  // 6
                case 55: return 7;  // 7
                case 56: return 8;  // 8
                case 57: return 9;  // 9
                
                // 字母鍵 A-Z
                case 65: return 10;  // A
                case 66: return 11;  // B
                case 67: return 12;  // C
                case 68: return 13;  // D
                case 69: return 14;  // E
                case 70: return 15;  // F
                case 71: return 16;  // G
                case 72: return 17;  // H
                case 73: return 18;  // I
                case 74: return 19;  // J
                case 75: return 20;  // K
                case 76: return 21;  // L
                case 77: return 22;  // M
                case 78: return 23;  // N
                case 79: return 24;  // O
                case 80: return 25;  // P
                case 81: return 26;  // Q
                case 82: return 27;  // R
                case 83: return 28;  // S
                case 84: return 29;  // T
                case 85: return 30;  // U
                case 86: return 31;  // V
                case 87: return 32;  // W
                case 88: return 33;  // X
                case 89: return 34;  // Y
                case 90: return 35;  // Z
                
                // 功能鍵 F1-F12
                case 112: return 36;  // F1
                case 113: return 37;  // F2
                case 114: return 38;  // F3
                case 115: return 39;  // F4
                case 116: return 40;  // F5
                case 117: return 41;  // F6
                case 118: return 42;  // F7
                case 119: return 43;  // F8
                case 120: return 44;  // F9
                case 121: return 45;  // F10
                case 122: return 46;  // F11
                case 123: return 47;  // F12
                
                // 特殊鍵
                case 8: return 48;    // Backspace
                case 9: return 49;    // Tab
                case 13: return 50;   // Enter
                case 27: return 51;   // Escape
                case 32: return 52;   // Space
                case 45: return 53;   // Insert
                case 46: return 54;   // Delete
                case 36: return 55;   // Home
                case 35: return 56;   // End
                case 33: return 57;   // Page Up
                case 34: return 58;   // Page Down
                
                // 方向鍵
                case 37: return 59;   // Left Arrow
                case 38: return 60;   // Up Arrow
                case 39: return 61;   // Right Arrow
                case 40: return 62;   // Down Arrow
                
                // 默認值，如果沒有匹配的按鍵代碼，返回 A 的索引
                default: return 10;  // A
            }
        }
        
        private int GetKeyCode(int index)
        {
            switch (index)
            {
                // 數字鍵 0-9
                case 0: return 48;  // 0
                case 1: return 49;  // 1
                case 2: return 50;  // 2
                case 3: return 51;  // 3
                case 4: return 52;  // 4
                case 5: return 53;  // 5
                case 6: return 54;  // 6
                case 7: return 55;  // 7
                case 8: return 56;  // 8
                case 9: return 57;  // 9
                
                // 字母鍵 A-Z
                case 10: return 65;  // A
                case 11: return 66;  // B
                case 12: return 67;  // C
                case 13: return 68;  // D
                case 14: return 69;  // E
                case 15: return 70;  // F
                case 16: return 71;  // G
                case 17: return 72;  // H
                case 18: return 73;  // I
                case 19: return 74;  // J
                case 20: return 75;  // K
                case 21: return 76;  // L
                case 22: return 77;  // M
                case 23: return 78;  // N
                case 24: return 79;  // O
                case 25: return 80;  // P
                case 26: return 81;  // Q
                case 27: return 82;  // R
                case 28: return 83;  // S
                case 29: return 84;  // T
                case 30: return 85;  // U
                case 31: return 86;  // V
                case 32: return 87;  // W
                case 33: return 88;  // X
                case 34: return 89;  // Y
                case 35: return 90;  // Z
                
                // 功能鍵 F1-F12
                case 36: return 112;  // F1
                case 37: return 113;  // F2
                case 38: return 114;  // F3
                case 39: return 115;  // F4
                case 40: return 116;  // F5
                case 41: return 117;  // F6
                case 42: return 118;  // F7
                case 43: return 119;  // F8
                case 44: return 120;  // F9
                case 45: return 121;  // F10
                case 46: return 122;  // F11
                case 47: return 123;  // F12
                
                // 特殊鍵
                case 48: return 8;    // Backspace
                case 49: return 9;    // Tab
                case 50: return 13;   // Enter
                case 51: return 27;   // Escape
                case 52: return 32;   // Space
                case 53: return 45;   // Insert
                case 54: return 46;   // Delete
                case 55: return 36;   // Home
                case 56: return 35;   // End
                case 57: return 33;   // Page Up
                case 58: return 34;   // Page Down
                
                // 方向鍵
                case 59: return 37;   // Left Arrow
                case 60: return 38;   // Up Arrow
                case 61: return 39;   // Right Arrow
                case 62: return 40;   // Down Arrow
                
                // 默認值，如果沒有匹配的索引，返回 A 的代碼
                default: return 65;  // A
            }
        }

        public void NotifyWindowClosed()
        {
            // 通知窗口已關閉，但不觸發保存
            RequestClose?.Invoke(this, false);
        }

        // IMEToggleKeyIndex -> ToggleInputMethodKeyIndex 的別名
        public int ToggleInputMethodKeyIndex
        {
            get => IMEToggleKeyIndex;
            set => IMEToggleKeyIndex = value;
        }

        // 添加缺失的属性
        public int TextToSqlFormatKeyIndex
        {
            get => GetKeyIndex(_settings.TextToSqlFormat.Key);
            set
            {
                _settings.TextToSqlFormat.Key = GetKeyCode(value);
                this.RaisePropertyChanged(nameof(TextToSqlFormatKeyIndex));
            }
        }

        public int TextToKeyboardInputKeyIndex
        {
            get => GetKeyIndex(_settings.TextToKeyboardInput.Key);
            set
            {
                _settings.TextToKeyboardInput.Key = GetKeyCode(value);
                this.RaisePropertyChanged(nameof(TextToKeyboardInputKeyIndex));
            }
        }

        // 添加 NotificationToggleKeyIndex -> ToggleNotificationKeyIndex 的別名
        public int ToggleNotificationKeyIndex
        {
            get => NotificationToggleKeyIndex;
            set => NotificationToggleKeyIndex = value;
        }

        // 添加這些屬性和命令
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

        // 添加這個缺失的方法
        private void InitializeLanguageSelection()
        {
            // 已在構造函數中設置了 _selectedLanguageIndex
            // 這裡什麼都不需要做
        }

        // 添加格式化熱鍵的方法
        private string FormatHotKey(HotKeySettings hotKey)
        {
            string result = "";
            if (hotKey.Ctrl) result += "Ctrl+";
            if (hotKey.Alt) result += "Alt+";
            if (hotKey.Shift) result += "Shift+";
            if (hotKey.Win) result += "Win+";
            
            // 獲取按鍵名稱
            char keyChar = (char)hotKey.Key;
            result += keyChar;
            
            return result;
        }

        // 添加這些新屬性
        public bool IsWindowsPlatform => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public bool EnableTextConversion
        {
            get => _settings.EnableTextConversion;
            set 
            {
                _settings.EnableTextConversion = value;
                this.RaisePropertyChanged(nameof(EnableTextConversion));
            }
        }

        public bool EnableClipboardToKeyboard
        {
            get => _settings.EnableClipboardToKeyboard;
            set 
            {
                _settings.EnableClipboardToKeyboard = value;
                this.RaisePropertyChanged(nameof(EnableClipboardToKeyboard));
            }
        }

        // 添加 SQL 格式相關的屬性
        public bool TextToSqlFormatCtrl
        {
            get => _settings.TextToSqlFormat.Ctrl;
            set
            {
                _settings.TextToSqlFormat.Ctrl = value;
                this.RaisePropertyChanged(nameof(TextToSqlFormatCtrl));
            }
        }

        public bool TextToSqlFormatAlt
        {
            get => _settings.TextToSqlFormat.Alt;
            set
            {
                _settings.TextToSqlFormat.Alt = value;
                this.RaisePropertyChanged(nameof(TextToSqlFormatAlt));
            }
        }

        public bool TextToSqlFormatShift
        {
            get => _settings.TextToSqlFormat.Shift;
            set
            {
                _settings.TextToSqlFormat.Shift = value;
                this.RaisePropertyChanged(nameof(TextToSqlFormatShift));
            }
        }

        public bool TextToSqlFormatWin
        {
            get => _settings.TextToSqlFormat.Win;
            set
            {
                _settings.TextToSqlFormat.Win = value;
                this.RaisePropertyChanged(nameof(TextToSqlFormatWin));
            }
        }

        // 添加鍵盤輸入相關的屬性
        public bool TextToKeyboardInputCtrl
        {
            get => _settings.TextToKeyboardInput.Ctrl;
            set
            {
                _settings.TextToKeyboardInput.Ctrl = value;
                this.RaisePropertyChanged(nameof(TextToKeyboardInputCtrl));
            }
        }

        public bool TextToKeyboardInputAlt
        {
            get => _settings.TextToKeyboardInput.Alt;
            set
            {
                _settings.TextToKeyboardInput.Alt = value;
                this.RaisePropertyChanged(nameof(TextToKeyboardInputAlt));
            }
        }

        public bool TextToKeyboardInputShift
        {
            get => _settings.TextToKeyboardInput.Shift;
            set
            {
                _settings.TextToKeyboardInput.Shift = value;
                this.RaisePropertyChanged(nameof(TextToKeyboardInputShift));
            }
        }

        public bool TextToKeyboardInputWin
        {
            get => _settings.TextToKeyboardInput.Win;
            set
            {
                _settings.TextToKeyboardInput.Win = value;
                this.RaisePropertyChanged(nameof(TextToKeyboardInputWin));
            }
        }

        // 添加安全版本的重置方法
        private void SafeResetIMEToggleShortcut()
        {
            try
            {
                var tempSettings = new HotKeySettings
                {
                    Key = 70, // F key
                    Alt = true,
                    Shift = true,
                    Ctrl = false,
                    Win = false
                };
                
                _settings.ToggleInputMethod = tempSettings;
                IMEToggleShortcut = FormatHotKey(tempSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置快捷鍵時出錯: {ex.Message}");
            }
        }
        
        private void SafeResetNotificationToggleShortcut()
        {
            try
            {
                var tempSettings = new HotKeySettings
                {
                    Key = 88, // X key
                    Ctrl = true,
                    Alt = true,
                    Shift = true,
                    Win = false
                };
                
                _settings.ToggleNotification = tempSettings;
                NotificationToggleShortcut = FormatHotKey(tempSettings);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置通知快捷鍵時出錯: {ex.Message}");
            }
        }
        
        private void SafeResetAllShortcuts()
        {
            SafeResetIMEToggleShortcut();
            SafeResetNotificationToggleShortcut();
        }

        // 添加對 TextToSqlFormat 和 TextToKeyboardInput 的重置方法
        private void SafeResetTextToSqlFormatShortcut()
        {
            try
            {
                var tempSettings = new HotKeySettings
                {
                    Key = 81, // Q key
                    Ctrl = true,
                    Alt = true,
                    Shift = false,
                    Win = false
                };
                
                _settings.TextToSqlFormat = tempSettings;
                this.RaisePropertyChanged(nameof(TextToSqlFormatCtrl));
                this.RaisePropertyChanged(nameof(TextToSqlFormatAlt));
                this.RaisePropertyChanged(nameof(TextToSqlFormatShift));
                this.RaisePropertyChanged(nameof(TextToSqlFormatWin));
                this.RaisePropertyChanged(nameof(TextToSqlFormatKeyIndex));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置SQL格式快捷鍵時出錯: {ex.Message}");
            }
        }

        private void SafeResetTextToKeyboardInputShortcut()
        {
            try
            {
                var tempSettings = new HotKeySettings
                {
                    Key = 75, // K key
                    Ctrl = true,
                    Alt = true,
                    Shift = false,
                    Win = false
                };
                
                _settings.TextToKeyboardInput = tempSettings;
                this.RaisePropertyChanged(nameof(TextToKeyboardInputCtrl));
                this.RaisePropertyChanged(nameof(TextToKeyboardInputAlt));
                this.RaisePropertyChanged(nameof(TextToKeyboardInputShift));
                this.RaisePropertyChanged(nameof(TextToKeyboardInputWin));
                this.RaisePropertyChanged(nameof(TextToKeyboardInputKeyIndex));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"重置鍵盤輸入快捷鍵時出錯: {ex.Message}");
            }
        }
    }
} 