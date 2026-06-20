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

    // Parse "2025/10-2026/2" etc. into a sortable YYYY*12+MM key from the start date
    function periodStartKey(period) {
        const m = (period || '').match(/(\d{4})\/(\d{1,2})/);
        return m ? parseInt(m[1], 10) * 12 + parseInt(m[2], 10) : 0;
    }

    // Extract the start year (e.g. "2026") for grouping
    function periodStartYear(period) {
        const m = (period || '').match(/(\d{4})/);
        return m ? m[1] : 'Other';
    }

    // タグは { type, name } の形式。色は type で決まる（lang / engine / design / tech）
    const TYPE_ORDER = { csharp: 0, cpp: 0, engine: 1, vcs: 2, design: 3, tech: 4 };

    function buildTagsHtml(game) {
        const tags = (game.tags || []).slice().sort(
            (a, b) => (TYPE_ORDER[a.type] ?? 9) - (TYPE_ORDER[b.type] ?? 9)
        );
        if (!tags.length) return "";

        const chips = tags.map(t => `<span class="game-tag tag-${t.type}">${t.name}</span>`);
        return `<div class="game-tags">${chips.join("")}</div>`;
    }

    function renderGameGrid() {
        const $grid = $('.game-grid');
        if (!$grid.length) return;

        $grid.empty();

        // Filter for released games (optional, depends on your preference)
        const displayGames = allGames.filter(game => game.status === "released");
        
        // Sort chronologically by the start date parsed from the "period" field
        displayGames.sort((a, b) => {
            const ka = periodStartKey(a.period);
            const kb = periodStartKey(b.period);
            return isNewestFirst ? kb - ka : ka - kb;
        });

        // Group games by year (parsed from the period start date)
        let currentYear = null;
        displayGames.forEach(game => {
            const year = periodStartYear(game.period);
            if (year !== currentYear) {
                currentYear = year;
                $grid.append(`<div class="year-divider"><span>${year}</span></div>`);
            }

            const tagsHtml = buildTagsHtml(game);
            const row = `
                <a class="game-row" href="games/${game.filename}" data-id="${game.id}">
                    <div class="game-row-thumb">
                        <img src="${game.image_path}" alt="${game.title}" onerror="this.src='https://via.placeholder.com/400x300?text=Image+Not+Found'">
                    </div>
                    <div class="game-row-body">
                        <div class="game-row-head">
                            <h3 class="game-row-title">${game.title}</h3>
                            ${game.star ? '<span class="recommend-badge">★ イチオシ</span>' : ''}
                            ${game.corporate_project ? '<span class="corporate-badge">★　企業プロジェクト</span>' : ''}
                        </div>
                        <div class="game-row-meta">
                            <span class="meta-pair"><span class="meta-k">制作期間</span><span class="meta-v">${game.period || '—'}</span></span>
                            <span class="meta-pair"><span class="meta-k">制作人数</span><span class="meta-v">${game.team || '—'}</span></span>
                        </div>
                        <p class="game-row-desc">${game.description}</p>
                        ${tagsHtml}
                    </div>
                    <span class="game-row-arrow material-symbols-outlined">arrow_forward</span>
                </a>
            `;
            $grid.append(row);
        });

        console.log("main.js: List rendered (NewestFirst: " + isNewestFirst + ")");
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

    function setupScrollReveal() {
        const targets = document.querySelectorAll('.reveal');
        if (!targets.length) return;

        // Fallback: if IntersectionObserver is unavailable, show everything
        if (!('IntersectionObserver' in window)) {
            targets.forEach(el => el.classList.add('is-visible'));
            return;
        }

        const observer = new IntersectionObserver((entries, obs) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    obs.unobserve(entry.target); // 一度表示したら監視解除
                }
            });
        }, { threshold: 0.12, rootMargin: '0px 0px -8% 0px' });

        targets.forEach(el => observer.observe(el));
    }

    $(function() {
        initGallery();
        setupScrollReveal();
    });

})(jQuery);
