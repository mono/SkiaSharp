using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class LocalWebServer(Process process, string url) : IAsyncDisposable
{
    public string Url { get; } = url;

    internal static async Task<LocalWebServer> Start(string host, string project, string configuration,
        ITestOutputHelper output, Action<ProcessStartInfo> configure)
    {
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();

        var url = $"http://127.0.0.1:{port}";
        var start = new ProcessStartInfo(host)
        {
            WorkingDirectory = Path.GetDirectoryName(project)!,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in new[]
        {
            "run", "--no-build", "--no-restore", "--no-launch-profile",
            "--project", project, "-c", configuration
        })
            start.ArgumentList.Add(argument);
        configure(start);
        start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
        start.Environment["ASPNETCORE_URLS"] = url;
        output.WriteLine($"{host} {string.Join(" ", start.ArgumentList)}");
        var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {project}");
        process.OutputDataReceived += (_, args) => { if (args.Data is { } line) output.WriteLine($"[server] {line}"); };
        process.ErrorDataReceived += (_, args) => { if (args.Data is { } line) output.WriteLine($"[server error] {line}"); };
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        var server = new LocalWebServer(process, url);
        try
        {
            using var client = new HttpClient(new HttpClientHandler { UseProxy = false })
                { Timeout = TimeSpan.FromSeconds(2) };
            var deadline = DateTime.UtcNow.AddSeconds(60);
            while (DateTime.UtcNow < deadline)
            {
                if (process.HasExited)
                    throw new InvalidOperationException($"Web server for {project} exited ({process.ExitCode}) before readiness.");
                try
                {
                    using var response = await client.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                        return server;
                }
                catch (HttpRequestException) { }
                catch (TaskCanceledException) { }
                await Task.Delay(250);
            }
            throw new TimeoutException($"Web server for {project} did not become ready at {url}.");
        }
        catch
        {
            await server.DisposeAsync();
            throw;
        }
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
        }
        finally
        {
            process.Dispose();
        }
    }
}
