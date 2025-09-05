#nullable disable

using System;
using System.IO;

namespace SkiaSharp
{
	/// <summary>
	/// Shaders specify the source color(s) for what is being drawn in the <see cref="T:SkiaSharp.SKPaint" />.
	/// </summary>
	/// <remarks>Shaders specify the source colors for what is being drawn. If a paint has no
	/// shader, then the paint's color is used. If the paint has a shader, then the
	/// shader's colors are used instead, but they are modulated by the paint's alpha.
	/// This makes it easy to create a shader once (for example, bitmap tiling or
	/// gradient) and then change its transparency without having to modify the
	/// original shader, only the paint's alpha needs to be modified.
	/// Shaders are created by calling one of the static "Create" methods.
	/// ## Examples
	/// ### Linear Gradient Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var colors = new SKColor[] {
	/// new SKColor(0, 0, 255),
	/// new SKColor(0, 255, 0)
	/// };
	/// var shader = SKShader.CreateLinearGradient(
	/// new SKPoint(0, 0),
	/// new SKPoint(255, 255),
	/// colors,
	/// null,
	/// SKShaderTileMode.Clamp);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Linear Gradient](~/images/linear.png "Linear Gradient")
	/// ### Radial Gradient Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var colors = new SKColor[] {
	/// new SKColor(0, 0, 255),
	/// new SKColor(0, 255, 0)
	/// };
	/// var shader = SKShader.CreateRadialGradient(
	/// new SKPoint(128, 128),
	/// 180,
	/// colors,
	/// null,
	/// SKShaderTileMode.Clamp);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Radial Gradient](~/images/radial.png "Radial Gradient")
	/// ### Two-point Conical Gradient Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var colors = new SKColor[] {
	/// new SKColor(0, 0, 255),
	/// new SKColor(0, 255, 0)
	/// };
	/// var shader = SKShader.CreateTwoPointConicalGradient(
	/// new SKPoint(128, 128),
	/// 128,
	/// new SKPoint(128, 16),
	/// 16,
	/// colors,
	/// null,
	/// SKShaderTileMode.Clamp);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Two-point Conical Gradient](~/images/twopoint.png "Two-point Conical Gradient")
	/// ### Sweep Gradient Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var colors = new SKColor[] {
	/// new SKColor(0, 255, 255),
	/// new SKColor(255, 0, 255),
	/// new SKColor(255, 255, 0),
	/// new SKColor(0, 255, 255)
	/// };
	/// var shader = SKShader.CreateSweepGradient(
	/// new SKPoint(128, 128),
	/// colors,
	/// null);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Sweep Gradient](~/images/sweep.png "Sweep Gradient")
	/// ### Fractal Perlin Noise Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var shader = SKShader.CreatePerlinNoiseFractalNoise(0.5f, 0.5f, 4, 0);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Fractal Perlin Noise](~/images/fractal-perlin-noise.png "Fractal Perlin Noise")
	/// ### Perlin Noise Turbulence Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the shader
	/// var shader = SKShader.CreatePerlinNoiseTurbulence(0.05f, 0.05f, 4, 0);
	/// // use the shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Fractal Perlin Noise](~/images/perlin-noise-turbulence.png "Fractal Perlin Noise")
	/// ### Compose Shader Example
	/// ```csharp
	/// var info = new SKImageInfo(256, 256);
	/// using (var surface = SKSurface.Create(info)) {
	/// SKCanvas canvas = surface.Canvas;
	/// canvas.Clear(SKColors.White);
	/// // create the first shader
	/// var colors = new SKColor[] {
	/// new SKColor(0, 255, 255),
	/// new SKColor(255, 0, 255),
	/// new SKColor(255, 255, 0),
	/// new SKColor(0, 255, 255)
	/// };
	/// var sweep = SKShader.CreateSweepGradient(new SKPoint(128, 128), colors, null);
	/// // create the second shader
	/// var turbulence = SKShader.CreatePerlinNoiseTurbulence(0.05f, 0.05f, 4, 0);
	/// // create the compose shader
	/// var shader = SKShader.CreateCompose(sweep, turbulence, SKBlendMode.SrcOver);
	/// // use the compose shader
	/// var paint = new SKPaint {
	/// Shader = shader
	/// };
	/// canvas.DrawPaint(paint);
	/// }
	/// ```
	/// The example above produces the following:
	/// ![Compose Shader](~/images/compose.png "Compose Shader")</remarks>
	public unsafe class SKShader : SKObject, ISKReferenceCounted
	{
		internal SKShader (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// WithColorFilter

		/// <param name="filter"></param>
		public SKShader WithColorFilter (SKColorFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return GetObject (SkiaApi.sk_shader_with_color_filter (Handle, filter.Handle));
		}

		// WithLocalMatrix

		/// <param name="localMatrix"></param>
		public SKShader WithLocalMatrix (SKMatrix localMatrix) =>
			GetObject (SkiaApi.sk_shader_with_local_matrix (Handle, &localMatrix));

		// CreateEmpty

		/// <summary>
		/// Creates a new "empty" shader that will not draw anything.
		/// </summary>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateEmpty () =>
			GetObject (SkiaApi.sk_shader_new_empty ());

		// CreateColor

		/// <summary>
		/// Creates a new shader that just draws the specified color.
		/// </summary>
		/// <param name="color">The color to paint.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateColor (SKColor color) =>
			GetObject (SkiaApi.sk_shader_new_color ((uint)color));

		/// <summary>
		/// Creates a new shader that just draws the specified color.
		/// </summary>
		/// <param name="color">The color to paint.</param>
		/// <param name="colorspace">The colorspace to use.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateColor (SKColorF color, SKColorSpace colorspace)
		{
			if (colorspace == null)
				throw new ArgumentNullException (nameof (colorspace));

			return GetObject (SkiaApi.sk_shader_new_color4f (&color, colorspace.Handle));
		}

		// CreateBitmap

		/// <summary>
		/// Creates a new shader that will draw with the specified bitmap.
		/// </summary>
		/// <param name="src">The bitmap to use inside the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		/// <remarks>If the bitmap cannot be used (has no pixels, or its dimensions exceed
		/// implementation limits) then an empty shader may be returned. If the source
		/// bitmap's color type is <see cref="SkiaSharp.SKColorType.Alpha8" /> then that mask will
		/// be colorized using the color on the paint.</remarks>
		public static SKShader CreateBitmap (SKBitmap src) =>
			CreateBitmap (src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		/// <summary>
		/// Creates a new shader that will draw with the specified bitmap.
		/// </summary>
		/// <param name="src">The bitmap to use inside the shader.</param>
		/// <param name="tmx">The tiling mode to use when sampling the bitmap in the x-direction.</param>
		/// <param name="tmy">The tiling mode to use when sampling the bitmap in the y-direction.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		/// <remarks>If the bitmap cannot be used (has no pixels, or its dimensions exceed
		/// implementation limits) then an empty shader may be returned. If the source
		/// bitmap's color type is <see cref="SkiaSharp.SKColorType.Alpha8" /> then that mask will
		/// be colorized using the color on the paint.</remarks>
		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		/// <summary>
		/// Creates a new shader that will draw with the specified bitmap.
		/// </summary>
		/// <param name="src">The bitmap to use inside the shader.</param>
		/// <param name="tmx">The tiling mode to use when sampling the bitmap in the x-direction.</param>
		/// <param name="tmy">The tiling mode to use when sampling the bitmap in the y-direction.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		/// <remarks>If the bitmap cannot be used (has no pixels, or its dimensions exceed
		/// implementation limits) then an empty shader may be returned. If the source
		/// bitmap's color type is <see cref="SkiaSharp.SKColorType.Alpha8" /> then that mask will
		/// be colorized using the color on the paint.</remarks>
		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix);
		}

