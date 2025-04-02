using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ChineseInputSwitcher.Views
{
    public partial class NotificationWindow : Window
    {
        public static readonly StyledProperty<string> MessageProperty =
            AvaloniaProperty.Register<NotificationWindow, string>(nameof(Message));

        public string Message
        {
            get => GetValue(MessageProperty);
            set => SetValue(MessageProperty, value);
        }

        public NotificationWindow()
        {
            InitializeComponent();
            this.Opacity = 1;
            
            // 淡出动画效果
            var timer = new System.Threading.Timer(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    this.Opacity -= 0.05;
                    if (this.Opacity <= 0)
                    {
                        this.Close();
                    }
                });
            }, null, 500, 20);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
} 