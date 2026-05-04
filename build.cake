#addin nuget:?package=Cake.Xamarin&version=3.1.0
#addin nuget:?package=Cake.XCode&version=5.0.0
#addin nuget:?package=Cake.FileHelpers&version=4.0.1
#addin nuget:?package=Cake.Json&version=6.0.1
#addin nuget:?package=NuGet.Packaging&version=6.9.1
#addin nuget:?package=SharpCompress&version=0.32.2
#addin nuget:?package=Mono.Cecil&version=0.11.5
#addin nuget:?package=Mono.ApiTools.ApiInfo&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiff&version=1.4.1
#addin nuget:?package=Mono.ApiTools.ApiDiffFormatted&version=1.4.1
#addin nuget:?package=Mono.ApiTools.NuGetDiff&version=1.4.1

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

#load "./scripts/infra/shared/shared.cake"
#load "./scripts/infra/native/shared/native-shared.cake"

var MDocPath = Context.Tools.Resolve ("mdoc.exe");

if (string.IsNullOrEmpty (CURRENT_PLATFORM)) {
    throw new Exception ("This script is not running on a known platform.");
}

#load "./scripts/infra/native/windows/msbuild.cake"
#load "./scripts/infra/managed/cake/UtilsManaged.cake"
#load "./scripts/infra/managed/cake/externals.cake"
#load "./scripts/infra/managed/cake/UpdateDocs.cake"


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

        var tfm = "net48";
        var testAssemblies = new List<string> { "SkiaSharp.Tests.Console" };
        if (SUPPORT_VULKAN)
            testAssemblies.Add ("SkiaSharp.Vulkan.Tests.Console");
        if (SUPPORT_DIRECT3D)
            testAssemblies.Add ("SkiaSharp.Direct3D.Tests.Console");
        foreach (var testAssembly in testAssemblies) {
            var csproj = $"./tests/{testAssembly}/{testAssembly}.csproj";

            // build
            if (!SKIP_BUILD) {
                RunDotNetBuild (csproj, platform: arch, properties: new Dictionary<string, string> {
                    { "TargetFramework", tfm }
                });
            }

            // test
            DirectoryPath results = $"./output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}/{tfm}-{arch}";
            var assName = testAssembly.Replace (".Console", "");
            EnsureDirectoryExists (results);
            try {
                RunTests ($"./tests/{testAssembly}/bin/{arch}/{CONFIGURATION}/{tfm}/{assName}.dll", results, arch == "x86");
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

    var tfm = "net10.0";
    var testAssemblies = new List<string> { "SkiaSharp.Tests.Console" };
    if (SUPPORT_VULKAN)
        testAssemblies.Add ("SkiaSharp.Vulkan.Tests.Console");
    if (SUPPORT_DIRECT3D)
        testAssemblies.Add ("SkiaSharp.Direct3D.Tests.Console");
    foreach (var testAssembly in testAssemblies) {
        var csproj = $"./tests/{testAssembly}/{testAssembly}.csproj";

        // build
        if (!SKIP_BUILD) {
            RunDotNetBuild (csproj, properties: new Dictionary<string, string> {
                { "TargetFramework", tfm }
            });
        }

        // test
        var results = $"./output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}/{tfm}";
        try {
            RunDotNetTest (csproj, results, properties: new Dictionary<string, string> {
                { "TargetFramework", tfm }
            });
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
    var tfm = "net10.0-android36.0";
    var rid = "android-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    FilePath app = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}/com.companyname.SkiaSharpTests-Signed.apk";

    Information ("=== Android Test Build Configuration ===");
    Information ("  Project:       {0}", csproj);
    Information ("  Configuration: {0}", configuration);
    Information ("  TFM:           {0}", tfm);
    Information ("  RID:           {0}", rid);
    Information ("  App Path:      {0}", app);
    Information ("  OS:            {0}", RuntimeInformation.OSDescription);
    Information ("  Arch:          {0}", RuntimeInformation.ProcessArchitecture);
    Information ("========================================");

    // build the app
    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj,
            configuration: configuration,
            properties: new Dictionary<string, string> {
                { "TargetFramework", tfm },
                { "RuntimeIdentifier", rid },
            });
    }

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.Android/{DATE_TIME_STR}";
    RunCake ("./scripts/infra/tests/xharness-android.cake", "Default", new Dictionary<string, string> {
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
    var tfm = "net10.0-ios";
    var rid = "iossimulator-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    var outputDir = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}";

    // package the app
    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj,
            configuration: configuration,
            properties: new Dictionary<string, string> {
                { "TargetFramework", tfm },
                { "RuntimeIdentifier", rid },
            });
    }

    // find the .app bundle (name may differ from AssemblyName in .NET 10)
    var appBundles = GetDirectories ($"{outputDir}/*.app");
    if (!appBundles.Any ())
        throw new Exception ($"No .app bundle found in {outputDir}");
    var app = appBundles.First ();
    Information ("Found app bundle: {0}", app);

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.iOS/{DATE_TIME_STR}";
    RunCake ("./scripts/infra/tests/xharness-apple.cake", "Default", new Dictionary<string, string> {
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
    var tfm = "net10.0-maccatalyst";
    var rid = "maccatalyst-" + RuntimeInformation.ProcessArchitecture.ToString ().ToLower ();
    var outputDir = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}";

    // package the app
    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj,
            configuration: configuration,
            properties: new Dictionary<string, string> {
                { "TargetFramework", tfm },
                { "RuntimeIdentifier", rid },
            });
    }

    // find the .app bundle (name may differ from AssemblyName in .NET 10)
    var appBundles = GetDirectories ($"{outputDir}/*.app");
    if (!appBundles.Any ())
        throw new Exception ($"No .app bundle found in {outputDir}");
    var app = appBundles.First ();
    Information ("Found app bundle: {0}", app);

    // run the tests
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.MacCatalyst/{DATE_TIME_STR}";
    RunCake ("./scripts/infra/tests/xharness-apple.cake", "Default", new Dictionary<string, string> {
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
    if (!SKIP_BUILD) {
        RunDotNetBuild ("./tests/SkiaSharp.Tests.Wasm.sln");
    }

    IProcess serverProc = null;
    try {
        var wasmProj = MakeAbsolute (File ("./tests/SkiaSharp.Tests.Wasm/SkiaSharp.Tests.Wasm.csproj")).FullPath;
        serverProc = RunAndReturnProcess ("dotnet", $"run --project {wasmProj} --no-build -c {CONFIGURATION}");
        DotNetRun ("./utils/WasmTestRunner/WasmTestRunner.csproj",
            $"--output=\"./output/logs/testlogs/SkiaSharp.Tests.Wasm/{DATE_TIME_STR}/\" " +
            (string.IsNullOrEmpty (CHROMEWEBDRIVER) ? "" : $"--driver=\"{CHROMEWEBDRIVER}\" ") +
            "--verbose " +
            "\"http://127.0.0.1:8000/\" ");
    } finally {
        serverProc?.Kill ();
    }
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

    // _NativeAssets handling (per-platform raw native binaries)
    var nativePlatforms = GetDirectories ("./output/native/*")
        .Select (d => d.GetDirectoryName ())
        .ToArray ();
    if (nativePlatforms.Length > 0) {
        var nativeSpecials = new Dictionary<string, string> ();
        nativeSpecials["_NativeAssets"] = "native";
        foreach (var platform in nativePlatforms) {
            nativeSpecials[$"_NativeAssets.{platform}"] = $"native/{platform}";
        }

        Information ("Detected {0} native asset artifacts to process:", nativeSpecials.Count);
        max = 0;
        foreach (var special in nativeSpecials) {
            if (special.Key.Length > max)
                max = special.Key.Length + 1;
        }
        foreach (var special in nativeSpecials) {
            Information ("  - {0}" + " ".PadRight(max - special.Key.Length) + "=> {1}", special.Key, special.Value);
        }

        foreach (var pair in nativeSpecials) {
            var id = pair.Key;
            var path = pair.Value;
            var nuspec = $"./output/{path}/{id}.nuspec";

            DeleteFiles ($"./output/{path}/*.nuspec");

            foreach (var version in versions) {
                var packageVersion = version.Value;

                var xdoc = XDocument.Load ("./scripts/infra/package/nuget/_NativeAssets.nuspec");
                var metadata = xdoc.Root.Element ("metadata");
                metadata.Element ("version").Value = packageVersion;
                metadata.Element ("id").Value = id;

                if (id == "_NativeAssets") {
                    var dependencies = metadata.Element ("dependencies");
                    foreach (var platform in nativePlatforms) {
                        dependencies.Add (new XElement ("dependency",
                            new XAttribute ("id", $"_NativeAssets.{platform}"),
                            new XAttribute ("version", packageVersion)));
                    }
                } else {
                    var platform = id.Substring (id.IndexOf (".") + 1);
                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", "**"),
                        new XAttribute ("target", $"tools/{platform}")));
                }
                {
                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));
                }

                xdoc.Save (nuspec);
                RunDotNetPack (
                    "./scripts/infra/package/nuget/NuGet.csproj",
                    OUTPUT_SPECIAL_NUGETS_PATH,
                    bl: $".{id}.{version.Key}",
                    additionalArgs: "/restore /nologo",
                    properties: new Dictionary<string, string> {
                        { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                    });
            }

            DeleteFiles ($"./output/{path}/*.nuspec");
        }
    }

    // NuGets and Symbols: bin-pack all nupkgs into ~200 MB numbered chunks
    if (GetFiles ("./output/nugets/*.nupkg").Count > 0) {
        const long MAX_CHUNK_SIZE = 200L * 1024 * 1024;

        var metaPackages = new[] {
            new { Id = "_NuGets",         SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = false },
            new { Id = "_NuGetsPreview",  SourceDir = "nugets",         IncludeSnupkg = false, IsPreview = true },
            new { Id = "_Symbols",        SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = false },
            new { Id = "_SymbolsPreview", SourceDir = "nugets-symbols", IncludeSnupkg = true,  IsPreview = true },
        };

        foreach (var meta in metaPackages) {
            // enumerate matching files
            var allFiles = GetFiles ($"./output/{meta.SourceDir}/*.nupkg").ToList ();
            if (meta.IncludeSnupkg)
                allFiles.AddRange (GetFiles ($"./output/{meta.SourceDir}/*.snupkg"));

            var matchingFiles = allFiles
                .Where (f => {
                    var name = f.GetFilename ().ToString ();
                    if (name.StartsWith ("_")) return false;
                    return meta.IsPreview ? name.Contains ("-") : !name.Contains ("-");
                })
                .Select (f => new { Path = f, Size = new FileInfo (f.FullPath).Length })
                .OrderByDescending (f => f.Size)
                .ToList ();

            if (matchingFiles.Count == 0)
                continue;

            // bin-pack using first-fit decreasing
            var chunks = new List<List<FilePath>> ();
            var chunkSizes = new List<long> ();

            foreach (var file in matchingFiles) {
                var placed = false;
                for (int i = 0; i < chunks.Count; i++) {
                    if (chunkSizes[i] + file.Size <= MAX_CHUNK_SIZE) {
                        chunks[i].Add (file.Path);
                        chunkSizes[i] += file.Size;
                        placed = true;
                        break;
                    }
                }
                if (!placed) {
                    chunks.Add (new List<FilePath> { file.Path });
                    chunkSizes.Add (file.Size);
                }
            }

            Information ("{0}: {1} files -> {2} chunk(s)", meta.Id, matchingFiles.Count, chunks.Count);
            for (int i = 0; i < chunks.Count; i++) {
                Information ("  Chunk {0}: {1} files, {2:F1} MB",
                    i + 1, chunks[i].Count, chunkSizes[i] / 1024.0 / 1024.0);
            }

            foreach (var version in versions) {
                var packageVersion = version.Value;

                // pack each chunk as a numbered dependency
                for (int i = 0; i < chunks.Count; i++) {
                    var chunkId = $"{meta.Id}.Dependencies.{i + 1}";
                    var nuspec = $"./output/{meta.SourceDir}/{chunkId}.nuspec";

                    DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load ("./scripts/infra/package/nuget/_Dependencies.nuspec");
                    var xmeta = xdoc.Root.Element ("metadata");
                    xmeta.Element ("id").Value = chunkId;
                    xmeta.Element ("version").Value = packageVersion;
                    xmeta.Element ("title").Value = $"{meta.Id.TrimStart ('_')} (Part {i + 1})";
                    xmeta.Element ("description").Value =
                        $"Part {i + 1} of {chunks.Count} of the {meta.Id.TrimStart ('_')} packages.";
                    xmeta.Element ("summary").Value = xmeta.Element ("description").Value;

                    var files = xdoc.Root.Element ("files");
                    foreach (var file in chunks[i]) {
                        files.Add (new XElement ("file",
                            new XAttribute ("src", MakeAbsolute (file).FullPath),
                            new XAttribute ("target", "tools/")));
                    }
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        "./scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{chunkId}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                // pack the parent meta-package with dependencies on all chunks
                {
                    var nuspec = $"./output/{meta.SourceDir}/{meta.Id}.nuspec";

                    DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");

                    var xdoc = XDocument.Load ($"./scripts/infra/package/nuget/{meta.Id}.nuspec");
                    var xmeta = xdoc.Root.Element ("metadata");
                    xmeta.Element ("version").Value = packageVersion;

                    var dependencies = xmeta.Element ("dependencies");
                    for (int i = 0; i < chunks.Count; i++) {
                        dependencies.Add (new XElement ("dependency",
                            new XAttribute ("id", $"{meta.Id}.Dependencies.{i + 1}"),
                            new XAttribute ("version", packageVersion)));
                    }

                    var files = xdoc.Root.Element ("files");
                    files.Add (new XElement ("file",
                        new XAttribute ("src", MakeAbsolute (File ("./scripts/infra/package/nuget/README.md")).FullPath),
                        new XAttribute ("target", "README.md")));

                    xdoc.Save (nuspec);
                    RunDotNetPack (
                        "./scripts/infra/package/nuget/NuGet.csproj",
                        OUTPUT_SPECIAL_NUGETS_PATH,
                        bl: $".{meta.Id}.{version.Key}",
                        additionalArgs: "/restore /nologo",
                        properties: new Dictionary<string, string> {
                            { "NuspecFile", MakeAbsolute (File (nuspec)).FullPath },
                        });
                }

                DeleteFiles ($"./output/{meta.SourceDir}/*.nuspec");
            }
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

    DeleteDir ("./output");
});

////////////////////////////////////////////////////////////////////////////////////////////////////
// SAMPLES - build sample projects (isolated via RunCake)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("samples")
    .Description ("Build all sample projects.")
    .IsDependentOn ("libs")
    .Does (() => RunCake ("./scripts/infra/samples/samples.cake", "Default"));

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
