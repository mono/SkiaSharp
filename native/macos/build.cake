DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/xcode.cake"

string SUPPORT_GRAPHITE_VAR = Argument("supportGraphite", EnvironmentVariable("SUPPORT_GRAPHITE") ?? "false");
bool SUPPORT_GRAPHITE = SUPPORT_GRAPHITE_VAR == "1" || SUPPORT_GRAPHITE_VAR.ToLower() == "true";

string SUPPORT_DAWN_VAR = Argument("supportDawn", EnvironmentVariable("SUPPORT_DAWN") ?? "false");
bool SUPPORT_DAWN = SUPPORT_DAWN_VAR == "1" || SUPPORT_DAWN_VAR.ToLower() == "true";

string GetDeploymentTarget(string arch)
{
    switch (arch.ToLower()) {
        case "arm64": return "11.0";
        default: return "10.13";
    }
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("x86_64", "x64");
    Build("arm64", "arm64");

    CreateFatDylib(OUTPUT_PATH.Combine("libSkiaSharp"));

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"macos/{arch}", "skia modules/skottie",
            $"target_os='mac' " +
            $"target_cpu='{skiaArch}' " +
            $"min_macos_version='{GetDeploymentTarget(arch)}' " +
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
            $"skia_enable_graphite={SUPPORT_GRAPHITE} ".ToLower() +
            $"skia_use_dawn={SUPPORT_DAWN} ".ToLower() +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++' ] " +
            $"extra_ldflags=[ '-stdlib=libc++' ]");

        RunXCodeBuild("libSkiaSharp/libSkiaSharp.xcodeproj", "libSkiaSharp", "macosx", arch, properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libSkiaSharp/bin/{CONFIGURATION}/macosx/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"libSkiaSharp/{arch}.xcarchive"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    Build("x86_64");
    Build("arm64");

    CreateFatDylib(OUTPUT_PATH.Combine("libHarfBuzzSharp"));

    void Build(string arch)
    {
        if (Skip(arch)) return;

        RunXCodeBuild("libHarfBuzzSharp/libHarfBuzzSharp.xcodeproj", "libHarfBuzzSharp", "macosx", arch, properties: new Dictionary<string, string> {
            { "MACOSX_DEPLOYMENT_TARGET", GetDeploymentTarget(arch) },
        });

        SafeCopy(
            $"libHarfBuzzSharp/bin/{CONFIGURATION}/macosx/{arch}.xcarchive",
            OUTPUT_PATH.Combine($"libHarfBuzzSharp/{arch}.xcarchive"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
