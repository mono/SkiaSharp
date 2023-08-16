using System;

namespace HarfBuzzSharp
{
	public unsafe class OpenTypeMetrics
	{
		private readonly Font font;

		public OpenTypeMetrics (Font font)
		{
			this.font = font ?? throw new ArgumentNullException (nameof (font));
		}

		public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position)
		{
			fixed (int* p = &position) {
				return HarfBuzzApi.hb_ot_metrics_get_position (font.Handle, metricsTag, p);
			}
		}

		public float GetVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_variation (font.Handle, metricsTag);

		public int GetXVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_x_variation (font.Handle, metricsTag);

		public int GetYVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_y_variation (font.Handle, metricsTag);
	}
}
