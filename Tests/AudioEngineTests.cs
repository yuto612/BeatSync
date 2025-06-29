using BGMSyncVisualizer.Audio;
using Xunit;

namespace BGMSyncVisualizer.Tests
{
    public class AudioEngineTests
    {
        [Fact]
        public void LoadFile_InvalidPath_Throws()
        {
            var engine = new AudioEngine();
            Assert.ThrowsAny<Exception>(() => engine.LoadFile("nonexistent.mp3"));
        }
    }
}
