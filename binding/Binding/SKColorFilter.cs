using System;

namespace SkiaSharp
{
	// TODO: `FilterColor` may be useful

	public unsafe class SKColorFilter : SKObject, ISKReferenceCounted
	{
		public const int ColorMatrixSize = 20;
		public const int TableMaxLength = 256;

		[Preserve]
		internal SKColorFilter (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// CreateBlendMode

		public static SKColorFilter CreateBlendMode (SKColor c, SKBlendMode mode) =>
			GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_mode ((uint)c, mode));

		// CreateLighting

		public static SKColorFilter CreateLighting (SKColor mul, SKColor add) =>
			GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_lighting ((uint)mul, (uint)add));

		// CreateCompose

		public static SKColorFilter CreateCompose (SKColorFilter outer, SKColorFilter inner)
		{
			if (outer == null)
				throw new ArgumentNullException (nameof (outer));
			if (inner == null)
				throw new ArgumentNullException (nameof (inner));

			return GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_compose (outer.Handle, inner.Handle));
		}

		// CreateColorMatrix

		public static SKColorFilter CreateColorMatrix (ReadOnlySpan<float> matrix)
		{
			if (matrix.Length != 20)
				throw new ArgumentException ("Matrix must have a length of 20.", nameof (matrix));

			fixed (float* m = matrix) {
				return GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_color_matrix (m));
			}
		}

		// CreateLumaColor

		public static SKColorFilter CreateLumaColor () =>
			GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_luma_color ());

		// CreateTable

		public static SKColorFilter CreateTable (byte[] table) =>
			CreateTable (table.AsSpan ());

		public static SKColorFilter CreateTable (byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB) =>
			CreateTable (tableA.AsSpan (), tableR.AsSpan (), tableG.AsSpan (), tableB.AsSpan ());

		public static SKColorFilter CreateTable (ReadOnlySpan<byte> table)
		{
			if (table.Length != TableMaxLength)
				throw new ArgumentException ($"Table must have a length of {TableMaxLength}.", nameof (table));

			fixed (byte* t = table) {
				return GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_table (t));
			}
		}

		public static SKColorFilter CreateTable (ReadOnlySpan<byte> tableA, ReadOnlySpan<byte> tableR, ReadOnlySpan<byte> tableG, ReadOnlySpan<byte> tableB)
		{
			if (tableA.Length != TableMaxLength)
				throw new ArgumentException ($"Table A must have a length of {TableMaxLength}.", nameof (tableA));
			if (tableR.Length != TableMaxLength)
				throw new ArgumentException ($"Table R must have a length of {TableMaxLength}.", nameof (tableR));
			if (tableG.Length != TableMaxLength)
				throw new ArgumentException ($"Table G must have a length of {TableMaxLength}.", nameof (tableG));
			if (tableB.Length != TableMaxLength)
				throw new ArgumentException ($"Table B must have a length of {TableMaxLength}.", nameof (tableB));

			fixed (byte* a = tableA)
			fixed (byte* r = tableR)
			fixed (byte* g = tableG)
			fixed (byte* b = tableB) {
				return GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_table_argb (a, r, g, b));
			}
		}

		// CreateHighContrast

		public static SKColorFilter CreateHighContrast (SKHighContrastConfig config) =>
			GetObject<SKColorFilter> (SkiaApi.sk_colorfilter_new_high_contrast (&config));

		public static SKColorFilter CreateHighContrast (bool grayscale, SKHighContrastConfigInvertStyle invertStyle, float contrast) =>
			CreateHighContrast (new SKHighContrastConfig (grayscale, invertStyle, contrast));
	}
}
