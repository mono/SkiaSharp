DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("x86_64", "x64", "arm64");

    RunLipo(OUTPUT_PATH, "libSkiaSharp.dylib", new [] {
       (FilePath) "x86_64/libSkiaSharp.dylib"
    });

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"macos/{arch}", "skia",
            $"target_os='mac' " +
            $"target_cpu='{skiaArch}' " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-mmacosx-version-min=10.7', '-stdlib=libc++' ] " +
            $"extra_ldflags=[ '-Wl,macosx_version_min=10.7', '-stdlib=libc++' ]");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", "macosx", arch);

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libSkiaSharp/bin/{CONFIGURATION}/{arch}/{CONFIGURATION}/", outDir);

        StripSign(outDir.CombineWithFilePath("libSkiaSharp.dylib"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("x86_64", "arm64");

    RunLipo(OUTPUT_PATH, "libHarfBuzzSharp.dylib", new [] {
       (FilePath) "x86_64/libHarfBuzzSharp.dylib"
    });

    void Build(string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", "macosx", arch);

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libHarfBuzzSharp/bin/{CONFIGURATION}/{arch}/{CONFIGURATION}/", outDir);

        StripSign(outDir.CombineWithFilePath("libHarfBuzzSharp.dylib"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
