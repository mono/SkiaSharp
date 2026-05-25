DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// VISUAL REGRESSION MATRIX — cross-backend pixel-diff harness
// (see documentation/dev/visual-tests.md)
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Description ("Run the cross-backend visual-regression matrix.")
    .Does (() =>
{
    // The matrix is opt-in (see documentation/dev/visual-tests.md);
    // setting this enables the [VisualTheory] gate.
    System.Environment.SetEnvironmentVariable ("SKIASHARP_VISUAL_TESTS", "1");

    var tfm = "net10.0";
    var csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj";

    // Build the test project + every out-of-process host whose toolchain is
    // present on this agent. Each host whose .app / .apk / wasm-publish
    // output is missing self-reports an unavailable reason in the matrix.
    if (!SKIP_BUILD) {
        RunDotNetBuild (csproj, properties: new Dictionary<string, string> {
            { "TargetFramework", tfm }
        });

        // RenderHost.Wasm — publishable on any OS with the wasm-tools workload.
        try {
            RunDotNetPublish ($"{ROOT_PATH}/tests/Hosts/RenderHost.Wasm/RenderHost.Wasm.csproj");
        } catch {
            Warning ("RenderHost.Wasm publish failed — wasm-* cells will skip.");
        }

        // RenderHost.iOS — macOS only (uses xcrun simctl + Xcode iOS SDK).
        if (IsRunningOnMacOs ()) {
            try {
                RunDotNetBuild ($"{ROOT_PATH}/tests/Hosts/RenderHost.iOS/RenderHost.iOS.csproj");
            } catch {
                Warning ("RenderHost.iOS build failed — ios-* cells will skip.");
            }
        }

        // RenderHost.Android — any OS with the android workload + NDK.
        try {
            RunDotNetBuild ($"{ROOT_PATH}/tests/Hosts/RenderHost.Android/RenderHost.Android.csproj");
        } catch {
            Warning ("RenderHost.Android build failed — android-* cells will skip.");
        }
    }

    var results = MakeAbsolute (Directory ($"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Visual/{DATE_TIME_STR}/{tfm}"));
    var csprojAbs = MakeAbsolute (File (csproj));
    DotNetTest (csprojAbs.FullPath, new DotNetTestSettings {
        Configuration = CONFIGURATION,
        NoBuild = true,
        Loggers = new[] { "xunit" },
        WorkingDirectory = csprojAbs.GetDirectory (),
        ResultsDirectory = results,
        Filter = "FullyQualifiedName~VisualMatrixTests",
        Verbosity = DotNetVerbosity.Normal,
        ArgumentCustomization = args => args
            .Append ("/p:Platform=\"AnyCPU\"")
            .Append ($"/p:TargetFramework={tfm}"),
    });
});

RunTarget(TARGET);
