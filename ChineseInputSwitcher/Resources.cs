using System.ComponentModel;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;
using System;
using System.Collections.Generic;
using ReactiveUI;

namespace ChineseInputSwitcher
{
    public static class Resources
    {
        private static readonly ResourceManager _resourceManager = new ResourceManager("ChineseInputSwitcher.Resources.Strings", typeof(Resources).Assembly);
        
        // 保存最近使用的資源字符串
        private static readonly Dictionary<string, string> _resourceCache = new Dictionary<string, string>();
        
        // 創建一個可觀察對象來通知界面更新
        private static readonly ReactiveObject _notifier = new ReactiveObject();

        static Resources() 
        {
            App.LanguageChanged += (s, e) => RefreshResourceCache();
        }
        
        private static void RefreshResourceCache()
        {
            Console.WriteLine("刷新資源緩存...");
            // 清除靜態資源的緩存，使其重新讀取當前語言的值
            _resourceManager.ReleaseAllResources();
            
            // 清空字符串緩存
            _resourceCache.Clear();
            
            // 通知所有綁定到 Resources 的界面更新
            _notifier.RaisePropertyChanged("*");
            
            Console.WriteLine($"語言已切換到: {CultureInfo.CurrentUICulture.Name}");
        }
        
        // 動態獲取資源字符串的方法
        public static string GetString(string key)
        {
            if (_resourceCache.TryGetValue(key, out string? cachedValue))
            {
                return cachedValue;
            }
            
            string result = _resourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;
            _resourceCache[key] = result;
            return result;
        }

        // 以下靜態屬性繼續保留, 但內部實現改為調用動態方法
        public static string AppTitle => GetString("AppTitle");
        public static string CurrentInputState => GetString("CurrentInputState");
        public static string SimplifiedChinese => GetString("SimplifiedChinese");
        public static string TraditionalChinese => GetString("TraditionalChinese");
        public static string NotAvailable => GetString("NotAvailable");
        public static string Unknown => GetString("Unknown");
        public static string Error => GetString("Error");
        public static string TextConversionTool => GetString("TextConversionTool");
        public static string InputOrPasteText => GetString("InputOrPasteText");
        public static string ConvertToSQLFormat => GetString("ConvertToSQLFormat");
        public static string SimulateKeyboardInput => GetString("SimulateKeyboardInput");
        public static string Shortcuts => GetString("Shortcuts");
        public static string IMEToggle => GetString("IMEToggle");
        public static string NotificationToggle => GetString("NotificationToggle");
        public static string SQLFormat => GetString("SQLFormat");
        public static string KeyboardInput => GetString("KeyboardInput");
        public static string Settings => GetString("Settings");
        public static string PlatformSettings => GetString("PlatformSettings");
        public static string EnableOnWindows => GetString("EnableOnWindows");
        public static string EnableOnMacOS => GetString("EnableOnMacOS");
        public static string EnableOnLinux => GetString("EnableOnLinux");
        public static string OK => GetString("OK");
        public static string Cancel => GetString("Cancel");
        public static string Language => GetString("Language");
        public static string FollowSystem => GetString("FollowSystem");
        public static string NotificationEnabled => GetString("NotificationEnabled");
        public static string NotificationDisabled => GetString("NotificationDisabled");
        public static string SQLFormatComplete => GetString("SQLFormatComplete");
        public static string EnableNotifications => GetString("EnableNotifications");
        public static string EnableMultiScreenNotifications => GetString("EnableMultiScreenNotifications");
        public static string AppDescription => GetString("AppDescription");
        public static string HowToUse => GetString("HowToUse");
        public static string HowToUseIMEToggle => GetString("HowToUseIMEToggle");
        public static string HowToUseNotifications => GetString("HowToUseNotifications");
        public static string HowToUseSQLFormat => GetString("HowToUseSQLFormat");
        public static string HowToUseKeyboardInput => GetString("HowToUseKeyboardInput");
        public static string Version => GetString("Version");
        public static string ShortcutSettings => GetString("ShortcutSettings");
        public static string About => GetString("About");
        public static string ConfigureShortcuts => GetString("ConfigureShortcuts");
        public static string Reset => GetString("Reset");
        public static string FunctionSettings => GetString("FunctionSettings");
        public static string EnableIMEToggleOnWindows => GetString("EnableIMEToggleOnWindows");
        public static string EnableTextConversion => GetString("EnableTextConversion");
        public static string EnableClipboardToKeyboard => GetString("EnableClipboardToKeyboard");
        public static string ResetAllShortcuts => GetString("ResetAllShortcuts");
        public static string NotificationSettings => GetString("NotificationSettings");
        public static string ShowOnAllScreens => GetString("ShowOnAllScreens");
        public static string Save => GetString("Save");
    }
} 