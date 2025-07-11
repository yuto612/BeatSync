# ❓ BeatSync 開発者 FAQ

## 開発環境について

### Q1: BeatSyncの開発に最適なIDEは何ですか？

**A**: 以下のIDEを推奨しています：

1. **Visual Studio 2022** (Windows) - 最も推奨
   - Avaloniaの完全サポート
   - 優れたデバッガー
   - IntelliSenseの完全対応

2. **JetBrains Rider** (クロスプラットフォーム)
   - 優秀なコード分析
   - Git統合
   - リファクタリング機能

3. **Visual Studio Code** (軽量)
   - クロスプラットフォーム
   - 豊富な拡張機能
   - 軽量で高速

### Q2: .NET 8以外のバージョンでも動作しますか？

**A**: いいえ、BeatSyncは.NET 8専用です。理由：

- Avalonia 11.0が.NET 8を要求
- 最新のC#言語機能を使用
- パフォーマンスの最適化

.NET 6や7で動作させるには大幅な修正が必要です。

### Q3: macOSやLinuxでの開発は可能ですか？

**A**: はい、可能です。ただし制限があります：

**対応状況**:
- ✅ コード編集・ビルド・実行
- ✅ テスト実行
- ⚠️ デザイナー（機能制限あり）
- ❌ Windows固有の音声機能

**推奨環境**: 
- メイン開発: Windows
- 副次開発: macOS/Linux

## プロジェクト構造について

### Q4: なぜMVVMパターンを採用しているのですか？

**A**: 以下の理由でMVVMを採用：

1. **テスタビリティ**: ビジネスロジックとUIの分離
2. **保守性**: 責任の明確な分離
3. **Avaloniaとの親和性**: フレームワークの標準パターン
4. **データバインディング**: 宣言的UI開発

### Q5: なぜNAudioを選択したのですか？

**A**: NAudioの選択理由：

**長所**:
- .NETネイティブライブラリ
- 豊富な音声フォーマット対応
- 高いパフォーマンス
- アクティブな開発コミュニティ
- 詳細な音声制御

**検討した代替案**:
- MediaFoundation: Windows限定
- BASS.NET: 商用ライセンス必要
- OpenTK: 低レベル過ぎる

### Q6: 設定ファイルがJSONな理由は？

**A**: JSON選択の理由：

1. **可読性**: 人間が読める形式
2. **エコシステム**: .NETでの標準サポート
3. **軽量**: XMLより軽量
4. **デバッグ**: 手動編集が容易

**他形式との比較**:
- XML: 冗長すぎる
- Binary: デバッグ困難
- SQLite: オーバースペック

## 音声処理について

### Q7: サポートされる音声フォーマットは？

**A**: 現在サポートしているフォーマット：

| フォーマット | 拡張子 | 備考 |
|-------------|--------|------|
| MP3 | .mp3 | 最も一般的 |
| WAV | .wav | 高音質・無圧縮 |
| FLAC | .flac | ロスレス圧縮 |
| M4A | .m4a | Apple標準 |
| AAC | .aac | 高効率圧縮 |

**追加予定**:
- OGG Vorbis（優先度: 中）
- WMA（優先度: 低）

### Q8: BPM同期の精度はどの程度ですか？

**A**: 同期精度の詳細：

- **目標精度**: ±10ms
- **実測精度**: ±5-15ms（環境依存）
- **測定方法**: `BpmSyncController.GetCurrentDrift()`

**精度に影響する要因**:
1. システムの負荷
2. オーディオドライバー
3. BPM値（高BPMほど困難）
4. ハードウェア性能

### Q9: 大きな音楽ファイルの処理はどうですか？

**A**: ファイルサイズ制限：

- **推奨サイズ**: 100MB以下
- **最大サイズ**: 500MB（メモリ次第）
- **処理方式**: ストリーミング読み込み

**大ファイルの最適化**:
```csharp
// 波形データのサンプリング間隔を調整
if (fileSize > 100 * 1024 * 1024) // 100MB
{
    samplingInterval *= 2; // 間隔を倍に
}
```

## UI・デザインについて

### Q10: フラッシュパターンの追加方法は？

**A**: 新しいフラッシュパターンの追加手順：

1. **Enumに追加**:
```csharp
public enum FlashPattern
{
    SingleArea,
    FourCircles,
    ProgressiveBar,
    NewPattern // 追加
}
```

