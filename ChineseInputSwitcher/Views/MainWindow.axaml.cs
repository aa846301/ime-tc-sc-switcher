using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ChineseInputSwitcher.ViewModels;
using System;
using Avalonia.Interactivity;

namespace ChineseInputSwitcher.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel? _viewModel;

        public MainWindow()
        {
            InitializeComponent();
            
            if (DataContext is MainViewModel viewModel)
            {
                _viewModel = viewModel;
            }
            
            // 訂閱窗口關閉事件
            this.Closing += MainWindow_Closing;
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        // 處理窗口關閉事件
        private void MainWindow_Closing(object? sender, WindowClosingEventArgs e)
        {
            // 取消關閉窗口的默認行為
            e.Cancel = true;
            
            // 隱藏窗口而不是關閉它
            this.Hide();
        }
    }
} 