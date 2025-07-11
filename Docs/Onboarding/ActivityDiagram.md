# 📊 BeatSync アクティビティ図

## メインフロー

### 1. アプリケーション起動フロー

```mermaid
flowchart TD
    A[アプリケーション開始] --> B[App.axaml.cs実行]
    B --> C[MainWindow初期化]
    C --> D[MainWindowViewModel作成]
    D --> E[AudioEngine初期化]
    E --> F[BpmSyncController作成]
    F --> G[BpmFlashController作成]
    G --> H[SettingsService初期化]
    H --> I[保存済み設定読み込み]
    I --> J[UI初期化完了]
    J --> K[ユーザー操作待機]
```

### 2. 音楽ファイル読み込みフロー

```mermaid
flowchart TD
    A[ユーザーがファイルをドラッグ] --> B[OnDragOver イベント]
    B --> C{サポート形式?}
    C -->|Yes| D[ドロップ許可表示]
    C -->|No| E[ドロップ拒否表示]
    D --> F[ユーザーがドロップ]
    F --> G[OnFileDrop イベント]
    G --> H[ファイルパス取得]
    H --> I[MainWindowViewModel.LoadFileAsync呼び出し]
    I --> J[AudioEngine.LoadFile実行]
    J --> K{読み込み成功?}
    K -->|Yes| L[波形データ生成]
    K -->|No| M[エラーメッセージ表示]
    L --> N[WaveformControl更新]
    N --> O[楽曲情報をSettingsServiceに保存]
    O --> P[ステータス更新: ファイル読み込み完了]
    M --> Q[ステータス更新: エラー]
```

### 3. BPM設定と同期開始フロー

```mermaid
flowchart TD
    A[ユーザーBPM入力] --> B[MainWindowViewModel.BPM設定]
    B --> C[BpmSyncController.BPM更新]
    C --> D[ビート間隔計算]
    D --> E[ユーザーが再生ボタン押下]
    E --> F{ファイル読み込み済み?}
    F -->|No| G[エラーメッセージ表示]
    F -->|Yes| H[AudioEngine.Play実行]
    H --> I[BpmSyncController.Start実行]
    I --> J[高精度タイマー開始]
    J --> K[フラッシュ同期開始]
    K --> L[再生状態表示更新]
```

### 4. BPM同期フラッシュフロー

```mermaid
flowchart TD
    A[タイマーイベント発火] --> B[BpmSyncController.OnTimerElapsed]
    B --> C[現在時刻取得]
    C --> D[ビートカウント計算]
    D --> E[ドリフト検出・補正]
    E --> F[BeatDetectedイベント発火]
    F --> G[BpmFlashController.OnBeatDetected]
    G --> H{選択されたパターン?}
    H -->|SingleArea| I[ApplySingleAreaFlash実行]
    H -->|FourCircles| J[ApplyFourCirclesFlash実行]
    H -->|ProgressiveBar| K[ApplyProgressiveBarFlash実行]
    I --> L[UI要素の色・透明度更新]
    J --> L
    K --> L
    L --> M[ビートカウンター表示更新]
    M --> N{再生継続中?}
    N -->|Yes| A
    N -->|No| O[フラッシュ停止]
```

### 5. 楽曲情報保存フロー

```mermaid
flowchart TD
    A[ユーザーがBPM保存ボタン押下] --> B[SaveCurrentBpmCommand実行]
    B --> C{ファイル読み込み済み?}
    C -->|No| D[エラーメッセージ表示]
    C -->|Yes| E[現在のBPM値取得]
    E --> F[現在のファイルパス取得]
    F --> G[ユーザーメモ取得]
    G --> H[TrackInfo作成/更新]
    H --> I[SettingsService.AddOrUpdateTrack呼び出し]
    I --> J[UserSettings更新]
    J --> K[JSON形式でファイル保存]
    K --> L[ImportedTracksコレクション更新]
    L --> M[UI楽曲リスト更新]
    M --> N[成功メッセージ表示]
```

### 6. 保存済み楽曲読み込みフロー

```mermaid
flowchart TD
    A[ユーザーが楽曲リストの読み込みボタン押下] --> B[TrackInfoViewModel.LoadCommand実行]
    B --> C[LoadRequestedイベント発火]
    C --> D[MainWindowViewModel.LoadTrackFromHistory実行]
    D --> E[TrackInfo.FilePathで音楽ファイル読み込み]
    E --> F{ファイル存在?}
    F -->|No| G[エラーメッセージ表示]
    F -->|Yes| H[AudioEngine.LoadFile実行]
    H --> I[保存済みBPM値で上書き]
    I --> J[保存済みメモで上書き]
    J --> K[LastUsedTime更新]
    K --> L[設定ファイル保存]
    L --> M[UI状態更新]
    M --> N[成功メッセージ表示]
```

### 7. 全画面モードフロー

```mermaid
flowchart TD
    A[ユーザーが全画面ボタン押下] --> B[ToggleFullscreenCommand実行]
    B --> C{現在の状態?}
    C -->|ウィンドウモード| D[FullscreenFlashWindow作成]
    C -->|全画面モード| E[全画面ウィンドウ閉じる]
    D --> F[現在のViewModelを共有]
    F --> G[全画面ウィンドウ表示]
    G --> H[フラッシュパターン同期開始]
    H --> I[ESCキー待機]
    I --> J[ユーザーがESC押下 or クリック]
    J --> E
    E --> K[通常ウィンドウに戻る]
    K --> L[全画面モードフラグ更新]
```

## エラーハンドリングフロー

