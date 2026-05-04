DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// APPLE TESTS — iOS, Mac Catalyst (build + xharness execution)
////////////////////////////////////////////////////////////////////////////////////////////////////

void RunAppleTests(string platform, string tfm, string configuration = "Debug")
{
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/skiasharp*");
    CleanDirectories ($"{PACKAGE_CACHE_PATH}/harfbuzzsharp*");

    FilePath csproj = "./tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    var rid = $"{platform.Replace("ios", "iossimulator")}-{RuntimeInformation.ProcessArchitecture.ToString().ToLower()}";
    var outputDir = $"./tests/SkiaSharp.Tests.Devices/bin/{configuration}/{tfm}/{rid}";

    // build the app
    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj,
            configuration: configuration,
            properties: new Dictionary<string, string> {
                { "TargetFramework", tfm },
                { "RuntimeIdentifier", rid },
            });
    }

    // find the .app bundle
    var appBundles = GetDirectories ($"{outputDir}/*.app");
    if (!appBundles.Any ())
        throw new Exception ($"No .app bundle found in {outputDir}");
    var app = appBundles.First ();
    Information ("Found app bundle: {0}", app);

    // determine xharness device target
    var device = platform == "maccatalyst"
        ? "maccatalyst"
        : $"{platform}-simulator-64";

    // run xharness
    DirectoryPath results = $"./output/logs/testlogs/SkiaSharp.Tests.Devices.{platform}/{DATE_TIME_STR}";
    CleanDirectories(results.FullPath);

    try {
        DotNetTool("xharness apple test " +
            $"--app=\"{app}\" " +
            $"--targets=\"{device}\" " +
            $"--output-directory=\"{results}\" " +
            $"--verbosity=\"Debug\" ");
    } finally {
        // ios test result files are weirdly named, so fix it up
        var resultsFile = GetFiles($"{results}/xunit-test-*.xml").FirstOrDefault();
        if (FileExists(resultsFile)) {
            CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
        }
    }

    var failed = XmlPeek($"{results}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
    if (!string.IsNullOrEmpty(failed)) {
        throw new Exception($"At least {failed} test(s) failed.");
    }
}

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() => RunAppleTests("ios", "net10.0-ios"));

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .Does (() => RunAppleTests("maccatalyst", "net10.0-maccatalyst"));

Task ("Default")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

RunTarget(TARGET);
