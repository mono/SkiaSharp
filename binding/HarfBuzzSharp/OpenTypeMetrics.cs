#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// To be added.
	/// </summary>
	/// <remarks>To be added.</remarks>
	public unsafe class OpenTypeMetrics
	{
		private readonly Font font;

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="font">To be added.</param>
		/// <remarks>To be added.</remarks>
		public OpenTypeMetrics (Font font)
		{
			this.font = font ?? throw new ArgumentNullException (nameof (font));
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="metricsTag">To be added.</param>
		/// <param name="position">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position)
		{
			fixed (int* p = &position) {
				return HarfBuzzApi.hb_ot_metrics_get_position (font.Handle, metricsTag, p);
			}
		}

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="metricsTag">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public float GetVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_variation (font.Handle, metricsTag);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="metricsTag">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public int GetXVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_x_variation (font.Handle, metricsTag);

		/// <summary>
		/// To be added.
		/// </summary>
		/// <param name="metricsTag">To be added.</param>
		/// <returns>To be added.</returns>
		/// <remarks>To be added.</remarks>
		public int GetYVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_y_variation (font.Handle, metricsTag);
	}
}
