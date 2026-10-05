DirectoryPath ROOT_PATH = MakeAbsolute(Directory("../../.."));

#load "../shared/shared.cake"
#load "../shared/msbuild.cake"
#load "test-shared.cake"

////////////////////////////////////////////////////////////////////////////////////////////////////
// APPLE TESTS — iOS, Mac Catalyst (build + dotnet test execution)
////////////////////////////////////////////////////////////////////////////////////////////////////

var IOS_SIMULATOR_NAME = Argument("iosSimulator", EnvironmentVariable("IOS_SIMULATOR_NAME") ?? "iPhone 16");
var IOS_CAUSAL_DIAGNOSTICS = EnvironmentVariable("SKIASHARP_IOS_CAUSAL_DIAGNOSTICS") == "true";

void WaitForIosDiagnosticStreamReady(FilePath ready)
{
    // Cake's timed IProcess.WaitForExit kills the process on timeout.
    for (var wait = 0; wait < 100 && !FileExists(ready); wait++)
        System.Threading.Thread.Sleep(100);
    if (!FileExists(ready))
        throw new Exception("The scoped iOS diagnostic stream did not become ready within 10 seconds.");
}

Task ("tests-ios")
    .Description ("Run all iOS tests.")
    .Does (() =>
{
    if (IOS_CAUSAL_DIAGNOSTICS &&
        (!IsRunningOnMacOs() || RuntimeInformation.OSArchitecture != System.Runtime.InteropServices.Architecture.X64))
        throw new Exception("The diagnostic-only iOS experiment requires the existing x64 macOS host.");

    // Create a unique simulator for this test run (matches DeviceRunners CI pattern)
    var simulatorName = $"SkiaSharp-Tests-{DateTime.UtcNow:yyyyMMdd-HHmmss}";
    Information("Creating iOS simulator: {0} (device type: {1})...", simulatorName, IOS_SIMULATOR_NAME);

    // Create simulator and capture UDID from JSON output
    RunProcess("dotnet", $"apple simulator create \"{simulatorName}\" --device-type \"{IOS_SIMULATOR_NAME}\" --format json", out var createStdout);

    try
    {
        var createJson = string.Join("", createStdout);
        var udid = System.Text.Json.JsonDocument.Parse(createJson).RootElement.GetProperty("udid").GetString();
        Information("  Created simulator with UDID: {0}", udid);

        // Boot by UDID
        DotNetTool($"apple simulator boot \"{udid}\" --wait");
        Information("  Simulator booted");

        FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
        DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.ios/{DATE_TIME_STR}";

        // Pass the simulator UDID to DeviceRunners so it targets the correct device
        var properties = new Dictionary<string, string> {
            { "DeviceRunnersDevice", udid },
        };

        var diagnosticStart = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var diagnosticScript = ROOT_PATH.CombineWithFilePath("scripts/infra/tests/collect-ios-diagnostics.py");
        DirectoryPath causalOutput = ROOT_OUTPUT_PATH.Combine($"logs/ios-causal/{DATE_TIME_STR}");
        IProcess causalStream = null;
        try
        {
            if (IOS_CAUSAL_DIAGNOSTICS)
            {
                Warning("Diagnostic-only NSZombie/GC experiment; this execution is not normal CI green evidence.");
                EnsureDirectoryExists(causalOutput.Combine("diagnostics"));
                causalStream = StartAndReturnProcess("python3", new ProcessSettings {
                    RedirectStandardError = true,
                    Arguments = new ProcessArgumentBuilder()
                        .AppendQuoted(diagnosticScript.FullPath)
                        .Append("--stream")
                        .Append("--expected-runtime").Append("com.apple.CoreSimulator.SimRuntime.iOS-26-5")
                        .Append("--device").AppendQuoted(udid)
                        .Append("--start").Append(diagnosticStart.ToString(System.Globalization.CultureInfo.InvariantCulture))
                        .Append("--output").AppendQuoted(causalOutput.FullPath),
                });
                if (causalStream == null)
                    throw new Exception("Unable to start the scoped iOS diagnostic stream.");
                var ready = causalOutput.CombineWithFilePath("diagnostics/stream-ready.json");
                WaitForIosDiagnosticStreamReady(ready);
            }
            RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-ios", noBuild: SKIP_BUILD, properties: properties);
        }
        catch
        {
            if (!IOS_CAUSAL_DIAGNOSTICS)
            {
                try
                {
                    RunProcess("python3", new ProcessSettings {
                        Arguments = new ProcessArgumentBuilder()
                            .AppendQuoted(diagnosticScript.FullPath)
                            .Append("--device").AppendQuoted(udid)
                            .Append("--start").Append(diagnosticStart.ToString(System.Globalization.CultureInfo.InvariantCulture))
                            .Append("--output").AppendQuoted(results.FullPath),
                    });
                }
                catch (Exception ex)
                {
                    Warning("iOS diagnostic capture failed; preserving the original test failure: {0}", ex.Message);
                }
            }
            throw;
        }
        finally
        {
            if (IOS_CAUSAL_DIAGNOSTICS)
            {
                if (causalStream != null)
                {
                    try
                    {
                        var stop = causalOutput.CombineWithFilePath("diagnostics/stream-stop");
                        System.IO.File.WriteAllText(stop.FullPath, "");
                        if (!causalStream.WaitForExit(30000))
                            Warning("Scoped diagnostic stream did not stop within its cleanup deadline.");
                        else
                        {
                            causalStream.WaitForExit();
                            System.IO.File.WriteAllLines(
                                causalOutput.CombineWithFilePath("diagnostics/stream-launcher.stderr.txt").FullPath,
                                causalStream.GetStandardError().Take(100).Select(line =>
                                    line.Length > 2048 ? line.Substring(0, 2048) + " [truncated]" : line));
                            if (causalStream.GetExitCode() != 0)
                                Warning("Scoped diagnostic stream failed; see its retained metadata and stderr.");
                        }
                    }
                    catch (Exception ex)
                    {
                        Warning("Scoped diagnostic stream cleanup failed; preserving the primary outcome: {0}", ex.Message);
                    }
                    finally
                    {
                        causalStream.Dispose();
                    }
                }
                try
                {
                    EnsureDirectoryExists(results.Combine("diagnostics"));
                    CopyDirectory(causalOutput.Combine("diagnostics"), results.Combine("diagnostics"));
                }
                catch (Exception ex)
                {
                    Warning("Scoped diagnostic stream artifact capture failed: {0}", ex.Message);
                }
                try
                {
                    RunProcess("python3", new ProcessSettings {
                        Arguments = new ProcessArgumentBuilder()
                            .AppendQuoted(diagnosticScript.FullPath)
                            .Append("--causal")
                            .Append("--project").AppendQuoted(csproj.FullPath)
                            .Append("--device").AppendQuoted(udid)
                            .Append("--start").Append(diagnosticStart.ToString(System.Globalization.CultureInfo.InvariantCulture))
                            .Append("--output").AppendQuoted(results.FullPath),
                    });
                }
                catch (Exception ex)
                {
                    Warning("Causal diagnostic capture failed; preserving the primary outcome: {0}", ex.Message);
                }
            }
        }
    }
    finally
    {
        // Always clean up the simulator
        Information("Deleting simulator: {0}", simulatorName);
        try
        {
            DotNetTool($"apple simulator delete --force \"{simulatorName}\"");
        }
        catch (Exception ex)
        {
            Warning($"Failed to delete simulator: {ex.Message}");
        }
    }
});

Task ("tests-maccatalyst")
    .Description ("Run all Mac Catalyst tests.")
    .Does (() =>
{
    FilePath csproj = $"{ROOT_PATH}/tests/SkiaSharp.Tests.Devices/SkiaSharp.Tests.Devices.csproj";
    DirectoryPath results = $"{ROOT_PATH}/output/logs/testlogs/SkiaSharp.Tests.Devices.maccatalyst/{DATE_TIME_STR}";

    RunDeviceRunnersTest(csproj, results, configuration: "Debug", framework: "net10.0-maccatalyst", noBuild: SKIP_BUILD);
});

Task ("Default")
    .IsDependentOn ("tests-ios")
    .IsDependentOn ("tests-maccatalyst");

RunTarget(TARGET);
