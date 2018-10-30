using System;

namespace SkiaSharp
{
	public class SKShader : SKObject
	{
		[Preserve]
		internal SKShader (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_shader_unref (Handle);
			}

			base.Dispose (disposing);
		}

		public static SKShader CreateEmpty ()
		{
			return GetObject<SKShader> (SkiaApi.sk_shader_new_empty ());
		}

		public static SKShader CreateColor (SKColor color)
		{
			return GetObject<SKShader> (SkiaApi.sk_shader_new_color (color));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_bitmap (src.Handle, tmx, tmy, IntPtr.Zero));
		}

		public static SKShader CreateBitmap (SKBitmap src, SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			return GetObject<SKShader> (SkiaApi.sk_shader_new_bitmap (src.Handle, tmx, tmy, ref localMatrix));
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
			return GetObject<SKShader> (SkiaApi.sk_shader_new_local_matrix (shader.Handle, ref localMatrix));
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (new SKPoint [] { start, end }, colors, IntPtr.Zero, colors.Length, mode, IntPtr.Zero));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (new SKPoint [] { start, end }, colors, colorPos, colors.Length, mode, IntPtr.Zero));
			}
		}

		public static SKShader CreateLinearGradient (SKPoint start, SKPoint end, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (new SKPoint [] { start, end }, colors, IntPtr.Zero, colors.Length, mode, ref localMatrix));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_linear_gradient (new SKPoint [] { start, end }, colors, colorPos, colors.Length, mode, ref localMatrix));
			}
		}

		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, IntPtr.Zero, colors.Length, mode, IntPtr.Zero));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, colorPos, colors.Length, mode, IntPtr.Zero));
			}
		}
		
		public static SKShader CreateRadialGradient (SKPoint center, float radius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, IntPtr.Zero, colors.Length, mode, ref localMatrix));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_radial_gradient (ref center, radius, colors, colorPos, colors.Length, mode, ref localMatrix));
			}
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos)
		{
			return CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360);
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKMatrix localMatrix)
		{
			return CreateSweepGradient (center, colors, colorPos, SKShaderTileMode.Clamp, 0, 360, localMatrix);
		}

		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, IntPtr.Zero, colors.Length, tileMode, startAngle, endAngle, IntPtr.Zero));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, colorPos, colors.Length, tileMode, startAngle, endAngle, IntPtr.Zero));
			}
		}
		
		public static SKShader CreateSweepGradient (SKPoint center, SKColor [] colors, float [] colorPos, SKShaderTileMode tileMode, float startAngle, float endAngle, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, IntPtr.Zero, colors.Length, tileMode, startAngle, endAngle, ref localMatrix));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_sweep_gradient (ref center, colors, colorPos, colors.Length, tileMode, startAngle, endAngle, ref localMatrix));
			}
		}

		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, IntPtr.Zero, colors.Length, mode, IntPtr.Zero));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, colorPos, colors.Length, mode, IntPtr.Zero));
			}
		}
		
		public static SKShader CreateTwoPointConicalGradient (SKPoint start, float startRadius, SKPoint end, float endRadius, SKColor [] colors, float [] colorPos, SKShaderTileMode mode, SKMatrix localMatrix)
		{
			if (colors == null)
				throw new ArgumentNullException (nameof (colors));
			if (colorPos == null) {
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, IntPtr.Zero, colors.Length, mode, ref localMatrix));
			} else {
				if (colors.Length != colorPos.Length)
					throw new ArgumentException ("The number of colors must match the number of color positions.");
				return GetObject<SKShader> (SkiaApi.sk_shader_new_two_point_conical_gradient (ref start, startRadius, ref end, endRadius, colors, colorPos, colors.Length, mode, ref localMatrix));
			}
		}
		
		public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, IntPtr.Zero));
		}

		public static SKShader CreatePerlinNoiseFractalNoise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_fractal_noise(baseFrequencyX, baseFrequencyY, numOctaves, seed, ref tileSize));
		}

		public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, IntPtr.Zero));
		}

		public static SKShader CreatePerlinNoiseTurbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, SKPointI tileSize)
		{
			return GetObject<SKShader>(SkiaApi.sk_shader_new_perlin_noise_turbulence(baseFrequencyX, baseFrequencyY, numOctaves, seed, ref tileSize));
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

