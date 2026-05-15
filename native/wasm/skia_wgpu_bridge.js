// SkiaSharp.NativeAssets.WebAssembly ships this file as an Emscripten JS
// library (consumer-side `--js-library` at the final emcc link), so it
// gets merged INTO dotnet.native.js itself.
//
// That placement is load-bearing. Starting in .NET 10, the .NET WASM SDK
// emits dotnet.native.js with `-sMODULARIZE`, wrapping the Emscripten
// runtime in an IIFE:
//
//     var createDotnetRuntime = (() => {
//         var Module = { /* ... runtime + Module.WebGPU.mgr* tables ... */ };
//         // ...
//     })();
//
// External JS — including any function reachable via `[JSImport]` — can't
// see `Module` directly. But `--js-library` code is linked INSIDE the IIFE
// before it closes, so it has lexical access to `Module`.
//
// The single job of this file: publish `Module` onto globalThis so the
// SkiaSharp WebGPU bindings can reach `Module.WebGPU.mgrDevice`,
// `mgrQueue`, `mgrTexture`, etc. via normal dotted-path `[JSImport]`s.
// Everything else — device acquisition, canvas setup, texture readback,
// shader uploads — is orchestrated from C#.

mergeInto(LibraryManager.library, {
    $SkiaSharpExposeModule__postset: 'SkiaSharpExposeModule();',
    $SkiaSharpExposeModule: function () {
        globalThis.skiaSharpModule = Module;
    },
    // Force-link anchor: emcc drops library-internal `$Name` symbols whose
    // C-ABI siblings aren't referenced by any C input. The consumer-side
    // targets file adds `$SkiaSharpExposeModule` to
    // DEFAULT_LIBRARY_FUNCS_TO_INCLUDE; this companion C export gives the
    // linker something concrete to keep too.
    sk_wasm_skiasharp_module_anchor__deps: ['$SkiaSharpExposeModule'],
    sk_wasm_skiasharp_module_anchor__sig: 'v',
    sk_wasm_skiasharp_module_anchor: function () {},
});
