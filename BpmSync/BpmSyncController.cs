using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BGMSyncVisualizer.BpmSync
{
    public class BpmSyncController
    {
        private CancellationTokenSource? _cts;
        private readonly object _lock = new();

        public double BPM { get; set; } = 120;
        public double IntervalSeconds => 60.0 / BPM;

        public event Action? Beat;

        public void Start(Func<double> getAudioPosition)
        {
            Stop();
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            Task.Run(async () =>
            {
                var sw = Stopwatch.StartNew();
                double next = IntervalSeconds;
                while (!token.IsCancellationRequested)
                {
                    double audioPos = getAudioPosition();
                    double diff = Math.Abs(audioPos - next);
                    if (diff > 0.01)
                    {
                        next = audioPos + IntervalSeconds;
                    }
                    else
                    {
                        Beat?.Invoke();
                        next += IntervalSeconds;
                    }
                    double delay = Math.Max(0, next - sw.Elapsed.TotalSeconds);
                    await Task.Delay(TimeSpan.FromSeconds(delay), token);
                }
            }, token);
        }

        public void Stop()
        {
            _cts?.Cancel();
            _cts = null;
        }
    }
}
