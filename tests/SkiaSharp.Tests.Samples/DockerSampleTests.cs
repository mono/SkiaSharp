using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public class DockerSampleTests(DockerSampleFixture docker, ITestOutputHelper output) : IClassFixture<DockerSampleFixture>
{
    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(DotNet.Setting("SamplesDirectory"), SampleLookup.HostPlatform,
                AppContext.GetData("SampleTest.Filter") as string ?? "")
            .Where(sample => sample.Kind == SampleKind.Docker)
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [Trait("Category", "DockerBuild")]
    [MemberData(nameof(Cases))]
    public async Task DockerImageBuilds(string folder, string dockerfile)
    {
        await docker.Image(folder, dockerfile, output);
    }

    [Theory(DisableDiscoveryEnumeration = true, SkipTestWithoutData = true)]
    [Trait("Category", "SampleRun")]
    [MemberData(nameof(Cases))]
    public async Task DockerSampleRuns(string folder, string dockerfile)
    {
        var tag = await docker.Image(folder, dockerfile, output);
        var name = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        var diagnostics = Path.Combine(DotNet.Setting("ArtifactsDirectory"), name);
        Directory.CreateDirectory(diagnostics);
        docker.OwnContainer(name);
        try
        {
            var requests = Path.Combine(DotNet.Setting("SamplesDirectory"), folder, "sample.http");
            if (File.Exists(requests))
                await RunHttp(tag, name, requests, diagnostics);
            else
                await RunConsole(tag, name, diagnostics);
        }
        finally
        {
            await docker.RemoveContainer(name, output);
        }
    }

    private async Task RunConsole(string tag, string name, string diagnostics)
    {
        var outputPath = OperatingSystem.IsWindows() ? @"C:\app\output.png" : "/app/output.png";
        await docker.Run(["run", "--name", name, tag, "SkiaSharp", "--output", outputPath],
            diagnostics, TimeSpan.FromMinutes(3), output);
        var image = Path.Combine(diagnostics, "output.png");
        await docker.Run(["cp", $"{name}:{outputPath}", image], diagnostics, TimeSpan.FromMinutes(1), output);
        SampleImage.Validate(File.ReadAllBytes(image));
    }

    private async Task RunHttp(string tag, string name, string requestsFile, string diagnostics)
    {
        var requests = ReadHttpRequests(requestsFile);
        await docker.Run(["run", "-d", "--name", name, "-p", HttpPortBinding(OperatingSystem.IsWindows()), tag],
            diagnostics, TimeSpan.FromMinutes(2), output);
        var binding = await docker.Run(["port", name, "8080/tcp"], diagnostics, TimeSpan.FromMinutes(1), output);
        var portText = binding.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        Assert.True(int.TryParse(portText[(portText.LastIndexOf(':') + 1)..], out var port) && port > 0,
            $"Docker did not publish an HTTP port: {binding}");
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(4) };
        var health = Rebase(requests[0], port);
        var ready = false;
        var lastError = "No response";
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                using var response = await client.GetAsync(health);
                if (response.IsSuccessStatusCode)
                {
                    ready = true;
                    break;
                }
                lastError = $"HTTP {(int)response.StatusCode}";
            }
            catch (HttpRequestException error) { lastError = error.Message; }
            catch (TaskCanceledException error) { lastError = error.Message; }
            await Task.Delay(TimeSpan.FromSeconds(1));
        }
        if (!ready)
        {
            output.WriteLine(await docker.Run(["logs", name], diagnostics, TimeSpan.FromMinutes(1), output));
            Assert.Fail($"Docker HTTP sample did not become ready at {health}: {lastError}");
        }

        var savedImage = false;
        foreach (var request in requests)
        {
            using var response = await client.GetAsync(Rebase(request, port));
            Assert.True(response.IsSuccessStatusCode, $"GET {request} returned {(int)response.StatusCode}");
            if (response.Content.Headers.ContentType?.MediaType != "image/png")
                continue;
            var bytes = await response.Content.ReadAsByteArrayAsync();
            SampleImage.Validate(bytes);
            await File.WriteAllBytesAsync(Path.Combine(diagnostics, "output.png"), bytes);
            savedImage = true;
        }
        Assert.True(savedImage, $"No PNG response was saved from {requestsFile}");
    }

    internal static IReadOnlyList<Uri> ReadHttpRequests(string file)
    {
        var requests = new List<Uri>();
        foreach (var line in File.ReadLines(file).Select(line => line.Trim()))
        {
            if (line is "" or "###")
                continue;
            if (!line.StartsWith("GET ", StringComparison.Ordinal) ||
                !Uri.TryCreate(line[4..], UriKind.Absolute, out var uri) ||
                uri.Scheme != Uri.UriSchemeHttp || uri.Host != "localhost" || uri.Port != 8080 ||
                uri.UserInfo.Length > 0 || uri.Fragment.Length > 0)
                throw new InvalidOperationException($"Unsupported sample.http request: '{line}' (use plain GET http://localhost:8080/...)");
            requests.Add(uri);
        }
        if (requests.Count == 0)
            throw new InvalidOperationException($"No GET requests in {file}");
        return requests;
    }

    internal static Uri Rebase(Uri request, int port) => new UriBuilder(request) { Host = "127.0.0.1", Port = port }.Uri;

    internal static string HttpPortBinding(bool windows) => windows ? "8080" : "127.0.0.1::8080";
}
