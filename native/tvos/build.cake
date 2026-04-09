DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string GetDeploymentTarget(string arch)
{
    return "11.0";
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("appletvsimulator", "x86_64", "x64");
    Build("appletvsimulator", "arm64", "arm64");
    Build("appletvos", "arm64", "arm64");

    SafeCopy(
        $"libSkiaSharp/bin/{CONFIGURATION}/appletvsimulator/x86_64.xcarchive",
        OUTPUT_PATH.Combine($"tvos/libSkiaSharp/x86_64.xcarchive"));

    CreateFatFramework(OUTPUT_PATH.Combine("tvos/libSkiaSharp"));
    CreateFatFramework(OUTPUT_PATH.Combine("tvossimulator/libSkiaSharp"));

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        var isSim = sdk.EndsWith("simulator");
        var platform = isSim ? "tvossimulator" : "tvos";

        GnNinja($"{platform}/{arch}", "skia modules/skottie",
            $"target_os='tvos' " +
            $"target_cpu='{skiaArch}' " +
            $"min_tvos_version='{GetDeploymentTarget(arch)}' " +
            $"ios_use_simulator={(isSim ? "true" : "false")} " +
            $"skia_use_harfbuzz=false " +
            $"skia_use_icu=false " +
            $"skia_use_metal=true " +
            $"skia_use_piex=true " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_enable_skottie=true " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] ");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch, properties: new Dictionary<string, string> {
            { "TVOS_DEPLOYMENT_TARGET", GetDeploymentTarget(arch) },
            { "SKIA_PLATFORM", platform },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{platform}/libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("appletvsimulator", "x86_64");
    Build("appletvsimulator", "arm64");
    Build("appletvos", "arm64");

    SafeCopy(
        $"libHarfBuzzSharp/bin/{CONFIGURATION}/appletvsimulator/x86_64.xcarchive",
        OUTPUT_PATH.Combine($"tvos/libHarfBuzzSharp/x86_64.xcarchive"));

    CreateFatFramework(OUTPUT_PATH.Combine("tvos/libHarfBuzzSharp"));
    CreateFatFramework(OUTPUT_PATH.Combine("tvossimulator/libHarfBuzzSharp"));

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        var isSim = sdk.EndsWith("simulator");
        var platform = isSim ? "tvossimulator" : "tvos";

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch, properties: new Dictionary<string, string> {
            { "TVOS_DEPLOYMENT_TARGET", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{platform}/libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
