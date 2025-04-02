using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Services;
using ChineseInputSwitcher.ViewModels;
using ChineseInputSwitcher.Views;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace ChineseInputSwitcher
{
    public partial class App : Application
    {
        private AppSettings? _settings;
        private IPlatformService? _platformService;
        private NotificationService? _notificationService;
        private TextTransformService? _textTransformService;
        private ClipboardService? _clipboardService;
        private HotKeyService? _hotKeyService;
        private TrayIconService? _trayIconService;
        private bool _isFirstRun = true;
        private LocalizationService? _localizationService;

        public static event EventHandler? LanguageChanged;

        public static void NotifyLanguageChanged()
        {
            Console.WriteLine("通知應用程序語言已更改");
            LanguageChanged?.Invoke(null, EventArgs.Empty);
        }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 初始化語言設置
            _settings = AppSettings.Load();
            
            // 初始化本地化服務
            _localizationService = new LocalizationService(_settings);
            Console.WriteLine("正在初始化應用程序語言...");
            _localizationService.InitializeLanguage();
            Console.WriteLine($"當前UI文化: {System.Threading.Thread.CurrentThread.CurrentUICulture.Name}");
            
            // 初始化服务
            _clipboardService = new ClipboardService();
            _textTransformService = new TextTransformService();
            _notificationService = new NotificationService(_settings);
            
            // 根据操作系统选择平台服务
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                _platformService = new WindowsPlatformService(_settings);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                _platformService = new MacOSPlatformService(_settings);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                _platformService = new LinuxPlatformService(_settings);
            }
            
            // 初始化热键服务
            _hotKeyService = new HotKeyService(
                _settings, 
                _platformService, 
                _notificationService, 
                _textTransformService
            );
            
            // 如果是Windows，設置熱鍵服務到平台服務
            if (_platformService is WindowsPlatformService windowsPlatformService)
            {
                windowsPlatformService.SetHotKeyService(_hotKeyService);
            }

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var mainViewModel = new MainViewModel(_settings, _platformService, _notificationService, _textTransformService, _localizationService);
                desktop.MainWindow = new MainWindow
                {
                    DataContext = mainViewModel
                };
                
                // 初始化托盤图标 - 必須在MainWindow創建後初始化
                _trayIconService = new TrayIconService(_settings, _platformService, _notificationService);
                _trayIconService.Initialize(desktop.MainWindow);
                
                // 注册热键
                _hotKeyService.RegisterAllHotKeys();
                
                desktop.Exit += OnApplicationExit;
                
                // 顯示歡迎通知
                if (_isFirstRun && _settings.EnableNotifications)
                {
                    Avalonia.Threading.Dispatcher.UIThread.Post(async () =>
                    {
                        await Task.Delay(1000); // 給應用程序一點時間初始化
                        if (_notificationService != null)
                        {
                            await _notificationService.ShowNotification("歡迎使用中文輸入工具箱");
                        }
                    });
                    _isFirstRun = false;
                }
                
                // 確保默認隱藏主窗口，僅在系統托盤顯示
                // 使用Dispatcher確保UI已經初始化
                Avalonia.Threading.Dispatcher.UIThread.Post(() => 
                {
                    desktop.MainWindow.Hide();
                }, Avalonia.Threading.DispatcherPriority.Background);
            }

            base.OnFrameworkInitializationCompleted();
        }
        
        private void OnApplicationExit(object? sender, ControlledApplicationLifetimeExitEventArgs e)
        {
            _settings?.Save();
            _hotKeyService?.UnregisterAllHotKeys();
            _trayIconService?.Dispose();
        }
    }
} 