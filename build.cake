#addin nuget:?package=Cake.Xamarin&version=3.0.2
#addin nuget:?package=Cake.XCode&version=4.2.0
#addin nuget:?package=Cake.FileHelpers&version=3.2.1
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=SharpCompress&version=0.24.0
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.3.2&loaddependencies=true
#addin nuget:?package=Xamarin.Nuget.Validator&version=1.1.1

#tool nuget:?package=mdoc&version=5.7.4.10
#tool nuget:?package=xunit.runner.console&version=2.4.1
#tool nuget:?package=vswhere&version=2.7.1

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

#load "cake/shared.cake"
#load "cake/native-shared.cake"

var SKIP_EXTERNALS = Argument ("skipexternals", "")
    .ToLower ().Split (new [] { ',' }, StringSplitOptions.RemoveEmptyEntries);
var SKIP_BUILD = Argument ("skipbuild", false);
var PACK_ALL_PLATFORMS = Argument ("packall", Argument ("PackAllPlatforms", false));
var BUILD_ALL_PLATFORMS = Argument ("buildall", Argument ("BuildAllPlatforms", false));
var PRINT_ALL_ENV_VARS = Argument ("printAllEnvVars", false);
var UNSUPPORTED_TESTS = Argument ("unsupportedTests", "");
var THROW_ON_TEST_FAILURE = Argument ("throwOnTestFailure", true);
var NUGET_DIFF_PRERELEASE = Argument ("nugetDiffPrerelease", false);
var COVERAGE = Argument ("coverage", false);
var CHROMEWEBDRIVER = Argument ("chromedriver", EnvironmentVariable ("CHROMEWEBDRIVER"));

var PLATFORM_SUPPORTS_VULKAN_TESTS = (IsRunningOnWindows () || IsRunningOnLinux ()).ToString ();
var SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? PLATFORM_SUPPORTS_VULKAN_TESTS);
var SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

var CakeToolPath = Context.Tools.Resolve ("Cake.exe");
var MDocPath = Context.Tools.Resolve ("mdoc.exe");

DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/SkiaSharpAPI"));

var PREVIEW_LABEL = Argument ("previewLabel", EnvironmentVariable ("PREVIEW_LABEL") ?? "preview");
var FEATURE_NAME = EnvironmentVariable ("FEATURE_NAME") ?? "";
var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? "0";
var GIT_SHA = Argument ("gitSha", EnvironmentVariable ("GIT_SHA") ?? "");
var GIT_BRANCH_NAME = Argument ("gitBranch", EnvironmentVariable ("GIT_BRANCH_NAME") ?? "");

var PREVIEW_FEED_URL = "https://pkgs.dev.azure.com/xamarin/public/_packaging/SkiaSharp/nuget/v3/index.json";

