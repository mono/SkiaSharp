DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/tizen"));

#load "../../cake/native-shared.cake"

DirectoryPath TIZEN_STUDIO_HOME = EnvironmentVariable ("TIZEN_STUDIO_HOME") ?? PROFILE_PATH.Combine ("tizen-studio");

var bat = IsRunningOnWindows() ? ".bat" : "";
var tizen = TIZEN_STUDIO_HOME.CombineWithFilePath($"tools/ide/bin/tizen{bat}").FullPath;

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .Does(() =>
{
    Build("armel", "arm");
    Build("i386", "x86");

    void Build(string arch, string skiaArch)
    {
        if (Skip(arch)) return;

        GnNinja($"tizen/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='tizen' target_cpu='{skiaArch}' " +
            $"skia_enable_gpu=true " +
            $"skia_use_icu=false " +
            $"skia_use_sfntly=false " +
            $"skia_use_piex=true " +
            $"skia_use_system_expat=false " +
            $"skia_use_system_freetype2=true " +
            $"skia_use_system_libjpeg_turbo=false " +
            $"skia_use_system_libpng=false " +
            $"skia_use_system_libwebp=false " +
            $"skia_use_system_zlib=true " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-DSK_BUILD_FOR_TIZEN' ] " +
            $"ncli='{TIZEN_STUDIO_HOME}' " +
            $"ncli_version='4.0'");

        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {skiaArch} -c llvm -C {CONFIGURATION}" ,
            WorkingDirectory = ROOT_PATH.Combine("native/libSkiaSharp_tizen").FullPath,
        });

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"libSkiaSharp/{CONFIGURATION}/libskiasharp.so"), outDir);
    }
});

Task("libHarfBuzzSharp")
    .Does(() =>
{
    var cmd = IsRunningOnWindows() ? ".cmd" : "";
    var ndkbuild = TIZEN_STUDIO_HOME.CombineWithFilePath($"ndk-build{cmd}").FullPath;

    Build("armel");
    Build("i386");

    void Build(string arch)
    {
        if (Skip(arch)) return;

        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {arch} -c llvm -C {CONFIGURATION}",
            WorkingDirectory = ROOT_PATH.Combine("native/libHarfBuzzSharp_tizen").FullPath
        });

        var outDir = OUTPUT_PATH.Combine(arch);
        EnsureDirectoryExists(outDir);
        CopyFileToDirectory(SKIA_PATH.CombineWithFilePath($"libHarfBuzzSharp/{CONFIGURATION}/libHarfBuzzSharp.so"), outDir);
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
