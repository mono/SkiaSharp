var ContactProperty;
(function (ContactProperty) {
    ContactProperty["Address"] = "address";
    ContactProperty["Email"] = "email";
    ContactProperty["Icon"] = "icon";
    ContactProperty["Name"] = "name";
    ContactProperty["Tel"] = "tel";
})(ContactProperty || (ContactProperty = {}));
;
var Windows;
(function (Windows) {
    var ApplicationModel;
    (function (ApplicationModel) {
        var Contacts;
        (function (Contacts) {
            class ContactPicker {
                static isSupported() {
                    return 'contacts' in navigator && 'ContactsManager' in window;
                }
                static async pickContacts(pickMultiple) {
                    const props = [ContactProperty.Name, ContactProperty.Email, ContactProperty.Tel, ContactProperty.Address];
                    const opts = {
                        multiple: pickMultiple
                    };
                    try {
                        const contacts = await navigator.contacts.select(props, opts);
                        return JSON.stringify(contacts);
                    }
                    catch (ex) {
                        console.log("Error occurred while picking contacts.");
                        return null;
                    }
                }
            }
            Contacts.ContactPicker = ContactPicker;
        })(Contacts = ApplicationModel.Contacts || (ApplicationModel.Contacts = {}));
    })(ApplicationModel = Windows.ApplicationModel || (Windows.ApplicationModel = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var ApplicationModel;
    (function (ApplicationModel) {
        var Core;
        (function (Core) {
            /**
             * Support file for the Windows.ApplicationModel.Core
             * */
            class CoreApplication {
                static initialize() {
                    // create a non-finishing promise
                    Uno.UI.Dispatching.NativeDispatcher.init(new Promise(resolve => window.setImmediate(async () => {
                        await CoreApplication.initializeExports();
                        CoreApplication._initializedExportsResolve(true);
                        resolve(true);
                    })));
                }
                /**
                 * Provides a promised that resolves when CoreApplication is initialized
                 */
                static async waitForInitialized() {
                    await CoreApplication._initializedExports;
                }
                static async initializeExports() {
                    if (Module.getAssemblyExports !== undefined) {
                        const unoExports = await Module.getAssemblyExports("Uno");
                        const unoUIDispatchingExports = await Module.getAssemblyExports("Uno.UI.Dispatching");
                        const runtimeWasmExports = await Module.getAssemblyExports("Uno.Foundation.Runtime.WebAssembly");
                        if (Object.entries(unoExports).length > 0) {
                            // DotnetExports may already have been initialized
                            globalThis.DotnetExports = globalThis.DotnetExports || {};
                            globalThis.DotnetExports.Uno = unoExports;
                            globalThis.DotnetExports.UnoUIDispatching = unoUIDispatchingExports;
                            globalThis.DotnetExports.UnoFoundationRuntimeWebAssembly = runtimeWasmExports;
                        }
                    }
                }
            }
            CoreApplication._initializedExports = new Promise((resolve) => { CoreApplication._initializedExportsResolve = resolve; });
            Core.CoreApplication = CoreApplication;
        })(Core = ApplicationModel.Core || (ApplicationModel.Core = {}));
    })(ApplicationModel = Windows.ApplicationModel || (Windows.ApplicationModel = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Utils;
    (function (Utils) {
        class Clipboard {
            static startContentChanged() {
                ['cut', 'copy', 'paste'].forEach(function (event) {
                    document.addEventListener(event, Clipboard.onClipboardChanged);
                });
            }
            static stopContentChanged() {
                ['cut', 'copy', 'paste'].forEach(function (event) {
                    document.removeEventListener(event, Clipboard.onClipboardChanged);
                });
            }
            static setText(text) {
                const nav = navigator;
                if (nav.clipboard) {
                    // Use clipboard object when available
                    nav.clipboard.writeText(text).catch((reason) => {
                        console.error(`Failed to write to clipboard: ${reason}`);
                    });
                    // Trigger change notification, as clipboard API does
                    // not execute "copy".
                    Clipboard.onClipboardChanged();
                }
                else {
                    // Hack when the clipboard is not available
                    const textarea = document.createElement("textarea");
                    textarea.value = text;
                    document.body.appendChild(textarea);
                    textarea.select();
                    document.execCommand("copy");
                    document.body.removeChild(textarea);
                }
                return "ok";
            }
            static getText() {
                // we return "" on failure instead of null to avoid crashing with an NRE if there
                // are no try blocks in the stack frames above.
                const nav = navigator;
                if (nav.clipboard) {
                    return nav.clipboard.readText().catch(reason => {
                        console.error(`Failed to read from clipboard: ${reason}`);
                        return "";
                    });
                }
                return Promise.resolve("");
            }
            static async getHtml() {
                // we return "" on failure instead of null to avoid crashing with an NRE if there
                // are no try blocks in the stack frames above.
                const nav = navigator;
                if (nav.clipboard && nav.clipboard.read) {
                    try {
                        const items = await nav.clipboard.read();
                        for (const item of items) {
                            if (item.types.includes('text/html')) {
                                const blob = await item.getType('text/html');
                                const html = await blob.text();
                                return html;
                            }
                        }
                        return "";
                    }
                    catch (reason) {
                        console.error(`Failed to read HTML from clipboard: ${reason}`);
                        return "";
                    }
                }
                return Promise.resolve("");
            }
            static async getImage() {
                // we return "" on failure instead of null to avoid crashing with an NRE if there
                // are no try blocks in the stack frames above.
                const nav = navigator;
                if (nav.clipboard && nav.clipboard.read) {
                    try {
                        const items = await nav.clipboard.read();
                        for (const item of items) {
                            const imageType = item.types.find(t => t.startsWith('image/'));
                            if (imageType) {
                                const blob = await item.getType(imageType);
                                const dataUrl = await new Promise((resolve, reject) => {
                                    const reader = new FileReader();
                                    reader.onload = () => {
                                        if (typeof reader.result === "string") {
                                            resolve(reader.result);
                                        }
                                        else {
                                            reject(new Error("Unexpected FileReader result type when reading image from clipboard."));
                                        }
                                    };
                                    reader.onerror = () => {
                                        var _a;
                                        reject((_a = reader.error) !== null && _a !== void 0 ? _a : new Error("Failed to read image from clipboard using FileReader."));
                                    };
                                    reader.readAsDataURL(blob);
                                });
                                const commaIndex = dataUrl.indexOf(",");
                                const base64 = commaIndex >= 0 ? dataUrl.substring(commaIndex + 1) : dataUrl;
                                return base64;
                            }
                        }
                        return "";
                    }
                    catch (reason) {
                        console.error(`Failed to read image from clipboard: ${reason}`);
                        return "";
                    }
                }
                return Promise.resolve("");
            }
            static async setImage(base64, mimeType) {
                const nav = navigator;
                if (nav.clipboard && nav.clipboard.write && typeof ClipboardItem !== 'undefined') {
                    try {
                        const bytes = Uint8Array.from(atob(base64), c => c.charCodeAt(0));
                        const blob = new Blob([bytes], { type: mimeType });
                        const item = new ClipboardItem({ [mimeType]: blob });
                        await nav.clipboard.write([item]);
                        Clipboard.onClipboardChanged();
                    }
                    catch (reason) {
                        console.error(`Failed to write image to clipboard: ${reason}`);
                    }
                }
            }
            static async setHtml(html, text) {
                const nav = navigator;
                if (nav.clipboard && nav.clipboard.write && typeof ClipboardItem !== 'undefined') {
                    try {
                        const htmlBlob = new Blob([html], { type: 'text/html' });
                        const textBlob = new Blob([text], { type: 'text/plain' });
                        const item = new ClipboardItem({
                            'text/html': htmlBlob,
                            'text/plain': textBlob
                        });
                        await nav.clipboard.write([item]);
                        // Trigger change notification
                        Clipboard.onClipboardChanged();
                    }
                    catch (reason) {
                        console.error(`Failed to write HTML to clipboard: ${reason}`);
                    }
                }
                else {
                    // Fallback: just write text if HTML clipboard API is not available
                    Clipboard.setText(text);
                }
            }
            static onClipboardChanged() {
                if (!Clipboard.dispatchContentChanged) {
                    if (globalThis.DotnetExports !== undefined) {
                        Clipboard.dispatchContentChanged = globalThis.DotnetExports.Uno.Windows.ApplicationModel.DataTransfer.Clipboard.DispatchContentChanged;
                    }
                    else {
                        throw `Clipboard: Unable to find dotnet exports`;
                    }
                }
                Clipboard.dispatchContentChanged();
            }
        }
        Utils.Clipboard = Clipboard;
    })(Utils = Uno.Utils || (Uno.Utils = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var ApplicationModel;
    (function (ApplicationModel) {
        var DataTransfer;
        (function (DataTransfer) {
            class DataTransferManager {
                static isSupported() {
                    var navigatorAny = navigator;
                    return typeof navigatorAny.share === "function";
                }
                static async showShareUI(title, text, url) {
                    var data = {};
                    if (title) {
                        data.title = title;
                    }
                    if (text) {
                        data.text = text;
                    }
                    if (url) {
                        data.url = url;
                    }
                    if (navigator.share) {
                        try {
                            await navigator.share(data);
                            return "true";
                        }
                        catch (e) {
                            console.log("Sharing failed:" + e);
                            return "false";
                        }
                    }
                    console.log("navigator.share API is not available in this browser");
                    return "false";
                }
            }
            DataTransfer.DataTransferManager = DataTransferManager;
        })(DataTransfer = ApplicationModel.DataTransfer || (ApplicationModel.DataTransfer = {}));
    })(ApplicationModel = Windows.ApplicationModel || (Windows.ApplicationModel = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Devices;
    (function (Devices) {
        var Enumeration;
        (function (Enumeration) {
            var Internal;
            (function (Internal) {
                var Providers;
                (function (Providers) {
                    var Midi;
                    (function (Midi) {
                        class MidiDeviceClassProvider {
                            static findDevices(findInputDevices) {
                                var result = "";
                                const midi = Uno.Devices.Midi.Internal.WasmMidiAccess.getMidi();
                                if (findInputDevices) {
                                    midi.inputs.forEach((input, key) => {
                                        const inputId = input.id;
                                        const name = input.name;
                                        const encodedMetadata = encodeURIComponent(inputId) + '#' + encodeURIComponent(name);
                                        result += encodedMetadata + '&';
                                    });
                                }
                                else {
                                    midi.outputs.forEach((output, key) => {
                                        const outputId = output.id;
                                        const name = output.name;
                                        const encodedMetadata = encodeURIComponent(outputId) + '#' + encodeURIComponent(name);
                                        result += encodedMetadata + '&';
                                    });
                                }
                                return result;
                            }
                        }
                        Midi.MidiDeviceClassProvider = MidiDeviceClassProvider;
                    })(Midi = Providers.Midi || (Providers.Midi = {}));
                })(Providers = Internal.Providers || (Internal.Providers = {}));
            })(Internal = Enumeration.Internal || (Enumeration.Internal = {}));
        })(Enumeration = Devices.Enumeration || (Devices.Enumeration = {}));
    })(Devices = Uno.Devices || (Uno.Devices = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Devices;
    (function (Devices) {
        var Enumeration;
        (function (Enumeration) {
            var Internal;
            (function (Internal) {
                var Providers;
                (function (Providers) {
                    var Midi;
                    (function (Midi) {
                        class MidiDeviceConnectionWatcher {
                            static startStateChanged() {
                                const midi = Uno.Devices.Midi.Internal.WasmMidiAccess.getMidi();
                                midi.addEventListener("statechange", MidiDeviceConnectionWatcher.onStateChanged);
                            }
                            static stopStateChanged() {
                                const midi = Uno.Devices.Midi.Internal.WasmMidiAccess.getMidi();
                                midi.removeEventListener("statechange", MidiDeviceConnectionWatcher.onStateChanged);
                            }
                            static onStateChanged(event) {
                                if (!MidiDeviceConnectionWatcher.dispatchStateChanged) {
                                    if (globalThis.DotnetExports !== undefined) {
                                        MidiDeviceConnectionWatcher.dispatchStateChanged = globalThis.DotnetExports.Uno.Uno.Devices.Enumeration.Internal.Providers.Midi.MidiDeviceConnectionWatcher.DispatchStateChanged;
                                    }
                                    else {
                                        throw `MidiDeviceConnectionWatcher: Unable to find dotnet exports`;
                                    }
                                }
                                const port = event.port;
                                const isInput = port.type == "input";
                                const isConnected = port.state == "connected";
                                MidiDeviceConnectionWatcher.dispatchStateChanged(port.id, port.name, isInput, isConnected);
                            }
                        }
                        Midi.MidiDeviceConnectionWatcher = MidiDeviceConnectionWatcher;
                    })(Midi = Providers.Midi || (Providers.Midi = {}));
                })(Providers = Internal.Providers || (Internal.Providers = {}));
            })(Internal = Enumeration.Internal || (Enumeration.Internal = {}));
        })(Enumeration = Devices.Enumeration || (Devices.Enumeration = {}));
    })(Devices = Uno.Devices || (Uno.Devices = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Geolocation;
        (function (Geolocation) {
            let GeolocationAccessStatus;
            (function (GeolocationAccessStatus) {
                GeolocationAccessStatus["Allowed"] = "Allowed";
                GeolocationAccessStatus["Denied"] = "Denied";
                GeolocationAccessStatus["Unspecified"] = "Unspecified";
            })(GeolocationAccessStatus || (GeolocationAccessStatus = {}));
            let PositionStatus;
            (function (PositionStatus) {
                PositionStatus["Ready"] = "Ready";
                PositionStatus["Initializing"] = "Initializing";
                PositionStatus["NoData"] = "NoData";
                PositionStatus["Disabled"] = "Disabled";
                PositionStatus["NotInitialized"] = "NotInitialized";
                PositionStatus["NotAvailable"] = "NotAvailable";
            })(PositionStatus || (PositionStatus = {}));
            class Geolocator {
                static initialize() {
                    var _a, _b, _c, _d, _e;
                    this.positionWatches = {};
                    if (!Geolocator.interopInitialized) {
                        const exports = (_e = (_d = (_c = (_b = (_a = globalThis.DotnetExports) === null || _a === void 0 ? void 0 : _a.Uno) === null || _b === void 0 ? void 0 : _b.Uno) === null || _c === void 0 ? void 0 : _c.Devices) === null || _d === void 0 ? void 0 : _d.Geolocation) === null || _e === void 0 ? void 0 : _e.Geolocator;
                        if (exports !== undefined) {
                            Geolocator.dispatchAccessRequest = exports.DispatchAccessRequest;
                            Geolocator.dispatchError = exports.DispatchError;
                            Geolocator.dispatchGeoposition = exports.DispatchGeoposition;
                        }
                        else {
                            throw `Geolocator: Unable to find dotnet exports`;
                        }
                        Geolocator.interopInitialized = true;
                    }
                }
                //checks for permission to the geolocation services
                static requestAccess() {
                    Geolocator.initialize();
                    if (navigator.geolocation) {
                        navigator.geolocation.getCurrentPosition((_) => {
                            Geolocator.dispatchAccessRequest(GeolocationAccessStatus.Allowed);
                        }, (error) => {
                            if (error.code == error.PERMISSION_DENIED) {
                                Geolocator.dispatchAccessRequest(GeolocationAccessStatus.Denied);
                            }
                            else if (error.code == error.POSITION_UNAVAILABLE ||
                                error.code == error.TIMEOUT) {
                                //position unavailable but we still have permission
                                Geolocator.dispatchAccessRequest(GeolocationAccessStatus.Allowed);
                            }
                            else {
                                Geolocator.dispatchAccessRequest(GeolocationAccessStatus.Unspecified);
                            }
                        }, { enableHighAccuracy: false, maximumAge: 86400000, timeout: 100 });
                    }
                    else {
                        Geolocator.dispatchAccessRequest(GeolocationAccessStatus.Denied);
                    }
                }
                //retrieves a single geoposition
                static getGeoposition(desiredAccuracyInMeters, maximumAge, timeout, requestId) {
                    Geolocator.initialize();
                    if (navigator.geolocation) {
                        this.getAccurateCurrentPosition((position) => Geolocator.handleGeoposition(position, requestId), (error) => Geolocator.handleError(error, requestId), desiredAccuracyInMeters, {
                            enableHighAccuracy: desiredAccuracyInMeters < 50,
                            maximumAge: maximumAge,
                            timeout: timeout
                        });
                    }
                    else {
                        Geolocator.dispatchError(PositionStatus.NotAvailable, requestId);
                    }
                }
                static startPositionWatch(desiredAccuracyInMeters, requestId) {
                    Geolocator.initialize();
                    if (navigator.geolocation) {
                        Geolocator.positionWatches[requestId] = navigator.geolocation.watchPosition((position) => Geolocator.handleGeoposition(position, requestId), (error) => Geolocator.handleError(error, requestId));
                        return true;
                    }
                    else {
                        return false;
                    }
                }
                static stopPositionWatch(desiredAccuracyInMeters, requestId) {
                    navigator.geolocation.clearWatch(Geolocator.positionWatches[requestId]);
                    delete Geolocator.positionWatches[requestId];
                }
                static handleGeoposition(position, requestId) {
                    var serializedGeoposition = position.coords.latitude + ":" +
                        position.coords.longitude + ":" +
                        position.coords.altitude + ":" +
                        position.coords.altitudeAccuracy + ":" +
                        position.coords.accuracy + ":" +
                        position.coords.heading + ":" +
                        position.coords.speed + ":" +
                        position.timestamp;
                    Geolocator.dispatchGeoposition(serializedGeoposition, requestId);
                }
                static handleError(error, requestId) {
                    if (error.code == error.TIMEOUT) {
                        Geolocator.dispatchError(PositionStatus.NoData, requestId);
                    }
                    else if (error.code == error.PERMISSION_DENIED) {
                        Geolocator.dispatchError(PositionStatus.Disabled, requestId);
                    }
                    else if (error.code == error.POSITION_UNAVAILABLE) {
                        Geolocator.dispatchError(PositionStatus.NotAvailable, requestId);
                    }
                }
                //this attempts to squeeze out the requested accuracy from the GPS by utilizing the set timeout
                //adapted from https://github.com/gregsramblings/getAccurateCurrentPosition/blob/master/geo.js		
                static getAccurateCurrentPosition(geolocationSuccess, geolocationError, desiredAccuracy, options) {
                    var lastCheckedPosition;
                    var locationEventCount = 0;
                    var watchId;
                    var timerId;
                    var checkLocation = function (position) {
                        lastCheckedPosition = position;
                        locationEventCount = locationEventCount + 1;
                        //is the accuracy enough?
                        if (position.coords.accuracy <= desiredAccuracy) {
                            clearTimeout(timerId);
                            navigator.geolocation.clearWatch(watchId);
                            foundPosition(position);
                        }
                    };
                    var stopTrying = function () {
                        navigator.geolocation.clearWatch(watchId);
                        foundPosition(lastCheckedPosition);
                    };
                    var onError = function (error) {
                        clearTimeout(timerId);
                        navigator.geolocation.clearWatch(watchId);
                        geolocationError(error);
                    };
                    var foundPosition = function (position) {
                        geolocationSuccess(position);
                    };
                    watchId = navigator.geolocation.watchPosition(checkLocation, onError, options);
                    timerId = setTimeout(stopTrying, options.timeout);
                }
                ;
            }
            Geolocator.interopInitialized = false;
            Geolocation.Geolocator = Geolocator;
        })(Geolocation = Devices.Geolocation || (Devices.Geolocation = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Input;
        (function (Input) {
            let PointerDeviceType;
            (function (PointerDeviceType) {
                PointerDeviceType[PointerDeviceType["Touch"] = 0] = "Touch";
                PointerDeviceType[PointerDeviceType["Pen"] = 1] = "Pen";
                PointerDeviceType[PointerDeviceType["Mouse"] = 2] = "Mouse";
            })(PointerDeviceType = Input.PointerDeviceType || (Input.PointerDeviceType = {}));
        })(Input = Devices.Input || (Devices.Input = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Midi;
        (function (Midi) {
            class MidiInPort {
                constructor(managedId, inputPort) {
                    this.messageReceived = (event) => {
                        var serializedMessage = event.data[0].toString();
                        for (var i = 1; i < event.data.length; i++) {
                            serializedMessage += ':' + event.data[i];
                        }
                        MidiInPort.dispatchMessage(this.managedId, serializedMessage, event.timeStamp);
                    };
                    this.managedId = managedId;
                    this.inputPort = inputPort;
                }
                static createPort(managedId, encodedDeviceId) {
                    const midi = Uno.Devices.Midi.Internal.WasmMidiAccess.getMidi();
                    const deviceId = decodeURIComponent(encodedDeviceId);
                    const input = midi.inputs.get(deviceId);
                    MidiInPort.instanceMap[managedId] = new MidiInPort(managedId, input);
                }
                static removePort(managedId) {
                    MidiInPort.stopMessageListener(managedId);
                    delete MidiInPort.instanceMap[managedId];
                }
                static startMessageListener(managedId) {
                    if (!MidiInPort.dispatchMessage) {
                        if (globalThis.DotnetExports !== undefined) {
                            MidiInPort.dispatchMessage = globalThis.DotnetExports.Uno.Windows.Devices.Midi.MidiInPort.DispatchMessage;
                        }
                        else {
                            throw `MidiInPort: Unable to find dotnet exports`;
                        }
                    }
                    const instance = MidiInPort.instanceMap[managedId];
                    instance.inputPort.addEventListener("midimessage", instance.messageReceived);
                }
                static stopMessageListener(managedId) {
                    const instance = MidiInPort.instanceMap[managedId];
                    instance.inputPort.removeEventListener("midimessage", instance.messageReceived);
                }
            }
            MidiInPort.instanceMap = {};
            Midi.MidiInPort = MidiInPort;
        })(Midi = Devices.Midi || (Devices.Midi = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Midi;
        (function (Midi) {
            class MidiOutPort {
                static sendBuffer(encodedDeviceId, timestamp, data) {
                    const midi = Uno.Devices.Midi.Internal.WasmMidiAccess.getMidi();
                    const deviceId = decodeURIComponent(encodedDeviceId);
                    const output = midi.outputs.get(deviceId);
                    output.send(data, timestamp);
                }
            }
            Midi.MidiOutPort = MidiOutPort;
        })(Midi = Devices.Midi || (Devices.Midi = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Devices;
    (function (Devices) {
        var Midi;
        (function (Midi) {
            var Internal;
            (function (Internal) {
                class WasmMidiAccess {
                    static request(systemExclusive) {
                        if (navigator.requestMIDIAccess) {
                            return navigator.requestMIDIAccess({ sysex: systemExclusive })
                                .then((midi) => {
                                WasmMidiAccess.midiAccess = midi;
                                return "true";
                            }, () => "false");
                        }
                        else {
                            return Promise.resolve("false");
                        }
                    }
                    static getMidi() {
                        return WasmMidiAccess.midiAccess;
                    }
                }
                Internal.WasmMidiAccess = WasmMidiAccess;
            })(Internal = Midi.Internal || (Midi.Internal = {}));
        })(Midi = Devices.Midi || (Devices.Midi = {}));
    })(Devices = Uno.Devices || (Uno.Devices = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Sensors;
        (function (Sensors) {
            class Accelerometer {
                static initialize() {
                    try {
                        if (typeof window.Accelerometer === "function") {
                            if (globalThis.DotnetExports !== undefined) {
                                Accelerometer.dispatchReading = globalThis.DotnetExports.Uno.Windows.Devices.Sensors.Accelerometer.DispatchReading;
                            }
                            else {
                                throw `Accelerometer: Unable to find dotnet exports`;
                            }
                            const AccelerometerClass = window.Accelerometer;
                            Accelerometer.accelerometer = new AccelerometerClass({ frequency: 60 });
                            return true;
                        }
                    }
                    catch (error) {
                        //sensor not available
                        console.log("Accelerometer could not be initialized:", error);
                    }
                    return false;
                }
                static startReading() {
                    Accelerometer.accelerometer.addEventListener("reading", Accelerometer.readingChangedHandler);
                    Accelerometer.accelerometer.start();
                }
                static stopReading() {
                    Accelerometer.accelerometer.removeEventListener("reading", Accelerometer.readingChangedHandler);
                    Accelerometer.accelerometer.stop();
                }
                static readingChangedHandler(event) {
                    Accelerometer.dispatchReading(Accelerometer.accelerometer.x, Accelerometer.accelerometer.y, Accelerometer.accelerometer.z);
                }
            }
            Sensors.Accelerometer = Accelerometer;
        })(Sensors = Devices.Sensors || (Devices.Sensors = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Sensors;
        (function (Sensors) {
            class Gyrometer {
                static initialize() {
                    try {
                        if (typeof window.Gyroscope === "function") {
                            if (globalThis.DotnetExports !== undefined) {
                                this.dispatchReading = globalThis.DotnetExports.Uno.Windows.Devices.Sensors.Gyrometer.DispatchReading;
                            }
                            else {
                                throw `Gyrometer: Unable to find dotnet exports`;
                            }
                            let GyroscopeClass = window.Gyroscope;
                            this.gyroscope = new GyroscopeClass({ referenceFrame: "device" });
                            return true;
                        }
                    }
                    catch (error) {
                        //sensor not available
                        console.log("Gyroscope could not be initialized.");
                    }
                    return false;
                }
                static startReading() {
                    this.gyroscope.addEventListener("reading", Gyrometer.readingChangedHandler);
                    this.gyroscope.start();
                }
                static stopReading() {
                    this.gyroscope.removeEventListener("reading", Gyrometer.readingChangedHandler);
                    this.gyroscope.stop();
                }
                static readingChangedHandler(event) {
                    Gyrometer.dispatchReading(Gyrometer.gyroscope.x, Gyrometer.gyroscope.y, Gyrometer.gyroscope.z);
                }
            }
            Sensors.Gyrometer = Gyrometer;
        })(Sensors = Devices.Sensors || (Devices.Sensors = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Sensors;
        (function (Sensors) {
            class LightSensor {
                static initialize() {
                    try {
                        if (typeof window.AmbientLightSensor === "function") {
                            if (globalThis.DotnetExports !== undefined) {
                                LightSensor.dispatchReading = globalThis.DotnetExports.Uno.Windows.Devices.Sensors.LightSensor.DispatchReading;
                            }
                            else {
                                throw `LightSensor: Unable to find dotnet exports`;
                            }
                            const AmbientLightSensorClass = window.AmbientLightSensor;
                            LightSensor.ambientLightSensor = new AmbientLightSensorClass();
                            return true;
                        }
                    }
                    catch (error) {
                        // Sensor not available
                        console.error("AmbientLightSensor could not be initialized.");
                    }
                    return false;
                }
                static startReading() {
                    LightSensor.ambientLightSensor.addEventListener("reading", LightSensor.readingChangedHandler);
                    LightSensor.ambientLightSensor.start();
                }
                static stopReading() {
                    LightSensor.ambientLightSensor.removeEventListener("reading", LightSensor.readingChangedHandler);
                    LightSensor.ambientLightSensor.stop();
                }
                static readingChangedHandler(event) {
                    LightSensor.dispatchReading(LightSensor.ambientLightSensor.illuminance);
                }
            }
            Sensors.LightSensor = LightSensor;
        })(Sensors = Devices.Sensors || (Devices.Sensors = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Devices;
    (function (Devices) {
        var Sensors;
        (function (Sensors) {
            class Magnetometer {
                static initialize() {
                    try {
                        if (typeof window.Magnetometer === "function") {
                            if (globalThis.DotnetExports !== undefined) {
                                this.dispatchReading = globalThis.DotnetExports.Uno.Windows.Devices.Sensors.Magnetometer.DispatchReading;
                            }
                            else {
                                throw `Magnetometer: Unable to find dotnet exports`;
                            }
                            let MagnetometerClass = window.Magnetometer;
                            this.magnetometer = new MagnetometerClass({ referenceFrame: 'device' });
                            return true;
                        }
                    }
                    catch (error) {
                        //sensor not available
                        console.log("Magnetometer could not be initialized.");
                    }
                    return false;
                }
                static startReading() {
                    this.magnetometer.addEventListener("reading", Magnetometer.readingChangedHandler);
                    this.magnetometer.start();
                }
                static stopReading() {
                    this.magnetometer.removeEventListener("reading", Magnetometer.readingChangedHandler);
                    this.magnetometer.stop();
                }
                static readingChangedHandler(event) {
                    Magnetometer.dispatchReading(Magnetometer.magnetometer.x, Magnetometer.magnetometer.y, Magnetometer.magnetometer.z);
                }
            }
            Sensors.Magnetometer = Magnetometer;
        })(Sensors = Devices.Sensors || (Devices.Sensors = {}));
    })(Devices = Windows.Devices || (Windows.Devices = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var UI;
    (function (UI) {
        var Dispatching;
        (function (Dispatching) {
            class NativeDispatcher {
                static init(isReady) {
                    isReady.then(() => {
                        NativeDispatcher._dispatcherCallback = globalThis.DotnetExports.UnoUIDispatching.Uno.UI.Dispatching.NativeDispatcher.DispatcherCallback;
                        NativeDispatcher.WakeUp(true);
                        NativeDispatcher._isReady = true;
                    });
                    ;
                }
                // Queues a dispatcher callback on the event loop
                static WakeUp(force) {
                    if (NativeDispatcher._isReady || force) {
                        window.setImmediate(() => {
                            try {
                                NativeDispatcher._dispatcherCallback();
                            }
                            catch (e) {
                                console.error(`Unhandled dispatcher exception: ${e} (${e.stack})`);
                                throw e;
                            }
                        });
                    }
                }
            }
            Dispatching.NativeDispatcher = NativeDispatcher;
        })(Dispatching = UI.Dispatching || (UI.Dispatching = {}));
    })(UI = Uno.UI || (Uno.UI = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var Gaming;
    (function (Gaming) {
        var Input;
        (function (Input) {
            class Gamepad {
                static getConnectedGamepadIds() {
                    const gamepads = navigator.getGamepads();
                    const separator = ";";
                    var result = '';
                    for (var gamepad of gamepads) {
                        if (gamepad) {
                            result += gamepad.index + separator;
                        }
                    }
                    return result;
                }
                static getReading(id) {
                    var gamepad = navigator.getGamepads()[id];
                    if (!gamepad) {
                        return "";
                    }
                    var result = "";
                    result += gamepad.timestamp;
                    result += '*';
                    for (var axisId = 0; axisId < gamepad.axes.length; axisId++) {
                        if (axisId != 0) {
                            result += '|';
                        }
                        result += gamepad.axes[axisId];
                    }
                    result += '*';
                    for (var buttonId = 0; buttonId < gamepad.buttons.length; buttonId++) {
                        if (buttonId != 0) {
                            result += '|';
                        }
                        result += gamepad.buttons[buttonId].value;
                    }
                    return result;
                }
                static startGamepadAdded() {
                    window.addEventListener("gamepadconnected", Gamepad.onGamepadConnected);
                }
                static endGamepadAdded() {
                    window.removeEventListener("gamepadconnected", Gamepad.onGamepadConnected);
                }
                static startGamepadRemoved() {
                    window.addEventListener("gamepaddisconnected", Gamepad.onGamepadDisconnected);
                }
                static endGamepadRemoved() {
                    window.removeEventListener("gamepaddisconnected", Gamepad.onGamepadDisconnected);
                }
                static onGamepadConnected(e) {
                    if (!Gamepad.dispatchGamepadAdded) {
                        if (globalThis.DotnetExports !== undefined) {
                            Gamepad.dispatchGamepadAdded = globalThis.DotnetExports.Uno.Windows.Gaming.Input.Gamepad.DispatchGamepadAdded;
                        }
                        else {
                            throw `Gamepad: Unable to find dotnet exports`;
                        }
                    }
                    Gamepad.dispatchGamepadAdded(e.gamepad.index);
                }
                static onGamepadDisconnected(e) {
                    if (!Gamepad.dispatchGamepadRemoved) {
                        if (globalThis.DotnetExports !== undefined) {
                            Gamepad.dispatchGamepadRemoved = globalThis.DotnetExports.Uno.Windows.Gaming.Input.Gamepad.DispatchGamepadRemoved;
                        }
                        else {
                            throw `Gamepad: Unable to find dotnet exports`;
                        }
                    }
                    Gamepad.dispatchGamepadRemoved(e.gamepad.index);
                }
            }
            Input.Gamepad = Gamepad;
        })(Input = Gaming.Input || (Gaming.Input = {}));
    })(Gaming = Windows.Gaming || (Windows.Gaming = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Graphics;
    (function (Graphics) {
        var Display;
        (function (Display) {
            let DisplayOrientations;
            (function (DisplayOrientations) {
                DisplayOrientations[DisplayOrientations["None"] = 0] = "None";
                DisplayOrientations[DisplayOrientations["Landscape"] = 1] = "Landscape";
                DisplayOrientations[DisplayOrientations["Portrait"] = 2] = "Portrait";
                DisplayOrientations[DisplayOrientations["LandscapeFlipped"] = 4] = "LandscapeFlipped";
                DisplayOrientations[DisplayOrientations["PortraitFlipped"] = 8] = "PortraitFlipped";
            })(DisplayOrientations || (DisplayOrientations = {}));
            class DisplayInformation {
                static getDevicePixelRatio() {
                    return globalThis.devicePixelRatio;
                }
                static getScreenWidth() {
                    return globalThis.screen.width;
                }
                static getScreenHeight() {
                    return globalThis.screen.height;
                }
                static getScreenOrientationAngle() {
                    var _a;
                    return (_a = globalThis.screen.orientation) === null || _a === void 0 ? void 0 : _a.angle;
                }
                static getScreenOrientationType() {
                    var _a;
                    return (_a = globalThis.screen.orientation) === null || _a === void 0 ? void 0 : _a.type;
                }
                static startOrientationChanged() {
                    window.screen.orientation.addEventListener("change", DisplayInformation.onOrientationChange);
                }
                static stopOrientationChanged() {
                    window.screen.orientation.removeEventListener("change", DisplayInformation.onOrientationChange);
                }
                static startDpiChanged() {
                    // DPI can be observed using matchMedia query, but only for certain breakpoints
                    // for accurate observation, we use polling
                    DisplayInformation.lastDpi = window.devicePixelRatio;
                    // start polling the devicePixel
                    DisplayInformation.dpiWatcher = window.setInterval(DisplayInformation.updateDpi, DisplayInformation.DpiCheckInterval);
                }
                static stopDpiChanged() {
                    window.clearInterval(DisplayInformation.dpiWatcher);
                }
                static async setOrientationAsync(uwpOrientations) {
                    const oldOrientation = screen.orientation.type;
                    const orientations = DisplayInformation.parseUwpOrientation(uwpOrientations);
                    if (orientations.includes(oldOrientation)) {
                        return;
                    }
                    // Setting the orientation requires briefly changing the device to fullscreen.
                    // This causes a glitch, which is unnecessary for devices which does not support
                    // setting the orientation, such as most desktop browsers.
                    // We therefore attempt to check for support, and do nothing if the feature is
                    // unavailable.
                    if (DisplayInformation.lockingSupported == null) {
                        try {
                            await screen.orientation.lock(oldOrientation);
                            DisplayInformation.lockingSupported = true;
                        }
                        catch (e) {
                            if (e instanceof DOMException && e.name === "NotSupportedError") {
                                DisplayInformation.lockingSupported = false;
                                console.log("This browser does not support setting the orientation.");
                            }
                            else {
                                // On most mobile devices we should reach this line.
                                DisplayInformation.lockingSupported = true;
                            }
                        }
                    }
                    if (!DisplayInformation.lockingSupported) {
                        return;
                    }
                    const wasFullscreen = document.fullscreenElement != null;
                    if (!wasFullscreen) {
                        await document.body.requestFullscreen();
                    }
                    for (const orientation of orientations) {
                        try {
                            // On success, screen.orientation should fire the 'change' event.
                            await screen.orientation.lock(orientation);
                            break;
                        }
                        catch (e) {
                            // Absorb all errors to ensure that the exitFullscreen block below is called.
                            console.log(`Failed to set the screen orientation to '${orientation}': ${e}`);
                        }
                    }
                    if (!wasFullscreen) {
                        await document.exitFullscreen();
                    }
                }
                static parseUwpOrientation(uwpOrientations) {
                    const orientations = [];
                    if (uwpOrientations & DisplayOrientations.Landscape) {
                        orientations.push("landscape-primary");
                    }
                    if (uwpOrientations & DisplayOrientations.Portrait) {
                        orientations.push("portrait-primary");
                    }
                    if (uwpOrientations & DisplayOrientations.LandscapeFlipped) {
                        orientations.push("landscape-secondary");
                    }
                    if (uwpOrientations & DisplayOrientations.PortraitFlipped) {
                        orientations.push("portrait-secondary");
                    }
                    return orientations;
                }
                static updateDpi() {
                    const currentDpi = window.devicePixelRatio;
                    if (Math.abs(DisplayInformation.lastDpi - currentDpi) > 0.001) {
                        if (DisplayInformation.dispatchDpiChanged == null) {
                            if (globalThis.DotnetExports !== undefined) {
                                DisplayInformation.dispatchDpiChanged = globalThis.DotnetExports.Uno.Windows.Graphics.Display.DisplayInformation.DispatchDpiChanged;
                            }
                            else {
                                throw `DisplayInformation: Unable to find dotnet exports`;
                            }
                        }
                        DisplayInformation.dispatchDpiChanged(currentDpi);
                    }
                    DisplayInformation.lastDpi = currentDpi;
                }
                static onOrientationChange() {
                    if (DisplayInformation.dispatchOrientationChanged == null) {
                        if (globalThis.DotnetExports !== undefined) {
                            DisplayInformation.dispatchOrientationChanged = globalThis.DotnetExports.Uno.Windows.Graphics.Display.DisplayInformation.DispatchOrientationChanged;
                        }
                        else {
                            throw `DisplayInformation: Unable to find dotnet exports`;
                        }
                    }
                    DisplayInformation.dispatchOrientationChanged(window.screen.orientation.type);
                }
            }
            DisplayInformation.DpiCheckInterval = 1000;
            Display.DisplayInformation = DisplayInformation;
        })(Display = Graphics.Display || (Graphics.Display = {}));
    })(Graphics = Windows.Graphics || (Windows.Graphics = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Helpers;
    (function (Helpers) {
        var Theming;
        (function (Theming) {
            let SystemTheme;
            (function (SystemTheme) {
                SystemTheme["Light"] = "Light";
                SystemTheme["Dark"] = "Dark";
            })(SystemTheme = Theming.SystemTheme || (Theming.SystemTheme = {}));
        })(Theming = Helpers.Theming || (Helpers.Theming = {}));
    })(Helpers = Uno.Helpers || (Uno.Helpers = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Helpers;
    (function (Helpers) {
        var Theming;
        (function (Theming) {
            class SystemThemeHelper {
                static getSystemTheme() {
                    if (window.matchMedia) {
                        if (window.matchMedia("(prefers-color-scheme: dark)").matches) {
                            return Theming.SystemTheme.Dark;
                        }
                        if (window.matchMedia("(prefers-color-scheme: light)").matches) {
                            return Theming.SystemTheme.Light;
                        }
                    }
                    return null;
                }
                static observeSystemTheme() {
                    if (!SystemThemeHelper.dispatchThemeChange) {
                        if (globalThis.DotnetExports !== undefined) {
                            SystemThemeHelper.dispatchThemeChange = globalThis.DotnetExports.Uno.Uno.Helpers.Theming.SystemThemeHelper.DispatchSystemThemeChange;
                        }
                        else {
                            throw `SystemThemeHelper: Unable to find dotnet exports`;
                        }
                    }
                    if (window.matchMedia) {
                        window.matchMedia('(prefers-color-scheme: dark)').addEventListener("change", () => {
                            SystemThemeHelper.dispatchThemeChange();
                        });
                    }
                }
            }
            Theming.SystemThemeHelper = SystemThemeHelper;
        })(Theming = Helpers.Theming || (Helpers.Theming = {}));
    })(Helpers = Uno.Helpers || (Uno.Helpers = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var Media;
    (function (Media) {
        class SpeechRecognizer {
            constructor(managedId, culture) {
                this.onResult = (event) => {
                    if (event.results[0].isFinal) {
                        if (!SpeechRecognizer.dispatchResult) {
                            if (globalThis.DotnetExports !== undefined) {
                                SpeechRecognizer.dispatchResult = globalThis.DotnetExports.Uno.Windows.Media.SpeechRecognition.SpeechRecognizer.DispatchResult;
                            }
                            else {
                                throw `SpeechRecognizer: Unable to find dotnet exports`;
                            }
                        }
                        SpeechRecognizer.dispatchResult(this.managedId, event.results[0][0].transcript, event.results[0][0].confidence);
                    }
                    else {
                        if (!SpeechRecognizer.dispatchHypothesis) {
                            if (globalThis.DotnetExports !== undefined) {
                                SpeechRecognizer.dispatchHypothesis = globalThis.DotnetExports.Uno.Windows.Media.SpeechRecognition.SpeechRecognizer.DispatchHypothesis;
                            }
                            else {
                                throw `SpeechRecognizer: Unable to find dotnet exports`;
                            }
                        }
                        SpeechRecognizer.dispatchHypothesis(this.managedId, event.results[0][0].transcript);
                    }
                };
                this.onSpeechStart = () => {
                    if (!SpeechRecognizer.dispatchStatus) {
                        if (globalThis.DotnetExports !== undefined) {
                            SpeechRecognizer.dispatchStatus = globalThis.DotnetExports.Uno.Windows.Media.SpeechRecognition.SpeechRecognizer.DispatchStatus;
                        }
                        else {
                            throw `SpeechRecognizer: Unable to find dotnet exports`;
                        }
                    }
                    SpeechRecognizer.dispatchStatus(this.managedId, "SpeechDetected");
                };
                this.onError = (event) => {
                    if (!SpeechRecognizer.dispatchError) {
                        if (globalThis.DotnetExports !== undefined) {
                            SpeechRecognizer.dispatchError = globalThis.DotnetExports.Uno.Windows.Media.SpeechRecognition.SpeechRecognizer.DispatchError;
                        }
                        else {
                            throw `SpeechRecognizer: Unable to find dotnet exports`;
                        }
                    }
                    SpeechRecognizer.dispatchError(this.managedId, event.error);
                };
                this.managedId = managedId;
                if (window.SpeechRecognition) {
                    this.recognition = new window.SpeechRecognition(culture);
                }
                else if (window.webkitSpeechRecognition) {
                    this.recognition = new window.webkitSpeechRecognition(culture);
                }
                if (this.recognition) {
                    this.recognition.addEventListener("result", this.onResult);
                    this.recognition.addEventListener("speechstart", this.onSpeechStart);
                    this.recognition.addEventListener("error", this.onError);
                }
            }
            static initialize(managedId, culture) {
                const recognizer = new SpeechRecognizer(managedId, culture);
                SpeechRecognizer.instanceMap[managedId] = recognizer;
            }
            static recognize(managedId) {
                const recognizer = SpeechRecognizer.instanceMap[managedId];
                if (recognizer.recognition) {
                    recognizer.recognition.continuous = false;
                    recognizer.recognition.interimResults = true;
                    recognizer.recognition.start();
                    return true;
                }
                else {
                    return false;
                }
            }
            static removeInstance(managedId) {
                const recognizer = SpeechRecognizer.instanceMap[managedId];
                recognizer.recognition.removeEventListener("result", recognizer.onResult);
                recognizer.recognition.removeEventListener("speechstart", recognizer.onSpeechStart);
                recognizer.recognition.removeEventListener("error", recognizer.onError);
                delete SpeechRecognizer.instanceMap[managedId];
            }
        }
        SpeechRecognizer.instanceMap = {};
        Media.SpeechRecognizer = SpeechRecognizer;
    })(Media = Windows.Media || (Windows.Media = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Networking;
    (function (Networking) {
        var Connectivity;
        (function (Connectivity) {
            class ConnectionProfile {
                static hasInternetAccess() {
                    return navigator.onLine;
                }
            }
            Connectivity.ConnectionProfile = ConnectionProfile;
        })(Connectivity = Networking.Connectivity || (Networking.Connectivity = {}));
    })(Networking = Windows.Networking || (Windows.Networking = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Networking;
    (function (Networking) {
        var Connectivity;
        (function (Connectivity) {
            class NetworkInformation {
                static startStatusChanged() {
                    window.addEventListener("online", NetworkInformation.networkStatusChanged);
                    window.addEventListener("offline", NetworkInformation.networkStatusChanged);
                }
                static stopStatusChanged() {
                    window.removeEventListener("online", NetworkInformation.networkStatusChanged);
                    window.removeEventListener("offline", NetworkInformation.networkStatusChanged);
                }
                static networkStatusChanged() {
                    if (NetworkInformation.dispatchStatusChanged == null) {
                        if (globalThis.DotnetExports !== undefined) {
                            NetworkInformation.dispatchStatusChanged = globalThis.DotnetExports.Uno.Windows.Networking.Connectivity.NetworkInformation.DispatchStatusChanged;
                        }
                        else {
                            throw `NetworkInformation: Unable to find dotnet exports`;
                        }
                    }
                    NetworkInformation.dispatchStatusChanged();
                }
            }
            Connectivity.NetworkInformation = NetworkInformation;
        })(Connectivity = Networking.Connectivity || (Networking.Connectivity = {}));
    })(Networking = Windows.Networking || (Windows.Networking = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Phone;
    (function (Phone) {
        var Devices;
        (function (Devices) {
            var Notification;
            (function (Notification) {
                class VibrationDevice {
                    static initialize() {
                        navigator.vibrate = navigator.vibrate || navigator.webkitVibrate || navigator.mozVibrate || navigator.msVibrate;
                        if (navigator.vibrate) {
                            return true;
                        }
                        return false;
                    }
                    static vibrate(duration) {
                        return window.navigator.vibrate(duration);
                    }
                }
                Notification.VibrationDevice = VibrationDevice;
            })(Notification = Devices.Notification || (Devices.Notification = {}));
        })(Devices = Phone.Devices || (Phone.Devices = {}));
    })(Phone = Windows.Phone || (Windows.Phone = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Security;
    (function (Security) {
        var Authentication;
        (function (Authentication) {
            var Web;
            (function (Web) {
                class WebAuthenticationBroker {
                    static getReturnUrl() {
                        return window.location.origin;
                    }
                    static authenticateUsingIframe(iframeId, urlNavigate, urlRedirect, timeout) {
                        return new Promise((ok, err) => {
                            let iframe;
                            if (iframeId) {
                                iframe = document.getElementById(iframeId);
                            }
                            if (!iframe) {
                                iframe = document.createElement("iframe");
                                iframe.style.opacity = "0";
                                iframe.style.pointerEvents = "none";
                                document.body.append(iframe);
                            }
                            const terminate = () => {
                                iframe.removeEventListener("load", onload);
                                iframe.src = "about:blank";
                                if (!iframeId) {
                                    iframe.remove();
                                }
                            };
                            const onload = () => {
                                if (!iframe.contentDocument) {
                                    return; // can't access right now
                                }
                                const currentUrl = iframe.contentDocument.URL;
                                console.log("iframe src=" + currentUrl);
                                if (currentUrl.indexOf(urlRedirect) === 0) {
                                    terminate();
                                    ok(`success|${currentUrl}`);
                                }
                            };
                            iframe.addEventListener("load", onload);
                            iframe.src = urlNavigate;
                        });
                    }
                    static authenticateUsingWindow(urlNavigate, urlRedirect, title, popUpWidth, popUpHeight, timeout) {
                        let win = null;
                        let timerSubscription = null;
                        return new Promise((ok, err) => {
                            let finished = false;
                            const close = () => {
                                if (win) {
                                    win.close();
                                    win = null;
                                }
                                if (timerSubscription) {
                                    window.clearInterval(timerSubscription);
                                    timerSubscription = null;
                                }
                                if (!finished) {
                                    err("Incomplete");
                                }
                            };
                            const completeSuccessfully = (url) => {
                                if (!finished) {
                                    ok(`success|${url}`);
                                    finished = true;
                                    close();
                                }
                            };
                            const completeUserCancel = () => {
                                if (!finished) {
                                    ok(`cancel`);
                                    finished = true;
                                    close();
                                }
                            };
                            const completeTimeout = () => {
                                if (!finished) {
                                    ok(`timeout`);
                                    finished = true;
                                    close();
                                }
                            };
                            const completeWithError = (error) => {
                                if (!finished) {
                                    err(error);
                                    finished = true;
                                    close();
                                }
                            };
                            try {
                                /**
                                 * adding winLeft and winTop to account for dual monitor
                                 * using screenLeft and screenTop for IE8 and earlier
                                 */
                                const winLeft = window.screenLeft ? window.screenLeft : window.screenX;
                                const winTop = window.screenTop ? window.screenTop : window.screenY;
                                /**
                                 * window.innerWidth displays browser window"s height and width excluding toolbars
                                 * using document.documentElement.clientWidth for IE8 and earlier
                                 */
                                const width = window.innerWidth ||
                                    document.documentElement.clientWidth ||
                                    document.body.clientWidth;
                                const height = window.innerHeight ||
                                    document.documentElement.clientHeight ||
                                    document.body.clientHeight;
                                const left = ((width / 2) - (popUpWidth / 2)) + winLeft;
                                const top = ((height / 2) - (popUpHeight / 2)) + winTop;
                                // open the window
                                win = window.open(urlNavigate, title, "width=" + popUpWidth + ", height=" + popUpHeight + ", top=" + top + ", left=" + left);
                                if (!win) {
                                    completeWithError("Can't open window");
                                    return;
                                }
                                if (win.focus) {
                                    win.focus();
                                }
                                win.addEventListener("beforeunload", close);
                                const onFinalUrlReached = (success, timedout, finalUrlOrMessage) => {
                                    if (success) {
                                        completeSuccessfully(finalUrlOrMessage);
                                    }
                                    else {
                                        if (timedout) {
                                            completeTimeout();
                                        }
                                        else {
                                            completeUserCancel();
                                        }
                                    }
                                };
                                timerSubscription = this.startMonitoringRedirect(win, urlRedirect, timeout, onFinalUrlReached);
                            }
                            catch (e) {
                                completeWithError(`${e}`);
                            }
                        });
                    }
                    static startMonitoringRedirect(win, urlRedirect, timeout, callback) {
                        const currentTime = (new Date()).getTime();
                        const maxTime = currentTime + timeout;
                        const subscription = window.setInterval(() => {
                            try {
                                if ((new Date()).getTime() > maxTime) {
                                    callback(false, true, null);
                                }
                                if (win.closed) {
                                    callback(false, false, "Popup closed");
                                    return;
                                }
                                const url = win.document.URL;
                                if (url.indexOf(urlRedirect) === 0) {
                                    callback(true, false, url);
                                }
                            }
                            catch (e) {
                                // Expected! DOMException / crossed origin until reached correct redirect page
                            }
                        }, 100);
                        return subscription;
                    }
                }
                Web.WebAuthenticationBroker = WebAuthenticationBroker;
            })(Web = Authentication.Web || (Authentication.Web = {}));
        })(Authentication = Security.Authentication || (Security.Authentication = {}));
    })(Security = Windows.Security || (Windows.Security = {}));
})(Windows || (Windows = {}));
// eslint-disable-next-line @typescript-eslint/no-namespace
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        class ApplicationDataContainer {
            static buildStorageKey(locality, key) {
                return `UnoApplicationDataContainer_${locality}_${key}`;
            }
            static buildStoragePrefix(locality) {
                return `UnoApplicationDataContainer_${locality}_`;
            }
            /**
             * Try to get a value from localStorage
             * */
            static getValue(locality, key) {
                const storageKey = ApplicationDataContainer.buildStorageKey(locality, key);
                if (localStorage.hasOwnProperty(storageKey)) {
                    return localStorage.getItem(storageKey);
                }
                else {
                    throw `ApplicationDataContainer.getValue failed for ${storageKey}`;
                }
            }
            /**
             * Set a value to localStorage
             * */
            static setValue(locality, key, value) {
                try {
                    const storageKey = ApplicationDataContainer.buildStorageKey(locality, key);
                    localStorage.setItem(storageKey, value);
                }
                catch (e) {
                    console.debug(`ApplicationDataContainer.setValue failed: ${e}`);
                }
                return true;
            }
            /**
             * Determines if a key is contained in localStorage
             * */
            static containsKey(locality, key) {
                const storageKey = ApplicationDataContainer.buildStorageKey(locality, key);
                try {
                    return localStorage.hasOwnProperty(storageKey);
                }
                catch (e) {
                    throw `ApplicationDataContainer.containsKey failed: ${e}`;
                }
            }
            /**
             * Gets a key by index in localStorage
             * */
            static getKeyByIndex(locality, index) {
                let localityIndex = 0;
                let returnKey = "";
                const prefix = ApplicationDataContainer.buildStoragePrefix(locality);
                try {
                    for (let i = 0; i < localStorage.length; i++) {
                        const storageKey = localStorage.key(i);
                        if (storageKey.startsWith(prefix)) {
                            if (localityIndex === index) {
                                return storageKey.substr(prefix.length);
                            }
                            localityIndex++;
                        }
                    }
                }
                catch (e) {
                    throw `ApplicationDataContainer.getKeyByIndex failed: ${e}`;
                }
            }
            /**
             * Determines the number of items contained in localStorage
             * */
            static getCount(locality) {
                let count = 0;
                const prefix = ApplicationDataContainer.buildStoragePrefix(locality);
                try {
                    for (let i = 0; i < localStorage.length; i++) {
                        const storageKey = localStorage.key(i);
                        if (storageKey.startsWith(prefix)) {
                            count++;
                        }
                    }
                }
                catch (e) {
                    console.debug(`ApplicationDataContainer.getCount failed: ${e}`);
                }
                return count;
            }
            /**
             * Clears items contained in localStorage
             * */
            static clear(locality) {
                const prefix = ApplicationDataContainer.buildStoragePrefix(locality);
                const itemsToRemove = [];
                try {
                    for (let i = 0; i < localStorage.length; i++) {
                        const storageKey = localStorage.key(i);
                        if (storageKey.startsWith(prefix)) {
                            itemsToRemove.push(storageKey);
                        }
                    }
                    for (const item in itemsToRemove) {
                        localStorage.removeItem(itemsToRemove[item]);
                    }
                }
                catch (e) {
                    throw `ApplicationDataContainer.clear failed: ${e}`;
                }
            }
            /**
             * Removes an item contained in localStorage
             * */
            static remove(locality, key) {
                const storageKey = ApplicationDataContainer.buildStorageKey(locality, key);
                let remove = false;
                try {
                    remove = localStorage.hasOwnProperty(storageKey);
                }
                catch (e) {
                    remove = false;
                    console.debug(`ApplicationDataContainer.remove failed: ${e}`);
                }
                if (remove) {
                    localStorage.removeItem(storageKey);
                }
                return remove;
            }
            /**
             * Gets a key by index in localStorage
             * */
            static getValueByIndex(locality, index) {
                let localityIndex = 0;
                let returnKey = "";
                const prefix = ApplicationDataContainer.buildStoragePrefix(locality);
                try {
                    for (let i = 0; i < localStorage.length; i++) {
                        const storageKey = localStorage.key(i);
                        if (storageKey.startsWith(prefix)) {
                            if (localityIndex === index) {
                                return localStorage.getItem(storageKey);
                            }
                            localityIndex++;
                        }
                    }
                }
                catch (e) {
                    throw `ApplicationDataContainer.getValueByIndex failed: ${e}`;
                }
            }
        }
        Storage.ApplicationDataContainer = ApplicationDataContainer;
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
// eslint-disable-next-line @typescript-eslint/no-namespace
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        class AssetManager {
            static async DownloadAssetsManifest(path) {
                const response = await fetch(path);
                return response.text();
            }
            static async DownloadAsset(path) {
                const response = await fetch(path);
                const arrayBuffer = await response.blob().then(b => b.arrayBuffer());
                const size = arrayBuffer.byteLength;
                const responseArray = new Uint8ClampedArray(arrayBuffer);
                const pData = Module._malloc(size);
                Module.HEAPU8.set(responseArray, pData);
                return `${pData};${size}`;
            }
        }
        Storage.AssetManager = AssetManager;
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        class NativeStorageFile {
            static async getBasicPropertiesAsync(guid) {
                const file = await Storage.NativeStorageItem.getFile(guid);
                var propertyString = "";
                propertyString += file.size;
                propertyString += "|";
                propertyString += file.lastModified;
                return propertyString;
            }
        }
        Storage.NativeStorageFile = NativeStorageFile;
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var __asyncValues = (this && this.__asyncValues) || function (o) {
    if (!Symbol.asyncIterator) throw new TypeError("Symbol.asyncIterator is not defined.");
    var m = o[Symbol.asyncIterator], i;
    return m ? m.call(o) : (o = typeof __values === "function" ? __values(o) : o[Symbol.iterator](), i = {}, verb("next"), verb("throw"), verb("return"), i[Symbol.asyncIterator] = function () { return this; }, i);
    function verb(n) { i[n] = o[n] && function (v) { return new Promise(function (resolve, reject) { v = o[n](v), settle(resolve, reject, v.done, v.value); }); }; }
    function settle(resolve, reject, d, v) { Promise.resolve(v).then(function(v) { resolve({ value: v, done: d }); }, reject); }
};
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        class NativeStorageFolder {
            /**
             * Creates a new folder inside another folder.
             * @param parentGuid The GUID of the folder to create in.
             * @param folderName The name of the new folder.
             */
            static async createFolderAsync(parentGuid, folderName) {
                try {
                    const parentHandle = Storage.NativeStorageItem.getItem(parentGuid);
                    const newDirectoryHandle = await parentHandle.getDirectoryHandle(folderName, {
                        create: true,
                    });
                    const info = Storage.NativeStorageItem.getInfos(newDirectoryHandle)[0];
                    return JSON.stringify(info);
                }
                catch (_a) {
                    console.log("Could not create folder" + folderName);
                    return null;
                }
            }
            /**
             * Creates a new file inside another folder.
             * @param parentGuid The GUID of the folder to create in.
             * @param folderName The name of the new file.
             */
            static async createFileAsync(parentGuid, fileName) {
                try {
                    const parentHandle = Storage.NativeStorageItem.getItem(parentGuid);
                    const newFileHandle = await parentHandle.getFileHandle(fileName, {
                        create: true,
                    });
                    const info = Storage.NativeStorageItem.getInfos(newFileHandle)[0];
                    return JSON.stringify(info);
                }
                catch (_a) {
                    console.log("Could not create file " + fileName);
                    return null;
                }
            }
            /**
             * Tries to get a folder in the given parent folder by name.
             * @param parentGuid The GUID of the parent folder to get.
             * @param folderName The name of the folder to look for.
             * @returns A GUID of the folder if found, otherwise null.
             */
            static async tryGetFolderAsync(parentGuid, folderName) {
                const parentHandle = Storage.NativeStorageItem.getItem(parentGuid);
                let nestedDirectoryHandle = undefined;
                try {
                    nestedDirectoryHandle = await parentHandle.getDirectoryHandle(folderName);
                }
                catch (ex) {
                    return null;
                }
                if (nestedDirectoryHandle) {
                    return JSON.stringify(Storage.NativeStorageItem.getInfos(nestedDirectoryHandle)[0]);
                }
                return null;
            }
            /**
            * Tries to get a file in the given parent folder by name.
            * @param parentGuid The GUID of the parent folder to get.
            * @param folderName The name of the folder to look for.
            * @returns A GUID of the folder if found, otherwise null.
            */
            static async tryGetFileAsync(parentGuid, fileName) {
                const parentHandle = Storage.NativeStorageItem.getItem(parentGuid);
                let fileHandle = undefined;
                try {
                    fileHandle = await parentHandle.getFileHandle(fileName);
                }
                catch (ex) {
                    return null;
                }
                if (fileHandle) {
                    return JSON.stringify(Storage.NativeStorageItem.getInfos(fileHandle)[0]);
                }
                return null;
            }
            static async deleteItemAsync(parentGuid, itemName) {
                try {
                    const parentHandle = Storage.NativeStorageItem.getItem(parentGuid);
                    await parentHandle.removeEntry(itemName, { recursive: true });
                    return "OK";
                }
                catch (_a) {
                    return null;
                }
            }
            static async getItemsAsync(folderGuid) {
                return await NativeStorageFolder.getEntriesAsync(folderGuid, true, true);
            }
            static async getFoldersAsync(folderGuid) {
                return await NativeStorageFolder.getEntriesAsync(folderGuid, false, true);
            }
            static async getFilesAsync(folderGuid) {
                return await NativeStorageFolder.getEntriesAsync(folderGuid, true, false);
            }
            static async getPrivateRootAsync() {
                if (!navigator.storage.getDirectory) {
                    return null;
                }
                const directory = await navigator.storage.getDirectory();
                if (!directory) {
                    return null;
                }
                const info = Storage.NativeStorageItem.getInfos(directory)[0];
                return JSON.stringify(info);
            }
            static async getEntriesAsync(guid, includeFiles, includeDirectories) {
                var e_1, _a, e_2, _b;
                const folderHandle = Storage.NativeStorageItem.getItem(guid);
                var entries = [];
                // Default to "modern" implementation
                if (folderHandle.values) {
                    try {
                        for (var _c = __asyncValues(folderHandle.values()), _d; _d = await _c.next(), !_d.done;) {
                            var entry = _d.value;
                            entries.push(entry);
                        }
                    }
                    catch (e_1_1) { e_1 = { error: e_1_1 }; }
                    finally {
                        try {
                            if (_d && !_d.done && (_a = _c.return)) await _a.call(_c);
                        }
                        finally { if (e_1) throw e_1.error; }
                    }
                }
                else {
                    try {
                        for (var _e = __asyncValues(folderHandle.getEntries()), _f; _f = await _e.next(), !_f.done;) {
                            var handle = _f.value;
                            entries.push(handle);
                        }
                    }
                    catch (e_2_1) { e_2 = { error: e_2_1 }; }
                    finally {
                        try {
                            if (_f && !_f.done && (_b = _e.return)) await _b.call(_e);
                        }
                        finally { if (e_2) throw e_2.error; }
                    }
                }
                var filteredHandles = [];
                // Filter
                for (var handle of entries) {
                    if (handle.kind == "file" && includeFiles) {
                        filteredHandles.push(handle);
                    }
                    else if (handle.kind == "directory" && includeDirectories) {
                        filteredHandles.push(handle);
                    }
                }
                // Get infos
                var infos = Storage.NativeStorageItem.getInfos(...filteredHandles);
                var json = JSON.stringify(infos);
                return json;
            }
        }
        Storage.NativeStorageFolder = NativeStorageFolder;
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        class NativeStorageItem {
            static addItem(guid, item) {
                NativeStorageItem._guidToItemMap.set(guid, item);
                NativeStorageItem._itemToGuidMap.set(item, guid);
            }
            static removeItem(guid) {
                const handle = NativeStorageItem._guidToItemMap.get(guid);
                NativeStorageItem._guidToItemMap.delete(guid);
                NativeStorageItem._itemToGuidMap.delete(handle);
            }
            static getItem(guid) {
                return NativeStorageItem._guidToItemMap.get(guid);
            }
            static async getFile(guid) {
                const item = NativeStorageItem.getItem(guid);
                if (item instanceof File) {
                    return item;
                }
                if (item instanceof FileSystemFileHandle) {
                    return await item.getFile();
                }
                if (item instanceof FileSystemDirectoryHandle) {
                    throw new Error("Item " + guid + " is a directory handle. You cannot use it as a File!");
                }
                throw new Error("Item " + guid + " is of an unknown type. You cannot use it as a File!");
            }
            static getGuid(item) {
                return NativeStorageItem._itemToGuidMap.get(item);
            }
            static getInfos(...items) {
                const itemsWithoutGuids = [];
                for (const item of items) {
                    const guid = NativeStorageItem.getGuid(item);
                    if (!guid) {
                        itemsWithoutGuids.push(item);
                    }
                }
                NativeStorageItem.storeItems(itemsWithoutGuids);
                const results = [];
                for (const item of items) {
                    const guid = NativeStorageItem.getGuid(item);
                    const info = new Storage.NativeStorageItemInfo();
                    info.id = guid;
                    info.name = item.name;
                    info.isFile = item instanceof File || item.kind === "file";
                    results.push(info);
                }
                return results;
            }
            static storeItems(handles) {
                const missingGuids = NativeStorageItem.generateGuids(handles.length);
                for (let i = 0; i < handles.length; i++) {
                    NativeStorageItem.addItem(missingGuids[i], handles[i]);
                }
            }
            static generateGuids(count) {
                if (!NativeStorageItem.generateGuidBinding) {
                    if (globalThis.DotnetExports !== undefined) {
                        NativeStorageItem.generateGuidBinding = globalThis.DotnetExports.Uno.Uno.Storage.NativeStorageItem.GenerateGuids;
                    }
                    else {
                        throw `NativeStorageItem: Unable to find dotnet exports`;
                    }
                }
                const guids = NativeStorageItem.generateGuidBinding(count);
                return guids.split(";");
            }
        }
        NativeStorageItem._guidToItemMap = new Map();
        NativeStorageItem._itemToGuidMap = new Map();
        Storage.NativeStorageItem = NativeStorageItem;
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        class NativeStorageItemInfo {
        }
        Storage.NativeStorageItemInfo = NativeStorageItemInfo;
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
// eslint-disable-next-line @typescript-eslint/no-namespace
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        class StorageFolder {
            /**
             * Determine if IndexDB is available, some browsers and modes disable it.
             * */
            static isIndexDBAvailable() {
                try {
                    // IndexedDB may not be available in private mode
                    window.indexedDB;
                    return true;
                }
                catch (err) {
                    return false;
                }
            }
            /**
             * Setup the storage persistence of a given set of paths.
             * */
            static async makePersistent(paths) {
                await Windows.ApplicationModel.Core.CoreApplication.waitForInitialized();
                for (var i = 0; i < paths.length; i++) {
                    this.setupStorage(paths[i]);
                }
                // Ensure to sync pseudo file system on unload (and periodically for safety)
                if (!this._isInitialized) {
                    // Request an initial sync to populate the file system
                    StorageFolder.synchronizeFileSystem(true, () => StorageFolder.onStorageInitialized());
                    window.addEventListener("beforeunload", () => this.synchronizeFileSystem(false));
                    setInterval(() => this.synchronizeFileSystem(false), 10000);
                    this._isInitialized = true;
                }
            }
            /**
             * Setup the storage persistence of a given path.
             * */
            static setupStorage(path) {
                if (!this.isIndexDBAvailable()) {
                    console.warn("IndexedDB is not available (private mode or uri starts with file:// ?), changes will not be persisted.");
                    StorageFolder.onStorageInitialized();
                    return;
                }
                if (typeof IDBFS === 'undefined') {
                    console.warn(`IDBFS is not enabled in the project configuration, persistence is disabled. See https://aka.platform.uno/wasm-idbfs for more details`);
                    StorageFolder.onStorageInitialized();
                    return;
                }
                console.debug("Making persistent: " + path);
                FS.mkdir(path);
                FS.mount(IDBFS, {}, path);
            }
            static onStorageInitialized() {
                if (!StorageFolder.dispatchStorageInitialized) {
                    if (globalThis.DotnetExports !== undefined) {
                        StorageFolder.dispatchStorageInitialized = globalThis.DotnetExports.Uno.Windows.Storage.StorageFolder.DispatchStorageInitialized;
                    }
                    else {
                        throw `StorageFolder: Unable to find dotnet exports`;
                    }
                }
                StorageFolder.dispatchStorageInitialized();
            }
            /**
             * Synchronize the IDBFS memory cache back to IndexedDB
             * populate: requests the filesystem to be popuplated from the IndexedDB
             * onSynchronized: function invoked when the synchronization finished
             * */
            static synchronizeFileSystem(populate, onSynchronized = null) {
                if (!StorageFolder._isSynchronizing) {
                    StorageFolder._isSynchronizing = true;
                    FS.syncfs(populate, err => {
                        StorageFolder._isSynchronizing = false;
                        if (onSynchronized) {
                            onSynchronized();
                        }
                        if (err) {
                            console.error(`Error synchronizing filesystem from IndexDB: ${err} (errno: ${err.errno})`);
                        }
                    });
                }
            }
        }
        StorageFolder._isInitialized = false;
        StorageFolder._isSynchronizing = false;
        Storage.StorageFolder = StorageFolder;
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        var Pickers;
        (function (Pickers) {
            class FileOpenPicker {
                static isNativeSupported() {
                    return typeof showOpenFilePicker === "function";
                }
                static async nativePickFilesAsync(multiple, showAllEntry, fileTypesJson, id, startIn) {
                    if (!FileOpenPicker.isNativeSupported()) {
                        return JSON.stringify([]);
                    }
                    const options = {
                        excludeAcceptAllOption: !showAllEntry,
                        id: id,
                        multiple: multiple,
                        startIn: startIn,
                        types: [],
                    };
                    const acceptTypes = JSON.parse(fileTypesJson);
                    for (const acceptType of acceptTypes) {
                        const pickerAcceptType = {
                            accept: {},
                            description: acceptType.description,
                        };
                        for (const acceptTypeItem of acceptType.accept) {
                            pickerAcceptType.accept[acceptTypeItem.mimeType] = acceptTypeItem.extensions;
                        }
                        options.types.push(pickerAcceptType);
                    }
                    try {
                        const selectedFiles = await showOpenFilePicker(options);
                        const infos = Uno.Storage.NativeStorageItem.getInfos(...selectedFiles);
                        const json = JSON.stringify(infos);
                        return json;
                    }
                    catch (e) {
                        console.log("User did not make a selection or the file selected was" +
                            "deemed too sensitive or dangerous to be exposed to the website - " + e);
                        return JSON.stringify([]);
                    }
                }
                static uploadPickFilesAsync(multiple, targetPath, accept) {
                    return new Promise((resolve, reject) => {
                        const inputElement = document.createElement("input");
                        inputElement.type = "file";
                        inputElement.multiple = multiple;
                        inputElement.accept = accept;
                        inputElement.onchange = async (e) => {
                            const existingFileNames = new Set();
                            var adjustedTargetPath = targetPath;
                            if (!adjustedTargetPath.endsWith('/')) {
                                adjustedTargetPath += '/';
                            }
                            var duplicateFileId = 0;
                            for (const file of inputElement.files) {
                                const fileBuffer = await file.arrayBuffer();
                                const fileBufferView = new Uint8Array(fileBuffer);
                                var targetFileName = "";
                                if (!existingFileNames.has(file.name)) {
                                    targetFileName = adjustedTargetPath + file.name;
                                    existingFileNames.add(file.name);
                                }
                                else {
                                    targetFileName = adjustedTargetPath + duplicateFileId + "_" + file.name;
                                    duplicateFileId++;
                                }
                                FS.writeFile(targetFileName, fileBufferView);
                            }
                            resolve(inputElement.files.length.toString());
                        };
                        inputElement.click();
                    });
                }
            }
            Pickers.FileOpenPicker = FileOpenPicker;
        })(Pickers = Storage.Pickers || (Storage.Pickers = {}));
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        var Pickers;
        (function (Pickers) {
            class FileSavePicker {
                static isNativeSupported() {
                    return typeof showSaveFilePicker === "function";
                }
                static async nativePickSaveFileAsync(showAllEntry, fileTypesJson, suggestedFileName, id, startIn) {
                    if (!FileSavePicker.isNativeSupported()) {
                        return null;
                    }
                    const options = {
                        excludeAcceptAllOption: !showAllEntry,
                        id: id,
                        startIn: startIn,
                        types: [],
                    };
                    const acceptTypes = JSON.parse(fileTypesJson);
                    for (const acceptType of acceptTypes) {
                        const pickerAcceptType = {
                            accept: {},
                            description: acceptType.description,
                        };
                        for (const acceptTypeItem of acceptType.accept) {
                            pickerAcceptType.accept[acceptTypeItem.mimeType] = acceptTypeItem.extensions;
                        }
                        options.types.push(pickerAcceptType);
                    }
                    if (suggestedFileName != "") {
                        // In case the suggested file name does not end with any extension provided by the app
                        // we attach the first one if such exists. This is because JS could otherwise truncate
                        // the filename incorrectly, e.g.:
                        // "this.is.a.filename" would get truncated to "this"
                        var lowerCaseFileName = suggestedFileName.toLowerCase();
                        if (!acceptTypes.some(f => f.accept.some(a => a.extensions.some(e => lowerCaseFileName.endsWith(e.toLowerCase())))) &&
                            acceptTypes.length > 0) {
                            suggestedFileName += acceptTypes[0].accept[0].extensions[0];
                        }
                        options.suggestedName = suggestedFileName;
                    }
                    try {
                        const selectedFile = await showSaveFilePicker(options);
                        const info = Uno.Storage.NativeStorageItem.getInfos(selectedFile)[0];
                        const json = JSON.stringify(info);
                        return json;
                    }
                    catch (e) {
                        console.log("User did not make a selection or the file selected was" +
                            "deemed too sensitive or dangerous to be exposed to the website - " + e);
                        return null;
                    }
                }
                static SaveAs(fileName, dataPtr, size) {
                    const buffer = new Uint8Array(size);
                    for (var i = 0; i < size; i++) {
                        buffer[i] = Module.getValue(dataPtr + i, "i8");
                    }
                    const a = window.document.createElement('a');
                    const blob = new Blob([buffer]);
                    a.href = window.URL.createObjectURL(blob);
                    a.download = fileName;
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Pickers.FileSavePicker = FileSavePicker;
        })(Pickers = Storage.Pickers || (Storage.Pickers = {}));
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var Storage;
    (function (Storage) {
        var Pickers;
        (function (Pickers) {
            class FolderPicker {
                static isNativeSupported() {
                    return typeof showDirectoryPicker === "function";
                }
                static async pickSingleFolderAsync(id, startIn) {
                    if (!FolderPicker.isNativeSupported()) {
                        return null;
                    }
                    try {
                        const options = {
                            id: id,
                            startIn: startIn,
                        };
                        const selectedFolder = await showDirectoryPicker(options);
                        const info = Uno.Storage.NativeStorageItem.getInfos(selectedFolder)[0];
                        return JSON.stringify(info);
                    }
                    catch (e) {
                        console.log("The user dismissed the prompt without making a selection, " +
                            "or the user agent deems the selected content to be too sensitive or dangerous - " + e);
                        return null;
                    }
                }
            }
            Pickers.FolderPicker = FolderPicker;
        })(Pickers = Storage.Pickers || (Storage.Pickers = {}));
    })(Storage = Windows.Storage || (Windows.Storage = {}));
})(Windows || (Windows = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        var Pickers;
        (function (Pickers) {
            class NativeFilePickerAcceptType {
            }
            Pickers.NativeFilePickerAcceptType = NativeFilePickerAcceptType;
        })(Pickers = Storage.Pickers || (Storage.Pickers = {}));
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        var Pickers;
        (function (Pickers) {
            class NativeFilePickerAcceptTypeItem {
            }
            Pickers.NativeFilePickerAcceptTypeItem = NativeFilePickerAcceptTypeItem;
        })(Pickers = Storage.Pickers || (Storage.Pickers = {}));
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        var Streams;
        (function (Streams) {
            class NativeFileReadStream {
                constructor(file) {
                    this._file = file;
                }
                static async openAsync(streamId, fileId) {
                    const file = await Storage.NativeStorageItem.getFile(fileId);
                    const fileSize = file.size;
                    const stream = new NativeFileReadStream(file);
                    NativeFileReadStream._streamMap.set(streamId, stream);
                    return fileSize.toString();
                }
                static async readAsync(streamId, targetArrayPointer, offset, count, position) {
                    var streamReader;
                    var readerNeedsRelease = true;
                    try {
                        const instance = NativeFileReadStream._streamMap.get(streamId);
                        var totalRead = 0;
                        var stream = await instance._file.slice(position, position + count).stream();
                        streamReader = stream.getReader();
                        var chunk = await streamReader.read();
                        while (!chunk.done && chunk.value) {
                            for (var i = 0; i < chunk.value.length; i++) {
                                Module.HEAPU8[targetArrayPointer + offset + totalRead + i] = chunk.value[i];
                            }
                            totalRead += chunk.value.length;
                            chunk = await streamReader.read();
                        }
                        // If this is the end of stream, it closed itself
                        readerNeedsRelease = !chunk.done;
                        return totalRead.toString();
                    }
                    finally {
                        // Reader must be released only if the underlying stream has not already closed it.				
                        // Otherwise the release operation sets a new Promise.reject as reader.closed which
                        // raises silent but observable exception in Chromium-based browsers.
                        if (streamReader && readerNeedsRelease) {
                            // Silently handling TypeError exceptions on closed event as the releaseLock()
                            // raises one in case of a successful close.
                            streamReader.closed.catch(reason => {
                                if (!(reason instanceof TypeError)) {
                                    throw reason;
                                }
                            });
                            streamReader.cancel();
                            streamReader.releaseLock();
                        }
                    }
                }
                static close(streamId) {
                    NativeFileReadStream._streamMap.delete(streamId);
                }
            }
            NativeFileReadStream._streamMap = new Map();
            Streams.NativeFileReadStream = NativeFileReadStream;
        })(Streams = Storage.Streams || (Storage.Streams = {}));
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var Storage;
    (function (Storage) {
        var Streams;
        (function (Streams) {
            class NativeFileWriteStream {
                constructor(stream) {
                    this._stream = stream;
                }
                static async openAsync(streamId, fileId) {
                    const item = Storage.NativeStorageItem.getItem(fileId);
                    if (item instanceof File) {
                        return "PermissionNotGranted";
                    }
                    const handle = item;
                    if (!await NativeFileWriteStream.verifyPermissionAsync(handle)) {
                        return "PermissionNotGranted";
                    }
                    const writableStream = await handle.createWritable({ keepExistingData: true });
                    const fileSize = (await handle.getFile()).size;
                    const stream = new NativeFileWriteStream(writableStream);
                    NativeFileWriteStream._streamMap.set(streamId, stream);
                    return fileSize.toString();
                }
                static async verifyPermissionAsync(fileHandle) {
                    const options = {};
                    options.mode = "readwrite";
                    // Check if permission was already granted. If so, return true.
                    if ((await fileHandle.queryPermission(options)) === 'granted') {
                        return true;
                    }
                    // Request permission. If the user grants permission, return true.
                    if ((await fileHandle.requestPermission(options)) === 'granted') {
                        return true;
                    }
                    // The user didn't grant permission, so return false.
                    return false;
                }
                static async writeAsync(streamId, dataArrayPointer, offset, count, position) {
                    const instance = NativeFileWriteStream._streamMap.get(streamId);
                    if (!instance._buffer || instance._buffer.length < count) {
                        instance._buffer = new Uint8Array(count);
                    }
                    var clampedArray = new Uint8Array(count);
                    for (var i = 0; i < count; i++) {
                        clampedArray[i] = Module.HEAPU8[dataArrayPointer + i + offset];
                    }
                    await instance._stream.write({
                        type: 'write',
                        data: clampedArray.subarray(0, count).buffer,
                        position: position
                    });
                    return "";
                }
                static async closeAsync(streamId) {
                    var instance = NativeFileWriteStream._streamMap.get(streamId);
                    if (instance) {
                        await instance._stream.close();
                        NativeFileWriteStream._streamMap.delete(streamId);
                    }
                    return "";
                }
                static async truncateAsync(streamId, length) {
                    var instance = NativeFileWriteStream._streamMap.get(streamId);
                    await instance._stream.truncate(length);
                    return "";
                }
            }
            NativeFileWriteStream._streamMap = new Map();
            Streams.NativeFileWriteStream = NativeFileWriteStream;
        })(Streams = Storage.Streams || (Storage.Streams = {}));
    })(Storage = Uno.Storage || (Uno.Storage = {}));
})(Uno || (Uno = {}));
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        class MemoryManager {
            static getAppMemoryUsage() {
                if (typeof Module === "object") {
                    // Returns an approximate memory usage for the current wasm module.
                    // Initial buffer size is determine by the initial wasm memory defined in
                    // emscripten.
                    return Module.HEAPU8.length;
                }
                return 0;
            }
        }
        System.MemoryManager = MemoryManager;
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var WakeLockType;
(function (WakeLockType) {
    WakeLockType["screen"] = "screen";
})(WakeLockType || (WakeLockType = {}));
;
;
;
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        var Display;
        (function (Display) {
            class DisplayRequest {
                static activateScreenLock() {
                    if (navigator.wakeLock) {
                        DisplayRequest.activeScreenLockPromise = navigator.wakeLock.request(WakeLockType.screen);
                        DisplayRequest.activeScreenLockPromise.catch(reason => console.log("Could not acquire screen lock (" + reason + ")"));
                    }
                    else {
                        console.log("Wake Lock API is not available in this browser.");
                    }
                }
                static deactivateScreenLock() {
                    if (DisplayRequest.activeScreenLockPromise) {
                        DisplayRequest.activeScreenLockPromise.then(sentinel => sentinel.release());
                        DisplayRequest.activeScreenLockPromise = null;
                    }
                }
            }
            Display.DisplayRequest = DisplayRequest;
        })(Display = System.Display || (System.Display = {}));
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        class Launcher {
            /**
            * Load the specified URL into a new tab or window
            * @param url URL to load
            * @returns "True" or "False", depending on whether a new window could be opened or not
            */
            static open(url) {
                const newWindow = window.open(url, "_blank");
                return newWindow != null
                    ? "True"
                    : "False";
            }
        }
        System.Launcher = Launcher;
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        var Power;
        (function (Power) {
            class PowerManager {
                static async initializeAsync() {
                    if (!PowerManager.battery) {
                        PowerManager.battery = await navigator.getBattery();
                    }
                    return null;
                }
                static startChargingChange() {
                    PowerManager.battery.addEventListener("chargingchange", PowerManager.onChargingChange);
                }
                static endChargingChange() {
                    PowerManager.battery.removeEventListener("chargingchange", PowerManager.onChargingChange);
                }
                static startRemainingChargePercentChange() {
                    PowerManager.battery.addEventListener("levelchange", PowerManager.onLevelChange);
                }
                static endRemainingChargePercentChange() {
                    PowerManager.battery.removeEventListener("levelchange", PowerManager.onLevelChange);
                }
                static startRemainingDischargeTimeChange() {
                    PowerManager.battery.addEventListener("dischargingtimechange", PowerManager.onDischargingTimeChange);
                }
                static endRemainingDischargeTimeChange() {
                    PowerManager.battery.removeEventListener("dischargingtimechange", PowerManager.onDischargingTimeChange);
                }
                static getBatteryStatus() {
                    if (PowerManager.battery) {
                        return PowerManager.battery.charging ? "Charging" : "Discharging";
                    }
                    return "Idle";
                }
                static getPowerSupplyStatus() {
                    if (PowerManager.battery) {
                        return PowerManager.battery.charging ? "Adequate" : "NotPresent";
                    }
                    return "NotPresent";
                }
                static getRemainingChargePercent() {
                    if (PowerManager.battery) {
                        return PowerManager.battery.level;
                    }
                    return 1.0;
                }
                static getRemainingDischargeTime() {
                    if (PowerManager.battery) {
                        const dischargingTime = PowerManager.battery.dischargingTime;
                        if (Number.isFinite(dischargingTime)) {
                            return dischargingTime;
                        }
                    }
                    return -1;
                }
                static onChargingChange() {
                    if (!PowerManager.dispatchChargingChanged) {
                        PowerManager.dispatchChargingChanged = globalThis.DotnetExports.Uno.Windows.System.Power.PowerManager.DispatchChargingChanged;
                    }
                    PowerManager.dispatchChargingChanged();
                }
                static onDischargingTimeChange() {
                    if (!PowerManager.dispatchRemainingDischargeTimeChanged) {
                        PowerManager.dispatchChargingChanged = globalThis.DotnetExports.Uno.Windows.System.Power.PowerManager.DispatchRemainingDischargeTimeChanged;
                    }
                    PowerManager.dispatchRemainingDischargeTimeChanged();
                }
                static onLevelChange() {
                    if (!PowerManager.dispatchRemainingChargePercentChanged) {
                        PowerManager.dispatchChargingChanged = globalThis.DotnetExports.Uno.Windows.System.Power.PowerManager.DispatchRemainingChargePercentChanged;
                    }
                    PowerManager.dispatchRemainingChargePercentChanged();
                }
            }
            Power.PowerManager = PowerManager;
        })(Power = System.Power || (System.Power = {}));
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        var Profile;
        (function (Profile) {
            class AnalyticsInfo {
                static getDeviceType() {
                    // Logic based on https://github.com/barisaydinoglu/Detectizr
                    var ua = navigator.userAgent;
                    if (!ua || ua === '') {
                        // No user agent.
                        return "unknown";
                    }
                    if (ua.match(/GoogleTV|SmartTV|SMART-TV|Internet TV|NetCast|NETTV|AppleTV|boxee|Kylo|Roku|DLNADOC|hbbtv|CrKey|CE\-HTML/i)) {
                        // if user agent is a smart TV - http://goo.gl/FocDk
                        return "Television";
                    }
                    else if (ua.match(/Xbox|PLAYSTATION|Wii/i)) {
                        // if user agent is a TV Based Gaming Console
                        return "GameConsole";
                    }
                    else if (ua.match(/QtCarBrowser/i)) {
                        // if the user agent is a car
                        return "Car";
                    }
                    else if (ua.match(/iP(a|ro)d/i) || (ua.match(/tablet/i) && !ua.match(/RX-34/i)) || ua.match(/FOLIO/i)) {
                        // if user agent is a Tablet
                        return "Tablet";
                    }
                    else if (ua.match(/Linux/i) && ua.match(/Android/i) && !ua.match(/Fennec|mobi|HTC Magic|HTCX06HT|Nexus One|SC-02B|fone 945/i)) {
                        // if user agent is an Android Tablet
                        return "Tablet";
                    }
                    else if (ua.match(/Kindle/i) || (ua.match(/Mac OS/i) && ua.match(/Silk/i)) || (ua.match(/AppleWebKit/i) && ua.match(/Silk/i) && !ua.match(/Playstation Vita/i))) {
                        // if user agent is a Kindle or Kindle Fire
                        return "Tablet";
                    }
                    else if (ua.match(/GT-P10|SC-01C|SHW-M180S|SGH-T849|SCH-I800|SHW-M180L|SPH-P100|SGH-I987|zt180|HTC( Flyer|_Flyer)|Sprint ATP51|ViewPad7|pandigital(sprnova|nova)|Ideos S7|Dell Streak 7|Advent Vega|A101IT|A70BHT|MID7015|Next2|nook/i) || (ua.match(/MB511/i) && ua.match(/RUTEM/i))) {
                        // if user agent is a pre Android 3.0 Tablet
                        return "Tablet";
                    }
                    else if (ua.match(/BOLT|Fennec|Iris|Maemo|Minimo|Mobi|mowser|NetFront|Novarra|Prism|RX-34|Skyfire|Tear|XV6875|XV6975|Google Wireless Transcoder/i) && !ua.match(/AdsBot-Google-Mobile/i)) {
                        // if user agent is unique phone User Agent
                        return "Mobile";
                    }
                    else if (ua.match(/Opera/i) && ua.match(/Windows NT 5/i) && ua.match(/HTC|Xda|Mini|Vario|SAMSUNG\-GT\-i8000|SAMSUNG\-SGH\-i9/i)) {
                        // if user agent is an odd Opera User Agent - http://goo.gl/nK90K
                        return "Mobile";
                    }
                    else if ((ua.match(/Windows( )?(NT|XP|ME|9)/) && !ua.match(/Phone/i)) && !ua.match(/Bot|Spider|ia_archiver|NewsGator/i) || ua.match(/Win( ?9|NT)/i) || ua.match(/Go-http-client/i)) {
                        // if user agent is Windows Desktop
                        return "Desktop";
                    }
                    else if (ua.match(/Macintosh|PowerPC/i) && !ua.match(/Silk|moatbot/i)) {
                        // if agent is Mac Desktop
                        return "Desktop";
                    }
                    else if (ua.match(/Linux/i) && ua.match(/X11/i) && !ua.match(/Charlotte|JobBot/i)) {
                        // if user agent is a Linux Desktop
                        return "Desktop";
                    }
                    else if (ua.match(/CrOS/)) {
                        // if user agent is a Chrome Book
                        return "Desktop";
                    }
                    else if (ua.match(/Solaris|SunOS|BSD/i)) {
                        // if user agent is a Solaris, SunOS, BSD Desktop
                        return "Desktop";
                    }
                    else {
                        // Otherwise returning the unknown type configured
                        return "Unknown";
                    }
                }
            }
            Profile.AnalyticsInfo = AnalyticsInfo;
        })(Profile = System.Profile || (System.Profile = {}));
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var System;
    (function (System) {
        var Profile;
        (function (Profile) {
            class AnalyticsVersionInfo {
                static getUserAgent() {
                    return navigator.userAgent;
                }
                static getBrowserName() {
                    // Opera 8.0+
                    if ((!!window.opr && !!window.opr.addons) || !!window.opera || navigator.userAgent.indexOf(' OPR/') >= 0) {
                        return "Opera";
                    }
                    // Firefox 1.0+
                    if (typeof window.InstallTrigger !== 'undefined') {
                        return "Firefox";
                    }
                    // Safari 3.0+ "[object HTMLElementConstructor]" 
                    if (/constructor/i.test(window.HTMLElement) ||
                        ((p) => p.toString() === "[object SafariRemoteNotification]")(typeof window.safari !== 'undefined' && window.safari.pushNotification)) {
                        return "Safari";
                    }
                    // Edge 20+
                    if (!!window.StyleMedia) {
                        return "Edge";
                    }
                    // Chrome 1 - 71
                    if (!!window.chrome && (!!window.chrome.webstore || !!window.chrome.runtime)) {
                        return "Chrome";
                    }
                }
            }
            Profile.AnalyticsVersionInfo = AnalyticsVersionInfo;
        })(Profile = System.Profile || (System.Profile = {}));
    })(System = Windows.System || (Windows.System = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var UI;
    (function (UI) {
        var Core;
        (function (Core) {
            class SystemNavigationManager {
                constructor() {
                    var that = this;
                    var dispatchBackRequest = globalThis.DotnetExports.Uno.Windows.UI.Core.SystemNavigationManager.DispatchBackRequest;
                    window.history.replaceState(0, document.title, null);
                    window.addEventListener("popstate", function (evt) {
                        if (that._isEnabled) {
                            if (evt.state === 0) {
                                // Push something in the stack only if we know that we reached the first page.
                                // There is no way to track our location in the stack, so we use indexes (in the 'state').
                                window.history.pushState(1, document.title, null);
                            }
                            dispatchBackRequest();
                        }
                        else if (evt.state === 1) {
                            // The manager is disabled, but the user requested to navigate forward to our dummy entry,
                            // but we prefer to keep this dummy entry in the forward stack (is more prompt to be cleared by the browser,
                            // and as it's less commonly used it should be less annoying for the user)
                            window.history.back();
                        }
                    });
                }
                static get current() {
                    if (!this._current) {
                        this._current = new SystemNavigationManager();
                    }
                    return this._current;
                }
                enable() {
                    if (this._isEnabled) {
                        return;
                    }
                    // Clear the back stack, so the only items will be ours (and we won't have any remaining forward item)
                    this.clearStack();
                    window.history.pushState(1, document.title, null);
                    // Then set the enabled flag so the handler will begin its work
                    this._isEnabled = true;
                }
                disable() {
                    if (!this._isEnabled) {
                        return;
                    }
                    // Disable the handler, then clear the history
                    // Note: As a side effect, the forward button will be enabled :(
                    this._isEnabled = false;
                    this.clearStack();
                }
                clearStack() {
                    // There is no way to determine our position in the stack, so we only navigate back if we determine that
                    // we are currently on our dummy target page.
                    if (window.history.state === 1) {
                        window.history.back();
                    }
                    window.history.replaceState(0, document.title, null);
                }
            }
            Core.SystemNavigationManager = SystemNavigationManager;
        })(Core = UI.Core || (UI.Core = {}));
    })(UI = Windows.UI || (Windows.UI = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var UI;
    (function (UI) {
        var Notifications;
        (function (Notifications) {
            class BadgeUpdater {
                static setNumber(value) {
                    if (navigator.setAppBadge) {
                        navigator.setAppBadge(value);
                    }
                }
                static clear() {
                    if (navigator.clearAppBadge) {
                        navigator.clearAppBadge();
                    }
                }
            }
            Notifications.BadgeUpdater = BadgeUpdater;
        })(Notifications = UI.Notifications || (UI.Notifications = {}));
    })(UI = Windows.UI || (Windows.UI = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var UI;
    (function (UI) {
        var ViewManagement;
        (function (ViewManagement) {
            class ApplicationView {
                static setFullScreenMode(turnOn) {
                    if (turnOn) {
                        if (document.fullscreenEnabled) {
                            document.documentElement.requestFullscreen();
                            return true;
                        }
                        else {
                            return false;
                        }
                    }
                    else {
                        document.exitFullscreen();
                        return true;
                    }
                }
                /**
                    * Sets the browser window title
                    * @param message the new title
                    */
                static setWindowTitle(title) {
                    document.title = title /* TODO JELA || UnoAppManifest.displayName */;
                    return "ok";
                }
                /**
                    * Gets the currently set browser window title
                    */
                static getWindowTitle() {
                    return document.title /* TODO JELA || UnoAppManifest.displayName */;
                }
            }
            ViewManagement.ApplicationView = ApplicationView;
        })(ViewManagement = UI.ViewManagement || (UI.ViewManagement = {}));
    })(UI = Windows.UI || (Windows.UI = {}));
})(Windows || (Windows = {}));
var Windows;
(function (Windows) {
    var UI;
    (function (UI) {
        var ViewManagement;
        (function (ViewManagement) {
            class ApplicationViewTitleBar {
                static setBackgroundColor(colorString) {
                    if (colorString == null) {
                        //remove theme-color meta
                        var metaThemeColorEntries = document.querySelectorAll("meta[name='theme-color']");
                        for (let entry of metaThemeColorEntries) {
                            entry.remove();
                        }
                    }
                    else {
                        var metaThemeColorEntries = document.querySelectorAll("meta[name='theme-color']");
                        var metaThemeColor;
                        if (metaThemeColorEntries.length == 0) {
                            //create meta
                            metaThemeColor = document.createElement("meta");
                            metaThemeColor.setAttribute("name", "theme-color");
                            document.head.appendChild(metaThemeColor);
                        }
                        else {
                            metaThemeColor = metaThemeColorEntries[0];
                        }
                        metaThemeColor.setAttribute("content", colorString);
                    }
                }
            }
            ViewManagement.ApplicationViewTitleBar = ApplicationViewTitleBar;
        })(ViewManagement = UI.ViewManagement || (UI.ViewManagement = {}));
    })(UI = Windows.UI || (Windows.UI = {}));
})(Windows || (Windows = {}));
