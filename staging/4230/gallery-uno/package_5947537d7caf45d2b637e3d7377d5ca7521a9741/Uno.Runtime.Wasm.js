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
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class Accessibility {
                    static createLiveElement(kind) {
                        const element = document.createElement("div");
                        element.classList.add("uno-aria-live");
                        element.setAttribute("aria-live", kind);
                        return element;
                    }
                    static setup() {
                        console.log('[A11y] Accessibility.setup() — initializing accessibility subsystem');
                        const browserExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports();
                        // Wire up managed callbacks from WebAssemblyAccessibility.cs
                        const accessibilityExports = browserExports.Uno.UI.Runtime.Skia.WebAssemblyAccessibility;
                        this.managedEnableAccessibility = accessibilityExports.EnableAccessibility;
                        this.managedIsAutoEnableAccessibility = accessibilityExports.IsAutoEnableAccessibility;
                        this.managedOnScroll = accessibilityExports.OnScroll;
                        this.managedOnInvoke = accessibilityExports.OnInvoke;
                        this.managedOnToggle = accessibilityExports.OnToggle;
                        this.managedOnRangeValueChange = accessibilityExports.OnRangeValueChange;
                        this.managedOnTextInput = accessibilityExports.OnTextInput;
                        this.managedOnExpandCollapse = accessibilityExports.OnExpandCollapse;
                        this.managedOnSelection = accessibilityExports.OnSelection;
                        this.managedOnFocus = accessibilityExports.OnFocus;
                        this.managedOnBlur = accessibilityExports.OnBlur;
                        this.containerElement = document.getElementById("uno-body");
                        // Create live regions for screen reader announcements
                        this.politeElement = Accessibility.createLiveElement("polite");
                        this.assertiveElement = Accessibility.createLiveElement("assertive");
                        this.containerElement.appendChild(this.politeElement);
                        this.containerElement.appendChild(this.assertiveElement);
                        const autoEnable = this.managedIsAutoEnableAccessibility();
                        if (!autoEnable) {
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
                            console.log('[A11y] Auto-enabling accessibility (FeatureConfiguration.AutomationPeer.AutoEnableAccessibility = true)');
                            this.managedEnableAccessibility();
                            Skia.LiveRegion.initialize();
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
                        if (isFocusable) {
                            element.tabIndex = 0;
                        }
                        else {
                            element.removeAttribute("tabIndex");
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
                        Skia.LiveRegion.initialize();
                        this.announceAssertive("Accessibility enabled successfully.");
                    }
                    /**
                     * Focuses a semantic element by handle.
                     * If the element isn't in the DOM yet (timing issue from batched mutations),
                     * retries once after a requestAnimationFrame. This handles the case where
                     * C# fires focus synchronously but the JS DOM mutation hasn't been flushed yet.
                     */
                    static focusSemanticElement(handle) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.focus();
                        }
                        else {
                            // Element might not be in DOM yet due to batched/deferred mutations.
                            // Retry once after the next animation frame.
                            requestAnimationFrame(() => {
                                const retryElement = Accessibility.getSemanticElementByHandle(handle);
                                if (retryElement) {
                                    retryElement.focus();
                                }
                                else {
                                    console.warn(`[A11y] TS focusSemanticElement: element NOT FOUND handle=${handle} (after retry)`);
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
                        // Set the active element to tabindex 0
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
                        console.debug(`[A11y] addRootElementToSemanticsRoot: handle=${rootHandle} size=${width}x${height} pos=(${x},${y}) focusable=${isFocusable}`);
                        let element = Accessibility.createSemanticElement(x, y, width, height, rootHandle, isFocusable);
                        this.semanticsRoot.appendChild(element);
                    }
                    static addSemanticElement(parentHandle, handle, index, width, height, x, y, role, automationId, isFocusable, ariaChecked, isVisible, horizontallyScrollable, verticallyScrollable, temporary) {
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
                            console.warn(`[A11y] addSemanticElement: PARENT NOT FOUND — handle=${handle} parentHandle=${parentHandle} controlType='${temporary}' role='${role}' label='${automationId}'. Falling back to semanticsRoot.`);
                            parent = this.semanticsRoot;
                            if (!parent) {
                                console.warn(`[A11y] addSemanticElement: semanticsRoot also null. Element will NOT appear in semantic tree.`);
                                return false;
                            }
                        }
                        console.debug(`[A11y] addSemanticElement: handle=${handle} parentHandle=${parentHandle} controlType='${temporary}' role='${role}' label='${automationId}' size=${width}x${height} pos=(${x},${y}) focusable=${isFocusable} visible=${isVisible}`);
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
                        if (automationId) {
                            element.setAttribute("aria-label", automationId);
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
                            console.warn(`[A11y] removeSemanticElement: child handle=${childHandle} not found in DOM (parent=${parentHandle})`);
                            return;
                        }
                        console.debug(`[A11y] removeSemanticElement: parent=${parentHandle} child=${childHandle}`);
                        // Use child.remove() instead of parent.removeChild(child) to handle
                        // cases where the child's actual DOM parent differs from the semantic parent
                        // (e.g., after re-parenting or when duplicate IDs existed previously).
                        child.remove();
                    }
                    static updateIsFocusable(handle, isFocusable) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            console.log(`[A11y] TS updateIsFocusable: handle=${handle} focusable=${isFocusable}`);
                            Accessibility.updateElementFocusability(element, isFocusable);
                        }
                        // Silently skip if element doesn't exist in the semantic DOM.
                        // Many controls get IsFocusable updates but aren't in the semantic
                        // tree (pruned as non-semantic). This is expected.
                    }
                    static updateAriaLabel(handle, automationId) {
                        console.log(`[A11y] TS updateAriaLabel: handle=${handle} label='${automationId}'`);
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-label", automationId);
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
                            element.setAttribute("role", role);
                        }
                    }
                    /**
                     * Updates aria-roledescription on a semantic element.
                     * Provides a human-readable description of the role for VoiceOver.
                     */
                    static updateAriaRoleDescription(handle, roleDescription) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-roledescription", roleDescription);
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
                     * Updates aria-pressed on a toggle button semantic element.
                     */
                    static updateAriaPressed(handle, pressed) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-pressed", pressed);
                        }
                    }
                    /**
                     * Updates aria-live on a semantic element for live region announcements.
                     * Screen readers monitor elements with aria-live for content changes.
                     */
                    static updateAriaLive(handle, ariaLive) {
                        const element = Accessibility.getSemanticElementByHandle(handle);
                        if (element) {
                            element.setAttribute("aria-live", ariaLive);
                            element.setAttribute("aria-atomic", "true");
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
                        console.log(`[A11y] TS updateAriaChecked: handle=${handle} checked=${ariaChecked}`);
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
                        console.log(`[A11y] TS hideSemanticElement: handle=${handle}`);
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
                Accessibility.debugOverlayElement = null;
                Skia.Accessibility = Accessibility;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
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
                            BrowserDragDropExtension._dispatchDropEventMethod = (await window.Module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser")).Uno.UI.Runtime.Skia.BrowserDragDropExtension.OnNativeDropEvent;
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
            var Skia;
            (function (Skia) {
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
                Skia.BrowserInputHelper = BrowserInputHelper;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                var _a;
                class BrowserInvisibleTextBoxViewExtension {
                    static initialize() {
                        if (BrowserInvisibleTextBoxViewExtension._exports == undefined) {
                            const browserExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports();
                            BrowserInvisibleTextBoxViewExtension._exports = browserExports.Uno.UI.Runtime.Skia.BrowserInvisibleTextBoxViewExtension;
                            BrowserInvisibleTextBoxViewExtension._imeExports = browserExports.Uno.UI.Runtime.Skia.WasmImeTextBoxExtension;
                            document.onselectionchange = () => {
                                let input = document.activeElement;
                                if (input instanceof HTMLInputElement) {
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
                    static createInput(isPasswordBox, text, acceptsReturn, inputMode, enterKeyHint) {
                        BrowserInvisibleTextBoxViewExtension.acceptsReturn = acceptsReturn;
                        const input = document.createElement(acceptsReturn && !isPasswordBox ? "textarea" : "input");
                        if (isPasswordBox) {
                            input.type = "password";
                            input.autocomplete = "password";
                        }
                        input.id = BrowserInvisibleTextBoxViewExtension.inputElementId;
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
                        input.style.top = "0px";
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
                            BrowserInvisibleTextBoxViewExtension._exports.OnNativePaste(ev.clipboardData.getData("text"));
                            ev.preventDefault();
                        };
                        // Handle Enter key from Android virtual keyboards which don't fire keydown events.
                        // Android keyboards typically fire beforeinput with inputType "insertLineBreak" or "insertParagraph" instead.
                        input.addEventListener("beforeinput", (ev) => {
                            if ((ev.inputType === "insertLineBreak" || ev.inputType === "insertParagraph") && !BrowserInvisibleTextBoxViewExtension.acceptsReturn) {
                                ev.preventDefault();
                                BrowserInvisibleTextBoxViewExtension._exports.OnEnterKeyPressed();
                            }
                        });
                        BrowserInvisibleTextBoxViewExtension.attachTextInputKeyHandlers(input, acceptsReturn);
                        input.addEventListener("compositionstart", () => {
                            BrowserInvisibleTextBoxViewExtension.isComposing = true;
                            BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionStarted();
                        });
                        input.addEventListener("compositionupdate", (ev) => {
                            // Use input.selectionStart for cursor position when available,
                            // as the IME may place the caret within the preedit string.
                            const selectionStart = input.selectionStart;
                            const cursorPosition = selectionStart === null
                                ? ev.data.length
                                : Math.max(0, Math.min(selectionStart, ev.data.length));
                            BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionUpdated(ev.data, cursorPosition);
                        });
                        input.addEventListener("compositionend", (ev) => {
                            BrowserInvisibleTextBoxViewExtension.isComposing = false;
                            // The browser fires an input event after compositionend with the committed text.
                            // Suppress it to avoid double-inserting — the commit is handled by OnCompositionCompleted.
                            BrowserInvisibleTextBoxViewExtension.suppressNextInput = true;
                            if (ev.data.length > 0) {
                                BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionCompleted(ev.data);
                            }
                            else {
                                BrowserInvisibleTextBoxViewExtension._imeExports.OnCompositionEnded();
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
                            // This enables focus navigation (e.g., Uno.Toolkit's AutoFocusNext) on mobile browsers
                            if ((ev.key === "Enter" || ev.keyCode === 13) && !acceptsReturn) {
                                // Don't call preventDefault() to allow the key event to propagate to document listeners
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
                    static focus(handle, isPassword, text, acceptsReturn, inputMode, enterKeyHint) {
                        var _a;
                        const semanticElement = document.getElementById(`uno-semantics-${handle}`);
                        if (semanticElement && document.activeElement === semanticElement) {
                            BrowserInvisibleTextBoxViewExtension.detach();
                            return false;
                        }
                        // NOTE: We can get focused as true while we have inputElement.
                        // This happens when TextBox is focused twice with different FocusStates (e.g, Pointer, Programmatic, Keyboard)
                        // For such case, we do call StartEntry twice without any EndEntry in between.
                        // So, cleanup the existing inputElement and create a new one.
                        (_a = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _a === void 0 ? void 0 : _a.remove();
                        this.createInput(isPassword, text, acceptsReturn, inputMode, enterKeyHint);
                        // It's necessary to actually focus the native input, not just make it visible. This is particularly
                        // important to mobile browsers (to open the software keyboard) and for assistive technology to not steal
                        // events and properly recognize password inputs to not read it.
                        BrowserInvisibleTextBoxViewExtension.inputElement.focus();
                        return true;
                    }
                    static blur() {
                        var _a;
                        // reset focus
                        (_a = document.activeElement) === null || _a === void 0 ? void 0 : _a.blur();
                        BrowserInvisibleTextBoxViewExtension.detach();
                    }
                    static detach() {
                        var _a;
                        (_a = BrowserInvisibleTextBoxViewExtension.inputElement) === null || _a === void 0 ? void 0 : _a.remove();
                        BrowserInvisibleTextBoxViewExtension.inputElement = null;
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
                    static updateSize(width, height) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input != null) {
                            input.style.width = `${width}`;
                            input.style.height = `${height}`;
                        }
                    }
                    static updatePosition(x, y) {
                        const input = BrowserInvisibleTextBoxViewExtension.inputElement;
                        if (input != null) {
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
                BrowserInvisibleTextBoxViewExtension.inputElementId = "uno-input";
                BrowserInvisibleTextBoxViewExtension.isMacOS = (_a = navigator === null || navigator === void 0 ? void 0 : navigator.platform.toUpperCase().includes('MAC')) !== null && _a !== void 0 ? _a : false;
                // Android soft keyboards report all key events with keyCode 229 ("Unidentified").
                // Text changes are synced via the oninput handler instead.
                BrowserInvisibleTextBoxViewExtension.ANDROID_IME_KEYCODE = 229;
                Skia.BrowserInvisibleTextBoxViewExtension = BrowserInvisibleTextBoxViewExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserKeyboardInputSource {
                    static initialize(inputSource) {
                        if (BrowserKeyboardInputSource._exports == undefined) {
                            const browserExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports();
                            BrowserKeyboardInputSource._exports = browserExports.Uno.UI.Runtime.Skia.BrowserKeyboardInputSource;
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
                        if (evt.key === "Tab" && Skia.Accessibility.isEnableAccessibilityButtonActive()) {
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
                        if (result == Skia.HtmlEventDispatchResult.PreventDefault) {
                            evt.preventDefault();
                        }
                    }
                }
                Skia.BrowserKeyboardInputSource = BrowserKeyboardInputSource;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserMediaPlayerExtension {
                    static buildImports() {
                        BrowserMediaPlayerExtension.unoExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports().Uno.UI.Runtime.Skia.BrowserMediaPlayerExtension;
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
                Skia.BrowserMediaPlayerExtension = BrowserMediaPlayerExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserMediaPlayerPresenterExtension {
                    static buildImports() {
                        BrowserMediaPlayerPresenterExtension.unoExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports().Uno.UI.Runtime.Skia.BrowserMediaPlayerPresenterExtension;
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
                Skia.BrowserMediaPlayerPresenterExtension = BrowserMediaPlayerPresenterExtension;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
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
            var Skia;
            (function (Skia) {
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
                })(HtmlPointerEvent = Skia.HtmlPointerEvent || (Skia.HtmlPointerEvent = {}));
                // TODO: Duplicate of Uno.UI.HtmlEventDispatchResult to merge!
                let HtmlEventDispatchResult;
                (function (HtmlEventDispatchResult) {
                    HtmlEventDispatchResult[HtmlEventDispatchResult["Ok"] = 0] = "Ok";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["StopPropagation"] = 1] = "StopPropagation";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["PreventDefault"] = 2] = "PreventDefault";
                    HtmlEventDispatchResult[HtmlEventDispatchResult["NotDispatched"] = 128] = "NotDispatched";
                })(HtmlEventDispatchResult = Skia.HtmlEventDispatchResult || (Skia.HtmlEventDispatchResult = {}));
                // TODO: Duplicate of Windows.Devices.Input.PointerDeviceType to import instead of duplicate!
                let PointerDeviceType;
                (function (PointerDeviceType) {
                    PointerDeviceType[PointerDeviceType["Touch"] = 0] = "Touch";
                    PointerDeviceType[PointerDeviceType["Pen"] = 1] = "Pen";
                    PointerDeviceType[PointerDeviceType["Mouse"] = 2] = "Mouse";
                })(PointerDeviceType = Skia.PointerDeviceType || (Skia.PointerDeviceType = {}));
                class BrowserPointerInputSource {
                    constructor(manageSource) {
                        // Cached reference to #uno-native-element-host. Refreshed if detached/replaced.
                        this._nativeElementHost = null;
                        this._bootTime = Date.now() - performance.now();
                        this._source = manageSource;
                        BrowserPointerInputSource._exports.OnInitialized(manageSource, this._bootTime);
                        this.subscribePointerEvents(); // Subscribe only after the managed initialization is done
                    }
                    static async initialize(inputSource) {
                        if (BrowserPointerInputSource._exports == undefined) {
                            const browserExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports();
                            BrowserPointerInputSource._exports = browserExports.Uno.UI.Runtime.Skia.BrowserPointerInputSource;
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
                        element.addEventListener("pointermove", this.onPointerEventReceived.bind(this), { capture: true });
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
                    onPointerEventReceived(evt) {
                        var _a;
                        let id = (_a = evt.target) === null || _a === void 0 ? void 0 : _a.id;
                        if (id === "uno-enable-accessibility") {
                            // We have a div to enable accessibility (see enableA11y in WebAssemblyWindowWrapper).
                            // Pressing space on keyboard to click it will trigger pointer event which we want to ignore.
                            return;
                        }
                        if (this.isEventFromNativeElementHost(evt.target)) {
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
                        const isZooming = Skia.BrowserInputHelper.isBrowserZoomEnabled && evt instanceof WheelEvent && evt.ctrlKey;
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
                //#region WheelLineSize
                BrowserPointerInputSource._wheelLineSize = undefined;
                Skia.BrowserPointerInputSource = BrowserPointerInputSource;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class BrowserRenderer {
                    constructor(managedHandle, canvas) {
                        this.canvas = canvas;
                        this.managedHandle = managedHandle;
                        const skiaSharpExports = Skia.WebAssemblyWindowWrapper.getAssemblyExports();
                        this.requestRender = () => skiaSharpExports.Uno.UI.Runtime.Skia.BrowserRenderer.RenderFrame(this.managedHandle);
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
                Skia.BrowserRenderer = BrowserRenderer;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
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
                Skia.FocusTrap = FocusTrap;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class ImageLoader {
                    static loadFromArray(array) {
                        const seqNo = ++ImageLoader.sequenceNumber;
                        return new Promise((resolve) => {
                            ImageLoader.pendingPromiseResolvers.set(seqNo, resolve);
                            if (ImageLoader.availableWorkers.length == 0) {
                                ImageLoader.pendingJobs.push(worker => worker.postMessage([array, seqNo], [array.buffer]));
                            }
                            else {
                                const worker = ImageLoader.availableWorkers.pop();
                                worker.postMessage([array, seqNo], [array.buffer]);
                            }
                        });
                    }
                }
                ImageLoader.workerScriptUrl = URL.createObjectURL(new Blob([`
const offscreenCanvas = new OffscreenCanvas(0, 0);
const context = offscreenCanvas.getContext("2d", { willReadFrequently: true });
onmessage = async (e) => {
	const [array, seqNo] = e.data;
	const image = new Blob([array]);
	try {
		const imageBitmap = await createImageBitmap(image, { premultiplyAlpha: "premultiply" });
		offscreenCanvas.width = imageBitmap.width;
		offscreenCanvas.height = imageBitmap.height;
		context.drawImage(imageBitmap, 0, 0);
		const imageData = context.getImageData(
			0, 0,
			imageBitmap.width,
			imageBitmap.height
		);
		
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
				width: imageBitmap.width,
				height: imageBitmap.height
			}
		},
		[imageData.data.buffer]);
	} catch (e) {
		postMessage({seqNo: seqNo, response: { error: e.toString() }});
	}
};`], { type: 'application/javascript' }));
                ImageLoader.availableWorkers = Array.from({ length: navigator.hardwareConcurrency }).map((_, i) => {
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
                });
                ImageLoader.pendingJobs = new Array();
                ImageLoader.sequenceNumber = 0;
                ImageLoader.pendingPromiseResolvers = new Map();
                Skia.ImageLoader = ImageLoader;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
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
                Skia.LiveRegion = LiveRegion;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
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
            var Skia;
            (function (Skia) {
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
                        return Skia.Accessibility.getCallbacks();
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
                            console.warn(`[A11y] TS appendToParent: parent NOT FOUND handle=${parentHandle} for element=${element.id} — falling back to semanticsRoot`);
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
                        if (Skia.Accessibility.isDebugModeEnabled()) {
                            element.style.outline = "2px solid rgba(0, 255, 0, 0.7)";
                            element.style.backgroundColor = "rgba(0, 255, 0, 0.1)";
                        }
                    }
                    /**
                     * Creates a button semantic element and appends it to the semantics root.
                     * Called from C# via JSImport.
                     */
                    static createButtonElement(parentHandle, handle, index, x, y, width, height, label, disabled) {
                        const element = document.createElement('button');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus and interaction
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createToggleButtonElement(parentHandle, handle, index, x, y, width, height, label, pressed, disabled) {
                        const element = document.createElement('button');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        // aria-pressed for toggle button pattern (distinct from aria-checked for checkboxes)
                        element.setAttribute('aria-pressed', pressed);
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createSwitchElement(parentHandle, handle, index, x, y, width, height, label, isOn, disabled) {
                        const element = document.createElement('button');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        // role="switch" with aria-checked for ToggleSwitch (ARIA switch pattern)
                        element.setAttribute('role', 'switch');
                        element.setAttribute('aria-checked', isOn);
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createSliderElement(parentHandle, handle, index, x, y, width, height, value, min, max, step, orientation, valueText) {
                        const element = document.createElement('input');
                        element.type = 'range';
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus and interaction
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
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
                        if (valueText) {
                            element.setAttribute('aria-valuetext', valueText);
                        }
                        if (orientation === 'vertical') {
                            // Some browsers support orient attribute, others need CSS
                            element.setAttribute('orient', 'vertical');
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
                    static createCheckboxElement(parentHandle, handle, index, x, y, width, height, checkedState, label) {
                        const element = document.createElement('input');
                        element.type = 'checkbox';
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus and interaction
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createRadioElement(parentHandle, handle, index, x, y, width, height, checked, label, groupName) {
                        const element = document.createElement('input');
                        element.type = 'radio';
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus and interaction
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        if (groupName) {
                            element.name = groupName;
                        }
                        element.checked = checked;
                        const callbacks = this.getCallbacks();
                        // Change event handler for toggle events
                        element.addEventListener('change', () => {
                            if (callbacks.onToggle) {
                                callbacks.onToggle(handle);
                            }
                        });
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a heading semantic element (h1-h6).
                     * VoiceOver uses headings for rotor navigation (VO+U → Headings).
                     * Called from C# via JSImport.
                     */
                    static createHeadingElement(parentHandle, handle, index, x, y, width, height, level, label) {
                        // Clamp heading level to valid h1-h6 range
                        const clampedLevel = Math.max(1, Math.min(6, level));
                        const element = document.createElement(`h${clampedLevel}`);
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus for screen readers
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        // Reset default heading styles so they don't affect layout
                        element.style.margin = '0';
                        element.style.padding = '0';
                        element.style.fontSize = 'inherit';
                        element.style.fontWeight = 'inherit';
                        if (label) {
                            element.setAttribute('aria-label', label);
                            element.textContent = label;
                        }
                        element.setAttribute('aria-level', String(clampedLevel));
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a text input semantic element.
                     * Called from C# via JSImport.
                     */
                    static createTextBoxElement(parentHandle, handle, index, x, y, width, height, value, multiline, password, isReadOnly, selectionStart, selectionEnd) {
                        let element;
                        if (multiline) {
                            element = document.createElement('textarea');
                        }
                        else {
                            element = document.createElement('input');
                            element.type = password ? 'password' : 'text';
                        }
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        // Enable focus and interaction
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        element.value = value;
                        const maxLen = value.length;
                        const initialSelectionStart = Math.max(0, Math.min(selectionStart, maxLen));
                        const initialSelectionEnd = Math.max(initialSelectionStart, Math.min(selectionEnd, maxLen));
                        try {
                            element.setSelectionRange(initialSelectionStart, initialSelectionEnd);
                        }
                        catch (_a) {
                            // Some browsers/input types may reject selection updates before focus.
                        }
                        if (isReadOnly) {
                            element.readOnly = true;
                        }
                        const callbacks = this.getCallbacks();
                        // Block native character insertion so the managed TextBox KeyDown path is the single
                        // source of text edits. Without this, the key is inserted both by the browser (into
                        // this <input>) and by managed OnKeyDownSkia, producing duplicated input once a11y
                        // moves focus to the semantic element instead of the invisible TextBox <input>.
                        Skia.BrowserInvisibleTextBoxViewExtension.attachTextInputKeyHandlers(element, multiline);
                        // Input event handler for text changes (T050)
                        element.addEventListener('input', () => {
                            var _a, _b;
                            if (callbacks.onTextInput) {
                                callbacks.onTextInput(handle, element.value, (_a = element.selectionStart) !== null && _a !== void 0 ? _a : 0, (_b = element.selectionEnd) !== null && _b !== void 0 ? _b : 0);
                            }
                        });
                        // Handle IME composition events for international text input (T055)
                        element.addEventListener('compositionend', () => {
                            var _a, _b;
                            if (callbacks.onTextInput) {
                                callbacks.onTextInput(handle, element.value, (_a = element.selectionStart) !== null && _a !== void 0 ? _a : 0, (_b = element.selectionEnd) !== null && _b !== void 0 ? _b : 0);
                            }
                        });
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a combobox semantic element.
                     * Called from C# via JSImport.
                     */
                    static createComboBoxElement(parentHandle, handle, index, x, y, width, height, expanded, selectedValue) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'combobox');
                        element.setAttribute('aria-expanded', String(expanded));
                        element.setAttribute('aria-haspopup', 'listbox');
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (selectedValue) {
                            element.setAttribute('aria-label', selectedValue);
                        }
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
                    static createListBoxElement(parentHandle, handle, index, x, y, width, height, multiselect) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'listbox');
                        element.tabIndex = 0;
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
                    static createListItemElement(parentHandle, handle, index, x, y, width, height, selected, positionInSet, sizeOfSet) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'option');
                        element.setAttribute('aria-selected', String(selected));
                        element.setAttribute('aria-posinset', String(positionInSet));
                        element.setAttribute('aria-setsize', String(sizeOfSet));
                        element.tabIndex = -1; // Focusable but not in tab order (parent listbox manages focus)
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
                    static updateTextBoxValue(handle, value, selectionStart, selectionEnd) {
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
                                    element.setSelectionRange(start, end);
                                }
                                catch (_a) {
                                    // Some input types (e.g., password in some browsers) don't support setSelectionRange
                                }
                            }
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
                    static createLinkElement(parentHandle, handle, index, x, y, width, height, label) {
                        const element = document.createElement('a');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        // Native <a> has implicit role="link" — no need to set explicitly
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createTabListElement(parentHandle, handle, index, x, y, width, height, label) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'tablist');
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a tab element with selection and keyboard support.
                     */
                    static createTabElement(parentHandle, handle, index, x, y, width, height, label, selected, positionInSet, sizeOfSet) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'tab');
                        element.setAttribute('aria-selected', String(selected));
                        element.tabIndex = selected ? 0 : -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createTreeElement(parentHandle, handle, index, x, y, width, height, label, multiselectable) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'tree');
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        if (multiselectable) {
                            element.setAttribute('aria-multiselectable', 'true');
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a treeitem element with level, expanded state, and selection.
                     */
                    static createTreeItemElement(parentHandle, handle, index, x, y, width, height, label, level, expanded, selected, positionInSet, sizeOfSet) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'treeitem');
                        element.tabIndex = -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                    static createGridElement(parentHandle, handle, index, x, y, width, height, label, rowCount, colCount) {
                        const element = document.createElement('table');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'grid');
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        if (rowCount > 0) {
                            element.setAttribute('aria-rowcount', String(rowCount));
                        }
                        if (colCount > 0) {
                            element.setAttribute('aria-colcount', String(colCount));
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a grid row element.
                     */
                    static createGridRowElement(parentHandle, handle, index, x, y, width, height, rowIndex) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'row');
                        element.style.pointerEvents = 'none';
                        if (rowIndex > 0) {
                            element.setAttribute('aria-rowindex', String(rowIndex));
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a grid cell element.
                     */
                    static createGridCellElement(parentHandle, handle, index, x, y, width, height, label, rowIndex, colIndex) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'gridcell');
                        element.tabIndex = -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        if (rowIndex > 0) {
                            element.setAttribute('aria-rowindex', String(rowIndex));
                        }
                        if (colIndex > 0) {
                            element.setAttribute('aria-colindex', String(colIndex));
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a column header element.
                     */
                    static createColumnHeaderElement(parentHandle, handle, index, x, y, width, height, label, colIndex) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'columnheader');
                        element.tabIndex = -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        if (colIndex > 0) {
                            element.setAttribute('aria-colindex', String(colIndex));
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a menu container element.
                     */
                    static createMenuElement(parentHandle, handle, index, x, y, width, height, label) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'menu');
                        element.tabIndex = 0;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
                        this.appendToParent(element, parentHandle, index);
                    }
                    /**
                     * Creates a menuitem element.
                     */
                    static createMenuItemElement(parentHandle, handle, index, x, y, width, height, label, disabled, hasSubmenu) {
                        const element = document.createElement('div');
                        this.applyCommonStyles(element, x, y, width, height, handle);
                        element.setAttribute('role', 'menuitem');
                        element.tabIndex = -1;
                        element.style.pointerEvents = 'none';
                        if (label) {
                            element.setAttribute('aria-label', label);
                        }
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
                Skia.SemanticElements = SemanticElements;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
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
                Skia.SoftwareBrowserRenderer = SoftwareBrowserRenderer;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class WebAssemblyWindowWrapper {
                    constructor(owner) {
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
                        WebAssemblyWindowWrapper.assemblyExports = await window.Module.getAssemblyExports("Uno.UI.Runtime.Skia.WebAssembly.Browser");
                        this.onResize = WebAssemblyWindowWrapper.assemblyExports.Uno.UI.Runtime.Skia.WebAssemblyWindowWrapper.OnResize;
                        this.containerElement = document.getElementById("uno-body");
                        if (!this.containerElement) {
                            // If not found, we simply create a new one.
                            this.containerElement = document.createElement("div");
                            this.containerElement.id = "uno-root";
                            document.body.appendChild(this.containerElement);
                        }
                        this.canvasElement = document.createElement("canvas");
                        this.canvasElement.id = "uno-canvas";
                        this.canvasElement.setAttribute("aria-hidden", "true");
                        this.containerElement.appendChild(this.canvasElement);
                        Skia.Accessibility.setup();
                        window.addEventListener("resize", x => this.resize());
                        window.addEventListener("contextmenu", x => {
                            x.preventDefault();
                        });
                        this.resize();
                    }
                    static removeLoading() {
                        const element = document.getElementById(WebAssemblyWindowWrapper.loadingElementId);
                        if (element) {
                            element.parentElement.removeChild(element);
                        }
                        let bootstrapperLoaders = document.getElementsByClassName(WebAssemblyWindowWrapper.unoPersistentLoaderClassName);
                        if (bootstrapperLoaders.length > 0) {
                            let bootstrapperLoader = bootstrapperLoaders[0];
                            bootstrapperLoader.parentElement.removeChild(bootstrapperLoader);
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
                    resize() {
                        var rect = document.documentElement.getBoundingClientRect();
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
                Skia.WebAssemblyWindowWrapper = WebAssemblyWindowWrapper;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Runtime;
        (function (Runtime) {
            var Skia;
            (function (Skia) {
                class WebGlBrowserRenderer {
                    constructor(canvas) {
                        this.canvas = canvas;
                        this.anyGL = window.GL;
                        this.glCtx = WebGlBrowserRenderer.createWebGLContext(this.canvas, this.anyGL);
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
                    static createWebGLContext(canvas, anyGL) {
                        var contextAttributes = {
                            alpha: 1,
                            depth: 1,
                            stencil: 8,
                            antialias: 1,
                            premultipliedAlpha: 1,
                            preserveDrawingBuffer: 0,
                            preferLowPowerToHighPerformance: 0,
                            failIfMajorPerformanceCaveat: 0,
                            majorVersion: 2,
                            minorVersion: 0,
                            enableExtensionsByDefault: 1,
                            explicitSwapControl: 0,
                            renderViaOffscreenBackBuffer: 0,
                        };
                        var ctx = anyGL.createContext(canvas, contextAttributes);
                        if (!ctx && contextAttributes.majorVersion > 1) {
                            console.warn('Falling back to WebGL 1.0');
                            contextAttributes.majorVersion = 1;
                            contextAttributes.minorVersion = 0;
                            ctx = anyGL.createContext(canvas, contextAttributes);
                        }
                        return ctx;
                    }
                }
                Skia.WebGlBrowserRenderer = WebGlBrowserRenderer;
            })(Skia = Runtime.Skia || (Runtime.Skia = {}));
        })(Runtime = UI.Runtime || (UI.Runtime = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
