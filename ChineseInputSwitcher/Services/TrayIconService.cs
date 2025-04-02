using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Views;
using Avalonia.Media;
using System.IO;
using ChineseInputSwitcher;  // 添加這個命名空間以引用 Resources
using static ChineseInputSwitcher.Resources;

namespace ChineseInputSwitcher.Services
{
    public class TrayIconService : IDisposable
    {
        private readonly AppSettings _settings;
        private readonly IPlatformService _platformService;
        private readonly NotificationService _notificationService;
        private TrayIcon? _trayIcon;
        private bool _disposed = false;
        
        public TrayIconService(AppSettings settings, IPlatformService platformService, NotificationService notificationService)
        {
            _settings = settings;
            _platformService = platformService;
            _notificationService = notificationService;
        }
        
        public void Initialize(Window? mainWindow)
        {
            // 檢查是否支持系統托盤
            bool isTraySupported = true; // 簡化判斷
            
            if (!isTraySupported)
                return;
                
            try
            {
                _trayIcon = new TrayIcon
                {
                    Icon = GetTrayIcon(),
                    ToolTipText = "Chinese IME Switcher",
                    Menu = CreateTrayMenu(mainWindow),
                    IsVisible = true
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error initializing tray icon: {ex.Message}");
            }
        }
        
        private NativeMenu CreateTrayMenu(Window? mainWindow)
        {
            var menu = new NativeMenu();
            
            // 顯示主窗口
            var showItem = new NativeMenuItem("顯示主窗口");
            showItem.Click += (s, e) => 
            {
                if (mainWindow != null)
                {
                    mainWindow.Show();
                    mainWindow.WindowState = WindowState.Normal;
                    mainWindow.Activate();
                }
            };
            menu.Add(showItem);
            
            // 切換輸入法（僅限Windows）
            if (_platformService.IsSupported && _settings.EnableOnWindows)
            {
                var toggleItem = new NativeMenuItem("切換輸入法");
                toggleItem.Click += async (s, e) => 
                {
                    var result = await _platformService.ToggleChineseInputMethod();
                    if (result)
                    {
                        await _notificationService.ShowNotification(_platformService.GetCurrentInputMethodState());
                    }
                };
                menu.Add(toggleItem);
            }
            
            // 開/關通知
            var notificationItem = new NativeMenuItem(_settings.EnableNotifications ? 
                "禁用通知" : "啟用通知");
            notificationItem.Click += async (s, e) => 
            {
                _settings.EnableNotifications = !_settings.EnableNotifications;
                _settings.Save();
                await _notificationService.ShowNotification(_settings.EnableNotifications ? 
                    "通知已啟用" : "通知已禁用");
                
                // 更新菜單項
                notificationItem.Header = _settings.EnableNotifications ? 
                    "禁用通知" : "啟用通知";
            };
            menu.Add(notificationItem);
            
            // 分隔線
            menu.Add(new NativeMenuItemSeparator());
            
            // 退出
            var exitItem = new NativeMenuItem("退出");
            exitItem.Click += (s, e) => 
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    desktop.Shutdown();
                }
            };
            menu.Add(exitItem);
            
            return menu;
        }
        
        private WindowIcon GetTrayIcon()
        {
            try 
            {
                // 使用應用程序圖標
                string iconPath = "Assets/app-icon.ico";
                
                // 嘗試從資源加載圖標
                using var stream = AssetLoader.Open(new Uri($"avares://ChineseInputSwitcher/{iconPath}"));
                return new WindowIcon(stream);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"無法加載托盤圖標: {ex.Message}");
                // 返回空值處理
                return null!;
            }
        }
        
        public void Dispose()
        {
            if (!_disposed)
            {
                _trayIcon!.IsVisible = false;
                _trayIcon = null;
                _disposed = true;
            }
        }
    }
} 