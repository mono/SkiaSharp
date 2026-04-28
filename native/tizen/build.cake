#addin nuget:?package=Cake.FileHelpers&version=3.2.1

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../.."));
DirectoryPath OUTPUT_PATH = MakeAbsolute(ROOT_PATH.Combine("output/native/tizen"));

#load "../../scripts/cake/native-shared.cake"

DirectoryPath TIZEN_STUDIO_HOME = EnvironmentVariable("TIZEN_STUDIO_HOME") ?? PROFILE_PATH.Combine("tizen-studio");

var bat = IsRunningOnWindows() ? ".bat" : "";
var tizen = TIZEN_STUDIO_HOME.CombineWithFilePath($"tools/ide/bin/tizen{bat}").FullPath;

void SetProjectProfile(string projectDir, string profile)
{
    var propFile = MakeAbsolute((FilePath)$"{projectDir}/project_def.prop").FullPath;
    var propContent = System.IO.File.ReadAllText(propFile);
    var regex = new System.Text.RegularExpressions.Regex(
        @"^profile = .+$", System.Text.RegularExpressions.RegexOptions.Multiline);
    if (!regex.IsMatch(propContent))
        throw new Exception($"Failed to set profile in '{propFile}': no 'profile = ...' line found.");
    var newContent = regex.Replace(propContent, $"profile = {profile}");
    System.IO.File.WriteAllText(propFile, newContent);
}

Task("libSkiaSharp")
    .IsDependentOn("git-sync-deps")
    .Does(() =>
{
    // Tizen 32-bit (armel / i586 on the mobile-6.0 rootstrap) was dropped
    // — the rootstrap ships gcc-9.2 / glibc 2.30 era headers that don't
    // play with current Skia, and the only deployment targets that ever
    // shipped 32-bit Tizen are years past EOL. Only tizen-8.0 64-bit
    // x86_64 + aarch64 are supported.
    Build("x86_64","x64",   "x86_64",  "tizen-8.0-emulator64.core",    "tizen-8.0",  "8.0");
    Build("aarch64","arm64","aarch64",  "tizen-8.0-device64.core",      "tizen-8.0",  "8.0");

    void Build(string outputDir, string skiaArch, string tizenArch, string rootstrap, string profile, string ncliVersion)
    {
        if (Skip(skiaArch)) return;

        GnNinja($"tizen/{outputDir}", "skia modules/skottie",
           $"target_os='tizen' " +
           $"target_cpu='{skiaArch}' " +
           $"skia_enable_ganesh=true " +
           $"skia_use_harfbuzz=false " +
           $"skia_use_icu=false " +
           $"skia_use_piex=true " +
           $"skia_use_system_expat=false " +
           $"skia_use_system_freetype2=true " +
           $"skia_use_system_libjpeg_turbo=false " +
           $"skia_use_system_libpng=false " +
           $"skia_use_system_libwebp=false " +
           $"skia_use_system_zlib=true " +
           $"skia_enable_skottie=true " +
           $"extra_cflags=[ '-DSKIA_C_DLL', '-DXML_DEV_URANDOM' ] " +
           $"ncli='{TIZEN_STUDIO_HOME}' " +
           $"ncli_version='{ncliVersion}'");

        SetProjectProfile("libSkiaSharp", profile);

        var buildDir = MakeAbsolute((DirectoryPath)$"libSkiaSharp/{CONFIGURATION}");
        if (DirectoryExists(buildDir))
            DeleteDirectory(buildDir, new DeleteDirectorySettings { Recursive = true, Force = true });

        RunProcess(tizen, new ProcessSettings {
           Arguments = $"build-native -a {tizenArch} -c llvm -C {CONFIGURATION} -r {rootstrap}",
           WorkingDirectory = MakeAbsolute((DirectoryPath)"libSkiaSharp").FullPath,
        });

        var outDir = OUTPUT_PATH.Combine(outputDir);
        EnsureDirectoryExists(outDir);
        CopyFile($"libSkiaSharp/{CONFIGURATION}/libskiasharp.so", outDir.CombineWithFilePath("libSkiaSharp.so"));
    }
});

Task("libHarfBuzzSharp")
    .Does(() =>
{
    // See the libSkiaSharp task above — Tizen 32-bit is no longer
    // supported.
    Build("x86_64","x64",  "x86_64",  "tizen-8.0-emulator64.core",  "tizen-8.0");
    Build("aarch64","arm64","aarch64", "tizen-8.0-device64.core",     "tizen-8.0");

    void Build(string outputDir, string skiaArch, string tizenArch, string rootstrap, string profile)
    {
        if (Skip(skiaArch)) return;

        SetProjectProfile("libHarfBuzzSharp", profile);

        var buildDir = MakeAbsolute((DirectoryPath)$"libHarfBuzzSharp/{CONFIGURATION}");
        if (DirectoryExists(buildDir))
            DeleteDirectory(buildDir, new DeleteDirectorySettings { Recursive = true, Force = true });

        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {tizenArch} -c llvm -C {CONFIGURATION} -r {rootstrap}",
            WorkingDirectory = MakeAbsolute((DirectoryPath)"libHarfBuzzSharp").FullPath,
        });

        var outDir = OUTPUT_PATH.Combine(outputDir);
        EnsureDirectoryExists(outDir);
        CopyFile($"libHarfBuzzSharp/{CONFIGURATION}/libharfbuzzsharp.so", outDir.CombineWithFilePath("libHarfBuzzSharp.so"));
    }
});

Task("Default")
    .IsDependentOn("libSkiaSharp")
    .IsDependentOn("libHarfBuzzSharp");

RunTarget(TARGET);
