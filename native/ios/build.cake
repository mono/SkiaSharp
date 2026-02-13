DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/cake/native-shared.cake"
#load "../../scripts/cake/xcode.cake"

string VARIANT = (BUILD_VARIANT ?? "ios").ToLower();

string GetDeploymentTarget() =>
    VARIANT switch
    {
        "maccatalyst" => "13.1",
        _ => "11.0",
    };

string GetDestination(string sdk) =>
    VARIANT switch
    {
        "ios" => sdk.EndsWith("simulator") ? "generic/platform=iOS Simulator" : "generic/platform=iOS",
        "maccatalyst" => "generic/platform=macOS,variant=Mac Catalyst",
        _ => throw new InvalidOperationException($"Unknown variant: {VARIANT}"),
    };

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        BuildSkia("iphonesimulator", "x86_64", "x64");
        BuildSkia("iphonesimulator", "arm64", "arm64");
        Build("iphonesimulator");
        BuildSkia("iphoneos", "arm64", "arm64");
        Build("iphoneos");
    } else if (VARIANT == "maccatalyst") {
        BuildSkia("macosx", "x86_64", "x64");
        BuildSkia("macosx", "arm64", "arm64");
        Build("macosx");
    }
    CreateXCFramework(OUTPUT_PATH.Combine($"{VARIANT}/libSkiaSharp"));

    void BuildSkia(string sdk, string xcodeArch, string skiaArch)
    {
        GnNinja($"{sdk}/{xcodeArch}", "skia modules/skottie",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
            $"min_{VARIANT}_version='{GetDeploymentTarget()}' " +
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
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] " +
            ADDITIONAL_GN_ARGS);
    }

    void Build(string sdk)
    {
        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { $"{VARIANT.ToUpper()}_DEPLOYMENT_TARGET_VERSION", GetDeploymentTarget() },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libSkiaSharp/{sdk}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator");
        Build("iphoneos");
    } else if (VARIANT == "maccatalyst") {
        Build("macosx");
    }
    CreateXCFramework(OUTPUT_PATH.Combine($"{VARIANT}/libHarfBuzzSharp"));

    void Build(string sdk)
    {
        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, GetDestination(sdk), properties: new Dictionary<string, string> {
            { $"{VARIANT.ToUpper()}_DEPLOYMENT_TARGET_VERSION", GetDeploymentTarget() },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libHarfBuzzSharp/{sdk}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
