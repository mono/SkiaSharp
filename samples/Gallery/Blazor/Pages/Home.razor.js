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

    // Initial layout
    layout();

    // Only re-layout when column count changes (resize breakpoints)
    window.addEventListener('resize', () => {
        const cols = getColCount(container.clientWidth);
        if (cols !== currentCols) layout();
    });

    return null;
}

// Call from Blazor after filter/search re-renders
export function relayout(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return;
    const cols = getColCount(container.clientWidth);
    doLayout(container, cols);
}

function getColCount(width) {
    if (width < 640) return 1;
    if (width < 960) return 2;
    if (width < 1200) return 3;
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

