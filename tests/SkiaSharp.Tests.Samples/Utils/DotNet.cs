using System.Diagnostics;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

public sealed class DotNet : IDisposable
{
    private readonly string host = Setting("DotNetHost");
    private bool verified;

    public string Root { get; }
    public string? SdkVersion { get; }
    public string DiagnosticsRoot { get; }
    public string PackageDirectory { get; } = Path.GetFullPath(Setting("PackageDirectory"));
    private string Cache => Path.Combine(Root, "cache");

    public DotNet() : this(OptionalVersion("SdkVersion"), OptionalVersion("WorkloadVersion"), "package-output")
    {
    }

    internal static DotNet ForSamples() =>
        new(OptionalVersion("SdkVersion"), OptionalVersion("WorkloadVersion"), "samples");

    internal DotNet(string? sdkVersion, string? workloadVersion, string label)
    {
        if (workloadVersion is not null && sdkVersion is null)
            throw new InvalidOperationException("SampleTest.WorkloadVersion requires SampleTest.SdkVersion");

        var repository = Path.GetFullPath(Setting("RepositoryDirectory"));
        var repoGlobal = Path.Combine(repository, "global.json");
        if (!File.Exists(repoGlobal))
            throw new FileNotFoundException("Repository global.json is missing", repoGlobal);

        SdkVersion = sdkVersion;
        var id = $"{label}-{Guid.NewGuid():N}";
        Root = Path.Combine(repository, "output", "samples-test-workspaces", id);
        DiagnosticsRoot = Path.Combine(Path.GetFullPath(Setting("ArtifactsDirectory")), id);
        Directory.CreateDirectory(Root);
        if (!Directory.Exists(PackageDirectory) || !Directory.EnumerateFiles(PackageDirectory, "*.nupkg", SearchOption.AllDirectories).Any())
            throw new InvalidOperationException($"Input NuGet packages are missing: {PackageDirectory}");
        File.WriteAllText(Path.Combine(Root, "Directory.Build.props"), "<Project />");
        File.WriteAllText(Path.Combine(Root, "Directory.Build.targets"), "<Project />");
        if (sdkVersion is not null)
        {
            var config = JsonNode.Parse(File.ReadAllText(repoGlobal)) as JsonObject
                ?? throw new InvalidOperationException($"Invalid repository global.json: {repoGlobal}");
            var sdk = config["sdk"] as JsonObject
                ?? throw new InvalidOperationException($"Missing sdk object in {repoGlobal}");
            sdk["version"] = sdkVersion;
            sdk["rollForward"] = "disable";
            sdk["allowPrerelease"] = true;
            if (workloadVersion is not null)
                sdk["workloadVersion"] = workloadVersion;
            else
                sdk.Remove("workloadVersion");
            if (sdk["paths"] is JsonArray paths)
                for (var i = 0; i < paths.Count; i++)
                {
                    var path = paths[i]?.GetValue<string>();
                    if (path is not null && path != "$host$" && !Path.IsPathFullyQualified(path))
                        paths[i] = Path.GetFullPath(Path.Combine(repository, path));
                }
            File.WriteAllText(Path.Combine(Root, "global.json"), config.ToJsonString());
        }
        WriteNuGetConfig(Path.Combine(Root, "NuGet.Config"), PackageDirectory);
    }

