import json
import os
import html
import re

def generate_games():
    # Load template
    template_path = 'game_template.html'
    if not os.path.exists(template_path):
        print(f"Error: {template_path} not found.")
        return

    with open(template_path, 'r', encoding='utf-8') as f:
        template_content = f.read()

    # Load data
    data_path = 'data/games_data.json'
    if not os.path.exists(data_path):
        print(f"Error: {data_path} not found.")
        return

    with open(data_path, 'r', encoding='utf-8') as f:
        games_data = json.load(f)

    # Generate files
    for game in games_data:
        content = template_content
        
        # Handle Technical Image conditional block
        tech_image = game.get('game_technical_image', '')
        if not tech_image:
            # Remove the entire block including markers
            content = re.sub(r'<!-- TECHNICAL_IMAGE_START -->.*?<!-- TECHNICAL_IMAGE_END -->', '', content, flags=re.DOTALL)
        else:
            # Keep the block but remove the markers
            content = content.replace('<!-- TECHNICAL_IMAGE_START -->', '')
            content = content.replace('<!-- TECHNICAL_IMAGE_END -->', '')
            content = content.replace('{{GAME_TECHNICAL_IMAGE}}', tech_image)
            
        # Handle Video conditional block
        video_url = game.get('video_embed_url', '')
        if not video_url:
            content = re.sub(r'<!-- VIDEO_START -->.*?<!-- VIDEO_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- VIDEO_START -->', '')
            content = content.replace('<!-- VIDEO_END -->', '')
            content = content.replace('{{VIDEO_EMBED_URL}}', video_url)

        # Handle Gallery conditional block
        gallery_items = game.get('screenshot_gallery', [])
        if not gallery_items:
            content = re.sub(r'<!-- GALLERY_START -->.*?<!-- GALLERY_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- GALLERY_START -->', '')
            content = content.replace('<!-- GALLERY_END -->', '')
            gallery_html = ""
            for img_path in gallery_items:
                gallery_html += f'<div class="screenshot-item"><img src="{img_path}" alt="Screenshot"></div>\n'
            content = content.replace('{{SCREENSHOT_GALLERY}}', gallery_html)

        # Handle Code Sample conditional block
        code_config = game.get('code_sample')
        code_data = None
        if code_config:
            code_data = get_code_sample_data(code_config.get('path'), code_config.get('language'))

        if not code_data:
            content = re.sub(r'<!-- CODE_SAMPLE_START -->.*?<!-- CODE_SAMPLE_END -->', '', content, flags=re.DOTALL)
        else:
            lang = code_data.get('language', 'plaintext')
            raw_code = code_data.get('code', '')
            
            safe_code = html.escape(raw_code)
            
            content = content.replace('<!-- CODE_SAMPLE_START -->', '')
            content = content.replace('<!-- CODE_SAMPLE_END -->', '')

            content = content.replace('{{CODE_LANG}}', lang)
            content = content.replace('{{CODE_SAMPLE}}', safe_code)

        credit = game.get('credit', '')
        if not credit:
            # Remove the entire block including markers
            content = re.sub(r'<!-- CREDIT_START -->.*?<!-- CREDIT_END -->', '', content, flags=re.DOTALL)
        else:
            # Keep the block but remove the markers
            content = content.replace('<!-- CREDIT_START -->', '')
            content = content.replace('<!-- CREDIT_END -->', '')
            content = content.replace('{{CREDIT}}', credit)

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
        content = content.replace('{{PLATFORM}}', game.get('platform', ''))
        content = content.replace('{{HOW_TO_PLAY}}', game.get('how_to_play', ''))
        content = content.replace('{{PLAYERS}}', game.get('players', ''))
        content = content.replace('{{PLAY_LINK}}', game.get('play_link', '#'))

        output_path = game.get('filename')
        if output_path:
            output_path = os.path.join('games', output_path)
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Generated: {output_path}")
        else:
            print("Error: 'filename' not specified for a game entry.")

def get_code_sample_data(file_path, language="csharp"):
    """
    讀取程式碼檔案並傳回字典格式
    """
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
        print(f"處理檔案 {file_path} 時發生錯誤: {str(e)}")
        return None

if __name__ == "__main__":
    generate_games()
