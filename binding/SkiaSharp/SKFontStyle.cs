#nullable disable

using System;
using System.Threading;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		private static SKFontStyle normal;
		private static SKFontStyle bold;
		private static SKFontStyle italic;
		private static SKFontStyle boldItalic;

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

		public static SKFontStyle Normal => Ensure (ref normal, SKFontStyleWeight.Normal, SKFontStyleSlant.Upright);

		public static SKFontStyle Bold => Ensure (ref bold, SKFontStyleWeight.Bold, SKFontStyleSlant.Upright);

		public static SKFontStyle Italic => Ensure (ref italic, SKFontStyleWeight.Normal, SKFontStyleSlant.Italic);

		public static SKFontStyle BoldItalic => Ensure (ref boldItalic, SKFontStyleWeight.Bold, SKFontStyleSlant.Italic);

		// SKFontStyle is ISKSkipObjectRegistration — HandleDictionary does not dedup, so
		// a race here may briefly create a second native object. The loser's wrapper
		// goes through normal finalization and unrefs the duplicate native object.
		private static SKFontStyle Ensure (ref SKFontStyle slot, SKFontStyleWeight weight, SKFontStyleSlant slant)
		{
			var existing = slot;
			if (existing is not null)
				return existing;
			// Immortal-from-ctor: IgnorePublicDispose is set before Handle is published.
			var style = new SKFontStyle (weight, SKFontStyleWidth.Normal, slant, immortal: true);
			return Interlocked.CompareExchange (ref slot, style, null) ?? style;
		}

		//

		internal static SKFontStyle GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKFontStyle (handle, true);
	}
}