### 1. ファイル読み込みエラー処理

```mermaid
flowchart TD
    A[ファイル読み込み試行] --> B{ファイル存在?}
    B -->|No| C[FileNotFoundエラー]
    B -->|Yes| D{サポート形式?}
    D -->|No| E[UnsupportedFormatエラー]
    D -->|Yes| F{ファイル破損?}
    F -->|Yes| G[CorruptedFileエラー]
    F -->|No| H[読み込み成功]
    
    C --> I[ユーザーフレンドリーメッセージ表示]
    E --> I
    G --> I
    I --> J[ステータスバー更新]
    J --> K[エラーログ記録]
    K --> L[ユーザー操作待機]
```

### 2. 音声再生エラー処理

```mermaid
flowchart TD
    A[音声再生試行] --> B{オーディオデバイス利用可能?}
    B -->|No| C[AudioDeviceエラー]
    B -->|Yes| D{ファイル読み込み済み?}
    D -->|No| E[NoFileLoadedエラー]
    D -->|Yes| F[再生開始]
    F --> G{再生成功?}
    G -->|No| H[PlaybackFailedエラー]
    G -->|Yes| I[再生状態更新]
    
    C --> J[エラーメッセージ表示]
    E --> J
    H --> J
    J --> K[ログ記録]
    K --> L[フォールバック処理]
```

## パフォーマンス最適化フロー

### 1. 波形データ生成最適化

```mermaid
flowchart TD
    A[音楽ファイル読み込み] --> B[ファイルサイズチェック]
    B --> C{100MB以上?}
    C -->|Yes| D[サンプリング間隔増加]
    C -->|No| E[標準サンプリング]
    D --> F[バックグラウンドスレッドで処理]
    E --> F
    F --> G[波形データ生成]
    G --> H[UIスレッドに結果送信]
    H --> I[WaveformControl更新]
    I --> J[メモリ使用量監視]
    J --> K{メモリ制限超過?}
    K -->|Yes| L[ガベージコレクション実行]
    K -->|No| M[処理完了]
    L --> M
```

### 2. 同期精度最適化

```mermaid
flowchart TD
    A[タイマーイベント] --> B[現在時刻取得]
    B --> C[理想ビート時刻計算]
    C --> D[実際時刻との差分算出]
    D --> E{ドリフト検出?}
    E -->|No| F[通常フラッシュ実行]
    E -->|Yes| G[補正値計算]
    G --> H[次回タイマー間隔調整]
    H --> I[補正後フラッシュ実行]
    F --> J[ドリフト履歴記録]
    I --> J
    J --> K[精度統計更新]
    K --> L{精度劣化検出?}
    L -->|Yes| M[警告ログ出力]
    L -->|No| N[正常完了]
    M --> N
```

## 設定管理フロー

### 1. 設定保存フロー

```mermaid
flowchart TD
    A[設定変更イベント] --> B[UserSettings更新]
    B --> C[JSON直列化]
    C --> D[一時ファイル作成]
    D --> E[一時ファイルに書き込み]
    E --> F{書き込み成功?}
    F -->|No| G[エラーログ記録]
    F -->|Yes| H[元ファイルをバックアップ]
    H --> I[一時ファイルを本ファイルに移動]
    I --> J{移動成功?}
    J -->|No| K[バックアップから復元]
    J -->|Yes| L[バックアップ削除]
    G --> M[設定保存失敗処理]
    K --> M
    L --> N[設定保存完了]
```

### 2. 設定読み込みフロー

```mermaid
flowchart TD
    A[アプリケーション起動] --> B[設定ファイル存在チェック]
    B --> C{ファイル存在?}
    C -->|No| D[デフォルト設定作成]
    C -->|Yes| E[ファイル読み込み]
    E --> F{JSON形式正常?}
    F -->|No| G[設定ファイル破損処理]
    F -->|Yes| H[JSON非直列化]
    H --> I{バージョン互換性?}
    I -->|No| J[設定移行処理]
    I -->|Yes| K[設定適用]
    
    D --> K
    G --> L[バックアップから復元試行]
    J --> K
    L --> M{復元成功?}
    M -->|No| D
    M -->|Yes| K
    K --> N[UI設定反映]
    N --> O[設定読み込み完了]
```

## ライフサイクル管理

### 1. アプリケーション終了フロー

```mermaid
flowchart TD
    A[ユーザーがアプリ終了] --> B[MainWindow.OnClosed]
    B --> C[音声再生停止]
    C --> D[BpmSyncController停止]
    D --> E[現在設定の保存]
    E --> F[AudioEngineリソース解放]
    F --> G[タイマーリソース解放]
    G --> H[一時ファイル削除]
    H --> I[ログファイル閉じる]
    I --> J[アプリケーション終了]
```

### 2. メモリ管理フロー

```mermaid
flowchart TD
    A[オブジェクト作成] --> B[IDisposableパターン実装?]
    B -->|Yes| C[Disposeメソッド実装]
    B -->|No| D[ファイナライザー検討]
    C --> E[usingステートメント使用]
    D --> F[WeakReferenceパターン使用]
    E --> G[自動リソース解放]
    F --> G
    G --> H[メモリ使用量監視]
    H --> I{閾値超過?}
    I -->|Yes| J[GC.Collect実行]
    I -->|No| K[正常動作継続]
    J --> K
```

---

これらのアクティビティ図は、BeatSyncアプリケーションの主要な処理フローと意思決定ポイントを視覚的に表現しています。新しい開発者がアプリケーションの動作を理解し、デバッグや機能拡張を行う際の参考として活用してください。