#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>
	/// Represents a glyph and its relation to the input text.
	/// </summary>
	public unsafe partial struct GlyphInfo
	{
		/// <summary>
		/// Gets the <see cref="T:HarfBuzzSharp.GlyphFlags" /> for this instance.
		/// </summary>
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
		Copyright = 0,
		FontFamily = 1,
		FontSubfamily = 2,
		UniqueId = 3,
		FullName = 4,
		VersionString = 5,
		PostscriptName = 6,
		Trademark = 7,
		Manufacturer = 8,
		Designer = 9,
		Description = 10,
		VendorUrl = 11,
		DesignerUrl = 12,
		License = 13,
		LicenseUrl = 14,
		TypographicFamily = 16,
		TypographicSubfamily = 17,
		MacFullName = 18,
		SampleText = 19,
		CidFindFontName = 20,
		WwsFamily = 21,
		WwsSubfamily = 22,
		LightBackground = 23,
		DarkBackground = 24,
		VariationsPostscriptPrefix = 25,

		Invalid = 0xFFFF
	}
}
