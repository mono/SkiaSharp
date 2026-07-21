DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/infra/native/shared/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
string EMSCRIPTEN_ROOT = Argument("emscripten", EnvironmentVariable("EMSCRIPTEN_SDK_ROOT") ?? EnvironmentVariable("EMSDK") ?? "");
string EMSCRIPTEN_VERSION = Argument("emscriptenVersion", EnvironmentVariable("EMSCRIPTEN_VERSION") ?? "").ToLower();
string[] EMSCRIPTEN_FEATURES = Argument("emscriptenFeatures", EnvironmentVariable("EMSCRIPTEN_FEATURES") ?? "").ToLower()
    .Split(",").Where(f => f != "none").ToArray();
bool SUPPORT_GPU = SUPPORT_GPU_VAR == "1" || SUPPORT_GPU_VAR == "true";
// Graphite/Dawn/WebGPU need Skia's is_canvaskit path, which m151 wrote against
// the newer WebGPU headers only present in emsdk >= 3.1.51. The .NET-8 (3.1.34)
// WASM matrix would fail to compile Dawn (WGPUBufferMapAsyncStatus_*, ShaderF16,
// wgpu::Buffer::GetMapState, etc.), so drop back to a raster + ganesh-only build
// there. Ganesh (WebGL) stays enabled either way.
bool SUPPORT_GRAPHITE = SUPPORT_GPU &&
    (string.IsNullOrEmpty(EMSCRIPTEN_VERSION) ||
     string.CompareOrdinal(EMSCRIPTEN_VERSION, "3.1.51") >= 0);

// Dawn's WebGPU-C++ interface has diverged from Emscripten's built-in
// `-sUSE_WEBGPU=1` port (deprecated in emsdk 4.0.10, removed in 4.0.18). Skia
// m151+'s Graphite backend targets the newer interface and needs Dawn's own
// `emdawnwebgpu` port instead. We fetch a pinned release tarball on demand so
// (a) the source tree stays lean, (b) the port lives next to `externals/skia`
// like all other native inputs, and (c) once staged into the WASM native output
// (see the externals-emdawnwebgpu task below) the port ships inside the native
// artifact, so pack + test agents get identical build inputs without re-fetching.
// The pin (tag / SHA512) and the sync helper live here because the WASM native
// build is now the only thing that fetches the port.
string EMDAWN_TAG = "v20260624.223603";
string EMDAWN_SHA512 = "615257384ad7df17174c5733c17d8ac0473dfdcddeac69e334d7109501954dc42e77ed54deb666bf44581fcf8e69c2365311626786cd267e52a3d48d7a9441c5";
DirectoryPath EMDAWN_ROOT = ROOT_PATH.Combine("externals/emdawnwebgpu_pkg");

