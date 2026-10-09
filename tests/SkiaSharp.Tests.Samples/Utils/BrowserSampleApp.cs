using System.Collections.Concurrent;
using Microsoft.Playwright;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class BrowserSampleApp
{
    internal static Task Capture(RunningApp app, string path, string selector, string golden) =>
        Capture(app, path, selector, golden, null);

    internal static async Task Capture(RunningApp app, string path, string selector, string golden,
        double? maxAverageColorErrorFraction)
    {
        var actual = Path.Combine(app.Diagnostics, "page.actual.png");
        try
        {
            await Run(app, path, async (page, _) =>
            {
                var cancellation = TestContext.Current.CancellationToken;
                await page.Locator(selector).First.WaitForAsync(new() { State = WaitForSelectorState.Visible }).WaitAsync(cancellation);
                await page.ScreenshotAsync(new() { Path = actual, FullPage = true }).WaitAsync(cancellation);
            });
        }
        catch
        {
            if (File.Exists(actual))
                TestContext.Current.AddFileAttachment(actual, "image/png");
            throw;
        }
        SampleImage.ValidateFile(actual, golden, maxAverageColorErrorFraction);
    }

    internal static async Task Run(RunningApp app, string path, Func<IPage, string, Task> test)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var diagnostics = app.Diagnostics;
        Directory.CreateDirectory(diagnostics);
        var log = new ConcurrentQueue<string>();
        var errors = new ConcurrentQueue<string>();

        void Record(string message, bool error = false)
        {
            log.Enqueue(message);
            if (error)
                errors.Enqueue(message);
            TestContext.Current.TestOutputHelper?.WriteLine(message);
        }

        try
        {
            using var ready = await app.WaitForResponse("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, ready.StatusCode);
            var address = RunningApp.RequestUri(await app.GetAddress(), path);
            cancellation.ThrowIfCancellationRequested();
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            await using var context = await browser.NewContextAsync(new()
            {
                ViewportSize = new() { Width = 1280, Height = 900 },
                DeviceScaleFactor = 1,
                ColorScheme = ColorScheme.Light
            });
            var page = await context.NewPageAsync();
            page.SetDefaultTimeout(60000);
            page.SetDefaultNavigationTimeout(60000);
            page.PageError += (_, error) => Record($"[page error] {error}", error: true);
            page.Console += (_, message) => Record($"[console {message.Type}] {message.Text}", message.Type == "error");
            var failed = false;
            try
            {
                Record($"Navigating to {address}");
                var response = await page.GotoAsync(address.AbsoluteUri,
                    new() { WaitUntil = WaitUntilState.Load }).WaitAsync(cancellation);
                Assert.NotNull(response);
                Assert.Equal(200, response.Status);
                await test(page, diagnostics);
                Assert.True(errors.IsEmpty, $"Browser errors:\n{string.Join("\n", errors)}");
            }
            catch
            {
                failed = true;
                throw;
            }
            finally
            {
                if (failed && !File.Exists(Path.Combine(diagnostics, "page.actual.png")))
                {
                    try
                    {
                        var screenshot = Path.Combine(diagnostics, "page.failure.png");
                        await page.ScreenshotAsync(new() { Path = screenshot, FullPage = true, Timeout = 10000 });
                        TestContext.Current.AddFileAttachment(screenshot, "image/png");
                    }
                    catch (Exception error)
                    {
                        Record($"[diagnostics] Page screenshot failed: {error.Message}");
                    }
                }
            }
        }
        catch (Exception error)
        {
            Record($"[failure] {error}");
            throw;
        }
        finally
        {
            var logPath = Path.Combine(diagnostics, "browser.log");
            await File.WriteAllLinesAsync(logPath, log);
            TestContext.Current.AddFileAttachment(logPath, "text/plain");
        }
    }
}
