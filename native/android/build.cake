DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/android"));

#load "../../scripts/cake/native-shared.cake"

DirectoryPath ANDROID_NDK_HOME = Argument("ndk", EnvironmentVariable("ANDROID_NDK_HOME") ?? EnvironmentVariable("ANDROID_NDK_ROOT") ?? PROFILE_PATH.Combine("android-ndk").FullPath);

string SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? "true");
bool SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

Information("Android NDK Path: {0}", ANDROID_NDK_HOME);
Information("Building Vulkan: {0}", SUPPORT_VULKAN);

void CheckAlignment(FilePath so)
{
    Information($"Making sure that everything is 16 KB aligned...");

    var prebuilt = ANDROID_NDK_HOME.CombineWithFilePath("toolchains/llvm/prebuilt").FullPath;
    var objdump = GetFiles($"{prebuilt}/*/bin/llvm-objdump*").FirstOrDefault() ?? throw new Exception("Could not find llvm-objdump");
    RunProcess(objdump.FullPath, $"-p {so}", out var stdout);

    var loads = stdout
        .Where(l => l.Trim().StartsWith("LOAD"))
        .ToList();

    if (loads.Any(l => !l.Trim().EndsWith("align 2**14"))) {
        Information(String.Join(Environment.NewLine + "    ", stdout));
        throw new Exception($"{so} contained a LOAD that was not 16 KB aligned.");
    } else {
        Information("Everything is 16 KB aligned:");
        Information(String.Join(Environment.NewLine, loads));
    }
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows())
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
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_freetype2=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_use_vulkan={SUPPORT_VULKAN} ".ToLower () +
            $"skia_enable_skottie=true " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM' ] " +
            $"extra_ldflags=[ '-Wl,-z,max-page-size=16384' ] " +
            $"ndk='{ANDROID_NDK_HOME}' " +
            $"ndk_api=21");

        var so = SKIA_PATH.CombineWithFilePath($"out/android/{arch}/libSkiaSharp.so");
        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(so, outDir);
        CheckAlignment(so);
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows())
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

        var so = $"libHarfBuzzSharp/libs/{arch}/libHarfBuzzSharp.so";
        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(so, outDir);
        CheckAlignment(so);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
