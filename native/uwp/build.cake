bool SKIP_ANGLE = Argument ("skipAngle", false);
string VC_TOOLSET_VERSION = Argument("vcToolsetVersion", "14.2");

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath VCPKG_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/vcpkg"));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/uwp"));

#load "../../cake/native-shared.cake"
#load "../../cake/msbuild.cake"

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86", "x86");
    Build("x64", "x64", "x64");
    Build("ARM", "arm", "ARM");
    Build("ARM64", "arm64", "ARM64");

    void Build(string arch, string skiaArch, string dir)
    {
        if (Skip(arch)) return;

        var win_vcvars_version = string.IsNullOrEmpty(VC_TOOLSET_VERSION) ? "" : $"win_vcvars_version='{VC_TOOLSET_VERSION}' ";
        var d = CONFIGURATION.ToLower() == "release" ? "" : "d";

        GnNinja($"uwp/{arch}", "SkiaSharp",
            $"target_os='winrt' " +
            $"target_cpu='{skiaArch}' " +
            $"skia_enable_fontmgr_win_gdi=false " +
            $"skia_use_icu=false " +
            $"skia_use_piex=true " +
            $"skia_use_sfntly=false " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=false " +
            $"skia_enable_skottie=true " +
            win_vcvars_version +
            $"extra_cflags=[  " +
            $"  '-DSKIA_C_DLL', '/MD{d}', '/EHsc', '/Z7', " +
            $"  '-DSK_HAS_DWRITE_1_H', '-DSK_HAS_DWRITE_2_H', '-DNO_GETENV', '-D_HAS_AUTO_PTR_ETC=1' ] " +
            $"extra_ldflags=[ '/DEBUG:FULL' ]");

        var outDir = OUTPUT_PATH.Combine(dir);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/uwp/{arch}/libSkiaSharp.dll"), outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"out/uwp/{arch}/libSkiaSharp.pdb"), outDir);
    }
});

Task("libHarfBuzzSharp")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86");
    Build("x64", "x64");
    Build("ARM", "arm");
    Build("ARM64", "arm64");

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        RunMSBuild("libHarfBuzzSharp/libHarfBuzzSharp.sln", platformTarget: arch);

        var outDir = OUTPUT_PATH.Combine(dir);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.dll", outDir);
        CopyFileToDirectory($"libHarfBuzzSharp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.pdb", outDir);
    }
});

Task("SkiaSharp.Views.Interop.UWP")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    Build("Win32", "x86");
    Build("x64", "x64");
    Build("ARM", "arm");
    Build("ARM64", "arm64");

    void Build(string arch, string dir)
    {
        if (Skip(arch)) return;

        RunMSBuild("SkiaSharp.Views.Interop.UWP/SkiaSharp.Views.Interop.UWP.sln", platformTarget: arch);

        var outDir = OUTPUT_PATH.Combine(dir);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory($"SkiaSharp.Views.Interop.UWP/bin/{arch}/{CONFIGURATION}/SkiaSharp.Views.Interop.UWP.dll", outDir);
        CopyFileToDirectory($"SkiaSharp.Views.Interop.UWP/bin/{arch}/{CONFIGURATION}/SkiaSharp.Views.Interop.UWP.pdb", outDir);
    }
});

Task("ANGLE")
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
    if (SKIP_ANGLE)
        return;

    if (!DirectoryExists (VCPKG_PATH))
        RunProcess ("git", $"clone --depth 1 https://github.com/microsoft/vcpkg.git --branch master --single-branch {VCPKG_PATH}");

    var vcpkg = VCPKG_PATH.CombineWithFilePath ("vcpkg.exe");
    if (!FileExists (vcpkg))
        RunProcess (VCPKG_PATH.CombineWithFilePath ("bootstrap-vcpkg.bat"));

    var platform_toolset = string.IsNullOrEmpty(VC_TOOLSET_VERSION) ? "" : $"v{VC_TOOLSET_VERSION.Replace(".", "")}";
    var toolset_suffix = string.IsNullOrEmpty(VC_TOOLSET_VERSION) ? "" : $"-{platform_toolset}";

    Build("x86");
    Build("x64");
    Build("arm");
    Build("arm64");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        var triplet = $"{arch}-uwp{toolset_suffix}";

        // make the versioned triplets
        if (!string.IsNullOrEmpty(VC_TOOLSET_VERSION)) {
            var cmake = VCPKG_PATH.CombineWithFilePath ($"triplets/community/{triplet}.cmake");
            if (!FileExists (cmake)) {
                var src = VCPKG_PATH.CombineWithFilePath ($"triplets/{arch}-uwp.cmake");
                if (!FileExists (src))
                    src = VCPKG_PATH.CombineWithFilePath ($"triplets/community/{arch}-uwp.cmake");
                CopyFile (src, cmake);
                System.IO.File.AppendAllLines (cmake.FullPath, new [] {
                    $"set(VCPKG_PLATFORM_TOOLSET \"{platform_toolset}\")",
                    $"set(VCPKG_DEP_INFO_OVERRIDE_VARS \"{platform_toolset}\")",
                });
            }
        }

        var d = CONFIGURATION.ToLower() == "release" ? "" : "debug/";
        var zd = CONFIGURATION.ToLower() == "release" ? "" : "d";

        RunProcess (vcpkg, $"install angle:{triplet}");

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/libEGL.dll"), outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/libEGL.pdb"), outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/libGLESv2.dll"), outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/libGLESv2.pdb"), outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/zlib{zd}1.dll"), outDir);
        CopyFileToDirectory(VCPKG_PATH.CombineWithFilePath ($"installed/{triplet}/{d}bin/zlib{zd}.pdb"), outDir);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp")
    .IsDependentOn("SkiaSharp.Views.Interop.UWP")
    .IsDependentOn("ANGLE");

RunTarget(TARGET);
