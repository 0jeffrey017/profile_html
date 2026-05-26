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
        video_enbed_url = None
        if not video_url:
            content = re.sub(r'<!-- VIDEO_START -->.*?<!-- VIDEO_END -->', '', content, flags=re.DOTALL)
        else:
            content = content.replace('<!-- VIDEO_START -->', '')
            content = content.replace('<!-- VIDEO_END -->', '')
            video_enbed_url = get_embed_url(video_url)
            content = content.replace('{{VIDEO_EMBED_URL}}', video_enbed_url)

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

        # Convert to list if it's a single object
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
            # Remove the entire block including markers
            content = re.sub(r'<!-- CREDIT_START -->.*?<!-- CREDIT_END -->', '', content, flags=re.DOTALL)
        else:
            # Keep the block but remove the markers
            content = content.replace('<!-- CREDIT_START -->', '')
            content = content.replace('<!-- CREDIT_END -->', '')

            credit_html = "<ul>\n"
            for cd in credit:
                credit_type = cd.get('type','')
                credit_name = cd.get('name','')
                credit_url = cd.get('url','')
                credit_html += f'    <li>{credit_type} : <a href="{credit_url}">{credit_name}</a></li>\n'
            credit_html += "<ul>\n"
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
        content = content.replace('{{PLATFORM}}', game.get('platform', ''))
        content = content.replace('{{HOW_TO_PLAY}}', game.get('how_to_play', ''))
        content = content.replace('{{PLAYERS}}', game.get('players', ''))
        
        play_link = game.get('play_link', '#')
        content = content.replace('{{PLAY_LINK}}', play_link)
        
        # Disable button if link is empty or just '#'
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

if __name__ == "__main__":
    generate_games()