var TRACKED_NUGETS = new Dictionary<string, Version> {
    { "SkiaSharp",                                     new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.Linux",                  new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.Linux.NoDependencies",   new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.NanoServer",             new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.WebAssembly",            new Version (1, 57, 0) },
    { "SkiaSharp.Views",                               new Version (1, 57, 0) },
    { "SkiaSharp.Views.Desktop.Common",                new Version (1, 57, 0) },
    { "SkiaSharp.Views.Gtk2",                          new Version (1, 57, 0) },
    { "SkiaSharp.Views.Gtk3",                          new Version (1, 57, 0) },
    { "SkiaSharp.Views.WindowsForms",                  new Version (1, 57, 0) },
    { "SkiaSharp.Views.WPF",                           new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms",                         new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms.WPF",                     new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms.GTK",                     new Version (1, 57, 0) },
    { "SkiaSharp.Views.Uno",                           new Version (1, 57, 0) },
    { "HarfBuzzSharp",                                 new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Linux",              new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.WebAssembly",        new Version (1, 0, 0) },
    { "SkiaSharp.HarfBuzz",                            new Version (1, 57, 0) },
    { "SkiaSharp.Vulkan.SharpVk",                      new Version (1, 57, 0) },
};

Information("Arguments:");
foreach (var arg in CAKE_ARGUMENTS) {
    Information($"    {arg.Key.PadRight(30)} {{0}}", arg.Value);
}

#load "cake/msbuild.cake"
#load "cake/UtilsManaged.cake"
#load "cake/externals.cake"
#load "cake/UpdateDocs.cake"
#load "cake/samples.cake"

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
    .WithCriteria(!SKIP_BUILD)
    .IsDependentOn ("externals")
    .Does (() =>
{
    // build the managed libraries
    var platform = "";
    if (!BUILD_ALL_PLATFORMS) {
        if (IsRunningOnWindows ()) {
            platform = ".Windows";
        } else if (IsRunningOnMac ()) {
            platform = ".Mac";
        } else if (IsRunningOnLinux ()) {
            platform = ".Linux";
        }
    }
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
    .Description ("Run all tests.")
    .IsDependentOn ("tests-netfx")
    .IsDependentOn ("tests-netcore")
    .IsDependentOn ("tests-android")
    .IsDependentOn ("tests-ios");

Task ("tests-netfx")
    .Description ("Run all Full .NET Framework tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    var failedTests = 0;

    void RunDesktopTest (string arch)
    {
        RunMSBuild ("./tests/SkiaSharp.Desktop.Tests.sln", platform: arch == "AnyCPU" ? "Any CPU" : arch);

        // SkiaSharp.Tests.dll
        try {
            RunTests ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/{CONFIGURATION}/SkiaSharp.Tests.dll", arch == "x86");
        } catch {
            failedTests++;
        }

        // SkiaSharp.Vulkan.Tests.dll
        if (SUPPORT_VULKAN) {
            try {
                RunTests ($"./tests/SkiaSharp.Vulkan.Desktop.Tests/bin/{arch}/{CONFIGURATION}/SkiaSharp.Vulkan.Tests.dll", arch == "x86");
            } catch {
                failedTests++;
            }
        }
    }

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    if (IsRunningOnWindows ()) {
        RunDesktopTest ("x86");
        RunDesktopTest ("x64");
    } else if (IsRunningOnMac ()) {
        RunDesktopTest ("AnyCPU");
    } else if (IsRunningOnLinux ()) {
        RunDesktopTest ("x64");
    }

    if (failedTests > 0) {
        if (THROW_ON_TEST_FAILURE)
            throw new Exception ($"There were {failedTests} failed tests.");
        else
            Warning ($"There were {failedTests} failed tests.");
    }
});

Task ("tests-netcore")
    .Description ("Run all .NET Core tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    var failedTests = 0;

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    // SkiaSharp.NetCore.Tests.csproj
    RunMSBuild ("./tests/SkiaSharp.NetCore.Tests.sln");
    try {
        RunNetCoreTests ("./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.csproj");
    } catch {
        failedTests++;
    }

    // SkiaSharp.Vulkan.NetCore.Tests.csproj
    if (SUPPORT_VULKAN) {
        try {
            RunNetCoreTests ("./tests/SkiaSharp.Vulkan.NetCore.Tests/SkiaSharp.Vulkan.NetCore.Tests.csproj");
        } catch {
            failedTests++;
        }
    }

    if (failedTests > 0) {
        if (THROW_ON_TEST_FAILURE)
            throw new Exception ($"There were {failedTests} failed tests.");
        else
            Warning ($"There were {failedTests} failed tests.");
    }
    if (COVERAGE) {
        RunCodeCoverage ("./tests/**/Coverage/**/*.xml", "./output/coverage");
    }
});

Task ("tests-android")
    .Description ("Run all Android tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    var failedTests = 0;

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    // SkiaSharp.Android.Tests.csproj
    try {
        // build the solution to copy all the files
        RunMSBuild ("./tests/SkiaSharp.Android.Tests.sln", configuration: "Debug");
        // package the app
        FilePath csproj = "./tests/SkiaSharp.Android.Tests/SkiaSharp.Android.Tests.csproj";
        RunMSBuild (csproj,
            targets: new [] { "SignAndroidPackage" }, 
            platform: "AnyCPU",
            configuration: "Debug");
        // run the tests
        DirectoryPath results = "./output/testlogs/SkiaSharp.Android.Tests";
        RunCake ("./cake/xharness-android.cake", "Default", new Dictionary<string, string> {
            { "project", MakeAbsolute(csproj).FullPath },
            { "configuration", "Debug" },
            { "exclusive", "true" },
            { "results", MakeAbsolute(results).FullPath },
        });
    } catch {
        failedTests++;
    }

    if (failedTests > 0) {
        if (THROW_ON_TEST_FAILURE)
            throw new Exception ($"There were {failedTests} failed tests.");
        else
            Warning ($"There were {failedTests} failed tests.");
    }
});

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .IsDependentOn ("externals")
    .Does (() =>
{
    var failedTests = 0;

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    // SkiaSharp.iOS.Tests.csproj
    try {
        // build the solution to copy all the files
        RunMSBuild ("./tests/SkiaSharp.iOS.Tests.sln", configuration: "Debug");
        // package the app
        FilePath csproj = "./tests/SkiaSharp.iOS.Tests/SkiaSharp.iOS.Tests.csproj";
        RunMSBuild (csproj,
            properties: new Dictionary<string, string> { { "BuildIpa", "true" } },
            platform: "iPhoneSimulator",
            configuration: "Debug");
        // run the tests
        DirectoryPath results = "./output/testlogs/SkiaSharp.iOS.Tests";
        RunCake ("./cake/xharness-ios.cake", "Default", new Dictionary<string, string> {
            { "project", MakeAbsolute(csproj).FullPath },
            { "configuration", "Debug" },
            { "exclusive", "true" },
            { "results", MakeAbsolute(results).FullPath },
        });
    } catch {
        failedTests++;
    }

    if (failedTests > 0) {
        if (THROW_ON_TEST_FAILURE)
            throw new Exception ($"There were {failedTests} failed tests.");
        else
            Warning ($"There were {failedTests} failed tests.");
    }
});

