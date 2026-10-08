using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Trait("Category", "Infrastructure")]
public class DeviceRequirementsTests
{
    private const string Devices = """
        {
          "devices": {
            "com.apple.CoreSimulator.SimRuntime.iOS-26-2": [
              { "name": "iPhone 16 Pro", "udid": "exact-id", "isAvailable": true },
              { "name": "Other phone", "udid": "other-id", "isAvailable": true }
            ],
            "com.apple.CoreSimulator.SimRuntime.iOS-26-5": [
              { "name": "iPhone 16 Pro", "udid": "newer-id", "isAvailable": true }
            ]
          }
        }
        """;

    [Fact]
    public void ResolvesOnlyTheDeclaredSimulatorAndRuntime() =>
        Assert.Equal("exact-id", TestDevices.RequireIosSimulator(Devices, "iPhone 16 Pro", "26.2"));

    [Theory]
    [InlineData("Missing phone", "26.2")]
    [InlineData("iPhone 16 Pro", "26.3")]
    public void MissingExactDeviceOrVersionFailsInsteadOfSelectingAnother(string name, string version) =>
        Assert.Throws<InvalidOperationException>(() => TestDevices.RequireIosSimulator(Devices, name, version));

    [Fact]
    public void UnavailableOrAmbiguousDevicesFail()
    {
        var unavailable = Devices.Replace("\"udid\": \"exact-id\", \"isAvailable\": true",
            "\"udid\": \"exact-id\", \"isAvailable\": false");
        Assert.Throws<InvalidOperationException>(() =>
            TestDevices.RequireIosSimulator(unavailable, "iPhone 16 Pro", "26.2"));
        var duplicate = Devices.Replace("\"name\": \"Other phone\"", "\"name\": \"iPhone 16 Pro\"");
        Assert.Throws<InvalidOperationException>(() =>
            TestDevices.RequireIosSimulator(duplicate, "iPhone 16 Pro", "26.2"));
    }

    [Fact]
    public void DeviceDesktopAndGoldenTestsHaveExplicitCategoryOptOuts()
    {
        foreach (var (type, category) in new[]
        {
            (typeof(MauiAndroidTests), "Device"),
            (typeof(MauiIosTests), "Device"),
            (typeof(MauiMacCatalystTests), "Desktop"),
            (typeof(MauiWindowsTests), "Desktop"),
            (typeof(HostGoldenTests), "Golden"),
            (typeof(ContainerGoldenTests), "Golden")
        })
            Assert.Contains(type.CustomAttributes, attribute =>
                attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[0].Value as string == "Category" &&
                attribute.ConstructorArguments[1].Value as string == category);
        Assert.Contains(typeof(BlazorViewTests).GetMethod(nameof(BlazorViewTests.MatchesGolden))!.CustomAttributes,
            attribute => attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[1].Value as string == "Golden");
        Assert.DoesNotContain(typeof(BlazorViewTests).GetMethod(nameof(BlazorViewTests.RendersCanvas))!.CustomAttributes,
            attribute => attribute.AttributeType == typeof(TraitAttribute) &&
                attribute.ConstructorArguments[1].Value as string == "Golden");
    }
}
