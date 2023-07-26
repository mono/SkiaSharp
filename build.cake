#addin nuget:?package=Cake.Xamarin&version=3.1.0
#addin nuget:?package=Cake.XCode&version=5.0.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.Json&version=6.0.1
#addin nuget:?package=NuGet.Packaging.Core&version=5.11.0
#addin nuget:?package=SharpCompress&version=0.32.2
#addin nuget:?package=Mono.Cecil&version=0.10.0
#addin nuget:?package=Mono.ApiTools&version=5.14.0.2
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.3.5

#tool nuget:?package=mdoc&version=5.8.9
#tool nuget:?package=xunit.runner.console&version=2.4.2
#tool nuget:?package=vswhere&version=2.8.4

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

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("."));

#load "./scripts/cake/shared.cake"
#load "./scripts/cake/native-shared.cake"

var SKIP_EXTERNALS = Argument ("skipexternals", "")
    .ToLower ().Split (new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
var SKIP_BUILD = Argument ("skipbuild", false);
var PRINT_ALL_ENV_VARS = Argument ("printAllEnvVars", false);
var UNSUPPORTED_TESTS = Argument ("unsupportedTests", "");
var THROW_ON_FIRST_TEST_FAILURE = Argument ("throwOnFirstTestFailure", false);
var NUGET_DIFF_PRERELEASE = Argument ("nugetDiffPrerelease", false);
var COVERAGE = Argument ("coverage", false);
var CHROMEWEBDRIVER = Argument ("chromedriver", EnvironmentVariable ("CHROMEWEBDRIVER"));

var PLATFORM_SUPPORTS_VULKAN_TESTS = (IsRunningOnWindows () || IsRunningOnLinux ()).ToString ();
var SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? PLATFORM_SUPPORTS_VULKAN_TESTS);
var SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

var MDocPath = Context.Tools.Resolve ("mdoc.exe");

DirectoryPath DOCS_ROOT_PATH = ROOT_PATH.Combine("docs");
DirectoryPath DOCS_PATH = DOCS_ROOT_PATH.Combine("SkiaSharpAPI");

