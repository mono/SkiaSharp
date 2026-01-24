using System.Diagnostics;
using Microsoft.Playwright;
using Xunit;
using Xunit.Abstractions;

namespace SkiaSharp.Tests.Integration;

/// <summary>
/// Tests that verify SkiaSharp packages work in Blazor WebAssembly applications.
/// </summary>
[Trait("Category", "Platform")]
public class BlazorTests(ITestOutputHelper output) : PlatformTestBase(output)
{
    [Fact]
    public Task Blazor_SKCanvasView() => TestBlazor("SKCanvasView", "SKPaintSurfaceEventArgs");

    [Fact]
    public Task Blazor_SKGLView() => TestBlazor("SKGLView", "SKPaintGLSurfaceEventArgs");

    private async Task TestBlazor(string canvasView, string eventArgsType)
    {
        Output.WriteLine($"Testing SkiaSharp {SkiaVersion} in Blazor WASM ({canvasView})");
        var projectDir = Path.Combine(TestDir, $"Blazor{canvasView}");
        
        await Run("dotnet", $"new blazorwasm -n Blazor{canvasView} -o {projectDir} --no-https");
        await Run("dotnet", $"add {projectDir} package SkiaSharp.Views.Blazor --version {SkiaVersion}");
        
        await File.WriteAllTextAsync(Path.Combine(projectDir, "Pages", "Home.razor"), $$"""
            @page "/"
            @using SkiaSharp
            @using SkiaSharp.Views.Blazor

            <div id="canvas-container">
                <{{canvasView}} OnPaintSurface="OnPaintSurface" 
                    WidthRequest="{{TestImage.Width}}" HeightRequest="{{TestImage.Height}}"
                    style="width:{{TestImage.Width}}px;height:{{TestImage.Height}}px;" />
            </div>
            <p id="render-status">@_status</p>

            @code {
                private string _status = "Not rendered";
                private void OnPaintSurface({{eventArgsType}} e)
                {
                    var canvas = e.Surface.Canvas;
                    {{TestImage.GetDrawCode()}}
                    _status = $"Rendered {e.Info.Width}x{e.Info.Height}";
                }
            }
            """);
        
        await Run("dotnet", $"build {projectDir} -c Release -p:WasmBuildNative=true", timeoutSeconds: 300);
        
        await VerifyWithPlaywright(projectDir, canvasView);
    }

    private async Task VerifyWithPlaywright(string projectDir, string canvasView)
    {
        var port = GetAvailablePort();
        using var serverProcess = StartServer(projectDir, port);
        
        try
        {
            var serverReady = await WaitForServer(port, timeoutSeconds: 30);
            Assert.True(serverReady, "Blazor server failed to start");
            
            using var playwright = await Playwright.CreateAsync();
            await using var browser = await playwright.Chromium.LaunchAsync(new() { Headless = true });
            var page = await browser.NewPageAsync();
            
            await page.GotoAsync($"http://localhost:{port}", new() { WaitUntil = WaitUntilState.NetworkIdle, Timeout = 60000 });
            await page.WaitForSelectorAsync("#render-status:has-text('Rendered')", new() { Timeout = 30000 });
            
            var canvasElement = await page.QuerySelectorAsync("#canvas-container canvas");
            Assert.NotNull(canvasElement);

            var screenshotBytes = await canvasElement.ScreenshotAsync();
            await VerifyScreenshot(screenshotBytes, $"blazor-{canvasView}", "blazor");
        }
        finally
        {
            StopProcess(serverProcess);
        }
    }

    private Process StartServer(string projectDir, int port)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project {projectDir} -c Release --urls http://localhost:{port}",
            WorkingDirectory = TestDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        })!;
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        return process;
    }

    private static int GetAvailablePort()
    {
        using var listener = new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback, 0);
        listener.Start();
        var port = ((System.Net.IPEndPoint)listener.LocalEndpoint).Port;
        listener.Stop();
        return port;
    }

    private static async Task<bool> WaitForServer(int port, int timeoutSeconds)
    {
        using var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) };
        var deadline = DateTime.UtcNow.AddSeconds(timeoutSeconds);
        
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                var response = await client.GetAsync($"http://localhost:{port}");
                if (response.IsSuccessStatusCode)
                    return true;
            }
            catch { }
            await Task.Delay(1000);
        }
        return false;
    }

    private static void StopProcess(Process process)
    {
        try { if (!process.HasExited) { process.Kill(true); process.WaitForExit(1000); } } catch { }
    }
}
