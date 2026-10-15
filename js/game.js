(() => {
  'use strict';
  if (document.body.classList.contains('portfolio')) {
    const background = document.createElement('script');
    background.src = new URL('sakura-background.js', document.currentScript.src).href;
    background.async = true;
    document.head.append(background);
  }
  const nav = document.querySelector('.site-nav');
  const toggle = document.querySelector('.nav-toggle');
  const setOpen = open => { nav?.classList.toggle('is-open', open); toggle?.setAttribute('aria-expanded', String(open)); };
  toggle?.addEventListener('click', () => setOpen(toggle.getAttribute('aria-expanded') !== 'true'));
  nav?.addEventListener('click', e => { if (e.target.closest('a')) setOpen(false); });
  document.addEventListener('click', e => { if (!nav?.contains(e.target) && !toggle?.contains(e.target)) setOpen(false); });
  document.querySelectorAll('.code-header').forEach(header => {
    const button = document.createElement('button');
    button.type = 'button'; button.className = header.className; button.innerHTML = header.innerHTML;
    header.replaceWith(button);
    button.setAttribute('aria-expanded', String(!button.parentElement.classList.contains('collapsed')));
    button.addEventListener('click', () => { const folded = button.parentElement.classList.toggle('collapsed'); button.setAttribute('aria-expanded', String(!folded)); });
  });
  const overlay = document.getElementById('gameplay-overlay');
  const play = document.getElementById('play-game-btn');
  const close = document.getElementById('close-overlay');
  const iframe = document.getElementById('game-iframe');
  const start = document.getElementById('start-game-btn');
  const main = document.querySelector('main');
  const header = document.querySelector('.site-header');
  const footer = document.querySelector('.page-footer-mini');
  function closeGame() {
    if (!overlay || overlay.style.display === 'none') return;
    overlay.style.display = 'none'; iframe.src = ''; document.body.style.overflow = '';
    [main, header, footer].forEach(el => { if (el) el.inert = false; });
    play.focus();
  }
  if (overlay) { overlay.setAttribute('role','dialog'); overlay.setAttribute('aria-modal','true'); overlay.setAttribute('aria-label','ゲームをプレイ'); iframe.title = 'ゲーム画面'; }
  if (play?.classList.contains('disabled')) play.disabled = true;
  play?.addEventListener('click', () => {
    const url = play.dataset.url;
    if (!url || url === '#') return;
    if (play.dataset.linkType === 'download') { window.open(url, '_blank', 'noopener,noreferrer'); return; }
    overlay.style.display = 'flex'; overlay.classList.remove('gameplay-active');
    document.getElementById('game-container').style.display = 'none';
    document.getElementById('start-btn-container').style.display = '';
    document.body.style.overflow = 'hidden';
    [main, header, footer].forEach(el => { if (el) el.inert = true; }); close.focus();
  });
  start?.addEventListener('click', () => { overlay.classList.add('gameplay-active'); document.getElementById('game-container').style.display = 'block'; document.getElementById('start-btn-container').style.display = 'none'; iframe.src = play.dataset.url; });
  close?.addEventListener('click', closeGame);
  overlay?.addEventListener('click', e => { if (e.target === overlay) closeGame(); });
  document.addEventListener('keydown', e => {
    if (e.key === 'Escape') { setOpen(false); closeGame(); }
    if (e.key === 'Tab' && overlay && overlay.style.display !== 'none') {
      const items = [...overlay.querySelectorAll('button, iframe, a[href]')].filter(el => el.getClientRects().length && !el.disabled);
      const first = items[0], last = items.at(-1);
      if (e.shiftKey && document.activeElement === first) { e.preventDefault(); last.focus(); }
      if (!e.shiftKey && document.activeElement === last) { e.preventDefault(); first.focus(); }
    }
  });
})();
