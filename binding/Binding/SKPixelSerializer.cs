using System;

namespace SkiaSharp
{
	[Obsolete]
	public abstract class SKPixelSerializer : SKObject
	{
		protected SKPixelSerializer ()
			: base (IntPtr.Zero, false)
		{
		}

		public bool UseEncodedData (IntPtr data, ulong length)
		{
			if (SKObject.SizeOf<IntPtr> () == 4 && length > UInt32.MaxValue)
				throw new ArgumentOutOfRangeException (nameof (length), "The length exceeds the size of pointers.");

			return OnUseEncodedData (data, (IntPtr)length);
		}

		public SKData Encode (SKPixmap pixmap)
		{
			if (pixmap == null)
				throw new ArgumentNullException (nameof (pixmap));

			return OnEncode (pixmap);
		}

		protected abstract bool OnUseEncodedData (IntPtr data, IntPtr length);

		protected abstract SKData OnEncode (SKPixmap pixmap);

		public static SKPixelSerializer Create (Func<SKPixmap, SKData> onEncode)
		{
			return new SKSimplePixelSerializer (null, onEncode);
		}

		public static SKPixelSerializer Create (Func<IntPtr, IntPtr, bool> onUseEncodedData, Func<SKPixmap, SKData> onEncode)
		{
			return new SKSimplePixelSerializer (onUseEncodedData, onEncode);
		}
	}

	[Obsolete]
	internal class SKSimplePixelSerializer : SKPixelSerializer
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

	[Obsolete]
	public abstract class SKManagedPixelSerializer : SKPixelSerializer
	{
		public SKManagedPixelSerializer()
		{
		}
	}
}
