using System;
using System.Threading;
using System.Threading.Tasks;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Pure-CPU rasterizer. Always available, fully deterministic.
	/// Useful as a baseline and as the smoke-test fallback when no GPU/ICD
	/// is reachable.
	/// </summary>
	public sealed class RasterRenderer : IRenderer
	{
		public string Name => "raster";
		public bool IsAvailable => true;
		public string UnavailableReason => null;

		public Task<byte[]> RenderAsync (ISkiaScene scene, SKImageInfo info, CancellationToken ct)
		{
			ct.ThrowIfCancellationRequested ();

			using var surface = SKSurface.Create (info)
				?? throw new InvalidOperationException ("SKSurface.Create returned null for raster");
			scene.Draw (surface.Canvas);

			var rgba = new SKImageInfo (info.Width, info.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = new byte[rgba.BytesSize];
			unsafe {
				fixed (byte* p = pixels) {
					if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
						throw new InvalidOperationException ("SKSurface.ReadPixels failed on raster");
				}
			}
			return Task.FromResult (pixels);
		}

		public void Dispose () { }
	}
}
