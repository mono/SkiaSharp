using System;
using System.Collections.Generic;

namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// How strictly a rendered image must match its golden. The CPU raster backend
	/// is bit-deterministic, so it is held to a near-exact tolerance. GPU backends
	/// vary with driver and antialiasing implementation, so they get a wider
	/// per-channel tolerance and allow a small fraction of outlier pixels. Values
	/// can be overridden per renderer, and per (renderer, scene) for a specific
	/// known-divergent cell.
	/// </summary>
	public readonly struct GoldenTolerance
	{
		/// <summary>Maximum allowed absolute per-channel (R/G/B/A) delta.</summary>
		public int ChannelTolerance { get; }

		/// <summary>
		/// Maximum fraction of pixels (0..1) allowed to exceed
		/// <see cref="ChannelTolerance"/> before the comparison fails.
		/// </summary>
		public double MaxOutlierFraction { get; }

		public GoldenTolerance(int channelTolerance, double maxOutlierFraction)
		{
			ChannelTolerance = channelTolerance;
			MaxOutlierFraction = maxOutlierFraction;
		}

		// CPU raster is bit-deterministic on a single platform, but the portable
		// shared raster golden (the {renderer}/ layer) is compared across desktop
		// architectures (captured on one, replayed on x64/arm64). Allow a 2-LSB
		// per-channel wobble on a tiny fraction of pixels to absorb
		// cross-architecture antialiasing rounding; a real regression moves far
		// more than that.
		public static readonly GoldenTolerance Deterministic = new(2, 0.002);

		// Hardware/driver GPU output: absorbs antialiasing and rounding variance.
		public static readonly GoldenTolerance Gpu = new(12, 0.02);

		private static readonly Dictionary<string, GoldenTolerance> ByRenderer = new(StringComparer.Ordinal)
		{
			["raster"] = Deterministic,
			["ganesh-gl"] = Gpu,
			["ganesh-metal"] = Gpu,
			["ganesh-vulkan"] = Gpu,
			["direct3d"] = Gpu,
		};

		// Per (renderer, scene) overrides for individually known-divergent cells.
		private static readonly Dictionary<string, GoldenTolerance> ByRendererScene = new(StringComparer.Ordinal)
		{
		};

		public static GoldenTolerance For(string rendererName, string sceneName)
		{
			if (ByRendererScene.TryGetValue(Key(rendererName, sceneName), out var perCell))
				return perCell;
			if (ByRenderer.TryGetValue(rendererName, out var perRenderer))
				return perRenderer;
			return Gpu;
		}

		private static string Key(string rendererName, string sceneName) =>
			rendererName + "/" + sceneName;
	}
}
