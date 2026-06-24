using System;
using System.Runtime.InteropServices;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// Helpers for reading rendered pixels back into a normalized RGBA8888 /
	/// premultiplied byte buffer — the canonical format every renderer returns and
	/// every golden image is compared in.
	///
	/// <para>
	/// Public because renderers in the satellite host projects (Vulkan, Direct3D)
	/// read their pixels back through the same helper as the shared renderers.
	/// </para>
	/// </summary>
	public static class RendererPixels
	{
		public static readonly SKColorType ColorType = SKColorType.Rgba8888;
		public static readonly SKAlphaType AlphaType = SKAlphaType.Premul;

		public static SKImageInfo NormalizedInfo(SKImageInfo info) =>
			new(info.Width, info.Height, ColorType, AlphaType);

		public static byte[] ReadRgba(SKSurface surface, SKImageInfo info)
		{
			var rgba = NormalizedInfo(info);
			var pixels = new byte[rgba.BytesSize];

			var handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
			try
			{
				if (!surface.ReadPixels(rgba, handle.AddrOfPinnedObject(), rgba.RowBytes, 0, 0))
					throw new InvalidOperationException("SKSurface.ReadPixels failed.");
			}
			finally
			{
				handle.Free();
			}

			return pixels;
		}
	}
}
