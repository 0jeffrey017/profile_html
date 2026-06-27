// games-carousel.js — 制作実績を slick カルーセルで1画面に表示

(function ($) {
    "use strict";

    let allGames = [];
    let isNewestFirst = true;

    // "2025/10-2026/2" 等を開始年月から並べ替え用キーに変換
    function periodStartKey(period) {
        const m = (period || '').match(/(\d{4})\/(\d{1,2})/);
        return m ? parseInt(m[1], 10) * 12 + parseInt(m[2], 10) : 0;
    }

    const TYPE_ORDER = { csharp: 0, cpp: 0, engine: 1, vcs: 2, design: 3, tech: 4 };

    function buildTagsHtml(game) {
        const tags = (game.tags || []).slice().sort(
            (a, b) => (TYPE_ORDER[a.type] ?? 9) - (TYPE_ORDER[b.type] ?? 9)
        );
        if (!tags.length) return "";
        const chips = tags.map(t => `<span class="game-tag tag-${t.type}">${t.name}</span>`);
        return `<div class="game-tags">${chips.join("")}</div>`;
    }

    function cardHtml(game) {
        const badges = `
            ${game.star ? '<span class="recommend-badge">★ イチオシ</span>' : ''}
            ${game.corporate_project ? '<span class="corporate-badge">★ 企業プロジェクト</span>' : ''}
        `;
        return `
            <div>
                <a class="carousel-card" href="games/${game.filename}" data-id="${game.id}">
                    <div class="cc-thumb">
                        <img src="${game.image_path}" alt="${game.title}"
                            onerror="this.src='https://via.placeholder.com/400x300?text=Image+Not+Found'">
                        <div class="cc-badges">${badges}</div>
                    </div>
                    <div class="cc-body">
                        <h3 class="cc-title">${game.title}</h3>
                        <div class="cc-meta game-row-meta">
                            <span class="meta-pair"><span class="meta-k">制作期間</span><span class="meta-v">${game.period || '—'}</span></span>
                            <span class="meta-pair"><span class="meta-k">制作人数</span><span class="meta-v">${game.team || '—'}</span></span>
                        </div>
                        <p class="cc-desc">${game.description}</p>
                        ${buildTagsHtml(game)}
                    </div>
                </a>
            </div>
        `;
    }

    function renderCarousel() {
        const $c = $('.games-carousel');
        if (!$c.length) return;

        // 既存スライダーを破棄してから再構築（並び替え対応）
        if ($c.hasClass('slick-initialized')) {
            $c.slick('unslick');
        }
        $c.empty();

        const displayGames = allGames.filter(g => g.status === "released");
        displayGames.sort((a, b) => {
            const ka = periodStartKey(a.period);
            const kb = periodStartKey(b.period);
            return isNewestFirst ? kb - ka : ka - kb;
        });

        displayGames.forEach(g => $c.append(cardHtml(g)));

        $c.slick({
            slidesToShow: 3,
            slidesToScroll: 1,
            arrows: true,
            dots: true,
            infinite: true,
            speed: 400,
            responsive: [
                { breakpoint: 1100, settings: { slidesToShow: 2 } },
                { breakpoint: 760, settings: { slidesToShow: 1 } }
            ]
        });
    }

    async function init() {
        try {
            const res = await fetch('data/games_data.json');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            allGames = await res.json();
            renderCarousel();

            $('#sort-toggle').on('click', function () {
                isNewestFirst = !isNewestFirst;
                $(this).html(`
                    <span class="material-symbols-outlined">search</span>
                    ${isNewestFirst ? 'Newest First' : 'Oldest First'}
                `);
                renderCarousel();
            });
        } catch (err) {
            console.error("games-carousel.js: failed to load:", err);
            $('.games-carousel').html('<p class="error-message">作品データの読み込みに失敗しました。</p>');
        }
    }

    $(init);

})(jQuery);