2. **実装メソッド追加**:
```csharp
private void ApplyNewPatternFlash(int beatNumber)
{
    // パターンの実装
}
```

3. **UI選択肢追加**:
```xml
<Button Content="新パターン"
        Command="{Binding SelectFlashPatternCommand}"
        CommandParameter="NewPattern"/>
```

### Q11: 色やテーマのカスタマイズは？

**A**: カスタマイズ方法：

**コードレベル**:
```csharp
// BpmFlashController.cs で色を変更
var beatColor = beatNumber == 1 ? Colors.Purple : Colors.Green;
```

**XAMLレベル**:
```xml
<!-- カスタムカラーリソース -->
<SolidColorBrush x:Key="BeatColor1" Color="#FF6B35"/>
<SolidColorBrush x:Key="BeatColor2" Color="#004E89"/>
```

**今後の計画**: 設定ファイルでの色設定対応

### Q12: 全画面モードの実装は？

**A**: 全画面モードの仕組み：

1. **専用ウィンドウ**: `FullscreenFlashWindow`
2. **ViewModel共有**: `MainWindowViewModel`を共有
3. **同期表示**: 同じフラッシュパターンを表示

**カスタマイズポイント**:
- ウィンドウサイズ
- フラッシュ表示領域
- ESCキー処理

## パフォーマンスについて

### Q13: メモリ使用量が多い原因は？

**A**: メモリ使用量の内訳：

| 要素 | 使用量 | 説明 |
|------|--------|------|
| 基本アプリ | ~50MB | .NET Runtime + Avalonia |
| 音声データ | ~100MB | 10分MP3の場合 |
| 波形データ | ~10MB | 表示用データ |
| フラッシュ処理 | ~30MB | UI要素とアニメーション |

**最適化方法**:
```csharp
// 不要なオブジェクトの明示的解放
GC.Collect();
GC.WaitForPendingFinalizers();
```

### Q14: CPUの使用量が高い場合は？

**A**: CPU使用量の最適化：

**主な要因**:
1. 音声処理（30-40%）
2. フラッシュアニメーション（20-30%）
3. 波形描画（10-20%）

**対策**:
```csharp
// フレームレート制限
_updateTimer.Interval = TimeSpan.FromMilliseconds(33); // 30fps
```

### Q15: 同期のずれが発生する場合は？

**A**: 同期ずれの原因と対策：

**一般的な原因**:
1. システムの高負荷
2. 不正確なBPM値
3. 音声レイテンシ

**対策コード**:
```csharp
// ドリフト補正の実装
var drift = GetCurrentDrift();
if (Math.Abs(drift) > 0.010) // 10ms以上
{
    AdjustTiming(drift);
}
```

## 設定・データについて

### Q16: 設定ファイルの場所は？

**A**: 設定ファイルの保存場所：

- **Windows**: `%AppData%\BeatSync\beatsync_settings.json`
- **macOS**: `~/Library/Application Support/BeatSync/beatsync_settings.json`
- **Linux**: `~/.config/BeatSync/beatsync_settings.json`

**手動編集**: 可能（アプリ終了後）

### Q17: 楽曲データベースの制限は？

**A**: データベース仕様：

- **形式**: JSON
- **楽曲数制限**: なし（メモリ次第）
- **推奨上限**: 1,000曲
- **検索**: LinearSearch（O(n)）

**大量データの場合**:
将来的にSQLiteへの移行を検討

### Q18: 設定のバックアップ方法は？

**A**: バックアップ手順：

1. **手動コピー**:
```bash
# 設定フォルダ全体をコピー
cp -r "%AppData%\BeatSync" "backup\BeatSync_backup"
```

2. **プログラマティック**:
```csharp
var backupPath = Path.Combine(Environment.GetFolderPath(
    Environment.SpecialFolder.Desktop), "beatsync_backup.json");
File.Copy(_settingsPath, backupPath);
```

## 開発・デバッグについて

### Q19: デバッグに便利な機能は？

**A**: デバッグ支援機能：

**ログ出力**:
```csharp
#if DEBUG
Console.WriteLine($"BPM: {bpm}, Beat: {beatCount}, Drift: {drift:F3}ms");
#endif
```

