DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

string VARIANT = (BUILD_VARIANT ?? "ios").ToLower();
string DEPLOYMENT_SDK = VARIANT == "maccatalyst" ? "[sdk=macosx*]" : "";

DirectoryPath OUTPUT_VARIANT_PATH = OUTPUT_PATH.Combine(VARIANT);

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
        Build("iphoneos", "armv7", "arm");
        Build("iphoneos", "arm64", "arm64");

        CreateFatFramework(OUTPUT_VARIANT_PATH.Combine("libSkiaSharp"));
    } else if (VARIANT == "maccatalyst") {
        Build("macosx", "x86_64", "x64");
        Build("macosx", "arm64", "arm64");

        CreateFatVersionedFramework(OUTPUT_VARIANT_PATH.Combine("libSkiaSharp"));
    }

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"{VARIANT}/{arch}", "skia modules/skottie",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
            $"min_{VARIANT}_version='{GetDeploymentTarget(arch)}' " +
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

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch, platform: VARIANT, properties: new Dictionary<string, string> {
            { $"IPHONEOS_DEPLOYMENT_TARGET{DEPLOYMENT_SDK}", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT == "ios") {
        Build("iphonesimulator", "i386");
        Build("iphonesimulator", "x86_64");
        Build("iphoneos", "armv7");
        Build("iphoneos", "arm64");

        CreateFatFramework(OUTPUT_VARIANT_PATH.Combine("libHarfBuzzSharp"));
    } else if (VARIANT == "maccatalyst") {
        Build("macosx", "x86_64");
        Build("macosx", "arm64");

        CreateFatVersionedFramework(OUTPUT_VARIANT_PATH.Combine("libHarfBuzzSharp"));
    }

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch, platform: VARIANT, properties: new Dictionary<string, string> {
            { $"IPHONEOS_DEPLOYMENT_TARGET{DEPLOYMENT_SDK}", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
