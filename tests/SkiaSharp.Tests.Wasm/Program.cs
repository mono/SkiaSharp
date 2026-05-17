using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using DeviceRunners.VisualRunners;
using DeviceRunners.VisualRunners.Blazor.Components;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.RootComponents.Add<TestRunnerApp>("#app");

builder.UseVisualTestRunner(conf => conf
	.AddXunit(useReflection: true)
	.AddTestAssembly(typeof(SkiaSharp.Tests.Wasm.WasmTests).Assembly)
	.AddTestAssembly(typeof(SkiaSharp.Tests.SKPaintTest).Assembly)
	.AddConsoleResultChannel());

await builder.Build().RunAsync();
