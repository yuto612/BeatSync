# 🏗️ BeatSync アーキテクチャ仕様書

## 概要

BeatSyncは.NET 8とAvalonia UIを使用したクロスプラットフォーム音楽ビジュアライザーアプリケーションです。音楽のBPM（Beat Per Minute）に同期して視覚的なフラッシュエフェクトを提供し、高精度のビート同期機能を実現しています。

## システム全体アーキテクチャ

### アーキテクチャパターン
- **MVVM（Model-View-ViewModel）**: Avalonia UIでの標準的なパターン
- **レイヤード・アーキテクチャ**: 明確な責任分離
- **コンポーネントベース**: 再利用可能なコンポーネント設計

### 技術スタック
```
プレゼンテーション層: Avalonia UI 11.0 + ReactiveUI
ビジネスロジック層: .NET 8 + C#
データアクセス層: JSON永続化
外部ライブラリ: NAudio (音声処理)
```

## 📁 プロジェクト構造詳細

```
BeatSync/
├── Audio/                     # 🎵 オーディオ処理層
│   ├── AudioEngine.cs        # 音声エンジンコア
│   └── LoopStream.cs         # ループ再生ストリーム
├── Sync/                      # ⏱️ 同期処理層
│   ├── BpmSyncController.cs  # BPM同期制御
│   ├── BpmFlashController.cs # フラッシュパターン制御
│   └── FlashPattern.cs       # パターン定義
├── UI/                        # 🖥️ プレゼンテーション層
│   ├── MainWindow.axaml      # メインビュー
│   ├── MainWindowViewModel.cs # メインビューモデル
│   ├── FullscreenFlashWindow.axaml # 全画面ビュー
│   ├── WaveformControl.axaml # 波形表示コントロール
│   ├── TrackInfoViewModel.cs # 楽曲情報ビューモデル
│   └── Converters/           # データバインディング変換
├── Services/                  # 🔧 サービス層
│   └── SettingsService.cs    # 設定管理サービス
├── Data/                      # 📊 データモデル層
│   └── UserSettings.cs       # ユーザー設定・楽曲データ
└── Tests/                     # 🧪 テスト
    ├── AudioEngineTests.cs
    └── MainWindowViewModelTests.cs
```

## 🔧 主要コンポーネント詳細

### 1. Audio Layer（オーディオ処理層）

#### AudioEngine.cs
**責任**: 音声ファイルの読み込み、再生、制御

**主要機能**:
- 複数音声フォーマットサポート（MP3, WAV, FLAC, M4A, AAC）
- リアルタイム波形データ生成
- 音量制御・ミュート機能
- 再生位置制御

**依存関係**: NAudio.Wave, NAudio.FileFormats

```csharp
public class AudioEngine : IDisposable
{
    private WaveOutEvent? _waveOut;
    private LoopStream? _loopStream;
    
    public event Action<float[]>? WaveformDataAvailable;
    public event Action? PlaybackStopped;
}
```

#### LoopStream.cs
**責任**: 指定時間からのループ再生機能

**主要機能**:
- 開始時間指定（秒単位）
- シームレスループ再生
- 正確な位置制御

### 2. Sync Layer（同期処理層）

#### BpmSyncController.cs
**責任**: 高精度BPM同期タイマー制御

**主要機能**:
- ±10ms精度のビート同期
- ドリフト検出・自動補正
- 4拍子サイクル管理
- 再生開始時間との同期

**設計パターン**: Observer Pattern

```csharp
public class BpmSyncController : IDisposable
{
    public event Action? BeatDetected;
    public event Action<int>? BeatCountChanged;
    
    public int BPM { get; set; }
    public bool IsRunning { get; private set; }
    public int BeatCount { get; private set; }
}
```

#### BpmFlashController.cs
**責任**: フラッシュパターンの実行制御

**主要機能**:
- 複数フラッシュパターン対応
- UI要素への色・透明度適用
- ビートタイミングでの視覚エフェクト

**サポートパターン**:
1. **SingleArea**: 単一エリアフラッシュ
2. **FourCircles**: 4つの円パターン
3. **ProgressiveBar**: プログレッシブバー

### 3. UI Layer（プレゼンテーション層）

#### MainWindowViewModel.cs
**責任**: アプリケーションのメインロジック制御

**主要機能**:
- ファイル読み込み（ドラッグ&ドロップ）
- BPM設定・音量制御
- フラッシュパターン選択
- 楽曲情報管理

**設計パターン**: MVVM + Command Pattern

#### UI レイアウト構造
```
┌─────────────────────────────────────────────────────┐
│                   Header                             │
├─────────────┬─────────────────┬─────────────────────┤
│  Left Panel │   Flash Area    │    Right Panel      │
│             │                 │                     │
│ • File      │ • Visual Flash  │ • Track List        │
│ • Notes     │ • Beat Counter  │ • BPM Control       │
│             │                 │ • Flash Patterns    │
├─────────────┴─────────────────┴─────────────────────┤
│                Waveform Area                        │
├─────────────────────────────────────────────────────┤
│               Control Buttons                       │
├─────────────────────────────────────────────────────┤
│                Status Bar                           │
└─────────────────────────────────────────────────────┘
```

### 4. Services Layer（サービス層）

#### SettingsService.cs
**責任**: ユーザー設定とアプリケーションデータの永続化

**主要機能**:
- JSON形式での設定保存
- 楽曲別BPM設定記録
- ユーザーメモの永続化
- アプリケーション設定管理

**保存場所**: `%AppData%/BeatSync/beatsync_settings.json`

