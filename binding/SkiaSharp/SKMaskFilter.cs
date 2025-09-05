#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	// TODO: `getFormat`
	// TODO: `computeFastBounds`

	/// <summary>
	/// Mask filters perform transformations on an alpha-channel mask before drawing. A mask filter is set using the <see cref="SKPaint.MaskFilter" /> property on <see cref="SKPaint" /> type.
	/// </summary>
	public unsafe class SKMaskFilter : SKObject, ISKReferenceCounted
	{
		private const float BlurSigmaScale = 0.57735f;
		/// <summary>
		/// Gets the maximum number of colors in the color lookup table.
		/// </summary>
		public const int TableMaxLength = 256;

		internal SKMaskFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// Converts from the (legacy) idea of specifying the blur "radius" to the standard notion of specifying its sigma.
		/// </summary>
		/// <param name="radius">The radius.</param>
		/// <returns>The sigma.</returns>
		public static float ConvertRadiusToSigma (float radius)
		{
			return radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;
		}

		/// <summary>
		/// Converts from the standard notion of specifying the blur sigma to the (legacy) idea of specifying its "radius".
		/// </summary>
		/// <param name="sigma">The sigma.</param>
		/// <returns>The radius.</returns>
		public static float ConvertSigmaToRadius (float sigma)
		{
			return sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;
		}

		/// <summary>
		/// Creates a mask filter that applies a blur.
		/// </summary>
		/// <param name="blurStyle">The style of blurring.</param>
		/// <param name="sigma">The standard deviation (greater than 0) of the Gaussian blur to apply.</param>
		/// <returns>Returns the new <see cref="SKMaskFilter" />, or <see langword="null" /> on error.</returns>
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));
		}

		/// <param name="blurStyle"></param>
		/// <param name="sigma"></param>
		/// <param name="respectCTM"></param>
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, bool respectCTM)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, respectCTM));
		}

		/// <summary>
		/// Creates a mask filter that applies a table lookup on each of the alpha values in the mask.
		/// </summary>
		/// <param name="table">The lookup table with exactly 256 elements.</param>
		/// <returns>Returns the new <see cref="SKMaskFilter" />, or <see langword="null" /> on error.</returns>
		public static SKMaskFilter CreateTable (byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException (nameof (table));
			if (table.Length != TableMaxLength)
				throw new ArgumentException ("Table must have a length of {SKColorTable.MaxLength}.", nameof (table));
			fixed (byte* t = table) {
				return GetObject (SkiaApi.sk_maskfilter_new_table (t));
			}
		}

		/// <summary>
		/// Creates a mask filter that applies gamma.
		/// </summary>
		/// <param name="gamma">The gamma.</param>
		/// <returns>Returns the new <see cref="SKMaskFilter" />, or <see langword="null" /> on error.</returns>
		public static SKMaskFilter CreateGamma (float gamma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_gamma (gamma));
		}

		/// <summary>
		/// Creates a mask filter that clips the alpha channel to the specified minimum and maximum alpha values.
		/// </summary>
		/// <param name="min">The minimum alpha value.</param>
		/// <param name="max">The maximum alpha value.</param>
		/// <returns>Returns the new <see cref="SKMaskFilter" />, or <see langword="null" /> on error.</returns>
		public static SKMaskFilter CreateClip (byte min, byte max)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_clip (min, max));
		}

		internal static SKMaskFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKMaskFilter (h, o));
	}
}

