DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/xcode.cake"

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
        var simX64 = Build("iphonesimulator", "x86_64", "x64");
        var simArm64 = Build("iphonesimulator", "arm64", "arm64");
        var deviceArm64 = Build("iphoneos", "arm64", "arm64");

        // device framework (runtimes/ios): device-arm64 + legacy simulator-x86_64,
        // preserving the exact arch layout the NuGet shipped from xcodebuild.
        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("ios/libSkiaSharp.framework"),
            new[] { deviceArm64, simX64 }.Where(d => d != null).ToArray(),
            GetDeploymentTarget("arm64"),
            new[] { "iPhoneOS" },
            new[] { 1, 2 });

        // simulator framework (runtimes/iossimulator): simulator-x86_64 + simulator-arm64.
        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("iossimulator/libSkiaSharp.framework"),
            new[] { simX64, simArm64 }.Where(d => d != null).ToArray(),
            GetDeploymentTarget("arm64"),
            new[] { "iPhoneSimulator" },
            new[] { 1, 2 });
    } else if (VARIANT == "maccatalyst") {
        var x64 = Build("macosx", "x86_64", "x64");
        var arm64 = Build("macosx", "arm64", "arm64");

        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("maccatalyst/libSkiaSharp.framework"),
            new[] { x64, arm64 }.Where(d => d != null).ToArray(),
            "10.15",
            new[] { "MacOSX" },
            new[] { 2 },
            versioned: true);
    }

    FilePath Build(string sdk, string arch, string skiaArch, string xcodeArch = null)
    {
        if (Skip(arch)) return null;

        xcodeArch = xcodeArch ?? arch;
        var isSim = sdk.EndsWith("simulator");
        var platform = VARIANT;
        if (VARIANT == "ios" && isSim)
            platform += "simulator";

        GnNinja($"{platform}/{xcodeArch}", "SkiaSharp",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
            $"min_{VARIANT}_version='{GetDeploymentTarget(arch)}' " +
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
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] " +
            ADDITIONAL_GN_ARGS);

        // GN's solink already emits the dynamic-library framework binary; the wrapper
        // lipos these per-arch dylibs and builds the .framework bundle.
        var dylib = SKIA_PATH.CombineWithFilePath($"out/{platform}/{xcodeArch}/libSkiaSharp.dylib");
        EnsureSingleArch(dylib, skiaArch == "x64" ? "x86_64" : skiaArch);
        return dylib;
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
        // Build("iphoneos", "arm64e");

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
