using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ChineseInputSwitcher.Models;

namespace ChineseInputSwitcher.Services
{
    public class NativeHotKeyManager
    {
        private readonly Dictionary<int, HotKeyDelegate> _registeredHotKeys = new Dictionary<int, HotKeyDelegate>();
        private int _hotKeyId = 0;
        
        public delegate void HotKeyDelegate(int id);
        
        public bool IsSupported => RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
        
        public int RegisterHotKey(HotKeySettings settings, HotKeyDelegate callback)
        {
            if (!IsSupported)
                return -1;
                
            try
            {
                int modifiers = 0;
                
                if (settings.Alt) modifiers |= 0x0001; // MOD_ALT
                if (settings.Ctrl) modifiers |= 0x0002; // MOD_CONTROL
                if (settings.Shift) modifiers |= 0x0004; // MOD_SHIFT
                if (settings.Win) modifiers |= 0x0008; // MOD_WIN
                
                int id = ++_hotKeyId;
                
                // 在 Windows 上注册全局热键
                if (IsSupported && NativeMethods.RegisterHotKey(IntPtr.Zero, id, modifiers, settings.Key))
                {
                    _registeredHotKeys[id] = callback;
                    return id;
                }
            }
            catch (Exception)
            {
                // 处理注册失败
            }
            
            return -1;
        }
        
        public bool UnregisterHotKey(int id)
        {
            if (!IsSupported || id < 0)
                return false;
                
            try
            {
                if (NativeMethods.UnregisterHotKey(IntPtr.Zero, id))
                {
                    _registeredHotKeys.Remove(id);
                    return true;
                }
            }
            catch (Exception)
            {
                // 处理注销失败
            }
            
            return false;
        }
        
        public void ProcessHotKeyMessage(int id)
        {
            if (_registeredHotKeys.TryGetValue(id, out var callback))
            {
                callback?.Invoke(id);
            }
        }
        
        private static class NativeMethods
        {
            [DllImport("user32.dll")]
            public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
            
            [DllImport("user32.dll")]
            public static extern bool UnregisterHotKey(IntPtr hWnd, int id);
        }
    }
} 