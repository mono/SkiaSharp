using System;

namespace HarfBuzzSharp
{
	public unsafe partial struct Feature
	{
		private const int MaxFeatureStringSize = 128;

		public Feature (Tag tag)
			: this (tag, 1u, 0, uint.MaxValue)
		{
		}

		public Feature (Tag tag, uint value)
			: this (tag, value, 0, uint.MaxValue)
		{
		}

		public Feature (Tag tag, uint value, uint start, uint end)
		{
			this.tag = tag;
			this.value = value;
			this.start = start;
			this.end = end;
		}

		public override string ToString ()
		{
			var buffer = new char[MaxFeatureStringSize];
			fixed (Feature* f = &this)
			fixed (char* b = buffer) {
				HarfBuzzApi.hb_feature_to_string (f, b, (uint)buffer.Length);
				return new string (b);
			}
		}

		public static bool TryParse (string s, out Feature feature)
		{
			fixed (Feature* f = &feature) {
				return HarfBuzzApi.hb_feature_from_string (s, -1, f);
			}
		}

		public static Feature Parse (string s) =>
			TryParse (s, out var feature) ? feature : throw new FormatException ("Unrecognized feature string format.");
	}

	public unsafe partial struct GlyphInfo
	{
		public GlyphFlags GlyphFlags {
			get {
				fixed (GlyphInfo* f = &this) {
					return HarfBuzzApi.hb_glyph_info_get_glyph_flags (f);
				}
			}
		}
	}

	public enum OpenTypeNameId
	{
		COPYRIGHT = 0,
		FONT_FAMILY = 1,
		FONT_SUBFAMILY = 2,
		UNIQUE_ID = 3,
		FULL_NAME = 4,
		VERSION_STRING = 5,
		POSTSCRIPT_NAME = 6,
		TRADEMARK = 7,
		MANUFACTURER = 8,
		DESIGNER = 9,
		DESCRIPTION = 10,
		VENDOR_URL = 11,
		DESIGNER_URL = 12,
		LICENSE = 13,
		LICENSE_URL = 14,
		TYPOGRAPHIC_FAMILY = 16,
		TYPOGRAPHIC_SUBFAMILY = 17,
		MAC_FULL_NAME = 18,
		SAMPLE_TEXT = 19,
		CID_FINDFONT_NAME = 20,
		WWS_FAMILY = 21,
		WWS_SUBFAMILY = 22,
		LIGHT_BACKGROUND = 23,
		DARK_BACKGROUND = 24,
		VARIATIONS_PS_PREFIX = 25,

		INVALID = 0xFFFF
	}

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
