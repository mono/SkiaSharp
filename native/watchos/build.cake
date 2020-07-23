DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/watchos"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("watchsimulator", "i386", "x86");
    Build("watchos", "armv7k", "arm");
    Build("watchos", "arm64_32", "arm64");

    CopyDirectory(OUTPUT_PATH.Combine("armv7k/libSkiaSharp.framework"), OUTPUT_PATH.Combine("libSkiaSharp.framework"));
    DeleteFile(OUTPUT_PATH.CombineWithFilePath("libSkiaSharp.framework/libSkiaSharp"));
    RunLipo(OUTPUT_PATH, "libSkiaSharp.framework/libSkiaSharp", new [] {
       (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "armv7k/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "arm64_32/libSkiaSharp.framework/libSkiaSharp"
    });

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"watchos/{arch}", "skia",
            $"target_os='watchos' " +
            $"target_cpu='{skiaArch}' " +
            $"skia_enable_gpu=false " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSK_BUILD_FOR_WATCHOS', '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-mwatchos-version-min=2.0' ] " +
            $"extra_ldflags=[ '-Wl,watchos_version_min=2.0' ]");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch);

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libSkiaSharp/bin/{CONFIGURATION}/{arch}/{CONFIGURATION}-{sdk}", outDir);

        StripSign(outDir.CombineWithFilePath("libSkiaSharp.framework"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("watchsimulator", "i386");
    Build("watchos", "armv7k");
    Build("watchos", "arm64_32");

    RunLipo(OUTPUT_PATH, "libHarfBuzzSharp.a", new [] {
       (FilePath) "i386/libHarfBuzzSharp.a",
       (FilePath) "armv7k/libHarfBuzzSharp.a",
       (FilePath) "arm64_32/libHarfBuzzSharp.a"
    });

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch);

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libHarfBuzzSharp/bin/{CONFIGURATION}/{arch}/{CONFIGURATION}-{sdk}", outDir);

        StripSign(outDir.CombineWithFilePath("libHarfBuzzSharp.a"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
