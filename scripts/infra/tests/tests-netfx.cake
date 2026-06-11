DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

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
        var testAssemblies = new List<string> {
            "SkiaSharp.Tests.Console",
            "SkiaSharp.Tests.SingletonInit.Console",
            "SkiaSharp.Vulkan.Tests.Console",
            "SkiaSharp.Direct3D.Tests.Console",
        };
        foreach (var testAssembly in testAssemblies) {
            var csproj = $"{ROOT_PATH}/tests/{testAssembly}/{testAssembly}.csproj";

            if (!SKIP_BUILD) {
                RunDotNetBuild (csproj, platform: arch, properties: new Dictionary<string, string> {
                    { "TargetFramework", tfm }
                });
            }

            DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/{testAssembly}/{DATE_TIME_STR}/{tfm}-{arch}";
            var assName = testAssembly.Replace (".Console", "");
            EnsureDirectoryExists (results);
            try {
                RunTests ($"{ROOT_PATH}/tests/{testAssembly}/bin/{arch}/{CONFIGURATION}/{tfm}/{assName}.exe", results, arch == "x86");
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
