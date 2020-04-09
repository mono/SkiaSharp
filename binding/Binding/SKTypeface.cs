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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKTypeface Default => defaultTypeface.Value;

		public static SKTypeface CreateDefault ()
		{
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_default ());
		}

		// FromFamilyName

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FromFamilyName(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) instead.")]
		public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style)
		{
			var weight = style.HasFlag (SKTypefaceStyle.Bold) ? SKFontStyleWeight.Bold : SKFontStyleWeight.Normal;
			var slant = style.HasFlag (SKTypefaceStyle.Italic) ? SKFontStyleSlant.Italic : SKFontStyleSlant.Upright;

			return FromFamilyName (familyName, weight, SKFontStyleWidth.Normal, slant);
		}

		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, new SKFontStyle (weight, width, slant));
		}

		public static SKTypeface FromFamilyName (string familyName)
		{
			return FromFamilyName (familyName, SKFontStyle.Normal);
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyle style)
		{
			if (style == null)
				throw new ArgumentNullException (nameof (style));

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name (familyName, style.Handle));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName (familyName, (int)weight, (int)width, slant);
		}

		// From*

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

		// CharsToGlyphs

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
		public int CharsToGlyphs (string chars, out ushort[] glyphs) =>
			GetGlyphs (chars, out glyphs);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding, out ushort[]) instead.")]
		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort[] glyphs) =>
			GetGlyphs (str, strlen, encoding, out glyphs);

		// Properties

		public string FamilyName {
			get {
				using var str = new SKString (SkiaApi.sk_typeface_get_family_name (Handle), true);
				return (string)str;
			}
		}

		public SKFontStyle FontStyle => GetObject<SKFontStyle> (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

		public bool IsBold => FontStyle.Weight >= (int)SKFontStyleWeight.SemiBold;

		public bool IsItalic => FontStyle.Slant != SKFontStyleSlant.Upright;

		public bool IsFixedPitch => SkiaApi.sk_typeface_is_fixed_pitch (Handle);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use FontWeight and FontSlant instead.")]
		public SKTypefaceStyle Style {
			get {
				var style = SKTypefaceStyle.Normal;
				if (FontWeight >= (int)SKFontStyleWeight.SemiBold)
					style |= SKTypefaceStyle.Bold;
				if (FontSlant != (int)SKFontStyleSlant.Upright)
					style |= SKTypefaceStyle.Italic;
				return style;
			}
		}

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em (Handle);

		public int GlyphCount => SkiaApi.sk_typeface_count_glyphs (Handle);

		// GetTableTags

		public int TableCount => SkiaApi.sk_typeface_count_tables (Handle);

		public UInt32[] GetTableTags ()
		{
			if (!TryGetTableTags (out var result)) {
				throw new Exception ("Unable to read the tables for the file.");
			}
			return result;
		}

		public bool TryGetTableTags (out UInt32[] tags)
		{
			var buffer = new UInt32[TableCount];
			fixed (UInt32* b = buffer) {
				if (SkiaApi.sk_typeface_get_table_tags (Handle, b) == 0) {
					tags = null;
					return false;
				}
			}
			tags = buffer;
			return true;
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
			fixed (byte* b = buffer) {
				if (!TryGetTableData (tag, 0, length, (IntPtr)b)) {
					tableData = null;
					return false;
				}
			}
			tableData = buffer;
			return true;
		}

		public bool TryGetTableData (UInt32 tag, int offset, int length, IntPtr tableData)
		{
			var actual = SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)offset, (IntPtr)length, (byte*)tableData);
			return actual != IntPtr.Zero;
		}

		// CountGlyphs (string/char)

		public int CountGlyphs (string str) =>
			CountGlyphs (str.AsSpan ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CountGlyphs(string) instead.")]
		public int CountGlyphs (string str, SKEncoding encoding) =>
			CountGlyphs (str.AsSpan ());

		public int CountGlyphs (ReadOnlySpan<char> str)
		{
			using var font = ToFont ();
			return font.CountGlyphs (str);
		}

		// CountGlyphs (byte[])

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CountGlyphs(byte[], SKTextEncoding) instead.")]
		public int CountGlyphs (byte[] str, SKEncoding encoding) =>
			CountGlyphs (str.AsSpan (), encoding.ToTextEncoding ());

		public int CountGlyphs (byte[] str, SKTextEncoding encoding) =>
			CountGlyphs (str.AsSpan (), encoding);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CountGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
		public int CountGlyphs (ReadOnlySpan<byte> str, SKEncoding encoding) =>
			CountGlyphs (str, encoding.ToTextEncoding ());

		public int CountGlyphs (ReadOnlySpan<byte> str, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.CountGlyphs (str, encoding);
		}

		// CountGlyphs (IntPtr)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CountGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding) =>
			CountGlyphs (str, strLen, encoding.ToTextEncoding ());

		public int CountGlyphs (IntPtr str, int strLen, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.CountGlyphs (str, strLen, encoding);
		}

		// GetGlyph (int)

		public ushort GetGlyph (int codepoint) =>
			SkiaApi.sk_typeface_unichar_to_glyph (Handle, codepoint);

		// GetGlyphs (int)

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

		// GetGlyphs (string/char, out)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string) instead.")]
		public int GetGlyphs (string text, out ushort[] glyphs)
		{
			glyphs = GetGlyphs (text);
			return glyphs.Length;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string) instead.")]
		public int GetGlyphs (string text, SKEncoding encoding, out ushort[] glyphs) =>
			GetGlyphs (text, out glyphs);

		// GetGlyphs (byte[], out)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(byte[], SKTextEncoding) instead.")]
		public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs) =>
			GetGlyphs (text.AsSpan (), encoding, out glyphs);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
		public int GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs)
		{
			glyphs = GetGlyphs (text, encoding);
			return glyphs.Length;
		}

		// GetGlyphs (IntPtr, out)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
		public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort[] glyphs)
		{
			glyphs = GetGlyphs (text, length, encoding);
			return glyphs.Length;
		}

		// GetGlyphs (string/char, out)

		public ushort[] GetGlyphs (string text) =>
			GetGlyphs (text.AsSpan ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(string) instead.")]
		public ushort[] GetGlyphs (string text, SKEncoding encoding) =>
			GetGlyphs (text.AsSpan ());

		public ushort[] GetGlyphs (ReadOnlySpan<char> text)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text);
		}

		// GetGlyphs (byte[], out)

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
		public ushort[] GetGlyphs (byte[] text, SKEncoding encoding) =>
			GetGlyphs (text.AsSpan (), encoding.ToTextEncoding ());

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(ReadOnlySpan<byte>, SKTextEncoding) instead.")]
		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding) =>
			GetGlyphs (text, encoding.ToTextEncoding ());

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, encoding);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GetGlyphs(IntPtr, int, SKTextEncoding) instead.")]
		public ushort[] GetGlyphs (IntPtr text, int length, SKEncoding encoding) =>
			GetGlyphs (text, length, encoding.ToTextEncoding ());

		public ushort[] GetGlyphs (IntPtr text, int length, SKTextEncoding encoding)
		{
			using var font = ToFont ();
			return font.GetGlyphs (text, length, encoding);
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
			fixed (ushort* gp = glyphs)
			fixed (int* ap = adjustments) {
				SkiaApi.sk_typeface_get_kerning_pair_adjustments (Handle, gp, glyphs.Length, ap);
			}
			return adjustments;
		}

		//

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
