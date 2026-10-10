using System.Collections.Concurrent;
using Microsoft.Playwright;
using Xunit;

namespace SkiaSharp.Tests.Samples.Utils;

internal sealed class BrowserRunningApp(
    IPage page, Uri address, string diagnostics, ConcurrentQueue<string> errors, Action<string> record)
{
    private int screenshots;

    internal IPage Page { get; } = page;

    internal async Task Navigate(string path)
    {
        var request = RunningApp.RequestUri(address, path);
        record($"Navigating to {request}");
        var response = await Page.GotoAsync(request.AbsoluteUri, new() { WaitUntil = WaitUntilState.Load })
            .WaitAsync(TestContext.Current.CancellationToken);
        Assert.NotNull(response);
        Assert.Equal(200, response.Status);
    }

    internal Task WaitForElement(string selector) =>
        Page.Locator(selector).First
            .WaitForAsync(new() { State = WaitForSelectorState.Visible })
            .WaitAsync(TestContext.Current.CancellationToken);

    internal async Task<string> Screenshot()
    {
        var name = ++screenshots == 1 ? "page" : $"page-{screenshots}";
        var actual = Path.Combine(diagnostics, $"{name}.actual.png");
        await Page.ScreenshotAsync(new() { Path = actual, FullPage = true }).WaitAsync(TestContext.Current.CancellationToken);

        try
        {
            AssertNoErrors();
        }
        catch
        {
            TestContext.Current.AddFileAttachment(actual, "image/png");
            throw;
        }
        return actual;
    }

    private void AssertNoErrors() =>
        Assert.True(errors.IsEmpty, $"Browser errors:\n{string.Join("\n", errors)}");

    internal static async Task Run(RunningApp app, Func<BrowserRunningApp, Task> test)
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
            var address = await app.GetAddress();
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
            var running = new BrowserRunningApp(page, address, diagnostics, errors, message => Record(message));
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
