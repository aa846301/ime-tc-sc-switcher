using System;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Media;
using Avalonia.Threading;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Services;
using ChineseInputSwitcher.Views;
using ReactiveUI;
using System.Runtime.InteropServices;
using Avalonia.Controls;

namespace ChineseInputSwitcher.ViewModels
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private readonly AppSettings? _settings;
        private readonly IPlatformService? _platformService;
        private readonly NotificationService? _notificationService;
        private readonly TextTransformService? _textTransformService;
        private readonly LocalizationService? _localizationService;
        private SettingsViewModel? _settingsViewModel;

        private string _currentInputMethodState = "未知";
        private string _textInput = "";
        private int _selectedLanguageIndex = 0;

        public string CurrentInputMethodState
        {
            get => _currentInputMethodState;
            private set => this.RaiseAndSetIfChanged(ref _currentInputMethodState, value);
        }

        public IBrush CurrentInputMethodColor =>
            CurrentInputMethodState == "簡體" ?
                new SolidColorBrush(Colors.Red) :
                new SolidColorBrush(Colors.Blue);

        public string TextInput
        {
            get => _textInput;
            set => this.RaiseAndSetIfChanged(ref _textInput, value);
        }

        public ICommand ToggleInputMethodCommand { get; private set; }
        public ICommand OpenSettingsCommand { get; private set; }
        public ICommand ConvertToSqlFormatCommand { get; private set; }
        public ICommand SimulateKeyboardInputCommand { get; private set; }

        public bool IsWindowsPlatform => _platformService?.IsSupported == true && RuntimeInformation.IsOSPlatform(OSPlatform.Windows);

        public int SelectedLanguageIndex
        {
            get => _selectedLanguageIndex;
            set
            {
                if (_selectedLanguageIndex != value)
                {
                    this.RaiseAndSetIfChanged(ref _selectedLanguageIndex, value);
                    // 當用戶選擇新語言時，更新設置並切換語言
                    ChangeLanguage(_selectedLanguageIndex);
                }
            }
        }

        public SettingsViewModel SettingsViewModel
        {
            get
            {
                if (_settingsViewModel == null && _settings != null)
                {
                    _settingsViewModel = new SettingsViewModel(_settings, _localizationService);
                }
                return _settingsViewModel!;
            }
        }

        public string AppTitle => Resources.AppTitle;
        public string LangLanguage => Resources.Language;
        public string LangFollowSystem => Resources.FollowSystem;
        public string LangCurrentInputState => Resources.CurrentInputState;
        public string LangTextConversionTool => Resources.TextConversionTool;
        public string LangInputOrPasteText => Resources.InputOrPasteText;
        public string LangSQLFormatConversion => Resources.ConvertToSQLFormat;
        public string LangKeyboardInputSimulation => Resources.SimulateKeyboardInput;
        public string LangShortcuts => Resources.Shortcuts;
        public string LangIMEToggleShortcut => Resources.IMEToggle;
        public string LangNotificationToggleShortcut => Resources.NotificationToggle;
        public string LangSQLFormatShortcut => Resources.SQLFormat;
        public string LangKeyboardInputShortcut => Resources.KeyboardInput;
        public string LangToggleIME => Resources.IMEToggle;
        public string LangSettings => Resources.Settings;

        public MainViewModel()
        {
            // 用于设计时，不应该执行任何逻辑
        }

        public MainViewModel(AppSettings settings, IPlatformService platformService, NotificationService notificationService, TextTransformService textTransformService, LocalizationService localizationService)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _platformService = platformService;
            _notificationService = notificationService;
            _textTransformService = textTransformService;
            _localizationService = localizationService;
            
            // 使用 Dispatcher 初始化命令
            Dispatcher.UIThread.Post(() => {
                // 初始化命令
                ToggleInputMethodCommand = ReactiveCommand.CreateFromTask(ToggleInputMethod);
                OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);
                ConvertToSqlFormatCommand = ReactiveCommand.CreateFromTask(ConvertToSqlFormat);
                SimulateKeyboardInputCommand = ReactiveCommand.CreateFromTask(SimulateKeyboardInput);
            });
            
            // 初始化資源字符串
            UpdateLocalizedResources();
            
            // 訂閱語言變更事件
            App.LanguageChanged += OnLanguageChanged;
            
            // 初始化其他設置...
        }

        private async Task UpdateInputMethodState()
        {
            if (_platformService != null)
            {
                var state = _platformService.GetCurrentInputMethodState();
                CurrentInputMethodState = state;
                this.RaisePropertyChanged(nameof(CurrentInputMethodColor));
            }
        }

        private async Task ToggleInputMethod()
        {
            if (!IsWindowsPlatform)
            {
                if (_notificationService != null)
                    await _notificationService.ShowNotification("僅在Windows平台支持此功能");
                return;
            }

            bool success = false;
            if (_platformService != null)
                success = await _platformService.ToggleChineseInputMethod();

            if (success)
            {
                await UpdateInputMethodState();
                if (_notificationService != null && !string.IsNullOrEmpty(CurrentInputMethodState))
                {
                    await _notificationService.ShowNotification(CurrentInputMethodState);
                }
            }
            else
            {
                if (_notificationService != null)
                    await _notificationService.ShowNotification("輸入法切換失敗");
            }
        }

        private async Task SimulateKeyboardInput()
        {
            if (string.IsNullOrWhiteSpace(TextInput) || _platformService == null)
                return;

            _platformService.SimulateKeyboardInput(TextInput);

            if (_notificationService != null)
            {
                await _notificationService.ShowNotification("已模拟键盘输入");
            }
        }

        private void OpenSettings()
        {
            var settingsViewModel = new SettingsViewModel(_settings, _localizationService);
            var settingsWindow = new SettingsWindow(settingsViewModel)
            {
                WindowStartupLocation = Avalonia.Controls.WindowStartupLocation.CenterOwner
            };

            if (Avalonia.Application.Current?.ApplicationLifetime is Avalonia.Controls.ApplicationLifetimes.IClassicDesktopStyleApplicationLifetime desktop)
            {
                settingsWindow.ShowDialog(desktop.MainWindow);
            }
        }

        private async Task ConvertToSqlFormat()
        {
            if (string.IsNullOrWhiteSpace(TextInput))
                return;
            
            string result = _textTransformService?.ConvertToSqlFormat(TextInput) ?? TextInput;
            TextInput = result;
            
            if (_notificationService != null)
                await _notificationService.ShowNotification(Resources.SQLFormatComplete);
        }

        private void InitializeLanguageSelection()
        {
            // 根據設置選擇語言索引
            switch (_settings?.Language)
            {
                case "system":
                    _selectedLanguageIndex = 0;
                    break;
                case "zh-Hant":
                    _selectedLanguageIndex = 1;
                    break;
                case "zh-Hans":
                    _selectedLanguageIndex = 2;
                    break;
                case "en":
                    _selectedLanguageIndex = 3;
                    break;
                case "ja":
                    _selectedLanguageIndex = 4;
                    break;
                default:
                    _selectedLanguageIndex = 0;
                    break;
            }
        }

        private void ChangeLanguage(int languageIndex)
        {
            string languageCode;
            
            // 將索引轉換為語言代碼
            switch (languageIndex)
            {
                case 0: // 跟隨系統
                    languageCode = "system";
                    break;
                case 1: // 繁體中文
                    languageCode = "zh-Hant";
                    break;
                case 2: // 簡體中文
                    languageCode = "zh-Hans";
                    break;
                case 3: // 英文
                    languageCode = "en";
                    break;
                case 4: // 日文
                    languageCode = "ja";
                    break;
                default:
                    languageCode = "system";
                    break;
            }
            
            // 更新設置
            _settings.Language = languageCode;
            _settings.Save();
            
            // 通過本地化服務切換語言
            _localizationService.SwitchLanguage(languageCode);
            
            // 重新加載資源和屬性
            UpdateLocalizedResources();
        }

        // 更新本地化資源
        private void UpdateLocalizedResources()
        {
            Dispatcher.UIThread.Post(() => {
                // 觸發所有綁定的本地化文本屬性更新
                this.RaisePropertyChanged(nameof(AppTitle));
                this.RaisePropertyChanged(nameof(LangLanguage));
                this.RaisePropertyChanged(nameof(LangFollowSystem));
                this.RaisePropertyChanged(nameof(LangCurrentInputState));
                this.RaisePropertyChanged(nameof(LangTextConversionTool));
                this.RaisePropertyChanged(nameof(LangInputOrPasteText));
                this.RaisePropertyChanged(nameof(LangSQLFormatConversion));
                this.RaisePropertyChanged(nameof(LangKeyboardInputSimulation));
                this.RaisePropertyChanged(nameof(LangShortcuts));
                this.RaisePropertyChanged(nameof(LangIMEToggleShortcut));
                this.RaisePropertyChanged(nameof(LangNotificationToggleShortcut));
                this.RaisePropertyChanged(nameof(LangSQLFormatShortcut));
                this.RaisePropertyChanged(nameof(LangKeyboardInputShortcut));
                this.RaisePropertyChanged(nameof(LangToggleIME));
                this.RaisePropertyChanged(nameof(LangSettings));
            });
        }

        // 處理語言變更事件
        private void OnLanguageChanged(object? sender, EventArgs e)
        {
            UpdateLocalizedResources();
        }

        // 在視圖模型被釋放時取消訂閱事件
        public void Dispose()
        {
            App.LanguageChanged -= OnLanguageChanged;
        }
    }
}