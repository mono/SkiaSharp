var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class AotProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                    this.attachProfilerHotKey();
                }
                static initialize(context, unoConfig) {
                    if (Bootstrap.Bootstrapper.ENVIRONMENT_IS_WEB && unoConfig.generate_aot_profile) {
                        return new AotProfilerSupport(context, unoConfig);
                    }
                    return null;
                }
                attachProfilerHotKey() {
                    const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                    console.info(`AOT Profiler stop hotkey: Shift+${altKeyName}+P (when application has focus), or Run Uno.WebAssembly.Bootstrap.AotProfilerSupport.saveAotProfile() from the browser debug console.`);
                    document.addEventListener("keydown", (evt) => {
                        if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                            this.saveAotProfile();
                        }
                    });
                }
                async initializeProfile() {
                    let anyContext = this._context;
                    if (anyContext.getAssemblyExports !== undefined) {
                        this._aotProfilerExports = await anyContext.getAssemblyExports("Uno.Wasm.AotProfiler");
                    }
                    else {
                        throw `Unable to find getAssemblyExports`;
                    }
                }
                async saveAotProfile() {
                    await this.initializeProfile();
                    this._aotProfilerExports.Uno.AotProfilerSupport.StopProfile();
                    var a = window.document.createElement('a');
                    var blob = new Blob([this._context.INTERNAL.aotProfileData]);
                    a.href = window.URL.createObjectURL(blob);
                    a.download = "aot.profile";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.AotProfilerSupport = AotProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class EmscriptenMemoryProfilerSupport {
                static initialize(unoConfig) {
                    if (!unoConfig.enable_memory_profiler) {
                        return;
                    }
                    const profiler = globalThis.emscriptenMemoryProfiler;
                    if (!profiler) {
                        console.warn("[MemoryProfiler] enable_memory_profiler is set but globalThis.emscriptenMemoryProfiler was not found. Ensure WasmShellEnableWasmMemoryProfiler is set to true.");
                        return;
                    }
                    profiler.updateUi = function () { };
                    EmscriptenMemoryProfilerSupport.attachHotKey();
                    console.debug("[MemoryProfiler] Emscripten memory profiler bridge activated.");
                }
                static getSnapshotJson() {
                    const profiler = globalThis.emscriptenMemoryProfiler;
                    if (!profiler) {
                        return JSON.stringify({});
                    }
                    const snapshot = {
                        totalMemoryAllocated: profiler.totalMemoryAllocated || 0,
                        totalMemoryUsedByHeap: profiler.totalMemoryUsedByHeap || 0,
                        totalTimesMallocCalled: profiler.totalTimesMallocCalled || 0,
                        totalTimesFreeCalled: profiler.totalTimesFreeCalled || 0,
                        totalTimesReallocCalled: profiler.totalTimesReallocCalled || 0,
                        stackBase: profiler.stackBase || 0,
                        stackTop: profiler.stackTop || 0,
                        stackMax: profiler.stackMax || 0,
                        stackTopWatermark: profiler.stackTopWatermark || 0,
                        sbrkValue: profiler.sbrkValue || 0,
                        allocationSiteCount: Object.keys(profiler.allocationsAtLoc || {}).length,
                        totalStaticMemory: profiler.totalStaticMemory || 0,
                        freeMemory: profiler.freeMemory || 0,
                        pagePreRunIsFinished: profiler.pagePreRunIsFinished || false,
                    };
                    return JSON.stringify(snapshot);
                }
                static attachHotKey() {
                    if (typeof document === "undefined") {
                        return;
                    }
                    console.info("[MemoryProfiler] Export hotkey: Ctrl+Shift+H (speedscope format).\n" +
                        "  For PerfView format, run: Uno.WebAssembly.Bootstrap.EmscriptenMemoryProfilerSupport.downloadSnapshot(\"perfview\")");
                    document.addEventListener("keydown", (evt) => {
                        if (evt.ctrlKey && evt.shiftKey && evt.code === "KeyH") {
                            evt.preventDefault();
                            EmscriptenMemoryProfilerSupport.downloadSnapshot();
                        }
                    });
                }
                static downloadSnapshot(format = "speedscope") {
                    const profiler = globalThis.emscriptenMemoryProfiler;
                    if (!profiler) {
                        console.warn("[MemoryProfiler] Cannot export: profiler not available.");
                        return;
                    }
                    const sites = EmscriptenMemoryProfilerSupport.captureAllocationSites(profiler);
                    let document;
                    let extension;
                    if (format === "perfview") {
                        document = EmscriptenMemoryProfilerSupport.buildPerfViewDocument(sites);
                        extension = ".PerfView.json";
                    }
                    else {
                        document = EmscriptenMemoryProfilerSupport.buildSpeedscopeDocument(sites);
                        extension = ".speedscope.json";
                    }
                    const json = JSON.stringify(document, null, 2);
                    const blob = new Blob([json], { type: "application/json" });
                    const timestamp = new Date().toISOString().replace(/[:.]/g, "-");
                    const filename = `memory-profile-${timestamp}${extension}`;
                    const a = window.document.createElement("a");
                    a.href = window.URL.createObjectURL(blob);
                    a.download = filename;
                    window.document.body.appendChild(a);
                    a.click();
                    window.document.body.removeChild(a);
                    window.URL.revokeObjectURL(a.href);
                    console.info(`[MemoryProfiler] Exported ${filename} (${format} format)`);
                }
                static buildSpeedscopeDocument(sites) {
                    const frameMap = new Map();
                    const frames = [];
                    for (const site of sites) {
                        for (const frame of site.stackFrames) {
                            if (!frameMap.has(frame.functionName)) {
                                frameMap.set(frame.functionName, frames.length);
                                frames.push({ name: frame.functionName });
                            }
                        }
                    }
                    const samples = [];
                    const weights = [];
                    let totalBytes = 0;
                    for (const site of sites) {
                        const indices = site.stackFrames
                            .map(f => frameMap.get(f.functionName))
                            .reverse();
                        samples.push(indices);
                        weights.push(site.outstandingBytes);
                        totalBytes += site.outstandingBytes;
                    }
                    return {
                        "$schema": "https://www.speedscope.app/file-format-schema.json",
                        shared: {
                            frames: frames,
                        },
                        profiles: [{
                                type: "sampled",
                                name: "Native Memory Allocations",
                                unit: "bytes",
                                startValue: 0,
                                endValue: totalBytes,
                                samples: samples,
                                weights: weights,
                            }],
                    };
                }
                static buildPerfViewDocument(sites) {
                    const perfSamples = [];
                    for (const site of sites) {
                        perfSamples.push({
                            Stack: site.stackFrames.map(f => f.functionName),
                            Metric: site.outstandingBytes,
                        });
                    }
                    return {
                        Samples: perfSamples,
                    };
                }
                static captureAllocationSites(profiler) {
                    const sites = [];
                    const allocationsAtLoc = profiler.allocationsAtLoc;
                    if (!allocationsAtLoc)
                        return sites;
                    for (const rawStack in allocationsAtLoc) {
                        if (!allocationsAtLoc.hasOwnProperty(rawStack))
                            continue;
                        const entry = allocationsAtLoc[rawStack];
                        const count = entry[0];
                        const bytes = entry[1];
                        if (count === 0)
                            continue;
                        const frames = EmscriptenMemoryProfilerSupport.parseStackTrace(rawStack);
                        sites.push({
                            callSiteKey: EmscriptenMemoryProfilerSupport.deriveCallSiteKey(frames),
                            outstandingCount: count,
                            outstandingBytes: bytes,
                            stackFrames: frames,
                        });
                    }
                    sites.sort((a, b) => b.outstandingBytes - a.outstandingBytes);
                    return sites;
                }
                static parseStackTrace(rawStack) {
                    const lines = rawStack.split("\n");
                    const frames = [];
                    for (const line of lines) {
                        const trimmed = line.trim();
                        if (!trimmed.startsWith("at "))
                            continue;
                        const wasmMatch = trimmed.match(/^at\s+(\S+)\s+\(.*?:wasm-function\[(\d+)\]:(\S+)\)/);
                        if (wasmMatch) {
                            let funcName = wasmMatch[1];
                            if (funcName.startsWith("dotnet.native.wasm.")) {
                                funcName = funcName.substring("dotnet.native.wasm.".length);
                            }
                            frames.push({
                                functionName: funcName,
                                wasmFunction: parseInt(wasmMatch[2], 10),
                                offset: wasmMatch[3],
                            });
                        }
                        else {
                            const simpleMatch = trimmed.match(/^at\s+(\S+)/);
                            let funcName = simpleMatch ? simpleMatch[1] : trimmed;
                            if (funcName.startsWith("dotnet.native.wasm.")) {
                                funcName = funcName.substring("dotnet.native.wasm.".length);
                            }
                            frames.push({
                                functionName: funcName,
                                wasmFunction: -1,
                                offset: "",
                            });
                        }
                    }
                    return frames;
                }
                static deriveCallSiteKey(frames) {
                    for (const f of frames) {
                        if (!EmscriptenMemoryProfilerSupport.ALLOCATOR_FUNCTIONS.has(f.functionName)) {
                            return f.functionName;
                        }
                    }
                    return frames.length > 0 ? frames[0].functionName : "unknown";
                }
            }
            EmscriptenMemoryProfilerSupport.ALLOCATOR_FUNCTIONS = new Set([
                "dlmalloc",
                "internal_memalign",
                "dlcalloc",
                "dlposix_memalign",
                "monoeg_malloc",
                "monoeg_g_calloc",
                "monoeg_malloc0",
            ]);
            Bootstrap.EmscriptenMemoryProfilerSupport = EmscriptenMemoryProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class HotReloadSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                }
                static async tryInitializeExports(getAssemblyExports) {
                    let exports = await getAssemblyExports("Uno.Wasm.MetadataUpdater");
                    this._getApplyUpdateCapabilitiesMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.GetApplyUpdateCapabilities;
                    this._applyHotReloadDeltaMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.ApplyHotReloadDelta;
                    this._initializeMethod = exports.Uno.Wasm.MetadataUpdate.WebAssemblyHotReload.Initialize;
                }
                async initializeHotReload() {
                    const webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                    let browserToolsVariable = this._context.config.environmentVariables['ASPNETCORE-BROWSER-TOOLS']
                        || this._context.config.environmentVariables['__ASPNETCORE_BROWSER_TOOLS'];
                    (function (Blazor) {
                        Blazor._internal = {
                            initialize: function () {
                                if (!HotReloadSupport._initializeMethod()) {
                                    console.warn("The application was compiled with the IL linker enabled, hot reload is disabled. See https://aka.platform.uno/wasm-il-linker for more details.");
                                }
                            },
                            applyExisting: async function () {
                                if (browserToolsVariable == "true") {
                                    try {
                                        var m = await import(`/_framework/blazor-hotreload.js`);
                                        await m.receiveHotReloadAsync();
                                    }
                                    catch (e) {
                                        console.error(`Failed to apply initial metadata delta ${e}`);
                                    }
                                }
                            },
                            getApplyUpdateCapabilities: function () {
                                Blazor._internal.initialize();
                                return HotReloadSupport._getApplyUpdateCapabilitiesMethod();
                            },
                            applyHotReload: function (moduleId, metadataDelta, ilDelta, pdbDelta, updatedTypes) {
                                Blazor._internal.initialize();
                                return HotReloadSupport._applyHotReloadDeltaMethod(moduleId, metadataDelta, ilDelta, pdbDelta || "", updatedTypes || []);
                            }
                        };
                    })(window.Blazor || (window.Blazor = {}));
                    window.Blazor._internal.initialize();
                    await window.Blazor._internal.applyExisting();
                }
            }
            Bootstrap.HotReloadSupport = HotReloadSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class Bootstrapper {
                constructor(unoConfig) {
                    this._unoConfig = unoConfig;
                    this._webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                    this._appBase = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_APP_BASE"];
                    this._previousTotalResources = 0;
                    this._currentTargetProgress = Bootstrapper.MINIMUM_INITIAL_TARGET;
                    this._lastProgressTimestamp = 0;
                    this._progressHistory = [];
                    this._estimatedFinalTotal = 0;
                    this._lastReportedValue = 0;
                    this.disableDotnet6Compatibility = false;
                    this.onConfigLoaded = config => this.configLoaded(config);
                    this.onDotnetReady = () => this.RuntimeReady();
                    globalThis.Uno = Uno;
                }
                static invokeJS(value) {
                    return eval(value);
                }
                static async bootstrap() {
                    try {
                        Bootstrapper.ENVIRONMENT_IS_WEB = typeof window === 'object';
                        Bootstrapper.ENVIRONMENT_IS_WORKER = typeof globalThis.importScripts === 'function';
                        Bootstrapper.ENVIRONMENT_IS_NODE = typeof globalThis.process === 'object' && typeof globalThis.process.versions === 'object' && typeof globalThis.process.versions.node === 'string';
                        Bootstrapper.ENVIRONMENT_IS_SHELL = !Bootstrapper.ENVIRONMENT_IS_WEB && !Bootstrapper.ENVIRONMENT_IS_NODE && !Bootstrapper.ENVIRONMENT_IS_WORKER;
                        let bootstrapper = null;
                        let DOMContentLoaded = false;
                        if (typeof window === 'object') {
                            globalThis.document.addEventListener("DOMContentLoaded", () => {
                                DOMContentLoaded = true;
                                bootstrapper?.preInit();
                            });
                        }
                        var config = await import('./uno-config.js');
                        if (document && document.uno_app_base_override) {
                            config.config.uno_app_base = document.uno_app_base_override;
                        }
                        bootstrapper = new Bootstrapper(config.config);
                        if (DOMContentLoaded) {
                            bootstrapper.preInit();
                        }
                        var m = await import(`../_framework/${config.config.dotnet_js_filename}`);
                        m.dotnet
                            .withModuleConfig({
                            preRun: [(module) => bootstrapper.wasmRuntimePreRun(module)],
                        })
                            .withRuntimeOptions(config.config.uno_runtime_options)
                            .withConfig({ loadAllSatelliteResources: config.config.uno_load_all_satellite_resources });
                        const dotnetRuntime = await m.default((context) => {
                            bootstrapper.configure(context);
                            return bootstrapper.asDotnetConfig();
                        });
                        bootstrapper._context.Module = dotnetRuntime.Module;
                        globalThis.Module = bootstrapper._context.Module;
                        bootstrapper._runMain = dotnetRuntime.runMain;
                        bootstrapper.setupExports(dotnetRuntime);
                    }
                    catch (e) {
                        throw `.NET runtime initialization failed (${e})`;
                    }
                }
                setupExports(dotnetRuntime) {
                    this._getAssemblyExports = dotnetRuntime.getAssemblyExports;
                    this._context.Module.getAssemblyExports = dotnetRuntime.getAssemblyExports;
                    globalThis.Module.getAssemblyExports = dotnetRuntime.getAssemblyExports;
                }
                asDotnetConfig() {
                    return {
                        disableDotnet6Compatibility: this.disableDotnet6Compatibility,
                        configSrc: this.configSrc,
                        baseUrl: this._unoConfig.uno_app_base,
                        mainScriptPath: `_framework/${this._unoConfig.dotnet_js_filename}`,
                        onConfigLoaded: this.onConfigLoaded,
                        onDotnetReady: this.onDotnetReady,
                        onAbort: this.onAbort,
                        exports: ["IDBFS", "FS"].concat(this._unoConfig.emcc_exported_runtime_methods),
                        onDownloadResourceProgress: (resourcesLoaded, totalResources) => this.reportDownloadResourceProgress(resourcesLoaded, totalResources),
                    };
                }
                configure(context) {
                    this._context = context;
                    globalThis.BINDING = this._context.BINDING;
                }
                async setupHotReload() {
                    if (Bootstrapper.ENVIRONMENT_IS_WEB && this.hasDebuggingEnabled()) {
                        await Bootstrap.HotReloadSupport.tryInitializeExports(this._getAssemblyExports);
                        this._hotReloadSupport = new Bootstrap.HotReloadSupport(this._context, this._unoConfig);
                    }
                }
                setupRequire() {
                    const anyModule = this._context.Module;
                    anyModule.imports = anyModule.imports || {};
                    anyModule.imports.require = globalThis.require;
                }
                wasmRuntimePreRun(module) {
                    if (this._unoConfig.uno_vfs_framework_assembly_load && module?.FS) {
                        try {
                            module.FS.mkdir("/managed");
                        }
                        catch (e) {
                        }
                        if (this._unoConfig.uno_vfs_framework_assembly_load_cleanup) {
                            this.setupVfsCleanupOnClose(module.FS);
                        }
                    }
                    if (Bootstrap.LogProfilerSupport.initializeLogProfiler(this._unoConfig)) {
                        this._logProfiler = new Bootstrap.LogProfilerSupport(this._context, this._unoConfig);
                    }
                }
                setupVfsCleanupOnClose(fs) {
                    const vfsPrefix = "/managed/";
                    const originalClose = fs.close.bind(fs);
                    fs.close = (stream) => {
                        const path = stream?.path;
                        const flags = stream?.flags ?? -1;
                        originalClose(stream);
                        const isReadOnly = flags >= 0 && (flags & 3) === 0;
                        if (isReadOnly && path && path.startsWith(vfsPrefix)) {
                            try {
                                fs.unlink(path);
                                if (this._monoConfig?.debugLevel && this._monoConfig.debugLevel > 0) {
                                    console.log(`[Bootstrap] VFS cleanup: deleted ${path}`);
                                }
                            }
                            catch (_e) {
                            }
                        }
                    };
                }
                RuntimeReady() {
                    this.configureGlobal();
                    this.setupRequire();
                    this.initializeRequire();
                    this._aotProfiler = Bootstrap.AotProfilerSupport.initialize(this._context, this._unoConfig);
                    Bootstrap.EmscriptenMemoryProfilerSupport.initialize(this._unoConfig);
                }
                configureGlobal() {
                    var thatGlobal = globalThis;
                    thatGlobal.config = this._unoConfig;
                    thatGlobal.Module = this._context.Module;
                    let anyModule = this._context.Module;
                    thatGlobal.lengthBytesUTF8 = anyModule.lengthBytesUTF8;
                    thatGlobal.UTF8ToString = anyModule.UTF8ToString;
                    thatGlobal.UTF8ArrayToString = anyModule.UTF8ArrayToString;
                    thatGlobal.IDBFS = anyModule.IDBFS;
                    thatGlobal.FS = anyModule.FS;
                    if (this._unoConfig.emcc_exported_runtime_methods) {
                        this._unoConfig.emcc_exported_runtime_methods.forEach((name) => {
                            thatGlobal[name] = anyModule[name];
                        });
                    }
                }
                configLoaded(config) {
                    this._monoConfig = config;
                    if (this._unoConfig.environmentVariables) {
                        for (let key in this._unoConfig.environmentVariables) {
                            if (this._unoConfig.environmentVariables.hasOwnProperty(key)) {
                                if (this._monoConfig.debugLevel)
                                    console.log(`Setting ${key}=${this._unoConfig.environmentVariables[key]}`);
                                this._monoConfig.environmentVariables[key] = this._unoConfig.environmentVariables[key];
                            }
                        }
                    }
                    if (this._unoConfig.generate_aot_profile) {
                        this._monoConfig.aotProfilerOptions = {
                            writeAt: "Uno.AotProfilerSupport::StopProfile",
                            sendTo: "System.Runtime.InteropServices.JavaScript.JavaScriptExports::DumpAotProfileData"
                        };
                    }
                    var logProfilerConfig = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_LOG_PROFILER_OPTIONS"];
                    if (logProfilerConfig) {
                        this._monoConfig.logProfilerOptions = {
                            configuration: logProfilerConfig
                        };
                    }
                    var browserProfilerInterval = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_BROWSER_PROFILER_SAMPLE_INTERVAL"];
                    if (browserProfilerInterval) {
                        this._monoConfig.browserProfilerOptions = {
                            sampleIntervalMs: parseInt(browserProfilerInterval)
                        };
                    }
                    if (this._unoConfig.uno_vfs_framework_assembly_load) {
                        this.redirectAssembliesToVfs(config);
                    }
                    const res = config.resources;
                    if (res?.satelliteResources) {
                        const vfsManagedDir = "/managed";
                        const moveArrayToVfs = (source, vfsDir, namePrefix) => {
                            if (!source)
                                return;
                            for (const entry of source) {
                                const vfsEntry = { ...entry };
                                if (namePrefix) {
                                    vfsEntry.name = namePrefix + "/" + vfsEntry.name;
                                }
                                vfsEntry.virtualPath = vfsDir + "/" + (entry.virtualPath || entry.name);
                                res.vfs = res.vfs || [];
                                res.vfs.push(vfsEntry);
                            }
                        };
                        for (const culture in res.satelliteResources) {
                            moveArrayToVfs(res.satelliteResources[culture], vfsManagedDir + "/" + culture, culture);
                        }
                    }
                    this.initializeProgressEstimation();
                }
                redirectAssembliesToVfs(config) {
                    const vfsManagedDir = "/managed";
                    if (!config.resources) {
                        return;
                    }
                    const res = config.resources;
                    if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                        console.log("[Bootstrap] Redirecting assembly loading to VFS-based loading for image cache deduplication");
                    }
                    config.environmentVariables = config.environmentVariables || {};
                    config.environmentVariables["MONO_PATH"] = vfsManagedDir;
                    const usesArrayResources = Array.isArray(res.assembly) ||
                        Array.isArray(res.coreAssembly);
                    if (usesArrayResources && !Array.isArray(res.vfs)) {
                        res.vfs = [];
                    }
                    const mainAssemblyName = config.mainAssemblyName;
                    if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                        const asmBefore = Array.isArray(res.assembly) ? res.assembly.length : typeof res.assembly;
                        const coreBefore = Array.isArray(res.coreAssembly) ? res.coreAssembly.length : typeof res.coreAssembly;
                        console.log(`[Bootstrap] VFS redirect: pre-processing state` +
                            ` (assembly: ${asmBefore}` +
                            `, coreAssembly: ${coreBefore}` +
                            `, mainAssemblyName: ${mainAssemblyName}` +
                            `, resourceKeys: ${Object.keys(res).join(",")})`);
                    }
                    const bundledAssemblies = new Set([
                        "System.Runtime.InteropServices.JavaScript",
                        "System.Private.CoreLib",
                    ]);
                    const assemblyNameOf = (entry) => {
                        const vp = entry.virtualPath || entry.name || "";
                        return vp.replace(/\.(wasm|dll)$/, "");
                    };
                    const mustKeepBundled = (entry) => {
                        const name = assemblyNameOf(entry);
                        return name === mainAssemblyName || bundledAssemblies.has(name);
                    };
                    const moveArrayToVfs = (source, vfsDir, keepPredicate, namePrefix) => {
                        if (!Array.isArray(res.vfs)) {
                            if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                                console.warn(`[Bootstrap] VFS redirect: skipping transformation because resources.vfs is not array-based`);
                            }
                            return undefined;
                        }
                        if (!Array.isArray(source)) {
                            if (source && this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                                console.warn(`[Bootstrap] VFS redirect: skipping non-array resource section` +
                                    ` (type: ${typeof source})`);
                            }
                            return undefined;
                        }
                        const kept = [];
                        for (const entry of source) {
                            if (keepPredicate && keepPredicate(entry)) {
                                kept.push(entry);
                            }
                            else {
                                const vfsEntry = { ...entry };
                                if (namePrefix) {
                                    vfsEntry.name = namePrefix + "/" + vfsEntry.name;
                                }
                                const originalVirtualPath = entry.virtualPath || entry.name;
                                const vfsFileName = originalVirtualPath.replace(/\.wasm$/, ".dll");
                                vfsEntry.virtualPath = vfsDir + "/" + vfsFileName;
                                res.vfs.push(vfsEntry);
                            }
                        }
                        return kept;
                    };
                    const newAssembly = moveArrayToVfs(res.assembly, vfsManagedDir, mustKeepBundled);
                    if (newAssembly !== undefined) {
                        res.assembly = newAssembly;
                    }
                    const newCoreAssembly = moveArrayToVfs(res.coreAssembly, vfsManagedDir, mustKeepBundled);
                    if (newCoreAssembly !== undefined) {
                        res.coreAssembly = newCoreAssembly;
                    }
                    const newPdb = moveArrayToVfs(res.pdb, vfsManagedDir);
                    if (newPdb !== undefined) {
                        res.pdb = newPdb;
                    }
                    const newCorePdb = moveArrayToVfs(res.corePdb, vfsManagedDir);
                    if (newCorePdb !== undefined) {
                        res.corePdb = newCorePdb;
                    }
                    if (res.satelliteResources) {
                        for (const culture in res.satelliteResources) {
                            if (res.satelliteResources.hasOwnProperty(culture)) {
                                const newSat = moveArrayToVfs(res.satelliteResources[culture], vfsManagedDir + "/" + culture, undefined, culture);
                                if (newSat !== undefined) {
                                    res.satelliteResources[culture] = newSat;
                                }
                            }
                        }
                    }
                    if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                        const vfsCount = Array.isArray(res.vfs) ? res.vfs.length : 0;
                        const asmIsArray = Array.isArray(res.assembly);
                        const coreIsArray = Array.isArray(res.coreAssembly);
                        console.log(`[Bootstrap] VFS redirect: ${vfsCount} entries moved to ${vfsManagedDir}` +
                            ` (assembly: ${asmIsArray ? res.assembly.length : typeof res.assembly}` +
                            `, coreAssembly: ${coreIsArray ? res.coreAssembly.length : typeof res.coreAssembly}` +
                            `, vfs: ${Array.isArray(res.vfs) ? "array" : typeof res.vfs}` +
                            `, mainAssemblyName: ${config.mainAssemblyName})`);
                    }
                }
                preInit() {
                    this.body = document.getElementById("uno-body");
                    this.initProgress();
                }
                async mainInit() {
                    try {
                        this.attachDebuggerHotkey();
                        await this.setupHotReload();
                        if (this._hotReloadSupport) {
                            await this._hotReloadSupport.initializeHotReload();
                        }
                        this._runMain(this._unoConfig.uno_main, []);
                        this.initializePWA();
                    }
                    catch (e) {
                        console.error(e);
                    }
                    this.cleanupInit();
                }
                cleanupInit() {
                    if (this.progress) {
                        this.progress.value = this.progress.max;
                    }
                    if (!this.bodyObserver && this.loader && this.loader.parentNode) {
                        this.loader.parentNode.removeChild(this.loader);
                    }
                }
                initializeProgressEstimation() {
                    if (this._monoConfig && this._monoConfig.assets && Array.isArray(this._monoConfig.assets)) {
                        const assets = this._monoConfig.assets;
                        let assemblyCount = 0;
                        let resourceCount = 0;
                        let pdbCount = 0;
                        let otherCount = 0;
                        for (const asset of assets) {
                            switch (asset.behavior) {
                                case 'assembly':
                                    assemblyCount++;
                                    break;
                                case 'resource':
                                    resourceCount++;
                                    break;
                                case 'pdb':
                                    pdbCount++;
                                    break;
                                default:
                                    otherCount++;
                            }
                        }
                        this._estimatedFinalTotal = Math.floor(assemblyCount * Bootstrapper.ASSEMBLY_DEPENDENCY_MULTIPLIER +
                            resourceCount +
                            pdbCount +
                            otherCount);
                        const initialTargetFromEstimate = this._estimatedFinalTotal * 0.2;
                        this._currentTargetProgress = Math.max(Bootstrapper.MINIMUM_INITIAL_TARGET, Math.min(initialTargetFromEstimate, 50));
                        if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                            console.log(`Progress estimation: ${assets.length} initial assets ` +
                                `(assemblies: ${assemblyCount}, resources: ${resourceCount}), ` +
                                `estimated final: ${this._estimatedFinalTotal}, ` +
                                `initial target: ${this._currentTargetProgress.toFixed(1)}%`);
                        }
                    }
                    else {
                        this._estimatedFinalTotal = 100;
                        this._currentTargetProgress = Bootstrapper.MINIMUM_INITIAL_TARGET;
                    }
                    this._previousTotalResources = 0;
                    this._lastProgressTimestamp = Date.now();
                    this._progressHistory = [];
                    this._lastReportedValue = 0;
                }
                reportDownloadResourceProgress(resourcesLoaded, totalResources) {
                    this.progress.max = 100;
                    const now = Date.now();
                    this._progressHistory.push({
                        time: now,
                        loaded: resourcesLoaded,
                        total: totalResources
                    });
                    const historyWindow = Bootstrapper.VELOCITY_WINDOW_MS * 3;
                    this._progressHistory = this._progressHistory.filter(entry => now - entry.time < historyWindow);
                    if (this._previousTotalResources !== totalResources) {
                        if (totalResources > this._estimatedFinalTotal) {
                            this._estimatedFinalTotal = totalResources * 1.1;
                        }
                        const remainingToReserve = Bootstrapper.FINAL_RESERVE_PERCENTAGE * 100 - this._currentTargetProgress;
                        this._currentTargetProgress = this._currentTargetProgress +
                            remainingToReserve * Bootstrapper.CONVERGENCE_RATE;
                        this._previousTotalResources = totalResources;
                        if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                            console.log(`Total increased to ${totalResources}, ` +
                                `target advanced to ${this._currentTargetProgress.toFixed(1)}%`);
                        }
                    }
                    let velocityAdjustment = 0;
                    if (this._progressHistory.length >= Bootstrapper.MIN_VELOCITY_SAMPLES) {
                        const oldest = this._progressHistory[0];
                        const timeDelta = now - oldest.time;
                        const loadedDelta = resourcesLoaded - oldest.loaded;
                        if (timeDelta > 0 && loadedDelta > 0) {
                            const loadRate = (loadedDelta / timeDelta) * 1000;
                            const currentCompletionRatio = resourcesLoaded / Math.max(totalResources, 1);
                            const targetCompletionRatio = this._currentTargetProgress / 100;
                            if (currentCompletionRatio >= targetCompletionRatio * 0.9) {
                                const advanceAmount = Math.min(Bootstrapper.VELOCITY_EXTRAPOLATION_CAP * 100, (100 - this._currentTargetProgress) * 0.1);
                                velocityAdjustment = advanceAmount;
                                if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0 && velocityAdjustment > 0) {
                                    console.log(`Velocity adjustment: +${velocityAdjustment.toFixed(1)}%, ` +
                                        `load rate: ${loadRate.toFixed(2)} res/s`);
                                }
                            }
                        }
                    }
                    const timeSinceLastUpdate = now - this._lastProgressTimestamp;
                    if (timeSinceLastUpdate > Bootstrapper.STALL_THRESHOLD_MS) {
                        const stallAdvancement = Math.min(2, (Bootstrapper.FINAL_RESERVE_PERCENTAGE * 100 - this._currentTargetProgress) * 0.05);
                        velocityAdjustment = Math.max(velocityAdjustment, stallAdvancement);
                    }
                    if (velocityAdjustment > 0) {
                        this._currentTargetProgress = Math.min(this._currentTargetProgress + velocityAdjustment, Bootstrapper.FINAL_RESERVE_PERCENTAGE * 100);
                    }
                    const completionRatio = resourcesLoaded / Math.max(totalResources, 1);
                    const scaledProgress = completionRatio * this._currentTargetProgress;
                    const newValue = Math.max(this._lastReportedValue, Math.min(scaledProgress, this._currentTargetProgress));
                    this.progress.value = newValue;
                    this._lastReportedValue = newValue;
                    this._lastProgressTimestamp = now;
                    if (this._monoConfig.debugLevel && this._monoConfig.debugLevel > 0) {
                        console.log(`Progress: ${newValue.toFixed(1)}% ` +
                            `(${resourcesLoaded}/${totalResources} resources, ` +
                            `target: ${this._currentTargetProgress.toFixed(1)}%)`);
                    }
                }
                initProgress() {
                    this.loader = this.body.querySelector(".uno-loader");
                    if (this.loader) {
                        this.loader.id = "loading";
                        const progress = this.loader.querySelector("progress");
                        progress.value = "";
                        this.progress = progress;
                        this.bodyObserver = new MutationObserver(() => {
                            if (!this.loader.classList.contains("uno-keep-loader")) {
                                this.loader.remove();
                            }
                            if (this.bodyObserver) {
                                this.bodyObserver.disconnect();
                                this.bodyObserver = null;
                            }
                        });
                        this.bodyObserver.observe(this.body, { childList: true });
                        this.loader.classList.add("uno-persistent-loader");
                    }
                    const configLoader = () => {
                        if (manifest && manifest.lightThemeBackgroundColor) {
                            this.loader.style.setProperty("--light-theme-bg-color", manifest.lightThemeBackgroundColor);
                        }
                        if (manifest && manifest.darkThemeBackgroundColor) {
                            this.loader.style.setProperty("--dark-theme-bg-color", manifest.darkThemeBackgroundColor);
                        }
                        if (manifest && manifest.splashScreenColor && manifest.splashScreenColor != "transparent") {
                            this.loader.style.setProperty("background-color", manifest.splashScreenColor);
                        }
                        if (manifest && manifest.accentColor) {
                            this.loader.style.setProperty("--accent-color", manifest.accentColor);
                        }
                        if (manifest && manifest.lightThemeAccentColor) {
                            this.loader.style.setProperty("--accent-color", manifest.lightThemeAccentColor);
                        }
                        if (manifest && manifest.darkThemeAccentColor) {
                            this.loader.style.setProperty("--dark-theme-accent-color", manifest.darkThemeAccentColor);
                        }
                        const img = this.loader.querySelector("img");
                        if (img) {
                            if (manifest && manifest.splashScreenImage) {
                                if (!manifest.splashScreenImage.match(/^(http(s)?:\/\/.)/g)) {
                                    manifest.splashScreenImage = `${this._unoConfig.uno_app_base}/${manifest.splashScreenImage}`;
                                }
                                img.setAttribute("src", manifest.splashScreenImage);
                            }
                            else {
                                img.setAttribute("src", "https://uno-assets.platform.uno/logos/uno-splashscreen-light.png");
                            }
                        }
                    };
                    let manifest = window["UnoAppManifest"];
                    if (manifest) {
                        configLoader();
                    }
                    else {
                        for (var i = 0; i < this._unoConfig.uno_dependencies.length; i++) {
                            if (this._unoConfig.uno_dependencies[i].endsWith('AppManifest')
                                || this._unoConfig.uno_dependencies[i].endsWith('AppManifest.js')) {
                                require([this._unoConfig.uno_dependencies[i]], function () {
                                    manifest = window["UnoAppManifest"];
                                    configLoader();
                                });
                                break;
                            }
                        }
                    }
                }
                isElectron() {
                    return navigator.userAgent.indexOf('Electron') !== -1;
                }
                initializeRequire() {
                    this._isUsingCommonJS = this._unoConfig.uno_shell_mode !== "BrowserEmbedded" && (Bootstrapper.ENVIRONMENT_IS_NODE || this.isElectron());
                    if (this._unoConfig.uno_enable_tracing)
                        console.log("Done loading the BCL");
                    if (this._unoConfig.uno_dependencies && this._unoConfig.uno_dependencies.length !== 0) {
                        let pending = this._unoConfig.uno_dependencies.length;
                        const checkDone = (dependency) => {
                            --pending;
                            if (this._unoConfig.uno_enable_tracing)
                                console.log(`Loaded dependency (${dependency}) - remains ${pending} other(s).`);
                            if (pending === 0) {
                                this.mainInit();
                            }
                        };
                        this._unoConfig.uno_dependencies.forEach((dependency) => {
                            if (this._unoConfig.uno_enable_tracing)
                                console.log(`Loading dependency (${dependency})`);
                            let processDependency = (instance) => {
                                if (instance && instance.HEAP8 !== undefined) {
                                    const existingInitializer = instance.onRuntimeInitialized;
                                    if (this._unoConfig.uno_enable_tracing)
                                        console.log(`Waiting for dependency (${dependency}) initialization`);
                                    instance.onRuntimeInitialized = () => {
                                        checkDone(dependency);
                                        if (existingInitializer)
                                            existingInitializer();
                                    };
                                }
                                else {
                                    checkDone(dependency);
                                }
                            };
                            this.require([dependency], processDependency);
                        });
                    }
                    else {
                        setTimeout(() => {
                            this.mainInit();
                        }, 0);
                    }
                }
                require(modules, callback) {
                    if (this._isUsingCommonJS) {
                        modules.forEach(id => {
                            setTimeout(() => {
                                const d = require('./' + id);
                                callback(d);
                            }, 0);
                        });
                    }
                    else {
                        if (typeof require === undefined) {
                            throw `Require.js has not been loaded yet. If you have customized your index.html file, make sure that <script src="./require.js"></script> does not contain the defer directive.`;
                        }
                        require(modules, callback);
                    }
                }
                hasDebuggingEnabled() {
                    return this._unoConfig.uno_debugging_enabled && this._currentBrowserIsChrome;
                }
                attachDebuggerHotkey() {
                    if (Bootstrapper.ENVIRONMENT_IS_WEB) {
                        let loadAssemblyUrls = this._monoConfig.assets.map(a => a.name);
                        this._currentBrowserIsChrome = window.chrome
                            && navigator.userAgent.indexOf("Edge") < 0;
                        this._hasReferencedPdbs = loadAssemblyUrls
                            .some(function (url) { return /\.pdb$/.test(url); });
                        const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                        if (this.hasDebuggingEnabled()) {
                            console.info(`Debugging hotkey: Shift+${altKeyName}+D (when application has focus)`);
                        }
                        document.addEventListener("keydown", (evt) => {
                            if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyD") {
                                if (!this._hasReferencedPdbs) {
                                    console.error("Cannot start debugging, because the application was not compiled with debugging enabled.");
                                }
                                else if (!this._currentBrowserIsChrome) {
                                    console.error("Currently, only Chrome is supported for debugging.");
                                }
                                else {
                                    this.launchDebugger();
                                }
                            }
                        });
                    }
                }
                launchDebugger() {
                    const link = document.createElement("a");
                    link.href = `_framework/debug?url=${encodeURIComponent(location.href)}`;
                    link.target = "_blank";
                    link.rel = "noopener noreferrer";
                    link.click();
                }
                initializePWA() {
                    if (typeof window === 'object') {
                        if (this._unoConfig.enable_pwa && 'serviceWorker' in navigator) {
                            if (navigator.serviceWorker.controller) {
                                console.debug("Active service worker found, skipping register");
                            }
                            else {
                                const _webAppBasePath = this._unoConfig.environmentVariables["UNO_BOOTSTRAP_WEBAPP_BASE_PATH"];
                                console.debug(`Registering service worker for ${_webAppBasePath}`);
                                navigator.serviceWorker
                                    .register(`${_webAppBasePath}service-worker.js`, {
                                    scope: _webAppBasePath,
                                    type: 'module'
                                })
                                    .then(function () {
                                    console.debug('Service Worker Registered');
                                });
                            }
                        }
                    }
                }
            }
            Bootstrapper.MINIMUM_INITIAL_TARGET = 30;
            Bootstrapper.INITIAL_TARGET_PERCENTAGE = 0.3;
            Bootstrapper.CONVERGENCE_RATE = 0.5;
            Bootstrapper.VELOCITY_WINDOW_MS = 500;
            Bootstrapper.MIN_VELOCITY_SAMPLES = 2;
            Bootstrapper.VELOCITY_EXTRAPOLATION_CAP = 0.1;
            Bootstrapper.STALL_THRESHOLD_MS = 1000;
            Bootstrapper.FINAL_RESERVE_PERCENTAGE = 0.95;
            Bootstrapper.ASSEMBLY_DEPENDENCY_MULTIPLIER = 1.5;
            Bootstrap.Bootstrapper = Bootstrapper;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
