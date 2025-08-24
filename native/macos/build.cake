DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string GetDeploymentTarget() =>
    "10.13";

string GetDestination(string sdk) =>
    "generic/platform=macOS";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    BuildSkia("macosx", "x86_64", "x64");
    BuildSkia("macosx", "arm64", "arm64");
    Build("macosx");

    CreateFatDylib(OUTPUT_PATH.Combine("libSkiaSharp"));

    void BuildSkia(string sdk, string xcodeArch, string skiaArch)
    {
        GnNinja($"{sdk}/{xcodeArch}", "skia modules/skottie",
            $"target_os='mac' " +
            $"target_cpu='{skiaArch}' " +
            $"min_macos_version='{GetDeploymentTarget()}' " +
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
    }

    void Build(string sdk)
    {
        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget() },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}.xcarchive",
            OUTPUT_PATH.Combine($"libSkiaSharp/{sdk}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("macosx");

    CreateFatDylib(OUTPUT_PATH.Combine("libHarfBuzzSharp"));

    void Build(string sdk)
    {
        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget() },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}.xcarchive",
            OUTPUT_PATH.Combine($"libHarfBuzzSharp/{sdk}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
