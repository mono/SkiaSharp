using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleImage
{
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");
    internal static GoldenStore Store => new(new[] { Root });
    internal static GoldenVerifier Verifier => new(Store);

    internal static GoldenRequest Host(string casePath) =>
        new(SampleLookup.HostPlatform.ToLowerInvariant(), casePath, architecture:
            RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

    internal static GoldenRequest Container(string casePath) =>
        new(OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux", casePath);
}
