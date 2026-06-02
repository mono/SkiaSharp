DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// WASM TESTS — build and run browser-based tests via dotnet test
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Wasm/SkiaSharp.Tests.Wasm.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Wasm/{DATE_TIME_STR}";

    RunDeviceRunnersTest(csproj, results, noBuild: SKIP_BUILD);
});

RunTarget(TARGET);
