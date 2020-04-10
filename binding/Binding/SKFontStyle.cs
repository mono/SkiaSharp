using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject
	{
		private static readonly Lazy<SKFontStyle> normal;
		private static readonly Lazy<SKFontStyle> bold;
		private static readonly Lazy<SKFontStyle> italic;
		private static readonly Lazy<SKFontStyle> boldItalic;

		static SKFontStyle()
		{
			normal = new Lazy<SKFontStyle> (() => new SKFontStyleStatic (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright));
			bold = new Lazy<SKFontStyle> (() => new SKFontStyleStatic (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright));
			italic = new Lazy<SKFontStyle> (() => new SKFontStyleStatic (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic));
			boldItalic = new Lazy<SKFontStyle> (() => new SKFontStyleStatic (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic));
		}

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

		public static SKFontStyle Normal => normal.Value;

		public static SKFontStyle Bold => bold.Value;

		public static SKFontStyle Italic => italic.Value;

		public static SKFontStyle BoldItalic => boldItalic.Value;

		internal static SKFontStyle GetObject (IntPtr handle) => GetOrAddObject (handle, (h, o) => new SKFontStyle (h, o));

		private sealed class SKFontStyleStatic : SKFontStyle
		{
			internal SKFontStyleStatic (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
				: base (weight, width, slant)
			{
				IgnorePublicDispose = true;
			}

			protected override void Dispose (bool disposing)
			{
				// do not dispose
			}
		}
	}
}
