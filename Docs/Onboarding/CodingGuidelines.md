# 📝 BeatSync コーディング規約ガイド

## 概要

BeatSyncプロジェクトでは、保守性、可読性、一貫性を重視したコーディング規約を採用しています。このガイドラインに従うことで、チーム開発を円滑に進め、高品質なコードベースを維持できます。

## 基本原則

### 1. 可読性の優先
- **明確性**: コードの意図が明確に分かること
- **簡潔性**: 不必要な複雑さを避けること
- **一貫性**: プロジェクト全体で統一されたスタイル

### 2. 保守性の確保
- **単一責任の原則**: クラス・メソッドは一つの責任のみ
- **開放閉鎖原則**: 拡張に開いて、修正に閉じる
- **依存性の注入**: 疎結合な設計

### 3. パフォーマンスの配慮
- **メモリ効率**: 不要なオブジェクト生成を避ける
- **処理効率**: O(n²)以上の計算量を避ける
- **リソース管理**: IDisposableの適切な実装

## C# コーディング規約

### 1. 命名規則

#### クラス・インターフェース・構造体
```csharp
// PascalCase を使用
public class AudioEngine { }
public interface IAudioPlayer { }
public struct BpmSettings { }

// 明確で説明的な名前
public class BpmSyncController  // Good
public class Controller         // Bad: 何をcontrolするか不明
```

#### メソッド・プロパティ
```csharp
// PascalCase を使用
public void LoadFile(string filePath) { }
public int BPM { get; set; }
public bool IsPlaying { get; private set; }

// 動詞で始める（メソッド）
public void StartPlayback() { }      // Good
public void PlaybackStart() { }      // Bad: 動詞が後
```

#### フィールド・変数
```csharp
// camelCase を使用
private int _bpm;                    // フィールドは _ プレフィックス
private readonly string _filePath;
private static readonly TimeSpan DefaultInterval = TimeSpan.FromSeconds(1);

// ローカル変数
public void Method()
{
    var currentTime = DateTime.Now;  // camelCase
    string fileName = "sample.mp3";
}
```

#### 定数・列挙型
```csharp
// PascalCase を使用
public const int MaxBpm = 300;
public const string DefaultAudioFormat = "mp3";

public enum FlashPattern
{
    SingleArea,
    FourCircles,
    ProgressiveBar
}
```

### 2. レイアウト・フォーマット

#### using文の順序
```csharp
// 1. System namespaces
using System;
using System.IO;
using System.Threading.Tasks;

// 2. Third-party libraries
using Avalonia.Controls;
using ReactiveUI;

// 3. Project namespaces
using BGMSyncVisualizer.Audio;
using BGMSyncVisualizer.Sync;
```

#### クラス構造の順序
```csharp
public class ExampleClass : BaseClass, IInterface
{
    // 1. Constants
    private const int DefaultValue = 100;
    
    // 2. Static fields
    private static readonly object _lockObject = new();
    
    // 3. Fields
    private readonly AudioEngine _audioEngine;
    private int _bpm = 120;
    
    // 4. Constructors
    public ExampleClass(AudioEngine audioEngine)
    {
        _audioEngine = audioEngine;
    }
    
    // 5. Properties
    public int BPM 
    { 
        get => _bpm; 
        set => this.RaiseAndSetIfChanged(ref _bpm, value); 
    }
    
    // 6. Events
    public event Action<int>? BpmChanged;
    
    // 7. Public methods
    public void StartPlayback() { }
    
    // 8. Private methods
    private void OnBpmChanged() { }
    
    // 9. IDisposable implementation
    public void Dispose()
    {
        _audioEngine?.Dispose();
    }
}
```

#### メソッド内の構造
```csharp
public async Task LoadFileAsync(string filePath)
{
    // 1. 引数チェック
    if (string.IsNullOrEmpty(filePath))
        throw new ArgumentException("ファイルパスが指定されていません", nameof(filePath));
    
    // 2. ローカル変数宣言
    var audioFormat = Path.GetExtension(filePath);
    bool loadSuccess = false;
    
    try
    {
        // 3. メインロジック
        loadSuccess = await _audioEngine.LoadFileAsync(filePath);
        
        if (loadSuccess)
        {
            // 4. 成功時の処理
            StatusMessage = "ファイルが正常に読み込まれました";
            IsFileLoaded = true;
        }
    }
    catch (Exception ex)
    {
        // 5. エラーハンドリング
        StatusMessage = $"ファイル読み込みエラー: {ex.Message}";
        throw;
    }
}
```

