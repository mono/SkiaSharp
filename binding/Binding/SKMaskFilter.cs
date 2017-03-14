//
// Bindings for SKMaskFilter
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKMaskFilter : SKObject
	{
		private const float BlurSigmaScale = 0.57735f;
		
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

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKBlurMaskFilterFlags flags)
		{
			return CreateBlur (blurStyle, sigma, SKRect.Empty, flags);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder)
		{
			return CreateBlur (blurStyle, sigma, occluder, SKBlurMaskFilterFlags.None);
		}

		public static SKMaskFilter CreateBlur (SKBlurStyle blurStyle, float sigma, SKRect occluder, SKBlurMaskFilterFlags flags)
		{
			return GetObject<SKMaskFilter> (SkiaApi.sk_maskfilter_new_blur_with_flags (blurStyle, sigma, ref occluder, flags));
		}

		public static SKMaskFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			if (table.Length != SKColorTable.MaxLength)
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

