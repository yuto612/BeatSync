using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System;
using System.IO;
using System.Linq;

namespace BGMSyncVisualizer.UI
{
    public partial class MainWindow : Window
    {
        private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

        public MainWindow()
        {
            InitializeComponent();
            var viewModel = new MainWindowViewModel();
            DataContext = viewModel;
            
            // Enable drag and drop
            DragDrop.SetAllowDrop(this, true);
            AddHandler(DragDrop.DropEvent, OnFileDrop);
            AddHandler(DragDrop.DragOverEvent, OnDragOver);
            
            // ファイル選択機能削除済み（ドラッグ&ドロップのみ）
        }

        private void OnDragOver(object? sender, DragEventArgs e)
        {
            if (e.Data.Contains(DataFormats.Files))
            {
                var files = e.Data.GetFiles()?.ToArray();
                if (files != null && files.Length > 0)
                {
                    var file = files[0];
                    var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                    var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac" };
                    e.DragEffects = supportedExtensions.Contains(extension) 
                        ? DragDropEffects.Copy 
                        : DragDropEffects.None;
                }
            }
            else
            {
                e.DragEffects = DragDropEffects.None;
            }
        }

        private async void OnFileDrop(object? sender, DragEventArgs e)
        {
            if (ViewModel == null)
                return;

            try
            {
                var files = e.Data.GetFiles()?.ToArray();
                if (files != null && files.Length > 0)
                {
                    var file = files[0];
                    var extension = Path.GetExtension(file.Name).ToLowerInvariant();
                    var supportedExtensions = new[] { ".mp3", ".wav", ".flac", ".m4a", ".aac" };
                    
                    if (supportedExtensions.Contains(extension))
                    {
                        var path = file.TryGetLocalPath();
                        if (!string.IsNullOrEmpty(path))
                        {
                            await ViewModel.LoadFileAsync(path);
                        }
                        else
                        {
                            ViewModel.StatusMessage = "ファイルパスを取得できませんでした";
                            ViewModel.StatusMessageColor = "Red";
                        }
                    }
                    else
                    {
                        ViewModel.StatusMessage = "サポートされているファイル形式: MP3, WAV, FLAC, M4A, AAC";
                        ViewModel.StatusMessageColor = "Red";
                    }
                }
            }
            catch (System.Exception ex)
            {
                ViewModel.StatusMessage = $"ファイルドロップエラー: {ex.Message}";
                ViewModel.StatusMessageColor = "Red";
            }
        }

        protected override void OnClosed(System.EventArgs e)
        {
            ViewModel?.Dispose();
            base.OnClosed(e);
        }
    }
}