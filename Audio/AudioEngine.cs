using System;
using System.Diagnostics;
using System.IO;
using NAudio.Wave;

namespace BGMSyncVisualizer.Audio
{
    public class AudioEngine : IDisposable
    {
        private WaveOutEvent? _waveOut;
        private AudioFileReader? _audioFile;
        private LoopStream? _loopStream;
        private float[]? _waveformData;
        private bool _disposed;
        private readonly object _playbackLock = new object();

        public AudioEngine()
        {
            try
            {
                NAudio.MediaFoundation.MediaFoundationApi.Startup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"MediaFoundation initialization failed: {ex.Message}");
            }
        }

        public event EventHandler? PlaybackEnded;

        public bool Loop { get; set; }

        public bool IsPlaying { get { lock (_playbackLock) { return _waveOut?.PlaybackState == PlaybackState.Playing; } } }

        public double DurationSeconds { get { lock (_playbackLock) { return _audioFile?.TotalTime.TotalSeconds ?? 0; } } }

        public double CurrentPositionSeconds
        {
            get { lock (_playbackLock) { return _loopStream?.CurrentTime.TotalSeconds ?? 0; } }
            set { lock (_playbackLock) { if (_loopStream != null) _loopStream.CurrentTime = TimeSpan.FromSeconds(value); } }
        }

        public float Volume 
        { 
            get { lock (_playbackLock) { return _waveOut?.Volume ?? 1.0f; } }
            set { lock (_playbackLock) { if (_waveOut != null) _waveOut.Volume = Math.Max(0f, Math.Min(1f, value)); } }
        }

        public void LoadFile(string filePath)
        {
            lock (_playbackLock)
            {
                DisposeResources();
                try
                {
                    _audioFile = new AudioFileReader(filePath);
                    _loopStream = new LoopStream(_audioFile);
                    _waveOut = new WaveOutEvent();
                    _waveOut.Init(_loopStream);
                    _waveOut.PlaybackStopped += OnPlaybackStopped;
                    GenerateWaveformData();
                }
                catch (Exception ex)
                {
                    DisposeResources();
                    throw new InvalidOperationException($"Failed to load audio file: {ex.Message}", ex);
                }
            }
        }

        public float[] GetWaveformSamples(int resolution)
        {
            lock (_playbackLock)
            {
                if (_waveformData == null) return Array.Empty<float>();
                if (_waveformData.Length <= resolution) return _waveformData;

                var result = new float[resolution];
                var step = (float)_waveformData.Length / resolution;
                for (int i = 0; i < resolution; i++)
                {
                    var start = (int)(i * step);
                    var end = Math.Min((int)((i + 1) * step), _waveformData.Length);
                    float max = 0;
                    for (int j = start; j < end; j++)
                    {
                        max = Math.Max(max, Math.Abs(_waveformData[j]));
                    }
                    result[i] = max;
                }
                return result;
            }
        }

        public void SetPlaybackPosition(double seconds)
        {
            lock (_playbackLock)
            {
                if (_loopStream == null) return;
                var totalSeconds = _loopStream.TotalTime.TotalSeconds;
                seconds = Math.Max(0, Math.Min(seconds, totalSeconds));
                _loopStream.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
        }

        public void Play()
        {
            lock (_playbackLock)
            {
                if (_waveOut == null || _loopStream == null)
                    throw new InvalidOperationException("No audio file loaded");

                _loopStream.EnableLooping = Loop;
                if (_waveOut.PlaybackState != PlaybackState.Playing)
                {
                    _waveOut.Play();
                }
            }
        }

        public void Stop()
        {
            lock (_playbackLock)
            {
                _waveOut?.Stop();
            }
        }

        private void GenerateWaveformData()
        {
            if (_audioFile == null) return;
            try
            {
                var originalPosition = _audioFile.Position;
                _audioFile.Position = 0;
                var sampleProvider = _audioFile.ToSampleProvider();
                var sampleCount = (int)(_audioFile.Length / (_audioFile.WaveFormat.BitsPerSample / 8));
                _waveformData = new float[sampleCount];
                sampleProvider.Read(_waveformData, 0, sampleCount);
                _audioFile.Position = originalPosition;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error generating waveform: {ex.Message}");
                _waveformData = null;
            }
        }

        private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
        {
            if (e.Exception != null)
            {
                Debug.WriteLine($"Playback Error: {e.Exception.Message}");
            }
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }

        private void DisposeResources()
        {
            _waveOut?.Stop();
            if (_waveOut != null)
            {
                _waveOut.PlaybackStopped -= OnPlaybackStopped;
                _waveOut.Dispose();
                _waveOut = null;
            }
            _loopStream?.Dispose();
            _loopStream = null;
            _audioFile?.Dispose();
            _audioFile = null;
            _waveformData = null;
        }

        public void Dispose()
        {
            lock (_playbackLock)
            {
                if (!_disposed)
                {
                    DisposeResources();
                    GC.SuppressFinalize(this);
                    _disposed = true;
                }
            }
        }
    }
}