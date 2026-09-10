#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>A mutable builder for constructing <see cref="T:SkiaSharp.SKPath" /> objects incrementally.</summary>
	/// <remarks><![CDATA[
	/// ## Remarks
	///
	/// `SKPathBuilder` provides a mutable API for assembling path contours before producing an <xref:SkiaSharp.SKPath>. Unlike `SKPath`, which is a sealed, immutable representation, `SKPathBuilder` is designed for incremental construction.
	///
	/// Use <xref:SkiaSharp.SKPathBuilder.Snapshot> to obtain a read-only copy of the current path without resetting the builder, or <xref:SkiaSharp.SKPathBuilder.Detach> to transfer ownership of the path to the caller and reset the builder.
	///
	/// `SKPathBuilder` extends <xref:SkiaSharp.SKObject> and must be disposed when no longer needed.
	///
	/// ## Examples
	///
	/// Building a triangle path:
	///
	/// ```csharp
	/// using var builder = new SKPathBuilder();
	/// builder.MoveTo(0, 0);
	/// builder.LineTo(100, 0);
	/// builder.LineTo(50, 100);
	/// builder.Close();
	/// using var path = builder.Detach();
	/// ```
	/// ]]></remarks>
	public unsafe class SKPathBuilder : SKObject, ISKSkipObjectRegistration
	{
		internal SKPathBuilder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Initializes a new, empty instance of the <see cref="T:SkiaSharp.SKPathBuilder" /> class.</summary>
		/// <remarks />
		public SKPathBuilder ()
			: this (SkiaApi.sk_pathbuilder_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathBuilder instance.");
			}
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKPathBuilder" /> class with the contours of the specified path.</summary>
		/// <param name="path">The path whose contours are copied into this builder.</param>
		/// <remarks />
		public SKPathBuilder (SKPath path)
			: this (SkiaApi.sk_pathbuilder_new_from_path (path?.Handle ?? throw new ArgumentNullException (nameof (path))), true)
		{
			GC.KeepAlive (path);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPathBuilder instance.");
			}
		}

		/// <summary>Releases the resources used by this <see cref="T:SkiaSharp.SKPathBuilder" />.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and native resources; <see langword="false" /> to release only native resources.</param>
		/// <remarks />
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Releases the native resources held by this <see cref="T:SkiaSharp.SKPathBuilder" />.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_pathbuilder_delete (Handle);

		/// <summary>Gets or sets the fill type rule used when the path is converted to an <see cref="T:SkiaSharp.SKPath" />.</summary>
		/// <value>The <see cref="T:SkiaSharp.SKPathFillType" /> that determines how overlapping contours are filled.</value>
		/// <remarks />
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

		/// <summary>Returns the built path and resets this builder to an empty state.</summary>
		/// <returns>The <see cref="T:SkiaSharp.SKPath" /> built so far. The builder is reset to an empty state after the call.</returns>
		/// <remarks />
		public SKPath Detach ()
		{
			var r = SKPath.GetObject (SkiaApi.sk_pathbuilder_detach_path (Handle));
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Returns a snapshot of the current path without modifying this builder.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKPath" /> containing a snapshot of the current contours. The builder is not modified.</returns>
		/// <remarks />
		public SKPath Snapshot ()
		{
			var r = SKPath.GetObject (SkiaApi.sk_pathbuilder_snapshot_path (Handle));
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Resets this builder to an empty state, discarding all contours and resetting the fill type to the default.</summary>
		/// <remarks />
		public void Reset ()
		{
			SkiaApi.sk_pathbuilder_reset (Handle);
			GC.KeepAlive (this);
		}

		// Move

		/// <summary>Begins a new contour at the specified point.</summary>
		/// <param name="point">The starting point of the new contour.</param>
		/// <remarks />
		public void MoveTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_move_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Begins a new contour at the specified coordinates.</summary>
		/// <param name="x">The x-coordinate of the starting point.</param>
		/// <param name="y">The y-coordinate of the starting point.</param>
		/// <remarks />
		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_pathbuilder_move_to (Handle, x, y);
			GC.KeepAlive (this);
		}

		/// <summary>Begins a new contour at a point offset from the current point.</summary>
		/// <param name="point">The offset from the current point to the starting point of the new contour.</param>
		/// <remarks />
		public void RMoveTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_rmove_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Begins a new contour at a position offset from the current point by the specified coordinates.</summary>
		/// <param name="dx">The x-offset from the current point.</param>
		/// <param name="dy">The y-offset from the current point.</param>
		/// <remarks />
		public void RMoveTo (float dx, float dy)
		{
			SkiaApi.sk_pathbuilder_rmove_to (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		// Line

		/// <summary>Appends a straight line segment from the current point to the specified point.</summary>
		/// <param name="point">The end point of the line segment.</param>
		/// <remarks />
		public void LineTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_line_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a straight line segment from the current point to the specified coordinates.</summary>
		/// <param name="x">The x-coordinate of the end point.</param>
		/// <param name="y">The y-coordinate of the end point.</param>
		/// <remarks />
		public void LineTo (float x, float y)
		{
			SkiaApi.sk_pathbuilder_line_to (Handle, x, y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative straight line segment from the current point using the specified offset.</summary>
		/// <param name="point">The offset from the current point to the end point of the line segment.</param>
		/// <remarks />
		public void RLineTo (SKPoint point)
		{
			SkiaApi.sk_pathbuilder_rline_to (Handle, point.X, point.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative straight line segment from the current point using the specified offset coordinates.</summary>
		/// <param name="dx">The x-offset from the current point.</param>
		/// <param name="dy">The y-offset from the current point.</param>
		/// <remarks />
		public void RLineTo (float dx, float dy)
		{
			SkiaApi.sk_pathbuilder_rline_to (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		// Quad

		/// <summary>Appends a quadratic Bézier curve to the current contour.</summary>
		/// <param name="point0">The control point of the quadratic Bézier.</param>
		/// <param name="point1">The end point of the quadratic Bézier.</param>
		/// <remarks />
		public void QuadTo (SKPoint point0, SKPoint point1)
		{
			SkiaApi.sk_pathbuilder_quad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a quadratic Bézier curve to the current contour using coordinate values.</summary>
		/// <param name="x0">The x-coordinate of the control point.</param>
		/// <param name="y0">The y-coordinate of the control point.</param>
		/// <param name="x1">The x-coordinate of the end point.</param>
		/// <param name="y1">The y-coordinate of the end point.</param>
		/// <remarks />
		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_pathbuilder_quad_to (Handle, x0, y0, x1, y1);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative quadratic Bézier curve to the current contour.</summary>
		/// <param name="point0">The offset from the current point to the control point.</param>
		/// <param name="point1">The offset from the current point to the end point.</param>
		/// <remarks />
		public void RQuadTo (SKPoint point0, SKPoint point1)
		{
			SkiaApi.sk_pathbuilder_rquad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative quadratic Bézier curve to the current contour using offset coordinates.</summary>
		/// <param name="dx0">The x-offset from the current point to the control point.</param>
		/// <param name="dy0">The y-offset from the current point to the control point.</param>
		/// <param name="dx1">The x-offset from the current point to the end point.</param>
		/// <param name="dy1">The y-offset from the current point to the end point.</param>
		/// <remarks />
		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			SkiaApi.sk_pathbuilder_rquad_to (Handle, dx0, dy0, dx1, dy1);
			GC.KeepAlive (this);
		}

		// Conic

		/// <summary>Appends a weighted conic curve to the current contour.</summary>
		/// <param name="point0">The control point of the conic.</param>
		/// <param name="point1">The end point of the conic.</param>
		/// <param name="w">The weight of the conic; values greater than 1 pull the curve toward the control point.</param>
		/// <remarks />
		public void ConicTo (SKPoint point0, SKPoint point1, float w)
		{
			SkiaApi.sk_pathbuilder_conic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a weighted conic curve to the current contour using coordinate values.</summary>
		/// <param name="x0">The x-coordinate of the control point.</param>
		/// <param name="y0">The y-coordinate of the control point.</param>
		/// <param name="x1">The x-coordinate of the end point.</param>
		/// <param name="y1">The y-coordinate of the end point.</param>
		/// <param name="w">The weight of the conic.</param>
		/// <remarks />
		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_pathbuilder_conic_to (Handle, x0, y0, x1, y1, w);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative weighted conic curve to the current contour.</summary>
		/// <param name="point0">The offset from the current point to the control point.</param>
		/// <param name="point1">The offset from the current point to the end point.</param>
		/// <param name="w">The weight of the conic.</param>
		/// <remarks />
		public void RConicTo (SKPoint point0, SKPoint point1, float w)
		{
			SkiaApi.sk_pathbuilder_rconic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative weighted conic curve to the current contour using offset coordinates.</summary>
		/// <param name="dx0">The x-offset from the current point to the control point.</param>
		/// <param name="dy0">The y-offset from the current point to the control point.</param>
		/// <param name="dx1">The x-offset from the current point to the end point.</param>
		/// <param name="dy1">The y-offset from the current point to the end point.</param>
		/// <param name="w">The weight of the conic.</param>
		/// <remarks />
		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			SkiaApi.sk_pathbuilder_rconic_to (Handle, dx0, dy0, dx1, dy1, w);
			GC.KeepAlive (this);
		}

		// Cubic

		/// <summary>Appends a cubic Bézier curve to the current contour.</summary>
		/// <param name="point0">The first control point of the cubic Bézier.</param>
		/// <param name="point1">The second control point of the cubic Bézier.</param>
		/// <param name="point2">The end point of the cubic Bézier.</param>
		/// <remarks />
		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			SkiaApi.sk_pathbuilder_cubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a cubic Bézier curve to the current contour using coordinate values.</summary>
		/// <param name="x0">The x-coordinate of the first control point.</param>
		/// <param name="y0">The y-coordinate of the first control point.</param>
		/// <param name="x1">The x-coordinate of the second control point.</param>
		/// <param name="y1">The y-coordinate of the second control point.</param>
		/// <param name="x2">The x-coordinate of the end point.</param>
		/// <param name="y2">The y-coordinate of the end point.</param>
		/// <remarks />
		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_pathbuilder_cubic_to (Handle, x0, y0, x1, y1, x2, y2);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative cubic Bézier curve to the current contour.</summary>
		/// <param name="point0">The offset from the current point to the first control point.</param>
		/// <param name="point1">The offset from the current point to the second control point.</param>
		/// <param name="point2">The offset from the current point to the end point.</param>
		/// <remarks />
		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative cubic Bézier curve to the current contour using offset coordinates.</summary>
		/// <param name="dx0">The x-offset from the current point to the first control point.</param>
		/// <param name="dy0">The y-offset from the current point to the first control point.</param>
		/// <param name="dx1">The x-offset from the current point to the second control point.</param>
		/// <param name="dy1">The y-offset from the current point to the second control point.</param>
		/// <param name="dx2">The x-offset from the current point to the end point.</param>
		/// <param name="dy2">The y-offset from the current point to the end point.</param>
		/// <remarks />
		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			SkiaApi.sk_pathbuilder_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);
			GC.KeepAlive (this);
		}

		// Arc

		/// <summary>Appends an SVG-style elliptical arc to the current contour.</summary>
		/// <param name="r">The radii of the ellipse (x-radius, y-radius).</param>
		/// <param name="xAxisRotate">The rotation of the ellipse's x-axis relative to the current coordinate system, in degrees.</param>
		/// <param name="largeArc">Specifies whether the large or small arc is drawn.</param>
		/// <param name="sweep">The direction in which the arc is drawn.</param>
		/// <param name="xy">The end point of the arc.</param>
		/// <remarks />
		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			SkiaApi.sk_pathbuilder_arc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends an SVG-style elliptical arc to the current contour using separate radius and endpoint coordinates.</summary>
		/// <param name="rx">The x-radius of the ellipse.</param>
		/// <param name="ry">The y-radius of the ellipse.</param>
		/// <param name="xAxisRotate">The rotation of the ellipse's x-axis, in degrees.</param>
		/// <param name="largeArc">Specifies whether the large or small arc is drawn.</param>
		/// <param name="sweep">The direction in which the arc is drawn.</param>
		/// <param name="x">The x-coordinate of the end point.</param>
		/// <param name="y">The y-coordinate of the end point.</param>
		/// <remarks />
		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_pathbuilder_arc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends an arc of the specified oval to the current contour.</summary>
		/// <param name="oval">The bounds of the oval that defines the arc.</param>
		/// <param name="startAngle">The starting angle of the arc, in degrees.</param>
		/// <param name="sweepAngle">The sweep angle of the arc, in degrees.</param>
		/// <param name="forceMoveTo"><see langword="true" /> to begin a new contour at the arc start; <see langword="false" /> to connect with a line from the current point.</param>
		/// <remarks />
		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_oval (Handle, &oval, startAngle, sweepAngle, forceMoveTo);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a circular arc to the current contour using two tangent points.</summary>
		/// <param name="point1">The first tangent point.</param>
		/// <param name="point2">The second tangent point and end of the arc.</param>
		/// <param name="radius">The radius of the circular arc.</param>
		/// <remarks />
		public void ArcTo (SKPoint point1, SKPoint point2, float radius)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, point1.X, point1.Y, point2.X, point2.Y, radius);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a circular arc to the current contour using two tangent points specified as coordinates.</summary>
		/// <param name="x1">The x-coordinate of the first tangent point.</param>
		/// <param name="y1">The y-coordinate of the first tangent point.</param>
		/// <param name="x2">The x-coordinate of the second tangent point.</param>
		/// <param name="y2">The y-coordinate of the second tangent point.</param>
		/// <param name="radius">The radius of the circular arc.</param>
		/// <remarks />
		public void ArcTo (float x1, float y1, float x2, float y2, float radius)
		{
			SkiaApi.sk_pathbuilder_arc_to_with_points (Handle, x1, y1, x2, y2, radius);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative SVG-style elliptical arc to the current contour.</summary>
		/// <param name="r">The radii of the ellipse (x-radius, y-radius).</param>
		/// <param name="xAxisRotate">The rotation of the ellipse's x-axis, in degrees.</param>
		/// <param name="largeArc">Specifies whether the large or small arc is drawn.</param>
		/// <param name="sweep">The direction in which the arc is drawn.</param>
		/// <param name="xy">The offset from the current point to the end point of the arc.</param>
		/// <remarks />
		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			SkiaApi.sk_pathbuilder_rarc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a relative SVG-style elliptical arc to the current contour using separate radius and offset coordinates.</summary>
		/// <param name="rx">The x-radius of the ellipse.</param>
		/// <param name="ry">The y-radius of the ellipse.</param>
		/// <param name="xAxisRotate">The rotation of the ellipse's x-axis, in degrees.</param>
		/// <param name="largeArc">Specifies whether the large or small arc is drawn.</param>
		/// <param name="sweep">The direction in which the arc is drawn.</param>
		/// <param name="x">The x-offset from the current point to the end point.</param>
		/// <param name="y">The y-offset from the current point to the end point.</param>
		/// <remarks />
		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			SkiaApi.sk_pathbuilder_rarc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);
			GC.KeepAlive (this);
		}

		// Close

		/// <summary>Closes the current contour by appending a line to its starting point.</summary>
		/// <remarks />
		public void Close ()
		{
			SkiaApi.sk_pathbuilder_close (Handle);
			GC.KeepAlive (this);
		}

		// Add shapes

		/// <summary>Appends a rectangle contour to the path.</summary>
		/// <param name="rect">The rectangle to add.</param>
		/// <param name="direction">The winding direction of the rectangle contour.</param>
		/// <remarks />
		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_rect (Handle, &rect, direction);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a rectangle contour to the path, starting at the specified corner.</summary>
		/// <param name="rect">The rectangle to add.</param>
		/// <param name="direction">The winding direction of the rectangle contour.</param>
		/// <param name="startIndex">The index of the corner (0–3) at which the contour begins.</param>
		/// <remarks />
		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException (nameof (startIndex), "Starting index must be in the range of 0..3 (inclusive).");

			SkiaApi.sk_pathbuilder_add_rect_start (Handle, &rect, direction, startIndex);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a round rectangle contour to the path.</summary>
		/// <param name="rect">The round rectangle to add.</param>
		/// <param name="direction">The winding direction of the round rectangle contour.</param>
		/// <remarks />
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect (Handle, rect.Handle, direction);
			GC.KeepAlive (rect);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a round rectangle contour to the path, starting at the specified point index.</summary>
		/// <param name="rect">The round rectangle to add.</param>
		/// <param name="direction">The winding direction of the round rectangle contour.</param>
		/// <param name="startIndex">The index of the starting point on the contour.</param>
		/// <remarks />
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_pathbuilder_add_rrect_start (Handle, rect.Handle, direction, startIndex);
			GC.KeepAlive (rect);
			GC.KeepAlive (this);
		}

		/// <summary>Appends an oval contour to the path.</summary>
		/// <param name="rect">The bounding rectangle of the oval.</param>
		/// <param name="direction">The winding direction of the oval contour.</param>
		/// <remarks />
		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_oval (Handle, &rect, direction);
			GC.KeepAlive (this);
		}

		/// <summary>Appends an arc of the specified oval as a new contour.</summary>
		/// <param name="oval">The bounds of the oval that defines the arc.</param>
		/// <param name="startAngle">The starting angle of the arc, in degrees, measured clockwise from the positive x-axis.</param>
		/// <param name="sweepAngle">The sweep angle of the arc, in degrees, measured clockwise.</param>
		/// <remarks />
		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			SkiaApi.sk_pathbuilder_add_arc (Handle, &oval, startAngle, sweepAngle);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a round rectangle contour with the specified corner radii to the path.</summary>
		/// <param name="rect">The bounding rectangle of the round rectangle.</param>
		/// <param name="rx">The horizontal radius of the rounded corners.</param>
		/// <param name="ry">The vertical radius of the rounded corners.</param>
		/// <param name="dir">The winding direction of the contour.</param>
		/// <remarks />
		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_rounded_rect (Handle, &rect, rx, ry, dir);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a circle contour to the path.</summary>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="dir">The winding direction of the circle contour.</param>
		/// <remarks />
		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			SkiaApi.sk_pathbuilder_add_circle (Handle, x, y, radius, dir);
			GC.KeepAlive (this);
		}

		/// <summary>Appends a polygon contour defined by the specified span of points.</summary>
		/// <param name="points">A read-only span of points defining the polygon vertices.</param>
		/// <param name="close"><see langword="true" /> to close the polygon by adding a line back to the first point; otherwise, <see langword="false" />.</param>
		/// <remarks />
		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			fixed (SKPoint* p = points) {
				SkiaApi.sk_pathbuilder_add_poly (Handle, p, points.Length, close);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Appends a polygon contour defined by the specified points.</summary>
		/// <param name="points">An array of points defining the polygon vertices.</param>
		/// <param name="close"><see langword="true" /> to close the polygon by adding a line back to the first point; otherwise, <see langword="false" />.</param>
		/// <remarks />
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

		/// <summary>Appends the contours from another path, translated by the specified offset, to this builder.</summary>
		/// <param name="other">The path whose contours are appended.</param>
		/// <param name="dx">The horizontal translation applied to the contours of <paramref name="other" /> before appending.</param>
		/// <param name="dy">The vertical translation applied to the contours of <paramref name="other" /> before appending.</param>
		/// <param name="mode">Controls how the contours of <paramref name="other" /> are joined to the existing contours.</param>
		/// <remarks />
		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path_offset (Handle, other.Handle, dx, dy, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the contours from another path, transformed by a matrix, to this builder.</summary>
		/// <param name="other">The path whose contours are appended.</param>
		/// <param name="matrix">A transformation matrix applied to the contours of <paramref name="other" /> before appending.</param>
		/// <param name="mode">Controls how the contours of <paramref name="other" /> are joined to the existing contours.</param>
		/// <remarks />
		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_pathbuilder_add_path_matrix (Handle, other.Handle, m, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the contours from another path to this builder.</summary>
		/// <param name="other">The path whose contours are appended.</param>
		/// <param name="mode">Controls how the contours of <paramref name="other" /> are joined to the existing contours.</param>
		/// <remarks />
		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_pathbuilder_add_path (Handle, other.Handle, mode);
			GC.KeepAlive (other);
			GC.KeepAlive (this);
		}

		/// <summary>Appends the contours from another path in reverse order.</summary>
		/// <param name="other">The path whose contours are reversed and appended.</param>
		/// <remarks />
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
