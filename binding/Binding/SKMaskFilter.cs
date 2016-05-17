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
		internal SKMaskFilter (IntPtr handle)
			: base (handle)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
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

		public static SKMaskFilter CreateEmboss(float blurSigma, SKPoint3 direction, float ambient, float specular)
		{
			return CreateEmboss(blurSigma, direction.X, direction.Y, direction.Z, ambient, specular);
		}

		public static SKMaskFilter CreateEmboss(float blurSigma, float directionX, float directionY, float directionZ, float ambient, float specular)
		{
			return GetObject<SKMaskFilter>(SkiaApi.sk_maskfilter_new_emboss(blurSigma, new[] { directionX, directionY, directionZ }, ambient, specular));
		}

		public static SKMaskFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			if (table.Length != 256)
				throw new ArgumentException("Table must have a length of 256.", "table");
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

