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
		internal SKTypeface (IntPtr handle)
			: base (handle)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				SkiaApi.sk_typeface_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			if (familyName == null)
				throw new ArgumentNullException ("familyName");
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_name (familyName, style));
		}

		public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			if (typeface == null)
				throw new ArgumentNullException ("typeface");
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_typeface (typeface.Handle, style));
		}

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_file (path, index));
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			return GetObject<SKTypeface> (SkiaApi.sk_typeface_create_from_stream (stream.Handle, index));
		}

		public int CountGlyphs (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("chars");
			
			unsafe {
				fixed (char *p = str) {
					return  SkiaApi.sk_typeface_chars_to_glyphs (Handle, (IntPtr)p, SKEncoding.Utf16, IntPtr.Zero, str.Length);
				}
			}
		}

		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding)
		{
			if (str == IntPtr.Zero)
				throw new ArgumentNullException ("str");

			return  SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, IntPtr.Zero, strLen);
		}

		public int CharsToGlyphs (string chars, out ushort [] glyphs)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			

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
			if (str == null)
				throw new ArgumentNullException ("str");


			unsafe {
				var n = SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, encoding, IntPtr.Zero, strlen);
				glyphs = new ushort[n];

				fixed (ushort *gp = &glyphs [0]){
					return SkiaApi.sk_typeface_chars_to_glyphs (Handle, str, SKEncoding.Utf16, (IntPtr) gp, n);
				}
			}
		}

		public string FamilyName {
			get {
				var r = SkiaApi.sk_typeface_get_family_name(Handle);
				try {
					var cstr = SkiaApi.sk_string_get_c_str(r);
					var clen = SkiaApi.sk_string_get_size(r);
					return Util.GetString(cstr, (int)clen, SKTextEncoding.Utf8); 
				} finally {
					SkiaApi.sk_string_destructor(r);
				}
			}
		}

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

