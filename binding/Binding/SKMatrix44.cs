using System;
using System.ComponentModel;

namespace SkiaSharp
{
	[Obsolete ("Use SKMatrix4x4 instead.")]
	[EditorBrowsable (EditorBrowsableState.Never)]
	public unsafe class SKMatrix44 : SKObject
	{
		internal SKMatrix44 (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_matrix44_destroy (Handle);

		public SKMatrix44 ()
			: this (SkiaApi.sk_matrix44_new (), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
		}

		public SKMatrix44 (SKMatrix44 src)
			: this (IntPtr.Zero, true)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			Handle = SkiaApi.sk_matrix44_new_copy (src.Handle);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
		}

		public SKMatrix44 (SKMatrix44 a, SKMatrix44 b)
			: this (IntPtr.Zero, true)
		{
			if (a == null)
				throw new ArgumentNullException (nameof (a));
			if (b == null)
				throw new ArgumentNullException (nameof (b));

			Handle = SkiaApi.sk_matrix44_new_concat (a.Handle, b.Handle);

			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
		}

		public SKMatrix44 (SKMatrix src)
			: this (SkiaApi.sk_matrix44_new_matrix (&src), true)
		{
			if (Handle == IntPtr.Zero)
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
		}

		// properties

		public SKMatrix Matrix {
			get {
				SKMatrix matrix;
				SkiaApi.sk_matrix44_to_matrix (Handle, &matrix);
				return matrix;
			}
		}

		public SKMatrix44TypeMask Type =>
			SkiaApi.sk_matrix44_get_type (Handle);

		public float this[int row, int column] {
			get => SkiaApi.sk_matrix44_get (Handle, row, column);
			set => SkiaApi.sk_matrix44_set (Handle, row, column, value);
		}

		// Create*

		public static SKMatrix44 CreateIdentity ()
		{
			var matrix = new SKMatrix44 ();
			matrix.SetIdentity ();
			return matrix;
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		public static SKMatrix44 CreateTranslate (float x, float y, float z) =>
			CreateTranslation (x, y, z);

		public static SKMatrix44 CreateTranslation (float x, float y, float z)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetTranslate (x, y, z);
			return matrix;
		}

		public static SKMatrix44 CreateScale (float x, float y, float z)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetScale (x, y, z);
			return matrix;
		}

