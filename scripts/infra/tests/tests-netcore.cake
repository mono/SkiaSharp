DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

var PLATFORM_SUPPORTS_VULKAN_TESTS = (IsRunningOnWindows () || IsRunningOnLinux ()).ToString ();
var SUPPORT_VULKAN_VAR = Argument ("supportVulkan", EnvironmentVariable ("SUPPORT_VULKAN") ?? PLATFORM_SUPPORTS_VULKAN_TESTS);
var SUPPORT_VULKAN = SUPPORT_VULKAN_VAR == "1" || SUPPORT_VULKAN_VAR.ToLower () == "true";

var PLATFORM_SUPPORTS_DIRECT3D_TESTS = IsRunningOnWindows ().ToString ();
var SUPPORT_DIRECT3D_VAR = Argument ("supportDirect3D", EnvironmentVariable ("SUPPORT_DIRECT3D") ?? PLATFORM_SUPPORTS_DIRECT3D_TESTS);
var SUPPORT_DIRECT3D = SUPPORT_DIRECT3D_VAR == "1" || SUPPORT_DIRECT3D_VAR.ToLower () == "true";

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
    var testAssemblies = new List<string> { "SkiaSharp.Tests.Console" };
    if (SUPPORT_VULKAN)
        testAssemblies.Add ("SkiaSharp.Vulkan.Tests.Console");
    if (SUPPORT_DIRECT3D)
        testAssemblies.Add ("SkiaSharp.Direct3D.Tests.Console");
    foreach (var testAssembly in testAssemblies) {
        var csproj = $"./tests/{testAssembly}/{testAssembly}.csproj";

        if (!SKIP_BUILD) {
            RunDotNetBuild (csproj, properties: new Dictionary<string, string> {
                { "TargetFramework", tfm }
            });
        }

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

RunTarget(TARGET);
