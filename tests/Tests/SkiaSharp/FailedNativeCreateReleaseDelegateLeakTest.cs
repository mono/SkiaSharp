#nullable disable
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	// Regression guard for the GCHandle leak on a FAILED native create that takes a
	// release/destroy delegate.
	//
	// When a factory such as SKImage.FromPixels / SKBitmap.InstallPixels / SKData.Create /
	// SKSurface.Create is given a managed release delegate, DelegateProxies.Create allocates a
	// GCHandle rooting that delegate (and its captured closure). That handle is freed only by the
	// native release proc. If the native create FAILS (returns null / false) the release proc is
	// never invoked, so - without an explicit free on the failure path - the GCHandle (and the
	// delegate's captures) leaks for the lifetime of the process.
	//
	// These tests force each factory to fail and assert the release-delegate closure becomes
	// collectable. On a regressed build the WeakReference stays alive and the test fails.
	public class FailedNativeCreateReleaseDelegateLeakTest : SKTest
	{
		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference FailFromPixels ()
		{
			var sentinel = new byte[64];
			var wr = new WeakReference (sentinel);

			var info = new SKImageInfo (16, 16, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			try {
				// rowBytes smaller than a single row makes the pixmap invalid, so
				// sk_image_new_raster returns null and the release proc never runs.
				using var pixmap = new SKPixmap (info, pixels, 4);
				var image = SKImage.FromPixels (pixmap, (addr, ctx) => GC.KeepAlive (sentinel), sentinel);
				Assert.Null (image);
			} finally {
				Marshal.FreeCoTaskMem (pixels);
			}

			return wr;
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference FailInstallPixels ()
		{
			var sentinel = new byte[64];
			var wr = new WeakReference (sentinel);

			var info = new SKImageInfo (16, 16, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			try {
				using var bitmap = new SKBitmap ();
				var result = bitmap.InstallPixels (info, pixels, 4, (addr, ctx) => GC.KeepAlive (sentinel), sentinel);
				Assert.False (result);
			} finally {
				Marshal.FreeCoTaskMem (pixels);
			}

			return wr;
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference FailCreateSurface ()
		{
			var sentinel = new byte[64];
			var wr = new WeakReference (sentinel);

			var info = new SKImageInfo (16, 16, SKColorType.Rgba8888, SKAlphaType.Premul);
			var pixels = Marshal.AllocCoTaskMem (info.BytesSize);
			try {
				var surface = SKSurface.Create (info, pixels, 4, (addr, ctx) => GC.KeepAlive (sentinel), sentinel, null);
				Assert.Null (surface);
			} finally {
				Marshal.FreeCoTaskMem (pixels);
			}

			return wr;
		}

		[SkippableFact]
		public async Task FailedFromPixelsDoesNotLeakReleaseDelegate ()
		{
			var wr = FailFromPixels ();
			await AssertEx.EventuallyGC (wr);
		}

		[SkippableFact]
		public async Task FailedInstallPixelsDoesNotLeakReleaseDelegate ()
		{
			var wr = FailInstallPixels ();
			await AssertEx.EventuallyGC (wr);
		}

		[SkippableFact]
		public async Task FailedCreateSurfaceDoesNotLeakReleaseDelegate ()
		{
			var wr = FailCreateSurface ();
			await AssertEx.EventuallyGC (wr);
		}
	}
}
