using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Blazor.Components;

using SkiaSharp.Tests;
using SkiaSharp.Tests.Wasm;

// Configure test paths for WASM VFS
TestConfig.Current = new WasmTestConfig();

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
	.AddXunit(useReflection: true)
	.AddTestAssembly(typeof(WasmTests).Assembly)
	.AddTestAssembly(typeof(SKPaintTest).Assembly)
	.AddConsoleResultChannel());

await builder.Build().RunAsync();
