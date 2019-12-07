DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/android"));

#load "../../cake/native-shared.cake"

DirectoryPath ANDROID_NDK_HOME = Argument("ndk", EnvironmentVariable("ANDROID_NDK_HOME") ?? EnvironmentVariable("ANDROID_NDK_ROOT") ?? PROFILE_PATH.Combine("android-ndk").FullPath);

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMac() || IsRunningOnWindows())
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
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='android' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL' ] " +
            $"ndk='{ANDROID_NDK_HOME}' " +
            $"ndk_api={(skiaArch == "x64" || skiaArch == "arm64" ? 21 : 9)}");

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/android/{arch}/libSkiaSharp.so"), outDir);
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMac() || IsRunningOnWindows())
    .Does(() =>
{
    var cmd = IsRunningOnWindows() ? ".cmd" : "";
    var ndkbuild = ANDROID_NDK_HOME.CombineWithFilePath($"ndk-build{cmd}").FullPath;

    Build("x86");
    Build("x86_64");
    Build("armeabi-v7a");
    Build("arm64-v8a");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        RunProcess(ndkbuild, new ProcessSettings {
            Arguments = $"APP_ABI={arch}",
            WorkingDirectory = "libHarfBuzzSharp",
        });

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/libs/{arch}/libHarfBuzzSharp.so", outDir);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
