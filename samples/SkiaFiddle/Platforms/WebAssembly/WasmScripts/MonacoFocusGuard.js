// Bridge from managed code into Monaco's live JS state. Monaco's
// onDidChangeContent → Accessor.setValue("Text", …) round-trip into the managed
// CodeEditor.Text property is flaky under Uno WASM — the property lags behind
// keystrokes, so reading CodeEditor.Text after the user types returns stale
// sample text. This global pulls the current model value straight from Monaco
// (via the global `monaco` object loaded by Monaco's own AMD loader) so the
// fiddle can fetch fresh text right before compiling.
//
// Editors are sorted by document position so index 0 is whatever appears first
// in the DOM — that matches XAML declaration order in MainPage.xaml: Draw
// editor first, Setup second.
globalThis.skiaFiddleGetMonacoValues = function () {
    try {
        if (typeof monaco === 'undefined' || !monaco.editor || typeof monaco.editor.getEditors !== 'function') {
            console.warn('[skiafiddle] monaco.editor.getEditors not yet available');
            return JSON.stringify([]);
        }
        var editors = monaco.editor.getEditors().slice();
        editors.sort(function (a, b) {
            var na = a.getDomNode();
            var nb = b.getDomNode();
            if (!na || !nb) return 0;
            var rel = na.compareDocumentPosition(nb);
            if (rel & Node.DOCUMENT_POSITION_FOLLOWING) return -1;
            if (rel & Node.DOCUMENT_POSITION_PRECEDING) return 1;
            return 0;
        });
        var values = editors.map(function (e) { return e.getValue(); });
        return JSON.stringify(values);
    } catch (err) {
        console.error('[skiafiddle] getMonacoValues failed:', err);
        return JSON.stringify([]);
    }
};

// Stop key events that originate inside a Monaco editor from reaching Uno's
// document-level routing. Uno's WASM Skia head listens at document/body for
// keystrokes and dispatches them through XAML's focused-element pipeline; on
// some characters (notably space and 's') the routing wins over Monaco's own
// textarea handling and the keystroke ends up firing accelerators / button
// clicks instead of typing. We register at document in capture phase so we
// run before Uno's listener (assuming we're attached first), and use
// stopImmediatePropagation to suppress every later document-level capture
// listener — Monaco's own listeners are on its inner textarea, so they fire
// in the target/bubble phase regardless and keep working.
(function () {
    if (window.__skiaFiddleKeyGuard) return;
    window.__skiaFiddleKeyGuard = true;
    var EVENTS = ['keydown', 'keyup', 'keypress'];
    EVENTS.forEach(function (type) {
        document.addEventListener(type, function (e) {
            if (!e.target || typeof e.target.closest !== 'function') return;
            if (!e.target.closest('.monaco-editor')) return;
            e.stopImmediatePropagation();
        }, true);
    });
})();
