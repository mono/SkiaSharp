using System.Net;
using System.Net.Sockets;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[RunGroup(SampleRunGroup.Build)]
public sealed class AppiumInfrastructureTests
{
    [Fact]
    public async Task MissingInstalledAndroidImageFailsBeforeDeviceCreation()
    {
        var sdk = Path.Combine(Path.GetTempPath(), $"appium-sdk-{Guid.NewGuid():N}");
        try
        {
            Directory.CreateDirectory(Path.Combine(sdk, "platform-tools"));
            File.WriteAllText(Path.Combine(sdk, "platform-tools",
                OperatingSystem.IsWindows() ? "adb.exe" : "adb"), "");
            var android = new Android(sdk);
            var error = await Assert.ThrowsAsync<DirectoryNotFoundException>(() => android.CreateAvd(AndroidDevice.PixelApi26));
            Assert.Contains("Tests never download images", error.Message);
        }
        finally
        {
            if (Directory.Exists(sdk))
                SampleWorkspace.DeleteDirectory(sdk);
        }
    }

    [Fact]
    public void DeviceProfilesKeepExplicitLegacyAndCurrentPorts()
    {
        Assert.Equal(5558, AndroidDevice.PixelApi26.Validate());
        Assert.Equal(5560, AndroidDevice.PixelApi37.Validate());
        Assert.Equal(26, AndroidDevice.PixelApi26.Api);
        Assert.Equal(37, AndroidDevice.PixelApi37.Api);
        IosDevice.Iphone16ProIos18_6.Validate();
        IosDevice.Iphone16Pro.Validate();
        Assert.Throws<ArgumentException>(() => (AndroidDevice.PixelApi26 with { Serial = "emulator-5557" }).Validate());
        Assert.Throws<ArgumentException>(() => (AndroidDevice.PixelApi26 with { Name = "Pixel_*" }).Validate());
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
