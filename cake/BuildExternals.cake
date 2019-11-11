
void GnNinja (DirectoryPath outDir, string target, string skiaArgs)
{
    var exe = IsRunningOnWindows () ? ".exe" : "";
    var quote = IsRunningOnWindows () ? "\"" : "'";
    var innerQuote = IsRunningOnWindows () ? "\\\"" : "\"";

    if (!string.IsNullOrEmpty(ADDITIONAL_GN_ARGS)) {
        skiaArgs += " " + ADDITIONAL_GN_ARGS;
    }

    // generate native skia build files
    RunProcess (SKIA_PATH.CombineWithFilePath($"bin/gn{exe}"), new ProcessSettings {
        Arguments = $"gen out/{outDir} --args={quote}{skiaArgs.Replace("'", innerQuote)}{quote}",
        WorkingDirectory = SKIA_PATH.FullPath,
    });

    // build native skia
    RunProcess (DEPOT_PATH.CombineWithFilePath ($"ninja{exe}"), new ProcessSettings {
        Arguments = $"{target} -C out/{outDir}",
        WorkingDirectory = SKIA_PATH.FullPath,
    });
}

void StripSign (FilePath target)
{
    target = MakeAbsolute (target);
    var archive = target;
    if (target.FullPath.EndsWith (".framework")) {
        archive = $"{target}/{target.GetFilenameWithoutExtension()}";
    }

    // strip anything we can
    RunProcess ("strip", new ProcessSettings {
        Arguments = $"-x -S {archive}",
    });

    // re-sign with empty
    RunProcess ("codesign", new ProcessSettings {
        Arguments = $"--force --sign - --timestamp=none {target}",
    });
}

void RunLipo (DirectoryPath directory, FilePath output, FilePath[] inputs)
{
    if (!IsRunningOnMac ()) {
        throw new InvalidOperationException ("lipo is only available on Unix.");
    }

    EnsureDirectoryExists (directory.CombineWithFilePath (output).GetDirectory ());

    var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
    RunProcess ("lipo", new ProcessSettings {
        Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
        WorkingDirectory = directory,
    });
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("externals-init")
    .Does (() =>
{
    RunProcess (PythonToolPath, new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath ("tools/git-sync-deps").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
});

// this builds the native C and C++ externals
Task ("externals-native");
Task ("externals-native-skip");

// this builds the native C and C++ externals for Windows
Task ("externals-windows")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("windows") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("windows"))
    .WithCriteria (IsRunningOnWindows ())
    .Does (() =>
{
    // libSkiaSharp

    var buildArch = new Action<string, string, string> ((arch, skiaArch, dir) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        var clang = string.IsNullOrEmpty(LLVM_HOME.FullPath) ? "" : $"clang_win='{LLVM_HOME}' ";

        // generate native skia build files
        GnNinja ($"win/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='win' target_cpu='{skiaArch}' " +
            clang +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '/MT', '/EHsc', '/Z7' ] " +
            $"extra_ldflags=[ '/DEBUG:FULL' ]");

        // copy libSkiaSharp to output
        var outDir = $"output/native/windows/{dir}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ($"out/win/{arch}/libSkiaSharp.dll"), outDir);
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ($"out/win/{arch}/libSkiaSharp.pdb"), outDir);
    });

    buildArch ("Win32", "x86", "x86");
    buildArch ("x64", "x64", "x64");

    // libHarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((arch, dir) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // build libHarfBuzzSharp
        RunMSBuild ("native-builds/libHarfBuzzSharp_windows/libHarfBuzzSharp.sln", platformTarget: arch);

        // copy libHarfBuzzSharp to output
        var outDir = $"output/native/windows/{dir}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_windows/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.dll", outDir);
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_windows/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.pdb", outDir);
    });

    buildHarfBuzzArch ("Win32", "x86");
    buildHarfBuzzArch ("x64", "x64");
});

