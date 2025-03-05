DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string GetDeploymentTarget(string arch) =>
    arch.ToLower() switch
    {
        "arm64" => "11.0",
        _ => "10.13",
    };

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("macosx", "x86_64", "x64");
    Build("macosx", "arm64", "arm64");

    CreateFatDylib(OUTPUT_PATH.Combine("libSkiaSharp"));

    void Build(string sdk, string xcodeArch, string skiaArch)
    {
        if (Skip(xcodeArch)) return;

        GnNinja($"{sdk}/{xcodeArch}", "skia modules/skottie",
            $"target_os='mac' " +
            $"target_cpu='{skiaArch}' " +
            $"min_macos_version='{GetDeploymentTarget(xcodeArch)}' " +
            $"skia_use_harfbuzz=false " +
            $"skia_use_icu=false " +
            $"skia_use_metal=true " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_enable_skottie=true " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++' ] " +
            $"extra_ldflags=[ '-stdlib=libc++' ]");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, xcodeArch, properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget(xcodeArch) },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{xcodeArch}.xcarchive",
            OUTPUT_PATH.Combine($"libSkiaSharp/{xcodeArch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("macosx", "x86_64");
    Build("macosx", "arm64");

    CreateFatDylib(OUTPUT_PATH.Combine("libHarfBuzzSharp"));

    void Build(string sdk, string xcodeArch)
    {
        if (Skip(xcodeArch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, xcodeArch, properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget(xcodeArch) },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{xcodeArch}.xcarchive",
            OUTPUT_PATH.Combine($"libHarfBuzzSharp/{xcodeArch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
