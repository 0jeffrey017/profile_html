(async () => {
  'use strict';
  const home = document.getElementById('home-featured');
  const featured = document.getElementById('featured-list');
  const grid = document.getElementById('games-grid');
  if (!home && !grid) return;
  const escape = value => String(value ?? '').replace(/[&<>"']/g, c => ({'&':'&amp;','<':'&lt;','>':'&gt;','"':'&quot;',"'":'&#39;'}[c]));
  try {
    const response = await fetch('data/games_data.json');
    if (!response.ok) throw new Error(response.status);
    const all = (await response.json()).filter(game => game.status === 'released');
    const solo = game => /個人/.test(game.team);
    function card(game, index) {
      const video = game.video_preview || '';
      return `<a class="work-card" href="games/${escape(game.filename)}"><div class="work-media"><img src="${escape(game.image_path)}" alt="${escape(game.title)}" loading="lazy" width="800" height="500">${video ? `<video src="${escape(video)}" muted loop playsinline preload="none" aria-hidden="true" tabindex="-1"></video>` : ''}<span class="work-index">${String(index+1).padStart(2,'0')} / ${game.corporate_project ? '企業連携' : solo(game) ? '個人制作' : 'チーム制作'}</span></div><div class="work-content">${game.review_focus ? `<p class="work-evidence">${escape(game.review_focus.evidence)}</p>` : ''}<p class="work-kind">${escape(game.engine || 'GAME DEVELOPMENT')}</p><div class="work-top"><h3>${escape(game.title)}</h3><span aria-hidden="true">↗</span></div><div class="work-meta"><span>制作期間 ${escape(game.period)}</span><span>${escape(game.team)}</span></div><p class="work-description">${escape(game.description)}</p><dl class="work-contribution"><dt>私の担当</dt><dd>${escape(game.review_focus?.role || (game.responsible || []).join(" / "))}</dd></dl>${game.review_focus ? `<p class="work-focus"><span>実装の見どころ</span>${escape(game.review_focus.detail)}</p>` : ''}<span class="work-readmore">担当・課題・コードを見る <span aria-hidden="true">→</span></span><div class="work-tags">${(game.tags || []).slice(0,5).map(tag=>`<span>${escape(tag.name)}</span>`).join('')}</div></div></a>`;
    }
    function previews() {
      if (!matchMedia('(hover: hover)').matches || matchMedia('(prefers-reduced-motion: reduce)').matches) return;
      document.querySelectorAll('.work-card').forEach(card => {
        const video = card.querySelector('video'); if (!video) return;
        card.addEventListener('mouseenter', () => { video.play().then(() => { if (card.matches(':hover')) video.classList.add('is-playing'); else video.pause(); }).catch(()=>{}); });
        card.addEventListener('mouseleave', () => { video.pause(); video.classList.remove('is-playing'); });
      });
    }
    function render(filter = 'all') {
      const games = all.filter(game => filter === 'all' || (filter === 'solo' ? solo(game) : !solo(game)));
      const selected = games.filter(game => game.star).sort((a,b)=>(a.featured_order ?? 99)-(b.featured_order ?? 99));
      if (home) home.innerHTML = selected.map(card).join('');
      if (featured) { featured.innerHTML = selected.map(card).join(''); document.getElementById('featured-games').hidden = !selected.length; }
      if (grid) { const others = games.filter(game => !game.star); grid.innerHTML = others.map(card).join(''); document.getElementById('all-games').hidden = !others.length; }
      const count = document.getElementById('results-count'); if (count) count.textContent = `${filter === 'all' ? 'すべて' : filter === 'team' ? 'チーム制作' : '個人制作'}：${games.length}作品`;
      previews();
    }
    document.querySelectorAll('[data-filter]').forEach(button => button.addEventListener('click', () => {
      document.querySelectorAll('[data-filter]').forEach(item => item.setAttribute('aria-pressed', String(item === button))); render(button.dataset.filter);
    }));
    render();
  } catch (error) {
    (grid || home).innerHTML = '<p class="error-message">作品を読み込めませんでした。ページを再読み込みしてください。</p>';
    console.error(error);
  }
})();
