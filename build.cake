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

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("skia"));

////////////////////////////////////////////////////////////////////////////////////////////////////
// TOOLS & FUNCTIONS - the bits to make it all work
////////////////////////////////////////////////////////////////////////////////////////////////////

var SetEnvironmentVariable = new Action<string, string> ((name, value) => {
    Information ("Setting Environment Variable {0} to {1}", name, value);
    Environment.SetEnvironmentVariable (name, value, EnvironmentVariableTarget.Process);
});
var AppendEnvironmentVariable = new Action<string, string> ((name, value) => {
    var old = EnvironmentVariable (name);
    var sep = IsRunningOnWindows () ? ';' : ':';
    
    if (!old.ToUpper ().Split (sep).Contains (value.ToUpper ())) {
        Information ("Adding {0} to Environment Variable {1}", value, name);
        value += sep + old;
        SetEnvironmentVariable (name, value);
    }
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

var RunGyp = new Action<string, string> ((defines, generators) =>
{
    SetEnvironmentVariable ("GYP_GENERATORS", generators);
    SetEnvironmentVariable ("GYP_DEFINES", defines);
    
    Information ("Running 'sync-and-gyp'...");
    Information ("\tGYP_GENERATORS = " + EnvironmentVariable ("GYP_GENERATORS"));
    Information ("\tGYP_DEFINES = " + EnvironmentVariable ("GYP_DEFINES"));
    
    var result = StartProcess ("python", new ProcessSettings {
        Arguments = SKIA_PATH.CombineWithFilePath("bin/sync-and-gyp").FullPath,
        WorkingDirectory = SKIA_PATH.FullPath,
    });
    if (result != 0) {
        throw new Exception ("sync-and-gyp failed with error: " + result);
    }
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
	if (!DirectoryExists (outputPath)) {
		CreateDirectory (outputPath);
	}

    NuGetPack (nuspecPath, new NuGetPackSettings { 
        Verbosity = NuGetVerbosity.Detailed,
        OutputDirectory = outputPath,		
        BasePath = "./",
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
        Arguments = string.Format ("update --delete --out=\"{0}\" \"{1}\"", docsRoot, assembly),
    });
});

var RunMdocMSXml = new Action<DirectoryPath, FilePath> ((docsRoot, output) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("export-msxdoc --out=\"{0}\" \"{1}\"", output, docsRoot),
    });
});

