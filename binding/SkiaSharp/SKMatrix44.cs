﻿#nullable disable

using System;
using System.Numerics;

namespace SkiaSharp
{
	public unsafe partial struct SKMatrix44
	{
		internal const float DegreesToRadians = (float)Math.PI / 180.0f;

		public readonly static SKMatrix44 Empty;

		public readonly static SKMatrix44 Identity = Matrix4x4.Identity;

		public SKMatrix44 ()
		{
		}

		public SKMatrix44 (SKMatrix src)
		{
			this = src;
		}

		public SKMatrix44 (SKMatrix44 src)
		{
			this = src;
		}

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

		public static SKMatrix44 CreateIdentity () => Identity;

		public static SKMatrix44 CreateTranslation (float x, float y, float z) =>
			Matrix4x4.CreateTranslation (x, y, z);

		public static SKMatrix44 CreateScale (float x, float y, float z) =>
			Matrix4x4.CreateScale (x, y, z);

		public static SKMatrix44 CreateScale (float x, float y, float z, float pivotX, float pivotY, float pivotZ) =>
			Matrix4x4.CreateScale (x, y, z, new Vector3 (pivotX, pivotY, pivotZ));

		// CreateRotation*

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

		public readonly float[] ToRowMajor ()
		{
			var dst = new float[16];
			ToRowMajor (dst);
			return dst;
		}

		public readonly float[] ToColumnMajor ()
		{
			var dst = new float[16];
			ToColumnMajor (dst);
			return dst;
		}

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

		public readonly bool IsInvertible =>
			Matrix4x4.Invert (this, out _);

		public readonly bool TryInvert (out SKMatrix44 inverse)
		{
			if (Matrix4x4.Invert (this, out var inv)) {
				inverse = inv;
				return true;
			}

			inverse = Empty;
			return false;
		}

		public readonly SKMatrix44 Invert ()
		{
			if (Matrix4x4.Invert (this, out var inverted))
				return inverted;

			return Empty;
		}

		// Transpose

		public readonly SKMatrix44 Transpose () =>
			Matrix4x4.Transpose (this);

		// Determinant

		public readonly float Determinant () =>
			((Matrix4x4)this).GetDeterminant ();

		// MapPoints

		public readonly SKPoint MapPoint (SKPoint point) =>
			Vector2.Transform (point, this);

		public readonly SKPoint3 MapPoint (SKPoint3 point) =>
			Vector3.Transform (point, this);

		public readonly SKPoint MapPoint (float x, float y) =>
			MapPoint (new SKPoint (x, y));

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
			if (srcVector4 == null)
				throw new ArgumentNullException (nameof (srcVector4));
			if (srcVector4.Length != 4)
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));

			var vector = new Vector4 (srcVector4[0], srcVector4[1], srcVector4[2], srcVector4[3]);
			var t = Vector4.Transform (vector, this);
			return new[] { t.X, t.Y, t.Z, t.W };
		}

		internal void MapScalars (ReadOnlySpan<float> srcVector4, Span<float> dstVector4)
		{
			if (srcVector4 == null)
				throw new ArgumentNullException (nameof (srcVector4));
			if (srcVector4.Length != 4)
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));
			if (dstVector4 == null)
				throw new ArgumentNullException (nameof (dstVector4));
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

		public static SKMatrix44 Concat (SKMatrix44 first, SKMatrix44 second) =>
			first * second;

		public readonly SKMatrix44 PreConcat (SKMatrix44 matrix) =>
			this * matrix;

		public readonly SKMatrix44 PostConcat (SKMatrix44 matrix) =>
			matrix * this;

		public static void Concat (ref SKMatrix44 target, SKMatrix44 first, SKMatrix44 second) =>
			target = first * second;

		// Operations

		public static SKMatrix44 Negate (SKMatrix44 value) =>
			-value;

		public static SKMatrix44 Add (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 + value2;

		public static SKMatrix44 Subtract (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 - value2;

		public static SKMatrix44 Multiply (SKMatrix44 value1, SKMatrix44 value2) =>
			value1 * value2;

		public static SKMatrix44 Multiply (SKMatrix44 value1, float value2) =>
			value1 * value2;

		// operators

		public static SKMatrix44 operator - (SKMatrix44 value) =>
			-((Matrix4x4)value);

		public static SKMatrix44 operator + (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) + ((Matrix4x4)value2);

		public static SKMatrix44 operator - (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) - ((Matrix4x4)value2);

		public static SKMatrix44 operator * (SKMatrix44 value1, SKMatrix44 value2) =>
			((Matrix4x4)value1) * ((Matrix4x4)value2);

		public static SKMatrix44 operator * (SKMatrix44 value1, float value2) =>
			((Matrix4x4)value1) * value2;

		// properties

		public SKMatrix Matrix =>
			new SKMatrix (
				m00, m01, m03,
				m10, m11, m13,
				m30, m31, m33);

		public float this[int row, int column] {
			get => column switch {
				0 => row switch {
					0 => m00,
					1 => m01,
					2 => m02,
					3 => m03,
					_ => throw new ArgumentOutOfRangeException (nameof (row))
				},
				1 => row switch {
					0 => m10,
					1 => m11,
					2 => m12,
					3 => m13,
					_ => throw new ArgumentOutOfRangeException (nameof (row))
				},
				2 => row switch {
					0 => m20,
					1 => m21,
					2 => m22,
					3 => m23,
					_ => throw new ArgumentOutOfRangeException (nameof (row))
				},
				3 => row switch {
					0 => m30,
					1 => m31,
					2 => m32,
					3 => m33,
					_ => throw new ArgumentOutOfRangeException (nameof (row))
				},
				_ => throw new ArgumentOutOfRangeException (nameof (column))
			};
			set {
				switch (column) {
					case 0:
						switch (row) {
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
								throw new ArgumentOutOfRangeException (nameof (row));
						};
						break;
					case 1:
						switch (row) {
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
								throw new ArgumentOutOfRangeException (nameof (row));
						};
						break;
					case 2:
						switch (row) {
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
								throw new ArgumentOutOfRangeException (nameof (row));
						};
						break;
					case 3:
						switch (row) {
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
								throw new ArgumentOutOfRangeException (nameof (row));
						};
						break;
					default:
						throw new ArgumentOutOfRangeException (nameof (column));
				};
			}
		}

		// type casting

		public static implicit operator SKMatrix44 (SKMatrix matrix) =>
			new SKMatrix44 (
				matrix.ScaleX, matrix.SkewX, 0, matrix.TransX,
				matrix.SkewY, matrix.ScaleY, 0, matrix.TransY,
				0, 0, 1, 0,
				matrix.Persp0, matrix.Persp1, 0, matrix.Persp2);

		public static implicit operator Matrix4x4 (SKMatrix44 matrix) =>
			new Matrix4x4 (
				matrix.m00, matrix.m10, matrix.m20, matrix.m30,
				matrix.m01, matrix.m11, matrix.m21, matrix.m31,
				matrix.m02, matrix.m12, matrix.m22, matrix.m32,
				matrix.m03, matrix.m13, matrix.m23, matrix.m33);

		public static implicit operator SKMatrix44 (Matrix4x4 matrix) =>
			new SKMatrix44 (
				matrix.M11, matrix.M21, matrix.M31, matrix.M41,
				matrix.M12, matrix.M22, matrix.M32, matrix.M42,
				matrix.M13, matrix.M23, matrix.M33, matrix.M43,
				matrix.M14, matrix.M24, matrix.M34, matrix.M44);
	}
}
