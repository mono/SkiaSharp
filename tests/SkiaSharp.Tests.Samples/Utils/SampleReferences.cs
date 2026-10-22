using System.Runtime.InteropServices;
using SkiaSharp.Tests.Goldens;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleReferences
{
    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");

    // Existing reviewed images are explicitly unversioned, just as before migration.
    internal static GoldenRequest Host(string caseName) =>
        new(SampleLookup.HostPlatform.ToLowerInvariant(), "base", caseName,
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant(),
            versionScope: GoldenVersionScope.Unversioned);

    internal static GoldenRequest Container(string caseName) =>
        new(OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux", "base", caseName, null,
            versionScope: GoldenVersionScope.Unversioned);

    internal static GoldenResolution Resolve(GoldenRequest request)
    {
        if (!Directory.Exists(Root))
            throw new DirectoryNotFoundException($"Missing packaged sample golden directory: {Root}");
        var keys = Directory.EnumerateFiles(Root, "*.png", SearchOption.AllDirectories)
            .Select(path => Path.GetRelativePath(Root, path).Replace('\\', '/'));
        var result = GoldenReference.Resolve(keys, request);
        var path = result.Key is null ? null : Path.Combine(Root, result.Key.Replace('/', Path.DirectorySeparatorChar));
        var resolution = new GoldenResolution(request, result.Key, path, result.Candidates);
        TestContext.Current.TestOutputHelper?.WriteLine(resolution.Describe());
        return resolution;
    }

    internal static string Require(GoldenResolution resolution, string actual)
    {
        if (resolution.Path is null)
            TestContext.Current.AddFileAttachment(actual, "image/png");
        Assert.True(resolution.Path is not null,
            $"Missing reviewed sample golden. {resolution.Describe()} Actual retained at {actual}.");
        return resolution.Path!;
    }

    internal static void Emit(GoldenResolution resolution, string actual)
    {
        var request = resolution.Request;
        var key = $"Samples/{request.Profile}/{request.Version}/{request.CaseName}" +
            (request.Variant is null ? "" : "." + request.Variant) +
            (request.Architecture is null ? "" : "." + request.Architecture) + ".png";
        Write("actual", actual);
        if (resolution.Path is { } reference)
            Write("reference", reference);
        var diff = Path.ChangeExtension(actual, ".diff.png");
        if (File.Exists(diff))
            Write("diff", diff);

        void Write(string role, string path)
        {
            using var image = SKImage.FromEncodedData(path)
                ?? throw new InvalidOperationException($"Cannot emit invalid PNG diagnostic: {path}");
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"##SKIA-GOLDEN## role={role} key={key} size={image.Width}x{image.Height} " +
                $"base64={Convert.ToBase64String(File.ReadAllBytes(path))}");
        }
    }
}
