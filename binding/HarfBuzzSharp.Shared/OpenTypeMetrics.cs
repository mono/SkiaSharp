using System;

namespace HarfBuzzSharp
{
	public unsafe readonly struct OpenTypeMetrics
	{
		private readonly IntPtr font;

		public OpenTypeMetrics (IntPtr font)
		{
			this.font = font;
		}

		public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position)
		{
			fixed (int* p = &position) {
				return HarfBuzzApi.hb_ot_metrics_get_position (font, metricsTag, p);
			}
		}

		public float GetVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_variation (font, metricsTag);

		public int GetXVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_x_variation (font, metricsTag);

		public int GetYVariation (OpenTypeMetricsTag metricsTag) =>
			HarfBuzzApi.hb_ot_metrics_get_y_variation (font, metricsTag);
	}
}
