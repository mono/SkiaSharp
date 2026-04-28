// Monaco renders into native HTML elements that overlay Uno's Skia-rendered canvas.
// Without this guard, key events typed inside the editor bubble up to Uno's document-
// level listener, which can trigger accelerators (e.g. Run on Enter), shift focus, or
// re-process the keystroke against the focused XAML element. We catch keyboard and
// composition events on each .monaco-editor and stopPropagation so Monaco's own DOM
// handlers fire first (bubble phase) and nothing reaches Uno.
(function () {
    if (window.__skiaFiddleMonacoFocusGuard) return;
    window.__skiaFiddleMonacoFocusGuard = true;

    var EVENTS = [
        'keydown', 'keyup', 'keypress',
        'beforeinput', 'input',
        'compositionstart', 'compositionupdate', 'compositionend',
    ];

    function guard(el) {
        if (el.__focusGuarded) return;
        el.__focusGuarded = true;
        EVENTS.forEach(function (type) {
            el.addEventListener(type, function (e) { e.stopPropagation(); }, false);
        });
    }

    function scan() {
        document.querySelectorAll('.monaco-editor').forEach(guard);
    }

    function start() {
        scan();
        new MutationObserver(scan).observe(document.body, { childList: true, subtree: true });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();
