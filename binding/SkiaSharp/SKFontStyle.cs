#nullable disable

using System;

namespace SkiaSharp
{
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		// Process-global preset font-style singletons. Populated eagerly (in dependency order, before the
		// default typeface which reads Normal) by SkiaSharpStatics.EnsureInitialized and rooted here so
		// the GC never collects the immortal wrappers. See SkiaSharpStatics (#3817).
		private static SKFontStyle normal;
		private static SKFontStyle bold;
		private static SKFontStyle italic;
		private static SKFontStyle boldItalic;

		internal static void InitializeStatics ()
		{
			// Idempotent so a retry after a partial-init failure does not create new immortal native
			// font styles (SKFontStyle bypasses the HandleDictionary dedup, so re-creating would leak).
			normal ??= MakeDisposeProtected (SKFontStyleWeight.Normal, SKFontStyleSlant.Upright);
			bold ??= MakeDisposeProtected (SKFontStyleWeight.Bold, SKFontStyleSlant.Upright);
			italic ??= MakeDisposeProtected (SKFontStyleWeight.Normal, SKFontStyleSlant.Italic);
			boldItalic ??= MakeDisposeProtected (SKFontStyleWeight.Bold, SKFontStyleSlant.Italic);
		}

		private static SKFontStyle MakeDisposeProtected (SKFontStyleWeight weight, SKFontStyleSlant slant)
		{
			var style = new SKFontStyle (weight, SKFontStyleWidth.Normal, slant);
			// The PreventPublicDisposal call here doesn't suffer from the case of skia
			// giving us the same handle as a return value of another pinvoke call,
			// because these are created by us and not returned by skia.
			style.PreventPublicDisposal ();
			// Process-global preset singleton: latch immortal so neither the finalizer nor DisposeInternal
			// can free the shared native font style.
			style.MakeImmortalSingleton ();
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

		public static SKFontStyle Normal {
			get {
				SkiaSharpStatics.EnsureInitialized ();
				return normal;
			}
		}

		public static SKFontStyle Bold {
			get {
				SkiaSharpStatics.EnsureInitialized ();
				return bold;
			}
		}

		public static SKFontStyle Italic {
			get {
				SkiaSharpStatics.EnsureInitialized ();
				return italic;
			}
		}

		public static SKFontStyle BoldItalic {
			get {
				SkiaSharpStatics.EnsureInitialized ();
				return boldItalic;
			}
		}

		//

		internal static SKFontStyle GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKFontStyle (handle, true);
	}
}
