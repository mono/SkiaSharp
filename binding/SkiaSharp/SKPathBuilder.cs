#nullable disable

using System;

namespace SkiaSharp
{
	public unsafe class SKPathBuilder : SKObject, ISKSkipObjectRegistration
	{
		internal SKPathBuilder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKPathBuilder ()
			: this (SkiaApi.sk_pathbuilder_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathBuilder instance.");
			}
		}

		public SKPathBuilder (SKPath path)
			: this (SkiaApi.sk_pathbuilder_new_from_path (path?.Handle ?? throw new ArgumentNullException (nameof (path))), true)
		{
			GC.KeepAlive (path);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathBuilder instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_pathbuilder_delete (Handle);

		public SKPathFillType FillType {
			get {
				var r = SkiaApi.sk_pathbuilder_get_filltype (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_pathbuilder_set_filltype (Handle, value);
				GC.KeepAlive (this);
			}
		}

		public SKPath Detach ()
		{
			var r = SKPath.GetObject (SkiaApi.sk_pathbuilder_detach_path (Handle));
			GC.KeepAlive (this);
			return r;
		}

		public SKPath Snapshot ()
		{
			var r = SKPath.GetObject (SkiaApi.sk_pathbuilder_snapshot_path (Handle));
			GC.KeepAlive (this);
			return r;
		}

		public void Reset ()
		{
			SkiaApi.sk_pathbuilder_reset (Handle);
			GC.KeepAlive (this);
		}

		// Move

		public void MoveTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_move_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_pathbuilder_move_to (Handle, x, y);
			GC.KeepAlive (this);
		}

		public void RMoveTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_rmove_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		public void RMoveTo (float dx, float dy)
		{
			SkiaApi.sk_pathbuilder_rmove_to (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		// Line

		public void LineTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_line_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		public void LineTo (float x, float y)
		{
			SkiaApi.sk_pathbuilder_line_to (Handle, x, y);
			GC.KeepAlive (this);
		}

		public void RLineTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_rline_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		public void RLineTo (float dx, float dy)
		{
			SkiaApi.sk_pathbuilder_rline_to (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		// Quad

		public void QuadTo (SKPoint point0, SKPoint point1)
		{
			SkiaApi.sk_pathbuilder_quad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);
			GC.KeepAlive (this);
		}

		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_pathbuilder_quad_to (Handle, x0, y0, x1, y1);
			GC.KeepAlive (this);
		}

		public void RQuadTo (SKPoint point0, SKPoint point1)
		{
			SkiaApi.sk_pathbuilder_rquad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);
			GC.KeepAlive (this);
		}

		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			SkiaApi.sk_pathbuilder_rquad_to (Handle, dx0, dy0, dx1, dy1);
			GC.KeepAlive (this);
		}

		// Conic

		public void ConicTo (SKPoint point0, SKPoint point1, float w)
		{
			SkiaApi.sk_pathbuilder_conic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);
			GC.KeepAlive (this);
		}

		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_pathbuilder_conic_to (Handle, x0, y0, x1, y1, w);
			GC.KeepAlive (this);
		}

		public void RConicTo (SKPoint point0, SKPoint point1, float w)
		{
			SkiaApi.sk_pathbuilder_rconic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);
			GC.KeepAlive (this);
		}

		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			SkiaApi.sk_pathbuilder_rconic_to (Handle, dx0, dy0, dx1, dy1, w);
			GC.KeepAlive (this);
		}

		// Cubic

		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			SkiaApi.sk_pathbuilder_cubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
			GC.KeepAlive (this);
		}

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_pathbuilder_cubic_to (Handle, x0, y0, x1, y1, x2, y2);
			GC.KeepAlive (this);
		}

		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
			GC.KeepAlive (this);
		}

		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);
			GC.KeepAlive (this);
		}

		// Arc

		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			SkiaApi.sk_pathbuilder_arc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
			GC.KeepAlive (this);
		}

		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_pathbuilder_arc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			GC.KeepAlive (this);
		}

		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_oval (Handle, &oval, startAngle, sweepAngle, forceMoveTo);
			GC.KeepAlive (this);
		}

		public void ArcTo (SKPoint point1, SKPoint point2, float radius)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, point1.X, point1.Y, point2.X, point2.Y, radius);
			GC.KeepAlive (this);
		}

		public void ArcTo (float x1, float y1, float x2, float y2, float radius)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, x1, y1, x2, y2, radius);
			GC.KeepAlive (this);
		}

		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			SkiaApi.sk_pathbuilder_rarc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
			GC.KeepAlive (this);
		}

		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_pathbuilder_rarc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			GC.KeepAlive (this);
		}

		// Close

		public void Close ()
		{
			SkiaApi.sk_pathbuilder_close (Handle);
			GC.KeepAlive (this);
		}

		// Add shapes

		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_rect (Handle, &rect, direction);
			GC.KeepAlive (this);
		}

		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException (nameof (startIndex), "Starting index must be in the range of 0..3 (inclusive).");

			SkiaApi.sk_pathbuilder_add_rect_start (Handle, &rect, direction, startIndex);
			GC.KeepAlive (this);
		}

		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect (Handle, rect.Handle, direction);
			GC.KeepAlive (rect);
			GC.KeepAlive (this);
		}

		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect_start (Handle, rect.Handle, direction, startIndex);
			GC.KeepAlive (rect);
			GC.KeepAlive (this);
		}

		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_oval (Handle, &rect, direction);
			GC.KeepAlive (this);
		}

		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			SkiaApi.sk_pathbuilder_add_arc (Handle, &oval, startAngle, sweepAngle);
			GC.KeepAlive (this);
		}

		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_rounded_rect (Handle, &rect, rx, ry, dir);
			GC.KeepAlive (this);
		}

		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_circle (Handle, x, y, radius, dir);
			GC.KeepAlive (this);
		}

		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			fixed (SKPoint* p = points) {
				SkiaApi.sk_pathbuilder_add_poly (Handle, p, points.Length, close);
				GC.KeepAlive (this);
			}
		}

		public void AddPoly (SKPoint[] points, bool close = true)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_pathbuilder_add_poly (Handle, p, points.Length, close);
				GC.KeepAlive (this);
			}
		}

		// Add path

		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path_offset (Handle, other.Handle, dx, dy, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_pathbuilder_add_path_matrix (Handle, other.Handle, m, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path (Handle, other.Handle, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		public void ReverseAddPath (SKPath other)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_reverse_add_path (Handle, other.Handle);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}
	}
}
