using System.Text.Json;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class TestDevices
{
    internal const int AppiumPort = 4723;
    internal const string AndroidName = "Pixel_API_36";
    internal const string AndroidVersion = "16";
    internal const int AndroidApiLevel = 36;
    internal const string AndroidSerial = "emulator-5554";
    internal const string IosName = "iPhone 16 Pro";
    internal const string IosVersion = "26.2";

    internal static string RequireIosSimulator(string json, string name, string version)
    {
        using var devices = JsonDocument.Parse(json);
        var runtime = "com.apple.CoreSimulator.SimRuntime.iOS-" + version.Replace('.', '-');
        var matches = new List<string>();
        if (devices.RootElement.GetProperty("devices").TryGetProperty(runtime, out var entries))
            foreach (var device in entries.EnumerateArray())
                if (device.GetProperty("name").GetString() == name && device.GetProperty("isAvailable").GetBoolean())
                    matches.Add(device.GetProperty("udid").GetString()
                        ?? throw new InvalidOperationException($"Simulator '{name}' has no UDID."));
        if (matches.Count != 1)
            throw new InvalidOperationException(
                $"Required exactly one available '{name}' simulator on iOS {version}, found {matches.Count}. " +
                "Install that runtime and create the named simulator; no other device/version will be selected.");
        return matches[0];
    }
}
