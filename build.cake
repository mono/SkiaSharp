#addin nuget:?package=Cake.Xamarin&version=2.0.1
#addin nuget:?package=Cake.XCode&version=3.0.0
#addin nuget:?package=Cake.FileHelpers&version=2.0.0

#reference "tools/SharpCompress/lib/net45/SharpCompress.dll"
#reference "tools/Newtonsoft.Json/lib/net45/Newtonsoft.Json.dll"

using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SharpCompress.Readers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

#load "cake/Utils.cake"

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));
var VERBOSITY = (Verbosity) Enum.Parse (typeof(Verbosity), Argument ("v", Argument ("verbosity", Argument ("Verbosity", "Verbose"))), true);

var NuGetSources = new [] { MakeAbsolute (Directory ("./output/nugets")).FullPath, "https://api.nuget.org/v3/index.json" };
var NugetToolPath = GetToolPath ("nuget.exe");
var CakeToolPath = GetToolPath ("Cake/Cake.exe");
var MDocPath = GetToolPath ("mdoc/tools/mdoc.exe");
var MSBuildToolPath = GetMSBuildToolPath (EnvironmentVariable ("MSBUILD_EXE"));
var PythonToolPath = EnvironmentVariable ("PYTHON_EXE") ?? "python";

var CI_TARGETS = new string[] { "CI", "WINDOWS-CI", "LINUX-CI", "MAC-CI" };
var IS_ON_CI = CI_TARGETS.Contains (TARGET.ToUpper ());
var IS_ON_FINAL_CI = TARGET.ToUpper () == "CI";

string ANDROID_HOME = EnvironmentVariable ("ANDROID_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-sdk-macosx";
string ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? ANDROID_HOME;
string ANDROID_NDK_HOME = EnvironmentVariable ("ANDROID_NDK_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-ndk";

string TIZEN_STUDIO_HOME = EnvironmentVariable ("TIZEN_STUDIO_HOME") ?? EnvironmentVariable ("HOME") + "/tizen-studio";

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath ANGLE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/angle"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/harfbuzz"));
DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/en"));

DirectoryPath PROFILE_PATH = EnvironmentVariable ("USERPROFILE") ?? EnvironmentVariable ("HOME");
DirectoryPath NUGET_PACKAGES = EnvironmentVariable ("NUGET_PACKAGES") ?? PROFILE_PATH.Combine (".nuget/packages");

var GIT_SHA = EnvironmentVariable ("GIT_COMMIT") ?? string.Empty;
if (!string.IsNullOrEmpty (GIT_SHA) && GIT_SHA.Length >= 6) {
    GIT_SHA = GIT_SHA.Substring (0, 6);
} else {
    GIT_SHA = "{GIT_SHA}";
}

var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? string.Empty;
if (string.IsNullOrEmpty (BUILD_NUMBER)) {
    BUILD_NUMBER = "0";
}

