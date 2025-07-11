# 📚 BeatSync API リファレンス

## 概要

BeatSyncの主要なクラスとそのAPIについて詳細に説明します。このリファレンスは開発者がアプリケーションの機能を拡張したり、内部構造を理解する際に役立ちます。

## Audio Layer

### AudioEngine クラス

音声ファイルの読み込み、再生制御を担当するコアクラス。

```csharp
public class AudioEngine : IDisposable
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `IsPlaying` | `bool` | 現在再生中かどうか |
| `Duration` | `TimeSpan` | 音楽ファイルの総再生時間 |
| `Position` | `TimeSpan` | 現在の再生位置 |
| `Volume` | `float` | 音量（0.0-1.0） |

#### イベント

```csharp
// 波形データが利用可能になった時に発火
public event Action<float[]>? WaveformDataAvailable;

// 再生が停止した時に発火
public event Action? PlaybackStopped;
```

#### メソッド

##### LoadFile
```csharp
public bool LoadFile(string filePath)
```
**説明**: 指定されたファイルパスから音楽ファイルを読み込みます。

**パラメータ**:
- `filePath` (string): 読み込む音楽ファイルのパス

**戻り値**: 
- `bool`: 読み込みが成功した場合 `true`

**サポート形式**: MP3, WAV, FLAC, M4A, AAC

**例外**:
- `FileNotFoundException`: ファイルが見つからない場合
- `InvalidDataException`: サポートされていない形式の場合

**使用例**:
```csharp
var audioEngine = new AudioEngine();
if (audioEngine.LoadFile(@"C:\Music\sample.mp3"))
{
    Console.WriteLine($"Duration: {audioEngine.Duration}");
}
```

##### Play
```csharp
public void Play()
```
**説明**: 音楽の再生を開始します。

**前提条件**: `LoadFile` が成功している必要があります。

##### Stop
```csharp
public void Stop()
```
**説明**: 音楽の再生を停止し、位置を開始位置にリセットします。

##### SetVolume
```csharp
public void SetVolume(float volume)
```
**説明**: 再生音量を設定します。

**パラメータ**:
- `volume` (float): 音量レベル（0.0 = 無音、1.0 = 最大音量）

##### SetStartTime
```csharp
public void SetStartTime(double seconds)
```
**説明**: 再生開始位置を設定します。

**パラメータ**:
- `seconds` (double): 開始位置（秒）

### LoopStream クラス

指定時間からのループ再生機能を提供。

```csharp
public class LoopStream : WaveStream
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `Length` | `long` | ストリームの長さ（バイト） |
| `Position` | `long` | 現在の位置（バイト） |
| `WaveFormat` | `WaveFormat` | 音声フォーマット情報 |

#### メソッド

##### SetStartTime
```csharp
public void SetStartTime(double seconds)
```
**説明**: ループ開始位置を設定します。

**パラメータ**:
- `seconds` (double): 開始位置（秒）

## Sync Layer

### BpmSyncController クラス

高精度BPM同期制御を提供するクラス。

```csharp
public class BpmSyncController : IDisposable
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `BPM` | `int` | 現在のBPM値（30-300） |
| `IsRunning` | `bool` | 同期が実行中かどうか |
| `BeatCount` | `int` | 現在のビートカウント（1-4） |
| `LastBeatTime` | `double` | 最後のビート時刻 |

#### イベント

```csharp
// ビートが検出された時に発火
public event Action? BeatDetected;

// ビートカウントが変更された時に発火
public event Action<int>? BeatCountChanged;
```

#### メソッド

##### Start
```csharp
public void Start()
```
**説明**: BPM同期を開始します。

**前提条件**: `BPM` プロパティが有効な値（30-300）に設定されている必要があります。

##### Stop
```csharp
public void Stop()
```
**説明**: BPM同期を停止します。

##### GetCurrentDrift
```csharp
public double GetCurrentDrift()
```
**説明**: 現在のタイミングドリフトを取得します。

**戻り値**: 
- `double`: ドリフト時間（秒）。正の値は遅れ、負の値は進みを示します。

**使用例**:
```csharp
var controller = new BpmSyncController();
controller.BPM = 120;
controller.BeatDetected += () => Console.WriteLine("Beat!");
controller.Start();

// 精度確認
var drift = controller.GetCurrentDrift();
Console.WriteLine($"Current drift: {drift:F3}s");
```

### BpmFlashController クラス

フラッシュパターンの実行制御を担当。

```csharp
public class BpmFlashController
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `SelectedFlashPattern` | `FlashPattern` | 現在選択されているフラッシュパターン |
| `BeatCounterText` | `string` | ビートカウンター表示テキスト |

#### イベント

```csharp
// フラッシュがトリガーされた時に発火
public event Action<FlashPattern, int>? FlashTriggered;
```

