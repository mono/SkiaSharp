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
// .NET FRAMEWORK TESTS — Windows only, x86 + x64
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
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

            if (!SKIP_BUILD) {
                RunDotNetBuild (csproj, platform: arch, properties: new Dictionary<string, string> {
                    { "TargetFramework", tfm }
                });
            }

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

RunTarget(TARGET);
