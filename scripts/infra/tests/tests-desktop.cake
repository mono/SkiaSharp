DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../native/windows/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// DESKTOP TESTS — .NET Framework and .NET Core
////////////////////////////////////////////////////////////////////////////////////////////////////

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

Task ("Default")
    .IsDependentOn ("tests-netfx")
    .IsDependentOn ("tests-netcore");

RunTarget(TARGET);
