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

    public async ValueTask InitializeAsync()
    {
        // Check if Appium is already running
        if (await IsAppiumRunning())
        {
            Console.WriteLine($"[AppiumFixture] Appium already running on port {Port}");
            return;
        }

        Console.WriteLine($"[AppiumFixture] Starting Appium on port {Port}...");
        
        // npm exec honors Appium's project-local or global extension context. --no prevents
        // npm from downloading Appium when the approved installation is unavailable.
        var (shell, shellArgs) = GetShellCommand($"npm exec --no -- appium --port {Port} --relaxed-security --log-timestamp");

        var psi = new ProcessStartInfo
        {
            FileName = shell,
            Arguments = shellArgs,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = FindRepositoryRoot()
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

    public async ValueTask DisposeAsync()
    {
        if (_appiumProcess != null && !_appiumProcess.HasExited)
        {
            Console.WriteLine("[AppiumFixture] Stopping Appium...");
            // Appium runs as a child of the shell wrapper, so the whole tree must be killed —
            // killing just the shell would leave the server alive and holding the port.
            _appiumProcess.Kill(entireProcessTree: true);
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

    private static (string Shell, string Arguments) GetShellCommand(string command) =>
        OperatingSystem.IsWindows()
            ? ("cmd.exe", $"/C {command}")
            : ("/bin/bash", $"-c \"{command}\"");

    private static string FindRepositoryRoot()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory); directory != null; directory = directory.Parent)
        {
            var git = Path.Combine(directory.FullName, ".git");
            if (Directory.Exists(git) || File.Exists(git))
                return directory.FullName;
        }

        return Directory.GetCurrentDirectory();
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
