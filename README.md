# 🎵 BeatSync

**音楽のビートに完璧に同期するビジュアルフラッシュアプリケーション**
.NET 8 と Avalonia UI で構築されたクロスプラットフォーム対応アプリケーションです。

<img width="1913" height="1014" alt="image" src="https://github.com/user-attachments/assets/92c7bf0f-bbc9-40f8-9395-51bb799d5cc8" />


## ✨ 特徴

- 🎯 **高精度BPM同期**: ±10ms精度でビートに完全同期
- 🎨 **多彩なフラッシュパターン**: 単一エリア、4つの円、プログレッシブバー
- 📱 **全画面モード**: 没入感のあるフルスクリーンビジュアライザー
- 🎛️ **直感的な操作**: ドラッグ&ドロップでファイル読み込み
- 🔊 **音量調節**: リアルタイム音量コントロール
- ⏰ **開始時間設定**: 分秒レベルでの再生開始位置指定
- 🔄 **ループ再生**: 設定した開始時間からの繰り返し再生
- 💾 **楽曲記録**: BPM設定を自動保存・読み込み
- 📝 **メモ機能**: 楽曲に関するメモを記録
- 🖥️ **クロスプラットフォーム**: Windows、macOS、Linux対応

## 🎮 フラッシュパターン

### 1. 単一エリア
- 1拍目: 強烈な赤いフラッシュ
- 2-4拍目: 青いフラッシュ
- ビートカウンター表示

### 2. 4つの円
- 各拍に対応する円が順番に光る
- 1拍目は強調表示
- グローエフェクト付き

### 3. プログレッシブバー
- 拍に合わせてバーが伸びる
- 横長レイアウト対応
- 視覚的なビート進行表示

## 🎵 サポート音楽ファイル

- **MP3** (.mp3) - 最も一般的な形式
- **WAV** (.wav) - 高音質・安定性重視
- **FLAC** (.flac) - ロスレス圧縮
- **M4A** (.m4a) - Apple形式
- **AAC** (.aac) - 高効率圧縮

## 📋 システム要件

### 最小要件
- **OS**: Windows 10+, macOS 10.15+, Linux (Ubuntu 18.04+)
- **.NET**: .NET 8 SDK
- **RAM**: 4GB以上
- **ストレージ**: 200MB以上
- **オーディオ**: サウンドカード必須

### 推奨要件
- **RAM**: 8GB以上
- **画面解像度**: 1920x1080以上（16:9最適化）
- **CPU**: デュアルコア 2.0GHz以上

## 🚀 クイックスタート

### 📥 インストール

#### 1. .NET 8 SDK をインストール
```bash
# ダウンロード: https://dotnet.microsoft.com/download/dotnet/8.0
# インストール確認
dotnet --version
```

#### 2. プロジェクトをクローン・実行
```bash
git clone <repository-url>
cd BPMSyncVisualizer
dotnet restore
dotnet build
dotnet run
```

詳細なセットアップ手順は [SETUP_GUIDE.md](SETUP_GUIDE.md) をご覧ください。

## 🎯 使用方法

### 基本的な使い方

1. **🎵 音楽ファイルを読み込み**
   - ドラッグ&ドロップで音楽ファイルを読み込み
   - 対応形式: MP3, WAV, FLAC, M4A, AAC

2. **⚙️ 設定を調整**
   - **BPM**: ±ボタンまたは直接入力 (30-300)
   - **音量**: スライダーで調整 (0-100%)
   - **開始時間**: `mm:ss` 形式で入力 (例: `01:30`)

3. **🎨 フラッシュパターンを選択**
   - 単一エリア: 基本的なフラッシュ
   - 4つの円: 各拍で円が光る
   - プログレッシブバー: バーが順番に光る

4. **▶️ 再生開始**
   - 設定した開始時間からBPM同期が開始
   - フラッシュ同期をON/OFFで切り替え可能

5. **🖥️ 全画面モード**
   - 「全画面フラッシュ」ボタンで没入感のあるビジュアライザー
   - ESCキーまたはクリックで終了

6. **💾 楽曲管理**
   - 「BPM保存」ボタンで現在のBPM設定を記録
   - 保存済み楽曲リストから過去の設定を読み込み
   - メモ欄で楽曲に関する情報を記録

### ⌨️ キーボードショートカット

- **Space**: 再生/停止切り替え
- **ESC**: 全画面モード終了
- **Tab**: コントロール間の移動

## 🏗️ アーキテクチャ

### 📁 プロジェクト構造
```
BPMSyncVisualizer/
├── Audio/                    # オーディオエンジン
│   ├── AudioEngine.cs       # NAudio基盤の音声処理
│   └── LoopStream.cs        # ループ再生機能
├── Sync/                     # BPM同期システム
│   ├── BpmSyncController.cs # 高精度タイマー制御
│   ├── BpmFlashController.cs # フラッシュパターン管理
│   └── FlashPattern.cs      # パターン定義
├── UI/                       # ユーザーインターフェース
│   ├── MainWindow.axaml     # メインウィンドウ
│   ├── MainWindowViewModel.cs # MVVMロジック
│   ├── FullscreenFlashWindow.axaml # 全画面表示
│   ├── WaveformControl.axaml # 波形表示コントロール
│   ├── TrackInfoViewModel.cs # 楽曲情報管理
│   └── Converters/          # データバインディング
├── Services/                 # サービス層
│   └── SettingsService.cs   # 設定・楽曲情報の永続化
├── Data/                     # データモデル
│   └── UserSettings.cs      # ユーザー設定・楽曲情報
├── Tests/                    # テストスイート
└── Docs/                     # ドキュメント
```

