#addin "Cake.Xamarin"
#addin "Cake.XCode"
#addin "Cake.FileHelpers"

using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));

var NuGetSources = new [] { "https://www.nuget.org/api/v2/" };
var NugetToolPath = GetToolPath ("../nuget.exe");
var XamarinComponentToolPath = GetToolPath ("../xamarin-component.exe");
var CakeToolPath = GetToolPath ("Cake.exe");
var NUnitConsoleToolPath = GetToolPath ("../NUnit.Console/tools/nunit3-console.exe");
var GenApiToolPath = GetToolPath ("../genapi.exe");
var MDocPath = GetMDocPath ();

DirectoryPath ROOT_PATH = MakeAbsolute(File(".")).GetDirectory();
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("skia"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// TOOLS & FUNCTIONS - the bits to make it all work
////////////////////////////////////////////////////////////////////////////////////////////////////

var SetEnvironmentVariable = new Action<string, string> ((name, value) => {
    Environment.SetEnvironmentVariable (name, value, EnvironmentVariableTarget.Process);
});
var AppendEnvironmentVariable = new Action<string, string> ((name, value) => {
    var old = EnvironmentVariable (name);
    value += (IsRunningOnWindows () ? ";" : ":") + old;
    SetEnvironmentVariable (name, value);
});
void ListEnvironmentVariables ()
{
    Information ("Environment Variables:");
    foreach (var envVar in EnvironmentVariables ()) {
        Information ("\tKey: {0}\tValue: \"{1}\"", envVar.Key, envVar.Value);
    }
}

FilePath GetToolPath (FilePath toolPath)
{
	var appRoot = Context.Environment.GetApplicationRoot ();
 	var appRootExe = appRoot.CombineWithFilePath (toolPath);
 	if (FileExists (appRootExe))
 		return appRootExe;
    throw new FileNotFoundException ("Unable to find tool: " + appRootExe); 
}

FilePath GetMDocPath ()
{
    FilePath mdocPath;
    if (IsRunningOnUnix ()) {
        mdocPath = "/Library/Frameworks/Mono.framework/Versions/Current/bin/mdoc";
    } else {
        DirectoryPath progFiles = Environment.GetFolderPath (Environment.SpecialFolder.ProgramFilesX86);
        mdocPath = progFiles.CombineWithFilePath ("Mono/bin/mdoc.bat");
    }
    if (!FileExists (mdocPath)) {
        mdocPath = "mdoc";
    }
    return mdocPath;
}

var RunNuGetRestore = new Action<FilePath> ((solution) =>
{
    NuGetRestore (solution, new NuGetRestoreSettings { 
        ToolPath = NugetToolPath
    });
});

var RunGyp = new Action (() =>
{
    Information ("Running 'sync-and-gyp'...");
    Information ("\tGYP_GENERATORS = " + EnvironmentVariable ("GYP_GENERATORS"));
    Information ("\tGYP_DEFINES = " + EnvironmentVariable ("GYP_DEFINES"));
    
    StartProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath("bin/sync-and-gyp").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
});

var RunInstallNameTool = new Action<DirectoryPath, string, string, FilePath> ((directory, oldName, newName, library) =>
{
    if (!IsRunningOnUnix ()) {
        throw new InvalidOperationException ("install_name_tool is only available on Unix.");
    }
    
	StartProcess ("install_name_tool", new ProcessSettings {
		Arguments = string.Format("-change {0} {1} \"{2}\"", oldName, newName, library),
		WorkingDirectory = directory,
	});
});

var RunLipo = new Action<DirectoryPath, FilePath, FilePath[]> ((directory, output, inputs) =>
{
    if (!IsRunningOnUnix ()) {
        throw new InvalidOperationException ("lipo is only available on Unix.");
    }
    
    var dir = directory.CombineWithFilePath (output).GetDirectory ();
    if (!DirectoryExists (dir)) {
        CreateDirectory (dir);
    }

	var inputString = string.Join(" ", inputs.Select (i => string.Format ("\"{0}\"", i)));
	StartProcess ("lipo", new ProcessSettings {
		Arguments = string.Format("-create -output \"{0}\" {1}", output, inputString),
		WorkingDirectory = directory,
	});
});

var PackageNuGet = new Action<FilePath, DirectoryPath> ((nuspecPath, outputPath) =>
{
	// NuGet messes up path on mac, so let's add ./ in front twice
	var basePath = IsRunningOnUnix () ? "././" : "./";

	if (!DirectoryExists (outputPath)) {
		CreateDirectory (outputPath);
	}

    NuGetPack (nuspecPath, new NuGetPackSettings { 
        Verbosity = NuGetVerbosity.Detailed,
        OutputDirectory = outputPath,		
        BasePath = basePath,
        ToolPath = NugetToolPath
    });				
});

var RunTests = new Action<FilePath> ((testAssembly) =>
{
    var dir = testAssembly.GetDirectory ();
    var result = StartProcess (NUnitConsoleToolPath, new ProcessSettings {
        Arguments = string.Format ("\"{0}\" --work=\"{1}\"", testAssembly, dir),
    });
    
    if (result != 0) {
        throw new Exception ("NUnit test failed with error: " + result);
    }
});

var RunMdocUpdate = new Action<FilePath, DirectoryPath> ((assembly, docsRoot) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("update --out=\"{0}\" \"{1}\"", docsRoot, assembly),
    });
});

