using ReactiveUI;
using System.Reactive;

namespace BGMSyncVisualizer
{
    public class MainWindowViewModel : ReactiveObject
    {
        private string? _statusMessage;
        public string? StatusMessage
        {
            get => _statusMessage;
            set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
        }
    }
}
