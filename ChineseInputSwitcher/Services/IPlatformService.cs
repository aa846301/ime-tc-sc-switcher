using System;
using System.Threading.Tasks;

namespace ChineseInputSwitcher.Services
{
    public interface IPlatformService
    {
        bool IsSupported { get; }
        string GetCurrentInputMethodState();
        Task<bool> ToggleChineseInputMethod();
        void SimulateKeyboardInput(string text);
        void RegisterGlobalHotKey();
        void UnregisterGlobalHotKey();
    }
} 