#### FlashPattern 列挙型

```csharp
public enum FlashPattern
{
    SingleArea,      // 単一エリアフラッシュ
    FourCircles,     // 4つの円パターン
    ProgressiveBar   // プログレッシブバーパターン
}
```

## UI Layer

### MainWindowViewModel クラス

アプリケーションのメインロジックを制御するビューモデル。

```csharp
public class MainWindowViewModel : ReactiveObject, IDisposable
```

#### 主要プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `BPM` | `int` | BPM値（30-300） |
| `IsFileLoaded` | `bool` | ファイルが読み込み済みかどうか |
| `IsPlaying` | `bool` | 再生中かどうか |
| `StatusMessage` | `string` | ステータスメッセージ |
| `Volume` | `double` | 音量（0.0-1.0） |
| `StartTimeText` | `string` | 開始時間（mm:ss形式） |
| `UserNotes` | `string` | ユーザーメモ |
| `ImportedTracks` | `ObservableCollection<TrackInfoViewModel>` | インポートされた楽曲リスト |

#### フラッシュ関連プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `SelectedFlashPattern` | `FlashPattern` | 選択されたフラッシュパターン |
| `FlashBackground` | `IBrush` | フラッシュ背景色 |
| `FlashOpacity` | `double` | フラッシュ透明度 |
| `Circle1Color` | `IBrush` | 円1の色（FourCirclesパターン） |
| `Circle2Color` | `IBrush` | 円2の色（FourCirclesパターン） |
| `Circle3Color` | `IBrush` | 円3の色（FourCirclesパターン） |
| `Circle4Color` | `IBrush` | 円4の色（FourCirclesパターン） |

#### コマンド

```csharp
public ICommand PlayCommand { get; }           // 再生コマンド
public ICommand StopCommand { get; }           // 停止コマンド
public ICommand ClearFileCommand { get; }      // ファイルクリアコマンド
public ICommand IncreaseBpmCommand { get; }    // BPM増加コマンド
public ICommand DecreaseBpmCommand { get; }    // BPM減少コマンド
public ICommand SaveCurrentBpmCommand { get; } // BPM保存コマンド
public ICommand ToggleFullscreenCommand { get; } // 全画面切り替えコマンド
```

#### メソッド

##### LoadFileAsync
```csharp
public async Task LoadFileAsync(string filePath)
```
**説明**: 音楽ファイルを非同期で読み込みます。

**パラメータ**:
- `filePath` (string): ファイルパス

**処理内容**:
1. ファイル存在確認
2. AudioEngineでの読み込み
3. 波形データ生成
4. 楽曲情報の設定更新

##### SaveCurrentBpm
```csharp
private void SaveCurrentBpm()
```
**説明**: 現在のBPM設定を楽曲情報として保存します。

### TrackInfoViewModel クラス

楽曲情報の表示と操作を担当するビューモデル。

```csharp
public class TrackInfoViewModel : ReactiveObject
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `TrackInfo` | `TrackInfo` | 楽曲情報データ |
| `FileName` | `string` | ファイル名 |
| `BpmDisplay` | `string` | BPM表示文字列 |
| `FilePath` | `string` | ファイルパス |
| `Bpm` | `int` | BPM値 |
| `Notes` | `string` | メモ |

#### コマンド

```csharp
public ReactiveCommand<Unit, Unit> LoadCommand { get; }   // 読み込みコマンド
public ReactiveCommand<Unit, Unit> RemoveCommand { get; } // 削除コマンド
```

#### イベント

```csharp
public event Action<TrackInfoViewModel>? LoadRequested;   // 読み込み要求
public event Action<TrackInfoViewModel>? RemoveRequested; // 削除要求
```

### WaveformControlViewModel クラス

波形表示制御を担当するビューモデル。

```csharp
public class WaveformControlViewModel : ReactiveObject
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `Points` | `PointCollection` | 波形表示用ポイントコレクション |
| `DurationSeconds` | `double` | 音楽の長さ（秒） |
| `CurrentPosition` | `double` | 現在の再生位置（秒） |

#### メソッド

##### UpdateWaveform
```csharp
public void UpdateWaveform(float[] waveformData)
```
**説明**: 波形データを更新し、表示用ポイントを生成します。

**パラメータ**:
- `waveformData` (float[]): 正規化された波形データ配列

## Services Layer

### SettingsService クラス

アプリケーション設定とデータの永続化を担当。

```csharp
public class SettingsService
```

#### メソッド

##### GetSettings
```csharp
public UserSettings GetSettings()
```
**説明**: 現在の設定を取得します。

**戻り値**: 
- `UserSettings`: 現在の設定オブジェクト

