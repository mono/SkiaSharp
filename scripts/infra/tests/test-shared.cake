#addin nuget:?package=Cake.FileHelpers&version=4.0.1

////////////////////////////////////////////////////////////////////////////////////////////////////
// DEVICE RUNNERS — shared helper for DeviceRunners.Testing.Targets based tests
////////////////////////////////////////////////////////////////////////////////////////////////////

void RunDeviceRunnersTest(
    FilePath testProject,
    DirectoryPath output,
    string configuration = null,
    string framework = null,
    bool noBuild = false,
    Dictionary<string, string> properties = null)
{
    CleanDirectories($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");
    EnsureDirectoryExists(OUTPUT_NUGETS_PATH);

    output = MakeAbsolute(output);
    CleanDirectories(output.FullPath);

    var msb = new DotNetMSBuildSettings();
    msb.Properties ["RestoreNoCache"] = new [] { "true" };
    msb.Properties ["RestorePackagesPath"] = new [] { PACKAGE_CACHE_PATH.FullPath };

    if (properties != null) {
        foreach (var prop in properties) {
            if (!string.IsNullOrEmpty(prop.Value)) {
                msb.Properties [prop.Key] = new [] { prop.Value };
            }
        }
    }

    var settings = new DotNetTestSettings {
        Configuration = configuration ?? CONFIGURATION,
        Framework = framework,
        MSBuildSettings = msb,
        NoBuild = noBuild,
        ResultsDirectory = output,
        Verbosity = DotNetVerbosity.Normal,
        ArgumentCustomization = args => {
            args = AppendForwardingLogger(args);
            var sep = IsRunningOnWindows() ? ";" : "%3B";
            return args
                .Append($"/p:RestoreSources=\"{string.Join(sep, GetNuGetSources())}\"")
                .Append("--logger").Append("trx");
        },
    };

    DotNetTest(MakeAbsolute(testProject).FullPath, settings);
}

////////////////////////////////////////////////////////////////////////////////////////////////////
// TEST UTILITIES — shared by desktop test cakes
////////////////////////////////////////////////////////////////////////////////////////////////////

// Runs a Microsoft.Testing.Platform test executable directly (used for .NET Framework, where
// the v3 test project builds a runnable exe). MTP report + hang-dump args are passed natively.
//
// allowNoTests: Microsoft.Testing.Platform returns exit code 8 ("Zero tests ran") both when no
// tests are discovered AND when every discovered test is dynamically skipped (it does not count
// skipped tests as having run, and the two cases are indistinguishable by exit code). Leave this
// false for suites that must always run something — there an exit 8 is a real misconfiguration and
// should fail the leg. Set it true only for hardware-gated suites (e.g. Vulkan/Direct3D) where an
// agent legitimately skips every test, matching the previous xUnit v2 all-skipped-is-success behavior.
void RunTests(FilePath testApp, DirectoryPath output, bool allowNoTests = false)
{
    var dir = testApp.GetDirectory();
    output = MakeAbsolute(output);
    EnsureDirectoryExists(output);

    var args = new ProcessArgumentBuilder()
        .Append("--results-directory").AppendQuoted(output.FullPath)
        .Append("--report-trx")
        .Append("--report-trx-filename").Append("TestResults.trx")
        .Append("--hangdump")
        .Append("--hangdump-timeout").Append("15m")
        .Append("--hangdump-type").Append("Mini");
    if (allowNoTests)
        args.Append("--ignore-exit-code").Append("8");

    var exitCode = StartProcess(testApp, new ProcessSettings {
        WorkingDirectory = dir,
        Arguments = args,
    });

    if (exitCode != 0)
        throw new Exception($"Tests failed: {testApp.GetFilename()} returned exit code {exitCode}.");
}

void RunDotNetTest(
    FilePath testProject,
    DirectoryPath output,
    string configuration = null,
    Dictionary<string, string> properties = null,
    bool allowNoTests = false)
{
    output = MakeAbsolute(output);
    var dir = testProject.GetDirectory();
    var settings = new DotNetTestSettings {
        Configuration = configuration ?? CONFIGURATION,
        NoBuild = true,
        WorkingDirectory = dir,
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
            // Everything after "--" is forwarded to the Microsoft.Testing.Platform runner.
            args = args
                .Append("--")
                .Append("--results-directory").AppendQuoted(output.FullPath)
                .Append("--report-trx")
                .Append("--report-trx-filename").Append("TestResults.trx")
                .Append("--hangdump")
                .Append("--hangdump-timeout").Append("15m")
                .Append("--hangdump-type").Append("Mini");
            // See RunTests: exit code 8 covers both "no tests discovered" and "all tests skipped".
            // Only suppress it for hardware-gated suites that may legitimately skip everything.
            if (allowNoTests)
                args = args
                    .Append("--ignore-exit-code").Append("8");
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
