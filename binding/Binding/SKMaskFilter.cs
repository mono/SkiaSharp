using System;

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

	public class SKMaskFilter : SKObject
	{
		private const float BlurSigmaScale = 0.57735f;
		public const int TableMaxLength = 256;

		[Preserve]
		internal SKMaskFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_maskfilter_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public static float ConvertRadiusToSigma(float radius)
		{
			return radius > 0 ? BlurSigmaScale * radius + 0.5f : 0.0f;
		}

		public static float ConvertSigmaToRadius(float sigma)
		{
			return sigma > 0.5f ? (sigma - 0.5f) / BlurSigmaScale : 0.0f;
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma)
		{
			return GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur (blurStyle, sigma));
		}

		[Obsolete("Use CreateBlur(SKBlurStyle, float) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags)
		{
			return CreateBlur (blurStyle, sigma, SKRect.Empty, true);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder)
		{
			return CreateBlur (blurStyle, sigma, occluder, true);
		}

		[Obsolete("Use CreateBlur(SKBlurStyle, float, SKRect) instead.")]
		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags)
		{
			return CreateBlur (blurStyle, sigma, occluder, true);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, bool respectCTM)
		{
			return GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, ref occluder, respectCTM));
		}

		public static SKMaskFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			if (table.Length != TableMaxLength)
				throw new ArgumentException("Table must have a length of {SKColorTable.MaxLength}.", nameof(table));
			return GetObject<SKMaskFilter>(SkiaApi.sk_maskfilter_new_table(table));
		}

		public static SKMaskFilter CreateGamma(float gamma)
		{
			return GetObject<SKMaskFilter>(SkiaApi.sk_maskfilter_new_gamma(gamma));
		}

		public static SKMaskFilter CreateClip(byte min, byte max)
		{
			return GetObject<SKMaskFilter>(SkiaApi.sk_maskfilter_new_clip(min, max));
		}
	}
}

