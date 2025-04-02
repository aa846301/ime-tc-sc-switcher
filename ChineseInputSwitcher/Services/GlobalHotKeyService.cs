using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ChineseInputSwitcher.Models;

namespace ChineseInputSwitcher.Services
{
    public class GlobalHotKeyService
    {
        private readonly NotificationService _notificationService;
        private readonly AppSettings _settings;

        public GlobalHotKeyService(NotificationService notificationService, AppSettings settings)
        {
            _notificationService = notificationService;
            _settings = settings;
        }

        public async Task OnToggleNotification()
        {
            _settings.EnableNotifications = !_settings.EnableNotifications;
            _settings.Save();
            await _notificationService.ShowNotification(_settings.EnableNotifications ? "通知已啟用" : "通知已禁用");
        }
    }
} 