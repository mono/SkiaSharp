using System.Runtime.InteropServices;
using SkiaSharp.Tests.Shared;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class SampleImage
{
    internal static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(2);

    internal static string Root => Path.Combine(AppContext.BaseDirectory, "Goldens");
    internal static GoldenStore Store { get; } = new(new[] { Root });

    internal static Golden Host(string casePath) =>
        Store.Resolve(
            SampleLookup.HostPlatform.ToLowerInvariant(),
            casePath,
            architecture: RuntimeInformation.ProcessArchitecture.ToString().ToLowerInvariant());

    internal static Golden Container(string casePath) =>
        Store.Resolve(
            OperatingSystem.IsWindows() ? "docker-windows" : "docker-linux",
            casePath);
}
