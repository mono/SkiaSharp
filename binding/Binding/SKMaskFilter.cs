using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[Flags]
	[Obsolete]
	public enum SKBlurMaskFilterFlags
	{
		None = 0x00,
		IgnoreTransform = 0x01,
		HighQuality = 0x02,
		All = IgnoreTransform | HighQuality,
	}

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

		// Convert*

		public static float ConvertRadiusToSigma (float radius) =>
			radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;

		public static float ConvertSigmaToRadius (float sigma) =>
			sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;

		// CreateBlur
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateBlur(SKBlurStyle, float) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags) =>
			CreateBlur (blurStyle, sigma, SKRect.Empty, true);

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateBlur(SKBlurStyle, float, SKRect) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags) =>
			CreateBlur (blurStyle, sigma, occluder, true);

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder) =>
			CreateBlur (blurStyle, sigma, occluder, true);

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, &occluder, respectCTM));

		// CreateTable

		public static SKMaskFilter CreateTable (byte[] table) =>
			CreateTable (table.AsSpan ());

		public static SKMaskFilter CreateTable (ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException ($"Table must have a length of {TableMaxLength}.", nameof (table));

			fixed (byte* t = table) {
				return GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_table (t));
			}
		}

		// CreateGamma

		public static SKMaskFilter CreateGamma (float gamma) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_gamma (gamma));

		// CreateClip

		public static SKMaskFilter CreateClip (byte min, byte max) =>
			GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_clip (min, max));
	}
}
