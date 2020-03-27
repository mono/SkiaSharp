using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject
	{
		[Preserve]
		internal SKFontStyle (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKFontStyle ()
			: this (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
		{
		}

		public SKFontStyle (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
			: this ((int)weight, (int)width, slant)
		{
		}

		public SKFontStyle (int weight, int width, SKFontStyleSlant slant)
			: this (SkiaApi.sk_fontstyle_new (weight, width, slant), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_fontstyle_delete (Handle);

		public int Weight => SkiaApi.sk_fontstyle_get_weight (Handle);

		public int Width => SkiaApi.sk_fontstyle_get_width (Handle);

		public SKFontStyleSlant Slant => SkiaApi.sk_fontstyle_get_slant (Handle);

		public static SKFontStyle Normal => new SKFontStyle (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

		public static SKFontStyle Bold => new SKFontStyle (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);

		public static SKFontStyle Italic => new SKFontStyle (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);

		public static SKFontStyle BoldItalic => new SKFontStyle (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);

		internal static SKFontStyle GetObject (IntPtr ptr, bool owns = true, bool unrefExisting = true)
		{
			if (GetInstance<SKFontStyle> (ptr, out var instance)) {
				if (unrefExisting && instance is ISKReferenceCounted refcnt) {
#if THROW_OBJECT_EXCEPTIONS
					if (refcnt.GetReferenceCount () == 1)
						throw new InvalidOperationException (
							$"About to unreference an object that has no references. " +
							$"H: {ptr:x} Type: {instance.GetType ()}");
#endif
					refcnt.SafeUnRef ();
				}
				return instance;
			}

			return new SKFontStyle (ptr, owns);
		}
	}
}
