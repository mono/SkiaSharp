using System;

namespace SkiaSharp
{
	internal unsafe class SKString : SKObject
	{
		[Preserve]
		internal SKString (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKString ()
			: base (SkiaApi.sk_string_new_empty (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKString instance.");
		}

		public SKString (string str)
			: this (StringUtilities.GetEncodedText (str, SKTextEncoding.Utf8))
		{
		}

		public SKString (ReadOnlySpan<byte> src)
			: this (CreateCopy (src), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKString instance.");
		}

		public SKString (ReadOnlySpan<byte> src, int length)
			: this (CreateCopy (src.Slice (0, length)), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKString instance.");
		}

		private static IntPtr CreateCopy (ReadOnlySpan<byte> src)
		{
			fixed (byte* s = src) {
				return SkiaApi.sk_string_new_with_copy (s, (IntPtr)src.Length);
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_string_destructor (Handle);

		public int Length =>
			(int)SkiaApi.sk_string_get_size (Handle);

		public override string ToString ()
		{
			var ptr = SkiaApi.sk_string_get_c_str (Handle);
			var span = new ReadOnlySpan<byte> (ptr, Length);
			return StringUtilities.GetString (span, SKTextEncoding.Utf8);
		}

		public static explicit operator string (SKString str) =>
			str.ToString ();

		public static SKString Create (string str) =>
			str == null ? null : new SKString (str);
	}
}