    public static (string Previous, string Current) ConsumerFrameworks()
    {
        var current = Setting("ConsumerTargetFramework");
        var match = Regex.Match(current, @"^net([0-9]+)\.0$");
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var major) || major < 6)
            throw new InvalidOperationException($"SampleTest.ConsumerTargetFramework must be a regular .NET TFM newer than net5.0: '{current}'");
        return ($"net{major - 1}.0", current);
    }

    private static string? OptionalVersion(string name)
    {
        var version = AppContext.GetData("SampleTest." + name) as string;
        if (version is null or "")
            return null;
        if (!Regex.IsMatch(version, @"^[0-9]+\.[0-9]+\.[0-9]+(?:\.[0-9]+)?(?:-[0-9A-Za-z][0-9A-Za-z.-]*)?\z"))
            throw new InvalidOperationException($"SampleTest.{name} must be a full exact version: '{version}'");
        return version;
    }

    public static void WriteNuGetConfig(string path, string packages)
    {
        new XDocument(new XElement("configuration",
            new XElement("packageSources",
                new XElement("clear"),
                new XElement("add", new XAttribute("key", "artifacts"), new XAttribute("value", packages)),
                new XElement("add", new XAttribute("key", "dotnet-public"),
                    new XAttribute("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json")),
                new XElement("add", new XAttribute("key", "dotnet-eng"),
                    new XAttribute("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json"))),
            new XElement("packageSourceMapping",
                new XElement("clear"),
                new XElement("packageSource", new XAttribute("key", "artifacts"),
                    new XElement("package", new XAttribute("pattern", "SkiaSharp*")),
                    new XElement("package", new XAttribute("pattern", "HarfBuzzSharp*"))),
                new XElement("packageSource", new XAttribute("key", "dotnet-public"),
                    new XElement("package", new XAttribute("pattern", "*"))),
                new XElement("packageSource", new XAttribute("key", "dotnet-eng"),
                    new XElement("package", new XAttribute("pattern", "*"))))))
            .Save(path);
    }

    public string NewProject(string name, string xml)
    {
        var directory = Path.Combine(Root, name);
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "Consumer.csproj"), xml);
        File.WriteAllText(Path.Combine(directory, "Program.cs"), "System.Console.WriteLine(\"Package consumer\");");
        return directory;
    }

    public Task Build(string directory, string? framework = null, string? outputDirectory = "output") =>
        Run(directory, "build", framework: framework, outputDirectory: outputDirectory);

    public Task Publish(string directory, string? rid = null, string? framework = null,
        string? outputDirectory = "output", string? diagnosticLabel = null) =>
        Run(directory, "publish", rid, framework, outputDirectory, diagnosticLabel);

    private async Task Run(string directory, string command, string? rid = null, string? framework = null,
        string? outputDirectory = "output", string? diagnosticLabel = null)
    {
        await VerifySdk(directory);
        var diagnostics = Path.Combine(DiagnosticsRoot, Path.GetFileName(directory), diagnosticLabel ?? command);
        Directory.CreateDirectory(diagnostics);
        var arguments = new List<string> { command, "Consumer.csproj", "-c", "Release",
            "--nologo", "-v:minimal", $"-bl:{Path.Combine(diagnostics, "build.binlog")}",
            $"-p:RestoreConfigFile={Path.Combine(Root, "NuGet.Config")}" };
        if (outputDirectory is not null)
        {
            arguments.Add("-o");
            arguments.Add(outputDirectory);
        }
        if (framework is not null)
        {
            arguments.Add("--framework");
            arguments.Add(framework);
        }
        if (rid is not null)
        {
            arguments.Add("-r");
            arguments.Add(rid);
        }
        var (exitCode, stdout, stderr) = await RunProcess(host, arguments, directory, TimeSpan.FromMinutes(5),
            TestContext.Current.TestOutputHelper, Configure);
        if (exitCode != 0)
            throw new InvalidOperationException($"dotnet {command} failed ({exitCode}); see {diagnostics}\n{stdout}\n{stderr}");
    }

    public async Task VerifySdk(string directory)
    {
        if (verified)
            return;
        var (exitCode, stdout, stderr) = await RunProcess(host, ["--version"], directory, TimeSpan.FromMinutes(1),
            TestContext.Current.TestOutputHelper, Configure);
        var selected = stdout.Trim();
        TestContext.Current.TestOutputHelper?.WriteLine($"Selected SDK: {selected}");
        if (exitCode != 0 || (SdkVersion is not null && selected != SdkVersion))
            throw new InvalidOperationException($"Expected SDK {SdkVersion ?? "from repository global.json"}, selected {selected} (exit {exitCode}): {stderr}");
        verified = true;
    }

    public async Task BuildSample(string solution, string diagnostics, ITestOutputHelper output)
    {
        if (!File.Exists(solution))
            throw new FileNotFoundException("Selected sample solution is missing", solution);
        await VerifySdk(Path.GetDirectoryName(solution)!);
        Directory.CreateDirectory(diagnostics);
        var arguments = new[] { "build", solution, "-c", Setting("Configuration"), "--nologo", "-v:minimal",
            $"-bl:{Path.Combine(diagnostics, "build.binlog")}",
            $"-p:RestoreConfigFile={Path.Combine(Root, "NuGet.Config")}", "-p:RestoreNoCache=true" };
        var (exitCode, stdout, stderr) = await RunProcess(host, arguments, Path.GetDirectoryName(solution)!,
            TimeSpan.FromMinutes(30), output, Configure);
        if (exitCode != 0)
            throw new InvalidOperationException($"Sample {solution} with SDK {SdkVersion} failed ({exitCode}); see {diagnostics}\n{stdout}\n{stderr}");
    }

    public async Task RunConsoleSample(string solution, string image, ITestOutputHelper output)
    {
        var project = Path.Combine(Path.GetDirectoryName(solution)!, "SkiaSharpSample", "SkiaSharpSample.csproj");
        if (!File.Exists(project))
            throw new FileNotFoundException("Console sample project is missing", project);
        await VerifySdk(Path.GetDirectoryName(project)!);
        var args = new[] { "run", "--no-build", "--no-restore", "--project", project,
            "-c", Setting("Configuration"), "--", "SkiaSharp", "--output", image };
        var (exitCode, stdout, stderr) = await RunProcess(host, args, Path.GetDirectoryName(project)!,
            TimeSpan.FromMinutes(5), output, Configure);
        if (exitCode != 0)
            throw new InvalidOperationException($"Console sample failed ({exitCode}):\n{stdout}\n{stderr}");
    }

    internal static async Task<(int ExitCode, string Output, string Error)> RunProcess(string executable,
        IEnumerable<string> arguments, string directory, TimeSpan timeout, ITestOutputHelper? output = null,
        Action<ProcessStartInfo>? configure = null)
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
        configure?.Invoke(start);
        output?.WriteLine($"{executable} {string.Join(" ", start.ArgumentList.Select(arg => $"\"{arg}\""))}");
        using var process = Process.Start(start) ?? throw new InvalidOperationException($"Could not start {executable}");
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        using var cancellation = new CancellationTokenSource(timeout);
        try
        {
            await process.WaitForExitAsync(cancellation.Token);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
            await process.WaitForExitAsync();
            throw new TimeoutException($"{executable} timed out after {timeout}: {directory}\n{await stdout}\n{await stderr}");
        }
        var text = await stdout;
        var errors = await stderr;
        output?.WriteLine(text);
        if (errors.Length > 0)
            output?.WriteLine(errors);
        return (process.ExitCode, text, errors);
    }

    private void Configure(ProcessStartInfo start)
    {
        foreach (var key in start.Environment.Keys.Where(k =>
            k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("Restore", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("NUGET_", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("NuGetPackageRoot", StringComparison.OrdinalIgnoreCase)).ToArray())
            start.Environment.Remove(key);
        start.Environment["NUGET_PACKAGES"] = Path.Combine(Cache, "packages");
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(Cache, "http");
        start.Environment["NUGET_SCRATCH"] = Path.Combine(Cache, "scratch");
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(Cache, "home");
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
    }

    public void CleanBuildOutput(string directory, params (string Framework, string OutputDirectory)[] outputs)
    {
        File.Copy(Path.Combine(directory, "obj", "project.assets.json"), Path.Combine(directory, "project.assets.json"));
        if (outputs.Length == 0)
            outputs = [("", "output")];
        foreach (var (framework, outputDirectory) in outputs)
        {
            var diagnostics = Path.Combine(directory, framework);
            Directory.CreateDirectory(diagnostics);
            if (framework.Length > 0)
                File.Copy(Path.Combine(directory, "project.assets.json"), Path.Combine(diagnostics, "project.assets.json"));
            foreach (var name in new[] { "Consumer.deps.json", "Consumer.runtimeconfig.json" })
                File.Copy(Path.Combine(directory, outputDirectory, name), Path.Combine(diagnostics, name));
        }
        foreach (var name in new[] { "bin", "obj", "output" })
        {
            var path = Path.Combine(directory, name);
            if (Directory.Exists(path))
                Directory.Delete(path, recursive: true);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(Cache))
            Directory.Delete(Cache, recursive: true);
    }

    public static string Setting(string name) =>
        AppContext.GetData("SampleTest." + name) as string is { Length: > 0 } value
            ? value : throw new InvalidOperationException($"SampleTest.{name} runtime configuration is missing");
}
