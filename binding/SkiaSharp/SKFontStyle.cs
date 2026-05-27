#nullable disable

using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		private static readonly SKFontStyle normal =
			MakeDisposeProtected (SKFontStyleWeight.Normal, SKFontStyleSlant.Upright);
		private static readonly SKFontStyle bold =
			MakeDisposeProtected (SKFontStyleWeight.Bold, SKFontStyleSlant.Upright);
		private static readonly SKFontStyle italic =
			MakeDisposeProtected (SKFontStyleWeight.Normal, SKFontStyleSlant.Italic);
		private static readonly SKFontStyle boldItalic =
			MakeDisposeProtected (SKFontStyleWeight.Bold, SKFontStyleSlant.Italic);

		private static SKFontStyle MakeDisposeProtected (SKFontStyleWeight weight, SKFontStyleSlant slant)
		{
			var style = new SKFontStyle (weight, SKFontStyleWidth.Normal, slant);
			// The PreventPublicDisposal call here doesn't suffer from the case of skia
			// giving us the same handle as a return value of another pinvoke call,
			// because these are created by us and not returned by skia.
			style.PreventPublicDisposal ();
			return style;
		}

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

		public static SKFontStyle Normal => normal;

		public static SKFontStyle Bold => bold;

		public static SKFontStyle Italic => italic;

		public static SKFontStyle BoldItalic => boldItalic;

		//

		internal static SKFontStyle GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKFontStyle (handle, true);
	}
}
