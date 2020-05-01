#addin nuget:?package=Cake.Xamarin&version=3.0.2
#addin nuget:?package=Cake.XCode&version=4.2.0
#addin nuget:?package=Cake.FileHelpers&version=3.2.1
#addin nuget:?package=Cake.Json&version=4.0.0
#addin nuget:?package=SharpCompress&version=0.24.0
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.3.0&loaddependencies=true
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
var PACK_ALL_PLATFORMS = Argument ("packall", Argument ("PackAllPlatforms", false));
var BUILD_ALL_PLATFORMS = Argument ("buildall", Argument ("BuildAllPlatforms", false));
var PRINT_ALL_ENV_VARS = Argument ("printAllEnvVars", false);
var AZURE_BUILD_ID = Argument ("azureBuildId", "");
var UNSUPPORTED_TESTS = Argument ("unsupportedTests", "");

var NuGetToolPath = Context.Tools.Resolve ("nuget.exe");
var CakeToolPath = Context.Tools.Resolve ("Cake.exe");
var MDocPath = Context.Tools.Resolve ("mdoc.exe");

DirectoryPath DOCS_PATH = MakeAbsolute(ROOT_PATH.Combine("docs/SkiaSharpAPI"));

var PREVIEW_LABEL = EnvironmentVariable ("PREVIEW_LABEL") ?? "preview";
var FEATURE_NAME = EnvironmentVariable ("FEATURE_NAME") ?? "";
var BUILD_NUMBER = EnvironmentVariable ("BUILD_NUMBER") ?? "0";

var AZURE_BUILD_SUCCESS = "https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds?statusFilter=completed&resultFilter=succeeded&definitions=4&branchName=refs/heads/master&$top=1&api-version=5.1";
var AZURE_BUILD_URL = "https://dev.azure.com/xamarin/6fd3d886-57a5-4e31-8db7-52a1b47c07a8/_apis/build/builds/{0}/artifacts?artifactName={1}&%24format=zip&api-version=5.1";

var TRACKED_NUGETS = new Dictionary<string, Version> {
    { "SkiaSharp",                                     new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.Linux",                  new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.Linux.NoDependencies",   new Version (1, 57, 0) },
    { "SkiaSharp.NativeAssets.NanoServer",             new Version (1, 57, 0) },
    { "SkiaSharp.Views",                               new Version (1, 57, 0) },
    { "SkiaSharp.Views.Desktop.Common",                new Version (1, 57, 0) },
    { "SkiaSharp.Views.Gtk2",                          new Version (1, 57, 0) },
    { "SkiaSharp.Views.Gtk3",                          new Version (1, 57, 0) },
    { "SkiaSharp.Views.WindowsForms",                  new Version (1, 57, 0) },
    { "SkiaSharp.Views.WPF",                           new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms",                         new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms.WPF",                     new Version (1, 57, 0) },
    { "SkiaSharp.Views.Forms.GTK",                     new Version (1, 57, 0) },
    { "HarfBuzzSharp",                                 new Version (1, 0, 0) },
    { "HarfBuzzSharp.NativeAssets.Linux",              new Version (1, 0, 0) },
    { "SkiaSharp.HarfBuzz",                            new Version (1, 57, 0) },
};

#load "cake/msbuild.cake"
#load "cake/UtilsManaged.cake"
#load "cake/externals.cake"
#load "cake/UpdateDocs.cake"
#load "cake/samples.cake"

Task ("determine-last-successful-build")
    .WithCriteria (string.IsNullOrEmpty (AZURE_BUILD_ID))
    .Does (() =>
{
    Warning ("A build ID (--azureBuildId=<ID>) was not specified, using the last successful build.");

    var successUrl = string.Format(AZURE_BUILD_SUCCESS);
    var json = ParseJson (FileReadText (DownloadFile (successUrl)));

    AZURE_BUILD_ID = (string)json ["value"] [0] ["id"];

    Information ($"Using last successful build ID {AZURE_BUILD_ID}");
});

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
    RunMSBuild ($"./source/SkiaSharpSource{platform}.sln",
        bl: $"./output/binlogs/libs{platform}.binlog");

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
    .IsDependentOn ("externals")
    .Does (() =>
{
    var failedTests = 0;

    void RunDesktopTest (string arch)
    {
        RunMSBuild ("./tests/SkiaSharp.Desktop.Tests/SkiaSharp.Desktop.Tests.sln",
            platform: arch == "AnyCPU" ? "Any CPU" : arch,
            bl: $"./output/binlogs/tests-desktop.{arch}.binlog");
        try {
            RunTests ($"./tests/SkiaSharp.Desktop.Tests/bin/{arch}/{CONFIGURATION}/SkiaSharp.Tests.dll", arch == "x86");
        } catch {
            failedTests++;
        }
    }

    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    // Full .NET Framework
    if (IsRunningOnWindows ()) {
        RunDesktopTest ("x86");
        RunDesktopTest ("x64");
    } else if (IsRunningOnMac ()) {
        RunDesktopTest ("AnyCPU");
    } else if (IsRunningOnLinux ()) {
        RunDesktopTest ("x64");
    }

    // .NET Core
    RunMSBuild ("./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.sln",
        bl: $"./output/binlogs/tests-netcore.binlog");
    try {
        RunNetCoreTests ("./tests/SkiaSharp.NetCore.Tests/SkiaSharp.NetCore.Tests.csproj");
    } catch {
        failedTests++;
    }

    if (failedTests > 0)
        throw new Exception ($"There were {failedTests} failed tests.");
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
                version.Value = v;
            }
            version.Value += suffix;
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
    }

    DeleteFiles ("./output/*/nuget/*.nuspec");
    foreach (var nuspec in GetFiles ("./nuget/*.nuspec")) {
        var xdoc = XDocument.Load (nuspec.FullPath);
        var metadata = xdoc.Root.Element ("metadata");
        var id = metadata.Element ("id").Value;
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
