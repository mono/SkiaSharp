#nullable disable

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

	/// <summary>
	/// OpenType layout table tags.
	/// </summary>
	public enum OpenTypeLayoutTableTag : uint
	{
		/// <summary>
		/// The Glyph Substitution (GSUB) table.
		/// </summary>
		Gsub = ('G' << 24) | ('S' << 16) | ('U' << 8) | 'B',

		/// <summary>
		/// The Glyph Positioning (GPOS) table.
		/// </summary>
		Gpos = ('G' << 24) | ('P' << 16) | ('O' << 8) | 'S',
	}

	/// <summary>
	/// Information about an OpenType layout feature's names.
	/// </summary>
	public struct OpenTypeFeatureNameIds
	{
		/// <summary>
		/// The name ID for the feature's label (user-visible name).
		/// </summary>
		public OpenTypeNameId LabelId { get; set; }

		/// <summary>
		/// The name ID for the feature's tooltip.
		/// </summary>
		public OpenTypeNameId TooltipId { get; set; }

		/// <summary>
		/// The name ID for sample text demonstrating the feature.
		/// </summary>
		public OpenTypeNameId SampleId { get; set; }

		/// <summary>
		/// The number of named parameters for this feature.
		/// </summary>
		public int NumNamedParameters { get; set; }

		/// <summary>
		/// The name ID of the first named parameter.
		/// </summary>
		public OpenTypeNameId FirstParamId { get; set; }
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
