using System;

namespace SkiaSharp
{
	public unsafe class SKImageFilter : SKObject, ISKReferenceCounted
	{
		internal SKImageFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// CreateMatrix

		[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]
		public static SKImageFilter CreateMatrix (SKMatrix matrix) =>
			CreateMatrix (in matrix);

		[Obsolete("Use SetMatrix(in SKMatrix, SKSamplingOptions, SKImageFilter) instead.", true)]
		public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter? input) =>
			CreateMatrix (in matrix, quality.ToSamplingOptions (), input);

		public static SKImageFilter CreateMatrix (in SKMatrix matrix) =>
			CreateMatrix (matrix, SKSamplingOptions.Default, null);

		public static SKImageFilter CreateMatrix (in SKMatrix matrix, SKSamplingOptions sampling) =>
			CreateMatrix (matrix, sampling, null);

		public static SKImageFilter CreateMatrix (in SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter? input)
		{
			fixed (SKMatrix* m = &matrix)
				return GetObject (SkiaApi.sk_imagefilter_new_matrix_transform (m, &sampling, input?.Handle ?? IntPtr.Zero));
		}


		// CreateAlphaThreshold

		[Obsolete("Use CreateAlphaThreshold(SKRegion, float, float, SKImageFilter) instead.", true)]
		public static SKImageFilter CreateAlphaThreshold(SKRectI region, float innerThreshold, float outerThreshold, SKImageFilter? input)
		{
			var reg = new SKRegion ();
			reg.SetRect (region);
			return CreateAlphaThreshold (reg, innerThreshold, outerThreshold, input);
		}

