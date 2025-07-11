using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BGMSyncVisualizer.Data;

public class UserSettings
{
    [JsonPropertyName("userNotes")]
    public string UserNotes { get; set; } = string.Empty;

    [JsonPropertyName("lastBpm")]
    public int LastBpm { get; set; } = 100;

    [JsonPropertyName("lastFlashPattern")]
    public string LastFlashPattern { get; set; } = "SingleArea";

    [JsonPropertyName("lastVolume")]
    public double LastVolume { get; set; } = 0.7;

    [JsonPropertyName("importedTracks")]
    public List<TrackInfo> ImportedTracks { get; set; } = new();

    [JsonPropertyName("lastUpdateTime")]
    public DateTime LastUpdateTime { get; set; } = DateTime.Now;
}

public class TrackInfo
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString();

    [JsonPropertyName("filePath")]
    public string FilePath { get; set; } = string.Empty;

    [JsonPropertyName("fileName")]
    public string FileName { get; set; } = string.Empty;

    [JsonPropertyName("bpm")]
    public int Bpm { get; set; } = 100;

    [JsonPropertyName("addedTime")]
    public DateTime AddedTime { get; set; } = DateTime.Now;

    [JsonPropertyName("lastUsedTime")]
    public DateTime LastUsedTime { get; set; } = DateTime.Now;
}