#nullable disable
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Xunit;

namespace SkiaSharp.Tests
{
	// Regression guard for the release-proc GC-handle leak on the allocation-failure path.
	//
	// SKImage.FromPixels / SKImage.FromTexture / SKSurface.Create / SKData.Create wrap a
	// managed release delegate in a GCHandle and hand it to native code. Native frees that
	// GCHandle by invoking the release proc when the native object is destroyed. But when the
	// native create FAILS (returns null), the release proc is never invoked, so without an
	// explicit cleanup the GCHandle - and everything it roots (the delegate and its captured
	// context) - leaks for the lifetime of the process.
	//
	// These tests drive the failure path (an invalid SKImageInfo / too-small rowBytes) with a
	// release proc that captures a tracked context object, then assert the context is
	// collectable. On a leaky build the context stays rooted by the orphaned GCHandle and the
	// GC assertion fails; with the fix it is freed.
	public class ReleaseProcHandleLeakTest : SKTest
	{
		[Fact]
		public async Task ImageFromPixelsFreesReleaseHandleWhenCreateFails ()
		{
			var refs = ImageFromPixelsFailures (50);
			await AssertEx.EventuallyGC (refs);
		}

		[Fact]
		public async Task SurfaceCreateFreesReleaseHandleWhenCreateFails ()
		{
			var refs = SurfaceCreateFailures (50);
			await AssertEx.EventuallyGC (refs);
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference[] ImageFromPixelsFailures (int count)
		{
			var mem = Marshal.AllocCoTaskMem (64 * 64 * 4);
			try {
				// unknown color type makes the native raster image creation fail (return null)
				var badInfo = new SKImageInfo (4, 4, SKColorType.Unknown, SKAlphaType.Unknown);
				var refs = new WeakReference[count];
				for (var i = 0; i < count; i++) {
					var context = new object ();
					refs[i] = new WeakReference (context);
					using var pixmap = new SKPixmap (badInfo, mem);
					var image = SKImage.FromPixels (pixmap, (addr, ctx) => { }, context);
					Assert.Null (image);
					image?.Dispose ();
				}
				return refs;
			} finally {
				Marshal.FreeCoTaskMem (mem);
			}
		}

		[MethodImpl (MethodImplOptions.NoInlining)]
		private static WeakReference[] SurfaceCreateFailures (int count)
		{
			var mem = Marshal.AllocCoTaskMem (64 * 64 * 4);
			try {
				var info = new SKImageInfo (4, 4, SKColorType.Rgba8888, SKAlphaType.Premul);
				var refs = new WeakReference[count];
				for (var i = 0; i < count; i++) {
					var context = new object ();
					refs[i] = new WeakReference (context);
					// a row-bytes value that is too small makes the native raster surface fail
					var surface = SKSurface.Create (info, mem, 1, (addr, ctx) => { }, context, null);
					Assert.Null (surface);
					surface?.Dispose ();
				}
				return refs;
			} finally {
				Marshal.FreeCoTaskMem (mem);
			}
		}
	}
}
