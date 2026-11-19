# Searchable
[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](https://opensource.org/licenses/MIT)
[![.NET: 10.0](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![YMM4](https://img.shields.io/badge/YMM4-v4.47%2B-green.svg)](https://manjubox.net/ymm4/)
[![Downloads](https://img.shields.io/github/downloads/leftcontroller0518/Searchable/total)](https://github.com/leftcontroller0518/Searchable/releases/latest)

**Searchable** は、ゆっくりMovieMaker4 (YMM4) のための高速・高機能な統合検索・コマンドパレットプラグインです。  
VS Code や Raycast のように、**`Ctrl + Shift + F`** を押すだけで画面中央にミニマルな検索バーがポップアップ。  
ウィンドウの呼び出し、アイテムの追加、映像・音声エフェクトの即時適用、キャラクター切り替え、各種操作コマンドをキーボードだけで瞬時に実行できます。

---

## 主な機能

### 1. `Ctrl + Shift + F` で瞬時に呼び出し
- YMM4のどの画面にいても、ショートカットキーで画面中央にスマートにポップアップします。
- メニューバーの「ツール」→「Searchable」からも起動可能です。
- 実行後やパレット外のクリック、`Esc` キーで瞬時に自動クローズします。

### 2. なんでも検索・即時実行
YMM4内のあらゆるアセットや操作を一元的に検索できます。

- **映像・音声エフェクト**
  - 全組み込みエフェクトおよび外部プラグインエフェクト（130種以上）を横断検索。<br>選択中のタイムラインアイテムへ**ワンキーで直接適用**（未選択時は自動でエフェクトアイテムを追加）。 
- **タイムラインアイテム追加**
  - テキスト、図形、エフェクトアイテム、場面切り替え、画面複製（フレームバッファ）、グループ制御、シーン、ボイスなどの新規配置。
- **ウィンドウ・ツールパネル**
  - キャラクター編集、台本編集、素材一覧、辞書、動画設定、環境設定などの主要画面や、各種ツールプラグイン（エフェクト一覧、PSDマネージャー等）の表示・フォーカス。
- **キャラクター切替**
  - プロジェクトに登録されているキャラクターを検索し、即座にアクティブ切り替え。
- **テンプレート適用**
  - 登録済みのアイテム・エフェクトテンプレートをタイムラインに展開。
- **編集操作コマンド**
  - 再生/停止、元に戻す/やり直し、アイテム分割、コピー/貼り付け/削除などのショートカット操作。

### 3. インクリメンタル＆ローマ字あいまい検索
- 入力中の文字列に応じてリアルタイムにスコアリング。
- 日本語入力中だけでなく、**ローマ字読み**（`kurippingu` / `text` / `daihon` / `kyara` 等）や英語キーワードにも対応しています。

### 4. キーボードファーストな操作性
- `↑` / `↓`: 検索候補の移動
- `Enter`: 選択中のアクションを実行して閉じる
- `Tab`: 検索スコープ（すべて / ウィンドウ / エフェクト / アイテム等）の切り替え
- `Esc`: パレットを閉じる

---

## インストール方法
1. [Releases](https://github.com/leftcontroller0518/Searchable/releases) より最新バージョンの `.ymme` ファイルをダウンロードします。
2. ダウンロードした `.ymme` ファイルをダブルクリックして実行するか、起動中の YMM4 ウィンドウにドラッグ＆ドロップしてインストールします。
3. YMM4 を再起動します。

---

## ショートカットキーの変更

デフォルトでは **`Ctrl + Shift + F`** に割り当てられています。  
キー割り当てを変更したい場合は、YMM4のメニューから：
1. **「ファイル」** → **「設定」**を開きます。
2. 左メニューの **「ショートカットキー」** を選択します。
3. コマンド一覧から **「Searchable検索バーを開く」** を探し、お好みのキーバインド（例: `Ctrl + P` や `Alt + Space` など）に変更してください。


## プロジェクト構成

```kousei
Searchable/
├── Directory.Build.props          # YMM4 インストールパス定義
├── Searchable.csproj              # プラグインプロジェクト設定 (.NET 10-windows)
├── SearchableCommandPlugin.cs     # ICommandPlugin 実装 (Ctrl+Shift+F ショートカット登録)
├── SearchableToolPlugin.cs        # IToolPlugin 実装 (YMM4ツールメニュー登録)
├── SearchableToolView.xaml(.cs)   # ツールパネル用ビュー
├── SearchWindow.xaml(.cs)         # コマンドパレットUI (WPFポップアップウィンドウ)
├── SearchableViewModel.cs         # リアルタイム検索・フィルタ・スコアリングロジック
├── SearchResultItem.cs            # 検索結果データモデル・ベクターアイコン定義
├── ItemCatalog.cs                 # YMM4内アセット・コマンド・エフェクト収集エンジン
├── Ymm4HostBridge.cs              # YMM4内部API連携・リフレクションブリッジ
└── README.md
```

---

## ライセンス

本プラグインは [MIT License](LICENSE) の下で公開されています。商用・非商用問わず、ライセンスの範囲内で自由にご利用いただけます。

---

## バグ報告・ご要望
不具合や動作不良、機能追加のご要望がございましたら、GitHub の [Issues](https://github.com/leftcontroller0518/Searchable/issues) または開発者の連絡先までお気軽にお寄せください。
