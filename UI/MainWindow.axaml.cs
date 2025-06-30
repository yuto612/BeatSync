using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using System.IO;
using System.Linq;

namespace BGMSyncVisualizer.UI;

public partial class MainWindow : Window
{
    private MainWindowViewModel? ViewModel => DataContext as MainWindowViewModel;

    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainWindowViewModel();
        
        // Enable drag and drop
        DragDrop.SetAllowDrop(this, true);
        AddHandler(DragDrop.DropEvent, OnFileDrop);
        AddHandler(DragDrop.DragOverEvent, OnDragOver);
        
        var selectFileButton = this.FindControl<Button>("SelectFileButton");
        if (selectFileButton != null)
        {
            selectFileButton.Click += OnSelectFileButtonClick;
        }
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
                e.DragEffects = (extension == ".mp3" || extension == ".wav") 
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
                
                if (extension == ".mp3" || extension == ".wav")
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
                    ViewModel.StatusMessage = "mp3またはwavファイルのみサポートしています";
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

    private async void OnSelectFileButtonClick(object? sender, RoutedEventArgs e)
    {
        try
        {
            var storageProvider = this.StorageProvider;
            var options = new FilePickerOpenOptions
            {
                Title = "オーディオファイルを選択",
                AllowMultiple = false,
                FileTypeFilter = new[]
                {
                    new FilePickerFileType("オーディオファイル") { Patterns = new[] { "*.mp3", "*.wav" } },
                    FilePickerFileTypes.All
                }
            };

            var files = await storageProvider.OpenFilePickerAsync(options);
            if (files.Count > 0 && ViewModel != null)
            {
                var path = files[0].TryGetLocalPath();
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
        }
        catch (System.Exception ex)
        {
            if (ViewModel != null)
            {
                ViewModel.StatusMessage = $"ファイル選択エラー: {ex.Message}";
                ViewModel.StatusMessageColor = "Red";
            }
        }
    }

    protected override void OnClosed(System.EventArgs e)
    {
        ViewModel?.Dispose();
        base.OnClosed(e);
    }
}