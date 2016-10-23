
////////////////////////////////////////////////////////////////////////////////////////////////////
// TOOLS & FUNCTIONS - the bits to make it all work
////////////////////////////////////////////////////////////////////////////////////////////////////

// find a better place for this / or fix the path issue
var VisualStudioPathFixup = new Action (() => {
    var props = SKIA_PATH.CombineWithFilePath ("out/gyp/libjpeg-turbo.props").FullPath;
    var xdoc = XDocument.Load (props);
    var temp = xdoc.Root
        .Elements (MSBuildNS + "ItemDefinitionGroup")
        .Elements (MSBuildNS + "assemble")
        .Elements (MSBuildNS + "CommandLineTemplate")
        .Single ();
    var newInclude = SKIA_PATH.Combine ("third_party/externals/libjpeg-turbo/win/").FullPath;
    if (!temp.Value.Contains (newInclude)) {
        temp.Value += " \"-I" + newInclude + "\"";
        xdoc.Save (props);
    }
});

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
            if (string.IsNullOrEmpty (filePair.Value)) {
                contents.Insert (0, include);
            } else {
                contents.Insert (contents.IndexOf (filePair.Value), include);
            }
            FileWriteLines (file, contents.ToArray ());
        } else if (index != -1 && !inject) {
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
    });
    
    // generate the PCL
    FilePath input = "binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll";
    var libPath = "/Library/Frameworks/Mono.framework/Versions/Current/lib/mono/4.5/,.";
    StartProcess (GenApiToolPath, new ProcessSettings {
        Arguments = string.Format("-libPath:{2} -out \"{0}\" \"{1}\"", input.GetFilename () + ".cs", input.GetFilename (), libPath),
        WorkingDirectory = input.GetDirectory ().FullPath,
    });
    CopyFile ("binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll.cs", "binding/SkiaSharp.Portable/SkiaPortable.cs");
});

// this builds the native C and C++ externals 
Task ("externals-native")
    .IsDependentOn ("externals-uwp")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
    .IsDependentOn ("externals-ios")
    .IsDependentOn ("externals-tvos")
    .IsDependentOn ("externals-watchos")
    .IsDependentOn ("externals-android")
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
    if (IsRunningOnUnix ()) {
        if (!DirectoryExists ("./output/osx")) CreateDirectory ("./output/osx");
        if (!DirectoryExists ("./output/mac")) CreateDirectory ("./output/mac");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/osx/");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/mac/");
    }
});

