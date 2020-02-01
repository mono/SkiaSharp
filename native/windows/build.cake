DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

DirectoryPath LLVM_HOME = Argument("llvm", EnvironmentVariable("LLVM_HOME") ?? "C:/Program Files/LLVM");

#load "../../cake/native-shared.cake"
#load "../../cake/msbuild.cake"

string VARIANT = BUILD_VARIANT ?? "windows";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86", "x86");
    Build("x64", "x64", "x64");

    void Build(string arch, string skiaArch, string dir)
    {
        if (Skip(arch)) return;

        var clang = string.IsNullOrEmpty(LLVM_HOME.FullPath) ? "" : $"clang_win='{LLVM_HOME}' ";

        GnNinja($"{VARIANT}/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='win' target_cpu='{skiaArch}' " +
            clang +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '/MT', '/EHsc', '/Z7' ] " +
            $"extra_ldflags=[ '/DEBUG:FULL' ] " +
            ADDITIONAL_GN_ARGS);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.dll"), outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.pdb"), outDir);
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86");
    Build("x64", "x64");

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        RunMSBuild("libHarfBuzzSharp/libHarfBuzzSharp.sln", platformTarget: arch);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.dll", outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.pdb", outDir);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
