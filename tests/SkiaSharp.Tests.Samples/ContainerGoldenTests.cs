using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Golden")]
[Trait("Category", "Docker")]
public class ContainerGoldenTests(DockerSampleFixture docker, ITestOutputHelper output) : IClassFixture<DockerSampleFixture>
{
    [Fact]
    public async Task LinuxConsumerMatchesTheBaseReferenceImage()
    {
        Assert.SkipWhen(OperatingSystem.IsWindows(), "The Linux golden probe requires the host's Linux Docker recipe.");

        using var workspace = new DotNet();
        var version = DotNet.Setting("SkiaSharpVersion");
        var project = workspace.NewProject("container-golden", $$"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net10.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                <PackageReference Include="SkiaSharp" Version="{{version}}" />
                <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="{{version}}" />
              </ItemGroup>
            </Project>
            """);
        await File.WriteAllTextAsync(Path.Combine(project, "Program.cs"), $$"""
            using SkiaSharp;

            using var bitmap = new SKBitmap({{TestImage.Width}}, {{TestImage.Height}});
            using var canvas = new SKCanvas(bitmap);
            {{TestImage.GetDrawCode()}}
            using var image = SKImage.FromBitmap(bitmap);
            using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
            File.WriteAllBytes("/app/output.png", encoded.ToArray());
            Console.WriteLine("SUCCESS");
            """, TestContext.Current.CancellationToken);
        await File.WriteAllTextAsync(Path.Combine(project, "Dockerfile"), """
            FROM mcr.microsoft.com/dotnet/sdk:10.0
            WORKDIR /src
            COPY . .
            RUN dotnet publish Consumer.csproj -c Release -o /app -p:RestoreConfigFile=/src/nuget.config
            WORKDIR /app
            ENTRYPOINT ["dotnet", "Consumer.dll"]
            """, TestContext.Current.CancellationToken);

        var tag = await docker.Image(project, "Dockerfile", output);
        var name = $"skiasharp-sample-test-{Guid.NewGuid():N}";
        var screenshots = Path.Combine(workspace.DiagnosticsRoot, "container-golden");
        Directory.CreateDirectory(screenshots);
        docker.OwnContainer(name);
        try
        {
            var result = await docker.Run(["run", "--name", name, tag], screenshots, TimeSpan.FromMinutes(3), output);
            Assert.Contains("SUCCESS", result);
            var image = Path.Combine(screenshots, "output.png");
            await docker.Run(["cp", $"{name}:/app/output.png", image], screenshots, TimeSpan.FromMinutes(1), output);
            await new ScreenshotVerifier(screenshots, output).VerifyScreenshot(
                await File.ReadAllBytesAsync(image, TestContext.Current.CancellationToken), "linux-console-skiasharp");
        }
        finally
        {
            await docker.RemoveContainer(name, output);
        }
    }
}
