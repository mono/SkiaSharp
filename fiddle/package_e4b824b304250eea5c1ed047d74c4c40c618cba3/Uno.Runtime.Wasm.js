var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            // Browser WebGPU device bring-up, SkiaSharp-Graphite-style: create the adapter+device in JavaScript
            // (navigator.gpu, awaited as a Task via [JSImport]) and graft the JS GPUDevice into emdawnwebgpu's C
            // handle table via Module.unoWebGpuImportDevice (installed by the emdawn __postset patch). This avoids the
            // in-WASM wgpuInstanceProcessEvents pump, which hangs when driven from a managed call stack on the browser.
            class WebGpuInit {
                // Counts a presented frame until the queue has finished everything submitted so far (the queue is FIFO, so
                // that covers the frame's own work).
                static trackPresentedFrame(queuePtr) {
                    const queue = WebGpuInit.getJsObject(queuePtr);
                    if (!queue || typeof queue.onSubmittedWorkDone !== "function") {
                        return;
                    }
                    WebGpuInit.framesInFlight++;
                    const done = () => { WebGpuInit.framesInFlight--; };
                    queue.onSubmittedWorkDone().then(done, done);
                }
                static presentedFramesInFlight() {
                    return WebGpuInit.framesInFlight;
                }
                // Returns the imported WGPUDevice pointer (as an unsigned int), or 0 on failure. instancePtr is a real
                // wgpuCreateInstance handle created on the managed side; it becomes the imported device's EventSource parent.
                static async createImportedDevice(instancePtr) {
                    var _a, _b;
                    try {
                        const gpu = navigator.gpu;
                        if (!gpu) {
                            console.error("WebGpuInit: navigator.gpu is unavailable");
                            return 0;
                        }
                        const adapter = await gpu.requestAdapter({ powerPreference: "high-performance" });
                        if (!adapter) {
                            console.error("WebGpuInit: navigator.gpu.requestAdapter returned null");
                            return 0;
                        }
                        // Request timestamp-query when the adapter offers it: it is the only way to read REAL GPU pass
                        // durations in the browser (the frame-phase log is CPU-only, so a GPU-bound frame merely shows up
                        // as time blocked in submit/present). Requesting it costs nothing; the queries themselves are
                        // written only when UNO_WEBGPU_GPUTIME=1. Chrome also gates this behind
                        // --enable-dawn-features=allow_unsafe_apis.
                        const requiredFeatures = [];
                        if ((_b = (_a = adapter.features) === null || _a === void 0 ? void 0 : _a.has) === null || _b === void 0 ? void 0 : _b.call(_a, "timestamp-query")) {
                            requiredFeatures.push("timestamp-query");
                        }
                        const device = await adapter.requestDevice({ requiredFeatures });
                        if (!device) {
                            console.error("WebGpuInit: adapter.requestDevice returned null");
                            return 0;
                        }
                        // A lost device otherwise only shows up later as every async operation aborting, with no reason given.
                        device.lost.then((info) => console.error("WebGpuInit: device lost (" + info.reason + "): " + info.message));
                        device.addEventListener("uncapturederror", (e) => { var _a; return console.error("WebGpuInit: uncaptured error: " + ((_a = e.error) === null || _a === void 0 ? void 0 : _a.message)); });
                        const module = window.Module;
                        if (!module || typeof module.unoWebGpuImportDevice !== "function") {
                            console.error("WebGpuInit: Module.unoWebGpuImportDevice is missing (emdawn __postset patch not applied?)");
                            return 0;
                        }
                        const devicePtr = module.unoWebGpuImportDevice(device, instancePtr) >>> 0;
                        console.log("WebGpuInit: imported device ptr=" + devicePtr);
                        return devicePtr;
                    }
                    catch (e) {
                        console.error("WebGpuInit: device creation failed: " + e);
                        return 0;
                    }
                }
                // Returns the live JS object emdawn's handle table holds for a wgpu handle pointer (reverse of import).
                static getJsObject(handlePtr) {
                    const module = window.Module;
                    return module && typeof module.unoWebGpuJsObject === "function" ? module.unoWebGpuJsObject(handlePtr >>> 0) : null;
                }
                // Imports a JS GPUDevice into emdawn's handle table and returns its wgpu WGPUDevice pointer.
                static importDevice(device, instancePtr) {
                    const module = window.Module;
                    if (!module || typeof module.unoWebGpuImportDevice !== "function") {
                        console.error("WebGpuInit.importDevice: Module.unoWebGpuImportDevice is missing");
                        return 0;
                    }
                    const ptr = module.unoWebGpuImportDevice(device, instancePtr) >>> 0;
                    console.log("WebGpuInit: (backend) imported device ptr=" + ptr);
                    return ptr;
                }
                // Creates the offscreen resolve texture in JS (JS-primary) and imports it into emdawn's handle table.
                static createAndImportOffscreenTexture(device, width, height, usage) {
                    const module = window.Module;
                    if (!module || typeof module.unoWebGpuImportTexture !== "function") {
                        console.error("WebGpuInit.createAndImportOffscreenTexture: Module.unoWebGpuImportTexture is missing");
                        return 0;
                    }
                    const tex = device.createTexture({
                        size: { width: width, height: height, depthOrArrayLayers: 1 },
                        format: "rgba8unorm",
                        sampleCount: 1,
                        usage: usage >>> 0,
                    });
                    return module.unoWebGpuImportTexture(tex, 0) >>> 0;
                }
                // Imports a JS GPUTextureView into emdawn's handle table and returns its wgpu view pointer.
                static importTextureView(view, parentPtr) {
                    const module = window.Module;
                    if (!module || typeof module.unoWebGpuImportTextureView !== "function") {
                        console.error("WebGpuInit.importTextureView: Module.unoWebGpuImportTextureView is missing");
                        return 0;
                    }
                    return module.unoWebGpuImportTextureView(view, parentPtr >>> 0) >>> 0;
                }
                // Maps a readback buffer (by its wgpu handle ptr) off the event loop and inspects it as RGBA8 (rows padded to
                // bytesPerRow). Returns the count of non-transparent pixels (alpha != 0), or -1 on failure, logs luminance
                // min/max, and stashes a PNG data-URL of the frame on window.__unoLastFramePng so a headless driver can save
                // an actual screenshot — the only way to observe WebGPU output where SwiftShader can't composite the canvas.
                static async mapReadStats(bufferPtr, w, h, bytesPerRow) {
                    try {
                        const module = window.Module;
                        const buffer = module && typeof module.unoWebGpuJsObject === "function"
                            ? module.unoWebGpuJsObject(bufferPtr) : null;
                        if (!buffer) {
                            console.error("WebGpuInit.mapReadStats: no JS buffer for ptr=" + bufferPtr);
                            return -1;
                        }
                        await buffer.mapAsync(1 /* GPUMapMode.READ */);
                        const src = new Uint8Array(buffer.getMappedRange());
                        const rgba = new Uint8ClampedArray(w * h * 4);
                        let opaque = 0, lumMin = 255, lumMax = 0;
                        for (let y = 0; y < h; y++) {
                            const row = y * bytesPerRow;
                            for (let x = 0; x < w; x++) {
                                const s = row + x * 4, d = (y * w + x) * 4;
                                const r = src[s], g = src[s + 1], b = src[s + 2], a = src[s + 3];
                                rgba[d] = r;
                                rgba[d + 1] = g;
                                rgba[d + 2] = b;
                                rgba[d + 3] = a;
                                if (a !== 0) {
                                    opaque++;
                                }
                                const lum = (r + g + b) / 3 | 0;
                                if (lum < lumMin) {
                                    lumMin = lum;
                                }
                                if (lum > lumMax) {
                                    lumMax = lum;
                                }
                            }
                        }
                        buffer.unmap();
                        try {
                            const canvas = document.createElement("canvas");
                            canvas.width = w;
                            canvas.height = h;
                            canvas.getContext("2d").putImageData(new ImageData(rgba, w, h), 0, 0);
                            window.__unoLastFramePng = canvas.toDataURL("image/png");
                        }
                        catch (e) { /* PNG capture is best-effort */ }
                        console.log("WebGpuInit.mapReadStats: opaque=" + opaque + " lumMin=" + lumMin + " lumMax=" + lumMax);
                        return opaque;
                    }
                    catch (e) {
                        console.error("WebGpuInit.mapReadStats: failed: " + e);
                        return -1;
                    }
                }
                // Maps a readback buffer (by wgpu handle ptr) off the event loop and returns its first byteLen bytes as a
                // base64 string. Used by SnapshotAsync (RenderTargetBitmap) — the only way to complete a GPU->CPU map on
                // WASM's single JS thread. (base64 marshals cleanly as a string; adequate for occasional RTB.)
                static async mapReadBase64(bufferPtr, byteLen) {
                    const module = window.Module;
                    const buffer = module && typeof module.unoWebGpuJsObject === "function"
                        ? module.unoWebGpuJsObject(bufferPtr) : null;
                    if (!buffer) {
                        console.error("WebGpuInit.mapReadBase64: no JS buffer for ptr=" + bufferPtr);
                        return "";
                    }
                    await buffer.mapAsync(1 /* GPUMapMode.READ */);
                    const src = new Uint8Array(buffer.getMappedRange()).subarray(0, byteLen);
                    let binary = "";
                    const chunk = 0x8000;
                    for (let i = 0; i < src.length; i += chunk) {
                        binary += String.fromCharCode.apply(null, src.subarray(i, i + chunk));
                    }
                    buffer.unmap();
                    return btoa(binary);
                }
            }
            WebGpuInit.framesInFlight = 0;
            Runtime.WebGpuInit = WebGpuInit;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class Accessibility {
                static createLiveElement(kind) {
                    const element = document.createElement("div");
                    element.classList.add("uno-aria-live");
                    element.setAttribute("aria-live", kind);
                    return element;
                }
                /**
                 * Emits a diagnostic message to the console, but only when accessibility
                 * debug mode is enabled (see AccessibilityDebugger / enableDebugMode).
                 * Normal runs keep the browser console clean — these traces are only
                 * useful while developing the a11y layer and would otherwise be emitted
                 * on every focus change / DOM mutation, even when accessibility is off.
                 */
                static debugLog(message) {
                    if (Accessibility.debugModeEnabled) {
                        console.debug(message);
                    }
                }
                /**
                 * Same as debugLog, but uses console.warn for fallback/recovery paths
                 * (e.g. an element not yet flushed to the DOM). Gated behind debug mode
                 * so it does not spam the console during normal operation.
                 */
                static debugWarn(message) {
                    if (Accessibility.debugModeEnabled) {
                        console.warn(message);
                    }
                }
                static setup() {
                    Accessibility.debugLog('[A11y] Accessibility.setup() — initializing accessibility subsystem');
                    const browserExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports();
                    // Wire up managed callbacks from WebAssemblyAccessibility.cs
                    const accessibilityExports = browserExports.Uno.UI.Runtime.WebAssemblyAccessibility;
                    this.managedEnableAccessibility = accessibilityExports.EnableAccessibility;
                    this.managedDisableAccessibility = accessibilityExports.DisableAccessibility;
                    this.managedIsAutoEnableAccessibility = accessibilityExports.IsAutoEnableAccessibility;
                    this.managedOnScroll = accessibilityExports.OnScroll;
                    this.managedOnInvoke = accessibilityExports.OnInvoke;
                    this.managedOnToggle = accessibilityExports.OnToggle;
                    this.managedOnRangeValueChange = accessibilityExports.OnRangeValueChange;
                    this.managedOnTextInput = accessibilityExports.OnTextInput;
                    this.managedOnTextSelectionChanged = accessibilityExports.OnTextSelectionChanged;
                    this.managedOnExpandCollapse = accessibilityExports.OnExpandCollapse;
                    this.managedOnSelection = accessibilityExports.OnSelection;
                    this.managedOnFocus = accessibilityExports.OnFocus;
                    this.managedOnBlur = accessibilityExports.OnBlur;
                    this.managedOnSentinelFocus = accessibilityExports.OnFocusSentinel;
                    this.containerElement = document.getElementById("uno-body");
                    // Create live regions for screen reader announcements
                    this.politeElement = Accessibility.createLiveElement("polite");
                    this.assertiveElement = Accessibility.createLiveElement("assertive");
                    this.containerElement.appendChild(this.politeElement);
                    this.containerElement.appendChild(this.assertiveElement);
                    const autoEnable = this.managedIsAutoEnableAccessibility();
                    if (!autoEnable) {
                        Accessibility.addEnableAccessibilityButton();
                    }
                    // Create semantic DOM root container (hidden but accessible).
                    // Uses position:fixed to match the canvas coordinate system (which is also
                    // position:fixed). Width/height:100% ensures the container covers the full
                    // viewport so overflow:hidden doesn't clip semantic elements at 0×0.
                    this.semanticsRoot = document.createElement("div");
                    this.semanticsRoot.id = "uno-semantics-root";
                    this.semanticsRoot.style.position = "fixed";
                    this.semanticsRoot.style.top = "0";
                    this.semanticsRoot.style.left = "0";
                    this.semanticsRoot.style.width = "100%";
                    this.semanticsRoot.style.height = "100%";
                    this.semanticsRoot.style.overflow = "hidden";
                    this.semanticsRoot.style.opacity = "0";
                    this.semanticsRoot.style.pointerEvents = "none";
                    this.semanticsRoot.setAttribute("aria-label", "Application content");
                    this.containerElement.appendChild(this.semanticsRoot);
                    if (autoEnable) {
                        // Auto-enable accessibility without requiring user interaction.
                        // The C# EnableAccessibility() has retry logic for when
                        // Window/RootElement aren't ready yet.
                        Accessibility.debugLog('[A11y] Auto-enabling accessibility (FeatureConfiguration.AutomationPeer.AutoEnableAccessibility = true)');
                        this.managedEnableAccessibility();
                        Runtime.LiveRegion.initialize();
                    }
                }
                /// <summary>
                /// Enables or disables debug mode for the accessibility layer.
                /// When enabled, semantic elements are visible with outlines.
                /// </summary>
                static enableDebugMode(enabled) {
                    this.debugModeEnabled = enabled;
                    if (this.semanticsRoot) {
                        if (enabled) {
                            // Make semantic elements visible for debugging
                            this.semanticsRoot.style.opacity = "1";
                            this.semanticsRoot.style.pointerEvents = "none"; // Don't interfere with canvas clicks
                            this.semanticsRoot.classList.add("uno-a11y-debug");
                            // Apply debug styles to all semantic elements
                            const elements = this.semanticsRoot.querySelectorAll("[id^='uno-semantics-']");
                            elements.forEach((el) => {
                                el.style.outline = "2px solid rgba(0, 255, 0, 0.7)";
                                el.style.backgroundColor = "rgba(0, 255, 0, 0.1)";
                            });
                        }
                        else {
                            // Hide semantic elements again
                            this.semanticsRoot.style.opacity = "0";
                            this.semanticsRoot.style.pointerEvents = "";
                            this.semanticsRoot.classList.remove("uno-a11y-debug");
                            // Remove debug styles
                            const elements = this.semanticsRoot.querySelectorAll("[id^='uno-semantics-']");
                            elements.forEach((el) => {
                                el.style.outline = "";
                                el.style.backgroundColor = "";
                            });
                        }
                    }
                }
                /// <summary>
                /// Gets whether debug mode is currently enabled.
                /// </summary>
                static isDebugModeEnabled() {
                    return this.debugModeEnabled;
                }
                // Callback accessors for SemanticElements.ts
                static getCallbacks() {
                    return {
                        onInvoke: this.managedOnInvoke,
                        onToggle: this.managedOnToggle,
                        onRangeValueChange: this.managedOnRangeValueChange,
                        onTextInput: this.managedOnTextInput,
                        onTextSelectionChanged: this.managedOnTextSelectionChanged,
                        onExpandCollapse: this.managedOnExpandCollapse,
                        onSelection: this.managedOnSelection,
                        onFocus: this.managedOnFocus,
                        onBlur: this.managedOnBlur
                    };
                }
                static createSemanticElement(x, y, width, height, handle, isFocusable) {
                    let element = document.createElement("div");
                    element.style.position = "absolute";
                    element.addEventListener('wheel', (e) => {
                        // When scrolling with wheel, we want to prevent scroll events.
                        e.preventDefault();
                    }, { passive: false });
                    element.addEventListener('scroll', (e) => {
                        let element = e.target;
                        this.managedOnScroll(handle, element.scrollLeft, element.scrollTop);
                    });
                    Accessibility.updateElementFocusability(element, isFocusable);
                    element.style.left = `${x}px`;
                    element.style.top = `${y}px`;
                    element.style.width = `${width}px`;
                    element.style.height = `${height}px`;
                    //element.style.boxShadow = "inset 0px 0px 5px 0px red"; // FOR DEBUGGING ONLY.
                    element.id = `uno-semantics-${handle}`;
                    return element;
                }
                static updateElementFocusability(element, isFocusable) {
                    // Focusable controls participate in the natural tab order (tabindex="0").
                    // Non-focusable controls must NOT participate, but they may still need to be
                    // programmatically focusable (for screen-reader navigation / focus recovery),
                    // so they get tabindex="-1" rather than having the attribute removed —
                    // native <button>/<input>/<a> default to tabbable when no tabindex is set.
                    if (isFocusable) {
                        element.tabIndex = 0;
                    }
                    else {
                        element.tabIndex = -1;
                    }
                    // Semantic elements must NEVER have pointer-events: all.
                    // Mouse events must pass through to the canvas below.
                    // Keyboard focus (Tab) and screen reader navigation work
                    // independently of pointer-events.
                    element.style.pointerEvents = "none";
                    element.style.touchAction = "none";
                }
                static getSemanticElementByHandle(handle) {
                    return document.getElementById(`uno-semantics-${handle}`);
                }
                static announcePolite(text) {
                    Accessibility.announce(Accessibility.politeElement, text);
                }
                static announceAssertive(text) {
                    Accessibility.announce(Accessibility.assertiveElement, text);
                }
                static announce(ariaLiveElement, text) {
                    let child = document.createElement("div");
                    child.innerText = text;
                    ariaLiveElement.appendChild(child);
                    setTimeout(() => {
                        if (child.parentNode === ariaLiveElement) {
                            ariaLiveElement.removeChild(child);
                        }
                    }, 300);
                }
                static addEnableAccessibilityButton() {
                    // Create enable accessibility button (for screen reader activation)
                    this.enableAccessibilityButton = document.createElement("div");
                    this.enableAccessibilityButton.id = "uno-enable-accessibility";
                    this.enableAccessibilityButton.setAttribute("aria-live", "polite");
                    this.enableAccessibilityButton.setAttribute("role", "button");
                    this.enableAccessibilityButton.setAttribute("tabindex", "0");
                    this.enableAccessibilityButton.setAttribute("aria-label", "Enable accessibility");
                    this.enableAccessibilityButton.addEventListener("click", this.onEnableAccessibilityButtonClicked.bind(this));
                    // Also add a keydown listener so keyboard users can activate it via Enter/Space
                    this.enableAccessibilityButton.addEventListener("keydown", (e) => {
                        if (e.key === "Enter" || e.key === " ") {
                            e.preventDefault();
                            this.onEnableAccessibilityButtonClicked(e);
                        }
                    });
                    // Prepend so the button is the first focusable element in the DOM,
                    // reachable by the very first Tab press (inspired by Flutter's
                    // DesktopSemanticsEnabler which prepends its placeholder to <body>).
                    this.containerElement.prepend(this.enableAccessibilityButton);
                }
                /**
                 * Tears the semantic DOM down again (see WebAssemblyAccessibility.DisableAccessibility).
                 * Used by runtime tests so accessibility does not stay on for every test that follows.
                 */
                static disableAccessibility() {
                    this.managedDisableAccessibility();
                }
                /**
                 * Called by the managed side once it has unhooked itself: removes every semantic element,
                 * the focus sentinels and the live regions, and brings the enable button back.
                 */
                static resetSemanticsRoot() {
                    var _a, _b, _c;
                    Runtime.SemanticElements.resetVirtualizedMutations();
                    while ((_a = this.semanticsRoot) === null || _a === void 0 ? void 0 : _a.firstChild) {
                        this.semanticsRoot.removeChild(this.semanticsRoot.firstChild);
                    }
                    (_b = this.focusSentinelStart) === null || _b === void 0 ? void 0 : _b.remove();
                    (_c = this.focusSentinelEnd) === null || _c === void 0 ? void 0 : _c.remove();
                    this.focusSentinelStart = null;
                    this.focusSentinelEnd = null;
                    this.isDepartingFocus = false;
                    Runtime.LiveRegion.teardown();
                    if (!this.managedIsAutoEnableAccessibility() && !Accessibility.isEnableAccessibilityButtonActive()) {
                        Accessibility.addEnableAccessibilityButton();
                    }
                }
                /**
                 * Returns true if the "Enable Accessibility" button is still in the DOM
                 * (i.e. accessibility has not yet been activated by the user).
                 */
                static isEnableAccessibilityButtonActive() {
                    return document.getElementById("uno-enable-accessibility") !== null;
                }
                static onEnableAccessibilityButtonClicked(evt) {
                    this.containerElement.removeChild(this.enableAccessibilityButton);
                    this.managedEnableAccessibility();
                    // Initialize subsystem TypeScript modules
                    Runtime.LiveRegion.initialize();
                    this.announceAssertive("Accessibility enabled successfully.");
                }
                /**
                 * Focuses a semantic element by handle.
                 * If the element isn't in the DOM yet (timing issue from batched mutations),
                 * retries once after a requestAnimationFrame. This handles the case where
                 * C# fires focus synchronously but the JS DOM mutation hasn't been flushed yet.
                 */
                static focusSemanticElement(handle) {
                    // XAML owns scrolling: a browser scroll of a semantic scroller would be fed back
                    // into the ScrollViewer through the scroll listener.
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.focus({ preventScroll: true });
                    }
                    else {
                        // Element might not be in DOM yet due to batched/deferred mutations.
                        // Retry once after the next animation frame.
                        requestAnimationFrame(() => {
                            const retryElement = Accessibility.getSemanticElementByHandle(handle);
                            if (retryElement) {
                                retryElement.focus({ preventScroll: true });
                            }
                            else {
                                Accessibility.debugWarn(`[A11y] TS focusSemanticElement: element NOT FOUND handle=${handle} (after retry)`);
                            }
                        });
                    }
                }
                /**
                 * Blurs a semantic element.
                 */
                static blurSemanticElement(handle) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.blur();
                    }
                }
                static installFocusSentinels() {
                    if (!this.focusSentinelStart) {
                        this.focusSentinelStart = Accessibility.createFocusSentinel("uno-focus-sentinel-start", true);
                    }
                    if (!this.focusSentinelEnd) {
                        this.focusSentinelEnd = Accessibility.createFocusSentinel("uno-focus-sentinel-end", false);
                    }
                    document.body.insertBefore(this.focusSentinelStart, document.body.firstChild);
                    document.body.appendChild(this.focusSentinelEnd);
                }
                static createFocusSentinel(id, isStart) {
                    const sentinel = document.createElement("div");
                    sentinel.id = id;
                    sentinel.tabIndex = 0;
                    sentinel.setAttribute("aria-hidden", "true");
                    sentinel.style.position = "fixed";
                    sentinel.style.width = "1px";
                    sentinel.style.height = "1px";
                    sentinel.style.padding = "0";
                    sentinel.style.margin = "-1px";
                    sentinel.style.overflow = "hidden";
                    sentinel.style.opacity = "0";
                    sentinel.style.pointerEvents = "none";
                    sentinel.style.top = "0";
                    sentinel.style.left = "0";
                    sentinel.addEventListener("focus", () => {
                        if (this.isDepartingFocus) {
                            return;
                        }
                        // Defer: browsers revert focus changes made synchronously inside a focus handler.
                        setTimeout(() => {
                            if (this.managedOnSentinelFocus) {
                                this.managedOnSentinelFocus(isStart);
                            }
                        }, 0);
                    });
                    return sentinel;
                }
                static focusDepartureSentinel(isForward) {
                    const sentinel = isForward ? this.focusSentinelEnd : this.focusSentinelStart;
                    if (!sentinel) {
                        return;
                    }
                    this.isDepartingFocus = true;
                    sentinel.focus();
                    setTimeout(() => { this.isDepartingFocus = false; }, 0);
                }
                static removeFocusSentinels() {
                    var _a, _b;
                    (_a = this.focusSentinelStart) === null || _a === void 0 ? void 0 : _a.remove();
                    (_b = this.focusSentinelEnd) === null || _b === void 0 ? void 0 : _b.remove();
                    this.focusSentinelStart = null;
                    this.focusSentinelEnd = null;
                }
                /**
                 * Updates roving tabindex within an ARIA widget group.
                 * Sets tabindex="0" on the active element and tabindex="-1" on
                 * other members of the same group. Only affects elements that
                 * belong to the same ARIA group (e.g., radio buttons sharing the
                 * same 'name' attribute), NOT all siblings.
                 *
                 * If groupHandle is 0, infers the group from the active element's
                 * 'name' attribute (radio buttons) or 'role' (tablist children).
                 * If no group can be inferred, does nothing — general focus
                 * management should not strip tabindex from unrelated elements.
                 */
                static updateRovingTabindex(groupHandle, activeHandle) {
                    const activeElement = Accessibility.getSemanticElementByHandle(activeHandle);
                    if (!activeElement) {
                        return;
                    }
                    // Promote the active element to the single tab stop (tabindex="0").
                    // Sibling group members are demoted to tabindex="-1" below.
                    activeElement.tabIndex = 0;
                    // Determine the group scope. Only radio buttons (sharing the
                    // same 'name') and tab-role children of a tablist are grouped.
                    const parent = activeElement.parentElement;
                    if (!parent) {
                        return;
                    }
                    let groupSelector = null;
                    if (activeElement instanceof HTMLInputElement &&
                        activeElement.type === 'radio' &&
                        activeElement.name) {
                        // Radio group: only affect radios with the same name
                        groupSelector = `input[type="radio"][name="${activeElement.name}"]`;
                    }
                    else if (activeElement.getAttribute('role') === 'tab' &&
                        parent.getAttribute('role') === 'tablist') {
                        // Tablist group: only affect tab-role children
                        groupSelector = '[role="tab"]';
                    }
                    else if (activeElement.getAttribute('role') === 'option' &&
                        parent.getAttribute('role') === 'listbox') {
                        // Listbox group: only affect option-role children
                        groupSelector = '[role="option"]';
                    }
                    else if (activeElement.getAttribute('role') === 'menuitem' &&
                        parent.getAttribute('role') === 'menu') {
                        // Menu group: only affect menuitem-role children
                        groupSelector = '[role="menuitem"]';
                    }
                    else if (activeElement.getAttribute('role') === 'treeitem') {
                        // Tree group: affect treeitem-role siblings at same level
                        groupSelector = '[role="treeitem"]';
                    }
                    if (!groupSelector) {
                        // No recognized ARIA group — do not touch sibling tabindexes.
                        // General focus management relies on natural tab order.
                        return;
                    }
                    // Only modify tabindex on elements within the same group
                    const groupMembers = parent.querySelectorAll(groupSelector);
                    groupMembers.forEach((member) => {
                        if (member !== activeElement && member.tabIndex === 0) {
                            member.tabIndex = -1;
                        }
                    });
                }
                static addRootElementToSemanticsRoot(rootHandle, width, height, x, y, isFocusable) {
                    Accessibility.debugLog(`[A11y] addRootElementToSemanticsRoot: handle=${rootHandle} size=${width}x${height} pos=(${x},${y}) focusable=${isFocusable}`);
                    let element = Accessibility.createSemanticElement(x, y, width, height, rootHandle, isFocusable);
                    this.semanticsRoot.appendChild(element);
                }
                static addSemanticElement(parentHandle, handle, index, width, height, x, y, role, ariaLabel, isFocusable, ariaChecked, isVisible, horizontallyScrollable, verticallyScrollable, temporary, xamlAutomationId) {
                    // Remove any pre-existing element with this handle to prevent duplicates
                    const existing = document.getElementById(`uno-semantics-${handle}`);
                    if (existing) {
                        existing.remove();
                    }
                    let parent = Accessibility.getSemanticElementByHandle(parentHandle);
                    if (!parent) {
                        // Fall back to the semantics root instead of failing.
                        // This matches the behavior of the SemanticElements factory path
                        // and ensures elements still appear in the accessibility tree
                        // even when their semantic parent was pruned.
                        Accessibility.debugWarn(`[A11y] addSemanticElement: PARENT NOT FOUND — handle=${handle} parentHandle=${parentHandle} controlType='${temporary}' role='${role}' label='${ariaLabel}'. Falling back to semanticsRoot.`);
                        parent = this.semanticsRoot;
                        if (!parent) {
                            Accessibility.debugWarn(`[A11y] addSemanticElement: semanticsRoot also null. Element will NOT appear in semantic tree.`);
                            return false;
                        }
                    }
                    Accessibility.debugLog(`[A11y] addSemanticElement: handle=${handle} parentHandle=${parentHandle} controlType='${temporary}' role='${role}' label='${ariaLabel}' size=${width}x${height} pos=(${x},${y}) focusable=${isFocusable} visible=${isVisible}`);
                    let element = Accessibility.createSemanticElement(x, y, width, height, handle, isFocusable);
                    element.setAttribute('ElementType', temporary);
                    if (!isVisible) {
                        element.hidden = true;
                    }
                    if (role) {
                        element.setAttribute("role", role);
                    }
                    if (ariaChecked) {
                        element.setAttribute("aria-checked", ariaChecked);
                    }
                    // ariaLabel is the *aria-label* source (peer-resolved Name / AutomationProperties.Name)
                    // and is intentionally NOT the AutomationId — keeping the parameter named for what it
                    // becomes guards against a future regression where AutomationId leaks back into the
                    // accessible name. xamlAutomationId below is the test/dev identifier and never an AT name.
                    if (ariaLabel && ariaLabel.trim().length > 0) {
                        element.setAttribute("aria-label", ariaLabel.trim());
                    }
                    if (xamlAutomationId && xamlAutomationId.trim().length > 0) {
                        element.setAttribute("xamlautomationid", xamlAutomationId);
                    }
                    if (horizontallyScrollable) {
                        element.style.overflowX = "scroll";
                    }
                    if (verticallyScrollable) {
                        element.style.overflowY = "scroll";
                    }
                    if (index != null && index < parent.childElementCount) {
                        parent.insertBefore(element, parent.children[index]);
                    }
                    else {
                        parent.appendChild(element);
                    }
                    return true;
                }
                static removeSemanticElement(parentHandle, childHandle) {
                    const child = Accessibility.getSemanticElementByHandle(childHandle);
                    if (!child) {
                        Accessibility.debugWarn(`[A11y] removeSemanticElement: child handle=${childHandle} not found in DOM (parent=${parentHandle})`);
                        return;
                    }
                    Accessibility.debugLog(`[A11y] removeSemanticElement: parent=${parentHandle} child=${childHandle}`);
                    // Use child.remove() instead of parent.removeChild(child) to handle
                    // cases where the child's actual DOM parent differs from the semantic parent
                    // (e.g., after re-parenting or when duplicate IDs existed previously).
                    child.remove();
                }
                static updateIsFocusable(handle, isFocusable) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        Accessibility.debugLog(`[A11y] TS updateIsFocusable: handle=${handle} focusable=${isFocusable}`);
                        Accessibility.updateElementFocusability(element, isFocusable);
                    }
                    // Silently skip if element doesn't exist in the semantic DOM.
                    // Many controls get IsFocusable updates but aren't in the semantic
                    // tree (pruned as non-semantic). This is expected.
                }
                static setXamlAutomationId(handle, automationId) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Mirror updateAriaLabel/setAriaStringAttribute: normalize on write by setting the
                        // trimmed value (and removing the attribute when empty) so live-sync matches the
                        // creation-time path and we never persist leading/trailing whitespace.
                        const trimmed = automationId ? automationId.trim() : "";
                        if (trimmed.length > 0) {
                            element.setAttribute("xamlautomationid", trimmed);
                        }
                        else {
                            element.removeAttribute("xamlautomationid");
                        }
                    }
                }
                static updateAriaLabel(handle, automationId) {
                    Accessibility.debugLog(`[A11y] TS updateAriaLabel: handle=${handle} label='${automationId}'`);
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Omit an empty/whitespace aria-label rather than emitting aria-label="" (which screen
                        // readers announce as "blank"); a nameless control must carry no aria-label attribute.
                        // Write the TRIMMED value so live-sync matches the creation-time path
                        // (setAriaStringAttribute) and never persists leading/trailing whitespace.
                        const trimmed = automationId ? automationId.trim() : "";
                        // WA-04: aria-labelledby takes ARIA precedence over aria-label. Never set a competing
                        // aria-label when the element is already named by aria-labelledby (order-independent
                        // with the aria-label removal in updateAriaLabelledBy) — this covers the case where a
                        // late live-update re-applies the name after the labelledby drain.
                        if (trimmed.length > 0 && !element.hasAttribute("aria-labelledby")) {
                            element.setAttribute("aria-label", trimmed);
                        }
                        else {
                            element.removeAttribute("aria-label");
                        }
                    }
                }
                /**
                 * Updates aria-description on a semantic element.
                 * VoiceOver reads this as secondary context after the name.
                 * Falls back to title attribute for broader browser compatibility.
                 */
                static updateAriaDescription(handle, description) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Use aria-description (modern) with title fallback (wider support)
                        element.setAttribute("aria-description", description);
                        element.title = description;
                    }
                }
                /**
                 * Updates the ARIA landmark role on a semantic element.
                 * VoiceOver rotor uses landmarks (main, navigation, search, etc.) for quick navigation.
                 */
                static updateLandmarkRole(handle, role) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (role) {
                            element.setAttribute("role", role);
                        }
                        else {
                            element.removeAttribute("role");
                        }
                    }
                }
                /**
                 * Updates aria-roledescription on a semantic element.
                 * Provides a human-readable description of the role for VoiceOver.
                 */
                static updateAriaRoleDescription(handle, roleDescription) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (roleDescription) {
                            element.setAttribute("aria-roledescription", roleDescription);
                        }
                        else {
                            element.removeAttribute("aria-roledescription");
                        }
                    }
                }
                static updateAriaLevel(handle, level) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (level > 0) {
                            element.setAttribute("aria-level", String(level));
                        }
                        else {
                            element.removeAttribute("aria-level");
                        }
                    }
                }
                static updatePositionInSet(handle, positionInSet, sizeOfSet) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.setAttribute("aria-posinset", String(positionInSet));
                        element.setAttribute("aria-setsize", String(sizeOfSet));
                    }
                }
                /**
                 * Updates aria-required on a semantic element.
                 * Screen readers announce the field as "required".
                 */
                static updateAriaRequired(handle, required) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (required) {
                            element.setAttribute("aria-required", "true");
                            // Also set the native required attribute for input elements
                            if (element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement) {
                                element.required = true;
                            }
                        }
                        else {
                            element.removeAttribute("aria-required");
                            if (element instanceof HTMLInputElement || element instanceof HTMLTextAreaElement) {
                                element.required = false;
                            }
                        }
                    }
                }
                /**
                 * Updates aria-invalid on a semantic element.
                 * Screen readers announce the field as "invalid" when its value fails form validation.
                 */
                static updateAriaInvalid(handle, invalid) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (invalid) {
                            element.setAttribute("aria-invalid", "true");
                        }
                        else {
                            element.removeAttribute("aria-invalid");
                        }
                    }
                }
                /**
                 * Updates aria-pressed on a toggle button semantic element.
                 */
                static updateAriaPressed(handle, pressed) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.setAttribute("aria-pressed", pressed);
                    }
                }
                /**
                 * Updates aria-keyshortcuts on a semantic element.
                 * Screen readers announce the formatted shortcut (e.g. "Ctrl+S") alongside the name.
                 */
                static updateAriaKeyShortcuts(handle, keyShortcuts) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (keyShortcuts) {
                            element.setAttribute("aria-keyshortcuts", keyShortcuts);
                        }
                        else {
                            element.removeAttribute("aria-keyshortcuts");
                        }
                    }
                }
                /**
                 * Updates aria-haspopup on a semantic element from the C# value (FR-028).
                 * The popup kind ("listbox", "menu", "dialog", …) is decided in C# from the control's
                 * ExpandCollapse pattern / control type, never hardcoded in TS.
                 */
                static updateAriaHasPopup(handle, hasPopup) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Mirror the omit-when-empty contract: aria-haspopup=" " is a malformed token
                        // that AT either rejects or treats as "true"; both are wrong. Trim and omit.
                        const trimmed = hasPopup ? hasPopup.trim() : "";
                        if (trimmed.length > 0) {
                            element.setAttribute("aria-haspopup", trimmed);
                        }
                        else {
                            element.removeAttribute("aria-haspopup");
                        }
                    }
                }
                /**
                 * Updates the HTML accesskey attribute on a semantic element (FR-028).
                 * Sourced from AutomationProperties.AccessKey (a mnemonic, e.g. "F"). This is distinct
                 * from aria-keyshortcuts (the AcceleratorKey activation shortcut).
                 */
                static setAccessKey(handle, accessKey) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Mirror the omit-when-empty contract used by updateAriaLabel / setXamlAutomationId:
                        // whitespace-only values would emit accesskey=" ", which is meaningless to AT and
                        // can interfere with browser shortcut handling.
                        const trimmed = accessKey ? accessKey.trim() : "";
                        if (trimmed.length > 0) {
                            element.setAttribute("accesskey", trimmed);
                        }
                        else {
                            element.removeAttribute("accesskey");
                        }
                    }
                }
                /**
                 * Updates aria-modal on a semantic element.
                 * Used for dialogs that should scope screen reader announcements.
                 */
                static updateAriaModal(handle, modal) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (modal) {
                            element.setAttribute("aria-modal", "true");
                        }
                        else {
                            element.removeAttribute("aria-modal");
                        }
                    }
                }
                /**
                 * Updates aria-busy on a semantic element.
                 * Mapped from AutomationProperties.ItemStatus when the status indicates the
                 * element is busy/loading, so screen readers suppress reading transient content.
                 */
                static updateAriaBusy(handle, busy) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (busy) {
                            element.setAttribute("aria-busy", "true");
                        }
                        else {
                            element.removeAttribute("aria-busy");
                        }
                    }
                }
                /**
                 * Updates the lang attribute on a semantic element.
                 * Mapped from AutomationProperties.Culture so screen readers pronounce the
                 * content using the correct locale.
                 */
                static updateLang(handle, lang) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // Mirror the omit-when-empty contract: lang=" " is invalid per the HTML/BCP-47
                        // language-tag grammar and would make AT fall back unpredictably. Trim and omit.
                        const trimmed = lang ? lang.trim() : "";
                        if (trimmed.length > 0) {
                            element.setAttribute("lang", trimmed);
                        }
                        else {
                            element.removeAttribute("lang");
                        }
                    }
                }
                /**
                 * Updates aria-live on a semantic element for live region announcements.
                 * Screen readers monitor elements with aria-live for content changes.
                 */
                static updateAriaLive(handle, ariaLive) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        // aria-atomic is intentionally NOT forced here (FR-028). Defaulting every live
                        // region to aria-atomic="true" makes screen readers re-announce the entire region
                        // on any change; the browser default (false — announce only changed nodes) is
                        // correct for the common status/log case. A region whose WinUI semantics require
                        // atomic announcement must opt in explicitly elsewhere.
                        element.setAttribute("aria-live", ariaLive);
                    }
                }
                /**
                 * Updates aria-describedby on a semantic element.
                 * References other semantic elements by their IDs (space-separated).
                 */
                /**
                 * Updates aria-labelledby on a semantic element.
                 * References the labeling element by its DOM ID.
                 */
                static updateAriaLabelledBy(handle, idList) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (idList) {
                            element.setAttribute("aria-labelledby", idList);
                            // WA-04: aria-labelledby takes ARIA precedence over aria-label. Remove any competing
                            // aria-label so the element is not named twice — this also handles the two-phase
                            // build where aria-label was applied before the labeller's semantic node existed.
                            element.removeAttribute("aria-label");
                        }
                        else {
                            element.removeAttribute("aria-labelledby");
                        }
                    }
                }
                static updateAriaDescribedBy(handle, idList) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (idList) {
                            element.setAttribute("aria-describedby", idList);
                        }
                        else {
                            element.removeAttribute("aria-describedby");
                        }
                    }
                }
                /**
                 * Updates aria-controls on a semantic element.
                 * References other semantic elements by their IDs (space-separated).
                 */
                static updateAriaControls(handle, idList) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (idList) {
                            element.setAttribute("aria-controls", idList);
                        }
                        else {
                            element.removeAttribute("aria-controls");
                        }
                    }
                }
                /**
                 * Updates aria-flowto on a semantic element.
                 * Defines the next element(s) in an alternative reading order.
                 */
                static updateAriaFlowTo(handle, idList) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        if (idList) {
                            element.setAttribute("aria-flowto", idList);
                        }
                        else {
                            element.removeAttribute("aria-flowto");
                        }
                    }
                }
                static updateAriaChecked(handle, ariaChecked) {
                    Accessibility.debugLog(`[A11y] TS updateAriaChecked: handle=${handle} checked=${ariaChecked}`);
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.setAttribute("aria-checked", ariaChecked);
                        // Also update native checkbox/radio checked property if applicable
                        if (element instanceof HTMLInputElement &&
                            (element.type === 'checkbox' || element.type === 'radio')) {
                            if (ariaChecked === 'true') {
                                element.checked = true;
                                element.indeterminate = false;
                            }
                            else if (ariaChecked === 'mixed') {
                                element.indeterminate = true;
                            }
                            else {
                                element.checked = false;
                                element.indeterminate = false;
                            }
                        }
                    }
                }
                static updateNativeScrollOffsets(handle, horizontalOffset, verticalOffset) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.scrollLeft = horizontalOffset;
                        element.scrollTop = verticalOffset;
                    }
                }
                static hideSemanticElement(handle) {
                    Accessibility.debugLog(`[A11y] TS hideSemanticElement: handle=${handle}`);
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.hidden = true;
                    }
                }
                static updateSemanticElementPositioning(handle, width, height, x, y) {
                    const element = Accessibility.getSemanticElementByHandle(handle);
                    if (element) {
                        element.hidden = false;
                        element.style.left = `${x}px`;
                        element.style.top = `${y}px`;
                        element.style.width = `${width}px`;
                        element.style.height = `${height}px`;
                    }
                }
                /**
                 * Updates the debug overlay panel with performance metrics and subsystem state.
                 * Called from C# AccessibilityDebugger when debug mode is enabled.
                 */
                static updateDebugOverlay(avgFrameOverheadMs, totalFrames, modalState) {
                    var _a;
                    if (!this.debugModeEnabled) {
                        if (this.debugOverlayElement) {
                            this.debugOverlayElement.remove();
                            this.debugOverlayElement = null;
                        }
                        return;
                    }
                    if (!this.debugOverlayElement) {
                        this.debugOverlayElement = document.createElement("div");
                        this.debugOverlayElement.id = "uno-a11y-debug-overlay";
                        this.debugOverlayElement.style.cssText =
                            "position:fixed;top:10px;right:10px;background:rgba(0,0,0,0.85);color:#0f0;" +
                                "font:12px monospace;padding:10px;border-radius:4px;z-index:99999;" +
                                "pointer-events:none;max-width:350px;";
                        document.body.appendChild(this.debugOverlayElement);
                    }
                    // Count semantic elements
                    const semanticCount = this.semanticsRoot
                        ? this.semanticsRoot.querySelectorAll("[id^='uno-semantics-']").length
                        : 0;
                    // Count virtualized containers
                    const virtualizedContainers = this.semanticsRoot
                        ? this.semanticsRoot.querySelectorAll("[role='listbox'], [role='grid']").length
                        : 0;
                    // Get active element info
                    const activeEl = document.activeElement;
                    const focusInfo = activeEl && ((_a = activeEl.id) === null || _a === void 0 ? void 0 : _a.startsWith("uno-semantics-"))
                        ? activeEl.id.replace("uno-semantics-", "")
                        : "none";
                    this.debugOverlayElement.innerHTML =
                        `<b>A11y Debug</b><br>` +
                            `Elements: ${semanticCount}<br>` +
                            `Avg frame: ${avgFrameOverheadMs.toFixed(2)}ms (${totalFrames} frames)<br>` +
                            `Virtualized containers: ${virtualizedContainers}<br>` +
                            `Focus: ${focusInfo}<br>` +
                            `Modal: ${modalState}`;
                }
            }
            Accessibility.debugModeEnabled = false;
            Accessibility.focusSentinelStart = null;
            Accessibility.focusSentinelEnd = null;
            Accessibility.isDepartingFocus = false;
            Accessibility.debugOverlayElement = null;
            Runtime.Accessibility = Accessibility;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var ApplicationModel;
    (function (ApplicationModel) {
        var DataTransfer;
        (function (DataTransfer) {
            var DragDrop;
            (function (DragDrop) {
                var Core;
                (function (Core) {
                    class BrowserDragDropExtension {
                        static async init() {
                            BrowserDragDropExtension._dispatchDropEventMethod = (await window.Module.getAssemblyExports("Uno.UI.Runtime.WebAssembly.Browser")).Uno.UI.Runtime.BrowserDragDropExtension.OnNativeDropEvent;
                            // Events fired on the drop target
                            // Note: dragenter and dragover events will enable drop on the app
                            document.addEventListener("dragenter", BrowserDragDropExtension.onDragDropEvent);
                            document.addEventListener("dragover", BrowserDragDropExtension.onDragDropEvent);
                            document.addEventListener("dragleave", BrowserDragDropExtension.onDragDropEvent); // Seems to be raised also on drop?
                            document.addEventListener("drop", BrowserDragDropExtension.onDragDropEvent);
                            // #18854: Prevent the browser default selection drag preview.
                            document.addEventListener('dragstart', e => e.preventDefault());
                        }
                        static onDragDropEvent(evt) {
                            if (evt.type == "dragleave"
                                && evt.clientX > 0
                                && evt.clientX < document.documentElement.clientWidth
                                && evt.clientY > 0
                                && evt.clientY < document.documentElement.clientHeight) {
                                // We ignore all dragleave events if the pointer is still over the window.
                                // This is to mute bubbling of drag leave when crossing boundaries of any elements on the app.
                                return;
                            }
                            // We use the dataItems only for enter, no needs to copy them every time!
                            let dataItems = "";
                            let allowedOperations = "";
                            if (evt.type == "dragenter") {
                                if (BrowserDragDropExtension._pendingDropId > 0) {
                                    // For the same reason as above, we ignore all dragenter if there is already a pending active drop
                                    return;
                                }
                                BrowserDragDropExtension._pendingDropId = ++BrowserDragDropExtension._nextDropId;
                                const items = new Array();
                                for (let itemId = 0; itemId < evt.dataTransfer.items.length; itemId++) {
                                    const item = evt.dataTransfer.items[itemId];
                                    items.push({ id: itemId, kind: item.kind, type: item.type });
                                }
                                dataItems = JSON.stringify(items);
                                allowedOperations = evt.dataTransfer.effectAllowed;
                            }
                            else if (evt.type == "drop") {
                                // Make sure to get **ALL** items content **before** returning from drop
                                // (data.items and each instance of item will be cleared)
                                BrowserDragDropExtension._idToContent.set(BrowserDragDropExtension._pendingDropId, BrowserDragDropExtension.beginRetrieveItems(evt.dataTransfer));
                            }
                            try {
                                const acceptedOperation = BrowserDragDropExtension._dispatchDropEventMethod(evt.type, allowedOperations, evt.dataTransfer.dropEffect, dataItems, evt.timeStamp, evt.clientX, evt.clientY, BrowserDragDropExtension._pendingDropId, evt.buttons, evt.shiftKey, evt.ctrlKey, evt.altKey);
                                evt.dataTransfer.dropEffect = acceptedOperation;
                            }
                            finally {
                                // No matter if the managed code handled the event, we want to prevent the default behavior (like opening a drop link)
                                evt.preventDefault();
                                if (evt.type == "dragleave" || evt.type == "drop") {
                                    BrowserDragDropExtension._pendingDropId = 0;
                                }
                            }
                        }
                        static beginRetrieveItems(data) {
                            const promises = [];
                            for (let i = 0; i < data.items.length; i++) {
                                if (data.items[i].kind == "string") {
                                    promises.push(BrowserDragDropExtension.getText(data.items[i]));
                                }
                                else {
                                    promises.push(BrowserDragDropExtension.getAsFile(data.items[i]));
                                }
                            }
                            return promises;
                        }
                        static retrieveText(pendingDropId, itemId) {
                            const data = BrowserDragDropExtension._idToContent.get(pendingDropId);
                            if (!data) {
                                throw new Error(`retrieveFiles failed to find pending drag and drop data for id ${pendingDropId}.`);
                            }
                            return data[itemId];
                        }
                        static async retrieveFiles(pendingDropId, itemIds) {
                            const data = BrowserDragDropExtension._idToContent.get(pendingDropId);
                            if (!data) {
                                throw new Error(`retrieveFiles failed to find pending drag and drop data for id ${pendingDropId}.`);
                            }
                            const selected = Array.from(itemIds).map(i => data[i]);
                            const fileHandles = await Promise.all(selected);
                            const infos = Uno.Storage.NativeStorageItem.getInfos(...fileHandles);
                            return JSON.stringify(infos);
                        }
                        static removeId(id) {
                            BrowserDragDropExtension._idToContent.delete(id);
                        }
                        static async getAsFile(item) {
                            if (item.getAsFileSystemHandle) {
                                return await item.getAsFileSystemHandle();
                            }
                            else {
                                return item.getAsFile();
                            }
                        }
                        static getText(item) {
                            return new Promise((resolve, reject) => {
                                const timeout = setTimeout(() => reject("Timeout: for security reason, you cannot access data before drop."), 15000);
                                item.getAsString(str => {
                                    clearTimeout(timeout);
                                    resolve(str);
                                });
                            });
                        }
                    }
                    BrowserDragDropExtension._nextDropId = 0;
                    BrowserDragDropExtension._idToContent = new Map();
                    Core.BrowserDragDropExtension = BrowserDragDropExtension;
                })(Core = DragDrop.Core || (DragDrop.Core = {}));
            })(DragDrop = DataTransfer.DragDrop || (DataTransfer.DragDrop = {}));
        })(DataTransfer = ApplicationModel.DataTransfer || (ApplicationModel.DataTransfer = {}));
    })(ApplicationModel = Windows.ApplicationModel || (Windows.ApplicationModel = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class BrowserInputHelper {
                static setBrowserZoomEnabled(enabled) {
                    BrowserInputHelper.isBrowserZoomEnabled = enabled;
                }
                static async lockKeys(keyCodes) {
                    if (!BrowserInputHelper.isKeyboardLockSupported()) {
                        throw new Error("Keyboard lock is not supported by this browser.");
                    }
                    const kb = navigator.keyboard;
                    await kb.lock(keyCodes.length > 0 ? keyCodes : undefined);
                }
                static isKeyboardLockSupported() {
                    const kb = navigator.keyboard;
                    return !!kb && typeof kb.lock === "function" && typeof kb.unlock === "function";
                }
                static unlockKeys() {
                    var _a;
                    (_a = navigator.keyboard) === null || _a === void 0 ? void 0 : _a.unlock();
                }
            }
            // Read by BrowserPointerInputSource.onPointerEventReceived
            BrowserInputHelper.isBrowserZoomEnabled = true;
            Runtime.BrowserInputHelper = BrowserInputHelper;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var _a, _b, _c;
            class BrowserInvisibleTextBoxViewExtension {
                static getNativePasteSourceLimit() {
                    BrowserInvisibleTextBoxViewExtension.initialize();
                    return BrowserInvisibleTextBoxViewExtension._exports.GetNativePasteSourceLimit();
                }
                static onNativePaste(source) {
                    BrowserInvisibleTextBoxViewExtension.initialize();
                    BrowserInvisibleTextBoxViewExtension._exports.OnNativePaste(source);
                }
                static initialize() {
                    if (BrowserInvisibleTextBoxViewExtension._exports == undefined) {
                        const browserExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports();
                        BrowserInvisibleTextBoxViewExtension._exports = browserExports.Uno.UI.Runtime.BrowserInvisibleTextBoxViewExtension;
                        BrowserInvisibleTextBoxViewExtension._imeExports = browserExports.Uno.UI.Runtime.WasmImeTextBoxExtension;
                        BrowserInvisibleTextBoxViewExtension.installTrailingClickGuard();
                        document.onselectionchange = () => {
                            let input = document.activeElement;
                            if (input instanceof HTMLInputElement || input instanceof HTMLTextAreaElement) {
                                BrowserInvisibleTextBoxViewExtension.isInSelectionChange = true;
                                if (BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange) {
                                    BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange = false;
                                    input.setSelectionRange(BrowserInvisibleTextBoxViewExtension.nextSelectionStart, BrowserInvisibleTextBoxViewExtension.nextSelectionEnd, BrowserInvisibleTextBoxViewExtension.nextSelectionDirection);
                                }
                                else {
                                    if (input.selectionDirection == "backward") {
                                        BrowserInvisibleTextBoxViewExtension._exports.OnSelectionChanged(input.selectionEnd, input.selectionStart - input.selectionEnd);
                                    }
                                    else {
                                        BrowserInvisibleTextBoxViewExtension._exports.OnSelectionChanged(input.selectionStart, input.selectionEnd - input.selectionStart);
                                    }
                                }
                                BrowserInvisibleTextBoxViewExtension.isInSelectionChange = false;
                            }
                        };
                    }
                }
                // Neutralizes the touch-only, WebKit-internal race that dismisses the soft keyboard right
                // after a TextBox tap (see swallowNextCanvasClick). Uno already preventDefaults pointer
                // events, but per the Pointer Events spec that does NOT suppress the compatibility
                // mouse events touch browsers synthesize, so the trailing mousedown still reaches the
                // canvas and blurs #uno-input. We can't preventDefault it from the pointer path, so we
                // intercept the mouse event itself, scoped to the single tap that just focused the input.
                static installTrailingClickGuard() {
                    // Any new pointer gesture disarms a stale flag, so only the mouse events synthesized
                    // from the very tap that focused the input are ever swallowed. Also records the pointer
                    // type: the bug is exclusive to touch/pen, where the compat mousedown is deferred until
                    // after focus. With a mouse, mousedown precedes focus, so there is nothing to guard.
                    document.addEventListener("pointerdown", (ev) => {
                        BrowserInvisibleTextBoxViewExtension.lastPointerType = ev.pointerType;
                        BrowserInvisibleTextBoxViewExtension.swallowNextCanvasClick = false;
                    }, { capture: true });
                    // focusin bubbles (unlike focus), so a single document-level listener catches the
                    // invisible input regardless of when it is (re)created.
                    document.addEventListener("focusin", (ev) => {
                        const target = ev.target;
                        if ((target === null || target === void 0 ? void 0 : target.id) === Runtime.UnoDomIds.input
                            && (BrowserInvisibleTextBoxViewExtension.lastPointerType === "touch"
                                || BrowserInvisibleTextBoxViewExtension.lastPointerType === "pen")) {
                            BrowserInvisibleTextBoxViewExtension.swallowNextCanvasClick = true;
                        }
                    }, { capture: true });
                    const swallow = (ev) => {
                        var _a;
                        if (BrowserInvisibleTextBoxViewExtension.swallowNextCanvasClick
                            && ((_a = ev.target) === null || _a === void 0 ? void 0 : _a.id) === Runtime.UnoDomIds.canvas) {
                            ev.preventDefault();
                            ev.stopImmediatePropagation();
                            BrowserInvisibleTextBoxViewExtension.swallowNextCanvasClick = false;
                        }
                    };
                    document.addEventListener("mousedown", swallow, { capture: true });
                    document.addEventListener("click", swallow, { capture: true });
                }
                static createInput(isPasswordBox, text, acceptsReturn, inputMode, enterKeyHint) {
                    BrowserInvisibleTextBoxViewExtension.acceptsReturn = acceptsReturn;
                    // A previous input may have been removed mid-composition without a compositionend;
                    // never carry that state over to a fresh element.
                    BrowserInvisibleTextBoxViewExtension.isComposing = false;
                    BrowserInvisibleTextBoxViewExtension.suppressNextInput = false;
                    const input = document.createElement(acceptsReturn && !isPasswordBox ? "textarea" : "input");
                    // The keydown/keyup handlers capture acceptsReturn by closure; record it so canRetarget
                    // only reuses the element when the captured behavior still matches.
                    input.__unoAcceptsReturn = acceptsReturn;
                    if (isPasswordBox) {
                        input.type = "password";
                        input.autocomplete = "password";
                    }
                    input.id = Runtime.UnoDomIds.input;
                    input.dataset.unoAcceptsReturn = acceptsReturn ? "true" : "false";
                    input.tabIndex = -1;
                    input.spellcheck = false;
                    input.style.whiteSpace = "pre-wrap";
                    input.style.position = "absolute";
                    input.style.padding = "0px";
                    input.style.opacity = "0";
                    input.style.color = "transparent";
                    input.style.background = "transparent";
                    input.style.caretColor = "transparent";
                    input.style.outline = "none";
                    input.style.border = "none";
                    input.style.resize = "none";
                    input.style.textShadow = "none";
                    input.style.overflow = "hidden";
                    input.style.pointerEvents = "none";
                    input.style.zIndex = "99";
                    // Placed before focus() so WebKit never sees an on-screen rect to reveal. Only top goes off-screen:
                    // a negative left would be scrollable overflow in a right-to-left document.
                    input.style.top = BrowserInvisibleTextBoxViewExtension.keepsInputOffscreen ? BrowserInvisibleTextBoxViewExtension.offscreenTop : "0px";
                    input.style.left = "0px";
                    input.value = text;
                    input.setAttribute("inputmode", inputMode);
                    input.setAttribute("enterkeyhint", enterKeyHint);
                    input.oninput = ev => {
                        // During IME composition, text state is managed by the composition event path.
                        // The oninput event still fires but we must skip the normal text sync.
                        // Also suppress the final input event after compositionend (browser fires input after compositionend).
                        if (BrowserInvisibleTextBoxViewExtension.isComposing || BrowserInvisibleTextBoxViewExtension.suppressNextInput) {
                            BrowserInvisibleTextBoxViewExtension.suppressNextInput = false;
                            return;
                        }
                        let input = ev.target;
                        if (input.selectionDirection == "backward") {
                            BrowserInvisibleTextBoxViewExtension._exports.OnInputTextChanged(input.value, input.selectionEnd, input.selectionStart - input.selectionEnd);
                        }
                        else {
                            BrowserInvisibleTextBoxViewExtension._exports.OnInputTextChanged(input.value, input.selectionStart, input.selectionEnd - input.selectionStart);
                        }
                    };
                    input.onpaste = ev => {
                        const source = ev.clipboardData.getData("text");
                        const sourceLimit = BrowserInvisibleTextBoxViewExtension._exports.GetNativePasteSourceLimit();
                        BrowserInvisibleTextBoxViewExtension._exports.OnNativePaste(source.length > sourceLimit ? source.substring(0, sourceLimit) : source);
                        ev.preventDefault();
                    };
                    // C# drives focus one-way (StartEntry/EndEntry call focus()/blur()), so a blur the
                    // browser initiates on its own — e.g. tapping outside, or the touch-synthesized
                    // mousedown in issue 1 — is otherwise invisible to the FocusManager and LostFocus
                    // never fires. Report only those; managed-initiated blurs set suppressBlurNotification.
                    input.addEventListener("blur", () => {
                        if (BrowserInvisibleTextBoxViewExtension.suppressBlurNotification) {
                            return;
                        }
                        BrowserInvisibleTextBoxViewExtension._exports.OnNativeBlur();
                    });
                    // Handle Enter key from Android virtual keyboards which don't fire keydown events.
                    // Android keyboards typically fire beforeinput with inputType "insertLineBreak" or "insertParagraph" instead.
                    input.addEventListener("beforeinput", (ev) => {
                        if ((ev.inputType === "insertLineBreak" || ev.inputType === "insertParagraph") && !BrowserInvisibleTextBoxViewExtension.acceptsReturn) {
                            ev.preventDefault();
                            BrowserInvisibleTextBoxViewExtension._exports.OnEnterKeyPressed();
                        }
                    });
                    BrowserInvisibleTextBoxViewExtension.attachTextInputKeyHandlers(input, acceptsReturn);
                    let activeCompositionGeneration = 0;
                    input.addEventListener("compositionstart", () => {
                        var _a;
                        activeCompositionGeneration = ++BrowserInvisibleTextBoxViewExtension.compositionGeneration;
                        BrowserInvisibleTextBoxViewExtension.isComposing = true;
                        BrowserInvisibleTextBoxViewExtension.compositionStart = (_a = input.selectionStart) !== null && _a !== void 0 ? _a : 0;
                        BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionStarted();
                    });
                    input.addEventListener("compositionupdate", (ev) => {
                        if (activeCompositionGeneration !== BrowserInvisibleTextBoxViewExtension.compositionGeneration) {
                            return;
                        }
                        // Use input.selectionStart for cursor position when available,
                        // as the IME may place the caret within the preedit string.
                        const selectionStart = input.selectionStart;
                        const cursorPosition = selectionStart === null
                            ? ev.data.length
                            : Math.max(0, Math.min(selectionStart - BrowserInvisibleTextBoxViewExtension.compositionStart, ev.data.length));
                        BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionUpdated(ev.data, cursorPosition);
                    });
                    input.addEventListener("compositionend", (ev) => {
                        if (activeCompositionGeneration !== BrowserInvisibleTextBoxViewExtension.compositionGeneration) {
                            return;
                        }
                        BrowserInvisibleTextBoxViewExtension.isComposing = false;
                        // The browser fires an input event after compositionend with the committed text.
                        // Suppress it to avoid double-inserting — the commit is handled by OnCompositionCompleted.
                        BrowserInvisibleTextBoxViewExtension.suppressNextInput = true;
                        if (ev.data.length > 0) {
                            BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionCompleted(ev.data);
                        }
                        else {
                            BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionCanceled();
                        }
                    });
                    document.body.appendChild(input);
                    BrowserInvisibleTextBoxViewExtension.inputElement = input;
                }
                // Applies the same keydown/keyup guards used on the invisible <input> to any text input
                // that must delegate character insertion to managed TextBox KeyDown handling.
                // Without these guards, focused text inputs (e.g. the a11y semantic <input>) would insert
                // the character natively AND via the managed path, producing duplicated input.
                static attachTextInputKeyHandlers(input, acceptsReturn) {
                    input.addEventListener("keydown", (ev) => {
                        const acceptsReturnNow = input === BrowserInvisibleTextBoxViewExtension.inputElement
                            ? BrowserInvisibleTextBoxViewExtension.acceptsReturn
                            : acceptsReturn;
                        // During IME composition, let the browser/IME handle all keys.
                        // stopPropagation prevents BrowserKeyboardInputSource from calling preventDefault.
                        if (ev.isComposing) {
                            ev.stopPropagation();
                            return;
                        }
                        if (ev.ctrlKey || (ev.metaKey && BrowserInvisibleTextBoxViewExtension.isMacOS)) {
                            // Due to browser security considerations, we need to let the clipboard operations be handled natively.
                            // So, we do stopPropagation instead of preventDefault
                            if (ev.key == "c" || ev.key == "C" || ev.key == "v" || ev.key == "V" || ev.key == "x" || ev.key == "X") {
                                ev.stopPropagation();
                                return;
                            }
                        }
                        // Allow Enter key to propagate when the TextBox doesn't accept returns
                        // Desktop/iOS path: keydown is the reliable signal; let it bubble to document so
                        // BrowserKeyboardInputSource raises the managed KeyDown. The flag prevents the
                        // keyup branch below from dispatching a duplicate OnEnterKeyPressed.
                        // This enables focus navigation (e.g., Uno.Toolkit's AutoFocusNext) on mobile browsers
                        if ((ev.key === "Enter" || ev.keyCode === 13) && !acceptsReturnNow) {
                            // Don't call preventDefault() to allow the key event to propagate to document listeners
                            BrowserInvisibleTextBoxViewExtension.enterHandledByKeyDown = true;
                            return;
                        }
                        // Android soft keyboards fire all keys as keyCode 229 / key "Unidentified".
                        // The C# side cannot identify these (maps to VirtualKey.None), so let the browser
                        // handle them natively. Text changes sync via the oninput handler.
                        // stopPropagation prevents the document-level BrowserKeyboardInputSource from
                        // calling preventDefault() on the event.
                        if (ev.keyCode === BrowserInvisibleTextBoxViewExtension.ANDROID_IME_KEYCODE) {
                            ev.stopPropagation();
                            return;
                        }
                        ev.preventDefault();
                    });
                    input.addEventListener("keyup", (ev) => {
                        const acceptsReturnNow = input === BrowserInvisibleTextBoxViewExtension.inputElement
                            ? BrowserInvisibleTextBoxViewExtension.acceptsReturn
                            : acceptsReturn;
                        // Android virtual keyboards (Gboard/SwiftKey/Samsung/AOSP) report keydown
                        // with keyCode 229 ("Unidentified") for Enter, which is stopPropagation'd
                        // above so it never reaches BrowserKeyboardInputSource. They DO report keyup
                        // with key === "Enter" though - use that to raise the managed KeyDown so
                        // focus-navigation patterns (Uno.Toolkit AutoFocusNext, FocusManager) work
                        // on Android browsers. The flag guards against double-dispatch on desktop/iOS,
                        // where the keydown branch already routed Enter through the document listener.
                        if (!acceptsReturnNow
                            && ev.key === "Enter"
                            && !BrowserInvisibleTextBoxViewExtension.enterHandledByKeyDown
                            && !ev.isComposing) {
                            ev.preventDefault();
                            BrowserInvisibleTextBoxViewExtension._exports.OnEnterKeyPressed();
                        }
                        if (ev.key === "Enter" || ev.keyCode === 13) {
                            BrowserInvisibleTextBoxViewExtension.enterHandledByKeyDown = false;
                        }
                        if (BrowserInvisibleTextBoxViewExtension.isComposing || ev.keyCode === BrowserInvisibleTextBoxViewExtension.ANDROID_IME_KEYCODE) {
                            ev.stopPropagation();
                        }
                    });
                }
                static setEnterKeyHint(enterKeyHint) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input) {
                        input.setAttribute("enterkeyhint", enterKeyHint);
                    }
                }
                static setInputMode(inputMode) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input) {
                        input.setAttribute("inputmode", inputMode);
                    }
                }
                static setTextPredictionEnabled(enabled) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input) {
                        // Password inputs keep their password-manager hint.
                        if (!(input instanceof HTMLInputElement && input.type === "password")) {
                            input.autocomplete = enabled ? "on" : "off";
                        }
                        input.setAttribute("autocorrect", enabled ? "on" : "off");
                    }
                }
                static setSpellCheckEnabled(enabled) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input) {
                        input.spellcheck = enabled;
                    }
                }
                static setAcceptsReturn(acceptsReturn) {
                    BrowserInvisibleTextBoxViewExtension.acceptsReturn = acceptsReturn;
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input) {
                        input.dataset.unoAcceptsReturn = acceptsReturn ? "true" : "false";
                    }
                }
                static focus(handle, isPassword, text, acceptsReturn, inputMode, enterKeyHint) {
                    // Supersede any detach a preceding managed blur scheduled: focus is moving between
                    // TextBoxes, and detaching in between would dismiss the soft keyboard (see blur).
                    BrowserInvisibleTextBoxViewExtension.detachGeneration++;
                    const semanticElement = document.getElementById(`uno-semantics-${handle}`);
                    if (semanticElement && document.activeElement === semanticElement) {
                        BrowserInvisibleTextBoxViewExtension.detach();
                        return false;
                    }
                    const existingInput = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (existingInput != null && BrowserInvisibleTextBoxViewExtension.canRetarget(existingInput, isPassword, acceptsReturn)) {
                        // Reuse the shared input in place: mobile browsers keep the soft keyboard up across
                        // a TextBox-to-TextBox move only while an editable element stays focused throughout.
                        BrowserInvisibleTextBoxViewExtension.acceptsReturn = acceptsReturn;
                        existingInput.setAttribute("inputmode", inputMode);
                        existingInput.setAttribute("enterkeyhint", enterKeyHint);
                        BrowserInvisibleTextBoxViewExtension.setText(text);
                        // It's necessary to actually focus the native input, not just make it visible. This is particularly
                        // important to mobile browsers (to open the software keyboard) and for assistive technology to not steal
                        // events and properly recognize password inputs to not read it.
                        if (document.activeElement !== existingInput) {
                            existingInput.focus(BrowserInvisibleTextBoxViewExtension.focusOptions);
                        }
                    }
                    else {
                        // The element kind must change (input/textarea/password), or an IME composition is in
                        // progress and must not leak into the next TextBox. Focus the new element BEFORE
                        // removing the old one so focus hands off editable-to-editable without a gap that
                        // would dismiss the soft keyboard. The implicit blur of the old element is
                        // managed-initiated, so suppress its notification.
                        existingInput === null || existingInput === void 0 ? void 0 : existingInput.removeAttribute("id");
                        this.createInput(isPassword, text, acceptsReturn, inputMode, enterKeyHint);
                        BrowserInvisibleTextBoxViewExtension.runSuppressingBlur(() => {
                            BrowserInvisibleTextBoxViewExtension.inputElement.focus(BrowserInvisibleTextBoxViewExtension.focusOptions);
                            existingInput === null || existingInput === void 0 ? void 0 : existingInput.remove();
                        });
                    }
                    BrowserInvisibleTextBoxViewExtension.currentHandle = Number(handle);
                    // Set for whichever element ends up live: the shared input is reused across TextBoxes, so tagging
                    // it only on creation leaves the policy unreported for every retargeted entry session. Inspectable
                    // from the Web Inspector when diagnosing keyboard or IME placement reports.
                    BrowserInvisibleTextBoxViewExtension.inputElement.dataset.unoPlacement =
                        BrowserInvisibleTextBoxViewExtension.keepsInputOffscreen ? "offscreen" : "tracking";
                    // The retarget path keeps the input focused, so no focusin fires and the trailing-click
                    // guard never arms; arm it here for both paths (see installTrailingClickGuard).
                    if (BrowserInvisibleTextBoxViewExtension.lastPointerType === "touch"
                        || BrowserInvisibleTextBoxViewExtension.lastPointerType === "pen") {
                        BrowserInvisibleTextBoxViewExtension.swallowNextCanvasClick = true;
                    }
                    return true;
                }
                // The shared input can be handed to another TextBox without being recreated only when the
                // element kind it was created with still matches the target TextBox.
                static canRetarget(input, isPassword, acceptsReturn) {
                    if (BrowserInvisibleTextBoxViewExtension.isComposing) {
                        return false;
                    }
                    const needsTextArea = acceptsReturn && !isPassword;
                    if (input instanceof HTMLTextAreaElement) {
                        return needsTextArea;
                    }
                    return !needsTextArea
                        && (input.type === "password") === isPassword
                        && input.__unoAcceptsReturn === acceptsReturn;
                }
                // Runs a managed-initiated focus mutation with the blur listener muted, so it isn't
                // reported back to the FocusManager (which already drove the change). Callers make the
                // blur dispatch synchronously inside the window: detachCore blurs explicitly before
                // removing, and the swap path in focus() focuses the successor (implicitly blurring the
                // old input) before removing it.
                static runSuppressingBlur(action) {
                    BrowserInvisibleTextBoxViewExtension.suppressBlurNotification = true;
                    try {
                        action();
                    }
                    finally {
                        BrowserInvisibleTextBoxViewExtension.suppressBlurNotification = false;
                    }
                }
                static detachCore() {
                    var _a, _b;
                    BrowserInvisibleTextBoxViewExtension.detachGeneration++;
                    BrowserInvisibleTextBoxViewExtension.currentHandle = 0;
                    // Blur explicitly before removing: the .blur() method dispatches synchronously, so it
                    // lands inside the suppression window. WebKit can otherwise defer the implicit blur that
                    // fires on element removal past that window, which would clear the wrong TextBox's focus.
                    (_a = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _a === void 0 ? void 0 : _a.blur();
                    (_b = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _b === void 0 ? void 0 : _b.remove();
                    BrowserInvisibleTextBoxViewExtension.inputElement = null;
                }
                static blur(handle) {
                    // Managed-initiated blur (EndEntry): the FocusManager already knows focus is leaving.
                    const blurredHandle = Number(handle);
                    if (blurredHandle !== 0
                        && BrowserInvisibleTextBoxViewExtension.currentHandle !== 0
                        && blurredHandle !== BrowserInvisibleTextBoxViewExtension.currentHandle) {
                        // Stale blur from a TextBox that no longer owns the shared input; detaching now
                        // would tear down its successor's entry session.
                        return;
                    }
                    // Don't detach synchronously: when focus is moving to another TextBox, EndEntry runs
                    // before StartEntry in the same task, and detaching in between commits the soft-keyboard
                    // dismissal on iOS even though another TextBox is about to take over. Defer by one
                    // microtask; an intervening focus()/detach() supersedes this via detachGeneration.
                    const generation = ++BrowserInvisibleTextBoxViewExtension.detachGeneration;
                    queueMicrotask(() => {
                        if (generation === BrowserInvisibleTextBoxViewExtension.detachGeneration) {
                            BrowserInvisibleTextBoxViewExtension.runSuppressingBlur(BrowserInvisibleTextBoxViewExtension.detachCore);
                        }
                    });
                }
                static detach() {
                    BrowserInvisibleTextBoxViewExtension.runSuppressingBlur(BrowserInvisibleTextBoxViewExtension.detachCore);
                }
                static hasInput() {
                    return BrowserInvisibleTextBoxViewExtension.inputElement != null;
                }
                static setText(text) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input != null) {
                        // During IME composition the browser manages the hidden input's value.
                        // Overwriting it would destroy the native composition state and cursor.
                        if (BrowserInvisibleTextBoxViewExtension.isComposing) {
                            return;
                        }
                        // input could be null beccause we could call setText without focusing first
                        if (input.value != text) {
                            // When setting input.value, the browser will try to set the selection to the end, which isn't what we want.
                            // The browser doesn't raise onselectionchange synchronously though, so we set a flag that we're waiting
                            // for a future selection change that is the result of setting value.
                            // And we set the existing values of selection start and selection end.
                            // On the next onselectionchange event, we will ignore the browser provided selection and use these values.
                            // Also, in case we got a managed selection in between here and the next onselectionchange, we will
                            // use that instead (see updateSelection below).
                            BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange = true;
                            BrowserInvisibleTextBoxViewExtension.nextSelectionStart = input.selectionStart;
                            BrowserInvisibleTextBoxViewExtension.nextSelectionEnd = input.selectionEnd;
                            BrowserInvisibleTextBoxViewExtension.nextSelectionDirection = input.selectionDirection;
                            input.value = text;
                        }
                    }
                }
                static replaceText(start, length, replacement) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input == null || BrowserInvisibleTextBoxViewExtension.isComposing) {
                        return;
                    }
                    start = Math.max(0, Math.min(start, input.value.length));
                    const end = Math.max(start, Math.min(start + length, input.value.length));
                    input.setRangeText(replacement, start, end, "preserve");
                }
                static invalidateComposition() {
                    const wasComposing = BrowserInvisibleTextBoxViewExtension.isComposing;
                    BrowserInvisibleTextBoxViewExtension.compositionGeneration++;
                    BrowserInvisibleTextBoxViewExtension.isComposing = false;
                    BrowserInvisibleTextBoxViewExtension.suppressNextInput = wasComposing;
                }
                static restartComposition() {
                    BrowserInvisibleTextBoxViewExtension.invalidateComposition();
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input == null || document.activeElement !== input) {
                        return;
                    }
                    const selectionStart = input.selectionStart;
                    const selectionEnd = input.selectionEnd;
                    const selectionDirection = input.selectionDirection;
                    // Managed-initiated: reporting this blur would unfocus the TextBox that owns the session.
                    BrowserInvisibleTextBoxViewExtension.runSuppressingBlur(() => {
                        input.blur();
                        input.focus({ preventScroll: true });
                    });
                    if (selectionStart !== null && selectionEnd !== null) {
                        input.setSelectionRange(selectionStart, selectionEnd, selectionDirection);
                    }
                }
                static updateSize(width, height) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    // Sized like the TextBox only where the input is positioned over it: a multi-line TextBox reports
                    // its full content height, which would stretch an off-screen input back into the viewport.
                    if (input != null && !BrowserInvisibleTextBoxViewExtension.keepsInputOffscreen) {
                        input.style.width = `${width}px`;
                        input.style.height = `${height}px`;
                    }
                }
                static updatePosition(x, y) {
                    const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                    if (input != null && !BrowserInvisibleTextBoxViewExtension.keepsInputOffscreen) {
                        input.style.top = `${Math.round(y)}px`;
                        input.style.left = `${Math.round(x)}px`;
                    }
                }
                static updateSelection(start, length, direction) {
                    // During IME composition the browser manages the hidden input's selection.
                    if (BrowserInvisibleTextBoxViewExtension.isComposing) {
                        return;
                    }
                    if (!BrowserInvisibleTextBoxViewExtension.isInSelectionChange) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        // See comment in setText.
                        if (BrowserInvisibleTextBoxViewExtension.waitingAsyncOnSelectionChange) {
                            BrowserInvisibleTextBoxViewExtension.nextSelectionStart = start;
                            BrowserInvisibleTextBoxViewExtension.nextSelectionEnd = start + length;
                            BrowserInvisibleTextBoxViewExtension.nextSelectionDirection = direction;
                        }
                        input === null || input === void 0 ? void 0 : input.setSelectionRange(start, start + length, direction);
                    }
                }
            }
            BrowserInvisibleTextBoxViewExtension.isMacOS = (_a = navigator === null || navigator === void 0 ? void 0 : navigator.platform.toUpperCase().includes('MAC')) !== null && _a !== void 0 ? _a : false;
            // iPadOS 13+ reports itself as a Mac, so touch support is what tells it apart from macOS Safari.
            BrowserInvisibleTextBoxViewExtension.isIOS = /iP(ad|hone|od)/.test((_b = navigator === null || navigator === void 0 ? void 0 : navigator.platform) !== null && _b !== void 0 ? _b : "")
                || ((navigator === null || navigator === void 0 ? void 0 : navigator.platform) === "MacIntel" && ((_c = navigator.maxTouchPoints) !== null && _c !== void 0 ? _c : 0) > 1);
            // iOS WebKit scrolls the page to reveal the focused editable, on focus and on every keyboard frame
            // change. The page is position:fixed, so that scroll pans the whole canvas and shifts hit-testing by
            // the same amount. Off-screen there is nothing to reveal; the TextBox draws its own caret, selection
            // and grippers, so the input's position and size are unused there.
            BrowserInvisibleTextBoxViewExtension.keepsInputOffscreen = BrowserInvisibleTextBoxViewExtension.isIOS;
            BrowserInvisibleTextBoxViewExtension.offscreenTop = "-10000px";
            // The InputPane drives the bring-into-view of the focused TextBox; the browser's own scroll-to-focus
            // would only pan the visual viewport (see keepsInputOffscreen).
            BrowserInvisibleTextBoxViewExtension.focusOptions = { preventScroll: true };
            // Visual handle of the TextBox that currently owns the shared input, so a stale managed
            // blur from a TextBox that already lost it cannot detach its successor's input.
            BrowserInvisibleTextBoxViewExtension.currentHandle = 0;
            // Bumped by focus()/detachCore() to supersede the deferred detach scheduled by blur():
            // a blur immediately followed by a focus is a TextBox-to-TextBox move, and detaching
            // would needlessly dismiss the soft keyboard (see blur).
            BrowserInvisibleTextBoxViewExtension.detachGeneration = 0;
            BrowserInvisibleTextBoxViewExtension.compositionStart = 0;
            BrowserInvisibleTextBoxViewExtension.compositionGeneration = 0;
            // Android soft keyboards report all key events with keyCode 229 ("Unidentified").
            // Text changes are synced via the oninput handler instead.
            BrowserInvisibleTextBoxViewExtension.ANDROID_IME_KEYCODE = 229;
            Runtime.BrowserInvisibleTextBoxViewExtension = BrowserInvisibleTextBoxViewExtension;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class BrowserKeyboardInputSource {
                static initialize(inputSource) {
                    if (BrowserKeyboardInputSource._exports == undefined) {
                        const browserExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports();
                        BrowserKeyboardInputSource._exports = browserExports.Uno.UI.Runtime.BrowserKeyboardInputSource;
                    }
                    BrowserKeyboardInputSource._source = inputSource;
                    BrowserKeyboardInputSource.subscribeKeyboardEvents();
                }
                static subscribeKeyboardEvents() {
                    document.addEventListener("keydown", BrowserKeyboardInputSource.onKeyboardEvent);
                    document.addEventListener("keyup", BrowserKeyboardInputSource.onKeyboardEvent);
                }
                static onKeyboardEvent(evt) {
                    // When the Enable Accessibility button is still in the DOM,
                    // allow Tab to reach it via native browser focus navigation
                    // before the managed FocusManager intercepts it.
                    if (evt.key === "Tab" && Runtime.Accessibility.isEnableAccessibilityButtonActive()) {
                        const active = document.activeElement;
                        const isOnButton = active instanceof HTMLElement && active.id === "uno-enable-accessibility";
                        if (evt.type === "keydown") {
                            // No element focused yet — let browser Tab to the prepended button
                            const isUnfocused = !active || active === document.body || active === document.documentElement;
                            if (isUnfocused) {
                                return;
                            }
                            // Button focused + Shift+Tab — let browser move focus back naturally
                            if (isOnButton && evt.shiftKey) {
                                return;
                            }
                            // Button focused + Tab (no shift) — fall through to managed FocusManager
                        }
                        // Don't route keyup to managed code while on the button
                        if (evt.type === "keyup" && isOnButton) {
                            return;
                        }
                    }
                    let result = BrowserKeyboardInputSource._exports.OnNativeKeyboardEvent(BrowserKeyboardInputSource._source, evt.type == "keydown", evt.ctrlKey, evt.shiftKey, evt.altKey, evt.metaKey, evt.code, evt.key);
                    if (result == Runtime.HtmlEventDispatchResult.PreventDefault) {
                        evt.preventDefault();
                    }
                }
            }
            Runtime.BrowserKeyboardInputSource = BrowserKeyboardInputSource;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class BrowserMediaPlayerExtension {
                static buildImports() {
                    BrowserMediaPlayerExtension.unoExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports().Uno.UI.Runtime.BrowserMediaPlayerExtension;
                }
                static getVideoPlaybackRate(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.playbackRate;
                    }
                    return 1;
                }
                static setVideoPlaybackRate(elementId, playbackRate) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.playbackRate = playbackRate;
                        videoElement.buffered;
                    }
                }
                static getIsVideoLooped(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.loop;
                    }
                    return false;
                }
                static setIsVideoLooped(elementId, isLooped) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.loop = isLooped;
                    }
                }
                static getDuration(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.duration;
                    }
                    return 0;
                }
                static setSource(elementId, uri) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.src = uri;
                        videoElement.load();
                    }
                }
                static play(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.play();
                    }
                }
                static pause(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.pause();
                    }
                }
                static stop(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.load(); // this is not a typo, loading an already-loaded video resets (i.e. stops) it.
                    }
                }
                static getPosition(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.currentTime;
                    }
                    return 0;
                }
                static setPosition(elementId, position) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.currentTime = position;
                    }
                }
                static setMuted(elementId, muted) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.muted = muted;
                    }
                }
                static setVolume(elementId, volume) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.volume = volume;
                    }
                }
                static setupEvents(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.onloadedmetadata = e => BrowserMediaPlayerExtension.unoExports.OnLoadedMetadata(elementId, videoElement.videoWidth !== 0);
                        videoElement.onstalled = e => BrowserMediaPlayerExtension.unoExports.OnStalled(elementId);
                        videoElement.onratechange = e => BrowserMediaPlayerExtension.unoExports.OnRateChange(elementId);
                        videoElement.ondurationchange = e => BrowserMediaPlayerExtension.unoExports.OnDurationChange(elementId);
                        videoElement.onended = e => BrowserMediaPlayerExtension.unoExports.OnEnded(elementId);
                        videoElement.onerror = e => BrowserMediaPlayerExtension.unoExports.OnError(elementId);
                        videoElement.onpause = e => BrowserMediaPlayerExtension.unoExports.OnPause(elementId);
                        videoElement.onplaying = e => BrowserMediaPlayerExtension.unoExports.OnPlaying(elementId);
                        videoElement.onseeked = e => BrowserMediaPlayerExtension.unoExports.OnSeeked(elementId);
                        videoElement.onvolumechange = e => BrowserMediaPlayerExtension.unoExports.OnVolumeChange(elementId);
                        // videoElement.ontimeupdate = e => BrowserMediaPlayerExtension.unoExports.OnTimeUpdate(elementId);
                    }
                }
            }
            Runtime.BrowserMediaPlayerExtension = BrowserMediaPlayerExtension;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class BrowserMediaPlayerPresenterExtension {
                static buildImports() {
                    BrowserMediaPlayerPresenterExtension.unoExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports().Uno.UI.Runtime.BrowserMediaPlayerPresenterExtension;
                }
                static getVideoNaturalHeight(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.videoHeight;
                    }
                    return 0;
                }
                static getVideoNaturalWidth(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        return videoElement.videoWidth;
                    }
                    return 0;
                }
                static requestFullscreen(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.requestFullscreen();
                    }
                }
                static exitFullscreen(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement && document.fullscreenElement === videoElement) {
                        document.exitFullscreen();
                    }
                }
                static requestPictureInPicture(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        // The cast is here because tsc complains about requestPictureInPicture not being present in HTMLVideoElement
                        videoElement.requestPictureInPicture();
                    }
                }
                static exitPictureInPicture(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement && document.fullscreenElement === videoElement) {
                        // The cast is here because tsc complains about exitPictureInPicture not being present in Document
                        document.exitPictureInPicture();
                    }
                }
                static updateStretch(elementId, stretch) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        switch (stretch) {
                            case "None":
                                videoElement.style.objectFit = "none";
                                break;
                            case "Fill":
                                videoElement.style.objectFit = "fill";
                                break;
                            case "Uniform":
                                videoElement.style.objectFit = "contain";
                                break;
                            case "UniformToFill":
                                videoElement.style.objectFit = "cover";
                                break;
                        }
                    }
                }
                static setupEvents(elementId) {
                    const videoElement = document.getElementById(elementId);
                    if (videoElement) {
                        videoElement.onfullscreenchange = e => {
                            if (!document.fullscreenElement) {
                                BrowserMediaPlayerPresenterExtension.unoExports.OnExitFullscreen(elementId);
                            }
                        };
                    }
                }
            }
            Runtime.BrowserMediaPlayerPresenterExtension = BrowserMediaPlayerPresenterExtension;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var NativeElementHosting;
        (function (NativeElementHosting) {
            class BrowserHtmlElement {
                static async initialize() {
                    let anyModule = window.Module;
                    if (anyModule.getAssemblyExports !== undefined) {
                        const browserExports = await anyModule.getAssemblyExports("Uno.UI");
                        BrowserHtmlElement.dispatchEventNativeElementMethod = browserExports.Uno.UI.NativeElementHosting.BrowserHtmlElement.DispatchEventNativeElementMethod;
                    }
                    else {
                        throw `BrowserHtmlElement: Unable to find dotnet exports`;
                    }
                }
                static setSvgClipPathForNativeElementHost(path, fillType) {
                    if (!document.getElementById("unoNativeElementHostClipPath")) {
                        const svgContainer = document.createElementNS("http://www.w3.org/2000/svg", "svg");
                        svgContainer.setAttribute("width", "0");
                        svgContainer.setAttribute("height", "0");
                        const defs = document.createElementNS("http://www.w3.org/2000/svg", "defs");
                        const clipPath = document.createElementNS("http://www.w3.org/2000/svg", "clipPath");
                        clipPath.setAttribute("id", "unoNativeElementHostClipPath");
                        this.clipPath = document.createElementNS("http://www.w3.org/2000/svg", "path");
                        clipPath.appendChild(this.clipPath);
                        defs.appendChild(clipPath);
                        svgContainer.appendChild(defs);
                        document.body.appendChild(svgContainer);
                    }
                    this.clipPath.setAttribute("d", path);
                    this.clipPath.setAttribute("clip-rule", fillType);
                }
                static getNativeElementHost() {
                    let nativeElementHost = document.getElementById("uno-native-element-host");
                    if (!nativeElementHost) {
                        nativeElementHost = document.createElement("div");
                        nativeElementHost.id = "uno-native-element-host";
                        nativeElementHost.style.position = "absolute";
                        nativeElementHost.style.height = "100%";
                        nativeElementHost.style.width = "100%";
                        nativeElementHost.style.overflow = "hidden";
                        nativeElementHost.style.clipPath = "url(#unoNativeElementHostClipPath)";
                        let unoBody = document.getElementById("uno-body");
                        unoBody.appendChild(nativeElementHost);
                    }
                    return nativeElementHost;
                }
                static isNativeElement(content) {
                    for (let child of this.getNativeElementHost().children) {
                        if (child.id === content) {
                            return true;
                        }
                    }
                    return false;
                }
                static attachNativeElement(content) {
                    let element = this.getElementOrThrow(content);
                    element.hidden = false;
                }
                static detachNativeElement(content) {
                    // A fling still running against this element would keep scrolling a ScrollViewer it is no
                    // longer part of.
                    Uno.UI.Runtime.BrowserPointerInputSource.cancelNativeScrollInertiaFor(content);
                    let element = this.getElementOrThrow(content);
                    element.hidden = true;
                }
                static arrangeNativeElement(content, x, y, width, height) {
                    let element = this.getElementOrThrow(content);
                    element.style.position = "absolute";
                    element.style.left = `${x}px`;
                    element.style.top = `${y}px`;
                    element.style.width = `${width}px`;
                    element.style.height = `${height}px`;
                }
                static changeNativeElementOpacity(content, opacity) {
                    let element = this.getElementOrThrow(content);
                    element.style.opacity = opacity.toString();
                }
                static createHtmlElement(id, tagName) {
                    let element = document.createElement(tagName);
                    element.id = id;
                    element.hidden = true;
                    this.getNativeElementHost().appendChild(element);
                }
                static disposeHtmlElement(id) {
                    this.getElementOrThrow(id).remove();
                }
                static setZIndex(id, zIndex) {
                    let element = this.getElementOrThrow(id);
                    element.style.zIndex = zIndex.toString();
                }
                static createSampleComponent(parentId, text) {
                    let element = this.getElementOrThrow(parentId);
                    let btn = document.createElement("button");
                    btn.textContent = text;
                    btn.style.display = "inline-block";
                    btn.style.width = "100%";
                    btn.style.height = "100%";
                    btn.style.backgroundColor = "#ff69b4"; /* Hot pink */
                    btn.style.color = "white";
                    element.appendChild(btn);
                    element.addEventListener("pointerdown", _ => alert(`button ${text} clicked`));
                }
                static setStyleString(elementId, name, value) {
                    const element = this.getElementOrThrow(elementId);
                    element.style.setProperty(name, value);
                }
                static resetStyle(elementId, names) {
                    const element = this.getElementOrThrow(elementId);
                    for (const name of names) {
                        element.style.setProperty(name, "");
                    }
                }
                static setClasses(elementId, cssClassesList, classIndex) {
                    const element = this.getElementOrThrow(elementId);
                    for (let i = 0; i < cssClassesList.length; i++) {
                        if (i === classIndex) {
                            element.classList.add(cssClassesList[i]);
                        }
                        else {
                            element.classList.remove(cssClassesList[i]);
                        }
                    }
                }
                static setUnsetCssClasses(elementId, classesToUnset) {
                    const element = this.getElementOrThrow(elementId);
                    classesToUnset.forEach(c => {
                        element.classList.remove(c);
                    });
                }
                static setAttribute(elementId, name, value) {
                    const element = this.getElementOrThrow(elementId);
                    element.setAttribute(name, value);
                }
                static getAttribute(elementId, name) {
                    const element = this.getElementOrThrow(elementId);
                    return element.getAttribute(name);
                }
                static removeAttribute(elementId, name) {
                    const element = this.getElementOrThrow(elementId);
                    element.removeAttribute(name);
                }
                static setContentHtml(elementId, html) {
                    const element = this.getElementOrThrow(elementId);
                    element.innerHTML = html;
                }
                static setInputPolicy(elementId, unoElementId, value) {
                    const element = this.getElementOrThrow(elementId);
                    BrowserHtmlElement.inputPolicies.set(element, { policy: value, unoElementId: unoElementId });
                    element.dataset.unoNativeInputPolicy = value.toString();
                }
                /** Returns the policy registered by managed code for this exact element, or null if it has none. */
                static getInputPolicy(element) {
                    var _a;
                    return (_a = BrowserHtmlElement.inputPolicies.get(element)) !== null && _a !== void 0 ? _a : null;
                }
                static registerNativeHtmlEvent(owner, elementId, eventName, managedHandler) {
                    const element = this.getElementOrThrow(elementId);
                    if (!BrowserHtmlElement.dispatchEventNativeElementMethod) {
                        throw `BrowserHtmlElement: The initialize method has not been called`;
                    }
                    const eventHandler = (event) => {
                        BrowserHtmlElement.dispatchEventNativeElementMethod(owner, eventName, managedHandler, event);
                    };
                    // Register the handler using a string representation of the managed handler
                    // the managed representation assumes that the string contains a unique id.
                    BrowserHtmlElement.nativeHandlersMap["" + managedHandler] = eventHandler;
                    element.addEventListener(eventName, eventHandler);
                }
                static unregisterNativeHtmlEvent(elementId, eventName, managedHandler) {
                    const element = this.getElementOrThrow(elementId);
                    if (!BrowserHtmlElement.dispatchEventNativeElementMethod) {
                        throw `BrowserHtmlElement: The initialize method has not been called`;
                    }
                    const key = "" + managedHandler;
                    const eventHandler = BrowserHtmlElement.nativeHandlersMap[key];
                    if (eventHandler) {
                        element.removeEventListener(eventName, eventHandler);
                        delete BrowserHtmlElement.nativeHandlersMap[key];
                    }
                }
                static invokeJS(command) {
                    return String(eval(command) || "");
                }
                static async invokeAsync(command) {
                    // Preseve the original emscripten marshalling semantics
                    // to always return a valid string.
                    var result = await eval(command);
                    return String(result || "");
                }
                /**
                 * Returns the element with the given id, or throws an error if not found.
                 */
                static getElementOrThrow(id) {
                    const element = document.getElementById(id);
                    if (!element) {
                        throw new Error(`BrowserHtmlElement: Element with id '${id}' not found.`);
                    }
                    return element;
                }
            }
            /** Native elements created with the BrowserHtmlElement class */
            BrowserHtmlElement.nativeHandlersMap = {};
            /**
             * Input policies of the hosted elements, keyed by element reference.
             * The map - not the DOM attribute - is the source of truth: hosted HTML is app- or third-party-supplied
             * and could otherwise carry a data-uno-native-input-policy attribute of its own to opt itself in.
             * The attribute is still written because uno.css keys the touch-action override off it.
             */
            BrowserHtmlElement.inputPolicies = new WeakMap();
            NativeElementHosting.BrowserHtmlElement = BrowserHtmlElement;
        })(NativeElementHosting = UI.NativeElementHosting || (UI.NativeElementHosting = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            //import PointerDeviceType = Windows.Devices.Input.PointerDeviceType;
            let HtmlPointerEvent;
            (function (HtmlPointerEvent) {
                HtmlPointerEvent[HtmlPointerEvent["pointerover"] = 1] = "pointerover";
                HtmlPointerEvent[HtmlPointerEvent["pointerout"] = 2] = "pointerout";
                HtmlPointerEvent[HtmlPointerEvent["pointerdown"] = 4] = "pointerdown";
                HtmlPointerEvent[HtmlPointerEvent["pointerup"] = 8] = "pointerup";
                HtmlPointerEvent[HtmlPointerEvent["pointercancel"] = 16] = "pointercancel";
                // Optional pointer events
                HtmlPointerEvent[HtmlPointerEvent["pointermove"] = 32] = "pointermove";
                HtmlPointerEvent[HtmlPointerEvent["lostpointercapture"] = 64] = "lostpointercapture";
                HtmlPointerEvent[HtmlPointerEvent["wheel"] = 128] = "wheel";
            })(HtmlPointerEvent = Runtime.HtmlPointerEvent || (Runtime.HtmlPointerEvent = {}));
            // TODO: Duplicate of Uno.UI.HtmlEventDispatchResult to merge!
            let HtmlEventDispatchResult;
            (function (HtmlEventDispatchResult) {
                HtmlEventDispatchResult[HtmlEventDispatchResult["Ok"] = 0] = "Ok";
                HtmlEventDispatchResult[HtmlEventDispatchResult["StopPropagation"] = 1] = "StopPropagation";
                HtmlEventDispatchResult[HtmlEventDispatchResult["PreventDefault"] = 2] = "PreventDefault";
                HtmlEventDispatchResult[HtmlEventDispatchResult["NotDispatched"] = 128] = "NotDispatched";
            })(HtmlEventDispatchResult = Runtime.HtmlEventDispatchResult || (Runtime.HtmlEventDispatchResult = {}));
            // TODO: Duplicate of Windows.Devices.Input.PointerDeviceType to import instead of duplicate!
            let PointerDeviceType;
            (function (PointerDeviceType) {
                PointerDeviceType[PointerDeviceType["Touch"] = 0] = "Touch";
                PointerDeviceType[PointerDeviceType["Pen"] = 1] = "Pen";
                PointerDeviceType[PointerDeviceType["Mouse"] = 2] = "Mouse";
            })(PointerDeviceType = Runtime.PointerDeviceType || (Runtime.PointerDeviceType = {}));
            let NativeElementInputPolicy;
            (function (NativeElementInputPolicy) {
                NativeElementInputPolicy[NativeElementInputPolicy["NativeOnly"] = 0] = "NativeOnly";
                NativeElementInputPolicy[NativeElementInputPolicy["Negotiated"] = 1] = "Negotiated";
            })(NativeElementInputPolicy = Runtime.NativeElementInputPolicy || (Runtime.NativeElementInputPolicy = {}));
            class BrowserPointerInputSource {
                constructor(manageSource) {
                    // Cached reference to #uno-native-element-host. Refreshed if detached/replaced.
                    this._nativeElementHost = null;
                    this._nativeScrollGesture = null;
                    this._nativeScrollInertiaFrame = null;
                    this._nativeScrollInertiaSession = 0;
                    this._nativeScrollInertiaGesture = null;
                    this._bootTime = Date.now() - performance.now();
                    this._source = manageSource;
                    BrowserPointerInputSource._instance = this;
                    BrowserPointerInputSource._exports.OnInitialized(manageSource, this._bootTime);
                    this.subscribePointerEvents(); // Subscribe only after the managed initialization is done
                }
                static async initialize(inputSource) {
                    if (BrowserPointerInputSource._exports == undefined) {
                        const browserExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports();
                        BrowserPointerInputSource._exports = browserExports.Uno.UI.Runtime.BrowserPointerInputSource;
                    }
                    new BrowserPointerInputSource(inputSource);
                }
                static setPointerCapture(pointerId) {
                    // Capture disabled for now on skia for wasm
                    //document.body.setPointerCapture(pointerId);
                }
                static releasePointerCapture(pointerId) {
                    // Capture disabled for now on skia for wasm
                    //document.body.releasePointerCapture(pointerId);
                }
                subscribePointerEvents() {
                    const element = document.body;
                    element.addEventListener("pointerover", this.onPointerEventReceived.bind(this), { capture: true });
                    element.addEventListener("pointerout", this.onPointerEventReceived.bind(this), { capture: true });
                    element.addEventListener("pointerdown", this.onPointerEventReceived.bind(this), { capture: true });
                    element.addEventListener("pointerup", this.onPointerEventReceived.bind(this), { capture: true });
                    //element.addEventListener("lostpointercapture", this.onPointerEventReceived.bind(this), { capture: true });
                    element.addEventListener("pointercancel", this.onPointerEventReceived.bind(this), { capture: true });
                    element.addEventListener("pointermove", this.onPointerEventReceived.bind(this), { capture: true, passive: false });
                    element.addEventListener("wheel", this.onPointerEventReceived.bind(this), { capture: true, passive: false });
                }
                // Retrieve and cache the native element host reference.
                // Refreshes if the node was detached or replaced (e.g., during hot reload).
                getNativeElementHostCached() {
                    if (this._nativeElementHost === null || this._nativeElementHost.isConnected === false) {
                        this._nativeElementHost = document.getElementById("uno-native-element-host");
                    }
                    return this._nativeElementHost;
                }
                // Returns true if the event originated from within the native host subtree.
                // Traverses regular DOM first, then crosses Shadow DOM boundaries when required.
                // Uses identity comparisons only; avoids selector matching, allocations, and redundant lookups.
                isEventFromNativeElementHost(eventTarget) {
                    const hostElement = this.getNativeElementHostCached();
                    if (hostElement === null) {
                        return false; // No host exists; nothing to filter.
                    }
                    let currentNode = eventTarget;
                    while (currentNode !== null) {
                        // Fast identity comparison.
                        if (currentNode === hostElement) {
                            return true;
                        }
                        // Normal DOM climb first (fastest path)
                        const parent = currentNode.parentNode;
                        if (parent) {
                            currentNode = parent;
                            continue;
                        }
                        // Only if parentNode is null, check for a shadow boundary.
                        const rootNode = currentNode.getRootNode();
                        // If we're inside a shadow root, jump to its host to continue traversal
                        if (rootNode instanceof ShadowRoot && rootNode.host) {
                            currentNode = rootNode.host; // cross shadow boundary
                            continue;
                        }
                        // Reached the top (Document or no further nodes)
                        break;
                    }
                    return false;
                }
                /**
                 * Returns the hosted element the event originated from - the direct child of #uno-native-element-host -
                 * together with the policy managed code registered for it. Resolution is by position in the tree and by
                 * element identity, never by reading an attribute off the event target: hosted HTML can carry any
                 * attribute it likes and must not be able to opt itself into a policy the app did not set.
                 */
                getNativeInputHost(eventTarget) {
                    const hostElement = this.getNativeElementHostCached();
                    if (hostElement === null) {
                        return null;
                    }
                    let currentNode = eventTarget;
                    while (currentNode !== null) {
                        if (currentNode instanceof HTMLElement && currentNode.parentNode === hostElement) {
                            const registration = Uno.UI.NativeElementHosting.BrowserHtmlElement.getInputPolicy(currentNode);
                            return registration === null
                                ? null
                                : {
                                    root: currentNode,
                                    policy: registration.policy === NativeElementInputPolicy.Negotiated
                                        ? NativeElementInputPolicy.Negotiated
                                        : NativeElementInputPolicy.NativeOnly,
                                    unoElementId: registration.unoElementId,
                                };
                        }
                        const parent = currentNode.parentNode;
                        if (parent) {
                            currentNode = parent;
                            continue;
                        }
                        const root = currentNode.getRootNode();
                        if (root instanceof ShadowRoot && root.host) {
                            currentNode = root.host;
                            continue;
                        }
                        break;
                    }
                    return null;
                }
                findScrollableElement(target, root) {
                    let current = target instanceof HTMLElement ? target : null;
                    while (current !== null) {
                        if (current instanceof HTMLIFrameElement) {
                            return null;
                        }
                        const style = window.getComputedStyle(current);
                        if ((style.overflowX === "auto" || style.overflowX === "scroll" || style.overflowY === "auto" || style.overflowY === "scroll")
                            && (current.scrollWidth > current.clientWidth || current.scrollHeight > current.clientHeight)) {
                            return current;
                        }
                        if (current === root) {
                            break;
                        }
                        current = current.parentElement;
                    }
                    return null;
                }
                applyNativeScrollDelta(gesture, horizontalDelta, verticalDelta, isIntermediate = true, isInertial = false) {
                    if (gesture.primaryAxis === "horizontal") {
                        verticalDelta = 0;
                    }
                    else if (gesture.primaryAxis === "vertical") {
                        horizontalDelta = 0;
                    }
                    let remainingHorizontalDelta = horizontalDelta;
                    let remainingVerticalDelta = verticalDelta;
                    // Non-scrollable native content has nothing to consume the gesture, so Uno takes it immediately.
                    if (!gesture.unoOwnsGesture && gesture.scrollTarget === null) {
                        gesture.unoOwnsGesture = true;
                    }
                    if (!gesture.unoOwnsGesture && gesture.scrollTarget !== null) {
                        const initialScrollLeft = gesture.scrollTarget.scrollLeft;
                        const initialScrollTop = gesture.scrollTarget.scrollTop;
                        const scrollRange = Math.max(gesture.scrollTarget.scrollWidth - gesture.scrollTarget.clientWidth, 0);
                        const maximumScrollTop = Math.max(gesture.scrollTarget.scrollHeight - gesture.scrollTarget.clientHeight, 0);
                        // In a right-to-left scroller scrollLeft runs from -scrollRange (fully scrolled) to 0, so the
                        // clamping bounds - not just the sign of the delta - depend on the direction.
                        const isRightToLeft = window.getComputedStyle(gesture.scrollTarget).direction === "rtl";
                        const minimumScrollLeft = isRightToLeft ? -scrollRange : 0;
                        const maximumScrollLeft = isRightToLeft ? 0 : scrollRange;
                        const requestedScrollLeft = initialScrollLeft + horizontalDelta;
                        const requestedScrollTop = initialScrollTop + verticalDelta;
                        const nextScrollLeft = Math.min(Math.max(requestedScrollLeft, minimumScrollLeft), maximumScrollLeft);
                        const nextScrollTop = Math.min(Math.max(requestedScrollTop, 0), maximumScrollTop);
                        gesture.scrollTarget.scrollLeft = nextScrollLeft;
                        gesture.scrollTarget.scrollTop = nextScrollTop;
                        // Do not use the observed scroll offset to calculate residual input: browsers may
                        // round it. That would incorrectly hand a fractional drag to Uno before the
                        // native scroller reaches an actual boundary.
                        remainingHorizontalDelta = requestedScrollLeft - nextScrollLeft;
                        remainingVerticalDelta = requestedScrollTop - nextScrollTop;
                        if (Math.abs(remainingHorizontalDelta) > 0.01 || Math.abs(remainingVerticalDelta) > 0.01) {
                            gesture.unoOwnsGesture = true;
                        }
                    }
                    if (gesture.unoOwnsGesture) {
                        return BrowserPointerInputSource._exports.OnNativeScrollDelta(gesture.unoElementId, remainingHorizontalDelta, remainingVerticalDelta, isIntermediate, isInertial) !== 0;
                    }
                    return remainingHorizontalDelta !== horizontalDelta || remainingVerticalDelta !== verticalDelta;
                }
                cancelNativeScrollInertia() {
                    this._nativeScrollInertiaSession++;
                    if (this._nativeScrollInertiaFrame !== null) {
                        cancelAnimationFrame(this._nativeScrollInertiaFrame);
                        this._nativeScrollInertiaFrame = null;
                        if (this._nativeScrollInertiaGesture !== null) {
                            this.completeNativeScroll(this._nativeScrollInertiaGesture);
                        }
                    }
                    this._nativeScrollInertiaGesture = null;
                }
                /** Public entry point so a detached element cannot leave a fling running against it. */
                static cancelNativeScrollInertiaFor(elementId) {
                    var _a;
                    const instance = BrowserPointerInputSource._instance;
                    if (instance !== null && ((_a = instance._nativeScrollInertiaGesture) === null || _a === void 0 ? void 0 : _a.root.id) === elementId) {
                        instance.cancelNativeScrollInertia();
                    }
                }
                /**
                 * Closes a chained scroll sequence so the ScrollViewers see a final non-intermediate view change.
                 * Every delta during a drag or fling is reported as intermediate; without this, consumers that treat
                 * IsIntermediate=false as "the scroll ended" would never observe the end.
                 */
                completeNativeScroll(gesture) {
                    if (!gesture.unoOwnsGesture) {
                        return;
                    }
                    try {
                        BrowserPointerInputSource._exports.OnNativeScrollCompleted(gesture.unoElementId);
                    }
                    catch (e) {
                        console.warn(`Failed to complete negotiated native scroll: ${e}`);
                    }
                }
                startNativeScrollInertia(gesture) {
                    const minimumVelocity = 0.01;
                    if (Math.abs(gesture.velocityX) < minimumVelocity && Math.abs(gesture.velocityY) < minimumVelocity) {
                        return;
                    }
                    this.cancelNativeScrollInertia();
                    const session = this._nativeScrollInertiaSession;
                    this._nativeScrollInertiaGesture = gesture;
                    let lastTimestamp = performance.now();
                    const stop = () => {
                        this._nativeScrollInertiaFrame = null;
                        this._nativeScrollInertiaGesture = null;
                        this.completeNativeScroll(gesture);
                    };
                    const step = (timestamp) => {
                        if (session !== this._nativeScrollInertiaSession) {
                            return;
                        }
                        // The element can be removed or the app torn down mid-fling; keep flinging only what is
                        // still on screen.
                        if (!gesture.root.isConnected) {
                            stop();
                            return;
                        }
                        const elapsed = Math.min(timestamp - lastTimestamp, 64);
                        lastTimestamp = timestamp;
                        gesture.velocityX *= Math.pow(0.95, elapsed / (1000 / 60));
                        gesture.velocityY *= Math.pow(0.95, elapsed / (1000 / 60));
                        if (Math.abs(gesture.velocityX) < minimumVelocity && Math.abs(gesture.velocityY) < minimumVelocity) {
                            stop();
                            return;
                        }
                        try {
                            // isInertial lets the managed side honor ScrollViewer.IsScrollInertiaEnabled: a delta refused
                            // there stops the fling, while the native scrollTarget keeps its own momentum until then.
                            if (this.applyNativeScrollDelta(gesture, gesture.velocityX * elapsed, gesture.velocityY * elapsed, /* isIntermediate */ true, /* isInertial */ true)) {
                                this._nativeScrollInertiaFrame = requestAnimationFrame(step);
                            }
                            else {
                                stop();
                            }
                        }
                        catch (e) {
                            console.warn(`Failed to apply negotiated native scroll inertia: ${e}`);
                            stop();
                        }
                    };
                    this._nativeScrollInertiaFrame = requestAnimationFrame(step);
                }
                tryHandleNegotiatedNativeInput(evt) {
                    const host = this.getNativeInputHost(evt.target);
                    if (host === null || host.policy === NativeElementInputPolicy.NativeOnly) {
                        return false;
                    }
                    const root = host.root;
                    if (evt.target instanceof Element && evt.target.closest(BrowserPointerInputSource.nativeInputSelector) !== null) {
                        return false;
                    }
                    if (evt instanceof WheelEvent) {
                        // Ctrl+wheel is browser zoom, not scroll - leave it to the normal pipeline, which already
                        // carves it out when BrowserInputHelper.isBrowserZoomEnabled.
                        if (evt.ctrlKey && Runtime.BrowserInputHelper.isBrowserZoomEnabled) {
                            return false;
                        }
                        // A wheel burst is not a touch gesture, but it still supersedes a fling that is still running.
                        this.cancelNativeScrollInertia();
                        const gesture = {
                            pointerId: 0,
                            root,
                            unoElementId: host.unoElementId,
                            scrollTarget: this.findScrollableElement(evt.target, root),
                            startX: evt.clientX,
                            startY: evt.clientY,
                            lastX: evt.clientX,
                            lastY: evt.clientY,
                            lastTimestamp: evt.timeStamp,
                            velocityX: 0,
                            velocityY: 0,
                            started: true,
                            primaryAxis: Math.abs(evt.deltaX) > Math.abs(evt.deltaY) ? "horizontal" : "vertical",
                            unoOwnsGesture: false,
                        };
                        const didScroll = this.applyNativeScrollDelta(gesture, evt.deltaX, evt.deltaY);
                        if (didScroll) {
                            evt.preventDefault();
                        }
                        return true;
                    }
                    if (evt.pointerType !== "touch" && evt.pointerType !== "pen") {
                        return false;
                    }
                    if (evt.type === "pointerdown") {
                        if (this._nativeScrollGesture !== null && this._nativeScrollGesture.pointerId !== evt.pointerId) {
                            // A second finger means a multi-touch gesture (pinch/zoom) which this negotiation does not
                            // arbitrate. Drop our single-pointer gesture and leave the whole interaction to the browser,
                            // which still has pinch-zoom available (see the touch-action rule in uno.css).
                            this.completeNativeScroll(this._nativeScrollGesture);
                            this._nativeScrollGesture = null;
                            return false;
                        }
                        this.cancelNativeScrollInertia();
                        this._nativeScrollGesture = {
                            pointerId: evt.pointerId,
                            root,
                            unoElementId: host.unoElementId,
                            scrollTarget: this.findScrollableElement(evt.target, root),
                            startX: evt.clientX,
                            startY: evt.clientY,
                            lastX: evt.clientX,
                            lastY: evt.clientY,
                            lastTimestamp: evt.timeStamp,
                            velocityX: 0,
                            velocityY: 0,
                            started: false,
                            primaryAxis: null,
                            unoOwnsGesture: false,
                        };
                        return true;
                    }
                    const gesture = this._nativeScrollGesture;
                    if (gesture === null || gesture.pointerId !== evt.pointerId) {
                        return true;
                    }
                    if (evt.type === "pointercancel") {
                        this._nativeScrollGesture = null;
                        this.completeNativeScroll(gesture);
                        return true;
                    }
                    if (evt.type === "pointermove") {
                        let horizontalDelta = gesture.lastX - evt.clientX;
                        let verticalDelta = gesture.lastY - evt.clientY;
                        if (!gesture.started) {
                            if (Math.hypot(evt.clientX - gesture.startX, evt.clientY - gesture.startY) < 8) {
                                gesture.lastX = evt.clientX;
                                gesture.lastY = evt.clientY;
                                gesture.lastTimestamp = evt.timeStamp;
                                return true;
                            }
                            gesture.started = true;
                            horizontalDelta = gesture.startX - evt.clientX;
                            verticalDelta = gesture.startY - evt.clientY;
                            gesture.primaryAxis = Math.abs(horizontalDelta) > Math.abs(verticalDelta) ? "horizontal" : "vertical";
                        }
                        const elapsed = Math.max(evt.timeStamp - gesture.lastTimestamp, 1);
                        gesture.velocityX = gesture.velocityX * 0.7 + horizontalDelta / elapsed * 0.3;
                        gesture.velocityY = gesture.velocityY * 0.7 + verticalDelta / elapsed * 0.3;
                        gesture.lastX = evt.clientX;
                        gesture.lastY = evt.clientY;
                        gesture.lastTimestamp = evt.timeStamp;
                        this.applyNativeScrollDelta(gesture, horizontalDelta, verticalDelta);
                        evt.preventDefault();
                        return true;
                    }
                    if (evt.type === "pointerup") {
                        this._nativeScrollGesture = null;
                        // startNativeScrollInertia reports the completion itself once the fling decays, so only close
                        // the sequence here when no fling is taking over.
                        if (gesture.started) {
                            this.startNativeScrollInertia(gesture);
                            if (this._nativeScrollInertiaGesture === null) {
                                this.completeNativeScroll(gesture);
                            }
                        }
                        else {
                            this.completeNativeScroll(gesture);
                        }
                    }
                    return true;
                }
                onPointerEventReceived(evt) {
                    var _a;
                    let id = (_a = evt.target) === null || _a === void 0 ? void 0 : _a.id;
                    if (id === "uno-enable-accessibility") {
                        // We have a div to enable accessibility (see enableA11y in WebAssemblyWindowWrapper).
                        // Pressing space on keyboard to click it will trigger pointer event which we want to ignore.
                        return;
                    }
                    if (this.isEventFromNativeElementHost(evt.target)) {
                        if (this.tryHandleNegotiatedNativeInput(evt)) {
                            return;
                        }
                        // Events from the native host are handled by the native control directly.
                        // We don't want to interfere with them.
                        return;
                    }
                    const event = BrowserPointerInputSource.toHtmlPointerEvent(evt.type);
                    let pointerId, pointerType, pressure;
                    let wheelDeltaX, wheelDeltaY;
                    if (evt instanceof WheelEvent) {
                        pointerId = evt.mozInputSource ? 0 : 1; // Try to match the mouse pointer ID 0 for FF, 1 for others
                        pointerType = PointerDeviceType.Mouse;
                        pressure = 0.5; // like WinUI
                        wheelDeltaX = evt.deltaX;
                        wheelDeltaY = evt.deltaY;
                        switch (evt.deltaMode) {
                            case WheelEvent.DOM_DELTA_LINE: // Actually this is supported only by FF
                                const lineSize = BrowserPointerInputSource.wheelLineSize;
                                wheelDeltaX *= lineSize;
                                wheelDeltaY *= lineSize;
                                break;
                            case WheelEvent.DOM_DELTA_PAGE:
                                wheelDeltaX *= document.documentElement.clientWidth;
                                wheelDeltaY *= document.documentElement.clientHeight;
                                break;
                        }
                    }
                    else {
                        pointerId = evt.pointerId;
                        pointerType = BrowserPointerInputSource.toPointerDeviceType(evt.pointerType);
                        pressure = evt.pressure;
                        wheelDeltaX = 0;
                        wheelDeltaY = 0;
                    }
                    const result = BrowserPointerInputSource._exports.OnNativeEvent(this._source, event, //byte @event, // ONE of NativePointerEvent
                    evt.timeStamp, //double timestamp,
                    pointerType, //int deviceType, // ONE of _PointerDeviceType
                    pointerId, //double pointerId, // Warning: This is a Number in JS, and it might be negative on safari for iOS
                    evt.clientX, //double x,
                    evt.clientY, //double y,
                    evt.ctrlKey, //bool ctrl,
                    evt.shiftKey, //bool shift,
                    evt.buttons, //int buttons,
                    evt.button, //int buttonUpdate,
                    pressure, //double pressure,
                    wheelDeltaX, //double wheelDeltaX,
                    wheelDeltaY, //double wheelDeltaY,
                    evt.relatedTarget !== null //bool hasRelatedTarget)
                    );
                    // pointer events may have some side effects (like changing focus or opening a context menu on right clicking)
                    // We blanket-disable all the native behaviour so we don't have to whack-a-mole all the edge cases.
                    // We only allow wheel events with ctrl key pressed to allow zooming in/out when BrowserInputHelper.isBrowserZoomEnabled is true.
                    const isZooming = Runtime.BrowserInputHelper.isBrowserZoomEnabled && evt instanceof WheelEvent && evt.ctrlKey;
                    if (result == HtmlEventDispatchResult.PreventDefault ||
                        !isZooming) {
                        evt.preventDefault();
                    }
                }
                static get wheelLineSize() {
                    // In web browsers, scroll might happen by pixels, line or page.
                    // But WinUI works only with pixels, so we have to convert it before send the value to the managed code.
                    // The issue is that there is no easy way get the "size of a line", instead we have to determine the CSS "line-height"
                    // defined in the browser settings. 
                    // https://stackoverflow.com/questions/20110224/what-is-the-height-of-a-line-in-a-wheel-event-deltamode-dom-delta-line
                    if (this._wheelLineSize == undefined) {
                        const el = document.createElement("div");
                        el.style.fontSize = "initial";
                        el.style.display = "none";
                        document.body.appendChild(el);
                        const fontSize = window.getComputedStyle(el).fontSize;
                        document.body.removeChild(el);
                        this._wheelLineSize = fontSize ? parseInt(fontSize) : 16; /* 16 = The current common default font size */
                        // Based on observations, even if the event reports 3 lines (the settings of windows),
                        // the browser will actually scroll of about 6 lines of text.
                        this._wheelLineSize *= 2.0;
                    }
                    return this._wheelLineSize;
                }
                //#endregion
                //#region Helpers
                static toHtmlPointerEvent(eventName) {
                    switch (eventName) {
                        case "pointerover":
                            return HtmlPointerEvent.pointerover;
                        case "pointerout":
                            return HtmlPointerEvent.pointerout;
                        case "pointerdown":
                            return HtmlPointerEvent.pointerdown;
                        case "pointerup":
                            return HtmlPointerEvent.pointerup;
                        case "pointercancel":
                            return HtmlPointerEvent.pointercancel;
                        case "pointermove":
                            return HtmlPointerEvent.pointermove;
                        case "wheel":
                            return HtmlPointerEvent.wheel;
                        default:
                            return undefined;
                    }
                }
                static toPointerDeviceType(type) {
                    switch (type) {
                        case "touch":
                            return PointerDeviceType.Touch;
                        case "pen":
                            // Note: As of 2019-11-28, once pen pressed events pressed/move/released are reported as TOUCH on Firefox
                            //		 https://bugzilla.mozilla.org/show_bug.cgi?id=1449660
                            return PointerDeviceType.Pen;
                        case "mouse":
                        default:
                            return PointerDeviceType.Mouse;
                    }
                }
            }
            BrowserPointerInputSource._instance = null;
            // Mirrors the exclusions in uno.css: these keep their native touch-action, so the browser still pans
            // and selects in them. A gesture anywhere inside one stays entirely native - negotiating it here as
            // well would apply the browser's own panning a second time. Iframes are in the list because pointer
            // events do not cross the frame boundary: the embedded document stays native-only until it explicitly
            // registers a bridge capable of reporting its scroll residual.
            BrowserPointerInputSource.nativeInputSelector = "input, textarea, select, iframe, [contenteditable]";
            //#region WheelLineSize
            BrowserPointerInputSource._wheelLineSize = undefined;
            Runtime.BrowserPointerInputSource = BrowserPointerInputSource;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class BrowserRenderer {
                constructor(managedHandle, canvas) {
                    this.canvas = canvas;
                    this.managedHandle = managedHandle;
                    const skiaSharpExports = Runtime.WebAssemblyWindowWrapper.getAssemblyExports();
                    this.requestRender = () => skiaSharpExports.Uno.UI.Runtime.BrowserRenderer.RenderFrame(this.managedHandle);
                    this.setCanvasSize();
                    window.addEventListener("resize", x => this.setCanvasSize());
                }
                static createInstance(managedHandle, canvasId) {
                    if (!canvasId)
                        throw 'No <canvas> element or ID was provided';
                    const canvas = document.getElementById(canvasId);
                    if (!canvas)
                        throw `No <canvas> with id ${canvasId} was found`;
                    return new BrowserRenderer(managedHandle, canvas);
                }
                setCanvasSize() {
                    var scale = window.devicePixelRatio || 1;
                    var rect = document.documentElement.getBoundingClientRect();
                    var width = rect.width;
                    var height = rect.height;
                    var w = width * scale;
                    var h = height * scale;
                    if (this.canvas.width !== w)
                        this.canvas.width = w;
                    if (this.canvas.height !== h)
                        this.canvas.height = h;
                    // We request to repaint on the next frame. Without this, the first frame after resizing the window will be
                    // blank and will cause a flickering effect when you drag the window's border to resize.
                    // See also https://github.com/unoplatform/uno-private/issues/902.
                    BrowserRenderer.invalidate(this);
                }
                static invalidate(instance) {
                    window.requestAnimationFrame(() => {
                        instance.requestRender();
                    });
                }
            }
            Runtime.BrowserRenderer = BrowserRenderer;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            // Shared helper for creating emscripten-managed WebGL contexts (window.GL) with a
            // WebGL 1.0 fallback. Used by both WebGlBrowserRenderer (the onscreen Skia render
            // surface) and WasmNativeOpenGLWrapper (GLCanvasElement's offscreen contexts).
            class EmscriptenWebGL {
                static assertGL() {
                    const anyGL = window.GL;
                    if (!anyGL || typeof anyGL.createContext !== "function") {
                        throw "Emscripten GL module (window.GL) is unavailable";
                    }
                    return anyGL;
                }
                // attributeOverrides adjusts the defaults per call site (e.g. antialias for the
                // onscreen Skia surface, preserveDrawingBuffer for GLCanvasElement readbacks).
                static createContext(canvas, attributeOverrides = {}) {
                    var _a;
                    const anyGL = EmscriptenWebGL.assertGL();
                    const contextAttributes = Object.assign({ alpha: 1, depth: 1, stencil: 8, antialias: 0, premultipliedAlpha: 1, preserveDrawingBuffer: 0, preferLowPowerToHighPerformance: 0, failIfMajorPerformanceCaveat: 0, majorVersion: 2, minorVersion: 0, enableExtensionsByDefault: 1, explicitSwapControl: 0, renderViaOffscreenBackBuffer: 0 }, attributeOverrides);
                    let ctx = anyGL.createContext(canvas, contextAttributes);
                    if (!ctx && contextAttributes.majorVersion > 1) {
                        console.warn("EmscriptenWebGL: falling back to WebGL 1.0");
                        contextAttributes.majorVersion = 1;
                        contextAttributes.minorVersion = 0;
                        ctx = anyGL.createContext(canvas, contextAttributes);
                    }
                    const gl = ctx > 0 ? (_a = anyGL.getContext(ctx)) === null || _a === void 0 ? void 0 : _a.GLctx : null;
                    if (gl) {
                        EmscriptenWebGL.fixStridedUploads(gl);
                    }
                    return ctx;
                }
                // With MAXIMUM_MEMORY above 2GB, emscripten avoids its garbage-free WebGL2 upload path (Firefox bug
                // 1838218) and slices the source as height * width rows, ignoring UNPACK_ROW_LENGTH and SKIP_*.
                // Skia uploads glyph atlas patches with a row length, so WebGL rejected them and glyphs went blank.
                // Re-slice such views to the strided size WebGL validates against. The 3D uploads aren't wrapped: Skia doesn't use them.
                static fixStridedUploads(gl) {
                    const anyGl = gl;
                    // WebGL 1 has no row length to honor.
                    if (anyGl.__unoStridedUploadsFixed || typeof anyGl.UNPACK_ROW_LENGTH !== "number") {
                        return;
                    }
                    anyGl.__unoStridedUploadsFixed = true;
                    const unpack = { alignment: 4, rowLength: 0, skipRows: 0, skipPixels: 0 };
                    const pixelStorei = gl.pixelStorei;
                    anyGl.pixelStorei = function (pname, param) {
                        switch (pname) {
                            case gl.UNPACK_ALIGNMENT:
                                unpack.alignment = param;
                                break;
                            case gl.UNPACK_ROW_LENGTH:
                                unpack.rowLength = param;
                                break;
                            case gl.UNPACK_SKIP_ROWS:
                                unpack.skipRows = param;
                                break;
                            case gl.UNPACK_SKIP_PIXELS:
                                unpack.skipPixels = param;
                                break;
                        }
                        return pixelStorei.call(gl, pname, param);
                    };
                    const isStrided = () => unpack.rowLength !== 0 || unpack.skipRows !== 0 || unpack.skipPixels !== 0;
                    const resize = (width, height, format, type, pixels) => {
                        if (!ArrayBuffer.isView(pixels)) {
                            return pixels;
                        }
                        const bytesPerPixel = EmscriptenWebGL.bytesPerPixel(gl, format, type);
                        if (!bytesPerPixel || width <= 0 || height <= 0) {
                            return pixels;
                        }
                        const alignment = unpack.alignment;
                        const rowBytes = Math.ceil(((unpack.rowLength || width) * bytesPerPixel) / alignment) * alignment;
                        const required = (unpack.skipRows + height - 1) * rowBytes + (unpack.skipPixels + width) * bytesPerPixel;
                        const view = pixels;
                        if (view.byteLength >= required || view.byteOffset + required > view.buffer.byteLength) {
                            return pixels;
                        }
                        // Rounding up to whole elements must not run past the end of the buffer.
                        const elementSize = view.BYTES_PER_ELEMENT || 1;
                        const length = Math.min(Math.ceil(required / elementSize), Math.floor((view.buffer.byteLength - view.byteOffset) / elementSize));
                        return new view.constructor(view.buffer, view.byteOffset, length);
                    };
                    const texImage2D = gl.texImage2D;
                    anyGl.texImage2D = function () {
                        // (target, level, internalformat, width, height, border, format, type, pixels)
                        if (arguments.length !== 9 || !isStrided()) {
                            return texImage2D.apply(gl, arguments);
                        }
                        const args = Array.prototype.slice.call(arguments);
                        args[8] = resize(args[3], args[4], args[6], args[7], args[8]);
                        return texImage2D.apply(gl, args);
                    };
                    const texSubImage2D = gl.texSubImage2D;
                    anyGl.texSubImage2D = function () {
                        // (target, level, xoffset, yoffset, width, height, format, type, pixels)
                        if (arguments.length !== 9 || !isStrided()) {
                            return texSubImage2D.apply(gl, arguments);
                        }
                        const args = Array.prototype.slice.call(arguments);
                        args[8] = resize(args[4], args[5], args[6], args[7], args[8]);
                        return texSubImage2D.apply(gl, args);
                    };
                }
                static bytesPerPixel(gl, format, type) {
                    switch (type) {
                        case gl.UNSIGNED_SHORT_5_6_5:
                        case gl.UNSIGNED_SHORT_4_4_4_4:
                        case gl.UNSIGNED_SHORT_5_5_5_1:
                            return 2;
                        case gl.UNSIGNED_INT_2_10_10_10_REV:
                        case gl.UNSIGNED_INT_10F_11F_11F_REV:
                        case gl.UNSIGNED_INT_5_9_9_9_REV:
                        case gl.UNSIGNED_INT_24_8:
                            return 4;
                    }
                    let componentSize;
                    switch (type) {
                        case gl.UNSIGNED_BYTE:
                        case gl.BYTE:
                            componentSize = 1;
                            break;
                        case gl.UNSIGNED_SHORT:
                        case gl.SHORT:
                        case gl.HALF_FLOAT:
                            componentSize = 2;
                            break;
                        case gl.UNSIGNED_INT:
                        case gl.INT:
                        case gl.FLOAT:
                            componentSize = 4;
                            break;
                        default:
                            return 0;
                    }
                    switch (format) {
                        case gl.RED:
                        case gl.RED_INTEGER:
                        case gl.ALPHA:
                        case gl.LUMINANCE:
                        case gl.DEPTH_COMPONENT:
                            return componentSize;
                        case gl.RG:
                        case gl.RG_INTEGER:
                        case gl.LUMINANCE_ALPHA:
                            return componentSize * 2;
                        case gl.RGB:
                        case gl.RGB_INTEGER:
                            return componentSize * 3;
                        case gl.RGBA:
                        case gl.RGBA_INTEGER:
                            return componentSize * 4;
                        default:
                            return 0;
                    }
                }
            }
            Runtime.EmscriptenWebGL = EmscriptenWebGL;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            /**
             * Modal focus trap for ContentDialog and modal overlays.
             * Manages aria-hidden on background, Tab/Shift+Tab wrapping,
             * nested modal support, and focus restoration on close.
             */
            class FocusTrap {
                /**
                 * Activates a focus trap for a modal dialog.
                 * Hides background elements and starts Tab wrapping.
                 */
                static activateFocusTrap(modalHandle, triggerHandle, focusableHandles) {
                    const parentState = FocusTrap.activeTrap;
                    // Hide all semantic elements outside the modal
                    const semanticsRoot = document.getElementById("uno-semantics-root");
                    const modalElement = document.getElementById(`uno-semantics-${modalHandle}`);
                    const hiddenElements = [];
                    if (semanticsRoot && modalElement) {
                        const allElements = semanticsRoot.querySelectorAll("[id^='uno-semantics-']");
                        allElements.forEach((el) => {
                            if (el !== modalElement && !modalElement.contains(el)) {
                                hiddenElements.push({
                                    element: el,
                                    originalAriaHidden: el.getAttribute("aria-hidden"),
                                    originalTabIndex: el.getAttribute("tabindex")
                                });
                                el.setAttribute("aria-hidden", "true");
                                el.setAttribute("tabindex", "-1");
                            }
                        });
                    }
                    // Set role="dialog" on modal element
                    if (modalElement) {
                        modalElement.setAttribute("role", "dialog");
                        modalElement.setAttribute("aria-modal", "true");
                    }
                    // Create keydown handler for Tab wrapping.
                    // Use capture phase so user code cannot stopPropagation() to escape the trap.
                    const keydownHandler = (e) => {
                        if (e.key === "Tab") {
                            const wrapped = FocusTrap.handleTrapTab(modalHandle, e.shiftKey);
                            if (wrapped) {
                                e.preventDefault();
                            }
                        }
                    };
                    document.addEventListener("keydown", keydownHandler, true);
                    FocusTrap.activeTrap = {
                        modalHandle,
                        triggerHandle,
                        focusableHandles,
                        hiddenElements,
                        keydownHandler,
                        parentState
                    };
                    // Focus the first focusable element in the modal
                    if (focusableHandles.length > 0) {
                        const firstElement = document.getElementById(`uno-semantics-${focusableHandles[0]}`);
                        if (firstElement) {
                            firstElement.focus();
                        }
                    }
                }
                /**
                 * Deactivates the focus trap for a modal dialog.
                 * Restores background elements and focus.
                 */
                static deactivateFocusTrap(modalHandle) {
                    var _a;
                    const trap = FocusTrap.activeTrap;
                    if (!trap) {
                        return;
                    }
                    // Handle out-of-order deactivation: if the requested modal is not
                    // the topmost, walk the parent chain to find and remove it.
                    if (trap.modalHandle !== modalHandle) {
                        let current = trap;
                        while (current) {
                            if (((_a = current.parentState) === null || _a === void 0 ? void 0 : _a.modalHandle) === modalHandle) {
                                const target = current.parentState;
                                document.removeEventListener("keydown", target.keydownHandler, true);
                                FocusTrap.restoreHiddenElements(target);
                                FocusTrap.removeDialogRole(target.modalHandle);
                                // Splice out of linked list
                                current.parentState = target.parentState;
                                return;
                            }
                            current = current.parentState;
                        }
                        return;
                    }
                    // Remove keydown handler (must match capture phase used in activate)
                    document.removeEventListener("keydown", trap.keydownHandler, true);
                    // Restore hidden elements
                    FocusTrap.restoreHiddenElements(trap);
                    // Remove dialog role
                    FocusTrap.removeDialogRole(modalHandle);
                    // Reactivate parent trap or clear
                    FocusTrap.activeTrap = trap.parentState;
                    // Restore focus to trigger element, with fallback to parent trap or body
                    if (trap.triggerHandle) {
                        const triggerElement = document.getElementById(`uno-semantics-${trap.triggerHandle}`);
                        if (triggerElement) {
                            triggerElement.focus();
                        }
                        else if (trap.parentState && trap.parentState.focusableHandles.length > 0) {
                            const fallback = document.getElementById(`uno-semantics-${trap.parentState.focusableHandles[0]}`);
                            if (fallback) {
                                fallback.focus();
                            }
                        }
                    }
                }
                /**
                 * Updates the focusable children within a modal.
                 */
                static updateFocusTrapChildren(modalHandle, focusableHandles) {
                    if (FocusTrap.activeTrap && FocusTrap.activeTrap.modalHandle === modalHandle) {
                        FocusTrap.activeTrap.focusableHandles = focusableHandles;
                    }
                }
                /**
                 * Handles Tab/Shift+Tab within a focus trap.
                 * Returns true if focus was wrapped.
                 */
                static handleTrapTab(modalHandle, shiftKey) {
                    const trap = FocusTrap.activeTrap;
                    if (!trap || trap.modalHandle !== modalHandle || trap.focusableHandles.length === 0) {
                        return false;
                    }
                    const activeElement = document.activeElement;
                    const handles = trap.focusableHandles;
                    // Find current position in focusable list
                    let currentIndex = -1;
                    for (let i = 0; i < handles.length; i++) {
                        if ((activeElement === null || activeElement === void 0 ? void 0 : activeElement.id) === `uno-semantics-${handles[i]}`) {
                            currentIndex = i;
                            break;
                        }
                    }
                    if (shiftKey) {
                        // Shift+Tab: wrap from first to last
                        if (currentIndex <= 0) {
                            const lastElement = document.getElementById(`uno-semantics-${handles[handles.length - 1]}`);
                            if (lastElement) {
                                lastElement.focus();
                                return true;
                            }
                        }
                    }
                    else {
                        // Tab: wrap from last to first
                        if (currentIndex >= handles.length - 1) {
                            const firstElement = document.getElementById(`uno-semantics-${handles[0]}`);
                            if (firstElement) {
                                firstElement.focus();
                                return true;
                            }
                        }
                    }
                    return false;
                }
                /**
                 * Returns whether a focus trap is currently active.
                 */
                static isFocusTrapActive() {
                    return FocusTrap.activeTrap !== null;
                }
                /**
                 * Returns the handle of the active modal, or 0 if no trap is active.
                 */
                static getActiveTrapHandle() {
                    var _a, _b;
                    return (_b = (_a = FocusTrap.activeTrap) === null || _a === void 0 ? void 0 : _a.modalHandle) !== null && _b !== void 0 ? _b : 0;
                }
                static restoreHiddenElements(trap) {
                    for (const item of trap.hiddenElements) {
                        if (item.originalAriaHidden !== null) {
                            item.element.setAttribute("aria-hidden", item.originalAriaHidden);
                        }
                        else {
                            item.element.removeAttribute("aria-hidden");
                        }
                        if (item.originalTabIndex !== null) {
                            item.element.setAttribute("tabindex", item.originalTabIndex);
                        }
                        else {
                            item.element.removeAttribute("tabindex");
                        }
                    }
                }
                static removeDialogRole(modalHandle) {
                    const modalElement = document.getElementById(`uno-semantics-${modalHandle}`);
                    if (modalElement) {
                        modalElement.removeAttribute("role");
                        modalElement.removeAttribute("aria-modal");
                    }
                }
            }
            FocusTrap.activeTrap = null;
            Runtime.FocusTrap = FocusTrap;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class ImageLoader {
                static createWorker() {
                    ImageLoader.workerCount++;
                    const worker = new Worker(ImageLoader.workerScriptUrl);
                    const listener = (ev) => {
                        const promiseResolver = ImageLoader.pendingPromiseResolvers.get(ev.data.seqNo);
                        ImageLoader.pendingPromiseResolvers.delete(ev.data.seqNo);
                        promiseResolver(ev.data.response);
                        if (ImageLoader.pendingJobs.length == 0) {
                            ImageLoader.availableWorkers.push(worker);
                        }
                        else {
                            const job = ImageLoader.pendingJobs.pop();
                            window.setImmediate(() => job(worker));
                        }
                    };
                    worker.addEventListener("message", listener);
                    return worker;
                }
                static loadFromArray(array) {
                    const seqNo = ++ImageLoader.sequenceNumber;
                    return new Promise((resolve) => {
                        ImageLoader.pendingPromiseResolvers.set(seqNo, resolve);
                        if (ImageLoader.availableWorkers.length != 0) {
                            ImageLoader.availableWorkers.pop().postMessage([array, seqNo], [array.buffer]);
                        }
                        else if (ImageLoader.workerCount < ImageLoader.maxWorkers) {
                            ImageLoader.createWorker().postMessage([array, seqNo], [array.buffer]);
                        }
                        else {
                            ImageLoader.pendingJobs.push(worker => worker.postMessage([array, seqNo], [array.buffer]));
                        }
                    });
                }
            }
            ImageLoader.workerScriptUrl = URL.createObjectURL(new Blob([`
const offscreenCanvas = new OffscreenCanvas(0, 0);
const context = offscreenCanvas.getContext("2d", { willReadFrequently: true });
const readPixels = async (array) => {
	const imageBitmap = await createImageBitmap(new Blob([array]), { premultiplyAlpha: "premultiply" });
	try {
		offscreenCanvas.width = imageBitmap.width;
		offscreenCanvas.height = imageBitmap.height;
		context.drawImage(imageBitmap, 0, 0);
		return context.getImageData(0, 0, imageBitmap.width, imageBitmap.height);
	} finally {
		// Workers live as long as the app: without this, each one keeps the last image's pixels in its
		// canvas (until the next decode resizes it) and in the bitmap (until the worker's own GC).
		imageBitmap.close();
		offscreenCanvas.width = 0;
		offscreenCanvas.height = 0;
	}
};
onmessage = async (e) => {
	const [array, seqNo] = e.data;
	try {
		const imageData = await readPixels(array);

		// Due to a bug in Skia on WASM, we need the pixels to be
		// alpha-premultiplied because using SKAlphaType.Unpremul is not working
		// correctly (see also https://github.com/unoplatform/uno/issues/20727),
		// so we multiply the RGB values by the alpha by hand instead.
		// This is somehow a LOT faster than doing it with webgl in a fragment shader
		const buffer = imageData.data;
		for (let i = 0; i < buffer.byteLength; i += 4) {
			const a = buffer[i + 3];
			buffer[i] = (buffer[i] * a) / 255;
			buffer[i + 1] = (buffer[i + 1] * a) / 255;
			buffer[i + 2] = (buffer[i + 2] * a) / 255;
		}
		postMessage({
			seqNo: seqNo,
			response: {
				error: null,
				bytes: new Uint8Array(imageData.data.buffer), // does not copy
				width: imageData.width,
				height: imageData.height
			}
		},
		[imageData.data.buffer]);
	} catch (e) {
		postMessage({seqNo: seqNo, response: { error: e.toString() }});
	}
};`], { type: 'application/javascript' }));
            // Workers are created on demand, up to one per logical processor: each one is an isolate with its own heap
            // and canvas, so creating them all up front costs every app memory whether or not it decodes an image.
            ImageLoader.maxWorkers = Math.max(1, navigator.hardwareConcurrency || 1);
            ImageLoader.workerCount = 0;
            ImageLoader.availableWorkers = [];
            ImageLoader.pendingJobs = new Array();
            ImageLoader.sequenceNumber = 0;
            ImageLoader.pendingPromiseResolvers = new Map();
            Runtime.ImageLoader = ImageLoader;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            /**
             * Live Region Manager for screen reader announcements.
             * Creates polite and assertive aria-live divs and manages content updates
             * with two-tier rate limiting (debounce + sustained throttle).
             */
            class LiveRegion {
                /**
                 * Initializes the live region elements.
                 * Called during accessibility activation.
                 */
                static initialize() {
                    const container = document.getElementById("uno-body");
                    if (!container) {
                        return;
                    }
                    // Create polite live region
                    LiveRegion.politeRegion = document.createElement("div");
                    LiveRegion.politeRegion.id = "uno-live-region-polite";
                    LiveRegion.politeRegion.setAttribute("aria-live", "polite");
                    LiveRegion.politeRegion.setAttribute("aria-atomic", "true");
                    LiveRegion.politeRegion.style.position = "absolute";
                    LiveRegion.politeRegion.style.width = "1px";
                    LiveRegion.politeRegion.style.height = "1px";
                    LiveRegion.politeRegion.style.overflow = "hidden";
                    LiveRegion.politeRegion.style.clip = "rect(0, 0, 0, 0)";
                    LiveRegion.politeRegion.style.whiteSpace = "nowrap";
                    LiveRegion.applyLangDir(LiveRegion.politeRegion);
                    container.appendChild(LiveRegion.politeRegion);
                    // Create assertive live region
                    LiveRegion.assertiveRegion = document.createElement("div");
                    LiveRegion.assertiveRegion.id = "uno-live-region-assertive";
                    LiveRegion.assertiveRegion.setAttribute("aria-live", "assertive");
                    LiveRegion.assertiveRegion.setAttribute("aria-atomic", "true");
                    LiveRegion.assertiveRegion.style.position = "absolute";
                    LiveRegion.assertiveRegion.style.width = "1px";
                    LiveRegion.assertiveRegion.style.height = "1px";
                    LiveRegion.assertiveRegion.style.overflow = "hidden";
                    LiveRegion.assertiveRegion.style.clip = "rect(0, 0, 0, 0)";
                    LiveRegion.assertiveRegion.style.whiteSpace = "nowrap";
                    LiveRegion.applyLangDir(LiveRegion.assertiveRegion);
                    container.appendChild(LiveRegion.assertiveRegion);
                }
                /**
                 * Updates live region content. Called from C# after rate limiting.
                 * @param handle - Element handle (unused, reserved for future per-element regions)
                 * @param content - Text content to announce
                 * @param liveSetting - 0=Off, 1=Polite, 2=Assertive
                 */
                static updateLiveRegionContent(handle, content, liveSetting) {
                    const region = liveSetting === 2 ? LiveRegion.assertiveRegion : LiveRegion.politeRegion;
                    if (!region) {
                        return;
                    }
                    // Clear and re-set to trigger screen reader announcement.
                    // Use setTimeout(0) instead of requestAnimationFrame to ensure the DOM
                    // mutation crosses a task boundary — some screen readers (NVDA, JAWS) require
                    // this to detect the change and announce the new content.
                    region.textContent = "";
                    setTimeout(() => {
                        region.textContent = content;
                    }, 0);
                }
                /**
                 * Removes the live region elements. Called when accessibility is disabled.
                 */
                static teardown() {
                    var _a, _b;
                    (_a = LiveRegion.politeRegion) === null || _a === void 0 ? void 0 : _a.remove();
                    (_b = LiveRegion.assertiveRegion) === null || _b === void 0 ? void 0 : _b.remove();
                }
                /**
                 * Clears any pending content from both live regions.
                 */
                static clearPendingAnnouncements() {
                    if (LiveRegion.politeRegion) {
                        LiveRegion.politeRegion.textContent = "";
                    }
                    if (LiveRegion.assertiveRegion) {
                        LiveRegion.assertiveRegion.textContent = "";
                    }
                }
                /**
                 * Copies lang and dir from the document root to a live region element
                 * so screen readers use the correct language and reading direction.
                 */
                static applyLangDir(element) {
                    const lang = document.documentElement.lang;
                    const dir = document.documentElement.dir;
                    if (lang) {
                        element.setAttribute("lang", lang);
                    }
                    if (dir) {
                        element.setAttribute("dir", dir);
                    }
                }
            }
            Runtime.LiveRegion = LiveRegion;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Interop;
        (function (Interop) {
            class Runtime {
                static init() {
                    return "";
                }
                static InvokeJS(command) {
                    // Preseve the original emscripten marshalling semantics
                    // to always return a valid string.
                    return String(eval(command) || "");
                }
            }
            Runtime.engine = Runtime.init();
            Interop.Runtime = Runtime;
        })(Interop = UI.Interop || (UI.Interop = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            /**
             * Factory for creating semantic DOM elements.
             * Each element type has specific event handlers that call back to managed code.
             */
            class SemanticElements {
                /**
                 * Gets the semantics root element.
                 */
                static getSemanticsRoot() {
                    return document.getElementById("uno-semantics-root");
                }
                /**
                 * Gets callbacks from Accessibility class.
                 */
                static getCallbacks() {
                    return Runtime.Accessibility.getCallbacks();
                }
                /**
                 * Sets an ARIA string attribute only when the value has meaningful (non-whitespace)
                 * content; otherwise the attribute is removed/omitted entirely.
                 *
                 * Emitting an empty (or whitespace-only) value such as `aria-label=""` is worse than
                 * omitting it: some screen readers announce "blank", and an empty value explicitly
                 * clears the accessible name rather than letting other name sources apply. A control
                 * with no accessible name must therefore have NO such attribute at all. This mirrors
                 * the omit-when-empty guard used by the generic Accessibility.ts setters.
                 */
                static setAriaStringAttribute(element, name, value) {
                    // Normalize: set the trimmed value (not the raw input). Leading/trailing whitespace
                    // in aria-label causes inconsistent AT announcements (e.g. "  Name  " announces
                    // differently across screen readers) and is never the author's intent.
                    const trimmed = value ? value.trim() : "";
                    if (trimmed.length > 0) {
                        element.setAttribute(name, trimmed);
                    }
                    else {
                        element.removeAttribute(name);
                    }
                }
                /**
                 * Removes any existing DOM element with the given id to prevent duplicates.
                 * Returns the removed element (for potential reuse) or null.
                 */
                static removeExistingById(id) {
                    const existing = document.getElementById(id);
                    if (existing) {
                        existing.remove();
                    }
                    return existing;
                }
                /**
                 * Appends a created semantic element to its proper parent in the tree,
                 * or falls back to the semantics root if the parent is not found.
                 * Automatically removes any pre-existing element with the same id.
                 */
                static appendToParent(element, parentHandle, index) {
                    // Remove any pre-existing element with this id to prevent duplicates
                    SemanticElements.removeExistingById(element.id);
                    let parent = null;
                    if (parentHandle !== 0) {
                        parent = document.getElementById(`uno-semantics-${parentHandle}`);
                    }
                    if (!parent) {
                        Runtime.Accessibility.debugWarn(`[A11y] TS appendToParent: parent NOT FOUND handle=${parentHandle} for element=${element.id} — falling back to semanticsRoot`);
                        parent = SemanticElements.getSemanticsRoot();
                    }
                    if (parent) {
                        if (index != null && index < parent.childElementCount) {
                            parent.insertBefore(element, parent.children[index]);
                        }
                        else {
                            parent.appendChild(element);
                        }
                    }
                }
                /**
                 * Applies common positioning and styling to all semantic elements.
                 */
                static applyCommonStyles(element, x, y, width, height, handle) {
                    element.style.position = 'absolute';
                    element.style.left = `${x}px`;
                    element.style.top = `${y}px`;
                    element.style.width = `${width}px`;
                    element.style.height = `${height}px`;
                    element.id = `uno-semantics-${handle}`;
                    const callbacks = this.getCallbacks();
                    // Common event handlers
                    element.addEventListener('focus', () => {
                        if (callbacks.onFocus) {
                            callbacks.onFocus(handle);
                        }
                    });
                    element.addEventListener('blur', () => {
                        if (callbacks.onBlur) {
                            callbacks.onBlur(handle);
                        }
                    });
                    // Apply debug styling if debug mode is enabled
                    if (Runtime.Accessibility.isDebugModeEnabled()) {
                        element.style.outline = "2px solid rgba(0, 255, 0, 0.7)";
                        element.style.backgroundColor = "rgba(0, 255, 0, 0.1)";
                    }
                }
                /**
                 * Creates a button semantic element and appends it to the semantics root.
                 * Called from C# via JSImport.
                 */
                static createButtonElement(parentHandle, handle, index, x, y, width, height, label, disabled, isFocusable) {
                    const element = document.createElement('button');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // A disabled control must not be a tab stop (T017), regardless of the peer focusability.
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable && !disabled);
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (disabled) {
                        element.disabled = true;
                        element.setAttribute('aria-disabled', 'true');
                    }
                    const callbacks = this.getCallbacks();
                    // Click handler for button activation.
                    // <button> natively fires 'click' on both Enter and Space,
                    // so no separate keydown handler is needed.
                    element.addEventListener('click', (e) => {
                        e.preventDefault();
                        if (callbacks.onInvoke) {
                            callbacks.onInvoke(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a toggle button semantic element (button with aria-pressed).
                 * Used for ToggleButton, AppBarToggleButton, etc.
                 * Called from C# via JSImport.
                 */
                static createToggleButtonElement(parentHandle, handle, index, x, y, width, height, label, pressed, disabled, isFocusable) {
                    const element = document.createElement('button');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // A disabled control must not be a tab stop (T017).
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable && !disabled);
                    // aria-pressed for toggle button pattern (distinct from aria-checked for checkboxes)
                    element.setAttribute('aria-pressed', pressed);
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (disabled) {
                        element.disabled = true;
                        element.setAttribute('aria-disabled', 'true');
                    }
                    const callbacks = this.getCallbacks();
                    // <button> natively fires 'click' on both Enter and Space,
                    // so no separate keydown handler is needed.
                    element.addEventListener('click', (e) => {
                        e.preventDefault();
                        if (callbacks.onToggle) {
                            callbacks.onToggle(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a switch semantic element (role="switch" with aria-checked).
                 * Used for ToggleSwitch which maps to the ARIA switch pattern.
                 * Called from C# via JSImport.
                 */
                static createSwitchElement(parentHandle, handle, index, x, y, width, height, label, isOn, disabled, isFocusable) {
                    const element = document.createElement('button');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // A disabled control must not be a tab stop (T017).
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable && !disabled);
                    // role="switch" with aria-checked for ToggleSwitch (ARIA switch pattern)
                    element.setAttribute('role', 'switch');
                    element.setAttribute('aria-checked', isOn);
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (disabled) {
                        element.disabled = true;
                        element.setAttribute('aria-disabled', 'true');
                    }
                    const callbacks = this.getCallbacks();
                    // <button> natively fires 'click' on both Enter and Space,
                    // so no separate keydown handler is needed.
                    element.addEventListener('click', (e) => {
                        e.preventDefault();
                        if (callbacks.onToggle) {
                            callbacks.onToggle(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a slider (range input) semantic element.
                 * Called from C# via JSImport.
                 */
                static createSliderElement(parentHandle, handle, index, x, y, width, height, value, min, max, step, orientation, valueText, isFocusable) {
                    const element = document.createElement('input');
                    element.type = 'range';
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable);
                    // Set range properties
                    element.min = String(min);
                    element.max = String(max);
                    element.value = String(value);
                    element.step = String(step);
                    // Set ARIA value attributes for screen readers
                    element.setAttribute('aria-valuenow', String(value));
                    element.setAttribute('aria-valuemin', String(min));
                    element.setAttribute('aria-valuemax', String(max));
                    // aria-valuetext: VoiceOver reads this instead of the raw number
                    // when a human-readable value description is available
                    this.setAriaStringAttribute(element, 'aria-valuetext', valueText);
                    // aria-orientation reflects the XAML Slider.Orientation so screen readers
                    // announce the axis. Emitted for both axes (the implicit default is
                    // horizontal, but being explicit is clearer for AT and tests).
                    element.setAttribute('aria-orientation', orientation === 'vertical' ? 'vertical' : 'horizontal');
                    if (orientation === 'vertical') {
                        // CSS still drives the native vertical rendering; this is presentational
                        // only and complements the semantic aria-orientation above.
                        element.style.writingMode = 'bt-lr';
                        element.style.webkitAppearance = 'slider-vertical';
                    }
                    const callbacks = this.getCallbacks();
                    // Input event handler for value changes (T030)
                    element.addEventListener('input', () => {
                        if (callbacks.onRangeValueChange) {
                            callbacks.onRangeValueChange(handle, parseFloat(element.value));
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a checkbox semantic element.
                 * Called from C# via JSImport.
                 */
                static createCheckboxElement(parentHandle, handle, index, x, y, width, height, checkedState, label, isFocusable) {
                    const element = document.createElement('input');
                    element.type = 'checkbox';
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable);
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    // Set checked state (WCAG 4.1.2: aria-checked must match visual state)
                    if (checkedState === 'true') {
                        element.checked = true;
                        element.setAttribute('aria-checked', 'true');
                    }
                    else if (checkedState === 'mixed') {
                        element.indeterminate = true;
                        element.setAttribute('aria-checked', 'mixed');
                    }
                    else {
                        element.setAttribute('aria-checked', 'false');
                    }
                    const callbacks = this.getCallbacks();
                    // Change event handler for toggle events (T040)
                    element.addEventListener('change', () => {
                        if (callbacks.onToggle) {
                            callbacks.onToggle(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a radio button semantic element.
                 * Called from C# via JSImport.
                 */
                static createRadioElement(parentHandle, handle, index, x, y, width, height, checked, label, groupName, isFocusable) {
                    const element = document.createElement('input');
                    element.type = 'radio';
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // Roving tabindex: only the checked radio in a group is a tab stop; others are
                    // reachable via arrow keys. A non-focusable (e.g. disabled) radio is never a
                    // tab stop (T017). When no radio in the group is checked yet, the APG roving
                    // pattern requires exactly one tab stop -- promote this radio if it is the
                    // first focusable one in its name group (checked below, after append).
                    element.tabIndex = (isFocusable && checked) ? 0 : -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (groupName) {
                        element.name = groupName;
                    }
                    element.checked = checked;
                    const callbacks = this.getCallbacks();
                    // RadioButton exposes ISelectionItemProvider (not Toggle); route DOM activation to
                    // OnSelection -> Select(). onToggle would be a no-op (the radio peer has no Toggle pattern).
                    element.addEventListener('change', () => {
                        if (callbacks.onSelection) {
                            callbacks.onSelection(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                    // APG roving-tabindex fallback: if this focusable, unchecked radio joined a
                    // named group that still has no tab stop, promote it to tabIndex=0 so the
                    // group can be reached via Tab. Without this, a group where nothing is
                    // initially checked would be entirely unreachable from the keyboard.
                    if (isFocusable && !checked && groupName) {
                        const groupRadios = document.querySelectorAll(`input[type="radio"][name="${CSS.escape(groupName)}"]`);
                        let hasTabStop = false;
                        for (let i = 0; i < groupRadios.length; i++) {
                            const r = groupRadios[i];
                            if (r !== element && r.tabIndex === 0) {
                                hasTabStop = true;
                                break;
                            }
                        }
                        if (!hasTabStop) {
                            element.tabIndex = 0;
                        }
                    }
                }
                /**
                 * Creates a heading semantic element (h1-h6).
                 * VoiceOver uses headings for rotor navigation (VO+U → Headings).
                 * Called from C# via JSImport.
                 */
                static createHeadingElement(parentHandle, handle, index, x, y, width, height, level, label, _isFocusable) {
                    // HTML only has <h1>-<h6>, so the tag is clamped to that range, but the TRUE WinUI
                    // level (1-9) is preserved on aria-level so assistive tech sees the real depth (FR-011).
                    const trueLevel = Math.max(1, Math.min(9, level));
                    const tagLevel = Math.min(6, trueLevel);
                    const element = document.createElement(`h${tagLevel}`);
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // A heading is a structural/rotor landmark, not a tab stop: it gets NO tabindex (T014).
                    // _isFocusable is accepted for interop-signature uniformity but intentionally unused here.
                    element.style.pointerEvents = 'none';
                    // Reset default heading styles so they don't affect layout
                    element.style.margin = '0';
                    element.style.padding = '0';
                    element.style.fontSize = 'inherit';
                    element.style.fontWeight = 'inherit';
                    const trimmedLabel = (label !== null && label !== void 0 ? label : '').trim();
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (trimmedLabel.length > 0) {
                        element.textContent = trimmedLabel;
                    }
                    element.setAttribute('aria-level', String(trueLevel));
                    this.appendToParent(element, parentHandle, index);
                }
                static createTextElement(parentHandle, handle, index, x, y, width, height, text, isBlock, _isFocusable) {
                    const element = document.createElement(isBlock ? 'p' : 'span');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    // Standalone body text: non-interactive, not a tab stop. Only textContent is exposed
                    // (no aria-label, no role) so it is read once as plain text. _isFocusable is unused.
                    element.style.pointerEvents = 'none';
                    element.style.margin = '0';
                    element.style.padding = '0';
                    element.style.fontSize = 'inherit';
                    element.style.fontWeight = 'inherit';
                    element.textContent = text;
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a text input semantic element.
                 * Called from C# via JSImport.
                 */
                static createTextBoxElement(parentHandle, handle, index, x, y, width, height, value, multiline, password, isReadOnly, selectionStart, selectionEnd, selectionIsBackward, isSpellCheckEnabled, isFocusable) {
                    let element;
                    if (multiline) {
                        element = document.createElement('textarea');
                    }
                    else {
                        element = document.createElement('input');
                        element.type = password ? 'password' : 'text';
                    }
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable);
                    element.value = value;
                    const maxLen = value.length;
                    const initialSelectionStart = Math.max(0, Math.min(selectionStart, maxLen));
                    const initialSelectionEnd = Math.max(initialSelectionStart, Math.min(selectionEnd, maxLen));
                    try {
                        element.setSelectionRange(initialSelectionStart, initialSelectionEnd, selectionIsBackward ? 'backward' : 'forward');
                    }
                    catch (_a) {
                        // Some browsers/input types may reject selection updates before focus.
                    }
                    const spellcheck = isSpellCheckEnabled && !password;
                    element.spellcheck = spellcheck;
                    element.setAttribute('spellcheck', String(spellcheck));
                    if (isReadOnly) {
                        element.readOnly = true;
                    }
                    const callbacks = this.getCallbacks();
                    // Block native character insertion so the managed TextBox KeyDown path is the single
                    // source of text edits. Without this, the key is inserted both by the browser (into
                    // this <input>) and by managed OnKeyDownSkia, producing duplicated input once a11y
                    // moves focus to the semantic element instead of the invisible TextBox <input>.
                    Runtime.BrowserInvisibleTextBoxViewExtension.attachTextInputKeyHandlers(element, multiline);
                    element.addEventListener('paste', (event) => {
                        var _a, _b;
                        const source = (_b = (_a = event.clipboardData) === null || _a === void 0 ? void 0 : _a.getData('text')) !== null && _b !== void 0 ? _b : '';
                        const sourceLimit = Runtime.BrowserInvisibleTextBoxViewExtension.getNativePasteSourceLimit();
                        Runtime.BrowserInvisibleTextBoxViewExtension.onNativePaste(source.length > sourceLimit ? source.substring(0, sourceLimit) : source);
                        event.preventDefault();
                    });
                    // Input event handler for text changes (T050)
                    element.addEventListener('input', (ev) => {
                        var _a, _b;
                        // Intermediate IME readings are not edits; compositionend reports the committed value once.
                        if (ev.isComposing) {
                            return;
                        }
                        if (callbacks.onTextInput) {
                            callbacks.onTextInput(handle, element.value, (_a = element.selectionStart) !== null && _a !== void 0 ? _a : 0, (_b = element.selectionEnd) !== null && _b !== void 0 ? _b : 0, element.selectionDirection === 'backward');
                        }
                    });
                    element.addEventListener('select', () => {
                        var _a, _b;
                        if (callbacks.onTextSelectionChanged) {
                            callbacks.onTextSelectionChanged(handle, (_a = element.selectionStart) !== null && _a !== void 0 ? _a : 0, (_b = element.selectionEnd) !== null && _b !== void 0 ? _b : 0, element.selectionDirection === 'backward');
                        }
                    });
                    // Handle IME composition events for international text input (T055)
                    element.addEventListener('compositionend', () => {
                        var _a, _b;
                        if (callbacks.onTextInput) {
                            callbacks.onTextInput(handle, element.value, (_a = element.selectionStart) !== null && _a !== void 0 ? _a : 0, (_b = element.selectionEnd) !== null && _b !== void 0 ? _b : 0, element.selectionDirection === 'backward');
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a combobox semantic element.
                 * Called from C# via JSImport.
                 */
                static createComboBoxElement(parentHandle, handle, index, x, y, width, height, expanded, selectedValue, isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'combobox');
                    element.setAttribute('aria-expanded', String(expanded));
                    // aria-haspopup is applied post-create from the C# value (AriaAttributes.HasPopup),
                    // not hardcoded here (FR-028).
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable);
                    this.setAriaStringAttribute(element, 'aria-label', selectedValue);
                    const callbacks = this.getCallbacks();
                    // Keyboard handlers for expand/collapse (WAI-ARIA combobox pattern)
                    element.addEventListener('keydown', (e) => {
                        if (e.key === 'Enter' || e.key === ' ' || (e.key === 'ArrowDown' && e.altKey)) {
                            e.preventDefault();
                            if (callbacks.onExpandCollapse) {
                                callbacks.onExpandCollapse(handle);
                            }
                        }
                        else if (e.key === 'Escape') {
                            // Escape collapses an open popup (WAI-ARIA combobox pattern)
                            if (element.getAttribute('aria-expanded') === 'true') {
                                e.preventDefault();
                                if (callbacks.onExpandCollapse) {
                                    callbacks.onExpandCollapse(handle);
                                }
                            }
                        }
                    });
                    // Click handler for expand/collapse
                    element.addEventListener('click', (e) => {
                        e.preventDefault();
                        if (callbacks.onExpandCollapse) {
                            callbacks.onExpandCollapse(handle);
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a listbox semantic element.
                 * Called from C# via JSImport.
                 */
                static createListBoxElement(parentHandle, handle, index, x, y, width, height, multiselect, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'listbox');
                    // Roving model (T015): the composite container is not itself a tab stop; focus rests on
                    // the active descendant item. _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    if (multiselect) {
                        element.setAttribute('aria-multiselectable', 'true');
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a list item semantic element.
                 * Called from C# via JSImport.
                 */
                static createListItemElement(parentHandle, handle, index, x, y, width, height, selected, positionInSet, sizeOfSet, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'option');
                    element.setAttribute('aria-selected', String(selected));
                    element.setAttribute('aria-posinset', String(positionInSet));
                    element.setAttribute('aria-setsize', String(sizeOfSet));
                    // Composite item stays at -1 (parent listbox manages focus via roving / activedescendant).
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    const callbacks = this.getCallbacks();
                    // Click handler for selection (T073)
                    element.addEventListener('click', () => {
                        if (callbacks.onSelection) {
                            callbacks.onSelection(handle);
                        }
                    });
                    // Keyboard handler for Enter/Space selection
                    element.addEventListener('keydown', (e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            if (callbacks.onSelection) {
                                callbacks.onSelection(handle);
                            }
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Updates the value of a slider element and its ARIA attributes.
                 */
                static updateSliderValue(handle, value, min, max, valueText) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element && element.type === 'range') {
                        element.min = String(min);
                        element.max = String(max);
                        element.value = String(value);
                        element.setAttribute('aria-valuenow', String(value));
                        element.setAttribute('aria-valuemin', String(min));
                        element.setAttribute('aria-valuemax', String(max));
                        if (valueText) {
                            element.setAttribute('aria-valuetext', valueText);
                        }
                        else {
                            element.removeAttribute('aria-valuetext');
                        }
                    }
                }
                /**
                 * Updates the value of a text input element.
                 */
                static updateTextBoxValue(handle, value, selectionStart, selectionEnd, selectionIsBackward) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA')) {
                        // Skip no-op writes to avoid caret and IME churn when the DOM value already matches.
                        if (element.value !== value) {
                            element.value = value;
                        }
                        // Negative sentinel from C# means "do not touch selection".
                        // This preserves the browser-managed caret for browser-originated a11y text input.
                        if (selectionStart >= 0 && selectionEnd >= 0) {
                            // Validate selection range to prevent exceptions
                            const maxLen = value.length;
                            const start = Math.max(0, Math.min(selectionStart, maxLen));
                            const end = Math.max(start, Math.min(selectionEnd, maxLen));
                            try {
                                element.setSelectionRange(start, end, selectionIsBackward ? 'backward' : 'forward');
                            }
                            catch (_a) {
                                // Some input types (e.g., password in some browsers) don't support setSelectionRange
                            }
                        }
                    }
                }
                /**
                 * Updates browser spell checking for a text input element.
                 */
                static updateTextBoxSpellCheck(handle, isSpellCheckEnabled) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA')) {
                        const spellcheck = isSpellCheckEnabled && element.getAttribute('type') !== 'password';
                        element.spellcheck = spellcheck;
                        element.setAttribute('spellcheck', String(spellcheck));
                    }
                }
                /**
                 * Updates the read-only state of a text input element.
                 */
                static updateTextBoxReadOnly(handle, isReadOnly) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA')) {
                        element.readOnly = isReadOnly;
                        if (isReadOnly) {
                            element.setAttribute('aria-readonly', 'true');
                        }
                        else {
                            element.removeAttribute('aria-readonly');
                        }
                    }
                }
                /**
                 * Updates the placeholder text of a text input element.
                 */
                static updateTextBoxPlaceholder(handle, placeholder) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA')) {
                        element.placeholder = placeholder !== null && placeholder !== void 0 ? placeholder : '';
                    }
                }
                /**
                 * Updates the expanded/collapsed state of a combobox element.
                 */
                static updateExpandCollapseState(handle, expanded) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element) {
                        element.setAttribute('aria-expanded', String(expanded));
                        // Clear activedescendant when collapsing
                        if (!expanded) {
                            element.removeAttribute('aria-activedescendant');
                        }
                    }
                }
                /**
                 * Updates aria-activedescendant on a combobox/listbox to point to the active option.
                 * Screen readers use this to announce the currently focused option without moving DOM focus.
                 */
                static updateActiveDescendant(containerHandle, activeItemHandle) {
                    const container = document.getElementById(`uno-semantics-${containerHandle}`);
                    if (container) {
                        if (activeItemHandle !== 0) {
                            const activeId = `uno-semantics-${activeItemHandle}`;
                            container.setAttribute('aria-activedescendant', activeId);
                        }
                        else {
                            container.removeAttribute('aria-activedescendant');
                        }
                    }
                }
                /**
                 * Updates the selected state of a list item element.
                 */
                static updateSelectionState(handle, selected) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element) {
                        element.setAttribute('aria-selected', String(selected));
                    }
                }
                /**
                 * Updates the disabled state of an element.
                 */
                static updateDisabledState(handle, disabled) {
                    const element = document.getElementById(`uno-semantics-${handle}`);
                    if (element) {
                        if ('disabled' in element) {
                            element.disabled = disabled;
                        }
                        element.setAttribute('aria-disabled', String(disabled));
                    }
                }
                /**
                 * Creates a link (anchor) semantic element and appends it to the semantics root.
                 * Called from C# via JSImport.
                 */
                static createLinkElement(parentHandle, handle, index, x, y, width, height, label, isFocusable) {
                    const element = document.createElement('a');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    Runtime.Accessibility.updateElementFocusability(element, isFocusable);
                    // Native <a> has implicit role="link" — no need to set explicitly
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    const callbacks = this.getCallbacks();
                    element.addEventListener('click', (e) => {
                        e.preventDefault();
                        if (callbacks.onInvoke) {
                            callbacks.onInvoke(handle);
                        }
                    });
                    element.addEventListener('keydown', (e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            if (callbacks.onInvoke) {
                                callbacks.onInvoke(handle);
                            }
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                // ===== Tab/Tree/Grid/Menu Semantic Elements =====
                /**
                 * Creates a tablist container element.
                 */
                static createTabListElement(parentHandle, handle, index, x, y, width, height, label, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'tablist');
                    // Roving model (T015): the composite container is not itself a tab stop; the active tab is.
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a tab element with selection and keyboard support.
                 */
                static createTabElement(parentHandle, handle, index, x, y, width, height, label, selected, positionInSet, sizeOfSet, isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'tab');
                    element.setAttribute('aria-selected', String(selected));
                    // Roving tabindex: only the selected tab is a tab stop; a non-focusable tab is never one.
                    element.tabIndex = (isFocusable && selected) ? 0 : -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (positionInSet > 0 && sizeOfSet > 0) {
                        element.setAttribute('aria-posinset', String(positionInSet));
                        element.setAttribute('aria-setsize', String(sizeOfSet));
                    }
                    const callbacks = this.getCallbacks();
                    element.addEventListener('click', () => {
                        if (callbacks.onSelection) {
                            callbacks.onSelection(handle);
                        }
                    });
                    element.addEventListener('keydown', (e) => {
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            if (callbacks.onSelection) {
                                callbacks.onSelection(handle);
                            }
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a tree container element.
                 */
                static createTreeElement(parentHandle, handle, index, x, y, width, height, label, multiselectable, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'tree');
                    // Roving model (T015): the composite container is not itself a tab stop; the active treeitem is.
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (multiselectable) {
                        element.setAttribute('aria-multiselectable', 'true');
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a treeitem element with level, expanded state, and selection.
                 */
                static createTreeItemElement(parentHandle, handle, index, x, y, width, height, label, level, expanded, selected, positionInSet, sizeOfSet, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'treeitem');
                    // Composite item stays at -1 (parent tree manages focus via roving / activedescendant).
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (level > 0) {
                        element.setAttribute('aria-level', String(level));
                    }
                    // expanded: "true", "false", or "none" (leaf node)
                    if (expanded !== 'none') {
                        element.setAttribute('aria-expanded', expanded);
                    }
                    element.setAttribute('aria-selected', String(selected));
                    if (positionInSet > 0 && sizeOfSet > 0) {
                        element.setAttribute('aria-posinset', String(positionInSet));
                        element.setAttribute('aria-setsize', String(sizeOfSet));
                    }
                    const callbacks = this.getCallbacks();
                    element.addEventListener('click', () => {
                        if (callbacks.onSelection) {
                            callbacks.onSelection(handle);
                        }
                    });
                    // WAI-ARIA tree item keyboard pattern
                    element.addEventListener('keydown', (e) => {
                        var _a, _b, _c, _d, _e, _f, _g;
                        const currentExpanded = element.getAttribute('aria-expanded');
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            if (callbacks.onSelection) {
                                callbacks.onSelection(handle);
                            }
                        }
                        else if (e.key === 'ArrowRight') {
                            if (currentExpanded === 'false') {
                                // Expand collapsed node
                                e.preventDefault();
                                if (callbacks.onExpandCollapse) {
                                    callbacks.onExpandCollapse(handle);
                                }
                            }
                            else if (currentExpanded === 'true') {
                                // Move to first child
                                e.preventDefault();
                                const firstChild = element.querySelector('[role="treeitem"]');
                                if (firstChild) {
                                    firstChild.focus();
                                }
                            }
                        }
                        else if (e.key === 'ArrowLeft') {
                            if (currentExpanded === 'true') {
                                // Collapse expanded node
                                e.preventDefault();
                                if (callbacks.onExpandCollapse) {
                                    callbacks.onExpandCollapse(handle);
                                }
                            }
                            else {
                                // Move to parent tree item
                                e.preventDefault();
                                const parentItem = (_a = element.parentElement) === null || _a === void 0 ? void 0 : _a.closest('[role="treeitem"]');
                                if (parentItem) {
                                    parentItem.focus();
                                }
                            }
                        }
                        else if (e.key === 'ArrowDown') {
                            // Move to next visible tree item
                            e.preventDefault();
                            const allItems = Array.from((_c = (_b = element.closest('[role="tree"]')) === null || _b === void 0 ? void 0 : _b.querySelectorAll('[role="treeitem"]')) !== null && _c !== void 0 ? _c : []);
                            const currentIndex = allItems.indexOf(element);
                            if (currentIndex >= 0 && currentIndex < allItems.length - 1) {
                                allItems[currentIndex + 1].focus();
                            }
                        }
                        else if (e.key === 'ArrowUp') {
                            // Move to previous visible tree item
                            e.preventDefault();
                            const allItems = Array.from((_e = (_d = element.closest('[role="tree"]')) === null || _d === void 0 ? void 0 : _d.querySelectorAll('[role="treeitem"]')) !== null && _e !== void 0 ? _e : []);
                            const currentIndex = allItems.indexOf(element);
                            if (currentIndex > 0) {
                                allItems[currentIndex - 1].focus();
                            }
                        }
                        else if (e.key === 'Home') {
                            // Move to first tree item
                            e.preventDefault();
                            const firstItem = (_f = element.closest('[role="tree"]')) === null || _f === void 0 ? void 0 : _f.querySelector('[role="treeitem"]');
                            if (firstItem) {
                                firstItem.focus();
                            }
                        }
                        else if (e.key === 'End') {
                            // Move to last tree item
                            e.preventDefault();
                            const allItems = (_g = element.closest('[role="tree"]')) === null || _g === void 0 ? void 0 : _g.querySelectorAll('[role="treeitem"]');
                            if (allItems && allItems.length > 0) {
                                allItems[allItems.length - 1].focus();
                            }
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a grid/table container element with row/column count.
                 */
                static createGridElement(parentHandle, handle, index, x, y, width, height, label, rowCount, colCount, multiSelectable, _isFocusable) {
                    const element = document.createElement('table');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'grid');
                    // Roving model (T015): the composite container is not itself a tab stop; the active cell is.
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (rowCount > 0) {
                        element.setAttribute('aria-rowcount', String(rowCount));
                    }
                    if (colCount > 0) {
                        element.setAttribute('aria-colcount', String(colCount));
                    }
                    if (multiSelectable) {
                        element.setAttribute('aria-multiselectable', 'true');
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a grid row element.
                 */
                static createGridRowElement(parentHandle, handle, index, x, y, width, height, rowIndex, selectable, selected, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'row');
                    element.style.pointerEvents = 'none';
                    // A row is a structural grouping, not a tab stop: it gets no tabindex.
                    // _isFocusable is accepted for signature uniformity.
                    if (rowIndex > 0) {
                        element.setAttribute('aria-rowindex', String(rowIndex));
                    }
                    if (selectable) {
                        element.setAttribute('aria-selected', String(selected));
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a grid cell element.
                 */
                static createGridCellElement(parentHandle, handle, index, x, y, width, height, label, rowIndex, colIndex, selectable, selected, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'gridcell');
                    // Composite item stays at -1 (parent grid manages focus via roving / activedescendant).
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (rowIndex > 0) {
                        element.setAttribute('aria-rowindex', String(rowIndex));
                    }
                    if (colIndex > 0) {
                        element.setAttribute('aria-colindex', String(colIndex));
                    }
                    if (selectable) {
                        element.setAttribute('aria-selected', String(selected));
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a column header element.
                 */
                static createColumnHeaderElement(parentHandle, handle, index, x, y, width, height, label, colIndex, sort, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'columnheader');
                    // Composite item stays at -1 (parent grid manages focus via roving / activedescendant).
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (colIndex > 0) {
                        element.setAttribute('aria-colindex', String(colIndex));
                    }
                    if (sort) {
                        element.setAttribute('aria-sort', sort);
                    }
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a menu container element.
                 */
                static createMenuElement(parentHandle, handle, index, x, y, width, height, label, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'menu');
                    // Roving model (T015): the composite container is not itself a tab stop; the active menuitem is.
                    // _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Creates a menuitem element.
                 */
                static createMenuItemElement(parentHandle, handle, index, x, y, width, height, label, disabled, hasSubmenu, _isFocusable) {
                    const element = document.createElement('div');
                    this.applyCommonStyles(element, x, y, width, height, handle);
                    element.setAttribute('role', 'menuitem');
                    // Composite item stays at -1 (parent menu manages focus via roving). A disabled menuitem
                    // is likewise not a tab stop (T017). _isFocusable is accepted for signature uniformity.
                    element.tabIndex = -1;
                    element.style.pointerEvents = 'none';
                    this.setAriaStringAttribute(element, 'aria-label', label);
                    if (disabled) {
                        element.setAttribute('aria-disabled', 'true');
                    }
                    if (hasSubmenu) {
                        element.setAttribute('aria-haspopup', 'menu');
                    }
                    const callbacks = this.getCallbacks();
                    element.addEventListener('click', () => {
                        if (callbacks.onInvoke) {
                            callbacks.onInvoke(handle);
                        }
                    });
                    // WAI-ARIA menu item keyboard pattern
                    element.addEventListener('keydown', (e) => {
                        var _a, _b, _c, _d;
                        if (e.key === 'Enter' || e.key === ' ') {
                            e.preventDefault();
                            if (callbacks.onInvoke) {
                                callbacks.onInvoke(handle);
                            }
                        }
                        else if (e.key === 'ArrowDown') {
                            // Move to next menu item
                            e.preventDefault();
                            const allItems = Array.from((_b = (_a = element.parentElement) === null || _a === void 0 ? void 0 : _a.querySelectorAll('[role="menuitem"]')) !== null && _b !== void 0 ? _b : []);
                            const currentIndex = allItems.indexOf(element);
                            if (currentIndex >= 0 && currentIndex < allItems.length - 1) {
                                allItems[currentIndex + 1].focus();
                            }
                        }
                        else if (e.key === 'ArrowUp') {
                            // Move to previous menu item
                            e.preventDefault();
                            const allItems = Array.from((_d = (_c = element.parentElement) === null || _c === void 0 ? void 0 : _c.querySelectorAll('[role="menuitem"]')) !== null && _d !== void 0 ? _d : []);
                            const currentIndex = allItems.indexOf(element);
                            if (currentIndex > 0) {
                                allItems[currentIndex - 1].focus();
                            }
                        }
                        else if (e.key === 'ArrowRight' && hasSubmenu) {
                            // Open submenu
                            e.preventDefault();
                            if (callbacks.onExpandCollapse) {
                                callbacks.onExpandCollapse(handle);
                            }
                        }
                        else if (e.key === 'Escape') {
                            // Close menu (move focus to parent)
                            e.preventDefault();
                            const parentMenu = element.parentElement;
                            if (parentMenu) {
                                parentMenu.focus();
                            }
                        }
                    });
                    this.appendToParent(element, parentHandle, index);
                }
                /**
                 * Schedules a virtualized mutation to be flushed in the next animation frame.
                 */
                static scheduleVirtualizedMutation(mutation) {
                    SemanticElements.virtualizedMutationQueue.push(mutation);
                    if (SemanticElements.virtualizedRafId === 0) {
                        SemanticElements.virtualizedRafId = requestAnimationFrame(() => {
                            SemanticElements.flushVirtualizedMutations();
                        });
                    }
                }
                /**
                 * Drops queued virtualized mutations; used when the semantic DOM is torn down.
                 */
                static resetVirtualizedMutations() {
                    if (SemanticElements.virtualizedRafId !== 0) {
                        cancelAnimationFrame(SemanticElements.virtualizedRafId);
                        SemanticElements.virtualizedRafId = 0;
                    }
                    SemanticElements.virtualizedMutationQueue = [];
                }
                /**
                 * Flushes all queued virtualized mutations.
                 */
                static flushVirtualizedMutations() {
                    const queue = SemanticElements.virtualizedMutationQueue;
                    SemanticElements.virtualizedMutationQueue = [];
                    SemanticElements.virtualizedRafId = 0;
                    for (const mutation of queue) {
                        mutation();
                    }
                }
                /**
                 * Registers a virtualized container (creates listbox/grid element).
                 */
                static registerVirtualizedContainer(containerHandle, role, label, multiselectable) {
                    // If an element for this container already exists (created by CreateListBoxElement),
                    // just update its attributes instead of creating a duplicate.
                    const existing = document.getElementById(`uno-semantics-${containerHandle}`);
                    if (existing) {
                        if (label) {
                            existing.setAttribute('aria-label', label);
                        }
                        if (multiselectable) {
                            existing.setAttribute('aria-multiselectable', 'true');
                        }
                        return;
                    }
                    const root = SemanticElements.getSemanticsRoot();
                    if (!root) {
                        return;
                    }
                    const element = document.createElement('div');
                    element.id = `uno-semantics-${containerHandle}`;
                    element.setAttribute('role', role);
                    element.style.position = 'absolute';
                    // Roving model (T015): the virtualized composite container is not itself a tab stop;
                    // focus rests on the active descendant item, consistent with createListBoxElement.
                    element.tabIndex = -1;
                    if (label) {
                        element.setAttribute('aria-label', label);
                    }
                    if (multiselectable) {
                        element.setAttribute('aria-multiselectable', 'true');
                    }
                    root.appendChild(element);
                }
                /**
                 * Adds a semantic element for a realized virtualized item.
                 * Batched via requestAnimationFrame.
                 */
                static addVirtualizedItem(containerHandle, itemHandle, index, totalCount, x, y, width, height, role, label) {
                    SemanticElements.scheduleVirtualizedMutation(() => {
                        const container = document.getElementById(`uno-semantics-${containerHandle}`);
                        if (!container) {
                            return;
                        }
                        // If an element for this item already exists (created by OnChildAdded→CreateListItemElement),
                        // just update it instead of creating a duplicate.
                        const existingItem = document.getElementById(`uno-semantics-${itemHandle}`);
                        if (existingItem) {
                            existingItem.setAttribute('aria-posinset', String(index + 1));
                            existingItem.setAttribute('aria-setsize', String(totalCount));
                            if (label) {
                                existingItem.setAttribute('aria-label', label);
                            }
                            // Ensure item is inside the correct container
                            if (existingItem.parentElement !== container) {
                                container.appendChild(existingItem);
                            }
                            return;
                        }
                        const element = document.createElement('div');
                        SemanticElements.applyCommonStyles(element, x, y, width, height, itemHandle);
                        element.setAttribute('role', role);
                        // aria-posinset is 1-based, index is 0-based
                        element.setAttribute('aria-posinset', String(index + 1));
                        element.setAttribute('aria-setsize', String(totalCount));
                        element.tabIndex = -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        const callbacks = SemanticElements.getCallbacks();
                        element.addEventListener('click', (e) => {
                            e.preventDefault();
                            if (callbacks.onSelection) {
                                callbacks.onSelection(itemHandle);
                            }
                        });
                        container.appendChild(element);
                    });
                }
                /**
                 * Removes a semantic element for an unrealized virtualized item.
                 * Batched via requestAnimationFrame.
                 */
                static removeVirtualizedItem(itemHandle) {
                    SemanticElements.scheduleVirtualizedMutation(() => {
                        const element = document.getElementById(`uno-semantics-${itemHandle}`);
                        if (element && element.parentElement) {
                            element.parentElement.removeChild(element);
                        }
                    });
                }
                /**
                 * Updates the total item count on all realized items in a container.
                 */
                static updateVirtualizedItemCount(containerHandle, totalCount) {
                    const container = document.getElementById(`uno-semantics-${containerHandle}`);
                    if (!container) {
                        return;
                    }
                    const items = container.querySelectorAll('[aria-posinset]');
                    items.forEach((el) => {
                        el.setAttribute('aria-setsize', String(totalCount));
                    });
                }
                /**
                 * Unregisters a virtualized container and removes all its semantic elements.
                 */
                static unregisterVirtualizedContainer(containerHandle) {
                    const element = document.getElementById(`uno-semantics-${containerHandle}`);
                    if (element && element.parentElement) {
                        element.parentElement.removeChild(element);
                    }
                }
            }
            // ===== Virtualized Container Functions =====
            /**
             * Queue for batching virtualized item DOM mutations via requestAnimationFrame.
             */
            SemanticElements.virtualizedMutationQueue = [];
            SemanticElements.virtualizedRafId = 0;
            Runtime.SemanticElements = SemanticElements;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class SoftwareBrowserRenderer {
                constructor(canvas) {
                    this.pixelBuffer = -1;
                    this.canvas = canvas;
                    this.ctx2D = this.canvas.getContext("2d");
                    if (!this.ctx2D) {
                        throw 'Unable to acquire 2D rendering context for the provided <canvas> element';
                    }
                }
                static tryCreateInstance(canvasId) {
                    try {
                        if (!canvasId)
                            throw 'No <canvas> element or ID was provided';
                        const canvas = document.getElementById(canvasId);
                        if (!canvas)
                            throw `No <canvas> with id ${canvasId} was found`;
                        const instance = new SoftwareBrowserRenderer(canvas);
                        return {
                            success: true,
                            instance: instance,
                        };
                    }
                    catch (e) {
                        return {
                            success: false,
                            error: e.toString()
                        };
                    }
                }
                static resizePixelBuffer(instance, width, height) {
                    if (instance.pixelBuffer !== -1) {
                        Module._free(instance.pixelBuffer);
                    }
                    instance.pixelBuffer = Module._malloc(width * height * 4);
                    instance.clampedArray = new Uint8ClampedArray(Module.HEAPU8.buffer, instance.pixelBuffer, width * height * 4);
                    return instance.pixelBuffer;
                }
                static isPixelBufferValid(instance) {
                    var _a;
                    // The clampedArray might suddenly becomes zero-length because the runtime
                    // decided to resize the heap.
                    return ((_a = instance.clampedArray) === null || _a === void 0 ? void 0 : _a.length) > 0;
                }
                static blitSoftware(instance, width, height) {
                    const imageData = new ImageData(instance.clampedArray, width, height);
                    instance.ctx2D.putImageData(imageData, 0, 0);
                }
            }
            Runtime.SoftwareBrowserRenderer = SoftwareBrowserRenderer;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            // Single source of truth for the well-known DOM element ids of the browser head.
            // Multiple modules (window wrapper, text-box view) key behaviour off these ids, so a
            // rename must happen in exactly one place.
            class UnoDomIds {
            }
            UnoDomIds.canvas = "uno-canvas";
            UnoDomIds.input = "uno-input";
            Runtime.UnoDomIds = UnoDomIds;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
// JS WebGL2 shim called from C# WasmGLFunctions.cs via [JSImport].
// Pointer args are byte offsets into Module.HEAP*; we decode them here.
// Object handles (programs/shaders/buffers/...) follow emscripten's library_webgl.js
// conventions so they coexist with anything window.GL has already touched.
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            function gl() {
                const ctx = window.GL.currentContext;
                if (!ctx || !ctx.GLctx) {
                    throw "WasmGLFunctions: no current WebGL context. Did MakeCurrent run?";
                }
                return ctx.GLctx;
            }
            function tables() {
                const G = window.GL;
                // Lazily ensure each handle table exists. Emscripten's library_webgl.js creates the
                // common tables (buffers, programs, ...) at startup, but the WebGL2-era ones (syncs,
                // transformFeedbacks, ...) vary by emscripten version and link configuration.
                G.buffers || (G.buffers = [null]);
                G.programs || (G.programs = [null]);
                G.shaders || (G.shaders = [null]);
                G.textures || (G.textures = [null]);
                G.vaos || (G.vaos = [null]);
                G.framebuffers || (G.framebuffers = [null]);
                G.renderbuffers || (G.renderbuffers = [null]);
                G.uniforms || (G.uniforms = [null]);
                G.samplers || (G.samplers = [null]);
                G.queries || (G.queries = [null]);
                G.syncs || (G.syncs = [null]);
                G.transformFeedbacks || (G.transformFeedbacks = [null]);
                return G;
            }
            function getNewId(table) {
                const G = window.GL;
                if (typeof G.getNewId === "function") {
                    return G.getNewId(table);
                }
                // Fallback: linear scan.
                for (let i = 1; i < table.length; i++) {
                    if (!table[i])
                        return i;
                }
                return table.length;
            }
            function readUInt32Array(ptr, count) {
                // Copy out so we don't depend on heap stability across re-grow.
                return new Uint32Array(window.Module.HEAPU32.buffer, ptr, count).slice();
            }
            function writeUInt32(ptr, value) {
                window.Module.HEAPU32[ptr >> 2] = value;
            }
            function writeInt32(ptr, value) {
                window.Module.HEAP32[ptr >> 2] = value;
            }
            function readCString(ptr, maxLen = -1) {
                const HEAPU8 = window.Module.HEAPU8;
                let end = ptr;
                if (maxLen < 0) {
                    while (HEAPU8[end] !== 0)
                        end++;
                }
                else {
                    end = ptr + maxLen;
                }
                const bytes = HEAPU8.subarray(ptr, end);
                return new TextDecoder("utf-8").decode(bytes);
            }
            function writeCString(ptr, bufSize, str, lengthOutPtr) {
                if (bufSize <= 0) {
                    if (lengthOutPtr !== 0)
                        writeInt32(lengthOutPtr, 0);
                    return;
                }
                const HEAPU8 = window.Module.HEAPU8;
                const bytes = new TextEncoder().encode(str);
                const writable = Math.min(bytes.length, bufSize - 1);
                HEAPU8.set(bytes.subarray(0, writable), ptr);
                HEAPU8[ptr + writable] = 0;
                if (lengthOutPtr !== 0)
                    writeInt32(lengthOutPtr, writable);
            }
            // One-shot portability warnings: emitted the first time a WebGL2-incompatible state
            // combination is detected, so users get an explanation instead of only the per-draw
            // INVALID_OPERATION spam the browser produces.
            const warnedOnce = new Set();
            function warnOnce(key, message) {
                if (!warnedOnce.has(key)) {
                    warnedOnce.add(key);
                    console.warn(message);
                }
            }
            // With a pixel buffer object bound, the pixels/data argument of the TexImage*/TexSubImage*/
            // ReadPixels family is a byte offset into the bound buffer rather than a client-memory
            // pointer, and WebGL2 has dedicated GLintptr overloads for that case.
            function unpackPboBound(ctx) {
                return !!ctx.getParameter(ctx.PIXEL_UNPACK_BUFFER_BINDING);
            }
            function packPboBound(ctx) {
                return !!ctx.getParameter(ctx.PIXEL_PACK_BUFFER_BINDING);
            }
            // WebGL2 (unlike desktop GL) fails every draw with INVALID_OPERATION while the front and
            // back stencil reference/value-mask/write-mask differ; only the funcs/ops may vary per face.
            // The mismatch is recorded when stencil state is set (transient divergence between two
            // consecutive Separate calls is legitimate) and only reported at draw time while the
            // stencil test is enabled.
            function recordStencilMismatch(ctx) {
                ctx.__unoStencilMismatch =
                    ctx.getParameter(ctx.STENCIL_REF) !== ctx.getParameter(ctx.STENCIL_BACK_REF)
                        || ctx.getParameter(ctx.STENCIL_VALUE_MASK) !== ctx.getParameter(ctx.STENCIL_BACK_VALUE_MASK)
                        || ctx.getParameter(ctx.STENCIL_WRITEMASK) !== ctx.getParameter(ctx.STENCIL_BACK_WRITEMASK);
            }
            function warnIfStencilMismatch(ctx) {
                if (ctx.__unoStencilMismatch && ctx.isEnabled(ctx.STENCIL_TEST)) {
                    warnOnce("stencilMismatch", "Draw call with diverging front/back stencil reference, value mask, or write mask. " +
                        "WebGL2 requires them to be identical (only the stencil funcs/ops may differ per face); " +
                        "draws fail with INVALID_OPERATION until they match again.");
                }
            }
            // Static C-string cache for glGetString. Each unique pname value gets a single allocation
            // in wasm memory that we never free, matching glGetString's "pointer remains valid" contract.
            const stringCache = {};
            function allocCString(s) {
                const bytes = new TextEncoder().encode(s);
                const ptr = window.Module._malloc(bytes.length + 1);
                const HEAPU8 = window.Module.HEAPU8;
                HEAPU8.set(bytes, ptr);
                HEAPU8[ptr + bytes.length] = 0;
                return ptr;
            }
            class WasmGLFunctions {
                // ------------------------------------------------------------------------------------
                // State / queries
                // ------------------------------------------------------------------------------------
                static glGetString(name) {
                    var _a;
                    const cached = stringCache[name];
                    if (cached)
                        return cached;
                    const ctx = gl();
                    let value;
                    if (name === 0x1F03 /* GL_EXTENSIONS */) {
                        // ES3 keeps the legacy combined extension string; WebGL2's getParameter does not.
                        value = ((_a = ctx.getSupportedExtensions()) !== null && _a !== void 0 ? _a : []).join(" ");
                    }
                    else {
                        try {
                            value = ctx.getParameter(name);
                            if (value == null)
                                value = "";
                        }
                        catch (_b) {
                            value = "";
                        }
                    }
                    const ptr = allocCString(value);
                    stringCache[name] = ptr;
                    return ptr;
                }
                static glGetIntegerv(pname, dataPtr) {
                    var _a;
                    // GL_MAJOR_VERSION (0x821B) and GL_MINOR_VERSION (0x821C) aren't queryable through
                    // WebGL2's getParameter (they were added to desktop GL/GLES core but never reflected
                    // into WebGL). Parse them out of the VERSION string instead. WebGL 2.0 corresponds to
                    // GLES 3.0 by spec; WebGL 1.0 corresponds to GLES 2.0.
                    if (pname === 0x821B || pname === 0x821C) {
                        const ctx = gl();
                        const versionStr = ctx.getParameter(ctx.VERSION) || "";
                        let major = 0;
                        let minor = 0;
                        const esMatch = versionStr.match(/OpenGL ES (\d+)\.(\d+)/);
                        if (esMatch) {
                            major = parseInt(esMatch[1], 10);
                            minor = parseInt(esMatch[2], 10);
                        }
                        else {
                            const webglMatch = versionStr.match(/WebGL (\d+)\.(\d+)/);
                            if (webglMatch) {
                                const wgMajor = parseInt(webglMatch[1], 10);
                                if (wgMajor >= 2) {
                                    major = 3;
                                    minor = 0;
                                }
                                else {
                                    major = 2;
                                    minor = 0;
                                }
                            }
                        }
                        writeInt32(dataPtr, pname === 0x821B ? major : minor);
                        return;
                    }
                    // GL_NUM_EXTENSIONS is likewise desktop/ES-core only; WebGL2's getParameter rejects it.
                    // Report the count behind getSupportedExtensions so the standard desktop enumeration
                    // idiom (GetIntegerv(NUM_EXTENSIONS) + GetStringi(EXTENSIONS, i)) works.
                    if (pname === 0x821D /* NUM_EXTENSIONS */) {
                        writeInt32(dataPtr, ((_a = gl().getSupportedExtensions()) !== null && _a !== void 0 ? _a : []).length);
                        return;
                    }
                    // More ES3-core scalar pnames WebGL2's getParameter rejects.
                    if (pname === 0x86A2 /* NUM_COMPRESSED_TEXTURE_FORMATS */) {
                        const ctx = gl();
                        const formats = ctx.getParameter(ctx.COMPRESSED_TEXTURE_FORMATS);
                        writeInt32(dataPtr, formats ? formats.length : 0);
                        return;
                    }
                    if (pname === 0x8DFA /* SHADER_COMPILER */) {
                        writeInt32(dataPtr, 1); // WebGL2 always compiles from source
                        return;
                    }
                    if (pname === 0x8DF9 /* NUM_SHADER_BINARY_FORMATS */ || pname === 0x87FE /* NUM_PROGRAM_BINARY_FORMATS */
                        || pname === 0x821E /* CONTEXT_FLAGS */) {
                        writeInt32(dataPtr, 0); // no binary shader/program formats, no context flags
                        return;
                    }
                    if (pname === 0x8DF8 /* SHADER_BINARY_FORMATS */ || pname === 0x87FF /* PROGRAM_BINARY_FORMATS */) {
                        return; // their counts are 0, so there is nothing to write
                    }
                    // Object-binding queries (FRAMEBUFFER_BINDING, RENDERBUFFER_BINDING, BUFFER_*_BINDING,
                    // TEXTURE_BINDING_*, VERTEX_ARRAY_BINDING, CURRENT_PROGRAM) return the bound WebGL*
                    // object - or null when nothing's bound. Reverse-map to the integer id we stamped onto
                    // them in glGen*. Look the object up in the table as a fallback in case .name wasn't
                    // set (e.g. objects auto-created on first bind by external code).
                    if (pname === 0x8CA6 /* FRAMEBUFFER_BINDING (== DRAW_) */ || pname === 0x8CAA /* READ_FRAMEBUFFER_BINDING */ ||
                        pname === 0x8CA7 /* RENDERBUFFER_BINDING */ ||
                        pname === 0x8894 /* ARRAY_BUFFER_BINDING */ || pname === 0x8895 /* ELEMENT_ARRAY_BUFFER_BINDING */ ||
                        pname === 0x8C8F /* TRANSFORM_FEEDBACK_BUFFER_BINDING */ || pname === 0x8A28 /* UNIFORM_BUFFER_BINDING */ ||
                        pname === 0x88ED /* PIXEL_PACK_BUFFER_BINDING */ || pname === 0x88EF /* PIXEL_UNPACK_BUFFER_BINDING */ ||
                        pname === 0x8F36 /* COPY_READ_BUFFER_BINDING */ || pname === 0x8F37 /* COPY_WRITE_BUFFER_BINDING */ ||
                        pname === 0x85B5 /* VERTEX_ARRAY_BINDING */ || pname === 0x8069 /* TEXTURE_BINDING_2D */ ||
                        pname === 0x806A /* TEXTURE_BINDING_3D */ || pname === 0x8C1D /* TEXTURE_BINDING_2D_ARRAY */ ||
                        pname === 0x8514 /* TEXTURE_BINDING_CUBE_MAP */ || pname === 0x8919 /* SAMPLER_BINDING */ ||
                        pname === 0x8E25 /* TRANSFORM_FEEDBACK_BINDING */ || pname === 0x8B8D /* CURRENT_PROGRAM */) {
                        const obj = gl().getParameter(pname);
                        if (!obj) {
                            writeInt32(dataPtr, 0);
                            return;
                        }
                        let id = (obj.name | 0);
                        if (!id) {
                            const candidates = [
                                tables().framebuffers, tables().renderbuffers, tables().buffers,
                                tables().textures, tables().vaos, tables().programs,
                                tables().samplers, tables().transformFeedbacks,
                            ];
                            for (const table of candidates) {
                                for (let i = 1; i < table.length; i++) {
                                    if (table[i] === obj) {
                                        id = i;
                                        break;
                                    }
                                }
                                if (id)
                                    break;
                            }
                        }
                        writeInt32(dataPtr, id);
                        return;
                    }
                    const result = gl().getParameter(pname);
                    if (typeof result === "number") {
                        writeInt32(dataPtr, result | 0);
                    }
                    else if (typeof result === "boolean") {
                        writeInt32(dataPtr, result ? 1 : 0);
                    }
                    else if (result && typeof result.length === "number") {
                        for (let i = 0; i < result.length; i++) {
                            writeInt32(dataPtr + i * 4, result[i] | 0);
                        }
                    }
                    else {
                        writeInt32(dataPtr, 0);
                    }
                }
                static glGetError() {
                    return gl().getError();
                }
                static glViewport(x, y, width, height) {
                    gl().viewport(x, y, width, height);
                }
                static glEnable(cap) {
                    const fixed = WasmGLFunctions.fixedCaps[cap];
                    if (fixed) {
                        if (!fixed.alwaysOn) {
                            warnOnce("enable" + cap, `glEnable(${fixed.name}): not available on WebGL2; the call is ignored.`);
                        }
                        return;
                    }
                    gl().enable(cap);
                }
                static glDisable(cap) {
                    const fixed = WasmGLFunctions.fixedCaps[cap];
                    if (fixed) {
                        if (fixed.alwaysOn) {
                            warnOnce("disable" + cap, `glDisable(${fixed.name}): always enabled on WebGL2 and cannot be disabled; the call is ignored.`);
                        }
                        return;
                    }
                    gl().disable(cap);
                }
                static glClear(mask) { gl().clear(mask); }
                static glClearColor(r, g, b, a) { gl().clearColor(r, g, b, a); }
                static glDrawArrays(mode, first, count) {
                    const ctx = gl();
                    warnIfStencilMismatch(ctx);
                    ctx.drawArrays(mode, first, count);
                }
                static glDrawElements(mode, count, type, indicesPtr) {
                    const ctx = gl();
                    warnIfStencilMismatch(ctx);
                    ctx.drawElements(mode, count, type, indicesPtr);
                }
                static glPixelStorei(pname, param) { gl().pixelStorei(pname, param); }
                // ------------------------------------------------------------------------------------
                // Buffers
                // ------------------------------------------------------------------------------------
                static glGenBuffers(n, buffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const buf = ctx.createBuffer();
                        const id = getNewId(t.buffers);
                        if (buf)
                            buf.name = id;
                        t.buffers[id] = buf;
                        writeUInt32(buffersPtr + i * 4, id);
                    }
                }
                static glDeleteBuffers(n, buffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(buffersPtr, n);
                    for (let i = 0; i < n; i++) {
                        const id = ids[i];
                        const buf = t.buffers[id];
                        if (buf) {
                            ctx.deleteBuffer(buf);
                            t.buffers[id] = null;
                        }
                    }
                }
                static glBindBuffer(target, buffer) {
                    gl().bindBuffer(target, buffer === 0 ? null : tables().buffers[buffer]);
                }
                static glBufferData(target, size, dataPtr, usage) {
                    const ctx = gl();
                    if (dataPtr === 0) {
                        ctx.bufferData(target, size, usage);
                    }
                    else {
                        const HEAPU8 = window.Module.HEAPU8;
                        ctx.bufferData(target, HEAPU8.subarray(dataPtr, dataPtr + size), usage);
                    }
                }
                // ------------------------------------------------------------------------------------
                // Vertex arrays
                // ------------------------------------------------------------------------------------
                static glGenVertexArrays(n, arraysPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const vao = ctx.createVertexArray();
                        const id = getNewId(t.vaos);
                        if (vao)
                            vao.name = id;
                        t.vaos[id] = vao;
                        writeUInt32(arraysPtr + i * 4, id);
                    }
                }
                static glDeleteVertexArrays(n, arraysPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(arraysPtr, n);
                    for (let i = 0; i < n; i++) {
                        const id = ids[i];
                        const vao = t.vaos[id];
                        if (vao) {
                            ctx.deleteVertexArray(vao);
                            t.vaos[id] = null;
                        }
                    }
                }
                static glBindVertexArray(array) {
                    gl().bindVertexArray(array === 0 ? null : tables().vaos[array]);
                }
                static glVertexAttribPointer(index, size, type, normalized, stride, pointer) {
                    gl().vertexAttribPointer(index, size, type, normalized !== 0, stride, pointer);
                }
                static glEnableVertexAttribArray(index) { gl().enableVertexAttribArray(index); }
                static glDisableVertexAttribArray(index) { gl().disableVertexAttribArray(index); }
                // ------------------------------------------------------------------------------------
                // Textures
                // ------------------------------------------------------------------------------------
                static glGenTextures(n, texturesPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const tex = ctx.createTexture();
                        const id = getNewId(t.textures);
                        if (tex)
                            tex.name = id;
                        t.textures[id] = tex;
                        writeUInt32(texturesPtr + i * 4, id);
                    }
                }
                static glDeleteTextures(n, texturesPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(texturesPtr, n);
                    for (let i = 0; i < n; i++) {
                        const id = ids[i];
                        const tex = t.textures[id];
                        if (tex) {
                            ctx.deleteTexture(tex);
                            t.textures[id] = null;
                        }
                    }
                }
                static glBindTexture(target, texture) {
                    gl().bindTexture(target, texture === 0 ? null : tables().textures[texture]);
                }
                static glActiveTexture(texture) { gl().activeTexture(texture); }
                static glTexImage2D(target, level, internalformat, width, height, border, format, type, pixelsPtr) {
                    const ctx = gl();
                    // WebGL2's color-renderable list contains only sized internal formats (RGB8, RGBA8...).
                    // The framework passes the unsized RGB / RGBA which produces a texture that isn't
                    // framebuffer-renderable -> FRAMEBUFFER_INCOMPLETE_ATTACHMENT. Auto-promote to the
                    // matching sized format when type is UNSIGNED_BYTE (the only one we currently care
                    // about; extend if other types start needing FBO-renderable textures).
                    if (type === 0x1401 /* GL_UNSIGNED_BYTE */) {
                        if (internalformat === 0x1907 /* GL_RGB */)
                            internalformat = 0x8051 /* GL_RGB8 */;
                        else if (internalformat === 0x1908 /* GL_RGBA */)
                            internalformat = 0x8058 /* GL_RGBA8 */;
                    }
                    if (width === 0 || height === 0) {
                        console.warn(`WasmGLFunctions.glTexImage2D: zero-sized texture (${width}x${height}) - will likely cause FRAMEBUFFER_INCOMPLETE_ATTACHMENT.`);
                    }
                    if (unpackPboBound(ctx)) {
                        ctx.texImage2D(target, level, internalformat, width, height, border, format, type, pixelsPtr);
                    }
                    else if (pixelsPtr === 0) {
                        ctx.texImage2D(target, level, internalformat, width, height, border, format, type, null);
                    }
                    else {
                        const HEAPU8 = window.Module.HEAPU8;
                        // 32 bpp upper bound; texImage2D ignores trailing bytes once it has w*h*bpp.
                        const byteCount = width * height * 4;
                        ctx.texImage2D(target, level, internalformat, width, height, border, format, type, HEAPU8.subarray(pixelsPtr, pixelsPtr + byteCount));
                    }
                }
                static glTexParameteri(target, pname, param) {
                    gl().texParameteri(target, pname, param);
                }
                static glTexParameterIuiv(target, pname, paramsPtr) {
                    // WebGL2 doesn't expose the texParameterIuiv vector variant (it was desktop GL only).
                    // Silk.NET resolves gl.TexParameterI(target, pname, uint) here with a 1-element params
                    // buffer; forward the single value to the scalar texParameteri, which covers every
                    // pname GLCanvasElement uses (TEXTURE_MIN_FILTER, TEXTURE_MAG_FILTER, etc.).
                    const value = window.Module.HEAPU32[paramsPtr >> 2] | 0;
                    gl().texParameteri(target, pname, value);
                }
                static glReadBuffer(mode) { gl().readBuffer(mode); }
                static glReadPixels(x, y, width, height, format, type, pixelsPtr) {
                    const packCtx = gl();
                    if (packPboBound(packCtx)) {
                        // pixelsPtr is a byte offset into the bound PIXEL_PACK_BUFFER. No BGRA
                        // translation is possible on this path (the data never passes through here).
                        packCtx.readPixels(x, y, width, height, format, type, pixelsPtr);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    // WebGL2 doesn't accept BGRA in readPixels. GLCanvasElement.Render() asks for
                    // BGRA/UNSIGNED_BYTE so the bytes drop straight into the WriteableBitmap's BGRA
                    // buffer. Translate by reading RGBA and swapping R/B channels in place.
                    const isBgraByte = format === 0x80E1 /* GL_BGRA */ && type === 0x1401 /* GL_UNSIGNED_BYTE */;
                    const effectiveFormat = isBgraByte ? 0x1908 /* GL_RGBA */ : format;
                    const byteCount = width * height * 4;
                    const view = new Uint8Array(HEAPU8.buffer, pixelsPtr, byteCount);
                    gl().readPixels(x, y, width, height, effectiveFormat, type, view);
                    if (isBgraByte) {
                        for (let i = 0; i + 3 < byteCount; i += 4) {
                            const tmp = view[i];
                            view[i] = view[i + 2];
                            view[i + 2] = tmp;
                        }
                    }
                }
                // ------------------------------------------------------------------------------------
                // Framebuffers / renderbuffers
                // ------------------------------------------------------------------------------------
                static glGenFramebuffers(n, framebuffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const fb = ctx.createFramebuffer();
                        const id = getNewId(t.framebuffers);
                        if (fb)
                            fb.name = id;
                        t.framebuffers[id] = fb;
                        writeUInt32(framebuffersPtr + i * 4, id);
                    }
                }
                static glDeleteFramebuffers(n, framebuffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(framebuffersPtr, n);
                    for (let i = 0; i < n; i++) {
                        const id = ids[i];
                        const fb = t.framebuffers[id];
                        if (fb) {
                            ctx.deleteFramebuffer(fb);
                            t.framebuffers[id] = null;
                        }
                    }
                }
                static glBindFramebuffer(target, framebuffer) {
                    if (framebuffer === 0) {
                        gl().bindFramebuffer(target, null);
                        return;
                    }
                    // Desktop GL auto-creates FBOs on first bind for any unknown name; WebGL2 requires
                    // an explicit createFramebuffer() first. GLCanvasElement.FrameBufferDetails relies
                    // on the desktop semantics (creates an FBO via gl.GenBuffer, then binds the resulting
                    // VBO id as a framebuffer name and lets the impl create-on-bind). Emulate that here
                    // so the framework's existing pattern works unchanged.
                    const t = tables();
                    let fb = t.framebuffers[framebuffer];
                    if (!fb) {
                        fb = gl().createFramebuffer();
                        if (fb)
                            fb.name = framebuffer;
                        t.framebuffers[framebuffer] = fb;
                    }
                    gl().bindFramebuffer(target, fb);
                }
                static glCheckFramebufferStatus(target) {
                    const ctx = gl();
                    const status = ctx.checkFramebufferStatus(target);
                    if (status !== 0x8CD5 /* GL_FRAMEBUFFER_COMPLETE */) {
                        const statusNames = {
                            0x8CD6: "INCOMPLETE_ATTACHMENT",
                            0x8CD7: "INCOMPLETE_MISSING_ATTACHMENT",
                            0x8CD9: "INCOMPLETE_DIMENSIONS",
                            0x8CDD: "UNSUPPORTED",
                            0x8D56: "INCOMPLETE_MULTISAMPLE",
                            0x8DA8: "INCOMPLETE_LAYER_TARGETS",
                        };
                        const name = statusNames[status] || `0x${status.toString(16)}`;
                        const lastErr = ctx.getError();
                        console.warn(`WasmGLFunctions: framebuffer incomplete = ${name} (0x${status.toString(16)}), most recent gl.getError = 0x${lastErr.toString(16)}`);
                    }
                    return status;
                }
                static glFramebufferTexture2D(target, attachment, textarget, texture, level) {
                    gl().framebufferTexture2D(target, attachment, textarget, texture === 0 ? null : tables().textures[texture], level);
                }
                static glFramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer) {
                    gl().framebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer === 0 ? null : tables().renderbuffers[renderbuffer]);
                }
                static glGenRenderbuffers(n, renderbuffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const rb = ctx.createRenderbuffer();
                        const id = getNewId(t.renderbuffers);
                        if (rb)
                            rb.name = id;
                        t.renderbuffers[id] = rb;
                        writeUInt32(renderbuffersPtr + i * 4, id);
                    }
                }
                static glDeleteRenderbuffers(n, renderbuffersPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(renderbuffersPtr, n);
                    for (let i = 0; i < n; i++) {
                        const id = ids[i];
                        const rb = t.renderbuffers[id];
                        if (rb) {
                            ctx.deleteRenderbuffer(rb);
                            t.renderbuffers[id] = null;
                        }
                    }
                }
                static glBindRenderbuffer(target, renderbuffer) {
                    gl().bindRenderbuffer(target, renderbuffer === 0 ? null : tables().renderbuffers[renderbuffer]);
                }
                static glRenderbufferStorage(target, internalformat, width, height) {
                    gl().renderbufferStorage(target, internalformat, width, height);
                }
                // ------------------------------------------------------------------------------------
                // Shaders / programs
                // ------------------------------------------------------------------------------------
                static glCreateShader(type) {
                    const ctx = gl();
                    const t = tables();
                    const sh = ctx.createShader(type);
                    if (!sh)
                        return 0;
                    const id = getNewId(t.shaders);
                    sh.name = id;
                    t.shaders[id] = sh;
                    return id;
                }
                static glDeleteShader(shader) {
                    const t = tables();
                    const sh = t.shaders[shader];
                    if (sh) {
                        gl().deleteShader(sh);
                        t.shaders[shader] = null;
                    }
                }
                static glShaderSource(shader, count, stringsPtr, lengthsPtr) {
                    const ctx = gl();
                    const t = tables();
                    const sh = t.shaders[shader];
                    if (!sh)
                        return;
                    const ptrs = readUInt32Array(stringsPtr, count);
                    const lens = lengthsPtr === 0 ? null : readUInt32Array(lengthsPtr, count);
                    let source = "";
                    for (let i = 0; i < count; i++) {
                        const len = lens ? (lens[i] | 0) : -1;
                        source += readCString(ptrs[i], len < 0 ? -1 : len);
                    }
                    ctx.shaderSource(sh, source);
                }
                static glCompileShader(shader) {
                    const sh = tables().shaders[shader];
                    if (sh)
                        gl().compileShader(sh);
                }
                static glGetShaderiv(shader, pname, paramsPtr) {
                    var _a, _b, _c, _d;
                    const sh = tables().shaders[shader];
                    if (!sh) {
                        writeInt32(paramsPtr, 0);
                        return;
                    }
                    // WebGL2 dropped the *_LENGTH pnames (strings are returned directly); compute them.
                    // Silk.NET's string-returning helpers (GetShaderInfoLog etc.) query these to size buffers.
                    if (pname === 0x8B84 /* INFO_LOG_LENGTH */) {
                        const len = (_b = (_a = gl().getShaderInfoLog(sh)) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0;
                        writeInt32(paramsPtr, len === 0 ? 0 : len + 1);
                        return;
                    }
                    if (pname === 0x8B88 /* SHADER_SOURCE_LENGTH */) {
                        const len = (_d = (_c = gl().getShaderSource(sh)) === null || _c === void 0 ? void 0 : _c.length) !== null && _d !== void 0 ? _d : 0;
                        writeInt32(paramsPtr, len === 0 ? 0 : len + 1);
                        return;
                    }
                    const v = gl().getShaderParameter(sh, pname);
                    writeInt32(paramsPtr, typeof v === "boolean" ? (v ? 1 : 0) : (v | 0));
                }
                static glGetShaderInfoLog(shader, bufSize, lengthPtr, infoLogPtr) {
                    var _a;
                    const sh = tables().shaders[shader];
                    const log = sh ? ((_a = gl().getShaderInfoLog(sh)) !== null && _a !== void 0 ? _a : "") : "";
                    writeCString(infoLogPtr, bufSize, log, lengthPtr);
                }
                static glCreateProgram() {
                    const ctx = gl();
                    const t = tables();
                    const p = ctx.createProgram();
                    if (!p)
                        return 0;
                    const id = getNewId(t.programs);
                    p.name = id;
                    t.programs[id] = p;
                    return id;
                }
                static glDeleteProgram(program) {
                    const t = tables();
                    const p = t.programs[program];
                    if (p) {
                        gl().deleteProgram(p);
                        t.programs[program] = null;
                    }
                }
                static glAttachShader(program, shader) {
                    const t = tables();
                    const p = t.programs[program];
                    const sh = t.shaders[shader];
                    if (p && sh)
                        gl().attachShader(p, sh);
                }
                static glDetachShader(program, shader) {
                    const t = tables();
                    const p = t.programs[program];
                    const sh = t.shaders[shader];
                    if (p && sh)
                        gl().detachShader(p, sh);
                }
                static glLinkProgram(program) {
                    const p = tables().programs[program];
                    if (p)
                        gl().linkProgram(p);
                }
                static glGetProgramiv(program, pname, paramsPtr) {
                    var _a, _b;
                    const p = tables().programs[program];
                    if (!p) {
                        writeInt32(paramsPtr, 0);
                        return;
                    }
                    // WebGL2 dropped the *_LENGTH pnames (strings are returned directly); compute them.
                    // Silk.NET's string-returning helpers (GetActiveUniform etc.) query these to size buffers.
                    if (pname === 0x8B84 /* INFO_LOG_LENGTH */) {
                        const len = (_b = (_a = gl().getProgramInfoLog(p)) === null || _a === void 0 ? void 0 : _a.length) !== null && _b !== void 0 ? _b : 0;
                        writeInt32(paramsPtr, len === 0 ? 0 : len + 1);
                        return;
                    }
                    if (pname === 0x8B87 /* ACTIVE_UNIFORM_MAX_LENGTH */ || pname === 0x8B8A /* ACTIVE_ATTRIBUTE_MAX_LENGTH */) {
                        const ctx = gl();
                        const isUniform = pname === 0x8B87;
                        const count = ctx.getProgramParameter(p, isUniform ? ctx.ACTIVE_UNIFORMS : ctx.ACTIVE_ATTRIBUTES);
                        let max = 0;
                        for (let i = 0; i < count; i++) {
                            const info = isUniform ? ctx.getActiveUniform(p, i) : ctx.getActiveAttrib(p, i);
                            if (info)
                                max = Math.max(max, info.name.length);
                        }
                        writeInt32(paramsPtr, max === 0 ? 0 : max + 1);
                        return;
                    }
                    if (pname === 0x8C76 /* TRANSFORM_FEEDBACK_VARYING_MAX_LENGTH */) {
                        const ctx = gl();
                        const count = ctx.getProgramParameter(p, ctx.TRANSFORM_FEEDBACK_VARYINGS);
                        let max = 0;
                        for (let i = 0; i < count; i++) {
                            const info = ctx.getTransformFeedbackVarying(p, i);
                            if (info)
                                max = Math.max(max, info.name.length);
                        }
                        writeInt32(paramsPtr, max === 0 ? 0 : max + 1);
                        return;
                    }
                    const v = gl().getProgramParameter(p, pname);
                    writeInt32(paramsPtr, typeof v === "boolean" ? (v ? 1 : 0) : (v | 0));
                }
                static glGetProgramInfoLog(program, bufSize, lengthPtr, infoLogPtr) {
                    var _a;
                    const p = tables().programs[program];
                    const log = p ? ((_a = gl().getProgramInfoLog(p)) !== null && _a !== void 0 ? _a : "") : "";
                    writeCString(infoLogPtr, bufSize, log, lengthPtr);
                }
                static glUseProgram(program) {
                    gl().useProgram(program === 0 ? null : tables().programs[program]);
                }
                static glGetUniformLocation(program, namePtr) {
                    const t = tables();
                    const p = t.programs[program];
                    if (!p)
                        return -1;
                    const name = readCString(namePtr);
                    const loc = gl().getUniformLocation(p, name);
                    if (!loc)
                        return -1;
                    const id = getNewId(t.uniforms);
                    t.uniforms[id] = loc;
                    return id;
                }
                static glUniform1i(location, v0) {
                    const t = tables();
                    const loc = location < 0 ? null : t.uniforms[location];
                    if (loc)
                        gl().uniform1i(loc, v0);
                }
                static glUniform1f(location, v0) {
                    const t = tables();
                    const loc = location < 0 ? null : t.uniforms[location];
                    if (loc)
                        gl().uniform1f(loc, v0);
                }
                static glUniform4f(location, v0, v1, v2, v3) {
                    const t = tables();
                    const loc = location < 0 ? null : t.uniforms[location];
                    if (loc)
                        gl().uniform4f(loc, v0, v1, v2, v3);
                }
                // ------------------------------------------------------------------------------------
                // Per-fragment / framebuffer state
                // ------------------------------------------------------------------------------------
                static glBlendFunc(sfactor, dfactor) { gl().blendFunc(sfactor, dfactor); }
                static glBlendFuncSeparate(srcRGB, dstRGB, srcAlpha, dstAlpha) { gl().blendFuncSeparate(srcRGB, dstRGB, srcAlpha, dstAlpha); }
                static glBlendEquation(mode) { gl().blendEquation(mode); }
                static glBlendEquationSeparate(modeRGB, modeAlpha) { gl().blendEquationSeparate(modeRGB, modeAlpha); }
                static glBlendColor(r, g, b, a) { gl().blendColor(r, g, b, a); }
                static glDepthFunc(func) { gl().depthFunc(func); }
                static glDepthMask(flag) { gl().depthMask(flag !== 0); }
                static glDepthRangef(n, f) { gl().depthRange(n, f); }
                static glStencilFunc(func, ref, mask) {
                    const ctx = gl();
                    ctx.stencilFunc(func, ref, mask);
                    recordStencilMismatch(ctx);
                }
                static glStencilFuncSeparate(face, func, ref, mask) {
                    const ctx = gl();
                    ctx.stencilFuncSeparate(face, func, ref, mask);
                    recordStencilMismatch(ctx);
                }
                static glStencilMask(mask) {
                    const ctx = gl();
                    ctx.stencilMask(mask);
                    recordStencilMismatch(ctx);
                }
                static glStencilMaskSeparate(face, mask) {
                    const ctx = gl();
                    ctx.stencilMaskSeparate(face, mask);
                    recordStencilMismatch(ctx);
                }
                static glStencilOp(fail, zfail, zpass) { gl().stencilOp(fail, zfail, zpass); }
                static glStencilOpSeparate(face, sfail, dpfail, dppass) { gl().stencilOpSeparate(face, sfail, dpfail, dppass); }
                static glCullFace(mode) { gl().cullFace(mode); }
                static glFrontFace(mode) { gl().frontFace(mode); }
                static glScissor(x, y, width, height) { gl().scissor(x, y, width, height); }
                static glPolygonOffset(factor, units) { gl().polygonOffset(factor, units); }
                static glColorMask(red, green, blue, alpha) { gl().colorMask(red !== 0, green !== 0, blue !== 0, alpha !== 0); }
                static glLineWidth(width) { gl().lineWidth(width); }
                static glHint(target, mode) { gl().hint(target, mode); }
                static glFinish() { gl().finish(); }
                static glFlush() { gl().flush(); }
                static glSampleCoverage(value, invert) { gl().sampleCoverage(value, invert !== 0); }
                static glIsEnabled(cap) {
                    const fixed = WasmGLFunctions.fixedCaps[cap];
                    if (fixed)
                        return fixed.alwaysOn ? 1 : 0;
                    return gl().isEnabled(cap) ? 1 : 0;
                }
                static glIsBuffer(buffer) {
                    const obj = buffer === 0 ? null : tables().buffers[buffer];
                    return obj && gl().isBuffer(obj) ? 1 : 0;
                }
                static glIsFramebuffer(framebuffer) {
                    const obj = framebuffer === 0 ? null : tables().framebuffers[framebuffer];
                    return obj && gl().isFramebuffer(obj) ? 1 : 0;
                }
                static glIsProgram(program) {
                    const obj = program === 0 ? null : tables().programs[program];
                    return obj && gl().isProgram(obj) ? 1 : 0;
                }
                static glIsRenderbuffer(renderbuffer) {
                    const obj = renderbuffer === 0 ? null : tables().renderbuffers[renderbuffer];
                    return obj && gl().isRenderbuffer(obj) ? 1 : 0;
                }
                static glIsShader(shader) {
                    const obj = shader === 0 ? null : tables().shaders[shader];
                    return obj && gl().isShader(obj) ? 1 : 0;
                }
                static glIsTexture(texture) {
                    const obj = texture === 0 ? null : tables().textures[texture];
                    return obj && gl().isTexture(obj) ? 1 : 0;
                }
                static glIsVertexArray(array) {
                    const obj = array === 0 ? null : tables().vaos[array];
                    return obj && gl().isVertexArray(obj) ? 1 : 0;
                }
                static glGetBooleanv(pname, dataPtr) {
                    const result = gl().getParameter(pname);
                    const HEAPU8 = window.Module.HEAPU8;
                    if (typeof result === "boolean") {
                        HEAPU8[dataPtr] = result ? 1 : 0;
                    }
                    else if (typeof result === "number") {
                        HEAPU8[dataPtr] = result !== 0 ? 1 : 0;
                    }
                    else if (result && typeof result.length === "number") {
                        for (let i = 0; i < result.length; i++) {
                            HEAPU8[dataPtr + i] = result[i] ? 1 : 0;
                        }
                    }
                    else {
                        HEAPU8[dataPtr] = 0;
                    }
                }
                static glGetFloatv(pname, dataPtr) {
                    const result = gl().getParameter(pname);
                    const HEAPF32 = window.Module.HEAPF32;
                    if (typeof result === "number") {
                        HEAPF32[dataPtr >> 2] = result;
                    }
                    else if (result && typeof result.length === "number") {
                        for (let i = 0; i < result.length; i++) {
                            HEAPF32[(dataPtr >> 2) + i] = result[i];
                        }
                    }
                    else {
                        HEAPF32[dataPtr >> 2] = 0;
                    }
                }
                // ------------------------------------------------------------------------------------
                // Uniforms (scalar / vector / matrix / introspection)
                // ------------------------------------------------------------------------------------
                static uniformLoc(location) {
                    return location < 0 ? null : tables().uniforms[location];
                }
                static glUniform2i(location, v0, v1) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2i(loc, v0, v1); }
                static glUniform3i(location, v0, v1, v2) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3i(loc, v0, v1, v2); }
                static glUniform4i(location, v0, v1, v2, v3) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform4i(loc, v0, v1, v2, v3); }
                static glUniform2f(location, v0, v1) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2f(loc, v0, v1); }
                static glUniform3f(location, v0, v1, v2) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3f(loc, v0, v1, v2); }
                static glUniform1ui(location, v0) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform1ui(loc, v0); }
                static glUniform2ui(location, v0, v1) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2ui(loc, v0, v1); }
                static glUniform3ui(location, v0, v1, v2) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3ui(loc, v0, v1, v2); }
                static glUniform4ui(location, v0, v1, v2, v3) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform4ui(loc, v0, v1, v2, v3); }
                // Vector overloads: the JS WebGL2 API accepts typed array views with offset/length.
                // We slice() to a fresh array so wasm heap re-growth (which can detach views) doesn't
                // cause the runtime to throw during the call.
                static glUniform1iv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform1iv(loc, new Int32Array(window.Module.HEAP32.buffer, valuePtr, count).slice()); }
                static glUniform2iv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2iv(loc, new Int32Array(window.Module.HEAP32.buffer, valuePtr, count * 2).slice()); }
                static glUniform3iv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3iv(loc, new Int32Array(window.Module.HEAP32.buffer, valuePtr, count * 3).slice()); }
                static glUniform4iv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform4iv(loc, new Int32Array(window.Module.HEAP32.buffer, valuePtr, count * 4).slice()); }
                static glUniform1fv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform1fv(loc, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count).slice()); }
                static glUniform2fv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2fv(loc, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 2).slice()); }
                static glUniform3fv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3fv(loc, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 3).slice()); }
                static glUniform4fv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform4fv(loc, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 4).slice()); }
                static glUniform1uiv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform1uiv(loc, new Uint32Array(window.Module.HEAPU32.buffer, valuePtr, count).slice()); }
                static glUniform2uiv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform2uiv(loc, new Uint32Array(window.Module.HEAPU32.buffer, valuePtr, count * 2).slice()); }
                static glUniform3uiv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform3uiv(loc, new Uint32Array(window.Module.HEAPU32.buffer, valuePtr, count * 3).slice()); }
                static glUniform4uiv(location, count, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniform4uiv(loc, new Uint32Array(window.Module.HEAPU32.buffer, valuePtr, count * 4).slice()); }
                static glUniformMatrix2fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix2fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 4).slice()); }
                static glUniformMatrix3fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix3fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 9).slice()); }
                static glUniformMatrix4fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix4fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 16).slice()); }
                static glUniformMatrix2x3fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix2x3fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 6).slice()); }
                static glUniformMatrix3x2fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix3x2fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 6).slice()); }
                static glUniformMatrix2x4fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix2x4fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 8).slice()); }
                static glUniformMatrix4x2fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix4x2fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 8).slice()); }
                static glUniformMatrix3x4fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix3x4fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 12).slice()); }
                static glUniformMatrix4x3fv(location, count, transpose, valuePtr) { const loc = WasmGLFunctions.uniformLoc(location); if (loc)
                    gl().uniformMatrix4x3fv(loc, transpose !== 0, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, count * 12).slice()); }
                static glGetUniformfv(program, location, paramsPtr) {
                    const p = tables().programs[program];
                    const loc = WasmGLFunctions.uniformLoc(location);
                    if (!p || !loc)
                        return;
                    const v = gl().getUniform(p, loc);
                    const HEAPF32 = window.Module.HEAPF32;
                    if (typeof v === "number")
                        HEAPF32[paramsPtr >> 2] = v;
                    else if (v && typeof v.length === "number") {
                        for (let i = 0; i < v.length; i++)
                            HEAPF32[(paramsPtr >> 2) + i] = v[i];
                    }
                }
                static glGetUniformiv(program, location, paramsPtr) {
                    const p = tables().programs[program];
                    const loc = WasmGLFunctions.uniformLoc(location);
                    if (!p || !loc)
                        return;
                    const v = gl().getUniform(p, loc);
                    const HEAP32 = window.Module.HEAP32;
                    if (typeof v === "number")
                        HEAP32[paramsPtr >> 2] = v | 0;
                    else if (typeof v === "boolean")
                        HEAP32[paramsPtr >> 2] = v ? 1 : 0;
                    else if (v && typeof v.length === "number") {
                        for (let i = 0; i < v.length; i++)
                            HEAP32[(paramsPtr >> 2) + i] = v[i] | 0;
                    }
                }
                static glGetUniformuiv(program, location, paramsPtr) {
                    const p = tables().programs[program];
                    const loc = WasmGLFunctions.uniformLoc(location);
                    if (!p || !loc)
                        return;
                    const v = gl().getUniform(p, loc);
                    const HEAPU32 = window.Module.HEAPU32;
                    if (typeof v === "number")
                        HEAPU32[paramsPtr >> 2] = v >>> 0;
                    else if (v && typeof v.length === "number") {
                        for (let i = 0; i < v.length; i++)
                            HEAPU32[(paramsPtr >> 2) + i] = v[i] >>> 0;
                    }
                }
                static glGetActiveAttrib(program, index, bufSize, lengthPtr, sizePtr, typePtr, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const info = gl().getActiveAttrib(p, index);
                    if (!info)
                        return;
                    writeInt32(sizePtr, info.size);
                    writeInt32(typePtr, info.type);
                    writeCString(namePtr, bufSize, info.name, lengthPtr);
                }
                static glGetActiveUniform(program, index, bufSize, lengthPtr, sizePtr, typePtr, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const info = gl().getActiveUniform(p, index);
                    if (!info)
                        return;
                    writeInt32(sizePtr, info.size);
                    writeInt32(typePtr, info.type);
                    writeCString(namePtr, bufSize, info.name, lengthPtr);
                }
                static glGetAttribLocation(program, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return -1;
                    return gl().getAttribLocation(p, readCString(namePtr));
                }
                static glBindAttribLocation(program, index, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    gl().bindAttribLocation(p, index, readCString(namePtr));
                }
                static glValidateProgram(program) {
                    const p = tables().programs[program];
                    if (p)
                        gl().validateProgram(p);
                }
                static glGetShaderSource(shader, bufSize, lengthPtr, sourcePtr) {
                    var _a;
                    const sh = tables().shaders[shader];
                    const src = sh ? ((_a = gl().getShaderSource(sh)) !== null && _a !== void 0 ? _a : "") : "";
                    writeCString(sourcePtr, bufSize, src, lengthPtr);
                }
                static glGetFragDataLocation(program, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return -1;
                    return gl().getFragDataLocation(p, readCString(namePtr));
                }
                static glGetUniformBlockIndex(program, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return 0xFFFFFFFF | 0; // GL_INVALID_INDEX
                    return gl().getUniformBlockIndex(p, readCString(namePtr));
                }
                static glUniformBlockBinding(program, uniformBlockIndex, uniformBlockBinding) {
                    const p = tables().programs[program];
                    if (p)
                        gl().uniformBlockBinding(p, uniformBlockIndex, uniformBlockBinding);
                }
                static glGetActiveUniformBlockiv(program, uniformBlockIndex, pname, paramsPtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const v = gl().getActiveUniformBlockParameter(p, uniformBlockIndex, pname);
                    const HEAP32 = window.Module.HEAP32;
                    if (typeof v === "number")
                        HEAP32[paramsPtr >> 2] = v | 0;
                    else if (typeof v === "boolean")
                        HEAP32[paramsPtr >> 2] = v ? 1 : 0;
                    else if (v && typeof v.length === "number") {
                        for (let i = 0; i < v.length; i++)
                            HEAP32[(paramsPtr >> 2) + i] = v[i] | 0;
                    }
                }
                static glGetActiveUniformBlockName(program, uniformBlockIndex, bufSize, lengthPtr, namePtr) {
                    var _a;
                    const p = tables().programs[program];
                    const name = p ? ((_a = gl().getActiveUniformBlockName(p, uniformBlockIndex)) !== null && _a !== void 0 ? _a : "") : "";
                    writeCString(namePtr, bufSize, name, lengthPtr);
                }
                static glGetUniformIndices(program, uniformCount, uniformNamesPtr, uniformIndicesPtr) {
                    var _a;
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const ptrs = readUInt32Array(uniformNamesPtr, uniformCount);
                    const names = [];
                    for (let i = 0; i < uniformCount; i++)
                        names.push(readCString(ptrs[i]));
                    // getUniformIndices returns Iterable<GLuint> in the .d.ts; widen to any for indexed access.
                    const indices = gl().getUniformIndices(p, names);
                    const HEAPU32 = window.Module.HEAPU32;
                    for (let i = 0; i < uniformCount; i++)
                        HEAPU32[(uniformIndicesPtr >> 2) + i] = ((_a = indices === null || indices === void 0 ? void 0 : indices[i]) !== null && _a !== void 0 ? _a : 0xFFFFFFFF) >>> 0;
                }
                static glGetActiveUniformsiv(program, uniformCount, uniformIndicesPtr, pname, paramsPtr) {
                    var _a;
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const indices = readUInt32Array(uniformIndicesPtr, uniformCount);
                    // getActiveUniforms returns Iterable<...>; widen to any.
                    const values = gl().getActiveUniforms(p, Array.from(indices), pname);
                    const HEAP32 = window.Module.HEAP32;
                    for (let i = 0; i < uniformCount; i++) {
                        const v = (_a = values === null || values === void 0 ? void 0 : values[i]) !== null && _a !== void 0 ? _a : 0;
                        HEAP32[(paramsPtr >> 2) + i] = (typeof v === "boolean" ? (v ? 1 : 0) : v) | 0;
                    }
                }
                // ------------------------------------------------------------------------------------
                // Buffers (extras beyond core)
                // ------------------------------------------------------------------------------------
                static glBufferSubData(target, offset, size, dataPtr) {
                    const HEAPU8 = window.Module.HEAPU8;
                    gl().bufferSubData(target, offset, HEAPU8.subarray(dataPtr, dataPtr + size));
                }
                static glCopyBufferSubData(readTarget, writeTarget, readOffset, writeOffset, size) {
                    gl().copyBufferSubData(readTarget, writeTarget, readOffset, writeOffset, size);
                }
                static glBindBufferBase(target, index, buffer) {
                    gl().bindBufferBase(target, index, buffer === 0 ? null : tables().buffers[buffer]);
                }
                static glBindBufferRange(target, index, buffer, offset, size) {
                    gl().bindBufferRange(target, index, buffer === 0 ? null : tables().buffers[buffer], offset, size);
                }
                static glGetBufferSubData(target, offset, size, dataPtr) {
                    const HEAPU8 = window.Module.HEAPU8;
                    gl().getBufferSubData(target, offset, HEAPU8.subarray(dataPtr, dataPtr + size));
                }
                static glGetBufferParameteriv(target, pname, paramsPtr) {
                    const v = gl().getBufferParameter(target, pname);
                    writeInt32(paramsPtr, typeof v === "number" ? (v | 0) : 0);
                }
                // ------------------------------------------------------------------------------------
                // Vertex attribute set/get variants
                // ------------------------------------------------------------------------------------
                static glVertexAttrib1f(index, v0) { gl().vertexAttrib1f(index, v0); }
                static glVertexAttrib2f(index, v0, v1) { gl().vertexAttrib2f(index, v0, v1); }
                static glVertexAttrib3f(index, v0, v1, v2) { gl().vertexAttrib3f(index, v0, v1, v2); }
                static glVertexAttrib4f(index, v0, v1, v2, v3) { gl().vertexAttrib4f(index, v0, v1, v2, v3); }
                static glVertexAttrib1fv(index, vPtr) { gl().vertexAttrib1fv(index, new Float32Array(window.Module.HEAPF32.buffer, vPtr, 1).slice()); }
                static glVertexAttrib2fv(index, vPtr) { gl().vertexAttrib2fv(index, new Float32Array(window.Module.HEAPF32.buffer, vPtr, 2).slice()); }
                static glVertexAttrib3fv(index, vPtr) { gl().vertexAttrib3fv(index, new Float32Array(window.Module.HEAPF32.buffer, vPtr, 3).slice()); }
                static glVertexAttrib4fv(index, vPtr) { gl().vertexAttrib4fv(index, new Float32Array(window.Module.HEAPF32.buffer, vPtr, 4).slice()); }
                static glVertexAttribI4i(index, x, y, z, w) { gl().vertexAttribI4i(index, x, y, z, w); }
                static glVertexAttribI4ui(index, x, y, z, w) { gl().vertexAttribI4ui(index, x, y, z, w); }
                static glVertexAttribI4iv(index, vPtr) { gl().vertexAttribI4iv(index, new Int32Array(window.Module.HEAP32.buffer, vPtr, 4).slice()); }
                static glVertexAttribI4uiv(index, vPtr) { gl().vertexAttribI4uiv(index, new Uint32Array(window.Module.HEAPU32.buffer, vPtr, 4).slice()); }
                static glVertexAttribIPointer(index, size, type, stride, pointer) {
                    gl().vertexAttribIPointer(index, size, type, stride, pointer);
                }
                static glVertexAttribDivisor(index, divisor) { gl().vertexAttribDivisor(index, divisor); }
                static glGetVertexAttribfv(index, pname, paramsPtr) {
                    const v = gl().getVertexAttrib(index, pname);
                    const HEAPF32 = window.Module.HEAPF32;
                    if (typeof v === "number")
                        HEAPF32[paramsPtr >> 2] = v;
                    else if (v && typeof v.length === "number")
                        for (let i = 0; i < v.length; i++)
                            HEAPF32[(paramsPtr >> 2) + i] = v[i];
                }
                static glGetVertexAttribiv(index, pname, paramsPtr) {
                    const v = gl().getVertexAttrib(index, pname);
                    const HEAP32 = window.Module.HEAP32;
                    if (typeof v === "number")
                        HEAP32[paramsPtr >> 2] = v | 0;
                    else if (typeof v === "boolean")
                        HEAP32[paramsPtr >> 2] = v ? 1 : 0;
                    else if (v && typeof v.length === "number")
                        for (let i = 0; i < v.length; i++)
                            HEAP32[(paramsPtr >> 2) + i] = v[i] | 0;
                }
                static glGetVertexAttribIiv(index, pname, paramsPtr) { WasmGLFunctions.glGetVertexAttribiv(index, pname, paramsPtr); }
                static glGetVertexAttribIuiv(index, pname, paramsPtr) {
                    const v = gl().getVertexAttrib(index, pname);
                    const HEAPU32 = window.Module.HEAPU32;
                    if (typeof v === "number")
                        HEAPU32[paramsPtr >> 2] = v >>> 0;
                    else if (v && typeof v.length === "number")
                        for (let i = 0; i < v.length; i++)
                            HEAPU32[(paramsPtr >> 2) + i] = v[i] >>> 0;
                }
                static glGetVertexAttribPointerv(index, pname, pointerPtr) {
                    const v = gl().getVertexAttribOffset(index, pname);
                    writeInt32(pointerPtr, v | 0);
                }
                // ------------------------------------------------------------------------------------
                // Textures (extras)
                // ------------------------------------------------------------------------------------
                static glTexParameterf(target, pname, param) { gl().texParameterf(target, pname, param); }
                static glTexParameterfv(target, pname, paramsPtr) {
                    const v = window.Module.HEAPF32[paramsPtr >> 2];
                    gl().texParameterf(target, pname, v);
                }
                static glTexParameteriv(target, pname, paramsPtr) {
                    const v = window.Module.HEAP32[paramsPtr >> 2];
                    gl().texParameteri(target, pname, v);
                }
                static glGenerateMipmap(target) { gl().generateMipmap(target); }
                static glCopyTexImage2D(target, level, internalformat, x, y, width, height, border) {
                    gl().copyTexImage2D(target, level, internalformat, x, y, width, height, border);
                }
                static glCopyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height) {
                    gl().copyTexSubImage2D(target, level, xoffset, yoffset, x, y, width, height);
                }
                static glTexStorage2D(target, levels, internalformat, width, height) {
                    gl().texStorage2D(target, levels, internalformat, width, height);
                }
                static glTexStorage3D(target, levels, internalformat, width, height, depth) {
                    gl().texStorage3D(target, levels, internalformat, width, height, depth);
                }
                static glCompressedTexImage2D(target, level, internalformat, width, height, border, imageSize, dataPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.compressedTexImage2D(target, level, internalformat, width, height, border, imageSize, dataPtr);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    ctx.compressedTexImage2D(target, level, internalformat, width, height, border, HEAPU8.subarray(dataPtr, dataPtr + imageSize));
                }
                static glGetTexParameterfv(target, pname, paramsPtr) {
                    const v = gl().getTexParameter(target, pname);
                    window.Module.HEAPF32[paramsPtr >> 2] = typeof v === "number" ? v : 0;
                }
                static glGetTexParameteriv(target, pname, paramsPtr) {
                    const v = gl().getTexParameter(target, pname);
                    writeInt32(paramsPtr, typeof v === "number" ? (v | 0) : (v ? 1 : 0));
                }
                static glGetInternalformativ(target, internalformat, pname, bufSize, paramsPtr) {
                    // WebGL2 spec returns Int32Array but lib.dom.d.ts types it as Iterable<number>; cast through any.
                    const v = gl().getInternalformatParameter(target, internalformat, pname);
                    if (!v)
                        return;
                    const HEAP32 = window.Module.HEAP32;
                    const len = Math.min(bufSize, v.length);
                    for (let i = 0; i < len; i++)
                        HEAP32[(paramsPtr >> 2) + i] = v[i] | 0;
                }
                // ------------------------------------------------------------------------------------
                // Framebuffers (extras)
                // ------------------------------------------------------------------------------------
                static glDrawBuffers(n, bufsPtr) {
                    const HEAP32 = window.Module.HEAP32;
                    const arr = [];
                    for (let i = 0; i < n; i++)
                        arr.push(HEAP32[(bufsPtr >> 2) + i]);
                    gl().drawBuffers(arr);
                }
                static glClearBufferiv(buffer, drawbuffer, valuePtr) {
                    gl().clearBufferiv(buffer, drawbuffer, new Int32Array(window.Module.HEAP32.buffer, valuePtr, 4).slice());
                }
                static glClearBufferuiv(buffer, drawbuffer, valuePtr) {
                    gl().clearBufferuiv(buffer, drawbuffer, new Uint32Array(window.Module.HEAPU32.buffer, valuePtr, 4).slice());
                }
                static glClearBufferfv(buffer, drawbuffer, valuePtr) {
                    gl().clearBufferfv(buffer, drawbuffer, new Float32Array(window.Module.HEAPF32.buffer, valuePtr, 4).slice());
                }
                static glClearBufferfi(buffer, drawbuffer, depth, stencil) {
                    gl().clearBufferfi(buffer, drawbuffer, depth, stencil);
                }
                static glRenderbufferStorageMultisample(target, samples, internalformat, width, height) {
                    gl().renderbufferStorageMultisample(target, samples, internalformat, width, height);
                }
                static glFramebufferTextureLayer(target, attachment, texture, level, layer) {
                    gl().framebufferTextureLayer(target, attachment, texture === 0 ? null : tables().textures[texture], level, layer);
                }
                static glInvalidateFramebuffer(target, numAttachments, attachmentsPtr) {
                    const HEAP32 = window.Module.HEAP32;
                    const arr = [];
                    for (let i = 0; i < numAttachments; i++)
                        arr.push(HEAP32[(attachmentsPtr >> 2) + i]);
                    gl().invalidateFramebuffer(target, arr);
                }
                static glInvalidateSubFramebuffer(target, numAttachments, attachmentsPtr, x, y, width, height) {
                    const HEAP32 = window.Module.HEAP32;
                    const arr = [];
                    for (let i = 0; i < numAttachments; i++)
                        arr.push(HEAP32[(attachmentsPtr >> 2) + i]);
                    gl().invalidateSubFramebuffer(target, arr, x, y, width, height);
                }
                static glGetFramebufferAttachmentParameteriv(target, attachment, pname, paramsPtr) {
                    const v = gl().getFramebufferAttachmentParameter(target, attachment, pname);
                    writeInt32(paramsPtr, typeof v === "number" ? (v | 0) : (v && typeof v.name === "number" ? v.name : 0));
                }
                static glGetRenderbufferParameteriv(target, pname, paramsPtr) {
                    const v = gl().getRenderbufferParameter(target, pname);
                    writeInt32(paramsPtr, typeof v === "number" ? (v | 0) : 0);
                }
                static glClearDepthf(d) { gl().clearDepth(d); }
                static glClearStencil(s) { gl().clearStencil(s); }
                // ------------------------------------------------------------------------------------
                // Drawing variants
                // ------------------------------------------------------------------------------------
                static glDrawArraysInstanced(mode, first, count, instanceCount) {
                    const ctx = gl();
                    warnIfStencilMismatch(ctx);
                    ctx.drawArraysInstanced(mode, first, count, instanceCount);
                }
                static glDrawElementsInstanced(mode, count, type, indicesPtr, instanceCount) {
                    const ctx = gl();
                    warnIfStencilMismatch(ctx);
                    ctx.drawElementsInstanced(mode, count, type, indicesPtr, instanceCount);
                }
                static glDrawRangeElements(mode, start, end, count, type, indicesPtr) {
                    const ctx = gl();
                    warnIfStencilMismatch(ctx);
                    ctx.drawRangeElements(mode, start, end, count, type, indicesPtr);
                }
                // ------------------------------------------------------------------------------------
                // Samplers
                // ------------------------------------------------------------------------------------
                static glGenSamplers(count, samplersPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < count; i++) {
                        const s = ctx.createSampler();
                        const id = getNewId(t.samplers);
                        if (s)
                            s.name = id;
                        t.samplers[id] = s;
                        writeUInt32(samplersPtr + i * 4, id);
                    }
                }
                static glDeleteSamplers(count, samplersPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(samplersPtr, count);
                    for (let i = 0; i < count; i++) {
                        const s = t.samplers[ids[i]];
                        if (s) {
                            ctx.deleteSampler(s);
                            t.samplers[ids[i]] = null;
                        }
                    }
                }
                static glIsSampler(sampler) {
                    const obj = sampler === 0 ? null : tables().samplers[sampler];
                    return obj && gl().isSampler(obj) ? 1 : 0;
                }
                static glBindSampler(unit, sampler) {
                    gl().bindSampler(unit, sampler === 0 ? null : tables().samplers[sampler]);
                }
                static glSamplerParameteri(sampler, pname, param) {
                    const s = tables().samplers[sampler];
                    if (s)
                        gl().samplerParameteri(s, pname, param);
                }
                static glSamplerParameterf(sampler, pname, param) {
                    const s = tables().samplers[sampler];
                    if (s)
                        gl().samplerParameterf(s, pname, param);
                }
                static glSamplerParameteriv(sampler, pname, paramsPtr) {
                    const s = tables().samplers[sampler];
                    if (!s)
                        return;
                    const v = window.Module.HEAP32[paramsPtr >> 2];
                    gl().samplerParameteri(s, pname, v);
                }
                static glSamplerParameterfv(sampler, pname, paramsPtr) {
                    const s = tables().samplers[sampler];
                    if (!s)
                        return;
                    const v = window.Module.HEAPF32[paramsPtr >> 2];
                    gl().samplerParameterf(s, pname, v);
                }
                static glGetSamplerParameteriv(sampler, pname, paramsPtr) {
                    const s = tables().samplers[sampler];
                    if (!s)
                        return;
                    const v = gl().getSamplerParameter(s, pname);
                    writeInt32(paramsPtr, typeof v === "number" ? (v | 0) : 0);
                }
                static glGetSamplerParameterfv(sampler, pname, paramsPtr) {
                    const s = tables().samplers[sampler];
                    if (!s)
                        return;
                    const v = gl().getSamplerParameter(s, pname);
                    window.Module.HEAPF32[paramsPtr >> 2] = typeof v === "number" ? v : 0;
                }
                // ------------------------------------------------------------------------------------
                // Queries
                // ------------------------------------------------------------------------------------
                static glGenQueries(n, idsPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const q = ctx.createQuery();
                        const id = getNewId(t.queries);
                        if (q)
                            q.name = id;
                        t.queries[id] = q;
                        writeUInt32(idsPtr + i * 4, id);
                    }
                }
                static glDeleteQueries(n, idsPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(idsPtr, n);
                    for (let i = 0; i < n; i++) {
                        const q = t.queries[ids[i]];
                        if (q) {
                            ctx.deleteQuery(q);
                            t.queries[ids[i]] = null;
                        }
                    }
                }
                static glIsQuery(id) {
                    const obj = id === 0 ? null : tables().queries[id];
                    return obj && gl().isQuery(obj) ? 1 : 0;
                }
                static glBeginQuery(target, id) {
                    const q = tables().queries[id];
                    if (q)
                        gl().beginQuery(target, q);
                }
                static glEndQuery(target) { gl().endQuery(target); }
                static glGetQueryiv(target, pname, paramsPtr) {
                    const v = gl().getQuery(target, pname);
                    writeInt32(paramsPtr, v && typeof v.name === "number" ? v.name : 0);
                }
                static glGetQueryObjectuiv(id, pname, paramsPtr) {
                    const q = tables().queries[id];
                    if (!q)
                        return;
                    const v = gl().getQueryParameter(q, pname);
                    writeUInt32(paramsPtr, typeof v === "number" ? (v >>> 0) : (v ? 1 : 0));
                }
                // ------------------------------------------------------------------------------------
                // Sync objects
                // ------------------------------------------------------------------------------------
                static glFenceSync(condition, flags) {
                    const ctx = gl();
                    const t = tables();
                    const s = ctx.fenceSync(condition, flags);
                    if (!s)
                        return 0;
                    const id = getNewId(t.syncs);
                    s.name = id;
                    t.syncs[id] = s;
                    return id;
                }
                static glIsSync(sync) {
                    const obj = sync === 0 ? null : tables().syncs[sync];
                    return obj && gl().isSync(obj) ? 1 : 0;
                }
                static glDeleteSync(sync) {
                    const t = tables();
                    const s = t.syncs[sync];
                    if (s) {
                        gl().deleteSync(s);
                        t.syncs[sync] = null;
                    }
                }
                static glClientWaitSync(sync, flags, timeout) {
                    const s = tables().syncs[sync];
                    return s ? gl().clientWaitSync(s, flags, timeout) : 0x911A; // GL_WAIT_FAILED
                }
                static glWaitSync(sync, flags, timeout) {
                    const s = tables().syncs[sync];
                    if (s)
                        gl().waitSync(s, flags, timeout);
                }
                static glGetSynciv(sync, pname, bufSize, lengthPtr, valuesPtr) {
                    const s = tables().syncs[sync];
                    if (!s || bufSize <= 0)
                        return;
                    const v = gl().getSyncParameter(s, pname);
                    writeInt32(valuesPtr, typeof v === "number" ? (v | 0) : 0);
                    if (lengthPtr !== 0)
                        writeInt32(lengthPtr, 1);
                }
                // ------------------------------------------------------------------------------------
                // Transform feedback
                // ------------------------------------------------------------------------------------
                static glGenTransformFeedbacks(n, idsPtr) {
                    const ctx = gl();
                    const t = tables();
                    for (let i = 0; i < n; i++) {
                        const tf = ctx.createTransformFeedback();
                        const id = getNewId(t.transformFeedbacks);
                        if (tf)
                            tf.name = id;
                        t.transformFeedbacks[id] = tf;
                        writeUInt32(idsPtr + i * 4, id);
                    }
                }
                static glDeleteTransformFeedbacks(n, idsPtr) {
                    const ctx = gl();
                    const t = tables();
                    const ids = readUInt32Array(idsPtr, n);
                    for (let i = 0; i < n; i++) {
                        const tf = t.transformFeedbacks[ids[i]];
                        if (tf) {
                            ctx.deleteTransformFeedback(tf);
                            t.transformFeedbacks[ids[i]] = null;
                        }
                    }
                }
                static glIsTransformFeedback(id) {
                    const obj = id === 0 ? null : tables().transformFeedbacks[id];
                    return obj && gl().isTransformFeedback(obj) ? 1 : 0;
                }
                static glBindTransformFeedback(target, id) {
                    gl().bindTransformFeedback(target, id === 0 ? null : tables().transformFeedbacks[id]);
                }
                static glBeginTransformFeedback(primitiveMode) { gl().beginTransformFeedback(primitiveMode); }
                static glEndTransformFeedback() { gl().endTransformFeedback(); }
                static glPauseTransformFeedback() { gl().pauseTransformFeedback(); }
                static glResumeTransformFeedback() { gl().resumeTransformFeedback(); }
                static glTransformFeedbackVaryings(program, count, varyingsPtr, bufferMode) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const ptrs = readUInt32Array(varyingsPtr, count);
                    const names = [];
                    for (let i = 0; i < count; i++)
                        names.push(readCString(ptrs[i]));
                    gl().transformFeedbackVaryings(p, names, bufferMode);
                }
                static glGetTransformFeedbackVarying(program, index, bufSize, lengthPtr, sizePtr, typePtr, namePtr) {
                    const p = tables().programs[program];
                    if (!p)
                        return;
                    const info = gl().getTransformFeedbackVarying(p, index);
                    if (!info)
                        return;
                    writeInt32(sizePtr, info.size);
                    writeInt32(typePtr, info.type);
                    writeCString(namePtr, bufSize, info.name, lengthPtr);
                }
                // ------------------------------------------------------------------------------------
                // Misc: glGetAttachedShaders, glGetShaderPrecisionFormat, indexed queries, glGetStringi
                // ------------------------------------------------------------------------------------
                static glGetAttachedShaders(program, maxCount, countPtr, shadersPtr) {
                    var _a;
                    const p = tables().programs[program];
                    if (!p) {
                        if (countPtr)
                            writeInt32(countPtr, 0);
                        return;
                    }
                    const shaders = (_a = gl().getAttachedShaders(p)) !== null && _a !== void 0 ? _a : [];
                    const t = tables();
                    const n = Math.min(maxCount, shaders.length);
                    if (countPtr)
                        writeInt32(countPtr, n);
                    // Reverse-map WebGLShader objects to ids using the shaders table.
                    for (let i = 0; i < n; i++) {
                        let id = 0;
                        const sh = shaders[i];
                        if (sh && typeof sh.name === "number") {
                            id = sh.name;
                        }
                        else {
                            for (let j = 1; j < t.shaders.length; j++) {
                                if (t.shaders[j] === sh) {
                                    id = j;
                                    break;
                                }
                            }
                        }
                        writeUInt32(shadersPtr + i * 4, id);
                    }
                }
                static glGetShaderPrecisionFormat(shadertype, precisiontype, rangePtr, precisionPtr) {
                    const fmt = gl().getShaderPrecisionFormat(shadertype, precisiontype);
                    if (!fmt) {
                        writeInt32(rangePtr, 0);
                        writeInt32(rangePtr + 4, 0);
                        writeInt32(precisionPtr, 0);
                        return;
                    }
                    writeInt32(rangePtr, fmt.rangeMin | 0);
                    writeInt32(rangePtr + 4, fmt.rangeMax | 0);
                    writeInt32(precisionPtr, fmt.precision | 0);
                }
                static glGetIntegeri_v(target, index, dataPtr) {
                    const v = gl().getIndexedParameter(target, index);
                    writeInt32(dataPtr, typeof v === "number" ? (v | 0) : 0);
                }
                static glGetInteger64v(pname, dataPtr) {
                    const v = gl().getParameter(pname);
                    // 64-bit write: low and high 32-bit halves. JavaScript Numbers can represent up to 2^53.
                    const n = typeof v === "number" ? v : 0;
                    const HEAP32 = window.Module.HEAP32;
                    HEAP32[dataPtr >> 2] = n | 0;
                    HEAP32[(dataPtr >> 2) + 1] = Math.floor(n / 0x100000000) | 0;
                }
                static glGetInteger64i_v(target, index, dataPtr) {
                    const v = gl().getIndexedParameter(target, index);
                    const n = typeof v === "number" ? v : 0;
                    const HEAP32 = window.Module.HEAP32;
                    HEAP32[dataPtr >> 2] = n | 0;
                    HEAP32[(dataPtr >> 2) + 1] = Math.floor(n / 0x100000000) | 0;
                }
                static glGetBufferParameteri64v(target, pname, paramsPtr) {
                    const v = gl().getBufferParameter(target, pname);
                    const n = typeof v === "number" ? v : 0;
                    const HEAP32 = window.Module.HEAP32;
                    HEAP32[paramsPtr >> 2] = n | 0;
                    HEAP32[(paramsPtr >> 2) + 1] = Math.floor(n / 0x100000000) | 0;
                }
                static glGetStringi(name, index) {
                    var _a;
                    // WebGL2 exposes indexed strings only for GL_EXTENSIONS (via getSupportedExtensions).
                    const key = name * 0x10000 + index;
                    const cached = stringCache[key];
                    if (cached)
                        return cached;
                    let value = "";
                    if (name === 0x1F03 /* GL_EXTENSIONS */) {
                        const exts = (_a = gl().getSupportedExtensions()) !== null && _a !== void 0 ? _a : [];
                        if (index < exts.length)
                            value = exts[index];
                    }
                    const ptr = allocCString(value);
                    stringCache[key] = ptr;
                    return ptr;
                }
                // ------------------------------------------------------------------------------------
                // Large-arity shims (>8 args, routed through uno_gl_shim.c)
                // ------------------------------------------------------------------------------------
                static glTexSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixelsPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, pixelsPtr);
                        return;
                    }
                    if (pixelsPtr === 0) {
                        ctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, null);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    const byteCount = width * height * 4;
                    ctx.texSubImage2D(target, level, xoffset, yoffset, width, height, format, type, HEAPU8.subarray(pixelsPtr, pixelsPtr + byteCount));
                }
                static glTexImage3D(target, level, internalformat, width, height, depth, border, format, type, pixelsPtr) {
                    const ctx = gl();
                    if (type === 0x1401 /* GL_UNSIGNED_BYTE */) {
                        if (internalformat === 0x1907 /* GL_RGB */)
                            internalformat = 0x8051 /* GL_RGB8 */;
                        else if (internalformat === 0x1908 /* GL_RGBA */)
                            internalformat = 0x8058 /* GL_RGBA8 */;
                    }
                    if (unpackPboBound(ctx)) {
                        ctx.texImage3D(target, level, internalformat, width, height, depth, border, format, type, pixelsPtr);
                    }
                    else if (pixelsPtr === 0) {
                        ctx.texImage3D(target, level, internalformat, width, height, depth, border, format, type, null);
                    }
                    else {
                        const HEAPU8 = window.Module.HEAPU8;
                        const byteCount = width * height * depth * 4;
                        ctx.texImage3D(target, level, internalformat, width, height, depth, border, format, type, HEAPU8.subarray(pixelsPtr, pixelsPtr + byteCount));
                    }
                }
                static glTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixelsPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.texSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, pixelsPtr);
                        return;
                    }
                    if (pixelsPtr === 0) {
                        ctx.texSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, null);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    const byteCount = width * height * depth * 4;
                    ctx.texSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, type, HEAPU8.subarray(pixelsPtr, pixelsPtr + byteCount));
                }
                static glCopyTexSubImage3D(target, level, xoffset, yoffset, zoffset, x, y, width, height) {
                    gl().copyTexSubImage3D(target, level, xoffset, yoffset, zoffset, x, y, width, height);
                }
                static glCompressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, dataPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.compressedTexImage3D(target, level, internalformat, width, height, depth, border, imageSize, dataPtr);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    ctx.compressedTexImage3D(target, level, internalformat, width, height, depth, border, HEAPU8.subarray(dataPtr, dataPtr + imageSize));
                }
                static glCompressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, dataPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.compressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, imageSize, dataPtr);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    ctx.compressedTexSubImage2D(target, level, xoffset, yoffset, width, height, format, HEAPU8.subarray(dataPtr, dataPtr + imageSize));
                }
                static glCompressedTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, dataPtr) {
                    const ctx = gl();
                    if (unpackPboBound(ctx)) {
                        ctx.compressedTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, imageSize, dataPtr);
                        return;
                    }
                    const HEAPU8 = window.Module.HEAPU8;
                    ctx.compressedTexSubImage3D(target, level, xoffset, yoffset, zoffset, width, height, depth, format, HEAPU8.subarray(dataPtr, dataPtr + imageSize));
                }
                static glBlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter) {
                    gl().blitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
                }
            }
            // Desktop/ES capabilities WebGL2 has no toggle for. alwaysOn: the behavior is
            // unconditionally active in WebGL2 (enabling is a no-op, disabling is impossible);
            // otherwise the feature does not exist (enabling is impossible, disabling is a no-op).
            // Passing these to WebGL2's enable/disable would raise INVALID_ENUM on every call.
            WasmGLFunctions.fixedCaps = {
                0x809D: { name: "GL_MULTISAMPLE", alwaysOn: true },
                0x884F: { name: "GL_TEXTURE_CUBE_MAP_SEAMLESS", alwaysOn: true },
                0x8642: { name: "GL_PROGRAM_POINT_SIZE", alwaysOn: true },
                0x8D69: { name: "GL_PRIMITIVE_RESTART_FIXED_INDEX", alwaysOn: true },
                0x8F9D: { name: "GL_PRIMITIVE_RESTART", alwaysOn: true },
                0x8DB9: { name: "GL_FRAMEBUFFER_SRGB", alwaysOn: false },
                0x0B20: { name: "GL_LINE_SMOOTH", alwaysOn: false },
                0x0B41: { name: "GL_POLYGON_SMOOTH", alwaysOn: false },
                0x864F: { name: "GL_DEPTH_CLAMP", alwaysOn: false },
            };
            Runtime.WasmGLFunctions = WasmGLFunctions;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class WasmNativeOpenGLWrapper {
                static tryCreateInstance() {
                    try {
                        // Detached canvas - GLCanvasElement reads pixels back from an FBO into a WriteableBitmap,
                        // so the WebGL drawing buffer never needs to be visible.
                        const canvas = document.createElement("canvas");
                        canvas.width = 1;
                        canvas.height = 1;
                        const glCtx = Runtime.EmscriptenWebGL.createContext(canvas, { preserveDrawingBuffer: 1 });
                        if (!glCtx || glCtx < 0) {
                            throw `Failed to create offscreen WebGL context (err=${glCtx})`;
                        }
                        return { success: true, glCtxHandle: glCtx };
                    }
                    catch (e) {
                        return { success: false, error: e.toString() };
                    }
                }
                static makeCurrent(glCtxHandle) {
                    const anyGL = Runtime.EmscriptenWebGL.assertGL();
                    const prev = (anyGL.currentContext && anyGL.currentContext.handle) || 0;
                    anyGL.makeContextCurrent(glCtxHandle);
                    return prev;
                }
                static restoreContext(prevHandle) {
                    Runtime.EmscriptenWebGL.assertGL().makeContextCurrent(prevHandle);
                }
                static destroy(glCtxHandle) {
                    const anyGL = Runtime.EmscriptenWebGL.assertGL();
                    if (anyGL.currentContext && anyGL.currentContext.handle === glCtxHandle) {
                        anyGL.makeContextCurrent(0);
                    }
                    anyGL.deleteContext(glCtxHandle);
                }
            }
            Runtime.WasmNativeOpenGLWrapper = WasmNativeOpenGLWrapper;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class WebAssemblyWindowWrapper {
                constructor(owner) {
                    this.lastReportedOcclusion = -1;
                    this.owner = owner;
                }
                static getAssemblyExports() {
                    return WebAssemblyWindowWrapper.assemblyExports;
                }
                static async initialize(owner) {
                    const instance = new WebAssemblyWindowWrapper(owner);
                    await instance.build();
                    WebAssemblyWindowWrapper.activeInstances[owner] = instance;
                }
                static persistBootstrapperLoader() {
                    let bootstrapperLoaders = document.getElementsByClassName(WebAssemblyWindowWrapper.unoPersistentLoaderClassName);
                    if (bootstrapperLoaders.length > 0) {
                        let bootstrapperLoader = bootstrapperLoaders[0];
                        bootstrapperLoader.classList.add(WebAssemblyWindowWrapper.unoKeepLoaderClassName);
                    }
                }
                async build() {
                    WebAssemblyWindowWrapper.assemblyExports = await window.Module.getAssemblyExports("Uno.UI.Runtime.WebAssembly.Browser");
                    this.onResize = WebAssemblyWindowWrapper.assemblyExports.Uno.UI.Runtime.WebAssemblyWindowWrapper.OnResize;
                    this.onViewportOcclusionChanged = WebAssemblyWindowWrapper.assemblyExports.Uno.UI.Runtime.WebAssemblyWindowWrapper.OnViewportOcclusionChanged;
                    this.onWindowFocusChanged = WebAssemblyWindowWrapper.assemblyExports.Uno.UI.Runtime.WebAssemblyWindowWrapper.OnWindowFocusChanged;
                    this.onDocumentVisibilityChanged = WebAssemblyWindowWrapper.assemblyExports.Uno.UI.Runtime.WebAssemblyWindowWrapper.OnDocumentVisibilityChanged;
                    this.containerElement = document.getElementById("uno-body");
                    if (!this.containerElement) {
                        // If not found, we simply create a new one.
                        this.containerElement = document.createElement("div");
                        this.containerElement.id = "uno-root";
                        document.body.appendChild(this.containerElement);
                    }
                    this.canvasElement = document.createElement("canvas");
                    this.canvasElement.id = Runtime.UnoDomIds.canvas;
                    this.canvasElement.setAttribute("aria-hidden", "true");
                    this.containerElement.appendChild(this.canvasElement);
                    Runtime.Accessibility.setup();
                    window.addEventListener("resize", x => this.resize());
                    this.observeViewportSize();
                    window.addEventListener("contextmenu", x => {
                        x.preventDefault();
                    });
                    // The browser has no window activation; focus/blur and page visibility are the equivalents.
                    window.addEventListener("focus", () => this.onWindowFocusChanged(this.owner, true));
                    window.addEventListener("blur", () => this.onWindowFocusChanged(this.owner, false));
                    document.addEventListener("visibilitychange", () => this.onDocumentVisibilityChanged(this.owner, !document.hidden));
                    // The on-screen keyboard shrinks the visual viewport without firing window "resize",
                    // so track it separately to report keyboard occlusion to the InputPane (issue 3).
                    if (window.visualViewport) {
                        window.visualViewport.addEventListener("resize", () => this.reportKeyboardOcclusion());
                        window.visualViewport.addEventListener("scroll", () => this.reportKeyboardOcclusion());
                    }
                    this.resize();
                }
                // Some mobile browsers (Chrome on iPad) can raise window "resize" before the layout
                // viewport has reflowed to the new orientation size, without raising a corrective
                // event afterward — leaving the Skia surface sized from a stale documentElement rect.
                // Observe the element resize() measures from and re-dispatch "resize" once it reports
                // its settled dimensions (ResizeObserver fires post-layout, so no polling is needed).
                observeViewportSize() {
                    if (typeof ResizeObserver === "undefined") {
                        console.debug("ResizeObserver is not available; the viewport settle correction is disabled.");
                        return;
                    }
                    const rootElement = document.documentElement;
                    const initialRect = rootElement.getBoundingClientRect();
                    this.lastObservedViewportSize = { width: initialRect.width, height: initialRect.height };
                    this.viewportResizeObserver = new ResizeObserver(() => {
                        const rect = rootElement.getBoundingClientRect();
                        const lastSize = this.lastObservedViewportSize;
                        if (lastSize && rect.width === lastSize.width && rect.height === lastSize.height) {
                            return;
                        }
                        this.lastObservedViewportSize = { width: rect.width, height: rect.height };
                        // Dispatch globally rather than calling this.resize(): other listeners size from
                        // this event too (BrowserRenderer sizes the canvas), and app code is owed the
                        // corrective resize the browser never fired.
                        window.dispatchEvent(new Event("resize"));
                    });
                    this.viewportResizeObserver.observe(rootElement);
                }
                // Reports how much of the viewport the on-screen keyboard occludes, in the same CSS pixels
                // as the window bounds. Gated on the invisible text input (#uno-input) being the active
                // element: the soft keyboard is only up during text entry, so viewport changes at any other
                // time (e.g. the mobile address bar collapsing) are deliberately reported as no occlusion.
                reportKeyboardOcclusion() {
                    var _a;
                    const viewport = window.visualViewport;
                    if (!viewport || !this.onViewportOcclusionChanged) {
                        return;
                    }
                    // Check focus before touching layout: visualViewport scroll/resize fires per frame during
                    // momentum scroll and keyboard animation, and getBoundingClientRect forces a reflow. When
                    // no text input is focused there is no keyboard, so skip the reflow entirely and report a
                    // single zero only if the last report was non-zero.
                    if (((_a = document.activeElement) === null || _a === void 0 ? void 0 : _a.id) !== Runtime.UnoDomIds.input) {
                        if (this.lastReportedOcclusion !== 0) {
                            this.lastReportedOcclusion = 0;
                            this.onViewportOcclusionChanged(this.owner, 0, 0, 0);
                        }
                        return;
                    }
                    const layout = document.documentElement.getBoundingClientRect();
                    const occludedHeight = Math.max(0, layout.height - viewport.height - viewport.offsetTop);
                    if (occludedHeight === this.lastReportedOcclusion) {
                        return;
                    }
                    this.lastReportedOcclusion = occludedHeight;
                    this.onViewportOcclusionChanged(this.owner, layout.width, layout.height, occludedHeight);
                }
                static removeLoading() {
                    var _a, _b, _c, _d;
                    const element = document.getElementById(WebAssemblyWindowWrapper.loadingElementId);
                    if (element) {
                        element.parentElement.removeChild(element);
                    }
                    let bootstrapperLoaders = document.getElementsByClassName(WebAssemblyWindowWrapper.unoPersistentLoaderClassName);
                    if (bootstrapperLoaders.length > 0) {
                        // Newer bootstrappers fade the loader out themselves; older ones don't expose dismissLoader.
                        const dismissLoader = (_d = (_c = (_b = (_a = globalThis.Uno) === null || _a === void 0 ? void 0 : _a.WebAssembly) === null || _b === void 0 ? void 0 : _b.Bootstrap) === null || _c === void 0 ? void 0 : _c.Bootstrapper) === null || _d === void 0 ? void 0 : _d.dismissLoader;
                        if (typeof dismissLoader === "function") {
                            dismissLoader();
                        }
                        else {
                            let bootstrapperLoader = bootstrapperLoaders[0];
                            bootstrapperLoader.parentElement.removeChild(bootstrapperLoader);
                        }
                    }
                }
                static getInstance(owner) {
                    const instance = this.activeInstances[owner];
                    if (!instance) {
                        throw `WebAssemblyWindowWrapper for instance ${owner} not found.`;
                    }
                    return instance;
                }
                static getContainerId(owner) {
                    return WebAssemblyWindowWrapper.getInstance(owner).containerElement.id;
                }
                static getCanvasId(owner) {
                    return WebAssemblyWindowWrapper.getInstance(owner).canvasElement.id;
                }
                static getSearchParams() {
                    if (typeof URLSearchParams === "function") {
                        return new URLSearchParams(window.location.search).toString();
                    }
                    else {
                        const queryIndex = document.location.search.indexOf("?");
                        if (queryIndex !== -1) {
                            return document.location.search.substring(queryIndex + 1);
                        }
                        return "";
                    }
                }
                resize() {
                    // Measures the same element observeViewportSize() watches — keep the two in sync.
                    var rect = document.documentElement.getBoundingClientRect();
                    this.lastObservedViewportSize = { width: rect.width, height: rect.height };
                    this.onResize(this.owner, rect.width, rect.height, globalThis.devicePixelRatio);
                }
                static setCursor(cssCursor) {
                    document.body.style.cursor = cssCursor;
                }
                resizeWindow(width, height) {
                    window.resizeTo(width, height);
                }
                moveWindow(x, y) {
                    window.moveTo(x, y);
                }
            }
            WebAssemblyWindowWrapper.unoPersistentLoaderClassName = "uno-persistent-loader";
            WebAssemblyWindowWrapper.loadingElementId = "uno-loading";
            WebAssemblyWindowWrapper.unoKeepLoaderClassName = "uno-keep-loader";
            WebAssemblyWindowWrapper.activeInstances = {};
            Runtime.WebAssemblyWindowWrapper = WebAssemblyWindowWrapper;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            class WebGlBrowserRenderer {
                constructor(canvas) {
                    this.canvas = canvas;
                    this.anyGL = Runtime.EmscriptenWebGL.assertGL();
                    this.glCtx = Runtime.EmscriptenWebGL.createContext(this.canvas, { antialias: 1 });
                    if (!this.glCtx || this.glCtx < 0)
                        throw `Failed to create WebGL context: err ${this.glCtx}`;
                }
                static tryCreateInstance(canvasId) {
                    try {
                        if (!canvasId)
                            throw 'No <canvas> element or ID was provided';
                        const canvas = document.getElementById(canvasId);
                        if (!canvas)
                            throw `No <canvas> with id ${canvasId} was found`;
                        const instance = new WebGlBrowserRenderer(canvas);
                        WebGlBrowserRenderer.makeCurrent(instance);
                        // Starting from .NET 7 the GLctx is defined in an inaccessible scope
                        // when the current GL context changes. We need to pick it up from the
                        // GL.currentContext instead.
                        let currentGLctx = instance.anyGL.currentContext && instance.anyGL.currentContext.GLctx;
                        if (!currentGLctx)
                            throw `Failed to get current WebGL context`;
                        return {
                            success: true,
                            instance: instance,
                            fbo: currentGLctx.getParameter(currentGLctx.FRAMEBUFFER_BINDING),
                            stencil: currentGLctx.getParameter(currentGLctx.STENCIL_BITS),
                            sample: 0,
                            depth: currentGLctx.getParameter(currentGLctx.DEPTH_BITS),
                        };
                    }
                    catch (e) {
                        return {
                            success: false,
                            error: e.toString()
                        };
                    }
                }
                static makeCurrent(instance) {
                    instance.anyGL.makeContextCurrent(instance.glCtx);
                }
            }
            Runtime.WebGlBrowserRenderer = WebGlBrowserRenderer;
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Microsoft;
(function (Microsoft) {
    var UI;
    (function (UI) {
        var Xaml;
        (function (Xaml) {
            var Controls;
            (function (Controls) {
                class WebView {
                    static buildImports(assembly) {
                        if (!WebView.unoExports) {
                            window.Module.getAssemblyExports(assembly)
                                .then((e) => {
                                WebView.unoExports = e.Microsoft.UI.Xaml.Controls.NativeWebView;
                            });
                        }
                    }
                    static reload(htmlId) {
                        document.getElementById(htmlId).contentWindow.location.reload();
                    }
                    static stop(htmlId) {
                        document.getElementById(htmlId).contentWindow.stop();
                    }
                    static goBack(htmlId) {
                        document.getElementById(htmlId).contentWindow.history.back();
                    }
                    static goForward(htmlId) {
                        document.getElementById(htmlId).contentWindow.history.forward();
                    }
                    static executeScript(htmlId, script) {
                        return document.getElementById(htmlId).contentWindow.eval(script);
                    }
                    static getDocumentTitle(htmlId) {
                        return document.getElementById(htmlId).contentDocument.title;
                    }
                    static setAttribute(htmlId, name, value) {
                        document.getElementById(htmlId).setAttribute(name, value);
                    }
                    static getAttribute(htmlId, name) {
                        return document.getElementById(htmlId).getAttribute(name);
                    }
                    static navigate(htmlId, url) {
                        const iframe = document.getElementById(htmlId);
                        if (iframe) {
                            try {
                                if (iframe.contentWindow) {
                                    iframe.contentWindow.location.href = url;
                                }
                            }
                            catch (e) {
                                // Fall back to setAttribute if contentWindow access fails (cross-origin)
                                iframe.setAttribute("src", url);
                            }
                        }
                    }
                    static initializeStyling(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.style.backgroundColor = "transparent";
                        iframe.style.border = "0";
                    }
                    static getPackageBase() {
                        if (WebView.cachedPackageBase !== null) {
                            return WebView.cachedPackageBase;
                        }
                        const pathsToCheck = [
                            ...Array.from(document.getElementsByTagName('script')).map(s => s.src),
                        ];
                        for (const path of pathsToCheck) {
                            const m = path === null || path === void 0 ? void 0 : path.match(/\/package_[^\/]+/);
                            if (m) {
                                const packageBase = "./" + m[0].substring(1);
                                WebView.cachedPackageBase = packageBase;
                                return packageBase;
                            }
                        }
                        WebView.cachedPackageBase = ".";
                        return ".";
                    }
                    static setupEvents(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.addEventListener('load', WebView.onLoad);
                    }
                    static cleanupEvents(htmlId) {
                        const iframe = document.getElementById(htmlId);
                        iframe.removeEventListener('load', WebView.onLoad);
                    }
                    static onLoad(event) {
                        const iframe = event.currentTarget;
                        const absoluteUrl = iframe.contentWindow.location.href;
                        WebView.unoExports.DispatchLoadEvent(iframe.id, absoluteUrl);
                        try {
                            if (iframe.contentWindow && WebView.unoExports.DispatchNewWindowRequested) {
                                const unoExports = WebView.unoExports;
                                if (!iframe.contentWindow.__unoOpenOverridden) {
                                    if (!iframe.contentWindow.__unoOriginalOpen) {
                                        iframe.contentWindow.__unoOriginalOpen = iframe.contentWindow.open;
                                    }
                                    iframe.contentWindow.open = function (url, target, features) {
                                        const referer = iframe.contentWindow.location.href;
                                        const handled = unoExports.DispatchNewWindowRequested(iframe.id, url || '', referer);
                                        if (!handled) {
                                            return iframe.contentWindow.__unoOriginalOpen.call(this, url, target, features);
                                        }
                                        return null;
                                    };
                                    iframe.contentWindow.__unoOpenOverridden = true;
                                }
                                iframe.contentDocument.addEventListener('click', (e) => {
                                    const target = e.target;
                                    const link = target.closest('a[target="_blank"]');
                                    if (link) {
                                        const targetUrl = link.href;
                                        const referer = iframe.contentWindow.location.href;
                                        const handled = unoExports.DispatchNewWindowRequested(iframe.id, targetUrl, referer);
                                        if (handled) {
                                            e.preventDefault();
                                            e.stopPropagation();
                                        }
                                    }
                                });
                            }
                        }
                        catch (e) {
                            // This can fail if the iframe content is cross-origin.
                            // We log this as a warning, as it's a known browser security feature.
                            // https://developer.mozilla.org/en-US/docs/Web/Security/Same-origin_policy
                            console.warn("Uno.WebView: Could not attach NewWindowRequested handlers. This is expected if the iframe content is cross-origin.", e);
                        }
                    }
                }
                WebView.cachedPackageBase = null;
                Controls.WebView = WebView;
            })(Controls = Xaml.Controls || (Xaml.Controls = {}));
        })(Xaml = UI.Xaml || (UI.Xaml = {}));
    })(UI = Microsoft.UI || (Microsoft.UI = {}));
})(Microsoft || (Microsoft = {}));