### 🔧 主要コンポーネント

#### AudioEngine
- **NAudio**ベースの音声処理エンジン
- 複数音楽フォーマット対応
- リアルタイム波形生成
- 音量調節・ループ再生

#### BpmSyncController
- **±10ms精度**の高精度タイマー
- 自動同期補正機能
- ドリフト検出・修正

#### BpmFlashController
- 4拍子サイクル管理
- 複数フラッシュパターン実行
- UI同期イベント管理

#### SettingsService
- JSON形式での設定永続化
- 楽曲別BPM情報の管理
- ユーザーメモの保存

## 🎛️ 高度な機能

### 🕒 開始時間設定
```
形式: mm:ss (例: 01:30 = 1分30秒)
範囲: 00:00 - 59:59
用途: サビや特定部分からの再生
```

### 🔄 ループ機能
- 設定した開始時間から繰り返し再生
- BPM同期も開始時間から正確に開始
- DJ練習やダンス練習に最適

### 💾 楽曲データベース
- 楽曲ファイルパスとBPM設定の自動関連付け
- 最終使用時間の記録
- メモ機能による楽曲情報管理
- ワンクリックでの過去設定読み込み

### 🎨 カスタマイズ可能な要素
- フラッシュ色（コード内で変更可能）
- BPM範囲（30-300）
- ビート強調パターン
- レスポンシブレイアウト（16:9最適化）

## 🔧 トラブルシューティング

### よくある問題と解決方法

#### 🚫 "dotnet コマンドが見つからない"
```bash
# .NET SDK の再インストール
# https://dotnet.microsoft.com/download/dotnet/8.0
dotnet --version  # 確認
```

#### 🎵 音楽ファイルが読み込めない
- サポート形式を確認 (MP3, WAV, FLAC, M4A, AAC)
- ファイル破損チェック
- パスに特殊文字が含まれていないか確認
- ドラッグ&ドロップを使用

#### 🖥️ 全画面モードで画面が真っ暗
- グラフィックドライバーを最新に更新
- 別のフラッシュパターンを試す
- ウィンドウモードで動作確認

#### ⏱️ BPM同期がずれる
- 正確なBPM値を設定
- 開始時間を曲の拍に合わせて調整
- 音楽ファイルの品質を確認

### 📞 サポート

詳細なトラブルシューティングは [SETUP_GUIDE.md](SETUP_GUIDE.md) をご覧ください。

## 🚀 実行可能ファイルの作成

### Windows用
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### macOS用 (Intel)
```bash
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true
```

### macOS用 (Apple Silicon)
```bash
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true
```

### Linux用
```bash
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

## 🧪 開発・テスト

### テスト実行
```bash
# 単体テスト
dotnet test Tests/BGMSyncVisualizer.Tests.csproj

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"
```

### デバッグビルド
```bash
dotnet build --configuration Debug
dotnet run --configuration Debug
```

## 🤝 貢献

プロジェクトへの貢献を歓迎します！

1. **フォーク** してブランチを作成
2. **機能追加** または **バグ修正**
3. **テスト** を追加・実行
4. **プルリクエスト** を送信

### 開発ガイドライン
- C# コーディング規約に従う
- 新機能にはテストを追加
- ドキュメントを更新
- コミットメッセージは明確に

## 📊 パフォーマンス

### 推奨制限
- **ファイルサイズ**: 100MB以下
- **再生時間**: 10分以下を推奨
- **BPM範囲**: 30-300 BPM
- **同期精度**: ±10ms

### メモリ使用量
- **起動時**: ~50MB
- **10分MP3**: ~150MB
- **フラッシュ動作**: +30MB

## 📚 技術スタック

- **フレームワーク**: .NET 8
- **UI**: Avalonia UI 11.0
- **音声処理**: NAudio 2.2
- **アーキテクチャ**: MVVM + ReactiveUI
- **データ永続化**: JSON
- **テスト**: xUnit
- **パターン**: Observer, Command, Factory

## 📝 ライセンス

**MIT License** - 詳細は [LICENSE](LICENSE) ファイルをご覧ください。

## 🙏 謝辞

- **Avalonia UI**: クロスプラットフォームUIフレームワーク
- **NAudio**: 音声処理ライブラリ
- **ReactiveUI**: リアクティブMVVMフレームワーク
- **DJコミュニティ**: インスピレーションとフィードバック

## 🔗 関連リンク

- [📖 セットアップガイド](SETUP_GUIDE.md)
- [📋 インストール要件](INSTALL_REQUIREMENTS.md)
- [📘 使用ガイド (日本語)](Docs/UsageGuide_ja.md)
- [🎨 フラッシュ色カスタマイズ](Docs/HowToCustomizeFlashColors_ja.md)
- [🔧 .NET 8 ドキュメント](https://docs.microsoft.com/dotnet/)
- [🎨 Avalonia UI ドキュメント](https://docs.avaloniaui.net/)
- [🎵 NAudio GitHub](https://github.com/naudio/NAudio)

---

**🎉 BeatSync で音楽とビジュアルの完璧な同期を体験してください！**
