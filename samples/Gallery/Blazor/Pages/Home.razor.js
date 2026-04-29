// Gallery masonry — JS does reordering only, CSS does layout.

export function reveal(gridSelector, skeletonSelector) {
    reorder(gridSelector);
    const skeleton = document.querySelector(skeletonSelector);
    if (skeleton) skeleton.style.display = 'none';
    const grid = document.querySelector(gridSelector);
    if (grid) grid.style.opacity = '1';
}

// Spread NEW items across CSS columns, rest fills in between.
// CSS columns fill top-to-bottom, so item at index 0 goes to col 1,
// index 1 to col 1, etc. To put a NEW item in each column, we need
// to place them at evenly-spaced positions in the flat list.
export function reorder(gridSelector) {
    const grid = document.querySelector(gridSelector);
    if (!grid) return;

    const cols = parseInt(getComputedStyle(grid).columnCount) || 3;
    const items = Array.from(grid.querySelectorAll(':scope > .grid-item'));
    if (items.length === 0) return;

    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const rest = items.filter(el => el.dataset.isNew !== 'true');
    const total = items.length;

    // Calculate how many items per column (roughly)
    const perCol = Math.ceil(total / cols);

    // Build result: place NEW items at the start of each column's slice
    const result = new Array(total);
    let ni = 0;

    // For each column, reserve the first slot for a NEW item if available
    for (let c = 0; c < cols && ni < newItems.length; c++) {
        result[c * perCol] = newItems[ni++];
    }
    // Remaining NEW items go after the first batch
    for (let c = 0; c < cols && ni < newItems.length; c++) {
        result[c * perCol + 1] = newItems[ni++];
    }

    // Fill remaining slots with rest items
    let ri = 0;
    for (let i = 0; i < total; i++) {
        if (!result[i] && ri < rest.length) {
            result[i] = rest[ri++];
        }
    }

    // Append any leftovers
    while (ni < newItems.length) result.push(newItems[ni++]);
    while (ri < rest.length) result.push(rest[ri++]);

    // Apply to DOM (moves nodes, no cloning)
    result.filter(Boolean).forEach(el => grid.appendChild(el));
}
