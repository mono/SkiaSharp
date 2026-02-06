using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp and HarfBuzzSharp packages work in Linux containers via Docker.
/// </summary>
[Trait("Category", "Platform")]
public class LinuxConsoleTests(ITestOutputHelper output) : PlatformTestBase(output)
{
    private static bool IsDockerAvailable()
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo("docker", "info")
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false
            };
            using var process = System.Diagnostics.Process.Start(psi)!;
            process.WaitForExit(10000);
            return process.ExitCode == 0;
        }
        catch
        {
            return false;
        }
    }

    [SkippableFact]
    public async Task SkiaSharpRunsOnLinux()
    {
        Skip.IfNot(IsDockerAvailable(), "Docker is not available");

        Output.WriteLine($"Testing SkiaSharp {SkiaVersion} in Linux Docker container");

        var drawCode = TestImage.GetDrawCode();
        var programCs = $$"""
            using SkiaSharp;
            
            using var bitmap = new SKBitmap({{TestImage.Width}}, {{TestImage.Height}});
            using var canvas = new SKCanvas(bitmap);
            
            {{drawCode}}
            
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite("/app/output.png");
            data.SaveTo(stream);
            Console.WriteLine("SUCCESS");
            """;

        var outputPath = Path.Combine(TestDir, "output.png");
        await RunInDocker("skiasharp-linux-test", programCs, outputPath,
            ("SkiaSharp", SkiaVersion),
            ("SkiaSharp.NativeAssets.Linux.NoDependencies", SkiaVersion));

        Assert.True(File.Exists(outputPath), "Output PNG should exist");
        var actualImage = await File.ReadAllBytesAsync(outputPath);
        await VerifyScreenshot(actualImage, "linux-console-skiasharp");
    }

    [SkippableFact]
    public async Task HarfBuzzSharpRunsOnLinux()
    {
        Skip.IfNot(IsDockerAvailable(), "Docker is not available");

        Output.WriteLine($"Testing HarfBuzzSharp {HarfBuzzVersion} in Linux Docker container");

        var programCs = $$"""
            using SkiaSharp;
            using HarfBuzzSharp;
            
            // Shape text with HarfBuzz
            {{TestText.GetShapeCode()}}
            {{TestText.GetOutputCode()}}
            
            // Render to image
            using var bitmap = new SKBitmap(400, 100);
            using var canvas = new SKCanvas(bitmap);
            canvas.Clear(SKColors.White);
            
            using var paint = new SKPaint { Color = SKColors.Black, IsAntialias = true };
            using var font = new SKFont(SKTypeface.Default, 32);
            canvas.DrawText("{{TestText.SampleText}}", 20, 60, SKTextAlign.Left, font, paint);
            
            using var image = SKImage.FromBitmap(bitmap);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);
            using var stream = File.OpenWrite("/app/output.png");
            data.SaveTo(stream);
            Console.WriteLine("SUCCESS");
            """;

        var outputPath = Path.Combine(TestDir, "output.png");
        var result = await RunInDocker("harfbuzz-linux-test", programCs, outputPath,
            ("SkiaSharp", SkiaVersion),
            ("SkiaSharp.NativeAssets.Linux.NoDependencies", SkiaVersion),
            ("HarfBuzzSharp", HarfBuzzVersion),
            ("HarfBuzzSharp.NativeAssets.Linux", HarfBuzzVersion));

        Assert.True(TestText.ValidateOutput(result), "HarfBuzz output validation failed");
        Assert.True(File.Exists(outputPath), "Output PNG should exist");

        var actualImage = await File.ReadAllBytesAsync(outputPath);
        await SaveScreenshot(actualImage, "linux-console-harfbuzzsharp");
        Assert.True(actualImage.Length > 200, "Screenshot should have content");
    }

    private async Task<string> RunInDocker(string projectName, string programCs, string outputPath,
        params (string Name, string Version)[] packages)
    {
        // Create project files in TestDir
        var projectDir = Path.Combine(TestDir, projectName);
        Directory.CreateDirectory(projectDir);

        File.WriteAllText(Path.Combine(projectDir, "Program.cs"), programCs);

        // Build csproj with package references
        var packageRefs = string.Join("\n    ",
            packages.Select(p => $"<PackageReference Include=\"{p.Name}\" Version=\"{p.Version}\" />"));

        File.WriteAllText(Path.Combine(projectDir, $"{projectName}.csproj"), $"""
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net8.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
              </PropertyGroup>
              <ItemGroup>
                {packageRefs}
              </ItemGroup>
            </Project>
            """);

        File.WriteAllText(Path.Combine(projectDir, "nuget.config"), """
            <?xml version="1.0" encoding="utf-8"?>
            <configuration>
              <packageSources>
                <clear />
                <add key="SkiaSharp Preview" value="https://aka.ms/skiasharp-eap/index.json" />
                <add key="NuGet.org" value="https://api.nuget.org/v3/index.json" />
              </packageSources>
            </configuration>
            """);

        // Build and run in Docker using dotnet publish (resolves RID-specific native assets)
        var dockerfile = Path.Combine(projectDir, "Dockerfile");
        File.WriteAllText(dockerfile, $"""
            FROM mcr.microsoft.com/dotnet/sdk:8.0
            WORKDIR /src
            COPY {projectName}.csproj nuget.config ./
            RUN dotnet restore
            COPY Program.cs ./
            RUN dotnet publish -c Release --no-restore -o /app
            WORKDIR /app
            ENTRYPOINT ["dotnet", "{projectName}.dll"]
            """);

        var tag = $"skiasharp-test-{projectName}".ToLowerInvariant();

        // Build Docker image
        Output.WriteLine("Building Docker image...");
        await Run("docker", $"build -t {tag} {projectDir}", timeoutSeconds: 180);

        // Run container with a dedicated output directory mounted
        Output.WriteLine("Running in Docker container...");
        var mountDir = Path.Combine(TestDir, $"{projectName}-out");
        Directory.CreateDirectory(mountDir);
        var result = await Run("docker",
            $"run --rm --entrypoint sh -v {mountDir}:/out {tag} -c \"dotnet {projectName}.dll && cp /app/output.png /out/output.png\"",
            timeoutSeconds: 60);

        Output.WriteLine(result);

        // Copy output from mount dir to expected path
        var mountedOutput = Path.Combine(mountDir, "output.png");
        if (File.Exists(mountedOutput))
            File.Copy(mountedOutput, outputPath, overwrite: true);

        Output.WriteLine(result);

        // Clean up image
        try { await Run("docker", $"rmi {tag}", timeoutSeconds: 30); } catch { }

        return result;
    }
}
