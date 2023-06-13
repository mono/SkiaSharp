DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

string VARIANT = (BUILD_VARIANT ?? "ios").ToLower();
string DEPLOYMENT_SDK = VARIANT == "maccatalyst" ? "[sdk=macosx*]" : "";

string GetDeploymentTarget(string arch)
{
    switch (VARIANT) {
        case "maccatalyst": return "13.0";
        default: return "8.0";
    }
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator", "i386", "x86");
        Build("iphonesimulator", "x86_64", "x64");
        Build("iphonesimulator", "arm64", "arm64");
        Build("iphoneos", "armv7", "arm");
        Build("iphoneos", "arm64", "arm64");
        Build("iphoneos", "arm64", "arm64", "arm64e");

        CreateFatFramework(OUTPUT_PATH.Combine("iosdevice/libSkiaSharp"));
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
        var platformSuffix = isSim ? "simulator" : "device";
        var platform = VARIANT + platformSuffix;

        GnNinja($"{platform}/{arch}", "skia modules/skottie",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
            $"min_{VARIANT}_version='{GetDeploymentTarget(arch)}' " +
            $"ios_use_simulator={(isSim ? "true" : "false")} " +
            $"skia_use_icu=false " +
            $"skia_use_metal={(sdk == "macosx" ? "false" : "true")} " +
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

        if (xcodeArch != arch) {
            SafeCopy(
                SKIA_PATH.Combine("out").Combine(platform).Combine(arch),
                SKIA_PATH.Combine("out").Combine(platform).Combine(xcodeArch));
        }

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, xcodeArch, platform: platform, properties: new Dictionary<string, string> {
            { $"IPHONEOS_DEPLOYMENT_TARGET{DEPLOYMENT_SDK}", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{platform}/libSkiaSharp/{arch}.xcarchive"));

        if (xcodeArch != arch) {
            SafeCopy(
                $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{xcodeArch}.xcarchive",
                OUTPUT_PATH.Combine($"{platform}/libSkiaSharp/{xcodeArch}.xcarchive"));
        }
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator", "i386");
        Build("iphonesimulator", "x86_64");
        Build("iphonesimulator", "arm64");
        Build("iphoneos", "armv7");
        Build("iphoneos", "arm64");
        Build("iphoneos", "arm64e");

        CreateFatFramework(OUTPUT_PATH.Combine("iosdevice/libHarfBuzzSharp"));
        CreateFatFramework(OUTPUT_PATH.Combine("iossimulator/libHarfBuzzSharp"));
    } else if (VARIANT == "maccatalyst") {
        Build("macosx", "x86_64");
        Build("macosx", "arm64");

        CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libHarfBuzzSharp"));
    }

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        var isSim = sdk.EndsWith("simulator");
        var platformSuffix = isSim ? "simulator" : "device";
        var platform = VARIANT + platformSuffix;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch, platform: platform, properties: new Dictionary<string, string> {
            { $"IPHONEOS_DEPLOYMENT_TARGET{DEPLOYMENT_SDK}", GetDeploymentTarget(arch) },
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
