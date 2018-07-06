using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	internal class SKString : SKObject
	{
		[Preserve]
		internal SKString (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKString ()
			: base (SkiaApi.sk_string_new_empty (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKString instance.");
			}
		}
		
		public SKString (byte [] src, long length)
			: base (SkiaApi.sk_string_new_with_copy (src, (IntPtr)length), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKString instance.");
			}
		}
		
		public SKString (byte [] src)
			: this (src, src.Length)
		{
		}
		
		public SKString (string str)
			: this (StringUtilities.GetEncodedText (str, SKTextEncoding.Utf8))
		{
		}
		
		public override string ToString ()
		{
			var cstr = SkiaApi.sk_string_get_c_str (Handle);
			var clen = SkiaApi.sk_string_get_size (Handle);
			return StringUtilities.GetString (cstr, (int)clen, SKTextEncoding.Utf8); 
		}

		public static explicit operator string (SKString skString)
		{
			return skString.ToString ();
		}
		
		internal static SKString Create (string str)
		{
			if (str == null) {
				return null;
			}
			return new SKString (str);
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_string_destructor (Handle);
			}

			base.Dispose (disposing);
		}
	}
}

