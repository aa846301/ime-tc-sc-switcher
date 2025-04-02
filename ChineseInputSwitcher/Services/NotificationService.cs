using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Threading;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.Views;

namespace ChineseInputSwitcher.Services
{
    public class NotificationService
    {
        private readonly AppSettings _settings;
        private List<NotificationWindow> _notificationWindows = new List<NotificationWindow>();
        private NotificationWindow? _notificationWindow;
        
        public NotificationService(AppSettings settings)
        {
            _settings = settings;
        }
        
        public async Task ShowNotification(string message, int duration = 2000)
        {
            if (!_settings.EnableNotifications)
                return;
                
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                try
                {
                    // 关闭现有通知
                    _notificationWindow?.Close();
                    
                    // 创建新通知窗口
                    _notificationWindow = new NotificationWindow();
                    _notificationWindow.Message = message;
                    _notificationWindow.Show();
                    
                    // 设置自动关闭计时器
                    var timer = new System.Timers.Timer(duration);
                    timer.Elapsed += (s, e) =>
                    {
                        Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            _notificationWindow?.Close();
                            _notificationWindow = null;
                        });
                        timer.Dispose();
                    };
                    timer.Start();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error showing notification: {ex.Message}");
                }
            });
        }
        
        private async Task CloseAllNotifications()
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                foreach (var window in _notificationWindows)
                {
                    window.Close();
                }
                _notificationWindows.Clear();
            });
        }
    }
} 