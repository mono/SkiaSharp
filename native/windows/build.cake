DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native"));

DirectoryPath LLVM_HOME = Argument("llvm", EnvironmentVariable("LLVM_HOME") ?? "C:/Program Files/LLVM");
string VC_TOOLSET_VERSION = Argument("vcToolsetVersion", "14.2");

string SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? "true");
bool SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

#load "../../cake/native-shared.cake"
#load "../../cake/msbuild.cake"

string VARIANT = BUILD_VARIANT ?? "windows";

Information("Native Arguments:");
Information($"    {"LLVM_HOME".PadRight(30)} {{0}}", LLVM_HOME);
Information($"    {"SUPPORT_VULKAN".PadRight(30)} {{0}}", SUPPORT_VULKAN);
Information($"    {"VARIANT".PadRight(30)} {{0}}", VARIANT);
Information($"    {"CONFIGURATION".PadRight(30)} {{0}}", CONFIGURATION);

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86", "x86");
    Build("x64", "x64", "x64");
    Build("ARM64", "arm64", "ARM64");

    void Build(string arch, string skiaArch, string dir)
    {
        if (Skip(arch)) return;

        var clang = string.IsNullOrEmpty(LLVM_HOME.FullPath) ? "" : $"clang_win='{LLVM_HOME}' ";
        var win_vcvars_version = string.IsNullOrEmpty(VC_TOOLSET_VERSION) ? "" : $"win_vcvars_version='{VC_TOOLSET_VERSION}' ";
        var d = CONFIGURATION.ToLower() == "release" ? "" : "d";

        GnNinja($"{VARIANT}/{arch}", "SkiaSharp",
            $"target_os='win'" +
            $"target_cpu='{skiaArch}' " +
            $"skia_enable_fontmgr_win_gdi=false " +
            $"skia_use_dng_sdk=true " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_enable_skottie=true " +
            $"skia_use_vulkan={SUPPORT_VULKAN} ".ToLower () +
            clang +
            win_vcvars_version +
            $"extra_cflags=[ '-DSKIA_C_DLL', '/MT{d}', '/EHsc', '/Z7', '-D_HAS_AUTO_PTR_ETC=1' ] " +
            $"extra_ldflags=[ '/DEBUG:FULL' ] " +
            ADDITIONAL_GN_ARGS);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.dll"), outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/{VARIANT}/{arch}/libSkiaSharp.pdb"), outDir);
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86");
    Build("x64", "x64");
    Build("ARM64", "arm64");

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        RunMSBuild("libHarfBuzzSharp/libHarfBuzzSharp.sln", platformTarget: arch);

        var outDir = OUTPUT_PATH.Combine($"{VARIANT}/{dir}");
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.dll", outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.pdb", outDir);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
