DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "tests-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// WINDOWS DEVICE TESTS
////////////////////////////////////////////////////////////////////////////////////////////////////

Task ("Default")
    .Description ("Run all Windows device tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.windows/{DATE_TIME_STR}";
    var tfm = "net11.0-windows10.0.19041.0";

    var properties = new Dictionary<string, string> {
        // Restrict restore too: --framework alone still restores the other platforms.
        { "TargetFramework", tfm },
        { "TargetFrameworks", tfm },
    };

    // Startup/result-channel diagnostics written by the packaged app (see
    // Platforms/Windows/StartupDiagnostics.cs); published with the test logs.
    var diagnosticsGlob = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/bin/**/device-runner-diagnostics.log";
    DeleteFiles(diagnosticsGlob);

    // TEMP: confirm which machine-wide runtimes a Windows-activated app can see.
    var machineRuntimes = $"{EnvironmentVariable("ProgramFiles")}/dotnet/shared/Microsoft.NETCore.App";
    Information($"Machine-wide runtimes in {machineRuntimes}: " +
        string.Join(", ", DirectoryExists(machineRuntimes) ? GetSubDirectories(machineRuntimes).Select(d => d.GetDirectoryName()) : new[] { "<missing>" }));

    try {
        RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: tfm, noBuild: SKIP_BUILD, properties: properties);
    } finally {
        EnsureDirectoryExists(results);
        var logs = GetFiles(diagnosticsGlob);
        if (!logs.Any()) {
            Warning($"No Windows device-test diagnostics found at {diagnosticsGlob}. The app may not have started or may not have been able to write the log.");
        }
        var logIndex = 0;
        foreach (var log in logs) {
            Information($"Collecting app diagnostics: {log}");
            Information(System.IO.File.ReadAllText(log.FullPath));
            CopyFile(log, results.CombineWithFilePath($"device-runner-diagnostics-{++logIndex}.log"));
        }
    }
});

RunTarget(TARGET);
