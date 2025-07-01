# BPM Sync Visualizer - セットアップガイド

## 📋 システム要件

### 必須環境
- **OS**: Windows 10/11, macOS 10.15+, または Linux
- **RAM**: 最低 4GB (推奨: 8GB以上)
- **ストレージ**: 200MB以上の空き容量
- **オーディオ**: サウンドカード必須

### サポートされる音楽ファイル形式
- MP3 (.mp3)
- WAV (.wav) 
- FLAC (.flac)
- M4A (.m4a)
- AAC (.aac)

## 🔧 インストール手順

### 1. .NET 8 SDK のインストール

#### Windows
1. [Microsoft .NET 8 ダウンロードページ](https://dotnet.microsoft.com/download/dotnet/8.0)にアクセス
2. "SDK x64" をダウンロード
3. インストーラーを実行してインストール

#### macOS
```bash
# Homebrew経由でインストール
brew install dotnet

# または公式インストーラーを使用
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Linux (Ubuntu/Debian)
```bash
# Microsoft パッケージリポジトリを追加
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# .NET SDK をインストール
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### 2. Git のインストール（オプション）

#### Windows
- [Git for Windows](https://git-scm.com/download/win) をダウンロードしてインストール

#### macOS
```bash
# Xcodeコマンドラインツールをインストール
xcode-select --install

# またはHomebrew経由
brew install git
```

#### Linux
```bash
# Ubuntu/Debian
sudo apt-get install git

# CentOS/RHEL
sudo yum install git
```

### 3. 追加の依存関係（Linux のみ）

#### Ubuntu/Debian
```bash
# オーディオライブラリとGUIライブラリ
sudo apt-get install libasound2-dev libx11-dev libxrandr-dev libxi-dev
```

#### CentOS/RHEL
```bash
# オーディオライブラリとGUIライブラリ
sudo yum install alsa-lib-devel libX11-devel libXrandr-devel libXi-devel
```

## 🚀 プロジェクトのセットアップ

### 方法1: Gitクローン（推奨）
```bash
# リポジトリをクローン
git clone <repository-url>
cd BPMSyncVisualizer

# 依存関係を復元
dotnet restore

# プロジェクトをビルド
dotnet build

# アプリケーションを実行
dotnet run
```

### 方法2: ZIPダウンロード
1. GitHubからZIPファイルをダウンロード
2. 任意のフォルダに展開
3. コマンドプロンプト/ターミナルで展開したフォルダに移動
4. 以下のコマンドを実行：

```bash
# 依存関係を復元
dotnet restore

# プロジェクトをビルド
dotnet build

# アプリケーションを実行
dotnet run
```

## 🎯 実行可能ファイルの作成

### 単一ファイル実行可能ファイル作成
```bash
# Windows (x64)
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true

# macOS (x64)
dotnet publish -c Release -r osx-x64 --self-contained true -p:PublishSingleFile=true

# macOS (ARM64 - M1/M2 Mac)
dotnet publish -c Release -r osx-arm64 --self-contained true -p:PublishSingleFile=true

# Linux (x64)
dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true
```

実行可能ファイルは `bin/Release/net8.0/{runtime}/publish/` フォルダに生成されます。

## ❗ トラブルシューティング

### よくある問題と解決方法

#### 1. "dotnet コマンドが見つからない"
- .NET SDK が正しくインストールされているか確認
- システムを再起動してPATHを更新
- コマンドプロンプト/ターミナルを管理者権限で実行

#### 2. "AudioEngine が初期化できない" (Windows)
- Windows Media Feature Pack がインストールされているか確認
- オーディオドライバーを最新版に更新

#### 3. "ファイルが読み込めない"
- ファイルが破損していないか確認
- サポートされている形式か確認
- ファイルパスに日本語や特殊文字が含まれていないか確認

#### 4. Linux でオーディオが再生されない
```bash
# PulseAudio の確認
pulseaudio --check -v

# ALSA の確認
aplay -l
```

#### 5. macOS でセキュリティ警告が表示される
- システム設定 > セキュリティとプライバシー で許可
- または以下のコマンドを実行：
```bash
xattr -cr /path/to/BGMSyncVisualizer.app
```

## 🛠️ 開発環境のセットアップ

### Visual Studio Code（推奨）
1. [Visual Studio Code](https://code.visualstudio.com/) をインストール
2. 以下の拡張機能をインストール：
   - C# for Visual Studio Code
   - .NET Install Tool for Extension Authors

### Visual Studio（Windows）
1. [Visual Studio 2022](https://visualstudio.microsoft.com/) をインストール
2. ".NET デスクトップ開発" ワークロードを選択

### JetBrains Rider
1. [JetBrains Rider](https://www.jetbrains.com/rider/) をインストール
2. .NET 8 SDK サポートを有効化

## 📞 サポート

問題が発生した場合：
1. まずこのガイドの トラブルシューティング セクションを確認
2. GitHub Issues でバグレポートを作成
3. 以下の情報を含めてください：
   - OS とバージョン
   - .NET バージョン (`dotnet --version` の出力)
   - エラーメッセージの完全なテキスト
   - 再現手順

## 📚 関連リンク

- [.NET 8 ドキュメント](https://docs.microsoft.com/dotnet/)
- [Avalonia UI ドキュメント](https://docs.avaloniaui.net/)
- [NAudio ライブラリ](https://github.com/naudio/NAudio)