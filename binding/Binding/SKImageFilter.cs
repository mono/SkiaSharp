using System;
using System.ComponentModel;

namespace SkiaSharp
{
	// TODO: `asAColorFilter`
	// TODO: `countInputs`, `getInput`
	// TODO: `cropRectIsSet`, `getCropRect`
	// TODO: `computeFastBounds`, `canComputeFastBounds`

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKColorChannel instead.")]
	public enum SKDisplacementMapEffectChannelSelectorType
	{
		Unknown = 0,
		R = 1,
		G = 2,
		B = 3,
		A = 4,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use CreateDropShadow or CreateDropShadowOnly instead.")]
	public enum SKDropShadowImageFilterShadowMode
	{
		DrawShadowAndForeground = 0,
		DrawShadowOnly = 1,
	}

	[EditorBrowsable (EditorBrowsableState.Never)]
	[Obsolete ("Use SKShaderTileMode instead.")]
	public enum SKMatrixConvolutionTileMode
	{
		Clamp = 0,
		Repeat = 1,
		ClampToBlack = 2,

	}

	public static partial class SkiaExtensions
	{
		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use SKColorChannel instead.")]
		public static SKColorChannel ToColorChannel (this SKDisplacementMapEffectChannelSelectorType channelSelectorType) =>
			channelSelectorType switch
			{
				SKDisplacementMapEffectChannelSelectorType.R => SKColorChannel.R,
				SKDisplacementMapEffectChannelSelectorType.G => SKColorChannel.G,
				SKDisplacementMapEffectChannelSelectorType.B => SKColorChannel.B,
				SKDisplacementMapEffectChannelSelectorType.A => SKColorChannel.A,
				_ => SKColorChannel.B,
			};

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use SKShaderTileMode instead.")]
		public static SKShaderTileMode ToShaderTileMode (this SKMatrixConvolutionTileMode tileMode) =>
			tileMode switch
			{
				SKMatrixConvolutionTileMode.Clamp => SKShaderTileMode.Clamp,
				SKMatrixConvolutionTileMode.Repeat => SKShaderTileMode.Repeat,
				_ => SKShaderTileMode.Decal,
			};
	}

	public unsafe class SKImageFilter : SKObject, ISKReferenceCounted
	{
		internal SKImageFilter(IntPtr handle, bool owns)
			: base(handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// CreateMatrix

		public static SKImageFilter CreateMatrix(SKMatrix matrix, SKFilterQuality quality, SKImageFilter input = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_matrix(&matrix, quality, input == null ? IntPtr.Zero : input.Handle));
		}

		// CreateAlphaThreshold

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
			return GetObject(SkiaApi.sk_imagefilter_new_alpha_threshold(region.Handle, innerThreshold, outerThreshold, input == null ? IntPtr.Zero : input.Handle));
		}

		// CreateBlur

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, input, cropRect);

		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			GetObject (SkiaApi.sk_imagefilter_new_blur (sigmaX, sigmaY, tileMode, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));

		// CreateColorFilter

