#addin nuget:?package=Cake.FileHelpers&version=4.0.1

#tool nuget:?package=xunit.runner.console&version=2.4.2

DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../native/windows/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// TEST UTILITIES
////////////////////////////////////////////////////////////////////////////////////////////////////

void RunTests(FilePath testAssembly, DirectoryPath output, bool is32)
{
    var dir = testAssembly.GetDirectory();
    var settings = new XUnit2Settings {
        ReportName = "TestResults",
        XmlReport = true,
        UseX86 = is32,
        NoAppDomain = true,
        Parallelism = ParallelismOption.All,
        OutputDirectory = MakeAbsolute(output).FullPath,
        WorkingDirectory = dir,
        ArgumentCustomization = args => args.Append("-verbose"),
    };
    XUnit2(new [] { testAssembly }, settings);
}

void RunDotNetTest(
    FilePath testProject,
    DirectoryPath output,
    string configuration = null,
    Dictionary<string, string> properties = null)
{
    output = MakeAbsolute(output);
    var dir = testProject.GetDirectory();
    var settings = new DotNetTestSettings {
        Configuration = configuration ?? CONFIGURATION,
        NoBuild = true,
        Loggers = new [] { "xunit" },
        WorkingDirectory = dir,
        ResultsDirectory = output,
        Verbosity = DotNetVerbosity.Normal,
        ArgumentCustomization = args => {
            args = args
                .Append("/p:Platform=\"AnyCPU\"");
            if (COVERAGE)
                args = args
                    .Append("/p:CollectCoverage=true")
                    .Append("/p:CoverletOutputFormat=cobertura")
                    .Append($"/p:CoverletOutput={output.Combine("Coverage").FullPath}/");
            if (properties != null) {
                foreach (var prop in properties) {
                    if (!string.IsNullOrEmpty(prop.Value)) {
                        args = args
                            .Append($"/p:{prop.Key}={prop.Value}");
                    }
                }
            }
            return args;
        },
    };
    DotNetTest(MakeAbsolute(testProject).FullPath, settings);
}

void RunCodeCoverage(string testResultsGlob, DirectoryPath output)
{
    try {
        DotNetTool(
            $"reportgenerator" +
            $"  -reports:{testResultsGlob}" +
            $"  -targetdir:{output}" +
            $"  -reporttypes:HtmlInline_AzurePipelines;Cobertura" +
            $"  -assemblyfilters:-*.Tests");
    } catch (Exception ex) {
        Error("Make sure to install the 'dotnet-reportgenerator-globaltool' .NET Core global tool.");
        Error(ex);
        throw;
    }
    var xml = $"{output}/Cobertura.xml";
    var root = FindRegexMatchGroupsInFile(xml, @"<source>(.*)<\/source>", 0)[1].Value;
    ReplaceTextInFiles(xml, root, "");
}

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
    .Does (() =>
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    var failedTests = 0;

    foreach ( var arch in new [] { "x86", "x64" }) {
        if (BUILD_ARCH.Length > 0 && !BUILD_ARCH.Contains("all") && !BUILD_ARCH.Contains(arch)) {
            Warning($"Skipping architecture: {arch}");
            continue;
        }

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

Task ("Default")
    .IsDependentOn ("tests");

RunTarget(TARGET);
