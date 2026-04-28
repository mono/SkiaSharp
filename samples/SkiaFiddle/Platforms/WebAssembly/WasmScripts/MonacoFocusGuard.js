// Bridge from managed code into Monaco's live JS state. Monaco's
// onDidChangeContent → Accessor.setValue("Text", …) round-trip into the managed
// CodeEditor.Text property is flaky under Uno WASM — the property lags behind
// keystrokes, so reading CodeEditor.Text after the user types returns stale
// sample text. This global pulls the current model value straight from Monaco
// (via the EditorContext registry that uno-monaco-helpers.js maintains) so the
// fiddle can fetch fresh text right before compiling.
//
// DOM order matches XAML declaration order in MainPage.xaml: index 0 is the
// Draw editor, index 1 is the Setup editor.
globalThis.skiaFiddleGetMonacoValues = function () {
    var values = [];
    document.querySelectorAll('.monaco-editor').forEach(function (el) {
        if (typeof EditorContext === 'undefined') return;
        var ctx = EditorContext.getEditorForElement(el);
        if (ctx && ctx.model) values.push(ctx.model.getValue());
    });
    return JSON.stringify(values);
};
