# How to add a new game

1. Open `games_data.json`.
2. Copy an existing game entry and paste it at the end of the list.
3. Update the fields for your new game:
   - `filename`: The name of the HTML file (e.g., `Game06.html`).
   - `title`: The title of the game.
   - `subtitle`: Subtitle/Genre.
   - `overview`: HTML string for the description.
   - `engine`: Unity, etc.
   - `period`: Production duration.
   - `responsible`: Your role.
   - `technical_details`: HTML string for list items.
   - `credit`: HTML string for credits.
   - `image_path`: Path to the thumbnail image.
   - `platform`: PC, WebGL, etc.
   - `how_to_play`: Controls.
   - `players`: Number of players.
   - `play_link`: URL to the game.
4. Save the file.
5. Run the following command in your terminal:
   ```bash
   py generate_game.py
   ```
6. (Optional) Update `main.js` if you want the game to appear on the home page grid.