var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview");
var FEATURE_NAME = EnvironmentVariable ("FEATURE_NAME") ?? "";
var BUILD_NUMBER = Argument ("buildNumber", EnvironmentVariable ("BUILD_NUMBER") ?? "0");
var BUILD_COUNTER = Argument ("buildCounter", EnvironmentVariable ("BUILD_COUNTER") ?? BUILD_NUMBER);
var GIT_SHA = Argument ("gitSha", EnvironmentVariable ("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument ("gitBranch", EnvironmentVariable ("GIT_BRANCH_NAME") ?? ""). Replace ("refs/heads/", "");
var GIT_URL = Argument ("gitUrl", EnvironmentVariable ("GIT_URL") ?? "");

var PREVIEW_NUGET_SUFFIX = "";
if (!string.IsNullOrEmpty (FEATURE_NAME)) {
    PREVIEW_NUGET_SUFFIX = $"featurepreview-{FEATURE_NAME}";
} else {
    PREVIEW_NUGET_SUFFIX = $"{PREVIEW_LABEL}";
}
if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
    PREVIEW_NUGET_SUFFIX += $".{BUILD_NUMBER}";
}

var MSBUILD_VERSION_PROPERTIES = new Dictionary<string, string> {
    { "GIT_SHA", GIT_SHA },
    { "GIT_BRANCH_NAME", GIT_BRANCH_NAME },
    { "GIT_URL", GIT_URL },
    { "BUILD_COUNTER", BUILD_COUNTER },
    { "BUILD_NUMBER", BUILD_NUMBER },
    { "FEATURE_NAME", FEATURE_NAME },
    { "PREVIEW_LABEL", PREVIEW_LABEL },
};

var CURRENT_PLATFORM = "";
if (IsRunningOnWindows ()) {
    CURRENT_PLATFORM = "Windows";
} else if (IsRunningOnMacOs ()) {
    CURRENT_PLATFORM = "Mac";
} else if (IsRunningOnLinux ()) {
    CURRENT_PLATFORM = "Linux";
} else {
    throw new Exception ("This script is not running on a known platform.");
}

var PREVIEW_FEED_URL = Argument ("previewFeed", "https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp/nuget/v3/index.json");

var TRACKED_NUGETS = new Dictionary<string, Version> {
    { "SkiaSharp",                                     new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.Linux",                  new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.Linux.NoDependencies",   new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.NanoServer",             new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.WebAssembly",            new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.Android",                new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.iOS",                    new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.MacCatalyst",            new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.macOS",                  new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.Tizen",                  new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.tvOS",                   new Version (1, 60, 0) },
    { "SkiaSharp.NativeAssets.Win32",                  new Version (1, 60, 0) },
    { "SkiaSharp.Views",                               new Version (1, 60, 0) },
    { "SkiaSharp.Views.Desktop.Common",                new Version (1, 60, 0) },
    { "SkiaSharp.Views.Gtk3",                          new Version (1, 60, 0) },
    { "SkiaSharp.Views.WindowsForms",                  new Version (1, 60, 0) },
    { "SkiaSharp.Views.WPF",                           new Version (1, 60, 0) },
    { "SkiaSharp.Views.Uno.WinUI",                     new Version (1, 60, 0) },
    { "SkiaSharp.Views.WinUI",                         new Version (1, 60, 0) },
    { "SkiaSharp.Views.Maui.Core",                     new Version (1, 60, 0) },
    { "SkiaSharp.Views.Maui.Controls",                 new Version (1, 60, 0) },
    { "SkiaSharp.Views.Maui.Controls.Compatibility",   new Version (1, 60, 0) },
    { "SkiaSharp.Views.Blazor",                        new Version (1, 60, 0) },
    { "HarfBuzzSharp",                                 new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Android",            new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.iOS",                new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Linux",              new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.MacCatalyst",        new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.macOS",              new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Tizen",              new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.tvOS",               new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.WebAssembly",        new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Win32",              new Version (1, 0, 0) },
    { "SkiaSharp.HarfBuzz",                            new Version (1, 60, 0) },
    { "SkiaSharp.Skottie",                             new Version (1, 60, 0) },
    { "SkiaSharp.SceneGraph",                          new Version (1, 60, 0) },
    { "SkiaSharp.Vulkan.SharpVk",                      new Version (1, 60, 0) },
};

var PREVIEW_ONLY_NUGETS = new List<string> {
};

var DATE_TIME_NOW = DateTime.Now;
var DATE_TIME_STR = DATE_TIME_NOW.ToString ("yyyyMMdd_hhmmss");

Information("Source Control:");
Information($"    {"PREVIEW_LABEL".PadRight(30)} {{0}}", PREVIEW_LABEL);
Information($"    {"FEATURE_NAME".PadRight(30)} {{0}}", FEATURE_NAME);
Information($"    {"BUILD_NUMBER".PadRight(30)} {{0}}", BUILD_NUMBER);
Information($"    {"GIT_SHA".PadRight(30)} {{0}}", GIT_SHA);
Information($"    {"GIT_BRANCH_NAME".PadRight(30)} {{0}}", GIT_BRANCH_NAME);
Information($"    {"GIT_URL".PadRight(30)} {{0}}", GIT_URL);

#load "./scripts/cake/msbuild.cake"
#load "./scripts/cake/UtilsManaged.cake"
#load "./scripts/cake/externals.cake"
#load "./scripts/cake/UpdateDocs.cake"
#load "./scripts/cake/samples.cake"

Task ("__________________________________")
    .Description ("__________________________________________________");

////////////////////////////////////////////////////////////////////////////////////////////////////
// EXTERNALS - the native C and C++ libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

// this builds all the externals
Task ("externals")
    .Description ("Build all external dependencies.")
    .IsDependentOn ("externals-native");

////////////////////////////////////////////////////////////////////////////////////////////////////
// LIBS - the managed C# libraries
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("libs")
    .Description ("Build all managed assemblies.")
    .WithCriteria (!SKIP_BUILD)
    .IsDependentOn ("externals")
    .Does (() =>
{
    RunDotNetBuild ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", properties: MSBUILD_VERSION_PROPERTIES);
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// TESTS - some test cases to make sure it works
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("tests")
    .Description ("Run all tests.")
    .IsDependentOn ("tests-netfx")
    .IsDependentOn ("tests-netcore")
    .IsDependentOn ("tests-android")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

Task ("tests-netfx")
    .Description ("Run all Full .NET Framework tests.")
    .WithCriteria (IsRunningOnWindows ())
    .IsDependentOn ("externals")
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    var failedTests = 0;

    foreach ( var arch in new [] { "x86", "x64" }) {
        if (Skip(arch)) continue;

        var testAssemblies = new List<string> { "SkiaSharp.Tests.Console" };
        if (SUPPORT_VULKAN)
            testAssemblies.Add ("SkiaSharp.Vulkan.Tests.Console");
        foreach (var testAssembly in testAssemblies) {
            // build
            var csproj = $"./tests/{testAssembly}/{testAssembly}.csproj";
            RunDotNetBuild (csproj, platform: arch);

            // test
            DirectoryPath results = $"./output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}";
            EnsureDirectoryExists (results);
            try {
                var assName = testAssembly.Replace (".Console", "");
                RunTests ($"./tests/{testAssembly}/bin/{arch}/{CONFIGURATION}/net472/{assName}.dll", arch == "x86");
            } catch {
                failedTests++;
                if (THROW_ON_FIRST_TEST_FAILURE)
                    throw;
            }
        }
    }

    if (failedTests > 0) {
        throw new Exception ($"There were {failedTests} failed test runs.");
    }
});

Task ("tests-netcore")
    .Description ("Run all .NET Core tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    if (IsRunningOnLinux ()) {
        try {
            RunProcess ("dpkg", "-s libfontconfig1 ttf-ancient-fonts ttf-mscorefonts-installer", out var _);
        } catch {
            Warning ("Running tests on Linux requires that FontConfig and various font packages are installed. Run the `./scripts/install-linux-test-requirements.sh` script file.");
        }
    }

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    var failedTests = 0;

    var testAssemblies = new List<string> { "SkiaSharp.Tests.Console" };
    if (SUPPORT_VULKAN)
        testAssemblies.Add ("SkiaSharp.Vulkan.Tests.Console");
    foreach (var testAssembly in testAssemblies) {
        // build
        var csproj = $"./tests/{testAssembly}/{testAssembly}.csproj";
        RunDotNetBuild (csproj);

        // test
        try {
            RunDotNetTest (csproj, $"./output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}");
        } catch {
            failedTests++;
            if (THROW_ON_FIRST_TEST_FAILURE)
                throw;
        }
    }

    if (failedTests > 0) {
        throw new Exception ($"There were {failedTests} failed test runs.");
    }

    if (COVERAGE) {
        RunCodeCoverage ("./output/logs/testlogs/**/Coverage/**/*.xml", "./output/coverage");
    }
});

