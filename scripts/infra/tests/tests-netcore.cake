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
    var testAssemblies = new List<string> {
        "SkiaSharp.Tests.Console",
        "SkiaSharp.Vulkan.Tests.Console",
        "SkiaSharp.Direct3D.Tests.Console",
    };
    foreach (var testAssembly in testAssemblies) {
        var csproj = $"{ROOT_PATH}/tests/{testAssembly}/{testAssembly}.csproj";

        if (!SKIP_BUILD) {
            RunDotNetBuild (csproj, properties: new Dictionary<string, string> {
                { "TargetFramework", tfm }
            });
        }

        var results = $"{ROOT_PATH}/output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}/{tfm}";
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
        RunCodeCoverage ($"{ROOT_PATH}/output/logs/testlogs/**/Coverage/**/*.xml", $"{ROOT_PATH}/output/coverage");
    }
});

RunTarget(TARGET);
