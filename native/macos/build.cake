#addin nuget:?package=Cake.XCode&version=4.2.0

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../cake/native-shared.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("x86_64", "x64");

    RunLipo(OUTPUT_PATH, "libSkiaSharp.dylib", new [] {
       (FilePath) "x86_64/libSkiaSharp.dylib"
    });

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"macos/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='mac' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-mmacosx-version-min=10.7', '-stdlib=libc++' ] " +
            $"extra_ldflags=[ '-Wl,macosx_version_min=10.7', '-stdlib=libc++' ]");

        XCodeBuild(new XCodeBuildSettings {
            Project = "libSkiaSharp/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libSkiaSharp/build/{CONFIGURATION}/", outDir);

        StripSign(outDir.CombineWithFilePath("libSkiaSharp.dylib"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("x86_64", "x64");

    RunLipo(OUTPUT_PATH, "libHarfBuzzSharp.dylib", new [] {
       (FilePath) "x86_64/libHarfBuzzSharp.dylib"
    });

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        XCodeBuild(new XCodeBuildSettings {
            Project = "libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        var outDir = OUTPUT_PATH.Combine(dir);
        EnsureDirectoryExists(outDir);
        CopyDirectory($"libHarfBuzzSharp/build/{CONFIGURATION}/", outDir);

        StripSign(outDir.CombineWithFilePath("libHarfBuzzSharp.dylib"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
