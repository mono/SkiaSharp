// Gallery masonry — JS creates flex columns and distributes cards.
// CSS column-count can't guarantee NEW-at-top-per-column because
// it balances heights unpredictably. So we create column divs.

let lastCols = 0;

export function reveal(gridSelector, skeletonSelector) {
    layout(gridSelector);
    document.querySelector(skeletonSelector)?.remove();
    const grid = document.querySelector(gridSelector);
    if (grid) grid.style.opacity = '1';

    // Re-layout when column count breakpoint changes
    window.addEventListener('resize', () => {
        const cols = getColCount();
        if (cols !== lastCols) layout(gridSelector);
    });
}

export function reorder(gridSelector) {
    layout(gridSelector);
}

function getColCount() {
    const w = window.innerWidth;
    if (w <= 680) return 1;
    if (w <= 995) return 2;
    return 3;
}

function layout(gridSelector) {
    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    const cols = getColCount();
    lastCols = cols;

    // Collect all .grid-item (may be inside column divs or direct children)
    const items = [];
    grid.querySelectorAll('.grid-item').forEach(el => items.push(el));

    // Remove existing column divs
    grid.querySelectorAll('.mc').forEach(c => {
        while (c.firstChild) grid.appendChild(c.firstChild);
        c.remove();
    });

    if (items.length === 0) return;

    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const rest = items.filter(el => el.dataset.isNew !== 'true');

    // Create column containers
    grid.style.display = 'flex';
    grid.style.gap = '1rem';
    grid.style.alignItems = 'flex-start';

    const colEls = [];
    for (let i = 0; i < cols; i++) {
        const col = document.createElement('div');
        col.className = 'mc';
        col.style.cssText = 'flex:1;display:flex;flex-direction:column;gap:1rem;min-width:0';
        colEls.push(col);
        grid.appendChild(col);
    }

    // 1) Distribute NEW items: one per column, round-robin
    newItems.forEach((el, i) => colEls[i % cols].appendChild(el));

    // 2) Distribute rest items: shortest column first
    rest.forEach(el => {
        let shortest = 0;
        let minH = colEls[0].offsetHeight;
        for (let c = 1; c < cols; c++) {
            const h = colEls[c].offsetHeight;
            if (h < minH) { minH = h; shortest = c; }
        }
        colEls[shortest].appendChild(el);
    });
}