#load "cake/UtilsManaged.cake"
#load "cake/BuildExternals.cake"
#load "cake/UpdateDocs.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .IsDependentOn ("externals-native")
    .Does (() => 
{
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - the managed C# libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .IsDependentOn ("externals")
    .Does (() => 
{
    // build the managed libraries
    var platform = "";
    if (IsRunningOnWindows ()) {
        platform = ".Windows";
    } else if (IsRunningOnMac ()) {
        platform = ".Mac";
    } else if (IsRunningOnLinux ()) {
        platform = ".Linux";
    }
    RunMSBuildRestore ($"./source/SkiaSharpSource{platform}.sln");
    RunMSBuild ($"./source/SkiaSharpSource{platform}.sln");

    // assemble the mdoc docs
    EnsureDirectoryExists ("./output/docs/mdoc/");
    RunProcess (MDocPath, new ProcessSettings {
        Arguments = $"assemble --out=\"./output/docs/mdoc/SkiaSharp\" \"{DOCS_PATH}\" --debug",
    });
    CopyFileToDirectory ("./docs/SkiaSharp.source", "./output/docs/mdoc/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - some test cases to make sure it works
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .Does (() => 
{
    var RunDestopTest = new Action<string> (arch => {
        var platform = "";
        if (IsRunningOnWindows ()) {
            platform = "windows";
        } else if (IsRunningOnMac ()) {
            platform = "mac";
        } else if (IsRunningOnLinux ()) {
            platform = "linux";
        }

        EnsureDirectoryExists ($"./output/tests/{platform}/{arch}");
        RunMSBuildRestore ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
        if (arch == "AnyCPU") {
            RunMSBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln");
        } else {
            RunMSBuildWithPlatform ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln", arch);
        }
        RunTests ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/Release/SkiaSharp.Tests.dll", null, arch == "x86");
        CopyFileToDirectory ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/Release/TestResult.xml", $"./output/tests/{platform}/{arch}");
    });

    // Full .NET Framework
    if (IsRunningOnWindows ()) {
        RunDestopTest ("x86");
        RunDestopTest ("x64");
    } else if (IsRunningOnMac ()) {
        RunDestopTest ("AnyCPU");
    } else if (IsRunningOnLinux ()) {
        // TODO: Disable x64 for the time being due to a bug in mono sn:
        //       https://github.com/mono/mono/issues/8218

        RunDestopTest ("AnyCPU");
        // RunDestopTest ("x64");
    }

    // .NET Core
    var netCoreTestProj = "./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.csproj";
    var xdoc = XDocument.Load (netCoreTestProj);
    var refs = xdoc.Root.Elements ("ItemGroup").Elements ("PackageReference");
    bool changed = false;
    foreach (var packageRef in refs) {
        var include = packageRef.Attribute ("Include").Value;
        var oldVersion = packageRef.Attribute ("Version").Value;
        var version = GetVersion (include);
        if (!string.IsNullOrEmpty (version)) {
            if (version != oldVersion) {
                packageRef.Attribute ("Version").Value = version;
                changed = true;
            }
        }
    }
    if (changed) {
        xdoc.Save (netCoreTestProj);
    }
    CleanDirectories ("./externals/packages/skiasharp*");
    CleanDirectories ("./externals/packages/harfbuzzsharp*");
    EnsureDirectoryExists ("./output/tests/netcore");
    RunMSBuildRestoreLocal (netCoreTestProj);
    RunNetCoreTests (netCoreTestProj, null);
    CopyFileToDirectory ("./tests/SkiaSharp.NetCore.Tests/TestResult.xml", "./output/tests/netcore");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - the demo apps showing off the work
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .Does (() => 
{
    // create the samples archive
    CreateSamplesZip ("./samples/", "./output/");

    // create the workbooks archive
    Zip ("./workbooks", "./output/workbooks.zip");

    var isLinux = IsRunningOnLinux ();
    var isMac = IsRunningOnMac ();
    var isWin = IsRunningOnWindows ();

    var buildMatrix = new Dictionary<string, bool> {
        { "android", isMac },
        { "gtk", isLinux || isMac },
        { "ios", isMac },
        { "macos", isMac },
        { "tvos", isMac },
        { "uwp", isWin },
        { "watchos", isMac },
        { "wpf", isWin },
    };

    var platformMatrix = new Dictionary<string, string> {
        { "ios", "iPhone" },
        { "tvos", "iPhoneSimulator" },
        { "uwp", "x86" },
        { "watchos", "iPhoneSimulator" },
        { "xamarin.forms.mac", "iPhone" },
        { "xamarin.forms.windows", "x86" },
    };

    var buildSample = new Action<FilePath> (sln => {
        var platform = sln.GetDirectory ().GetDirectoryName ().ToLower ();
        var name = sln.GetFilenameWithoutExtension ();
        var slnPlatform = name.GetExtension ();
        if (!string.IsNullOrEmpty (slnPlatform)) {
            slnPlatform = slnPlatform.ToLower ();
        }

        if (!buildMatrix.ContainsKey (platform) || buildMatrix [platform]) {
            string buildPlatform = null;
            if (!string.IsNullOrEmpty (slnPlatform)) {
                if (platformMatrix.ContainsKey (platform + slnPlatform)) {
                    buildPlatform = platformMatrix [platform + slnPlatform];
                }
            }
            if (string.IsNullOrEmpty (buildPlatform) && platformMatrix.ContainsKey (platform)) {
                buildPlatform = platformMatrix [platform];
            }

            RunMSBuildRestore (sln);
            if (string.IsNullOrEmpty (buildPlatform)) {
                RunMSBuild (sln);
            } else {
                RunMSBuildWithPlatform (sln, buildPlatform);
            }
        }
    });

    var solutions = GetFiles ("./samples/**/*.sln");
    foreach (var sln in solutions) {
        var name = sln.GetFilenameWithoutExtension ();
        var slnPlatform = name.GetExtension ();

        if (string.IsNullOrEmpty (slnPlatform)) {
            // this is the main solution
            var variants = GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.sln");
            if (!variants.Any ()) {
                // there is no platform variant
                buildSample (sln);
            } else {
                // skip as there is a platform variant
            }
        } else {
            // this is a platform variant
            slnPlatform = slnPlatform.ToLower ();
            var shouldBuild = 
                (isLinux && slnPlatform == ".linux") ||
                (isMac && slnPlatform == ".mac") ||
                (isWin && slnPlatform == ".windows");
            if (shouldBuild) {
                buildSample (sln);
            } else {
                // skip this as this is not the correct platform
            }
        }
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - building the package for NuGet.org
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .IsDependentOn ("libs")
    .Does (() => 
{
    var platform = "";
    if (!IS_ON_FINAL_CI) {
        if (IsRunningOnWindows ()) {
            platform = "windows";
        } else if (IsRunningOnMac ()) {
            platform = "macos";
        } else if (IsRunningOnLinux ()) {
            platform = "linux";
        }
    }

    var removePlatforms = new Action<XDocument> ((xdoc) => {
        var files = xdoc.Root
            .Elements ("files")
            .Elements ("file");
        foreach (var file in files.ToArray ()) {
            // remove the files that aren't available
            var nuspecPlatform = file.Attribute ("platform");
            if (nuspecPlatform != null) {
                if (!string.IsNullOrEmpty (platform)) {
                    // handle the platform builds
                    if (!string.IsNullOrEmpty (nuspecPlatform.Value)) {
                        if (!nuspecPlatform.Value.Split (',').Contains (platform)) {
                            file.Remove ();
                        }
                    }
                } else {
                    // special case as we don't add linux-only files on CI
                    if (nuspecPlatform.Value == "linux") {
                        file.Remove ();
                    }
                }
                nuspecPlatform.Remove ();
            }
            // copy the src arrtibute and set it for the target
            file.Add (new XAttribute ("target", file.Attribute ("src").Value));
        }
    });

    var setVersion = new Action<XDocument, string> ((xdoc, suffix) => {
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id");
        var version = metadata.Element ("version");

        // <version>
        if (id != null && version != null) {
            var v = GetVersion (id.Value);
            if (!string.IsNullOrEmpty (v)) {
                version.Value = v + suffix;
            }
        }

        // <dependency>
        var dependencies = metadata
            .Elements ("dependencies")
            .Elements ("dependency");
        var groupDependencies = metadata
            .Elements ("dependencies")
            .Elements ("group")
            .Elements ("dependency");
        foreach (var package in dependencies.Union (groupDependencies)) {
            var depId = package.Attribute ("id");
            var depVersion = package.Attribute ("version");
            if (depId != null && depVersion != null) {
                var v = GetVersion (depId.Value);
                if (!string.IsNullOrEmpty (v)) {
                    depVersion.Value = v + suffix;
                }
            }
        }
    });

    foreach (var nuspec in GetFiles ("./nuget/*.nuspec")) {
        var xdoc = XDocument.Load (nuspec.FullPath);
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id");

        removePlatforms (xdoc);

        var outDir = $"./output/{id.Value}/nuget";
        DeleteFiles ($"{outDir}/*.nuspec");

        setVersion (xdoc, "");
        xdoc.Save ($"{outDir}/{id.Value}.nuspec");

        setVersion (xdoc, $"-build-{BUILD_NUMBER}");
        xdoc.Save ($"{outDir}/{id.Value}.prerelease.nuspec");

        // the legal
        CopyFile ("./LICENSE.txt", $"{outDir}/LICENSE.txt");
        CopyFile ("./External-Dependency-Info.txt", $"{outDir}/THIRD-PARTY-NOTICES.txt");
    }

    DeleteFiles ("output/nugets/*.nupkg");
    foreach (var nuspec in GetFiles ("./output/*/nuget/*.nuspec")) {
        PackageNuGet (nuspec, "./output/nugets/");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .IsDependentOn ("clean-externals")
    .IsDependentOn ("clean-managed")
    .Does (() => 
{
});
Task ("clean-managed").Does (() => 
{
    CleanDirectories ("./binding/*/bin");
    CleanDirectories ("./binding/*/obj");
    DeleteFiles ("./binding/*/project.lock.json");

    CleanDirectories ("./samples/*/*/bin");
    CleanDirectories ("./samples/*/*/obj");
    CleanDirectories ("./samples/*/*/AppPackages");
    CleanDirectories ("./samples/*/*/*/bin");
    CleanDirectories ("./samples/*/*/*/obj");
    DeleteFiles ("./samples/*/*/*/project.lock.json");
    CleanDirectories ("./samples/*/*/*/AppPackages");
    CleanDirectories ("./samples/*/*/packages");

    CleanDirectories ("./tests/**/bin");
    CleanDirectories ("./tests/**/obj");
    CleanDirectories ("./tests/**/artifacts");
    DeleteFiles ("./tests/**/project.lock.json");

    CleanDirectories ("./source/*/*/bin");
    CleanDirectories ("./source/*/*/obj");
    DeleteFiles ("./source/*/*/project.lock.json");
    CleanDirectories ("./source/*/*/Generated Files");
    CleanDirectories ("./source/packages");

    DeleteFiles ("./nuget/*.prerelease.nuspec");

    if (DirectoryExists ("./output"))
        DeleteDirectory ("./output", true);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs");

Task ("Everything")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CI - the master target to build everything
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("CI")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

Task ("Mac-CI")
    .IsDependentOn ("CI");

Task ("Windows-CI")
    .IsDependentOn ("CI");

Task ("Linux-CI")
    .IsDependentOn ("CI");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW 
////////////////////////////////////////////////////////////////////////////////////////////////////

Information ("Cake.exe ToolPath: {0}", CakeToolPath);
Information ("NuGet.exe ToolPath: {0}", NugetToolPath);
Information ("msbuild.exe ToolPath: {0}", MSBuildToolPath);

if (IS_ON_CI) {
    Information ("Detected that we are building on CI, {0}.", IS_ON_FINAL_CI ? "and on FINAL CI" : "but NOT on final CI");
} else {
    Information ("Detected that we are {0} on CI.", "NOT");
}

Information ("Environment Variables:");
foreach (var envVar in EnvironmentVariables ()) {
    Information ("\tKey: {0}\tValue: \"{1}\"", envVar.Key, envVar.Value);
}

RunTarget (TARGET);