		public static SKMatrix44 CreateRotation (float x, float y, float z, float radians)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetRotationAbout (x, y, z, radians);
			return matrix;
		}

		public static SKMatrix44 CreateRotationDegrees (float x, float y, float z, float degrees)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetRotationAboutDegrees (x, y, z, degrees);
			return matrix;
		}

		// From

		public static SKMatrix44 FromRowMajor (float[] src) =>
			FromRowMajor (src.AsSpan());

		private static SKMatrix44 FromRowMajor (ReadOnlySpan<float> src)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetRowMajor (src);
			return matrix;
		}

		public static SKMatrix44 FromColumnMajor (float[] src) =>
			FromColumnMajor (src.AsSpan ());

		private static SKMatrix44 FromColumnMajor (ReadOnlySpan<float> src)
		{
			var matrix = new SKMatrix44 ();
			matrix.SetColumnMajor (src);
			return matrix;
		}

		// To*

		public float[] ToColumnMajor ()
		{
			var dst = new float[16];
			ToColumnMajor (dst);
			return dst;
		}

		public void ToColumnMajor (float[] dst) =>
			ToColumnMajor (dst.AsSpan());

		private void ToColumnMajor (Span<float> dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			fixed (float* d = dst) {
				SkiaApi.sk_matrix44_as_col_major (Handle, d);
			}
		}

		public float[] ToRowMajor ()
		{
			var dst = new float[16];
			ToRowMajor (dst);
			return dst;
		}

		public void ToRowMajor (float[] dst) =>
			ToRowMajor (dst.AsSpan ());

		private void ToRowMajor (Span<float> dst)
		{
			if (dst == null)
				throw new ArgumentNullException (nameof (dst));
			if (dst.Length != 16)
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));

			fixed (float* d = dst) {
				SkiaApi.sk_matrix44_as_row_major (Handle, d);
			}
		}

		// Equal

		public static bool Equal (SKMatrix44 left, SKMatrix44 right)
		{
			if (left == null)
				throw new ArgumentNullException (nameof (left));
			if (right == null)
				throw new ArgumentNullException (nameof (right));

			return SkiaApi.sk_matrix44_equals (left.Handle, right.Handle);
		}

		// Set*

		public void SetIdentity () =>
			SkiaApi.sk_matrix44_set_identity (Handle);

		public void SetColumnMajor (float[] src) =>
			SetColumnMajor (src.AsSpan ());

		private void SetColumnMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			fixed (float* s = src) {
				SkiaApi.sk_matrix44_set_col_major (Handle, s);
			}
		}

		public void SetRowMajor (float[] src) =>
			SetRowMajor (src.AsSpan ());

		private void SetRowMajor (ReadOnlySpan<float> src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));
			if (src.Length != 16)
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));

			fixed (float* s = src) {
				SkiaApi.sk_matrix44_set_row_major (Handle, s);
			}
		}

		public void Set3x3ColumnMajor (float[] src)
		{
			if (src.Length != 9)
				throw new ArgumentException ("The source array must be 9 entries.", nameof (src));

			var row = stackalloc float[9] { src[0], src[3], src[6], src[1], src[4], src[7], src[2], src[5], src[8] };
			SkiaApi.sk_matrix44_set_3x3_row_major (Handle, row);
		}

		public void Set3x3RowMajor (float[] src)
		{
			if (src.Length != 9)
				throw new ArgumentException ("The source array must be 9 entries.", nameof (src));

			fixed (float* s = src) {
				SkiaApi.sk_matrix44_set_3x3_row_major (Handle, s);
			}
		}

		public void SetTranslate (float dx, float dy, float dz) =>
			SkiaApi.sk_matrix44_set_translate (Handle, dx, dy, dz);

		public void SetScale (float sx, float sy, float sz) =>
			SkiaApi.sk_matrix44_set_scale (Handle, sx, sy, sz);

		public void SetRotationAboutDegrees (float x, float y, float z, float degrees) =>
			SkiaApi.sk_matrix44_set_rotate_about_degrees (Handle, x, y, z, degrees);

		public void SetRotationAbout (float x, float y, float z, float radians) =>
			SkiaApi.sk_matrix44_set_rotate_about_radians (Handle, x, y, z, radians);

		public void SetRotationAboutUnit (float x, float y, float z, float radians) =>
			SkiaApi.sk_matrix44_set_rotate_about_radians_unit (Handle, x, y, z, radians);

		public void SetConcat (SKMatrix44 a, SKMatrix44 b)
		{
			if (a == null)
				throw new ArgumentNullException (nameof (a));
			if (b == null)
				throw new ArgumentNullException (nameof (b));

			SkiaApi.sk_matrix44_set_concat (Handle, a.Handle, b.Handle);
		}

		// Pre* / Post*

		public void PreTranslate (float dx, float dy, float dz) =>
			SkiaApi.sk_matrix44_pre_translate (Handle, dx, dy, dz);

		public void PostTranslate (float dx, float dy, float dz) =>
			SkiaApi.sk_matrix44_post_translate (Handle, dx, dy, dz);

		public void PreScale (float sx, float sy, float sz) =>
			SkiaApi.sk_matrix44_pre_scale (Handle, sx, sy, sz);

		public void PostScale (float sx, float sy, float sz) =>
			SkiaApi.sk_matrix44_post_scale (Handle, sx, sy, sz);

		public void PreConcat (SKMatrix44 m)
		{
			if (m == null)
				throw new ArgumentNullException (nameof (m));

			SkiaApi.sk_matrix44_pre_concat (Handle, m.Handle);
		}

		public void PostConcat (SKMatrix44 m)
		{
			if (m == null)
				throw new ArgumentNullException (nameof (m));

			SkiaApi.sk_matrix44_post_concat (Handle, m.Handle);
		}

		// Invert

		public bool IsInvertible =>
			SkiaApi.sk_matrix44_invert (Handle, IntPtr.Zero);

		public SKMatrix44 Invert ()
		{
			var inverse = new SKMatrix44 ();
			if (!Invert (inverse)) {
				inverse.Dispose ();
				inverse = null;
			}
			return inverse;
		}

		public bool Invert (SKMatrix44 inverse)
		{
			if (inverse == null)
				throw new ArgumentNullException (nameof (inverse));

			return SkiaApi.sk_matrix44_invert (Handle, inverse.Handle);
		}

		// Transpose

		public void Transpose () =>
			SkiaApi.sk_matrix44_transpose (Handle);

		// MapScalars

		public float[] MapScalars (float x, float y, float z, float w)
		{
			var srcVector4 = new float[4] { x, y, z, w };
			var dstVector4 = new float[4];
			MapScalars (srcVector4, dstVector4);
			return dstVector4;
		}

		public float[] MapScalars (float[] srcVector4)
		{
			var dstVector4 = new float[4];
			MapScalars (srcVector4, dstVector4);
			return dstVector4;
		}

		public void MapScalars (float[] srcVector4, float[] dstVector4)
		{
			if (srcVector4 == null)
				throw new ArgumentNullException (nameof (srcVector4));
			if (srcVector4.Length != 4)
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));
			if (dstVector4 == null)
				throw new ArgumentNullException (nameof (dstVector4));
			if (dstVector4.Length != 4)
				throw new ArgumentException ("The destination vector array must be 4 entries.", nameof (dstVector4));

			fixed (float* s = srcVector4)
			fixed (float* d = dstVector4) {
				SkiaApi.sk_matrix44_map_scalars (Handle, s, d);
			}
		}

		// MapPoints

		public SKPoint MapPoint (SKPoint src) =>
			MapPoints (new[] { src })[0];

		public SKPoint[] MapPoints (SKPoint[] src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			var count = src.Length;
			var src2Length = count * 2;
			//var src4Length = count * 4;

			var src2 = new float[src2Length];
			for (int i = 0, i2 = 0; i < count; i++, i2 += 2) {
				src2[i2] = src[i].X;
				src2[i2 + 1] = src[i].Y;
			}

			var dst4 = MapVector2 (src2);

			var dst = new SKPoint[count];
			for (int i = 0, i4 = 0; i < count; i++, i4 += 4) {
				dst[i].X = dst4[i4];
				dst[i].Y = dst4[i4 + 1];
			}

			return dst;
		}

		// MapVector2

		public float[] MapVector2 (float[] src2)
		{
			if (src2 == null)
				throw new ArgumentNullException (nameof (src2));
			if (src2.Length % 2 != 0)
				throw new ArgumentException ("The source vector array must be a set of pairs.", nameof (src2));

			var dst4 = new float[src2.Length * 2];
			MapVector2 (src2, dst4);
			return dst4;
		}

		public void MapVector2 (float[] src2, float[] dst4)
		{
			if (src2 == null)
				throw new ArgumentNullException (nameof (src2));
			if (src2.Length % 2 != 0)
				throw new ArgumentException ("The source vector array must be a set of pairs.", nameof (src2));
			if (dst4 == null)
				throw new ArgumentNullException (nameof (dst4));
			if (dst4.Length % 4 != 0)
				throw new ArgumentException ("The destination vector array must be a set quads.", nameof (dst4));
			if (src2.Length / 2 != dst4.Length / 4)
				throw new ArgumentException ("The source vector array must have the same number of pairs as the destination vector array has quads.", nameof (dst4));

			fixed (float* s = src2)
			fixed (float* d = dst4) {
				SkiaApi.sk_matrix44_map2 (Handle, s, src2.Length / 2, d);
			}
		}

		// Preserves2DAxisAlignment

		public bool Preserves2DAxisAlignment (float epsilon) =>
			SkiaApi.sk_matrix44_preserves_2d_axis_alignment (Handle, epsilon);

		// Determinant

		public double Determinant () =>
			SkiaApi.sk_matrix44_determinant (Handle);

		// operators

		public static explicit operator SKMatrix44 (SKMatrix matrix) =>
			new SKMatrix44 (matrix);

		public static explicit operator SKMatrix44 (SKMatrix4x4 matrix)
		{
			Span<float> mat = stackalloc float[16];
			matrix.ToColumnMajor (mat);
			return FromColumnMajor (mat);
		}

		internal static SKMatrix44 GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new SKMatrix44 (h, o));
	}
}
