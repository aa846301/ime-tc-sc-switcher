using System;
using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using ChineseInputSwitcher.Models;
using ChineseInputSwitcher.ViewModels;

namespace ChineseInputSwitcher.Views
{
    public partial class SettingsWindow : Window
    {
        public AppSettings? Settings { get; }
        
        // 键盘按键选项
        private readonly List<string> _keyOptions = new List<string>
        {
            "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", 
            "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z",
            "F1", "F2", "F3", "F4", "F5", "F6", "F7", "F8", "F9", "F10", "F11", "F12"
        };
        
        public int ToggleInputMethodKeyIndex
        {
            get => Settings?.ToggleInputMethod.Key ?? 0;
            set 
            {
                if (Settings != null)
                    Settings.ToggleInputMethod.Key = GetKeyCode(value);
            }
        }
        
        public int ToggleNotificationKeyIndex
        {
            get => Settings?.ToggleNotification.Key ?? 0;
            set 
            {
                if (Settings != null)
                    Settings.ToggleNotification.Key = GetKeyCode(value);
            }
        }
        
        public int TextToSqlFormatKeyIndex
        {
            get => Settings?.TextToSqlFormat.Key ?? 0;
            set 
            {
                if (Settings != null)
                    Settings.TextToSqlFormat.Key = GetKeyCode(value);
            }
        }
        
        public int TextToKeyboardInputKeyIndex
        {
            get => Settings?.TextToKeyboardInput.Key ?? 0;
            set 
            {
                if (Settings != null)
                    Settings.TextToKeyboardInput.Key = GetKeyCode(value);
            }
        }

        private readonly SettingsViewModel? _viewModel;

        public SettingsWindow()
        {
            InitializeComponent();
        }

        public SettingsWindow(AppSettings settings) : this()
        {
            var viewModel = new SettingsViewModel(settings);
            DataContext = viewModel;
            
            viewModel.RequestClose += (sender, e) =>
            {
                Close(e);
            };
        }

        public SettingsWindow(SettingsViewModel viewModel) : this()
        {
            DataContext = viewModel;
            _viewModel = viewModel;

            // 設置關閉事件
            _viewModel.RequestClose += (sender, result) =>
            {
                Close(result);
            };
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
        
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            Settings?.Save();
            Close(true);
        }
        
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
        
        private int GetKeyIndex(int keyCode)
        {
            // 将按键代码转换为索引
            // 这里需要实现实际的转换逻辑
            return 0;
        }
        
        private int GetKeyCode(int index)
        {
            // 将索引转换为按键代码
            // 这里需要实现实际的转换逻辑
            return 65; // 默认为 A 键
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _viewModel?.NotifyWindowClosed();
        }
    }
} 