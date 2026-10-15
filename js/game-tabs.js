(() => {
  'use strict';
  const buttons = [...document.querySelectorAll('.tab-btn')];
  const panels = [...document.querySelectorAll('.tab-panel')];
  if (!buttons.length) return;
  buttons[0].parentElement.setAttribute('role', 'tablist');
  buttons[0].parentElement.setAttribute('aria-label', '作品の詳細');
  buttons.forEach(button => {
    button.id = `tab-${button.dataset.tab}`; button.setAttribute('role','tab');
    button.setAttribute('aria-controls', `panel-${button.dataset.tab}`);
  });
  panels.forEach(panel => { panel.id = `panel-${panel.dataset.panel}`; panel.setAttribute('role','tabpanel'); panel.setAttribute('aria-labelledby', `tab-${panel.dataset.panel}`); panel.tabIndex = 0; });
  function activate(name, update = true) {
    if (!buttons.some(button => button.dataset.tab === name)) name = 'overview';
    buttons.forEach(button => { const active = button.dataset.tab === name; button.classList.toggle('is-active',active); button.setAttribute('aria-selected',String(active)); button.tabIndex = active ? 0 : -1; });
    panels.forEach(panel => { const active = panel.dataset.panel === name; panel.classList.toggle('is-active', active); panel.hidden = !active; if (!active) panel.querySelectorAll('video').forEach(video => video.pause()); });
    if (update) history.replaceState(null,'',`#${name}`);
  }
  document.addEventListener('click', e => { const target = e.target.closest('.tab-btn, [data-tab-target]'); if (target) { e.preventDefault(); const name = target.dataset.tab || target.dataset.tabTarget; activate(name); if (target.hasAttribute('data-tab-target')) { const tab = buttons.find(button => button.dataset.tab === name); tab?.focus(); tab?.scrollIntoView({block: 'nearest'}); } } });
  buttons.forEach((button,index) => button.addEventListener('keydown', e => {
    const next = {ArrowRight:(index+1)%buttons.length,ArrowLeft:(index+buttons.length-1)%buttons.length,Home:0,End:buttons.length-1}[e.key];
    if (next !== undefined) { e.preventDefault(); activate(buttons[next].dataset.tab); buttons[next].focus(); }
  }));
  window.addEventListener('hashchange', () => activate(location.hash.slice(1), false));
  activate(location.hash.slice(1), false);
})();