Task ("tests-wasm")
    .Description ("Run WASM tests.")
    .IsDependentOn ("externals-wasm")
    .Does (() =>
{
    var failedTests = 0;

    RunMSBuild ("./tests/SkiaSharp.Wasm.Tests.sln");

    var pubDir = "./tests/SkiaSharp.Wasm.Tests/bin/publish/";
    RunNetCorePublish("./tests/SkiaSharp.Wasm.Tests/SkiaSharp.Wasm.Tests.csproj", pubDir);
    IProcess serverProc = null;
    try {
        serverProc = RunAndReturnProcess(PYTHON_EXE, new ProcessSettings {
            Arguments = "server.py",
            WorkingDirectory = pubDir,
        });
        DotNetCoreRun("./utils/WasmTestRunner/WasmTestRunner.csproj",
            "http://localhost:8000/ " +
            "-o ./tests/SkiaSharp.Wasm.Tests/TestResults/ " +
            (string.IsNullOrEmpty(CHROMEWEBDRIVER) ? "" : $"-d {CHROMEWEBDRIVER}"));
    } catch {
        failedTests++;
    } finally {
        serverProc?.Kill();
    }

    if (failedTests > 0) {
        if (THROW_ON_TEST_FAILURE)
            throw new Exception ($"There were {failedTests} failed tests.");
        else
            Warning ($"There were {failedTests} failed tests.");
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

    // create the workbooks archive
    Zip ("./workbooks", "./output/workbooks.zip");

    // create the samples archive
    CreateSamplesDirectory ("./samples/", "./output/samples/");
    Zip ("./output/samples/", "./output/samples.zip");

    // create the preview samples archive
    var suffix = string.IsNullOrEmpty (BUILD_NUMBER)
        ? $"{PREVIEW_LABEL}"
        : $"{PREVIEW_LABEL}.{BUILD_NUMBER}";
    CreateSamplesDirectory ("./samples/", "./output/samples-preview/", suffix);
    Zip ("./output/samples-preview/", "./output/samples-preview.zip");
});

Task ("samples")
    .Description ("Build all samples.")
    .IsDependentOn ("samples-generate")
    .Does(() =>
{
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

    void BuildSample (FilePath sln)
    {
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

            RunNuGetRestorePackagesConfig (sln);
            RunMSBuild (sln, platform: buildPlatform);
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
    var solutions = GetFiles ("./output/samples/**/*.sln");
    foreach (var sln in solutions) {
        var name = sln.GetFilenameWithoutExtension ();
        var slnPlatform = name.GetExtension ();

        if (string.IsNullOrEmpty (slnPlatform)) {
            // this is the main solution
            var variants = GetFiles (sln.GetDirectory ().CombineWithFilePath (name) + ".*.sln");
            if (!variants.Any ()) {
                // there is no platform variant
                BuildSample (sln);
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
                BuildSample (sln);
            } else {
                // skip this as this is not the correct platform
            }
        }
    }

    CleanDirectory ("./output/samples/");
    DeleteDirectory ("./output/samples/");
    CleanDirectory ("./output/samples-preview/");
    DeleteDirectory ("./output/samples-preview/");
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

    void RemovePlatforms (XDocument xdoc)
    {
        var files = xdoc.Root
            .Elements ("files")
            .Elements ("file");
        foreach (var file in files.ToArray ()) {
            // remove the files that aren't available
            var nuspecPlatform = file.Attribute ("platform");
            if (!string.IsNullOrEmpty (nuspecPlatform?.Value)) {
                nuspecPlatform.Remove ();
                if (!string.IsNullOrEmpty (platform)) {
                    // handle the platform builds
                    if (!nuspecPlatform.Value.Split (',').Contains (platform)) {
                        file.Remove ();
                    }
                }
            }
            // copy the src attribute and set it for the target if there is none already
            if (string.IsNullOrEmpty (file.Attribute ("target")?.Value)) {
                file.Add (new XAttribute ("target", file.Attribute ("src").Value));
            }
            // make sure all the paths have the correct slash
            if (IsRunningOnWindows ()) {
                file.Attribute ("src").Value = file.Attribute ("src").Value.Replace ("/", "\\");
                file.Attribute ("target").Value = file.Attribute ("target").Value.Replace ("/", "\\");
            }
        }
    }

    void SetVersion (XDocument xdoc, string suffix)
    {
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id");
        var version = metadata.Element ("version");

        // <version>
        if (id != null && version != null) {
            var v = GetVersion (id.Value);
            if (!string.IsNullOrEmpty (v)) {
                if (id.Value.StartsWith("SkiaSharp") || id.Value.StartsWith("HarfBuzzSharp"))
                    v += suffix;
                version.Value = v;
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
                    if (depId.Value.StartsWith("SkiaSharp") || depId.Value.StartsWith("HarfBuzzSharp"))
                        v += suffix;
                    depVersion.Value = v;
                }
            }
        }
    }

    DeleteFiles ("./output/*/nuget/*.nuspec");
    foreach (var nuspec in GetFiles ("./nuget/*.nuspec")) {
        var xdoc = XDocument.Load (nuspec.FullPath);
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id").Value;
        if (id.StartsWith ("_"))
            continue;
        var dir = id;
        if (id.Contains(".NativeAssets.")) {
            dir = id.Substring(0, id.IndexOf(".NativeAssets."));
        }

        var preview = "";
        if (!string.IsNullOrEmpty (FEATURE_NAME)) {
            preview += $"-featurepreview-{FEATURE_NAME}";
        } else {
            preview += $"-{PREVIEW_LABEL}";
        }
        if (!string.IsNullOrEmpty (BUILD_NUMBER)) {
            preview += $".{BUILD_NUMBER}";
        }

        RemovePlatforms (xdoc);

        var outDir = $"./output/{dir}/nuget";
        EnsureDirectoryExists (outDir);

        SetVersion (xdoc, "");
        xdoc.Save ($"{outDir}/{id}.nuspec");

        SetVersion (xdoc, $"{preview}");
        xdoc.Save ($"{outDir}/{id}.prerelease.nuspec");

        // the placeholders
        FileWriteText ($"{outDir}/_._", "");

        // the legal
        CopyFile ("./LICENSE.txt", $"{outDir}/LICENSE.txt");
        CopyFile ("./External-Dependency-Info.txt", $"{outDir}/THIRD-PARTY-NOTICES.txt");
    }

    DeleteFiles ($"{OUTPUT_NUGETS_PATH}/*.nupkg");
    foreach (var nuspec in GetFiles ("./output/*/nuget/*.nuspec")) {
        PackageNuGet (nuspec, OUTPUT_NUGETS_PATH);
    }

    // setup validation options
    var options = new Xamarin.Nuget.Validator.NugetValidatorOptions {
        Copyright = "© Microsoft Corporation. All rights reserved.",
        Author = "Microsoft",
        Owner = "Microsoft",
        NeedsProjectUrl = true,
        NeedsLicenseUrl = true,
        ValidateRequireLicenseAcceptance = true,
        ValidPackageNamespace = new [] { "SkiaSharp", "HarfBuzzSharp" },
    };

    var nupkgFiles = GetFiles ($"{OUTPUT_NUGETS_PATH}/*.nupkg");

    Information ("Found ({0}) Nuget's to validate", nupkgFiles.Count ());

    foreach (var nupkgFile in nupkgFiles) {
        Verbose ("Verifiying Metadata of {0}", nupkgFile.GetFilename ());

        var result = Xamarin.Nuget.Validator.NugetValidator.Validate(MakeAbsolute(nupkgFile).FullPath, options);
        if (!result.Success) {
            Information ("Metadata validation failed for: {0} \n\n", nupkgFile.GetFilename ());
            Information (string.Join("\n    ", result.ErrorMessages));
            throw new Exception ($"Invalid Metadata for: {nupkgFile.GetFilename ()}");

        } else {
            Information ("Metadata validation passed for: {0}", nupkgFile.GetFilename ());
        }
    }
});

Task ("nuget-special")
    .Description ("Pack all special NuGets.")
    .IsDependentOn ("nuget-normal")
    .Does (() =>
{
    EnsureDirectoryExists ($"{OUTPUT_SPECIAL_NUGETS_PATH}");
    DeleteFiles ($"{OUTPUT_SPECIAL_NUGETS_PATH}/*.nupkg");

    // get a list of all the version number variants
    var versions = new List<string> ();
    if (!string.IsNullOrEmpty (PREVIEW_LABEL) && PREVIEW_LABEL.StartsWith ("pr.")) {
        var v = $"0.0.0-{PREVIEW_LABEL}";
        if (!string.IsNullOrEmpty (BUILD_NUMBER))
            v += $".{BUILD_NUMBER}";
        versions.Add (v);
    } else {
        if (!string.IsNullOrEmpty (GIT_SHA)) {
            var v = $"0.0.0-commit.{GIT_SHA}";
            if (!string.IsNullOrEmpty (BUILD_NUMBER))
                v += $".{BUILD_NUMBER}";
            versions.Add (v);
        }
        if (!string.IsNullOrEmpty (GIT_BRANCH_NAME)) {
            var v = $"0.0.0-branch.{GIT_BRANCH_NAME.Replace ("/", ".")}";
            if (!string.IsNullOrEmpty (BUILD_NUMBER))
                v += $".{BUILD_NUMBER}";
            versions.Add (v);
        }
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
    }

    foreach (var pair in specials) {
        var id = pair.Key;
        var path = pair.Value;
        var nuspec = $"./output/{path}/{id}.nuspec";

        DeleteFiles ($"./output/{path}/*.nuspec");

        foreach (var packageVersion in versions) {
            // update the version
            var fn = id.StartsWith ("_NativeAssets.") ? "_NativeAssets" : id;
            var xdoc = XDocument.Load ($"./nuget/{fn}.nuspec");
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
                    new XAttribute ("src", $"*/**"),
                    new XAttribute ("target", $"tools/{platform}")));
            }

            xdoc.Save (nuspec);
            PackageNuGet (nuspec, OUTPUT_SPECIAL_NUGETS_PATH, true);
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

    DeleteFiles ("./nuget/*.prerelease.nuspec");

    if (DirectoryExists ("./output"))
        DeleteDirectory ("./output", true);
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
