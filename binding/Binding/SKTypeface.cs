using System;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	[Flags]
	[Obsolete("Use SKFontStyleWeight and SKFontStyleSlant instead.")]
	public enum SKTypefaceStyle {
		Normal     = 0,
		Bold       = 0x01,
		Italic     = 0x02,
		BoldItalic = 0x03
	}

	public class SKTypeface : SKObject
	{
		[Preserve]
		internal SKTypeface (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_typeface_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public static SKTypeface Default => GetObject<SKTypeface> (SkiaApi.sk_typeface_ref_default ());

		public static SKTypeface CreateDefault ()
		{
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_default ());
		}

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

			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name_with_font_style (familyName, style.Handle));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName(familyName, (int)weight, (int)width, slant);
		}

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

			var utf8path = StringUtilities.GetEncodedText (path, SKEncoding.Utf8);
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_file(utf8path, index));
		}

		public static SKTypeface FromStream (Stream stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));

			if (!stream.CanSeek)
			{
				var fontStream = new MemoryStream ();
				stream.CopyTo (fontStream);
				fontStream.Flush ();
				fontStream.Position = 0;

				stream.Dispose ();
				stream = null;

				stream = fontStream;
				fontStream = null;
			}

			return FromStream (new SKManagedStream (stream, true), index);
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			var typeface = GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
			stream.RevokeOwnership ();
			return typeface;
		}

		public static SKTypeface FromData (SKData data, int index = 0)
		{
			return SKTypeface.FromStream (new SKMemoryStream (data), index);
		}

		[Obsolete ("Use GetGlyphs(string, out ushort[]) instead.")]
		public int CharsToGlyphs (string chars, out ushort[] glyphs)
			=> GetGlyphs (chars, out glyphs);

		[Obsolete ("Use GetGlyphs(IntPtr, int, SKEncoding, out ushort[]) instead.")]
		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort [] glyphs)
			=> GetGlyphs (str, strlen, encoding, out glyphs);

		public string FamilyName => (string)GetObject<SKString> (SkiaApi.sk_typeface_get_family_name (Handle));

		public SKFontStyle FontStyle => GetObject<SKFontStyle> (SkiaApi.sk_typeface_get_fontstyle (Handle));

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);

		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);

		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);

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

		public int UnitsPerEm => SkiaApi.sk_typeface_get_units_per_em(Handle);

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
			if (SkiaApi.sk_typeface_get_table_tags (Handle, buffer) == 0) {
				tags = null;
				return false;
			}
			tags = buffer;
			return true;
		}

		public int GetTableSize (UInt32 tag) =>
			(int)SkiaApi.sk_typeface_get_table_size (Handle, tag);

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
			unsafe {
				fixed (byte* b = buffer) {
					if (!TryGetTableData (tag, 0, length, (IntPtr)b)) {
						tableData = null;
						return false;
					}
				}
			}
			tableData = buffer;
			return true;
		}

		public bool TryGetTableData (UInt32 tag, int offset, int length, IntPtr tableData)
		{
			unsafe {
				var actual = SkiaApi.sk_typeface_get_table_data (Handle, tag, (IntPtr)offset, (IntPtr)length, (byte*)tableData);
				return actual != IntPtr.Zero;
			}
		}

		public int CountGlyphs (string str) => CountGlyphs (str, SKEncoding.Utf16);

		public int CountGlyphs (string str, SKEncoding encoding)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));

			var bytes = StringUtilities.GetEncodedText (str, encoding);
			return CountGlyphs (bytes, encoding);
		}

		public int CountGlyphs (byte[] str, SKEncoding encoding) =>
			CountGlyphs (new ReadOnlySpan<byte> (str), encoding);

		public int CountGlyphs (ReadOnlySpan<byte> str, SKEncoding encoding)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));

			unsafe {
				fixed (byte* p = str) {
					return CountGlyphs ((IntPtr)p, str.Length, encoding);
				}
			}
		}

		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding)
		{
			if (str == IntPtr.Zero && strLen != 0)
				throw new ArgumentNullException (nameof (str));

			unsafe {
				return SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, (ushort*)IntPtr.Zero, strLen);
			}
		}

		public int GetGlyphs (string text, out ushort [] glyphs) => GetGlyphs (text, SKEncoding.Utf16, out glyphs);

		public int GetGlyphs (string text, SKEncoding encoding, out ushort [] glyphs)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			var bytes = StringUtilities.GetEncodedText (text, encoding);
			return GetGlyphs (bytes, encoding, out glyphs);
		}

		public int GetGlyphs (byte[] text, SKEncoding encoding, out ushort[] glyphs) =>
			GetGlyphs (new ReadOnlySpan<byte> (text), encoding, out glyphs);

		public int GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding, out ushort[] glyphs)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				fixed (byte* p = text) {
					return GetGlyphs ((IntPtr)p, text.Length, encoding, out glyphs);
				}
			}
		}

		public int GetGlyphs (IntPtr text, int length, SKEncoding encoding, out ushort [] glyphs)
		{
			if (text == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (text));

			unsafe {
				var n = SkiaApi.sk_typeface_chars_to_glyphs (Handle, text, encoding, (ushort*)IntPtr.Zero, length);

				if (n <= 0) {
					glyphs = new ushort[0];
					return 0;
				}

				glyphs = new ushort[n];
				fixed (ushort* gp = glyphs) {
					return SkiaApi.sk_typeface_chars_to_glyphs (Handle, text, encoding, gp, n);
				}
			}
		}

		public ushort [] GetGlyphs (string text) => GetGlyphs (text, SKEncoding.Utf16);

		public ushort [] GetGlyphs (string text, SKEncoding encoding)
		{
			GetGlyphs (text, encoding, out var glyphs);
			return glyphs;
		}

		public ushort[] GetGlyphs (byte[] text, SKEncoding encoding) =>
			GetGlyphs (new ReadOnlySpan<byte> (text), encoding);

		public ushort[] GetGlyphs (ReadOnlySpan<byte> text, SKEncoding encoding)
		{
			GetGlyphs (text, encoding, out var glyphs);
			return glyphs;
		}

		public ushort [] GetGlyphs (IntPtr text, int length, SKEncoding encoding)
		{
			GetGlyphs (text, length, encoding, out var glyphs);
			return glyphs;
		}

		public SKStreamAsset OpenStream () =>
			OpenStream (out _);

		public SKStreamAsset OpenStream (out int ttcIndex) =>
			GetObject<SKStreamAssetImplementation> (SkiaApi.sk_typeface_open_stream (Handle, out ttcIndex));
	}
}
