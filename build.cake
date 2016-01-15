#addin "Cake.Xamarin"
#addin "Cake.XCode"

#load "common.cake"

using System.Xml;
using System.Xml.Linq;

var ROOT_PATH = MakeAbsolute(File(".")).GetDirectory();
var DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("depot_tools"));
var SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("skia"));

var RunGyp = new Action (() => {
    StartProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath("bin/sync-and-gyp").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
});

var SetEnvironmentVariable = new Action<string, string> ((name, value) => {
    Environment.SetEnvironmentVariable (name, value, EnvironmentVariableTarget.Process);
});
var AppendEnvironmentVariable = new Action<string, string> ((name, value) => {
    var old = EnvironmentVariable (name);
    value += (IsRunningOnWindows () ? ";" : ":") + old;
    SetEnvironmentVariable (name, value);
});


CakeSpec.Libs = new ISolutionBuilder [] {
	new IOSSolutionBuilder {
		SolutionPath = "binding/SkiaSharp.sln",
        IsWindowsCompatible = false,
        IsMacCompatible = true,
		OutputFiles = new [] { 
			new OutputFileCopy {
				FromFile = "./binding/SkiaSharp.Android/bin/Release/SkiaSharp.dll",
				ToDirectory = "./output/android/"
			},
			new OutputFileCopy {
				FromFile = "./binding/SkiaSharp.iOS/bin/Release/SkiaSharp.dll",
				ToDirectory = "./output/ios/"
			},
			new OutputFileCopy {
				FromFile = "./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.dll",
				ToDirectory = "./output/osx/"
			},
			new OutputFileCopy {
				FromFile = "./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll",
				ToDirectory = "./output/portable/"
			},
		}
	},	
	new DefaultSolutionBuilder {
		SolutionPath = "binding/SkiaSharp.Windows.sln",
        IsWindowsCompatible = true,
        IsMacCompatible = false,
		OutputFiles = new [] { 
			new OutputFileCopy {
				FromFile = "./binding/SkiaSharp.Windows/bin/Release/SkiaSharp.dll",
				ToDirectory = "./output/windows/"
			},
		}
	},
};

CakeSpec.Samples = new ISolutionBuilder [] {
	new IOSSolutionBuilder { 
        IsWindowsCompatible = false,
        IsMacCompatible = true,
        SolutionPath = "./samples/Skia.OSX.Demo/Skia.OSX.Demo.sln"
    },
	new IOSSolutionBuilder { 
        IsWindowsCompatible = false,
        IsMacCompatible = true,
        SolutionPath = "./samples/Skia.Forms.Demo/Skia.Forms.Demo.sln" 
    },
	new DefaultSolutionBuilder { 
        IsWindowsCompatible = true,
        IsMacCompatible = false,
        SolutionPath = "./samples/Skia.Windows.Demo/Skia.Windows.Demo.sln"
    },
};


Task ("externals")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
    .IsDependentOn ("externals-ios")
    .IsDependentOn ("externals-android")
    .Does (() => 
{
});

