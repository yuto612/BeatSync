using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using BGMSyncVisualizer.Data;

namespace BGMSyncVisualizer.Services;

public class SettingsService
{
    private const string SettingsFileName = "beatsync_settings.json";
    private readonly string _settingsPath;
    private UserSettings _currentSettings;

    public SettingsService()
    {
        var appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var appFolder = Path.Combine(appDataPath, "BeatSync");
        Directory.CreateDirectory(appFolder);
        _settingsPath = Path.Combine(appFolder, SettingsFileName);
        
        _currentSettings = new UserSettings();
        LoadSettings();
    }

    public UserSettings GetSettings()
    {
        return _currentSettings;
    }

    public async Task SaveSettingsAsync(UserSettings settings)
    {
        try
        {
            settings.LastUpdateTime = DateTime.Now;
            _currentSettings = settings;
            
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var jsonString = JsonSerializer.Serialize(settings, jsonOptions);
            await File.WriteAllTextAsync(_settingsPath, jsonString);
            
            Console.WriteLine($"設定を保存しました: {_settingsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定保存エラー: {ex.Message}");
            throw;
        }
    }

    public void SaveSettings(UserSettings settings)
    {
        try
        {
            settings.LastUpdateTime = DateTime.Now;
            _currentSettings = settings;
            
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            
            var jsonString = JsonSerializer.Serialize(settings, jsonOptions);
            File.WriteAllText(_settingsPath, jsonString);
            
            Console.WriteLine($"設定を保存しました: {_settingsPath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定保存エラー: {ex.Message}");
            throw;
        }
    }

    public void LoadSettings()
    {
        try
        {
            if (File.Exists(_settingsPath))
            {
                var jsonString = File.ReadAllText(_settingsPath);
                var loadedSettings = JsonSerializer.Deserialize<UserSettings>(jsonString);
                
                if (loadedSettings != null)
                {
                    _currentSettings = loadedSettings;
                    Console.WriteLine($"設定を読み込みました: {_settingsPath}");
                }
                else
                {
                    Console.WriteLine("設定ファイルが無効です。デフォルト設定を使用します。");
                    _currentSettings = new UserSettings();
                }
            }
            else
            {
                Console.WriteLine("設定ファイルが見つかりません。デフォルト設定を使用します。");
                _currentSettings = new UserSettings();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"設定読み込みエラー: {ex.Message}");
            _currentSettings = new UserSettings();
        }
    }

    public TrackInfo? GetTrackInfo(string filePath)
    {
        return _currentSettings.ImportedTracks.Find(t => t.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
    }

    public void AddOrUpdateTrack(TrackInfo trackInfo)
    {
        var existingTrack = _currentSettings.ImportedTracks.Find(t => t.FilePath.Equals(trackInfo.FilePath, StringComparison.OrdinalIgnoreCase));
        
        if (existingTrack != null)
        {
            existingTrack.Bpm = trackInfo.Bpm;
            existingTrack.LastUsedTime = DateTime.Now;
        }
        else
        {
            trackInfo.Id = Guid.NewGuid().ToString();
            trackInfo.AddedTime = DateTime.Now;
            trackInfo.LastUsedTime = DateTime.Now;
            _currentSettings.ImportedTracks.Add(trackInfo);
        }
    }

    public void RemoveTrack(string filePath)
    {
        _currentSettings.ImportedTracks.RemoveAll(t => t.FilePath.Equals(filePath, StringComparison.OrdinalIgnoreCase));
    }
}