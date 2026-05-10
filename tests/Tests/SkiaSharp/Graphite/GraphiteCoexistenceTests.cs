using System;
using SkiaSharp.Tests.Visual;
using Xunit;

namespace SkiaSharp.Tests.Graphite
{
	/// <summary>
	/// A single .NET process must be able to instantiate and use both
	/// <see cref="GRContext"/> (Ganesh) and <see cref="SKGraphiteContext"/>
	/// (Graphite) without one corrupting the other. Both backends share the
	/// same VkInstance/VkDevice via <see cref="VulkanLoader.Shared"/>,
	/// exercising the "two consumers, one device" path.
	/// </summary>
	public class GraphiteCoexistenceTests : BaseTest
	{
		[SkippableFact]
		public unsafe void Coexist_GaneshAndGraphite_BothRenderSameScene ()
		{
			Skip.IfNot (IsLinux, "Coexistence test relies on Lavapipe (Linux/CI only).");
			Skip.IfNot (SKGraphiteContext.IsBackendAvailable (SKGraphiteBackend.Vulkan),
				"Graphite/Vulkan not available in this libSkiaSharp build.");
			Skip.IfNot (VulkanLoader.Shared.IsAvailable,
				$"Vulkan loader unavailable: {VulkanLoader.Shared.FailureReason}");

			var info = new SKImageInfo (96, 96, SKColorType.Rgba8888, SKAlphaType.Premul);

			byte[] ganeshPixels;
			byte[] graphitePixels;

			using (var ganesh = new GaneshVulkanSetup ())
			using (var graphite = new GraphiteVulkanSetup ()) {
				Assert.True (ganesh.IsAvailable,    $"Ganesh+Vulkan setup unavailable: {ganesh.UnavailableReason}");
				Assert.True (graphite.IsAvailable,  $"Graphite+Vulkan setup unavailable: {graphite.UnavailableReason}");

				// Both setups draw the SAME scene through their respective canvases.
				Action<SKCanvas> draw = canvas => {
					canvas.Clear (SKColors.White);
					using var paint = new SKPaint { Color = SKColors.DarkGreen, IsAntialias = true };
					canvas.DrawCircle (48, 48, 32, paint);
				};

				using (var s = ganesh.CreateSurface (info)) {
					draw (s.Canvas);
					ganeshPixels = s.ReadPixels ();
				}

				using (var s = graphite.CreateSurface (info)) {
					draw (s.Canvas);
					graphitePixels = s.ReadPixels ();
				}
			}

			// Both must have produced output, and neither destabilised the other.
			Assert.Equal (ganeshPixels.Length, graphitePixels.Length);

			// Pixel-by-pixel: <1% per-channel tolerance for "same scene"
			// across backends. Compute the mean absolute difference to make the
			// failure message quantitative if it ever fires.
			long sumAbsDiff = 0;
			int  pxCount = info.Width * info.Height;
			int  maxDelta = 0;
			for (int i = 0; i < ganeshPixels.Length; i++) {
				int d = Math.Abs (ganeshPixels[i] - graphitePixels[i]);
				sumAbsDiff += d;
				if (d > maxDelta) maxDelta = d;
			}
			double meanDelta = (double)sumAbsDiff / ganeshPixels.Length;
			Assert.True (meanDelta < 2.55, // ~1% of 255
				$"Ganesh and Graphite produced visibly different pixels for the same scene: " +
				$"mean per-channel delta {meanDelta:F2} (max {maxDelta}), threshold 2.55 (~1%).");
		}
	}
}
