// Gallery masonry — JS does reordering only, CSS does layout.

// Called on first load: reorder cards, hide skeleton, reveal grid.
export function reveal(gridSelector, skeletonSelector) {
    reorder(gridSelector);
    const skeleton = document.querySelector(skeletonSelector);
    if (skeleton) skeleton.style.display = 'none';
    const grid = document.querySelector(gridSelector);
    if (grid) grid.style.opacity = '1';
}

// Called after Blazor re-renders (filter/search change): reorder cards.
export function reorder(gridSelector) {
    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    // Read column count from CSS — no magic numbers
    const cols = parseInt(getComputedStyle(grid).columnCount) || 3;

    const items = Array.from(grid.querySelectorAll(':scope > .grid-item'));
    if (items.length === 0) return;

    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const rest = items.filter(el => el.dataset.isNew !== 'true');

    // CSS columns fill top-to-bottom. To spread NEW across columns evenly,
    // we interleave them at intervals of `cols` positions. This way, each
    // NEW item lands in a different column.
    const result = [];
    let ni = 0, ri = 0;
    const totalNew = newItems.length;
    const spacing = totalNew > 0 ? Math.max(1, Math.floor(items.length / totalNew)) : items.length;

    for (let i = 0; i < items.length; i++) {
        if (ni < totalNew && i % spacing === 0) {
            result.push(newItems[ni++]);
        } else if (ri < rest.length) {
            result.push(rest[ri++]);
        }
    }
    while (ni < totalNew) result.push(newItems[ni++]);
    while (ri < rest.length) result.push(rest[ri++]);

    // Append in new order (moves existing DOM nodes, no cloning)
    result.forEach(el => grid.appendChild(el));
}
