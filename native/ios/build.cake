DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/apple.cake"

string VARIANT = (BUILD_VARIANT ?? "ios").ToLower();

string GetDeploymentTarget(string arch)
{
    switch (VARIANT) {
        case "maccatalyst": return "13.1";
        default: return "11.0";
    }
}

string SkiaGnArgs(string skiaArch, string arch, bool isSim) =>
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
    ADDITIONAL_GN_ARGS;

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
        // the exact arch layout the published iOS NuGet expects.
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

        GnNinja($"{platform}/{xcodeArch}", "SkiaSharp", SkiaGnArgs(skiaArch, arch, isSim));

        // GN's solink already emits the dynamic-library framework binary; the wrapper
        // lipos these per-arch dylibs and builds the .framework bundle.
        var dylib = SKIA_PATH.CombineWithFilePath($"out/{platform}/{xcodeArch}/libSkiaSharp.dylib");
        EnsureSingleArch(dylib, skiaArch == "x64" ? "x86_64" : skiaArch);
        return dylib;
    }
});

Task("libHarfBuzzSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        var simX64 = Build("iphonesimulator", "x86_64", "x64");
        var simArm64 = Build("iphonesimulator", "arm64", "arm64");
        var deviceArm64 = Build("iphoneos", "arm64", "arm64");

        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("ios/libHarfBuzzSharp.framework"),
            new[] { deviceArm64, simX64 }.Where(d => d != null).ToArray(),
            GetDeploymentTarget("arm64"),
            new[] { "iPhoneOS" },
            new[] { 1, 2 });

        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("iossimulator/libHarfBuzzSharp.framework"),
            new[] { simX64, simArm64 }.Where(d => d != null).ToArray(),
            GetDeploymentTarget("arm64"),
            new[] { "iPhoneSimulator" },
            new[] { 1, 2 });
    } else if (VARIANT == "maccatalyst") {
        var x64 = Build("macosx", "x86_64", "x64");
        var arm64 = Build("macosx", "arm64", "arm64");

        CreateFrameworkFromDylibs(
            OUTPUT_PATH.Combine("maccatalyst/libHarfBuzzSharp.framework"),
            new[] { x64, arm64 }.Where(d => d != null).ToArray(),
            "10.15",
            new[] { "MacOSX" },
            new[] { 2 },
            versioned: true);
    }

    FilePath Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return null;

        var isSim = sdk.EndsWith("simulator");
        var platform = VARIANT;
        if (VARIANT == "ios" && isSim)
            platform += "simulator";

        // Reuse the same out dir + args as libSkiaSharp (identical args => no re-gen);
        // only the ninja target differs. The HarfBuzzSharp GN target is self-contained.
        GnNinja($"{platform}/{arch}", "HarfBuzzSharp", SkiaGnArgs(skiaArch, arch, isSim));

        var dylib = SKIA_PATH.CombineWithFilePath($"out/{platform}/{arch}/libHarfBuzzSharp.dylib");
        EnsureSingleArch(dylib, skiaArch == "x64" ? "x86_64" : skiaArch);
        return dylib;
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
