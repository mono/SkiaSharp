#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>
	/// Convexity for paths.
	/// </summary>
	public enum SKPathConvexity
	{
		/// <summary>
		/// The path's convexity is unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// The path is convex.
		/// </summary>
		Convex = 1,
		/// <summary>
		/// The path is concave.
		/// </summary>
		Concave = 2,
	}

	/// <summary>
	/// A compound geometric path.
	/// </summary>
	/// <remarks>A path encapsulates compound (multiple contour) geometric paths consisting of straight line segments, quadratic curves, and cubic curves.</remarks>
	public unsafe class SKPath : SKObject, ISKSkipObjectRegistration
	{
		internal SKPath (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates an empty path.
		/// </summary>
		public SKPath ()
			: this (SkiaApi.sk_path_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPath instance.");
			}
		}

		/// <summary>
		/// Creates a path by making a copy of an existing path.
		/// </summary>
		/// <param name="path">The path to clone.</param>
		/// <remarks>This constructor can throw InvalidOperationException if there is a problem copying the source path.</remarks>
		public SKPath (SKPath path)
			: this (SkiaApi.sk_path_clone (path.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKPath instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_path_delete (Handle);

		/// <summary>
		/// Gets or sets the path's fill type.
		/// </summary>
		/// <remarks>This is used to define how "inside" is computed. The default value is <see cref="SKPathFillType.Winding" />.</remarks>
		public SKPathFillType FillType {
			get => SkiaApi.sk_path_get_filltype (Handle);
			set => SkiaApi.sk_path_set_filltype (Handle, value);
		}

		/// <summary>
		/// Gets or sets the path's convexity.
		/// </summary>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public SKPathConvexity Convexity => IsConvex ? SKPathConvexity.Convex : SKPathConvexity.Concave;

		/// <summary>
		/// Gets a value indicating whether the path is convex.
		/// </summary>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public bool IsConvex => SkiaApi.sk_path_is_convex (Handle);

		/// <summary>
		/// Gets a value indicating whether the path is concave.
		/// </summary>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public bool IsConcave => !IsConvex;

		/// <summary>
		/// Gets a value indicating whether or not the path is empty (contains no lines or curves).
		/// </summary>
		public bool IsEmpty => VerbCount == 0;

		/// <summary>
		/// Gets a value indicating whether the path is a single oval or circle.
		/// </summary>
		/// <remarks>See also <see cref="SKPath.GetOvalBounds" />.</remarks>
		public bool IsOval => SkiaApi.sk_path_is_oval (Handle, null);

		/// <summary>
		/// Gets a value indicating whether the path is a single, round rectangle.
		/// </summary>
		/// <remarks>See also <see cref="SKPath.GetRoundRect" />.</remarks>
		public bool IsRoundRect => SkiaApi.sk_path_is_rrect (Handle, IntPtr.Zero);

		/// <summary>
		/// Gets a value indicating whether the path is a single, straight line.
		/// </summary>
		/// <remarks>See also <see cref="SKPath.GetLine" />.</remarks>
		public bool IsLine => SkiaApi.sk_path_is_line (Handle, null);

		/// <summary>
		/// Gets a value indicating whether the path is a single rectangle.
		/// </summary>
		/// <remarks>See also <see cref="SKPath.GetRect" /> and <see cref="SKPath.GetRect(System.Boolean@,SkiaSharp.SKPathDirection@)" />.</remarks>
		public bool IsRect => SkiaApi.sk_path_is_rect (Handle, null, null, null);

		/// <summary>
		/// Gets a set of flags indicating if the path contains one or more segments of that type.
		/// </summary>
		public SKPathSegmentMask SegmentMasks => (SKPathSegmentMask)SkiaApi.sk_path_get_segment_masks (Handle);

		/// <summary>
		/// Gets the number of verbs in the path.
		/// </summary>
		public int VerbCount => SkiaApi.sk_path_count_verbs (Handle);

		/// <summary>
		/// Gets the number of points on the path.
		/// </summary>
		public int PointCount => SkiaApi.sk_path_count_points (Handle);

		/// <summary>
		/// Gets the point at the specified index.
		/// </summary>
		/// <value>The point at the specified index.</value>
		/// <param name="index">The index of the point to get.</param>
		/// <remarks>If the index is out of range (i.e. is not 0 &lt;= index &lt; <see cref="SKPath.PointCount" />), then the returned coordinates will be (0, 0).</remarks>
		public SKPoint this[int index] => GetPoint (index);

		/// <summary>
		/// Gets all the points in the path.
		/// </summary>
		/// <remarks>The number of points will be <see cref="SkiaSharp.SKPath.PointCount" />, To only
		/// return a subset of the points in the path, use
		/// <see cref="SkiaSharp.SKPath.GetPoints%2A" />.</remarks>
		public SKPoint[] Points => GetPoints (PointCount);

		/// <summary>
		/// Return the last point on the path. If no points have been added, (0, 0) is returned.
		/// </summary>
		public SKPoint LastPoint {
			get {
				SKPoint point;
				SkiaApi.sk_path_get_last_point (Handle, &point);
				return point;
			}
		}

		/// <summary>
		/// Gets the bounds of the path's points. If the path contains zero points/verbs, this will return the empty rectangle.
		/// </summary>
		/// <value>Gets the bounds of the path's points.</value>
		/// <remarks>This bounds may be larger than the actual shape, since curves do not extend as far as their control points. Additionally this bound encompasses all points, even isolated MoveTo either preceding or following the last non-degenerate contour.</remarks>
		public SKRect Bounds {
			get {
				SKRect rect;
				SkiaApi.sk_path_get_bounds (Handle, &rect);
				return rect;
			}
		}

		/// <summary>
		/// Gets the "tight" bounds of the path. Unlike <see cref="SKPath.Bounds" />, the control points of curves are excluded.
		/// </summary>
		/// <value>The tight bounds of the path.</value>
		public SKRect TightBounds {
			get {
				if (GetTightBounds (out var rect)) {
					return rect;
				} else {
					return SKRect.Empty;
				}
			}
		}

		/// <summary>
		/// Returns the oval bounds of the path.
		/// </summary>
		/// <returns>Returns the oval bounds of the path.</returns>
		/// <remarks>If the path is not a single oval or circle, then an empty rectangle is returned. See also <see cref="SKPath.IsOval" />.</remarks>
		public SKRect GetOvalBounds ()
		{
			SKRect bounds;
			if (SkiaApi.sk_path_is_oval (Handle, &bounds)) {
				return bounds;
			} else {
				return SKRect.Empty;
			}
		}

		/// <summary>
		/// Returns the round rectangle of the path.
		/// </summary>
		/// <returns>Returns the round rectangle of the path.</returns>
		/// <remarks>If the path is not a single round rectangle, then <see langword="null" /> is returned. See also <see cref="SKPath.IsRoundRect" />.</remarks>
		public SKRoundRect GetRoundRect ()
		{
			var rrect = new SKRoundRect ();
			var result = SkiaApi.sk_path_is_rrect (Handle, rrect.Handle);
			if (result) {
				return rrect;
			} else {
				rrect.Dispose ();
				return null;
			}
		}

		/// <summary>
		/// Returns the two points of the path.
		/// </summary>
		/// <returns>Returns the two points of the path.</returns>
		/// <remarks>If the path is not a single, straight line, then <see langword="null" /> is returned. See also <see cref="SKPath.IsLine" />.</remarks>
		public SKPoint[] GetLine ()
		{
			var temp = new SKPoint[2];
			fixed (SKPoint* t = temp) {
				var result = SkiaApi.sk_path_is_line (Handle, t);
				if (result) {
					return temp;
				} else {
					return null;
				}
			}
		}

		/// <summary>
		/// Returns the rectangle of the path.
		/// </summary>
		/// <returns>Returns the rectangle of the path.</returns>
		/// <remarks>If the path is not a single rectangle, then an empty rectangle is returned. See also <see cref="SKPath.IsRect" />.</remarks>
		public SKRect GetRect () =>
			GetRect (out var isClosed, out var direction);

		/// <summary>
		/// Returns the rectangle of the path.
		/// </summary>
		/// <param name="isClosed">Whether or not the rectangle is closed.</param>
		/// <param name="direction">The direction of the rectangle.</param>
		/// <returns>Returns the rectangle of the path.</returns>
		/// <remarks>If the path is not a single rectangle, then an empty rectangle is returned. See also <see cref="SKPath.IsRect" />.</remarks>
		public SKRect GetRect (out bool isClosed, out SKPathDirection direction)
		{
			byte c;
			fixed (SKPathDirection* d = &direction) {
				SKRect rect;
				var result = SkiaApi.sk_path_is_rect (Handle, &rect, &c, d);
				isClosed = c > 0;
				if (result) {
					return rect;
				} else {
					return SKRect.Empty;
				}
			}
		}

		/// <summary>
		/// Returns the point at the specified index.
		/// </summary>
		/// <param name="index">The index of the point to return.</param>
		/// <returns>The point at the specified index.</returns>
		/// <remarks>If the index is out of range (i.e. is not 0 &lt;= index &lt; <see cref="SKPath.PointCount" />), then the returned coordinates will be (0, 0).</remarks>
		public SKPoint GetPoint (int index)
		{
			if (index < 0 || index >= PointCount)
				throw new ArgumentOutOfRangeException (nameof (index));

			SKPoint point;
			SkiaApi.sk_path_get_point (Handle, index, &point);
			return point;
		}

		/// <summary>
		/// Returns a subset of points in the path. Up to max points are copied.
		/// </summary>
		/// <param name="max">The maximum number of points to copy into points.</param>
		/// <returns>Returns the requested set of points.</returns>
		public SKPoint[] GetPoints (int max)
		{
			var points = new SKPoint[max];
			GetPoints (points, max);
			return points;
		}

		/// <summary>
		/// Returns a subset of points in the path. Up to max points are copied.
		/// </summary>
		/// <param name="points">The array to hold the points.</param>
		/// <param name="max">The maximum number of points to copy into points.</param>
		/// <returns>Returns the actual number of points in the path</returns>
		public int GetPoints (SKPoint[] points, int max)
		{
			fixed (SKPoint* p = points) {
				return SkiaApi.sk_path_get_points (Handle, p, max);
			}
		}

		/// <summary>
		/// Returns true if the point (x, y) is contained by the path, taking into account the <see cref="SKPath.FillType" />.
		/// </summary>
		/// <param name="x">The x-coordinate to check.</param>
		/// <param name="y">The y-coordinate to check.</param>
		/// <returns>Returns true if the point (x, y) is contained by the path.</returns>
		public bool Contains (float x, float y) =>
			SkiaApi.sk_path_contains (Handle, x, y);

		/// <summary>
		/// Set the beginning of the next contour to the point.
		/// </summary>
		/// <param name="offset">The amount to offset the entire path.</param>
		public void Offset (SKPoint offset) =>
			Offset (offset.X, offset.Y);

		/// <summary>
		/// Offset the path by the specified distance.
		/// </summary>
		/// <param name="dx">The amount in the x-direction to offset the entire path.</param>
		/// <param name="dy">The amount in the y-direction to offset the entire path.</param>
		public void Offset (float dx, float dy)
		{
			var matrix = SKMatrix.CreateTranslation (dx, dy);
			Transform (in matrix);
		}

		/// <summary>
		/// Set the beginning of the next contour to the point.
		/// </summary>
		/// <param name="point">The coordinates of the start of a new contour.</param>
		public void MoveTo (SKPoint point) =>
			SkiaApi.sk_path_move_to (Handle, point.X, point.Y);

		/// <summary>
		/// Set the beginning of the next contour to the point.
		/// </summary>
		/// <param name="x">The x-coordinate of the start of a new contour.</param>
		/// <param name="y">The y-coordinate of the start of a new contour.</param>
		public void MoveTo (float x, float y) =>
			SkiaApi.sk_path_move_to (Handle, x, y);

		/// <summary>
		/// Same as <see cref="SKPath.MoveTo(SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="point">The amount to add to the coordinates of the last point on this contour, to specify the start of a new contour.</param>
		public void RMoveTo (SKPoint point) =>
			SkiaApi.sk_path_rmove_to (Handle, point.X, point.Y);

		/// <summary>
		/// Same as <see cref="SKPath.MoveTo(System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="dx">The amount to add to the x-coordinate of the last point on this contour, to specify the start of a new contour.</param>
		/// <param name="dy">The amount to add to the x-coordinate of the last point on this contour, to specify the start of a new contour.</param>
		public void RMoveTo (float dx, float dy) =>
			SkiaApi.sk_path_rmove_to (Handle, dx, dy);

		/// <summary>
		/// Add a line from the last point to the specified point (x, y).
		/// </summary>
		/// <param name="point">The coordinates of the end of a line.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void LineTo (SKPoint point) =>
			SkiaApi.sk_path_line_to (Handle, point.X, point.Y);

		/// <summary>
		/// Add a line from the last point to the specified point (x, y).
		/// </summary>
		/// <param name="x">The x-coordinate of the end of a line.</param>
		/// <param name="y">The y-coordinate of the end of a line.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void LineTo (float x, float y) =>
			SkiaApi.sk_path_line_to (Handle, x, y);

		/// <summary>
		/// Same as <see cref="SKPath.LineTo(SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="point">The amount to add to the coordinates of the last point on this contour, to specify the end of a line.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RLineTo (SKPoint point) =>
			SkiaApi.sk_path_rline_to (Handle, point.X, point.Y);

		/// <summary>
		/// Same as <see cref="SKPath.LineTo(System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="dx">The amount to add to the x-coordinate of the last point on this contour, to specify the end of a line.</param>
		/// <param name="dy">The amount to add to the y-coordinate of the last point on this contour, to specify the end of a line.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RLineTo (float dx, float dy) =>
			SkiaApi.sk_path_rline_to (Handle, dx, dy);

		/// <summary>
		/// Add a quadratic bezier from the last point.
		/// </summary>
		/// <param name="point0">The coordinates of the control point on a quadratic curve.</param>
		/// <param name="point1">The coordinates of the end point on a quadratic curve.</param>
		/// <remarks>Add a quadratic bezier from the last point, approaching control point
		/// (`point0`), and ending at `point1`.
		/// If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void QuadTo (SKPoint point0, SKPoint point1) =>
			SkiaApi.sk_path_quad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);

		/// <summary>
		/// Add a quadratic bezier from the last point.
		/// </summary>
		/// <param name="x0">The x-coordinate of the control point on a quadratic curve.</param>
		/// <param name="y0">The y-coordinate of the control point on a quadratic curve.</param>
		/// <param name="x1">The x-coordinate of the end point on a quadratic curve.</param>
		/// <param name="y1">The y-coordinate of the end point on a quadratic curve.</param>
		/// <remarks>Add a quadratic bezier from the last point, approaching control point
		/// (`x0`, `y0`), and ending at (`x1`, `y1`).
		/// If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void QuadTo (float x0, float y0, float x1, float y1) =>
			SkiaApi.sk_path_quad_to (Handle, x0, y0, x1, y1);

		/// <summary>
		/// Same as <see cref="SKPath.QuadTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RQuadTo (SKPoint point0, SKPoint point1) =>
			SkiaApi.sk_path_rquad_to (Handle, point0.X, point0.Y, point1.X, point1.Y);

		/// <summary>
		/// Same as <see cref="SKPath.QuadTo(System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RQuadTo (float dx0, float dy0, float dx1, float dy1) =>
			SkiaApi.sk_path_rquad_to (Handle, dx0, dy0, dx1, dy1);

		/// <summary>
		/// Add a conic path from the last point.
		/// </summary>
		/// <param name="point0">The coordinates of the control point of the conic curve.</param>
		/// <param name="point1">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void ConicTo (SKPoint point0, SKPoint point1, float w) =>
			SkiaApi.sk_path_conic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);

		/// <summary>
		/// Add a conic path from the last point.
		/// </summary>
		/// <param name="x0">The x-coordinate of the control point of the conic curve.</param>
		/// <param name="y0">The y-coordinate of the control point of the conic curve.</param>
		/// <param name="x1">The x-coordinate of the end point of the conic curve.</param>
		/// <param name="y1">The y-coordinate of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void ConicTo (float x0, float y0, float x1, float y1, float w) =>
			SkiaApi.sk_path_conic_to (Handle, x0, y0, x1, y1, w);

		/// <summary>
		/// Same as <see cref="SKPath.ConicTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RConicTo (SKPoint point0, SKPoint point1, float w) =>
			SkiaApi.sk_path_rconic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, w);

		/// <summary>
		/// Same as <see cref="SKPath.ConicTo(System.Single,System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w) =>
			SkiaApi.sk_path_rconic_to (Handle, dx0, dy0, dx1, dy1, w);

		/// <summary>
		/// Adds a cubic bezier from the last point.
		/// </summary>
		/// <param name="point0">The coordinates of the 1st control point on a cubic curve.</param>
		/// <param name="point1">The coordinates of the 2nd control point on a cubic curve.</param>
		/// <param name="point2">The coordinates of the end point on a cubic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2) =>
			SkiaApi.sk_path_cubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);

		/// <summary>
		/// Adds a cubic bezier from the last point.
		/// </summary>
		/// <param name="x0">The x-coordinate of the 1st control point on a cubic curve.</param>
		/// <param name="y0">The y-coordinate of the 1st control point on a cubic curve.</param>
		/// <param name="x1">The x-coordinate of the 2nd control point on a cubic curve.</param>
		/// <param name="y1">The y-coordinate of the 2nd control point on a cubic curve.</param>
		/// <param name="x2">The x-coordinate of the end point on a cubic curve.</param>
		/// <param name="y2">The y-coordinate of the end point on a cubic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2) =>
			SkiaApi.sk_path_cubic_to (Handle, x0, y0, x1, y1, x2, y2);

		/// <summary>
		/// Same as <see cref="SKPath.CubicTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint,SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="point2">The amount to add to the coordinates of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2) =>
			SkiaApi.sk_path_rcubic_to (Handle, point0.X, point0.Y, point1.X, point1.Y, point2.X, point2.Y);

		/// <summary>
		/// Same as <see cref="SKPath.CubicTo(System.Single,System.Single,System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="dx2">The amount to add to the x-coordinate of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <param name="dy2">The amount to add to the y-coordinate of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <remarks>If no <see cref="SkiaSharp.SKPath.MoveTo%2A" /> call has been made for this contour,
		/// the first point is automatically set to (0, 0).</remarks>
		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2) =>
			SkiaApi.sk_path_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);

		/// <summary>
		/// Appends an elliptical arc from the current point in the format used by SVG.
		/// </summary>
		/// <param name="r">The radius.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="xy">The destination coordinate.</param>
		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy) =>
			SkiaApi.sk_path_arc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);

		/// <summary>
		/// Appends an elliptical arc from the current point in the format used by SVG.
		/// </summary>
		/// <param name="rx">The radius in the x-direction.</param>
		/// <param name="ry">The radius in the y-direction.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="x">The destination x-coordinate.</param>
		/// <param name="y">The destination y-coordinate.</param>
		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y) =>
			SkiaApi.sk_path_arc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);

		/// <summary>
		/// Appends the specified arc to the path.
		/// </summary>
		/// <param name="oval">The bounding oval defining the shape and size of the arc.</param>
		/// <param name="startAngle">The starting angle (in degrees) where the arc begins.</param>
		/// <param name="sweepAngle">The sweep angle (in degrees) measured clockwise.</param>
		/// <param name="forceMoveTo">Whether to always begin a new contour with the arc.</param>
		/// <remarks>If the start of the arc is different from the path's current last point, then
		/// an automatic <see cref="SkiaSharp.SKPath.LineTo%2A" /> is added to connect the current
		/// contour to the start of the arc. However, if the path is empty, then we call
		/// <see cref="SkiaSharp.SKPath.MoveTo%2A" /> with the first point of the arc.</remarks>
		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo) =>
			SkiaApi.sk_path_arc_to_with_oval (Handle, &oval, startAngle, sweepAngle, forceMoveTo);

		/// <summary>
		/// Appends a line and arc to the current path.
		/// </summary>
		/// <param name="point1">The corner coordinates.</param>
		/// <param name="point2">The destination coordinates.</param>
		/// <param name="radius">The corner radius.</param>
		/// <remarks>This is the same as the PostScript call "arct".</remarks>
		public void ArcTo (SKPoint point1, SKPoint point2, float radius) =>
			SkiaApi.sk_path_arc_to_with_points (Handle, point1.X, point1.Y, point2.X, point2.Y, radius);

		/// <summary>
		/// Appends a line and arc to the current path.
		/// </summary>
		/// <param name="x1">The corner x-coordinate.</param>
		/// <param name="y1">The corner y-coordinate.</param>
		/// <param name="x2">The destination x-coordinate.</param>
		/// <param name="y2">The destination y-coordinate.</param>
		/// <param name="radius">The corner radius.</param>
		/// <remarks>This is the same as the PostScript call "arct".</remarks>
		public void ArcTo (float x1, float y1, float x2, float y2, float radius) =>
			SkiaApi.sk_path_arc_to_with_points (Handle, x1, y1, x2, y2, radius);

		/// <summary>
		/// The same as <see cref="SKPath.ArcTo(SkiaSharp.SKPoint,System.Single,SkiaSharp.SKPathArcSize,SkiaSharp.SKPathDirection,SkiaSharp.SKPoint)" />, but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="r">The radius.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="xy">The destination coordinates relative to the last point.</param>
		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy) =>
			SkiaApi.sk_path_rarc_to (Handle, r.X, r.Y, xAxisRotate, largeArc, sweep, xy.X, xy.Y);

		/// <summary>
		/// The same as <see cref="SKPath.ArcTo(System.Single,System.Single,System.Single,SkiaSharp.SKPathArcSize,SkiaSharp.SKPathDirection,System.Single,System.Single)" />, but the coordinates are considered relative to the last point on this contour.
		/// </summary>
		/// <param name="rx">The radius in the x-direction.</param>
		/// <param name="ry">The radius in the y-direction.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="x">The destination x-coordinate relative to the last point.</param>
		/// <param name="y">The destination y-coordinate relative to the last point.</param>
		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y) =>
			SkiaApi.sk_path_rarc_to (Handle, rx, ry, xAxisRotate, largeArc, sweep, x, y);

		/// <summary>
		/// Closes the current contour.
		/// </summary>
		/// <remarks>If the current point is not equal to the first point of the contour, a line segment is automatically added.</remarks>
		public void Close () =>
			SkiaApi.sk_path_close (Handle);

		/// <summary>
		/// Clear any lines and curves from the path, making it empty.
		/// </summary>
		/// <remarks>Any internal storage for those lines/curves is retained, making reuse of the path potentially faster.</remarks>
		public void Rewind () =>
			SkiaApi.sk_path_rewind (Handle);

		/// <summary>
		/// Clear any lines and curves from the path, making it empty.
		/// </summary>
		/// <remarks>This frees up internal storage associated with those segments.</remarks>
		public void Reset () =>
			SkiaApi.sk_path_reset (Handle);

		/// <summary>
		/// Adds a closed rectangle contour to the path.
		/// </summary>
		/// <param name="rect">The rectangle to add as a closed contour to the path</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise) =>
			SkiaApi.sk_path_add_rect (Handle, &rect, direction);

		/// <summary>
		/// Adds a closed rectangle contour to the path.
		/// </summary>
		/// <param name="rect">The rectangle to add as a closed contour to the path</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <param name="startIndex">Initial point of the contour (initial <see cref="SKPath.MoveTo(SkiaSharp.SKPoint)" />), expressed as a corner index, starting in the upper-left position, clock-wise. Must be in the range of 0..3.</param>
		/// <remarks>Add a closed rectangle contour to the path with an initial point of the contour (startIndex) expressed as a corner index.</remarks>
		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException (nameof (startIndex), "Starting index must be in the range of 0..3 (inclusive).");

			SkiaApi.sk_path_add_rect_start (Handle, &rect, direction, startIndex);
		}

		/// <summary>
		/// Adds a closed rectangle with rounded corners to the current path.
		/// </summary>
		/// <param name="rect">The rounded rectangle.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_path_add_rrect (Handle, rect.Handle, direction);
		}

		/// <summary>
		/// Adds a closed rectangle with rounded corners to the current path.
		/// </summary>
		/// <param name="rect">The rounded rectangle.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <param name="startIndex">Initial point of the contour (initial <see cref="SKPath.MoveTo(SkiaSharp.SKPoint)" />), expressed as an index of the radii minor/major points, ordered clock-wise. Must be in the range of 0..7.</param>
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			SkiaApi.sk_path_add_rrect_start (Handle, rect.Handle, direction, startIndex);
		}

		/// <summary>
		/// Adds a closed oval contour to the path.
		/// </summary>
		/// <param name="rect">The bounding oval to add as a closed contour to the path.</param>
		/// <param name="direction">The direction to wind the oval's contour.</param>
		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise) =>
			SkiaApi.sk_path_add_oval (Handle, &rect, direction);

		/// <summary>
		/// Adds the specified arc to the path as a new contour.
		/// </summary>
		/// <param name="oval">The bounds of oval used to define the size of the arc.</param>
		/// <param name="startAngle">Starting angle (in degrees) where the arc begins.</param>
		/// <param name="sweepAngle">Sweep angle (in degrees) measured clockwise.</param>
		public void AddArc (SKRect oval, float startAngle, float sweepAngle) =>
			SkiaApi.sk_path_add_arc (Handle, &oval, startAngle, sweepAngle);

		/// <summary>
		/// Returns the bounds of the path's points.
		/// </summary>
		/// <param name="rect">The bounds, if the path contains any points.</param>
		/// <returns>Returns true if the path is not empty, otherwise false.</returns>
		/// <remarks>This bounds may be larger than the actual shape, since curves do not extend as far as their control points. Additionally this bound encompasses all points, even isolated MoveTo either preceding or following the last non-degenerate contour.</remarks>
		public bool GetBounds (out SKRect rect)
		{
			var isEmpty = IsEmpty;
			if (isEmpty) {
				rect = SKRect.Empty;
			} else {
				fixed (SKRect* r = &rect) {
					SkiaApi.sk_path_get_bounds (Handle, r);
				}
			}
			return !isEmpty;
		}

		/// <summary>
		/// Computes a bounds that is conservatively "snug" around the path.
		/// </summary>
		/// <returns>Returns the bounds.</returns>
		/// <remarks><para>This assumes that the path will be filled.</para><para></para><para>It does not attempt to collapse away contours that are logically empty (e.g. MoveTo(x, y) + LineTo(x, y)) but will include them in the calculation.</para></remarks>
		public SKRect ComputeTightBounds ()
		{
			SKRect rect;
			SkiaApi.sk_path_compute_tight_bounds (Handle, &rect);
			return rect;
		}

		public void Transform (in SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_path_transform (Handle, m);
		}

		public void Transform (in SKMatrix matrix, SKPath destination)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_path_transform_to_dest (Handle, m, destination.Handle);
		}

		/// <summary>
		/// Applies a transformation matrix to the all the elements in the path.
		/// </summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		[Obsolete("Use Transform(in SKMatrix) instead.", true)]
		public void Transform (SKMatrix matrix) =>
			Transform (in matrix);

		/// <summary>
		/// Applies a transformation matrix to the all the elements in the path.
		/// </summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		/// <param name="destination">The instance that should contain the final, transformed path.</param>
		[Obsolete("Use Transform(in SKMatrix matrix, SKPath destination) instead.", true)]
		public void Transform (SKMatrix matrix, SKPath destination) =>
			Transform (in matrix, destination);

		/// <summary>
		/// Extends the current path with the path elements from another path offset by (<paramref name="dx" />, <paramref name="dy" />), using the specified extension mode.
		/// </summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="dx">The amount to translate the path in X as it is added.</param>
		/// <param name="dy">The amount to translate the path in Y as it is added.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_path_add_path_offset (Handle, other.Handle, dx, dy, mode);
		}

		/// <summary>
		/// Extends the current path with the path elements from another path, by applying the specified transformation matrix, using the specified extension mode.
		/// </summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="matrix">Transformation matrix applied to the <paramref name="other" /> path.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_path_add_path_matrix (Handle, other.Handle, m, mode);
		}

		/// <summary>
		/// Extends the current path with the path elements from another path, using the specified extension mode.
		/// </summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_path_add_path (Handle, other.Handle, mode);
		}

		/// <summary>
		/// Extends the current path with the path elements from another path in reverse order.
		/// </summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		public void AddPathReverse (SKPath other)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			SkiaApi.sk_path_add_path_reverse (Handle, other.Handle);
		}

		/// <summary>
		/// Adds a closed rectangle with rounded corners to the current path.
		/// </summary>
		/// <param name="rect">The bounds of a the rounded rectangle.</param>
		/// <param name="rx">The x-radius of the rounded corners.</param>
		/// <param name="ry">The y-radius of the rounded corners</param>
		/// <param name="dir">The direction to wind the rectangle's contour.</param>
		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise) =>
			SkiaApi.sk_path_add_rounded_rect (Handle, &rect, rx, ry, dir);

		/// <summary>
		/// Adds a closed circle contour to the path.
		/// </summary>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="dir">The direction to wind the circle's contour.</param>
		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise) =>
			SkiaApi.sk_path_add_circle (Handle, x, y, radius, dir);

		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_path_add_poly (Handle, p, points.Length, close);
			}
		}

		/// <summary>
		/// Adds a new contour made of just lines.
		/// </summary>
		/// <param name="points">The points that make up the polygon.</param>
		/// <param name="close">Whether or not to close the path.</param>
		public void AddPoly (SKPoint[] points, bool close = true)
		{
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_path_add_poly (Handle, p, points.Length, close);
			}
		}

		/// <summary>
		/// Creates an iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.
		/// </summary>
		/// <param name="forceClose">When this is true, each contour (as defined by a new starting move command) will be completed with a close verb regardless of the contour's contents.</param>
		/// <returns>Returns an object that can be used to iterate over the various elements of the path.</returns>
		/// <remarks>This iterator is able to clean up the path as the values are returned. If you
		/// do not desire to get verbs that have been cleaned up, use the
		/// <see cref="SkiaSharp.SKPath.CreateRawIterator%2A" /> method instead.</remarks>
		public Iterator CreateIterator (bool forceClose) =>
			new Iterator (this, forceClose);

		/// <summary>
		/// Creates a raw iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.
		/// </summary>
		/// <returns>Returns an object that can be used to iterate over the various elements of the path.</returns>
		/// <remarks>Unlike the <see cref="SkiaSharp.SKPath.CreateIterator%2A" /> method, this iterator
		/// does not clean up or normalize the values in the path. It returns the raw
		/// elements contained in the path.</remarks>
		public RawIterator CreateRawIterator () =>
			new RawIterator (this);

		/// <summary>
		/// Compute the result of a logical operation on two paths.
		/// </summary>
		/// <param name="other">The second operand.</param>
		/// <param name="op">The logical operator.</param>
		/// <param name="result">The path that will be used to set the result to. The current path will be <see cref="SKPath.Reset" />.</param>
		/// <returns>Returns true if the operation was successful, otherwise false.</returns>
		public bool Op (SKPath other, SKPathOp op, SKPath result)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_op (Handle, other.Handle, op, result.Handle);
		}

		/// <summary>
		/// Compute the result of a logical operation on two paths.
		/// </summary>
		/// <param name="other">The second operand.</param>
		/// <param name="op">The logical operator.</param>
		/// <returns>Returns the resulting path if the operation was successful, otherwise <see langword="null" />.</returns>
		public SKPath Op (SKPath other, SKPathOp op)
		{
			var result = new SKPath ();
			if (Op (other, op, result)) {
				return result;
			} else {
				result.Dispose ();
				return null;
			}
		}

		/// <summary>
		/// Simplifies the current path.
		/// </summary>
		/// <param name="result">The path to store the simplified path data. If simplification failed, then this is unmodified.</param>
		/// <returns>Returns true if simplification was successful, otherwise false.</returns>
		/// <remarks>The curve order is reduced where possible so that cubics may be turned into quadratics, and quadratics maybe turned into lines.</remarks>
		public bool Simplify (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_simplify (Handle, result.Handle);
		}

		/// <summary>
		/// Return a simplified copy of the current path.
		/// </summary>
		/// <returns>Returns the new path if simplification was successful, otherwise null.</returns>
		/// <remarks>The curve order is reduced where possible so that cubics may be turned into quadratics, and quadratics maybe turned into lines.</remarks>
		public SKPath Simplify ()
		{
			var result = new SKPath ();
			if (Simplify (result)) {
				return result;
			} else {
				result.Dispose ();
				return null;
			}
		}

		/// <summary>
		/// Gets the "tight" bounds of the path. Unlike <see cref="SKPath.GetBounds(SkiaSharp.SKRect@)" />, the control points of curves are excluded.
		/// </summary>
		/// <param name="result">The tight bounds of the path.</param>
		/// <returns>Returns true if the bounds could be computed, otherwise false.</returns>
		public bool GetTightBounds (out SKRect result)
		{
			fixed (SKRect* r = &result) {
				return SkiaApi.sk_pathop_tight_bounds (Handle, r);
			}
		}

		/// <param name="result"></param>
		public bool ToWinding (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_as_winding (Handle, result.Handle);
		}

		public SKPath ToWinding ()
		{
			var result = new SKPath ();
			if (ToWinding (result)) {
				return result;
			} else {
				result.Dispose ();
				return null;
			}
		}

		/// <summary>
		/// Returns a SVG path data representation of the current path.
		/// </summary>
		public string ToSvgPathData ()
		{
			using var str = new SKString ();
			SkiaApi.sk_path_to_svg_string (Handle, str.Handle);
			return (string)str;
		}

		/// <summary>
		/// Creates a path based on the SVG path data string.
		/// </summary>
		/// <param name="svgPath">The SVG path data.</param>
		/// <returns>Returns the new path if successful, otherwise <see langword="null" />.</returns>
		public static SKPath ParseSvgPathData (string svgPath)
		{
			var path = new SKPath ();
			var success = SkiaApi.sk_path_parse_svg_string (path.Handle, svgPath);
			if (!success) {
				path.Dispose ();
				path = null;
			}
			return path;
		}

		/// <summary>
		/// Chop a conic into a number of quads.
		/// </summary>
		/// <param name="p0">The coordinates of the starting point of the conic curve.</param>
		/// <param name="p1">The coordinates of the control point of the conic curve.</param>
		/// <param name="p2">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <param name="pow2">The tolerance to use (1 &lt;&lt; pow2).</param>
		/// <returns>Returns the collection of points that make up the conic curve.</returns>
		public static SKPoint[] ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2)
		{
			ConvertConicToQuads (p0, p1, p2, w, out var pts, pow2);
			return pts;
		}

		/// <summary>
		/// Chop a conic into a number of quads.
		/// </summary>
		/// <param name="p0">The coordinates of the starting point of the conic curve.</param>
		/// <param name="p1">The coordinates of the control point of the conic curve.</param>
		/// <param name="p2">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <param name="pts">The collection of points.</param>
		/// <param name="pow2">The tolerance to use (1 &lt;&lt; pow2).</param>
		/// <returns>Returns the number of quads.</returns>
		public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2)
		{
			var quadCount = 1 << pow2;
			var ptCount = 2 * quadCount + 1;
			pts = new SKPoint[ptCount];
			return ConvertConicToQuads (p0, p1, p2, w, pts, pow2);
		}

		/// <summary>
		/// Chop a conic into a number of quads.
		/// </summary>
		/// <param name="p0">The coordinates of the starting point of the conic curve.</param>
		/// <param name="p1">The coordinates of the control point of the conic curve.</param>
		/// <param name="p2">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <param name="pts">The collection to store the points.</param>
		/// <param name="pow2">The tolerance to use (1 &lt;&lt; pow2).</param>
		/// <returns>Returns the number of quads.</returns>
		/// <remarks>The amount of storage needed for pts is: 1 + 2 * (1 &lt;&lt; pow2)</remarks>
		public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, SKPoint[] pts, int pow2)
		{
			if (pts == null)
				throw new ArgumentNullException (nameof (pts));
			fixed (SKPoint* ptsptr = pts) {
				return SkiaApi.sk_path_convert_conic_to_quads (&p0, &p1, &p2, w, ptsptr, pow2);
			}
		}

		//

		internal static SKPath GetObject (IntPtr handle, bool owns = true) =>
			handle == IntPtr.Zero ? null : new SKPath (handle, owns);

		//

		/// <summary>
		/// Iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.
		/// </summary>
		/// <remarks>Iterators are created by calling the
		/// <xref:SkiaSharp.SKPath.CreateIterator%2A?displayProperty=nameWithType> method.</remarks>
		public class Iterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKPath path;

			internal Iterator (SKPath path, bool forceClose)
				: base (SkiaApi.sk_path_create_iter (path.Handle, forceClose ? 1 : 0), true)
			{
				this.path = path;
			}

			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			protected override void DisposeNative () =>
				SkiaApi.sk_path_iter_destroy (Handle);

			/// <param name="points"></param>
			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

			/// <param name="points"></param>
			public SKPathVerb Next (Span<SKPoint> points)
			{
				if (points == null)
					throw new ArgumentNullException (nameof (points));
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements.", nameof (points));

				fixed (SKPoint* p = points) {
					return SkiaApi.sk_path_iter_next (Handle, p);
				}
			}

			/// <summary>
			/// Return the weight for the current conic.
			/// </summary>
			/// <remarks>Only valid if the current segment return by
			/// <see cref="SkiaSharp.SKPath.Iterator.Next%2A" /> was <see cref="SkiaSharp.SKPathVerb.Conic" />.</remarks>
			public float ConicWeight () =>
				SkiaApi.sk_path_iter_conic_weight (Handle);

			/// <summary>
			/// Returns a value indicating whether the last call to <see cref="SKPath.Iterator.Next(SkiaSharp.SKPoint[],System.Boolean,System.Boolean)" /> returns a line which was the result of a <see cref="SKPath.Close" /> command.
			/// </summary>
			/// <returns>Returns true if the last call to <see cref="SKPath.Iterator.Next(SkiaSharp.SKPoint[],System.Boolean,System.Boolean)" /> returned a line which was the result of a <see cref="SKPath.Close" /> command.</returns>
			/// <remarks>If the call to <see cref="SKPath.Iterator.Next(SkiaSharp.SKPoint[],System.Boolean,System.Boolean)" /> returned a different value than <see cref="SKPathVerb.Line" />, the result is undefined.</remarks>
			public bool IsCloseLine () =>
				SkiaApi.sk_path_iter_is_close_line (Handle) != 0;

			/// <summary>
			/// Returns a value indicating whether the current contour is closed.
			/// </summary>
			/// <returns>Returns true if the current contour is closed (has a <see cref="SKPathVerb.Close" />).</returns>
			public bool IsCloseContour () =>
				SkiaApi.sk_path_iter_is_closed_contour (Handle) != 0;
		}

		/// <summary>
		/// Iterator object to scan through the verbs in the path, providing the associated points.
		/// </summary>
		/// <remarks>Iterators are created by calling the
		/// <xref:SkiaSharp.SKPath.CreateRawIterator%2A?displayProperty=nameWithType>
		/// method.</remarks>
		public class RawIterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKPath path;

			internal RawIterator (SKPath path)
				: base (SkiaApi.sk_path_create_rawiter (path.Handle), true)
			{
				this.path = path;
			}

			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			protected override void DisposeNative () =>
				SkiaApi.sk_path_rawiter_destroy (Handle);

			/// <summary>
			/// Returns the next verb in this iteration of the path.
			/// </summary>
			/// <param name="points">The storage for the points representing the current verb and/or segment. Should be an array of four points.</param>
			/// <returns>The verb of the current segment.</returns>
			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

			/// <param name="points"></param>
			public SKPathVerb Next (Span<SKPoint> points)
			{
				if (points == null)
					throw new ArgumentNullException (nameof (points));
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements.", nameof (points));
				fixed (SKPoint* p = points) {
					return SkiaApi.sk_path_rawiter_next (Handle, p);
				}
			}

			/// <summary>
			/// Returns the weight for the current conic.
			/// </summary>
			/// <remarks>Only valid if the current segment returned by <see cref="SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> was <see cref="SKPathVerb.Conic" />.</remarks>
			public float ConicWeight () =>
				SkiaApi.sk_path_rawiter_conic_weight (Handle);

			/// <summary>
			/// Returns what the next verb will be, but do not visit the next segment.
			/// </summary>
			/// <returns>Returns the verb for the next segment.</returns>
			public SKPathVerb Peek () =>
				SkiaApi.sk_path_rawiter_peek (Handle);
		}

		/// <summary>
		/// Perform a series of path operations, optimized for unioning many paths together.
		/// </summary>
		public class OpBuilder : SKObject, ISKSkipObjectRegistration
		{
			/// <summary>
			/// Creates an instance of <see cref="SKPath.OpBuilder" />.
			/// </summary>
			public OpBuilder ()
				: base (SkiaApi.sk_opbuilder_new (), true)
			{
			}

			/// <summary>
			/// Add one or more paths and their operand.
			/// </summary>
			/// <param name="path">The second operand.</param>
			/// <param name="op">The operator to apply to the existing and supplied paths.</param>
			/// <remarks>The builder is empty before the first path is added, so the result of a single add is ("empty-path" OP "path").</remarks>
			public void Add (SKPath path, SKPathOp op) =>
				SkiaApi.sk_opbuilder_add (Handle, path.Handle, op);

			/// <summary>
			/// Computes the sum of all paths and operands, and resets the builder to its initial state.
			/// </summary>
			/// <param name="result">The product of the operands.</param>
			/// <returns>Returns true if the operation succeeded, otherwise false.</returns>
			public bool Resolve (SKPath result)
			{
				if (result == null)
					throw new ArgumentNullException (nameof (result));

				return SkiaApi.sk_opbuilder_resolve (Handle, result.Handle);
			}

			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			protected override void DisposeNative () =>
				SkiaApi.sk_opbuilder_destroy (Handle);
		}
	}
}
