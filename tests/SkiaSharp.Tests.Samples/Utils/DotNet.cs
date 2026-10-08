using System.Xml.Linq;

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

    public static async Task BuildSample(SampleWorkspace workspace, string solution, string diagnostics, string configuration = "Release")
    {
        if (!File.Exists(solution))
            throw new FileNotFoundException("Selected sample solution is missing", solution);

        Directory.CreateDirectory(diagnostics);
        var directory = Path.GetDirectoryName(solution)!;
        var selected = await ProcessRunner.Run("dotnet", ["--version"], directory, TimeSpan.FromMinutes(1), workspace.ConfigureProcess);
        if (selected.ExitCode != 0)
            throw new InvalidOperationException($"The dotnet host on PATH could not select an SDK: {selected.Output}\n{selected.Error}");

        string[] arguments =
        [
            "build",
            solution,
            "-c", configuration,
            "--nologo",
            "-v:minimal",
            $"-bl:{Path.Combine(diagnostics, "build.binlog")}",
            $"-p:RestoreConfigFile={Path.Combine(workspace.Root, "NuGet.Config")}",
            "-p:RestoreNoCache=true",
            "-p:UseSharedCompilation=false"
        ];
        var result = await ProcessRunner.Run("dotnet", arguments, directory, TimeSpan.FromMinutes(30), workspace.ConfigureProcess);

        var log = Path.Combine(diagnostics, "build.log");
        await File.WriteAllTextAsync(log, result.Output + result.Error);
        SampleArtifacts.AttachFile(log, "text/plain");

        var binlog = Path.Combine(diagnostics, "build.binlog");
        if (File.Exists(binlog))
            SampleArtifacts.AttachFile(binlog, "application/octet-stream");

        if (result.ExitCode != 0)
            throw new InvalidOperationException($"Sample {solution} failed ({result.ExitCode}); see {diagnostics}\n{result.Output}\n{result.Error}");
    }
}
