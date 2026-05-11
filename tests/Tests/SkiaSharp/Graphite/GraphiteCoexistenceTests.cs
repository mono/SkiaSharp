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

			// Both renderers draw the SAME scene; both pull from VulkanLoader.Shared,
			// exercising the "two consumers, one device" coexistence path.
			var scene = new CoexistenceScene { Info = info };
			var ganesh   = new GaneshVulkanRenderer ();
			var graphite = new GraphiteVulkanRenderer ();
			Assert.True (ganesh.IsAvailable,   $"Ganesh+Vulkan renderer unavailable: {ganesh.UnavailableReason}");
			Assert.True (graphite.IsAvailable, $"Graphite+Vulkan renderer unavailable: {graphite.UnavailableReason}");

			var ganeshPixels   = ganesh.RenderAsync   (scene, info, default).GetAwaiter ().GetResult ();
			var graphitePixels = graphite.RenderAsync (scene, info, default).GetAwaiter ().GetResult ();

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

		// Test-local scene; lives outside the SceneCatalog so it stays scoped
		// to this test's specific assertion (cross-backend parity).
		private sealed class CoexistenceScene : ISkiaScene
		{
			public SKImageInfo Info { get; set; }
			public string Name => nameof (CoexistenceScene);
			public SKImageInfo SuggestedInfo => Info;
			public SceneRequirements Requires => SceneRequirements.None;
			public void Draw (SKCanvas canvas)
			{
				canvas.Clear (SKColors.White);
				using var paint = new SKPaint { Color = SKColors.DarkGreen, IsAntialias = true };
				canvas.DrawCircle (48, 48, 32, paint);
			}
		}
	}
}
