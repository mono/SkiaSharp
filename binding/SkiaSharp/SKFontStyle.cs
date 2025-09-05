#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Represents a particular style (bold, italic, condensed) of a typeface.
	/// </summary>
	public class SKFontStyle : SKObject, ISKSkipObjectRegistration
	{
		private static readonly SKFontStyle normal;
		private static readonly SKFontStyle bold;
		private static readonly SKFontStyle italic;
		private static readonly SKFontStyle boldItalic;

		static SKFontStyle ()
		{
			normal = new SKFontStyleStatic (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
			bold = new SKFontStyleStatic (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright);
			italic = new SKFontStyleStatic (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
			boldItalic = new SKFontStyleStatic (SKFontStyleWeight.Bold, SKFontStyleWidth.Normal, SKFontStyleSlant.Italic);
		}

		internal SKFontStyle (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates a new <see cref="SKFontStyle" /> with a normal weight, a normal width and upright.
		/// </summary>
		public SKFontStyle ()
			: this (SKFontStyleWeight.Normal, SKFontStyleWidth.Normal, SKFontStyleSlant.Upright)
		{
		}

		/// <summary>
		/// Creates a new <see cref="SKFontStyle" /> with the specified weight, width and slant.
		/// </summary>
		/// <param name="weight">The weight (light or bold).</param>
		/// <param name="width">The width (condensed or expanded).</param>
		/// <param name="slant">The slant (italic).</param>
		public SKFontStyle (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
			: this ((int)weight, (int)width, slant)
		{
		}

		/// <summary>
		/// Creates a new <see cref="SKFontStyle" /> with the specified weight, width and slant.
		/// </summary>
		/// <param name="weight">The weight (light or bold).</param>
		/// <param name="width">The width (condensed or expanded).</param>
		/// <param name="slant">The slant (italic).</param>
		public SKFontStyle (int weight, int width, SKFontStyleSlant slant)
			: this (SkiaApi.sk_fontstyle_new (weight, width, slant), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_fontstyle_delete (Handle);

		/// <summary>
		/// Gets the weight of this style.
		/// </summary>
		/// <remarks>
		/// The weight could potentially be one of the values of <see cref="SKFontStyleWeight" />.
		/// </remarks>
		public int Weight => SkiaApi.sk_fontstyle_get_weight (Handle);

		/// <summary>
		/// Gets the width of this style.
		/// </summary>
		/// <remarks>
		/// The weight could potentially be one of the values of <see cref="SKFontStyleWidth" />.
		/// </remarks>
		public int Width => SkiaApi.sk_fontstyle_get_width (Handle);

		/// <summary>
		/// Gets the slant of this style.
		/// </summary>
		public SKFontStyleSlant Slant => SkiaApi.sk_fontstyle_get_slant (Handle);

		/// <summary>
		/// Gets a new normal (upright and not bold) font style.
		/// </summary>
		public static SKFontStyle Normal => normal;

		/// <summary>
		/// Gets a new upright font style that is bold.
		/// </summary>
		public static SKFontStyle Bold => bold;

		/// <summary>
		/// Gets a new italic font style.
		/// </summary>
		public static SKFontStyle Italic => italic;

		/// <summary>
		/// Gets a new italic font style that is bold.
		/// </summary>
		public static SKFontStyle BoldItalic => boldItalic;

		//

		internal static SKFontStyle GetObject (IntPtr handle) =>
			handle == IntPtr.Zero ? null : new SKFontStyle (handle, true);

		private sealed class SKFontStyleStatic : SKFontStyle
		{
			internal SKFontStyleStatic (SKFontStyleWeight weight, SKFontStyleWidth width, SKFontStyleSlant slant)
				: base (weight, width, slant)
			{
			}

			protected override void Dispose (bool disposing) { }
		}
	}
}
