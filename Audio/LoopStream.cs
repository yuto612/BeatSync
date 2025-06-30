using NAudio.Wave;
using System;

namespace BGMSyncVisualizer.Audio;

/// <summary>
/// Stream for looping playback.
/// Original source from NAudio Demo project.
/// </summary>
public class LoopStream : WaveStream
{
    private readonly WaveStream _sourceStream;
    private readonly object _lockObject = new object();

    /// <summary>
    /// Creates a new Loop stream.
    /// </summary>
    /// <param name="sourceStream">The stream to read from. Note: the Read method of this stream should return 0 when it reaches the end
    /// or else looping will not work.</param>
    public LoopStream(WaveStream sourceStream)
    {
        _sourceStream = sourceStream;
        EnableLooping = true;
    }

    /// <summary>
    /// Use this to turn looping on or off.
    /// </summary>
    public bool EnableLooping { get; set; }

    /// <summary>
    /// Return source stream's wave format.
    /// </summary>
    public override WaveFormat WaveFormat => _sourceStream.WaveFormat;

    /// <summary>
    /// LoopStream simply returns the source stream's length.
    /// </summary>
    public override long Length => _sourceStream.Length;

    /// <summary>
    /// LoopStream simply passes on positioning to the source stream.
    /// </summary>
    public override long Position
    {
        get { lock (_lockObject) { return _sourceStream.Position; } }
        set { lock (_lockObject) { _sourceStream.Position = value; } }
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        lock (_lockObject)
        {
            int totalBytesRead = 0;

            while (totalBytesRead < count)
            {
                int bytesRead = _sourceStream.Read(buffer, offset + totalBytesRead, count - totalBytesRead);
                if (bytesRead == 0)
                {
                    if (_sourceStream.Position == 0 || !EnableLooping)
                    {
                        // Source stream has ended or looping is disabled
                        break;
                    }
                    // Loop by resetting the source stream's position
                    _sourceStream.Position = 0;
                }
                totalBytesRead += bytesRead;
            }
            return totalBytesRead;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _sourceStream.Dispose();
        }
        base.Dispose(disposing);
    }
} 