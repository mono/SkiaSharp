// Bridge from managed code into Monaco's live JS state. Monaco's
// onDidChangeContent → Accessor.setValue("Text", …) round-trip into the managed
// CodeEditor.Text property is flaky under Uno WASM — the property lags behind
// keystrokes, so reading CodeEditor.Text after the user types returns stale
// sample text. This global pulls the current model value straight from Monaco
// (via the global `monaco` object loaded by Monaco's own AMD loader) so the
// fiddle can fetch fresh text right before compiling.
//
// Editors are sorted by document position so index 0 is whatever appears first
// in the DOM — that matches XAML declaration order in MainPage.xaml: Setup
// editor first (top), Draw editor second (bottom).
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

// Wrap document.addEventListener so any keyboard listener Uno installs at
// document scope ignores events that originate inside a Monaco editor. Uno's
// WASM Skia head listens at the document for keystrokes and dispatches them
// through XAML's focused-element pipeline — that's how 's' / space / etc.
// were leaking into the ComboBox or focused button while the user typed.
//
// We wrap rather than stopPropagation/stopImmediatePropagation because those
// also abort propagation to the *target*, which means Monaco's own textarea
// handlers (the ones that implement Ctrl+C/V, backspace at line boundaries,
// and friends) never fire either. Wrapping skips Uno's handler in particular
// without affecting the dispatch otherwise — Monaco's listeners are at the
// textarea level, not on document, so they're not wrapped here.
//
// Must run before Uno's runtime registers its listener (i.e. while the script
// is still loading; Uno's C# runtime registers listeners later, after the
// WASM module boots).
(function () {
    if (window.__skiaFiddleDocumentAELPatched) return;
    window.__skiaFiddleDocumentAELPatched = true;

    var KEY_EVENTS = ['keydown', 'keyup', 'keypress'];
    var origAdd = document.addEventListener;
    var origRemove = document.removeEventListener;

    document.addEventListener = function (type, handler, options) {
        if (typeof handler === 'function' && KEY_EVENTS.indexOf(type) !== -1) {
            var wrapped = function (e) {
                if (e && e.target && typeof e.target.closest === 'function' &&
                    e.target.closest('.monaco-editor')) {
                    return;
                }
                return handler.apply(this, arguments);
            };
            handler.__skiaFiddleWrapped = wrapped;
            return origAdd.call(this, type, wrapped, options);
        }
        return origAdd.call(this, type, handler, options);
    };

    document.removeEventListener = function (type, handler, options) {
        if (typeof handler === 'function' && handler.__skiaFiddleWrapped) {
            return origRemove.call(this, type, handler.__skiaFiddleWrapped, options);
        }
        return origRemove.call(this, type, handler, options);
    };
})();
