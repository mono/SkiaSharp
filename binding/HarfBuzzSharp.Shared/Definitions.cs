using System;
using System.Runtime.InteropServices;
using System.Text;
using hb_codepoint_t = System.UInt32;
using hb_mask_t = System.UInt32;
using hb_position_t = System.Int32;
using hb_var_int_t = System.Int32;

namespace HarfBuzzSharp
{
	public enum MemoryMode
	{
		Duplicate,
		ReadOnly,
		Writeable,
		ReadOnlyMayMakeWriteable
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct Feature
	{
		private const int MaxFeatureStringSize = 128;

		private Tag tag;
		private uint val;
		private uint start;
		private uint end;

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
			this.val = value;
			this.start = start;
			this.end = end;
		}

		public Tag Tag {
			get => tag;
			set => tag = value;
		}

		public uint Value {
			get => val;
			set => val = value;
		}

		public uint Start {
			get => start;
			set => start = value;
		}

		public uint End {
			get => end;
			set => end = value;
		}

		public override string ToString ()
		{
			var buffer = new StringBuilder (MaxFeatureStringSize);
			HarfBuzzApi.hb_feature_to_string (ref this, buffer, MaxFeatureStringSize);
			return buffer.ToString ();
		}

		public static bool TryParse (string s, out Feature feature) =>
			HarfBuzzApi.hb_feature_from_string (s, -1, out feature);

