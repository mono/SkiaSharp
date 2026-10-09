using System.Collections.Concurrent;
using Microsoft.Playwright;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal static class BrowserSampleApp
{
    internal static async Task Run(RunningApp app, string path, Func<IPage, string, Task> test)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var diagnostics = Path.Combine(app.Diagnostics, "browser");
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
                    new() { WaitUntil = WaitUntilState.DOMContentLoaded }).WaitAsync(cancellation);
                Assert.NotNull(response);
                Assert.Equal(200, response.Status);
                await test(page, diagnostics);
            }
            catch
            {
                failed = true;
                throw;
            }
            finally
            {
                try
                {
                    var screenshot = Path.Combine(diagnostics, "page.png");
                    await page.ScreenshotAsync(new() { Path = screenshot, Timeout = 10000 });
                    TestContext.Current.AddFileAttachment(screenshot, "image/png");
                }
                catch (Exception error)
                {
                    Record($"[diagnostics] Page screenshot failed: {error.Message}");
                }

                if (!failed)
                    Assert.True(errors.IsEmpty, $"Browser errors:\n{string.Join("\n", errors)}");
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