##### SaveSettingsAsync
```csharp
public async Task SaveSettingsAsync(UserSettings settings)
```
**説明**: 設定を非同期で保存します。

**パラメータ**:
- `settings` (UserSettings): 保存する設定

##### LoadSettings
```csharp
public void LoadSettings()
```
**説明**: 設定ファイルから設定を読み込みます。

**保存場所**: `%AppData%/BeatSync/beatsync_settings.json`

##### GetTrackInfo
```csharp
public TrackInfo? GetTrackInfo(string filePath)
```
**説明**: 指定されたファイルパスの楽曲情報を取得します。

**パラメータ**:
- `filePath` (string): ファイルパス

**戻り値**: 
- `TrackInfo?`: 楽曲情報（見つからない場合はnull）

##### AddOrUpdateTrack
```csharp
public void AddOrUpdateTrack(TrackInfo trackInfo)
```
**説明**: 楽曲情報を追加または更新します。

**パラメータ**:
- `trackInfo` (TrackInfo): 楽曲情報

##### RemoveTrack
```csharp
public void RemoveTrack(string filePath)
```
**説明**: 指定されたファイルパスの楽曲情報を削除します。

**パラメータ**:
- `filePath` (string): ファイルパス

## Data Layer

### UserSettings クラス

ユーザー設定データの構造定義。

```csharp
public class UserSettings
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `UserNotes` | `string` | ユーザーメモ |
| `ImportedTracks` | `List<TrackInfo>` | インポートされた楽曲リスト |
| `LastUpdateTime` | `DateTime` | 最終更新時刻 |

### TrackInfo クラス

楽曲情報データの構造定義。

```csharp
public class TrackInfo
```

#### プロパティ

| プロパティ | 型 | 説明 |
|-----------|---|------|
| `Id` | `string` | 一意識別子（GUID） |
| `FileName` | `string` | ファイル名 |
| `FilePath` | `string` | ファイルパス |
| `Bpm` | `int` | BPM値 |
| `Notes` | `string` | メモ |
| `AddedTime` | `DateTime` | 追加時刻 |
| `LastUsedTime` | `DateTime` | 最終使用時刻 |

## Converters

### EnumToBoolConverter クラス

XAML で列挙型をbool値に変換するコンバーター。

```csharp
public class EnumToBoolConverter : IValueConverter
```

#### メソッド

##### Convert
```csharp
public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
```
**説明**: 列挙型の値をbool値に変換します。

**使用例（XAML）**:
```xml
<Border IsVisible="{Binding SelectedFlashPattern, 
                   Converter={x:Static converters:EnumToBoolConverter.Instance}, 
                   ConverterParameter=SingleArea}" />
```

## 使用例とサンプルコード

### 基本的な音楽再生
```csharp
// AudioEngineの初期化と使用
var audioEngine = new AudioEngine();
audioEngine.PlaybackStopped += () => Console.WriteLine("再生停止");

// ファイル読み込みと再生
if (audioEngine.LoadFile("sample.mp3"))
{
    audioEngine.SetVolume(0.7f);
    audioEngine.Play();
}
```

### BPM同期フラッシュ
```csharp
// 同期コントローラーの初期化
var syncController = new BpmSyncController();
var flashController = new BpmFlashController(syncController, viewModel);

// BPM設定と同期開始
syncController.BPM = 120;
syncController.BeatDetected += () => 
{
    Console.WriteLine($"Beat {syncController.BeatCount}");
};

syncController.Start();
```

### 楽曲情報の管理
```csharp
// 設定サービスの使用
var settingsService = new SettingsService();

// 楽曲情報の追加
var trackInfo = new TrackInfo
{
    FileName = "sample.mp3",
    FilePath = @"C:\Music\sample.mp3",
    Bpm = 120,
    Notes = "お気に入りの曲"
};

settingsService.AddOrUpdateTrack(trackInfo);
await settingsService.SaveSettingsAsync(settingsService.GetSettings());
```

### カスタムフラッシュパターンの追加

新しいフラッシュパターンを追加する場合：

1. `FlashPattern` 列挙型に新しい値を追加
2. `BpmFlashController` に対応するメソッドを実装

```csharp
// FlashPattern.cs に追加
public enum FlashPattern
{
    SingleArea,
    FourCircles,
    ProgressiveBar,
    NewPattern // 新しいパターン
}

// BpmFlashController.cs に追加
private void ApplyNewPatternFlash(int beatNumber)
{
    // 新しいパターンの実装
    switch (beatNumber)
    {
        case 1:
            // 1拍目の処理
            break;
        // 他の拍の処理...
    }
}
```

---

このAPIリファレンスは、BeatSyncの全ての主要クラスとメソッドを網羅的に説明しています。新機能の追加や既存機能の拡張を行う際は、このリファレンスを参考にして適切なAPIを使用してください。