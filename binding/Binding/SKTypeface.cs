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
			using (var stream = SKFileStream.OpenStream (path)) {
				if (stream == null) {
					return null;
				} else {
					return FromStream (stream, index);
				}
			}
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

		public int CountGlyphs (string str)
		{
			if (str == null)
				throw new ArgumentNullException (nameof (str));
			
			unsafe {
				fixed (char *p = str) {
					return  SkiaApi.sk_typeface_chars_to_glyphs (Handle, (IntPtr)p, SKEncoding.Utf16, IntPtr.Zero, str.Length);
				}
			}
		}

		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding)
		{
			if (str == IntPtr.Zero)
				throw new ArgumentNullException (nameof (str));

			return  SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, IntPtr.Zero, strLen);
		}

		public int CharsToGlyphs (string chars, out ushort [] glyphs)
		{
			if (chars == null)
				throw new ArgumentNullException (nameof (chars));

			unsafe {
				fixed (char *p = chars){
					var n = SkiaApi.sk_typeface_chars_to_glyphs (Handle, (IntPtr) p, SKEncoding.Utf16, IntPtr.Zero, chars.Length);
					glyphs = new ushort[n];

					fixed (ushort *gp = &glyphs [0]){
						return SkiaApi.sk_typeface_chars_to_glyphs (Handle, (IntPtr) p, SKEncoding.Utf16, (IntPtr) gp, n);
					}
				}
			}
		}

		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort [] glyphs)
		{
			if (str == IntPtr.Zero)
				throw new ArgumentNullException (nameof (str));

			unsafe {
				var n = SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, IntPtr.Zero, strlen);
				glyphs = new ushort[n];

				fixed (ushort *gp = &glyphs [0]){
					return SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, (IntPtr) gp, n);
				}
			}
		}

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

		public UInt32[] GetTableTags()
		{
			int tableCount = SkiaApi.sk_typeface_count_tables(Handle);
			UInt32[] result = new UInt32[tableCount];
			int r = SkiaApi.sk_typeface_get_table_tags(Handle, result);
			if (r == 0) {
				throw new Exception("Unable to read the tables for the file.");
			}
			return result;
		}

		public byte[] GetTableData(UInt32 tag)
		{
			IntPtr dataSize = SkiaApi.sk_typeface_get_table_size(Handle, tag);
			byte[] result = new byte[(int)dataSize];
			IntPtr r = SkiaApi.sk_typeface_get_table_data(Handle, tag, IntPtr.Zero, dataSize, result);
			if (r == IntPtr.Zero) {
				throw new Exception("Unable to read the data table.");
			}
			return result;
		}

		public bool TryGetTableData(UInt32 tag, out byte[] tableData)
		{
			IntPtr dataSize = SkiaApi.sk_typeface_get_table_size(Handle, tag);
			tableData = new byte[(int)dataSize];
			IntPtr r = SkiaApi.sk_typeface_get_table_data(Handle, tag, IntPtr.Zero, dataSize, tableData);
			if (r == IntPtr.Zero) {
				tableData = null;
				return false;
			}
			return true;
		}

		public SKStreamAsset OpenStream()
		{
			int ttcIndex;
			return OpenStream(out ttcIndex);
		}

		public SKStreamAsset OpenStream(out int ttcIndex)
		{
			return GetObject<SKStreamAssetImplementation>(SkiaApi.sk_typeface_open_stream(Handle, out ttcIndex));
		}
	}
}
