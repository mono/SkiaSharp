using OpenQA.Selenium.Appium;
using OpenQA.Selenium.Appium.Android;
using OpenQA.Selenium.Appium.iOS;
using SkiaSharp.Tests.Samples.Utils;
using Xunit;

namespace SkiaSharp.Tests.Samples;

[Collection("Maui devices")]
[Trait("Category", "Device")]
[Trait("Category", "Golden")]
public sealed class MauiSampleTests(AppiumFixture appium) : SampleTestBase
{
    [Theory]
    [InlineData(false, MauiDevice.AndroidName, MauiDevice.AndroidVersion)]
    [InlineData(true, MauiDevice.IosName, MauiDevice.IosVersion)]
    public Task BasicMauiRendersAndDraws(bool ios, string deviceName, string version) =>
        Run("Basic/Maui", "SkiaSharpSample/SkiaSharpSample.csproj",
            "com.companyname.SkiaSharpSample", ios, deviceName, version, ui =>
            {
                var cpu = ui.CaptureStable("basic-canvas-cpu", "basic-cpu");
                MauiUi.AssertRendered(cpu);

                ui.NavigateBasic("Drawing");
                var empty = ui.CaptureStable("basic-canvas-drawing", "drawing-empty");
                ui.Drag(ui.Find("basic-canvas-drawing"), 0.25, 0.3, 0.7, 0.5);
                var drawn = ui.CaptureStable("basic-canvas-drawing", "drawing-stroke");
                MauiUi.AssertSceneChanged(empty, drawn);
                ui.Click("basic-drawing-clear");
                var cleared = ui.CaptureStable("basic-canvas-drawing", "drawing-cleared");
                SampleImage.CompareGolden(cleared, empty);

                ui.NavigateBasic("GPU");
                ui.WaitText("basic-gpu-fps", text => text.StartsWith("FPS:", StringComparison.Ordinal) && !text.Contains("--"));
                MauiUi.AssertRendered(ui.Capture("basic-canvas-gpu", "basic-gpu-smoke"));
                ui.NavigateBasic("CPU");
                var returned = ui.CaptureStable("basic-canvas-cpu", "basic-cpu-returned");
                SampleImage.CompareGolden(returned, cpu);
                Compare(cpu, ios, deviceName, version, "basic-cpu");
            });

    [Theory]
    [InlineData(false, MauiDevice.AndroidName, MauiDevice.AndroidVersion)]
    [InlineData(true, MauiDevice.IosName, MauiDevice.IosVersion)]
    public Task GalleryMauiFiltersAndAdjustsGradient(bool ios, string deviceName, string version) =>
        Run("Gallery", "Maui/SkiaSharpSample.Maui.csproj",
            "com.skiasharp.gallery.maui", ios, deviceName, version, ui =>
            {
                ui.Click("gallery-header-cpu");
                var search = ui.Find("gallery-search");
                search.SendKeys("Gradient");
                ui.SubmitSearch();
                ui.WaitText("gallery-result-count", text => !text.StartsWith("0 ", StringComparison.Ordinal));
                ui.Click("gallery-filter-trigger");
                ui.Click("category-shaders---effects");
                ui.Find("gallery-filter-popup-count");
                ui.Click("gallery-filter-done");
                ui.Find("gallery-active-category-shaders---effects");
                ui.Click("sample-gradient");
                ui.WaitText("sample-title", text => text == "Gradient");
                ui.WaitText("sample-render-status", text => text.StartsWith("CPU", StringComparison.Ordinal) && text.Contains("  |"));
                var cpu = ui.CaptureStable("sample-canvas-cpu", "gallery-gradient-cpu");
                MauiUi.AssertRendered(cpu);
                var before = ui.Text("control-angle-value");
                ui.Drag(ui.Find("control-angle"), 0.2, 0.5, 0.7, 0.5);
                ui.WaitText("control-angle-value", text => text != before);
                var adjusted = ui.CaptureStable("sample-canvas-cpu", "gallery-gradient-adjusted");
                MauiUi.AssertSceneChanged(cpu, adjusted);
                ui.Click("sample-back");
                ui.Find("gallery-search");
                ui.Click("gallery-header-gpu");
                ui.Click("sample-gradient");
                ui.WaitText("sample-render-status", text => text.StartsWith("GPU", StringComparison.Ordinal) && text.Contains("  |"));
                var gpu = ui.CaptureStable("sample-canvas-gpu", "gallery-gradient-gpu");
                MauiUi.AssertRendered(gpu);
                ui.Click("sample-back");
                ui.Find("gallery-search");

                Assert.Multiple(
                    () => Compare(cpu, ios, deviceName, version, "gallery-gradient-cpu"),
                    () => Compare(gpu, ios, deviceName, version, "gallery-gradient-gpu"));
            });

