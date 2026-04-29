// Gallery masonry — JS reorders DOM so CSS columns place NEW at top.

export function reveal(gridSelector, skeletonSelector) {
    reorder(gridSelector);
    const skeleton = document.querySelector(skeletonSelector);
    if (skeleton) skeleton.style.display = 'none';
    const grid = document.querySelector(gridSelector);
    if (grid) grid.style.opacity = '1';

    // Re-run reorder when column count changes on resize
    let lastCols = parseInt(getComputedStyle(grid).columnCount) || 3;
    window.addEventListener('resize', () => {
        const cols = parseInt(getComputedStyle(grid).columnCount) || 3;
        if (cols !== lastCols) {
            lastCols = cols;
            reorder(gridSelector);
        }
    });
}

export function reorder(gridSelector) {
    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    const cols = parseInt(getComputedStyle(grid).columnCount) || 3;
    const items = Array.from(grid.querySelectorAll('.grid-item'));
    if (items.length === 0) return;

    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const rest = items.filter(el => el.dataset.isNew !== 'true');

    // CSS columns fill top-to-bottom. With N items and C columns,
    // column K gets items at indices K*(N/C) through (K+1)*(N/C)-1.
    // To place a NEW item at the top of column K, put it at index K*(N/C).
    const total = items.length;
    const result = new Array(total).fill(null);

    // Reserve top slots: for each column, place NEW items starting at
    // the column's first index
    const colSize = Math.ceil(total / cols);
    let ni = 0;
    for (let c = 0; c < cols; c++) {
        const colStart = c * colSize;
        // Place as many NEW items as available for this column
        while (ni < newItems.length && ni < (c + 1) * Math.ceil(newItems.length / cols)) {
            // Find next empty slot in this column's range
            let slot = colStart + (ni - c * Math.floor(newItems.length / cols));
            if (slot < total && !result[slot]) {
                result[slot] = newItems[ni++];
            } else {
                break;
            }
        }
    }
    // Place any remaining NEW items
    for (let i = 0; i < total && ni < newItems.length; i++) {
        if (!result[i]) result[i] = newItems[ni++];
    }

    // Fill rest
    let ri = 0;
    for (let i = 0; i < total; i++) {
        if (!result[i] && ri < rest.length) {
            result[i] = rest[ri++];
        }
    }

    // Append leftovers
    while (ri < rest.length) result.push(rest[ri++]);

    result.filter(Boolean).forEach(el => grid.appendChild(el));
}
