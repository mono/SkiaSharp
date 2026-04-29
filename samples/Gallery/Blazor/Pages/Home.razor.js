// masonry.js — JS-driven column layout for the Gallery home page
// Distributes .grid-item elements into columns, NEW items first at top of each column.

export function initMasonry(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return null;

    let currentCols = 0;

    function layout() {
        const cols = getColCount(container.clientWidth);
        if (cols === currentCols && container.querySelector('.masonry-col')) return;
        currentCols = cols;
        doLayout(container, cols);
    }

    // Initial layout, then reveal
    layout();
    container.style.opacity = '1';

    // Hide skeleton
    const skeleton = document.querySelector('.skeleton-grid');
    if (skeleton) skeleton.style.display = 'none';

    // Only re-layout when column count changes
    window.addEventListener('resize', () => {
        const cols = getColCount(container.clientWidth);
        if (cols !== currentCols) {
            currentCols = cols;
            doLayout(container, cols);
        }
    });

    return null;
}

// Call from Blazor after filter/search re-renders
export function relayout(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;

    // Fade out existing items
    const allItems = container.querySelectorAll('.grid-item');
    allItems.forEach(el => el.classList.add('fading-out'));

    // After fade-out transition, rearrange and fade back in
    setTimeout(() => {
        const cols = getColCount(container.clientWidth);
        doLayout(container, cols);

        // Fade in with stagger
        const items = container.querySelectorAll('.grid-item');
        items.forEach((el, i) => {
            el.style.transitionDelay = (i * 20) + 'ms';
            // Force reflow before removing class
            void el.offsetHeight;
            el.classList.remove('fading-out');
        });

        // Clean up transition delays after animation
        setTimeout(() => {
            items.forEach(el => el.style.transitionDelay = '');
        }, items.length * 20 + 350);
    }, 250);
}

function getColCount(width) {
    // Use window width for consistent breakpoints (container width varies with scrollbar)
    const w = window.innerWidth;
    if (w < 640) return 1;
    if (w < 960) return 2;
    if (w < 1400) return 3;
    return 4;
}

function doLayout(container, cols) {
    const gap = 16;

    // Pull all grid-items out of any existing column wrappers
    const existingCols = container.querySelectorAll('.masonry-col');
    existingCols.forEach(col => {
        while (col.firstChild) container.appendChild(col.firstChild);
        col.remove();
    });

    const items = Array.from(container.querySelectorAll(':scope > .grid-item'));
    if (items.length === 0) return;

    // Split into NEW and rest
    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const restItems = items.filter(el => el.dataset.isNew !== 'true');

    // Style container
    container.style.display = 'flex';
    container.style.gap = gap + 'px';
    container.style.alignItems = 'flex-start';

    // Create columns
    const columns = [];
    for (let i = 0; i < cols; i++) {
        const col = document.createElement('div');
        col.className = 'masonry-col';
        col.style.flex = '1';
        col.style.display = 'flex';
        col.style.flexDirection = 'column';
        col.style.gap = gap + 'px';
        col.style.minWidth = '0';
        columns.push(col);
        container.appendChild(col);
    }

    // NEW items round-robin
    newItems.forEach((item, i) => columns[i % cols].appendChild(item));

    // Rest items to shortest column
    restItems.forEach(item => {
        let shortest = 0;
        let minH = columns[0].offsetHeight;
        for (let c = 1; c < cols; c++) {
            const h = columns[c].offsetHeight;
            if (h < minH) { minH = h; shortest = c; }
        }
        columns[shortest].appendChild(item);
    });
}