// this builds the native C and C++ externals for Windows UWP
Task ("externals-uwp")
    .IsDependentOn ("externals-init")
    .IsDependentOn ("externals-angle-uwp")
    .IsDependeeOf (ShouldBuildExternal ("uwp") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("uwp"))
    .WithCriteria (IsRunningOnWindows ())
    .Does (() =>
{
    // libSkiaSharp

    var buildArch = new Action<string, string, string> ((arch, skiaArch, dir) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // generate native skia build files
        GnNinja ($"winrt/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='winrt' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[  " +
            $"  '-DSKIA_C_DLL', '/MD', '/EHsc', '/Z7', " +
            $"  '-DWINAPI_FAMILY=WINAPI_FAMILY_APP', '-DSK_BUILD_FOR_WINRT', '-DSK_HAS_DWRITE_1_H', '-DSK_HAS_DWRITE_2_H', '-DNO_GETENV' ] " +
            $"extra_ldflags=[ '/DEBUG:FULL', '/APPCONTAINER', 'WindowsApp.lib' ]");

        // copy libSkiaSharp to output
        var outDir = $"output/native/uwp/{dir}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ($"out/winrt/{arch}/libSkiaSharp.dll"), outDir);
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ($"out/winrt/{arch}/libSkiaSharp.pdb"), outDir);
    });

    buildArch ("x64", "x64", "x64");
    buildArch ("Win32", "x86", "x86");
    buildArch ("ARM", "arm", "ARM");

    // libHarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((arch, dir) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // build libHarfBuzzSharp
        RunMSBuild ("native-builds/libHarfBuzzSharp_uwp/libHarfBuzzSharp.sln", platformTarget: arch);

        // copy libHarfBuzzSharp to output
        var outDir = $"output/native/uwp/{dir}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_uwp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.dll", outDir);
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_uwp/bin/{arch}/{CONFIGURATION}/libHarfBuzzSharp.pdb", outDir);
    });

    buildHarfBuzzArch ("Win32", "x86");
    buildHarfBuzzArch ("x64", "x64");
    buildHarfBuzzArch ("ARM", "arm");

    // SkiaSharp.Views.Interop.UWP

    var buildInteropArch = new Action<string, string> ((arch, dir) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // build SkiaSharp.Views.Interop.UWP
        RunMSBuild ("source/SkiaSharp.Views.Interop.UWP.sln", platformTarget: arch);

        // copy SkiaSharp.Views.Interop.UWP to native
        var outDir = $"./output/native/uwp/{dir}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory ($"source/SkiaSharp.Views.Interop.UWP/bin/{arch}/{CONFIGURATION}/SkiaSharp.Views.Interop.UWP.dll", outDir);
        CopyFileToDirectory ($"source/SkiaSharp.Views.Interop.UWP/bin/{arch}/{CONFIGURATION}/SkiaSharp.Views.Interop.UWP.pdb", outDir);
    });

    buildInteropArch ("Win32", "x86");
    buildInteropArch ("x64", "x64");
    buildInteropArch ("ARM", "arm");

    // copy ANGLE externals
    EnsureDirectoryExists ("./output/native/uwp/arm/");
    EnsureDirectoryExists ("./output/native/uwp/x86/");
    EnsureDirectoryExists ("./output/native/uwp/x64/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/ARM/libEGL.dll"), "./output/native/uwp/arm/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/ARM/libGLESv2.dll"), "./output/native/uwp/arm/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/Win32/libEGL.dll"), "./output/native/uwp/x86/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/Win32/libGLESv2.dll"), "./output/native/uwp/x86/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/x64/libEGL.dll"), "./output/native/uwp/x64/");
    CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/x64/libGLESv2.dll"), "./output/native/uwp/x64/");
});

// this builds the native C and C++ externals for Mac OS X
Task ("externals-macos")
    .IsDependentOn ("externals-osx");
Task ("externals-osx")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("osx") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("osx"))
    .WithCriteria (IsRunningOnMac ())
    .Does (() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        // generate native skia build files
        GnNinja ($"mac/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='mac' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-mmacosx-version-min=10.7', '-stdlib=libc++' ] " +
            $"extra_ldflags=[ '-Wl,macosx_version_min=10.7', '-stdlib=libc++' ]");

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists ($"output/native/osx/{arch}");
        CopyDirectory ($"native-builds/libSkiaSharp_osx/build/{CONFIGURATION}/", $"output/native/osx/{arch}");

        StripSign ($"output/native/osx/{arch}/libSkiaSharp.dylib");
    });

    buildArch ("x86_64", "x64");

    // create the fat dylib
    RunLipo ("output/native/osx/", "libSkiaSharp.dylib", new [] {
        (FilePath) "x86_64/libSkiaSharp.dylib"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((arch, skiaArch) => {
        // build libHarfBuzzSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libHarfBuzzSharp_osx/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists ($"output/native/osx/{arch}");
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_osx/build/{CONFIGURATION}/libHarfBuzzSharp.dylib", $"output/native/osx/{arch}");

        StripSign ($"output/native/osx/{arch}/libHarfBuzzSharp.dylib");
    });

    buildHarfBuzzArch ("x86_64", "x64");

    // create the fat dylib
    RunLipo ("output/native/osx/", "libHarfBuzzSharp.dylib", new [] {
        (FilePath) "x86_64/libHarfBuzzSharp.dylib"
    });
});

