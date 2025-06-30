using BGMSyncVisualizer.UI;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace BGMSyncVisualizer.Tests
{
    public class MainWindowViewModelTests : IDisposable
    {
        private MainWindowViewModel _viewModel;
        private string _testWavFile;

        public MainWindowViewModelTests()
        {
            _viewModel = new MainWindowViewModel();
            _testWavFile = CreateTestWavFile();
        }

        private string CreateTestWavFile()
        {
            var tempFile = Path.GetTempFileName() + ".wav";
            
            // Create a simple 2-second WAV file with a sine wave
            using (var writer = new NAudio.Wave.WaveFileWriter(tempFile, new NAudio.Wave.WaveFormat(44100, 16, 1)))
            {
                // Generate 2 seconds of 440Hz sine wave
                var samples = 88200; // 2 seconds at 44.1kHz
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
        public void MainWindowViewModel_ShouldInitializeCorrectly()
        {
            Assert.NotNull(_viewModel);
            Assert.False(_viewModel.IsFileLoaded);
            Assert.False(_viewModel.IsPlaying);
            Assert.Equal(120, _viewModel.BPM);
            Assert.Equal(0.0, _viewModel.StartTimeSeconds);
        }

        [Fact]
        public async Task LoadFileAsync_WithValidFile_ShouldLoadSuccessfully()
        {
            await _viewModel.LoadFileAsync(_testWavFile);
            
            Assert.True(_viewModel.IsFileLoaded);
            Assert.True(_viewModel.WaveformViewModel.DurationSeconds > 0);
            Assert.True(_viewModel.WaveformViewModel.WaveformData.Length > 0);
        }

        [Fact]
        public async Task LoadFileAsync_WithNonExistentFile_ShouldHandleError()
        {
            var nonExistentFile = "non_existent_file.wav";
            
            await _viewModel.LoadFileAsync(nonExistentFile);
            
            Assert.False(_viewModel.IsFileLoaded);
            Assert.Contains("ファイルが見つかりません", _viewModel.StatusMessage);
        }

        [Fact]
        public void BPM_SetValidValue_ShouldUpdateCorrectly()
        {
            _viewModel.BPM = 140;
            
            Assert.Equal(140, _viewModel.BPM);
        }

        [Fact]
        public void BPM_SetInvalidValue_ShouldRejectAndShowError()
        {
            _viewModel.BPM = 400; // Too high
            
            Assert.Equal(120, _viewModel.BPM); // Should remain unchanged
            Assert.Contains("BPMは30から300の間で入力してください", _viewModel.StatusMessage);
        }

        [Fact]
        public async Task PlayCommand_WithoutLoadedFile_ShouldNotPlay()
        {
            // Try to play without loading a file
            if (_viewModel.PlayCommand.CanExecute(null))
            {
                _viewModel.PlayCommand.Execute(null);
            }
            
            Assert.False(_viewModel.IsPlaying);
        }

        [Fact]
        public async Task PlayCommand_WithLoadedFile_ShouldStartPlayback()
        {
            await _viewModel.LoadFileAsync(_testWavFile);
            
            // Wait a moment for file to be fully loaded
            await Task.Delay(100);
            
            if (_viewModel.PlayCommand.CanExecute(null))
            {
                _viewModel.PlayCommand.Execute(null);
                
                // Wait a moment for playback to start
                await Task.Delay(200);
                
                Assert.True(_viewModel.IsPlaying);
            }
        }

        [Fact]
        public async Task StopCommand_WhenPlaying_ShouldStopPlayback()
        {
            await _viewModel.LoadFileAsync(_testWavFile);
            await Task.Delay(100);
            
            if (_viewModel.PlayCommand.CanExecute(null))
            {
                _viewModel.PlayCommand.Execute(null);
                await Task.Delay(200);
                
                if (_viewModel.StopCommand.CanExecute(null))
                {
                    _viewModel.StopCommand.Execute(null);
                    await Task.Delay(100);
                    
                    Assert.False(_viewModel.IsPlaying);
                }
            }
        }

        [Fact]
        public async Task PlaybackLifecycle_ShouldWorkWithoutCrashing()
        {
            // This is the critical test - simulate the full playback lifecycle
            // that was causing crashes in the real application
            
            await _viewModel.LoadFileAsync(_testWavFile);
            await Task.Delay(100);
            
            Assert.True(_viewModel.IsFileLoaded);
            
            // Start playback
            if (_viewModel.PlayCommand.CanExecute(null))
            {
                _viewModel.PlayCommand.Execute(null);
                await Task.Delay(200); // Wait for playback to start
                
                Assert.True(_viewModel.IsPlaying);
                
                // Let it play for a bit to simulate the timer tick behavior
                await Task.Delay(500);
                
                // The timer should be updating positions without crashing
                Assert.True(_viewModel.IsPlaying);
                
                // Stop playback
                if (_viewModel.StopCommand.CanExecute(null))
                {
                    _viewModel.StopCommand.Execute(null);
                    await Task.Delay(100);
                    
                    Assert.False(_viewModel.IsPlaying);
                }
            }
        }

        [Fact]
        public async Task WaveformSeek_ShouldUpdateStartTime()
        {
            await _viewModel.LoadFileAsync(_testWavFile);
            await Task.Delay(100);
            
            // Simulate clicking on waveform at 50% position
            var normalizedPosition = 0.5;
            _viewModel.WaveformViewModel.OnWaveformClicked(normalizedPosition);
            
            var expectedStartTime = normalizedPosition * _viewModel.WaveformViewModel.DurationSeconds;
            Assert.True(Math.Abs(_viewModel.StartTimeSeconds - expectedStartTime) < 0.1);
        }

        public void Dispose()
        {
            _viewModel?.Dispose();
            
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