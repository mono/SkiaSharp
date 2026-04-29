// masonry.js — JS-driven column layout for the Gallery home page
// Distributes .grid-item elements into columns, NEW items first at top of each column.

export function initMasonry(containerSelector) {
    const container = document.querySelector(containerSelector);
    if (!container) return null;

    let layoutInProgress = false;

    const layout = () => {
        if (layoutInProgress) return;
        layoutInProgress = true;
        requestAnimationFrame(() => {
            doLayout(container);
            layoutInProgress = false;
        });
    };

    layout();

    // Re-layout on resize (debounced)
    let timer;
    const observer = new ResizeObserver(() => {
        clearTimeout(timer);
        timer = setTimeout(layout, 150);
    });
    observer.observe(container);

    // Re-layout when Blazor re-renders (new .grid-item children appear)
    const mutObs = new MutationObserver((mutations) => {
        // Only react if grid-items were added directly (Blazor re-render), not our columns
        const hasNewItems = mutations.some(m =>
            Array.from(m.addedNodes).some(n => n.classList && n.classList.contains('grid-item')));
        if (hasNewItems) {
            clearTimeout(timer);
            timer = setTimeout(layout, 50);
        }
    });
    mutObs.observe(container, { childList: true, subtree: false });

    return { dispose: () => { observer.disconnect(); mutObs.disconnect(); } };
}

function doLayout(container) {
    // Collect all grid-items (may be inside .masonry-col wrappers or direct children)
    const existingCols = container.querySelectorAll('.masonry-col');
    const items = [];

    // Pull items out of any existing column wrappers back to container
    existingCols.forEach(col => {
        while (col.firstChild) {
            items.push(col.firstChild);
            container.appendChild(col.firstChild);
        }
        col.remove();
    });

    // Also grab any direct .grid-item children
    container.querySelectorAll(':scope > .grid-item').forEach(el => {
        if (!items.includes(el)) items.push(el);
    });

    if (items.length === 0) return;

    // Determine column count from container width
    const width = container.clientWidth;
    let cols;
    if (width < 640) cols = 1;
    else if (width < 960) cols = 2;
    else if (width < 1200) cols = 3;
    else cols = 4;

    const gap = 16;

    // Split into NEW and rest (preserve Blazor's render order within each group)
    const newItems = items.filter(el => el.dataset.isNew === 'true');
    const restItems = items.filter(el => el.dataset.isNew !== 'true');

    // Style the container
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

    // Distribute NEW items round-robin across columns (each column gets NEW at top)
    newItems.forEach((item, i) => {
        columns[i % cols].appendChild(item);
    });

    // Distribute rest items to shortest column (balanced fill)
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
