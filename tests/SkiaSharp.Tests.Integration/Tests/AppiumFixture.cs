using System.Diagnostics;
using Xunit;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Shared Appium server fixture for all MAUI tests.
/// Appium tests must run sequentially since only one app can be focused at a time.
/// </summary>
public class AppiumFixture : IAsyncLifetime
{
    public const int Port = 4723;
    private Process? _appiumProcess;

    public async Task InitializeAsync()
    {
        // Check if Appium is already running
        if (await IsAppiumRunning())
        {
            Console.WriteLine($"[AppiumFixture] Appium already running on port {Port}");
            return;
        }

        Console.WriteLine($"[AppiumFixture] Starting Appium on port {Port}...");
        
        var psi = new ProcessStartInfo
        {
            FileName = "appium",
            Arguments = $"--port {Port} --relaxed-security --log-timestamp",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        _appiumProcess = Process.Start(psi);
        if (_appiumProcess == null)
            throw new Exception("Failed to start Appium process");

        _appiumProcess.OutputDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"[Appium] {e.Data}"); };
        _appiumProcess.ErrorDataReceived += (_, e) => { if (e.Data != null) Console.WriteLine($"[Appium ERR] {e.Data}"); };
        _appiumProcess.BeginOutputReadLine();
        _appiumProcess.BeginErrorReadLine();

        // Wait for Appium to be ready
        var ready = await WaitForAppiumReady(timeoutSeconds: 30);
        if (!ready)
            throw new Exception("Appium server failed to start within timeout");

        Console.WriteLine($"[AppiumFixture] Appium ready on port {Port}");
    }

    public async Task DisposeAsync()
    {
        if (_appiumProcess != null && !_appiumProcess.HasExited)
        {
            Console.WriteLine("[AppiumFixture] Stopping Appium...");
            _appiumProcess.Kill();
            await _appiumProcess.WaitForExitAsync();
            _appiumProcess.Dispose();
        }
    }

    private static async Task<bool> IsAppiumRunning()
    {
        try
        {
            using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
            var response = await client.GetAsync($"http://127.0.0.1:{Port}/status");
            return response.IsSuccessStatusCode;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> WaitForAppiumReady(int timeoutSeconds)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync($"http://127.0.0.1:{Port}/status");
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch
            {
                // Not ready yet
            }
            await Task.Delay(1000);
        }
        return false;
    }
}

/// <summary>
/// Collection definition for MAUI/Appium tests.
/// Tests in this collection run sequentially and share the Appium server.
/// </summary>
[CollectionDefinition("Appium")]
public class AppiumCollection : ICollectionFixture<AppiumFixture>
{
}