var RunMdocAssemble = new Action<DirectoryPath, FilePath> ((docsRoot, output) =>
{
    StartProcess (MDocPath, new ProcessSettings {
        Arguments = string.Format ("assemble --out=\"{0}\" \"{1}\"", output, docsRoot),
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

var MSBuildNS = (XNamespace) "http://schemas.microsoft.com/developer/msbuild/2003";

var SetXValue = new Action<XElement, string, string> ((root, element, value) => {
    var node = root.Element (MSBuildNS + element);
    if (node == null)
        root.Add (new XElement (MSBuildNS + element, value));
    else
        node.Value = value;
});
var AddXValue = new Action<XElement, string, string> ((root, element, value) => {
    var node = root.Element (MSBuildNS + element);
    if (node == null)
        root.Add (new XElement (MSBuildNS + element, value));
    else
        node.Value += value;
});
var SetXValues = new Action<XElement, string[], string, string> ((root, parents, element, value) => {
    IEnumerable<XElement> nodes = new [] { root };
    foreach (var p in parents) {
        nodes = nodes.Elements (MSBuildNS + p);
    }
    foreach (var n in nodes) {
        SetXValue (n, element, value);
    }
});
var AddXValues = new Action<XElement, string[], string, string> ((root, parents, element, value) => {
    IEnumerable<XElement> nodes = new [] { root };
    foreach (var p in parents) {
        nodes = nodes.Elements (MSBuildNS + p);
    }
    foreach (var n in nodes) {
        AddXValue (n, element, value);
    }
});

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
    .IsDependentOn ("externals-uwp")
    .IsDependentOn ("externals-windows")
    .IsDependentOn ("externals-osx")
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
    }
    if (IsRunningOnUnix ()) {
        if (!DirectoryExists ("./output/osx")) CreateDirectory ("./output/osx");
        if (!DirectoryExists ("./output/mac")) CreateDirectory ("./output/mac");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/osx/");
        CopyFileToDirectory ("./native-builds/lib/osx/libSkiaSharp.dylib", "./output/mac/");
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
        !FileExists ("native-builds/lib/windows/x86/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/windows/x64/libSkiaSharp.dll"))
    .Does (() =>  
{
    var buildArch = new Action<string, string, string> ((platform, skiaArch, dir) => {
        RunGyp ("skia_arch_type='" + skiaArch + "'", "ninja,msvs");
        VisualStudioPathFixup ();
        DotNetBuild ("native-builds/libSkiaSharp_windows/libSkiaSharp_" + dir + ".sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { platform };
        });
        if (!DirectoryExists ("native-builds/lib/windows/" + dir)) CreateDirectory ("native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/Release/libSkiaSharp.lib", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/Release/libSkiaSharp.dll", "native-builds/lib/windows/" + dir);
        CopyFileToDirectory ("native-builds/libSkiaSharp_windows/Release/libSkiaSharp.pdb", "native-builds/lib/windows/" + dir);
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);
    
    buildArch ("Win32", "x86", "x86");
    buildArch ("x64", "x86_64", "x64");
});
// this builds the native C and C++ externals for Windows UWP
Task ("externals-uwp")
    .WithCriteria (IsRunningOnWindows ())
    .WithCriteria (
        !FileExists ("native-builds/lib/uwp/ARM/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/uwp/x86/libSkiaSharp.dll") ||
        !FileExists ("native-builds/lib/uwp/x64/libSkiaSharp.dll"))
    .Does (() =>  
{
    var convertDesktopToUWP = new Action<FilePath, string> ((projectFilePath, platform) => {
        //
        // TODO: the stuff in this block must be moved into the gyp files !!
        //
        
        var projectFile = MakeAbsolute (projectFilePath).FullPath;
        var xdoc = XDocument.Load (projectFile);
        
        var configType = xdoc.Root
            .Elements (MSBuildNS + "PropertyGroup")
            .Elements (MSBuildNS + "ConfigurationType")
            .Select (e => e.Value)
            .FirstOrDefault ();
        if (configType != "StaticLibrary") {
            // skip over "Utility" projects as they aren't actually 
            // library projects, but intermediate build steps.
            return;
        } else {
            // special case for ARM, gyp does not yet have ARM, 
            // so it defaults to Win32
            // update and reload
            if (platform.ToUpper () == "ARM") {
                ReplaceTextInFiles (projectFile, "Win32", "ARM");
                ReplaceTextInFiles (projectFile, "SkTLS_win.cpp", "SkTLS_none.cpp");
                xdoc = XDocument.Load (projectFile);
            }
        }
        
        var globals = xdoc.Root
            .Elements (MSBuildNS + "PropertyGroup")
            .Where (e => e.Attribute ("Label") != null && e.Attribute ("Label").Value == "Globals")
            .Single ();
            
        globals.Elements (MSBuildNS + "WindowsTargetPlatformVersion").Remove ();
        SetXValue (globals, "Keyword", "StaticLibrary");
        SetXValue (globals, "AppContainerApplication", "true");
        SetXValue (globals, "ApplicationType", "Windows Store");
        SetXValue (globals, "WindowsTargetPlatformVersion", "10.0.10586.0");
        SetXValue (globals, "WindowsTargetPlatformMinVersion", "10.0.10240.0");
        SetXValue (globals, "ApplicationTypeRevision", "10.0");
        SetXValue (globals, "DefaultLanguage", "en-US");

        var properties = xdoc.Root
            .Elements (MSBuildNS + "PropertyGroup")
            .Elements (MSBuildNS + "LinkIncremental")
            .First ()
            .Parent;
        SetXValue (properties, "GenerateManifest","false");
        SetXValue (properties, "IgnoreImportLibrary","false");
        
        SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "CompileAsWinRT", "false");
        //AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "AdditionalOptions", " /sdl ");
        AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "PreprocessorDefinitions", ";SK_BUILD_FOR_WINRT;WINAPI_FAMILY=WINAPI_FAMILY_APP;");
        AddXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "ClCompile" }, "DisableSpecificWarnings", ";4146;4703;");
        SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "SubSystem", "Console");
        SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "IgnoreAllDefaultLibraries", "false");
        SetXValues (xdoc.Root, new [] { "ItemDefinitionGroup", "Link" }, "GenerateWindowsMetadata", "false");

        xdoc.Root
            .Elements (MSBuildNS + "ItemDefinitionGroup")
            .Elements (MSBuildNS + "Link")
            .Elements (MSBuildNS + "AdditionalDependencies")
            .Remove ();
            
        xdoc.Save (projectFile);
    });

    var buildArch = new Action<string, string> ((platform, arch) => {
        CleanDirectories ("native-builds/libSkiaSharp_uwp/" + arch);
        CleanDirectories ("native-builds/libSkiaSharp_uwp/Release");
        CleanDirectories ("native-builds/libSkiaSharp_uwp/Generated Files");
        ProcessSolutionProjects ("native-builds/libSkiaSharp_uwp/libSkiaSharp_" + arch + ".sln", (projectName, projectPath) => {
            if (projectName != "libSkiaSharp")
                convertDesktopToUWP (projectPath, platform);
        });
        VisualStudioPathFixup ();
        DotNetBuild ("native-builds/libSkiaSharp_uwp/libSkiaSharp_" + arch + ".sln", c => { 
            c.Configuration = "Release"; 
            c.Properties ["Platform"] = new [] { platform };
        });
        if (!DirectoryExists ("native-builds/lib/uwp/" + arch)) CreateDirectory ("native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/Release/libSkiaSharp.lib", "native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/Release/libSkiaSharp.dll", "native-builds/lib/uwp/" + arch);
        CopyFileToDirectory ("native-builds/libSkiaSharp_uwp/Release/libSkiaSharp.pdb", "native-builds/lib/uwp/" + arch);
    });

    // set up the gyp environment variables
    AppendEnvironmentVariable ("PATH", DEPOT_PATH.FullPath);

    RunGyp ("skia_arch_type='x86_64' skia_gpu=0", "ninja,msvs");
    buildArch ("x64", "x64");
    
    RunGyp ("skia_arch_type='x86' skia_gpu=0", "ninja,msvs");
    buildArch ("Win32", "x86");
    
    RunGyp ("skia_arch_type='arm' arm_version=7 arm_neon=0 skia_gpu=0", "ninja,msvs");
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
        RunGyp ("skia_arch_type='" + skiaArch + "'", "ninja,xcode");
        
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
    
    RunGyp ("skia_os='ios' skia_arch_type='arm' armv7=1 arm_neon=0", "ninja,xcode");
    
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
// this builds the native C and C++ externals for Android
Task ("externals-android")
    .WithCriteria (IsRunningOnUnix ())
    .WithCriteria (
        !FileExists ("native-builds/lib/android/x86/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/x86_64/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/armeabi/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/armeabi-v7a/libSkiaSharp.so") ||
        !FileExists ("native-builds/lib/android/arm64-v8a/libSkiaSharp.so"))
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
        WorkingDirectory = ROOT_PATH.Combine ("native-builds/libSkiaSharp_android").FullPath,
    }); 

    foreach (var folder in new [] { "x86", "x86_64", "armeabi", "armeabi-v7a", "arm64-v8a" }) {
        if (!DirectoryExists ("native-builds/lib/android/" + folder)) {
            CreateDirectory ("native-builds/lib/android/" + folder);
        }
        CopyFileToDirectory ("native-builds/libSkiaSharp_android/libs/" + folder + "/libSkiaSharp.so", "native-builds/lib/android/" + folder);
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - the managed C# libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .IsDependentOn ("libs-base")
    .IsDependentOn ("libs-windows")
    .IsDependentOn ("libs-osx")
    .Does (() => 
{
});
Task ("libs-base")
    .Does (() => 
{
    // set the SHA on the assembly info 
    var sha = EnvironmentVariable ("GIT_COMMIT") ?? string.Empty;
    if (!string.IsNullOrEmpty (sha) && sha.Length >= 6) {
        sha = sha.Substring (0, 6);
        Information ("Setting Git SHA to {0}.", sha);
        ReplaceTextInFiles ("./binding/SkiaSharp/Properties/SkiaSharpAssemblyInfo.cs", "{GIT_SHA}", sha);
    }
});
Task ("libs-windows")
    .WithCriteria (IsRunningOnWindows ())
    .IsDependentOn ("externals")
    .IsDependentOn ("libs-base")
    .Does (() => 
{
    // build
    RunNuGetRestore ("binding/SkiaSharp.Windows.sln");
    DotNetBuild ("binding/SkiaSharp.Windows.sln", c => { 
        c.Configuration = "Release"; 
    });
    
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/windows/")) CreateDirectory ("./output/windows/");
    if (!DirectoryExists ("./output/uwp/")) CreateDirectory ("./output/uwp/");
    
    // copy build output
    CopyFileToDirectory ("./binding/SkiaSharp.Portable/bin/Release/SkiaSharp.dll", "./output/portable/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.pdb", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.dll.config", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.Desktop/bin/Release/SkiaSharp.Desktop.targets", "./output/windows/");
    CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.dll", "./output/uwp/");
    CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.pdb", "./output/uwp/");
    CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.pri", "./output/uwp/");
    CopyFileToDirectory ("./binding/SkiaSharp.UWP/bin/Release/SkiaSharp.UWP.targets", "./output/uwp/");
});
Task ("libs-osx")
    .WithCriteria (IsRunningOnUnix ())
    .IsDependentOn ("externals")
    .IsDependentOn ("libs-base")
    .Does (() => 
{
    // build
    RunNuGetRestore ("binding/SkiaSharp.Mac.sln");
    DotNetBuild ("binding/SkiaSharp.Mac.sln", c => { 
        c.Configuration = "Release"; 
    });

    if (!DirectoryExists ("./output/android/")) CreateDirectory ("./output/android/");
    if (!DirectoryExists ("./output/ios/")) CreateDirectory ("./output/ios/");
    if (!DirectoryExists ("./output/osx/")) CreateDirectory ("./output/osx/");
    if (!DirectoryExists ("./output/portable/")) CreateDirectory ("./output/portable/");
    if (!DirectoryExists ("./output/mac/")) CreateDirectory ("./output/mac/");
    
    // copy build output
    CopyFileToDirectory ("./binding/SkiaSharp.Android/bin/Release/SkiaSharp.dll", "./output/android/");
    CopyFileToDirectory ("./binding/SkiaSharp.iOS/bin/Release/SkiaSharp.dll", "./output/ios/");
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
    .IsDependentOn ("libs-base")
    .IsDependentOn ("externals-genapi")
    .Does (() => 
{
    RunMdocUpdate ("./binding/SkiaSharp.Generic/bin/Release/SkiaSharp.dll", "./docs/en/");
    
    if (!DirectoryExists ("./output/docs/msxml/")) CreateDirectory ("./output/docs/msxml/");
    RunMdocMSXml ("./docs/en/", "./output/docs/msxml/SkiaSharp.xml");
    
    if (!DirectoryExists ("./output/docs/mdoc/")) CreateDirectory ("./output/docs/mdoc/");
    RunMdocAssemble ("./docs/en/", "./output/docs/mdoc/SkiaSharp");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - building the package for NuGet.org
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .IsDependentOn ("libs")
    .IsDependentOn ("docs")
    .Does (() => 
{
    if (IsRunningOnWindows ()) {
        PackageNuGet ("./nuget/SkiaSharp.Windows.nuspec", "./output/");
    }

    if (IsRunningOnUnix ()) {
        PackageNuGet ("./nuget/SkiaSharp.Mac.nuspec", "./output/");
        PackageNuGet ("./nuget/SkiaSharp.nuspec", "./output/");
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
    CleanDirectories ("native-builds/libSkiaSharp_android/obj");
    CleanDirectories ("native-builds/libSkiaSharp_android/libs");
    // ios
    CleanDirectories ("native-builds/libSkiaSharp_ios/build");
    // osx
    CleanDirectories ("native-builds/libSkiaSharp_osx/build");
    // windows
    CleanDirectories ("native-builds/libSkiaSharp_windows/Release");
    CleanDirectories ("native-builds/libSkiaSharp_windows/x64/Release");
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

Task ("Windows-CI")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
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
