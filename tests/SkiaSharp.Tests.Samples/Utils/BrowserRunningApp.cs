using System.Collections.Concurrent;
using Microsoft.Playwright;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class BrowserRunningApp(
    IPage page, Uri address, string diagnostics, ConcurrentQueue<string> errors)
{
    private int screenshots;

    internal IPage Page { get; } = page;

    internal async Task Navigate(string path)
    {
        var request = RunningApp.RequestUri(address, path);
        TestContext.Current.TestOutputHelper?.WriteLine($"Navigating to {request}");

        var response = await Page.GotoAsync(request.AbsoluteUri, new() { WaitUntil = WaitUntilState.Load })
            .WaitAsync(TestContext.Current.CancellationToken);

        AssertNoErrors();
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
    }

    internal async Task WaitForElement(string selector)
    {
        await Page.Locator(selector).First
            .WaitForAsync(new() { State = WaitForSelectorState.Visible })
            .WaitAsync(TestContext.Current.CancellationToken);

        AssertNoErrors();
    }

    internal async Task<string> Screenshot()
    {
        var name = ++screenshots == 1 ? "page" : $"page-{screenshots}";
        var actual = Path.Combine(diagnostics, $"{name}.actual.png");

        await Page.ScreenshotAsync(new() { Path = actual, FullPage = true })
            .WaitAsync(TestContext.Current.CancellationToken);

        // Retain the capture even when browser errors prevent golden validation.
        if (!errors.IsEmpty)
            TestContext.Current.AddFileAttachment(actual, "image/png");
        AssertNoErrors();

        return actual;
    }

    internal void AssertNoErrors() =>
        Assert.True(errors.IsEmpty, $"Browser errors:\n{string.Join("\n", errors)}");

    internal static async Task Run(RunningApp app, Func<BrowserRunningApp, Task> test)
    {
        var cancellation = TestContext.Current.CancellationToken;
        var diagnostics = app.Diagnostics;
        Directory.CreateDirectory(diagnostics);
        var errors = new ConcurrentQueue<string>();

        try
        {
            // Wait for the owned sample server before starting Chromium.
            using var ready = await app.WaitForResponse("/");
            Assert.Equal(System.Net.HttpStatusCode.OK, ready.StatusCode);
            var address = await app.GetAddress();
            cancellation.ThrowIfCancellationRequested();

            // Keep the browser context consistent with the reviewed screenshots.
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

            // Collect across operations so errors between helper calls are not lost.
            page.PageError += (_, error) =>
            {
                TestContext.Current.TestOutputHelper?.WriteLine($"[page error] {error}");
                errors.Enqueue($"[page error] {error}");
            };
            page.Console += (_, message) =>
            {
                TestContext.Current.TestOutputHelper?.WriteLine($"[console {message.Type}] {message.Text}");
                if (message.Type == "error")
                    errors.Enqueue($"[console error] {message.Text}");
            };

            // Run the test and check errors from any direct Page interactions.
            var running = new BrowserRunningApp(page, address, diagnostics, errors);
            var failed = false;
            try
            {
                await test(running);
                running.AssertNoErrors();
            }
            catch
            {
                failed = true;
                throw;
            }
            finally
            {
                // Capture early failures that did not reach Screenshot.
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
                        TestContext.Current.TestOutputHelper?.WriteLine($"[diagnostics] Page screenshot failed: {error.Message}");
                    }
                }
            }
        }
        catch (Exception error)
        {
            TestContext.Current.TestOutputHelper?.WriteLine($"[failure] {error}");
            throw;
        }
    }
}
