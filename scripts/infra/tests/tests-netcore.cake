DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// .NET CORE TESTS — cross-platform
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
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
    var testProjects = new List<FilePath> {
        $"{ROOT_PATH}/tests/SkiaSharp.Tests.Console.slnx",
    };
    if (IsRunningOnLinux ())
        testProjects.Add ($"{ROOT_PATH}/tests/SkiaSharp.Views.Gtk4.Tests/SkiaSharp.Views.Gtk4.Tests.csproj");

    var properties = new Dictionary<string, string> {
        { "TargetFramework", tfm },
        { "TargetFrameworks", tfm },
    };
    foreach (var testProject in testProjects) {

        if (!SKIP_BUILD) {
            RunDotNetBuild (testProject, properties: properties);
        }

        var results = $"{ROOT_PATH}/output/logs/testlogs/{testProject.GetFilenameWithoutExtension ()}/{DATE_TIME_STR}/{tfm}";
        try {
            RunDotNetTest (testProject, results, properties: properties);
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
        RunCodeCoverage ($"{ROOT_PATH}/output/logs/testlogs/**/Coverage/**/*.xml", $"{ROOT_PATH}/output/coverage");
    }
});

RunTarget(TARGET);
