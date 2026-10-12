using System.Drawing;
using System.Net;
using System.Net.Sockets;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

public sealed class AppiumInfrastructureTests
{
    [Fact]
    public void DeviceProfilesKeepExplicitLegacyAndCurrentPorts()
    {
        Assert.Equal(5558, AndroidDevice.PixelApi24.Validate());
        Assert.Equal(5556, AndroidDevice.PixelApi37.Validate());
        Assert.Equal(24, AndroidDevice.PixelApi24.Api);
        Assert.Equal(37, AndroidDevice.PixelApi37.Api);
        IosDevice.Iphone16ProIos18_6.Validate();
        IosDevice.Iphone16Pro.Validate();
        Assert.Throws<ArgumentException>(() => (AndroidDevice.PixelApi24 with { Serial = "emulator-5557" }).Validate());
        Assert.Throws<ArgumentException>(() => (AndroidDevice.PixelApi24 with { Name = "Pixel_*" }).Validate());
        Assert.Throws<ArgumentException>(() => (IosDevice.Iphone16Pro with { Version = "latest" }).Validate());
    }

    [Fact]
    public void InteractionMustChangeTheSceneNotJustAnOverlay()
    {
        var root = Path.Combine(Path.GetTempPath(), $"appium-scene-{Guid.NewGuid():N}");
        Directory.CreateDirectory(root);
        try
        {
            var before = Path.Combine(root, "before.png");
            var after = Path.Combine(root, "after.png");
            using var bitmap = new SKBitmap(16, 16);
            bitmap.Erase(SKColors.White);
            Save(before);
            bitmap.SetPixel(15, 15, SKColors.Red);
            Save(after);
            var error = Assert.ThrowsAny<Exception>(() => AppiumRunningApp.AssertSceneChanged(before, after));
            Assert.Contains("rendered scene did not change", error.Message);
            bitmap.SetPixel(4, 4, SKColors.Blue);
            Save(after);
            AppiumRunningApp.AssertSceneChanged(before, after);

            void Save(string path)
            {
                using var image = SKImage.FromBitmap(bitmap);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);
                File.WriteAllBytes(path, data.ToArray());
            }
        }
        finally
        {
            SampleWorkspace.DeleteDirectory(root);
        }
    }

    [Fact]
    public void IosSelectionRequiresExactUniqueAvailableDevice()
    {
        const string json = """
            [
              {"name":"iPhone 16 Pro","isAvailable":true,"udid":"selected","state":"Shutdown",
               "runtime":{"name":"iOS 26.2","version":"26.2"}},
              {"name":"iPhone 17 Pro","isAvailable":true,"udid":"foreign","state":"Booted",
               "runtime":{"name":"iOS 26.2","version":"26.2"}}
            ]
            """;
        Assert.Equal(("selected", "Shutdown"), IosRunningApp.RequireSimulator(json, "iPhone 16 Pro", "26.2"));
        Assert.Throws<InvalidOperationException>(() => IosRunningApp.RequireSimulator(json, "iPhone 16 Pro", "26.3"));
        Assert.Throws<InvalidOperationException>(() => IosRunningApp.RequireSimulator(json, "iPhone 16", "26.2"));
        Assert.Throws<InvalidOperationException>(() => IosRunningApp.RequireSimulator(
            json.Replace("iPhone 17 Pro", "iPhone 16 Pro"), "iPhone 16 Pro", "26.2"));
        Assert.Throws<InvalidOperationException>(() => IosRunningApp.RequireSimulator(
            json.Replace("\"isAvailable\":true", "\"isAvailable\":false"), "iPhone 16 Pro", "26.2"));
    }

    [Theory]
    [InlineData(400, 800, 400, 800, 10, 20, 110, 220)]
    [InlineData(400, 800, 1200, 2400, 30, 60, 330, 660)]
    public void CanvasCropUsesMeasuredScreenshotScale(int width, int height, int pixelsX, int pixelsY,
        int left, int top, int right, int bottom) =>
        Assert.Equal(new SKRectI(left, top, right, bottom),
            AppiumRunningApp.ScreenshotBounds(new Point(10, 20), new Size(100, 200),
                new Size(width, height), new Size(pixelsX, pixelsY)));

    [Fact]
    public void CanvasCropRejectsOffscreenElements() =>
        Assert.Throws<InvalidOperationException>(() => AppiumRunningApp.ScreenshotBounds(
            new Point(-1, 0), new Size(100, 100), new Size(400, 800), new Size(1200, 2400)));

    [Fact]
    public void AppiumDoesNotReuseAnOccupiedPort()
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        try
        {
            var port = ((IPEndPoint)listener.LocalEndpoint).Port;
            Assert.Throws<InvalidOperationException>(() => AppiumFixture.RequireFreePort(port));
            Assert.True(listener.Server.IsBound);
        }
        finally
        {
            listener.Stop();
        }
    }
}
