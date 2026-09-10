using System;

namespace SkiaSharp
{
	/// <summary>Image filters for use with the <see cref="P:SkiaSharp.SKPaint.ImageFilter" /> property of a <see cref="T:SkiaSharp.SKPaint" />.</summary>
	/// <remarks />
	public unsafe class SKImageFilter : SKObject, ISKReferenceCounted
	{
		internal SKImageFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKImageFilter" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKImageFilter" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// CreateMatrix

		/// <param name="matrix">The transformation matrix.</param>
		/// <summary>Creates an image filter that applies a transformation matrix.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		[Obsolete("Use SetMatrix(in SKMatrix) instead.", true)]
		public static SKImageFilter CreateMatrix (SKMatrix matrix) =>
			CreateMatrix (in matrix);

		/// <summary>Creates an image filter that applies a matrix using the legacy filtering quality.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <param name="quality">The legacy filtering quality.</param>
		/// <param name="input">The input filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>A new image filter, or <see langword="null" /> on error.</returns>
		/// <remarks />
		[Obsolete("Use SetMatrix(in SKMatrix, SKSamplingOptions, SKImageFilter) instead.", true)]
		public static SKImageFilter CreateMatrix (SKMatrix matrix, SKFilterQuality quality, SKImageFilter? input) =>
			CreateMatrix (in matrix, quality.ToSamplingOptions (), input);

		/// <summary>Creates an image filter that applies a transformation matrix.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrix (in SKMatrix matrix) =>
			CreateMatrix (matrix, SKSamplingOptions.Default, null);

		/// <summary>Creates an image filter that applies a transformation matrix.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrix (in SKMatrix matrix, SKSamplingOptions sampling) =>
			CreateMatrix (matrix, sampling, null);

		/// <summary>Creates an image filter that applies a transformation matrix.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrix (in SKMatrix matrix, SKSamplingOptions sampling, SKImageFilter? input)
		{
			fixed (SKMatrix* m = &matrix) {
				var filter = GetObject (SkiaApi.sk_imagefilter_new_matrix_transform (m, &sampling, input?.Handle ?? IntPtr.Zero));
				GC.KeepAlive (input);
				return filter;
			}
		}

