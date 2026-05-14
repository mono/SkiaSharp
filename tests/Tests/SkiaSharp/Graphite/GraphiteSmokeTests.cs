using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	public class GraphiteSmokeTests : BaseTest
	{
		[SkippableFact]
		public void IsBackendAvailable_Vulkan_ReturnsTrue ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Assert.True (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"libSkiaSharp built without Graphite/Vulkan — rebuild with SUPPORT_GRAPHITE=true SUPPORT_VULKAN=true.");
		}

		[SkippableFact]
		public unsafe void Vulkan_Graphite_DrawAndReadBack_RedRRect ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			Assert.NotNull (ctx);

			using var recorder = ctx.CreateRecorder ();
			Assert.NotNull (recorder);

			var info = new SKImageInfo (256, 256, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create (recorder, info);
			Assert.NotNull (surface);

			var canvas = surface.Canvas;
			canvas.Clear (SKColors.White);
			using (var paint = new SKPaint { Color = SKColors.Red, IsAntialias = true }) {
				canvas.DrawRoundRect (SKRect.Create (32, 32, 192, 192), 24, 24, paint);
			}

			using var recording = recorder.Snap ();
			Assert.NotNull (recording);

			Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
			Assert.True (ctx.Submit (new SKGraphiteSubmitInfo {
				Sync = true,
			}));

			// Synchronous CPU readback via Skia's async path (sync wrapper).
			const int W = 256, H = 256;
			var pixels = new byte[W * H * 4];
			Assert.True (ctx.ReadPixelsSync (surface, info, pixels, 0, 0));

			// RGBA_8888: low byte = R.
			int center = (128 * W + 128) * 4;
			Assert.True (pixels[center + 0] > 200, $"R={pixels[center + 0]} expected >200");
			Assert.True (pixels[center + 1] < 50,  $"G={pixels[center + 1]} expected <50");
			Assert.True (pixels[center + 2] < 50,  $"B={pixels[center + 2]} expected <50");

			// Corner should still be white.
			Assert.Equal (255, pixels[0]);
			Assert.Equal (255, pixels[1]);
			Assert.Equal (255, pixels[2]);
		}

		[SkippableFact]
		public void Submit_WithSync_ThrowsOnNonYieldingContext ()
		{
			// SKGraphiteContext auto-flags itself non-yielding when its backend is Dawn-in-browser
			// (Emscripten without ASYNCIFY can't pump the Dawn event loop from a managed stack).
			// We can't easily build a real Dawn-in-browser context from here, so flip the private
			// flag via reflection on an already-running Vulkan context. The Submit-side guard
			// lives in managed code and doesn't care about the actual backend.
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			Assert.NotNull (ctx);

			var field = typeof (SKGraphiteContext).GetField ("isNonYielding",
				BindingFlags.Instance | BindingFlags.NonPublic);
			Assert.NotNull (field);
			field.SetValue (ctx, true);

			// Sync = true on a non-yielding context must throw before reaching native code.
			var ex = Assert.Throws<InvalidOperationException> (() =>
				ctx.Submit (new SKGraphiteSubmitInfo { Sync = true }));
			Assert.Contains ("not supported in this environment", ex.Message);

			// Sync = false stays a normal call (returns true; no pending work, no failure).
			Assert.True (ctx.Submit (new SKGraphiteSubmitInfo { Sync = false }));
			// Parameterless Submit() likewise unaffected.
			Assert.True (ctx.Submit ());
		}

		[SkippableFact]
		public unsafe void Vulkan_Graphite_BackendContextCanBeDisposedEarly ()
		{
			// Variant A regression: SKGraphiteContext.CreateVulkan must take ownership of the
			// GetProc GCHandle from the SKGraphiteVkBackendContext. After that, the backend
			// context is safe to dispose immediately — the still-live SKGraphiteContext
			// continues to dispatch Vulkan calls through the captured delegate without
			// referring back to the backend-context object.

			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			Assert.NotNull (ctx);

			// Dispose the backend-context aggressively. If Variant A is broken (or reverted to
			// the original "user keeps it alive" rule), the next Vulkan call from inside Skia
			// will dereference a freed GCHandle and the test crashes hard.
			fixture.DisposeBackendContextOnly ();

			using (ctx) {
				// CreateRecorder pokes Vulkan (queries device caps); a render exercises the
				// dispatch table fully. Both must succeed against the now-bc-less context.
				using var rec = ctx.CreateRecorder ();
				Assert.NotNull (rec);

				var info = new SKImageInfo (64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
				using var surface = SKSurface.Create (rec, info);
				Assert.NotNull (surface);
				surface.Canvas.Clear (SKColors.Blue);

				using var recording = rec.Snap ();
				Assert.NotNull (recording);
				Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));

				var pixels = new byte[64 * 64 * 4];
				Assert.True (ctx.ReadPixelsSync (surface, info, pixels, 0, 0));
				// Spot-check: should be blue (B=255, R=0, G=0).
				Assert.True (pixels[2] > 200, $"B={pixels[2]} expected >200");
				Assert.True (pixels[0] < 50,  $"R={pixels[0]} expected <50");
			}
		}
	}
}
