#nullable disable

using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		// Process-global preset font-style singletons, built once by this type's static
		// constructor from the raw handles SkiaSharpStatics acquired, and rooted here so the GC never
		// collects them (which also guarantees their finalizers never run). SKFontStyle bypasses the
		// HandleDictionary (ISKSkipObjectRegistration), so each wrapper is dispose-protected directly.
		// See SkiaSharpStatics (#3817).
		private static readonly SKFontStyle normal;
		private static readonly SKFontStyle bold;
		private static readonly SKFontStyle italic;
		private static readonly SKFontStyle boldItalic;

		static SKFontStyle ()
		{
			normal = MakeDisposeProtected (SkiaSharpStatics.NormalFontStyle);
			bold = MakeDisposeProtected (SkiaSharpStatics.BoldFontStyle);
			italic = MakeDisposeProtected (SkiaSharpStatics.ItalicFontStyle);
			boldItalic = MakeDisposeProtected (SkiaSharpStatics.BoldItalicFontStyle);
		}

		private static SKFontStyle MakeDisposeProtected (IntPtr handle)
		{
			var style = new SKFontStyle (handle, true);
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
