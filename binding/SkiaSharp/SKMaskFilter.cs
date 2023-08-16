using System;
using System.ComponentModel;

namespace SkiaSharp
{
	// TODO: `getFormat`
	// TODO: `computeFastBounds`

	public unsafe class SKMaskFilter : SKObject, ISKReferenceCounted
	{
		private const float BlurSigmaScale = 0.57735f;
		public const int TableMaxLength = 256;

		internal SKMaskFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static float ConvertRadiusToSigma (float radius)
		{
			return radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;
		}

		public static float ConvertSigmaToRadius (float sigma)
		{
			return sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, bool respectCTM)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, respectCTM));
		}

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

		public static SKMaskFilter CreateGamma (float gamma)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_gamma (gamma));
		}

		public static SKMaskFilter CreateClip (byte min, byte max)
		{
			return GetObject (SkiaApi.sk_maskfilter_new_clip (min, max));
		}

		internal static SKMaskFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKMaskFilter (h, o));
	}
}

