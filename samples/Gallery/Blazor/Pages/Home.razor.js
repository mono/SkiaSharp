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
// NEW items go first (they'll fill the top of CSS columns), then the rest.
export function reorder(gridSelector) {
    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    const items = Array.from(grid.querySelectorAll(':scope > .grid-item'));
    if (items.length === 0) return;

    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const rest = items.filter(el => el.dataset.isNew !== 'true');

    // NEW first, then rest. CSS columns fill top-to-bottom, so
    // NEW items naturally appear at the top of each column.
    newItems.forEach(el => grid.appendChild(el));
    rest.forEach(el => grid.appendChild(el));
}