Task ("tests-android")
    .Description ("Run all Android tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    FilePath csproj = "./tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    var configuration = "Release";
    var tfm = "net7.0-android";
    var rid = "android-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    FilePath app = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}/com.companyname.SkiaSharpTests-Signed.apk";

    // build the app
    RunDotNetBuild (csproj,
        configuration: configuration,
        properties: new Dictionary<string, string> {
            { "TargetFramework", tfm },
            { "RuntimeIdentifier", rid },
        });

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.Android/{DATE_TIME_STR}";
    RunCake ("./scripts/cake/xharness-android.cake", "Default", new Dictionary<string, string> {
        { "app", MakeAbsolute (app).FullPath },
        { "results", MakeAbsolute (results).FullPath },
    });
});

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    FilePath csproj = "./tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    var configuration = "Debug";
    var tfm = "net7.0-ios";
    var rid = "iossimulator-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    FilePath app = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}/SkiaSharp.Tests.Devices.app";

    // package the app
    RunDotNetBuild (csproj,
        configuration: configuration,
        properties: new Dictionary<string, string> {
            { "TargetFramework", tfm },
            { "RuntimeIdentifier", rid },
        });

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.iOS/{DATE_TIME_STR}";
    RunCake ("./scripts/cake/xharness-apple.cake", "Default", new Dictionary<string, string> {
        { "app", MakeAbsolute (app).FullPath },
        { "results", MakeAbsolute (results).FullPath },
    });
});

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    FilePath csproj = "./tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    var configuration = "Debug";
    var tfm = "net7.0-maccatalyst";
    var rid = "maccatalyst-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    FilePath app = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}/SkiaSharp.Tests.Devices.app";

    // package the app
    RunDotNetBuild (csproj,
        configuration: configuration,
        properties: new Dictionary<string, string> {
            { "TargetFramework", tfm },
            { "RuntimeIdentifier", rid },
        });

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.MacCatalyst/{DATE_TIME_STR}";
    RunCake ("./scripts/cake/xharness-apple.cake", "Default", new Dictionary<string, string> {
        { "app", MakeAbsolute (app).FullPath },
        { "results", MakeAbsolute (results).FullPath },
        { "device", "maccatalyst" },
    });
});

