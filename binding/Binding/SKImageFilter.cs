using System;

namespace SkiaSharp
{
	// TODO: `asAColorFilter`
	// TODO: `countInputs`, `getInput`
	// TODO: `cropRectIsSet`, `getCropRect`
	// TODO: `computeFastBounds`, `canComputeFastBounds`

	public unsafe class SKImageFilter : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKImageFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// CreateMatrix

		public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter input = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_matrix (&matrix, quality, input?.Handle ?? IntPtr.Zero));

		// CreateAlphaThreshold

		public static SKImageFilter CreateAlphaThreshold (SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter input = null) =>
			CreateAlphaThreshold (new SKRegion (region), innerThreshold, outerThreshold, input);

		public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter input = null)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_alpha_threshold (region.Handle, innerThreshold, outerThreshold, input?.Handle ?? IntPtr.Zero));
		}

		// CreateBlur

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_blur (sigmaX, sigmaY, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreateColorFilter

		public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter input = null, CropRect cropRect = null)
		{
			if (cf == null)
				throw new ArgumentNullException (nameof (cf));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_color_filter (cf.Handle, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
		}

		// CreateCompose

		public static SKImageFilter CreateCompose (SKImageFilter outer, SKImageFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException (nameof (outer));
			if (inner == null)
				throw new ArgumentNullException (nameof (inner));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_compose (outer.Handle, inner.Handle));
		}

		// CreateDisplacementMapEffect

		public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, CropRect cropRect = null)
		{
			if (displacement == null)
				throw new ArgumentNullException (nameof (displacement));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_displacement_map_effect (xChannelSelector, yChannelSelector, scale, displacement.Handle, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
		}

		// CreateDropShadow

		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_drop_shadow (dx, dy, sigmaX, sigmaY, (uint)color, shadowMode, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// Create*LitDiffuse

		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_distant_lit_diffuse (&direction, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_point_lit_diffuse (&location, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_spot_lit_diffuse (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// Create*LitSpecular

		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_distant_lit_specular (&direction, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_point_lit_specular (&location, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_spot_lit_specular (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreateMagnifier

		public static SKImageFilter CreateMagnifier (SKRect src, float inset, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_magnifier (&src, inset, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreateMatrixConvolution

		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, CropRect cropRect = null)
		{
			if (kernel == null)
				throw new ArgumentNullException (nameof (kernel));
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException ("Kernel length must match the dimensions of the kernel size (Width * Height).", nameof (kernel));

			fixed (float* k = kernel) {
				return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_matrix_convolution (&kernelSize, k, gain, bias, &kernelOffset, tileMode, convolveAlpha, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
			}
		}

		// CreateMerge

		public static SKImageFilter CreateMerge (SKImageFilter first, SKImageFilter second, CropRect cropRect = null) =>
			CreateMerge (new[] { first, second }, cropRect);

		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters, CropRect cropRect = null)
		{
			if (filters == null)
				throw new ArgumentNullException (nameof (filters));

			var handles = new IntPtr[filters.Length];
			for (var i = 0; i < filters.Length; i++) {
				handles[i] = filters[i].Handle;
			}

			fixed (IntPtr* h = handles) {
				return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_merge (h, filters.Length, cropRect?.Handle ?? IntPtr.Zero));
			}
		}

		// CreateDilate

		public static SKImageFilter CreateDilate (int radiusX, int radiusY, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_dilate (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreateErode

		public static SKImageFilter CreateErode (int radiusX, int radiusY, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_erode (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreateOffset

		public static SKImageFilter CreateOffset (float dx, float dy, SKImageFilter input = null, CropRect cropRect = null) =>
			GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_offset (dx, dy, input?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));

		// CreatePicture

		public static SKImageFilter CreatePicture (SKPicture picture)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_picture (picture.Handle));
		}

		public static SKImageFilter CreatePicture (SKPicture picture, SKRect cropRect)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_picture_with_croprect (picture.Handle, &cropRect));
		}

		// CreateTile

		public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter input)
		{
			if (input == null)
				throw new ArgumentNullException (nameof (input));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input.Handle));
		}

		// CreateBlendMode

		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter background, SKImageFilter foreground = null, CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException (nameof (background));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_xfermode (mode, background.Handle, foreground?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
		}

		// CreateArithmetic

		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground = null, CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException (nameof (background));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_arithmetic (k1, k2, k3, k4, enforcePMColor, background.Handle, foreground?.Handle ?? IntPtr.Zero, cropRect?.Handle ?? IntPtr.Zero));
		}

		// CreateImage

		public static SKImageFilter CreateImage (SKImage image)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_image_source_default (image.Handle));
		}

		public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_image_source (image.Handle, &src, &dst, filterQuality));
		}

		// CreatePaint

		public static SKImageFilter CreatePaint (SKPaint paint, CropRect cropRect = null)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			return GetObject<SKImageFilter> (SkiaApi.sk_imagefilter_new_paint (paint.Handle, cropRect?.Handle ?? IntPtr.Zero));
		}

		//

		public class CropRect : SKObject
		{
			internal CropRect (IntPtr handle, bool owns)
				: base (handle, owns)
			{
			}

			public CropRect ()
				: this (SkiaApi.sk_imagefilter_croprect_new (), true)
			{
			}

			public CropRect (SKRect rect, SKCropRectFlags flags = SKCropRectFlags.HasAll)
				: this (SkiaApi.sk_imagefilter_croprect_new_with_rect (&rect, (uint)flags), true)
			{
			}

			protected override void DisposeNative () =>
				SkiaApi.sk_imagefilter_croprect_destructor (Handle);

			public SKRect Rect {
				get {
					SKRect rect;
					SkiaApi.sk_imagefilter_croprect_get_rect (Handle, &rect);
					return rect;
				}
			}

			public SKCropRectFlags Flags =>
				(SKCropRectFlags)SkiaApi.sk_imagefilter_croprect_get_flags (Handle);
		}
	}
}
