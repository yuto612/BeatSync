using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BGMSyncVisualizer.UI;

public partial class FullscreenFlashWindow : Window
{
    public FullscreenFlashWindow()
    {
        InitializeComponent();
        
        // ESCキーで閉じる
        KeyDown += OnKeyDown;
        
        // クリックで閉じる
        PointerPressed += OnPointerPressed;
        
        // ウィンドウがアクティブになったときに最前面に表示
        Activated += OnActivated;
    }

    private void OnKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            Close();
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        Close();
    }

    private void OnActivated(object? sender, EventArgs e)
    {
        // 最前面に保持
        Topmost = true;
    }
}