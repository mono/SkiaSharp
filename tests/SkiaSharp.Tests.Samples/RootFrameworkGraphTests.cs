using System.Text.Json;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public sealed class RootFrameworkGraphTests
{
    [Fact]
    public async Task RootOnlySelectionKeepsDeclaredLibraryFrameworksWithoutRestoringPackages()
    {
        var directory = Path.Combine(Path.GetTempPath(), $"sample-graph-{Guid.NewGuid():N}");
        try
        {
            var source = Path.Combine(directory, "inputs", "App");
            Directory.CreateDirectory(Path.Combine(source, "Library"));
            File.WriteAllText(Path.Combine(source, "App.csproj"), """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup><TargetFrameworks>net10.0;net8.0</TargetFrameworks></PropertyGroup>
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
            var config = Path.Combine(workspace.Root, "graph.nuget.config");
            File.WriteAllText(config, "<configuration><packageSources><clear /></packageSources></configuration>");
            RootFrameworkSelection.Write(workspace, project, "net10.0", workspace.DiagnosticsRoot);
            var graph = Path.Combine(workspace.DiagnosticsRoot, "graph.json");
            var result = await Evaluate(project, "net10.0", graph);
            Assert.True(result.ExitCode == 0, result.Output + result.Error);
            TestContext.Current.AddFileAttachment(graph, "application/json");
            using var document = JsonDocument.Parse(File.ReadAllText(graph));
            var projects = document.RootElement.GetProperty("projects");
            Assert.Equal(2, projects.EnumerateObject().Count());
            Assert.Equal(["net10.0"], Frameworks(project));
            Assert.Equal(["net10.0", "netstandard2.1"],
                Frameworks(Path.Combine(workspace.Root, "samples", "App", "Library", "Library.csproj")));

            var invalid = await Evaluate(project, "net10.0-wrongplatform", Path.Combine(workspace.DiagnosticsRoot, "invalid.json"));
            Assert.NotEqual(0, invalid.ExitCode);
            Assert.Contains("not declared", invalid.Output + invalid.Error);

            var unselected = Path.Combine(workspace.DiagnosticsRoot, "unselected.json");
            var original = await Evaluate(Path.Combine(workspace.Root, "different.csproj"), "net10.0", unselected);
            Assert.True(original.ExitCode == 0, original.Output + original.Error);
            using var untouched = JsonDocument.Parse(File.ReadAllText(unselected));
            Assert.Equal(["net10.0", "net8.0"], untouched.RootElement.GetProperty("projects").GetProperty(project)
                .GetProperty("restore").GetProperty("originalTargetFrameworks").EnumerateArray()
                .Select(item => item.GetString() ?? throw new InvalidOperationException("Graph framework is missing.")).ToArray());

            string[] Frameworks(string path)
            {
                var selected = Assert.Single(projects.EnumerateObject(),
                    item => Path.GetFileName(item.Name) == Path.GetFileName(path));
                return selected.Value.GetProperty("restore").GetProperty("originalTargetFrameworks")
                    .EnumerateArray().Select(item => item.GetString()
                        ?? throw new InvalidOperationException("Graph framework is missing.")).ToArray();
            }

            Task<(int ExitCode, string Output, string Error)> Evaluate(string selectedRoot, string framework, string output) =>
                DotNet.Run(workspace,
                    ["msbuild", project, "-t:GenerateRestoreGraphFile", "-nologo", "-v:quiet",
                        $"-p:SampleRootProject={selectedRoot}", $"-p:SampleRootFramework={framework}",
                        $"-p:RestoreGraphOutputPath={output}", $"-p:RestoreConfigFile={config}"],
                    TimeSpan.FromMinutes(2));
        }
        finally
        {
            if (Directory.Exists(directory))
                SampleWorkspace.DeleteDirectory(directory);
        }
    }
}
