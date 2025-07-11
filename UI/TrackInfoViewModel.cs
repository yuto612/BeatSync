using BGMSyncVisualizer.Data;
using ReactiveUI;
using System;
using System.Reactive;

namespace BGMSyncVisualizer.UI;

public class TrackInfoViewModel : ReactiveObject
{
    public TrackInfo TrackInfo { get; }

    public TrackInfoViewModel(TrackInfo trackInfo)
    {
        TrackInfo = trackInfo;
        LoadCommand = ReactiveCommand.Create(() => LoadRequested?.Invoke(this));
        RemoveCommand = ReactiveCommand.Create(() => RemoveRequested?.Invoke(this));
    }

    public string FileName => TrackInfo.FileName;
    public string BpmDisplay => $"{TrackInfo.Bpm} BPM";
    public string FilePath => TrackInfo.FilePath;
    public int Bpm => TrackInfo.Bpm;
    
    public void RefreshDisplay()
    {
        this.RaisePropertyChanged(nameof(BpmDisplay));
        this.RaisePropertyChanged(nameof(Bpm));
    }
    
    public ReactiveCommand<Unit, Unit> LoadCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
    
    public event Action<TrackInfoViewModel>? LoadRequested;
    public event Action<TrackInfoViewModel>? RemoveRequested;
}