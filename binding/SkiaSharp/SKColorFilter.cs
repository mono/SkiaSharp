#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Color filters for use with the <see cref="SKPaint.ColorFilter" /> property of a <see cref="SKPaint" />.
	/// </summary>
	public unsafe class SKColorFilter : SKObject, ISKReferenceCounted
	{
		/// <summary>
		/// The size of the color matrix.
		/// </summary>
		public const int ColorMatrixSize = 20;
		/// <summary>
		/// The size of a color table for a color component.
		/// </summary>
		public const int TableMaxLength = 256;

		private static readonly SKColorFilter srgbToLinear;
		private static readonly SKColorFilter linearToSrgb;

		static SKColorFilter ()
		{
			// TODO: This is not the best way to do this as it will create a lot of objects that
			//       might not be needed, but it is the only way to ensure that the static
			//       instances are created before any access is made to them.
			//       See more info: SKObject.EnsureStaticInstanceAreInitialized()

			srgbToLinear = new SKColorFilterStatic (SkiaApi.sk_colorfilter_new_srgb_to_linear_gamma ());
			linearToSrgb = new SKColorFilterStatic (SkiaApi.sk_colorfilter_new_linear_to_srgb_gamma ());
		}

		internal static void EnsureStaticInstanceAreInitialized ()
		{
			// IMPORTANT: do not remove to ensure that the static instances
			//            are initialized before any access is made to them
		}

		internal SKColorFilter(IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public static SKColorFilter CreateSrgbToLinearGamma() => srgbToLinear;

		public static SKColorFilter CreateLinearToSrgbGamma() => linearToSrgb;

		/// <summary>
		/// Creates a new color filter that uses the specified color and mode.
		/// </summary>
		/// <param name="c">The source color used with the specified mode.</param>
		/// <param name="mode">The blend mode mode that is applied to each color.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />, or <see langword="null" /> if the mode will have no effect.</returns>
		/// <remarks>
		/// If the <paramref name="mode" /> is <see cref="SKBlendMode.Dst" />, this function will return <see langword="null" /> (since that mode will have no effect on the result).
		/// </remarks>
		public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_mode((uint)c, mode));
		}

		/// <summary>
		/// Creates a new lighting color filter that multiplies the RGB channels by one color, and then adds a second color, pinning the result for each component to [0..255].
		/// </summary>
		/// <param name="mul">The color to multiply the source color by. The alpha component is ignored.</param>
		/// <param name="add">The color to add to the source color. The alpha component is ignored.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_lighting((uint)mul, (uint)add));
		}

		/// <summary>
		/// Creates a new composition color filter, whose effect is to first apply the inner filter and then apply the outer filter to the result of the inner.
		/// </summary>
		/// <param name="outer">The outer (second) filter to apply.</param>
		/// <param name="inner">The inner (first) filter to apply.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject (SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
		}

		public static SKColorFilter CreateLerp(float weight, SKColorFilter filter0, SKColorFilter filter1)
		{
			_ = filter0 ?? throw new ArgumentNullException(nameof(filter0));
			_ = filter1 ?? throw new ArgumentNullException(nameof(filter1));

			return GetObject (SkiaApi.sk_colorfilter_new_lerp(weight, filter0.Handle, filter1.Handle));
		}

		/// <summary>
		/// Creates a new color filter that transforms a color by a 4x5 (row-major) matrix.
		/// </summary>
		/// <param name="matrix">An array of <see cref="SKColorFilter.ColorMatrixSize" /> elements.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		/// <remarks>
		/// The matrix is in row-major order and the translation column is specified in unnormalized, 0...255, space.
		/// </remarks>
		public static SKColorFilter CreateColorMatrix(float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));
			return CreateColorMatrix(matrix.AsSpan());
		}

		public static SKColorFilter CreateColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_color_matrix (m));
			}
		}

		public static SKColorFilter CreateHslaColorMatrix(ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			fixed (float* m = matrix) {
				return GetObject (SkiaApi.sk_colorfilter_new_hsla_matrix (m));
			}
		}

		/// <summary>
		/// Creates a new luminance-to-alpha color filter.
		/// </summary>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		public static SKColorFilter CreateLumaColor()
		{
			return GetObject (SkiaApi.sk_colorfilter_new_luma_color());
		}

		/// <summary>
		/// Creates a new table color filter.
		/// </summary>
		/// <param name="table">The table of values for each color component, with a length of <see cref="SKColorTable.MaxLength" />.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			return CreateTable(table.AsSpan());
		}

		public static SKColorFilter CreateTable(ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException($"Table must have a length of {TableMaxLength}.", nameof(table));
			fixed (byte* t = table) {
				return GetObject (SkiaApi.sk_colorfilter_new_table (t));
			}
		}

		/// <summary>
		/// Creates a new table color filter.
		/// </summary>
		/// <param name="tableA">The table of values for the alpha component, with a length of <see cref="SKColorTable.MaxLength" />.</param>
		/// <param name="tableR">The table of values for the red component, with a length of <see cref="SKColorTable.MaxLength" />.</param>
		/// <param name="tableG">The table of values for the green component, with a length of <see cref="SKColorTable.MaxLength" />.</param>
		/// <param name="tableB">The table of values for the blue component, with a length of <see cref="SKColorTable.MaxLength" />.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
		{
			if (tableA == null)
				throw new ArgumentNullException(nameof(tableA));
			if (tableR == null)
				throw new ArgumentNullException(nameof(tableR));
			if (tableG == null)
				throw new ArgumentNullException(nameof(tableG));
			if (tableB == null)
				throw new ArgumentNullException(nameof(tableB));
			return CreateTable(tableA.AsSpan(), tableR.AsSpan(), tableG.AsSpan(), tableB.AsSpan());
		}

		public static SKColorFilter CreateTable(ReadOnlySpan<byte> tableA, ReadOnlySpan<byte> tableR, ReadOnlySpan<byte> tableG, ReadOnlySpan<byte> tableB)
		{
			if (tableA.Length != TableMaxLength)
				throw new ArgumentException($"Table A must have a length of {TableMaxLength}.", nameof(tableA));
			if (tableR.Length != TableMaxLength)
				throw new ArgumentException($"Table R must have a length of {TableMaxLength}.", nameof(tableR));
			if (tableG.Length != TableMaxLength)
				throw new ArgumentException($"Table G must have a length of {TableMaxLength}.", nameof(tableG));
			if (tableB.Length != TableMaxLength)
				throw new ArgumentException($"Table B must have a length of {TableMaxLength}.", nameof(tableB));

			fixed (byte* a = tableA)
			fixed (byte* r = tableR)
			fixed (byte* g = tableG)
			fixed (byte* b = tableB) {
				return GetObject (SkiaApi.sk_colorfilter_new_table_argb (a, r, g, b));
			}
		}

		/// <summary>
		/// Creates a new high contrast color filter which provides transformations to improve contrast for users with low vision.
		/// </summary>
		/// <param name="config">The high contrast configuration settings.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		/// <remarks>
		/// Applies the following transformations in this order: conversion to grayscale, color inversion, increasing the resulting contrast.
		/// </remarks>
		public static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
		{
			return GetObject (SkiaApi.sk_colorfilter_new_high_contrast(&config));
		}

		/// <summary>
		/// Creates a new high contrast color filter which provides transformations to improve contrast for users with low vision.
		/// </summary>
		/// <param name="grayscale">Whether or not the color will be converted to grayscale.</param>
		/// <param name="invertStyle">Whether or not to invert brightness, lightness, or neither.</param>
		/// <param name="contrast">The amount to adjust the contrast by, in the range -1.0 through 1.0.</param>
		/// <returns>Returns the new <see cref="SKColorFilter" />.</returns>
		/// <remarks>
		/// Applies the following transformations in this order: conversion to grayscale, color inversion, increasing the resulting contrast.
		/// </remarks>
		public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
		}

		internal static SKColorFilter GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKColorFilter (h, o));
			
		private sealed class SKColorFilterStatic : SKColorFilter
		{
			internal SKColorFilterStatic (IntPtr x)
				: base (x, false)
			{
			}

			protected override void Dispose (bool disposing) { }
		}
	}
}
