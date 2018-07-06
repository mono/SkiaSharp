using System;

namespace SkiaSharp
{
	public class SKPixelSerializer : SKObject
	{
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_pixelserializer_unref (Handle);
			}

			base.Dispose (disposing);
		}

		[Preserve]
		internal SKPixelSerializer (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public bool UseEncodedData (IntPtr data, ulong length)
		{
			if (SizeOf<IntPtr> () == 4 && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");

			return SkiaApi.sk_pixelserializer_use_encoded_data (Handle, data, (IntPtr)length);
		}

		public SKData Encode (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return GetObject<SKData> (SkiaApi.sk_pixelserializer_encode (Handle, pixmap.Handle));
		}

		public static SKPixelSerializer Create (Func<SKPixmap, SKData> onEncode)
		{
			return new SKSimplePixelSerializer (null, onEncode);
		}

		public static SKPixelSerializer Create (Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode)
		{
			return new SKSimplePixelSerializer (onUseEncodedData, onEncode);
		}
	}

	internal class SKSimplePixelSerializer : SKManagedPixelSerializer
	{
		private readonly Func<IntPtr, IntPtr, bool> onUseEncodedData;
		private readonly Func<SKPixmap, SKData> onEncode;

		public SKSimplePixelSerializer (Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode)
		{
			this.onUseEncodedData = onUseEncodedData;
			this.onEncode = onEncode;
		}

		protected override SKData OnEncode (SKPixmap pixmap)
		{
			return onEncode?.Invoke (pixmap) ?? null;
		}

		protected override bool OnUseEncodedData (IntPtr data, IntPtr length)
		{
			return onUseEncodedData?.Invoke (data, length) ?? false;
		}
	}
}
