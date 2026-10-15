import json
import os
import html
import re

SITE_BASE_URL = 'https://0jeffrey017.github.io/profile_html'

def generate_games():
    # ゲーム詳細ページのテンプレートを読み込む。
    template_path = 'game_template.html'
    if not os.path.exists(template_path):
        print(f"Error: {template_path} not found.")
        return

    with open(template_path, 'r', encoding='utf-8') as f:
        template_content = f.read()

    # 各作品の紹介文と設定を読み込む。
    data_path = 'data/games_data.json'
    if not os.path.exists(data_path):
        print(f"Error: {data_path} not found.")
        return

    with open(data_path, 'r', encoding='utf-8') as f:
        games_data = json.load(f)

    # 公開済み作品を JSON の順に並べ、前後の作品へのリンクを作る。
    nav_list = [g for g in games_data if g.get('status') == 'released']
    nav_index = {id(g): i for i, g in enumerate(nav_list)}

    # 作品ごとにテンプレートへデータを埋め込む。
    for game in games_data:
        content = template_content

        # 制作体制・期間・担当の要約も、本文と同じ作品データから作る。
        esc = lambda value: html.escape(str(value), quote=True)
        roles = game.get('responsible', [])
        summary = (
            '<dl class="project-summary">'
            f'<div><dt>制作体制</dt><dd>{esc(game.get("team", ""))}</dd></div>'
            f'<div><dt>制作期間</dt><dd>{esc(game.get("period", ""))}</dd></div>'
            f'<div><dt>私の担当</dt><dd>{esc(" / ".join(roles[:2]))}</dd></div></dl>'
        )
        content = content.replace('{{REVIEW_SUMMARY}}', summary)
        focus = game.get('review_focus', {})
        focus_html = ''
        if focus:
            focus_html = (
                '<section class="review-focus" aria-label="実装の見どころ">'
                '<h2>実装の見どころ</h2>'
                f'<p>{esc(focus.get("detail", ""))}</p>'
                '<a class="text-link" href="#problems" data-tab-target="problems">課題と対応を読む →</a> '
                '<a class="text-link" href="#code" data-tab-target="code">コード抜粋を読む →</a>'
                '</section>'
            )
        content = content.replace('{{REVIEW_FOCUS}}', focus_html)
        preview = game.get('video_preview') or os.path.splitext(game.get('image_path', ''))[0] + '.mp4'
        preview_html = (
            '<div class="game-detail-image">'
            f'<img src="../{esc(game.get("image_path", ""))}" '
            f'alt="{esc(game.get("title", ""))}の作品画像" loading="lazy"></div>'
        )
        if os.path.isfile(preview):
            preview_html = (
                '<figure class="project-preview">'
                f'<video controls playsinline preload="none" poster="../{esc(game.get("image_path", ""))}" '
                f'aria-label="{esc(game.get("title", ""))}のプレイ動画">'
                f'<source src="../{esc(preview)}" type="video/mp4">'
                f'<a href="../{esc(preview)}">プレイ動画を開く</a></video>'
                '<figcaption>プレイ動画 / 再生ボタンで開始</figcaption></figure>'
            )
        content = content.replace('{{LOCAL_PREVIEW}}', preview_html)

        # 最初と最後の作品をつなぎ、前後の作品へ移動できるようにする。
        n = len(nav_list)
        if n > 1 and id(game) in nav_index:
            i = nav_index[id(game)]
            prev_game = nav_list[(i - 1) % n]
            next_game = nav_list[(i + 1) % n]
        else:
            # 一覧にない作品や、作品が1件だけの場合は自身へリンクする。
            prev_game = next_game = game
        content = content.replace('{{PREV_FILENAME}}', prev_game.get('filename', '#'))
        content = content.replace('{{PREV_TITLE}}', html.escape(prev_game.get('title', '')))
        content = content.replace('{{NEXT_FILENAME}}', next_game.get('filename', '#'))
        content = content.replace('{{NEXT_TITLE}}', html.escape(next_game.get('title', '')))
        
        # 技術図がなければ、図の表示領域ごと取り除く。
        tech_image = game.get('game_technical_image', '')
        if not tech_image:
            # 開始・終了マーカーを含む表示領域を取り除く。
            content = re.sub(r'<!-- TECHNICAL_IMAGE_START -->.*?<!-- TECHNICAL_IMAGE_END -->', '', content, flags=re.DOTALL)
        else:
            # 表示領域を残し、生成用のマーカーだけを取り除く。
            content = content.replace('<!-- TECHNICAL_IMAGE_START -->', '')
            content = content.replace('<!-- TECHNICAL_IMAGE_END -->', '')
            content = content.replace('{{GAME_TECHNICAL_IMAGE}}', tech_image)
            
        # 動画の URL がある作品だけ、埋め込み動画を表示する。
        video_url = game.get('video_embed_url', '')
        video_enbed_url = None
        if not video_url:
            content = re.sub(r'<!-- VIDEO_START -->.*?<!-- VIDEO_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- VIDEO_START -->', '')
            content = content.replace('<!-- VIDEO_END -->', '')
            video_enbed_url = get_embed_url(video_url)
            content = content.replace('{{VIDEO_EMBED_URL}}', video_enbed_url)

        # スクリーンショットがある作品だけ、ギャラリーを表示する。
        gallery_items = game.get('screenshot_gallery', [])
        if not gallery_items:
            content = re.sub(r'<!-- GALLERY_START -->.*?<!-- GALLERY_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- GALLERY_START -->', '')
            content = content.replace('<!-- GALLERY_END -->', '')
            gallery_html = ""
            for img_path in gallery_items:
                gallery_html += f'<div class="screenshot-item"><img src="{img_path}" alt="Screenshot" loading="lazy"></div>\n'
            content = content.replace('{{SCREENSHOT_GALLERY}}', gallery_html)

        # コードサンプルを読み込み、紹介文とともに表示する。
        code_config = game.get('code_sample')

        # サンプルが1件の形式でも、リストに揃えて処理する。
        if code_config and isinstance(code_config, dict):
            code_config = [code_config]

        code_samples_html = ""
        if code_config:
            for config in code_config:
                lang_config = config.get('language') or config.get('intro', 'plaintext')
                exp_config = config.get('explanation' , 'plaintext')
                code_data = get_code_sample_data(config.get('path'), lang_config)
                if code_data:
                    lang = code_data.get('language', 'plaintext')
                    raw_code = code_data.get('code', '')
                    safe_code = html.escape(raw_code)

                    file_name = os.path.basename(config.get('path', ''))
                    intro_text = config.get('intro', '')
                    
                    if intro_text:
                        header_text = f"{intro_text}"
                    else:
                        header_text = f"File: {file_name}"

                    code_samples_html += f"""
                    <div class="code-explanation">{exp_config}</div>
                    <div class="code-container">
                        <div class="code-header">{header_text}</div>
                        <pre><code class="language-{lang}">{safe_code}</code></pre>
                    </div>
                    """

        if not code_samples_html:
            content = re.sub(r'<!-- CODE_SAMPLES_START -->.*?<!-- CODE_SAMPLES_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- CODE_SAMPLES_START -->', '')
            content = content.replace('<!-- CODE_SAMPLES_END -->', '')
            content = content.replace('{{CODE_SAMPLES}}', code_samples_html)


        credit = game.get('credit', [])
        if not credit:
            # クレジットがなければ、マーカーを含む表示領域を取り除く。
            content = re.sub(r'<!-- CREDIT_START -->.*?<!-- CREDIT_END -->', '', content, flags=re.DOTALL)
        else:
            # クレジットの表示領域を残し、マーカーだけを取り除く。
            content = content.replace('<!-- CREDIT_START -->', '')
            content = content.replace('<!-- CREDIT_END -->', '')

            credit_html = "<ul>\n"
            for cd in credit:
                credit_type = cd.get('type','')
                credit_name = cd.get('name','')
                credit_url = cd.get('url','')
                credit_html += f'    <li>{credit_type} : <a href="{credit_url}" target="_blank" rel="noopener noreferrer">{credit_name}</a></li>\n'
            credit_html += "</ul>\n"
            content = content.replace('{{CREDIT}}', credit_html)

        responsibles = game.get('responsible', [])

        responsibles_html = "<ul>\n"
        for res in responsibles:
            responsibles_html += f"    <li>{res}</li>\n"
        responsibles_html += "</ul>"
        content = content.replace('{{RESPONSIBLE}}', responsibles_html)

        challengeAndSolutions = game.get('challenges_solutions', [])
        challengeAndSolutions_html = ""
        for cs in challengeAndSolutions:
            challengeAndSolutions_html += f"""
            <div class="challenge-card">

                <div class="challenge-section">
                    <h4>Problem / 問題点</h4>
                    <p>{cs.get('challenge', '')}</p>
                </div>

                <div class="solution-section">
                    <h4>Solution / 解決策</h4>
                    <p>{cs.get('solution', '')}</p>
                </div>

            </div>
            """
        content = content.replace(
            '{{CHALLENGES_SOLUTIONS}}',
            challengeAndSolutions_html
        )
        
        content = content.replace('{{TITLE}}', game.get('title', ''))
        content = content.replace('{{SUBTITLE}}', game.get('subtitle', ''))
        content = content.replace('{{OVERVIEW}}', game.get('overview', ''))
        content = content.replace('{{ENGINE}}', game.get('engine', ''))
        content = content.replace('{{PERIOD}}', game.get('period', ''))
        content = content.replace('{{IMAGE_PATH}}', game.get('image_path', ''))

        # 検索・SNS 向けのページ URL と画像 URL を絶対 URL で設定する。
        page_url = f"{SITE_BASE_URL}/games/{game.get('filename', '')}"
        abs_image = f"{SITE_BASE_URL}/{game.get('image_path', '')}"
        content = content.replace('{{PAGE_URL}}', page_url)
        content = content.replace('{{ABS_IMAGE_PATH}}', abs_image)
        content = content.replace('{{PLATFORM}}', game.get('platform', ''))
        content = content.replace('{{HOW_TO_PLAY}}', game.get('how_to_play', ''))
        content = content.replace('{{PLAYERS}}', game.get('players', ''))
        
        play_link = game.get('play_link', '#')
        play_link_type = game.get('play_link_type', 'play')
        content = content.replace('{{PLAY_LINK_TYPE}}', html.escape(play_link_type, quote=True))
        content = content.replace('{{PLAY_BUTTON_LABEL}}', 'Google Driveからダウンロード' if play_link_type == 'download' else 'Play Game')
        content = content.replace('{{PLAY_LINK}}', play_link)
        manual_link = game.get('manual_link', '')
        manual_button = (
            f'<a class="play-button manual-button" href="../{html.escape(manual_link, quote=True)}" target="_blank" rel="noopener noreferrer">ゲームマニュアル</a>'
            if manual_link else ''
        )
        content = content.replace('                                {{MANUAL_BUTTON}}\n', f'                                {manual_button}\n' if manual_button else '')
        
        # 公開先が未設定の場合は、プレイ・ダウンロードボタンを無効にする。
        if not play_link or play_link == '#':
            content = content.replace('{{PLAY_BUTTON_DISABLED}}', 'disabled')
        else:
            content = content.replace('{{PLAY_BUTTON_DISABLED}}', '')

        output_path = game.get('filename')
        if output_path:
            output_path = os.path.join('games', output_path)
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Generated: {output_path}")
        else:
            print("Error: 'filename' not specified for a game entry.")

