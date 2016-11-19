//
// Bindings for SKTypeface
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
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
		
		public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name (familyName, style));
		}

		public static SKTypeface FromFamilyName (string familyName, int weight, int width, SKFontStyleSlant slant)
		{
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name_with_font_style (familyName, weight, width, slant));
		}

		public static SKTypeface FromFamilyName (string familyName, SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
		{
			return FromFamilyName(familyName, (int)weight, (int)width, slant);
		}

		public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			if (typeface == null)
				throw new ArgumentNullException (nameof (typeface));
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_typeface (typeface.Handle, style));
		}

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_file (path, index));
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException (nameof (stream));
			var typeface = GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
			stream.RevokeOwnership (typeface);
			return typeface;
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

		public string FamilyName {
			get {
				return (string) GetObject<SKString> (SkiaApi.sk_typeface_get_family_name (Handle));
			}
		}

		public int FontWeight => SkiaApi.sk_typeface_get_font_weight (Handle);
		public int FontWidth => SkiaApi.sk_typeface_get_font_width (Handle);
		public SKFontStyleSlant FontSlant => SkiaApi.sk_typeface_get_font_slant (Handle);
		public SKTypefaceStyle Style => SkiaApi.sk_typeface_get_style (Handle);

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

	}
}

