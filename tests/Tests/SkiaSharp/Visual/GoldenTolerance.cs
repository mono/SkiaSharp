namespace SkiaSharp.Tests.Visual
{
	/// <summary>
	/// How strictly a rendered image must match its golden.
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

		// CPU raster is bit-deterministic on one platform, but the shared raster
		// golden is captured on one architecture and replayed on others. Allow a
		// 2-LSB wobble on a tiny fraction of pixels to absorb cross-architecture
		// antialiasing rounding; a real regression moves far more than that.
		public static readonly GoldenTolerance Deterministic = new(2, 0.002);

		// Absorbs GPU driver and antialiasing variance.
		public static readonly GoldenTolerance Gpu = new(12, 0.02);

		public static GoldenTolerance For(string rendererName) =>
			rendererName == GpuBackends.Raster ? Deterministic : Gpu;
	}
}
