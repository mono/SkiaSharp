
////////////////////////////////////////////////////////////////////////////////////////////////////
// TOOLS & FUNCTIONS - the bits to make it all work
////////////////////////////////////////////////////////////////////////////////////////////////////

var InjectCompatibilityExternals = new Action<bool> ((inject) => {
    // some methods don't yet exist, so we must add the compat layer to them.
    // we need this as we can't modify the third party files
    // all we do is insert our header before all the others
    var compatHeader = "native-builds/src/WinRTCompat.h";
    var compatSource = "native-builds/src/WinRTCompat.c";
    var files = new Dictionary<FilePath, string> { 
        { "externals/skia/third_party/externals/dng_sdk/source/dng_string.cpp", "#if qWinOS" },
        { "externals/skia/third_party/externals/dng_sdk/source/dng_utils.cpp", "#if qWinOS" },
        { "externals/skia/third_party/externals/dng_sdk/source/dng_pthread.cpp", "#if qWinOS" },
        { "externals/skia/third_party/externals/zlib/deflate.c", "#include <assert.h>" },
        { "externals/skia/third_party/externals/libjpeg-turbo/simd/jsimd_x86_64.c", "#define JPEG_INTERNALS" },
        { "externals/skia/third_party/externals/libjpeg-turbo/simd/jsimd_i386.c", "#define JPEG_INTERNALS" },
        { "externals/skia/third_party/externals/libjpeg-turbo/simd/jsimd_arm.c", "#define JPEG_INTERNALS" },
        { "externals/skia/third_party/externals/libjpeg-turbo/simd/jsimd_arm64.c", "#define JPEG_INTERNALS" },
    };
    foreach (var filePair in files) {
        var file = filePair.Key;

        if (!FileExists (file))
            continue;

        var root = string.Join ("/", file.GetDirectory ().Segments.Select (x => ".."));
        var include = "#include \"" + root + "/" + compatHeader + "\"";
        
        var contents = FileReadLines (file).ToList ();
        var index = contents.IndexOf (include);
        if (index == -1 && inject) {
            Information ("Injecting modifications into third party code: {0}...", file);
            if (string.IsNullOrEmpty (filePair.Value)) {
                contents.Insert (0, include);
            } else {
                contents.Insert (contents.IndexOf (filePair.Value), include);
            }
            FileWriteLines (file, contents.ToArray ());
        } else if (index != -1 && !inject) {
            Information ("Removing injected modifications from third party code: {0}...", file);
            int idx = 0;
            if (string.IsNullOrEmpty (filePair.Value)) {
                idx = 0;
            } else {
                idx = contents.IndexOf (filePair.Value) - 1;
            }
            if (contents [idx] == include) {
                contents.RemoveAt (idx);
            }
            FileWriteLines (file, contents.ToArray ());
        }
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds the managed PCL external 
Task ("externals-genapi")
    .Does (() => 
{
    // build the dummy project
    DotNetBuild ("binding/SkiaSharp.Generic.sln", c => { 
        c.Configuration = "Release"; 
        c.Properties ["Platform"] = new [] { "\"Any CPU\"" };
        c.Verbosity = VERBOSITY;
    });
    
    // generate the PCL
    FilePath input = "binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll";
    var libPath = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/,.";
    RunProcess (GenApiToolPath, new ProcessSettings {
        Arguments = string.Format("-libPath:{2} -out \"{0}\" \"{1}\"", input.GetFilename () + ".cs", input.GetFilename (), libPath),
        WorkingDirectory = input.GetDirectory ().FullPath,
    });
    // bug in the generator whicj doesn't use enums in attributes
    ReplaceTextInFiles ("binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll.cs", "[System.ComponentModel.EditorBrowsableAttribute(1)]", "[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)]");
    CopyFile ("binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll.cs", "binding/SkiaSharp.Portable/SkiaPortable.cs");
});

Task ("externals-init")
    .Does (() =>  
{
    RunProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath ("tools/git-sync-deps").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });

    // insert compatibility modifications for external code
    InjectCompatibilityExternals (true);
});

// this builds the native C and C++ externals 
Task ("externals-native")
    .IsDependentOn ("externals-uwp")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
    .IsDependentOn ("externals-ios")
    .IsDependentOn ("externals-tvos")
    .IsDependentOn ("externals-android")
    .IsDependentOn ("externals-linux")
    .Does (() => 
{
    // copy all the native files into the output
    CopyDirectory ("./native-builds/lib/", "./output/native/");
    
    // copy the non-embedded native files into the output
    if (IsRunningOnWindows ()) {
        if (!DirectoryExists ("./output/windows/x86")) CreateDirectory ("./output/windows/x86");
        if (!DirectoryExists ("./output/windows/x64")) CreateDirectory ("./output/windows/x64");
        CopyFileToDirectory ("./native-builds/lib/windows/x86/libSkiaSharp.dll", "./output/windows/x86/");
        CopyFileToDirectory ("./native-builds/lib/windows/x86/libSkiaSharp.pdb", "./output/windows/x86/");
        CopyFileToDirectory ("./native-builds/lib/windows/x64/libSkiaSharp.dll", "./output/windows/x64/");
        CopyFileToDirectory ("./native-builds/lib/windows/x64/libSkiaSharp.pdb", "./output/windows/x64/");
        if (!DirectoryExists ("./output/uwp/x86")) CreateDirectory ("./output/uwp/x86");
        if (!DirectoryExists ("./output/uwp/x64")) CreateDirectory ("./output/uwp/x64");
        if (!DirectoryExists ("./output/uwp/arm")) CreateDirectory ("./output/uwp/arm");
        CopyFileToDirectory ("./native-builds/lib/uwp/x86/libSkiaSharp.dll", "./output/uwp/x86/");
        CopyFileToDirectory ("./native-builds/lib/uwp/x86/libSkiaSharp.pdb", "./output/uwp/x86/");
        CopyFileToDirectory ("./native-builds/lib/uwp/x64/libSkiaSharp.dll", "./output/uwp/x64/");
        CopyFileToDirectory ("./native-builds/lib/uwp/x64/libSkiaSharp.pdb", "./output/uwp/x64/");
        CopyFileToDirectory ("./native-builds/lib/uwp/arm/libSkiaSharp.dll", "./output/uwp/arm/");
        CopyFileToDirectory ("./native-builds/lib/uwp/arm/libSkiaSharp.pdb", "./output/uwp/arm/");
        // copy ANGLE externals
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/ARM/libEGL.dll"), "./output/uwp/arm/");
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/ARM/libGLESv2.dll"), "./output/uwp/arm/");
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/Win32/libEGL.dll"), "./output/uwp/x86/");
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/Win32/libGLESv2.dll"), "./output/uwp/x86/");
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/x64/libEGL.dll"), "./output/uwp/x64/");
        CopyFileToDirectory (ANGLE_PATH.CombineWithFilePath ("uwp/bin/UAP/x64/libGLESv2.dll"), "./output/uwp/x64/");
    }
    if (IsRunningOnMac ()) {
        if (!DirectoryExists ("./output/osx")) CreateDirectory ("./output/osx");
        if (!DirectoryExists ("./output/mac")) CreateDirectory ("./output/mac");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/osx/");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/mac/");
    }
    if (IsRunningOnLinux ()) {
        if (!DirectoryExists ("./output/linux/x64/")) CreateDirectory ("./output/linux/x64/");
        if (!DirectoryExists ("./output/linux/x86/")) CreateDirectory ("./output/linux/x86/");
        if (!DirectoryExists ("./output/linux/arm/")) CreateDirectory ("./output/linux/arm/");
        foreach (var arch in new [] { "x64", "x86", "arm" }) {
            var so = "./native-builds/lib/linux/" + arch + "/libSkiaSharp.so." + VERSION_SONAME;
            if (FileExists (so)) {
                CopyFileToDirectory (so, "./output/linux/" + arch + "/");
                // the second copy excludes the file version
                CopyFile (so, "./output/linux/" + arch + "/libSkiaSharp.so");
            }
        }
    }
});

