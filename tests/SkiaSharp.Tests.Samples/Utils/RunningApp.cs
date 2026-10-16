using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal abstract class RunningApp(string diagnostics)
{
    private readonly HttpClient client = new(new HttpClientHandler
    {
        UseProxy = false,
        AllowAutoRedirect = false
    })
    {
        Timeout = TimeSpan.FromSeconds(4)
    };

    private Uri? address;

    internal string Diagnostics { get; } = diagnostics;

    internal abstract Task<Uri> GetAddress();

    internal abstract Task<(int ExitCode, string Output, string Error)> WaitForExit();

    protected abstract Task Stop();

    protected abstract Task<(string Output, string Error)> ReadOutput();

    internal async Task<HttpResponseMessage> GetResponse(string path)
    {
        return await client.GetAsync(await RequestUri(path), TestContext.Current.CancellationToken);
    }

    internal async Task<HttpResponseMessage> WaitForResponse(string path)
    {
        var request = await RequestUri(path);
        var lastError = "No response";
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                var response = await GetResponse(path);
                if (response.IsSuccessStatusCode)
                    return response;
                lastError = $"HTTP {(int)response.StatusCode}";
                response.Dispose();
            }
            catch (HttpRequestException error) { lastError = error.Message; }
            catch (TaskCanceledException error) when (!TestContext.Current.CancellationToken.IsCancellationRequested)
            {
                lastError = error.Message;
            }
            await Task.Delay(TimeSpan.FromSeconds(1), TestContext.Current.CancellationToken);
        }
        throw new TimeoutException($"HTTP sample did not become ready at {request}: {lastError}");
    }

    internal async Task Run(Func<Task> test)
    {
        try
        {
            await test();
        }
        finally
        {
            try
            {
                try
                {
                    await Stop();
                }
                finally
                {
                    var result = await ReadOutput();

                    Directory.CreateDirectory(Diagnostics);
                    var stdoutLog = Path.Combine(Diagnostics, "run.stdout.log");
                    var stderrLog = Path.Combine(Diagnostics, "run.stderr.log");
                    await File.WriteAllTextAsync(stdoutLog, result.Output);
                    await File.WriteAllTextAsync(stderrLog, result.Error);
                    TestContext.Current.TestOutputHelper?.WriteLine(result.Output);
                    TestContext.Current.TestOutputHelper?.WriteLine(result.Error);
                    TestContext.Current.AddFileAttachment(stdoutLog, "text/plain");
                    TestContext.Current.AddFileAttachment(stderrLog, "text/plain");
                }
            }
            finally
            {
                client.Dispose();
            }
        }
    }

    internal async Task<Uri> RequestUri(string path)
    {
        address ??= await GetAddress();
        if (!path.StartsWith('/') || path.StartsWith("//", StringComparison.Ordinal) ||
            path.Contains('\\') || path.Any(char.IsControl) ||
            !Uri.TryCreate(address, path, out var request) ||
            request.Scheme != address.Scheme || request.Authority != address.Authority || request.Fragment.Length > 0)
            throw new ArgumentException($"HTTP sample requests must stay on their owned server: {path}", nameof(path));
        return request;
    }
}
