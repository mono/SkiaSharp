//
// Bindings for SKPixelSerializer
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2017 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

#if __IOS__
using ObjCRuntime;
#endif

namespace SkiaSharp
{
	public abstract class SKManagedPixelSerializer : SKPixelSerializer
	{
		// delegate declarations
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate bool use_delegate (IntPtr serializer, IntPtr buffer, IntPtr size);
		[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
		internal delegate IntPtr encode_delegate (IntPtr serializer, IntPtr pixmap);

		// so the GC doesn't collect the delegate
		private static readonly use_delegate fUse;
		private static readonly encode_delegate fEncode;

		static SKManagedPixelSerializer ()
		{
			fUse = new use_delegate (UseInternal);
			fEncode = new encode_delegate (EncodeInternal);

			SkiaApi.sk_managedpixelserializer_set_delegates (
				Marshal.GetFunctionPointerForDelegate (fUse),
				Marshal.GetFunctionPointerForDelegate (fEncode));
		}

		[Preserve]
		internal SKManagedPixelSerializer (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SKManagedPixelSerializer ()
			: base (SkiaApi.sk_managedpixelserializer_new (), true)
		{
		}


		protected abstract bool OnUseEncodedData (IntPtr data, IntPtr length);

		protected abstract SKData OnEncode (SKPixmap pixmap);


		// internal proxy
		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (use_delegate))]
		#endif
		private static bool UseInternal (IntPtr cserializer, IntPtr buffer, IntPtr size)
		{
			var serializer = GetObject<SKManagedPixelSerializer> (cserializer, false);
			return serializer.OnUseEncodedData (buffer, size);
		}

		#if __IOS__
		[ObjCRuntime.MonoPInvokeCallback (typeof (SKBitmapReleaseDelegateInternal))]
		#endif
		private static IntPtr EncodeInternal (IntPtr cserializer, IntPtr pixmap)
		{
			var serializer = GetObject<SKManagedPixelSerializer> (cserializer, false);
			var data = serializer.OnEncode (GetObject<SKPixmap> (pixmap, false));
			return data == null ? IntPtr.Zero : data.Handle;
		}
	}
}