		public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold) =>
			CreateAlphaThreshold (region, innerThreshold, outerThreshold, null);

		public static SKImageFilter CreateAlphaThreshold (SKRegion region, float innerThreshold, float outerThreshold, SKImageFilter? input)
		{
			_ = region ?? throw new ArgumentNullException (nameof (region));
			return GetObject (SkiaApi.sk_imagefilter_new_alpha_threshold (region.Handle, innerThreshold, outerThreshold, input?.Handle ?? IntPtr.Zero));
		}

		// CreateBlur

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, null, null);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter? input) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, input, null);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter? input, SKRect cropRect) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, input, &cropRect);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode) =>
			CreateBlur (sigmaX, sigmaY, tileMode, null, null);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input) =>
			CreateBlur (sigmaX, sigmaY, tileMode, input, null);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect cropRect) =>
			CreateBlur (sigmaX, sigmaY, tileMode, input, &cropRect);

		private static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_blur (sigmaX, sigmaY, tileMode, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateColorFilter

		public static SKImageFilter CreateColorFilter (SKColorFilter cf) =>
			CreateColorFilter (cf, null, null);

		public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input) =>
			CreateColorFilter (cf, input, null);

		public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input, SKRect cropRect) =>
			CreateColorFilter (cf, input, &cropRect);

		private static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input, SKRect* cropRect)
		{
			_ = cf ?? throw new ArgumentNullException (nameof (cf));
			return GetObject (SkiaApi.sk_imagefilter_new_color_filter (cf.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
		}

		// CreateCompose

		public static SKImageFilter CreateCompose (SKImageFilter outer, SKImageFilter inner)
		{
			_ = outer ?? throw new ArgumentNullException (nameof (outer));
			_ = inner ?? throw new ArgumentNullException (nameof (inner));
			return GetObject (SkiaApi.sk_imagefilter_new_compose (outer.Handle, inner.Handle));
		}

		// CreateDisplacementMapEffect

		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, null, null);

		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, input, null);

		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect cropRect) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, input, &cropRect);

		private static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect* cropRect)
		{
			_ = displacement ?? throw new ArgumentNullException (nameof (displacement));
			return GetObject (SkiaApi.sk_imagefilter_new_displacement_map_effect (xChannelSelector, yChannelSelector, scale, displacement.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
		}

		// CreateDropShadow

		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, null, null);

		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, input, null);

		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, input, &cropRect);

		private static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_drop_shadow (dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateDropShadowOnly

		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, null, null);

		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, input, null);

		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, input, &cropRect);

		private static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_drop_shadow_only (dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateDistantLitDiffuse

		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, null, null);

		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, input, null);

		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_distant_lit_diffuse (&direction, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreatePointLitDiffuse

		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, null, null);

		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, input, null);

		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_point_lit_diffuse (&location, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateSpotLitDiffuse

		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, null, null);

		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, null);

		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_spot_lit_diffuse (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateDistantLitSpecular

		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, null, null);

		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, input, null);

		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_distant_lit_specular (&direction, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreatePointLitSpecular

		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, null, null);

		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, input, null);

		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_point_lit_specular (&location, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateSpotLitSpecular

		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, null, null);

		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, null);

		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_spot_lit_specular (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateMatrixConvolution

		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, null, null);

		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, null);

		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect cropRect) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, &cropRect);

		private static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect* cropRect)
		{
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException ("Kernel length must match the dimensions of the kernel size (Width * Height).", nameof (kernel));
			fixed (float* k = kernel) {
				return GetObject (SkiaApi.sk_imagefilter_new_matrix_convolution (&kernelSize, k, gain, bias, &kernelOffset, tileMode, convolveAlpha, input?.Handle ?? IntPtr.Zero, cropRect));
			}
		}

		// CreateMerge

		public static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second) =>
			CreateMerge (first, second, null);

		public static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second, SKRect cropRect) =>
			CreateMerge (first, second, &cropRect);

		private static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_merge_simple (first?.Handle ?? IntPtr.Zero, second?.Handle ?? IntPtr.Zero, cropRect));

		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters) =>
			CreateMerge (filters, null);

		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters, SKRect cropRect) =>
			CreateMerge (filters, &cropRect);

		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters, SKRect* cropRect)
		{
			var handles = new IntPtr[filters.Length];
			for (var i = 0; i < filters.Length; i++) {
				handles[i] = filters[i]?.Handle ?? IntPtr.Zero;
			}
			fixed (IntPtr* h = handles) {
				return GetObject (SkiaApi.sk_imagefilter_new_merge (h, filters.Length, cropRect));
			}
		}

		// CreateDilate

		public static SKImageFilter CreateDilate (float radiusX, float radiusY) =>
			CreateDilate (radiusX, radiusY, null, null);

		public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateDilate (radiusX, radiusY, input, null);

		public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateDilate (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_dilate (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateErode

		public static SKImageFilter CreateErode (float radiusX, float radiusY) =>
			CreateErode (radiusX, radiusY, null, null);

		public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateErode (radiusX, radiusY, input, null);

		public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateErode (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_erode (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreateOffset

		public static SKImageFilter CreateOffset (float radiusX, float radiusY) =>
			CreateOffset (radiusX, radiusY, null, null);

		public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateOffset (radiusX, radiusY, input, null);

		public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateOffset (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_offset (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreatePicture

		public static SKImageFilter CreatePicture (SKPicture picture)
		{
			_ = picture ?? throw new ArgumentNullException (nameof (picture));
			return GetObject (SkiaApi.sk_imagefilter_new_picture (picture.Handle));
		}

		public static SKImageFilter CreatePicture (SKPicture picture, SKRect cropRect)
		{
			_ = picture ?? throw new ArgumentNullException (nameof (picture));
			return GetObject (SkiaApi.sk_imagefilter_new_picture_with_rect (picture.Handle, &cropRect));
		}

		// CreateTile

		public static SKImageFilter CreateTile (SKRect src, SKRect dst) =>
			CreateTile (src, dst, null);

		public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input)
		{
			_ = input ?? throw new ArgumentNullException (nameof (input));
			return GetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input.Handle));
		}

		// CreateBlendMode

		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background) =>
			CreateBlendMode (mode, background, null, null);

		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground) =>
			CreateBlendMode (mode, background, foreground, null);

		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect) =>
			CreateBlendMode (mode, background, foreground, &cropRect);

		private static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_blend (mode, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));

		// CreateArithmetic

		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, null, null);

		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, foreground, null);

		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, foreground, &cropRect);

		private static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_arithmetic (k1, k2, k3, k4, enforcePMColor, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));

		// CreateImage

		public static SKImageFilter CreateImage (SKImage image) =>
			CreateImage (image, new SKSamplingOptions (SKCubicResampler.Mitchell));

		public static SKImageFilter CreateImage (SKImage image, SKSamplingOptions sampling)
		{
			_ = image ?? throw new ArgumentNullException (nameof (image));
			return GetObject (SkiaApi.sk_imagefilter_new_image_simple (image.Handle, &sampling));
		}

		public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKSamplingOptions sampling)
		{
			_ = image ?? throw new ArgumentNullException (nameof (image));
			return GetObject (SkiaApi.sk_imagefilter_new_image (image.Handle, &src, &dst, &sampling));
		}

		[Obsolete("Use CreateImage(SKImage, SKRect, SKRect, SKSamplingOptions) instead.", true)]
		public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality) =>
			CreateImage (image, src, dst, filterQuality.ToSamplingOptions ());

		// CreateMagnifier

		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, null, null);

		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, input, null);

		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect cropRect) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, input, &cropRect);

		private static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_magnifier (&lensBounds, zoomAmount, inset, &sampling, input?.Handle ?? IntPtr.Zero, cropRect));

		// CreatePaint

		[Obsolete("Use CreateShader(SKShader) instead.", true)]
		public static SKImageFilter CreatePaint (SKPaint paint)
		{
			_ = paint ?? throw new ArgumentNullException (nameof (paint));
			return CreateShader(paint.Shader, paint.IsDither, null);
		}

		[Obsolete("Use CreateShader(SKShader, bool, SKRect) instead.", true)]
		public static SKImageFilter CreatePaint (SKPaint paint, SKRect cropRect)
		{
			_ = paint ?? throw new ArgumentNullException (nameof (paint));
			return CreateShader(paint.Shader, paint.IsDither, &cropRect);
		}

		// CreateShader

		public static SKImageFilter CreateShader (SKShader? shader) =>
			CreateShader (shader, false, null);

		public static SKImageFilter CreateShader (SKShader? shader, bool dither) =>
			CreateShader (shader, dither, null);

		public static SKImageFilter CreateShader (SKShader? shader, bool dither, SKRect cropRect) =>
			CreateShader (shader, dither, &cropRect);

		private static SKImageFilter CreateShader (SKShader? shader, bool dither, SKRect* cropRect) =>
			GetObject (SkiaApi.sk_imagefilter_new_shader (shader?.Handle ?? IntPtr.Zero, dither, cropRect));

		//

		internal static SKImageFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKImageFilter (h, o));
	}
}
