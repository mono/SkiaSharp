using System;

namespace SkiaSharp
{
	public unsafe class SKShader : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKShader (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// WithColorFilter

		public SKShader WithColorFilter (SKColorFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return GetObject<SKShader> (SkiaApi.sk_shader_with_color_filter (Handle, filter.Handle));
		}

		// WithLocalMatrix

		public SKShader WithLocalMatrix (in SKMatrix localMatrix)
		{
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_with_local_matrix (Handle, m));
			}
		}

		// CreateEmpty

		public static SKShader CreateEmpty () =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_empty ());

		// CreateColor

		public static SKShader CreateColor (SKColor color) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_color ((uint)color));

		// CreateBitmap

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx = SKShaderTileMode.Clamp, SKShaderTileMode tmy = SKShaderTileMode.Clamp)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, in SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix);
		}

		// CreateImage

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx = SKShaderTileMode.Clamp, SKShaderTileMode tmy = SKShaderTileMode.Clamp)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, in SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix);
		}

		// CreatePicture

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx = SKShaderTileMode.Clamp, SKShaderTileMode tmy = SKShaderTileMode.Clamp)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, tile);
		}

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, in SKMatrix localMatrix, SKRect tile)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix, tile);
		}

		// CreateLinearGradient

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, ReadOnlySpan<SKColor> colors, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, null, mode);

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = new SKPoint[] { start, end };
			fixed (SKPoint* p = points)
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (p, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode, in SKMatrix localMatrix)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = new SKPoint[] { start, end };
			fixed (SKPoint* p = points)
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos)
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (p, (uint*)c, cp, colors.Length, mode, m));
			}
		}

		// CreateRadialGradient

		public static SKShader CreateRadialGradient (SKPoint center, float radius, ReadOnlySpan<SKColor> colors, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, null, mode);

		public static SKShader CreateRadialGradient (SKPoint center, float radius, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		public static SKShader CreateRadialGradient (SKPoint center, float radius, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode, in SKMatrix localMatrix)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos)
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, m));
			}
		}

		// CreateSweepGradient

		public static SKShader CreateSweepGradient (SKPoint center, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos = default, SKShaderTileMode tileMode = SKShaderTileMode.Clamp, float startAngle = 0f, float endAngle = 360f)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (center.X, center.Y, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}

		public static SKShader CreateSweepGradient (SKPoint center, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, in SKMatrix localMatrix)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos)
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (center.X, center.Y, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, m));
			}
		}

		// CreateTwoPointConicalGradient

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, ReadOnlySpan<SKColor> colors, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, null, mode);

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, ReadOnlySpan<SKColor> colors, ReadOnlySpan<float> colorPos, SKShaderTileMode mode, in SKMatrix localMatrix)
		{
			if (!colorPos.IsEmpty && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos)
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, m));
			}
		}

		// CreatePerlinNoiseFractalNoise

		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreatePerlinNoiseImprovedNoise

		public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_perlin_noise_improved_noise (baseFrequencyX, baseFrequencyY, numOctaves, z));

		// CreatePerlinNoiseTurbulence

		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject<SKShader> (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreateBlend

		public static SKShader CreateBlend (SKShader shaderA, SKShader shaderB, SKBlendMode mode = SKBlendMode.SrcOver)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));

			return GetObject<SKShader> (SkiaApi.sk_shader_new_blend (mode, shaderA.Handle, shaderB.Handle, null));
		}

		public static SKShader CreateBlend (SKShader shaderA, SKShader shaderB, SKBlendMode mode, in SKMatrix localMatrix)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));

			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_blend (mode, shaderA.Handle, shaderB.Handle, m));
			}
		}
	}
}
