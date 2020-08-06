using System;

namespace HarfBuzzSharp
{
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
}
