using BGMSyncVisualizer.Audio;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace BGMSyncVisualizer.Tests
{
    public class AudioEngineTests : IDisposable
    {
        private AudioEngine _audioEngine;
        private string _testWavFile;

        public AudioEngineTests()
        {
            _audioEngine = new AudioEngine();
            _testWavFile = CreateTestWavFile();
        }

        private string CreateTestWavFile()
        {
            var tempFile = Path.GetTempFileName() + ".wav";
            
            // Create a simple 1-second WAV file with a sine wave
            using (var writer = new NAudio.Wave.WaveFileWriter(tempFile, new NAudio.Wave.WaveFormat(44100, 16, 1)))
            {
                // Generate 1 second of 440Hz sine wave
                var samples = 44100; // 1 second at 44.1kHz
                var amplitude = 0.25f;
                var frequency = 440.0; // A4 note
                
                for (int i = 0; i < samples; i++)
                {
                    var time = (double)i / 44100.0;
                    var sample = (float)(amplitude * Math.Sin(frequency * 2.0 * Math.PI * time));
                    writer.WriteSample(sample);
                }
            }
            
            return tempFile;
        }

        [Fact]
        public void AudioEngine_ShouldInitializeCorrectly()
        {
            Assert.NotNull(_audioEngine);
            Assert.False(_audioEngine.IsPlaying);
            Assert.Equal(0, _audioEngine.DurationSeconds);
            Assert.Equal(0, _audioEngine.CurrentPositionSeconds);
        }

        [Fact]
        public void LoadFile_WithValidWavFile_ShouldLoadSuccessfully()
        {
            _audioEngine.LoadFile(_testWavFile);
            
            Assert.True(_audioEngine.DurationSeconds > 0);
            Assert.True(_audioEngine.DurationSeconds <= 1.1); // Should be approximately 1 second
            Assert.False(_audioEngine.IsPlaying);
        }

        [Fact]
        public void LoadFile_WithNonExistentFile_ShouldThrowException()
        {
            var nonExistentFile = "non_existent_file.wav";
            
            Assert.Throws<FileNotFoundException>(() => _audioEngine.LoadFile(nonExistentFile));
        }

        [Fact]
        public void LoadFile_WithUnsupportedFormat_ShouldThrowException()
        {
            var unsupportedFile = Path.GetTempFileName() + ".txt";
            File.WriteAllText(unsupportedFile, "This is not an audio file");
            
            try
            {
                Assert.Throws<NotSupportedException>(() => _audioEngine.LoadFile(unsupportedFile));
            }
            finally
            {
                File.Delete(unsupportedFile);
            }
        }

        [Fact]
        public void Play_WithoutLoadedFile_ShouldThrowException()
        {
            Assert.Throws<InvalidOperationException>(() => _audioEngine.Play());
        }

        [Fact]
        public void Play_WithLoadedFile_ShouldStartPlayback()
        {
            _audioEngine.LoadFile(_testWavFile);
            _audioEngine.Play();
            
            // Give it a moment to start
            Task.Delay(100).Wait();
            
            Assert.True(_audioEngine.IsPlaying);
        }

        [Fact]
        public void Stop_WhenPlaying_ShouldStopPlayback()
        {
            _audioEngine.LoadFile(_testWavFile);
            _audioEngine.Play();
            
            // Give it a moment to start
            Task.Delay(100).Wait();
            
            _audioEngine.Stop();
            
            // Give it a moment to stop
            Task.Delay(100).Wait();
            
            Assert.False(_audioEngine.IsPlaying);
        }

        [Fact]
        public void SetPlaybackPosition_WithValidPosition_ShouldSetCorrectPosition()
        {
            _audioEngine.LoadFile(_testWavFile);
            
            var targetPosition = 0.5; // 0.5 seconds
            _audioEngine.SetPlaybackPosition(targetPosition);
            
            // Allow some tolerance for position accuracy
            var actualPosition = _audioEngine.CurrentPositionSeconds;
            Assert.True(Math.Abs(actualPosition - targetPosition) < 0.1, 
                $"Expected position around {targetPosition}, got {actualPosition}");
        }

        [Fact]
        public void GetWaveformSamples_WithLoadedFile_ShouldReturnSamples()
        {
            _audioEngine.LoadFile(_testWavFile);
            
            var samples = _audioEngine.GetWaveformSamples(100);
            
            Assert.NotNull(samples);
            Assert.True(samples.Length > 0);
            Assert.True(samples.Length <= 100);
        }

        [Fact]
        public void GetWaveformSamples_WithoutLoadedFile_ShouldReturnEmpty()
        {
            var samples = _audioEngine.GetWaveformSamples(100);
            
            Assert.NotNull(samples);
            Assert.Empty(samples);
        }

        [Fact]
        public void PlaybackLifecycle_ShouldWorkCorrectly()
        {
            // Load file
            _audioEngine.LoadFile(_testWavFile);
            Assert.False(_audioEngine.IsPlaying);
            Assert.True(_audioEngine.DurationSeconds > 0);
            
            // Start playback
            _audioEngine.Play();
            Task.Delay(100).Wait(); // Allow time to start
            Assert.True(_audioEngine.IsPlaying);
            
            // Check position updates
            var initialPosition = _audioEngine.CurrentPositionSeconds;
            Task.Delay(200).Wait(); // Wait a bit
            var laterPosition = _audioEngine.CurrentPositionSeconds;
            Assert.True(laterPosition >= initialPosition, "Position should advance during playback");
            
            // Stop playback
            _audioEngine.Stop();
            Task.Delay(100).Wait(); // Allow time to stop
            Assert.False(_audioEngine.IsPlaying);
        }

        public void Dispose()
        {
            _audioEngine?.Dispose();
            
            if (File.Exists(_testWavFile))
            {
                try
                {
                    File.Delete(_testWavFile);
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
        }
    }
}