#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a particular style (bold, italic, condensed) of a typeface.</summary>
	/// <remarks />
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

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFontStyle" /> with a normal weight, a normal width and upright.</summary>
		/// <remarks />
		public SKFontStyle ()
			: this (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFontStyle" /> with the specified weight, width and slant.</summary>
		/// <param name="weight">The weight (light or bold).</param>
		/// <param name="width">The width (condensed or expanded).</param>
		/// <param name="slant">The slant (italic).</param>
		/// <remarks />
		public SKFontStyle (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
			: this ((int)weight, (int)width, slant)
		{
		}

		/// <summary>Creates a new <see cref="T:SkiaSharp.SKFontStyle" /> with the specified weight, width and slant.</summary>
		/// <param name="weight">The weight (light or bold).</param>
		/// <param name="width">The width (condensed or expanded).</param>
		/// <param name="slant">The slant (italic).</param>
		/// <remarks />
		public SKFontStyle (int weight, int width, SKFontStyleSlant slant)
			: this (SkiaApi.sk_fontstyle_new (weight, width, slant), true)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKFontStyle" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKFontStyle" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_fontstyle_delete (Handle);

		/// <summary>Gets the weight of this style.</summary>
		/// <value>The weight of this style.</value>
		/// <remarks>The weight could potentially be one of the values of <see cref="T:SkiaSharp.SKFontStyleWeight" />.</remarks>
		public int Weight => SkiaApi.sk_fontstyle_get_weight (Handle);

		/// <summary>Gets the width of this style.</summary>
		/// <value>The width of this style.</value>
		/// <remarks>The weight could potentially be one of the values of <see cref="T:SkiaSharp.SKFontStyleWidth" />.</remarks>
		public int Width => SkiaApi.sk_fontstyle_get_width (Handle);

		/// <summary>Gets the slant of this style.</summary>
		/// <value>The slant of this style.</value>
		/// <remarks />
		public SKFontStyleSlant Slant => SkiaApi.sk_fontstyle_get_slant (Handle);

		/// <summary>Gets a new normal (upright and not bold) font style.</summary>
		/// <value>A new normal font style.</value>
		/// <remarks />
		public static SKFontStyle Normal => normal;

		/// <summary>Gets a new upright font style that is bold.</summary>
		/// <value>A new upright bold font style.</value>
		/// <remarks />
		public static SKFontStyle Bold => bold;

		/// <summary>Gets a new italic font style.</summary>
		/// <value>A new italic font style.</value>
		/// <remarks />
		public static SKFontStyle Italic => italic;

		/// <summary>Gets a new italic font style that is bold.</summary>
		/// <value>A new bold italic font style.</value>
		/// <remarks />
		public static SKFontStyle BoldItalic => boldItalic;

		//

		internal static SKFontStyle GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKFontStyle (handle, true);
	}
}