// this builds the native C and C++ externals for iOS
Task ("externals-ios")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("ios") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("ios"))
    .WithCriteria (IsRunningOnMac ())
    .Does (() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string, string> ((sdk, arch, skiaArch) => {
        // generate native skia build files
        GnNinja ($"ios/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='ios' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL', '-mios-version-min=8.0' ] " +
            $"extra_ldflags=[ '-Wl,ios_version_min=8.0' ]");

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists ($"output/native/ios/{arch}");
        CopyDirectory ($"native-builds/libSkiaSharp_ios/build/{CONFIGURATION}-{sdk}", $"output/native/ios/{arch}");

        StripSign ($"output/native/ios/{arch}/libSkiaSharp.framework");
    });

    buildArch ("iphonesimulator", "i386", "x86");
    buildArch ("iphonesimulator", "x86_64", "x64");
    buildArch ("iphoneos", "armv7", "arm");
    buildArch ("iphoneos", "arm64", "arm64");

    // create the fat framework
    CopyDirectory ("output/native/ios/armv7/libSkiaSharp.framework/", "output/native/ios/libSkiaSharp.framework/");
    DeleteFile ("output/native/ios/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("output/native/ios/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "armv7/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((sdk, arch) => {
        // build libHarfBuzzSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libHarfBuzzSharp_ios/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp_ios to output
        EnsureDirectoryExists ($"output/native/ios/{arch}");
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_ios/build/{CONFIGURATION}-{sdk}/libHarfBuzzSharp.a", $"output/native/ios/{arch}");

        StripSign ($"output/native/ios/{arch}/libHarfBuzzSharp.a");
    });

    buildHarfBuzzArch ("iphonesimulator", "i386");
    buildHarfBuzzArch ("iphonesimulator", "x86_64");
    buildHarfBuzzArch ("iphoneos", "armv7");
    buildHarfBuzzArch ("iphoneos", "arm64");

    // create the fat archive
    RunLipo ("output/native/ios/", "libHarfBuzzSharp.a", new [] {
        (FilePath) "i386/libHarfBuzzSharp.a",
        (FilePath) "x86_64/libHarfBuzzSharp.a",
        (FilePath) "armv7/libHarfBuzzSharp.a",
        (FilePath) "arm64/libHarfBuzzSharp.a"
    });
});

// this builds the native C and C++ externals for tvOS
Task ("externals-tvos")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("tvos") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("tvos"))
    .WithCriteria (IsRunningOnMac ())
    .Does (() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string, string> ((sdk, arch, skiaArch) => {
        // generate native skia build files
        GnNinja ($"tvos/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='tvos' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSK_BUILD_FOR_TVOS', '-DSKIA_C_DLL', '-mtvos-version-min=9.0' ] " +
            $"extra_ldflags=[ '-Wl,tvos_version_min=9.0' ]");

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_tvos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists ($"output/native/tvos/{arch}");
        CopyDirectory ($"native-builds/libSkiaSharp_tvos/build/{CONFIGURATION}-{sdk}", $"output/native/tvos/{arch}");

        StripSign ($"output/native/tvos/{arch}/libSkiaSharp.framework");
    });

    buildArch ("appletvsimulator", "x86_64", "x64");
    buildArch ("appletvos", "arm64", "arm64");

    // create the fat framework
    CopyDirectory ("output/native/tvos/arm64/libSkiaSharp.framework/", "output/native/tvos/libSkiaSharp.framework/");
    DeleteFile ("output/native/tvos/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("output/native/tvos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((sdk, arch) => {
        // build libHarfBuzzSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libHarfBuzzSharp_tvos/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists ($"output/native/tvos/{arch}");
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_tvos/build/{CONFIGURATION}-{sdk}/libHarfBuzzSharp.a", $"output/native/tvos/{arch}");

        StripSign ($"output/native/tvos/{arch}/libHarfBuzzSharp.a");
    });

    buildHarfBuzzArch ("appletvsimulator", "x86_64");
    buildHarfBuzzArch ("appletvos", "arm64");

    // create the fat framework
    RunLipo ("output/native/tvos/", "libHarfBuzzSharp.a", new [] {
        (FilePath) "x86_64/libHarfBuzzSharp.a",
        (FilePath) "arm64/libHarfBuzzSharp.a"
    });
});

