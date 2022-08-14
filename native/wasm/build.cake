DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
string EMSCRIPTEN_VERSION = Argument("emscriptenVersion", EnvironmentVariable("EMSCRIPTEN_VERSION") ?? "").ToLower();
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

    GnNinja($"wasm", "SkiaSharp",
        $"target_os='linux' " +
        $"target_cpu='wasm' " +
        $"is_static_skiasharp=true " +
        $"skia_enable_ccpr=false " +
        $"skia_enable_fontmgr_custom_directory=false " +
        $"skia_enable_fontmgr_custom_empty=false " +
        $"skia_enable_fontmgr_custom_embedded=true " +
        $"skia_enable_fontmgr_empty=false " +
        $"skia_enable_gpu={(SUPPORT_GPU ? "true" : "false")} " +
        (SUPPORT_GPU ? "skia_gl_standard='webgl'" : "") +
        $"skia_enable_nvpr=false " +
        $"skia_enable_pdf=true " +
        $"skia_use_dng_sdk=false " +
        $"skia_use_webgl=true " +
        $"skia_use_fontconfig=false " +
        $"skia_use_freetype=true " +
        $"skia_use_harfbuzz=false " +
        $"skia_use_icu=false " +
        $"skia_use_piex=false " +
        $"skia_use_sfntly=false " +
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
        $"  '-DSKNX_NO_SIMD', '-DSK_DISABLE_AAA', '-DGR_GL_CHECK_ALLOC_WITH_GET_ERROR=0', " +
        $"  '-s', 'WARN_UNALIGNED=1' " + // '-s', 'USE_WEBGL2=1' (experimental)
        $"] " +
        $"extra_cflags_cc=[ '-frtti' ] " +
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
    EnsureDirectoryExists(outDir);
    CopyFileToDirectory(a, outDir);
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    GnNinja($"wasm", "HarfBuzzSharp",
        $"target_os='linux' " +
        $"target_cpu='wasm' " +
        $"is_static_skiasharp=true " +
        $"visibility_hidden=false " +
        COMPILERS +
        ADDITIONAL_GN_ARGS);

    var outDir = OUTPUT_PATH.Combine($"wasm");
    if (!string.IsNullOrEmpty(EMSCRIPTEN_VERSION))
        outDir = outDir.Combine("libHarfBuzzSharp.a").Combine(EMSCRIPTEN_VERSION);
    EnsureDirectoryExists(outDir);
    var so = SKIA_PATH.CombineWithFilePath($"out/wasm/libHarfBuzzSharp.a");
    CopyFileToDirectory(so, outDir);
    CopyFile(so, outDir.CombineWithFilePath("libHarfBuzzSharp.a"));
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
