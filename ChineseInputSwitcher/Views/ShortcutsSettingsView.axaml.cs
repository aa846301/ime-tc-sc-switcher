using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using ChineseInputSwitcher.ViewModels;
using System;

namespace ChineseInputSwitcher.Views
{
    public partial class ShortcutsSettingsView : UserControl
    {
        public ShortcutsSettingsView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void ShortcutTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox textBox && DataContext is SettingsViewModel viewModel)
            {
                string keyCombination = "";
                if (e.KeyModifiers.HasFlag(KeyModifiers.Control))
                    keyCombination += "Ctrl+";
                if (e.KeyModifiers.HasFlag(KeyModifiers.Alt))
                    keyCombination += "Alt+";
                if (e.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    keyCombination += "Shift+";
                if (e.KeyModifiers.HasFlag(KeyModifiers.Meta))
                    keyCombination += "Win+";

                if (e.Key != Key.None)
                    keyCombination += e.Key.ToString();

                Console.WriteLine($"按鍵輸入: {keyCombination}");

                if (!string.IsNullOrEmpty(keyCombination))
                {
                    // 根據 TextBox 的 Name 屬性識別正在編輯的快捷鍵
                    switch (textBox.Name)
                    {
                        case "IMEToggleTextBox":
                            viewModel.IMEToggleShortcut = keyCombination;
                            break;
                        case "NotificationToggleTextBox":
                            viewModel.NotificationToggleShortcut = keyCombination;
                            break;
                        case "TextToSqlFormatTextBox":
                            viewModel.TextToSqlFormatShortcut = keyCombination;
                            break;
                        case "TextToKeyboardInputTextBox":
                            viewModel.TextToKeyboardInputShortcut = keyCombination;
                            break;
                    }
                }
                e.Handled = true;
            }
        }
    }
} 