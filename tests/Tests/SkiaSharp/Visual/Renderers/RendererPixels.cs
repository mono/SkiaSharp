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

		/// <summary>
		/// Reads pixels back from a Graphite-backed <paramref name="surface"/> into the normalized
		/// RGBA8888/premul buffer. Graphite surfaces do NOT support the synchronous
		/// <see cref="SKSurface.ReadPixels(SKImageInfo, IntPtr, int, int, int)"/> in shipping builds
		/// — Skia gates that on <c>GPU_TEST_UTILS</c> (see <c>src/gpu/graphite/Device.cpp</c>), so it
		/// returns false. Use the async <see cref="SKGraphiteContext.RequestReadPixels(SKSurface, SKImageInfo, SKRectI, Action{SKGraphiteAsyncReadResult})"/>
		/// path the C API exposes for exactly this purpose, driven synchronously to completion here.
		/// </summary>
		public static byte[] ReadRgbaGraphite(SKGraphiteContext context, SKSurface surface, SKImageInfo info)
		{
			if (context == null)
				throw new ArgumentNullException(nameof(context));
			if (surface == null)
				throw new ArgumentNullException(nameof(surface));

			var rgba = NormalizedInfo(info);
			byte[] pixels = null;
			var failed = false;
			var done = false;

			context.RequestReadPixels(
				surface,
				rgba,
				new SKRectI(0, 0, rgba.Width, rgba.Height),
				result =>
				{
					done = true;
					if (result is null || result.PlaneCount < 1)
					{
						failed = true;
						return;
					}

					var srcBase = result.GetPlaneData(0);
					if (srcBase == IntPtr.Zero)
					{
						failed = true;
						return;
					}

					// The async result plane may be padded; copy row-by-row into the
					// tightly-packed golden buffer, dropping any per-row padding.
					var buffer = new byte[rgba.BytesSize];
					var srcRowBytes = result.GetPlaneRowBytes(0);
					var rowBytes = Math.Min(srcRowBytes, rgba.RowBytes);
					for (var y = 0; y < rgba.Height; y++)
						Marshal.Copy(srcBase + (y * srcRowBytes), buffer, y * rgba.RowBytes, rowBytes);

					pixels = buffer;
				});

			// Flush the queued readback copy and wait for the GPU, then pump the context
			// until the async callback has fired (it runs on the thread that drives completion).
			context.Submit(new SKGraphiteSubmitInfo { Sync = true });
			for (var i = 0; i < 10_000 && !done; i++)
				context.CheckAsyncWorkCompletion();

			if (!done)
				throw new InvalidOperationException("Graphite async readback callback never fired.");
			if (failed || pixels is null)
				throw new InvalidOperationException("Graphite async readback failed.");

			return pixels;
		}
	}
}
