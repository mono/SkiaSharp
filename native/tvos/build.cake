DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/apple.cake"

string GetDeploymentTarget(string arch)
{
    return "11.0";
}

string SkiaGnArgs(string skiaArch, string arch, bool isSim) =>
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
    $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] " +
    ADDITIONAL_GN_ARGS;

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    var simX64 = Build("appletvsimulator", "x86_64", "x64");
    var simArm64 = Build("appletvsimulator", "arm64", "arm64");
    var deviceArm64 = Build("appletvos", "arm64", "arm64");

    // device framework (runtimes/tvos): device-arm64 + legacy simulator-x86_64,
    // the exact arch layout the published tvOS NuGet expects.
    CreateFrameworkFromDylibs(
        OUTPUT_PATH.Combine("tvos/libSkiaSharp.framework"),
        new[] { deviceArm64, simX64 }.Where(d => d != null).ToArray(),
        GetDeploymentTarget("arm64"),
        new[] { "AppleTVOS" },
        new[] { 3 });

    // simulator framework (runtimes/tvossimulator): simulator-x86_64 + simulator-arm64.
    CreateFrameworkFromDylibs(
        OUTPUT_PATH.Combine("tvossimulator/libSkiaSharp.framework"),
        new[] { simX64, simArm64 }.Where(d => d != null).ToArray(),
        GetDeploymentTarget("arm64"),
        new[] { "AppleTVSimulator" },
        new[] { 3 });

    FilePath Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return null;

        var isSim = sdk.EndsWith("simulator");
        var platform = isSim ? "tvossimulator" : "tvos";

        GnNinja($"{platform}/{arch}", "SkiaSharp", SkiaGnArgs(skiaArch, arch, isSim));

        // GN's solink already emits the dynamic-library framework binary; the wrapper
        // lipos these per-arch dylibs and builds the .framework bundle.
        var dylib = SKIA_PATH.CombineWithFilePath($"out/{platform}/{arch}/libSkiaSharp.dylib");
        EnsureSingleArch(dylib, skiaArch == "x64" ? "x86_64" : skiaArch);
        return dylib;
    }
});

Task("libHarfBuzzSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    var simX64 = Build("appletvsimulator", "x86_64", "x64");
    var simArm64 = Build("appletvsimulator", "arm64", "arm64");
    var deviceArm64 = Build("appletvos", "arm64", "arm64");

    CreateFrameworkFromDylibs(
        OUTPUT_PATH.Combine("tvos/libHarfBuzzSharp.framework"),
        new[] { deviceArm64, simX64 }.Where(d => d != null).ToArray(),
        GetDeploymentTarget("arm64"),
        new[] { "AppleTVOS" },
        new[] { 3 });

    CreateFrameworkFromDylibs(
        OUTPUT_PATH.Combine("tvossimulator/libHarfBuzzSharp.framework"),
        new[] { simX64, simArm64 }.Where(d => d != null).ToArray(),
        GetDeploymentTarget("arm64"),
        new[] { "AppleTVSimulator" },
        new[] { 3 });

    FilePath Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return null;

        var isSim = sdk.EndsWith("simulator");
        var platform = isSim ? "tvossimulator" : "tvos";

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
