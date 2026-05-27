// main.js - Dynamic Game Gallery Configuration

(function($) {
    "use strict";

    let allGames = [];
    let isNewestFirst = true;

    async function initGallery() {
        console.log("main.js: Initializing game grid...");
        
        try {
            // Fetch data from JSON source
            const response = await fetch('data/games_data.json');
            if (!response.ok) throw new Error(`HTTP error! status: ${response.status}`);
            allGames = await response.json();
            
            renderGameGrid();
            setupEventListeners();
        } catch (error) {
            console.error("main.js: Failed to load game data:", error);
            $('.game-grid').html('<p class="error-message">Failed to load projects. Please try again later.</p>');
        }
    }

    function renderGameGrid() {
        const $grid = $('.game-grid');
        if (!$grid.length) return;

        $grid.empty();

        // Filter for released games (optional, depends on your preference)
        const displayGames = allGames.filter(game => game.status === "released");
        
        // Sort based on chronological mapping (Game01 is newest, Game10 is oldest)
        displayGames.sort((a, b) => {
            const idA = parseInt(a.id.replace('game', ''));
            const idB = parseInt(b.id.replace('game', ''));
            return isNewestFirst ? idA - idB : idB - idA;
        });

        displayGames.forEach(game => {
            const gameCard = `
                <div class="game-card" data-id="${game.id}">
                    <a href="games/${game.filename}" class="game-link">
                        <div class="game-image-wrapper">
                            <img src="${game.image_path}" alt="${game.title}" onerror="this.src='https://via.placeholder.com/400x300?text=Image+Not+Found'">
                        </div>
                        <div class="game-info">
                            <span class="game-time">${game.period}</span>
                            <h3 class="game-name">${game.title}</h3>
                            <p class="game-desc">${game.description}</p>
                        </div>
                    </a>
                </div>
            `;
            $grid.append(gameCard);
        });

        // Add "Coming Soon" card at the end if sorting newest first
        if (isNewestFirst) {
            $grid.append(`
                <div class="game-card coming-soon">
                    <div class="game-image-wrapper">
                        <img src="Image/GameCode.png" alt="Coming Soon">
                    </div>
                    <div class="game-info">
                        <span class="game-time">Future</span>
                        <h3 class="game-name">未公開タイトル</h3>
                        <p class="game-desc">開発中の新しいプロジェクトです。</p>
                    </div>
                </div>
            `);
        }
        
        console.log("main.js: Grid rendered (NewestFirst: " + isNewestFirst + ")");
    }

    function setupEventListeners() {
        $('#sort-toggle').on('click', function() {
            isNewestFirst = !isNewestFirst;
            $(this).html(`
                <span class="material-symbols-outlined">search</span>
                ${isNewestFirst ? 'Newest First' : 'Oldest First'}
            `);
            renderGameGrid();
        });
    }

    $(function() {
        initGallery();
    });

})(jQuery);
