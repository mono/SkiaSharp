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

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKShader CreateEmpty ()
		{
			return GetObject<SKShader> (SkiaApi.sk_shader_new_empty ());
		}

		public static SKShader CreateColor (SKColor color)
		{
			return GetObject<SKShader> (SkiaApi.sk_shader_new_color ((uint)color));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_bitmap (src.Handle, tmx, tmy, null));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_bitmap (src.Handle, tmx, tmy, &localMatrix));
		}

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_picture (src.Handle, tmx, tmy, null, null));
		}

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_picture (src.Handle, tmx, tmy, null, &tile));
		}

		public static SKShader CreatePicture (SKPicture src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_picture (src.Handle, tmx, tmy, &localMatrix, &tile));
		}

		public static SKShader CreateColorFilter (SKShader shader, SKColorFilter filter)
		{
			if (shader == null)
				throw new ArgumentNullException (nameof (shader));
			if (filter == null)
				throw new ArgumentNullException (nameof (filter));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_color_filter (shader.Handle, filter.Handle));
		}

		public static SKShader CreateLocalMatrix (SKShader shader, SKMatrix localMatrix)
		{
			if (shader == null)
				throw new ArgumentNullException (nameof (shader));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_local_matrix (shader.Handle, &localMatrix));
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor[] colors, SKShaderTileMode mode) =>
			CreateLinearGradient (start, end, colors, null, mode);

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = new SKPoint[] { start, end };
			fixed (SKPoint* p = points)
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (p, (uint*)c, cp, colors.Length, mode, null));
			}
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			var points = new SKPoint[] { start, end };
			fixed (SKPoint* p = points)
			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (p, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateRadialGradient (center, radius, colors, null, mode);

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos!=null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, null));
			}
		}
		
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (&center, radius, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors) =>
			CreateSweepGradient (center, colors, null);

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos)
		{
			return CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360);
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKMatrix localMatrix)
		{
			return CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor[] colors, SKShaderTileMode tileMode, float startAngle, float endAngle) =>
			CreateSweepGradient (center, colors, null, tileMode, startAngle, endAngle);

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (&center, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, null));
			}
		}
		
		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (&center, (uint*)c, cp, colors.Length, tileMode, startAngle, endAngle, &localMatrix));
			}
		}

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor[] colors, SKShaderTileMode mode) =>
			CreateTwoPointConicalGradient (start, startRadius, end, endRadius, colors, null, mode);

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, null));
			}
		}
		
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos != null && colors.Length != colorPos.Length)
				throw new ArgumentException ("The number of colors must match the number of color positions.");

			fixed (SKColor* c = colors)
			fixed (float* cp = colorPos) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (&start, startRadius, &end, endRadius, (uint*)c, cp, colors.Length, mode, &localMatrix));
			}
		}
		
		public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, null));
		}
		
		public static SKShader CreatePerlinNoiseImprovedNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float z)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_improved_noise(baseFrequencyX, baseFrequencyY, numOctaves, z));
		}

		public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
		{
			var size = (SKSizeI)tileSize;
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, &size));
		}

		public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, null));
		}

		public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
		{
			var size = (SKSizeI)tileSize;
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, &size));
		}

		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_compose (shaderA.Handle, shaderB.Handle));
		}

		public static SKShader CreateCompose (SKShader shaderA, SKShader shaderB, SKBlendMode mode)
		{
			if (shaderA == null)
				throw new ArgumentNullException (nameof (shaderA));
			if (shaderB == null)
				throw new ArgumentNullException (nameof (shaderB));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_compose_with_mode (shaderA.Handle, shaderB.Handle, mode));
		}
	}
}