// this builds the native C and C++ externals for watchOS
Task ("externals-watchos")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("watchos") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("watchos"))
    .WithCriteria (IsRunningOnMac ())
    .Does (() =>
{
    // SkiaSharp

    var buildArch = new Action<string, string, string> ((sdk, arch, skiaArch) => {
        // generate native skia build files
        GnNinja ($"watchos/{arch}", "skia",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='watchos' target_cpu='{skiaArch}' " +
            $"skia_enable_gpu=false " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSK_BUILD_FOR_WATCHOS', '-DSKIA_C_DLL', '-mwatchos-version-min=2.0' ] " +
            $"extra_ldflags=[ '-Wl,watchos_version_min=2.0' ]");

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_watchos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libSkiaSharp to output
        EnsureDirectoryExists ($"output/native/watchos/{arch}");
        CopyDirectory ($"native-builds/libSkiaSharp_watchos/build/{CONFIGURATION}-{sdk}", $"output/native/watchos/{arch}");

        StripSign ($"output/native/watchos/{arch}/libSkiaSharp.framework");
    });

    buildArch ("watchsimulator", "i386", "x86");
    buildArch ("watchos", "armv7k", "arm");
    buildArch ("watchos", "arm64_32", "arm64");

    // create the fat framework
    CopyDirectory ("output/native/watchos/armv7k/libSkiaSharp.framework/", "output/native/watchos/libSkiaSharp.framework/");
    DeleteFile ("output/native/watchos/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("output/native/watchos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "armv7k/libSkiaSharp.framework/libSkiaSharp",
        (FilePath) "arm64_32/libSkiaSharp.framework/libSkiaSharp"
    });

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((sdk, arch) => {
        // build libHarfBuzzSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libHarfBuzzSharp_watchos/libHarfBuzzSharp.xcodeproj",
            Target = "libHarfBuzzSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = CONFIGURATION,
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists ($"output/native/watchos/{arch}");
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_watchos/build/{CONFIGURATION}-{sdk}/libHarfBuzzSharp.a", $"output/native/watchos/{arch}");

        StripSign ($"output/native/watchos/{arch}/libHarfBuzzSharp.a");
    });

    buildHarfBuzzArch ("watchsimulator", "i386");
    buildHarfBuzzArch ("watchos", "armv7k");
    buildHarfBuzzArch ("watchos", "arm64_32");

    // create the fat framework
    RunLipo ("output/native/watchos/", "libHarfBuzzSharp.a", new [] {
        (FilePath) "i386/libHarfBuzzSharp.a",
        (FilePath) "armv7k/libHarfBuzzSharp.a",
        (FilePath) "arm64_32/libHarfBuzzSharp.a"
    });
});

// this builds the native C and C++ externals for Android
Task ("externals-android")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("android") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("android"))
    .WithCriteria (IsRunningOnMac () || IsRunningOnWindows ())
    .Does (() =>
{
    var cmd = IsRunningOnWindows () ? ".cmd" : "";
    var ndkbuild = ANDROID_NDK_HOME.CombineWithFilePath ($"ndk-build{cmd}").FullPath;

    // SkiaSharp

    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // generate native skia build files
        GnNinja ($"android/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='android' target_cpu='{skiaArch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"extra_cflags=[ '-DSKIA_C_DLL' ] " +
            $"ndk='{ANDROID_NDK_HOME}' " +
            $"ndk_api={(skiaArch == "x64" || skiaArch == "arm64" ? 21 : 9)}");

        var outDir = $"output/native/android/{arch}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ($"out/android/{arch}/libSkiaSharp.so"), outDir);
    });

    buildArch ("x86", "x86");
    buildArch ("x86_64", "x64");
    buildArch ("armeabi-v7a", "arm");
    buildArch ("arm64-v8a", "arm64");

    // HarfBuzzSharp

    var buildHarfBuzzArch = new Action<string> ((arch) => {
        if (!ShouldBuildArch (arch)) {
            Warning ($"Skipping architecture: {arch}.");
            return;
        }

        // build libHarfBuzzSharp
        RunProcess (ndkbuild, new ProcessSettings {
            Arguments = $"APP_ABI={arch}",
            WorkingDirectory = ROOT_PATH.Combine ("native-builds/libHarfBuzzSharp_android").FullPath,
        });

        // copy libSkiaSharp to output
        var outDir = $"output/native/android/{arch}";
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory ($"native-builds/libHarfBuzzSharp_android/libs/{arch}/libHarfBuzzSharp.so", outDir);
    });

    buildHarfBuzzArch ("x86");
    buildHarfBuzzArch ("x86_64");
    buildHarfBuzzArch ("armeabi-v7a");
    buildHarfBuzzArch ("arm64-v8a");
});

