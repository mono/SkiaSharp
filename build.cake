#addin nuget:?package=Cake.Xamarin&version=3.0.0
#addin nuget:?package=Cake.XCode&version=4.0.0
#addin nuget:?package=Cake.FileHelpers&version=3.0.0
#addin nuget:?package=SharpCompress&version=0.22.0
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.0.0&loaddependencies=true

#tool "nuget:?package=xunit.runner.console&version=2.4.0"
#tool "nuget:?package=mdoc&version=5.7.3.1"
#tool "nuget:?package=vswhere&version=2.5.2"

using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using SharpCompress.Common;
using SharpCompress.Readers;
using Mono.ApiTools;
using NuGet.Packaging;
using NuGet.Versioning;

#load "cake/Utils.cake"

var TARGET = Argument ("t", Argument ("target", Argument ("Target", "Default")));
var VERBOSITY = (Verbosity) Enum.Parse (typeof(Verbosity), Argument ("v", Argument ("verbosity", Argument ("Verbosity", "Normal"))), true);
var SKIP_EXTERNALS = Argument ("skipexternals", Argument ("SkipExternals", "")).ToLower ().Split (',');
var PACK_ALL_PLATFORMS = Argument ("packall", Argument ("PackAll", Argument ("PackAllPlatforms", TARGET.ToLower() == "ci" || TARGET.ToLower() == "nuget-only")));
var PRINT_ALL_ENV_VARS = Argument ("printAllEnvVars", false);
var ARTIFACTS_ROOT_URL = Argument ("artifactsRootUrl", "");

var NuGetSources = new [] { MakeAbsolute (Directory ("./output/nugets")).FullPath, "https://api.nuget.org/v3/index.json" };
var NuGetToolPath = Context.Tools.Resolve ("nuget.exe");
var CakeToolPath = Context.Tools.Resolve ("Cake.exe");
var MDocPath = Context.Tools.Resolve ("mdoc.exe");
var MSBuildToolPath = GetMSBuildToolPath (EnvironmentVariable ("MSBUILD_EXE"));
var PythonToolPath = EnvironmentVariable ("PYTHON_EXE") ?? "python";

DirectoryPath ANDROID_SDK_ROOT = EnvironmentVariable ("ANDROID_SDK_ROOT") ?? EnvironmentVariable ("ANDROID_HOME") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-sdk-macosx";
DirectoryPath ANDROID_NDK_HOME = EnvironmentVariable ("ANDROID_NDK_HOME") ?? EnvironmentVariable ("ANDROID_NDK_ROOT") ?? EnvironmentVariable ("HOME") + "/Library/Developer/Xamarin/android-ndk";
DirectoryPath TIZEN_STUDIO_HOME = EnvironmentVariable ("TIZEN_STUDIO_HOME") ?? EnvironmentVariable ("HOME") + "/tizen-studio";

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));
DirectoryPath DEPOT_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/depot_tools"));
DirectoryPath SKIA_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/skia"));
DirectoryPath ANGLE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/angle"));
DirectoryPath HARFBUZZ_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/harfbuzz"));
DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/xml"));
DirectoryPath PACKAGE_CACHE_PATH = MakeAbsolute(ROOT_PATH.Combine("externals/package_cache"));

DirectoryPath PROFILE_PATH = EnvironmentVariable ("USERPROFILE") ?? EnvironmentVariable ("HOME");
DirectoryPath NUGET_PACKAGES = EnvironmentVariable ("NUGET_PACKAGES") ?? PROFILE_PATH.Combine (".nuget/packages");

var FEATURE_NAME = EnvironmentVariable ("FEATURE_NAME") ?? "";
var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? "";
if (string.IsNullOrEmpty (BUILD_NUMBER)) {
    BUILD_NUMBER = "0";
}

