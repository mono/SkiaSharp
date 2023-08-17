DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/cake/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
string EMSCRIPTEN_ROOT = Argument("emscripten", EnvironmentVariable("EMSCRIPTEN_SDK_ROOT") ?? EnvironmentVariable("EMSDK") ?? "");
string EMSCRIPTEN_VERSION = Argument("emscriptenVersion", EnvironmentVariable("EMSCRIPTEN_VERSION") ?? "").ToLower();
string[] EMSCRIPTEN_FEATURES = Argument("emscriptenFeatures", EnvironmentVariable("EMSCRIPTEN_FEATURES") ?? "").ToLower()
    .Split(",").Where(f => f != "none").ToArray();
bool SUPPORT_GPU = SUPPORT_GPU_VAR == "1" || SUPPORT_GPU_VAR == "true";

string CC = Argument("cc", "emcc");
string CXX = Argument("cxx", "em++");
string AR = Argument("ar", "emar");
string COMPILERS = $"cc='{CC}' cxx='{CXX}' ar='{AR}' ";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
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

    GnNinja($"wasm", "SkiaSharp",
        $"target_os='linux' " +
        $"target_cpu='wasm' " +
        $"is_static_skiasharp=true " +
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
        $"skia_use_piex=false " +
        $"skia_use_sfntly=false " +
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
        $"use_PIC=false " +
        $"extra_cflags=[ " +
        $"  '-DSKIA_C_DLL', '-DXML_POOR_ENTROPY', " + 
        $" {(!hasSimdEnabled ? "'-DSKNX_NO_SIMD', " : "")} '-DSK_DISABLE_AAA', '-DGR_GL_CHECK_ALLOC_WITH_GET_ERROR=0', " +
        $"  '-s', 'WARN_UNALIGNED=1' " + // '-s', 'USE_WEBGL2=1' (experimental)
        $"  { (hasSimdEnabled ? ", '-msimd128'" : "") } " +
        $"  { (hasThreadingEnabled ? ", '-pthread'" : "") } " +
        $"  { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } " +
        $"] " +
        // SIMD support is based on https://github.com/google/skia/blob/1f193df9b393d50da39570dab77a0bb5d28ec8ef/modules/canvaskit/compile.sh#L57
        $"extra_cflags_cc=[ '-frtti' { (hasSimdEnabled ? ", '-msimd128'" : "") } { (hasThreadingEnabled ? ", '-pthread'" : "") } { (hasWasmEH ? ", '-fwasm-exceptions'" : "") } ] " +
        $"skia_emsdk_dir='{EMSCRIPTEN_ROOT}'" +
        COMPILERS +
        ADDITIONAL_GN_ARGS);

    var a = SKIA_PATH.CombineWithFilePath($"out/wasm/libSkiaSharp.a");

    // separate all the .a files into .o files
    var skiaOut = SKIA_PATH.Combine("out/wasm");
    var mergeDir = skiaOut.Combine("obj/merge");
    EnsureDirectoryExists(mergeDir);
    CleanDirectories(mergeDir.FullPath);
    foreach (var file in GetFiles($"{skiaOut}/*.a")) {
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
    bool hasSimdEnabled = EMSCRIPTEN_FEATURES.Contains("simd") || EMSCRIPTEN_FEATURES.Contains("_simd");;
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
        $"visibility_hidden=false " +
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
    var so = SKIA_PATH.CombineWithFilePath($"out/wasm/libHarfBuzzSharp.a");
    CopyFileToDirectory(so, outDir);
    CopyFile(so, outDir.CombineWithFilePath("libHarfBuzzSharp.a"));
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
