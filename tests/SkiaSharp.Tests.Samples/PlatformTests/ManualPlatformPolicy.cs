using Xunit;

namespace SkiaSharp.Tests.Samples.PlatformTests;

internal static class ManualPlatformPolicy
{
    internal static bool IsCI =>
        IsEnabled(Environment.GetEnvironmentVariable("TF_BUILD")) ||
        IsEnabled(Environment.GetEnvironmentVariable("GITHUB_ACTIONS")) ||
        IsEnabled(Environment.GetEnvironmentVariable("CI"));

    internal static bool IsEnabled(string? value) =>
        !string.IsNullOrWhiteSpace(value) &&
        !value.Equals("false", StringComparison.OrdinalIgnoreCase) &&
        value != "0";

    internal static void RequireLocalOptIn()
    {
        if (IsCI)
            Assert.Skip("Manual platform/browser tests are not run in CI.");
        if (!IsEnabled(Environment.GetEnvironmentVariable("SKIASHARP_RUN_MANUAL_PLATFORM_TESTS")))
            Assert.Skip("Set SKIASHARP_RUN_MANUAL_PLATFORM_TESTS=1 to run local platform/browser tests.");
    }
}