    private async Task Run(string folder, string project, string appId, bool ios, string deviceName,
        string version, Action<MauiUi> scenario)
    {
        if (ios)
            Assert.SkipUnless(OperatingSystem.IsMacOS(), "iOS simulator tests require macOS.");
        var workspace = PrepareSample(folder);
        await using var device = new MauiDevice(workspace, ios);
        await device.Prepare(deviceName, version, appId);

        var framework = ios ? "net10.0-ios" : "net10.0-android";
        await BuildSample(folder, project, "Debug", framework, new Dictionary<string, string>
        {
            [ios ? "RuntimeIdentifier" : "AndroidPackageFormats"] = ios ? "iossimulator-arm64" : "apk"
        });
        var output = Path.Combine(workspace.Root, "samples", folder, Path.GetDirectoryName(project)!,
            "bin", "Debug", framework);
        var artifact = ios
            ? Assert.Single(Directory.EnumerateDirectories(Path.Combine(output, "iossimulator-arm64"), "*.app"))
            : Assert.Single(Directory.EnumerateFiles(output, "*-Signed.apk", SearchOption.AllDirectories));
        await device.Install(artifact);
        var activity = ios ? null : await device.AndroidActivity();
        await appium.Run(workspace, ios, async address =>
        {
            var options = new AppiumOptions
            {
                PlatformName = ios ? "iOS" : "Android",
                AutomationName = ios ? "XCUITest" : "UiAutomator2",
                DeviceName = deviceName,
                PlatformVersion = version
            };
            options.AddAdditionalAppiumOption("udid", device.Id);
            options.AddAdditionalAppiumOption("noReset", true);
            options.AddAdditionalAppiumOption("autoLaunch", false);
            options.AddAdditionalAppiumOption("newCommandTimeout", 120);
            if (ios)
            {
                options.AddAdditionalAppiumOption("bundleId", appId);
                options.AddAdditionalAppiumOption("wdaLocalPort", AppiumFixture.IosPort);
                options.AddAdditionalAppiumOption("showXcodeLog", true);
                options.AddAdditionalAppiumOption("showIOSLog", true);
                options.AddAdditionalAppiumOption("derivedDataPath", Path.Combine(workspace.Root, "wda"));
                options.AddAdditionalAppiumOption("updatedWDABundleId", MauiDevice.WdaBundleId);
                options.AddAdditionalAppiumOption("waitForIdleTimeout", 1);
                device.OwnWda();
            }
            else
            {
                options.AddAdditionalAppiumOption("appPackage", appId);
                options.AddAdditionalAppiumOption("appActivity", activity);
                options.AddAdditionalAppiumOption("systemPort", AppiumFixture.AndroidPort);
            }
            AppiumDriver? driver = null;
            MauiUi? ui = null;
            Exception? failure = null;
            try
            {
                driver = ios
                    ? new IOSDriver(address, options, TimeSpan.FromMinutes(3))
                    : new AndroidDriver(address, options, TimeSpan.FromMinutes(3));
                await device.Launch(activity);
                ui = new MauiUi(driver, ios, appId, workspace);
                scenario(ui);
            }
            catch (Exception error)
            {
                failure = error;
                var path = Path.Combine(workspace.DiagnosticsRoot, "scenario.failure.log");
                await File.WriteAllTextAsync(path, error.ToString());
                TestContext.Current.AddFileAttachment(path, "text/plain");
                throw;
            }
            finally
            {
                try
                {
                    ui?.SaveDiagnostics();
                    await device.CaptureLogs();
                }
                catch (Exception error) when (failure is not null)
                {
                    throw new AggregateException("MAUI scenario and diagnostics both failed.", failure, error);
                }
                finally
                {
                    driver?.Dispose();
                }
            }
        });
    }

    private static void Compare(string actual, bool ios, string device, string version, string name) =>
        SampleImage.CompareGolden(actual, Path.Combine(Repo.RootDir, "tests", "SkiaSharp.Tests.Samples", "Expected",
            "Maui", ios ? "iOS" : "Android", device.Replace(' ', '-'), version, name + ".png"));
}
