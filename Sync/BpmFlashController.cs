using ReactiveUI;
using System;

namespace BGMSyncVisualizer.Sync;

public class BpmFlashController : ReactiveObject, IDisposable
{
    private int _currentBeat = 1;
    private int _beatsPerMeasure = 4;
    private FlashPattern _selectedFlashPattern = FlashPattern.SingleArea;
    private bool _isRunning = false;

    public event EventHandler<FlashEventArgs>? FlashEvent;

    public int CurrentBeat 
    { 
        get => _currentBeat;
        private set => this.RaiseAndSetIfChanged(ref _currentBeat, value);
    }

    public int BeatsPerMeasure
    {
        get => _beatsPerMeasure;
        set => this.RaiseAndSetIfChanged(ref _beatsPerMeasure, Math.Max(2, Math.Min(8, value)));
    }

    public FlashPattern SelectedFlashPattern
    {
        get => _selectedFlashPattern;
        set => this.RaiseAndSetIfChanged(ref _selectedFlashPattern, value);
    }

    public bool IsRunning
    {
        get => _isRunning;
        private set => this.RaiseAndSetIfChanged(ref _isRunning, value);
    }

    public string BeatCounterText => $"{CurrentBeat}/{BeatsPerMeasure}";

    public void Start()
    {
        IsRunning = true;
        CurrentBeat = 1;
        Console.WriteLine("BpmFlashController: Started");
    }

    public void Stop()
    {
        IsRunning = false;
        CurrentBeat = 1;
        Console.WriteLine("BpmFlashController: Stopped");
        
        // リセット時のフラッシュイベントを送信
        FlashEvent?.Invoke(this, new FlashEventArgs
        {
            Pattern = SelectedFlashPattern,
            Beat = CurrentBeat,
            IsStrong = false,
            IsActive = false
        });
    }

    public void OnBeatTrigger()
    {
        if (!IsRunning) return;

        try
        {
            // 4拍子サイクル
            CurrentBeat = CurrentBeat >= BeatsPerMeasure ? 1 : CurrentBeat + 1;
            
            // 1拍目は強いビート
            bool isStrongBeat = CurrentBeat == 1;
            
            Console.WriteLine($"BpmFlashController: Beat {CurrentBeat}/{BeatsPerMeasure} - Pattern: {SelectedFlashPattern} - Strong: {isStrongBeat}");
            
            // 選択されたフラッシュパターンのイベントを発生
            FlashEvent?.Invoke(this, new FlashEventArgs
            {
                Pattern = SelectedFlashPattern,
                Beat = CurrentBeat,
                IsStrong = isStrongBeat,
                IsActive = true
            });
            
            this.RaisePropertyChanged(nameof(BeatCounterText));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"BpmFlashController.OnBeatTrigger: Error: {ex.Message}");
        }
    }

    public void Dispose()
    {
        Stop();
    }
}

public class FlashEventArgs : EventArgs
{
    public FlashPattern Pattern { get; set; }
    public int Beat { get; set; }
    public bool IsStrong { get; set; }
    public bool IsActive { get; set; }
}