### 3. コメント規約

#### XMLドキュメンテーション
```csharp
/// <summary>
/// 指定されたBPMでビート同期を開始します
/// </summary>
/// <param name="bpm">BPM値（30-300の範囲）</param>
/// <param name="startTime">同期開始時間</param>
/// <returns>同期が正常に開始された場合true</returns>
/// <exception cref="ArgumentOutOfRangeException">BPMが範囲外の場合</exception>
public bool StartSync(int bpm, TimeSpan startTime)
{
    // 実装...
}
```

#### インラインコメント
```csharp
public void ProcessBeat()
{
    // ビートカウントを更新（1-4の循環）
    _beatCount = (_beatCount % 4) + 1;
    
    // NOTE: ドリフト補正は10ms以上の場合のみ実行
    if (Math.Abs(_driftTime.TotalMilliseconds) > 10)
    {
        CorrectDrift();
    }
    
    // TODO: 将来的にビート強度の動的調整を追加
    TriggerFlash(_beatCount);
}
```

#### コメントタグの使用
```csharp
// TODO: [優先度: 高] NAudio以外の音声ライブラリ対応を検討
// HACK: 一時的な回避策、適切な解決方法を要検討
// NOTE: この実装はWindows固有の動作に依存
// FIXME: メモリリークの可能性があるため修正が必要
// BUG: 特定の条件下でNullReferenceExceptionが発生
```

### 4. 例外処理

#### 例外処理のベストプラクティス
```csharp
public async Task<bool> LoadFileAsync(string filePath)
{
    try
    {
        // 特定の例外を期待する処理
        return await _audioEngine.LoadAsync(filePath);
    }
    catch (FileNotFoundException ex)
    {
        // 具体的な例外処理
        _logger.LogError(ex, "音楽ファイルが見つかりません: {FilePath}", filePath);
        StatusMessage = "指定されたファイルが見つかりません";
        return false;
    }
    catch (UnauthorizedAccessException ex)
    {
        // アクセス権限エラー
        _logger.LogError(ex, "ファイルアクセス権限エラー: {FilePath}", filePath);
        StatusMessage = "ファイルへのアクセス権限がありません";
        return false;
    }
    catch (Exception ex)
    {
        // 予期しない例外
        _logger.LogError(ex, "予期しないエラー: {FilePath}", filePath);
        StatusMessage = "予期しないエラーが発生しました";
        throw; // 再スロー
    }
}
```

#### 引数検証
```csharp
public void SetBpm(int bpm)
{
    if (bpm < 30 || bpm > 300)
        throw new ArgumentOutOfRangeException(nameof(bpm), bpm, "BPMは30-300の範囲で指定してください");
    
    _bpm = bpm;
}

public void LoadFile(string filePath)
{
    _ = filePath ?? throw new ArgumentNullException(nameof(filePath));
    
    if (string.IsNullOrWhiteSpace(filePath))
        throw new ArgumentException("ファイルパスが空です", nameof(filePath));
    
    // 処理続行...
}
```

## MVVM パターン規約

### 1. ViewModelの実装

#### プロパティの実装
```csharp
public class MainWindowViewModel : ReactiveObject
{
    private int _bpm = 120;
    
    // ReactiveUIを使用したプロパティ
    public int BPM
    {
        get => _bpm;
        set => this.RaiseAndSetIfChanged(ref _bpm, value);
    }
    
    // 読み取り専用プロパティ
    public string BpmDisplay => $"{BPM} BPM";
    
    // 条件付きプロパティ
    public bool CanPlay => IsFileLoaded && !IsPlaying;
}
```

#### コマンドの実装
```csharp
public class MainWindowViewModel : ReactiveObject
{
    public MainWindowViewModel()
    {
        // シンプルなコマンド
        PlayCommand = ReactiveCommand.Create(Play, this.WhenAnyValue(x => x.CanPlay));
        
        // 非同期コマンド
        LoadFileCommand = ReactiveCommand.CreateFromTask<string>(LoadFileAsync);
        
        // パラメータ付きコマンド
        SetBpmCommand = ReactiveCommand.Create<int>(SetBpm);
    }
    
    public ReactiveCommand<Unit, Unit> PlayCommand { get; }
    public ReactiveCommand<string, Unit> LoadFileCommand { get; }
    public ReactiveCommand<int, Unit> SetBpmCommand { get; }
    
    private void Play() { /* 実装 */ }
    private async Task LoadFileAsync(string filePath) { /* 実装 */ }
    private void SetBpm(int bpm) { /* 実装 */ }
}
```

