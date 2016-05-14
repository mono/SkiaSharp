//
// Bindings for SKImageFilter
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKImageFilter : SKObject
	{
		[Preserve]
		internal SKImageFilter(IntPtr handle)
			: base(handle)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero)
			{
				SkiaApi.sk_imagefilter_unref(Handle);
			}

			base.Dispose(disposing);
		}
		
		public static SKImageFilter CreateMatrix(SKMatrix matrix, SKFilterQuality quality, SKImageFilter input = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_matrix(ref matrix, quality, input == null ? IntPtr.Zero : input.Handle));
		}

		public static SKImageFilter CreateAlphaThreshold(SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_alpha_threshold(ref region, innerThreshold, outerThreshold, input == null ? IntPtr.Zero : input.Handle));
		}

		public static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_blur(sigmaX, sigmaY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (cf == null)
				throw new ArgumentNullException("cf");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_color_filter(cf.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException("outer");
			if (inner == null)
				throw new ArgumentNullException("inner");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_compose(outer.Handle, inner.Handle));
		}

		public static SKImageFilter CreateCompose(SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (displacement == null)
				throw new ArgumentNullException("displacement");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_displacement_map_effect(xChannelSelector, yChannelSelector, scale, displacement.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateDownSample(float scale, SKImageFilter input = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_downsample(scale, input == null ? IntPtr.Zero : input.Handle));
		}

		public static SKImageFilter CreateDownSample(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_drop_shadow(dx, dy, sigmaX, sigmaY, color, shadowMode, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_distant_lit_diffuse(ref direction, lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_point_lit_diffuse(ref location, lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_spot_lit_diffuse(ref location, ref target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_distant_lit_specular(ref direction, lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_point_lit_specular(ref location, lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_spot_lit_specular(ref location, ref target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateMagnifier(SKRect src, float inset, SKImageFilter input = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_magnifier(ref src, inset, input == null ? IntPtr.Zero : input.Handle));
		}

		public static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (kernel == null)
				throw new ArgumentNullException("kernel");
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException("Kernel length must match the dimensions of the kernel size (Width * Height).", "kernel");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_matrix_convolution(ref kernelSize, kernel, gain, bias, ref kernelOffset, tileMode, convolveAlpha, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKXferMode[] modes = null, SKImageFilter.CropRect cropRect = null)
		{
			if (filters == null)
				throw new ArgumentNullException("filters");
			if (modes != null && modes.Length != filters.Length)
				throw new ArgumentException("The numbers of modes must match the number of filters.", "modes");
			var f = new IntPtr[filters.Length];
			for (int i = 0; i < filters.Length; i++)
			{
				f[i] = filters[i].Handle;
			}
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_merge(f, filters.Length, modes, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateDilate(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_dilate(radiusX, radiusY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateErode(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_erode(radiusX, radiusY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateOffset(float dx, float dy, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_offset(dx, dy, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreatePicture(SKPicture picture)
		{
			if (picture == null)
				throw new ArgumentNullException("picture");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_picture(picture.Handle));
		}

		public static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect)
		{
			if (picture == null)
				throw new ArgumentNullException("picture");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_picture_with_croprect(picture.Handle, ref cropRect));
		}

		public static SKImageFilter CreatePictureForLocalspace(SKPicture picture, SKRect cropRect, SKFilterQuality filterQuality)
		{
			if (picture == null)
				throw new ArgumentNullException("picture");
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_picture_for_localspace(picture.Handle, ref cropRect, filterQuality));
		}

		public class CropRect : SKObject
		{
			internal CropRect(IntPtr handle)
				: base(handle)
			{
			}

			public CropRect()
				: this(SkiaApi.sk_imagefilter_croprect_new())
			{
			}

			public CropRect(SKRect rect, SKCropRectFlags flags = SKCropRectFlags.HasAll)
				: this(SkiaApi.sk_imagefilter_croprect_new_with_rect(ref rect, flags))
			{
			}
			
			protected override void Dispose(bool disposing)
			{
				if (Handle != IntPtr.Zero)
				{
					SkiaApi.sk_imagefilter_croprect_destructor(Handle);
				}
			}
			
			public SKRect Rect
			{
				get
				{
					SKRect rect;
					SkiaApi.sk_imagefilter_croprect_get_rect(Handle, out rect);
					return rect;
				}
			}

			public SKCropRectFlags Flags
			{
				get { return SkiaApi.sk_imagefilter_croprect_get_flags(Handle); }
			}
		}
	}
}
