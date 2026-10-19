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
  const element = (tag, className, text) => {
    const node = document.createElement(tag);
    node.className = className;
    if (text !== undefined) node.textContent = text;
    return node;
  };
  const durationLabel = months => {
    const years = Math.floor(months / 12);
    const remainder = months % 12;
    return `${years ? `${years}年` : ''}${remainder ? `${remainder}か月` : ''}` || '1か月未満';
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
      const longest = preview ? previewMax : Math.max(...dated.map(row => Math.max(1, row.endIndex - row.startIndex)));
      const span = Math.max(24, Math.ceil(longest / 12) * 12);
      const chart = element('div', 'experience-gantt');
      const years = span / 12;
      chart.style.setProperty('--experience-years', years);
      const header = element('div', 'experience-gantt-header');
      header.append(element('span', 'experience-tool-heading', 'スキル'));
      const axis = element('div', 'experience-axis');
      for (let year = 0; year < years; year++) {
        axis.append(element('span', '', `${year}〜${year + 1}年`));
      }
      header.append(axis);
      chart.append(header);
      rows.forEach(row => {
        const line = element('div', 'experience-gantt-row');
        const track = element('div', 'experience-track');
        const months = preview ? Math.max(0, Number(row.previewMonths) || 0) : row.valid ? Math.max(1, row.endIndex - row.startIndex) : 0;
        const summary = element('div', 'experience-summary');
        summary.append(element('span', 'experience-tool', row.tool));
        line.append(summary);
        if (months) {
          const bar = element('div', `experience-bar${preview ? ' is-preview' : ''}`);
          bar.style.left = '0';
          bar.style.width = `${months / span * 100}%`;
          const description = preview ? `${row.tool}：${months}か月の経験（長さのみの仮配置）` : `${row.tool}：約${durationLabel(months)}、${row.start} 〜 ${row.end === 'Now' ? '現在' : row.end}`;
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
      note.textContent = preview ? '1区切り＝1年。' : '学習・制作の経験期間';
    })
    .catch(() => {
      note.textContent = '開発経験の期間を読み込めませんでした。';
    });
})();
