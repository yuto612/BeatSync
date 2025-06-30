# BPM Sync Visualizer

BPM（Beat Per Minute）に同期してUIが点滅するビジュアライザーアプリケーションです。.NET 8とAvalonia UIで構築されたクロスプラットフォーム対応デスクトップアプリケーションです。

## ⚠️ 重要: Windows 10ユーザーの方へ

**必ず最初に [INSTALL_REQUIREMENTS.md](INSTALL_REQUIREMENTS.md) をお読みください。**

Windows 10では MP3再生に追加のコンポーネント（Windows Media Feature Pack）が必要な場合があります。

## 概要

BPM Sync Visualizerは、mp3またはwavファイルをインポートし、波形を表示し、カスタム再生開始ポイントを設定し、BPM値を入力して、ミリ秒レベルの精度でビートに完全に同期したUIカラーフラッシュをトリガーできるアプリケーションです。

## 機能

- **音声インポート**: mp3/wavファイルのドラッグ&ドロップまたはファイル選択
- **波形表示**: インタラクティブなシークバー付きリアルタイム波形表示  
- **カスタム開始ポイント**: クリックで再生開始位置を設定（ビート1）
- **BPM同期**: 音声再生に同期した高精度UIフラッシュ（現在無効化中）
- **クロスプラットフォーム**: Windows、macOS、Linux対応
- **アクセシビリティ**: 色覚サポート対応UIとキーボードナビゲーション

## システム要件

- .NET 8 SDK
- 対応OS: Windows 10+ (Version 1903以降推奨), macOS 10.15+, Linux (Ubuntu 18.04+)
- 音声コーデック: MP3 (要Windows Media Feature Pack), WAV
- RAM: 最低512MB、推奨1GB以上
- **ファイル制限**: 最大100MB、推奨10分以内

## インストール

### 前提条件
1. .NET 8 SDKをhttps://dotnet.microsoft.com/downloadからインストール
2. インストール確認: `dotnet --version`
3. **Windows 10の場合**: [INSTALL_REQUIREMENTS.md](INSTALL_REQUIREMENTS.md)の追加要件を確認

### ソースからビルド
```bash
git clone https://github.com/yourusername/BPMSyncVisualizer.git
cd BPMSyncVisualizer
dotnet restore
dotnet build
```

### アプリケーションの実行
```bash
dotnet run
```

## 使用方法

### ステップバイステップガイド

1. **アプリケーション起動**
   - プロジェクトディレクトリで`dotnet run`を実行
   - メインウィンドウがファイルインポートエリアと共に開きます

2. **音声ファイルのインポート**
   - mp3/wavファイルをドロップエリアにドラッグ&ドロップ、または
   - 「ファイル選択」ボタンをクリックして参照

3. **再生開始ポイントの設定**
   - 波形上の任意の場所をクリックして開始位置を設定
   - オレンジ色のマーカーが選択した開始ポイントを表示
   - 時間表示がmm:ss形式で更新されます

4. **BPMの設定**
   - BPM値を入力（30-300の範囲）
   - 整数値のみサポート（例：120）
   - 無効な値はエラーメッセージを表示

5. **同期再生の開始**
   - 「再生」ボタンをクリック
   - 選択した開始ポイントから音声が開始
   - 注意：UIフラッシュ機能は安定性のため現在無効化されています

6. **再生停止**
   - 「停止」ボタンをクリック
   - 音声が即座に停止
   - UIが通常状態に戻ります

### キーボードショートカット
- `Space`: 再生/停止切り替え
- `Tab`: コントロール間のナビゲーション
- `Enter`: BPM入力の確定

## アーキテクチャ

### コアコンポーネント

- **AudioEngine**: NAudioベースの音声再生と波形生成
- **BpmSyncController**: ビート同期用高精度タイマー（現在無効化）
- **WaveformControl**: インタラクティブ波形表示
- **MainWindow**: MVVMベースのUI連携

### 主要技術機能

- **精密タイミング**: ±10ms精度（BPM同期機能は現在無効化）
- **メモリ効率**: 10分ファイルで300MB未満のRAM使用量
- **ノンブロッキングUI**: すべての音声処理をバックグラウンドスレッドで実行
- **エラーハンドリング**: 破損ファイルやエッジケースの適切な処理

