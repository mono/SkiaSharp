using System;

namespace SkiaSharp
{
	public unsafe partial struct SKMatrix4x4
	{
		public SKMatrix4x4 (
			float m11, float m12, float m13, float m14,
			float m21, float m22, float m23, float m24,
			float m31, float m32, float m33, float m34,
			float m41, float m42, float m43, float m44)
		{
			this.m11 = m11;
			this.m12 = m12;
			this.m13 = m13;
			this.m14 = m14;

			this.m21 = m21;
			this.m22 = m22;
			this.m23 = m23;
			this.m24 = m24;

			this.m31 = m31;
			this.m32 = m32;
			this.m33 = m33;
			this.m34 = m34;

			this.m41 = m41;
			this.m42 = m42;
			this.m43 = m43;
			this.m44 = m44;
		}

		public SKMatrix4x4 (in SKMatrix src)
		{
			m11 = src.ScaleX;
			m21 = src.SkewX;
			m31 = 0;
			m41 = src.TransX;

			m12 = src.ScaleY;
			m22 = src.SkewY;
			m32 = 0;
			m42 = src.TransY;

			m13 = 0;
			m23 = 0;
			m33 = 1;
			m43 = 0;

			m14 = src.Persp0;
			m24 = src.Persp1;
			m34 = 0;
			m44 = src.Persp2;
		}

		// properties

		public readonly SKMatrix Matrix =>
			new (m11, m21, m41,
				 m12, m22, m42,
				 m14, m24, m44);

		public float this[int index] {
			readonly get => index switch {
				0 => m11,
				1 => m12,
				2 => m13,
				3 => m14,
				4 => m21,
				5 => m22,
				6 => m23,
				7 => m24,
				8 => m31,
				9 => m32,
				10 => m33,
				11 => m34,
				12 => m41,
				13 => m42,
				14 => m43,
				15 => m44,
				_ => throw new ArgumentOutOfRangeException (nameof (index)),
			};
			set {
				switch (index) {
#pragma warning disable IDE0055
					case 0: m11 = value; break;
					case 1: m12 = value; break;
					case 2: m13 = value; break;
					case 3: m14 = value; break;
					case 4: m21 = value; break;
					case 5: m22 = value; break;
					case 6: m23 = value; break;
					case 7: m24 = value; break;
					case 8: m31 = value; break;
					case 9: m32 = value; break;
					case 10: m33 = value; break;
					case 11: m34 = value; break;
					case 12: m41 = value; break;
					case 13: m42 = value; break;
					case 14: m43 = value; break;
					case 15: m44 = value; break;
					default: throw new ArgumentOutOfRangeException (nameof (index));
#pragma warning restore IDE0055
				}
			}
		}

		public float this[int row, int column] {
			readonly get {
				if (row < 0 || row > 3)
					throw new ArgumentOutOfRangeException (nameof (row));
				if (column < 0 || column > 3)
					throw new ArgumentOutOfRangeException (nameof (column));
				return this[(row * 4) + column];
			}
			set {
				if (row < 0 || row > 3)
					throw new ArgumentOutOfRangeException (nameof (row));
				if (column < 0 || column > 3)
					throw new ArgumentOutOfRangeException (nameof (column));
				this[(row * 4) + column] = value;
			}
		}

		// Create*

		public static SKMatrix4x4 CreateIdentity () =>
			new (1, 0, 0, 0,
				 0, 1, 0, 0,
				 0, 0, 1, 0,
				 0, 0, 0, 1);

		public static SKMatrix4x4 CreateTranslation (float x, float y, float z) =>
			new (1, 0, 0, 0,
				 0, 1, 0, 0,
				 0, 0, 1, 0,
				 x, y, z, 1);

		public static SKMatrix4x4 CreateScale (float x, float y, float z) =>
			new (x, 0, 0, 0,
				 0, y, 0, 0,
				 0, 0, z, 0,
				 0, 0, 0, 1);

