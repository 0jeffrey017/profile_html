(() => {
  const host = document.querySelector('[data-experience-chart]');
  if (!host) return;
  const note = document.querySelector('[data-experience-note]');
  const monthIndex = value => {
    if (value == "Now") {
      const today = new Date();
      const year = today.getFullYear();
      const month = today.getMonth() + 1;
      return year * 12 + month - 1;
    }
    if (typeof value !== 'string' || !/^\d{4}-(0[1-9]|1[0-2])$/.test(value)) return null;
    const [year, month] = value.split('-').map(Number);
    return year * 12 + month - 1;
  };
  const monthLabel = index => `${Math.floor(index / 12)}.${String(index % 12 + 1).padStart(2, '0')}`;
  const element = (tag, className, text) => {
    const node = document.createElement(tag);
    node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  };
  fetch('data/development-experience.json')
    .then(response => {
      if (!response.ok) throw new Error('Experience data unavailable');
      return response.json();
    })
    .then(data => {
      if (!Array.isArray(data) || !data.length) throw new Error('No experience data');
      const rows = data.map(row => {
        const start = monthIndex(row.start);
        const end = monthIndex(row.end);
        return { ...row, startIndex: start, endIndex: end, valid: start !== null && end !== null && end >= start };
      });
      const dated = rows.filter(row => row.valid);
      const preview = dated.length === 0;
      const previewMax = Math.max(24, ...rows.map(row => Number(row.previewMonths) || 0));
      const start = preview ? 0 : Math.min(...dated.map(row => row.startIndex));
      const end = preview ? previewMax : Math.max(start + 1, ...dated.map(row => Math.max(row.endIndex, row.startIndex + 1)));
      const span = end - start;
      const chart = element('div', 'experience-gantt');
      const header = element('div', 'experience-gantt-header');
      header.append(element('span', 'experience-tool-heading', 'ツール'));
      const axis = element('div', 'experience-axis');
      const ticks = Math.min(4, span);
      for (let i = 0; i <= ticks; i++) {
        axis.append(element('span', '', preview ? `${Math.round(span * i / ticks)}か月` : monthLabel(Math.round(start + span * i / ticks))));
      }
      header.append(axis);
      chart.append(header);
      rows.forEach(row => {
        const line = element('div', 'experience-gantt-row');
        line.append(element('span', 'experience-tool', row.tool));
        const track = element('div', 'experience-track');
        const months = preview ? Math.max(0, Number(row.previewMonths) || 0) : row.valid ? Math.max(1, row.endIndex - row.startIndex) : 0;
        if (months) {
          const offset = preview ? 0 : row.startIndex - start;
          const bar = element('div', `experience-bar${preview ? ' is-preview' : ''}`);
          bar.style.left = `${offset / span * 100}%`;
          bar.style.width = `${months / span * 100}%`;
          const description = preview ? `${row.tool}：${months}か月の経験（長さのみの仮配置）` : `${row.tool}：${row.start} 〜 ${row.end}`;
          track.setAttribute('role', 'img');
          track.setAttribute('aria-label', description);
          bar.title = description;
          track.append(bar);
        } else {
          track.append(element('span', 'experience-unset', '期間未設定'));
        }
        line.append(track);
        chart.append(line);
      });
      host.replaceChildren(chart);
      note.textContent = preview ? '開始・終了日は未設定です。現在は経験期間の長さだけを仮配置しています。' : '学習・制作に取り組んだ期間';
    })
    .catch(() => {
      note.textContent = '開発経験の期間を読み込めませんでした。';
    });
})();
