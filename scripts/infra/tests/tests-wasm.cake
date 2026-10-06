DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// WASM TESTS — build and run the browser-based SkiaSharp.Tests.Wasm suite via dotnet test.
//
// Pass --previewtfm=true to build and run the suite as the preview .NET (-p:UsePreviewTFM=true).
// The preview SDK and its wasm-tools workload must already be installed and selected (global.json)
// by the caller.
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Wasm/SkiaSharp.Tests.Wasm.csproj";

    var previewTfm = Argument("previewtfm", false);

    var subdir = previewTfm ? "SkiaSharp.Tests.Wasm.Preview" : "SkiaSharp.Tests.Wasm";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/{subdir}/{DATE_TIME_STR}";

    var properties = previewTfm
        ? new Dictionary<string, string> { ["UsePreviewTFM"] = "true" }
        : null;

    RunDeviceRunnersTest(csproj, results, noBuild: SKIP_BUILD, properties: properties);
});

RunTarget(TARGET);
