using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Input.Platform;

namespace ChineseInputSwitcher.Services
{
    public class ClipboardService
    {
        private IClipboard? _clipboard;
        
        public ClipboardService()
        {
            try 
            {
                if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                {
                    var topLevel = TopLevel.GetTopLevel(desktop.MainWindow);
                    if (topLevel != null)
                    {
                        _clipboard = topLevel.Clipboard;
                    }
                }
            }
            catch 
            {
                // 忽略錯誤
            }
        }
        
        public async Task<string> GetText()
        {
            if (_clipboard == null)
                return string.Empty;
                
            try
            {
                return await _clipboard.GetTextAsync() ?? string.Empty;
            }
            catch
            {
                return string.Empty;
            }
        }
        
        public async Task SetText(string text)
        {
            if (_clipboard == null)
                return;
                
            try
            {
                await _clipboard.SetTextAsync(text);
            }
            catch
            {
                // 處理剪貼板錯誤
            }
        }
    }
} 