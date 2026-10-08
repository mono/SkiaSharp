using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

// Builds and runs the actual Docker samples, checking console and HTTP image output.
[Trait("Category", "Docker")]
public class DockerSampleTests : IClassFixture<DockerSampleFixture>, IAsyncLifetime
{
    private readonly DockerSampleFixture docker;
    private readonly string containerName;
    private readonly string diagnostics;

    private static readonly Dictionary<string, (string OutputPath, string PortBinding)> Hosts = new()
    {
        ["Windows"] = (@"C:\app\output.png", "8080"),
        ["Mac"] = ("/app/output.png", "127.0.0.1::8080"),
        ["Linux"] = ("/app/output.png", "127.0.0.1::8080")
    };

    public DockerSampleTests(DockerSampleFixture docker)
    {
        this.docker = docker;
        containerName = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        diagnostics = Path.Combine(Repo.ArtifactsDir, containerName);
        Directory.CreateDirectory(diagnostics);
        docker.OwnContainer(containerName);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;

    // xUnit disposes each test instance, including when its test fails.
    public async ValueTask DisposeAsync() => await docker.RemoveContainer(containerName);

    internal static (string OutputPath, string PortBinding) HostSettings(string platform) => Hosts[platform];

    public static IEnumerable<object[]> Cases() =>
        SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform)
            .Where(sample => sample.Kind == SampleKind.Docker)
            .Select(sample => new object[] { sample.Folder, sample.FileName });

    public static IEnumerable<object[]> ConsoleCases()
    {
        var sample = DockerSample("DockerConsole");
        yield return [sample.Folder, sample.FileName, HostSettings(SampleLookup.HostPlatform).OutputPath];
    }

    public static IEnumerable<object[]> HttpCases()
    {
        var sample = DockerSample("DockerWebApi");
        yield return [sample.Folder, sample.FileName, HostSettings(SampleLookup.HostPlatform).PortBinding];
    }

    private static SampleEntry DockerSample(string name) =>
        Assert.Single(SampleLookup.Discover(Repo.SamplesDir, SampleLookup.HostPlatform),
            sample => sample.Kind == SampleKind.Docker && sample.Folder == Path.Combine("Basic", name));

    [Theory]
    [Trait("Category", "DockerBuild")]
    [MemberData(nameof(Cases))]
    public async Task DockerImageBuilds(string folder, string dockerfile)
    {
        await docker.Image(folder, dockerfile);
    }

    [Theory]
    [Trait("Category", "SampleRun")]
    [MemberData(nameof(ConsoleCases))]
    public async Task DockerConsoleSampleRuns(string folder, string dockerfile, string outputPath)
    {
        var tag = await docker.Image(folder, dockerfile);
        await RunConsole(tag, outputPath);
    }

    [Theory]
    [Trait("Category", "SampleRun")]
    [MemberData(nameof(HttpCases))]
    public async Task DockerHttpSampleRuns(string folder, string dockerfile, string portBinding)
    {
        var tag = await docker.Image(folder, dockerfile);
        var requests = Path.Combine(Repo.SamplesDir, folder, "sample.http");
        await RunHttp(tag, requests, portBinding);
    }

    private async Task RunConsole(string tag, string outputPath)
    {
        await docker.Run(["run", "--name", containerName, tag, "SkiaSharp", "--output", outputPath],
            TimeSpan.FromMinutes(3));
        var image = Path.Combine(diagnostics, "output.png");
        await docker.Run(["cp", $"{containerName}:{outputPath}", image]);
        SampleImage.Validate(File.ReadAllBytes(image));
        SampleArtifacts.AttachFile(image, "image/png");
    }

    private async Task RunHttp(string tag, string requestsFile, string portBinding)
    {
        var requests = ReadHttpRequests(requestsFile);
        Assert.Equal(2, requests.Count);
        await docker.Run(["run", "-d", "--name", containerName, "-p", portBinding, tag],
            TimeSpan.FromMinutes(2));
        var binding = await docker.Run(["port", containerName, "8080/tcp"]);
        var portText = binding.Split('\n', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
        Assert.True(int.TryParse(portText[(portText.LastIndexOf(':') + 1)..], out var port) && port > 0,
            $"Docker did not publish an HTTP port: {binding}");
        using var client = new HttpClient(new HttpClientHandler { UseProxy = false }) { Timeout = TimeSpan.FromSeconds(4) };
        var health = Rebase(requests[0], port);
        var ready = false;
        var lastError = "No response";
        // Wait for the health request before checking the image response.
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
            await docker.Run(["logs", containerName]);
            Assert.Fail($"Docker HTTP sample did not become ready at {health}: {lastError}");
        }

        using var imageResponse = await client.GetAsync(Rebase(requests[1], port));
        Assert.True(imageResponse.IsSuccessStatusCode, $"GET {requests[1]} returned {(int)imageResponse.StatusCode}");
        Assert.Equal("image/png", imageResponse.Content.Headers.ContentType?.MediaType);
        var bytes = await imageResponse.Content.ReadAsByteArrayAsync();
        SampleImage.Validate(bytes);
        var image = Path.Combine(diagnostics, "output.png");
        await File.WriteAllBytesAsync(image, bytes);
        SampleArtifacts.AttachFile(image, "image/png");
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
}