**精度測定**:
```csharp
var accuracy = _bpmSyncController.GetCurrentDrift();
_debugInfo = $"Accuracy: ±{Math.Abs(accuracy):F1}ms";
```

**パフォーマンス監視**:
```csharp
var stopwatch = Stopwatch.StartNew();
// 処理実行
stopwatch.Stop();
Debug.WriteLine($"処理時間: {stopwatch.ElapsedMilliseconds}ms");
```

### Q20: テストの書き方は？

**A**: テスト作成のベストプラクティス：

**単体テスト例**:
```csharp
[TestMethod]
public void BpmSyncController_SetValidBpm_ShouldUpdateBpm()
{
    // Arrange
    var controller = new BpmSyncController();
    
    // Act
    controller.BPM = 120;
    
    // Assert
    Assert.AreEqual(120, controller.BPM);
}
```

**モックを使用したテスト**:
```csharp
[TestMethod]
public void MainWindowViewModel_LoadFile_CallsAudioEngine()
{
    // Arrange
    var mockAudioEngine = new Mock<IAudioEngine>();
    var viewModel = new MainWindowViewModel(mockAudioEngine.Object);
    
    // Act
    viewModel.LoadFileAsync("test.mp3").Wait();
    
    // Assert
    mockAudioEngine.Verify(x => x.LoadFile("test.mp3"), Times.Once);
}
```

### Q21: ビルドエラーが発生する場合は？

**A**: よくあるビルドエラーと解決方法：

**NAudio関連エラー**:
```bash
# NuGetパッケージの再インストール
dotnet remove package NAudio
dotnet add package NAudio --version 2.2.1
```

**Avalonia関連エラー**:
```bash
# キャッシュクリア
dotnet clean
rm -rf bin obj
dotnet restore
```

**依存関係エラー**:
```bash
# 全パッケージの更新確認
dotnet list package --outdated
```

## トラブルシューティング

### Q22: 音が出ない場合は？

**A**: 音声出力の確認手順：

1. **システム確認**:
   - Windows: サウンド設定
   - オーディオドライバー

2. **アプリ確認**:
   - 音量設定（0-100%）
   - ファイル形式サポート
   - NAudioの初期化

3. **デバッグ確認**:
```csharp
Console.WriteLine($"Audio device: {WaveOut.DeviceCount}");
Console.WriteLine($"Volume: {_audioEngine.Volume}");
```

### Q23: フラッシュが表示されない場合は？

**A**: フラッシュ表示の確認：

**よくある原因**:
1. BPM同期が停止
2. フラッシュパターンの選択
3. 透明度設定

**デバッグ方法**:
```csharp
// フラッシュイベントの確認
_flashController.FlashTriggered += (pattern, beat) =>
    Console.WriteLine($"Flash: {pattern}, Beat: {beat}");
```

### Q24: アプリがクラッシュする場合は？

**A**: クラッシュの調査方法：

**例外情報の取得**:
```csharp
AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
{
    var ex = e.ExceptionObject as Exception;
    File.WriteAllText("crash.log", ex?.ToString());
};
```

**よくあるクラッシュ原因**:
1. 音声ドライバーの問題
2. メモリ不足
3. ファイルアクセス権限

## 機能拡張について

### Q25: 新しい音声フォーマットの追加方法は？

**A**: 音声フォーマット追加手順：

1. **NAudioサポート確認**
2. **AudioEngineの拡張**:
```csharp
public bool LoadFile(string filePath)
{
    var extension = Path.GetExtension(filePath).ToLower();
    return extension switch
    {
        ".mp3" => LoadMp3(filePath),
        ".wav" => LoadWav(filePath),
        ".ogg" => LoadOgg(filePath), // 新形式
        _ => false
    };
}
```

3. **ファイルフィルター更新**

### Q26: プラグインシステムの追加は可能ですか？

**A**: プラグインシステムの実装案：

**基本アーキテクチャ**:
```csharp
public interface IFlashPattern
{
    string Name { get; }
    void Apply(int beatNumber, IFlashContext context);
}

// プラグイン読み込み
var plugins = Directory.GetFiles("plugins", "*.dll")
    .SelectMany(LoadFlashPatterns);
```

**現在の状況**: 未実装、将来のロードマップに含まれる

---

このFAQは開発者が遭遇する可能性が高い質問と回答をまとめています。新しい質問や解決策があれば、このドキュメントを更新してください。