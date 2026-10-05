using System.Diagnostics;
using System.Text.Json;
using System.Xml.Linq;

namespace SkiaSharp.Tests.MSBuild.Utils;

public sealed class DotNet : IDisposable
{
    private readonly string root;
    // WinUI's XAML compiler still uses MAX_PATH for referenced assemblies.
    private readonly string cache = Directory.CreateTempSubdirectory("skms-").FullName;
    private readonly string host = Setting("DotNetHost");

    public string PackageDirectory { get; } = Path.GetFullPath(Setting("PackageDirectory"));
    public string SdkVersion { get; } = Setting("SdkVersion");
    public string PackagesCacheDirectory => Path.Combine(cache, "p");
    public static string ConsumerTargetFramework => $"net{ConsumerSdkMajor}.0";
    public static int ConsumerSdkMajor
    {
        get
        {
            var version = Setting("SdkVersion");
            if (!int.TryParse(version.Split('.')[0], out var major) || major is not (10 or 11))
                throw new NotSupportedException($"Unsupported package consumer SDK: {version}");
            return major;
        }
    }

    public DotNet()
    {
        root = Path.Combine(Path.GetFullPath(Setting("ArtifactsDirectory")), Path.GetRandomFileName());
        Directory.CreateDirectory(root);
        File.WriteAllText(Path.Combine(root, "Directory.Build.props"), "<Project />");
        File.WriteAllText(Path.Combine(root, "Directory.Build.targets"), "<Project />");
        File.WriteAllText(Path.Combine(root, "global.json"), JsonSerializer.Serialize(new
        {
            sdk = new { version = SdkVersion, allowPrerelease = SdkVersion.Contains('-'), rollForward = "disable" }
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

    public async Task<string> NewMauiProject(string name, string xml, string app, string mauiProgram)
    {
        var directory = Path.Combine(root, name);
        Directory.CreateDirectory(directory);
        await Execute(directory, host, ["new", "maui", "--name", "Consumer", "--output", ".", "--framework", ConsumerTargetFramework,
            "--no-restore"], "template", 1);
        foreach (var file in new[] { "App.xaml", "App.xaml.cs", "AppShell.xaml", "AppShell.xaml.cs",
            "MainPage.xaml", "MainPage.xaml.cs" })
            File.Delete(Path.Combine(directory, file));
        File.WriteAllText(Path.Combine(directory, "Consumer.csproj"), xml);
        File.WriteAllText(Path.Combine(directory, "App.cs"), app);
        File.WriteAllText(Path.Combine(directory, "MauiProgram.cs"), mauiProgram);
        return directory;
    }

    public Task Build(string directory, string configuration = "Release", int timeoutMinutes = 5) =>
        Run(directory, "build", configuration: configuration, timeoutMinutes: timeoutMinutes);

    public Task Publish(string directory, string? rid = null) => Run(directory, "publish", rid);

    private async Task Run(string directory, string command, string? rid = null,
        string configuration = "Release", int timeoutMinutes = 5)
    {
        var arguments = new List<string> { command, "Consumer.csproj", "-c", configuration, "-o", "output",
            "--nologo", "--disable-build-servers", "-v:minimal", "-bl:build.binlog",
            $"-p:RestoreConfigFile={Path.Combine(root, "NuGet.Config")}" };
        if (rid is not null)
        {
            arguments.Add("-r");
            arguments.Add(rid);
        }
        await Execute(directory, host, arguments, command, timeoutMinutes);
    }

    public Task InspectAppleBinary(string directory, string binary) =>
        Execute(directory, "/usr/bin/otool", ["-L", binary], Path.GetFileName(binary) + "-link", 1);

    private async Task Execute(string directory, string executable, IEnumerable<string> arguments, string logName,
        int timeoutMinutes)
    {
        var start = new ProcessStartInfo(executable)
        {
            WorkingDirectory = directory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };
        foreach (var argument in arguments)
            start.ArgumentList.Add(argument);
        foreach (var key in start.Environment.Keys.Where(k =>
            k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("Restore", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("NUGET_", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("NuGetPackageRoot", StringComparison.OrdinalIgnoreCase)).ToArray())
            start.Environment.Remove(key);
        start.Environment["NUGET_PACKAGES"] = PackagesCacheDirectory;
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(cache, "http");
        start.Environment["NUGET_SCRATCH"] = Path.Combine(cache, "scratch");
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(cache, "home");
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        var prefix = logName == "build" || logName == "publish" ? "" : logName + "-";
        File.WriteAllText(Path.Combine(directory, prefix + "command.txt"),
            executable + " " + string.Join(" ", start.ArgumentList.Select(a => $"\"{a}\"")));

        using var process = Process.Start(start) ?? throw new InvalidOperationException("Could not start dotnet");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(timeoutMinutes));
        try
        {
            await process.WaitForExitAsync(timeout.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"{executable} {logName} timed out; see {directory}");
        }
        finally
        {
            File.WriteAllText(Path.Combine(directory, prefix + "stdout.txt"), await stdout);
            File.WriteAllText(Path.Combine(directory, prefix + "stderr.txt"), await stderr);
        }
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"{executable} {logName} failed ({process.ExitCode}); see {directory}\n{await stdout}\n{await stderr}");
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
