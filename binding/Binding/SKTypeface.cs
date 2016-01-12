//
// Bindings for SKTypeface
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKTypeface : IDisposable
	{
		internal IntPtr handle;
		bool owns;

		internal SKTypeface (IntPtr handle, bool owns)
		{
			this.owns = owns;
			this.handle = handle;
		}

		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				if (owns)
					SkiaApi.sk_typeface_unref (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKTypeface()
		{
			Dispose (false);
		}

		public static SKTypeface FromFamilyName (string familyName, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			if (familyName == null)
				throw new ArgumentNullException ("familyName");
			return new SKTypeface (SkiaApi.sk_typeface_create_from_name (familyName, style), owns: true);
		}

		public static SKTypeface FromTypeface (SKTypeface typeface, SKTypefaceStyle style = SKTypefaceStyle.Normal)
		{
			if (typeface == null)
				throw new ArgumentNullException ("typeface");
			return new SKTypeface (SkiaApi.sk_typeface_create_from_typeface (typeface.handle, style), owns: true);
		}

		public static SKTypeface FromFile (string path, int index = 0)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			return new SKTypeface (SkiaApi.sk_typeface_create_from_file (path, index), owns: true);
		}

		public static SKTypeface FromStream (SKStreamAsset stream, int index = 0)
		{
			if (stream == null)
				throw new ArgumentNullException ("stream");
			return new SKTypeface (SkiaApi.sk_typeface_create_from_stream (stream.handle, index), owns: true);
		}

		public int CountGlyphs (string str)
		{
			if (str == null)
				throw new ArgumentNullException ("chars");
			
			unsafe {
				fixed (char *p = str) {
					return  SkiaApi.sk_typeface_chars_to_glyphs (handle, (IntPtr)p, SKEncoding.Utf16, IntPtr.Zero, str.Length);
				}
			}
		}

		public int CountGlyphs (IntPtr str, int strLen, SKEncoding encoding)
		{
			if (str == IntPtr.Zero)
				throw new ArgumentNullException ("str");

			return  SkiaApi.sk_typeface_chars_to_glyphs (handle, str, encoding, IntPtr.Zero, strLen);
		}

		public int CharsToGlyphs (string chars, out ushort [] glyphs)
		{
			if (chars == null)
				throw new ArgumentNullException ("chars");
			

			unsafe {
				fixed (char *p = chars){
					var n = SkiaApi.sk_typeface_chars_to_glyphs (handle, (IntPtr) p, SKEncoding.Utf16, IntPtr.Zero, chars.Length);
					glyphs = new ushort[n];

					fixed (ushort *gp = &glyphs [0]){
						return SkiaApi.sk_typeface_chars_to_glyphs (handle, (IntPtr) p, SKEncoding.Utf16, (IntPtr) gp, n);
					}
				}
			}
		}

		public int CharsToGlyphs (IntPtr str, int strlen, SKEncoding encoding, out ushort [] glyphs)
		{
			if (str == null)
				throw new ArgumentNullException ("str");


			unsafe {
				var n = SkiaApi.sk_typeface_chars_to_glyphs (handle, str, encoding, IntPtr.Zero, strlen);
				glyphs = new ushort[n];

				fixed (ushort *gp = &glyphs [0]){
					return SkiaApi.sk_typeface_chars_to_glyphs (handle, str, SKEncoding.Utf16, (IntPtr) gp, n);
				}
			}
		}
	}
}

