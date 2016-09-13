//
// Bindings for SKPath
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKPath : SKObject
	{
		public enum Verb {
			Move, Line, Quad, Conic, Cubic, Close, Done
		}

		public enum AddMode {
			Append,
			Extend
		}

		
		[Preserve]
		internal SKPath (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPath ()
			: this (SkiaApi.sk_path_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPath instance.");
			}
		}

		public SKPath(SKPath path)
			: this (SkiaApi.sk_path_clone(path.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKPath instance.");
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_path_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public SKPathFillType FillType {
			get {
				return SkiaApi.sk_path_get_filltype (Handle);
			}
			set {
				SkiaApi.sk_path_set_filltype (Handle, value);
			}
		}

		public int PointCount {
			get {
				return SkiaApi.sk_path_count_points (Handle);
			}
		}

		public SKPoint this [int index] {
			get {
				return GetPoint (index);
			}
		}

		public SKPoint[] Points {
			get {
				return GetPoints (PointCount);
			}
		}

		public SKPoint LastPoint
		{
			get {
				SKPoint point;
				SkiaApi.sk_path_get_last_point (Handle, out point);
				return point;
			}
		}

		public SKPoint GetPoint (int index)
		{
			SKPoint point;
			SkiaApi.sk_path_get_point (Handle, index, out point);
			return point;
		}

		public SKPoint[] GetPoints (int max)
		{
			SKPoint[] points = new SKPoint [max];
			GetPoints (points, max);
			return points;
		}

		public int GetPoints (SKPoint[] points, int max)
		{
			return SkiaApi.sk_path_get_points (Handle, points, max);
		}

		public bool Contains (float x, float y)
		{
			return SkiaApi.sk_path_contains (Handle, x, y);
		}

		public void Offset (float dx, float dy)
		{
			Transform (SKMatrix.MakeTranslation(dx, dy));
		}

		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_path_move_to (Handle, x, y);
		}

		public void RMoveTo (float dx, float dy)
		{
			SkiaApi.sk_path_rmove_to (Handle, dx, dy);
		}

		public void LineTo (float x, float y)
		{
			SkiaApi.sk_path_line_to (Handle, x, y);
		}

		public void RLineTo (float dx, float dy)
		{
			SkiaApi.sk_path_rline_to (Handle, dx, dy);
		}

		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_path_quad_to (Handle, x0, y0, x1, y1);
		}

		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			SkiaApi.sk_path_rquad_to (Handle, dx0, dy0, dx1, dy1);
		}

		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_path_conic_to (Handle, x0, y0, x1, y1, w);
		}

		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			SkiaApi.sk_path_rconic_to (Handle, dx0, dy0, dx1, dy1, w);
		}

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_path_cubic_to (Handle, x0, y0, x1, y1, x2, y2);
		}

		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			SkiaApi.sk_path_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);
		}

		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_path_arc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_path_rarc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		public void Close ()
		{
			SkiaApi.sk_path_close (Handle);
		}

		public void Rewind()
		{
			SkiaApi.sk_path_rewind(Handle);
		}

		public void Reset()
		{
			SkiaApi.sk_path_reset(Handle);
		}

		public void AddRect (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_rect (Handle, ref rect, direction);
		}

		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException (nameof (startIndex), "startIndex must be 0 - 3");

			SkiaApi.sk_path_add_rect_start (Handle, ref rect, direction, startIndex);
		}

		public void AddOval (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_oval (Handle, ref rect, direction);
		}

		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			SkiaApi.sk_path_add_arc (Handle, ref oval, startAngle, sweepAngle);
		}

		public bool GetBounds (out SKRect rect)
		{
			return SkiaApi.sk_path_get_bounds (Handle, out rect);
		}

		public void Transform (SKMatrix matrix)
		{
			SkiaApi.sk_path_transform (Handle, ref matrix);
		}

		public void AddPath (SKPath other, float dx, float dy, AddMode mode = AddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			
			SkiaApi.sk_path_add_path_offset (Handle, other.Handle, dx, dy, mode);
		}

		public void AddPath (SKPath other, ref SKMatrix matrix, AddMode mode = AddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			
			SkiaApi.sk_path_add_path_matrix (Handle, other.Handle, ref matrix, mode);
		}

		public void AddPath (SKPath other, AddMode mode = AddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			
			SkiaApi.sk_path_add_path (Handle, other.Handle, mode);
		}

		public void AddPathReverse (SKPath other)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			
			SkiaApi.sk_path_add_path_reverse (Handle, other.Handle);
		}

		public void AddRoundedRect (SKRect rect, float rx, float ry, SKPathDirection dir)
		{
			SkiaApi.sk_path_add_rounded_rect (Handle, ref rect, rx, ry, dir);
		}

		public void AddCircle (float x, float y, float radius, SKPathDirection dir)
		{
			SkiaApi.sk_path_add_circle (Handle, x, y, radius, dir);
		}

		public Iterator CreateIterator (bool forceClose)
		{
			return new Iterator (this, forceClose);
		}

		public RawIterator CreateRawIterator ()
		{
			return new RawIterator (this);
		}
		
		public bool Op (SKPath other, SKPathOp op, SKPath result)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_op (Handle, other.Handle, op, result.Handle);
		}
		
		public bool Simplify (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_simplify (Handle, result.Handle);
		}
		
		public bool GetTightBounds (out SKRect result)
		{
			return SkiaApi.sk_pathop_tight_bounds (Handle, out result);
		}
		
		public class Iterator : SKNativeObject
		{
			SKPath Path => path;
			SKPath path;

			internal Iterator (SKPath path, bool forceClose)
				: base (SkiaApi.sk_path_create_iter (path.Handle, forceClose ? 1 : 0))
			{
				this.path = path;
			}
			
			protected override void Dispose (bool disposing)
			{
				if (Handle != IntPtr.Zero){
					// safe to call from a background thread to release resources.
					SkiaApi.sk_path_iter_destroy (Handle);
				}

				base.Dispose (disposing);
			}
			
			public Verb Next (SKPoint [] points, bool doConsumeDegenerates = true, bool exact = false)
			{
				if (points == null)
					throw new ArgumentNullException (nameof (points));
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements", nameof (points));
				return SkiaApi.sk_path_iter_next (Handle, points, doConsumeDegenerates ? 1 : 0, exact ? 1 : 0);
			}

			public float ConicWeight () => SkiaApi.sk_path_iter_conic_weight (Handle);
			public bool IsCloseLine () => SkiaApi.sk_path_iter_is_close_line (Handle) != 0;
			public bool IsCloseContour () => SkiaApi.sk_path_iter_is_closed_contour (Handle) != 0;
		}

		public class RawIterator : SKNativeObject
		{
			SKPath Path => path;
			SKPath path;

			internal RawIterator (SKPath path)
				: base (SkiaApi.sk_path_create_rawiter (path.Handle))
			{
				this.path = path;
			}

			protected override void Dispose (bool disposing)
			{
				if (Handle != IntPtr.Zero){
					// safe to call from a background thread to release resources.
					SkiaApi.sk_path_rawiter_destroy (Handle);
				}

				base.Dispose (disposing);
			}
			
			public Verb Next (SKPoint [] points)
			{
				if (points == null)
					throw new ArgumentNullException (nameof (points));
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements", nameof (points));
				return SkiaApi.sk_path_rawiter_next (Handle, points);
			}

			public float ConicWeight () => SkiaApi.sk_path_rawiter_conic_weight (Handle);
			public Verb Peek () => SkiaApi.sk_path_rawiter_peek (Handle);
		}

		public class OpBuilder : SKNativeObject
		{
			public OpBuilder ()
				: base (SkiaApi.sk_opbuilder_new ())
			{
			}

			public void Add (SKPath path, SKPathOp op)
			{
				SkiaApi.sk_opbuilder_add (Handle, path.Handle, op);
			}

			public bool Resolve (SKPath result)
			{
				if (result == null)
					throw new ArgumentNullException (nameof (result));

				return SkiaApi.sk_opbuilder_resolve (Handle, result.Handle);
			}

			protected override void Dispose (bool disposing)
			{
				if (Handle != IntPtr.Zero) {
					SkiaApi.sk_opbuilder_destroy (Handle);
				}

				base.Dispose (disposing);
			}
		}
	}
}

