using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

/// <summary>
/// Shared Appium server fixture for all MAUI tests.
/// Appium tests must run sequentially since only one app can be focused at a time.
/// </summary>
public class AppiumFixture : IAsyncLifetime
{
    public const int Port = TestDevices.AppiumPort;
    private Process? _appiumProcess;

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    internal async Task Start()
    {
        if (_appiumProcess is { HasExited: false })
            return;
        if (_appiumProcess is { HasExited: true })
            throw new InvalidOperationException($"Owned Appium exited ({_appiumProcess.ExitCode}).");

        if (await IsPortOccupied())
            throw new InvalidOperationException($"Required Appium port {Port} is already in use; refusing to reuse another user's server.");

        Console.WriteLine($"[AppiumFixture] Starting Appium on port {Port}...");
        
        // npm exec honors Appium's project-local or global extension context. --no prevents
        // npm from downloading Appium when the approved installation is unavailable.
        var (shell, shellArgs) = GetShellCommand($"npm exec --no -- appium --address 127.0.0.1 --port {Port} --relaxed-security --log-timestamp");

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
        try
        {
            if (!await WaitForAppiumReady(timeoutSeconds: 30))
                throw new Exception("Appium server failed to start within timeout");
        }
        catch
        {
            await DisposeAsync();
            throw;
        }

        Console.WriteLine($"[AppiumFixture] Appium ready on port {Port}");
    }

    public async ValueTask DisposeAsync()
    {
        if (_appiumProcess != null)
        {
            try
            {
                if (!_appiumProcess.HasExited)
                {
                    Console.WriteLine("[AppiumFixture] Stopping owned Appium...");
                    _appiumProcess.Kill(entireProcessTree: true);
                    await _appiumProcess.WaitForExitAsync();
                }
            }
            finally
            {
                _appiumProcess.Dispose();
                _appiumProcess = null;
            }
        }
    }

    private static async Task<bool> IsPortOccupied()
    {
        using var client = new TcpClient();
        try
        {
            await client.ConnectAsync(IPAddress.Loopback, Port).WaitAsync(TimeSpan.FromSeconds(2));
            return true;
        }
        catch (SocketException)
        {
            return false;
        }
    }

    private async Task<bool> WaitForAppiumReady(int timeoutSeconds)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);

        while (DateTime.UtcNow < deadline)
        {
            if (_appiumProcess is { HasExited: true })
                throw new InvalidOperationException($"Owned Appium exited before becoming ready (exit {_appiumProcess.ExitCode})");
            try
            {
                using var response = await client.GetAsync($"http://127.0.0.1:{Port}/status");
                if (response.IsSuccessStatusCode && _appiumProcess is { HasExited: false })
                    return true;
            }
            catch (HttpRequestException error) { Console.WriteLine($"[Appium readiness] {error.Message}"); }
            catch (TaskCanceledException error) { Console.WriteLine($"[Appium readiness] {error.Message}"); }
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