var RunMdocMSXml = new Action<DirectoryPath, FilePath> ((docsRoot, output) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("export-msxdoc --out=\"{0}\" \"{1}\"", output, docsRoot),
    });
});

var ProcessSolutionProjects = new Action<FilePath, Action<string, FilePath>> ((solutionFilePath, process) => {
    var solutionFile = MakeAbsolute (solutionFilePath).FullPath;
    foreach (var line in FileReadLines (solutionFile)) {
        var match = Regex.Match (line, @"Project\(""(?<type>.*)""\) = ""(?<name>.*)"", ""(?<path>.*)"", "".*""");
        if (match.Success && match.Groups ["type"].Value == "{8BC9CEB8-8B4A-11D0-8D11-00A0C91BC942}") {
            var path = match.Groups["path"].Value;
            var projectFilePath = MakeAbsolute (solutionFilePath.GetDirectory ().CombineWithFilePath (path));
            Information ("Processing project file: " + projectFilePath);
            process (match.Groups["name"].Value, projectFilePath);
        }
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .IsDependentOn ("externals-genapi")
    .IsDependentOn ("externals-native")
    .Does (() => 
{
});
// this builds the native C and C++ externals 
Task ("externals-native")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
    .IsDependentOn ("externals-tvos")
    .IsDependentOn ("externals-ios")
    .IsDependentOn ("externals-android")
    .Does (() => 
{
    // copy all the native files into the output
    CopyDirectory ("./native-builds/lib/", "./output/native/");
    
    // copy the non-embedded native files into the output
    if (IsRunningOnWindows ()) {
        if (!DirectoryExists ("./output/windows/x86")) CreateDirectory ("./output/windows/x86");
        if (!DirectoryExists ("./output/windows/x64")) CreateDirectory ("./output/windows/x64");
        CopyFileToDirectory ("./native-builds/lib/windows/x86/libskia_windows.dll", "./output/windows/x86/");
        CopyFileToDirectory ("./native-builds/lib/windows/x86/libskia_windows.pdb", "./output/windows/x86/");
        CopyFileToDirectory ("./native-builds/lib/windows/x64/libskia_windows.dll", "./output/windows/x64/");
        CopyFileToDirectory ("./native-builds/lib/windows/x64/libskia_windows.pdb", "./output/windows/x64/");
    }
    if (IsRunningOnUnix ()) {
        if (!DirectoryExists ("./output/osx")) CreateDirectory ("./output/osx");
        if (!DirectoryExists ("./output/mac")) CreateDirectory ("./output/mac");
        CopyFileToDirectory ("./native-builds/lib/osx/libskia_osx.dylib", "./output/osx/");
        CopyFileToDirectory ("./native-builds/lib/osx/libskia_osx.dylib", "./output/mac/");
    }
});
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
// this builds the native C and C++ externals for Windows
Task ("externals-windows")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (
        !FileExists ("native-builds/lib/windows/x86/libskia_windows.dll") ||
        !FileExists ("native-builds/lib/windows/x64/libskia_windows.dll"))
    .Does (() =>  
{
    var fixup = new Action (() => {
        var props = SKIA_PATH.CombineWithFilePath ("out/gyp/libjpeg-turbo.props").FullPath;
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
    
    var buildArch = new Action<string, string, string> ((platform, skiaArch, dir) => {
        SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='" + skiaArch + "'");
        RunGyp ();
        fixup ();
        DotNetBuild ("native-builds/libskia_windows/libskia_windows_" + dir + ".sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { platform };
        });
        if (!DirectoryExists ("native-builds/lib/windows/" + dir)) CreateDirectory ("native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.lib", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.dll", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libskia_windows/Release/libskia_windows.pdb", "native-builds/lib/windows/" + dir);
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,msvs");
        
    buildArch ("Win32", "x86", "x86");
    buildArch ("x64", "x86_64", "x64");
});
// this builds the native C and C++ externals for Mac OS X
Task ("externals-osx")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/osx/libskia_osx.dylib"))
    .Does (() =>  
{
    var buildArch = new Action<string, string> ((arch, skiaArch) => {
        // clean last builds
        CleanDirectories ("skia/out");
        CleanDirectories ("skia/xcodebuild");

        // prepare for build
        SetEnvironmentVariable ("GYP_DEFINES", "skia_arch_type='" + skiaArch + "'");
        RunGyp ();
        
        // build
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
        RunInstallNameTool ("native-builds/lib/osx/" + arch, "lib/libskia_osx.dylib", "@loader_path/libskia_osx.dylib", "libskia_osx.dylib");
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,xcode");
    
    // build
    buildArch ("i386", "x86");
    buildArch ("x86_64", "x86_64");
    
    // create the fat dylib
    RunLipo ("native-builds/lib/osx/", "libskia_osx.dylib", new [] {
        (FilePath) "i386/libskia_osx.dylib", 
        (FilePath) "x86_64/libskia_osx.dylib"
    });
});
// this builds the native C and C++ externals for iOS
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
    SetEnvironmentVariable ("GYP_DEFINES", "skia_os='ios' skia_arch_type='arm' arm_version=7 arm_neon=0");
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,xcode");
        
    // clean last builds
    CleanDirectories ("skia/out");
    CleanDirectories ("skia/xcodebuild");

    // prepare for build
    RunGyp ();
    
    // build
    buildArch ("iphonesimulator", "i386");
    buildArch ("iphonesimulator", "x86_64");
    buildArch ("iphoneos", "armv7");
    buildArch ("iphoneos", "armv7s");
    buildArch ("iphoneos", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/ios/armv7/libskia_ios.framework/", "native-builds/lib/ios/libskia_ios.framework/");
    DeleteFile ("native-builds/lib/ios/libskia_ios.framework/libskia_ios");
    RunLipo ("native-builds/lib/ios/", "libskia_ios.framework/libskia_ios", new [] {
        (FilePath) "i386/libskia_ios.framework/libskia_ios", 
        (FilePath) "x86_64/libskia_ios.framework/libskia_ios", 
        (FilePath) "armv7/libskia_ios.framework/libskia_ios", 
        (FilePath) "armv7s/libskia_ios.framework/libskia_ios", 
        (FilePath) "arm64/libskia_ios.framework/libskia_ios"
    });
});
// this builds the native C and C++ externals for tvOS
Task ("externals-tvos")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/tvos/libskia_tvos.framework/libskia_tvos"))
    .Does (() => 
{
    var fixup = new Action (() => {
        var glob = "./skia/out/gyp/*.xcodeproj/project.pbxproj";
        ReplaceTextInFiles (glob, "SDKROOT = iphoneos;", "SDKROOT = appletvos;");
        ReplaceTextInFiles (glob, "IPHONEOS_DEPLOYMENT_TARGET = 9.2;", "TVOS_DEPLOYMENT_TARGET = 9.1;");
        ReplaceTextInFiles (glob, "TARGETED_DEVICE_FAMILY = \"1,2\";", "TARGETED_DEVICE_FAMILY = 3;");
        ReplaceTextInFiles (glob, "\"CODE_SIGN_IDENTITY[sdk=iphoneos*]\" = \"iPhone Developer\";", "");
    });

    var buildArch = new Action<string, string> ((sdk, arch) => {
        XCodeBuild (new XCodeBuildSettings {
            Project = "native-builds/libskia_tvos/libskia_tvos.xcodeproj",
            Target = "libskia_tvos",
            Sdk = sdk,
            Arch = arch,
            Configuration = "Release",
        });
        if (!DirectoryExists ("native-builds/lib/tvos/" + arch)) {
            CreateDirectory ("native-builds/lib/tvos/" + arch);
        }
        CopyDirectory ("native-builds/libskia_tvos/build/Release-" + sdk, "native-builds/lib/tvos/" + arch);
    });
    
    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    SetEnvironmentVariable ("GYP_DEFINES", "skia_os='ios' skia_arch_type='arm' arm_version=7 arm_neon=0");
    SetEnvironmentVariable ("GYP_GENERATORS", "ninja,xcode");
        
    // clean last builds
    CleanDirectories ("skia/out");
    CleanDirectories ("skia/xcodebuild");
    
    // prepare for build
    RunGyp ();
    fixup ();
    
    // build
    buildArch ("appletvsimulator", "x86_64");
    buildArch ("appletvos", "arm64");
    
    // create the fat framework
    CopyDirectory ("native-builds/lib/tvos/x86_64/libskia_tvos.framework/", "native-builds/lib/tvos/libskia_tvos.framework/");
    DeleteFile ("native-builds/lib/tvos/libskia_tvos.framework/libskia_tvos");
    RunLipo ("native-builds/lib/tvos/", "libskia_tvos.framework/libskia_tvos", new [] {
        (FilePath) "x86_64/libskia_tvos.framework/libskia_tvos", 
        (FilePath) "arm64/libskia_tvos.framework/libskia_tvos"
    });
});
// this builds the native C and C++ externals for Android
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

    foreach (var folder in new [] { "x86", "x86_64", "armeabi", "armeabi-v7a", "arm64-v8a" }) {
        if (!DirectoryExists ("native-builds/lib/android/" + folder)) {
            CreateDirectory ("native-builds/lib/android/" + folder);
        }
        CopyFileToDirectory ("native-builds/libskia_android/libs/" + folder + "/libskia_android.so", "native-builds/lib/android/" + folder);
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - the managed C# libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .IsDependentOn ("libs-windows")
    .IsDependentOn ("libs-osx")
    .Does (() => 
{
});
Task ("libs-windows")
    .WithCriteria (IsRunningOnWindows ())
    .IsDependentOn ("externals")
    .Does (() => 
{
    // build
    RunNuGetRestore ("binding/SkiaSharp.Windows.sln");
    DotNetBuild ("binding/SkiaSharp.Windows.sln", c => { 
        c.Configuration = "Release"; 
    });
    
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/windows/")) CreateDirectory ("./output/windows/");
    
    // copy build output
    CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.pdb", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll.config", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.Desktop.targets", "./output/windows/");
});
Task ("libs-osx")
    .WithCriteria (IsRunningOnUnix ())
    .IsDependentOn ("externals")
    .Does (() => 
{
    // build
    RunNuGetRestore ("binding/SkiaSharp.Mac.sln");
    DotNetBuild ("binding/SkiaSharp.Mac.sln", c => { 
        c.Configuration = "Release"; 
    });

    if (!DirectoryExists ("./output/android/")) CreateDirectory ("./output/android/");
    if (!DirectoryExists ("./output/ios/")) CreateDirectory ("./output/ios/");
    if (!DirectoryExists ("./output/tvos/")) CreateDirectory ("./output/tvos/");
    if (!DirectoryExists ("./output/osx/")) CreateDirectory ("./output/osx/");
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/mac/")) CreateDirectory ("./output/mac/");
    
    // copy build output
    CopyFileToDirectory ("./binding/SkiaSharp.Android/bin/Release/SkiaSharp.dll", "./output/android/");
    CopyFileToDirectory ("./binding/SkiaSharp.iOS/bin/Release/SkiaSharp.dll", "./output/ios/");
    CopyFileToDirectory ("./binding/SkiaSharp.tvOS/bin/Release/SkiaSharp.dll", "./output/tvos/");
    CopyFileToDirectory ("./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.dll", "./output/osx/");
    CopyFileToDirectory ("./binding/SkiaSharp.OSX/bin/Release/SkiaSharp.OSX.targets", "./output/osx/");
    CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/mac/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.Desktop.targets", "./output/mac/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll.config", "./output/mac/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - some test cases to make sure it works
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .IsDependentOn ("libs")
    .Does (() => 
{
    RunNuGetRestore ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
    
    // Windows (x86 and x64)
    if (IsRunningOnWindows ()) {
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x86" };
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/SkiaSharp.Desktop.Tests.dll");
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "x64" };
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/x86/Release/SkiaSharp.Desktop.Tests.dll");
    }
    // Mac OSX (Any CPU)
    if (IsRunningOnUnix ()) {
        DotNetBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunTests("./tests/SkiaSharp.Desktop.Tests/bin/Release/SkiaSharp.Desktop.Tests.dll");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - the demo apps showing off the work
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .IsDependentOn ("libs")
    .Does (() => 
{
    if (IsRunningOnUnix ()) {
        RunNuGetRestore ("./samples/Skia.OSX.Demo/Skia.OSX.Demo.sln");
        DotNetBuild ("./samples/Skia.OSX.Demo/Skia.OSX.Demo.sln", c => { 
            c.Configuration = "Release"; 
        });
        RunNuGetRestore ("./samples/Skia.Forms.Demo/Skia.Forms.Demo.sln");
        DotNetBuild ("./samples/Skia.Forms.Demo/Skia.Forms.Demo.sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { "iPhone" };
        });
    }
    
    RunNuGetRestore ("./samples/Skia.WindowsDesktop.Demo/Skia.WindowsDesktop.Demo.sln");
    DotNetBuild ("./samples/Skia.WindowsDesktop.Demo/Skia.WindowsDesktop.Demo.sln", c => { 
        c.Configuration = "Release"; 
        c.Properties ["Platform"] = new [] { "x86" };
    });
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DOCS - building the API documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("docs")
    .IsDependentOn ("externals-genapi")
    .Does (() => 
{
    RunMdocUpdate ("./binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll", "./docs/en/");
    
    if (!DirectoryExists ("./output/xml-docs/")) CreateDirectory ("./output/xml-docs/");
    RunMdocMSXml ("./docs/en/", "./output/xml-docs/SkiaSharp.xml");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - building the package for NuGet.org
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .IsDependentOn ("libs")
    .Does (() => 
{
    if (IsRunningOnWindows ()) {
        PackageNuGet ("./nuget/Xamarin.SkiaSharp.Windows.nuspec", "./output/");
    }

    if (IsRunningOnUnix ()) {
        PackageNuGet ("./nuget/Xamarin.SkiaSharp.Mac.nuspec", "./output/");
        PackageNuGet ("./nuget/Xamarin.SkiaSharp.nuspec", "./output/");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// COMPONENT - building the package for components.xamarin.com
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("component")
    .IsDependentOn ("nuget")
    .Does (() => 
{
    // TODO: Not yet ready
    
    // if (!DirectoryExists ("./output/")) {
    //     CreateDirectory ("./output/");
    // }
    
    // FilePath yaml = "./component/component.yaml";
    // var yamlDir = yaml.GetDirectory ();
    // PackageComponent (yamlDir, new XamarinComponentSettings { 
    //     ToolPath = XamarinComponentToolPath
    // });

    // MoveFiles (yamlDir.FullPath.TrimEnd ('/') + "/*.xam", "./output/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .IsDependentOn ("clean-externals")
    .Does (() => 
{
    CleanDirectories ("./binding/**/bin");
    CleanDirectories ("./binding/**/obj");

    CleanDirectories ("./samples/**/bin");
    CleanDirectories ("./samples/**/obj");

    CleanDirectories ("./tests/**/bin");
    CleanDirectories ("./tests/**/obj");

    if (DirectoryExists ("./output"))
        DeleteDirectory ("./output", true);
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
    CleanDirectories ("native-builds/libskia_tvos/build");
    // osx
    CleanDirectories ("native-builds/libskia_osx/build");
    // windows
    CleanDirectories ("native-builds/libskia_windows/Release");
    CleanDirectories ("native-builds/libskia_windows/x64/Release");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CI - the master target to build everything
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("CI")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("component")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW 
////////////////////////////////////////////////////////////////////////////////////////////////////

Information ("Cake.exe ToolPath: {0}", CakeToolPath);
Information ("Cake.exe NUnitConsoleToolPath: {0}", NUnitConsoleToolPath);
Information ("NuGet.exe ToolPath: {0}", NugetToolPath);
Information ("Xamarin-Component.exe ToolPath: {0}", XamarinComponentToolPath);
Information ("genapi.exe ToolPath: {0}", GenApiToolPath);

ListEnvironmentVariables ();

RunTarget (TARGET);
