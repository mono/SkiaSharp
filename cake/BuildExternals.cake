
////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds the native C and C++ externals
Task("externals-native");
Task("externals-native-skip");

foreach (var platform in GetFiles()) {
    Task("externals-windows")
        .IsDependentOn("externals-init")
        .IsDependeeOf(ShouldBuildExternal("windows") ? "externals-native" : "externals-native-skip")
        .WithCriteria(ShouldBuildExternal("windows"))
        .WithCriteria(IsRunningOnWindows())
        .Does(() =>
    {
    });
}

// this builds the native C and C++ externals for Windows UWP
Task("externals-uwp")
    .IsDependentOn("externals-init")
    .IsDependentOn("externals-angle-uwp")
    .IsDependeeOf(ShouldBuildExternal("uwp") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("uwp"))
    .WithCriteria(IsRunningOnWindows())
    .Does(() =>
{
});

// this builds the native C and C++ externals for Mac OS X
Task("externals-macos")
    .IsDependentOn("externals-osx");
Task("externals-osx")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("osx") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("osx"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for iOS
Task("externals-ios")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("ios") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("ios"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
});

// this builds the native C and C++ externals for tvOS
Task("externals-tvos")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("tvos") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("tvos"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string, string>((sdk, arch, skiaArch) => {
        // generate native skia build files
        GnNinja($"tvos/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='tvos' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSK_BUILD_FOR_TVOS', '-DSKIA_C_DLL', '-mtvos-version-min=9.0' ] " +
            $"extra_ldflags=[ '-Wl,tvos_version_min=9.0' ]");

        // build libSkiaSharp
        XCodeBuild(new XCodeBuildSettings {
            Project = "native/libSkiaSharp_tvos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists($"output/native/tvos/{arch}");
        CopyDirectory($"native/libSkiaSharp_tvos/build/{CONFIGURATION}-{sdk}", $"output/native/tvos/{arch}");

        StripSign($"output/native/tvos/{arch}/libSkiaSharp.framework");
    });

    buildArch("appletvsimulator", "x86_64", "x64");
    buildArch("appletvos", "arm64", "arm64");

    // create the fat framework
    CopyDirectory("output/native/tvos/arm64/libSkiaSharp.framework/", "output/native/tvos/libSkiaSharp.framework/");
    DeleteFile("output/native/tvos/libSkiaSharp.framework/libSkiaSharp");
    RunLipo("output/native/tvos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
       (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string>((sdk, arch) => {
        // build libHarfBuzzSharp
        XCodeBuild(new XCodeBuildSettings {
            Project = "native/libHarfBuzzSharp_tvos/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists($"output/native/tvos/{arch}");
        CopyFileToDirectory($"native/libHarfBuzzSharp_tvos/build/{CONFIGURATION}-{sdk}/libHarfBuzzSharp.a", $"output/native/tvos/{arch}");

        StripSign($"output/native/tvos/{arch}/libHarfBuzzSharp.a");
    });

    buildHarfBuzzArch("appletvsimulator", "x86_64");
    buildHarfBuzzArch("appletvos", "arm64");

    // create the fat framework
    RunLipo("output/native/tvos/", "libHarfBuzzSharp.a", new [] {
       (FilePath) "x86_64/libHarfBuzzSharp.a",
       (FilePath) "arm64/libHarfBuzzSharp.a"
    });
});

// this builds the native C and C++ externals for watchOS
Task("externals-watchos")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("watchos") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("watchos"))
    .WithCriteria(IsRunningOnMac())
    .Does(() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string, string>((sdk, arch, skiaArch) => {
        // generate native skia build files
        GnNinja($"watchos/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='watchos' target_cpu='{skiaArch}' " +
            $"skia_enable_gpu=false " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSK_BUILD_FOR_WATCHOS', '-DSKIA_C_DLL', '-mwatchos-version-min=2.0' ] " +
            $"extra_ldflags=[ '-Wl,watchos_version_min=2.0' ]");

        // build libSkiaSharp
        XCodeBuild(new XCodeBuildSettings {
            Project = "native/libSkiaSharp_watchos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists($"output/native/watchos/{arch}");
        CopyDirectory($"native/libSkiaSharp_watchos/build/{CONFIGURATION}-{sdk}", $"output/native/watchos/{arch}");

        StripSign($"output/native/watchos/{arch}/libSkiaSharp.framework");
    });

    buildArch("watchsimulator", "i386", "x86");
    buildArch("watchos", "armv7k", "arm");
    buildArch("watchos", "arm64_32", "arm64");

    // create the fat framework
    CopyDirectory("output/native/watchos/armv7k/libSkiaSharp.framework/", "output/native/watchos/libSkiaSharp.framework/");
    DeleteFile("output/native/watchos/libSkiaSharp.framework/libSkiaSharp");
    RunLipo("output/native/watchos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
       (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "armv7k/libSkiaSharp.framework/libSkiaSharp",
       (FilePath) "arm64_32/libSkiaSharp.framework/libSkiaSharp"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string>((sdk, arch) => {
        // build libHarfBuzzSharp
        XCodeBuild(new XCodeBuildSettings {
            Project = "native/libHarfBuzzSharp_watchos/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists($"output/native/watchos/{arch}");
        CopyFileToDirectory($"native/libHarfBuzzSharp_watchos/build/{CONFIGURATION}-{sdk}/libHarfBuzzSharp.a", $"output/native/watchos/{arch}");

        StripSign($"output/native/watchos/{arch}/libHarfBuzzSharp.a");
    });

    buildHarfBuzzArch("watchsimulator", "i386");
    buildHarfBuzzArch("watchos", "armv7k");
    buildHarfBuzzArch("watchos", "arm64_32");

    // create the fat framework
    RunLipo("output/native/watchos/", "libHarfBuzzSharp.a", new [] {
       (FilePath) "i386/libHarfBuzzSharp.a",
       (FilePath) "armv7k/libHarfBuzzSharp.a",
       (FilePath) "arm64_32/libHarfBuzzSharp.a"
    });
});

// this builds the native C and C++ externals for Android
Task("externals-android")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("android") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("android"))
    .WithCriteria(IsRunningOnMac() || IsRunningOnWindows())
    .Does(() =>
{
});

// this builds the native C and C++ externals for Linux
Task("externals-linux")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("linux") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("linux"))
    .WithCriteria(IsRunningOnLinux())
    .Does(() =>
{
});

Task("externals-tizen")
    .IsDependentOn("externals-init")
    .IsDependeeOf(ShouldBuildExternal("tizen") ? "externals-native" : "externals-native-skip")
    .WithCriteria(ShouldBuildExternal("tizen"))
    .Does(() =>
{
    var bat = IsRunningOnWindows() ? ".bat" : "";
    var tizen = TIZEN_STUDIO_HOME.CombineWithFilePath($"tools/ide/bin/tizen{bat}").FullPath;

    // libSkiaSharp

    var buildArch = new Action<string, string>((arch, skiaArch) => {
        // generate native skia build files
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

        // build libSkiaSharp
        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {skiaArch} -c llvm -C {CONFIGURATION}" ,
            WorkingDirectory = ROOT_PATH.Combine("native/libSkiaSharp_tizen").FullPath,
        });

        // copy libSkiaSharp to output
        var outDir = $"output/native/tizen/{arch}";
        var libSkiaSharp = $"native/libSkiaSharp_tizen/{CONFIGURATION}/libskiasharp.so";
        EnsureDirectoryExists(outDir);
        CopyFile(libSkiaSharp, $"{outDir}/libSkiaSharp.so");
    });

    buildArch("armel", "arm");
    buildArch("i386", "x86");

    // libHarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string>((arch, skiaArch) => {
        // build libHarfBuzzSharp
        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {skiaArch} -c llvm -C {CONFIGURATION}",
            WorkingDirectory = ROOT_PATH.Combine("native/libHarfBuzzSharp_tizen").FullPath
        });

        // copy libHarfBuzzSharp to output
        var outDir = $"output/native/tizen/{arch}";
        var so = $"native/libHarfBuzzSharp_tizen/{CONFIGURATION}/libharfbuzzsharp.so";
        EnsureDirectoryExists($"output/native/tizen/{arch}");
        CopyFile(so, $"{outDir}/libHarfBuzzSharp.so");
    });

    buildHarfBuzzArch("armel", "arm");
    buildHarfBuzzArch("i386", "x86");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("externals-download")
    .IsDependentOn("download-last-successful-build")
    .Does(() =>
{
    var artifactName = "native-default";
    var artifactFilename = $"{artifactName}.zip";
    var url = string.Format(AZURE_BUILD_URL, AZURE_BUILD_ID, artifactName);

    var outputPath = "./output";
    EnsureDirectoryExists(outputPath);
    CleanDirectories(outputPath);

    DownloadFile(url, $"{outputPath}/{artifactFilename}");
    Unzip($"{outputPath}/{artifactFilename}", outputPath);
    MoveDirectory($"{outputPath}/{artifactName}", $"{outputPath}/native");
});

Task("externals-angle-uwp")
    .WithCriteria(!FileExists(ANGLE_PATH.CombineWithFilePath("uwp/ANGLE.WindowsStore.nuspec")))
    .Does(() =>
{
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task("clean-externals")
    .Does(() =>
{
    // skia
    CleanDirectories("externals/skia/out");
    CleanDirectories("externals/skia/xcodebuild");

    // angle
    CleanDirectories("externals/angle");

    // all
    CleanDirectories("output/native");
    // ios
    CleanDirectories("native/libSkiaSharp_ios/build");
    CleanDirectories("native/libHarfBuzzSharp_ios/build");
    // tvos
    CleanDirectories("native/libSkiaSharp_tvos/build");
    CleanDirectories("native/libHarfBuzzSharp_tvos/build");
    // watchos
    CleanDirectories("native/libSkiaSharp_watchos/build");
    CleanDirectories("native/libHarfBuzzSharp_watchos/build");
    // osx
    CleanDirectories("native/libSkiaSharp_osx/build");
    CleanDirectories("native/libHarfBuzzSharp_osx/build");
});
