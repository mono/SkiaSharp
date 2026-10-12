using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class AppiumCollection : ICollectionFixture<AppiumFixture>
{
    public const string Name = "Appium";
}

public sealed class AppiumFixture
{
    internal const int Port = 4727;
    internal const int AndroidPort = 8217;
    internal const int IosPort = 8117;
    internal const int MacPort = 10117;
    internal const int WindowsPort = 4737;
    private static readonly SemaphoreSlim focus = new(1, 1);

    internal async Task Run(AppiumRunningApp app, Func<AppiumRunningApp, Task> scenario)
    {
        ArgumentNullException.ThrowIfNull(app);
        ArgumentNullException.ThrowIfNull(scenario);
        await focus.WaitAsync(TestContext.Current.CancellationToken);
        try
        {
            await RunOwned(app, scenario);
        }
        finally
        {
            focus.Release();
        }
    }

    private static async Task RunOwned(AppiumRunningApp app, Func<AppiumRunningApp, Task> scenario)
    {
        Process? server = null;
        Task<string>? stdout = null;
        Task<string>? stderr = null;
        var stopped = false;
        var diagnostics = app.Diagnostics;
        string[] arguments = ["--address", "127.0.0.1", "--port", Port.ToString(), "--log-timestamp"];
        var listening = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var address = new Uri($"http://127.0.0.1:{Port}");
        await app.Run(address, scenario, StartServer, StopServer);

        async Task StopServer()
        {
            if (stopped || server is null)
                return;
            stopped = true;
            List<Exception> failures = [];
            await AppiumRunningApp.Attempt(failures, async () =>
            {
                if (!server.HasExited)
                    server.Kill(entireProcessTree: true);
                await server.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(30));
            });
            await AppiumRunningApp.Attempt(failures, async () =>
                await SaveLog("server.stdout.log", await stdout!.WaitAsync(TimeSpan.FromSeconds(30))));
            await AppiumRunningApp.Attempt(failures, async () =>
                await SaveLog("server.stderr.log", await stderr!.WaitAsync(TimeSpan.FromSeconds(30))));
            server.Dispose();
            AppiumRunningApp.ThrowFailures(failures);
        }

        async Task StartServer()
        {
            RequireFreePort(Port);
            Directory.CreateDirectory(diagnostics);
            try
            {
                server = ProcessRunner.Start(
                    OperatingSystem.IsWindows() ? "cmd.exe" : "appium",
                    OperatingSystem.IsWindows() ? ["/d", "/c", "appium", .. arguments] : arguments,
                    app.Workspace.Root);
            }
            catch (Win32Exception error)
            {
                throw new InvalidOperationException(
                    "Install Appium and the selected platform driver explicitly before selecting UI tests. Nothing is installed by discovery or fixtures.", error);
            }
            stdout = DrainOutput();
            stderr = server.StandardError.ReadToEndAsync();
            await WaitUntilReady(server, address, listening.Task);
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
            while (await server!.StandardOutput.ReadLineAsync().ConfigureAwait(false) is { } line)
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
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false })
        {
            Timeout = TimeSpan.FromSeconds(2)
        };
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
