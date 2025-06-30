using ReactiveUI;
using System;
using System.IO;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;
using BGMSyncVisualizer.Audio;
using BGMSyncVisualizer.Sync;
using Avalonia.Threading;
using Avalonia.Media;
using Avalonia.Animation;
using Avalonia.Styling;

namespace BGMSyncVisualizer.UI;

public class MainWindowViewModel : ReactiveObject, IDisposable
{
    private readonly AudioEngine _audioEngine;
    private readonly DispatcherTimer _positionTimer;
    private readonly BpmSyncController _bpmSyncController;
    private readonly BpmFlashController _flashController;

    private bool _isFileLoaded;
    private bool _isPlaying;
    private double _startTimeSeconds;
    private string _statusMessage = "mp3またはwavファイルをドロップするか、クリックして選択してください";
    private string _statusMessageColor = "Gray";

    // BPM Flash related properties
    private int _bpm = 100;
    private bool _isBpmSyncEnabled = true;
    private bool _isFlashing;
    private IBrush _flashBackground = Brushes.White;
    private IBrush _flashTextColor = Brushes.Black;
    private double _flashOpacity = 1.0;
    
    // New UI properties
    private string _currentFileName = "No file selected";
    private string _audioDurationText = "00:00";
    private string _currentPositionText = "00:00";
    
    // Flash Pattern properties
    private FlashPattern _selectedFlashPattern = FlashPattern.SingleArea;
    private IBrush _circle1Color = Brushes.Gray;
    private IBrush _circle2Color = Brushes.Gray;
    private IBrush _circle3Color = Brushes.Gray;
    private IBrush _circle4Color = Brushes.Gray;
    private double _circle1GlowRadius = 0;
    private double _circle2GlowRadius = 0;
    private double _circle3GlowRadius = 0;
    private double _circle4GlowRadius = 0;
    private Color _circle1GlowColor = Colors.Transparent;
    private Color _circle2GlowColor = Colors.Transparent;
    private Color _circle3GlowColor = Colors.Transparent;
    private Color _circle4GlowColor = Colors.Transparent;
    
    // Progressive Bar properties
    private IBrush _bar1Color = Brushes.Gray;
    private IBrush _bar2Color = Brushes.Gray;
    private IBrush _bar3Color = Brushes.Gray;
    private IBrush _bar4Color = Brushes.Gray;
    private IBrush _bar1TextColor = Brushes.White;
    private IBrush _bar2TextColor = Brushes.White;
    private IBrush _bar3TextColor = Brushes.White;
    private IBrush _bar4TextColor = Brushes.White;
    
    // Pattern Selection properties
    private IBrush _pattern1Background = new SolidColorBrush(Color.Parse("#3182CE"));
    private IBrush _pattern2Background = new SolidColorBrush(Color.Parse("#4A5568"));
    private IBrush _pattern3Background = new SolidColorBrush(Color.Parse("#4A5568"));

    public MainWindowViewModel()
    {
        Console.WriteLine("=== BPM Sync Visualizer Starting ===");
        Console.WriteLine("MainWindowViewModel: Initializing components...");
        
        _audioEngine = new AudioEngine();
        Console.WriteLine("MainWindowViewModel: AudioEngine created");
        
        _bpmSyncController = new BpmSyncController();
        _bpmSyncController.FlashStateChanged += OnFlashStateChanged;
        Console.WriteLine("MainWindowViewModel: BpmSyncController created");
        
        _flashController = new BpmFlashController();
        _flashController.FlashEvent += OnFlashEvent;
        Console.WriteLine("MainWindowViewModel: BpmFlashController created");
        
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
        
        // BPM Commands
        IncreaseBpmCommand = ReactiveCommand.Create(() => { if (BPM < 300) BPM++; });
        DecreaseBpmCommand = ReactiveCommand.Create(() => { if (BPM > 30) BPM--; });
        SetBpmCommand = ReactiveCommand.Create<int>(newBpm => BPM = newBpm);
        
        // Flash Pattern Commands
        SelectFlashPatternCommand = ReactiveCommand.Create<string>(SelectFlashPattern);
        
        Console.WriteLine("MainWindowViewModel: Commands created");
        
        Console.WriteLine("=== MainWindowViewModel initialization completed ===");
    }

    public WaveformControlViewModel WaveformViewModel { get; }

    public ICommand SelectFileCommand { get; }
    public ICommand PlayCommand { get; }
    public ICommand StopCommand { get; }
    
