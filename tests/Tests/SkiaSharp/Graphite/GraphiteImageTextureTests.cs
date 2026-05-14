using System;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// Coverage for <see cref="SKImage.ToTextureImage"/> and
	/// <see cref="SKImage.FromTexture"/> against a Graphite recorder. These two
	/// factories are the public path for round-tripping pixels through GPU
	/// memory and form the basis of any custom <see cref="SKGraphiteImageProvider"/>
	/// implementation.
	/// </summary>
	public unsafe class GraphiteImageTextureTests : BaseTest
	{
		[SkippableFact]
		public void ToTextureImage_UploadsRasterToGraphiteBackedImage ()
		{
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();
			Assert.NotNull (recorder);

			using var bitmap = new SKBitmap (new SKImageInfo (32, 32, SKColorType.Rgba8888, SKAlphaType.Premul));
			bitmap.Erase (SKColors.Red);
			using var raster = SKImage.FromBitmap (bitmap);
			Assert.False (raster.IsTextureBacked, "Source must start as a raster image.");

			using var uploaded = SKImage.ToTextureImage (recorder, raster, mipmapped: false);
			Assert.NotNull (uploaded);
			Assert.True (uploaded.IsTextureBacked, "ToTextureImage should produce a GPU-backed SKImage.");
			Assert.Equal (raster.Width, uploaded.Width);
			Assert.Equal (raster.Height, uploaded.Height);

			// Drain the recorder so all the temporary GPU work allocated by the upload
			// is dispatched before the test exits.
			using (var recording = recorder.Snap ()) {
				if (recording != null)
					Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
			}
			Assert.True (ctx.Submit (new SKGraphiteSubmitInfo { Sync = true }));
		}

		[SkippableFact]
		public void ToTextureImage_InstanceForm_RoundTripsViaSelf ()
		{
			// The instance-form ToTextureImage(recorder) wraps the static form with
			// `this`. Verify the overload exists on a known-raster image and produces
			// a GPU-backed copy.
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();

			using var bitmap = new SKBitmap (new SKImageInfo (16, 16, SKColorType.Rgba8888, SKAlphaType.Premul));
			bitmap.Erase (SKColors.Blue);
			using var raster = SKImage.FromBitmap (bitmap);

			using var uploaded = raster.ToTextureImage (recorder);
			Assert.NotNull (uploaded);
			Assert.True (uploaded.IsTextureBacked);

			using (var recording = recorder.Snap ()) {
				if (recording != null)
					Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
			}
			Assert.True (ctx.Submit (new SKGraphiteSubmitInfo { Sync = true }));
		}

		[SkippableFact]
		public void FromTexture_WrapsRecorderAllocatedBackendTexture ()
		{
			// Allocate a Graphite-managed GPU texture on the recorder, render into it
			// via SKSurface.Create(recorder, bt), then wrap it as an SKImage via
			// FromTexture. The wrapped image must be GPU-backed and readback-able.
			Skip.IfNot (IsLinux, "Lavapipe smoke is Linux/CI only.");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");

			using var fixture = LavapipeFixture.TryCreate ();
			Skip.IfNot (fixture.IsAvailable, $"Lavapipe unavailable: {fixture.FailureReason}");

			const int W = 64, H = 64;

			using var ctx = SKGraphiteContext.CreateVulkan (fixture.BackendContext);
			using var recorder = ctx.CreateRecorder ();
			Assert.NotNull (recorder);

			// Vulkan TextureInfo matching the recorder's render-target caps. Keep this
			// in sync with GraphiteBackendTextureTests.RENDER_TARGET_USAGE.
			var vkInfo = new SKGraphiteVkTextureInfo {
				SampleCount     = 1,
				Mipmapped       = false,
				Flags           = 0,
				Format          = 37,   // VK_FORMAT_R8G8B8A8_UNORM
				ImageTiling     = 0,    // VK_IMAGE_TILING_OPTIMAL
				ImageUsageFlags = 0x1 | 0x2 | 0x4 | 0x10 | 0x80, // TRANSFER_SRC|DST|SAMPLED|COLOR_ATTACHMENT|INPUT_ATTACHMENT
				SharingMode     = 0,    // EXCLUSIVE
				AspectMask      = 0x1,  // COLOR
			};
			using var info = SKGraphiteTextureInfo.CreateVulkan (vkInfo);
			Assert.NotNull (info);

			using var bt = recorder.CreateBackendTexture (W, H, info);
			Assert.NotNull (bt);
			Assert.True (bt.IsValid);

			// Render into the backend texture: paint it solid green.
			using (var surface = SKSurface.Create (recorder, bt, SKColorType.Rgba8888)) {
				Assert.NotNull (surface);
				surface.Canvas.Clear (SKColors.LimeGreen);

				using var recording = recorder.Snap ();
				Assert.NotNull (recording);
				Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
			}

			// Wrap the same backend texture as a Graphite-backed SKImage.
			using var wrappedImage = SKImage.FromTexture (recorder, bt, SKColorType.Rgba8888, SKAlphaType.Premul);
			Assert.NotNull (wrappedImage);
			Assert.True (wrappedImage.IsTextureBacked, "FromTexture-wrapped image must be GPU-backed.");
			Assert.Equal (W, wrappedImage.Width);
			Assert.Equal (H, wrappedImage.Height);

			// Draw the wrapped image onto a fresh Graphite surface and verify the
			// destination pixels carry the green we painted into the source texture.
			var dstInfo = new SKImageInfo (W, H, SKColorType.Rgba8888, SKAlphaType.Premul);
			using (var dst = SKSurface.Create (recorder, dstInfo)) {
				Assert.NotNull (dst);
				dst.Canvas.Clear (SKColors.Black);
				dst.Canvas.DrawImage (wrappedImage, 0, 0);

				using (var recording = recorder.Snap ()) {
					Assert.NotNull (recording);
					Assert.Equal (SKGraphiteInsertStatus.Success, ctx.InsertRecording (recording));
				}
				Assert.True (ctx.Submit (new SKGraphiteSubmitInfo { Sync = true }));

				var pixels = new byte[W * H * 4];
				fixed (byte* p = pixels) {
					Assert.True (ctx.ReadPixels (dst, dstInfo, (IntPtr)p, W * 4, 0, 0));
				}
				int center = (32 * W + 32) * 4;
				// LimeGreen is roughly (50, 205, 50). Allow some slack.
				Assert.True (pixels[center + 1] > 150, $"G={pixels[center + 1]} expected >150");
				Assert.True (pixels[center + 0] < 100, $"R={pixels[center + 0]} expected <100");
			}

			recorder.DeleteBackendTexture (bt);
		}
	}
}
