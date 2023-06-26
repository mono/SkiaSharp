DirectoryPath ROOT_PATH = MakeAbsolute(Directory(".."));

#load "shared.cake"

// required
FilePath PROJECT = Argument("project", EnvironmentVariable("IOS_TEST_PROJECT") ?? "");
string TEST_DEVICE = Argument("device", EnvironmentVariable("IOS_TEST_DEVICE") ?? "ios-simulator-64_14.4"); // comma separated in the form <platform>-<device|simulator>[-<32|64>][_<version>] (eg: ios-simulator-64_13.4,[...])

// optional
var TEST_APP = Argument("app", EnvironmentVariable("IOS_TEST_APP") ?? "");
var TEST_RESULTS = Argument("results", EnvironmentVariable("IOS_TEST_RESULTS") ?? "");

// other
string PLATFORM = TEST_DEVICE.ToLower().Contains("simulator") ? "iPhoneSimulator" : "iPhone";

Information("Project File: {0}", PROJECT);

Task("Default")
    .Does(() =>
{
    if (string.IsNullOrEmpty(TEST_APP)) {
        if (string.IsNullOrEmpty(PROJECT.FullPath))
            throw new Exception("If no app was specified, an app must be provided.");
        var binDir = PROJECT.GetDirectory().Combine("bin").Combine(PLATFORM).Combine(CONFIGURATION).FullPath;
        var apps = GetDirectories(binDir + "/*.app");
        TEST_APP = apps.First().FullPath;
    }
    if (string.IsNullOrEmpty(TEST_RESULTS)) {
        TEST_RESULTS = TEST_APP + "-results";
    }

    Information("Test App: {0}", TEST_APP);
    Information("Test Device: {0}", TEST_DEVICE);
    Information("Test App: {0}", TEST_APP);
    Information("Test Results Directory: {0}", TEST_RESULTS);

    CleanDirectories(TEST_RESULTS);

    try {
        DotNetTool("xharness apple test " +
            $"--app=\"{TEST_APP}\" " +
            $"--targets=\"{TEST_DEVICE}\" " +
            $"--output-directory=\"{TEST_RESULTS}\" " +
            $"--verbosity=\"Debug\" ");
    } finally {
        // ios test result files are weirdly named, so fix it up
        var resultsFile = GetFiles($"{TEST_RESULTS}/xunit-test-*.xml").FirstOrDefault();
        if (FileExists(resultsFile)) {
            CopyFile(resultsFile, resultsFile.GetDirectory().CombineWithFilePath("TestResults.xml"));
        }
    }

    // this _may_ not be needed, but just in case
    var failed = XmlPeek($"{TEST_RESULTS}/TestResults.xml", "/assemblies/assembly[@failed > 0 or @errors > 0]/@failed");
    if (!string.IsNullOrEmpty(failed)) {
        throw new Exception($"At least {failed} test(s) failed.");
    }
});

RunTarget(TARGET);
