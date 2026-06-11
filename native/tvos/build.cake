DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/xcode.cake"

string GetDeploymentTarget(string arch)
{
    return "11.0";
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    var simX64 = Build("appletvsimulator", "x86_64", "x64");
    var simArm64 = Build("appletvsimulator", "arm64", "arm64");
    var deviceArm64 = Build("appletvos", "arm64", "arm64");

    // device framework (runtimes/tvos): device-arm64 + legacy simulator-x86_64,
    // preserving the exact arch layout the NuGet shipped from xcodebuild.
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

        GnNinja($"{platform}/{arch}", "SkiaSharp",
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
            ADDITIONAL_GN_ARGS);

        // GN's solink already emits the dynamic-library framework binary; the wrapper
        // lipos these per-arch dylibs and builds the .framework bundle.
        var dylib = SKIA_PATH.CombineWithFilePath($"out/{platform}/{arch}/libSkiaSharp.dylib");
        EnsureSingleArch(dylib, skiaArch == "x64" ? "x86_64" : skiaArch);
        return dylib;
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