		public static SKMatrix4x4 CreateRotation (float x, float y, float z, float radians)
		{
			var matrix = new SKMatrix4x4 ();
			matrix.SetRotationAbout (x, y, z, radians);
			return matrix;
		}

		public static SKMatrix4x4 CreateRotationDegrees (float x, float y, float z, float degrees)
		{
			var matrix = new SKMatrix4x4 ();
			matrix.SetRotationAboutDegrees (x, y, z, degrees);
			return matrix;
		}

		// From

		public static SKMatrix4x4 FromRowMajor (ReadOnlySpan<float> src)
		{
			var matrix = new SKMatrix4x4 ();
			matrix.SetRowMajor (src);
			return matrix;
		}

		public static SKMatrix4x4 FromColumnMajor (ReadOnlySpan<float> src)
		{
			var matrix = new SKMatrix4x4 ();
			matrix.SetColumnMajor (src);
			return matrix;
		}

		// To*

		public readonly float[] ToColumnMajor ()
		{
			var dst = new float[16];
			ToColumnMajor (dst);
			return dst;
		}

		public readonly void ToColumnMajor (Span<float> dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			dst[0] = m11;
			dst[1] = m12;
			dst[2] = m13;
			dst[3] = m14;

			dst[4] = m21;
			dst[5] = m22;
			dst[6] = m23;
			dst[7] = m24;

			dst[8] = m31;
			dst[9] = m32;
			dst[10] = m33;
			dst[11] = m34;

			dst[12] = m41;
			dst[13] = m42;
			dst[14] = m43;
			dst[15] = m44;
		}

		public readonly float[] ToRowMajor ()
		{
			var dst = new float[16];
			ToRowMajor (dst);
			return dst;
		}

		public readonly void ToRowMajor (Span<float> dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			dst[0] = m11;
			dst[1] = m21;
			dst[2] = m31;
			dst[3] = m41;

			dst[4] = m12;
			dst[5] = m22;
			dst[6] = m32;
			dst[7] = m42;

			dst[8] = m13;
			dst[9] = m23;
			dst[10] = m33;
			dst[11] = m43;

			dst[12] = m14;
			dst[13] = m24;
			dst[14] = m34;
			dst[15] = m44;
		}

		// Set*

		public void SetIdentity () =>
			this = CreateIdentity ();

		public void SetColumnMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			m11 = src[0];
			m12 = src[1];
			m13 = src[2];
			m14 = src[3];

			m21 = src[4];
			m22 = src[5];
			m23 = src[6];
			m24 = src[7];

			m31 = src[8];
			m32 = src[9];
			m33 = src[10];
			m34 = src[11];

			m41 = src[12];
			m42 = src[13];
			m43 = src[14];
			m44 = src[15];
		}

		public void SetRowMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			m11 = src[0];
			m21 = src[1];
			m31 = src[2];
			m41 = src[3];

			m12 = src[4];
			m22 = src[5];
			m32 = src[6];
			m42 = src[7];

			m13 = src[8];
			m23 = src[9];
			m33 = src[10];
			m43 = src[11];

