using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

[CollectionDefinition("Maui devices", DisableParallelization = true)]
public sealed class MauiDeviceCollection : ICollectionFixture<AppiumFixture>;

public sealed class AppiumFixture
{
    internal const int Port = 4727;
    internal const int AndroidPort = 8217;
    internal const int IosPort = 8117;

    internal async Task Run(SampleWorkspace workspace, bool ios, Func<Uri, Task> test)
    {
        RequireFreePort(Port);
        RequireFreePort(ios ? IosPort : AndroidPort);
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "appium");
        Directory.CreateDirectory(diagnostics);
        string[] arguments = ["--address", "127.0.0.1", "--port", Port.ToString(), "--log-timestamp"];
        using var server = ProcessRunner.Start(
            OperatingSystem.IsWindows() ? "cmd.exe" : "appium",
            OperatingSystem.IsWindows() ? ["/d", "/c", "appium", .. arguments] : arguments,
            workspace.Root);
        var listening = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var stdout = DrainOutput();
        var stderr = server.StandardError.ReadToEndAsync();
        try
        {
            var address = new Uri($"http://127.0.0.1:{Port}");
            await WaitUntilReady(server, address, listening.Task);
            await test(address);
        }
        finally
        {
            try
            {
                if (!server.HasExited)
                    server.Kill(entireProcessTree: true);
                await server.WaitForExitAsync();
            }
            finally
            {
                await SaveLog("server.stdout.log", await stdout);
                await SaveLog("server.stderr.log", await stderr);
            }
        }

        async Task SaveLog(string name, string text)
        {
            var path = Path.Combine(diagnostics, name);
            await File.WriteAllTextAsync(path, text);
            TestContext.Current.AddFileAttachment(path, "text/plain");
        }

        async Task<string> DrainOutput()
        {
            var text = new StringBuilder();
            while (await server.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                text.AppendLine(line);
                if (line.Contains($"Appium REST http interface listener started on http://127.0.0.1:{Port}", StringComparison.Ordinal))
                    listening.TrySetResult();
            }
            return text.ToString();
        }
    }

    internal static void RequireFreePort(int port)
    {
        var listener = new TcpListener(IPAddress.Loopback, port);
        try
        {
            listener.Start();
        }
        catch (SocketException error)
        {
            throw new InvalidOperationException(
                $"Required owned Appium/driver port {port} is occupied. Refusing to reuse or stop a foreign server.", error);
        }
        finally
        {
            listener.Stop();
        }
    }

    private static async Task WaitUntilReady(Process server, Uri address, Task listening)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(30);
        while (DateTime.UtcNow < deadline)
        {
            if (server.HasExited)
                throw new InvalidOperationException($"Owned Appium exited before readiness: {server.ExitCode}.");
            try
            {
                using var response = await client.GetAsync(new Uri(address, "/status"));
                if (response.IsSuccessStatusCode && listening.IsCompletedSuccessfully && !server.HasExited)
                {
                    using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
                    if (json.RootElement.GetProperty("value").GetProperty("ready").GetBoolean())
                        return;
                }
            }
            catch (Exception error) when (error is HttpRequestException or TaskCanceledException)
            {
                TestContext.Current.TestOutputHelper?.WriteLine($"Owned Appium readiness: {error.Message}");
            }
            await Task.Delay(250);
        }
        throw new TimeoutException("Owned Appium did not become ready within 30 seconds; inspect attached server logs.");
    }
}
