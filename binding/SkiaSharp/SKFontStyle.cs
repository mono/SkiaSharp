#nullable disable

using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		// Eager init via field initializers (cctor) is safe here: SKFontStyle is
		// ISKSkipObjectRegistration (no HandleDictionary dedup risk), the ctor only
		// reads enum constants (no cross-singleton dependencies), and the CLR's
		// once-per-AppDomain cctor guarantee removes the race that lazy init would
		// have for this type (sk_fontstyle_new allocates a fresh native object every
		// call, so a concurrent lazy init would briefly leak the loser's allocation).
		private static readonly SKFontStyle normal =
			new SKFontStyle (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, immortal: true);
		private static readonly SKFontStyle bold =
			new SKFontStyle (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright, immortal: true);
		private static readonly SKFontStyle italic =
			new SKFontStyle (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic, immortal: true);
		private static readonly SKFontStyle boldItalic =
			new SKFontStyle (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic, immortal: true);

		internal SKFontStyle (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal SKFontStyle (IntPtr handle, bool owns, bool immortal)
			: base (handle, owns, immortal)
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

		internal SKFontStyle (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant, bool immortal)
			: this (SkiaApi.sk_fontstyle_new ((int)weight, (int)width, slant), true, immortal)
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
