using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace SkiaSharp.Tests.MSBuild.Utils;

public sealed class DotNet : IDisposable
{
    private readonly string root;
    private readonly string cache = Path.Combine(Path.GetTempPath(), $"skiasharp-msbuild-{Guid.NewGuid():N}");
    private readonly string host = Setting("DotNetHost");

    public string PackageDirectory { get; } = Path.GetFullPath(Setting("PackageDirectory"));

    public DotNet()
    {
        root = Path.Combine(Path.GetFullPath(Setting("ArtifactsDirectory")), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "Directory.Build.props"), "<Project />");
        File.WriteAllText(Path.Combine(root, "Directory.Build.targets"), "<Project />");
        File.WriteAllText(Path.Combine(root, "global.json"), JsonSerializer.Serialize(new
        {
            sdk = new { version = Setting("SdkVersion"), rollForward = "disable" }
        }));
        new XDocument(new XElement("configuration",
            new XElement("packageSources",
                new XElement("clear"),
                new XElement("add", new XAttribute("key", "artifacts"), new XAttribute("value", PackageDirectory)),
                new XElement("add", new XAttribute("key", "dotnet-public"),
                    new XAttribute("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json"))),
            new XElement("packageSourceMapping",
                new XElement("clear"),
                new XElement("packageSource", new XAttribute("key", "artifacts"),
                    new XElement("package", new XAttribute("pattern", "SkiaSharp*")),
                    new XElement("package", new XAttribute("pattern", "HarfBuzzSharp*"))),
                new XElement("packageSource", new XAttribute("key", "dotnet-public"),
                    new XElement("package", new XAttribute("pattern", "*"))))))
            .Save(Path.Combine(root, "NuGet.Config"));
    }

    public string NewProject(string name, string xml)
    {
        var directory = Path.Combine(root, name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "Consumer.csproj"), xml);
        File.WriteAllText(Path.Combine(directory, "Program.cs"), "System.Console.WriteLine(\"Package consumer\");");
        return directory;
    }

    public Task Build(string directory) => Run(directory, "build");

    public Task Publish(string directory, string? rid = null) => Run(directory, "publish", rid);

    private async Task Run(string directory, string command, string? rid = null)
    {
        var start = new ProcessStartInfo(host)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in new[] { command, "Consumer.csproj", "-c", "Release", "-o", "output",
            "--nologo", "-v:minimal", "-bl:build.binlog", $"-p:RestoreConfigFile={Path.Combine(root, "NuGet.Config")}" })
            start.ArgumentList.Add(argument);
        if (rid is not null)
        {
            start.ArgumentList.Add("-r");
            start.ArgumentList.Add(rid);
        }
        foreach (var key in start.Environment.Keys.Where(k =>
            k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("Restore", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("NUGET_", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("NuGetPackageRoot", StringComparison.OrdinalIgnoreCase)).ToArray())
            start.Environment.Remove(key);
        start.Environment["NUGET_PACKAGES"] = Path.Combine(cache, "packages");
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(cache, "http");
        start.Environment["NUGET_SCRATCH"] = Path.Combine(cache, "scratch");
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(cache, "home");
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true";
        File.WriteAllText(Path.Combine(directory, "command.txt"),
            host + " " + string.Join(" ", start.ArgumentList.Select(a => $"\"{a}\"")));

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(5));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"dotnet {command} timed out; see {directory}");
        }
        finally
        {
            File.WriteAllText(Path.Combine(directory, "stdout.txt"), await stdout);
            File.WriteAllText(Path.Combine(directory, "stderr.txt"), await stderr);
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"dotnet {command} failed ({process.ExitCode}); see {directory}\n{await stdout}\n{await stderr}");
    }

    public void CleanBuildOutput(string directory)
    {
        File.Copy(Path.Combine(directory, "obj", "project.assets.json"), Path.Combine(directory, "project.assets.json"));
        File.Copy(Path.Combine(directory, "output", "Consumer.deps.json"), Path.Combine(directory, "Consumer.deps.json"));
        foreach (var name in new[] { "bin", "obj", "output" })
        {
            var path = Path.Combine(directory, name);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(cache))
            Directory.Delete(cache, recursive: true);
    }

    private static string Setting(string name) =>
        AppContext.GetData("MSBuildTest." + name) as string is { Length: > 0 } value
            ? value : throw new InvalidOperationException($"MSBuildTest.{name} runtime configuration is missing");
}