		public static Feature Parse (string s) =>
			TryParse (s, out var feature) ? feature : throw new FormatException ("Unrecognized feature string format.");
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphInfo
	{
		private hb_codepoint_t codepoint;
		private hb_mask_t mask;
		private uint cluster;

		// < private >
		private hb_var_int_t var1;
		private hb_var_int_t var2;

		public hb_codepoint_t Codepoint {
			get => codepoint;
			set => codepoint = value;
		}

		public hb_mask_t Mask {
			get => mask;
			set => mask = value;
		}

		public uint Cluster {
			get => cluster;
			set => cluster = value;
		}

		public GlyphFlags GlyphFlags =>
			HarfBuzzApi.hb_glyph_info_get_glyph_flags (ref this);
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphPosition
	{
		private hb_position_t x_advance;
		private hb_position_t y_advance;
		private hb_position_t x_offset;
		private hb_position_t y_offset;

		// < private >
		private hb_var_int_t var;

		public hb_position_t XAdvance {
			get => x_advance;
			set => x_advance = value;
		}

		public hb_position_t YAdvance {
			get => y_advance;
			set => y_advance = value;
		}

		public hb_position_t XOffset {
			get => x_offset;
			set => x_offset = value;
		}

		public hb_position_t YOffset {
			get => y_offset;
			set => y_offset = value;
		}
	}

	public enum Direction
	{
		Invalid = 0,

		LeftToRight = 4,
		RightToLeft,
		TopToBottom,
		BottomToTop
	}

	public enum ClusterLevel
	{
		MonotoneGraphemes = 0,
		MonotoneCharacters = 1,
		Characters = 2,
		Default = MonotoneGraphemes
	}

	[Flags]
	public enum BufferFlags : uint
	{
		Default = 0x00000000u,
		BeginningOfText = 0x00000001u,
		EndOfText = 0x00000002u,
		PreserveDefaultIgnorables = 0x00000004u,
		RemoveDefaultIgnorables = 0x00000008u,
		DoNotInsertDottedCircle = 0x00000010u,
	}

	public enum ContentType
	{
		Invalid = 0,
		Unicode,
		Glyphs
	}

	[Flags]
	public enum GlyphFlags
	{
		UnsafeToBreak = 0x00000001,
		Defined = 0x00000001
	}

	[Flags]
	public enum SerializeFlag : uint
	{
		Default = 0x00000000u,
		NoClusters = 0x00000001u,
		NoPositions = 0x00000002u,
		NoGlyphNames = 0x00000004u,
		GlyphExtents = 0x00000008u,
		GlyphFlags = 0x00000010u,
		NoAdvances = 0x00000020u
	}

	public enum SerializeFormat : uint
	{
		Text = (((byte)'T' << 24) | ((byte)'E' << 16) | ((byte)'X' << 8) | (byte)'T'),
		Json = (((byte)'J' << 24) | ((byte)'S' << 16) | ((byte)'O' << 8) | (byte)'N'),
		Invalid = 0
	}

	public struct GlyphExtents
	{
		hb_position_t x_bearing; /* left side of glyph from origin. */
		hb_position_t y_bearing; /* top side of glyph from origin. */
		hb_position_t width; /* distance from left to right side. */
		hb_position_t height; /* distance from top to bottom side. */

		public hb_position_t XBearing {
			get => x_bearing;
			set => x_bearing = value;
		}
		public hb_position_t YBearing {
			get => y_bearing;
			set => y_bearing = value;
		}
		public hb_position_t Width {
			get => width;
			set => width = value;
		}
		public hb_position_t Height {
			get => height;
			set => height = value;
		}
	}

	public struct FontExtents
	{
		private hb_position_t ascender; // typographic ascender.
		private hb_position_t descender; // typographic descender.
		private hb_position_t line_gap; // suggested line spacing gap.

		// < private >
		private hb_position_t reserved9;
		private hb_position_t reserved8;
		private hb_position_t reserved7;
		private hb_position_t reserved6;
		private hb_position_t reserved5;
		private hb_position_t reserved4;
		private hb_position_t reserved3;
		private hb_position_t reserved2;
		private hb_position_t reserved1;

		public hb_position_t Ascender {
			get => ascender;
			set => ascender = value;
		}

		public hb_position_t Descender {
			get => descender;
			set => descender = value;
		}

		public hb_position_t LineGap {
			get => line_gap;
			set => line_gap = value;
		}
	}

	public enum UnicodeCombiningClass
	{
		NotReordered = 0,
		Overlay = 1,
		Nukta = 7,
		KanaVoicing = 8,
		Virama = 9,

		// Hebrew
		CCC10 = 10,
		CCC11 = 11,
		CCC12 = 12,
		CCC13 = 13,
		CCC14 = 14,
		CCC15 = 15,
		CCC16 = 16,
		CCC17 = 17,
		CCC18 = 18,
		CCC19 = 19,
		CCC20 = 20,
		CCC21 = 21,
		CCC22 = 22,
		CCC23 = 23,
		CCC24 = 24,
		CCC25 = 25,
		CCC26 = 26,

		// Arabic
		CCC27 = 27,
		CCC28 = 28,
		CCC29 = 29,
		CCC30 = 30,
		CCC31 = 31,
		CCC32 = 32,
		CCC33 = 33,
		CCC34 = 34,
		CCC35 = 35,

		// Syriac
		CCC36 = 36,

		// Telugu
		CCC84 = 84,
		CCC91 = 91,

		// Thai
		CCC103 = 103,
		CCC107 = 107,

		// Lao
		CCC118 = 118,
		CCC122 = 122,

		// Tibetan
		CCC129 = 129,
		CCC130 = 130,
		CCC133 = 132,

		AttachedBelowLeft = 200,
		AttachedBelow = 202,
		AttachedAbove = 214,
		AttachedAboveRight = 216,
		BelowLeft = 218,
		Below = 220,
		BelowRight = 222,
		Left = 224,
		Right = 226,
		AboveLeft = 228,
		Above = 230,
		AboveRight = 232,
		DoubleBelow = 233,
		DoubleAbove = 234,
		IotaSubscript = 240,

		Invalid = 255
	}

	public enum UnicodeGeneralCategory
	{
		Control,              // Cc
		Format,               // Cf
		Unassigned,           // Cn
		PrivateUse,           // Co
		Surrogate,            // Cs

		LowercaseLetter,      // Ll
		ModifierLetter,       // Lm
		OtherLetter,          // Lo
		TitlecaseLetter,      // Lt
		UppercaseLetter,      // Lu

		SpacingMark,          // Mc
		EnclosingMark,        // Me
		NonSpacingMark,       // Mn

		DecimalNumber,        // Nd
		LetterNumber,         // Nl
		OtherNumber,          // No

		ConnectPunctuation,   // Pc
		DashPunctuation,      // Pd
		ClosePunctuation,     // Pe
		FinalPunctuation,     // Pf
		InitialPunctuation,   // Pi
		OtherPunctuation,     // Po
		OpenPunctuation,      // Ps

		CurrencySymbol,       // Sc
		ModifierSymbol,       // Sk
		MathSymbol,           // Sm
		OtherSymbol,          // So

		LineSeparator,        // Zl
		ParagraphSeparator,   // Zp
		SpaceSeparator        // Zs
	}

	public enum OpenTypeMetricsTag : uint
	{
		HorizontalAscender = (((byte)'h' << 24) | ((byte)'a' << 16) | ((byte)'s' << 8) | (byte)'c'),
		HorizontalDescender = (((byte)'h' << 24) | ((byte)'d' << 16) | ((byte)'s' << 8) | (byte)'c'),
		HorizontalLineGap = (((byte)'h' << 24) | ((byte)'l' << 16) | ((byte)'g' << 8) | (byte)'p'),
		HorizontalClippingAscent = (((byte)'h' << 24) | ((byte)'c' << 16) | ((byte)'l' << 8) | (byte)'a'),
		HorizontalClippingDescent = (((byte)'h' << 24) | ((byte)'c' << 16) | ((byte)'l' << 8) | (byte)'d'),

		VerticalAscender = (((byte)'v' << 24) | ((byte)'a' << 16) | ((byte)'s' << 8) | (byte)'c'),
		VerticalDescender = (((byte)'v' << 24) | ((byte)'d' << 16) | ((byte)'s' << 8) | (byte)'c'),
		VerticalLineGap = (((byte)'v' << 24) | ((byte)'l' << 16) | ((byte)'g' << 8) | (byte)'p'),

		HorizontalCaretRise = (((byte)'h' << 24) | ((byte)'c' << 16) | ((byte)'r' << 8) | (byte)'s'),
		HorizontalCaretRun = (((byte)'h' << 24) | ((byte)'c' << 16) | ((byte)'r' << 8) | (byte)'n'),
		HorizontalCaretOffset = (((byte)'h' << 24) | ((byte)'c' << 16) | ((byte)'o' << 8) | (byte)'f'),

		VerticalCaretRise = (((byte)'v' << 24) | ((byte)'c' << 16) | ((byte)'r' << 8) | (byte)'s'),
		VerticalCaretRun = (((byte)'v' << 24) | ((byte)'c' << 16) | ((byte)'r' << 8) | (byte)'n'),
		VerticalCaretOffset = (((byte)'v' << 24) | ((byte)'c' << 16) | ((byte)'o' << 8) | (byte)'f'),

		XHeight = (((byte)'x' << 24) | ((byte)'h' << 16) | ((byte)'g' << 8) | (byte)'t'),

		CapHeight = (((byte)'c' << 24) | ((byte)'p' << 16) | ((byte)'h' << 8) | (byte)'t'),

		SubScriptEmXSize = (((byte)'s' << 24) | ((byte)'b' << 16) | ((byte)'x' << 8) | (byte)'s'),
		SubScriptEmYSize = (((byte)'s' << 24) | ((byte)'b' << 16) | ((byte)'y' << 8) | (byte)'s'),
		SubScriptEmXOffset = (((byte)'s' << 24) | ((byte)'b' << 16) | ((byte)'x' << 8) | (byte)'o'),
		SubScriptEmYOffset = (((byte)'s' << 24) | ((byte)'b' << 16) | ((byte)'y' << 8) | (byte)'o'),

		SuperScriptEmXSize = (((byte)'s' << 24) | ((byte)'p' << 16) | ((byte)'x' << 8) | (byte)'s'),
		SuperScriptEmYSize = (((byte)'s' << 24) | ((byte)'p' << 16) | ((byte)'y' << 8) | (byte)'s'),
		SuperScriptEmXOffset = (((byte)'s' << 24) | ((byte)'p' << 16) | ((byte)'x' << 8) | (byte)'o'),
		SuperScriptEmYOffset = (((byte)'s' << 24) | ((byte)'p' << 16) | ((byte)'y' << 8) | (byte)'o'),

		StrikeoutSize = (((byte)'s' << 24) | ((byte)'t' << 16) | ((byte)'r' << 8) | (byte)'s'),
		StrikeoutOffset = (((byte)'s' << 24) | ((byte)'t' << 16) | ((byte)'r' << 8) | (byte)'o'),

		UnderlineSize = (((byte)'u' << 24) | ((byte)'n' << 16) | ((byte)'d' << 8) | (byte)'s'),
		UnderlineOffset = (((byte)'u' << 24) | ((byte)'n' << 16) | ((byte)'d' << 8) | (byte)'o'),
	}

	public readonly struct OpenTypeMetrics
	{
		private readonly IntPtr font;

		public OpenTypeMetrics (IntPtr font)
		{
			this.font = font;
		}

		public bool TryGetPosition (OpenTypeMetricsTag metricsTag, out int position) => HarfBuzzApi.hb_ot_metrics_get_position (font, metricsTag, out position);

		public float GetVariation (OpenTypeMetricsTag metricsTag) => HarfBuzzApi.hb_ot_metrics_get_variation (font, metricsTag);

		public int GetXVariation (OpenTypeMetricsTag metricsTag) => HarfBuzzApi.hb_ot_metrics_get_x_variation (font, metricsTag);

		public int GetYVariation (OpenTypeMetricsTag metricsTag) => HarfBuzzApi.hb_ot_metrics_get_y_variation (font, metricsTag);
	}
}
