#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Provides access to OpenType font metrics.</summary>
	/// <remarks />
	public unsafe class OpenTypeMetrics
	{
		private readonly Font font;

		/// <summary>Initializes a new instance of the <see cref="T:HarfBuzzSharp.OpenTypeMetrics" /> struct for the specified font.</summary>
		/// <param name="font">The font to retrieve metrics from.</param>
		/// <remarks />
		public OpenTypeMetrics (Font font)
		{
			this.font = font ?? throw new ArgumentNullException (nameof (font));
		}

		/// <summary>Tries to get the position for the specified metrics tag.</summary>
		/// <param name="metricsTag">The metrics tag to query.</param>
		/// <param name="position">When this method returns, contains the position value if found.</param>
		/// <returns><see langword="true" /> if the position was found; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position)
		{
			fixed (int* p = &position) {
				return HarfBuzzApi.hb_ot_metrics_get_position (font.Handle, metricsTag, p);
			}
		}

		/// <summary>Gets the variation value for the specified metrics tag.</summary>
		/// <param name="metricsTag">The metrics tag to query.</param>
		/// <returns>The variation value.</returns>
		/// <remarks />
		public float GetVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_variation (font.Handle, metricsTag);

		/// <summary>Gets the horizontal variation value for the specified metrics tag.</summary>
		/// <param name="metricsTag">The metrics tag to query.</param>
		/// <returns>The horizontal variation value in font units.</returns>
		/// <remarks />
		public int GetXVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_x_variation (font.Handle, metricsTag);

		/// <summary>Gets the vertical variation value for the specified metrics tag.</summary>
		/// <param name="metricsTag">The metrics tag to query.</param>
		/// <returns>The vertical variation value in font units.</returns>
		/// <remarks />
		public int GetYVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_y_variation (font.Handle, metricsTag);
	}
}