// this builds the native C and C++ externals for Windows
Task ("externals-windows")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnWindows ())
    .Does (() =>  
{
    var buildArch = new Action<string, string, string> ((arch, skiaArch, dir) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn.exe"), new ProcessSettings {
            Arguments = 
                "gen out/win/" + arch + " " + 
                "--args=\"" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\\\"win\\\" target_cpu=\\\"" + skiaArch + "\\\"" +
                "  skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true" +
                "  extra_cflags=[ \\\"-DSKIA_C_DLL\\\", \\\"/MD\\\", \\\"/EHsc\\\" ]" +
                "  extra_ldflags=[ ]" +
                "\"",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja.exe"), new ProcessSettings {
            Arguments = "-C out/win/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        MSBuild ("native-builds/libSkiaSharp_windows/libSkiaSharp.sln", new MSBuildSettings { 
            Configuration = "Release",
            PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), arch),
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/windows/" + dir)) CreateDirectory ("native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + arch + "/Release/libSkiaSharp.lib", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + arch + "/Release/libSkiaSharp.dll", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + arch + "/Release/libSkiaSharp.pdb", "native-builds/lib/windows/" + dir);
    });

    buildArch ("Win32", "x86", "x86");
    buildArch ("x64", "x64", "x64");
});

