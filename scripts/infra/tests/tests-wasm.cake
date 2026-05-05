DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// WASM TESTS — build and run browser-based tests
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Does (() =>
{
    if (!SKIP_BUILD) {
        RunDotNetBuild ($"{ROOT_PATH}/tests/SkiaSharp.Tests.Wasm.sln");
    }

    IProcess serverProc = null;
    try {
        var wasmProj = MakeAbsolute (File ($"{ROOT_PATH}/tests/SkiaSharp.Tests.Wasm/SkiaSharp.Tests.Wasm.csproj")).FullPath;
        serverProc = RunAndReturnProcess ("dotnet", $"run --project {wasmProj} --no-build -c {CONFIGURATION}");
        DotNetRun ($"{ROOT_PATH}/utils/WasmTestRunner/WasmTestRunner.csproj",
            $"--output=\"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Wasm/{DATE_TIME_STR}/\" " +
            (string.IsNullOrEmpty (CHROMEWEBDRIVER) ? "" : $"--driver=\"{CHROMEWEBDRIVER}\" ") +
            "--verbose " +
            "\"http://127.0.0.1:8000/\" ");
    } finally {
        serverProc?.Kill ();
    }
});

RunTarget(TARGET);
