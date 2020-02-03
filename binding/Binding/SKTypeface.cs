using System;
using System.ComponentModel;
using System.IO;

namespace SkiaSharp
{
	[EditorBrowsable (EditorBrowsableState.Never)]
	[Flags]
	[Obsolete ("Use SKFontStyleWeight and SKFontStyleSlant instead.")]
	public enum SKTypefaceStyle
	{
		Normal = 0,
		Bold = 0x01,
		Italic = 0x02,
		BoldItalic = 0x03
	}

	public unsafe class SKTypeface : SKObject, ISKReferenceCounted
	{
		private static readonly Lazy<SKTypeface> defaultTypeface;

		static SKTypeface ()
		{
			defaultTypeface = new Lazy<SKTypeface> (() => new SKTypefaceStatic (SkiaApi.sk_typeface_ref_default ()));
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		[Preserve]
		internal SKTypeface (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// Default

		public static SKTypeface Default =>
			defaultTypeface.Value;

		public static SKTypeface CreateDefault () =>
			GetObject<SKTypeface> (SkiaApi.sk_typeface_create_default ());

		// FromXxx

		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant) =>
			FromFamilyName (familyName, new SKFontStyle (weight, width, slant));

		public static SKTypeface FromFamilyName (string familyName) =>
			FromFamilyName (familyName, SKFontStyle.Normal);

		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name (familyName, style.Handle));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant) =>
			FromFamilyName (familyName, (int)weight, (int)width, slant);

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			var utf8path = StringUtilities.GetEncodedText (path, SKTextEncoding.Utf8);
			fixed (byte* u = utf8path) {
				return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_file (u, index));
			}
		}

		public static SKTypeface FromStream (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			return FromStream (new SKManagedStream (stream, true), index);
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (stream is SKManagedStream managed) {
				stream = managed.ToMemoryStream ();
				managed.Dispose ();
			}

			var typeface = GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
		}

		public static SKTypeface FromData (SKData data, int index = 0)
		{
			if (data == null)
				throw new ArgumentNullException (nameof (data));

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_data (data.Handle, index));
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete]
		public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style)
		{
			if (typeface == null)
				throw new ArgumentNullException (nameof (typeface));

			var weight = style.HasFlag (SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
			var width = SKFontStyleWidth.Normal;
			var slant = style.HasFlag (SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

			return SKFontManager.Default.MatchTypeface (typeface, new SKFontStyle (weight, width, slant));
		}

		// Properties

		public string FamilyName => (string)GetObject<SKString> (SkiaApi.sk_typeface_get_family_name (Handle));

		public SKFontStyle FontStyle => GetObject<SKFontStyle> (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

		public int GlyphsCount => SkiaApi.sk_typeface_count_glyphs (Handle);

		public int TableTagsCount => SkiaApi.sk_typeface_count_tables (Handle);

		// GetTableTags

		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		public bool TryGetTableTags (out UInt32[] tags)
		{
			var buffer = new UInt32[TableTagsCount];
			if (TryGetTableTags (buffer)) {
				tags = null;
				return false;
			}
			tags = buffer;
			return true;
		}

		public bool TryGetTableTags (Span<UInt32> tags)
		{
			fixed (UInt32* b = tags) {
				return SkiaApi.sk_typeface_get_table_tags (Handle, b) != 0;
			}
		}

		// GetTableSize

		public int GetTableSize (UInt32 tag) =>
			(int)SkiaApi.sk_typeface_get_table_size (Handle, tag);

		// GetTableData

		public byte[] GetTableData (UInt32 tag)
		{
			if (!TryGetTableData (tag, out var result)) {
				throw new Exception ("Unable to read the data table.");
			}
			return result;
		}

		public bool TryGetTableData (UInt32 tag, out byte[] tableData)
		{
			var length = GetTableSize (tag);
			var buffer = new byte[length];
			if (!TryGetTableData (tag, 0, buffer)) {
				tableData = null;
				return false;
			}
			tableData = buffer;
			return true;
		}

		public bool TryGetTableData (UInt32 tag, int start, int length, IntPtr tableData) =>
			TryGetTableData (tag, start, tableData.AsSpan (length));

		public bool TryGetTableData (UInt32 tag, int start, Span<byte> tableData)
		{
			fixed (byte* b = tableData) {
				return SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)start, (IntPtr)tableData.Length, b) != IntPtr.Zero;
			}
		}

		// CharsToGlyphs

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
		public int CharsToGlyphs (string chars, out ushort[] glyphs)
		{
			glyphs = GetGlyphs (chars);
			return glyphs.Length;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(IntPtr, int, SKEncoding, out ushort[]) instead.")]
		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs)
		{
			glyphs = GetGlyphs (str, strlen, encoding.ToTextEncoding ());
			return glyphs.Length;
		}

		// GetGlyph

		public ushort GetGlyph (int codepoint) =>
			SkiaApi.sk_typeface_unichar_to_glyph (Handle, codepoint);

		// GetGlyphs

		public ushort[] GetGlyphs (ReadOnlySpan<int> codepoints)
		{
			var glyphs = new ushort[codepoints.Length];
			GetGlyphs (codepoints, glyphs);
			return glyphs;
		}

		public void GetGlyphs (ReadOnlySpan<int> codepoints, Span<ushort> glyphs)
		{
			fixed (int* up = codepoints)
			fixed (ushort* gp = glyphs) {
				SkiaApi.sk_typeface_unichars_to_glyphs (Handle, up, codepoints.Length, gp);
			}
		}

		// GetGlyphs

		public ushort[] GetGlyphs (string text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, encoding);
		}

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, length, encoding);
		}

		public void GetGlyphs (string text, Span<ushort> glyphs)
		{
			using var font = ToFont ();
			font.GetGlyphs (text, glyphs);
		}

		public void GetGlyphs (ReadOnlySpan<char> text, Span<ushort> glyphs)
		{
			using var font = ToFont ();
			font.GetGlyphs (text, glyphs);
		}

		public void GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding, Span<ushort> glyphs)
		{
			using var font = ToFont ();
			font.GetGlyphs (text, encoding, glyphs);
		}

		public void GetGlyphs (IntPtr text, int length, SKTextEncoding encoding, Span<ushort> glyphs)
		{
			using var font = ToFont ();
			font.GetGlyphs (text, length, encoding, glyphs);
		}

		// ContainsGlyph

		public bool ContainsGlyph (int codepoint) =>
			GetGlyph (codepoint) != 0;

		// ContainsGlyphs

		public bool ContainsGlyphs (ReadOnlySpan<int> codepoints) =>
			ContainsGlyphs (GetGlyphs (codepoints));

		public bool ContainsGlyphs (string text) =>
			ContainsGlyphs (GetGlyphs (text));

		public bool ContainsGlyphs (ReadOnlySpan<char> text) =>
			ContainsGlyphs (GetGlyphs (text));

		public bool ContainsGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding) =>
			ContainsGlyphs (GetGlyphs (text, encoding));

		public bool ContainsGlyphs (IntPtr text, int length, SKTextEncoding encoding) =>
			ContainsGlyphs (GetGlyphs (text, length, encoding));

		private bool ContainsGlyphs (ushort[] glyphs) =>
			Array.IndexOf (glyphs, 0) != -1;

		// CountGlyphs

		public int CountGlyphs (string text)
		{
			using var font = ToFont ();
			return font.CountGlyphs (text);
		}

		public int CountGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.CountGlyphs (text);
		}

		public int CountGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.CountGlyphs (text, encoding);
		}

		public int CountGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.CountGlyphs (text, length, encoding);
		}

		// ToFont

		public SKFont ToFont () =>
			new SKFont (this);

		public SKFont ToFont (float size, float scaleX = SKFont.DefaultScaleX, float skewX = SKFont.DefaultSkewX) =>
			new SKFont (this, size, scaleX, skewX);

		// OpenStream

		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		public SKStreamAsset OpenStream (out int ttcIndex)
		{
			fixed (int* ttc = &ttcIndex) {
				return GetObject<SKStreamAssetImplementation> (SkiaApi.sk_typeface_open_stream (Handle, ttc));
			}
		}

		// GetKerningPairAdjustments

		public int[] GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs)
		{
			var adjustments = new int[glyphs.Length];
			GetKerningPairAdjustments (glyphs, adjustments);
			return adjustments;
		}

		public void GetKerningPairAdjustments (ReadOnlySpan<ushort> glyphs, Span<int> adjustments)
		{
			fixed (ushort* gp = glyphs)
			fixed (int* ap = adjustments) {
				SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, gp, glyphs.Length, ap);
			}
		}

		private sealed class SKTypefaceStatic : SKTypeface
		{
			internal SKTypefaceStatic (IntPtr x)
				: base (x, false)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