// this builds the native C and C++ externals for Windows UWP
Task ("externals-uwp")
    .IsDependentOn ("externals-angle-uwp")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnWindows ())
    .Does (() =>  
{
    var buildArch = new Action<string, string, string> ((arch, skiaArch, dir) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn.exe"), new ProcessSettings {
            Arguments = 
                "gen out/winrt/" + arch + " " + 
                "--args=\"" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\\\"winrt\\\" target_cpu=\\\"" + skiaArch + "\\\"" +
                "  skia_use_icu=false skia_use_sfntly=false skia_use_piex=true skia_use_dng_sdk=true" +
                "  extra_cflags=[ " + 
                "    \\\"-DSKIA_C_DLL\\\", \\\"/MD\\\", \\\"/EHsc\\\", " + 
                "    \\\"-DWINAPI_FAMILY=WINAPI_FAMILY_APP\\\", \\\"-DSK_BUILD_FOR_WINRT\\\", \\\"-DSK_HAS_DWRITE_1_H\\\", \\\"-DSK_HAS_DWRITE_2_H\\\", \\\"-DNO_GETENV\\\" ]" +
                "  extra_ldflags=[ \\\"/APPCONTAINER\\\" ]" +
                "\"",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja.exe"), new ProcessSettings {
            Arguments = "-C out/winrt/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        MSBuild ("native-builds/libSkiaSharp_uwp/libSkiaSharp.sln", new MSBuildSettings { 
            Configuration = "Release",
            PlatformTarget = (PlatformTarget)Enum.Parse(typeof(PlatformTarget), arch),
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/uwp/" + dir)) CreateDirectory ("native-builds/lib/uwp/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + arch + "/Release/libSkiaSharp.lib", "native-builds/lib/uwp/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + arch + "/Release/libSkiaSharp.dll", "native-builds/lib/uwp/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + arch + "/Release/libSkiaSharp.pdb", "native-builds/lib/uwp/" + dir);
    });

    buildArch ("x64", "x64", "x64");
    buildArch ("Win32", "x86", "x86");
    buildArch ("ARM", "arm", "ARM");
});

