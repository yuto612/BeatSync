using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Threading;
using System;
using System.Linq;

namespace BGMSyncVisualizer.UI;

public partial class WaveformControl : UserControl
{
    private WaveformControlViewModel? _viewModel;
    private Canvas? _waveformCanvas;
    private Rectangle? _currentPositionIndicator;

    public WaveformControl()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        AttachedToVisualTree += OnAttachedToVisualTree;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
        }

        _viewModel = DataContext as WaveformControlViewModel;
        if (_viewModel != null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }
    }

    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _waveformCanvas = this.FindControl<Canvas>("WaveformCanvas");
        _currentPositionIndicator = this.FindControl<Rectangle>("CurrentPositionIndicator");

        UpdateWaveform();
        UpdateMarkers();
    }

    private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(() =>
        {
            switch (e.PropertyName)
            {
                case nameof(WaveformControlViewModel.WaveformData):
                    UpdateWaveform();
                    break;
                case nameof(WaveformControlViewModel.CurrentPosition):
                    UpdateMarkers();
                    break;
            }
        });
    }

    private void UpdateWaveform()
    {
        if (_waveformCanvas == null || _viewModel == null)
            return;

        _waveformCanvas.Children.Clear();

        var waveformData = _viewModel.WaveformData;
        if (waveformData == null || waveformData.Length == 0)
            return;

        // Use control bounds instead of canvas bounds
        var width = this.Bounds.Width;
        var height = this.Bounds.Height;

        if (width <= 0 || height <= 0)
        {
            // Defer update until size is available
            Dispatcher.UIThread.Post(() => UpdateWaveform(), DispatcherPriority.Background);
            return;
        }

        var stepX = width / waveformData.Length;
        var centerY = height / 2;

        for (int i = 0; i < waveformData.Length; i++)
        {
            var amplitude = waveformData[i];
            var lineHeight = Math.Max(1, amplitude * centerY * 0.8); // Leave some margin

            var line = new Rectangle
            {
                Width = Math.Max(1, stepX),
                Height = lineHeight,
                Fill = new SolidColorBrush(Color.Parse("#38A169")), // Modern green
                StrokeThickness = 0
            };

            Canvas.SetLeft(line, i * stepX);
            Canvas.SetTop(line, centerY - lineHeight / 2);
            _waveformCanvas.Children.Add(line);
        }
    }

    private void UpdateMarkers()
    {
        if (_currentPositionIndicator == null || _viewModel == null)
            return;

        var width = this.Bounds.Width;
        if (width <= 0)
            return;

        var currentX = _viewModel.CurrentPosition * width;
        Canvas.SetLeft(_currentPositionIndicator, currentX);
        _currentPositionIndicator.IsVisible = _viewModel.CurrentPosition > 0;
    }



    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdateWaveform();
        UpdateMarkers();
    }
}