var TRACKED_NUGETS = new Dictionary<string, Version> {
    { "SkiaSharp",                          new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.Linux",       new Version (1, 57, 0) },
    { "SkiaSharp.Views",                    new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms",              new Version (1, 57, 0) },
    { "HarfBuzzSharp",                      new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Linux",   new Version (1, 0, 0) },
    { "SkiaSharp.HarfBuzz",                 new Version (1, 57, 0) },
};

#load "cake/UtilsManaged.cake"
#load "cake/BuildExternals.cake"
#load "cake/UpdateDocs.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .IsDependentOn ("externals-native");

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
    var RunDesktopTest = new Action<string> (arch => {
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
        RunTests ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/Release/SkiaSharp.Tests.dll", arch == "x86");
        CopyFileToDirectory ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/Release/TestResult.xml", $"./output/tests/{platform}/{arch}");
    });

    // Full .NET Framework
    if (IsRunningOnWindows ()) {
        RunDesktopTest ("x86");
        RunDesktopTest ("x64");
    } else if (IsRunningOnMac ()) {
        RunDesktopTest ("AnyCPU");
    } else if (IsRunningOnLinux ()) {
        // TODO: Disable x64 for the time being due to a bug in mono sn:
        //       https://github.com/mono/mono/issues/8218

        RunDesktopTest ("AnyCPU");
        // RunDesktopTest ("x64");
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
    CleanDirectories ("./tests/packages/skiasharp*");
    CleanDirectories ("./tests/packages/harfbuzzsharp*");
    EnsureDirectoryExists ("./output/tests/netcore");
    RunMSBuildRestoreLocal (netCoreTestProj, "./tests/packages");
    RunNetCoreTests (netCoreTestProj);
    CopyFile ("./tests/SkiaSharp.NetCore.Tests/TestResults/TestResults.xml", "./output/tests/netcore/TestResult.xml");
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
        { "android", isMac || isWin },
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
    .IsDependentOn ("nuget-only");

Task ("nuget-only")
    .Does (() =>
{
    var platform = "";
    if (!PACK_ALL_PLATFORMS) {
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
                }
                nuspecPlatform.Remove ();
            }
            // copy the src attribute and set it for the target
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

    DeleteFiles ("./output/*/nuget/*.nuspec");
    foreach (var nuspec in GetFiles ("./nuget/*.nuspec")) {
        var xdoc = XDocument.Load (nuspec.FullPath);
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id").Value;
        var dir = id;
        if (id.Contains(".NativeAssets")) {
            dir = id.Substring(0, id.IndexOf(".NativeAssets"));
        }
        var feature = "";
        if (!string.IsNullOrEmpty (FEATURE_NAME)) {
            feature = $"-{FEATURE_NAME}";
        }
        var preview = "";
        if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
            preview = $"-preview{BUILD_NUMBER}";
        }

        removePlatforms (xdoc);

        var outDir = $"./output/{dir}/nuget";

        setVersion (xdoc, $"{feature}");
        xdoc.Save ($"{outDir}/{id}.nuspec");

        setVersion (xdoc, $"{preview}{feature}");
        xdoc.Save ($"{outDir}/{id}.prerelease.nuspec");

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
// DOCS - creating the xml, markdown and other documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("update-docs")
    .IsDependentOn ("docs-api-diff")
    .IsDependentOn ("docs-api-diff-past")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .IsDependentOn ("clean-externals")
    .IsDependentOn ("clean-managed");
Task ("clean-managed")
    .Does (() =>
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

Task ("Nothing");

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

Information ("");

Information ("Tool Paths:");
Information ("  Cake.exe:   {0}", CakeToolPath);
Information ("  mdoc:       {0}", MDocPath);
Information ("  msbuild:    {0}", MSBuildToolPath);
Information ("  nuget.exe:  {0}", NuGetToolPath);
Information ("  python:     {0}", PythonToolPath);
Information ("");

Information ("Build Paths:");
Information ("  ~:              {0}", PROFILE_PATH);
Information ("  NuGet Cache:    {0}", NUGET_PACKAGES);
Information ("  root:           {0}", ROOT_PATH);
Information ("  docs:           {0}", DOCS_PATH);
Information ("  package_cache:  {0}", PACKAGE_CACHE_PATH);
Information ("  ANGLE:          {0}", ANGLE_PATH);
Information ("  depot_tools:    {0}", DEPOT_PATH);
Information ("  harfbuzz:       {0}", HARFBUZZ_PATH);
Information ("  skia:           {0}", SKIA_PATH);
Information ("");

Information ("SDK Paths:");
Information ("  Android SDK:   {0}", ANDROID_SDK_ROOT);
Information ("  Android NDK:   {0}", ANDROID_NDK_HOME);
Information ("  Tizen Studio:  {0}", TIZEN_STUDIO_HOME);
Information ("");

Information ("Environment Variables (whitelisted):");
var envVarsWhitelist = new [] {
    "path", "psmodulepath", "pwd", "shell", "processor_architecture",
    "processor_identifier", "node_name", "node_labels", "branch_name",
    "os", "build_url", "build_number", "number_of_processors",
    "node_label", "build_id", "git_sha", "git_branch_name",
    "feature_name"
};
var envVars = EnvironmentVariables ();
var max = envVars.Max (v => v.Key.Length) + 2;
foreach (var envVar in envVars.OrderBy (e => e.Key.ToLower ())) {
    if (!PRINT_ALL_ENV_VARS && !envVarsWhitelist.Contains (envVar.Key.ToLower ()))
        continue;
    var spaces = string.Concat (Enumerable.Repeat (" ", max - envVar.Key.Length));
    var toSplit = new [] { "path", "psmodulepath" };
    if (toSplit.Contains (envVar.Key.ToLower ())) {
        var paths = new string [0];
        if (IsRunningOnWindows ()) {
            paths = envVar.Value.Split (';');
        } else {
            paths = envVar.Value.Split (':');
        }
        Information ($"  {envVar.Key}:{spaces}{{0}}", paths.FirstOrDefault ());
        var keySpaces = string.Concat (Enumerable.Repeat (" ", envVar.Key.Length));
        foreach (var path in paths.Skip (1)) {
            Information ($"  {keySpaces} {spaces}{{0}}", path);
        }
    } else {
        Information ($"  {envVar.Key}:{spaces}{{0}}", envVar.Value);
    }
}
Information ("");

RunTarget (TARGET);
