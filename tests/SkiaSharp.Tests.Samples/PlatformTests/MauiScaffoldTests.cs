using System.Xml.Linq;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

[Trait("Category", "Infrastructure")]
public class MauiScaffoldTests
{
    [Fact]
    public void NarrowTargetFrameworkRemovesConditionalTemplateTargets()
    {
        var projectDirectory = Path.Combine(DotNet.Setting("RepositoryDirectory"), "output",
            "samples-test-workspaces", $"maui-scaffold-test-{Guid.NewGuid():N}");
        Directory.CreateDirectory(projectDirectory);
        var projectPath = Path.Combine(projectDirectory, "Consumer.csproj");
        try
        {
            File.WriteAllText(projectPath, """
                <Project Sdk="Microsoft.NET.Sdk">
                  <PropertyGroup>
                    <TargetFrameworks>net10.0-android</TargetFrameworks>
                    <TargetFrameworks Condition="!$([MSBuild]::IsOSPlatform('linux'))">$(TargetFrameworks);net10.0-ios;net10.0-maccatalyst</TargetFrameworks>
                    <TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('windows'))">$(TargetFrameworks);net10.0-windows10.0.19041.0</TargetFrameworks>
                    <ApplicationId>com.example.consumer</ApplicationId>
                  </PropertyGroup>
                </Project>
                """);

            MauiTestBase.NarrowTargetFramework(projectPath, "net10.0-ios");

            var project = XDocument.Load(projectPath);
            Assert.Equal("net10.0-ios", Assert.Single(project.Descendants("TargetFramework")).Value);
            Assert.Empty(project.Descendants("TargetFrameworks"));
            Assert.Equal("com.example.consumer", Assert.Single(project.Descendants("ApplicationId")).Value);
        }
        finally
        {
            Directory.Delete(projectDirectory, recursive: true);
        }
    }
}
