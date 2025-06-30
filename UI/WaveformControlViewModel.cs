using ReactiveUI;
using System;

namespace BGMSyncVisualizer.UI;

public class WaveformControlViewModel : ReactiveObject
{
    private float[] _waveformData = Array.Empty<float>();
    private double _startMarkerPosition = 0.0;
    private double _currentPosition = 0.0;
    private double _durationSeconds = 0.0;
    private bool _canSeek = false;

    public float[] WaveformData
    {
        get => _waveformData;
        set => this.RaiseAndSetIfChanged(ref _waveformData, value);
    }

    public double StartMarkerPosition
    {
        get => _startMarkerPosition;
        set => this.RaiseAndSetIfChanged(ref _startMarkerPosition, Math.Max(0, Math.Min(1, value)));
    }

    public double CurrentPosition
    {
        get => _currentPosition;
        set 
        {
            try
            {
                // Add additional validation for NaN and Infinity
                if (double.IsNaN(value) || double.IsInfinity(value))
                {
                    Console.WriteLine($"WaveformControlViewModel.CurrentPosition: Invalid value received: {value}");
                    return;
                }
                
                var clampedValue = Math.Max(0, Math.Min(1, value));
                Console.WriteLine($"WaveformControlViewModel.CurrentPosition: Setting value from {value:F3} to {clampedValue:F3}");
                
                this.RaiseAndSetIfChanged(ref _currentPosition, clampedValue);
                Console.WriteLine($"WaveformControlViewModel.CurrentPosition: Value updated successfully to {clampedValue:F3}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CRITICAL ERROR in WaveformControlViewModel.CurrentPosition setter: {ex}");
                System.Diagnostics.Debug.WriteLine($"CRITICAL ERROR in WaveformControlViewModel.CurrentPosition setter: {ex}");
            }
        }
    }

    public double DurationSeconds
    {
        get => _durationSeconds;
        set => this.RaiseAndSetIfChanged(ref _durationSeconds, value);
    }

    public bool CanSeek
    {
        get => _canSeek;
        set => this.RaiseAndSetIfChanged(ref _canSeek, value);
    }

    public event EventHandler<double>? SeekRequested;

    public void OnWaveformClicked(double normalizedPosition)
    {
        System.Diagnostics.Debug.WriteLine($"WaveformViewModel: OnWaveformClicked called with position {normalizedPosition}");
        System.Diagnostics.Debug.WriteLine($"WaveformViewModel: DurationSeconds = {DurationSeconds}");
        
        StartMarkerPosition = normalizedPosition;
        var seekTime = normalizedPosition * DurationSeconds;
        
        System.Diagnostics.Debug.WriteLine($"WaveformViewModel: Invoking SeekRequested with time {seekTime}");
        SeekRequested?.Invoke(this, seekTime);
    }

    public string GetTimeString(double normalizedPosition)
    {
        var seconds = normalizedPosition * DurationSeconds;
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }
}