    // BPM Commands
    public ICommand IncreaseBpmCommand { get; }
    public ICommand DecreaseBpmCommand { get; }
    public ICommand SetBpmCommand { get; }
    
    // Flash Pattern Commands
    public ICommand SelectFlashPatternCommand { get; }

    public bool IsFileLoaded
    {
        get => _isFileLoaded;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isFileLoaded, value);
            this.RaisePropertyChanged(nameof(CanSeek));
            WaveformViewModel.CanSeek = CanSeek;
        }
    }

    public bool IsPlaying
    {
        get => _isPlaying;
        private set
        {
            this.RaiseAndSetIfChanged(ref _isPlaying, value);
            this.RaisePropertyChanged(nameof(CanSeek));
            WaveformViewModel.CanSeek = CanSeek;
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

    // BPM Flash Properties
    public int BPM
    {
        get => _bpm;
        set
        {
            try
            {
                if (value >= 30 && value <= 300)
                {
                    this.RaiseAndSetIfChanged(ref _bpm, value);
                    if (_bpmSyncController != null)
                    {
                        _bpmSyncController.BPM = value;
                    }
                    this.RaisePropertyChanged(nameof(CurrentBpmLabel));
                    
                    // Update status to show valid BPM
                    if (!IsPlaying)
                    {
                        StatusMessage = $"BPM設定: {value}";
                        StatusMessageColor = "Green";
                    }
                }
                else
                {
                    // Show validation error
                    StatusMessage = $"BPMは30-300の範囲で入力してください（現在: {value}）";
                    StatusMessageColor = "Red";
                    Console.WriteLine($"Invalid BPM value: {value}. Must be between 30-300");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"BPM設定エラー: {ex.Message}";
                StatusMessageColor = "Red";
                Console.WriteLine($"Error setting BPM: {ex}");
            }
        }
    }

    public bool IsBpmSyncEnabled
    {
        get => _isBpmSyncEnabled;
        set => this.RaiseAndSetIfChanged(ref _isBpmSyncEnabled, value);
    }

    public bool IsFlashing
    {
        get => _isFlashing;
        private set => this.RaiseAndSetIfChanged(ref _isFlashing, value);
    }

    public IBrush FlashBackground
    {
        get => _flashBackground;
        private set => this.RaiseAndSetIfChanged(ref _flashBackground, value);
    }

    public IBrush FlashTextColor
    {
        get => _flashTextColor;
        private set => this.RaiseAndSetIfChanged(ref _flashTextColor, value);
    }

    public double FlashOpacity
    {
        get => _flashOpacity;
        private set => this.RaiseAndSetIfChanged(ref _flashOpacity, value);
    }

    public string CurrentBpmLabel => $"{_bpm} BPM ({60.0 / _bpm:F2}s間隔)";

    // New UI Properties for XAML bindings
    public string CurrentFileName
    {
        get => _currentFileName;
        set => this.RaiseAndSetIfChanged(ref _currentFileName, value);
    }

    public string AudioDurationText
    {
        get => _audioDurationText;
        set => this.RaiseAndSetIfChanged(ref _audioDurationText, value);
    }

    public string CurrentPositionText
    {
        get => _currentPositionText;
        set => this.RaiseAndSetIfChanged(ref _currentPositionText, value);
    }

    public string FlashStatusText => IsPlaying ? "PLAYING" : "READY";

    public string CurrentBpmDisplay => $"{BPM} BPM";

    public string SyncDebugInfo => _bpmSyncController?.IsRunning == true 
        ? $"ビート#{_bpmSyncController.BeatCount} | ドリフト:{_bpmSyncController.GetCurrentDrift():F3}s"
        : "同期停止中";

    // Flash Pattern Properties
    public FlashPattern SelectedFlashPattern
    {
        get => _selectedFlashPattern;
        set 
        {
            this.RaiseAndSetIfChanged(ref _selectedFlashPattern, value);
            _flashController.SelectedFlashPattern = value;
            UpdatePatternButtonColors();
            this.RaisePropertyChanged(nameof(CurrentPatternDescription));
        }
    }

    public string CurrentPatternDescription => SelectedFlashPattern switch
    {
        FlashPattern.SingleArea => "単一エリア - 1拍目を強調した基本フラッシュ",
        FlashPattern.FourCircles => "4つの円 - 各拍に対応する円が順番に光る",
        FlashPattern.ProgressiveBar => "プログレッシブバー - 拍に合わせてバーが伸びる",
        _ => "Unknown Pattern"
    };

    public string BeatCounterText => _flashController?.BeatCounterText ?? "1/4";

    // Four Circles Properties
    public IBrush Circle1Color { get => _circle1Color; private set => this.RaiseAndSetIfChanged(ref _circle1Color, value); }
    public IBrush Circle2Color { get => _circle2Color; private set => this.RaiseAndSetIfChanged(ref _circle2Color, value); }
    public IBrush Circle3Color { get => _circle3Color; private set => this.RaiseAndSetIfChanged(ref _circle3Color, value); }
    public IBrush Circle4Color { get => _circle4Color; private set => this.RaiseAndSetIfChanged(ref _circle4Color, value); }
    public double Circle1GlowRadius { get => _circle1GlowRadius; private set => this.RaiseAndSetIfChanged(ref _circle1GlowRadius, value); }
    public double Circle2GlowRadius { get => _circle2GlowRadius; private set => this.RaiseAndSetIfChanged(ref _circle2GlowRadius, value); }
    public double Circle3GlowRadius { get => _circle3GlowRadius; private set => this.RaiseAndSetIfChanged(ref _circle3GlowRadius, value); }
    public double Circle4GlowRadius { get => _circle4GlowRadius; private set => this.RaiseAndSetIfChanged(ref _circle4GlowRadius, value); }
    public Color Circle1GlowColor { get => _circle1GlowColor; private set => this.RaiseAndSetIfChanged(ref _circle1GlowColor, value); }
    public Color Circle2GlowColor { get => _circle2GlowColor; private set => this.RaiseAndSetIfChanged(ref _circle2GlowColor, value); }
    public Color Circle3GlowColor { get => _circle3GlowColor; private set => this.RaiseAndSetIfChanged(ref _circle3GlowColor, value); }
    public Color Circle4GlowColor { get => _circle4GlowColor; private set => this.RaiseAndSetIfChanged(ref _circle4GlowColor, value); }

    // Progressive Bar Properties
    public IBrush Bar1Color { get => _bar1Color; private set => this.RaiseAndSetIfChanged(ref _bar1Color, value); }
    public IBrush Bar2Color { get => _bar2Color; private set => this.RaiseAndSetIfChanged(ref _bar2Color, value); }
    public IBrush Bar3Color { get => _bar3Color; private set => this.RaiseAndSetIfChanged(ref _bar3Color, value); }
    public IBrush Bar4Color { get => _bar4Color; private set => this.RaiseAndSetIfChanged(ref _bar4Color, value); }
    public IBrush Bar1TextColor { get => _bar1TextColor; private set => this.RaiseAndSetIfChanged(ref _bar1TextColor, value); }
    public IBrush Bar2TextColor { get => _bar2TextColor; private set => this.RaiseAndSetIfChanged(ref _bar2TextColor, value); }
    public IBrush Bar3TextColor { get => _bar3TextColor; private set => this.RaiseAndSetIfChanged(ref _bar3TextColor, value); }
    public IBrush Bar4TextColor { get => _bar4TextColor; private set => this.RaiseAndSetIfChanged(ref _bar4TextColor, value); }

    // Pattern Selection Button Colors
    public IBrush Pattern1Background { get => _pattern1Background; private set => this.RaiseAndSetIfChanged(ref _pattern1Background, value); }
    public IBrush Pattern2Background { get => _pattern2Background; private set => this.RaiseAndSetIfChanged(ref _pattern2Background, value); }
    public IBrush Pattern3Background { get => _pattern3Background; private set => this.RaiseAndSetIfChanged(ref _pattern3Background, value); }

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
            CurrentFileName = Path.GetFileName(filePath);
            AudioDurationText = FormatTime(_audioEngine.DurationSeconds);
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
            this.RaisePropertyChanged(nameof(FlashStatusText));
            
            Console.WriteLine("MainWindowViewModel.Play: Starting position timer");
            _positionTimer.Start();
            
            Console.WriteLine("MainWindowViewModel.Play: Setting success status");
            StatusMessage = "再生中...";
            StatusMessageColor = "Green";

            // Start BPM sync if enabled
            if (IsBpmSyncEnabled)
            {
                try
                {
                    _bpmSyncController.BPM = _bpm;
                    _bpmSyncController.Start(StartTimeSeconds, _audioEngine);
                    
                    // Start Flash Controller
                    _flashController.Start();
                    Console.WriteLine($"MainWindowViewModel.Play: BPM sync and flash controller started with {_bpm} BPM");
                }
                catch (Exception syncEx)
                {
                    Console.WriteLine($"MainWindowViewModel.Play: BPM sync error: {syncEx.Message}");
                }
            }

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
            
            // Stop BPM sync and flash controller
            _bpmSyncController.Stop();
            _flashController.Stop();
            Console.WriteLine("MainWindowViewModel.Stop: BPM sync and flash controller stopped");
            
            Console.WriteLine("MainWindowViewModel.Stop: Stopping position timer");
            _positionTimer.Stop();
            
            Console.WriteLine("MainWindowViewModel.Stop: Setting IsPlaying to false");
            IsPlaying = false;
            this.RaisePropertyChanged(nameof(FlashStatusText));
            CurrentPositionText = "00:00";
            
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
                    CurrentPositionText = FormatTime(position);
                }
                else
                {
                    WaveformViewModel.CurrentPosition = 0;
                    CurrentPositionText = "00:00";
                }
            }

            // Update sync debug info during playback
            if (_bpmSyncController?.IsRunning == true)
            {
                this.RaisePropertyChanged(nameof(SyncDebugInfo));
            }
        }
        catch (Exception ex)
        {
            // Ignore timer errors to prevent crashes, but log them for debugging
            System.Diagnostics.Debug.WriteLine($"Error in position timer tick: {ex.Message}");
            Console.WriteLine($"Error in position timer tick: {ex.Message}");
        }
    }

    private void OnFlashStateChanged(object? sender, bool isFlashing)
    {
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                IsFlashing = isFlashing;
                
                // 既存のBmpSyncControllerからのビートでBmpFlashControllerをトリガー
                if (isFlashing && _flashController.IsRunning)
                {
                    _flashController.OnBeatTrigger();
                }
                
                // 単一エリアパターンの場合のみ、従来のフラッシュロジックを維持
                if (SelectedFlashPattern == FlashPattern.SingleArea)
                {
                    // Enhanced color transition for flash effect with better contrast
                    if (isFlashing)
                    {
                        // Dark flash with bright accent
                        FlashBackground = new SolidColorBrush(Color.Parse("#1A202C"));
                        FlashTextColor = new SolidColorBrush(Color.Parse("#F7FAFC"));
                    }
                    else
                    {
                        // Light flash with dark text
                        FlashBackground = new SolidColorBrush(Color.Parse("#F7FAFC"));
                        FlashTextColor = new SolidColorBrush(Color.Parse("#2D3748"));
                    }
                    
                    // Subtle opacity animation for smoother visual transition
                    FlashOpacity = isFlashing ? 0.95 : 1.0;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MainWindowViewModel.OnFlashStateChanged: Error: {ex.Message}");
        }
    }

    private void OnFlashEvent(object? sender, FlashEventArgs e)
    {
        try
        {
            Dispatcher.UIThread.Post(() =>
            {
                // 拍数カウンターを更新
                this.RaisePropertyChanged(nameof(BeatCounterText));
                
                // パターンに応じたフラッシュを実行
                switch (e.Pattern)
                {
                    case FlashPattern.SingleArea:
                        ExecuteSingleAreaFlash(e);
                        break;
                    case FlashPattern.FourCircles:
                        ExecuteFourCirclesFlash(e);
                        break;
                    case FlashPattern.ProgressiveBar:
                        ExecuteProgressiveBarFlash(e);
                        break;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"MainWindowViewModel.OnFlashEvent: Error: {ex.Message}");
        }
    }

    private void SelectFlashPattern(string patternName)
    {
        if (Enum.TryParse<FlashPattern>(patternName, out var pattern))
        {
            SelectedFlashPattern = pattern;
            Console.WriteLine($"MainWindowViewModel: Flash pattern changed to {pattern}");
        }
    }

    private void UpdatePatternButtonColors()
    {
        var activeColor = new SolidColorBrush(Color.Parse("#3182CE"));
        var inactiveColor = new SolidColorBrush(Color.Parse("#4A5568"));

        Pattern1Background = SelectedFlashPattern == FlashPattern.SingleArea ? activeColor : inactiveColor;
        Pattern2Background = SelectedFlashPattern == FlashPattern.FourCircles ? activeColor : inactiveColor;
        Pattern3Background = SelectedFlashPattern == FlashPattern.ProgressiveBar ? activeColor : inactiveColor;
    }

    private void ExecuteSingleAreaFlash(FlashEventArgs e)
    {
        if (e.IsActive)
        {
            if (e.IsStrong)
            {
                // 1拍目は非常に強く光る - 鮮やかな赤
                FlashBackground = new SolidColorBrush(Color.Parse("#FF0000"));
                FlashTextColor = new SolidColorBrush(Color.Parse("#FFFFFF"));
                FlashOpacity = 1.0;
            }
            else
            {
                // 2-4拍目は青く光る
                FlashBackground = new SolidColorBrush(Color.Parse("#0066FF"));
                FlashTextColor = new SolidColorBrush(Color.Parse("#FFFFFF"));
                FlashOpacity = 0.9;
            }
        }
        else
        {
            // リセット - 暗い背景
            FlashBackground = new SolidColorBrush(Color.Parse("#1A202C"));
            FlashTextColor = new SolidColorBrush(Color.Parse("#F7FAFC"));
            FlashOpacity = 1.0;
        }
    }

    private void ExecuteFourCirclesFlash(FlashEventArgs e)
    {
        // 全ての円をリセット
        var inactiveColor = new SolidColorBrush(Color.Parse("#4A5568"));
        var activeColor = new SolidColorBrush(Color.Parse("#3182CE"));
        var strongColor = new SolidColorBrush(Color.Parse("#E53E3E"));

        Circle1Color = inactiveColor;
        Circle2Color = inactiveColor;
        Circle3Color = inactiveColor;
        Circle4Color = inactiveColor;
        Circle1GlowRadius = 0;
        Circle2GlowRadius = 0;
        Circle3GlowRadius = 0;
        Circle4GlowRadius = 0;

        if (e.IsActive)
        {
            var glowRadius = e.IsStrong ? 20.0 : 10.0;
            var circleColor = e.IsStrong ? strongColor : activeColor;
            var glowColor = e.IsStrong ? Colors.Red : Colors.Blue;

            switch (e.Beat)
            {
                case 1:
                    Circle1Color = circleColor;
                    Circle1GlowRadius = glowRadius;
                    Circle1GlowColor = glowColor;
                    break;
                case 2:
                    Circle2Color = circleColor;
                    Circle2GlowRadius = glowRadius;
                    Circle2GlowColor = glowColor;
                    break;
                case 3:
                    Circle3Color = circleColor;
                    Circle3GlowRadius = glowRadius;
                    Circle3GlowColor = glowColor;
                    break;
                case 4:
                    Circle4Color = circleColor;
                    Circle4GlowRadius = glowRadius;
                    Circle4GlowColor = glowColor;
                    break;
            }
        }
    }

    private void ExecuteProgressiveBarFlash(FlashEventArgs e)
    {
        var inactiveColor = new SolidColorBrush(Color.Parse("#4A5568"));
        var activeColor = new SolidColorBrush(Color.Parse("#3182CE"));
        var strongColor = new SolidColorBrush(Color.Parse("#E53E3E"));
        var whiteText = new SolidColorBrush(Color.Parse("#F7FAFC"));

        if (!e.IsActive)
        {
            // リセット
            Bar1Color = inactiveColor;
            Bar2Color = inactiveColor;
            Bar3Color = inactiveColor;
            Bar4Color = inactiveColor;
            Bar1TextColor = whiteText;
            Bar2TextColor = whiteText;
            Bar3TextColor = whiteText;
            Bar4TextColor = whiteText;
            return;
        }

        // 現在の拍まで色を付ける
        for (int i = 1; i <= 4; i++)
        {
            var barColor = inactiveColor;
            var textColor = whiteText;

            if (i <= e.Beat)
            {
                if (i == 1 && e.IsStrong)
                {
                    barColor = strongColor;
                }
                else if (i <= e.Beat)
                {
                    barColor = activeColor;
                }
            }

            switch (i)
            {
                case 1:
                    Bar1Color = barColor;
                    Bar1TextColor = textColor;
                    break;
                case 2:
                    Bar2Color = barColor;
                    Bar2TextColor = textColor;
                    break;
                case 3:
                    Bar3Color = barColor;
                    Bar3TextColor = textColor;
                    break;
                case 4:
                    Bar4Color = barColor;
                    Bar4TextColor = textColor;
                    break;
            }
        }
    }


    private string FormatTime(double seconds)
    {
        var timeSpan = TimeSpan.FromSeconds(seconds);
        return $"{timeSpan.Minutes:D2}:{timeSpan.Seconds:D2}";
    }

    public void Dispose()
    {
        _positionTimer?.Stop();
        _bpmSyncController?.Dispose();
        _flashController?.Dispose();
        _audioEngine?.Dispose();
    }
}