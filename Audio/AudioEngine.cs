using NAudio.Wave;
using System;

namespace BGMSyncVisualizer.Audio
{
    public class AudioEngine : IDisposable
    {
        private IWavePlayer? _outputDevice;
        private AudioFileReader? _audioFile;

        public bool IsPlaying => _outputDevice?.PlaybackState == PlaybackState.Playing;
        public double DurationSeconds => _audioFile?.TotalTime.TotalSeconds ?? 0;
        public double CurrentPositionSeconds => _audioFile?.CurrentTime.TotalSeconds ?? 0;

        public void LoadFile(string path)
        {
            Stop();
            _audioFile = new AudioFileReader(path);
            _outputDevice = new WaveOutEvent();
            _outputDevice.Init(_audioFile);
        }

        public float[] GetWaveformSamples(int resolution)
        {
            // 簡易実装: ダウンサンプリング
            if (_audioFile == null) return Array.Empty<float>();
            var samples = new float[resolution];
            int sampleRate = _audioFile.WaveFormat.SampleRate;
            int channels = _audioFile.WaveFormat.Channels;
            int totalSamples = (int)(_audioFile.Length / (sizeof(float) * channels));
            int step = Math.Max(totalSamples / resolution, 1);
            _audioFile.Position = 0;
            var buffer = new float[channels];
            for (int i = 0; i < resolution; i++)
            {
                _audioFile.Read(buffer, 0, buffer.Length);
                samples[i] = buffer[0];
                _audioFile.Position += step * sizeof(float) * channels;
            }
            _audioFile.Position = 0;
            return samples;
        }

        public void SetPlaybackPosition(double seconds)
        {
            if (_audioFile != null)
            {
                _audioFile.CurrentTime = TimeSpan.FromSeconds(seconds);
            }
        }

        public void Play()
        {
            _outputDevice?.Play();
        }

        public void Stop()
        {
            if (_outputDevice != null)
            {
                _outputDevice.Stop();
                _outputDevice.Dispose();
                _outputDevice = null;
            }
            if (_audioFile != null)
            {
                _audioFile.Dispose();
                _audioFile = null;
            }
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