Uno.WebAssembly.Bootstrap.Bootstrapper.bootstrap();
var Uno;
(function (Uno) {
    var WebAssembly;
    (function (WebAssembly) {
        var Bootstrap;
        (function (Bootstrap) {
            class LogProfilerSupport {
                constructor(context, unoConfig) {
                    this._context = context;
                    this._unoConfig = unoConfig;
                }
                static initializeLogProfiler(unoConfig) {
                    const options = unoConfig.environmentVariables["UNO_BOOTSTRAP_LOG_PROFILER_OPTIONS"];
                    if (options) {
                        this._logProfilerEnabled = true;
                        return true;
                    }
                    return false;
                }
                postInitializeLogProfiler() {
                    if (LogProfilerSupport._logProfilerEnabled) {
                        this.attachHotKey();
                        setInterval(() => {
                            this.ensureInitializeProfilerMethods();
                            this._flushLogProfile();
                        }, 5000);
                    }
                }
                attachHotKey() {
                    if (Bootstrap.Bootstrapper.ENVIRONMENT_IS_WEB) {
                        if (LogProfilerSupport._logProfilerEnabled) {
                            const altKeyName = navigator.platform.match(/^Mac/i) ? "Cmd" : "Alt";
                            console.info(`Log Profiler save hotkey: Shift+${altKeyName}+P (when application has focus), or Run this.saveLogProfile() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyP") {
                                    this.saveLogProfile();
                                }
                            });
                            console.info(`Log Profiler take heap shot hotkey: Shift+${altKeyName}+H (when application has focus), or Run this.takeHeapShot() from the browser debug console.`);
                            document.addEventListener("keydown", (evt) => {
                                if (evt.shiftKey && (evt.metaKey || evt.altKey) && evt.code === "KeyH") {
                                    this.takeHeapShot();
                                }
                            });
                        }
                    }
                }
                ensureInitializeProfilerMethods() {
                    if (LogProfilerSupport._logProfilerEnabled && !this._flushLogProfile) {
                        this._flushLogProfile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:FlushProfile");
                        this._getLogProfilerProfileOutputFile = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:GetProfilerProfileOutputFile");
                        this.triggerHeapShotLogProfiler = this._context.BINDING.bind_static_method("[Uno.Wasm.LogProfiler] Uno.LogProfilerSupport:TriggerHeapShot");
                    }
                }
                takeHeapShot() {
                    this.ensureInitializeProfilerMethods();
                    this.triggerHeapShotLogProfiler();
                }
                readProfileFile() {
                    this.ensureInitializeProfilerMethods();
                    this._flushLogProfile();
                    var profileFilePath = this._getLogProfilerProfileOutputFile();
                    var stat = FS.stat(profileFilePath);
                    if (stat && stat.size > 0) {
                        return FS.readFile(profileFilePath);
                    }
                    else {
                        console.debug(`Unable to fetch the profile file ${profileFilePath} as it is empty`);
                        return null;
                    }
                }
                saveLogProfile() {
                    this.ensureInitializeProfilerMethods();
                    var profileArray = this.readProfileFile();
                    var a = window.document.createElement('a');
                    a.href = window.URL.createObjectURL(new Blob([profileArray]));
                    a.download = "profile.mlpd";
                    document.body.appendChild(a);
                    a.click();
                    document.body.removeChild(a);
                }
            }
            Bootstrap.LogProfilerSupport = LogProfilerSupport;
        })(Bootstrap = WebAssembly.Bootstrap || (WebAssembly.Bootstrap = {}));
    })(WebAssembly = Uno.WebAssembly || (Uno.WebAssembly = {}));
})(Uno || (Uno = {}));