void SyncEmdawnwebgpuPort()
{
    // The port ships its own VERSION.txt containing a natural-language string:
    //   "Dawn release v<tag> at revision <sha>."
    // Reusing it as our up-to-date probe keeps the working tree byte-identical
    // to the upstream release (no marker files of ours to `git status`-noise).
    var versionFile = EMDAWN_ROOT.CombineWithFilePath("VERSION.txt");
    var needsExtract = !FileExists(versionFile) ||
        !System.IO.File.ReadAllText(versionFile.FullPath).Contains(EMDAWN_TAG);

    if (needsExtract) {
        var zipName = $"emdawnwebgpu_pkg-{EMDAWN_TAG}.zip";
        var zipUrl = $"https://github.com/google/dawn/releases/download/{EMDAWN_TAG}/{zipName}";
        var cacheDir = ROOT_PATH.Combine("externals/package_cache/emdawnwebgpu");
        EnsureDirectoryExists(cacheDir);
        var zipPath = cacheDir.CombineWithFilePath(zipName);

        if (!FileExists(zipPath)) {
            Information($"Downloading {zipUrl}");
            DownloadFile(zipUrl, zipPath);
        }

        // Verify SHA512 before touching the working tree — a corrupted /
        // tampered zip must never overwrite a good port on disk.
        string actualSha512;
        using (var stream = System.IO.File.OpenRead(zipPath.FullPath))
        using (var sha = System.Security.Cryptography.SHA512.Create()) {
            var hash = sha.ComputeHash(stream);
            var sb = new System.Text.StringBuilder(hash.Length * 2);
            foreach (var b in hash) sb.Append(b.ToString("x2"));
            actualSha512 = sb.ToString();
        }
        if (!string.Equals(actualSha512, EMDAWN_SHA512, StringComparison.OrdinalIgnoreCase)) {
            DeleteFile(zipPath);
            throw new Exception(
                $"emdawnwebgpu port {EMDAWN_TAG} SHA512 mismatch.\n" +
                $"  expected: {EMDAWN_SHA512}\n" +
                $"  actual:   {actualSha512}\n" +
                $"Downloaded zip was deleted; re-run to retry.");
        }

        if (DirectoryExists(EMDAWN_ROOT))
            CleanDirectories(EMDAWN_ROOT.FullPath);
        EnsureDirectoryExists(EMDAWN_ROOT);
        // The zip unpacks with `emdawnwebgpu_pkg/` at its root, so extracting
        // to `externals/` lands the contents directly at EMDAWN_ROOT —
        // VERSION.txt included.
        Unzip(zipPath, ROOT_PATH.Combine("externals"));
    } else {
        Information($"emdawnwebgpu port already at {EMDAWN_TAG}, skipping extract.");
    }

    // emdawnwebgpu's library_webgpu.js declares __deps: ['$stackSave',
    // '$stackRestore', ...] against emscripten's JS library system. That works
    // on newer emsdks where those names exist as `$`-form aliases, but emsdk
    // 3.1.56 (shipped with .NET 10 WASM SDK) has stackSave and stackRestore
    // only as bare WASM_SYSTEM_EXPORTS — no `$` alias — so the ports mechanism
    // resolves them but a bare `--js-library` load bombs at compiler.mjs with
    // "undefined symbol: $stackSave". Rewrite those two to their bare names so
    // consumers on 3.1.56's emsdk can link. Idempotent: applying to already-
    // patched files is a no-op. Runs on every sync (not just first-extract)
    // so a manual re-download can't leave a mismatched library on disk.
    var libJs = EMDAWN_ROOT.CombineWithFilePath("webgpu/src/library_webgpu.js");
    var lib = System.IO.File.ReadAllText(libJs.FullPath);
    var patched = lib
        .Replace("'$stackSave'", "'stackSave'")
        .Replace("'$stackRestore'", "'stackRestore'");
    if (patched != lib) {
        System.IO.File.WriteAllText(libJs.FullPath, patched);
        Information("Patched library_webgpu.js: $stackSave/$stackRestore -> bare names for emsdk 3.1.56.");
    }
}

string CC = Argument("cc", "emcc");
string CXX = Argument("cxx", "em++");
string AR = Argument("ar", "emar");
string COMPILERS = $"cc='{CC}' cxx='{CXX}' ar='{AR}' ";

