//
// Bindings for GRContext
//
// Author:
//   Matthew Leibowitz
//
// SkMatrix could benefit from bringing some of the operators defined in C++
//
// Copyright 2017 Xamarin Inc
//

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	[StructLayout(LayoutKind.Sequential)]
	public struct SKMatrix {
		private float scaleX, skewX, transX;
		private float skewY, scaleY, transY;
		private float persp0, persp1, persp2;

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

		public float ScaleX {
			get { return scaleX; }
			set { scaleX = value; }
		}

		public float SkewX {
			get { return skewX; }
			set { skewX = value; }
		}

		public float TransX {
			get { return transX; }
			set { transX = value; }
		}

		public float SkewY {
			get { return skewY; }
			set { skewY = value; }
		}

		public float ScaleY {
			get { return scaleY; }
			set { scaleY = value; }
		}

		public float TransY {
			get { return transY; }
			set { transY = value; }
		}

		public float Persp0 {
			get { return persp0; }
			set { persp0 = value; }
		}

		public float Persp1 {
			get { return persp1; }
			set { persp1 = value; }
		}

		public float Persp2 {
			get { return persp2; }
			set { persp2 = value; }
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

		const float degToRad = (float)System.Math.PI / 180.0f;
		
		public static SKMatrix MakeRotationDegrees (float degrees)
		{
			return MakeRotation (degrees * degToRad);
		}

		public static SKMatrix MakeRotationDegrees (float degrees, float pivotx, float pivoty)
		{
			return MakeRotation (degrees * degToRad, pivotx, pivoty);
		}

		static void SetSinCos (ref SKMatrix matrix, float sin, float cos)
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

		static void SetSinCos (ref SKMatrix matrix, float sin, float cos, float pivotx, float pivoty)
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
			var sin = (float) Math.Sin (degrees * degToRad);
			var cos = (float) Math.Cos (degrees * degToRad);
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
			var sin = (float) Math.Sin (degrees * degToRad);
			var cos = (float) Math.Cos (degrees * degToRad);
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
			return SkiaApi.sk_matrix_try_invert (ref this, out inverse) != 0;
		}

		public static void Concat (ref SKMatrix target, SKMatrix first, SKMatrix second)
		{
			SkiaApi.sk_matrix_concat (ref target, ref first, ref second);
		}

		public static void Concat (ref SKMatrix target, ref SKMatrix first, ref SKMatrix second)
		{
			SkiaApi.sk_matrix_concat (ref target, ref first, ref second);
		}

		public static void PreConcat (ref SKMatrix target, SKMatrix matrix)
		{
			SkiaApi.sk_matrix_pre_concat (ref target, ref matrix);
		}

		public static void PreConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			SkiaApi.sk_matrix_pre_concat (ref target, ref matrix);
		}

		public static void PostConcat (ref SKMatrix target, SKMatrix matrix)
		{
			SkiaApi.sk_matrix_post_concat (ref target, ref matrix);
		}

		public static void PostConcat (ref SKMatrix target, ref SKMatrix matrix)
		{
			SkiaApi.sk_matrix_post_concat (ref target, ref matrix);
		}

		public static void MapRect (ref SKMatrix matrix, out SKRect dest, ref SKRect source)
		{
			SkiaApi.sk_matrix_map_rect (ref matrix, out dest, ref source);
		}

		public SKRect MapRect (SKRect source)
		{
			SKRect result;
			MapRect (ref this, out result, ref source);
			return result;
		}

		public void MapPoints (SKPoint [] result, SKPoint [] points)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			int dl = result.Length;
			if (dl != points.Length)
				throw new ArgumentException ("Buffers must be the same size.");
			unsafe {
				fixed (SKPoint *rp = &result[0]){
					fixed (SKPoint *pp = &points[0]){
						SkiaApi.sk_matrix_map_points (ref this, (IntPtr) rp, (IntPtr) pp, dl);
					}
				}
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
			int dl = result.Length;
			if (dl != vectors.Length)
				throw new ArgumentException ("Buffers must be the same size.");
			unsafe {
				fixed (SKPoint *rp = &result[0]){
					fixed (SKPoint *pp = &vectors[0]){
						SkiaApi.sk_matrix_map_vectors (ref this, (IntPtr) rp, (IntPtr) pp, dl);
					}
				}
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

		[Obsolete ("Use MapPoint instead.")]
		public SKPoint MapXY (float x, float y)
		{
			return MapPoint (x, y);
		}

		public SKPoint MapPoint (SKPoint point)
		{
			return MapPoint (point.X, point.Y);
		}

		public SKPoint MapPoint (float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_xy (ref this, x, y, out result);
			return result;
		}

		public SKPoint MapVector (float x, float y)
		{
			SKPoint result;
			SkiaApi.sk_matrix_map_vector(ref this, x, y, out result);
			return result;
		}

		public float MapRadius (float radius)
		{
			return SkiaApi.sk_matrix_map_radius (ref this, radius);
		}
	}

	public class SK3dView : SKObject
	{
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_3dview_destroy (Handle);
			}

			base.Dispose (disposing);
		}

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
		
		public SKMatrix Matrix {
			get {
				SKMatrix matrix = SKMatrix.MakeIdentity ();
				GetMatrix (ref matrix);
				return matrix;
			}
		}
		
		public void GetMatrix (ref SKMatrix matrix)
		{
			SkiaApi.sk_3dview_get_matrix (Handle, ref matrix);
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

		public void DotWithNormal (float dx, float dy, float dz)
		{
			SkiaApi.sk_3dview_dot_with_normal (Handle, dx, dy, dz);
		}
	}
}
