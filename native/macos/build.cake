DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/osx"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/apple/apple.cake"

string GetDeploymentTarget(string arch)
{
    switch (arch.ToLower()) {
        case "arm64": return "11.0";
        default: return "10.13";
    }
}

string SkiaGnArgs(string skiaArch, string arch) =>
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
    $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_ARC4RANDOM_BUF', '-stdlib=libc++' ] " +
    $"extra_ldflags=[ '-stdlib=libc++' ]";

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    var dylibs = new List<FilePath>();

    Build("x86_64", "x64");
    Build("arm64", "arm64");

    // combine the per-architecture GN/ninja dylibs into a single fat dylib
    var fatDylib = OUTPUT_PATH.CombineWithFilePath("libSkiaSharp.dylib");
    EnsureDirectoryExists(OUTPUT_PATH);
    RunLipo(fatDylib, dylibs.ToArray());
    StripSign(fatDylib);

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"macos/{arch}", "SkiaSharp", SkiaGnArgs(skiaArch, arch));

        // GN's solink rule already sets the install name to
        // @rpath/libSkiaSharp.dylib, matching the old xcodeproj output.
        dylibs.Add(SKIA_PATH.CombineWithFilePath($"out/macos/{arch}/libSkiaSharp.dylib"));
    }
});

Task("libHarfBuzzSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs())
    .Does(() =>
{
    var dylibs = new List<FilePath>();

    Build("x86_64", "x64");
    Build("arm64", "arm64");

    // combine the per-architecture GN/ninja dylibs into a single fat dylib
    var fatDylib = OUTPUT_PATH.CombineWithFilePath("libHarfBuzzSharp.dylib");
    EnsureDirectoryExists(OUTPUT_PATH);
    RunLipo(fatDylib, dylibs.ToArray());
    StripSign(fatDylib);

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        // Reuse the same out dir + args as libSkiaSharp (identical args => no re-gen);
        // only the ninja target differs. The HarfBuzzSharp GN target is self-contained
        // (just harfbuzz-subset.cc) and emits @rpath/libHarfBuzzSharp.dylib via solink.
        GnNinja($"macos/{arch}", "HarfBuzzSharp", SkiaGnArgs(skiaArch, arch));

        dylibs.Add(SKIA_PATH.CombineWithFilePath($"out/macos/{arch}/libHarfBuzzSharp.dylib"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