		// CreateImage

		/// <param name="src"></param>
		public static SKShader CreateImage (SKImage src) =>
			src?.ToShader () ?? throw new ArgumentNullException (nameof (src));

		/// <param name="src"></param>
		/// <param name="tmx"></param>
		/// <param name="tmy"></param>
		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			src?.ToShader (tmx, tmy) ?? throw new ArgumentNullException (nameof (src));

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) =>
			src?.ToShader (tmx, tmy, sampling) ?? throw new ArgumentNullException (nameof (src));

		[Obsolete ("Use CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling) instead.")]
		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality) =>
			src?.ToShader (tmx, tmy, quality.ToSamplingOptions()) ?? throw new ArgumentNullException (nameof (src));

		/// <param name="src"></param>
		/// <param name="tmx"></param>
		/// <param name="tmy"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix) =>
			src?.ToShader (tmx, tmy, localMatrix) ?? throw new ArgumentNullException (nameof (src));

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) =>
			src?.ToShader (tmx, tmy, sampling, localMatrix) ?? throw new ArgumentNullException (nameof (src));

		[Obsolete ("Use CreateImage(SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKSamplingOptions sampling, SKMatrix localMatrix) instead.")]
		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterQuality quality, SKMatrix localMatrix) =>
			src?.ToShader (tmx, tmy, quality.ToSamplingOptions(), localMatrix) ?? throw new ArgumentNullException (nameof (src));

		// CreatePicture

		/// <param name="src"></param>
		public static SKShader CreatePicture (SKPicture src) =>
			src?.ToShader () ?? throw new ArgumentNullException (nameof (src));

		/// <summary>
		/// Creates a new shader that will draw with the specified picture.
		/// </summary>
		/// <param name="src">The picture to use inside the shader.</param>
		/// <param name="tmx">The tiling mode to use when sampling the picture in the x-direction.</param>
		/// <param name="tmy">The tiling mode to use when sampling the picture in the y-direction.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			src?.ToShader (tmx, tmy) ?? throw new ArgumentNullException (nameof (src));

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode) =>
			src?.ToShader (tmx, tmy, filterMode) ?? throw new ArgumentNullException (nameof (src));

		/// <summary>
		/// Creates a new shader that will draw with the specified picture.
		/// </summary>
		/// <param name="src">The picture to use inside the shader.</param>
		/// <param name="tmx">The tiling mode to use when sampling the picture in the x-direction.</param>
		/// <param name="tmy">The tiling mode to use when sampling the picture in the y-direction.</param>
		/// <param name="tile">The tile rectangle in picture coordinates.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		/// <remarks>The tile rectangle represents the subset (or superset) of the picture used when building a tile. It is not affected by
		/// the local matrix and does not imply scaling (only translation and cropping).</remarks>
		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile) =>
			src?.ToShader (tmx, tmy, tile) ?? throw new ArgumentNullException (nameof (src));

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKRect tile) =>
			src?.ToShader (tmx, tmy, filterMode, tile) ?? throw new ArgumentNullException (nameof (src));

		/// <summary>
		/// Creates a new shader that will draw with the specified picture.
		/// </summary>
		/// <param name="src">The picture to use inside the shader.</param>
		/// <param name="tmx">The tiling mode to use when sampling the picture in the x-direction.</param>
		/// <param name="tmy">The tiling mode to use when sampling the picture in the y-direction.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <param name="tile">The tile rectangle in picture coordinates.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		/// <remarks>The tile rectangle represents the subset (or superset) of the picture used when building a tile. It is not affected by
		/// the local matrix and does not imply scaling (only translation and cropping).</remarks>
		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile) =>
			src?.ToShader (tmx, tmy, localMatrix, tile) ?? throw new ArgumentNullException (nameof (src));

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKFilterMode filterMode, SKMatrix localMatrix, SKRect tile) =>
			src?.ToShader (tmx, tmy, filterMode, localMatrix, tile) ?? throw new ArgumentNullException (nameof (src));

		// CreateLinearGradient

		/// <summary>
		/// Creates a shader that generates a linear gradient between the two specified points.
		/// </summary>
		/// <param name="start">The start point for the gradient.</param>
		/// <param name="end">The end point for the gradient.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, null, mode);

		/// <summary>
		/// Creates a shader that generates a linear gradient between the two specified points.
		/// </summary>
		/// <param name="start">The start point for the gradient.</param>
		/// <param name="end">The end point for the gradient.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = stackalloc SKPoint[] { start, end };
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_linear_gradient (points, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		/// <summary>
		/// Creates a shader that generates a linear gradient between the two specified points.
		/// </summary>
		/// <param name="start">The start point for the gradient.</param>
		/// <param name="end">The end point for the gradient.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = stackalloc SKPoint[] { start, end };
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_linear_gradient (points, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}

		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="mode"></param>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, colorspace, null, mode);

		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = stackalloc SKPoint[] { start, end };
			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_linear_gradient_color4f (points, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, null));
			}
		}

		/// <param name="start"></param>
		/// <param name="end"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = stackalloc SKPoint[] { start, end };
			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_linear_gradient_color4f (points, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, &localMatrix));
			}
		}

		// CreateRadialGradient

		/// <summary>
		/// Creates a shader that generates a radial gradient given the center and radius.
		/// </summary>
		/// <param name="center">The center of the circle for this gradient.</param>
		/// <param name="radius">The positive radius of the circle for this gradient.</param>
		/// <param name="colors">The array colors to be distributed between the center and edge of the circle.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, null, mode);

		/// <summary>
		/// Creates a shader that generates a radial gradient given the center and radius.
		/// </summary>
		/// <param name="center">The center of the circle for this gradient.</param>
		/// <param name="radius">The positive radius of the circle for this gradient.</param>
		/// <param name="colors">The array colors to be distributed between the center and edge of the circle.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		/// <summary>
		/// Creates a shader that generates a radial gradient given the center and radius.
		/// </summary>
		/// <param name="center">The center of the circle for this gradient.</param>
		/// <param name="radius">The positive radius of the circle for this gradient.</param>
		/// <param name="colors">The array colors to be distributed between the center and edge of the circle.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}

		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="mode"></param>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, colorspace, null, mode);

		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_radial_gradient_color4f (&center, radius, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, null));
			}
		}

		/// <param name="center"></param>
		/// <param name="radius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_radial_gradient_color4f (&center, radius, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, &localMatrix));
			}
		}

		// CreateSweepGradient

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors) =>
			CreateSweepGradient (center, colors, null, SKShaderTileMode.Clamp, 0, 360);

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos) =>
			CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360);

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKMatrix localMatrix) =>
			CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <param name="tileMode">The tiling mode.</param>
		/// <param name="startAngle">The start of the angular range.</param>
		/// <param name="endAngle">The end of the angular range.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, SKShaderTileMode tileMode, float startAngle, float endAngle) =>
			CreateSweepGradient (center, colors, null, tileMode, startAngle, endAngle);

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="tileMode">The tiling mode.</param>
		/// <param name="startAngle">The start of the angular range.</param>
		/// <param name="endAngle">The end of the angular range.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_sweep_gradient (&center, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}

		/// <summary>
		/// Creates a shader that generates a sweep gradient given a center.
		/// </summary>
		/// <param name="center">The coordinates of the center of the sweep.</param>
		/// <param name="colors">The array colors to be distributed around the center.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="tileMode">The tiling mode.</param>
		/// <param name="startAngle">The start of the angular range.</param>
		/// <param name="endAngle">The end of the angular range.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_sweep_gradient (&center, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, &localMatrix));
			}
		}

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace) =>
			CreateSweepGradient (center, colors, colorspace, null, SKShaderTileMode.Clamp, 0, 360);

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos) =>
			CreateSweepGradient (center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0, 360);

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKMatrix localMatrix) =>
			CreateSweepGradient (center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="tileMode"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode tileMode, float startAngle, float endAngle) =>
			CreateSweepGradient (center, colors, colorspace, null, tileMode, startAngle, endAngle);

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="tileMode"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_sweep_gradient_color4f (&center, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}

		/// <param name="center"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="tileMode"></param>
		/// <param name="startAngle"></param>
		/// <param name="endAngle"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_sweep_gradient_color4f (&center, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, tileMode, startAngle, endAngle, &localMatrix));
			}
		}

		// CreateTwoPointConicalGradient

		/// <summary>
		/// Creates a shader that generates a conical gradient given two circles.
		/// </summary>
		/// <param name="start">The coordinates for the starting point.</param>
		/// <param name="startRadius">The radius at the starting point.</param>
		/// <param name="end">The coordinates for the end point.</param>
		/// <param name="endRadius">The radius at the end point.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or null on error.</returns>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, null, mode);

		/// <summary>
		/// Creates a shader that generates a conical gradient given two circles.
		/// </summary>
		/// <param name="start">The coordinates for the starting point.</param>
		/// <param name="startRadius">The radius at the starting point.</param>
		/// <param name="end">The coordinates for the end point.</param>
		/// <param name="endRadius">The radius at the end point.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or null on error.</returns>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		/// <summary>
		/// Creates a shader that generates a conical gradient given two circles.
		/// </summary>
		/// <param name="start">The coordinates for the starting point.</param>
		/// <param name="startRadius">The radius at the starting point.</param>
		/// <param name="end">The coordinates for the end point.</param>
		/// <param name="endRadius">The radius at the end point.</param>
		/// <param name="colors">The array colors to be distributed between the two points.</param>
		/// <param name="colorPos">The positions (in the range of 0..1) of each corresponding color, or null to evenly distribute the colors.</param>
		/// <param name="mode">The tiling mode.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}

		/// <param name="start"></param>
		/// <param name="startRadius"></param>
		/// <param name="end"></param>
		/// <param name="endRadius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="mode"></param>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, colorspace, null, mode);

		/// <param name="start"></param>
		/// <param name="startRadius"></param>
		/// <param name="end"></param>
		/// <param name="endRadius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_two_point_conical_gradient_color4f (&start, startRadius, &end, endRadius, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, null));
			}
		}

		/// <param name="start"></param>
		/// <param name="startRadius"></param>
		/// <param name="end"></param>
		/// <param name="endRadius"></param>
		/// <param name="colors"></param>
		/// <param name="colorspace"></param>
		/// <param name="colorPos"></param>
		/// <param name="mode"></param>
		/// <param name="localMatrix"></param>
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColorF* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject (SkiaApi.sk_shader_new_two_point_conical_gradient_color4f (&start, startRadius, &end, endRadius, c, colorspace?.Handle ?? IntPtr.Zero, cp, colors.Length, mode, &localMatrix));
			}
		}

		// CreatePerlinNoiseFractalNoise

		/// <summary>
		/// Creates a new shader that draws Perlin fractal noise.
		/// </summary>
		/// <param name="baseFrequencyX">The frequency in the x-direction in the range of 0..1.</param>
		/// <param name="baseFrequencyY">The frequency in the y-direction in the range of 0..1.</param>
		/// <param name="numOctaves">The number of octaves, usually fairly small.</param>
		/// <param name="seed">The randomization seed.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		/// <summary>
		/// Creates a new shader that draws Perlin fractal noise.
		/// </summary>
		/// <param name="baseFrequencyX">The frequency in the x-direction in the range of 0..1.</param>
		/// <param name="baseFrequencyY">The frequency in the y-direction in the range of 0..1.</param>
		/// <param name="numOctaves">The number of octaves, usually fairly small.</param>
		/// <param name="seed">The randomization seed.</param>
		/// <param name="tileSize">The tile size used to modify the frequencies so that the noise will be tileable for the given size.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize) =>
			CreatePerlinNoiseFractalNoise (baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);

		/// <param name="baseFrequencyX"></param>
		/// <param name="baseFrequencyY"></param>
		/// <param name="numOctaves"></param>
		/// <param name="seed"></param>
		/// <param name="tileSize"></param>
		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreatePerlinNoiseTurbulence

		/// <summary>
		/// Creates a new shader that draws Perlin turbulence noise.
		/// </summary>
		/// <param name="baseFrequencyX">The frequency in the x-direction in the range of 0..1.</param>
		/// <param name="baseFrequencyY">The frequency in the y-direction in the range of 0..1.</param>
		/// <param name="numOctaves">The number of octaves, usually fairly small.</param>
		/// <param name="seed">The randomization seed.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		/// <summary>
		/// Creates a new shader that draws Perlin turbulence noise.
		/// </summary>
		/// <param name="baseFrequencyX">The frequency in the x-direction in the range of 0..1.</param>
		/// <param name="baseFrequencyY">The frequency in the y-direction in the range of 0..1.</param>
		/// <param name="numOctaves">The number of octaves, usually fairly small.</param>
		/// <param name="seed">The randomization seed.</param>
		/// <param name="tileSize">The tile size used to modify the frequencies so that the noise will be tileable for the given size.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize) =>
			CreatePerlinNoiseTurbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);

		/// <param name="baseFrequencyX"></param>
		/// <param name="baseFrequencyY"></param>
		/// <param name="numOctaves"></param>
		/// <param name="seed"></param>
		/// <param name="tileSize"></param>
		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreateCompose

		/// <summary>
		/// Create a new compose shader, which combines two shaders by the <see cref="F:SkiaSharp.SKBlendMode.SrcOver" /> blend mode.
		/// </summary>
		/// <param name="shaderA">The colors from this shader are seen as the destination by the blend mode.</param>
		/// <param name="shaderB">The colors from this shader are seen as the source by the blend mode.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB) =>
			CreateCompose (shaderA, shaderB, SKBlendMode.SrcOver);

		/// <summary>
		/// Create a new compose shader, which combines two shaders by a blend mode.
		/// </summary>
		/// <param name="shaderA">The colors from this shader are seen as the destination by the blend mode.</param>
		/// <param name="shaderB">The colors from this shader are seen as the source by the blend mode.</param>
		/// <param name="mode">The blend mode that combines the two shaders.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKBlendMode mode)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));

			return GetObject (SkiaApi.sk_shader_new_blend (mode, shaderA.Handle, shaderB.Handle));
		}

		// CreateBlend

		public static SKShader CreateBlend (SKBlendMode mode, SKShader shaderA, SKShader shaderB)
		{
			_ = shaderA ?? throw new ArgumentNullException (nameof (shaderA));
			_ = shaderB ?? throw new ArgumentNullException (nameof (shaderB));
			return GetObject (SkiaApi.sk_shader_new_blend (mode, shaderA.Handle, shaderB.Handle));
		}

		public static SKShader CreateBlend (SKBlender blender, SKShader shaderA, SKShader shaderB)
		{
			_ = shaderA ?? throw new ArgumentNullException (nameof (shaderA));
			_ = shaderB ?? throw new ArgumentNullException (nameof (shaderB));
			_ = blender ?? throw new ArgumentNullException (nameof (blender));
			return GetObject (SkiaApi.sk_shader_new_blender (blender.Handle, shaderA.Handle, shaderB.Handle));
		}

		// CreateColorFilter

		/// <summary>
		/// Creates a new shader that produces the same colors as invoking this shader and then applying the color filter.
		/// </summary>
		/// <param name="shader">The shader to apply.</param>
		/// <param name="filter">The color filter to apply.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or an empty shader on error. This function never returns null.</returns>
		public static SKShader CreateColorFilter (SKShader shader, SKColorFilter filter)
		{
			if (shader == null)
				throw new ArgumentNullException (nameof (shader));
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return shader.WithColorFilter (filter);
		}

		// CreateLocalMatrix

		/// <summary>
		/// Creates a shader that first applies the specified matrix and then applies the shader.
		/// </summary>
		/// <param name="shader">The shader to apply.</param>
		/// <param name="localMatrix">The matrix to apply before applying the shader.</param>
		/// <returns>Returns a new <see cref="T:SkiaSharp.SKShader" />, or null on error.</returns>
		public static SKShader CreateLocalMatrix (SKShader shader, SKMatrix localMatrix)
		{
			if (shader == null)
				throw new ArgumentNullException (nameof (shader));

			return shader.WithLocalMatrix (localMatrix);
		}

		internal static SKShader GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKShader (h, o));
	}
}
