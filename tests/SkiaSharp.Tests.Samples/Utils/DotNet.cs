using System.Diagnostics;
using System.Xml.Linq;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

// Supplies dotnet build commands and their NuGet restore configuration.
public static class DotNet
{
    public static void WriteNuGetConfig(string path, string packagesDir) =>
        new XDocument(new XElement("configuration",
            new XElement("packageSources",
                new XElement("clear"),
                new XElement("add", new XAttribute("key", "artifacts"), new XAttribute("value", packagesDir)),
                new XElement("add", new XAttribute("key", "dotnet-public"), new XAttribute("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-public/nuget/v3/index.json")),
                new XElement("add", new XAttribute("key", "dotnet-eng"), new XAttribute("value", "https://pkgs.dev.azure.com/dnceng/public/_packaging/dotnet-eng/nuget/v3/index.json"))),
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

    public static async Task<string> GetVersion(SampleWorkspace workspace)
    {
        var result = await Run(workspace, ["--version"]);
        if (result.ExitCode != 0)
            throw new InvalidOperationException($"The dotnet host on PATH could not select an SDK: {result.Output}\n{result.Error}");
        var selected = result.Output.Trim();
        TestContext.Current.TestOutputHelper?.WriteLine($"Consumer SDK: {selected}; runner runtime: {Environment.Version}");
        if (workspace.SdkVersion is not null && selected != workspace.SdkVersion)
            throw new InvalidOperationException($"Expected consumer SDK {workspace.SdkVersion}, selected {selected}.");
        return selected;
    }

    public static async Task Build(SampleWorkspace workspace, string project, string diagnostics, string configuration = "Release")
    {
        project = Path.GetFullPath(project);
        diagnostics = Path.GetFullPath(diagnostics);
        Directory.CreateDirectory(diagnostics);
        var stdoutLog = Path.Combine(diagnostics, "build.stdout.log");
        var stderrLog = Path.Combine(diagnostics, "build.stderr.log");
        var binlog = Path.Combine(diagnostics, "build.binlog");

        await File.WriteAllTextAsync(stdoutLog, "");
        await File.WriteAllTextAsync(stderrLog, "");
        try
        {
            string[] arguments =
            [
                "build",
                project,
                "-c", configuration,
                "--nologo",
                "-v:minimal",
                $"-bl:{binlog}",
                $"-p:RestoreConfigFile={Path.Combine(workspace.Root, "NuGet.Config")}",
                "-p:RestoreNoCache=true",
                "-p:UseSharedCompilation=false"
            ];
            var result = await Run(workspace, arguments, TimeSpan.FromMinutes(30), Path.GetDirectoryName(project));
            await File.WriteAllTextAsync(stdoutLog, result.Output);
            await File.WriteAllTextAsync(stderrLog, result.Error);
            if (result.ExitCode != 0)
                throw new InvalidOperationException($"dotnet build {project} failed ({result.ExitCode}); see {diagnostics}");
        }
        catch (Exception error)
        {
            var failureLog = Path.Combine(diagnostics, "build.failure.log");
            await File.WriteAllTextAsync(failureLog, error.ToString());
            TestContext.Current.AddFileAttachment(failureLog, "text/plain");
            throw;
        }
        finally
        {
            TestContext.Current.AddFileAttachment(stdoutLog, "text/plain");
            TestContext.Current.AddFileAttachment(stderrLog, "text/plain");
            if (File.Exists(binlog))
                TestContext.Current.AddFileAttachment(binlog, "application/octet-stream");
        }
    }

    internal static Task<(int ExitCode, string Output, string Error)> Run(
        SampleWorkspace workspace,
        IEnumerable<string> arguments,
        TimeSpan? timeout = null,
        string? workingDir = null) =>
        ProcessRunner.Run(
            "dotnet",
            arguments,
            workingDir ?? workspace.Root,
            timeout ?? TimeSpan.FromMinutes(1),
            start => ConfigureProcess(start, workspace));

    internal static async Task Run(
        SampleWorkspace workspace,
        string project,
        string configuration,
        IEnumerable<string> arguments,
        Func<DotNetRunningApp, Task> test)
    {
        var diagnostics = Path.Combine(workspace.DiagnosticsRoot, "run");
        Directory.CreateDirectory(diagnostics);

        using var process = ProcessRunner.Start("dotnet",
            ["run", "--project", project, "-c", configuration, "--no-build", "--no-launch-profile", "--", .. arguments],
            Path.GetDirectoryName(project)!,
            start =>
            {
                ConfigureProcess(start, workspace);
                start.Environment.Remove("ASPNETCORE_HTTPS_PORT");
                start.Environment.Remove("ASPNETCORE_HTTPS_PORTS");
                start.Environment["ASPNETCORE_ENVIRONMENT"] = "Development";
                start.Environment["Logging__LogLevel__Microsoft.Hosting.Lifetime"] = "Information";
            });

        var app = new DotNetRunningApp(process, diagnostics);
        await app.Run(() => test(app));
    }

    internal static void ConfigureProcess(ProcessStartInfo start, SampleWorkspace workspace)
    {
        // Keep the host PATH, but replace inherited build settings with private caches.
        foreach (var key in start.Environment.Keys.Where(k =>
            k.StartsWith("DOTNET_", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("MSBUILD", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("Restore", StringComparison.OrdinalIgnoreCase) ||
            k.StartsWith("NUGET_", StringComparison.OrdinalIgnoreCase) ||
            k.Equals("NuGetPackageRoot", StringComparison.OrdinalIgnoreCase)).ToArray())
            start.Environment.Remove(key);
        var cache = Path.Combine(workspace.Root, "cache");
        start.Environment["NUGET_PACKAGES"] = Path.Combine(cache, "packages");
        start.Environment["NUGET_HTTP_CACHE_PATH"] = Path.Combine(cache, "http");
        start.Environment["NUGET_SCRATCH"] = Path.Combine(cache, "scratch");
        start.Environment["DOTNET_CLI_HOME"] = Path.Combine(cache, "home");
        start.Environment["DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE"] = "true";
        start.Environment["MSBUILDDISABLENODEREUSE"] = "1";
        start.Environment["DOTNET_CLI_USE_MSBUILD_SERVER"] = "0";
    }

}
