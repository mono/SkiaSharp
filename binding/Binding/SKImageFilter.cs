using System;

namespace SkiaSharp
{
	// TODO: `asAColorFilter`
	// TODO: `countInputs`, `getInput`
	// TODO: `cropRectIsSet`, `getCropRect`
	// TODO: `computeFastBounds`, `canComputeFastBounds`

	public class SKImageFilter : SKObject
	{
		[Preserve]
		internal SKImageFilter(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle)
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
			var reg = new SKRegion ();
			reg.SetRect (region);
			return CreateAlphaThreshold (reg, innerThreshold, outerThreshold, input);

		}

		public static SKImageFilter CreateAlphaThreshold(SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input = null)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_alpha_threshold(region.Handle, innerThreshold, outerThreshold, input == null ? IntPtr.Zero : input.Handle));
		}

		public static SKImageFilter CreateBlur(float sigmaX, float sigmaY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_blur(sigmaX, sigmaY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (cf == null)
				throw new ArgumentNullException(nameof(cf));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_color_filter(cf.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_compose(outer.Handle, inner.Handle));
		}

		public static SKImageFilter CreateDisplacementMapEffect(SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (displacement == null)
				throw new ArgumentNullException(nameof(displacement));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_displacement_map_effect(xChannelSelector, yChannelSelector, scale, displacement.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateDropShadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
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

		public static SKImageFilter CreateMagnifier(SKRect src, float inset, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_magnifier(ref src, inset, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateMatrixConvolution(SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (kernel == null)
				throw new ArgumentNullException(nameof(kernel));
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException("Kernel length must match the dimensions of the kernel size (Width * Height).", nameof(kernel));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_matrix_convolution(ref kernelSize, kernel, gain, bias, ref kernelOffset, tileMode, convolveAlpha, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		[Obsolete("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge(new [] { first, second }, new [] { mode, mode }, cropRect);
		}

		public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge(new [] { first, second }, cropRect);
		}

		[Obsolete("Use CreateMerge(SKImageFilter[], SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge (filters, cropRect);
		}

		public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKImageFilter.CropRect cropRect = null)
		{
			if (filters == null)
				throw new ArgumentNullException(nameof(filters));
			var f = new IntPtr[filters.Length];
			for (int i = 0; i < filters.Length; i++)
			{
				f[i] = filters[i].Handle;
			}
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_merge(f, filters.Length, cropRect == null ? IntPtr.Zero : cropRect.Handle));
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
				throw new ArgumentNullException(nameof(picture));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_picture(picture.Handle));
		}

		public static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect)
		{
			if (picture == null)
				throw new ArgumentNullException(nameof(picture));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_picture_with_croprect(picture.Handle, ref cropRect));
		}

		public static SKImageFilter CreateTile(SKRect src, SKRect dst, SKImageFilter input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_tile(ref src, ref dst, input.Handle));
		}

		public static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException(nameof(background));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_xfermode(mode, background.Handle, foreground == null ? IntPtr.Zero : foreground.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException(nameof(background));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_arithmetic(k1, k2, k3, k4, enforcePMColor, background.Handle, foreground == null ? IntPtr.Zero : foreground.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateImage(SKImage image)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_image_source_default(image.Handle));
		}

		public static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_image_source(image.Handle, ref src, ref dst, filterQuality));
		}

		public static SKImageFilter CreatePaint(SKPaint paint, SKImageFilter.CropRect cropRect = null)
		{
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));
			return GetObject<SKImageFilter>(SkiaApi.sk_imagefilter_new_paint(paint.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public class CropRect : SKObject
		{
			internal CropRect(IntPtr handle, bool owns)
				: base(handle, owns)
			{
			}

			public CropRect()
				: this(SkiaApi.sk_imagefilter_croprect_new(), true)
			{
			}

			public CropRect(SKRect rect, SKCropRectFlags flags = SKCropRectFlags.HasAll)
				: this(SkiaApi.sk_imagefilter_croprect_new_with_rect(ref rect, flags), true)
			{
			}
			
			protected override void Dispose(bool disposing)
			{
				if (Handle != IntPtr.Zero && OwnsHandle)
				{
					SkiaApi.sk_imagefilter_croprect_destructor(Handle);
				}
			}
			
			public SKRect Rect
			{
				get
				{
					SkiaApi.sk_imagefilter_croprect_get_rect (Handle, out var rect);
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
