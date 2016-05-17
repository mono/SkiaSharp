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
		public const int MinCubeSize = 4;
		public const int MaxCubeSize = 64;

		public static bool IsValid3DColorCube(SKData cubeData, int cubeDimension)
		{
			var minMemorySize = 4 * cubeDimension * cubeDimension * cubeDimension;
			return
				(cubeDimension >= MinCubeSize) && (cubeDimension <= MaxCubeSize) &&
				(null != cubeData) && (cubeData.Size >= minMemorySize);
		}

		[Preserve]
		internal SKColorFilter(IntPtr handle)
			: base (handle)
		{
		}
		
		protected override void Dispose(bool disposing)
		{
			if (Handle != IntPtr.Zero)
			{
				SkiaApi.sk_colorfilter_unref(Handle);
			}

			base.Dispose(disposing);
		}
		
		public static SKColorFilter CreateXferMode(SKColor c, SKXferMode mode)
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
				throw new ArgumentNullException("outer");
			if (inner == null)
				throw new ArgumentNullException("inner");
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_compose(outer.Handle, inner.Handle));
		}

		public static SKColorFilter CreateColorCube(byte[] cubeData, int cubeDimension)
		{
			return CreateColorCube(new SKData(cubeData), cubeDimension);
		}

		public static SKColorFilter CreateColorCube(SKData cubeData, int cubeDimension)
		{
			if (!IsValid3DColorCube(cubeData, cubeDimension))
				throw new ArgumentNullException("cubeData");
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_color_cube(cubeData.Handle, cubeDimension));
		}

		public static SKColorFilter CreateColorMatrix(float[] matrix)
		{
			if (matrix == null)
				throw new ArgumentNullException("matrix");
			if (matrix.Length != 20)
				throw new ArgumentException("Matrix must have a length of 20.", "matrix");
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_color_matrix(matrix));
		}

		public static SKColorFilter CreateLumaColor()
		{
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_luma_color());
		}

		public static SKColorFilter CreateTable(byte[] table)
		{
			if (table == null)
				throw new ArgumentNullException("table");
			if (table.Length != 256)
				throw new ArgumentException("Table must have a length of 256.", "table");
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table(table));
		}

		public static SKColorFilter CreateTable(byte[] tableA, byte[] tableR, byte[] tableG, byte[] tableB)
		{
			if (tableA != null && tableA.Length != 256)
				throw new ArgumentException("Table A must have a length of 256.", "tableA");
			if (tableR != null && tableR.Length != 256)
				throw new ArgumentException("Table R must have a length of 256.", "tableR");
			if (tableG != null && tableG.Length != 256)
				throw new ArgumentException("Table G must have a length of 256.", "tableG");
			if (tableB != null && tableB.Length != 256)
				throw new ArgumentException("Table B must have a length of 256.", "tableB");
			return GetObject<SKColorFilter>(SkiaApi.sk_colorfilter_new_table_argb(tableA, tableR, tableG, tableB));
		}
	}
}
