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
		private Tag tag;
		private uint val;
		private uint start;
		private uint end;

		public Feature (Tag tag, bool isEnabled = true, uint start = 0, uint end = uint.MaxValue)
		{
			this.tag = tag;
			this.val = isEnabled ? 1u : 0u;
			this.start = start;
			this.end = end;
		}

		public Tag Tag {
			get => tag;
			set => tag = value;
		}

		public bool IsEnabled {
			get => val == 1u;
			set => val = value ? 1u : 0u;
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
			const int AllocatedSize = 128;
			var buffer = new StringBuilder (AllocatedSize);
			HarfBuzzApi.hb_feature_to_string (ref this, buffer, AllocatedSize);
			return buffer.ToString ();
		}

		public static Feature FromString (string s)
		{
			if (HarfBuzzApi.hb_feature_from_string (s, -1, out var feature)) {
				return feature;
			}

			return new Feature(Tag.None);
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphInfo
	{
		private hb_codepoint_t codepoint;
		private hb_mask_t mask;
		private uint cluster;

		private hb_var_int_t var1;
		private hb_var_int_t var2;

		public hb_codepoint_t Codepoint {
			get { return codepoint; }
			set { codepoint = value; }
		}

		public hb_mask_t Mask {
			get { return mask; }
			set { mask = value; }
		}

		public uint Cluster {
			get { return cluster; }
			set { cluster = value; }
		}

		public GlyphFlags GlyphFlags {
			get {
				return HarfBuzzApi.hb_glyph_info_get_glyph_flags (ref this);
			}
		}
	}

	[StructLayout (LayoutKind.Sequential)]
	public struct GlyphPosition
	{
		private hb_position_t x_advance;
		private hb_position_t y_advance;
		private hb_position_t x_offset;
		private hb_position_t y_offset;

		/*< private >*/
		private hb_var_int_t var;

		public hb_position_t XAdvance {
			get { return x_advance; }
			set { x_advance = value; }
		}

		public hb_position_t YAdvance {
			get { return y_advance; }
			set { y_advance = value; }
		}

		public hb_position_t XOffset {
			get { return x_offset; }
			set { x_offset = value; }
		}

		public hb_position_t YOffset {
			get { return y_offset; }
			set { y_offset = value; }
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
	public enum Flags : uint
	{
		Default = 0x00000000u,
		Bot = 0x00000001u,
		Eot = 0x00000002u,
		PreserveDefaultIgnorables = 0x00000004u,
		RemoveDefaultIgnorables = 0x00000008u
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
		private hb_position_t ascender; /* typographic ascender. */

		private hb_position_t descender; /* typographic descender. */

		private hb_position_t line_gap; /* suggested line spacing gap. */

		/*< private >*/

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

	public struct Scale
	{
		public Scale (int x, int y)
		{
			X = x;
			Y = y;
		}

		public int X { get; }

		public int Y { get; }

		public override bool Equals (object obj)
		{
			var other = (Scale)obj;

			return X == other.X && Y == other.Y;
		}

		public override int GetHashCode ()
		{
			unchecked {
				return (X * 397) ^ Y;
			}
		}

		public override string ToString () => $"({X}, {Y})";
	}

	public enum UnicodeCombiningClass
	{
		NotReordered = 0,
		Overlay = 1,
		Nukta = 7,
		KanaVoicing = 8,
		Virama = 9,

		/* Hebrew */
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

		/* Arabic */
		CCC27 = 27,
		CCC28 = 28,
		CCC29 = 29,
		CCC30 = 30,
		CCC31 = 31,
		CCC32 = 32,
		CCC33 = 33,
		CCC34 = 34,
		CCC35 = 35,

		/* Syriac */
		CCC36 = 36,

		/* Telugu */
		CCC84 = 84,
		CCC91 = 91,

		/* Thai */
		CCC103 = 103,
		CCC107 = 107,

		/* Lao */
		CCC118 = 118,
		CCC122 = 122,

		/* Tibetan */
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
		Control,            /* Cc */
		Format,         /* Cf */
		Unassigned,     /* Cn */
		PrivateUse,        /* Co */
		Surrogate,      /* Cs */
		LowercaseLetter,       /* Ll */
		ModifierLetter,        /* Lm */
		OtherLetter,       /* Lo */
		TitlecaseLetter,       /* Lt */
		UppercaseLetter,       /* Lu */
		SpacingMark,       /* Mc */
		EnclosingMark,     /* Me */
		NonSpacingMark,       /* Mn */
		DecimalNumber,     /* Nd */
		LetterNumber,      /* Nl */
		OtherNumber,       /* No */
		ConnectPunctuation,    /* Pc */
		DashPunctuation,       /* Pd */
		ClosePunctuation,  /* Pe */
		FinalPunctuation,  /* Pf */
		InitialPunctuation,    /* Pi */
		OtherPunctuation,  /* Po */
		OpenPunctuation,       /* Ps */
		CurrencySymbol,        /* Sc */
		ModifierSymbol,        /* Sk */
		MathSymbol,        /* Sm */
		OtherSymbol,       /* So */
		LineSeparator,     /* Zl */
		ParagraphSeparator,    /* Zp */
		SpaceSeparator     /* Zs */
	}
}
