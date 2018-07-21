using System;

namespace SkiaSharp
{
	// TODO: `FilterColor` may be useful

	public class SKColorFilter : SKObject
	{
		public const int ColorMatrixSize = 20;
		public const int TableMaxLength = 256;

		[Preserve]
		internal SKColorFilter(IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}
		
		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle)
			{
				SkiaApi.sk_colorfilter_unref(Handle);
			}

			base.Dispose(disposing);
		}

		public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_mode(c, mode));
		}

		public static SKColorFilter CreateLighting(SKColor mul, SKColor add)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_lighting(mul, add));
		}

		public static SKColorFilter CreateCompose(SKColorFilter outer, SKColorFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
		}

		public static SKColorFilter CreateColorMatrix(float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException(nameof(matrix));
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", nameof(matrix));
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_color_matrix(matrix));
		}

		public static SKColorFilter CreateLumaColor()
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_luma_color());
		}

		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			if (table.Length != TableMaxLength)
				throw new ArgumentException($"Table must have a length of {TableMaxLength}.", nameof(table));
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table(table));
		}

		public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
		{
			if (tableA != null && tableA.Length != TableMaxLength)
				throw new ArgumentException($"Table A must have a length of {TableMaxLength}.", nameof(tableA));
			if (tableR != null && tableR.Length != TableMaxLength)
				throw new ArgumentException($"Table R must have a length of {TableMaxLength}.", nameof(tableR));
			if (tableG != null && tableG.Length != TableMaxLength)
				throw new ArgumentException($"Table G must have a length of {TableMaxLength}.", nameof(tableG));
			if (tableB != null && tableB.Length != TableMaxLength)
				throw new ArgumentException($"Table B must have a length of {TableMaxLength}.", nameof(tableB));

			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table_argb(tableA, tableR, tableG, tableB));
		}

		public static SKColorFilter CreateHighContrast(SKHighContrastConfig config)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_high_contrast(ref config));
		}

		public static SKColorFilter CreateHighContrast(bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast)
		{
			return CreateHighContrast(new SKHighContrastConfig(grayscale, invertStyle, contrast));
		}
	}
}
