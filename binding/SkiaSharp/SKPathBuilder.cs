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
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathBuilder instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_pathbuilder_delete (Handle);

		public SKPathFillType FillType {
			get => SkiaApi.sk_pathbuilder_get_filltype (Handle);
			set => SkiaApi.sk_pathbuilder_set_filltype (Handle, value);
		}

		public SKPath Detach () =>
			SKPath.GetObject (SkiaApi.sk_pathbuilder_detach_path (Handle));

		public SKPath Snapshot () =>
			SKPath.GetObject (SkiaApi.sk_pathbuilder_snapshot_path (Handle));

		public void Reset () =>
			SkiaApi.sk_pathbuilder_reset (Handle);

		// Move

		public void MoveTo (SKPoint point) =>
			SkiaApi.sk_pathbuilder_move_to (Handle, point.X, point.Y);

		public void MoveTo (float x, float y) =>
			SkiaApi.sk_pathbuilder_move_to (Handle, x, y);

		public void RMoveTo (SKPoint point) =>
			SkiaApi.sk_pathbuilder_rmove_to (Handle, point.X, point.Y);

		public void RMoveTo (float dx, float dy) =>
			SkiaApi.sk_pathbuilder_rmove_to (Handle, dx, dy);

		// Line

		public void LineTo (SKPoint point) =>
			SkiaApi.sk_pathbuilder_line_to (Handle, point.X, point.Y);

		public void LineTo (float x, float y) =>
			SkiaApi.sk_pathbuilder_line_to (Handle, x, y);

		public void RLineTo (SKPoint point) =>
			SkiaApi.sk_pathbuilder_rline_to (Handle, point.X, point.Y);

		public void RLineTo (float dx, float dy) =>
			SkiaApi.sk_pathbuilder_rline_to (Handle, dx, dy);

		// Quad

		public void QuadTo (SKPoint point0, SKPoint point1) =>
			SkiaApi.sk_pathbuilder_quad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);

		public void QuadTo (float x0, float y0, float x1, float y1) =>
			SkiaApi.sk_pathbuilder_quad_to (Handle, x0, y0, x1, y1);

		public void RQuadTo (SKPoint point0, SKPoint point1) =>
			SkiaApi.sk_pathbuilder_rquad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);

		public void RQuadTo (float dx0, float dy0, float dx1, float dy1) =>
			SkiaApi.sk_pathbuilder_rquad_to (Handle, dx0, dy0, dx1, dy1);

		// Conic

		public void ConicTo (SKPoint point0, SKPoint point1, float w) =>
			SkiaApi.sk_pathbuilder_conic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);

		public void ConicTo (float x0, float y0, float x1, float y1, float w) =>
			SkiaApi.sk_pathbuilder_conic_to (Handle, x0, y0, x1, y1, w);

		public void RConicTo (SKPoint point0, SKPoint point1, float w) =>
			SkiaApi.sk_pathbuilder_rconic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);

		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w) =>
			SkiaApi.sk_pathbuilder_rconic_to (Handle, dx0, dy0, dx1, dy1, w);

		// Cubic

		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2) =>
			SkiaApi.sk_pathbuilder_cubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2) =>
			SkiaApi.sk_pathbuilder_cubic_to (Handle, x0, y0, x1, y1, x2, y2);

		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2) =>
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);

		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2) =>
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);

		// Arc

		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy) =>
			SkiaApi.sk_pathbuilder_arc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);

		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y) =>
			SkiaApi.sk_pathbuilder_arc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);

		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo) =>
			SkiaApi.sk_pathbuilder_arc_to_with_oval (Handle, &oval, startAngle, sweepAngle, forceMoveTo);

		public void ArcTo (SKPoint point1, SKPoint point2, float radius) =>
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, point1.X, point1.Y, point2.X, point2.Y, radius);

		public void ArcTo (float x1, float y1, float x2, float y2, float radius) =>
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, x1, y1, x2, y2, radius);

		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy) =>
			SkiaApi.sk_pathbuilder_rarc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);

		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y) =>
			SkiaApi.sk_pathbuilder_rarc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);

		// Close

		public void Close () =>
			SkiaApi.sk_pathbuilder_close (Handle);

		// Add shapes

		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise) =>
			SkiaApi.sk_pathbuilder_add_rect (Handle, &rect, direction);

		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException (nameof (startIndex), "Starting index must be in the range of 0..3 (inclusive).");

			SkiaApi.sk_pathbuilder_add_rect_start (Handle, &rect, direction, startIndex);
		}

		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect (Handle, rect.Handle, direction);
		}

		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect_start (Handle, rect.Handle, direction, startIndex);
		}

		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise) =>
			SkiaApi.sk_pathbuilder_add_oval (Handle, &rect, direction);

		public void AddArc (SKRect oval, float startAngle, float sweepAngle) =>
			SkiaApi.sk_pathbuilder_add_arc (Handle, &oval, startAngle, sweepAngle);

		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise) =>
			SkiaApi.sk_pathbuilder_add_rounded_rect (Handle, &rect, rx, ry, dir);

		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise) =>
			SkiaApi.sk_pathbuilder_add_circle (Handle, x, y, radius, dir);

		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_pathbuilder_add_poly (Handle, p, points.Length, close);
			}
		}

		public void AddPoly (SKPoint[] points, bool close = true)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_pathbuilder_add_poly (Handle, p, points.Length, close);
			}
		}

		// Add path

		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path_offset (Handle, other.Handle, dx, dy, mode);
		}

		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_pathbuilder_add_path_matrix (Handle, other.Handle, m, mode);
		}

		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path (Handle, other.Handle, mode);
		}

		public void ReverseAddPath (SKPath other)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_reverse_add_path (Handle, other.Handle);
		}
	}
}