// this builds the native C and C++ externals for Mac OS X
Task ("externals-osx")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnMac ())
    .Does (() =>  
{
    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn"), new ProcessSettings {
            Arguments = 
                "gen out/mac/" + arch + " " + 
                "--args='" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\"mac\" target_cpu=\"" + skiaArch + "\"" +
                "  skia_use_icu=false skia_use_sfntly=false" +
                "  extra_cflags=[ \"-DSKIA_C_DLL\", \"-ffunction-sections\", \"-fdata-sections\", \"-mmacosx-version-min=10.9\" ]" +
                "  extra_ldflags=[ \"-Wl,macosx_version_min=10.9\" ]" +
                "'",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja"), new ProcessSettings {
            Arguments = "-C out/mac/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = "Release",
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/osx/" + arch)) {
            CreateDirectory ("native-builds/lib/osx/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_osx/build/Release/", "native-builds/lib/osx/" + arch);

        // strip anything we can
        RunProcess ("strip", new ProcessSettings {
            Arguments = "-x -S libSkiaSharp.dylib",
            WorkingDirectory = "native-builds/lib/osx/" + arch,
        });

        // re-sign with empty
        RunProcess ("codesign", new ProcessSettings {
            Arguments = "--force --sign - --timestamp=none libSkiaSharp.dylib",
            WorkingDirectory = "native-builds/lib/osx/" + arch,
        });
    });

    buildArch ("i386", "x86");
    buildArch ("x86_64", "x64");
    
    // create the fat dylib
    RunLipo ("native-builds/lib/osx/", "libSkiaSharp.dylib", new [] {
        (FilePath) "i386/libSkiaSharp.dylib", 
        (FilePath) "x86_64/libSkiaSharp.dylib"
    });
});

// this builds the native C and C++ externals for iOS
Task ("externals-ios")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnMac ())
    .Does (() => 
{
    var buildArch = new Action<string, string, string> ((sdk, arch, skiaArch) => {
        // generate native skia build files

        var specifics = "";
        // several instances of "error: type 'XXX' requires 8 bytes of alignment and the default allocator only guarantees 4 bytes [-Werror,-Wover-aligned]
        // https://groups.google.com/forum/#!topic/skia-discuss/hU1IPFwU6bI
        if (arch == "armv7" || arch == "armv7s") {
            specifics += ", \"-Wno-over-aligned\"";
        }

        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn"), new ProcessSettings {
            Arguments = 
                "gen out/ios/" + arch + " " + 
                "--args='" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\"ios\" target_cpu=\"" + skiaArch + "\"" +
                "  skia_use_icu=false skia_use_sfntly=false" +
                "  extra_cflags=[ \"-DSKIA_C_DLL\", \"-ffunction-sections\", \"-fdata-sections\", \"-mios-version-min=8.0\" " + specifics + " ]" +
                "  extra_ldflags=[ \"-Wl,ios_version_min=8.0\" ]" +
                "'",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja"), new ProcessSettings {
            Arguments = "-C out/ios/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/ios/" + arch)) {
            CreateDirectory ("native-builds/lib/ios/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_ios/build/Release-" + sdk, "native-builds/lib/ios/" + arch);

        // strip anything we can
        RunProcess ("strip", new ProcessSettings {
            Arguments = "-x -S libSkiaSharp",
            WorkingDirectory = "native-builds/lib/ios/" + arch + "/libSkiaSharp.framework",
        });

        // re-sign with empty
        RunProcess ("codesign", new ProcessSettings {
            Arguments = "--force --sign - --timestamp=none libSkiaSharp.framework",
            WorkingDirectory = "native-builds/lib/ios/" + arch,
        });
    });

    buildArch ("iphonesimulator", "i386", "x86");
    buildArch ("iphonesimulator", "x86_64", "x64");
    buildArch ("iphoneos", "armv7", "arm");
    buildArch ("iphoneos", "arm64", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/ios/armv7/libSkiaSharp.framework/", "native-builds/lib/ios/libSkiaSharp.framework/");
    DeleteFile ("native-builds/lib/ios/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("native-builds/lib/ios/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "armv7/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });
});

// this builds the native C and C++ externals for tvOS
Task ("externals-tvos")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnMac ())
    .Does (() => 
{
    var buildArch = new Action<string, string, string> ((sdk, arch, skiaArch) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn"), new ProcessSettings {
            Arguments = 
                "gen out/tvos/" + arch + " " + 
                "--args='" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\"tvos\" target_cpu=\"" + skiaArch + "\"" +
                "  skia_use_icu=false skia_use_sfntly=false" +
                "  extra_cflags=[ \"-DSKIA_C_DLL\", \"-mtvos-version-min=9.0\" ]" +
                "  extra_ldflags=[ \"-Wl,tvos_version_min=9.0\" ]" +
                "'",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja"), new ProcessSettings {
            Arguments = "-C out/tvos/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_tvos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/tvos/" + arch)) {
            CreateDirectory ("native-builds/lib/tvos/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_tvos/build/Release-" + sdk, "native-builds/lib/tvos/" + arch);

        // strip anything we can
        RunProcess ("strip", new ProcessSettings {
            Arguments = "-x -S libSkiaSharp",
            WorkingDirectory = "native-builds/lib/tvos/" + arch + "/libSkiaSharp.framework",
        });

        // re-sign with empty
        RunProcess ("codesign", new ProcessSettings {
            Arguments = "--force --sign - --timestamp=none libSkiaSharp.framework",
            WorkingDirectory = "native-builds/lib/tvos/" + arch,
        });
    });

    buildArch ("appletvsimulator", "x86_64", "x64");
    buildArch ("appletvos", "arm64", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/tvos/arm64/libSkiaSharp.framework/", "native-builds/lib/tvos/libSkiaSharp.framework/");
    DeleteFile ("native-builds/lib/tvos/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("native-builds/lib/tvos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });
});

