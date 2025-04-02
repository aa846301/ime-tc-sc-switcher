using System;
using System.Threading.Tasks;
using ChineseInputSwitcher.Models;

namespace ChineseInputSwitcher.Services
{
    public class HotKeyService
    {
        private readonly AppSettings _settings;
        private readonly IPlatformService? _platformService;
        private readonly NotificationService _notificationService;
        private readonly TextTransformService _textTransformService;
        private readonly GlobalHotKeyService _globalHotKeyService;
        private readonly ClipboardService _clipboardService = new ClipboardService();
        
        public HotKeyService(AppSettings settings, IPlatformService? platformService, 
                            NotificationService notificationService, 
                            TextTransformService textTransformService)
        {
            _settings = settings;
            _platformService = platformService;
            _notificationService = notificationService;
            _textTransformService = textTransformService;
            _globalHotKeyService = new GlobalHotKeyService(notificationService, settings);
        }
        
        public void RegisterAllHotKeys()
        {
            _platformService?.RegisterGlobalHotKey();
        }
        
        public void UnregisterAllHotKeys()
        {
            _platformService?.UnregisterGlobalHotKey();
        }
        
        public async Task HandleHotKey(int key, bool ctrl, bool alt, bool shift, bool win)
        {
            try
            {
                // 切換輸入法熱鍵
                if (MatchHotKey(_settings.ToggleInputMethod, key, ctrl, alt, shift, win))
                {
                    if (_platformService != null)
                        await _platformService.ToggleChineseInputMethod();
                }
                // 切換通知顯示熱鍵
                else if (MatchHotKey(_settings.ToggleNotification, key, ctrl, alt, shift, win))
                {
                    await _globalHotKeyService.OnToggleNotification();
                }
                // SQL格式轉換熱鍵
                else if (MatchHotKey(_settings.TextToSqlFormat, key, ctrl, alt, shift, win))
                {
                    var text = await GetClipboardText();
                    if (!string.IsNullOrEmpty(text))
                    {
                        var formattedText = _textTransformService.ConvertToSqlFormat(text);
                        await SetClipboardText(formattedText);
                        await _notificationService.ShowNotification("已轉換為SQL格式");
                    }
                }
                // 鍵盤輸入模擬熱鍵
                else if (MatchHotKey(_settings.TextToKeyboardInput, key, ctrl, alt, shift, win))
                {
                    var text = await GetClipboardText();
                    if (!string.IsNullOrEmpty(text) && _platformService != null)
                    {
                        _platformService.SimulateKeyboardInput(text);
                        await _notificationService.ShowNotification("已模擬鍵盤輸入");
                    }
                }
            }
            catch (Exception ex)
            {
                await _notificationService.ShowNotification($"錯誤: {ex.Message}");
            }
        }
        
        private bool MatchHotKey(HotKeySettings settings, int key, bool ctrl, bool alt, bool shift, bool win)
        {
            return settings.Key == key && 
                   settings.Ctrl == ctrl && 
                   settings.Alt == alt && 
                   settings.Shift == shift && 
                   settings.Win == win;
        }
        
        private async Task<string> GetClipboardText()
        {
            return await _clipboardService.GetText();
        }
        
        private async Task SetClipboardText(string text)
        {
            await _clipboardService.SetText(text);
        }
    }
} 