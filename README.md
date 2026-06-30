# Jeffrey LEE | ゲームプログラマー ポートフォリオ

Unity・C# を中心としたゲームプログラマー、リイショウケツ（Jeffrey LEE）のポートフォリオサイトです。
ビルド不要の静的 HTML/CSS/JS で構成され、GitHub Pages で公開しています。

🔗 https://0jeffrey017.github.io/profile_html/

## ページ構成

各ページは1画面（スクロールなし）に収まるレイアウトを基本とし、共通ヘッダーナビ（モバイルではハンバーガーメニュー）で遷移します。

| ページ | 内容 |
|---|---|
| [index.html](index.html) | About / 自己紹介・自己PR・The Future / 今後の展望 |
| [games.html](games.html) | Game Projects / 制作実績（slick-carousel によるカルーセル表示） |
| [background.html](background.html) | Background / 経歴・Qualifications / 資格 |
| [contact.html](contact.html) | Contact / 連絡先（メール・GitHub・LinkedIn・YouTube・履歴書） |
| [Resume/Resume.html](Resume/Resume.html) | Resume / 履歴書（PDF埋め込み・ダウンロード） |
| `games/*.html` | ゲーム個別詳細ページ（生成物、直接編集しない） |

## ゲーム詳細ページ（生成パイプライン）

`games/` 配下の各ページは手書きではなく、テンプレートとデータから自動生成しています。

```
data/games_data.json  →  game_template.html  →  python generate_game.py  →  games/*.html + sitemap.xml
```

- **[data/games_data.json](data/games_data.json)** — 全ゲームの情報（タイトル・期間・担当・課題と解決策・タグ・コードサンプル参照パス等）を持つ唯一のソース
- **[game_template.html](game_template.html)** — `{{PLACEHOLDER}}` とコメントマーカー（`<!-- XXX_START/END -->`）による条件付きブロックを持つテンプレート。タブ構成は 概要 / 問題解決 / 技術図（任意）/ コード
- **[generate_game.py](generate_game.py)** — `games_data.json` を読み込みテンプレートへ流し込んで `games/*.html` を出力。あわせて `sitemap.xml` も自動生成（手動メンテによる乖離を防止）。前後のゲームへのナビゲーション（Prev/Next）も自動付与

ゲーム情報やテンプレートを変更した場合は、必ず以下を再実行してください。

```bash
python generate_game.py
```

`games/*.html` を直接編集しても、次回の生成で上書きされるため変更は反映されません。

## ディレクトリ構成

```
.
├── index.html / games.html / background.html / contact.html   # メインページ
├── Resume/Resume.html                                          # 履歴書ページ
├── games/                                                      # 生成されたゲーム詳細ページ
├── game_template.html                                          # ゲーム詳細ページのテンプレート
├── generate_game.py                                            # ゲーム詳細ページ + sitemap.xml の生成スクリプト
├── data/games_data.json                                        # ゲーム実績データ（唯一のソース）
├── css/                                                        # base / layout / home / pages / game-detail / game-tabs / code-sample / Resume
├── js/                                                          # game.js（背景アニメ・モバイルナビ等）/ games-carousel.js（カルーセル）/ game-tabs.js（タブ切替）
├── Scripts/                                                     # 各ゲームのサンプルコード（コード詳細タブの参照元）
├── Image/                                                       # サムネイル・プロフィール写真・各種ブランドアイコン
├── assets/Resume_Jeffrey.pdf                                    # 履歴書PDF
└── sitemap.xml / robots.txt                                     # SEO（sitemap.xml は generate_game.py の生成物）
```

## ローカルでの確認

ビルド不要ですが、`fetch` で JSON を読み込む都合上 `file://` では動作しないため、簡易サーバー経由で開いてください。

```bash
python -m http.server 8000
# http://localhost:8000/index.html
```

## 技術スタック

- HTML / CSS / Vanilla JS（フレームワークなし）
- [slick-carousel](https://kenwheeler.github.io/slick/) — ゲーム制作実績カルーセル
- [highlight.js](https://highlightjs.org/) — サンプルコードのシンタックスハイライト
- Google Fonts（Poppins / Inter）, Material Symbols
- schema.org の JSON-LD 構造化データ（`index.html`）

## デプロイ

`main` ブランチへの反映が GitHub Pages にそのまま公開されます（ビルドステップなし）。
