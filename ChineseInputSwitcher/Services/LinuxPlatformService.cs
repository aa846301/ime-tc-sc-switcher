using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ChineseInputSwitcher.Models;

namespace ChineseInputSwitcher.Services
{
    public class LinuxPlatformService : IPlatformService
    {
        private readonly AppSettings _settings;

        public LinuxPlatformService(AppSettings settings)
        {
            _settings = settings;
            
            // 檢查是否有桌面環境
            if (!HasDesktopEnvironment())
            {
                Console.WriteLine("錯誤: 無法在沒有桌面環境的 Linux 系統上使用此應用程序");
                Console.WriteLine("請在具有圖形界面的環境中運行，或僅使用命令行功能");
            }
        }

        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Linux);

        private bool HasDesktopEnvironment()
        {
            // 檢查常見的桌面環境變量
            string[] desktopVars = { "XDG_CURRENT_DESKTOP", "GNOME_DESKTOP_SESSION_ID", "KDE_FULL_SESSION", "DESKTOP_SESSION" };
            
            foreach (var var in desktopVars)
            {
                if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable(var)))
                {
                    return true;
                }
            }
            
            // 檢查 DISPLAY 變量
            return !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("DISPLAY"));
        }

        public string GetCurrentInputMethodState()
        {
            return "NotSupportedLinux";
        }

        public Task<bool> ToggleChineseInputMethod()
        {
            // Linux 不支持切換輸入法
            return Task.FromResult(false);
        }

        public void SimulateKeyboardInput(string text)
        {
            if (!IsSupported)
                return;

            // Linux 鍵盤模擬實現
        }

        public void RegisterGlobalHotKey()
        {
            // 註冊熱鍵
        }

        public void UnregisterGlobalHotKey()
        {
            // 註銷熱鍵
        }
    }
} 