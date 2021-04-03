DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"
#load "../../cake/xcode.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("iphonesimulator", "i386", "x86");
    Build("iphonesimulator", "x86_64", "x64");
    Build("iphoneos", "armv7", "arm");
    Build("iphoneos", "arm64", "arm64");
    Build("macosx", "x86_64", "x64");
    Build("macosx", "arm64", "arm64");

    CreateFatFramework(OUTPUT_PATH.Combine("ios/libSkiaSharp"));
    CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libSkiaSharp"));

    void Build(string sdk, string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        var os = sdk == "macosx" ? "maccatalyst" : "ios";

        GnNinja($"{os}/{arch}", "skia",
            $"target_cpu='{skiaArch}' " +
            $"target_os='{os}' " +
            $"skia_use_icu=false " +
            $"skia_use_metal={(sdk == "macosx" ? "false" : "true")} " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF' ] ");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", sdk, arch, platform: os);

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{os}/libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    Build("iphonesimulator", "i386");
    Build("iphonesimulator", "x86_64");
    Build("iphoneos", "armv7");
    Build("iphoneos", "arm64");
    Build("macosx", "x86_64");
    Build("macosx", "arm64");

    CreateFatFramework(OUTPUT_PATH.Combine("ios/libHarfBuzzSharp"));
    CreateFatVersionedFramework(OUTPUT_PATH.Combine("maccatalyst/libHarfBuzzSharp"));

    void Build(string sdk, string arch)
    {
        if (Skip(arch)) return;

        var os = sdk == "macosx" ? "maccatalyst" : "ios";

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", sdk, arch, platform: os);

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/{sdk}/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"{os}/libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
