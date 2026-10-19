# 見どころの設定

`data/games_data.json` の対象作品に `highlights` を追加します。見出し・説明文・画像・動画をひとまとまりにし、複数の見どころを並べられます。

```json
"highlights": [
  {
    "title": "月を見上げると変化する回廊",
    "text": "マップが変化する様子を画像と動画で紹介します。",
    "media": [
      {
        "type": "image",
        "src": "Image/GameImage/Crescent_Moon_image.webp",
        "alt": "廻り月の作品画像",
        "caption": "月が照らす回廊"
      },
      {
        "type": "video",
        "src": "Image/GameImage/Crescent_Moon_image.mp4",
        "poster": "Image/GameImage/Crescent_Moon_image.webp",
        "caption": "プレイ動画"
      }
    ]
  }
]
```

- `title`：見出し
- `text`：説明文（HTMLタグは使わず、通常の文章で記載）
- `media`：掲載順の画像・動画。文章だけなら省略できます。
- `type`：画像は `image`、動画は `video`
- `src`：リポジトリのルートを基準にしたファイルパス。先頭の `/` や `../` は不要です。
- `alt`：画像の内容を説明する文章。動画では読み上げ用の名前になります。
- `caption`：画像・動画の下に表示する説明（任意）
- `poster`：動画を再生する前の静止画（任意）。省略すると作品の `image_path` を使います。

例は既存の作品画像・プレイ動画を使っています。個別の演出や機能を紹介するときは、対応する素材に差し替えてください。動画はブラウザーで再生できるMP4などを使用します。自動再生はせず、再生ボタンで開始します。

変更後は、プロジェクトのルートで `generate_game.py` をPythonで実行し、`games/*.html` を再生成します。生成されたHTMLを直接編集すると、次回の再生成で上書きされます。

`highlights` がない作品には、既存の `review_focus.detail` を表示します。それもなければ作品の `description` を使います。