// this builds the native C and C++ externals for Android
Task ("externals-android")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnMac ())
    .Does (() => 
{
    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn"), new ProcessSettings {
            Arguments = 
                "gen out/android/" + arch + " " + 
                "--args='" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\"android\" target_cpu=\"" + skiaArch + "\"" +
                "  skia_use_icu=false skia_use_sfntly=false" +
                "  extra_cflags=[ \"-DSKIA_C_DLL\", \"-ffunction-sections\", \"-fdata-sections\" ]" +
                "  ndk=\"" + ANDROID_NDK_HOME + "\"" + 
                "  ndk_api=" + (skiaArch == "x64" || skiaArch == "arm64" ? 21 : 9) +
                "'",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja"), new ProcessSettings {
            Arguments = "-C out/android/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });
    });

    buildArch ("x86", "x86");
    buildArch ("x86_64", "x64");
    buildArch ("armeabi-v7a", "arm");
    buildArch ("arm64-v8a", "arm64");

    // build libSkiaSharp
    var ndkbuild = MakeAbsolute (Directory (ANDROID_NDK_HOME)).CombineWithFilePath ("ndk-build").FullPath;
    RunProcess (ndkbuild, new ProcessSettings {
        Arguments = "",
        WorkingDirectory = ROOT_PATH.Combine ("native-builds/libSkiaSharp_android").FullPath,
    }); 

    // copy libSkiaSharp to output
    foreach (var folder in new [] { "x86", "x86_64", "armeabi-v7a", "arm64-v8a" }) {
        if (!DirectoryExists ("native-builds/lib/android/" + folder)) {
            CreateDirectory ("native-builds/lib/android/" + folder);
        }
        CopyFileToDirectory ("native-builds/libSkiaSharp_android/libs/" + folder + "/libSkiaSharp.so", "native-builds/lib/android/" + folder);
    }
});

