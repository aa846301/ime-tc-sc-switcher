using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;

namespace ChineseInputSwitcher.Models
{
    public class HotKeySettings
    {
        public int Key { get; set; }
        public bool Ctrl { get; set; }
        public bool Alt { get; set; }
        public bool Shift { get; set; }
        public bool Win { get; set; }
    }

    public class AppSettings
    {
        public bool EnableNotifications { get; set; } = true;
        public bool EnableMultiScreenNotifications { get; set; } = true;
        public HotKeySettings ToggleInputMethod { get; set; } = new HotKeySettings 
        { 
            Key = 70, // F key
            Alt = true,
            Shift = true,
            Ctrl = false,
            Win = false
        };
        public HotKeySettings ToggleNotification { get; set; } = new HotKeySettings
        {
            Key = 88, // X key
            Ctrl = true,
            Alt = true,
            Shift = true,
            Win = false
        };
        public HotKeySettings TextToSqlFormat { get; set; } = new HotKeySettings
        {
            Key = 83, // S key
            Ctrl = true,
            Alt = true,
            Shift = false,
            Win = false
        };
        public HotKeySettings TextToKeyboardInput { get; set; } = new HotKeySettings
        {
            Key = 75, // K key
            Ctrl = true,
            Alt = true,
            Shift = false,
            Win = false
        };
        
        // 平台特定设置
        public bool EnableOnWindows { get; set; } = true;
        public bool EnableOnMacOS { get; set; } = false;
        public bool EnableOnLinux { get; set; } = false;

        // 在 AppSettings 類中添加語言設置字段
        public string Language { get; set; } = "system"; // 默認使用系統語言

        // 添加這些屬性
        public bool EnableTextConversion { get; set; } = true;
        public bool EnableClipboardToKeyboard { get; set; } = true;

        private static string SettingsFilePath => 
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "ChineseInputSwitcher",
                "settings.json");

        public static AppSettings Load()
        {
            try
            {
                if (File.Exists(SettingsFilePath))
                {
                    string json = File.ReadAllText(SettingsFilePath);
                    return JsonSerializer.Deserialize<AppSettings>(json) ?? new AppSettings();
                }
            }
            catch (Exception)
            {
                // 如果加载失败，返回默认设置
            }
            
            return new AppSettings();
        }

        public void Save()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsFilePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                string json = JsonSerializer.Serialize(this, new JsonSerializerOptions
                {
                    WriteIndented = true
                });
                
                File.WriteAllText(SettingsFilePath, json);
            }
            catch (Exception)
            {
                // 处理保存失败
            }
        }

        public AppSettings Clone()
        {
            var cloned = new AppSettings
            {
                Language = this.Language,
                EnableNotifications = this.EnableNotifications,
                EnableMultiScreenNotifications = this.EnableMultiScreenNotifications,
                EnableOnWindows = this.EnableOnWindows,
                EnableOnMacOS = this.EnableOnMacOS,
                EnableOnLinux = this.EnableOnLinux,
                ToggleInputMethod = this.ToggleInputMethod,
                ToggleNotification = this.ToggleNotification,
                TextToSqlFormat = this.TextToSqlFormat,
                TextToKeyboardInput = this.TextToKeyboardInput,
                EnableTextConversion = this.EnableTextConversion,
                EnableClipboardToKeyboard = this.EnableClipboardToKeyboard
            };
            return cloned;
        }

        public void CopyFrom(AppSettings other)
        {
            this.Language = other.Language;
            this.EnableNotifications = other.EnableNotifications;
            this.EnableMultiScreenNotifications = other.EnableMultiScreenNotifications;
            this.EnableOnWindows = other.EnableOnWindows;
            this.EnableOnMacOS = other.EnableOnMacOS;
            this.EnableOnLinux = other.EnableOnLinux;
            this.ToggleInputMethod = other.ToggleInputMethod;
            this.ToggleNotification = other.ToggleNotification;
            this.TextToSqlFormat = other.TextToSqlFormat;
            this.TextToKeyboardInput = other.TextToKeyboardInput;
            this.EnableTextConversion = other.EnableTextConversion;
            this.EnableClipboardToKeyboard = other.EnableClipboardToKeyboard;
        }
    }
} 