			m14 = src[12];
			m24 = src[13];
			m34 = src[14];
			m44 = src[15];
		}

		public void Set3x3ColumnMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 9)
				throw new ArgumentException ("The source array must be 9 entries.", nameof (src));

			m11 = src[0];
			m21 = src[1];
			m31 = 0;
			m41 = src[2];

			m12 = src[3];
			m22 = src[4];
			m32 = 0;
			m42 = src[5];

			m13 = 0;
			m23 = 0;
			m33 = 1;
			m43 = 0;

			m14 = src[6];
			m24 = src[7];
			m34 = 0;
			m44 = src[8];
		}

		public void Set3x3RowMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 9)
				throw new ArgumentException ("The source array must be 9 entries.", nameof (src));

			m11 = src[0];
			m12 = src[1];
			m13 = 0;
			m14 = src[2];

			m21 = src[3];
			m22 = src[4];
			m23 = 0;
			m24 = src[5];

			m31 = 0;
			m32 = 0;
			m33 = 1;
			m34 = 0;

			m41 = src[6];
			m42 = src[7];
			m43 = 0;
			m44 = src[8];
		}

		public void SetTranslation (float dx, float dy, float dz) =>
			this = CreateTranslation (dx, dy, dz);

		public void SetScale (float sx, float sy, float sz) =>
			this = CreateScale (sx, sy, sz);

		public void SetRotationAboutDegrees (float x, float y, float z, float degrees) =>
			SetRotationAbout (x, y, z, degrees * SKMatrix.DegreesToRadians);

		public void SetRotationAbout (float x, float y, float z, float radians)
		{
			var length = x * x + y * y + z * z;

			if (length == 0) {
				SetIdentity ();
				return;
			}

			if (length != 1) {
				var scale = 1 / (float)Math.Sqrt (length);
				x *= scale;
				y *= scale;
				z *= scale;
			}

			SetRotationAboutUnit (x, y, z, radians);
		}

		public void SetRotationAboutUnit (float x, float y, float z, float radians)
		{
			var sa = (float)Math.Sin (radians);
			var ca = (float)Math.Cos (radians);
			var xx = x * x;
			var yy = y * y;
			var zz = z * z;
			var xy = x * y;
			var xz = x * z;
			var yz = y * z;

			m11 = xx + ca * (1.0f - xx);
			m12 = xy - ca * xy + sa * z;
			m13 = xz - ca * xz - sa * y;
			m14 = 0;

			m21 = xy - ca * xy - sa * z;
			m22 = yy + ca * (1.0f - yy);
			m23 = yz - ca * yz + sa * x;
			m24 = 0;

			m31 = xz - ca * xz + sa * y;
			m32 = yz - ca * yz - sa * x;
			m33 = zz + ca * (1.0f - zz);
			m34 = 0;

			m41 = 0;
			m42 = 0;
			m43 = 0;
			m44 = 1;
		}

		public void SetConcat (in SKMatrix4x4 a, in SKMatrix4x4 b) =>
			this = new (
				a.m11 * b.m11 + a.m12 * b.m21 + a.m13 * b.m31 + a.m14 * b.m41,
				a.m11 * b.m12 + a.m12 * b.m22 + a.m13 * b.m32 + a.m14 * b.m42,
				a.m11 * b.m13 + a.m12 * b.m23 + a.m13 * b.m33 + a.m14 * b.m43,
				a.m11 * b.m14 + a.m12 * b.m24 + a.m13 * b.m34 + a.m14 * b.m44,

				a.m21 * b.m11 + a.m22 * b.m21 + a.m23 * b.m31 + a.m24 * b.m41,
				a.m21 * b.m12 + a.m22 * b.m22 + a.m23 * b.m32 + a.m24 * b.m42,
				a.m21 * b.m13 + a.m22 * b.m23 + a.m23 * b.m33 + a.m24 * b.m43,
				a.m21 * b.m14 + a.m22 * b.m24 + a.m23 * b.m34 + a.m24 * b.m44,

				a.m31 * b.m11 + a.m32 * b.m21 + a.m33 * b.m31 + a.m34 * b.m41,
				a.m31 * b.m12 + a.m32 * b.m22 + a.m33 * b.m32 + a.m34 * b.m42,
				a.m31 * b.m13 + a.m32 * b.m23 + a.m33 * b.m33 + a.m34 * b.m43,
				a.m31 * b.m14 + a.m32 * b.m24 + a.m33 * b.m34 + a.m34 * b.m44,

				a.m41 * b.m11 + a.m42 * b.m21 + a.m43 * b.m31 + a.m44 * b.m41,
				a.m41 * b.m12 + a.m42 * b.m22 + a.m43 * b.m32 + a.m44 * b.m42,
				a.m41 * b.m13 + a.m42 * b.m23 + a.m43 * b.m33 + a.m44 * b.m43,
				a.m41 * b.m14 + a.m42 * b.m24 + a.m43 * b.m34 + a.m44 * b.m44);

		// Pre* / Post*

		public void PreTranslate (float dx, float dy, float dz) =>
			PreConcat (CreateTranslation (dx, dy, dz));

		public void PostTranslate (float dx, float dy, float dz) =>
			PostConcat (CreateTranslation (dx, dy, dz));

		public void PreScale (float sx, float sy, float sz) =>
			PreConcat (CreateScale (sx, sy, sz));

		public void PostScale (float sx, float sy, float sz) =>
			PostConcat (CreateScale (sx, sy, sz));

		public void PreConcat (in SKMatrix4x4 m) =>
			SetConcat (this, m);

		public void PostConcat (in SKMatrix4x4 m) =>
			SetConcat (m, this);

		// Invert

		public readonly bool Invert (out SKMatrix4x4 inverse)
		{
			float a = m11, b = m12, c = m13, d = m14;
			float e = m21, f = m22, g = m23, h = m24;
			float i = m31, j = m32, k = m33, l = m34;
			float m = m41, n = m42, o = m43, p = m44;

			var kp_lo = k * p - l * o;
			var jp_ln = j * p - l * n;
			var jo_kn = j * o - k * n;
			var ip_lm = i * p - l * m;
			var io_km = i * o - k * m;
			var in_jm = i * n - j * m;

			var a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
			var a12 = -(e * kp_lo - g * ip_lm + h * io_km);
			var a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
			var a14 = -(e * jo_kn - f * io_km + g * in_jm);

			var det = a * a11 + b * a12 + c * a13 + d * a14;

			if (Math.Abs (det) < float.Epsilon) {
				inverse = CreateIdentity ();
				return false;
			}

			var gp_ho = g * p - h * o;
			var fp_hn = f * p - h * n;
			var fo_gn = f * o - g * n;
			var ep_hm = e * p - h * m;
			var eo_gm = e * o - g * m;
			var en_fm = e * n - f * m;

			var gl_hk = g * l - h * k;
			var fl_hj = f * l - h * j;
			var fk_gj = f * k - g * j;
			var el_hi = e * l - h * i;
			var ek_gi = e * k - g * i;
			var ej_fi = e * j - f * i;

			var invDet = 1.0f / det;

			inverse = new SKMatrix4x4 (
				a11 * invDet,
				a12 * invDet,
				a13 * invDet,
				a14 * invDet,

				-(b * kp_lo - c * jp_ln + d * jo_kn) * invDet,
				+(a * kp_lo - c * ip_lm + d * io_km) * invDet,
				-(a * jp_ln - b * ip_lm + d * in_jm) * invDet,
				+(a * jo_kn - b * io_km + c * in_jm) * invDet,

				+(b * gp_ho - c * fp_hn + d * fo_gn) * invDet,
				-(a * gp_ho - c * ep_hm + d * eo_gm) * invDet,
				+(a * fp_hn - b * ep_hm + d * en_fm) * invDet,
				-(a * fo_gn - b * eo_gm + c * en_fm) * invDet,

				-(b * gl_hk - c * fl_hj + d * fk_gj) * invDet,
				+(a * gl_hk - c * el_hi + d * ek_gi) * invDet,
				-(a * fl_hj - b * el_hi + d * ej_fi) * invDet,
				+(a * fk_gj - b * ek_gi + c * ej_fi) * invDet);
			return true;
		}

		// Transpose

		public void Transpose () =>
			this = new (
				m11, m21, m31, m41,
				m12, m22, m32, m42,
				m13, m23, m33, m43,
				m14, m24, m34, m44);

		// MapVector4

		public readonly float[] MapVector4 (float x, float y, float z, float w)
		{
			Span<float> srcVector4 = stackalloc float[4] { x, y, z, w };
			var dstVector4 = new float[4];
			MapVector4 (srcVector4, dstVector4);
			return dstVector4;
		}

		public readonly void MapVector4 (ReadOnlySpan<float> srcVector4, Span<float> dstVector4)
		{
			if (srcVector4.Length % 4 != 0)
				throw new ArgumentException ("The source vector array must be multiples of 4.", nameof (srcVector4));
			if (dstVector4.Length % 4 != 0)
				throw new ArgumentException ("The destination vector array must be multiples of 4.", nameof (dstVector4));

			Span<float> working = stackalloc float[4];

			for (var i = 0; i < srcVector4.Length; i += 4) {
				var current = srcVector4.Slice (i);
				var destination = dstVector4.Slice (i);

				var c0 = current[0];
				var c1 = current[1];
				var c2 = current[2];
				var c3 = current[3];

				working[0] = m11 * c0 + m21 * c1 + m31 * c2 + m41 * c3;
				working[1] = m12 * c0 + m22 * c1 + m32 * c2 + m42 * c3;
				working[2] = m13 * c0 + m23 * c1 + m33 * c2 + m43 * c3;
				working[3] = m14 * c0 + m24 * c1 + m34 * c2 + m44 * c3;

				working.CopyTo (destination);
			}
		}

		// MapPoints

		public readonly SKPoint MapPoint (in SKPoint src)
		{
			Span<SKPoint> s = stackalloc[] { src };
			Span<SKPoint> d = stackalloc SKPoint[1];

			MapPoints (s, d);

			return d[0];
		}

		public readonly void MapPoints (ReadOnlySpan<SKPoint> src, Span<SKPoint> dst)
		{
			if (src.Length != dst.Length)
				throw new ArgumentException ("The destination array must have the same number of entries as the source array.", nameof (dst));

			for (var i = 0; i < src.Length; i++) {
				var s = src[i];
				dst[i] = new SKPoint (
					m11 * s.X + m21 * s.Y + m41,
					m12 * s.X + m22 * s.Y + m42);
			}
		}

		// MapVector2

		public readonly float[] MapVector2 (float x, float y)
		{
			Span<float> src2 = stackalloc float[2] { x, y };
			var dst4 = new float[4];
			MapVector2 (src2, dst4);
			return dst4;
		}

		public readonly void MapVector2 (ReadOnlySpan<float> src2, Span<float> dst4)
		{
			if (src2.Length % 2 != 0)
				throw new ArgumentException ("The source vector array must be a set of pairs.", nameof (src2));
			if (dst4.Length % 4 != 0)
				throw new ArgumentException ("The destination vector array must be a set quads.", nameof (dst4));
			if (src2.Length / 2 != dst4.Length / 4)
				throw new ArgumentException ("The source vector array must have the same number of pairs as the destination vector array has quads.", nameof (dst4));

			Span<float> working = stackalloc float[4];

			for (int i = 0, j = 0; i < src2.Length; i += 2, j += 4) {
				var current = src2.Slice (i);
				var destination = dst4.Slice (j);

				var c0 = current[0];
				var c1 = current[1];

				working[0] = m11 * c0 + m21 * c1 + m41;
				working[1] = m12 * c0 + m22 * c1 + m42;
				working[2] = m13 * c0 + m23 * c1 + m43;
				working[3] = m14 * c0 + m24 * c1 + m44;

				working.CopyTo (destination);
			}
		}

		// GetDeterminant

		public readonly double GetDeterminant ()
		{
			float a = m11, b = m12, c = m13, d = m14;
			float e = m21, f = m22, g = m23, h = m24;
			float i = m31, j = m32, k = m33, l = m34;
			float m = m41, n = m42, o = m43, p = m44;

			var kp_lo = k * p - l * o;
			var jp_ln = j * p - l * n;
			var jo_kn = j * o - k * n;
			var ip_lm = i * p - l * m;
			var io_km = i * o - k * m;
			var in_jm = i * n - j * m;

			var a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
			var a12 = -(e * kp_lo - g * ip_lm + h * io_km);
			var a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
			var a14 = -(e * jo_kn - f * io_km + g * in_jm);

			var det = a * a11 + b * a12 + c * a13 + d * a14;

			return det;
		}

		// operators

		public static implicit operator SKMatrix4x4 (in SKMatrix matrix) =>
			new SKMatrix4x4 (matrix);
	}
}
