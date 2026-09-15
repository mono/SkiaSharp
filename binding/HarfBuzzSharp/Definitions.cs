#nullable disable

using System;

namespace HarfBuzzSharp
{
	/// <summary>Represents a glyph and its relation to the input text.</summary>
	/// <remarks />
	public unsafe partial struct GlyphInfo
	{
		/// <summary>Gets the <see cref="T:HarfBuzzSharp.GlyphFlags" /> for this instance.</summary>
		/// <value>The glyph flags for this instance.</value>
		/// <remarks />
		public GlyphFlags GlyphFlags {
			get {
				fixed (GlyphInfo* f = &this) {
					return HarfBuzzApi.hb_glyph_info_get_glyph_flags (f);
				}
			}
		}
	}

	/// <summary>Specifies OpenType name table entry identifiers.</summary>
	/// <remarks />
	public enum OpenTypeNameId
	{
		/// <summary>Copyright notice (ID 0).</summary>
		Copyright = 0,
		/// <summary>Font family name (ID 1).</summary>
		FontFamily = 1,
		/// <summary>Font subfamily name (ID 2).</summary>
		FontSubfamily = 2,
		/// <summary>Unique font identifier (ID 3).</summary>
		UniqueId = 3,
		/// <summary>Full font name (ID 4).</summary>
		FullName = 4,
		/// <summary>Version string (ID 5).</summary>
		VersionString = 5,
		/// <summary>PostScript name (ID 6).</summary>
		PostscriptName = 6,
		/// <summary>Trademark notice (ID 7).</summary>
		Trademark = 7,
		/// <summary>Manufacturer name (ID 8).</summary>
		Manufacturer = 8,
		/// <summary>Designer name (ID 9).</summary>
		Designer = 9,
		/// <summary>Description (ID 10).</summary>
		Description = 10,
		/// <summary>Vendor URL (ID 11).</summary>
		VendorUrl = 11,
		/// <summary>Designer URL (ID 12).</summary>
		DesignerUrl = 12,
		/// <summary>License description (ID 13).</summary>
		License = 13,
		/// <summary>License URL (ID 14).</summary>
		LicenseUrl = 14,
		/// <summary>Typographic family name (ID 16).</summary>
		TypographicFamily = 16,
		/// <summary>Typographic subfamily name (ID 17).</summary>
		TypographicSubfamily = 17,
		/// <summary>Compatible full name for Mac (ID 18).</summary>
		MacFullName = 18,
		/// <summary>Sample text (ID 19).</summary>
		SampleText = 19,
		/// <summary>PostScript CID findfont name (ID 20).</summary>
		CidFindFontName = 20,
		/// <summary>WWS family name (ID 21).</summary>
		WwsFamily = 21,
		/// <summary>WWS subfamily name (ID 22).</summary>
		WwsSubfamily = 22,
		/// <summary>Light background palette (ID 23).</summary>
		LightBackground = 23,
		/// <summary>Dark background palette (ID 24).</summary>
		DarkBackground = 24,
		/// <summary>Variations PostScript name prefix (ID 25).</summary>
		VariationsPostscriptPrefix = 25,

		/// <summary>Invalid name ID.</summary>
		Invalid = 0xFFFF
	}
}
