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

    // 静止画(ポスター)と同名の .mp4 を対にする。video_preview があればそれを優先。
    function videoPathFor(game) {
        if (game.video_preview) return game.video_preview;
        return (game.image_path || '').replace(/\.(png|jpe?g|webp)$/i, '.mp4');
    }

    function cardHtml(game) {
        const badges = buildBadgesHtml(game);
        const video = videoPathFor(game);
        const videoHtml = video
            ? `<video class="cc-video" src="${video}" muted loop playsinline preload="none" tabindex="-1" aria-hidden="true"></video>`
            : '';
        return `
            <div>
                <a class="carousel-card" href="games/${game.filename}" data-id="${game.id}">
                    <div class="cc-thumb">
                        <img class="cc-img" src="${game.image_path}" alt="${game.title}"
                            loading="lazy"
                            onerror="this.onerror=null;this.src='Image/placeholder.svg'">
                        ${videoHtml}
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

    // カーソルを合わせると .mp4 プレビューをフェードイン再生、外すと停止してポスターに戻す
    function playPreview(card) {
        const video = card.querySelector('.cc-video');
        // 動画なし / 過去に読み込み失敗なら何もしない（ポスターのまま）
        if (!video || video.dataset.failed) return;

        video.play()
            .then(() => video.classList.add('is-playing'))
            .catch(() => { video.dataset.failed = '1'; });
    }

    function stopPreview(card) {
        const video = card.querySelector('.cc-video');
        if (!video) return;
        video.pause();
        video.classList.remove('is-playing');
        try { video.currentTime = 0; } catch (e) { /* 未読み込み時は無視 */ }
    }

    function bindPreview() {
        const touchLike = window.matchMedia('(hover: none)').matches;

        if (!touchLike) {
            // デスクトップ：ホバーで再生（動的生成カードに対応するため委譲で登録）
            $(document)
                .on('mouseenter.preview', '.carousel-card', function () { playPreview(this); })
                .on('mouseleave.preview', '.carousel-card', function () { stopPreview(this); });
            return;
        }

        // タッチ端末：ホバーが無いので、画面内に入ったカードを自動再生する
        if (!('IntersectionObserver' in window)) return;
        const observer = new IntersectionObserver((entries) => {
            entries.forEach((entry) => {
                if (entry.isIntersecting) {
                    playPreview(entry.target);
                } else {
                    stopPreview(entry.target);
                }
            });
        }, { threshold: 0.6 });
        document.querySelectorAll('.carousel-card').forEach(card => observer.observe(card));
    }

    async function init() {
        try {
            const res = await fetch('data/games_data.json');
            if (!res.ok) throw new Error(`HTTP ${res.status}`);
            allGames = await res.json();
            renderFeatured();
            renderGrid();
            bindPreview();
        } catch (err) {
            console.error("games-carousel.js: failed to load:", err);
            $('#games-grid').html('<p class="error-message">作品データの読み込みに失敗しました。</p>');
        }
    }

    $(init);

})(jQuery);
