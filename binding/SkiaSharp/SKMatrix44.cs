#nullable disable

using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace SkiaSharp
{
	/// <summary>A 3D, 4x4 matrix.</summary>
	/// <remarks />
	public unsafe partial struct SKMatrix44
	{
		internal const float DegreesToRadians = (float)Math.PI / 180.0f;

		/// <summary>Represents an empty matrix with all values set to zero.</summary>
		/// <remarks />
		public readonly static SKMatrix44 Empty;

		/// <summary>Represents the identity matrix.</summary>
		/// <remarks />
		public readonly static SKMatrix44 Identity = Matrix4x4.Identity;

		/// <summary>Creates a new, uninitialized instance of <see cref="T:SkiaSharp.SKMatrix44" />.</summary>
		/// <remarks />
		public SKMatrix44 ()
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMatrix44" /> using the values from a <see cref="T:SkiaSharp.SKMatrix" /> instance.</summary>
		/// <param name="src">The <see cref="T:SkiaSharp.SKMatrix" /> instance.</param>
		/// <remarks>When converting from <see cref="T:SkiaSharp.SKMatrix" /> to <see cref="T:SkiaSharp.SKMatrix44" />, the third row and column remain as identity.</remarks>
		public SKMatrix44 (SKMatrix src)
		{
			this = src;
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKMatrix44" /> using the values from another instance.</summary>
		/// <param name="src">The matrix to copy.</param>
		/// <remarks />
		public SKMatrix44 (SKMatrix44 src)
		{
			this = src;
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKMatrix44" /> struct with the specified values.</summary>
		/// <param name="m00">The value at row 0, column 0.</param>
		/// <param name="m01">The value at row 0, column 1.</param>
		/// <param name="m02">The value at row 0, column 2.</param>
		/// <param name="m03">The value at row 0, column 3.</param>
		/// <param name="m10">The value at row 1, column 0.</param>
		/// <param name="m11">The value at row 1, column 1.</param>
		/// <param name="m12">The value at row 1, column 2.</param>
		/// <param name="m13">The value at row 1, column 3.</param>
		/// <param name="m20">The value at row 2, column 0.</param>
		/// <param name="m21">The value at row 2, column 1.</param>
		/// <param name="m22">The value at row 2, column 2.</param>
		/// <param name="m23">The value at row 2, column 3.</param>
		/// <param name="m30">The value at row 3, column 0.</param>
		/// <param name="m31">The value at row 3, column 1.</param>
		/// <param name="m32">The value at row 3, column 2.</param>
		/// <param name="m33">The value at row 3, column 3.</param>
		/// <remarks />
		public SKMatrix44 (
			float m00, float m01, float m02, float m03,
			float m10, float m11, float m12, float m13,
			float m20, float m21, float m22, float m23,
			float m30, float m31, float m32, float m33)
		{
			this.m00 = m00;
			this.m01 = m01;
			this.m02 = m02;
			this.m03 = m03;

			this.m10 = m10;
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;

			this.m20 = m20;
			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;

			this.m30 = m30;
			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
		}

		// Create*

		/// <summary>Creates a new identity matrix.</summary>
		/// <returns>Returns the new identity matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateIdentity () => Identity;

		/// <summary>Creates a new 3D translation matrix.</summary>
		/// <param name="x">The amount, along the x-axis, to translate.</param>
		/// <param name="y">The amount, along the y-axis, to translate.</param>
		/// <param name="z">The amount, along the z-axis, to translate.</param>
		/// <returns>Returns the new translation matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateTranslation (float x, float y, float z) =>
			Matrix4x4.CreateTranslation (x, y, z);

		/// <summary>Creates a new scale matrix.</summary>
		/// <param name="x">The amount, along the x-axis, to scale.</param>
		/// <param name="y">The amount, along the y-axis, to scale.</param>
		/// <param name="z">The amount, along the z-axis, to scale.</param>
		/// <returns>Returns the new scale matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateScale (float x, float y, float z) =>
			Matrix4x4.CreateScale (x, y, z);

		/// <summary>Creates a 3D scaling matrix with a pivot point.</summary>
		/// <param name="x">The amount, along the x-axis, to scale.</param>
		/// <param name="y">The amount, along the y-axis, to scale.</param>
		/// <param name="z">The amount, along the z-axis, to scale.</param>
		/// <param name="pivotX">The x-coordinate of the pivot point.</param>
		/// <param name="pivotY">The y-coordinate of the pivot point.</param>
		/// <param name="pivotZ">The z-coordinate of the pivot point.</param>
		/// <returns>Returns the new scale matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateScale (float x, float y, float z, float pivotX, float pivotY, float pivotZ) =>
			Matrix4x4.CreateScale (x, y, z, new Vector3 (pivotX, pivotY, pivotZ));

		// CreateRotation*

		/// <summary>Creates a new rotation matrix.</summary>
		/// <param name="x">The x-axis to rotate around.</param>
		/// <param name="y">The y-axis to rotate around.</param>
		/// <param name="z">The z-axis to rotate around.</param>
		/// <param name="radians">The amount, in radians, to rotate by.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateRotation (float x, float y, float z, float radians) =>
			Matrix4x4.CreateFromAxisAngle (new Vector3 (x, y, z), radians);

		//public static SKMatrix44 CreateRotationX (float radians) =>
		//	Matrix4x4.CreateRotationX (radians);

		//public static SKMatrix44 CreateRotationX (float radians, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationX (radians, new Vector3 (pivotX, pivotY, pivotZ));

		//public static SKMatrix44 CreateRotationY (float radians) =>
		//	Matrix4x4.CreateRotationY (radians);

		//public static SKMatrix44 CreateRotationY (float radians, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationY (radians, new Vector3 (pivotX, pivotY, pivotZ));

		//public static SKMatrix44 CreateRotationZ (float radians) =>
		//	Matrix4x4.CreateRotationZ (radians);

		//public static SKMatrix44 CreateRotationZ (float radians, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationZ (radians, new Vector3 (pivotX, pivotY, pivotZ));

		// CreateRotation*Degrees

		/// <summary>Creates a new rotation matrix.</summary>
		/// <param name="x">The x-axis to rotate around.</param>
		/// <param name="y">The y-axis to rotate around.</param>
		/// <param name="z">The z-axis to rotate around.</param>
		/// <param name="degrees">The amount, in degrees, to rotate by.</param>
		/// <returns>Returns the new rotation matrix.</returns>
		/// <remarks />
		public static SKMatrix44 CreateRotationDegrees (float x, float y, float z, float degrees) =>
			Matrix4x4.CreateFromAxisAngle (new Vector3 (x, y, z), degrees * DegreesToRadians);

		//public static SKMatrix44 CreateRotationXDegrees (float degrees) =>
		//	Matrix4x4.CreateRotationX (degrees * DegreesToRadians);

		//public static SKMatrix44 CreateRotationXDegrees (float degrees, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationX (degrees * DegreesToRadians, new Vector3 (pivotX, pivotY, pivotZ));

		//public static SKMatrix44 CreateRotationYDegrees (float degrees) =>
		//	Matrix4x4.CreateRotationY (degrees * DegreesToRadians);

		//public static SKMatrix44 CreateRotationYDegrees (float degrees, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationY (degrees * DegreesToRadians, new Vector3 (pivotX, pivotY, pivotZ));

		//public static SKMatrix44 CreateRotationZDegrees (float degrees) =>
		//	Matrix4x4.CreateRotationZ (degrees * DegreesToRadians);

		//public static SKMatrix44 CreateRotationZDegrees (float degrees, float pivotX, float pivotY, float pivotZ) =>
		//	Matrix4x4.CreateRotationZ (degrees * DegreesToRadians, new Vector3 (pivotX, pivotY, pivotZ));

		// From*

		/// <summary>Creates a matrix from values in row-major order.</summary>
		/// <param name="src">A span containing 16 values in row-major order.</param>
		/// <returns>Returns the new matrix.</returns>
		/// <remarks />
		public static SKMatrix44 FromRowMajor (ReadOnlySpan<float> src)
		{
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			return new SKMatrix44 (
				src[0], src[1], src[2], src[3],
				src[4], src[5], src[6], src[7],
				src[8], src[9], src[10], src[11],
				src[12], src[13], src[14], src[15]);
		}

		/// <summary>Creates a matrix from values in column-major order.</summary>
		/// <param name="src">A span containing 16 values in column-major order.</param>
		/// <returns>Returns the new matrix.</returns>
		/// <remarks />
		public static SKMatrix44 FromColumnMajor (ReadOnlySpan<float> src)
		{
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			return new SKMatrix44 (
				src[0], src[4], src[8], src[12],
				src[1], src[5], src[9], src[13],
				src[2], src[6], src[10], src[14],
				src[3], src[7], src[11], src[15]);
		}

		// To*

		/// <summary>Converts the current matrix to a row-major array.</summary>
		/// <returns>Returns the new row-major array.</returns>
		/// <remarks />
		public readonly float[] ToRowMajor ()
		{
			var dst = new float[16];
			ToRowMajor (dst);
			return dst;
		}

		/// <summary>Converts the current matrix to a column-major array.</summary>
		/// <returns>Returns the new column-major array.</returns>
		/// <remarks />
		public readonly float[] ToColumnMajor ()
		{
			var dst = new float[16];
			ToColumnMajor (dst);
			return dst;
		}

		/// <summary>Copies the matrix values to a span in row-major order.</summary>
		/// <param name="dst">A span to receive the 16 matrix values in row-major order.</param>
		/// <remarks />
		public readonly void ToRowMajor (Span<float> dst)
		{
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			dst[00] = m00;
			dst[01] = m01;
			dst[02] = m02;
			dst[03] = m03;

			dst[04] = m10;
			dst[05] = m11;
			dst[06] = m12;
			dst[07] = m13;

			dst[08] = m20;
			dst[09] = m21;
			dst[10] = m22;
			dst[11] = m23;

			dst[12] = m30;
			dst[13] = m31;
			dst[14] = m32;
			dst[15] = m33;
		}

		/// <summary>Copies the matrix values to a span in column-major order.</summary>
		/// <param name="dst">A span to receive the 16 matrix values in column-major order.</param>
		/// <remarks />
		public readonly void ToColumnMajor (Span<float> dst)
		{
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			dst[00] = m00;
			dst[01] = m10;
			dst[02] = m20;
			dst[03] = m30;

			dst[04] = m01;
			dst[05] = m11;
			dst[06] = m21;
			dst[07] = m31;

			dst[08] = m02;
			dst[09] = m12;
			dst[10] = m22;
			dst[11] = m32;

			dst[12] = m03;
			dst[13] = m13;
			dst[14] = m23;
			dst[15] = m33;
		}

		// Invert

		/// <summary>Gets whether this matrix is invertible.</summary>
		/// <value>Returns <see langword="true" /> if the matrix can be inverted, otherwise <see langword="false" />.</value>
		/// <remarks />
		public readonly bool IsInvertible =>
			Matrix4x4.Invert (this, out _);

		/// <summary>Attempts to invert the matrix.</summary>
		/// <param name="inverse">Receives the inverted matrix if successful.</param>
		/// <returns>Returns <see langword="true" /> if the matrix was successfully inverted, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public readonly bool TryInvert (out SKMatrix44 inverse)
		{
			if (Matrix4x4.Invert (this, out var inv)) {
				inverse = inv;
				return true;
			}

			inverse = Empty;
			return false;
		}

		/// <summary>Creates an inverted matrix from the current matrix.</summary>
		/// <returns>Returns the inverted matrix if it is invertible, otherwise <see langword="null" />.</returns>
		/// <remarks />
		public readonly SKMatrix44 Invert ()
		{
			if (Matrix4x4.Invert (this, out var inverted))
				return inverted;

			return Empty;
		}

		// Transpose

		/// <summary>Transposes the current matrix.</summary>
		/// <returns>Returns the transposed matrix.</returns>
		/// <remarks />
		public readonly SKMatrix44 Transpose () =>
			Matrix4x4.Transpose (this);

		// Determinant

		/// <summary>Calculates the determinant of the matrix.</summary>
		/// <returns>Returns the determinant.</returns>
		/// <remarks />
		public readonly float Determinant () =>
			((Matrix4x4)this).GetDeterminant ();

		// MapPoints

		/// <summary>Applies the matrix to a point.</summary>
		/// <param name="point">The point to transform.</param>
		/// <returns>Returns a new point with the matrix applied.</returns>
		/// <remarks />
		public readonly SKPoint MapPoint (SKPoint point) =>
			Vector2.Transform (point, this);

		/// <summary>Transforms a 3D point using this matrix.</summary>
		/// <param name="point">The 3D point to transform.</param>
		/// <returns>Returns the transformed point.</returns>
		/// <remarks />
		public readonly SKPoint3 MapPoint (SKPoint3 point) =>
			Vector3.Transform (point, this);

		/// <summary>Transforms a 2D point using this matrix.</summary>
		/// <param name="x">The x-coordinate of the point.</param>
		/// <param name="y">The y-coordinate of the point.</param>
		/// <returns>Returns the transformed point.</returns>
		/// <remarks />
		public readonly SKPoint MapPoint (float x, float y) =>
			MapPoint (new SKPoint (x, y));

		/// <summary>Transforms a 3D point using this matrix.</summary>
		/// <param name="x">The x-coordinate of the point.</param>
		/// <param name="y">The y-coordinate of the point.</param>
		/// <param name="z">The z-coordinate of the point.</param>
		/// <returns>Returns the transformed point.</returns>
		/// <remarks />
		public readonly SKPoint3 MapPoint (float x, float y, float z) =>
			MapPoint (new SKPoint3 (x, y, z));

		// MapScalars

		// TODO: create a vector type

		internal float[] MapScalars (float x, float y, float z, float w)
		{
			var t = Vector4.Transform (new Vector4 (x, y, z, w), this);
			return new[] { t.X, t.Y, t.Z, t.W };
		}

		internal float[] MapScalars (ReadOnlySpan<float> srcVector4)
		{
			if (srcVector4.Length != 4)
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));

			var vector = new Vector4 (srcVector4[0], srcVector4[1], srcVector4[2], srcVector4[3]);
			var t = Vector4.Transform (vector, this);
			return new[] { t.X, t.Y, t.Z, t.W };
		}

		internal void MapScalars (ReadOnlySpan<float> srcVector4, Span<float> dstVector4)
		{
			if (srcVector4.Length != 4)
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));
			if (dstVector4.Length != 4)
				throw new ArgumentException ("The destination vector array must be 4 entries.", nameof (dstVector4));

			var vector = new Vector4 (srcVector4[0], srcVector4[1], srcVector4[2], srcVector4[3]);
			var t = Vector4.Transform (vector, this);
			dstVector4[0] = t.X;
			dstVector4[1] = t.Y;
			dstVector4[2] = t.Z;
			dstVector4[3] = t.W;
		}

		// *Concat

		/// <summary>Concatenates two matrices and returns the result.</summary>
		/// <param name="first">The first matrix.</param>
		/// <param name="second">The second matrix.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks />
		public static SKMatrix44 Concat (SKMatrix44 first, SKMatrix44 second) =>
			first * second;

		/// <summary>Pre-concatenates the matrix with the specified matrix.</summary>
		/// <param name="matrix">The matrix to concatenate.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks />
		public readonly SKMatrix44 PreConcat (SKMatrix44 matrix) =>
			this * matrix;

		/// <summary>Post-concatenates the current matrix with the specified matrix.</summary>
		/// <param name="matrix">The matrix to concatenate.</param>
		/// <returns>Returns the concatenated matrix.</returns>
		/// <remarks />
		public readonly SKMatrix44 PostConcat (SKMatrix44 matrix) =>
			matrix * this;

		/// <summary>Concatenates two matrices into a target matrix.</summary>
		/// <param name="target">The matrix to receive the concatenated result.</param>
		/// <param name="first">The first matrix.</param>
		/// <param name="second">The second matrix.</param>
		/// <remarks />
		public static void Concat (ref SKMatrix44 target, SKMatrix44 first, SKMatrix44 second) =>
			target = first * second;

		// Operations

		/// <summary>Negates a matrix by multiplying all elements by -1.</summary>
		/// <param name="value">The matrix to negate.</param>
		/// <returns>Returns the negated matrix.</returns>
		/// <remarks />
		public static SKMatrix44 Negate (SKMatrix44 value) =>
			-value;

		/// <summary>Adds two matrices and returns the result.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		/// <returns>Returns the sum of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 Add (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 + value2;

		/// <summary>Subtracts two matrices and returns the result.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix to subtract.</param>
		/// <returns>Returns the difference of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 Subtract (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 - value2;

		/// <summary>Multiplies two matrices and returns the result.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		/// <returns>Returns the product of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 Multiply (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 * value2;

		/// <summary>Multiplies a matrix by a scalar value.</summary>
		/// <param name="value1">The matrix to scale.</param>
		/// <param name="value2">The scalar value to multiply by.</param>
		/// <returns>Returns the scaled matrix.</returns>
		/// <remarks />
		public static SKMatrix44 Multiply (SKMatrix44 value1, float value2) =>
			value1 * value2;

		// operators

		/// <summary>Negates a matrix.</summary>
		/// <param name="value">The matrix to negate.</param>
		/// <returns>Returns the negated matrix.</returns>
		/// <remarks />
		public static SKMatrix44 operator - (SKMatrix44 value) =>
			-((Matrix4x4)value);

		/// <summary>Adds two matrices.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		/// <returns>Returns the sum of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 operator + (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) + ((Matrix4x4)value2);

		/// <summary>Subtracts two matrices.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix to subtract.</param>
		/// <returns>Returns the difference of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 operator - (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) - ((Matrix4x4)value2);

		/// <summary>Multiplies two matrices.</summary>
		/// <param name="value1">The first matrix.</param>
		/// <param name="value2">The second matrix.</param>
		/// <returns>Returns the product of the two matrices.</returns>
		/// <remarks />
		public static SKMatrix44 operator * (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) * ((Matrix4x4)value2);

		/// <summary>Multiplies a matrix by a scalar value.</summary>
		/// <param name="value1">The matrix to scale.</param>
		/// <param name="value2">The scalar value.</param>
		/// <returns>Returns the scaled matrix.</returns>
		/// <remarks />
		public static SKMatrix44 operator * (SKMatrix44 value1, float value2) =>
			((Matrix4x4)value1) * value2;

		// properties

		/// <summary>Gets the <see cref="T:SkiaSharp.SKMatrix" /> equivalent of the current matrix.</summary>
		/// <value>The equivalent 3x3 matrix.</value>
		/// <remarks>When converting from <see cref="T:SkiaSharp.SKMatrix44" /> to <see cref="T:SkiaSharp.SKMatrix" />, the third row and column is dropped.</remarks>
		public SKMatrix Matrix =>
			new SKMatrix (
				m00, m10, m30,
				m01, m11, m31,
				m03, m13, m33);

		/// <summary>Gets or sets a value in the matrix.</summary>
		/// <param name="row">The row to retrieve the value from.</param>
		/// <param name="column">The column to retrieve the value from.</param>
		/// <value>Returns the value found at the specified coordinates.</value>
		/// <remarks />
		public float this[int row, int column] {
			get => row switch {
				0 => column switch {
					0 => m00,
					1 => m01,
					2 => m02,
					3 => m03,
					_ => throw new ArgumentOutOfRangeException (nameof (column))
				},
				1 => column switch {
					0 => m10,
					1 => m11,
					2 => m12,
					3 => m13,
					_ => throw new ArgumentOutOfRangeException (nameof (column))
				},
				2 => column switch {
					0 => m20,
					1 => m21,
					2 => m22,
					3 => m23,
					_ => throw new ArgumentOutOfRangeException (nameof (column))
				},
				3 => column switch {
					0 => m30,
					1 => m31,
					2 => m32,
					3 => m33,
					_ => throw new ArgumentOutOfRangeException (nameof (column))
				},
				_ => throw new ArgumentOutOfRangeException (nameof (row))
			};
			set {
				switch (row) {
					case 0:
						switch (column) {
							case 0:
								m00 = value;
								break;
							case 1:
								m01 = value;
								break;
							case 2:
								m02 = value;
								break;
							case 3:
								m03 = value;
								break;
							default:
								throw new ArgumentOutOfRangeException (nameof (column));
						};
						break;
					case 1:
						switch (column) {
							case 0:
								m10 = value;
								break;
							case 1:
								m11 = value;
								break;
							case 2:
								m12 = value;
								break;
							case 3:
								m13 = value;
								break;
							default:
								throw new ArgumentOutOfRangeException (nameof (column));
						};
						break;
					case 2:
						switch (column) {
							case 0:
								m20 = value;
								break;
							case 1:
								m21 = value;
								break;
							case 2:
								m22 = value;
								break;
							case 3:
								m23 = value;
								break;
							default:
								throw new ArgumentOutOfRangeException (nameof (column));
						};
						break;
					case 3:
						switch (column) {
							case 0:
								m30 = value;
								break;
							case 1:
								m31 = value;
								break;
							case 2:
								m32 = value;
								break;
							case 3:
								m33 = value;
								break;
							default:
								throw new ArgumentOutOfRangeException (nameof (column));
						};
						break;
					default:
						throw new ArgumentOutOfRangeException (nameof (row));
				};
			}
		}

		// type casting

		/// <summary>Converts an <see cref="T:SkiaSharp.SKMatrix" /> to an <see cref="T:SkiaSharp.SKMatrix44" />.</summary>
		/// <param name="matrix">The <see cref="T:SkiaSharp.SKMatrix" /> to convert.</param>
		/// <returns>Returns the converted matrix.</returns>
		/// <remarks />
		public static implicit operator SKMatrix44 (SKMatrix matrix) =>
			new SKMatrix44 (
				matrix.ScaleX, matrix.SkewY, 0, matrix.Persp0,
				matrix.SkewX, matrix.ScaleY, 0, matrix.Persp1,
				0, 0, 1, 0,
				matrix.TransX, matrix.TransY, 0, matrix.Persp2);

		/// <summary>Converts an <see cref="T:SkiaSharp.SKMatrix44" /> to a <see cref="T:System.Numerics.Matrix4x4" />.</summary>
		/// <param name="matrix">The <see cref="T:SkiaSharp.SKMatrix44" /> to convert.</param>
		/// <returns>Returns the converted matrix.</returns>
		/// <remarks />
		public static implicit operator Matrix4x4 (SKMatrix44 matrix) =>
			Unsafe.As<SKMatrix44, Matrix4x4> (ref matrix);

		/// <summary>Converts a <see cref="T:System.Numerics.Matrix4x4" /> to an <see cref="T:SkiaSharp.SKMatrix44" />.</summary>
		/// <param name="matrix">The <see cref="T:System.Numerics.Matrix4x4" /> to convert.</param>
		/// <returns>Returns the converted matrix.</returns>
		/// <remarks />
		public static implicit operator SKMatrix44 (Matrix4x4 matrix) =>
			Unsafe.As<Matrix4x4, SKMatrix44> (ref matrix);
	}
}