// this builds the native C and C++ externals for Windows
Task ("externals-windows")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (
        !FileExists ("native-builds/lib/windows/x86/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/windows/x64/libSkiaSharp.dll"))
    .Does (() =>  
{
    var buildArch = new Action<string, string, string> ((platform, skiaArch, dir) => {
        RunGyp ("skia_arch_type='" + skiaArch + "' skia_gpu=1", "msvs");
        ProcessSolutionProjects ("native-builds/libSkiaSharp_windows/libSkiaSharp_" + dir + ".sln", (projectName, projectPath) => {
            if (projectName != "libSkiaSharp") {
                RedirectBuildOutputs (projectPath);
            }
        });
        VisualStudioPathFixup ();
        DotNetBuild ("native-builds/libSkiaSharp_windows/libSkiaSharp_" + dir + ".sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { platform };
        });
        if (!DirectoryExists ("native-builds/lib/windows/" + dir)) CreateDirectory ("native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + platform + "/Release/libSkiaSharp.lib", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + platform + "/Release/libSkiaSharp.dll", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/bin/" + platform + "/Release/libSkiaSharp.pdb", "native-builds/lib/windows/" + dir);
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    buildArch ("Win32", "x86", "x86");
    buildArch ("x64", "x86_64", "x64");
});

// this builds the native C and C++ externals for Windows UWP
Task ("externals-uwp")
    .IsDependentOn ("externals-angle-uwp")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (
        !FileExists ("native-builds/lib/uwp/ARM/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/uwp/x86/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/uwp/x64/libSkiaSharp.dll"))
    .Does (() =>  
{
    var buildArch = new Action<string, string> ((platform, arch) => {
        ProcessSolutionProjects ("native-builds/libSkiaSharp_uwp/libSkiaSharp_" + arch + ".sln", (projectName, projectPath) => {
            if (projectName != "libSkiaSharp") {
                RedirectBuildOutputs (projectPath);
                TransformToUWP (projectPath, platform);
            }
        });
        InjectCompatibilityExternals (true);
        VisualStudioPathFixup ();
        DotNetBuild ("native-builds/libSkiaSharp_uwp/libSkiaSharp_" + arch + ".sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { platform };
        });
        if (!DirectoryExists ("native-builds/lib/uwp/" + arch)) CreateDirectory ("native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + platform + "/Release/libSkiaSharp.lib", "native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + platform + "/Release/libSkiaSharp.dll", "native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/bin/" + platform + "/Release/libSkiaSharp.pdb", "native-builds/lib/uwp/" + arch);
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);

    RunGyp ("skia_arch_type='x86_64' skia_gpu=1", "msvs");
    buildArch ("x64", "x64");
    
    RunGyp ("skia_arch_type='x86' skia_gpu=1", "msvs");
    buildArch ("Win32", "x86");
    
    RunGyp ("skia_arch_type='arm' arm_version=7 arm_neon=0 skia_gpu=1", "msvs");
    buildArch ("ARM", "arm");
});

// this builds the native C and C++ externals for Mac OS X
Task ("externals-osx")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/osx/libSkiaSharp.dylib"))
    .Does (() =>  
{
    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        RunGyp ("skia_arch_type='" + skiaArch + "' skia_gpu=1 skia_osx_deployment_target=10.8", "xcode");
        
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_osx/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = "macosx",
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/osx/" + arch)) {
            CreateDirectory ("native-builds/lib/osx/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_osx/build/Release/", "native-builds/lib/osx/" + arch);
        RunInstallNameTool ("native-builds/lib/osx/" + arch, "lib/libSkiaSharp.dylib", "@loader_path/libSkiaSharp.dylib", "libSkiaSharp.dylib");
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    buildArch ("i386", "x86");
    buildArch ("x86_64", "x86_64");
    
    // create the fat dylib
    RunLipo ("native-builds/lib/osx/", "libSkiaSharp.dylib", new [] {
        (FilePath) "i386/libSkiaSharp.dylib", 
        (FilePath) "x86_64/libSkiaSharp.dylib"
    });
});

// this builds the native C and C++ externals for watchOS
Task ("externals-watchos")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/watchos/libSkiaSharp.framework/libSkiaSharp"))
    .Does (() => 
{
    var buildArch = new Action<string, string> ((sdk, arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_watchos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/watchos/" + arch)) {
            CreateDirectory ("native-builds/lib/watchos/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_watchos/build/Release-" + sdk, "native-builds/lib/watchos/" + arch);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    RunGyp ("skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0 skia_gpu=1 ios_sdk_version=3.0", "xcode");
    TransformToWatchOS ("./externals/skia/out/gyp");
    
    buildArch ("watchsimulator", "i386");
    // buildArch ("watchos", "armv7k");
    
    // // create the fat framework
    // CopyDirectory ("native-builds/lib/watchos/arm64/libSkiaSharp.framework/", "native-builds/lib/watchos/libSkiaSharp.framework/");
    // DeleteFile ("native-builds/lib/watchos/libSkiaSharp.framework/libSkiaSharp");
    // RunLipo ("native-builds/lib/watchos/", "libSkiaSharp.framework/libSkiaSharp", new [] {
    //     (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp", 
    //     (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    // }); 
}); 

// this builds the native C and C++ externals for iOS
Task ("externals-ios")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/ios/libSkiaSharp.framework/libSkiaSharp"))
    .Does (() => 
{
    var buildArch = new Action<string, string> ((sdk, arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_ios/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/ios/" + arch)) {
            CreateDirectory ("native-builds/lib/ios/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_ios/build/Release-" + sdk, "native-builds/lib/ios/" + arch);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    RunGyp ("skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0 skia_gpu=1 ios_sdk_version=8.0", "xcode");
    
    buildArch ("iphonesimulator", "i386");
    buildArch ("iphonesimulator", "x86_64");
    buildArch ("iphoneos", "armv7");
    buildArch ("iphoneos", "armv7s");
    buildArch ("iphoneos", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/ios/armv7/libSkiaSharp.framework/", "native-builds/lib/ios/libSkiaSharp.framework/");
    DeleteFile ("native-builds/lib/ios/libSkiaSharp.framework/libSkiaSharp");
    RunLipo ("native-builds/lib/ios/", "libSkiaSharp.framework/libSkiaSharp", new [] {
        (FilePath) "i386/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "x86_64/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "armv7/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "armv7s/libSkiaSharp.framework/libSkiaSharp", 
        (FilePath) "arm64/libSkiaSharp.framework/libSkiaSharp"
    });
});

// this builds the native C and C++ externals for tvOS
Task ("externals-tvos")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/tvos/libSkiaSharp.framework/libSkiaSharp"))
    .Does (() => 
{
    var buildArch = new Action<string, string> ((sdk, arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libSkiaSharp_tvos/libSkiaSharp.xcodeproj",
            Target = "libSkiaSharp",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/tvos/" + arch)) {
            CreateDirectory ("native-builds/lib/tvos/" + arch);
        }
        CopyDirectory ("native-builds/libSkiaSharp_tvos/build/Release-" + sdk, "native-builds/lib/tvos/" + arch);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    RunGyp ("skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0 skia_gpu=1 ios_sdk_version=9.0", "xcode");
    TransformToTvOS ("./externals/skia/out/gyp");
    
    buildArch ("appletvsimulator", "x86_64");
    buildArch ("appletvos", "arm64");
    
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
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/android/x86/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/x86_64/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/armeabi-v7a/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/arm64-v8a/libSkiaSharp.so"))
    .Does (() => 
{
    var ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-sdk-macosx";
    var ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? ANDROID_HOME;
    var ANDROID_NDK_HOME = EnvironmentVariable ("ANDROID_NDK_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-ndk";
    
    var buildArch = new Action<string, string> ((arch, folder) => {
        StartProcess (SKIA_PATH.CombineWithFilePath ("platform_tools/android/bin/android_ninja").FullPath, new ProcessSettings {
            Arguments = "-d " + arch + " skia_lib pdf sfntly icuuc",
            WorkingDirectory = SKIA_PATH.FullPath,
        });
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_DEFINES", "");
    SetEnvironmentVariable ("GYP_GENERATORS", "");
    SetEnvironmentVariable ("BUILDTYPE", "Release");
    SetEnvironmentVariable ("ANDROID_HOME", ANDROID_HOME);
    SetEnvironmentVariable ("ANDROID_SDK_ROOT", ANDROID_SDK_ROOT);
    SetEnvironmentVariable ("ANDROID_NDK_HOME", ANDROID_NDK_HOME);
    
    SetEnvironmentVariable ("GYP_DEFINES", "skia_gpu=1");
    buildArch ("x86", "x86");
    SetEnvironmentVariable ("GYP_DEFINES", "skia_gpu=1");
    buildArch ("x86_64", "x86_64");
    SetEnvironmentVariable ("GYP_DEFINES", "arm_neon=1 arm_version=7 skia_gpu=1");
    buildArch ("arm_v7_neon", "armeabi-v7a");
    SetEnvironmentVariable ("GYP_DEFINES", "arm_neon=0 arm_version=8 skia_gpu=1");
    buildArch ("arm64", "arm64-v8a");
        
    var ndkbuild = MakeAbsolute (Directory (ANDROID_NDK_HOME)).CombineWithFilePath ("ndk-build").FullPath;
    StartProcess (ndkbuild, new ProcessSettings {
        Arguments = "",
        WorkingDirectory = ROOT_PATH.Combine ("native-builds/libSkiaSharp_android").FullPath,
    }); 

    foreach (var folder in new [] { "x86", "x86_64", "armeabi-v7a", "arm64-v8a" }) {
        if (!DirectoryExists ("native-builds/lib/android/" + folder)) {
            CreateDirectory ("native-builds/lib/android/" + folder);
        }
        CopyFileToDirectory ("native-builds/libSkiaSharp_android/libs/" + folder + "/libSkiaSharp.so", "native-builds/lib/android/" + folder);
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
    var angleVersion = "2.1.10";
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

Task ("clean-externals").Does (() =>
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
    // watchos
    CleanDirectories ("native-builds/libSkiaSharp_watchos/build");
    // osx
    CleanDirectories ("native-builds/libSkiaSharp_osx/build");
    // windows
    CleanDirectories ("native-builds/libSkiaSharp_windows/bin");
    CleanDirectories ("native-builds/libSkiaSharp_windows/obj");
    // uwp
    CleanDirectories ("native-builds/libSkiaSharp_uwp/bin");
    CleanDirectories ("native-builds/libSkiaSharp_uwp/obj");
    CleanDirectories ("externals/angle/uwp");
    
    // remove compatibility
    InjectCompatibilityExternals (false);
});
