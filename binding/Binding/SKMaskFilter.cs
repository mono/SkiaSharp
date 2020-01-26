using System;

namespace SkiaSharp
{
	// TODO: `getFormat`
	// TODO: `computeFastBounds`

	public unsafe class SKMaskFilter : SKObject, ISKReferenceCounted
	{
		private const float BlurSigmaScale = 0.57735f;
		public const int TableMaxLength = 256;

		[Preserve]
		internal SKMaskFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public static float ConvertRadiusToSigma (float radius) =>
			radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;

		public static float ConvertSigmaToRadius (float sigma) =>
			sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM = true) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, &occluder, respectCTM));

		public static SKMaskFilter CreateTable (ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException ($"Table must have a length of {TableMaxLength}.", nameof (table));

			fixed (byte* t = table) {
				return GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_table (t));
			}
		}

		public static SKMaskFilter CreateGamma (float gamma) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_gamma (gamma));

		public static SKMaskFilter CreateClip (byte min, byte max) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_clip (min, max));
	}
}
