using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using BGMSyncVisualizer.Audio;
using Avalonia.Threading;

namespace BGMSyncVisualizer.UI;

public class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly AudioEngine _audioEngine;
    private readonly DispatcherTimer _positionTimer;

    private bool _isFileLoaded;
    private bool _isPlaying;
    private double _startTimeSeconds;
    private string _statusMessage = "mp3またはwavファイルをドロップするか、クリックして選択してください";
    private string _statusMessageColor = "Gray";

    public MainWindowViewModel()
    {
        Console.WriteLine("=== BPM Sync Visualizer Starting ===");
        Console.WriteLine("MainWindowViewModel: Initializing components...");
        
        _audioEngine = new AudioEngine();
        Console.WriteLine("MainWindowViewModel: AudioEngine created");
        
        WaveformViewModel = new WaveformControlViewModel();
        Console.WriteLine("MainWindowViewModel: WaveformViewModel created");

        _audioEngine.PlaybackEnded += OnPlaybackEnded;
        WaveformViewModel.SeekRequested += OnSeekRequested;
        Console.WriteLine("MainWindowViewModel: Essential event handlers attached");

        _positionTimer = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(100) };
        _positionTimer.Tick += OnPositionTimerTick;
        Console.WriteLine("MainWindowViewModel: Position timer created");

        SelectFileCommand = ReactiveCommand.CreateFromTask(SelectFileAsync);
        PlayCommand = ReactiveCommand.Create(Play, this.WhenAnyValue(x => x.IsFileLoaded, x => x.IsPlaying, (loaded, playing) => loaded && !playing));
        StopCommand = ReactiveCommand.Create(Stop, this.WhenAnyValue(x => x.IsPlaying));
        Console.WriteLine("MainWindowViewModel: Commands created");
        
        Console.WriteLine("=== MainWindowViewModel initialization completed ===");
    }

    public WaveformControlViewModel WaveformViewModel { get; }

    public ICommand SelectFileCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand StopCommand { get; }

    public bool IsFileLoaded
    {
        get => _isFileLoaded;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isFileLoaded, value);
            this.RaisePropertyChanged(nameof(CanSeek));
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isPlaying, value);
            this.RaisePropertyChanged(nameof(CanSeek));
        }
    }

    public bool CanSeek => IsFileLoaded && !IsPlaying;

    public double StartTimeSeconds
    {
        get => _startTimeSeconds;
        private set => this.RaiseAndSetIfChanged(ref _startTimeSeconds, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public string StatusMessageColor
    {
        get => _statusMessageColor;
        set => this.RaiseAndSetIfChanged(ref _statusMessageColor, value);
    }

    public bool Loop { get; set; } = false;

    public string CurrentStartText => $"開始時間: {TimeSpan.FromSeconds(_startTimeSeconds):mm\\:ss}";

    public async Task LoadFileAsync(string filePath)
    {
        try
        {
            Console.WriteLine($"=== LoadFileAsync: Starting file load for {filePath} ===");
            StatusMessage = "ファイルを読み込み中...";
            StatusMessageColor = "Blue";
            
            if (!File.Exists(filePath))
            {
                Console.WriteLine($"LoadFileAsync: File not found: {filePath}");
                StatusMessage = "ファイルが見つかりません";
                StatusMessageColor = "Red";
                return;
            }

            Console.WriteLine("LoadFileAsync: Calling AudioEngine.LoadFile in background task");
            await Task.Run(() => _audioEngine.LoadFile(filePath));
            
            Console.WriteLine("LoadFileAsync: Getting waveform samples");
            var waveformData = await Task.Run(() => _audioEngine.GetWaveformSamples(2000));
            Console.WriteLine($"LoadFileAsync: Got {waveformData.Length} waveform samples");
            
            WaveformViewModel.WaveformData = waveformData;
            WaveformViewModel.DurationSeconds = _audioEngine.DurationSeconds;
            Console.WriteLine($"LoadFileAsync: Set waveform data, duration = {_audioEngine.DurationSeconds}");
            
            IsFileLoaded = true;
            StartTimeSeconds = 0.0;
            StatusMessage = $"ファイル読み込み完了: {Path.GetFileName(filePath)}";
            StatusMessageColor = "Green";
            
            this.RaisePropertyChanged(nameof(CurrentStartText));
            Console.WriteLine("=== LoadFileAsync: File load completed successfully ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR in LoadFileAsync: {ex}");
            StatusMessage = $"ファイル読み込みエラー: {ex.Message}";
            StatusMessageColor = "Red";
            IsFileLoaded = false;
            WaveformViewModel.WaveformData = Array.Empty<float>();
            WaveformViewModel.DurationSeconds = 0;
        }
    }

    private async Task SelectFileAsync()
    {
        // This would typically open a file dialog
        // For now, we'll provide a placeholder implementation
        StatusMessage = "ファイル選択機能は実装中です。ドラッグ&ドロップをご利用ください。";
        StatusMessageColor = "Orange";
        await Task.CompletedTask;
    }

    private void Play()
    {
        try
        {
            Console.WriteLine("MainWindowViewModel.Play: Starting play method");
            
            if (!IsFileLoaded)
            {
                Console.WriteLine("MainWindowViewModel.Play: No file loaded");
                StatusMessage = "音楽ファイルが読み込まれていません";
                StatusMessageColor = "Red";
                return;
            }

            Console.WriteLine("MainWindowViewModel.Play: Checking duration");
            if (_audioEngine.DurationSeconds <= 0)
            {
                Console.WriteLine("MainWindowViewModel.Play: Invalid duration");
                StatusMessage = "無効な音楽ファイルです";
                StatusMessageColor = "Red";
                return;
            }

            Console.WriteLine("MainWindowViewModel.Play: Setting status to preparing");
            StatusMessage = "再生準備中...";
            StatusMessageColor = "Blue";

            // --- Simplified Playback Logic ---
            Console.WriteLine($"MainWindowViewModel.Play: Starting direct playback. Loop: {Loop}, StartTime: {StartTimeSeconds}s");
            _audioEngine.Loop = Loop;
            _audioEngine.SetPlaybackPosition(StartTimeSeconds);
            _audioEngine.Play();
            // --- End of Simplified Logic ---

            Console.WriteLine("MainWindowViewModel.Play: Setting IsPlaying to true");
            IsPlaying = true;
            
            Console.WriteLine("MainWindowViewModel.Play: Starting position timer");
            _positionTimer.Start();
            
            Console.WriteLine("MainWindowViewModel.Play: Setting success status");
            StatusMessage = "再生中...";
            StatusMessageColor = "Green";

            Console.WriteLine("MainWindowViewModel.Play: Play method completed successfully");
            System.Diagnostics.Debug.WriteLine("Audio playback started successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR in MainWindowViewModel.Play: {ex}");
            IsPlaying = false;
            StatusMessage = $"再生エラー: {ex.Message}";
            StatusMessageColor = "Red";
            System.Diagnostics.Debug.WriteLine($"Play error: {ex}");
        }
    }

    private void Stop()
    {
        try
        {
            Console.WriteLine("MainWindowViewModel.Stop: Starting stop method");

            // --- Simplified Stop Logic ---
            _audioEngine.Stop();
            // --- End of Simplified Logic ---
            
            Console.WriteLine("MainWindowViewModel.Stop: Stopping position timer");
            _positionTimer.Stop();
            
            Console.WriteLine("MainWindowViewModel.Stop: Setting IsPlaying to false");
            IsPlaying = false;
            
            Console.WriteLine("MainWindowViewModel.Stop: Resetting WaveformViewModel.CurrentPosition");
            WaveformViewModel.CurrentPosition = 0;
            
            Console.WriteLine("MainWindowViewModel.Stop: Setting stop status");
            StatusMessage = "再生停止";
            StatusMessageColor = "Gray";
            
            Console.WriteLine("MainWindowViewModel.Stop: Stop method completed successfully");
            System.Diagnostics.Debug.WriteLine("Audio playback stopped successfully");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR in MainWindowViewModel.Stop: {ex}");
            StatusMessage = $"停止エラー: {ex.Message}";
            StatusMessageColor = "Red";
            System.Diagnostics.Debug.WriteLine($"Stop error: {ex}");
        }
    }

    private void OnPlaybackEnded(object? sender, EventArgs e)
    {
        try
        {
            Console.WriteLine("MainWindowViewModel.OnPlaybackEnded: Playback ended event received");
            Dispatcher.UIThread.Post(() => 
            {
                Console.WriteLine("MainWindowViewModel.OnPlaybackEnded: Executing Stop() on UI thread");
                Stop();
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CRITICAL ERROR in MainWindowViewModel.OnPlaybackEnded: {ex}");
        }
    }

    private void OnSeekRequested(object? sender, double seconds)
    {
        System.Diagnostics.Debug.WriteLine($"MainWindowViewModel: OnSeekRequested called with seconds {seconds}");
        StartTimeSeconds = seconds;
        this.RaisePropertyChanged(nameof(CurrentStartText));
        
        StatusMessage = $"開始位置設定: {TimeSpan.FromSeconds(seconds):mm\\:ss}";
        StatusMessageColor = "Blue";
    }

    private void OnPositionTimerTick(object? sender, EventArgs e)
    {
        try
        {
            if (_audioEngine.IsPlaying && WaveformViewModel != null)
            {
                var position = _audioEngine.CurrentPositionSeconds;
                var duration = _audioEngine.DurationSeconds;
                
                if (duration > 0)
                {
                    WaveformViewModel.CurrentPosition = Math.Max(0, Math.Min(1, position / duration));
                }
                else
                {
                    WaveformViewModel.CurrentPosition = 0;
                }
            }
        }
        catch (Exception ex)
        {
            // Ignore timer errors to prevent crashes, but log them for debugging
            System.Diagnostics.Debug.WriteLine($"Error in position timer tick: {ex.Message}");
            Console.WriteLine($"Error in position timer tick: {ex.Message}");
        }
    }

    public void Dispose()
    {
        _positionTimer?.Stop();
        _audioEngine?.Dispose();
    }
}