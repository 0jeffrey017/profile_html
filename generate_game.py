import json
import os

def generate_games():
    # Load template
    template_path = 'game_template.html'
    if not os.path.exists(template_path):
        print(f"Error: {template_path} not found.")
        return

    with open(template_path, 'r', encoding='utf-8') as f:
        template_content = f.read()

    # Load data
    data_path = 'games_data.json'
    if not os.path.exists(data_path):
        print(f"Error: {data_path} not found.")
        return

    with open(data_path, 'r', encoding='utf-8') as f:
        games_data = json.load(f)

    # Generate files
    import re
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

        credit = game.get('credit', '')
        if not credit:
            # Remove the entire block including markers
            content = re.sub(r'<!-- CREDIT_START -->.*?<!-- CREDIT_END -->', '', content, flags=re.DOTALL)
        else:
            # Keep the block but remove the markers
            content = content.replace('<!-- CREDIT_START -->', '')
            content = content.replace('<!-- CREDIT_END -->', '')
            content = content.replace('{{CREDIT}}', credit)

        content = content.replace('{{TITLE}}', game.get('title', ''))
        content = content.replace('{{SUBTITLE}}', game.get('subtitle', ''))
        content = content.replace('{{OVERVIEW}}', game.get('overview', ''))
        content = content.replace('{{ENGINE}}', game.get('engine', ''))
        content = content.replace('{{PERIOD}}', game.get('period', ''))
        content = content.replace('{{RESPONSIBLE}}', game.get('responsible', ''))
        content = content.replace('{{TECHNICAL_DETAILS}}', game.get('technical_details', ''))
        content = content.replace('{{IMAGE_PATH}}', game.get('image_path', ''))
        content = content.replace('{{PLATFORM}}', game.get('platform', ''))
        content = content.replace('{{HOW_TO_PLAY}}', game.get('how_to_play', ''))
        content = content.replace('{{PLAYERS}}', game.get('players', ''))
        content = content.replace('{{PLAY_LINK}}', game.get('play_link', '#'))

        output_path = game.get('filename')
        if output_path:
            with open(output_path, 'w', encoding='utf-8') as f:
                f.write(content)
            print(f"Generated: {output_path}")
        else:
            print("Error: 'filename' not specified for a game entry.")

if __name__ == "__main__":
    generate_games()
