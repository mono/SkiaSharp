//
// Bindings for SKColorFilter
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKColorFilter : SKObject
	{
		[Obsolete("Use MinColorCubeDimension instead.")]
		public const int MinCubeSize = 4;
		[Obsolete("Use MaxColorCubeDimension instead.")]
		public const int MaxCubeSize = 64;

		public const int MinColorCubeDimension = 4;
		public const int MaxColorCubeDimension = 64;
		public const int ColorMatrixSize = 20;

		public static bool IsValid3DColorCube(SKData cubeData, int cubeDimension)
		{
			var minMemorySize = 4 * cubeDimension * cubeDimension * cubeDimension;
			return
				(cubeDimension >= MinColorCubeDimension) && (cubeDimension <= MaxColorCubeDimension) &&
				(null != cubeData) && (cubeData.Size >= minMemorySize);
		}

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
		
		public static SKColorFilter CreateXferMode(SKColor c, SKXferMode mode)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_mode(c, mode));
		}

		public static SKColorFilter CreateBlendMode(SKColor c, SKBlendMode mode)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_mode(c, (SKXferMode)mode));
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

		public static SKColorFilter CreateColorCube(byte[] cubeData, int cubeDimension)
		{
			if (cubeData == null)
				throw new ArgumentNullException(nameof(cubeData));
			return CreateColorCube(new SKData(cubeData), cubeDimension);
		}

		public static SKColorFilter CreateColorCube(SKData cubeData, int cubeDimension)
		{
			if (cubeData == null)
				throw new ArgumentNullException(nameof(cubeData));
			if (!IsValid3DColorCube(cubeData, cubeDimension))
				throw new ArgumentException("Invalid cube data.", nameof(cubeData));
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_color_cube(cubeData.Handle, cubeDimension));
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

		public static SKColorFilter CreateGamma(float gamma)
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_gamma(gamma));
		}

		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException(nameof(table));
			if (table.Length != SKColorTable.MaxLength)
				throw new ArgumentException($"Table must have a length of {SKColorTable.MaxLength}.", nameof(table));
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table(table));
		}

		public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
		{
			if (tableA != null && tableA.Length != SKColorTable.MaxLength)
				throw new ArgumentException($"Table A must have a length of {SKColorTable.MaxLength}.", nameof(tableA));
			if (tableR != null && tableR.Length != SKColorTable.MaxLength)
				throw new ArgumentException($"Table R must have a length of {SKColorTable.MaxLength}.", nameof(tableR));
			if (tableG != null && tableG.Length != SKColorTable.MaxLength)
				throw new ArgumentException($"Table G must have a length of {SKColorTable.MaxLength}.", nameof(tableG));
			if (tableB != null && tableB.Length != SKColorTable.MaxLength)
				throw new ArgumentException($"Table B must have a length of {SKColorTable.MaxLength}.", nameof(tableB));

			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table_argb(tableA, tableR, tableG, tableB));
		}
	}
}
