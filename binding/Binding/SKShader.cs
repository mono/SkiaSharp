using System;

namespace SkiaSharp
{
	public unsafe class SKShader : SKObject, ISKReferenceCounted
	{
		internal SKShader (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		// WithColorFilter

		public SKShader WithColorFilter (SKColorFilter filter)
		{
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return GetObject (SkiaApi.sk_shader_with_color_filter (Handle, filter.Handle));
		}

		// WithLocalMatrix

		public SKShader WithLocalMatrix (SKMatrix localMatrix) =>
			GetObject (SkiaApi.sk_shader_with_local_matrix (Handle, &localMatrix));

		// CreateEmpty

		public static SKShader CreateEmpty () =>
			GetObject (SkiaApi.sk_shader_new_empty ());

		// CreateColor

		public static SKShader CreateColor (SKColor color) =>
			GetObject (SkiaApi.sk_shader_new_color ((uint)color));

		public static SKShader CreateColor (SKColorF color, SKColorSpace colorspace)
		{
			if (colorspace == null)
				throw new ArgumentNullException (nameof (colorspace));

			return GetObject (SkiaApi.sk_shader_new_color4f (&color, colorspace.Handle));
		}

		// CreateBitmap

		public static SKShader CreateBitmap (SKBitmap src) =>
			CreateBitmap (src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix);
		}

		// CreateImage

		public static SKShader CreateImage (SKImage src) =>
			CreateImage (src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy);
		}

		public static SKShader CreateImage (SKImage src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix);
		}

		// CreatePicture

		public static SKShader CreatePicture (SKPicture src) =>
			CreatePicture (src, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy)
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

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return src.ToShader (tmx, tmy, localMatrix, tile);
		}

		// CreateLinearGradient

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, null, mode);

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

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, colorspace, null, mode);

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

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, null, mode);

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

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, colorspace, null, mode);

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

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors) =>
			CreateSweepGradient (center, colors, null, SKShaderTileMode.Clamp, 0, 360);

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos) =>
			CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360);

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, float[] colorPos, SKMatrix localMatrix) =>
			CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, SKShaderTileMode tileMode, float startAngle, float endAngle) =>
			CreateSweepGradient (center, colors, null, tileMode, startAngle, endAngle);

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

		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace) =>
			CreateSweepGradient (center, colors, colorspace, null, SKShaderTileMode.Clamp, 0, 360);

		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos) =>
			CreateSweepGradient (center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0, 360);

		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, float[] colorPos, SKMatrix localMatrix) =>
			CreateSweepGradient (center, colors, colorspace, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);

		public static SKShader CreateSweepGradient (SKPoint center, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode tileMode, float startAngle, float endAngle) =>
			CreateSweepGradient (center, colors, colorspace, null, tileMode, startAngle, endAngle);

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

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, null, mode);

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

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColorF[] colors, SKColorSpace colorspace, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, colorspace, null, mode);

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

		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize) =>
			CreatePerlinNoiseFractalNoise (baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);

		public static SKShader CreatePerlinNoiseFractalNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_fractal_noise (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreatePerlinNoiseImprovedNoise

		public static SKShader CreatePerlinNoiseImprovedNoise (float baseFrequencyX, float baseFrequencyY, int numOctaves, float z) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_improved_noise (baseFrequencyX, baseFrequencyY, numOctaves, z));

		// CreatePerlinNoiseTurbulence

		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, null));

		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize) =>
			CreatePerlinNoiseTurbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, (SKSizeI)tileSize);

		public static SKShader CreatePerlinNoiseTurbulence (float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKSizeI tileSize) =>
			GetObject (SkiaApi.sk_shader_new_perlin_noise_turbulence (baseFrequencyX, baseFrequencyY, numOctaves, seed, &tileSize));

		// CreateCompose

		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB) =>
			CreateCompose (shaderA, shaderB, SKBlendMode.SrcOver);

		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKBlendMode mode)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));

			return GetObject (SkiaApi.sk_shader_new_blend (mode, shaderA.Handle, shaderB.Handle, null));
		}

		// CreateLerp

		public static SKShader CreateLerp (float weight, SKShader dst, SKShader src)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return GetObject (SkiaApi.sk_shader_new_lerp (weight, dst.Handle, src.Handle, null));
		}

		// CreateColorFilter

		public static SKShader CreateColorFilter (SKShader shader, SKColorFilter filter)
		{
			if (shader == null)
				throw new ArgumentNullException (nameof (shader));
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));

			return shader.WithColorFilter (filter);
		}

		// CreateLocalMatrix

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
