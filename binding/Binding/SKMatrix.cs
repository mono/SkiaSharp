using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe partial struct SKMatrix
	{
		internal const float DegreesToRadians = (float)System.Math.PI / 180.0f;

		private class Indices {
			public const int ScaleX = 0;
			public const int SkewX = 1;
			public const int TransX = 2;
			public const int SkewY = 3;
			public const int ScaleY = 4;
			public const int TransY = 5;
			public const int Persp0 = 6;
			public const int Persp1 = 7;
			public const int Persp2 = 8;

			public const int Count = 9;
		};

		public SKMatrix (
			float scaleX, float skewX, float transX,
			float skewY, float scaleY, float transY,
			float persp0, float persp1, float persp2)
		{
			this.scaleX = scaleX;
			this.skewX = skewX;
			this.transX = transX;
			this.skewY = skewY;
			this.scaleY = scaleY;
            this.transY = transY;
			this.persp0 = persp0;
			this.persp1 = persp1;
			this.persp2 = persp2;
		}

		public float [] Values {
			get {
				return new float [9] {
					scaleX, skewX, transX,
					skewY, scaleY, transY,
					persp0, persp1, persp2 };
			}
			set {
				if (value == null)
					throw new ArgumentNullException (nameof (Values));
				if (value.Length != Indices.Count)
					throw new ArgumentException ($"The matrix array must have a length of {Indices.Count}.", nameof (Values));

				scaleX = value [Indices.ScaleX];
				skewX = value [Indices.SkewX];
				transX = value [Indices.TransX];

				skewY = value [Indices.SkewY];
				scaleY = value [Indices.ScaleY];
				transY = value [Indices.TransY];

				persp0 = value [Indices.Persp0];
				persp1 = value [Indices.Persp1];
				persp2 = value [Indices.Persp2];
			}
		}

		public void GetValues(float[] values)
		{
			if (values == null)
				throw new ArgumentNullException (nameof (values));
			if (values.Length != Indices.Count)
				throw new ArgumentException ($"The matrix array must have a length of {Indices.Count}.", nameof (values));

			values [Indices.ScaleX] = scaleX;
			values [Indices.SkewX] = skewX;
			values [Indices.TransX] = transX;
			values [Indices.SkewY] = skewY;
			values [Indices.ScaleY] = scaleY;
			values [Indices.TransY] = transY;
			values [Indices.Persp0] = persp0;
			values [Indices.Persp1] = persp1;
			values [Indices.Persp2] = persp2;
		}

#if OPTIMIZED_SKMATRIX

		//
		// If we manage to get an sk_matrix_t that contains the extra
		// the fTypeMask flag, we could accelerate various operations
		// as well, as this caches state of what is needed to be done.
		//
	
		[Flags]
		enum Mask : uint {
			Identity = 0,
			Translate = 1,
			Scale = 2,
			Affine = 4,
			Perspective = 8,
			RectStaysRect = 0x10,
			OnlyPerspectiveValid = 0x40,
			Unknown = 0x80,
			OrableMasks = Translate | Scale | Affine | Perspective,
			AllMasks = OrableMasks | RectStaysRect
		}
		Mask typeMask;

		Mask GetMask ()
		{
			if (typeMask.HasFlag (Mask.Unknown))
				typeMask = (Mask) SkiaApi.sk_matrix_get_type (ref this);

		        // only return the public masks
			return (Mask) ((uint)typeMask & 0xf);
		}
#endif

		static float sdot (float a, float b, float c, float d) => a * b + c * d;
		static float scross(float a, float b, float c, float d) => a * b - c * d;

		public static SKMatrix MakeIdentity ()
		{
			return new SKMatrix () { scaleX = 1, scaleY = 1, persp2 = 1
#if OPTIMIZED_SKMATRIX
					, typeMask = Mask.Identity | Mask.RectStaysRect
#endif
			};
		}

		public void SetScaleTranslate (float sx, float sy, float tx, float ty)
		{
			scaleX = sx;
			skewX = 0;
			transX = tx;

			skewY = 0;
			scaleY = sy;
			transY = ty;

			persp0 = 0;
			persp1 = 0;
			persp2 = 1;

#if OPTIMIZED_SKMATRIX
			typeMask = Mask.RectStaysRect | 
				((sx != 1 || sy != 1) ? Mask.Scale : 0) |
				((tx != 0 || ty != 0) ? Mask.Translate : 0);
#endif
		}

		public static SKMatrix MakeScale (float sx, float sy)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			return new SKMatrix () { scaleX = sx, scaleY = sy, persp2 = 1, 
#if OPTIMIZED_SKMATRIX
typeMask = Mask.Scale | Mask.RectStaysRect
#endif
			};
				
		}

		/// <summary>
		/// Set the matrix to scale by sx and sy, with a pivot point at (px, py).
		/// The pivot point is the coordinate that should remain unchanged by the
		/// specified transformation.
		public static SKMatrix MakeScale (float sx, float sy, float pivotX, float pivotY)
		{
			if (sx == 1 && sy == 1)
				return MakeIdentity ();
			float tx = pivotX - sx * pivotX;
			float ty = pivotY - sy * pivotY;

#if OPTIMIZED_SKMATRIX
			Mask mask = Mask.RectStaysRect | 
				((sx != 1 || sy != 1) ? Mask.Scale : 0) |
				((tx != 0 || ty != 0) ? Mask.Translate : 0);
#endif
			return new SKMatrix () { 
				scaleX = sx, scaleY = sy, 
				transX = tx, transY = ty,
				persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = mask
#endif
			};
		}

		public static SKMatrix MakeTranslation (float dx, float dy)
		{
			if (dx == 0 && dy == 0)
				return MakeIdentity ();
			
			return new SKMatrix () { 
				scaleX = 1, scaleY = 1,
				transX = dx, transY = dy,
				persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = Mask.Translate | Mask.RectStaysRect
#endif
			};
		}

		public static SKMatrix MakeRotation (float radians)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);

			var matrix = new SKMatrix ();
			SetSinCos (ref matrix, sin, cos);
			return matrix;
		}

		public static SKMatrix MakeRotation (float radians, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);

			var matrix = new SKMatrix ();
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
			return matrix;
		}

		public static SKMatrix MakeRotationDegrees (float degrees)
		{
			return MakeRotation (degrees * DegreesToRadians);
		}

		public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty)
		{
			return MakeRotation (degrees * DegreesToRadians, pivotx, pivoty);
		}

		private static void SetSinCos (ref SKMatrix matrix, float sin, float cos)
		{
			matrix.scaleX = cos;
			matrix.skewX = -sin;
			matrix.transX = 0;
			matrix.skewY = sin;
			matrix.scaleY = cos;
			matrix.transY = 0;
			matrix.persp0 = 0;
			matrix.persp1 = 0;
			matrix.persp2 = 1;
#if OPTIMIZED_SKMATRIX
			matrix.typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid;
#endif
		}

		private static void SetSinCos (ref SKMatrix matrix, float sin, float cos, float pivotx, float pivoty)
		{
			float oneMinusCos = 1-cos;
			
			matrix.scaleX = cos;
			matrix.skewX = -sin;
			matrix.transX = sdot(sin, pivoty, oneMinusCos, pivotx);
			matrix.skewY = sin;
			matrix.scaleY = cos;
			matrix.transY = sdot(-sin, pivotx, oneMinusCos, pivoty);
			matrix.persp0 = 0;
			matrix.persp1 = 0;
			matrix.persp2 = 1;
#if OPTIMIZED_SKMATRIX
			matrix.typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid;
#endif
		}
		
		public static void Rotate (ref SKMatrix matrix, float radians, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
		}

		public static void RotateDegrees (ref SKMatrix matrix, float degrees, float pivotx, float pivoty)
		{
			var sin = (float) Math.Sin (degrees * DegreesToRadians);
			var cos = (float) Math.Cos (degrees * DegreesToRadians);
			SetSinCos (ref matrix, sin, cos, pivotx, pivoty);
		}

		public static void Rotate (ref SKMatrix matrix, float radians)
		{
			var sin = (float) Math.Sin (radians);
			var cos = (float) Math.Cos (radians);
			SetSinCos (ref matrix, sin, cos);
		}

		public static void RotateDegrees (ref SKMatrix matrix, float degrees)
		{
			var sin = (float) Math.Sin (degrees * DegreesToRadians);
			var cos = (float) Math.Cos (degrees * DegreesToRadians);
			SetSinCos (ref matrix, sin, cos);
		}

		public static SKMatrix MakeSkew (float sx, float sy)
		{
			return new SKMatrix () {
				scaleX = 1,
				skewX = sx,
				transX = 0,
				skewY = sy,
				scaleY = 1,
				transY = 0,
				persp0 = 0,
				persp1 = 0,
				persp2 = 1,
#if OPTIMIZED_SKMATRIX
				typeMask = Mask.Unknown | Mask.OnlyPerspectiveValid
#endif
			};
		}

		public bool TryInvert (out SKMatrix inverse)
		{
			fixed (SKMatrix* i = &inverse)
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_try_invert (t, i);
			}
		}

		public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second)
		{
			fixed (SKMatrix* t = &target) {
				SkiaApi.sk_matrix_concat (t, &first, &second);
			}
		}

		public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* f = &first)
			fixed (SKMatrix* s = &second) {
				SkiaApi.sk_matrix_concat (t, f, s);
			}
		}

		public static void PreConcat (ref SKMatrix target, SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target) {
				SkiaApi.sk_matrix_pre_concat (t, &matrix);
			}
		}

		public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_pre_concat (t, m);
			}
		}

		public static void PostConcat (ref SKMatrix target, SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target) {
				SkiaApi.sk_matrix_post_concat (t, &matrix);
			}
		}

		public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			fixed (SKMatrix* t = &target)
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_matrix_post_concat (t, m);
			}
		}

		public static void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
		{
			fixed (SKMatrix* m = &matrix)
			fixed (SKRect* d = &dest)
			fixed (SKRect* s = &source) {
				SkiaApi.sk_matrix_map_rect (m, d, s);
			}
		}

		public SKRect MapRect (SKRect source)
		{
			MapRect (ref this, out var result, ref source);
			return result;
		}

		public void MapPoints (SKPoint [] result, SKPoint [] points)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			if (result.Length != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = points) {
				SkiaApi.sk_matrix_map_points (t, rp, pp, result.Length);
			}
		}

		public SKPoint [] MapPoints (SKPoint [] points)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			var res = new SKPoint [points.Length];
			MapPoints (res, points);
			return res;
		}

		public void MapVectors (SKPoint [] result, SKPoint [] vectors)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));
			if (result.Length != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");

			fixed (SKMatrix* t = &this)
			fixed (SKPoint* rp = result)
			fixed (SKPoint* pp = vectors) {
				SkiaApi.sk_matrix_map_vectors (t, rp, pp, result.Length);
			}
		}

		public SKPoint [] MapVectors (SKPoint [] vectors)
		{
			if (vectors == null)
				throw new ArgumentNullException (nameof (vectors));

			var res = new SKPoint [vectors.Length];
			MapVectors (res, vectors);
			return res;
		}

		public SKPoint MapPoint (SKPoint point)
		{
			return MapPoint (point.X, point.Y);
		}

		public SKPoint MapPoint (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_xy (t, x, y, &result);
			}
			return result;
		}

		public SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			fixed (SKMatrix* t = &this) {
				SkiaApi.sk_matrix_map_vector (t, x, y, &result);
			}
			return result;
		}

		public float MapRadius (float radius)
		{
			fixed (SKMatrix* t = &this) {
				return SkiaApi.sk_matrix_map_radius (t, radius);
			}
		}
	}

	public unsafe class SK3dView : SKObject
	{
		[Preserve]
		internal SK3dView (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		public SK3dView ()
			: this (SkiaApi.sk_3dview_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SK3dView instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_3dview_destroy (Handle);

		public SKMatrix Matrix {
			get {
				SKMatrix matrix = SKMatrix.MakeIdentity ();
				GetMatrix (ref matrix);
				return matrix;
			}
		}
		
		public void GetMatrix (ref SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_3dview_get_matrix (Handle, m);
			}
		}
		
		public void Save ()
		{
			SkiaApi.sk_3dview_save (Handle);
		}
		
		public void Restore ()
		{
			SkiaApi.sk_3dview_restore (Handle);
		}
		
		public void Translate (float x, float y, float z)
		{
			SkiaApi.sk_3dview_translate (Handle, x, y, z);
		}
		
		public void TranslateX (float x)
		{
			Translate (x, 0, 0);
		}
		
		public void TranslateY (float y)
		{
			Translate (0, y, 0);
		}
		
		public void TranslateZ (float z)
		{
			Translate (0, 0, z);
		}
		
		public void RotateXDegrees (float degrees)
		{
			SkiaApi.sk_3dview_rotate_x_degrees (Handle, degrees);
		}
		
		public void RotateXRadians (float radians)
		{
			SkiaApi.sk_3dview_rotate_x_radians (Handle, radians);
		}
		
		public void RotateYDegrees (float degrees)
		{
			SkiaApi.sk_3dview_rotate_y_degrees (Handle, degrees);
		}
		
		public void RotateYRadians (float radians)
		{
			SkiaApi.sk_3dview_rotate_y_radians (Handle, radians);
		}
		
		public void RotateZDegrees (float degrees)
		{
			SkiaApi.sk_3dview_rotate_z_degrees (Handle, degrees);
		}
		
		public void RotateZRadians (float radians)
		{
			SkiaApi.sk_3dview_rotate_z_radians (Handle, radians);
		}
		
		public void ApplyToCanvas (SKCanvas canvas)
		{
			if (canvas == null)
				throw new ArgumentNullException (nameof (canvas));

			SkiaApi.sk_3dview_apply_to_canvas (Handle, canvas.Handle);
		}

		public float DotWithNormal (float dx, float dy, float dz)
		{
			return SkiaApi.sk_3dview_dot_with_normal (Handle, dx, dy, dz);
		}
	}

	public unsafe class SKMatrix44 : SKObject
	{
		[Preserve]
		internal SKMatrix44 (IntPtr x, bool owns)
			: base (x, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_matrix44_destroy (Handle);

		public SKMatrix44 ()
			: this (SkiaApi.sk_matrix44_new(), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
			}
		}

		public SKMatrix44 (SKMatrix44 src)
			: this (IntPtr.Zero, true)
		{
			if (src == null) {
				throw new ArgumentNullException (nameof (src));
			}
			Handle = SkiaApi.sk_matrix44_new_copy (src.Handle);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
			}
		}

		public SKMatrix44 (SKMatrix44 a, SKMatrix44 b)
			: this (IntPtr.Zero, true)
		{
			if (a == null) {
				throw new ArgumentNullException (nameof (a));
			}
			if (b == null) {
				throw new ArgumentNullException (nameof (b));
			}

			Handle = SkiaApi.sk_matrix44_new_concat (a.Handle, b.Handle);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
			}
		}

		public SKMatrix44 (SKMatrix src)
			: this (CreateNew (ref src), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKMatrix44 instance.");
			}
		}

		private static IntPtr CreateNew (ref SKMatrix src)
		{
			fixed (SKMatrix* s = &src) {
				return SkiaApi.sk_matrix44_new_matrix (s);
			}
		}

		public static SKMatrix44 CreateIdentity ()
		{
			var matrix = new SKMatrix44();
			matrix.SetIdentity ();
			return matrix;
		}

		public static SKMatrix44 CreateTranslate (float x, float y, float z)
		{
			var matrix = new SKMatrix44();
			matrix.SetTranslate (x, y, z);
			return matrix;
		}

		public static SKMatrix44 CreateScale (float x, float y, float z)
		{
			var matrix = new SKMatrix44();
			matrix.SetScale (x, y, z);
			return matrix;
		}

		public static SKMatrix44 CreateRotation (float x, float y, float z, float radians)
		{
			var matrix = new SKMatrix44();
			matrix.SetRotationAbout (x, y, z, radians);
			return matrix;
		}

		public static SKMatrix44 CreateRotationDegrees (float x, float y, float z, float degrees)
		{
			var matrix = new SKMatrix44();
			matrix.SetRotationAboutDegrees (x, y, z, degrees);
			return matrix;
		}

		public static SKMatrix44 FromRowMajor (float [] src)
		{
			var matrix = new SKMatrix44();
			matrix.SetRowMajor (src);
			return matrix;
		}

		public static SKMatrix44 FromColumnMajor (float [] src)
		{
			var matrix = new SKMatrix44();
			matrix.SetColumnMajor (src);
			return matrix;
		}

		public static bool Equal (SKMatrix44 left, SKMatrix44 right)
		{
			if (left == null) {
				throw new ArgumentNullException (nameof (left));
			}
			if (right == null) {
				throw new ArgumentNullException (nameof (right));
			}

			return SkiaApi.sk_matrix44_equals (left.Handle, right.Handle);
		}

		public SKMatrix Matrix {
			get {
				SKMatrix matrix;
				SkiaApi.sk_matrix44_to_matrix (Handle, &matrix);
				return matrix;
			}
		}

		public SKMatrix44TypeMask Type => SkiaApi.sk_matrix44_get_type (Handle);

		public void SetIdentity ()
		{
			SkiaApi.sk_matrix44_set_identity (Handle);
		}

		public float this [int row, int column] {
			get {
				return SkiaApi.sk_matrix44_get (Handle, row, column);
			}
			set {
				SkiaApi.sk_matrix44_set (Handle, row, column, value);
			}
		}

		public float [] ToColumnMajor ()
		{
			var dst = new float [16];
			ToColumnMajor (dst);
			return dst;
		}

		public void ToColumnMajor (float [] dst)
		{
			if (dst == null) {
				throw new ArgumentNullException (nameof (dst));
			}
			if (dst.Length != 16) {
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));
			}
			fixed (float* d = dst) {
				SkiaApi.sk_matrix44_as_col_major (Handle, d);
			}
		}

		public float [] ToRowMajor ()
		{
			var dst = new float [16];
			ToRowMajor (dst);
			return dst;
		}

		public void ToRowMajor (float [] dst)
		{
			if (dst == null) {
				throw new ArgumentNullException (nameof (dst));
			}
			if (dst.Length != 16) {
				throw new ArgumentException ("The destination array must be 16 entries.", nameof (dst));
			}
			fixed (float* d = dst) {
				SkiaApi.sk_matrix44_as_row_major (Handle, d);
			}
		}

		public void SetColumnMajor (float [] src)
		{
			if (src == null) {
				throw new ArgumentNullException (nameof (src));
			}
			if (src.Length != 16) {
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));
			}
			fixed (float* s = src) {
				SkiaApi.sk_matrix44_set_col_major (Handle, s);
			}
		}

		public void SetRowMajor (float [] src)
		{
			if (src == null) {
				throw new ArgumentNullException (nameof (src));
			}
			if (src.Length != 16) {
				throw new ArgumentException ("The source array must be 16 entries.", nameof (src));
			}
			fixed (float* s = src) {
				SkiaApi.sk_matrix44_set_row_major (Handle, s);
			}
		}

		public void SetTranslate (float dx, float dy, float dz)
		{
			SkiaApi.sk_matrix44_set_translate (Handle, dx, dy, dz);
		}

		public void PreTranslate (float dx, float dy, float dz)
		{
			SkiaApi.sk_matrix44_pre_translate (Handle, dx, dy, dz);
		}

		public void PostTranslate (float dx, float dy, float dz)
		{
			SkiaApi.sk_matrix44_post_translate (Handle, dx, dy, dz);
		}

		public void SetScale (float sx, float sy, float sz)
		{
			SkiaApi.sk_matrix44_set_scale (Handle, sx, sy, sz);
		}

		public void PreScale (float sx, float sy, float sz)
		{
			SkiaApi.sk_matrix44_pre_scale (Handle, sx, sy, sz);
		}

		public void PostScale (float sx, float sy, float sz)
		{
			SkiaApi.sk_matrix44_post_scale (Handle, sx, sy, sz);
		}

		public void SetRotationAboutDegrees (float x, float y, float z, float degrees)
		{
			SkiaApi.sk_matrix44_set_rotate_about_degrees (Handle, x, y, z, degrees);
		}

		public void SetRotationAbout (float x, float y, float z, float radians)
		{
			SkiaApi.sk_matrix44_set_rotate_about_radians (Handle, x, y, z, radians);
		}

		public void SetRotationAboutUnit (float x, float y, float z, float radians)
		{
			SkiaApi.sk_matrix44_set_rotate_about_radians_unit (Handle, x, y, z, radians);
		}

		public void SetConcat (SKMatrix44 a, SKMatrix44 b)
		{
			if (a == null) {
				throw new ArgumentNullException (nameof (a));
			}
			if (b == null) {
				throw new ArgumentNullException (nameof (b));
			}

			SkiaApi.sk_matrix44_set_concat (Handle, a.Handle, b.Handle);
		}

		public void PreConcat (SKMatrix44 m)
		{
			if (m == null) {
				throw new ArgumentNullException (nameof (m));
			}

			SkiaApi.sk_matrix44_pre_concat (Handle, m.Handle);
		}

		public void PostConcat (SKMatrix44 m)
		{
			if (m == null) {
				throw new ArgumentNullException (nameof (m));
			}

			SkiaApi.sk_matrix44_post_concat (Handle, m.Handle);
		}

		public SKMatrix44 Invert ()
		{
			var inverse = new SKMatrix44 ();
			if (Invert (inverse)) {
				return inverse;
			} else {
				inverse.Dispose ();
				return null;
			}
		}

		public bool Invert (SKMatrix44 inverse)
		{
			if (inverse == null) {
				throw new ArgumentNullException (nameof (inverse));
			}

			return SkiaApi.sk_matrix44_invert (Handle, inverse.Handle);
		}

		public void Transpose ()
		{
			SkiaApi.sk_matrix44_transpose (Handle);
		}

		public float [] MapScalars (float x, float y, float z, float w)
		{
			var srcVector4 = new float [4] { x, y, z, w };
			var dstVector4 = new float [4];
			MapScalars (srcVector4, dstVector4);
			return dstVector4;
		}

		public float [] MapScalars (float [] srcVector4)
		{
			var dstVector4 = new float [4];
			MapScalars (srcVector4, dstVector4);
			return dstVector4;
		}

		public void MapScalars (float [] srcVector4, float [] dstVector4)
		{
			if (srcVector4 == null) {
				throw new ArgumentNullException (nameof (srcVector4));
			}
			if (srcVector4.Length != 4) {
				throw new ArgumentException ("The source vector array must be 4 entries.", nameof (srcVector4));
			}
			if (dstVector4 == null) {
				throw new ArgumentNullException (nameof (dstVector4));
			}
			if (dstVector4.Length != 4) {
				throw new ArgumentException ("The destination vector array must be 4 entries.", nameof (dstVector4));
			}

			fixed (float* s = srcVector4)
			fixed (float* d = dstVector4) {
				SkiaApi.sk_matrix44_map_scalars (Handle, s, d);
			}
		}

		public SKPoint MapPoint (SKPoint src)
		{
			return MapPoints (new [] { src }) [0];
		}

		public SKPoint [] MapPoints (SKPoint[] src)
		{
			if (src == null) {
				throw new ArgumentNullException (nameof (src));
			}
			
			var count = src.Length;
			var src2Length = count * 2;
			//var src4Length = count * 4;

			var src2 = new float [src2Length];
			for (int i = 0, i2 = 0; i < count; i++, i2 += 2) {
				src2 [i2] = src [i].X;
				src2 [i2 + 1] = src [i].Y;
			}

			var dst4 = MapVector2 (src2);

			var dst = new SKPoint [count];
			for (int i = 0, i4 = 0; i < count; i++, i4 += 4) {
				dst [i].X = dst4 [i4];
				dst [i].Y = dst4 [i4 + 1];
			}

			return dst;
		}
		
		public float [] MapVector2 (float [] src2)
		{
			if (src2 == null) {
				throw new ArgumentNullException (nameof (src2));
			}
			if (src2.Length % 2 != 0) {
				throw new ArgumentException ("The source vector array must be a set of pairs.", nameof (src2));
			}

			var dst4 = new float [src2.Length * 2];
			MapVector2 (src2, dst4);
			return dst4;
		}

		public void MapVector2 (float [] src2, float [] dst4)
		{
			if (src2 == null) {
				throw new ArgumentNullException (nameof (src2));
			}
			if (src2.Length % 2 != 0) {
				throw new ArgumentException ("The source vector array must be a set of pairs.", nameof (src2));
			}
			if (dst4 == null) {
				throw new ArgumentNullException (nameof (dst4));
			}
			if (dst4.Length % 4 != 0) {
				throw new ArgumentException ("The destination vector array must be a set quads.", nameof (dst4));
			}
			if (src2.Length / 2 != dst4.Length / 4) {
				throw new ArgumentException ("The source vector array must have the same number of pairs as the destination vector array has quads.", nameof (dst4));
			}

			fixed (float* s = src2)
			fixed (float* d = dst4) {
				SkiaApi.sk_matrix44_map2 (Handle, s, src2.Length / 2, d);
			}
		}

		public bool Preserves2DAxisAlignment (float epsilon)
		{
			return SkiaApi.sk_matrix44_preserves_2d_axis_alignment (Handle, epsilon);
		}

		public double Determinant ()
		{
			return SkiaApi.sk_matrix44_determinant (Handle);
		}
	}

	public unsafe partial struct SKRotationScaleMatrix
	{
		public static readonly SKRotationScaleMatrix Empty;

		public SKRotationScaleMatrix (float scos, float ssin, float tx, float ty)
		{
			fSCos = scos;
			fSSin = ssin;
			fTX = tx;
			fTY = ty;
		}

		public SKMatrix ToMatrix () =>
			new SKMatrix (fSCos, -fSSin, fTX, fSSin, fSCos, fTY, 0, 0, 1);

		public static SKRotationScaleMatrix FromDegrees (float scale, float degrees, float tx, float ty, float anchorX, float anchorY) =>
			FromRadians (scale, degrees * SKMatrix.DegreesToRadians, tx, ty, anchorX, anchorY);

		public static SKRotationScaleMatrix FromRadians (float scale, float radians, float tx, float ty, float anchorX, float anchorY)
		{
			var s = (float)Math.Sin (radians) * scale;
			var c = (float)Math.Cos (radians) * scale;
			var x = tx + -c * anchorX + s * anchorY;
			var y = ty + -s * anchorX - c * anchorY;

			return new SKRotationScaleMatrix (c, s, x, y);
		}

		public static SKRotationScaleMatrix CreateIdentity () =>
			new SKRotationScaleMatrix (1, 0, 0, 0);

		public static SKRotationScaleMatrix CreateTranslate (float x, float y) =>
			new SKRotationScaleMatrix (1, 0, x, y);

		public static SKRotationScaleMatrix CreateScale (float s) =>
			new SKRotationScaleMatrix (s, 0, 0, 0);

		public static SKRotationScaleMatrix CreateRotation (float radians, float anchorX, float anchorY) =>
			FromRadians (1, radians, 0, 0, anchorX, anchorY);

		public static SKRotationScaleMatrix CreateRotationDegrees (float degrees, float anchorX, float anchorY) =>
			FromDegrees (1, degrees, 0, 0, anchorX, anchorY);
	}
}