### 2. View-ViewModel バインディング

#### XAMLでのバインディング
```xml
<!-- データコンテキストの明示 -->
<Window x:DataType="vm:MainWindowViewModel">
    <!-- 双方向バインディング -->
    <NumericUpDown Value="{Binding BPM, Mode=TwoWay}" />
    
    <!-- コマンドバインディング -->
    <Button Content="再生" Command="{Binding PlayCommand}" />
    
    <!-- 条件付き表示 -->
    <TextBlock Text="{Binding StatusMessage}" 
               IsVisible="{Binding HasStatusMessage}" />
    
    <!-- コンバーター使用 -->
    <Border IsVisible="{Binding FlashPattern, 
                       Converter={x:Static converters:EnumToBoolConverter.Instance}, 
                       ConverterParameter=SingleArea}" />
</Window>
```

## 非同期プログラミング規約

### 1. async/await パターン

#### 基本的な非同期メソッド
```csharp
// 非同期メソッドは Async サフィックス
public async Task<bool> LoadFileAsync(string filePath)
{
    try
    {
        // ConfigureAwait(false) でデッドロック回避
        var result = await _audioEngine.LoadAsync(filePath).ConfigureAwait(false);
        
        // UIスレッドでの実行が必要な場合
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            StatusMessage = "読み込み完了";
        });
        
        return result;
    }
    catch (Exception ex)
    {
        // 例外も非同期的に処理
        await LogErrorAsync(ex).ConfigureAwait(false);
        throw;
    }
}
```

#### CancellationTokenの使用
```csharp
public async Task ProcessLongRunningTaskAsync(CancellationToken cancellationToken = default)
{
    for (int i = 0; i < 1000; i++)
    {
        // キャンセル要求をチェック
        cancellationToken.ThrowIfCancellationRequested();
        
        // 重い処理
        await ProcessItemAsync(i, cancellationToken).ConfigureAwait(false);
        
        // 定期的なキャンセルチェック
        if (i % 100 == 0)
        {
            cancellationToken.ThrowIfCancellationRequested();
        }
    }
}
```

### 2. タスク並列処理

#### 並列実行
```csharp
public async Task LoadMultipleFilesAsync(IEnumerable<string> filePaths)
{
    // 並列実行（ただし同時実行数を制限）
    var semaphore = new SemaphoreSlim(Environment.ProcessorCount);
    var tasks = filePaths.Select(async filePath =>
    {
        await semaphore.WaitAsync();
        try
        {
            return await LoadSingleFileAsync(filePath);
        }
        finally
        {
            semaphore.Release();
        }
    });
    
    var results = await Task.WhenAll(tasks);
}
```

## パフォーマンス規約

### 1. メモリ管理

#### IDisposableの実装
```csharp
public class AudioEngine : IDisposable
{
    private WaveOutEvent? _waveOut;
    private bool _disposed = false;
    
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                // マネージドリソースを解放
                _waveOut?.Dispose();
            }
            
            // アンマネージドリソースを解放
            // （このクラスでは該当なし）
            
            _disposed = true;
        }
    }
    
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
```

#### using文の使用
```csharp
// using宣言（C# 8.0+）
public void ProcessFile(string filePath)
{
    using var fileStream = File.OpenRead(filePath);
    using var reader = new BinaryReader(fileStream);
    
    // ファイル処理...
    // スコープ終了時に自動的にDispose
}

// 従来のusing文
public void ProcessFileTraditional(string filePath)
{
    using (var fileStream = File.OpenRead(filePath))
    using (var reader = new BinaryReader(fileStream))
    {
        // ファイル処理...
    }
}
```

### 2. コレクション操作

#### LINQ の効率的な使用
```csharp
public class TrackManager
{
    private readonly List<TrackInfo> _tracks = new();
    
    // 効率的なフィルタリング
    public IEnumerable<TrackInfo> GetRecentTracks(int days)
    {
        var cutoffDate = DateTime.Now.AddDays(-days);
        
        // 遅延実行でメモリ効率を向上
        return _tracks.Where(t => t.LastUsedTime >= cutoffDate)
                     .OrderByDescending(t => t.LastUsedTime);
    }
    
    // インデックスアクセスの活用
    public TrackInfo? FindTrackByPath(string filePath)
    {
        // FirstOrDefault は見つかった時点で停止
        return _tracks.FirstOrDefault(t => 
            string.Equals(t.FilePath, filePath, StringComparison.OrdinalIgnoreCase));
    }
}
```

