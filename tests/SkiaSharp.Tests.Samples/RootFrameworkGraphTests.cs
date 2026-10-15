using System.Text.Json;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
public sealed class RootFrameworkGraphTests
{
    [Theory]
    [InlineData("<TargetFramework>net10.0</TargetFramework>", new[] { "net10.0" })]
    [InlineData("<TargetFrameworks>net10.0;net8.0</TargetFrameworks>", new[] { "net10.0", "net8.0" })]
    public async Task FrameworkSelectionLeavesRootAndLibraryRestoreDeclarationsUnchanged(
        string declaration, string[] declaredFrameworks)
    {
        var directory = Path.Combine(Repo.ArtifactsDir, SampleWorkspace.CreateIdentity("sample-graph"));
        try
        {
            var source = Path.Combine(directory, "inputs", "App");
            Directory.CreateDirectory(Path.Combine(source, "Library"));
            File.WriteAllText(Path.Combine(source, "App.csproj"), $"""
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>{declaration}</PropertyGroup>
                  <ItemGroup><ProjectReference Include="Library/Library.csproj" /></ItemGroup>
                </Project>
                """);
            File.WriteAllText(Path.Combine(source, "Library", "Library.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><TargetFrameworks>net10.0;netstandard2.1</TargetFrameworks></PropertyGroup>
                </Project>
                """);
            using var workspace = new SampleWorkspace("App", Path.Combine(directory, "inputs"), Repo.PackagesDir,
                Path.Combine(directory, "workspaces"), Path.Combine(directory, "diagnostics"));
            var project = Path.Combine(workspace.Root, "samples", "App", "App.csproj");
            var library = Path.Combine(workspace.Root, "samples", "App", "Library", "Library.csproj");
            var originalProject = File.ReadAllText(project);
            var originalLibrary = File.ReadAllText(library);
            var config = Path.Combine(workspace.Root, "graph.nuget.config");
            File.WriteAllText(config, "<configuration><packageSources><clear /></packageSources></configuration>");
            DotNet.BuildArguments(workspace, project, workspace.DiagnosticsRoot, new() { Framework = "net10.0" });
            var graph = Path.Combine(workspace.DiagnosticsRoot, "graph.json");
            var result = await Evaluate(graph);
            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            TestContext.Current.AddFileAttachment(graph, "application/json");
            using var document = JsonDocument.Parse(File.ReadAllText(graph));
            var projects = document.RootElement.GetProperty("projects");
            Assert.Equal(2, projects.EnumerateObject().Count());
            Assert.Equal(declaredFrameworks, Frameworks(project));
            Assert.Equal(["net10.0", "netstandard2.1"],
                Frameworks(library));

            foreach (var framework in new[] { "net11.0", "net10.0-wrongplatform" })
            {
                var invalid = Assert.Throws<ArgumentException>(() =>
                    DotNet.BuildArguments(workspace, project, workspace.DiagnosticsRoot, new() { Framework = framework }));
                Assert.Contains("not declared", invalid.Message);
                Assert.Contains("net10.0", invalid.Message);
            }

            Assert.Equal(originalProject, File.ReadAllText(project));
            Assert.Equal(originalLibrary, File.ReadAllText(library));
            Assert.Equal(originalProject, File.ReadAllText(Path.Combine(source, "App.csproj")));
            Assert.Equal(originalLibrary, File.ReadAllText(Path.Combine(source, "Library", "Library.csproj")));

            string[] Frameworks(string path)
            {
                var selected = Assert.Single(projects.EnumerateObject(),
                    item => Path.GetFileName(item.Name) == Path.GetFileName(path));
                return selected.Value.GetProperty("restore").GetProperty("originalTargetFrameworks")
                    .EnumerateArray().Select(item => item.GetString()
                        ?? throw new InvalidOperationException("Graph framework is missing.")).ToArray();
            }

            Task<(int ExitCode, string Output, string Error)> Evaluate(string output) =>
                DotNet.Execute(workspace,
                    ["msbuild", project, "-t:GenerateRestoreGraphFile", "-nologo", "-v:quiet",
                        $"-p:RestoreGraphOutputPath={output}", $"-p:RestoreConfigFile={config}"],
                    new DotNetOptions { Timeout = TimeSpan.FromMinutes(2) });
        }
        finally
        {
            if (Directory.Exists(directory))
                SampleWorkspace.DeleteDirectory(directory);
        }
    }
}
