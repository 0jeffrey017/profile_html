// games-carousel.js — 制作実績を「イチオシ（上）＋グリッド（下）」で表示する

(function ($) {
    "use strict";

    let allGames = [];

    const TYPE_ORDER = { csharp: 0, cpp: 0, engine: 1, vcs: 2, design: 3, tech: 4 };

    function buildTagsHtml(game) {
        const tags = (game.tags || []).slice().sort(
            (a, b) => (TYPE_ORDER[a.type] ?? 9) - (TYPE_ORDER[b.type] ?? 9)
        );
        if (!tags.length) return "";
        const chips = tags.map(t => `<span class="game-tag tag-${t.type}">${t.name}</span>`);
        return `<div class="game-tags">${chips.join("")}</div>`;
    }

    function buildBadgesHtml(game) {
        return `
            ${game.star ? '<span class="recommend-badge">★ イチオシ</span>' : ''}
            ${game.corporate_project ? '<span class="corporate-badge">★ 企業プロジェクト</span>' : ''}
        `;
    }

    // 静止画(ポスター)と同名の .gif を対にする。image_gif があればそれを優先。
    function gifPathFor(game) {
        if (game.image_gif) return game.image_gif;
        return (game.image_path || '').replace(/\.(png|jpe?g|webp)$/i, '.gif');
    }

    function cardHtml(game) {
        const badges = buildBadgesHtml(game);
        const gif = gifPathFor(game);
        return `
            <div>
                <a class="carousel-card" href="games/${game.filename}" data-id="${game.id}">
                    <div class="cc-thumb">
                        <img class="cc-img" src="${game.image_path}" alt="${game.title}"
                            data-poster="${game.image_path}" data-gif="${gif}" loading="lazy"
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

    // イチオシ作品（star）は上部の独立セクションに表示する
    function renderFeatured() {
        const $f = $('#featured-list');
        if (!$f.length) return;

        const featured = allGames.filter(g => g.status === "released" && g.star);
        if (!featured.length) {
            $('#featured-games').hide();
            return;
        }
        $f.html(featured.map(cardHtml).join(''));
    }

    // それ以外の作品はグリッドで一覧表示する（並び替えなし・JSON順）
    function renderGrid() {
        const $g = $('#games-grid');
        if (!$g.length) return;

        const games = allGames.filter(g => g.status === "released" && !g.star);
        $g.html(games.map(cardHtml).join(''));
    }

    // カーソルを合わせると .gif を再生（差し替え）、外すと静止画に戻す＝停止
    function playGif(card) {
        const img = card.querySelector('.cc-img');
        if (!img) return;
        const gif = img.dataset.gif;
        // gif 未指定 / 読み込み失敗済み / 既に再生中なら何もしない
        if (!gif || img.dataset.gifFailed || img.src.endsWith(gif)) return;

        // 先読みして、存在する場合のみ差し替える（壊れ画像のチラつきを防ぐ）
        const pre = new Image();
        pre.onload = () => { if (card.matches(':hover')) img.src = gif; };
        pre.onerror = () => { img.dataset.gifFailed = '1'; };
        pre.src = gif;
    }

    function stopGif(card) {
        const img = card.querySelector('.cc-img');
        if (img && img.dataset.poster) img.src = img.dataset.poster;
    }

    function bindHoverGif() {
        // 動的生成カードに対応するため委譲で登録（jQuery は mouseenter/leave を委譲対応）
        $(document)
            .on('mouseenter.gif', '.carousel-card', function () { playGif(this); })
            .on('mouseleave.gif', '.carousel-card', function () { stopGif(this); });
    }

    async function init() {
        try {
            const res = await fetch('data/games_data.json');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            allGames = await res.json();
            renderFeatured();
            renderGrid();
            bindHoverGif();
        } catch (err) {
            console.error("games-carousel.js: failed to load:", err);
            $('#games-grid').html('<p class="error-message">作品データの読み込みに失敗しました。</p>');
        }
    }

    $(init);

})(jQuery);
