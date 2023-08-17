DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string VARIANT = (BUILD_VARIANT ?? "ios").ToLower();

string GetDeploymentTarget(string arch)
{
    switch (VARIANT) {
        case "maccatalyst": return "13.1";
        default: return "11.0";
    }
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator", "x86_64", "x64");
        Build("iphonesimulator", "arm64", "arm64");
        Build("iphoneos", "arm64", "arm64");
        Build("iphoneos", "arm64", "arm64", "arm64e");

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/iphonesimulator/x86_64.xcarchive",
            OUTPUT_PATH.Combine($"ios/libSkiaSharp/x86_64.xcarchive"));

        CreateFatFramework(OUTPUT_PATH.Combine("ios/libSkiaSharp"));
        CreateFatFramework(OUTPUT_PATH.Combine("iossimulator/libSkiaSharp"));
    } else if (VARIANT == "maccatalyst") {
        Build("macosx", "x86_64", "x64");
        Build("macosx", "arm64", "arm64");

        CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libSkiaSharp"));
    }

    void Build(string sdk, string arch, string skiaArch, string xcodeArch = null)
    {
        if (Skip(arch)) return;

        xcodeArch = xcodeArch ?? arch;
        var isSim = sdk.EndsWith("simulator");
        var platform = VARIANT;
        if (VARIANT == "ios" && isSim)
            platform += "simulator";

        GnNinja($"{platform}/{xcodeArch}", "skia modules/skottie",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
            $"min_{VARIANT}_version='{GetDeploymentTarget(arch)}' " +
            $"ios_use_simulator={(isSim ? "true" : "false")} " +
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
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] " +
            ADDITIONAL_GN_ARGS);

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, xcodeArch, properties: new Dictionary<string, string> {
            { $"{VARIANT.ToUpper()}_DEPLOYMENT_TARGET_VERSION", GetDeploymentTarget(arch) },
            { $"SKIA_PLATFORM", platform },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{xcodeArch}.xcarchive",
            OUTPUT_PATH.Combine($"{platform}/libSkiaSharp/{xcodeArch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator", "x86_64");
        Build("iphonesimulator", "arm64");
        Build("iphoneos", "arm64");
        Build("iphoneos", "arm64e");

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/iphonesimulator/x86_64.xcarchive",
            OUTPUT_PATH.Combine($"ios/libHarfBuzzSharp/x86_64.xcarchive"));

        CreateFatFramework(OUTPUT_PATH.Combine("ios/libHarfBuzzSharp"));
        CreateFatFramework(OUTPUT_PATH.Combine("iossimulator/libHarfBuzzSharp"));
    } else if (VARIANT == "maccatalyst") {
        Build("macosx", "x86_64");
        Build("macosx", "arm64");

        CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libHarfBuzzSharp"));
    }

    void Build(string sdk, string arch, string xcodeArch = null)
    {
        if (Skip(arch)) return;

        xcodeArch = xcodeArch ?? arch;
        var isSim = sdk.EndsWith("simulator");
        var platform = VARIANT;
        if (VARIANT == "ios" && isSim)
            platform += "simulator";

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, xcodeArch, properties: new Dictionary<string, string> {
            { $"{VARIANT.ToUpper()}_DEPLOYMENT_TARGET_VERSION", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{xcodeArch}.xcarchive",
            OUTPUT_PATH.Combine($"{platform}/libHarfBuzzSharp/{xcodeArch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