// this builds the native C and C++ externals for Linux
Task ("externals-linux")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("linux") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("linux"))
    .WithCriteria (IsRunningOnLinux ())
    .Does (() =>
{
    var SUPPORT_GPU = (EnvironmentVariable ("SUPPORT_GPU") ?? "1") == "1"; // 1 == true, 0 == false

    var CC = EnvironmentVariable ("CC");
    var CXX = EnvironmentVariable ("CXX");
    var AR = EnvironmentVariable ("AR");
    var CUSTOM_COMPILERS = "";
    if (!string.IsNullOrEmpty (CC))
        CUSTOM_COMPILERS += $"cc='{CC}' ";
    if (!string.IsNullOrEmpty (CXX))
        CUSTOM_COMPILERS += $"cxx='{CXX}' ";
    if (!string.IsNullOrEmpty (AR))
        CUSTOM_COMPILERS += $"ar='{AR}' ";

    // libSkiaSharp

    var buildArch = new Action<string> ((arch) => {
        var soname = GetVersion ("libSkiaSharp", "soname");

        // generate native skia build files
        GnNinja ($"linux/{arch}", "SkiaSharp",
            $"is_official_build=true skia_enable_tools=false " +
            $"target_os='linux' target_cpu='{arch}' " +
            $"skia_use_icu=false skia_use_sfntly=false skia_use_piex=true " +
            $"skia_use_system_expat=false skia_use_system_freetype2=false skia_use_system_libjpeg_turbo=false skia_use_system_libpng=false skia_use_system_libwebp=false skia_use_system_zlib=false " +
            $"skia_enable_gpu={(SUPPORT_GPU ? "true" : "false")} " +
            $"extra_cflags=[ '-DSKIA_C_DLL' ] " +
            $"extra_ldflags=[ '-static-libstdc++', '-static-libgcc', '-Wl,--version-script={ROOT_PATH.CombineWithFilePath("native-builds/libSkiaSharp_linux/libSkiaSharp.map")}' ] " +
            $"{CUSTOM_COMPILERS} " +
            $"linux_soname_version='{soname}'");

        // copy libSkiaSharp to output
        var outDir = $"output/native/linux/{arch}";
        var libSkiaSharp = SKIA_PATH.CombineWithFilePath ($"out/linux/{arch}/libSkiaSharp.so.{soname}");
        EnsureDirectoryExists (outDir);
        CopyFileToDirectory (libSkiaSharp, outDir);
        CopyFile (libSkiaSharp, $"{outDir}/libSkiaSharp.so");
    });

    buildArch ("x64");

    // libHarfBuzzSharp

    var buildHarfBuzzArch = new Action<string> ((arch) => {
        // build libHarfBuzzSharp
        // RunProcess ("make", new ProcessSettings {
        //     Arguments = "clean",
        //     WorkingDirectory = "native-builds/libHarfBuzzSharp_linux",
        // });
        RunProcess ("make", new ProcessSettings {
            Arguments = $"ARCH={arch} SONAME_VERSION={GetVersion ("HarfBuzz", "soname")} LDFLAGS=-static-libstdc++",
            WorkingDirectory = "native-builds/libHarfBuzzSharp_linux",
        });

        // copy libHarfBuzzSharp to output
        EnsureDirectoryExists ($"output/native/linux/{arch}");
        var so = $"native-builds/libHarfBuzzSharp_linux/bin/{arch}/libHarfBuzzSharp.so.{GetVersion ("HarfBuzz", "soname")}";
        CopyFileToDirectory (so, $"output/native/linux/{arch}");
        CopyFile (so, $"output/native/linux/{arch}/libHarfBuzzSharp.so");
    });

    buildHarfBuzzArch ("x64");
});