// this builds the native C and C++ externals for Linux
Task ("externals-linux")
    .IsDependentOn ("externals-init")
    .WithCriteria (IsRunningOnLinux ())
    .Does (() => 
{
    var arches = EnvironmentVariable ("BUILD_ARCH") ?? (Environment.Is64BitOperatingSystem ? "x64" : "x86");  // x64, x86, ARM
    var BUILD_ARCH = arches.Split (',').Select (a => a.Trim ()).ToArray ();
    var SUPPORT_GPU = (EnvironmentVariable ("SUPPORT_GPU") ?? "1") == "1"; // 1 == true, 0 == false

    var buildArch = new Action<string> ((arch) => {
        // generate native skia build files
        RunProcess (SKIA_PATH.CombineWithFilePath("bin/gn"), new ProcessSettings {
            Arguments = 
                "gen out/linux/" + arch + " " + 
                "--args='" +
                "  is_official_build=true skia_enable_tools=false" +
                "  target_os=\"linux\" target_cpu=\"" + arch + "\"" +
                "  skia_use_icu=false skia_use_sfntly=false skia_use_system_freetype2=false" +
                "  skia_enable_gpu=" + (SUPPORT_GPU ? "true" : "false") +
                "  extra_cflags=[ \"-DSKIA_C_DLL\", \"-ffunction-sections\", \"-fdata-sections\" ]" +
                "  extra_ldflags=[ ]" +
                "'",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build native skia
        RunProcess (DEPOT_PATH.CombineWithFilePath ("ninja"), new ProcessSettings {
            Arguments = "-C out/linux/" + arch,
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        // build libSkiaSharp
        // RunProcess ("make", new ProcessSettings {
        //     Arguments = "clean",
        //     WorkingDirectory = "native-builds/libSkiaSharp_linux",
        // });
        RunProcess ("make", new ProcessSettings {
            Arguments = "ARCH=" + arch + " VERSION=" + VERSION_FILE + " SUPPORT_GPU=" + SUPPORT_GPU,
            WorkingDirectory = "native-builds/libSkiaSharp_linux",
        });

        // copy libSkiaSharp to output
        if (!DirectoryExists ("native-builds/lib/linux/" + arch)) {
            CreateDirectory ("native-builds/lib/linux/" + arch);
        }
        CopyFileToDirectory ("native-builds/libSkiaSharp_linux/bin/" + arch + "/libSkiaSharp.so." + VERSION_SONAME, "native-builds/lib/linux/" + arch);
    });

    foreach (var arch in BUILD_ARCH) {
        buildArch (arch);
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS DOWNLOAD - download any externals that are needed
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("externals-angle-uwp")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (!FileExists (ANGLE_PATH.CombineWithFilePath ("uwp/ANGLE.WindowsStore.nuspec")))
    .Does (() =>  
{
    var angleVersion = "2.1.13";
    var angleUrl = "https://www.nuget.org/api/v2/package/ANGLE.WindowsStore/" + angleVersion;
    var angleRoot = ANGLE_PATH.Combine ("uwp");
    var angleNupkg = angleRoot.CombineWithFilePath ("angle_" + angleVersion + ".nupkg");

    if (!DirectoryExists (angleRoot)) {
        CreateDirectory (angleRoot);
    } else {
        CleanDirectory (angleRoot);
    }
    DownloadFile (angleUrl, angleNupkg);
    Unzip (angleNupkg, angleRoot);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("externals-deinit").Does (() =>
{
    // remove compatibility
    InjectCompatibilityExternals (false);
});

Task ("clean-externals")
    .IsDependentOn ("externals-deinit")
    .Does (() =>
{
    // skia
    CleanDirectories ("externals/skia/out");
    CleanDirectories ("externals/skia/xcodebuild");

    // all
    CleanDirectories ("native-builds/lib");
    // android
    CleanDirectories ("native-builds/libSkiaSharp_android/obj");
    CleanDirectories ("native-builds/libSkiaSharp_android/libs");
    // ios
    CleanDirectories ("native-builds/libSkiaSharp_ios/build");
    // tvos
    CleanDirectories ("native-builds/libSkiaSharp_tvos/build");
    // osx
    CleanDirectories ("native-builds/libSkiaSharp_osx/build");
    // windows
    CleanDirectories ("native-builds/libSkiaSharp_windows/bin");
    CleanDirectories ("native-builds/libSkiaSharp_windows/obj");
    // uwp
    CleanDirectories ("native-builds/libSkiaSharp_uwp/bin");
    CleanDirectories ("native-builds/libSkiaSharp_uwp/obj");
    CleanDirectories ("externals/angle/uwp");
    // linux
    CleanDirectories ("native-builds/libSkiaSharp_linux/bin");
    CleanDirectories ("native-builds/libSkiaSharp_linux/obj");
});
