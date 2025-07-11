# 🛠️ BeatSync 開発環境セットアップガイド

## 前提条件

### システム要件

#### 最小要件
- **OS**: Windows 10+ / macOS 10.15+ / Linux (Ubuntu 18.04+)
- **.NET**: .NET 8 SDK
- **IDE**: Visual Studio 2022 / Visual Studio Code / JetBrains Rider
- **RAM**: 8GB以上推奨
- **ストレージ**: 2GB以上の空き容量

#### 推奨環境
- **OS**: Windows 11 / macOS 12+ / Ubuntu 20.04+
- **RAM**: 16GB以上
- **CPU**: 4コア以上
- **解像度**: 1920x1080以上

## セットアップ手順

### 1. .NET 8 SDK インストール

#### Windows
```powershell
# Wingetを使用
winget install Microsoft.DotNet.SDK.8

# または公式インストーラーをダウンロード
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### macOS
```bash
# Homebrewを使用
brew install dotnet@8

# または公式インストーラーをダウンロード
# https://dotnet.microsoft.com/download/dotnet/8.0
```

#### Linux (Ubuntu/Debian)
```bash
# Microsoft のパッケージリポジトリを追加
wget https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb

# .NET 8 SDKをインストール
sudo apt update
sudo apt install -y dotnet-sdk-8.0
```

#### インストール確認
```bash
# バージョン確認
dotnet --version
# 期待される出力: 8.0.xxx

# SDKの確認
dotnet --list-sdks
# 期待される出力: 8.0.xxx [インストールパス]
```

### 2. IDE/エディターセットアップ

#### Visual Studio 2022 (推奨: Windows)

**インストール必要コンポーネント**:
- .NET デスクトップ開発
- ASP.NET と Web 開発
- .NET 8 ターゲット フレームワーク

**拡張機能（推奨）**:
- ReSharper (有料)
- GitLens
- Avalonia for Visual Studio

#### Visual Studio Code (クロスプラットフォーム)

**必須拡張機能**:
```bash
# C# 拡張機能
code --install-extension ms-dotnettools.csharp

# Avalonia拡張機能
code --install-extension AvaloniaTeam.vscode-avalonia

# C# Dev Kit
code --install-extension ms-dotnettools.csdevkit
```

**推奨拡張機能**:
```bash
# GitLens
code --install-extension eamodio.gitlens

# Error Lens
code --install-extension usernamehw.errorlens

# NuGet Gallery
code --install-extension patcx.vscode-nuget-gallery

# XML Tools
code --install-extension DotJoshJohnson.xml
```

#### JetBrains Rider

**プラグイン（推奨）**:
- Avalonia
- GitToolBox
- Rainbow Brackets
- .ignore

### 3. プロジェクトクローンとセットアップ

```bash
# リポジトリをクローン
git clone <repository-url>
cd BPMSyncVisualizer

# 依存関係の復元
dotnet restore

# プロジェクトのビルド
dotnet build

# ビルド成功の確認
echo "ビルドが成功した場合、bin/Debug/net8.0/ にファイルが生成されます"
```

### 4. 開発依存関係の確認

#### NuGet パッケージ
```xml
<!-- BGMSyncVisualizer.csproj から抜粋 -->
<PackageReference Include="Avalonia" Version="11.0.10" />
<PackageReference Include="Avalonia.Desktop" Version="11.0.10" />
<PackageReference Include="Avalonia.Themes.Fluent" Version="11.0.10" />
<PackageReference Include="Avalonia.Fonts.Inter" Version="11.0.10" />
<PackageReference Include="Avalonia.ReactiveUI" Version="11.0.10" />
<PackageReference Include="NAudio" Version="2.2.1" />
<PackageReference Include="NAudio.Winmm" Version="2.2.1" />
```

#### 依存関係の詳細確認
```bash
# パッケージ依存関係ツリー表示
dotnet list package --include-transitive