def get_code_sample_data(file_path, language="csharp"):
    if not file_path or not os.path.exists(file_path):
        return None

    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            raw_code = f.read()

        return {
            "language": language,
            "code": raw_code
        }

    except Exception as e:
        print(f" {file_path} Error: {str(e)}")
        return None

def get_embed_url(video_url):
    if "youtu.be/" in video_url:
        video_id = video_url.split("/")[-1]
        return f"https://www.youtube.com/embed/{video_id}"
    elif "watch?v=" in video_url:
        video_id = video_url.split("v=")[-1].split("&")[0]
        return f"https://www.youtube.com/embed/{video_id}"
    return video_url

def generate_sitemap():
    data_path = 'data/games_data.json'
    if not os.path.exists(data_path):
        print(f"Error: {data_path} not found.")
        return

    with open(data_path, 'r', encoding='utf-8') as f:
        games_data = json.load(f)

    static_pages = [
        'index.html',
        'games.html',
        'background.html',
        'contact.html',
        'Resume/Resume.html',
    ]

    urls = [f"{SITE_BASE_URL}/{page}" for page in static_pages]
    urls += [
        f"{SITE_BASE_URL}/games/{game.get('filename', '')}"
        for game in games_data if game.get('filename')
    ]

    lines = ['<?xml version="1.0" encoding="UTF-8"?>',
              '<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">']
    lines += [f"  <url><loc>{url}</loc></url>" for url in urls]
    lines.append('</urlset>')

    with open('sitemap.xml', 'w', encoding='utf-8') as f:
        f.write('\n'.join(lines) + '\n')
    print(f"Generated: sitemap.xml ({len(urls)} URLs)")

if __name__ == "__main__":
    generate_games()
    generate_sitemap()
