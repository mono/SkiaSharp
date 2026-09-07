var __awaiter = (this && this.__awaiter) || function (thisArg, _arguments, P, generator) {
    function adopt(value) { return value instanceof P ? value : new P(function (resolve) { resolve(value); }); }
    return new (P || (P = Promise))(function (resolve, reject) {
        function fulfilled(value) { try { step(generator.next(value)); } catch (e) { reject(e); } }
        function rejected(value) { try { step(generator["throw"](value)); } catch (e) { reject(e); } }
        function step(result) { result.done ? resolve(result.value) : adopt(result.value).then(fulfilled, rejected); }
        step((generator = generator.apply(thisArg, _arguments || [])).next());
    });
};
//}
//}
class ParentAccessor {
    constructor(managedOwner) {
        this._managedOwner = managedOwner;
    }
    static setup() {
        return __awaiter(this, void 0, void 0, function* () {
            let anyModule = window.Module;
            if (anyModule.getAssemblyExports !== undefined) {
                const browserExports = yield anyModule.getAssemblyExports("MonacoEditorComponent");
                ParentAccessor._managedGetJsonValue = browserExports.Monaco.Helpers.ParentAccessor.ManagedGetJsonValue;
                ParentAccessor._managedCallAction = browserExports.Monaco.Helpers.ParentAccessor.ManagedCallAction;
                ParentAccessor._managedCallActionWithParameters = browserExports.Monaco.Helpers.ParentAccessor.ManagedCallActionWithParameters;
                ParentAccessor._managedCallEvent = browserExports.Monaco.Helpers.ParentAccessor.ManagedCallEvent;
                ParentAccessor._managedClose = browserExports.Monaco.Helpers.ParentAccessor.ManagedClose;
                ParentAccessor._managedSetValue = browserExports.Monaco.Helpers.ParentAccessor.ManagedSetValue;
                ParentAccessor._managedSetValueWithType = browserExports.Monaco.Helpers.ParentAccessor.ManagedSetValueWithType;
            }
        });
    }
    getJsonValue(name) {
        return ParentAccessor._managedGetJsonValue(this._managedOwner, name);
    }
    callAction(name) {
        return ParentAccessor._managedCallAction(this._managedOwner, name);
    }
    callActionWithParameters(name, parameter1, parameter2) {
        return ParentAccessor._managedCallActionWithParameters(this._managedOwner, name, [parameter1, parameter2]);
    }
    callActionWithParameters2(name, parameters) {
        return ParentAccessor._managedCallActionWithParameters(this._managedOwner, name, parameters);
    }
    close() {
        ParentAccessor._managedClose(this._managedOwner);
    }
    //getChildValue(name: string, child: string): Promise<any>;
    //getJsonValue(name: string): Promise<string>;
    //getValue(name: string): Promise<any>;
    setValue(name, value) {
        return __awaiter(this, void 0, void 0, function* () {
            ParentAccessor._managedSetValue(this._managedOwner, name, value);
        });
    }
    setValueWithType(name, value, type) {
        ParentAccessor._managedSetValueWithType(this._managedOwner, name, value, type);
    }
    //callActionWithParameters(name: string, parameter1: string, parameter2: string): boolean;
    callEvent(name, parameter1, parameter2) {
        return ParentAccessor._managedCallEvent(this._managedOwner, name, [parameter1, parameter2]);
    }
}
////namespace Monaco.Helpers {
//    interface ParentAccessor {
//        callAction(name: string): boolean;
//        callActionWithParameters(name: string, parameters: string[]): boolean;
//        callEvent(name: string, parameters: string[]): Promise<string>
//        close();
//        getChildValue(name: string, child: string): Promise<any>;
//        getJsonValue(name: string): Promise<string>;
//        getValue(name: string): Promise<any>;
//        setValue(name: string, value: any): Promise<undefined>;
//        setValue(name: string, value: string, type: string): Promise<undefined>;
//        setValueWithType(name: string, value: string, type: string);
//        callActionWithParameters(name: string, parameter1: string, parameter2: string): boolean;
//        callEvent(name: string, callbackMethod: string, parameter1: string, parameter2: string);
//        getJsonValue(name: string, returnId: string);
//}
////}
//}
const initializeMonacoEditor = (managedOwner, element) => {
    {
        //  console.debug("Grabbing Monaco Options");
        var opt = {};
    }
    ;
    //console.debug("Getting Host container");
    //console.debug("Creating Editor");
    const editor = monaco.editor.create(element, opt);
    var editorContext = EditorContext.registerEditorForElement(element, editor);
    editorContext.Debug = new DebugLogger(managedOwner);
    editorContext.Keyboard = new KeyboardListener(managedOwner);
    editorContext.Accessor = new ParentAccessor(managedOwner);
    editorContext.Theme = new ThemeListener(managedOwner);
    //console.debug("Getting Editor model");
    editorContext.model = editor.getModel();
    // Listen for Content Changes
    //console.debug("Listening for changes in the editor model - " + (!editorContext.model));
    editorContext.model.onDidChangeContent((event) => {
        {
            editorContext.Accessor.setValue("Text", stringifyForMarshalling(editorContext.model.getValue()));
        }
    });
    // Listen for Selection Changes
    //console.debug("Listening for changes in the editor selection");
    editor.onDidChangeCursorSelection((event) => {
        {
            if (!editorContext.modifingSelection) {
                {
                    editorContext.Accessor.setValue("SelectedText", stringifyForMarshalling(editorContext.model.getValueInRange(event.selection)));
                    editorContext.Accessor.setValueWithType("SelectedRange", stringifyForMarshalling(JSON.stringify(event.selection)), "Selection");
                }
            }
        }
    });
    // Set theme
    //console.debug("Getting parent theme value");
    let theme = getParentJsonValue(element, "RequestedTheme");
    theme = {
        "0": "Default",
        "1": "Light",
        "2": "Dark"
    }[theme];
    //console.debug("Current theme value - " + theme);
    if (theme == "Default") {
        {
            //        console.debug("Loading default theme");
            theme = getThemeCurrentThemeName(element);
        }
    }
    //  console.debug("Changing theme");
    changeTheme(element, theme, getThemeIsHighContrast(element));
    // Update Monaco Size when we receive a window resize event
    //    console.debug("Listen for resize events on the window and resize the editor");
    window.addEventListener("resize", () => {
        {
            editor.layout();
        }
    });
    // Disable WebView Scrollbar so Monaco Scrollbar can do heavy lifting
    document.body.style.overflow = 'hidden';
    // Callback to Parent that we're loaded
    //   console.debug("Loaded Monaco");
    editorContext.Accessor.callAction("Loaded");
    // console.debug("Ending Monaco Load");
};
class DebugLogger {
    constructor(managedOwner) {
        this._managedOwner = managedOwner;
    }
    static setup() {
        return __awaiter(this, void 0, void 0, function* () {
        });
    }
}
class KeyboardListener {
    constructor(managedOwner) {
        this._managedOwner = managedOwner;
    }
    static setup() {
        return __awaiter(this, void 0, void 0, function* () {
        });
    }
}
class ThemeListener {
    constructor(managedOwner) {
        this._managedOwner = managedOwner;
    }
    static setup() {
        return __awaiter(this, void 0, void 0, function* () {
            let anyModule = window.Module;
            if (anyModule.getAssemblyExports !== undefined) {
                const browserExports = yield anyModule.getAssemblyExports("MonacoEditorComponent");
                ThemeListener._managedGetCurrentThemeName = browserExports.Monaco.Helpers.ThemeListener.ManagedGetCurrentThemeName;
                ThemeListener._managedGetIsHighContrast = browserExports.Monaco.Helpers.ThemeListener.ManagedGetIsHighContrast;
            }
        });
    }
    getIsHighContrast() {
        return ThemeListener._managedGetIsHighContrast(this._managedOwner);
    }
    getCurrentThemeName() {
        return ThemeListener._managedGetCurrentThemeName(this._managedOwner);
    }
}
globalThis.createMonacoEditor = (managedOwner, elementId, basePath) => __awaiter(this, void 0, void 0, function* () {
    //  console.debug("Create dynamic style element");
    var head = document.head || document.getElementsByTagName('head')[0];
    var style = document.createElement('style');
    style.id = 'dynamic';
    head.appendChild(style);
    yield DebugLogger.setup();
    yield KeyboardListener.setup();
    yield ParentAccessor.setup();
    yield ThemeListener.setup();
    //    console.debug("Starting Monaco Load");
    window.require.config({ paths: { 'vs': `${basePath}/MonacoEditorComponent/monaco-editor/min/vs` } });
    window.require(['vs/editor/editor.main'], function () {
        initializeMonacoEditor(managedOwner, document.getElementById(elementId));
    });
});
const replaceAll = (str, find, rep) => {
    if (find == "\\") {
        find = "\\\\";
    }
    return (`${str}`).replace(new RegExp(find, "g"), rep);
};
const sanitize = (jsonString) => {
    if (jsonString == null) {
        //console.log('Sanitized is null');
        return null;
    }
    const replacements = "%&\\\"'{}:,";
    for (let i = 0; i < replacements.length; i++) {
        jsonString = replaceAll(jsonString, replacements.charAt(i), `%${replacements.charCodeAt(i)}`);
    }
    //console.log('Sanitized: ' + jsonString);
    return jsonString;
};
const desantize = (parameter) => {
    //System.Diagnostics.Debug.WriteLine($"Encoded String: {parameter}");
    if (parameter == null)
        return parameter;
    const replacements = "&\\\"'{}:,%";
    //System.Diagnostics.Debug.WriteLine($"Replacements: >{replacements}<");
    for (let i = 0; i < replacements.length; i++) {
        //console.log("Replacing: >%" + replacements.charCodeAt(i) + "< with >" + replacements.charAt(i) + "< ");
        parameter = replaceAll(parameter, "%" + replacements.charCodeAt(i), replacements.charAt(i));
    }
    //console.log("Decoded String: " + parameter );
    return parameter;
};
const stringifyForMarshalling = (value) => sanitize(value);
const getParentValue = (element, name) => {
    return EditorContext.getEditorForElement(element).Accessor.getJsonValue(name);
};
const getParentJsonValue = (element, name) => EditorContext.getEditorForElement(element).Accessor.getJsonValue(name);
const getThemeIsHighContrast = (element) => EditorContext.getEditorForElement(element).Theme.getIsHighContrast() == "true";
const getThemeCurrentThemeName = (element) => EditorContext.getEditorForElement(element).Theme.getCurrentThemeName();
const callParentEventAsync = (element, name, parameters) => __awaiter(this, void 0, void 0, function* () {
    let result = yield EditorContext.getEditorForElement(element).Accessor.callEvent(name, parameters != null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null, parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null);
    if (result) {
        result = desantize(result);
    }
    else {
        // console.debug('No Parent event result for ' + name);
    }
    return result;
});
const callParentActionWithParameters = (element, name, parameters) => EditorContext.getEditorForElement(element).Accessor.callActionWithParameters(name, parameters != null && parameters.length > 0 ? stringifyForMarshalling(parameters[0]) : null, parameters != null && parameters.length > 1 ? stringifyForMarshalling(parameters[1]) : null);
globalThis.InvokeJS = (elementId, command) => {
    var r = eval(`var element = globalThis.document.getElementById(\"${elementId}\"); ${command}`) || "";
    return JSON.stringify(r);
};
globalThis.refreshLayout = (elementId) => {
    EditorContext.getEditorForElement(document.getElementById(elementId)).editor.layout();
};
globalThis.languageIdFromExtension = (extension) => {
    if (extension != null) {
        const lower = extension.toLowerCase();
        const langs = monaco.languages.getLanguages();
        for (const l of langs) {
            if (!l.extensions)
                continue;
            if (l.extensions.some(ext => lower.endsWith(ext)))
                return l.id;
        }
    }
    return 'plaintext';
};
///<reference path="../monaco-editor/monaco.d.ts" />
class EditorContext {
    static registerEditorForElement(element, editor) {
        var value = EditorContext.getEditorForElement(element);
        value.editor = editor;
        return value;
    }
    static getEditorForElement(element) {
        var context = EditorContext._editors.get(element);
        if (!context) {
            context = new EditorContext();
            EditorContext._editors.set(element, context);
        }
        return context;
    }
    static getElementFromModel(model) {
        for (let [key, value] of EditorContext._editors) {
            if (value.model === model) {
                return key;
            }
        }
        return null;
    }
    constructor() {
        this.modifingSelection = false;
        this.contexts = {};
        this.decorations = [];
    }
}
EditorContext._editors = new Map();
const registerHoverProvider = function (unused, languageId) {
    return monaco.languages.registerHoverProvider(languageId, {
        provideHover: function (model, position) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "HoverProvider" + languageId, [JSON.stringify(position)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
};
const addAction = function (element, action) {
    var editorContext = EditorContext.getEditorForElement(element);
    action.run = function (ed) {
        editorContext.Accessor.callAction("Action" + action.id);
    };
    editorContext.editor.addAction(action);
};
const addCommand = function (element, keybindingStr, handlerName, context) {
    var editorContext = EditorContext.getEditorForElement(element);
    return editorContext.editor.addCommand(parseInt(keybindingStr), function () {
        const objs = [];
        if (arguments) { // Use arguments as Monaco will pass each as it's own parameter, so we don't know how many that may be.
            for (let i = 1; i < arguments.length; i++) { // Skip first one as that's the sender?
                objs.push(JSON.stringify(arguments[i]));
            }
        }
        editorContext.Accessor.callActionWithParameters2(handlerName, objs);
    }, context);
};
const createContext = function (element, context) {
    var editorContext = EditorContext.getEditorForElement(element);
    if (context) {
        editorContext.contexts[context.key] = editorContext.editor.createContextKey(context.key, context.defaultValue);
    }
};
const updateContext = function (element, key, value) {
    var editorContext = EditorContext.getEditorForElement(element);
    editorContext.contexts[key].set(value);
};
// link:CodeEditor.Properties.cs:updateContent
const updateContent = function (element, content) {
    var editorContext = EditorContext.getEditorForElement(element);
    // Need to ignore updates from us notifying of a change
    if (content !== editorContext.model.getValue()) {
        editorContext.model.setValue(content);
    }
};
const updateDecorations = function (element, newHighlights) {
    var editorContext = EditorContext.getEditorForElement(element);
    if (newHighlights) {
        editorContext.decorations = editorContext.editor.deltaDecorations(editorContext.decorations, newHighlights);
    }
    else {
        editorContext.decorations = editorContext.editor.deltaDecorations(editorContext.decorations, []);
    }
};
const updateStyle = function (innerStyle) {
    var style = document.getElementById("dynamic");
    style.innerHTML = innerStyle;
};
const getOptions = function (element) {
    return __awaiter(this, void 0, void 0, function* () {
        var editorContext = EditorContext.getEditorForElement(element);
        let opt = null;
        try {
            opt = getParentValue(element, "Options");
        }
        finally {
        }
        if (opt !== null && typeof opt === "object") {
            return opt;
        }
        return {};
    });
};
const updateOptions = function (element, opt) {
    var editorContext = EditorContext.getEditorForElement(element);
    if (opt !== null && typeof opt === "object") {
        editorContext.editor.updateOptions(opt);
    }
};
const updateLanguage = function (element, language) {
    var editorContext = EditorContext.getEditorForElement(element);
    monaco.editor.setModelLanguage(editorContext.model, language);
};
const changeTheme = function (element, theme, highcontrast) {
    var editorContext = EditorContext.getEditorForElement(element);
    let newTheme = 'vs';
    if (highcontrast == "True" || highcontrast == "true") {
        newTheme = 'hc-black';
    }
    else if (theme == "Dark") {
        newTheme = 'vs-dark';
    }
    monaco.editor.setTheme(newTheme);
};
const keyDown = function (element, event) {
    return __awaiter(this, void 0, void 0, function* () {
        var editorContext = EditorContext.getEditorForElement(element);
        //Debug.log("Key Down:" + event.keyCode + " " + event.ctrlKey);
        const result = yield editorContext.Keyboard.keyDown(event.keyCode, event.ctrlKey, event.shiftKey, event.altKey, event.metaKey);
        if (result) {
            event.cancelBubble = true;
            event.preventDefault();
            event.stopPropagation();
            event.stopImmediatePropagation();
            return false;
        }
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
function isTextEdit(edit) {
    return edit.textEdit !== undefined;
}
const registerCodeActionProvider = function (unused, languageId) {
    return monaco.languages.registerCodeActionProvider(languageId, {
        provideCodeActions: function (model, range, context, token) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "ProvideCodeActions" + languageId, [JSON.stringify(range), JSON.stringify(context)]).then(result => {
                if (result) {
                    const list = JSON.parse(result);
                    // Need to add in the model.uri to any edits to connect the dots
                    if (list.actions &&
                        list.actions.length > 0) {
                        list.actions.forEach((action) => {
                            if (action.edit &&
                                action.edit.edits &&
                                action.edit.edits.length > 0) {
                                action.edit.edits.forEach((inneredit) => {
                                    if (isTextEdit(inneredit)) {
                                        inneredit.resource = model.uri;
                                    }
                                });
                            }
                        });
                    }
                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };
                    return list;
                }
            });
        },
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerCodeLensProvider = function (unused, languageId) {
    return monaco.languages.registerCodeLensProvider(languageId, {
        provideCodeLenses: function (model, token) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "ProvideCodeLenses" + languageId, []).then(result => {
                if (result) {
                    const list = JSON.parse(result);
                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };
                    return list;
                }
                return null;
            });
        },
        resolveCodeLens: function (model, codeLens, token) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "ResolveCodeLens" + languageId, [JSON.stringify(codeLens)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
                return null;
            });
        }
        // TODO: onDidChange, don't know what this does.
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerColorProvider = function (unused, languageId) {
    return monaco.languages.registerColorProvider(languageId, {
        provideColorPresentations: function (model, colorInfo, token) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "ProvideColorPresentations" + languageId, [JSON.stringify(colorInfo)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        },
        provideDocumentColors: function (model, token) {
            var element = EditorContext.getElementFromModel(model);
            return callParentEventAsync(element, "ProvideDocumentColors" + languageId, []).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
const registerCompletionItemProvider = function (element, languageId, characters) {
    var editorContext = EditorContext.getEditorForElement(element);
    return monaco.languages.registerCompletionItemProvider(languageId, {
        triggerCharacters: characters,
        provideCompletionItems: function (model, position, context, token) {
            return callParentEventAsync(element, "CompletionItemProvider" + languageId, [JSON.stringify(position), JSON.stringify(context)]).then(result => {
                if (result) {
                    const list = JSON.parse(result);
                    // Add dispose method for IDisposable that Monaco is looking for.
                    list.dispose = () => { };
                    return list;
                }
            });
        },
        resolveCompletionItem: function (item, token) {
            return callParentEventAsync(element, "CompletionItemRequested" + languageId, [JSON.stringify(item)]).then(result => {
                if (result) {
                    return JSON.parse(result);
                }
            });
        }
    });
};
///<reference path="../monaco-editor/monaco.d.ts" />
// link:CodeEditor.Properties.cs:updateSelectedContent
const updateSelectedContent = function (element, content) {
    var editorContext = EditorContext.getEditorForElement(element);
    let selection = editorContext.editor.getSelection();
    // Need to ignore updates from us notifying of a change
    if (content != editorContext.model.getValueInRange(selection)) {
        editorContext.modifingSelection = true;
        let range = new monaco.Range(selection.startLineNumber, selection.startColumn, selection.endLineNumber, selection.endColumn);
        let op = { identifier: { major: 1, minor: 1 }, range, text: content, forceMoveMarkers: true };
        // Make change to selection
        //TODO how to properly fix this code?
        //model.pushEditOperations([], [op]);
        editorContext.model.pushEditOperations([], [op], null);
        // Update selection to new text.
        const newEndLineNumber = selection.startLineNumber + content.split('\r').length - 1; // TODO: Not sure if line end is situational/platform specific... investigate more.
        const newEndColumn = (selection.startLineNumber === selection.endLineNumber)
            ? selection.startColumn + content.length
            : content.length - content.lastIndexOf('\r');
        selection = selection.setEndPosition(newEndLineNumber, newEndColumn);
        // Update other selection bound for direction.
        //TODO how to properly fix this code?
        selection = selection.setEndPosition(selection.endLineNumber, selection.endColumn);
        //if (selection.getDirection() == monaco.SelectionDirection.LTR) {
        //    selection.positionColumn = selection.endColumn;
        //    selection.positionLineNumber = selection.endLineNumber;
        //} else {
        //    selection.selectionStartColumn = selection.endColumn;
        //    selection.selectionStartLineNumber = selection.endLineNumber;
        //}
        editorContext.modifingSelection = false;
        editorContext.editor.setSelection(selection);
    }
};
