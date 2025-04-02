using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using ChineseInputSwitcher.Models;

namespace ChineseInputSwitcher.Services
{
    public class MacOSPlatformService : IPlatformService
    {
        private readonly AppSettings _settings;

        public MacOSPlatformService(AppSettings settings)
        {
            _settings = settings;
        }

        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.OSX);

        public string GetCurrentInputMethodState()
        {
            return "NotSupportedMacOS";
        }

        public Task<bool> ToggleChineseInputMethod()
        {
            // macOS 不支持切換輸入法
            return Task.FromResult(false);
        }

        public void SimulateKeyboardInput(string text)
        {
            if (!IsSupported)
                return;

            // MacOS 鍵盤模擬實現
            // 可以使用 AppleScript 或其他方式實現
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