Task ("externals-tizen")
    .IsDependentOn ("externals-init")
    .IsDependeeOf (ShouldBuildExternal ("tizen") ? "externals-native" : "externals-native-skip")
    .WithCriteria (ShouldBuildExternal ("tizen"))
    .Does (() =>
{
    var bat = IsRunningOnWindows () ? ".bat" : "";
    var tizen = TIZEN_STUDIO_HOME.CombineWithFilePath ($"tools/ide/bin/tizen{bat}").FullPath;

    // libSkiaSharp

    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        // generate native skia build files
        GnNinja ($"tizen/{arch}", "skia",
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
        RunProcess (tizen, new ProcessSettings {
            Arguments = $"build-native -a {skiaArch} -c llvm -C {CONFIGURATION}" ,
            WorkingDirectory = ROOT_PATH.Combine ("native-builds/libSkiaSharp_tizen").FullPath,
        });

        // copy libSkiaSharp to output
        var outDir = $"output/native/tizen/{arch}";
        var libSkiaSharp = $"native-builds/libSkiaSharp_tizen/{CONFIGURATION}/libskiasharp.so";
        EnsureDirectoryExists (outDir);
        CopyFile (libSkiaSharp, $"{outDir}/libSkiaSharp.so");
    });

    buildArch ("armel", "arm");
    buildArch ("i386", "x86");

    // libHarfBuzzSharp

    var buildHarfBuzzArch = new Action<string, string> ((arch, skiaArch) => {
        // build libHarfBuzzSharp
        RunProcess(tizen, new ProcessSettings {
            Arguments = $"build-native -a {skiaArch} -c llvm -C {CONFIGURATION}",
            WorkingDirectory = ROOT_PATH.Combine ("native-builds/libHarfBuzzSharp_tizen").FullPath
        });

        // copy libHarfBuzzSharp to output
        var outDir = $"output/native/tizen/{arch}";
        var so = $"native-builds/libHarfBuzzSharp_tizen/{CONFIGURATION}/libharfbuzzsharp.so";
        EnsureDirectoryExists ($"output/native/tizen/{arch}");
        CopyFile (so, $"{outDir}/libHarfBuzzSharp.so");
    });

    buildHarfBuzzArch ("armel", "arm");
    buildHarfBuzzArch ("i386", "x86");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("externals-download")
    .IsDependentOn ("download-last-successful-build")
    .Does (() =>
{
    var artifactName = "native-default";
    var artifactFilename = $"{artifactName}.zip";
    var url = string.Format(AZURE_BUILD_URL, AZURE_BUILD_ID, artifactName);

    var outputPath = "./output";
    EnsureDirectoryExists (outputPath);
    CleanDirectories (outputPath);

    DownloadFile (url, $"{outputPath}/{artifactFilename}");
    Unzip ($"{outputPath}/{artifactFilename}", outputPath);
    MoveDirectory ($"{outputPath}/{artifactName}", $"{outputPath}/native");
});

Task ("externals-angle-uwp")
    .WithCriteria (!FileExists (ANGLE_PATH.CombineWithFilePath ("uwp/ANGLE.WindowsStore.nuspec")))
    .Does (() =>
{
    var id = "ANGLE.WindowsStore";
    var version = GetVersion (id, "release");
    var angleUrl = $"https://api.nuget.org/v3-flatcontainer/{id.ToLower ()}/{version}/{id.ToLower ()}.{version}.nupkg";
    var angleRoot = ANGLE_PATH.Combine ("uwp");
    var angleNupkg = angleRoot.CombineWithFilePath ($"angle_{version}.nupkg");

    EnsureDirectoryExists (angleRoot);
    CleanDirectory (angleRoot);

    DownloadFile (angleUrl, angleNupkg);
    Unzip (angleNupkg, angleRoot);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean-externals")
    .Does (() =>
{
    // skia
    CleanDirectories ("externals/skia/out");
    CleanDirectories ("externals/skia/xcodebuild");

    // angle
    CleanDirectories ("externals/angle");

    // all
    CleanDirectories ("output/native");
    // ios
    CleanDirectories ("native-builds/libSkiaSharp_ios/build");
    CleanDirectories ("native-builds/libHarfBuzzSharp_ios/build");
    // tvos
    CleanDirectories ("native-builds/libSkiaSharp_tvos/build");
    CleanDirectories ("native-builds/libHarfBuzzSharp_tvos/build");
    // watchos
    CleanDirectories ("native-builds/libSkiaSharp_watchos/build");
    CleanDirectories ("native-builds/libHarfBuzzSharp_watchos/build");
    // osx
    CleanDirectories ("native-builds/libSkiaSharp_osx/build");
    CleanDirectories ("native-builds/libHarfBuzzSharp_osx/build");
});
