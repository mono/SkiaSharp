DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

#load "../../cake/native-shared.cake"

string SUPPORT_GPU_VAR = Argument("supportGpu", EnvironmentVariable("SUPPORT_GPU") ?? "true").ToLower();
bool SUPPORT_GPU = SUPPORT_GPU_VAR == "1" || SUPPORT_GPU_VAR == "true";

string SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? "true");
bool SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

string CC = Argument("cc", EnvironmentVariable("CC"));
string CXX = Argument("cxx", EnvironmentVariable("CXX"));
string AR = Argument("ar", EnvironmentVariable("AR"));

string VARIANT = BUILD_VARIANT ?? "linux";

if (BUILD_ARCH.Length == 0)
    BUILD_ARCH = new [] { "x64" };

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var ARCH in BUILD_ARCH) {
        if (Skip(ARCH)) return;

        var COMPILERS = "";
        if (!string.IsNullOrEmpty(CC))
            COMPILERS += $"cc='{CC}' ";
        if (!string.IsNullOrEmpty(CXX))
            COMPILERS += $"cxx='{CXX}' ";
        if (!string.IsNullOrEmpty(AR))
            COMPILERS += $"ar='{AR}' ";

        var soname = GetVersion("libSkiaSharp", "soname");
        var map = MakeAbsolute((FilePath)"libSkiaSharp/libSkiaSharp.map");

        GnNinja($"{VARIANT}/{ARCH}", "SkiaSharp",
            $"target_os='linux' " +
            $"target_cpu='{ARCH}' " +
            $"is_official_build=true " +
            $"skia_enable_gpu={(SUPPORT_GPU ? "true" : "false")} " +
            $"skia_enable_tools=false " +
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
            $"extra_asmflags=[] " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DHAVE_SYSCALL_GETRANDOM', '-DXML_DEV_URANDOM' ] " +
            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={map}' ] " +
            COMPILERS +
            $"linux_soname_version='{soname}' " +
            ADDITIONAL_GN_ARGS);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{ARCH}");
        EnsureDirectoryExists(outDir);
        var so = SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{ARCH}/libSkiaSharp.so.{soname}");
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libSkiaSharp.so"));
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
    foreach (var ARCH in BUILD_ARCH) {
        if (Skip(ARCH)) return;

        var COMPILERS = "";
        if (!string.IsNullOrEmpty(CC))
            COMPILERS += $"CC='{CC}' ";
        if (!string.IsNullOrEmpty(CXX))
            COMPILERS += $"CXX='{CXX}' ";

        var soname = GetVersion("HarfBuzz", "soname");

        RunProcess("make", new ProcessSettings {
            Arguments = $"{COMPILERS} ARCH={ARCH} SONAME_VERSION={soname} VARIANT={VARIANT} LDFLAGS=-static-libstdc++",
            WorkingDirectory = "libHarfBuzzSharp",
        });

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{ARCH}");
        EnsureDirectoryExists(outDir);
        var so = $"libHarfBuzzSharp/bin/{VARIANT}/{ARCH}/libHarfBuzzSharp.so.{soname}";
        CopyFileToDirectory(so, outDir);
        CopyFile(so, outDir.CombineWithFilePath("libHarfBuzzSharp.so"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
