DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/tvos"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string GetDeploymentTarget() =>
    "11.0";

string GetDestination(string sdk) =>
    sdk.EndsWith("simulator") ? "generic/platform=tvOS Simulator" : "generic/platform=tvOS";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    BuildSkia("appletvsimulator", "x86_64", "x64");
    BuildSkia("appletvsimulator", "arm64", "arm64");
    Build("appletvsimulator");
    BuildSkia("appletvos", "arm64", "arm64");
    Build("appletvos");

    CreateXCFramework(OUTPUT_PATH.Combine("libSkiaSharp"));

    void BuildSkia(string sdk, string xcodeArch, string skiaArch)
    {
        GnNinja($"{sdk}/{xcodeArch}", "skia modules/skottie",
            $"target_os='tvos' " +
            $"target_cpu='{skiaArch}' " +
            $"min_ios_version='{GetDeploymentTarget()}' " +
            $"ios_use_simulator={(sdk.EndsWith("simulator") ? "true" : "false")} " +
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
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] ");
    }

    void Build(string sdk)
    {
        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { "TVOS_DEPLOYMENT_TARGET", GetDeploymentTarget() },
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
    Build("appletvsimulator");
    Build("appletvos");

    CreateXCFramework(OUTPUT_PATH.Combine("libHarfBuzzSharp"));

    void Build(string sdk)
    {
        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { "TVOS_DEPLOYMENT_TARGET", GetDeploymentTarget() },
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
