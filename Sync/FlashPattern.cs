namespace BGMSyncVisualizer.Sync;

public enum FlashPattern
{
    SingleArea,
    FourCircles,
    ProgressiveBar
}

public class FlashPatternInfo
{
    public FlashPattern Pattern { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}