Task ("externals-windows")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (
        !FileExists ("native-builds/lib/windows/x86/libskia_windows.dll") ||
        !FileExists ("native-builds/lib/windows/x64/libskia_windows.dll"))
    .Does (() =>  
{
    var fixup = new Action (() => {
        var props = SKIA_PATH.Combine ("out/gyp/libjpeg-turbo.props").FullPath;
        var xdoc = XDocument.Load (props);
        var ns = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";
        var temp = xdoc.Root
            .Elements (ns + "ItemDefinitionGroup")
            .Elements (ns + "assemble")
            .Elements (ns + "CommandLineTemplate")
            .Single ();
        var newInclude = SKIA_PATH.Combine ("third_party/externals/libjpeg-turbo/win/").FullPath;
        if (!temp.Value.Contains (newInclude)) {
            temp.Value += " \"-I" + newInclude + "\"";
            xdoc.Save (props);
        }
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,msvs");
        
    // build the x86 vesion
    SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='x86'");
    RunGyp ();
    fixup ();
    DotNetBuild ("native-builds/libskia_windows/libskia_windows_x86.sln", c => { 
        c.Configuration = "Release"; 
        c.Properties ["Platform"] = new [] { "Win32" };
    });
    if (!DirectoryExists ("native-builds/lib/windows/x86")) {
        CreateDirectory ("native-builds/lib/windows/x86");
    }
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.lib", "native-builds/lib/windows/x86");
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.dll", "native-builds/lib/windows/x86");
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.pdb", "native-builds/lib/windows/x86");
    
    // build the x64 vesion
    SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='x86_64'");
    RunGyp ();
    fixup ();
    DotNetBuild ("native-builds/libskia_windows/libskia_windows_x64.sln", c => { 
        c.Configuration = "Release"; 
        c.Properties ["Platform"] = new [] { "x64" };
    });
    if (!DirectoryExists ("native-builds/lib/windows/x64")) {
        CreateDirectory ("native-builds/lib/windows/x64");
    }
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.lib", "native-builds/lib/windows/x64");
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.dll", "native-builds/lib/windows/x64");
    CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.pdb", "native-builds/lib/windows/x64");
});
Task ("externals-osx")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/osx/liblibskia_osx.dylib"))
    .Does (() =>  
{
    var buildArch = new Action<string> ((arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libskia_osx/libskia_osx.xcodeproj",
            Target = "libskia_osx",
            Sdk = "macosx",
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/osx/" + arch)) {
            CreateDirectory ("native-builds/lib/osx/" + arch);
        }
        CopyDirectory ("native-builds/libskia_osx/build/Release/", "native-builds/lib/osx/" + arch);
        RunInstallNameTool ("native-builds/lib/osx/" + arch, "lib/liblibskia_osx.dylib", "@loader_path/liblibskia_osx.dylib", "liblibskia_osx.dylib");
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,xcode");
    
    RunGyp ();
    
    buildArch ("i386");
    buildArch ("x86_64");
    
    // create the fat dylib
    RunLipo ("native-builds/lib/osx/", "liblibskia_osx.dylib", 
        "i386/liblibskia_osx.dylib", 
        "x86_64/liblibskia_osx.dylib");
});
Task ("externals-ios")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/ios/libskia_ios.framework/libskia_ios"))
    .Does (() => 
{
    var buildArch = new Action<string, string> ((sdk, arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libskia_ios/libskia_ios.xcodeproj",
            Target = "libskia_ios",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/ios/" + arch)) {
            CreateDirectory ("native-builds/lib/ios/" + arch);
        }
        CopyDirectory ("native-builds/libskia_ios/build/Release-" + sdk, "native-builds/lib/ios/" + arch);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_DEFINES", "skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0");
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,xcode");
        
    RunGyp ();
    
    buildArch ("iphonesimulator", "i386");
    buildArch ("iphonesimulator", "x86_64");
    buildArch ("iphoneos", "armv7");
    buildArch ("iphoneos", "armv7s");
    buildArch ("iphoneos", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/ios/armv7/libskia_ios.framework/", "native-builds/lib/ios/libskia_ios.framework/");
    DeleteFile ("native-builds/lib/ios/libskia_ios.framework/libskia_ios");
    RunLipo ("native-builds/lib/ios/", "libskia_ios.framework/libskia_ios", 
        "i386/libskia_ios.framework/libskia_ios", 
        "x86_64/libskia_ios.framework/libskia_ios", 
        "armv7/libskia_ios.framework/libskia_ios", 
        "armv7s/libskia_ios.framework/libskia_ios", 
        "arm64/libskia_ios.framework/libskia_ios");
});
Task ("externals-android")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/android/x86/libskia_android.so") ||
        !FileExists ("native-builds/lib/android/x86_64/libskia_android.so") ||
        !FileExists ("native-builds/lib/android/armeabi/libskia_android.so") ||
        !FileExists ("native-builds/lib/android/armeabi-v7a/libskia_android.so") ||
        !FileExists ("native-builds/lib/android/arm64-v8a/libskia_android.so"))
    .Does (() => 
{
    var ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-sdk-macosx";
    var ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? ANDROID_HOME;
    var ANDROID_NDK_HOME = EnvironmentVariable ("ANDROID_NDK_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-ndk";
    
    var buildArch = new Action<string, string> ((arch, folder) => {
        StartProcess (SKIA_PATH.CombineWithFilePath ("platform_tools/android/bin/android_ninja").FullPath, new ProcessSettings {
            Arguments = "-d " + arch + " \"skia_lib\"",
            WorkingDirectory = SKIA_PATH.FullPath,
        });

        if (!DirectoryExists ("native-builds/lib/android/" + folder)) {
            CreateDirectory ("native-builds/lib/android/" + folder);
        }
        CopyFileToDirectory (SKIA_PATH.CombineWithFilePath ("out/config/android-" + arch + "/Release/lib/libskia_android.so"), "native-builds/lib/android/" + folder);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_DEFINES", "");
    SetEnvironmentVariable ("GYP_GENERATORS", "");
    SetEnvironmentVariable ("BUILDTYPE", "Release");
    SetEnvironmentVariable ("ANDROID_HOME", ANDROID_HOME);
    SetEnvironmentVariable ("ANDROID_SDK_ROOT", ANDROID_SDK_ROOT);
    SetEnvironmentVariable ("ANDROID_NDK_HOME", ANDROID_NDK_HOME);
    
    buildArch ("x86", "x86");
    buildArch ("x86_64", "x86_64");
    buildArch ("arm", "armeabi");
    buildArch ("arm_v7_neon", "armeabi-v7a");
    buildArch ("arm64", "arm64-v8a");
        
    var ndkbuild = MakeAbsolute (Directory (ANDROID_NDK_HOME)).CombineWithFilePath ("ndk-build").FullPath;
    StartProcess (ndkbuild, new ProcessSettings {
        Arguments = "",
        WorkingDirectory = ROOT_PATH.Combine ("native-builds/libskia_android").FullPath,
    }); 
});

Task ("clean")
    .IsDependentOn ("clean-externals")
    .Does (() => 
{
});

Task ("clean-externals").Does (() =>
{
    // skia
    CleanDirectories ("skia/out");
    CleanDirectories ("skia/xcodebuild");

    // all
    CleanDirectories ("native-builds/lib");
    // android
    CleanDirectories ("native-builds/libskia_android/obj");
    CleanDirectories ("native-builds/libskia_android/libs");
    // ios
    CleanDirectories ("native-builds/libskia_ios/build");
    // osx
    CleanDirectories ("native-builds/libskia_osx/build");
    // windows
    CleanDirectories ("native-builds/libskia_windows/Release");
    CleanDirectories ("native-builds/libskia_windows/x64/Release");
});


DefineDefaultTasks ();

RunTarget (TARGET);
