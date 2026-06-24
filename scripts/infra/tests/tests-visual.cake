DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// VISUAL REGRESSION MATRIX — cross-backend golden-image harness
// (see documentation/dev/golden-image-tests.md)
//
// The visual matrix is shared test code that already runs in-process inside the
// regular test stages (tests-netcore, tests-android, tests-ios, tests-wasm). This
// target is a focused convenience: it runs ONLY the cells tagged
// [Trait("Category", "Visual")] in the desktop Console host, which is the host
// that owns the desktop GPU renderers (raster, ganesh-gl, ganesh-metal,
// ganesh-vulkan). Use it to:
//
//   * run just the visual suite locally without the rest of the test run, and
//   * (re)record/seed goldens in CI by passing --updateGoldens=true, which is
//     how per-platform GPU goldens are generated on the agents that have the
//     driver / software ICD (the captured PNGs are published as an artifact to
//     be committed).
//
// Unlike the prior-art harness this replaces, it uses the real test runner
// (no fictional RunDotNetPublish), keeps strict failure discipline (a non-zero
// exit fails the target), and has no opt-in env gate — the cells run whenever
// the target runs.
////////////////////////////////////////////////////////////////////////////////////////////////////

// Set true to (re)record goldens into the source tree instead of comparing.
var UPDATE_GOLDENS = Argument("updateGoldens", false);
// Optional record scope: shared | renderer | platform (see GoldenStore).
var GOLDEN_SCOPE = Argument("goldenScope", "");

Task("Default")
    .Description("Run the cross-backend visual-regression matrix in the desktop Console host.")
    .Does(() =>
{
    var tfm = "net10.0";
    var csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj";

    if (!SKIP_BUILD) {
        RunDotNetBuild(csproj, properties: new Dictionary<string, string> {
            { "TargetFramework", tfm }
        });
    }

    var binDir = MakeAbsolute(Directory($"{ROOT_PATH}/tests/SkiaSharp.Tests.Console/bin/{CONFIGURATION}/{tfm}"));
    var exe = binDir.CombineWithFilePath(IsRunningOnWindows() ? "SkiaSharp.Tests.exe" : "SkiaSharp.Tests");
    var results = MakeAbsolute(Directory($"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Visual/{DATE_TIME_STR}/{tfm}"));
    EnsureDirectoryExists(results);

    // The test process reads these to switch from compare to record mode.
    var env = new Dictionary<string, string>();
    if (UPDATE_GOLDENS)
        env["SKIASHARP_UPDATE_GOLDENS"] = "1";
    if (!string.IsNullOrEmpty(GOLDEN_SCOPE))
        env["SKIASHARP_GOLDEN_SCOPE"] = GOLDEN_SCOPE;

    var exitCode = StartProcess(exe, new ProcessSettings {
        WorkingDirectory = binDir,
        EnvironmentVariables = env,
        Arguments = new ProcessArgumentBuilder()
            .Append("--filter-trait").AppendQuoted("Category=Visual")
            .Append("--results-directory").AppendQuoted(results.FullPath)
            .Append("--report-trx")
            .Append("--report-trx-filename").Append("TestResults.trx")
            .Append("--hangdump")
            .Append("--hangdump-timeout").Append("15m")
            .Append("--hangdump-type").Append("Mini"),
    });

    if (exitCode != 0)
        throw new Exception($"Visual tests failed: SkiaSharp.Tests returned exit code {exitCode}.");
});

RunTarget(TARGET);