## テスト規約

### 1. 単体テストの作成

#### テストメソッドの命名
```csharp
[TestClass]
public class AudioEngineTests
{
    // [メソッド名]_[条件]_[期待結果] パターン
    [TestMethod]
    public void LoadFile_ValidMp3File_ReturnsTrue()
    {
        // Arrange
        var audioEngine = new AudioEngine();
        var testFilePath = "TestData/sample.mp3";
        
        // Act
        var result = audioEngine.LoadFile(testFilePath);
        
        // Assert
        Assert.IsTrue(result);
        Assert.IsTrue(audioEngine.IsFileLoaded);
    }
    
    [TestMethod]
    public void SetBpm_ValueOutOfRange_ThrowsArgumentOutOfRangeException()
    {
        // Arrange
        var controller = new BpmSyncController();
        
        // Act & Assert
        Assert.ThrowsException<ArgumentOutOfRangeException>(() => 
            controller.BPM = 500);
    }
}
```

### 2. モックとスタブ

#### 依存関係のモック化
```csharp
[TestMethod]
public void PlayCommand_FileLoaded_CallsAudioEnginePlay()
{
    // Arrange
    var mockAudioEngine = new Mock<IAudioEngine>();
    mockAudioEngine.Setup(x => x.IsFileLoaded).Returns(true);
    
    var viewModel = new MainWindowViewModel(mockAudioEngine.Object);
    
    // Act
    viewModel.PlayCommand.Execute().Subscribe();
    
    // Assert
    mockAudioEngine.Verify(x => x.Play(), Times.Once);
}
```

## エラーハンドリング規約

### 1. ログ出力

#### 構造化ログの使用
```csharp
public class AudioEngine
{
    private readonly ILogger<AudioEngine> _logger;
    
    public bool LoadFile(string filePath)
    {
        try
        {
            _logger.LogInformation("音楽ファイル読み込み開始: {FilePath}", filePath);
            
            // ファイル読み込み処理...
            
            _logger.LogInformation("音楽ファイル読み込み完了: {FilePath}, Duration: {Duration}", 
                                 filePath, duration);
            return true;
        }
        catch (FileNotFoundException ex)
        {
            _logger.LogWarning(ex, "ファイルが見つかりません: {FilePath}", filePath);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "音楽ファイル読み込みエラー: {FilePath}", filePath);
            throw;
        }
    }
}
```

### 2. エラー報告

#### ユーザーフレンドリーなエラーメッセージ
```csharp
public class ErrorMessageHelper
{
    public static string GetUserFriendlyMessage(Exception ex)
    {
        return ex switch
        {
            FileNotFoundException => "指定されたファイルが見つかりません。ファイルパスを確認してください。",
            UnauthorizedAccessException => "ファイルへのアクセス権限がありません。",
            InvalidDataException => "ファイルが破損しているか、サポートされていない形式です。",
            OutOfMemoryException => "ファイルが大きすぎます。より小さいファイルを選択してください。",
            _ => "予期しないエラーが発生しました。サポートにお問い合わせください。"
        };
    }
}
```

## セキュリティ規約

### 1. ファイルパス検証
```csharp
public bool IsValidFilePath(string filePath)
{
    if (string.IsNullOrWhiteSpace(filePath))
        return false;
    
    try
    {
        // パストラバーサル攻撃の防止
        var fullPath = Path.GetFullPath(filePath);
        var allowedDirectory = Path.GetFullPath(@"C:\AllowedMusicFiles");
        
        return fullPath.StartsWith(allowedDirectory, StringComparison.OrdinalIgnoreCase);
    }
    catch
    {
        return false;
    }
}
```

### 2. 設定ファイルの検証
```csharp
public bool ValidateUserSettings(UserSettings settings)
{
    // BPM値の範囲チェック
    if (settings.ImportedTracks.Any(t => t.Bpm < 30 || t.Bpm > 300))
        return false;
    
    // ファイルパスの検証
    if (settings.ImportedTracks.Any(t => !IsValidFilePath(t.FilePath)))
        return false;
    
    return true;
}
```

---

この規約に従うことで、BeatSyncプロジェクトの品質と保守性を高く保つことができます。新しいコードを書く際は、これらのガイドラインを参考にして実装してください。