## トラブルシューティング

### よくある問題

**アプリケーションがクラッシュする**
- [INSTALL_REQUIREMENTS.md](INSTALL_REQUIREMENTS.md) の要件を確認
- 管理者権限で実行: `右クリック → 管理者として実行`
- PowerShellでエラーログを確認: `dotnet run`

**MP3ファイルが再生できない**
- Windows Media Feature Packのインストールが必要
- 詳細は [INSTALL_REQUIREMENTS.md](INSTALL_REQUIREMENTS.md) を参照

**音声は再生されるがアプリが落ちる**
- ファイルサイズを確認（100MB以下推奨）
- WAV形式に変換して試行
- 管理者権限で実行

**音声ファイルが読み込めない**
- ファイルが有効なmp3/wav形式であることを確認
- ファイル権限を確認
- 別のファイル場所を試行

### エラーメッセージ

- "BPMは30から300の間で入力してください": BPM値が範囲外
- "MP3 codec not available": Windows Media Feature Packが必要
- "MediaFoundation DLL not found": Windowsアップデートまたは管理者権限が必要
- "File too large": ファイルサイズが100MBを超過

## 開発情報

### プロジェクト構造
```
/BGMSyncVisualizer/
  /Audio/                 # 音声エンジンコンポーネント
    AudioEngine.cs
  /UI/                    # ユーザーインターフェースコンポーネント
    MainWindow.axaml
    MainWindowViewModel.cs
    WaveformControl.axaml
    WaveformControlViewModel.cs
  /BpmSync/              # BPM同期ロジック（現在無効化）
    BpmSyncController.cs
  /Tests/                # ユニットテスト
    AudioEngineTests.cs
    MainWindowViewModelTests.cs
  /Docs/                 # ドキュメント
    INSTALL_REQUIREMENTS.md
```

### テスト実行
```bash
# ユニットテスト実行
dotnet test Tests/BGMSyncVisualizer.Tests.csproj

# リリースバージョンビルド
dotnet build -c Release
```

### 現在の制限事項
- **BPM同期フラッシュ機能**: 安定性のため一時的に無効化
- **位置追跡タイマー**: クラッシュ防止のため無効化
- **推奨ファイル形式**: WAV（最も安定）

## よくある質問

**Q: どの音声形式がサポートされていますか？**
A: 現在MP3とWAVファイルがサポートされています。安定性のためWAVファイルを推奨します。

**Q: カスタムフラッシュ色は使用できますか？**
A: 現在のバージョンでは安定性のためフラッシュ機能を無効化しています。将来のリリースで復活予定です。

**Q: BPM同期の精度はどの程度ですか？**
A: システムは±10ms精度を維持する設計ですが、現在この機能は無効化されています。

**Q: ライブパフォーマンスで使用できますか？**
A: 現在は基本的な音声再生のみサポートしています。BPM同期機能の復活後に推奨されます。

## 貢献

1. リポジトリをフォーク
2. フィーチャーブランチを作成
3. 変更を実施
4. 新機能のテストを追加
5. プルリクエストを送信

## ライセンス

MIT License - 詳細はLICENSEファイルを参照してください。

## 謝辞

- Avalonia UIフレームワークで構築
- NAudioによる音声処理
- DJおよび音楽制作ワークフローからインスパイア

## 既知の問題と解決策

### 現在の安定化措置
アプリケーションの安定性向上のため、以下の機能を一時的に無効化しています：

1. **BPM同期フラッシュ機能** - 復活作業中
2. **リアルタイム位置更新** - クラッシュ防止のため無効化
3. **高頻度タイマー処理** - 安定性優先で無効化

### 基本機能
現在安定して動作する機能：
- ✅ 音声ファイルの読み込み
- ✅ 波形表示
- ✅ 再生開始位置の設定
- ✅ 基本的な音声再生・停止
- ❌ BPM同期フラッシュ（開発中）
- ❌ リアルタイム再生位置表示（開発中）