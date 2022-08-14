DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

string VARIANT = BUILD_VARIANT ?? "ios";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT.ToLower() == "ios") {
        Build("iphonesimulator", "i386", "x86");
        Build("iphonesimulator", "x86_64", "x64");
        Build("iphoneos", "armv7", "arm");
        Build("iphoneos", "arm64", "arm64");

        CreateFatFramework(OUTPUT_PATH.Combine("ios/libSkiaSharp"));
    } else if (VARIANT.ToLower() == "maccatalyst") {
        Build("macosx", "x86_64", "x64");
        Build("macosx", "arm64", "arm64");

        CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libSkiaSharp"));
    }

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"{VARIANT}/{arch}", "skia modules/skottie",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{VARIANT}' " +
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
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] ");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch, platform: VARIANT);

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    if (VARIANT.ToLower() == "ios") {
        Build("iphonesimulator", "i386");
        Build("iphonesimulator", "x86_64");
        Build("iphoneos", "armv7");
        Build("iphoneos", "arm64");

        CreateFatFramework(OUTPUT_PATH.Combine("ios/libHarfBuzzSharp"));
    } else if (VARIANT.ToLower() == "maccatalyst") {
        Build("macosx", "x86_64");
        Build("macosx", "arm64");

        CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libHarfBuzzSharp"));
    }

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch, platform: VARIANT);

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{VARIANT}/libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
