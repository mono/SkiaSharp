DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/ios"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("iphonesimulator", "i386", "x86");
    Build("iphonesimulator", "x86_64", "x64");
    Build("iphoneos", "armv7", "arm");
    Build("iphoneos", "arm64", "arm64");

    CopyDirectory(OUTPUT_PATH.Combine("armv7/libSkiaSharp.framework"), OUTPUT_PATH.Combine("libSkiaSharp.framework"));
    DeleteFile(OUTPUT_PATH.CombineWithFilePath("libSkiaSharp.framework/libSkiaSharp"));
    RunLipo(OUTPUT_PATH, "libSkiaSharp.framework/libSkiaSharp", new [] {
       (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "armv7/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"ios/{arch}", "skia",
            $"target_cpu='{skiaArch}' " +
            $"target_os='ios' " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-mios-version-min=8.0' ] " +
            $"extra_ldflags=[ '-Wl,ios_version_min=8.0' ]");

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
    Build("iphonesimulator", "i386");
    Build("iphonesimulator", "x86_64");
    Build("iphoneos", "armv7");
    Build("iphoneos", "arm64");

    RunLipo(OUTPUT_PATH, "libHarfBuzzSharp.a", new [] {
       (FilePath) "i386/libHarfBuzzSharp.a",
       (FilePath) "x86_64/libHarfBuzzSharp.a",
       (FilePath) "armv7/libHarfBuzzSharp.a",
       (FilePath) "arm64/libHarfBuzzSharp.a"
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