Task ("tests-wasm")
    .Description ("Run WASM tests.")
    .IsDependentOn ("externals-wasm")
    .Does (() =>
{
    RunMSBuild ("./tests/SkiaSharp.Tests.Wasm.sln");

    var pubDir = "./tests/SkiaSharp.Tests.Wasm/bin/publish/";
    RunDotNetPublish("./tests/SkiaSharp.Tests.Wasm/SkiaSharp.Tests.Wasm.csproj", pubDir);
    IProcess serverProc = null;
    try {
        serverProc = RunAndReturnProcess(PYTHON_EXE, new ProcessSettings {
            Arguments = "server.py",
            WorkingDirectory = pubDir,
        });
        DotNetCoreRun("./utils/WasmTestRunner/WasmTestRunner.csproj",
            "--output=\"./tests/SkiaSharp.Tests.Wasm/TestResults/\" " +
            (string.IsNullOrEmpty(CHROMEWEBDRIVER) ? "" : $"--driver=\"{CHROMEWEBDRIVER}\" ") +
            "--verbose " +
            "\"http://127.0.0.1:8000/\" ");
    } finally {
        serverProc?.Kill();
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - the demo apps showing off the work
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples-generate")
    .Description ("Generate and zip the samples directory structure.")
    .Does (() =>
{
    EnsureDirectoryExists ("./output/");

    // create the interactive archive
    Zip ("./interactive", "./output/interactive.zip");

    // create the samples archive
    CreateSamplesDirectory ("./samples/", "./output/samples/");
    Zip ("./output/samples/", "./output/samples.zip");

    // create the preview samples archive
    CreateSamplesDirectory ("./samples/", "./output/samples-preview/", PREVIEW_NUGET_SUFFIX);
    Zip ("./output/samples-preview/", "./output/samples-preview.zip");
});

Task ("samples")
    .Description ("Build all samples.")
    .IsDependentOn ("samples-generate")
    .Does(() =>
{
    var isLinux = IsRunningOnLinux ();
    var isMac = IsRunningOnMacOs ();
    var isWin = IsRunningOnWindows ();

    void BuildSample (FilePath sln, bool dryrun)
    {
        var platform = sln.GetDirectory ().GetDirectoryName ().ToLower ();
        var name = sln.GetFilenameWithoutExtension ();
        var slnPlatform = name.GetExtension ();
        if (!string.IsNullOrEmpty (slnPlatform)) {
            slnPlatform = slnPlatform.ToLower ();
        }

        if (dryrun) {
            Information ($"    BUILD       {sln}");
        } else {
            Information ($"Building sample {sln} ({platform})...");
            RunDotNetBuild (sln);
        }
    }

    // build the newly migrated samples
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    // TODO: Docker seems to be having issues on the old DevOps agents

    // // copy all the packages next to the Dockerfile files
    // var dockerfiles = GetFiles ("./output/samples/**/Dockerfile");
    // foreach (var dockerfile in dockerfiles) {
    //     CopyDirectory (OUTPUT_NUGETS_PATH, dockerfile.GetDirectory ().Combine ("packages"));
    // }

    // // build the run.ps1 (typically for Dockerfiles)
    // var runs = GetFiles ("./output/samples/**/run.ps1");
    // foreach (var run in runs) {
    //     RunProcess ("pwsh", new ProcessSettings {
    //         Arguments = run.FullPath,
    //         WorkingDirectory = run.GetDirectory (),
    //     });
    // }

    // build solutions locally
    var actualSamples = PREVIEW_ONLY_NUGETS.Count > 0
        ? "samples-preview"
        : "samples";
    var solutions =
        GetFiles ($"./output/{actualSamples}/**/*.sln").Union (
        GetFiles ($"./output/{actualSamples}/**/*.slnf"))
        .OrderBy (x => x.FullPath)
        .ToArray ();

    Information ("Solutions found:");
    foreach (var sln in solutions) {
        Information ("    " + sln);
    }

    var firstLoop = true;
    foreach (var dryrun in new [] { true, true, false }) {
        if (dryrun)
            Information ("Sample builds:");

        foreach (var sln in solutions) {
            // might have been deleted due to a platform build and cleanup
            if (!FileExists (sln))
                continue;

            var name = sln.GetFilenameWithoutExtension ();
            var slnPlatform = name.GetExtension ();
            if (string.IsNullOrEmpty (slnPlatform)) {
                // this is the main solution
                var variants =
                    GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.sln").Union (
                    GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.slnf"));
                if (!variants.Any ()) {
                    // there is no platform variant
                    BuildSample (sln, dryrun);
                    // delete the built sample
                    if (!dryrun)
                        CleanDir (sln.GetDirectory ().FullPath);
                } else {
                    // skip as there is a platform variant
                    if (dryrun && firstLoop)
                        Information ($"    SKIP   (PS) {sln} (has platform specific)");
                }
            } else {
                // this is a platform variant
                slnPlatform = slnPlatform.ToLower ();
                if (slnPlatform == ($".{CURRENT_PLATFORM.ToLower ()}")) {
                    // this is the correct platform variant
                    BuildSample (sln, dryrun);
                    // delete the built sample
                    if (!dryrun) {
                        CleanDir (sln.GetDirectory ().FullPath);
                    }
                } else {
                    // skip this as this is not the correct platform
                    if (dryrun && firstLoop)
                        Information ($"    SKIP   (AP) {sln} (has alternate platform)");
                }
            }
        }

        firstLoop = false;
    }

    CleanDir ("./output/samples/");
    DeleteDir ("./output/samples/");
    CleanDir ("./output/samples-preview/");
    DeleteDir ("./output/samples-preview/");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// NUGET - building the package for NuGet.org
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("nuget")
    .Description ("Pack all NuGets.")
    .IsDependentOn ("nuget-normal")
    .IsDependentOn ("nuget-special");

Task ("nuget-normal")
    .Description ("Pack all NuGets (build all required dependencies).")
    .IsDependentOn ("libs")
    .Does (() =>
{
    var props = new Dictionary<string, string> (MSBUILD_VERSION_PROPERTIES) {
        { "BuildingInsideUnoSourceGenerator", "true" },
        { "BuildProjectReferences", "false" },
    };

    // pack stable
    RunDotNetPack ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pack", properties: props);

    // pack preview
    props ["VersionSuffix"] = PREVIEW_NUGET_SUFFIX;
    RunDotNetPack ($"./source/SkiaSharpSource.{CURRENT_PLATFORM}.slnf", bl: ".pre.pack", properties: props);

    // move symbols to a special location to avoid signing
    EnsureDirectoryExists ($"{OUTPUT_SYMBOLS_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SYMBOLS_NUGETS_PATH}/*.nupkg");
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.snupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
    MoveFiles ($"{OUTPUT_NUGETS_PATH}/*.symbols.nupkg", OUTPUT_SYMBOLS_NUGETS_PATH);
});

Task ("nuget-special")
    .Description ("Pack all special NuGets.")
    .IsDependentOn ("nuget-normal")
    .Does (() =>
{
    EnsureDirectoryExists ($"{OUTPUT_SPECIAL_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SPECIAL_NUGETS_PATH}/*.nupkg");

    // get a list of all the version number variants
    var versions = new Dictionary<string, string> ();
    if (!string.IsNullOrEmpty (PREVIEW_LABEL) && PREVIEW_LABEL.StartsWith ("pr.")) {
        var v = $"0.0.0-{PREVIEW_LABEL}";
        if (!string.IsNullOrEmpty (BUILD_COUNTER))
            v += $".{BUILD_COUNTER}";
        versions.Add ("pr", v);
    } else {
        if (!string.IsNullOrEmpty (GIT_SHA)) {
            var v = $"0.0.0-commit.{GIT_SHA}";
            if (!string.IsNullOrEmpty (BUILD_COUNTER))
                v += $".{BUILD_COUNTER}";
            versions.Add ("commit", v);
        }
        if (!string.IsNullOrEmpty (GIT_BRANCH_NAME)) {
            var v = $"0.0.0-branch.{GIT_BRANCH_NAME.Replace ("/", ".")}";
            if (!string.IsNullOrEmpty (BUILD_COUNTER))
                v += $".{BUILD_COUNTER}";
            versions.Add ("branch", v);
        }
    }
    Information ("Detected {0} special versions to process:", versions.Count);
    var max = 0;
    foreach (var version in versions) {
        if (version.Key.Length > max)
            max = version.Key.Length + 1;
    }
    foreach (var version in versions) {
        Information ("  - {0}" + " ".PadRight(max - version.Key.Length) + "=> {1}", version.Key, version.Value);
    }

    // get a list of all the nuspecs to pack
    var specials = new Dictionary<string, string> ();
    var nativePlatforms = GetDirectories ("./output/native/*")
        .Select (d => d.GetDirectoryName ())
        .ToArray ();
    if (nativePlatforms.Length > 0) {
        specials[$"_NativeAssets"] = $"native";
        foreach (var platform in nativePlatforms) {
            specials[$"_NativeAssets.{platform}"] = $"native/{platform}";
        }
    }
    if (GetFiles ("./output/nugets/*.nupkg").Count > 0) {
        specials[$"_NuGets"] = $"nugets";
        specials[$"_NuGetsPreview"] = $"nugets";
        specials[$"_Symbols"] = $"nugets-symbols";
        specials[$"_SymbolsPreview"] = $"nugets-symbols";
    }
    Information ("Detected {0} special artifacts to process:", specials.Count);
    max = 0;
    foreach (var special in specials) {
        if (special.Key.Length > max)
            max = special.Key.Length + 1;
    }
    foreach (var special in specials) {
        Information ("  - {0}" + " ".PadRight(max - special.Key.Length) + "=> {1}", special.Key, special.Value);
    }

    foreach (var pair in specials) {
        var id = pair.Key;
        var path = pair.Value;
        var nuspec = $"./output/{path}/{id}.nuspec";

        DeleteFiles ($"./output/{path}/*.nuspec");

        foreach (var version in versions) {
            // update the version
            var packageVersion = version.Value;
            var fn = id.StartsWith ("_NativeAssets.") ? "_NativeAssets" : id;
            var xdoc = XDocument.Load ($"./scripts/nuget/{fn}.nuspec");
            var metadata = xdoc.Root.Element ("metadata");
            metadata.Element ("version").Value = packageVersion;
            metadata.Element ("id").Value = id;

            if (id == "_NativeAssets") {
                // handle the root package
                var dependencies = metadata.Element ("dependencies");
                foreach (var platform in nativePlatforms) {
                    dependencies.Add (new XElement ("dependency",
                        new XAttribute ("id", $"_NativeAssets.{platform}"),
                        new XAttribute ("version", packageVersion)));
                }
            } else if (id.StartsWith ("_NativeAssets.")) {
                // handle the dependencies
                var platform = id.Substring (id.IndexOf (".") + 1);
                var files = xdoc.Root.Element ("files");
                files.Add (new XElement ("file",
                    new XAttribute ("src", $"**"),
                    new XAttribute ("target", $"tools/{platform}")));
            }
            // add the readme
            {
                var files = xdoc.Root.Element ("files");
                files.Add (new XElement ("file",
                    new XAttribute ("src", MakeAbsolute(File("./scripts/nuget/README.md")).FullPath),
                    new XAttribute ("target", $"README.md")));
            }

            // save and pack
            xdoc.Save (nuspec);
            RunDotNetPack (
                "./scripts/nuget/NuGet.csproj",
                OUTPUT_SPECIAL_NUGETS_PATH,
                bl: $".{id}.{version.Key}",
                additionalArgs: "/restore /nologo",
                properties: new Dictionary<string, string> {
                    { "NuspecFile", MakeAbsolute(File(nuspec)).FullPath },
                });
        }

        DeleteFiles ($"./output/{path}/*.nuspec");
    }
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DOCS - creating the xml, markdown and other documentation
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("update-docs")
    .Description ("Regenerate all docs.")
    .IsDependentOn ("docs-api-diff")
    .IsDependentOn ("docs-update-frameworks")
    .IsDependentOn ("docs-format-docs");

////////////////////////////////////////////////////////////////////////////////////////////////////
// CLEAN - remove all the build artefacts
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("clean")
    .Description ("Clean up.")
    .IsDependentOn ("clean-externals")
    .IsDependentOn ("clean-managed");

Task ("clean-managed")
    .Description ("Clean up (managed only).")
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

    DeleteDir ("./output");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEFAULT - target for common development
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("----------------------------------")
    .Description ("--------------------------------------------------");

Task ("Default")
    .Description ("Build all managed assemblies and external dependencies.")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs");

Task ("Everything")
    .Description ("Build, pack and test everything.")
    .IsDependentOn ("externals")
    .IsDependentOn ("libs")
    .IsDependentOn ("nuget")
    .IsDependentOn ("tests")
    .IsDependentOn ("samples");

////////////////////////////////////////////////////////////////////////////////////////////////////
// BUILD NOW
////////////////////////////////////////////////////////////////////////////////////////////////////

RunTarget (TARGET);
