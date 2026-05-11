using System;
using System.Runtime.InteropServices.JavaScript;
using System.Threading.Tasks;
using SkiaSharp;
using SkiaSharp.Tests.Visual;

namespace SkiaSharp.Tests.RenderHost.Wasm;

// Browser-side render host. Three in-page renderers: raster (CPU),
// ganesh-gles (WebGL2 via Ganesh), graphite-dawn (WebGPU via Graphite).
// JS-side asks for a scene by name, picks a renderer, gets a base64-RGBA
// string back.
//
// Public surface is one [JSExport]: RenderSceneAsync. Init for the GPU
// backends is lazy + async — the first call to a GPU renderer initialises
// it, subsequent calls reuse the context.
public static partial class Program
{
	// Keep Main alive forever so the .NET runtime stays up servicing [JSExport]
	// calls. The default lifecycle for a WebAssembly SDK Exe is "Main runs at
	// startup, runtime exits when Main returns" — which is incompatible with
	// the call-from-JS-after-init model we need here.
	public static async Task Main () => await Task.Delay (System.Threading.Timeout.InfiniteTimeSpan);

	[JSExport]
	public static async Task<string> RenderSceneAsync (string rendererName, string sceneName, int width, int height)
	{
		var scene = SceneCatalog.Get (sceneName);
		var info = new SKImageInfo (width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

		byte[] pixels = rendererName switch {
			"wasm-raster"        => WasmRenderers.RenderRaster (scene, info),
			"wasm-ganesh-gles"   => WasmRenderers.RenderGaneshGl (scene, info),
			"wasm-graphite-dawn" => await WasmRenderers.RenderGraphiteDawnAsync (scene, info),
			_ => throw new ArgumentException ($"Unknown renderer '{rendererName}'"),
		};

		// Base64 keeps the JS boundary marshalling cheap and avoids
		// transferring a giant byte[] (JSExport supports it but the
		// round-trip is slower for medium sizes).
		return Convert.ToBase64String (pixels);
	}
}
