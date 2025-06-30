# BPM Sync Visualizer - インストール要件

## システム要件

### 必須環境
- **OS**: Windows 10 (Version 1903以降推奨) / Windows 11
- **.NET**: .NET 8.0 Runtime または SDK
- **RAM**: 最低512MB、推奨1GB以上
- **ストレージ**: 100MB以上の空き容量

## 必須コンポーネント

### 1. .NET 8.0 SDK/Runtime
```bash
# インストール確認
dotnet --version

# インストールが必要な場合
# https://dotnet.microsoft.com/download/dotnet/8.0 からダウンロード
```

### 2. Windows Media Feature Pack (Windows 10 N/KN版のみ)
Windows 10 N版またはKN版を使用している場合、MP3再生に必要:

**インストール方法:**
1. 設定 → アプリ → オプション機能
2. 「機能の追加」をクリック
3. 「Windows Media Feature Pack」を検索してインストール

**または:**
- [Microsoft公式ページ](https://support.microsoft.com/ja-jp/topic/windows-10-n-%E3%81%8A%E3%82%88%E3%81%B3-windows-10-kn-%E7%89%88%E7%94%A8%E3%81%AE-media-feature-pack-ca6e2c2c-92c1-3f2c-c9fa-bc1bac94ccec)からダウンロード

### 3. Visual C++ Redistributable (通常は既にインストール済み)
必要に応じて最新版をインストール:
- [Microsoft Visual C++ 2015-2022 Redistributable](https://docs.microsoft.com/ja-jp/cpp/windows/latest-supported-vc-redist)

## オーディオファイル制限

### サポート形式
- **WAV**: 制限なし（推奨形式）
- **MP3**: MP3コーデックが必要

### ファイルサイズ制限
- **最大ファイルサイズ**: 100MB
- **推奨最大長**: 10分
- **推奨サンプルレート**: 44.1kHz または 48kHz
- **推奨ビットレート**: 128kbps以上（MP3の場合）

### 非対応形式
- FLAC, AAC, M4A, OGG, WMA等

## トラブルシューティング

### アプリケーションが起動しない場合

#### 1. 管理者権限で実行
```bash
# PowerShellを管理者として実行
cd "プロジェクトフォルダ"
dotnet run
```

#### 2. 詳細エラー確認
```bash
# エラーログを確認
dotnet run --verbosity diagnostic
```

### MP3ファイルが再生できない場合

#### エラーメッセージ別対処法:

**「MP3 codec not available」**
→ Windows Media Feature Packをインストール

**「MediaFoundation DLL not found」**
→ Windowsアップデートを実行、または管理者権限で実行

**「MediaFoundation access denied」**
→ 管理者権限で実行

**「COM error (likely codec issue)」**
→ K-Lite Codec Packのインストールを検討

### WAVファイルが再生できない場合

**「WaveOut initialization failed」**
→ 他のオーディオアプリケーションを終了して再試行

**「Audio device not available」**
→ オーディオドライバーを更新

## 代替MP3コーデック

Windows Media Feature Packが利用できない場合:

### K-Lite Codec Pack (Basic版)
1. [K-Lite公式サイト](https://codecguide.com/download_kl.htm)からBasic版をダウンロード
2. インストール時に「LAV Filters」を選択
3. アプリケーションを再起動

## 実行手順

### 1. プロジェクトの取得
```bash
git clone [リポジトリURL]
cd BPMSyncVisualizer
```

### 2. 依存関係の復元
```bash
dotnet restore
```

### 3. ビルドと実行
```bash
# ビルド
dotnet build

# 実行
dotnet run
```

### 4. テスト実行（任意）
```bash
dotnet test Tests/BGMSyncVisualizer.Tests.csproj
```

## パフォーマンス最適化

### 推奨設定
- 大きなMP3ファイル（>50MB）はWAVに変換して使用
- 長時間ファイル（>10分）は分割して使用
- 同時に複数のオーディオアプリケーションを起動しない

### リソース使用量
- **メモリ使用量**: ファイルサイズの約2-3倍
- **CPU使用量**: 再生中は5-15%（ファイルサイズ依存）

## サポート

問題が解決しない場合:
1. PowerShellでの詳細エラーログをコピー
2. 使用しているWindowsバージョンを確認
3. 問題のオーディオファイル情報（形式、サイズ、長さ）を記録
4. GitHubのIssuesで報告