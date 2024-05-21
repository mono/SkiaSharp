using System;
using System.Diagnostics.CodeAnalysis;
#if NETSTANDARD1_3 || WINDOWS_UWP
using System.Reflection;
#endif

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

		public SKString (ReadOnlySpan<byte> src, long length)
			: base (CreateCopy (src, length), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKString instance.");
			}
		}

		private static IntPtr CreateCopy (ReadOnlySpan<byte> src, long length)
		{
			if (length > src.Length)
				throw new ArgumentOutOfRangeException (nameof (length));

			fixed (byte* s = src) {
				return SkiaApi.sk_string_new_with_copy (s, (IntPtr)length);
			}
		}

		public SKString (ReadOnlySpan<byte> src)
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

		[return: NotNullIfNotNull (nameof (str))]
		internal static SKString? Create (string? str)
		{
			if (str is null) {
				return null;
			}
			return new SKString (str);
		}

		internal static SKStringRaw CreateRaw (string? str) =>
			new SKStringRaw (str);

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_string_destructor (Handle);

		internal static SKString? GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKString (handle, true);

		internal readonly ref struct SKStringRaw
		{
			public SKStringRaw ()
			{
				Handle = IntPtr.Zero;
			}

			public SKStringRaw (string? text)
				: this (text.AsSpan ())
			{
			}

			public SKStringRaw (ReadOnlySpan<char> text)
			{
				if (text.Length == 0) {
					Handle = IntPtr.Zero;
					return;
				}

				var bufferSize = StringUtilities.GetMaxByteCount (text, SKTextEncoding.Utf8);
				var buffer = stackalloc byte[bufferSize];
				var bufferSpan = new Span<byte> (buffer, bufferSize);

				var bytesSize = StringUtilities.GetEncodedText (text, bufferSpan, SKTextEncoding.Utf8);

				Handle = SkiaApi.sk_string_new_with_copy (buffer, (IntPtr)bytesSize);
			}

			public readonly IntPtr Handle;

			public void Dispose ()
			{
				if (Handle != IntPtr.Zero)
					SkiaApi.sk_string_destructor (Handle);
			}
		}
	}
}

