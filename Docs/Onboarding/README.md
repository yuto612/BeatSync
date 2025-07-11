# 🚀 BeatSync オンボーディングガイド

BeatSyncプロジェクトへようこそ！このディレクトリには、新しい開発者がプロジェクトを理解し、効率的に開発を開始するために必要な全ての資料が含まれています。

## 📚 オンボーディング資料一覧

### 🏗️ アーキテクチャ・設計
- **[Architecture.md](./Architecture.md)** - システム全体のアーキテクチャ仕様
- **[ClassDiagram.md](./ClassDiagram.md)** - 詳細なクラス構造とUML図
- **[ActivityDiagram.md](./ActivityDiagram.md)** - 主要なワークフローとアクティビティ図

### 🛠️ 開発環境・規約
- **[DevelopmentSetup.md](./DevelopmentSetup.md)** - 開発環境セットアップガイド
- **[CodingGuidelines.md](./CodingGuidelines.md)** - コーディング規約とベストプラクティス

### 📖 技術リファレンス
- **[APIReference.md](./APIReference.md)** - 全クラス・メソッドの詳細API仕様
- **[FAQ.md](./FAQ.md)** - 開発者向けよくある質問と回答

## 🎯 学習パス

### 初心者向け（1-2日）
1. **[Architecture.md](./Architecture.md)** - 全体像の把握
2. **[DevelopmentSetup.md](./DevelopmentSetup.md)** - 環境構築
3. **メインプロジェクトのビルド・実行**
4. **[FAQ.md](./FAQ.md)** の「開発環境について」セクション

### 中級者向け（3-5日）
1. **[ClassDiagram.md](./ClassDiagram.md)** - 詳細設計の理解
2. **[ActivityDiagram.md](./ActivityDiagram.md)** - 処理フローの把握
3. **[CodingGuidelines.md](./CodingGuidelines.md)** - 規約習得
4. **簡単な機能追加・修正**

### 上級者向け（1週間〜）
1. **[APIReference.md](./APIReference.md)** - 完全なAPI理解
2. **全テストスイートの実行・理解**
3. **新機能設計・実装**
4. **ドキュメント更新・拡張**

## 🚀 クイックスタート（30分で始める）

### 1. 前提条件確認
```bash
# .NET 8 SDK確認
dotnet --version
# 期待値: 8.0.xxx

# Git確認
git --version
```

### 2. プロジェクトクローン
```bash
git clone <repository-url>
cd BPMSyncVisualizer
```

### 3. 依存関係インストール
```bash
dotnet restore
```

### 4. ビルド・実行
```bash
dotnet build
dotnet run
```

### 5. テスト実行
```bash
dotnet test
```

## 💡 重要なコンセプト

### アーキテクチャの核心
- **MVVM パターン**: UI とビジネスロジックの分離
- **ReactiveUI**: リアクティブプログラミング
- **レイヤード設計**: 明確な責任分離

### 技術スタック
- **.NET 8**: 最新のC#機能
- **Avalonia UI**: クロスプラットフォームUI
- **NAudio**: 音声処理
- **JSON**: 設定管理

### 主要機能
- **高精度BPM同期**: ±10ms精度
- **多彩なフラッシュパターン**: 3種類の視覚エフェクト
- **楽曲データベース**: 自動BPM記録
- **ドラッグ&ドロップ**: 直感的ファイル読み込み

## 🎨 プロジェクトの特色

### 音楽×プログラミング
BeatSyncは音楽の**ビート**とプログラムの**同期処理**を組み合わせた独自のプロジェクトです。DJやダンサー、音楽愛好家のための実用的なツールでありながら、技術的にも興味深い課題が多数含まれています。

### 高精度同期への挑戦
- **±10ms精度**の実現
- **ドリフト補正**アルゴリズム
- **マルチスレッド**同期処理

### クロスプラットフォーム対応
- Windows、macOS、Linux対応
- 統一されたユーザー体験
- プラットフォーム固有の最適化

## 🤝 貢献ガイドライン

### 新機能開発の流れ
1. **Issue作成** - 機能要求・バグ報告
2. **設計検討** - アーキテクチャ適合性確認
3. **ブランチ作成** - `feature/機能名` または `fix/バグ名`
4. **開発・テスト** - 規約に従った実装
5. **プルリクエスト** - レビューとマージ

### コード品質基準
- **テストカバレッジ**: 80%以上
- **コーディング規約**: [CodingGuidelines.md](./CodingGuidelines.md) 準拠
- **パフォーマンス**: メモリリークなし
- **ドキュメント**: 新機能は必ずドキュメント更新

## 🔍 トラブルシューティング

### よくある問題
1. **ビルドエラー** → [FAQ.md](./FAQ.md) Q21参照
2. **音声出力問題** → [FAQ.md](./FAQ.md) Q22参照
3. **同期ずれ** → [FAQ.md](./FAQ.md) Q15参照

### サポートチャネル
- **GitHub Issues**: バグ報告・機能要求
- **Discussions**: 質問・議論
- **Wiki**: 詳細な技術情報

## 📝 開発ワークフロー

### 日常的な作業
```bash
# 作業開始
git pull origin main
dotnet restore
dotnet build

# 開発中
dotnet test                    # テスト実行
dotnet format                  # コード整形
git add . && git commit -m ""  # コミット

# 作業終了
git push origin feature/xxx
```

### リリース準備
```bash
# リリースビルド
dotnet build -c Release
dotnet test -c Release
dotnet publish -c Release -r win-x64 --self-contained
```

## 🎯 次のステップ

### 初回開発タスク候補
1. **小さなUI改善** - ボタンやレイアウトの調整
2. **新しいフラッシュパターン** - 視覚エフェクトの追加
3. **設定項目追加** - ユーザー設定の拡張
4. **テスト追加** - カバレッジ向上
5. **ドキュメント改善** - 説明の追加・修正

### 中長期的な機能候補
- **プラグインシステム**
- **音声エフェクト機能**
- **クラウド同期**
- **モバイルアプリ対応**
- **Webアプリ版**

## 📞 連絡先・サポート

### 開発チーム
- **プロジェクトリード**: [GitHub Profile]
- **アーキテクト**: [GitHub Profile]
- **UI/UX**: [GitHub Profile]

### コミュニティ
- **GitHub**: [Repository URL]
- **Discord**: [サーバー招待URL]
- **Twitter**: [@BeatSyncApp]

---

**🎉 BeatSyncプロジェクトへの参加を歓迎します！**

音楽とプログラミングの融合を通じて、創造的で技術的に興味深いソフトウェアを一緒に作り上げましょう。このオンボーディング資料を活用して、効率的にプロジェクトに参加してください。

ご質問やサポートが必要な場合は、遠慮なく[FAQ.md](./FAQ.md)を確認するか、GitHub Issuesでお知らせください。