### 5. Data Layer（データモデル層）

#### UserSettings.cs
**責任**: アプリケーションデータの構造定義

**データ構造**:
```json
{
  "userNotes": "string",
  "importedTracks": [
    {
      "id": "guid",
      "fileName": "string",
      "filePath": "string",
      "bpm": 120,
      "notes": "string",
      "addedTime": "datetime",
      "lastUsedTime": "datetime"
    }
  ],
  "lastUpdateTime": "datetime"
}
```

## 🔄 データフロー

### 1. 音楽ファイル読み込みフロー
```
ユーザー操作（ドラッグ&ドロップ）
    ↓
MainWindow.OnFileDrop
    ↓
MainWindowViewModel.LoadFileAsync
    ↓
AudioEngine.LoadFile
    ↓
波形データ生成
    ↓
UI更新（WaveformControl）
```

### 2. BPM同期フラッシュフロー
```
ユーザーBPM設定
    ↓
BpmSyncController.BPM設定
    ↓
高精度タイマー開始
    ↓
BeatDetectedイベント発火
    ↓
BpmFlashController.OnBeatDetected
    ↓
UI要素への視覚エフェクト適用
```

### 3. 楽曲情報保存フロー
```
BPM保存ボタン押下
    ↓
MainWindowViewModel.SaveCurrentBpmCommand
    ↓
TrackInfoの作成・更新
    ↓
SettingsService.AddOrUpdateTrack
    ↓
JSON形式で永続化
    ↓
UI楽曲リスト更新
```

## 🎨 フラッシュパターンアルゴリズム

### 単一エリアパターン
```csharp
private void ApplySingleAreaFlash(int beatNumber)
{
    switch (beatNumber)
    {
        case 1: // 1拍目 - 強調
            SetFlashColor(Colors.Red, 1.0);
            break;
        case 2:
        case 3:
        case 4: // 2-4拍目
            SetFlashColor(Colors.Blue, 0.8);
            break;
    }
}
```

### 4つの円パターン
```csharp
private void ApplyFourCirclesFlash(int beatNumber)
{
    // 全円をリセット
    ResetAllCircles();
    
    // 対応する円を光らせる
    var circle = GetCircle(beatNumber);
    circle.Fill = beatNumber == 1 ? Colors.Red : Colors.Blue;
    circle.GlowRadius = 20;
}
```

## 🔧 設定管理システム

### 設定ファイル構造
**場所**: `%AppData%/BeatSync/`
**ファイル**: `beatsync_settings.json`

### 楽曲情報管理
- **自動保存**: ファイル読み込み時に楽曲エントリ作成
- **BPM記録**: ユーザーが「BPM保存」時に更新
- **使用履歴**: 最終使用時間を自動記録
- **メモ機能**: ユーザーノートの永続化

## 🎯 パフォーマンス最適化

### メモリ管理
- **IDisposableパターン**: リソース確実解放
- **弱参照**: メモリリーク防止
- **音声データストリーミング**: 大ファイル対応

### 同期精度
- **高精度タイマー**: System.Threading.Timer使用
- **ドリフト補正**: 累積誤差自動修正
- **UI同期**: Dispatcher.UIThread使用

### レスポンシブデザイン
- **16:9レイアウト**: 1920x1080最適化
- **スケーラブルUI**: Viewbox使用
- **動的レイアウト**: Grid比率調整

## 🔐 エラーハンドリング戦略

### 階層的エラー処理
1. **UI層**: ユーザーフレンドリーなメッセージ表示
2. **サービス層**: ログ記録とフォールバック処理
3. **データ層**: データ整合性チェック

### 例外処理パターン
```csharp
try
{
    // 危険な処理
}
catch (SpecificException ex)
{
    // 特定例外の処理
    LogError(ex);
    ShowUserMessage("具体的なエラーメッセージ");
}
catch (Exception ex)
{
    // 一般例外の処理
    LogError(ex);
    ShowUserMessage("予期しないエラーが発生しました");
}
```

## 🧪 テスト戦略

### 単体テスト
- **AudioEngine**: 音声処理ロジック
- **BpmSyncController**: 同期精度
- **SettingsService**: データ永続化

### 統合テスト
- **UI操作フロー**: ファイル読み込み→再生→同期
- **データ永続化**: 設定保存→読み込み→検証

### パフォーマンステスト
- **メモリ使用量**: 長時間再生での安定性
- **同期精度**: BPM同期の正確性測定

## 🚀 拡張性設計

### 新しいフラッシュパターン追加
1. `FlashPattern` enumに新パターン追加
2. `BpmFlashController`に処理メソッド追加
3. UIにパターン選択ボタン追加

### 新しい音声フォーマット対応
1. NAudioのサポート確認
2. `AudioEngine.LoadFile`メソッド拡張
3. ファイルフィルター更新

### クロスプラットフォーム拡張
- **macOS**: 特定のUI調整
- **Linux**: ファイルパス処理調整
- **モバイル**: Avalonia.Mobile使用

## 📊 監視・ログ

### ログレベル
- **Error**: アプリケーション異常
- **Warning**: 機能制限や警告
- **Info**: 重要な状態変化
- **Debug**: 開発時詳細情報

### パフォーマンス監視
- **メモリ使用量**: GC圧迫監視
- **CPU使用率**: 重い処理の検出
- **同期精度**: タイミングドリフト測定

---

このアーキテクチャ仕様書は、BeatSyncの技術的な基盤と設計思想を包括的に説明しています。新しい開発者がプロジェクトに参加する際の技術的なガイドとして活用してください。