// Sync Dawn's emdawnwebgpu port into externals/emdawnwebgpu_pkg (needed at
// compile time for libSkiaSharp's --use-port) and then stage a copy into the
// WASM native output so it travels with the native artifact — exactly like the
// libSkiaSharp*.a files. Downstream pack and test agents consume the port
// straight from output/native/wasm/emdawnwebgpu_pkg (the downloaded native
// artifact) and never have to re-fetch it. Idempotent: the sync is a no-op when
// VERSION.txt already matches EMDAWN_TAG. Runs unconditionally (not gated on
// SUPPORT_GRAPHITE) so the port is always present in the artifact regardless of
// which emsdk-version leg produced it.
Task("externals-emdawnwebgpu")
    .Does(() =>
{
    SyncEmdawnwebgpuPort();

    var portOutput = OUTPUT_PATH.Combine("wasm/emdawnwebgpu_pkg");
    EnsureDirectoryExists(portOutput);
    CleanDirectories(portOutput.FullPath);
    CopyDirectory(EMDAWN_ROOT, portOutput);
});

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .IsDependentOn("externals-emdawnwebgpu")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    bool hasSimdEnabled = EMSCRIPTEN_FEATURES.Contains("simd") || EMSCRIPTEN_FEATURES.Contains("_simd");
    bool hasThreadingEnabled = EMSCRIPTEN_FEATURES.Contains("mt");
    bool hasWasmEH = EMSCRIPTEN_FEATURES.Contains("_wasmeh");

    var emscriptenFeaturesModifiers = 
        EMSCRIPTEN_FEATURES
        .Where(f => !f.StartsWith("_"))
        .ToArray();

    // Skia's Dawn-on-WebGPU sources include <webgpu/webgpu.h> and
    // <webgpu/webgpu_cpp.h>. Historically Emscripten's `-sUSE_WEBGPU=1` port
    // dropped those into `emsdk/system/include/webgpu/`, so a static-archive
    // build like this one got them "for free" without ever passing -sUSE_WEBGPU
    // at compile time. That port was deprecated in emsdk 4.0.10 and removed in
    // 4.0.18, so we forward `--use-port=<emdawnwebgpu.port.py>` to emcc, which
    // registers the same include paths + link-time JS glue via Dawn's own port.
    // Kept out of extra_cflags_cc because emcc reads it from extra_cflags too
    // (and duplicating triggers "port already loaded" diagnostics).
    // Skia's Graphite Dawn sources are riddled with `#if defined(__EMSCRIPTEN__)`
    // branches that assume Emscripten's *old* -sUSE_WEBGPU=1 headers
    // (ShaderModuleWGSLDescriptor, VertexBufferNotUsed, ComputePassTimestampWrites,
    // legacy OnSubmittedWorkDone). emdawnwebgpu delivers the *native* Dawn API
    // through the Emscripten toolchain, so we need Skia to take its native-Dawn
    // `#else` branches. The mono/skia carry-patch on this submodule redirects
    // every such gate to also check `!defined(SKIA_USING_EMDAWNWEBGPU)`;
    // defining it here activates that override. Only meaningful under the port.
    string emdawnPortArg = SUPPORT_GRAPHITE
        ? $", '--use-port={EMDAWN_ROOT.CombineWithFilePath("emdawnwebgpu.port.py")}'"
          + ", '-DSKIA_USING_EMDAWNWEBGPU=1'"
        : "";

    GnNinja($"wasm", "SkiaSharp",
        $"target_os='linux' " +
        $"target_cpu='wasm' " +
        $"is_static_skiasharp=true " +
        // is_canvaskit is the Skia switch that makes :graphite's Dawn-backed
        // sources resolve <webgpu/webgpu_cpp.h> via the Emscripten-style
        // bindings header layout (as opposed to native-Dawn generated headers
        // that only exist off-tree). emdawnwebgpu ships that same layout, so
        // we keep is_canvaskit tracking SUPPORT_GRAPHITE.
        $"is_canvaskit={SUPPORT_GRAPHITE} ".ToLower() +
        $"skia_enable_fontmgr_custom_directory=false " +
        $"skia_enable_fontmgr_custom_empty=false " +
        $"skia_enable_fontmgr_custom_embedded=true " +
        $"skia_enable_fontmgr_empty=false " +
        $"skia_enable_ganesh={(SUPPORT_GPU ? "true" : "false")} " +
        (SUPPORT_GPU ? "skia_gl_standard='webgl'" : "") +
        $"skia_enable_pdf=true " +
        $"skia_use_dng_sdk=false " +
        $"skia_use_webgl=true " +
        $"skia_use_fontconfig=false " +
        $"skia_use_freetype=true " +
        $"skia_use_harfbuzz=false " +
        $"skia_use_icu=false " +
        $"skia_use_partition_alloc=false " +
        $"skia_use_piex=false " +
        $"skia_use_expat=true " +
        $"skia_use_libwebp_encode=true " +
        $"skia_use_system_expat=false " +
        $"skia_use_system_freetype2=false " +
        $"skia_use_system_libjpeg_turbo=false " +
        $"skia_use_system_libpng=false " +
        $"skia_use_system_libwebp=false " +
        $"skia_use_system_zlib=false " +
        $"skia_use_vulkan=false " +
        $"skia_use_wuffs=true " +
        $"skia_enable_skottie=true " +
        $"skia_enable_graphite={SUPPORT_GRAPHITE} ".ToLower() +
        $"skia_use_dawn={SUPPORT_GRAPHITE} ".ToLower() +
        $"skia_use_webgpu={SUPPORT_GRAPHITE} ".ToLower() +
        $"extra_cflags=[ " +
        $"  '-DSKIA_C_DLL', '-DSK_AVOID_SLOW_RASTER_PIPELINE_BLURS', '-DXML_POOR_ENTROPY', " +
        $" {(!hasSimdEnabled ? "'-DSKNX_NO_SIMD', " : "")} '-DSK_DISABLE_AAA', '-DGR_GL_CHECK_ALLOC_WITH_GET_ERROR=0', " +
        $"  '-s', 'WARN_UNALIGNED=1' " + // '-s', 'USE_WEBGL2=1' (experimental)
        $"  { (hasSimdEnabled ? ", '-msimd128'" : "") } " +
        $"  { (hasThreadingEnabled ? ", '-pthread'" : "") } " +
        $"  { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } " +
        $"  {emdawnPortArg} " +
        $"] " +
        // SIMD support is based on https://github.com/google/skia/blob/1f193df9b393d50da39570dab77a0bb5d28ec8ef/modules/canvaskit/compile.sh#L57
        $"extra_cflags_cc=[ '-frtti' { (hasSimdEnabled ? ", '-msimd128'" : "") } { (hasThreadingEnabled ? ", '-pthread'" : "") } { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } ] " +
        $"skia_emsdk_dir='{EMSCRIPTEN_ROOT}'" +
        COMPILERS +
        ADDITIONAL_GN_ARGS);

    var a = SKIA_PATH.CombineWithFilePath($"out/wasm/libSkiaSharp.a");

    // Extract every Skia static archive's .o files and merge them into libSkiaSharp.a
    // so the consumer's final link sees a single fat archive. Archive naming differs
    // by build mode:
    //   * Default WASM build → `*.a.wasm` (set by gn/toolchain/BUILD.gn).
    //   * canvaskit / Graphite-on-WebGPU build (is_canvaskit=true) → `*.a` (no .wasm
    //     suffix), because that's the canvaskit convention upstream.
    // The merge must skip libSkiaSharp.a itself (we're appending INTO it) and skip
    // libHarfBuzzSharp.a.wasm (separate consumer-facing archive). Globbing both
    // patterns keeps both build modes working.
    var skiaOut = SKIA_PATH.Combine("out/wasm");
    var mergeDir = skiaOut.Combine("obj/merge");
    EnsureDirectoryExists(mergeDir);
    CleanDirectories(mergeDir.FullPath);
    var archivesToMerge = GetFiles($"{skiaOut}/*.a.wasm").ToList();
    archivesToMerge.AddRange(GetFiles($"{skiaOut}/*.wasm.a"));
    archivesToMerge.AddRange(GetFiles($"{skiaOut}/*.a"));
    foreach (var file in archivesToMerge) {
        var name = file.GetFilename().FullPath;
        // libSkiaSharp.a is our output; libHarfBuzzSharp.* ships separately.
        if (name == "libSkiaSharp.a" || name.StartsWith("libHarfBuzzSharp."))
            continue;
        RunProcess(AR, new ProcessSettings {
            Arguments = $"x \"{file}\"",
            WorkingDirectory = mergeDir.FullPath,
        });
    }

    // add the default font
    var input = SKIA_PATH.CombineWithFilePath("modules/canvaskit/fonts/NotoMono-Regular.ttf");
    var embed_resources = SKIA_PATH.CombineWithFilePath("tools/embed_resources.py");
    RunProcess(PYTHON_EXE, new ProcessSettings {
        Arguments = $"{embed_resources} --name SK_EMBEDDED_FONTS --input {input} --output {input}.cpp --align 4",
        WorkingDirectory = SKIA_PATH.FullPath,
    });
    RunProcess(CC, $"-std=c++17 -I. {input}.cpp -r -o {mergeDir}/NotoMonoRegularttf.o");

    // merge all the .o files into the final .a file
    var oFiles = GetFiles($"{mergeDir}/*.o");
    RunProcess(AR, $"-crs {a} {string.Join(" ", oFiles)}");

    var outDir = OUTPUT_PATH.Combine($"wasm");
    if (!string.IsNullOrEmpty(EMSCRIPTEN_VERSION))
        outDir = outDir.Combine("libSkiaSharp.a").Combine(EMSCRIPTEN_VERSION);
    if (emscriptenFeaturesModifiers.Length != 0)
        outDir = outDir.Combine(string.Join(",", emscriptenFeaturesModifiers));
    EnsureDirectoryExists(outDir);
    CopyFileToDirectory(a, outDir);
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    bool hasSimdEnabled = EMSCRIPTEN_FEATURES.Contains("simd") || EMSCRIPTEN_FEATURES.Contains("_simd");
    bool hasThreadingEnabled = EMSCRIPTEN_FEATURES.Contains("mt");
    bool hasWasmEH = EMSCRIPTEN_FEATURES.Contains("_wasmeh");

    var emscriptenFeaturesModifiers = 
        EMSCRIPTEN_FEATURES
        .Where(f => !f.StartsWith("_"))
        .ToArray();

    GnNinja($"wasm", "HarfBuzzSharp",
        $"target_os='linux' " +
        $"target_cpu='wasm' " +
        $"is_static_skiasharp=true " +
        // See libSkiaSharp target above.
        $"skia_use_partition_alloc=false " +
        $"extra_cflags=[ '-s', 'WARN_UNALIGNED=1' { (hasSimdEnabled ? ", '-msimd128'" : "") } { (hasThreadingEnabled ? ", '-pthread'" : "") } { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } ] " +
        $"extra_cflags_cc=[ '-frtti' { (hasSimdEnabled ? ", '-msimd128'" : "") } { (hasThreadingEnabled ? ", '-pthread'" : "") } { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } ] " +
        $"skia_emsdk_dir='{EMSCRIPTEN_ROOT}'" +
        COMPILERS +
        ADDITIONAL_GN_ARGS);

    var outDir = OUTPUT_PATH.Combine($"wasm");
    if (!string.IsNullOrEmpty(EMSCRIPTEN_VERSION))
        outDir = outDir.Combine("libHarfBuzzSharp.a").Combine(EMSCRIPTEN_VERSION);
    if (emscriptenFeaturesModifiers.Length != 0)
        outDir = outDir.Combine(string.Join(",", emscriptenFeaturesModifiers));
    EnsureDirectoryExists(outDir);
    // Emsdk 3.1.56 emits `libHarfBuzzSharp.wasm.a`; newer versions emit
    // `libHarfBuzzSharp.a.wasm`. Take whichever exists.
    var hb = SKIA_PATH.CombineWithFilePath($"out/wasm/libHarfBuzzSharp.a.wasm");
    if (!FileExists(hb))
        hb = SKIA_PATH.CombineWithFilePath($"out/wasm/libHarfBuzzSharp.wasm.a");
    CopyFileToDirectory(hb, outDir);
    CopyFile(hb, outDir.CombineWithFilePath("libHarfBuzzSharp.a"));
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