# 古いパッケージの確認
dotnet list package --outdated
```

### 5. デバッグ設定

#### Visual Studio Code launch.json
```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": "Launch BeatSync",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/bin/Debug/net8.0/BGMSyncVisualizer.dll",
            "args": [],
            "cwd": "${workspaceFolder}",
            "console": "internalConsole",
            "stopAtEntry": false
        },
        {
            "name": "Attach to BeatSync",
            "type": "coreclr",
            "request": "attach"
        }
    ]
}
```

#### Visual Studio Code tasks.json
```json
{
    "version": "2.0.0",
    "tasks": [
        {
            "label": "build",
            "command": "dotnet",
            "type": "process",
            "args": [
                "build",
                "${workspaceFolder}/BGMSyncVisualizer.csproj",
                "/property:GenerateFullPaths=true",
                "/consoleloggerparameters:NoSummary"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "clean",
            "command": "dotnet",
            "type": "process",
            "args": [
                "clean",
                "${workspaceFolder}/BGMSyncVisualizer.csproj"
            ],
            "problemMatcher": "$msCompile"
        },
        {
            "label": "test",
            "command": "dotnet",
            "type": "process",
            "args": [
                "test",
                "${workspaceFolder}/Tests/BGMSyncVisualizer.Tests.csproj"
            ],
            "problemMatcher": "$msCompile"
        }
    ]
}
```

### 6. テスト環境セットアップ

#### テスト実行
```bash
# 全テスト実行
dotnet test

# 特定のテストクラス実行
dotnet test --filter ClassName=AudioEngineTests

# カバレッジレポート生成
dotnet test --collect:"XPlat Code Coverage"

# カバレッジレポートの表示（要: reportgenerator）
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults/*/coverage.cobertura.xml" -targetdir:"CoverageReport" -reporttypes:Html
```

#### テストデータ準備
```bash
# テスト用音楽ファイルフォルダ作成
mkdir TestData/AudioFiles

# サンプル音楽ファイルを配置
# - TestData/AudioFiles/sample.mp3
# - TestData/AudioFiles/sample.wav
# - TestData/AudioFiles/sample.flac
```

### 7. Git設定

#### .gitignore確認
```gitignore
# .NETで生成されるファイル
bin/
obj/
*.dll
*.pdb

# Visual Studio
.vs/
*.user
*.suo

# JetBrains Rider
.idea/

# VS Code
.vscode/settings.json

# OS固有
.DS_Store
Thumbs.db

# 設定ファイル（機密情報）
**/appsettings.local.json
**/secrets.json
```

#### Git Hooks設定
```bash
# pre-commit フック（コード品質チェック）
echo '#!/bin/sh
dotnet format --verify-no-changes
if [ $? -ne 0 ]; then
    echo "Code formatting issues found. Run 'dotnet format' to fix."
    exit 1
fi

dotnet build
if [ $? -ne 0 ]; then
    echo "Build failed."
    exit 1
fi
' > .git/hooks/pre-commit

chmod +x .git/hooks/pre-commit
```

### 8. 開発ツール設定

#### EditorConfig
```ini
# .editorconfig
root = true

[*]
charset = utf-8
end_of_line = crlf
insert_final_newline = true
indent_style = space
indent_size = 4
trim_trailing_whitespace = true

[*.{yml,yaml}]
indent_size = 2

[*.json]
indent_size = 2

[*.xml]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
```

#### dotnet format設定
```xml
<!-- .editorconfig に追加 -->
[*.cs]
dotnet_sort_system_directives_first = true
dotnet_separate_import_directive_groups = false
csharp_new_line_before_open_brace = all
csharp_new_line_before_else = true
csharp_new_line_before_catch = true
csharp_new_line_before_finally = true
```

### 9. パフォーマンス監視ツール

#### JetBrains dotMemory Unit（メモリリーク検出）
```bash
# NuGetパッケージ追加
dotnet add Tests/BGMSyncVisualizer.Tests.csproj package JetBrains.dotMemoryUnit
```

#### Application Insights（本番監視）
```bash
# 監視用パッケージ（オプション）
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

### 10. ローカル開発サーバー設定

#### 設定ファイル作成
```json
// appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft": "Information"
    }
  },
  "BeatSync": {
    "EnableDebugMode": true,
    "LogBpmAccuracy": true,
    "MaxFileSize": 104857600
  }
}
```

#### 環境変数設定
```bash
# Windows (PowerShell)
$env:DOTNET_ENVIRONMENT="Development"
$env:BEATSYNC_DEBUG="true"

# macOS/Linux (Bash)
export DOTNET_ENVIRONMENT="Development"
export BEATSYNC_DEBUG="true"
```

## 開発ワークフロー

### 1. 日常的な開発作業

```bash
# 毎日の開始時
git pull origin main
dotnet restore
dotnet build

# 機能開発時
git checkout -b feature/new-feature-name
# 開発作業...
dotnet test
git add .
git commit -m "feat: 新機能の説明"
git push origin feature/new-feature-name
```

### 2. デバッグのベストプラクティス

#### ログ設定
```csharp
// Program.cs または適切な場所で
#if DEBUG
Console.WriteLine("デバッグモードで実行中");
#endif
```

#### ブレークポイント設定推奨箇所
- `AudioEngine.LoadFile()` - ファイル読み込み時
- `BpmSyncController.OnTimerElapsed()` - 同期タイミング
- `MainWindowViewModel.LoadFileAsync()` - UI処理

### 3. パフォーマンステスト

```bash
# Release ビルドでのパフォーマンステスト
dotnet build -c Release
dotnet run -c Release

# メモリ使用量監視
dotnet-counters monitor --process-id <PID> --counters System.Runtime
```

## トラブルシューティング

### よくある問題と解決方法

#### 1. Avaloniaデザイナーが動作しない
```bash
# VS Code の場合
# - Avalonia拡張機能を再インストール
# - settings.jsonでavalon previewerを有効化
```

#### 2. NAudioでサポートされていない音楽形式
```csharp
// 追加コーデックのインストールが必要な場合
// MediaFoundationApi.Startup(); // Windows のみ
```

#### 3. ビルド時の権限エラー
```bash
# ファイルロック問題の解決
dotnet clean
taskkill /f /im dotnet.exe  # Windows
# または
pkill dotnet  # macOS/Linux
```

#### 4. テスト実行時のファイルパス問題
```csharp
// テスト内で相対パスの代わりに絶対パスを使用
var testDataPath = Path.Combine(Directory.GetCurrentDirectory(), "TestData");
```

### デバッグログ設定

```csharp
// MainWindowViewModel.cs に追加
#if DEBUG
private void LogDebugInfo(string message)
{
    Console.WriteLine($"[DEBUG] {DateTime.Now:HH:mm:ss.fff} - {message}");
}
#endif
```

---

この開発環境セットアップガイドに従って環境を構築すれば、BeatSyncの開発を効率的に始めることができます。問題が発生した場合は、トラブルシューティングセクションを参照してください。