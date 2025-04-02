using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
    }
} 