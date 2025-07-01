using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using BGMSyncVisualizer.Audio;

namespace BGMSyncVisualizer.Sync
{
    public class BpmSyncController : IDisposable
    {
        private readonly object _syncLock = new object();
        private Timer? _flashTimer;
        private bool _isRunning;
        private int _bpm = 120;
        private DateTime _syncStartTime;
        private AudioEngine? _audioEngine;
        private int _beatCount;
        private bool _disposed;

        public event EventHandler<bool>? FlashStateChanged;

        public int BPM
        {
            get => _bpm;
            set
            {
                if (value < 30 || value > 300)
                    throw new ArgumentOutOfRangeException(nameof(value), "BPM must be between 30 and 300");
                _bpm = value;
            }
        }

        public double IntervalSeconds => 60.0 / _bpm;

        public int IntervalMilliseconds => (int)(IntervalSeconds * 1000);

        public bool IsRunning
        {
            get { lock (_syncLock) { return _isRunning; } }
        }

        public int BeatCount
        {
            get { lock (_syncLock) { return _beatCount; } }
        }

        public void Start(double startTimeSec, AudioEngine audioEngine)
        {
            lock (_syncLock)
            {
                try
                {
                    if (_isRunning)
                        Stop();

                    if (audioEngine == null)
                        throw new ArgumentNullException(nameof(audioEngine), "AudioEngine cannot be null");

                    _audioEngine = audioEngine;
                    _syncStartTime = DateTime.UtcNow;
                    _beatCount = 0;
                    _isRunning = true;

                    // Calculate first beat timing based on start time
                    var firstBeatDelay = CalculateFirstBeatDelay(startTimeSec);
                    
                    // Use high-precision timer for ±10ms accuracy
                    _flashTimer = new Timer(OnFlashTimer, null, firstBeatDelay, IntervalMilliseconds);
                    
                    Debug.WriteLine($"BpmSyncController: Started with BPM={_bpm}, Interval={IntervalMilliseconds}ms, FirstDelay={firstBeatDelay}ms");
                }
                catch (Exception ex)
                {
                    _isRunning = false;
                    _flashTimer?.Dispose();
                    _flashTimer = null;
                    _audioEngine = null;
                    Debug.WriteLine($"BpmSyncController: Start failed: {ex.Message}");
                    throw new InvalidOperationException($"Failed to start BPM sync: {ex.Message}", ex);
                }
            }
        }

        public void Stop()
        {
            lock (_syncLock)
            {
                if (!_isRunning)
                    return;

                _isRunning = false;
                _flashTimer?.Dispose();
                _flashTimer = null;
                _audioEngine = null;
                _beatCount = 0;

                // Reset flash state
                FlashStateChanged?.Invoke(this, false);
                
                Debug.WriteLine("BpmSyncController: Stopped");
            }
        }

        private int CalculateFirstBeatDelay(double startTimeSec)
        {
            // Calculate offset based on start time to align with beat grid
            var beatOffset = (startTimeSec % IntervalSeconds) * 1000;
            var delay = IntervalMilliseconds - (int)beatOffset;
            
            // Ensure positive delay
            if (delay < 0)
                delay += IntervalMilliseconds;
                
            return Math.Max(10, delay); // Minimum 10ms delay
        }

        private void OnFlashTimer(object? state)
        {
            if (!_isRunning)
                return;

            try
            {
                lock (_syncLock)
                {
                    _beatCount++;
                    
                    // Check for sync drift if audio is playing
                    CheckSyncDrift();
                }

                // Trigger flash state change on UI thread - 毎拍でフラッシュ
                FlashStateChanged?.Invoke(this, true);
                
                Debug.WriteLine($"BpmSyncController: Beat #{_beatCount}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BpmSyncController: Timer error: {ex.Message}");
            }
        }

        private void CheckSyncDrift()
        {
            if (_audioEngine == null || !_audioEngine.IsPlaying)
                return;

            try
            {
                var expectedElapsed = _beatCount * IntervalSeconds;
                var actualElapsed = (DateTime.UtcNow - _syncStartTime).TotalSeconds;
                var drift = Math.Abs(actualElapsed - expectedElapsed);

                if (drift > 0.02) // 20ms drift threshold
                {
                    Debug.WriteLine($"BpmSyncController: Sync drift detected: {drift:F3}s");
                    
                    // Auto-resync if drift exceeds threshold
                    if (drift > 0.05) // 50ms threshold for auto-resync
                    {
                        ResyncTimer();
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BpmSyncController: Sync check error: {ex.Message}");
            }
        }

        private void ResyncTimer()
        {
            try
            {
                Debug.WriteLine("BpmSyncController: Performing auto-resync");
                
                // Reset sync start time
                _syncStartTime = DateTime.UtcNow;
                
                // Recreate timer with corrected timing
                _flashTimer?.Dispose();
                _flashTimer = new Timer(OnFlashTimer, null, IntervalMilliseconds, IntervalMilliseconds);
                
                Debug.WriteLine("BpmSyncController: Auto-resync completed");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"BpmSyncController: Resync error: {ex.Message}");
            }
        }

        public TimeSpan GetElapsedTime()
        {
            lock (_syncLock)
            {
                return _isRunning ? DateTime.UtcNow - _syncStartTime : TimeSpan.Zero;
            }
        }

        public double GetCurrentDrift()
        {
            if (!_isRunning || _audioEngine == null)
                return 0;

            try
            {
                var expectedElapsed = _beatCount * IntervalSeconds;
                var actualElapsed = GetElapsedTime().TotalSeconds;
                return Math.Abs(actualElapsed - expectedElapsed);
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                Stop();
                GC.SuppressFinalize(this);
                _disposed = true;
            }
        }
    }
}