using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Pure-CPU raster surface. Always available, fully deterministic. Useful
	/// as a baseline for tests that should produce identical pixels regardless
	/// of GPU backend (and as the smoke-test fallback when no GPU/software ICD
	/// is reachable).
	/// </summary>
	public sealed class RasterSetup : VisualSetup
	{
		public override string Name        => "raster";
		public override bool   IsAvailable => true;

		public override VisualSurface CreateSurface (SKImageInfo info) =>
			new RasterSurface (info);

		private sealed class RasterSurface : VisualSurface
		{
			private SKSurface surface;
			public override SKImageInfo ImageInfo { get; }
			public override SKCanvas    Canvas    => surface.Canvas;

			public RasterSurface (SKImageInfo info)
			{
				ImageInfo = info;
				surface   = SKSurface.Create (info);
			}

			public override byte[] ReadPixels ()
			{
				var pixels = new byte[ImageInfo.BytesSize];
				var rgba = new SKImageInfo (ImageInfo.Width, ImageInfo.Height, SKColorType.Rgba8888, SKAlphaType.Premul);
				unsafe {
					fixed (byte* p = pixels) {
						if (!surface.ReadPixels (rgba, (IntPtr)p, rgba.RowBytes, 0, 0))
							throw new InvalidOperationException ("SKSurface.ReadPixels failed on raster surface");
					}
				}
				return pixels;
			}

			public override void Dispose ()
			{
				surface?.Dispose ();
				surface = null;
			}
		}
	}
}
