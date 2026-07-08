// ─────────────────────────────────────────────────────────────
// "Moving faster with AI" dashboard renderer.
//
// Renders three panels from a build-time cache (./dashboard-data.json):
//   1. Adoption at a glance   — NuGet download figures
//   2. Time to keep up        — Chrome milestone → sync-PR cadence (the hero)
//   3. What the automation costs — per-run tokens/turns
//
// The data is cached when the site rebuilds; a source that was unreachable at
// build time keeps its last-known values and an "as of <date>" note (produced by
// scripts/infra/docs/generate-ai-dashboard.py). This script only reads the cache
// and never calls a third-party API, so the page cannot be slowed or broken by an
// upstream outage. Day-deltas are computed here, never read from the file.
// ─────────────────────────────────────────────────────────────

(() => {
  'use strict';

  const root = document.querySelector('[data-dashboard]');
  if (!root) return;

  // ── Formatting helpers ──────────────────────────────────────

  // 289720015 -> "289.7M", 74059 -> "74,059", 5098046 -> "5.1M".
  const compact = (n) => {
    if (n == null || isNaN(n)) return '—';
    if (n >= 1e9) return trim(n / 1e9) + 'B';
    if (n >= 1e6) return trim(n / 1e6) + 'M';
    if (n >= 1e3) return trim(n / 1e3) + 'K';
    return String(n);
  };
  const trim = (x) => {
    const s = x.toFixed(1);
    return s.endsWith('.0') ? s.slice(0, -2) : s;
  };
  const grouped = (n) =>
    (n == null || isNaN(n)) ? '—' : n.toLocaleString('en-US');

  // "2026-07-02" -> "Jul 2, 2026" (parsed as UTC to avoid an off-by-one).
  const MONTHS = ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun',
    'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
  const prettyDate = (iso) => {
    if (!iso) return '';
    const [y, m, d] = iso.split('-').map(Number);
    if (!y || !m || !d) return iso;
    return `${MONTHS[m - 1]} ${d}, ${y}`;
  };
  const shortDate = (iso) => {
    if (!iso) return '';
    const [, m, d] = iso.split('-').map(Number);
    if (!m || !d) return iso;
    return `${MONTHS[m - 1]} ${d}`;
  };
  // Whole-day difference between two ISO dates (b - a), UTC.
  const daysBetween = (a, b) => {
    if (!a || !b) return null;
    const ms = Date.parse(b + 'T00:00:00Z') - Date.parse(a + 'T00:00:00Z');
    if (isNaN(ms)) return null;
    return Math.round(ms / 86400000);
  };
  const dayLabel = (n) => {
    if (n == null) return '—';
    if (n <= 0) return 'same day';
    return n === 1 ? '1 day' : `${n} days`;
  };
  // "+3" / "−417" with a real minus sign for negatives.
  const fmtSigned = (n) => (n > 0 ? '+' + n : '\u2212' + Math.abs(n));

  const el = (tag, cls, html) => {
    const node = document.createElement(tag);
    if (cls) node.className = cls;
    if (html != null) node.innerHTML = html;
    return node;
  };
  const esc = (s) => String(s == null ? '' : s).replace(/[&<>"]/g, (c) =>
    ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]));

  const panel = (name) => root.querySelector(`[data-panel="${name}"]`);
  const setAsOf = (p, iso) => {
    const tag = p && p.querySelector('[data-asof]');
    if (tag) tag.textContent = iso ? `as of ${prettyDate(iso)}` : '';
  };
  const bodyOf = (p) => p && p.querySelector('[data-body]');

  // ── Panel 1: Adoption at a glance ───────────────────────────

  function renderAdoption(data) {
    const p = panel('adoption');
    if (!p || !data) return fail(p, 'adoption');
    const body = bodyOf(p);
    body.innerHTML = '';

    const prim = data.primary || {};
    const stats = el('div', 'dash-figs');
    stats.appendChild(fig(compact(prim.total), 'total downloads', grouped(prim.total)));
    const ga = prim.currentGa || {};
    stats.appendChild(fig(compact(ga.downloads),
      `downloads of ${esc(ga.version || 'the current GA')}`, grouped(ga.downloads)));
    body.appendChild(stats);

    // Downloads by release line (major.minor): cumulative stable vs previews.
    const lines = prim.lines || [];
    if (lines.length) {
      body.appendChild(el('p', 'dash-subhead', 'Downloads by release line'));
      const table = el('table', 'ver-table');
      table.innerHTML =
        '<thead><tr><th>Version</th>' +
        '<th class="ver-num">Stable</th>' +
        '<th class="ver-num">Previews</th></tr></thead>';
      const tbody = el('tbody');
      lines.forEach((l) => {
        const tr = el('tr');
        const label = l.stableVersion
          ? `${esc(l.line)}<span class="ver-sub">${esc(l.stableVersion)}</span>`
          : `${esc(l.line)}<span class="ver-sub">preview only</span>`;
        tr.appendChild(el('th', 'ver-line', label));
        tr.appendChild(verCell(l.stable));
        tr.appendChild(verCell(l.previews));
        tbody.appendChild(tr);
      });
      table.appendChild(tbody);
      const wrap = el('div', 'ver-table-wrap');
      wrap.appendChild(table);
      body.appendChild(wrap);
    }

    linkSource(p, data.sourceUrl);
    setAsOf(p, data.asOf);
    p.setAttribute('aria-busy', 'false');
  }

  function fig(value, label, title) {
    const f = el('div', 'dash-fig');
    const num = el('span', 'dash-fig-num', esc(value));
    if (title) num.title = title + ' downloads';
    f.appendChild(num);
    f.appendChild(el('span', 'dash-fig-label', label));
    return f;
  }

  // A downloads cell: compact number, exact count on hover, "—" for zero.
  function verCell(n) {
    const td = el('td', 'ver-num');
    if (!n) {
      td.classList.add('ver-zero');
      td.textContent = '—';
      return td;
    }
    td.textContent = compact(n);
    td.title = grouped(n) + ' downloads';
    return td;
  }

  // ── Panel 2: Time to keep up with upstream (the hero) ───────

  function renderCadence(data) {
    const p = panel('cadence');
    if (!p || !data) return fail(p, 'cadence');
    const body = bodyOf(p);
    body.innerHTML = '';

    const caption = p.querySelector('[data-caption]');
    if (caption && data.caption) caption.textContent = data.caption;
    linkAttr(p, '[data-prs-link]', data.prsUrl);
    linkSource(p, data.scheduleUrl);

    const milestones = (data.milestones || []).map((m) => ({
      ...m,
      branchToPr: daysBetween(m.branchPoint, m.prOpened),
      prToMerge: daysBetween(m.prOpened, m.prMerged),
      mergeToShip: daysBetween(m.prMerged, m.firstPreview),
      mergedToStable: daysBetween(m.prMerged, m.stableDate),
    }));
    if (!milestones.length) return fail(p, 'cadence');

    // Each metric column: lower is better, so a negative change (faster than the
    // previous milestone) is the win and renders green.
    const cols = [
      { key: 'branchToPr', label: 'Behind Chrome',
        tip: 'Chrome branch cut → our sync PR opened' },
      { key: 'prToMerge', label: 'Review',
        tip: 'PR opened → merged (humans review the API)' },
      { key: 'mergeToShip', label: 'To NuGet',
        tip: 'merged → first package published on NuGet' },
    ];

    const table = el('table', 'cad-table');
    const thead = el('thead');
    const hrow = el('tr');
    hrow.appendChild(el('th', 'cad-th-ms', 'Milestone'));
    cols.forEach((c) => {
      const th = el('th', null, `${esc(c.label)}<span class="cad-unit">days</span>`);
      th.title = c.tip;
      hrow.appendChild(th);
    });
    thead.appendChild(hrow);
    table.appendChild(thead);

    const tbody = el('tbody');
    milestones.forEach((m, i) => {
      const prev = i > 0 ? milestones[i - 1] : null;
      const isNewest = i === milestones.length - 1;
      const tr = el('tr', isNewest ? 'cad-tr-newest' : null);

      const th = el('th', 'cad-ms-cell');
      th.setAttribute('scope', 'row');
      let msHtml = `<span class="cad-ms">m${esc(m.milestone)}</span>`;
      if (isNewest) msHtml += '<span class="cad-win">newest</span>';
      if (m.note) msHtml += `<span class="cad-note">${esc(m.note)}</span>`;
      th.innerHTML = msHtml;
      tr.appendChild(th);

      cols.forEach((c) => {
        const val = m[c.key];
        const prevVal = prev ? prev[c.key] : null;
        tr.appendChild(metricCell(val, prevVal));
      });
      tbody.appendChild(tr);
    });
    table.appendChild(tbody);

    const wrap = el('div', 'cad-table-wrap');
    wrap.appendChild(table);
    body.appendChild(wrap);

    body.appendChild(el('p', 'cad-legend-note',
      'Each cell is the number of days; the chip is the change from the ' +
      'milestone above it — days and percent, where ' +
      '<span class="cad-delta cad-delta-good">&minus;</span> is faster and ' +
      '<span class="cad-delta cad-delta-bad">+</span> slower.'));

    // Bonus context on the newest milestone: ahead of Chrome stable.
    const newest = milestones[milestones.length - 1];
    if (newest && newest.mergedToStable != null && newest.mergedToStable > 0) {
      body.appendChild(el('p', 'cad-ahead',
        `m${esc(newest.milestone)} merged <b>${dayLabel(newest.mergedToStable)}</b> ` +
        `before Chrome ${esc(newest.milestone)} reaches stable ` +
        `(${esc(shortDate(newest.stableDate))}) — ahead of Chrome, not chasing it.`));
    }

    setAsOf(p, data.asOf);
    p.setAttribute('aria-busy', 'false');
  }

  // A metric cell: the value (in days) plus a signed change chip vs the previous
  // milestone — both the day delta and the percent change. Fewer days is better,
  // so a drop is a "good" (green) delta.
  function metricCell(val, prevVal) {
    const td = el('td', 'cad-cell');
    if (val == null) {
      td.classList.add('cad-cell-empty');
      td.innerHTML = '<span class="cad-val">—</span>';
      td.title = 'not yet shipped';
      return td;
    }
    const value = val <= 0
      ? '<span class="cad-val cad-val-zero">same day</span>'
      : `<span class="cad-val">${val}</span>`;
    let chip = '';
    if (prevVal != null) {
      const delta = val - prevVal;
      const cls = delta < 0 ? 'good' : delta > 0 ? 'bad' : 'flat';
      let text = delta === 0 ? '±0' : fmtSigned(delta);
      // Percent change needs a non-zero base; skip it when the previous was zero.
      if (prevVal !== 0 && delta !== 0) {
        text += ` <span class="cad-pct">${fmtSigned(Math.round((delta / prevVal) * 100))}%</span>`;
      }
      chip = `<span class="cad-delta cad-delta-${cls}">${text}</span>`;
    }
    td.innerHTML = value + chip;
    return td;
  }

  // ── Panel 3: What the automation costs (per run) ────────────

  function renderCost(data) {
    const p = panel('cost');
    if (!p || !data) return fail(p, 'cost');
    const body = bodyOf(p);
    body.innerHTML = '';

    const cards = el('div', 'cost-cards');
    (data.workflows || []).forEach((w) => {
      const card = el('article', 'cost-card');
      const head = el('div', 'cost-card-head');
      const name = w.workflowUrl
        ? `<a href="${esc(w.workflowUrl)}" target="_blank" rel="noopener">${esc(w.name)}</a>`
        : esc(w.name);
      head.appendChild(el('h4', 'cost-name', name));
      if (w.cadence) head.appendChild(el('span', 'cost-cadence', esc(w.cadence)));
      card.appendChild(head);

      const metrics = el('div', 'cost-metrics');
      metrics.appendChild(costMetric(compact(w.tokens), 'effective tokens', grouped(w.tokens)));
      metrics.appendChild(costMetric(grouped(w.turns), 'agent turns'));
      card.appendChild(metrics);
      cards.appendChild(card);
    });
    body.appendChild(cards);

    if (data.note) body.appendChild(el('p', 'cost-note', esc(data.note)));

    linkSource(p, data.sourceUrl);
    setAsOf(p, data.asOf);
    p.setAttribute('aria-busy', 'false');
  }

  function costMetric(value, label, title) {
    const m = el('div', 'cost-metric');
    const num = el('span', 'cost-num', esc(value));
    if (title) num.title = title;
    m.appendChild(num);
    m.appendChild(el('span', 'cost-label', label));
    return m;
  }

  // ── Shared helpers ──────────────────────────────────────────

  function linkSource(p, url) { linkAttr(p, '[data-source-link]', url); }
  function linkAttr(p, sel, url) {
    const a = p && p.querySelector(sel);
    if (a && url) a.setAttribute('href', url);
  }

  function fail(p, name) {
    if (!p) return;
    const body = bodyOf(p);
    if (body) {
      body.innerHTML =
        '<p class="dash-error">This data could not be loaded. ' +
        'The linked source below has the current figures.</p>';
    }
    p.setAttribute('aria-busy', 'false');
  }

  function failAll(msg) {
    ['adoption', 'cadence', 'cost'].forEach((n) => fail(panel(n), n));
    root.setAttribute('data-dashboard-error', msg || 'load-failed');
  }

  // ── Load the cached data and render ─────────────────────────

  fetch('./dashboard-data.json', { cache: 'no-cache' })
    .then((r) => {
      if (!r.ok) throw new Error('HTTP ' + r.status);
      return r.json();
    })
    .then((data) => {
      try { renderAdoption(data.adoption); } catch (e) { fail(panel('adoption')); }
      try { renderCadence(data.cadence); } catch (e) { fail(panel('cadence')); }
      try { renderCost(data.cost); } catch (e) { fail(panel('cost')); }
    })
    .catch((e) => failAll(String(e)));
})();
