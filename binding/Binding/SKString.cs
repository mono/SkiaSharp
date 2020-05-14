using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	internal unsafe class SKString : SKObject, ISKSkipObjectRegistration
	{
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
			: base (CreateCopy (src, length), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKString instance.");
			}
		}
		
		private static IntPtr CreateCopy (byte [] src, long length)
		{
			fixed (byte* s = src) {
				return SkiaApi.sk_string_new_with_copy (s, (IntPtr)length);
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
			return StringUtilities.GetString ((IntPtr)cstr, (int)clen, SKTextEncoding.Utf8);
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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_string_destructor (Handle);

		internal static SKString GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKString (handle, true);
	}
}

