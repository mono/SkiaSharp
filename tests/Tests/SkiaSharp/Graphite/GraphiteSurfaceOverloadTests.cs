using System;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// Coverage for the parameterized <see cref="SKSurface.Create"/> overloads that
	/// take a Graphite recorder: the base <c>(recorder, info)</c> form is exercised
	/// by every other Graphite smoke test, so this file targets the three less-used
	/// shapes — <c>mipmapped</c>, <c>SKSurfaceProperties</c>, and the combined
	/// <c>mipmapped + props</c> form.
	/// </summary>
	public unsafe class GraphiteSurfaceOverloadTests : BaseTest
	{
		[SkippableFact]
		public void Create_WithMipmapped_RendersAndReadsBack ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();

			var info = new SKImageInfo (64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var surface = SKSurface.Create (recorder, info, mipmapped: true);
			Assert.NotNull (surface);

			DrawAndAssertRedFill (ctx, recorder, surface, info);
		}

		[SkippableFact]
		public void Create_WithSurfaceProperties_RendersAndReadsBack ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();

			var info = new SKImageInfo (64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var props = new SKSurfaceProperties (SKPixelGeometry.RgbHorizontal);
			using var surface = SKSurface.Create (recorder, info, props);
			Assert.NotNull (surface);

			DrawAndAssertRedFill (ctx, recorder, surface, info);
		}

		[SkippableFact]
		public void Create_WithMipmappedAndSurfaceProperties_RendersAndReadsBack ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();

			var info = new SKImageInfo (64, 64, SKColorType.Rgba8888, SKAlphaType.Premul);
			using var props = new SKSurfaceProperties (SKPixelGeometry.RgbVertical);
			using var surface = SKSurface.Create (recorder, info, mipmapped: true, props);
			Assert.NotNull (surface);

			DrawAndAssertRedFill (ctx, recorder, surface, info);
		}

		[SkippableFact]
		public void Create_WithNullRecorder_Throws ()
		{
			// Argument-validation contract is part of the public API.
			Assert.Throws<ArgumentNullException> (() =>
				SKSurface.Create ((SKGraphiteRecorder)null, new SKImageInfo (16, 16, SKColorType.Rgba8888, SKAlphaType.Premul), mipmapped: false));
		}

		private static unsafe void DrawAndAssertRedFill (SKGraphiteContext ctx, SKGraphiteRecorder recorder, SKSurface surface, SKImageInfo info)
		{
			surface.Canvas.Clear (SKColors.Red);

			using (var recording = recorder.Snap ()) {
				Assert.NotNull (recording);
				Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
			}
			Assert.True (ctx.Submit (new SKGraphiteSubmitInfo { Sync = true }));

			var pixels = new byte[info.Width * info.Height * 4];
			Assert.True (ctx.ReadPixelsSync (surface, info, pixels, 0, 0));
			int center = ((info.Height / 2) * info.Width + (info.Width / 2)) * 4;
			Assert.True (pixels[center + 0] > 200, $"R={pixels[center + 0]} expected >200");
			Assert.True (pixels[center + 1] < 50,  $"G={pixels[center + 1]} expected <50");
			Assert.True (pixels[center + 2] < 50,  $"B={pixels[center + 2]} expected <50");
		}
	}
}