		// CreateBlur

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, null, null);

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter? input) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, input, null);

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKImageFilter? input, SKRect cropRect) =>
			CreateBlur (sigmaX, sigmaY, SKShaderTileMode.Decal, input, &cropRect);

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode) =>
			CreateBlur (sigmaX, sigmaY, tileMode, null, null);

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input) =>
			CreateBlur (sigmaX, sigmaY, tileMode, input, null);

		/// <summary>Creates an image filter that applies a Gaussian blur.</summary>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect cropRect) =>
			CreateBlur (sigmaX, sigmaY, tileMode, input, &cropRect);

		private static SKImageFilter CreateBlur (float sigmaX, float sigmaY, SKShaderTileMode tileMode, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_blur (sigmaX, sigmaY, tileMode, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateColorFilter

		/// <summary>Creates an image filter that applies a color filter.</summary>
		/// <param name="cf">The color filter to apply.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateColorFilter (SKColorFilter cf) =>
			CreateColorFilter (cf, null, null);

		/// <summary>Creates an image filter that applies a color filter.</summary>
		/// <param name="cf">The color filter to apply.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input) =>
			CreateColorFilter (cf, input, null);

		/// <summary>Creates an image filter that applies a color filter.</summary>
		/// <param name="cf">The color filter to apply.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input, SKRect cropRect) =>
			CreateColorFilter (cf, input, &cropRect);

		private static SKImageFilter CreateColorFilter (SKColorFilter cf, SKImageFilter? input, SKRect* cropRect)
		{
			_ = cf ?? throw new ArgumentNullException (nameof (cf));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_color_filter (cf.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (cf);
			GC.KeepAlive (input);
			return filter;
		}

		// CreateCompose

		/// <summary>Creates an image filter, whose effect is to first apply the inner filter and then apply the outer filter to the result of the inner.</summary>
		/// <param name="outer">The outer (second) filter to apply.</param>
		/// <param name="inner">The inner (first) filter to apply.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateCompose (SKImageFilter outer, SKImageFilter inner)
		{
			_ = outer ?? throw new ArgumentNullException (nameof (outer));
			_ = inner ?? throw new ArgumentNullException (nameof (inner));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_compose (outer.Handle, inner.Handle));
			GC.KeepAlive (outer);
			GC.KeepAlive (inner);
			return filter;
		}

		// CreateCrop

		/// <summary>Creates an image filter that restricts the source to the specified rectangle, treating pixels outside the rectangle as transparent.</summary>
		/// <param name="rect">The crop rectangle, in the local coordinate space of the filter.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks><para>This overload is equivalent to calling <see cref="M:SkiaSharp.SKImageFilter.CreateCrop(SkiaSharp.SKRect,SkiaSharp.SKShaderTileMode)" /> with <see cref="F:SkiaSharp.SKShaderTileMode.Decal" />.</para></remarks>
		public static SKImageFilter CreateCrop (SKRect rect) =>
			CreateCrop (rect, SKShaderTileMode.Decal, null);

		/// <summary>Creates an image filter that restricts the source to the specified rectangle, using the supplied tile mode for pixels that fall outside it.</summary>
		/// <param name="rect">The crop rectangle, in the local coordinate space of the filter.</param>
		/// <param name="tileMode">The tile mode that determines how pixels outside <paramref name="rect" /> are produced.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateCrop (SKRect rect, SKShaderTileMode tileMode) =>
			CreateCrop (rect, tileMode, null);

		/// <summary>Creates an image filter that restricts the output of <paramref name="input" /> to the specified rectangle, using the supplied tile mode for pixels that fall outside it.</summary>
		/// <param name="rect">The crop rectangle, in the local coordinate space of the filter.</param>
		/// <param name="tileMode">The tile mode that determines how pixels outside <paramref name="rect" /> are produced.</param>
		/// <param name="input">The input filter whose output is cropped, or <see langword="null" /> to crop the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateCrop (SKRect rect, SKShaderTileMode tileMode, SKImageFilter? input)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_crop (&rect, tileMode, input?.Handle ?? IntPtr.Zero));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateDisplacementMapEffect

		/// <summary>Creates an image filter that uses one image to displace pixels in another.</summary>
		/// <param name="xChannelSelector">The color channel to use for X displacement.</param>
		/// <param name="yChannelSelector">The color channel to use for Y displacement.</param>
		/// <param name="scale">The displacement scale factor.</param>
		/// <param name="displacement">The displacement map filter.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, null, null);

		/// <summary>Creates an image filter that uses one image to displace pixels in another.</summary>
		/// <param name="xChannelSelector">The color channel to use for X displacement.</param>
		/// <param name="yChannelSelector">The color channel to use for Y displacement.</param>
		/// <param name="scale">The displacement scale factor.</param>
		/// <param name="displacement">The displacement map filter.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, input, null);

		/// <summary>Creates an image filter that uses one image to displace pixels in another.</summary>
		/// <param name="xChannelSelector">The color channel to use for X displacement.</param>
		/// <param name="yChannelSelector">The color channel to use for Y displacement.</param>
		/// <param name="scale">The displacement scale factor.</param>
		/// <param name="displacement">The displacement map filter.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect cropRect) =>
			CreateDisplacementMapEffect (xChannelSelector, yChannelSelector, scale, displacement, input, &cropRect);

		private static SKImageFilter CreateDisplacementMapEffect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, float scale, SKImageFilter displacement, SKImageFilter? input, SKRect* cropRect)
		{
			_ = displacement ?? throw new ArgumentNullException (nameof (displacement));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_displacement_map_effect (xChannelSelector, yChannelSelector, scale, displacement.Handle, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (displacement);
			GC.KeepAlive (input);
			return filter;
		}

		// CreateDropShadow

		/// <summary>Creates an image filter that draws a drop shadow.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, null, null);

		/// <summary>Creates an image filter that draws a drop shadow.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, input, null);

		/// <summary>Creates an image filter that draws a drop shadow.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect) =>
			CreateDropShadow (dx, dy, sigmaX, sigmaY, color, input, &cropRect);

		private static SKImageFilter CreateDropShadow (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_drop_shadow (dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateDropShadowOnly

		/// <summary>Creates an image filter that draws only the drop shadow without the source image.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, null, null);

		/// <summary>Creates an image filter that draws only the drop shadow without the source image.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, input, null);

		/// <summary>Creates an image filter that draws only the drop shadow without the source image.</summary>
		/// <param name="dx">The X offset of the shadow.</param>
		/// <param name="dy">The Y offset of the shadow.</param>
		/// <param name="sigmaX">The Gaussian blur standard deviation in the X direction.</param>
		/// <param name="sigmaY">The Gaussian blur standard deviation in the Y direction.</param>
		/// <param name="color">The color of the shadow.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect cropRect) =>
			CreateDropShadowOnly (dx, dy, sigmaX, sigmaY, color, input, &cropRect);

		private static SKImageFilter CreateDropShadowOnly (float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_drop_shadow_only (dx, dy, sigmaX, sigmaY, (uint)color, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateEmpty

		/// <summary>Creates an image filter that produces a fully transparent result regardless of the source.</summary>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateEmpty () =>
			GetObject (SkiaApi.sk_imagefilter_new_empty ());

		// CreateDistantLitDiffuse

		/// <summary>Creates an image filter that applies distant light diffuse lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, null, null);

		/// <summary>Creates an image filter that applies distant light diffuse lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, input, null);

		/// <summary>Creates an image filter that applies distant light diffuse lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreateDistantLitDiffuse (direction, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreateDistantLitDiffuse (SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_distant_lit_diffuse (&direction, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreatePointLitDiffuse

		/// <summary>Creates an image filter that applies point light diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, null, null);

		/// <summary>Creates an image filter that applies point light diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, input, null);

		/// <summary>Creates an image filter that applies point light diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreatePointLitDiffuse (location, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreatePointLitDiffuse (SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_point_lit_diffuse (&location, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateSpotLitDiffuse

		/// <summary>Creates an image filter that applies spotlight diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, null, null);

		/// <summary>Creates an image filter that applies spotlight diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, null);

		/// <summary>Creates an image filter that applies spotlight diffuse lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="kd">The diffuse lighting constant.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect cropRect) =>
			CreateSpotLitDiffuse (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, &cropRect);

		private static SKImageFilter CreateSpotLitDiffuse (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_spot_lit_diffuse (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, kd, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateDistantLitSpecular

		/// <summary>Creates an image filter that applies distant light specular lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, null, null);

		/// <summary>Creates an image filter that applies distant light specular lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, input, null);

		/// <summary>Creates an image filter that applies distant light specular lighting using bump mapping.</summary>
		/// <param name="direction">The direction of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreateDistantLitSpecular (direction, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreateDistantLitSpecular (SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_distant_lit_specular (&direction, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreatePointLitSpecular

		/// <summary>Creates an image filter that applies point light specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, null, null);

		/// <summary>Creates an image filter that applies point light specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, input, null);

		/// <summary>Creates an image filter that applies point light specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreatePointLitSpecular (location, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreatePointLitSpecular (SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_point_lit_specular (&location, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateSpotLitSpecular

		/// <summary>Creates an image filter that applies spotlight specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, null, null);

		/// <summary>Creates an image filter that applies spotlight specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, null);

		/// <summary>Creates an image filter that applies spotlight specular lighting using bump mapping.</summary>
		/// <param name="location">The position of the light.</param>
		/// <param name="target">The spotlight target point.</param>
		/// <param name="specularExponent">The spotlight falloff exponent.</param>
		/// <param name="cutoffAngle">The spotlight cutoff angle in degrees.</param>
		/// <param name="lightColor">The color of the light.</param>
		/// <param name="surfaceScale">The surface height scale factor for bump mapping.</param>
		/// <param name="ks">The specular lighting constant.</param>
		/// <param name="shininess">The specular highlight exponent (higher values create tighter highlights).</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect cropRect) =>
			CreateSpotLitSpecular (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, &cropRect);

		private static SKImageFilter CreateSpotLitSpecular (SKPoint3 location, SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_spot_lit_specular (&location, &target, specularExponent, cutoffAngle, (uint)lightColor, surfaceScale, ks, shininess, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateMatrixConvolution

		/// <summary>Creates an image filter that applies a matrix convolution.</summary>
		/// <param name="kernelSize">The size of the convolution kernel.</param>
		/// <param name="kernel">The convolution kernel values.</param>
		/// <param name="gain">The gain factor applied to each pixel.</param>
		/// <param name="bias">The bias added to each pixel after applying the kernel.</param>
		/// <param name="kernelOffset">The offset within the kernel for the source pixel.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <param name="convolveAlpha">Whether to also convolve the alpha channel.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, null, null);

		/// <summary>Creates an image filter that applies a matrix convolution.</summary>
		/// <param name="kernelSize">The size of the convolution kernel.</param>
		/// <param name="kernel">The convolution kernel values.</param>
		/// <param name="gain">The gain factor applied to each pixel.</param>
		/// <param name="bias">The bias added to each pixel after applying the kernel.</param>
		/// <param name="kernelOffset">The offset within the kernel for the source pixel.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <param name="convolveAlpha">Whether to also convolve the alpha channel.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, null);

		/// <summary>Creates an image filter that applies a matrix convolution.</summary>
		/// <param name="kernelSize">The size of the convolution kernel.</param>
		/// <param name="kernel">The convolution kernel values.</param>
		/// <param name="gain">The gain factor applied to each pixel.</param>
		/// <param name="bias">The bias added to each pixel after applying the kernel.</param>
		/// <param name="kernelOffset">The offset within the kernel for the source pixel.</param>
		/// <param name="tileMode">The tile mode for edge handling.</param>
		/// <param name="convolveAlpha">Whether to also convolve the alpha channel.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect cropRect) =>
			CreateMatrixConvolution (kernelSize, kernel, gain, bias, kernelOffset, tileMode, convolveAlpha, input, &cropRect);

		private static SKImageFilter CreateMatrixConvolution (SKSizeI kernelSize, ReadOnlySpan<float> kernel, float gain, float bias, SKPointI kernelOffset, SKShaderTileMode tileMode, bool convolveAlpha, SKImageFilter? input, SKRect* cropRect)
		{
			if (kernel.Length != kernelSize.Width * kernelSize.Height)
				throw new ArgumentException ("Kernel length must match the dimensions of the kernel size (Width * Height).", nameof (kernel));
			fixed (float* k = kernel) {
				var filter = GetObject (SkiaApi.sk_imagefilter_new_matrix_convolution (&kernelSize, k, gain, bias, &kernelOffset, tileMode, convolveAlpha, input?.Handle ?? IntPtr.Zero, cropRect));
				GC.KeepAlive (input);
				return filter;
			}
		}

		// CreateMerge

		/// <summary>Creates an image filter that merges multiple filters together.</summary>
		/// <param name="first">The first filter to compose.</param>
		/// <param name="second">The second filter to compose.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second) =>
			CreateMerge (first, second, null);

		/// <summary>Creates an image filter that merges multiple filters together.</summary>
		/// <param name="first">The first filter to compose.</param>
		/// <param name="second">The second filter to compose.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second, SKRect cropRect) =>
			CreateMerge (first, second, &cropRect);

		private static SKImageFilter CreateMerge (SKImageFilter? first, SKImageFilter? second, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_merge_simple (first?.Handle ?? IntPtr.Zero, second?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (first);
			GC.KeepAlive (second);
			return filter;
		}

		/// <summary>Creates an image filter that merges multiple filters together.</summary>
		/// <param name="filters">The array of image filters to merge.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters) =>
			CreateMerge (filters, null);

		/// <summary>Creates an image filter that merges multiple filters together.</summary>
		/// <param name="filters">The array of image filters to merge.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMerge (ReadOnlySpan<SKImageFilter> filters, SKRect cropRect) =>
			CreateMerge (filters, &cropRect);

		/// <summary>Creates an image filter that merges multiple filters together.</summary>
		/// <param name="filters">The array of image filters to merge.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
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

		/// <summary>Creates an image filter that applies a morphological dilation, expanding bright regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDilate (float radiusX, float radiusY) =>
			CreateDilate (radiusX, radiusY, null, null);

		/// <summary>Creates an image filter that applies a morphological dilation, expanding bright regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateDilate (radiusX, radiusY, input, null);

		/// <summary>Creates an image filter that applies a morphological dilation, expanding bright regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateDilate (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateDilate (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_dilate (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateErode

		/// <summary>Creates an image filter that applies a morphological erosion, expanding dark regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateErode (float radiusX, float radiusY) =>
			CreateErode (radiusX, radiusY, null, null);

		/// <summary>Creates an image filter that applies a morphological erosion, expanding dark regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateErode (radiusX, radiusY, input, null);

		/// <summary>Creates an image filter that applies a morphological erosion, expanding dark regions.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateErode (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateErode (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_erode (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateOffset

		/// <summary>Creates an image filter that offsets the input image.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateOffset (float radiusX, float radiusY) =>
			CreateOffset (radiusX, radiusY, null, null);

		/// <summary>Creates an image filter that offsets the input image.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input) =>
			CreateOffset (radiusX, radiusY, input, null);

		/// <summary>Creates an image filter that offsets the input image.</summary>
		/// <param name="radiusX">The morphology radius in the X direction.</param>
		/// <param name="radiusY">The morphology radius in the Y direction.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input, SKRect cropRect) =>
			CreateOffset (radiusX, radiusY, input, &cropRect);

		private static SKImageFilter CreateOffset (float radiusX, float radiusY, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_offset (radiusX, radiusY, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreatePicture

		/// <summary>Creates an image filter that draws a picture.</summary>
		/// <param name="picture">The picture to draw.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePicture (SKPicture picture)
		{
			_ = picture ?? throw new ArgumentNullException (nameof (picture));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_picture (picture.Handle));
			GC.KeepAlive (picture);
			return filter;
		}

		/// <summary>Creates an image filter that draws a picture.</summary>
		/// <param name="picture">The picture to draw.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreatePicture (SKPicture picture, SKRect cropRect)
		{
			_ = picture ?? throw new ArgumentNullException (nameof (picture));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_picture_with_rect (picture.Handle, &cropRect));
			GC.KeepAlive (picture);
			return filter;
		}

		// CreateTile

		/// <summary>Creates an image filter that tiles the source image.</summary>
		/// <param name="src">The source rectangle to tile.</param>
		/// <param name="dst">The destination rectangle where tiles are drawn.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateTile (SKRect src, SKRect dst) =>
			CreateTile (src, dst, null);

		/// <summary>Creates an image filter that tiles the image being drawn.</summary>
		/// <param name="src">The pixels to tile.</param>
		/// <param name="dst">The pixels where the tiles are drawn.</param>
		/// <param name="input">The input filter to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateTile (SKRect src, SKRect dst, SKImageFilter? input)
		{
			_ = input ?? throw new ArgumentNullException (nameof (input));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_tile (&src, &dst, input.Handle));
			GC.KeepAlive (input);
			return filter;
		}

		// CreateBlendMode

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="mode">The blend mode to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background) =>
			CreateBlendMode (mode, background, null, null);

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="mode">The blend mode to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground) =>
			CreateBlendMode (mode, background, foreground, null);

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="mode">The blend mode to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect) =>
			CreateBlendMode (mode, background, foreground, &cropRect);

		private static SKImageFilter CreateBlendMode (SKBlendMode mode, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_blend (mode, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (background);
			GC.KeepAlive (foreground);
			return filter;
		}

		// CreateBlendMode (Blender)

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="blender">The custom blender to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter? background) =>
			CreateBlendMode (blender, background, null, null);

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="blender">The custom blender to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter? background, SKImageFilter? foreground) =>
			CreateBlendMode (blender, background, foreground, null);

		/// <summary>Creates an image filter that blends two images using the specified blend mode.</summary>
		/// <param name="blender">The custom blender to use.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect) =>
			CreateBlendMode (blender, background, foreground, &cropRect);

		private static SKImageFilter CreateBlendMode (SKBlender blender, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
		{
			_ = blender ?? throw new ArgumentNullException (nameof (blender));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_blender (blender.Handle, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (blender);
			GC.KeepAlive (background);
			GC.KeepAlive (foreground);
			return filter;
		}

		// CreateArithmetic

		/// <summary>Creates an image filter that combines two images using the arithmetic blend formula: result = k1*src*dst + k2*src + k3*dst + k4.</summary>
		/// <param name="k1">The first arithmetic blend coefficient.</param>
		/// <param name="k2">The second arithmetic blend coefficient.</param>
		/// <param name="k3">The third arithmetic blend coefficient.</param>
		/// <param name="k4">The fourth arithmetic blend coefficient.</param>
		/// <param name="enforcePMColor">Whether to enforce premultiplied colors in the result.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, null, null);

		/// <summary>Creates an image filter that combines two images using the arithmetic blend formula: result = k1*src*dst + k2*src + k3*dst + k4.</summary>
		/// <param name="k1">The first arithmetic blend coefficient.</param>
		/// <param name="k2">The second arithmetic blend coefficient.</param>
		/// <param name="k3">The third arithmetic blend coefficient.</param>
		/// <param name="k4">The fourth arithmetic blend coefficient.</param>
		/// <param name="enforcePMColor">Whether to enforce premultiplied colors in the result.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, foreground, null);

		/// <summary>Creates an image filter that combines two images using the arithmetic blend formula: result = k1*src*dst + k2*src + k3*dst + k4.</summary>
		/// <param name="k1">The first arithmetic blend coefficient.</param>
		/// <param name="k2">The second arithmetic blend coefficient.</param>
		/// <param name="k3">The third arithmetic blend coefficient.</param>
		/// <param name="k4">The fourth arithmetic blend coefficient.</param>
		/// <param name="enforcePMColor">Whether to enforce premultiplied colors in the result.</param>
		/// <param name="background">The background image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="foreground">The foreground image filter, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect cropRect) =>
			CreateArithmetic (k1, k2, k3, k4, enforcePMColor, background, foreground, &cropRect);

		private static SKImageFilter CreateArithmetic (float k1, float k2, float k3, float k4, bool enforcePMColor, SKImageFilter? background, SKImageFilter? foreground, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_arithmetic (k1, k2, k3, k4, enforcePMColor, background?.Handle ?? IntPtr.Zero, foreground?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (background);
			GC.KeepAlive (foreground);
			return filter;
		}

		// CreateImage

		/// <summary>Creates an image filter that draws an image.</summary>
		/// <param name="image">The image to draw.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateImage (SKImage image) =>
			CreateImage (image, new SKSamplingOptions (SKCubicResampler.Mitchell));

		/// <summary>Creates an image filter from an image.</summary>
		/// <param name="image">The source image.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateImage (SKImage image, SKSamplingOptions sampling)
		{
			_ = image ?? throw new ArgumentNullException (nameof (image));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_image_simple (image.Handle, &sampling));
			GC.KeepAlive (image);
			return filter;
		}

		/// <summary>Creates an image filter from an image.</summary>
		/// <param name="image">The source image.</param>
		/// <param name="src">The source rectangle to tile.</param>
		/// <param name="dst">The destination rectangle where tiles are drawn.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKSamplingOptions sampling)
		{
			_ = image ?? throw new ArgumentNullException (nameof (image));
			var filter = GetObject (SkiaApi.sk_imagefilter_new_image (image.Handle, &src, &dst, &sampling));
			GC.KeepAlive (image);
			return filter;
		}

		/// <summary>Creates an image filter from an image using the legacy filtering quality.</summary>
		/// <param name="image">The source image.</param>
		/// <param name="src">The source rectangle.</param>
		/// <param name="dst">The destination rectangle.</param>
		/// <param name="filterQuality">The legacy filtering quality.</param>
		/// <returns>A new image filter, or <see langword="null" /> on error.</returns>
		/// <remarks />
		[Obsolete("Use CreateImage(SKImage, SKRect, SKRect, SKSamplingOptions) instead.", true)]
		public static SKImageFilter CreateImage (SKImage image, SKRect src, SKRect dst, SKFilterQuality filterQuality) =>
			CreateImage (image, src, dst, filterQuality.ToSamplingOptions ());

		// CreateMagnifier

		/// <summary>Creates an image filter that applies a magnifier effect.</summary>
		/// <param name="lensBounds">The bounds of the magnifier lens.</param>
		/// <param name="zoomAmount">The zoom magnification factor.</param>
		/// <param name="inset">The inset amount for the magnifier effect.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, null, null);

		/// <summary>Creates an image filter that applies a magnifier effect.</summary>
		/// <param name="lensBounds">The bounds of the magnifier lens.</param>
		/// <param name="zoomAmount">The zoom magnification factor.</param>
		/// <param name="inset">The inset amount for the magnifier effect.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, input, null);

		/// <summary>Creates an image filter that applies a magnifier effect.</summary>
		/// <param name="lensBounds">The bounds of the magnifier lens.</param>
		/// <param name="zoomAmount">The zoom magnification factor.</param>
		/// <param name="inset">The inset amount for the magnifier effect.</param>
		/// <param name="sampling">The sampling options for the transformation.</param>
		/// <param name="input">The input filter to use, or <see langword="null" /> to use the source bitmap.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect cropRect) =>
			CreateMagnifier (lensBounds, zoomAmount, inset, sampling, input, &cropRect);

		private static SKImageFilter CreateMagnifier (SKRect lensBounds, float zoomAmount, float inset, SKSamplingOptions sampling, SKImageFilter? input, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_magnifier (&lensBounds, zoomAmount, inset, &sampling, input?.Handle ?? IntPtr.Zero, cropRect));
			GC.KeepAlive (input);
			return filter;
		}

		// CreatePaint

		/// <summary>Creates an image filter from the paint's shader.</summary>
		/// <param name="paint">The paint to use.</param>
		/// <returns>A new image filter, or <see langword="null" /> on error.</returns>
		/// <remarks />
		[Obsolete("Use CreateShader(SKShader) instead.", true)]
		public static SKImageFilter CreatePaint (SKPaint paint)
		{
			_ = paint ?? throw new ArgumentNullException (nameof (paint));
			return CreateShader(paint.Shader, paint.IsDither, null);
		}

		/// <summary>Creates an image filter from the paint's shader and crop rectangle.</summary>
		/// <param name="paint">The paint to use.</param>
		/// <param name="cropRect">The crop rectangle.</param>
		/// <returns>A new image filter, or <see langword="null" /> on error.</returns>
		/// <remarks />
		[Obsolete("Use CreateShader(SKShader, bool, SKRect) instead.", true)]
		public static SKImageFilter CreatePaint (SKPaint paint, SKRect cropRect)
		{
			_ = paint ?? throw new ArgumentNullException (nameof (paint));
			return CreateShader(paint.Shader, paint.IsDither, &cropRect);
		}

		// CreateShader

		/// <summary>Creates an image filter from a shader.</summary>
		/// <param name="shader">The shader to use.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateShader (SKShader? shader) =>
			CreateShader (shader, false, null);

		/// <summary>Creates an image filter from a shader.</summary>
		/// <param name="shader">The shader to use.</param>
		/// <param name="dither">Whether to dither the output.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateShader (SKShader? shader, bool dither) =>
			CreateShader (shader, dither, null);

		/// <summary>Creates an image filter from a shader.</summary>
		/// <param name="shader">The shader to use.</param>
		/// <param name="dither">Whether to dither the output.</param>
		/// <param name="cropRect">The rectangle to which the output processing will be limited.</param>
		/// <returns>Returns the new <see cref="T:SkiaSharp.SKImageFilter" />, or <see langword="null" /> on error.</returns>
		/// <remarks />
		public static SKImageFilter CreateShader (SKShader? shader, bool dither, SKRect cropRect) =>
			CreateShader (shader, dither, &cropRect);

		private static SKImageFilter CreateShader (SKShader? shader, bool dither, SKRect* cropRect)
		{
			var filter = GetObject (SkiaApi.sk_imagefilter_new_shader (shader?.Handle ?? IntPtr.Zero, dither, cropRect));
			GC.KeepAlive (shader);
			return filter;
		}

		//

		internal static SKImageFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKImageFilter (h, o));
	}
}