		public static SKImageFilter CreateColorFilter(SKColorFilter cf, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (cf == null)
				throw new ArgumentNullException(nameof(cf));
			return GetObject(SkiaApi.sk_imagefilter_new_color_filter(cf.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateCompose

		public static SKImageFilter CreateCompose(SKImageFilter outer, SKImageFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject(SkiaApi.sk_imagefilter_new_compose(outer.Handle, inner.Handle));
		}

		// CreateDisplacementMapEffect

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateDisplacementMapEffect(SKColorChannel, SKColorChannel, float, SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateDisplacementMapEffect (SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			CreateDisplacementMapEffect (xChannelSelector.ToColorChannel (), yChannelSelector.ToColorChannel (), scale, displacement, input, cropRect);

		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (displacement == null)
				throw new ArgumentNullException (nameof (displacement));
			return GetObject (SkiaApi.sk_imagefilter_new_displacement_map_effect (xChannelSelector, yChannelSelector, scale, displacement.Handle, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateDropShadow

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateDropShadow or CreateDropShadowOnly instead.")]
		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			shadowMode == SKDropShadowImageFilterShadowMode.DrawShadowOnly
				? CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, input, cropRect)
				: CreateDropShadow (dx, dy, sigmaX, sigmaY, color, input, cropRect);

		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			GetObject (SkiaApi.sk_imagefilter_new_drop_shadow (dx, dy, sigmaX, sigmaY, (uint)color, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));

		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			GetObject (SkiaApi.sk_imagefilter_new_drop_shadow_only (dx, dy, sigmaX, sigmaY, (uint)color, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));

		// Create*LitDiffuse

		public static SKImageFilter CreateDistantLitDiffuse(SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_diffuse(&direction, (uint)lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreatePointLitDiffuse(SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_point_lit_diffuse(&location, (uint)lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateSpotLitDiffuse(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_diffuse(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// Create*LitSpecular

		public static SKImageFilter CreateDistantLitSpecular(SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_distant_lit_specular(&direction, (uint)lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreatePointLitSpecular(SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_point_lit_specular(&location, (uint)lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		public static SKImageFilter CreateSpotLitSpecular(SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_spot_lit_specular(&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateMagnifier

		public static SKImageFilter CreateMagnifier(SKRect src, float inset, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_magnifier(&src, inset, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateMatrixConvolution

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use CreateMatrixConvolution(SKSizeI, float[], float, float, SKPointI, SKShaderTileMode, bool, SKImageFilter, SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode.ToShaderTileMode (), convolveAlpha, input, cropRect);

		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, float[] kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			if (kernel == null)
				throw new ArgumentNullException (nameof (kernel));
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException ("Kernel length must match the dimensions of the kernel size (Width * Height).", nameof (kernel));
			fixed (float* k = kernel) {
				return GetObject (SkiaApi.sk_imagefilter_new_matrix_convolution (&kernelSize, k, gain, bias, &kernelOffset, tileMode, convolveAlpha, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
			}
		}

		// CreateMerge

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use CreateMerge(SKImageFilter, SKImageFilter, SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKBlendMode mode, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge(new [] { first, second }, cropRect);
		}

		public static SKImageFilter CreateMerge(SKImageFilter first, SKImageFilter second, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge(new [] { first, second }, cropRect);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete("Use CreateMerge(SKImageFilter[], SKImageFilter.CropRect) instead.")]
		public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKBlendMode[] modes, SKImageFilter.CropRect cropRect = null)
		{
			return CreateMerge (filters, cropRect);
		}

		public static SKImageFilter CreateMerge(SKImageFilter[] filters, SKImageFilter.CropRect cropRect = null)
		{
			if (filters == null)
				throw new ArgumentNullException(nameof(filters));
			var handles = new IntPtr[filters.Length];
			for (int i = 0; i < filters.Length; i++)
			{
				handles[i] = filters[i]?.Handle ?? IntPtr.Zero;
			}
			fixed (IntPtr* h = handles) {
				return GetObject (SkiaApi.sk_imagefilter_new_merge (h, filters.Length, cropRect == null ? IntPtr.Zero : cropRect.Handle));
			}
		}

		// CreateDilate

		public static SKImageFilter CreateDilate(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_dilate(radiusX, radiusY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateErode

		public static SKImageFilter CreateErode(int radiusX, int radiusY, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_erode(radiusX, radiusY, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateOffset

		public static SKImageFilter CreateOffset(float dx, float dy, SKImageFilter input = null, SKImageFilter.CropRect cropRect = null)
		{
			return GetObject(SkiaApi.sk_imagefilter_new_offset(dx, dy, input == null ? IntPtr.Zero : input.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreatePicture

		public static SKImageFilter CreatePicture(SKPicture picture)
		{
			if (picture == null)
				throw new ArgumentNullException(nameof(picture));
			return GetObject(SkiaApi.sk_imagefilter_new_picture(picture.Handle));
		}

		public static SKImageFilter CreatePicture(SKPicture picture, SKRect cropRect)
		{
			if (picture == null)
				throw new ArgumentNullException(nameof(picture));
			return GetObject(SkiaApi.sk_imagefilter_new_picture_with_croprect(picture.Handle, &cropRect));
		}

		// CreateTile

		public static SKImageFilter CreateTile(SKRect src, SKRect dst, SKImageFilter input)
		{
			if (input == null)
				throw new ArgumentNullException(nameof(input));
			return GetObject(SkiaApi.sk_imagefilter_new_tile(&src, &dst, input.Handle));
		}

		// CreateBlendMode

		public static SKImageFilter CreateBlendMode(SKBlendMode mode, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException(nameof(background));
			return GetObject(SkiaApi.sk_imagefilter_new_xfermode(mode, background.Handle, foreground == null ? IntPtr.Zero : foreground.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateArithmetic

		public static SKImageFilter CreateArithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter background, SKImageFilter foreground = null, SKImageFilter.CropRect cropRect = null)
		{
			if (background == null)
				throw new ArgumentNullException(nameof(background));
			return GetObject(SkiaApi.sk_imagefilter_new_arithmetic(k1, k2, k3, k4, enforcePMColor, background.Handle, foreground == null ? IntPtr.Zero : foreground.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		// CreateImage

		public static SKImageFilter CreateImage(SKImage image)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			return GetObject(SkiaApi.sk_imagefilter_new_image_source_default(image.Handle));
		}

		public static SKImageFilter CreateImage(SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality)
		{
			if (image == null)
				throw new ArgumentNullException(nameof(image));
			return GetObject(SkiaApi.sk_imagefilter_new_image_source(image.Handle, &src, &dst, filterQuality));
		}

		// CreatePaint

		public static SKImageFilter CreatePaint(SKPaint paint, SKImageFilter.CropRect cropRect = null)
		{
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));
			return GetObject(SkiaApi.sk_imagefilter_new_paint(paint.Handle, cropRect == null ? IntPtr.Zero : cropRect.Handle));
		}

		internal static SKImageFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKImageFilter (h, o));

		//

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
				: this(SkiaApi.sk_imagefilter_croprect_new_with_rect(&rect, (uint)flags), true)
			{
			}

			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			protected override void DisposeNative () =>
				SkiaApi.sk_imagefilter_croprect_destructor (Handle);

			public SKRect Rect
			{
				get
				{
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
