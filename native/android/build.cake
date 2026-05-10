DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/android"));

#load "../../scripts/infra/native/shared/native-shared.cake"
#load "../../scripts/infra/native/android/ndk.cake"

string SUPPORT_GRAPHITE_VAR = Argument ("supportGraphite", EnvironmentVariable ("SUPPORT_GRAPHITE") ?? "false");
bool SUPPORT_GRAPHITE = SUPPORT_GRAPHITE_VAR == "1" || SUPPORT_GRAPHITE_VAR.ToLower () == "true";

Information("Android NDK Path: {0}", ANDROID_NDK_HOME);
Information("Building Graphite: {0}", SUPPORT_GRAPHITE);

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    // Linux is supported as a host since NDK r19+ ships an official linux-x86_64 toolchain.
    // Upstream CI only exercises the macOS/Windows path; Linux works for cross-compiling
    // arm64/arm/x64/x86 Android targets via the same prebuilt LLVM toolchain.
    .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows() || IsRunningOnLinux())
    .Does(() =>
{
    Build("x86", "x86");
    Build("x86_64", "x64");
    Build("armeabi-v7a", "arm");
    Build("arm64-v8a", "arm64");

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"android/{arch}", "SkiaSharp",
            $"target_cpu='{skiaArch}' " +
            $"target_os='android' " +
            $"skia_use_harfbuzz=false " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_freetype2=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_use_vulkan=true " +
            $"skia_enable_graphite={SUPPORT_GRAPHITE} ".ToLower () +
            $"skia_enable_skottie=true " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM', '-g', '-ggdb3' ] " +
            $"extra_ldflags=[ '-Wl,-z,max-page-size=16384' ] " +
            $"ndk='{ANDROID_NDK_HOME}' " +
            $"ndk_api=21");

        StripCopy(
            SKIA_PATH.CombineWithFilePath($"out/android/{arch}/libSkiaSharp.so"),
            OUTPUT_PATH.Combine(arch));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows() || IsRunningOnLinux())
    .Does(() =>
{
    Build("x86");
    Build("x86_64");
    Build("armeabi-v7a");
    Build("arm64-v8a");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        RunNdkBuild(arch, "libHarfBuzzSharp");

        StripCopy(
            $"libHarfBuzzSharp/libs/{arch}/libHarfBuzzSharp.so",
            OUTPUT_PATH.Combine(arch));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
