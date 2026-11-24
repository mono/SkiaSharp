#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	/// <summary>Convexity for paths.</summary>
	/// <remarks />
	public enum SKPathConvexity
	{
		/// <summary>The path's convexity is unknown.</summary>
		Unknown = 0,
		/// <summary>The path is convex.</summary>
		Convex = 1,
		/// <summary>The path is concave.</summary>
		Concave = 2,
	}

	/// <summary>A compound geometric path.</summary>
	/// <remarks>A path encapsulates compound (multiple contour) geometric paths consisting of straight line segments, quadratic curves, and cubic curves.</remarks>
	public unsafe class SKPath : SKObject, ISKSkipObjectRegistration
	{
		private SKPathBuilder _builder;

		internal SKPath (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// External code (SKCanvas.DrawPath, SKRegion.SetPath, SKPathBuilder.AddPath, etc.)
		// reads path.Handle directly and P/Invokes with it. If mutations have been batched
		// into _builder but not yet flushed, base.Handle points at a stale native SkPath.
		// Flushing in the getter keeps every reader — internal or external — honest.
		//
		// Skip the flush once disposal has begun. SKPathBuilder is itself an SKObject with
		// its own finalizer, so on the finalizer thread it may already have been collected
		// (its Handle is IntPtr.Zero, native pointer freed). Touching it here would call
		// sk_pathbuilder_detach_path on a null/dangling handle. The pending mutations are
		// going to be discarded with the path anyway, and DisposeNative cleans up _builder
		// defensively.
		/// <summary>Gets or sets the handle to the underlying native path object.</summary>
		/// <value>The underlying native <c>sk_path_t</c> handle.</value>
		/// <remarks></remarks>
		public override IntPtr Handle {
			get {
				if (!IsDisposed)
					FlushBuilder ();
				return base.Handle;
			}
			protected set => base.Handle = value;
		}

		/// <summary>Creates an empty path.</summary>
		/// <remarks />
		public SKPath ()
			: this (SkiaApi.sk_path_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPath instance.");
			}
		}

		/// <summary>Creates a path by making a copy of an existing path.</summary>
		/// <param name="path">The path to clone.</param>
		/// <remarks>This constructor can throw InvalidOperationException if there is a problem copying the source path.</remarks>
		public SKPath (SKPath path)
			: this (SkiaApi.sk_path_clone (path.Handle), true)
		{
			GC.KeepAlive (path);
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKPath instance.");
			}
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPath" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPath" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			_builder?.Dispose ();
			_builder = null;
			SkiaApi.sk_path_delete (Handle);
		}

		/// <summary>Gets or sets the path's fill type.</summary>
		/// <value>One of the enumeration values that specifies the path's fill type.</value>
		/// <remarks>This is used to define how "inside" is computed. The default value is <see cref="F:SkiaSharp.SKPathFillType.Winding" />.</remarks>
		public SKPathFillType FillType {
			get {
				var r = SkiaApi.sk_path_get_filltype (Handle);
				GC.KeepAlive (this);
				return r;
			}
			set {
				SkiaApi.sk_path_set_filltype (Handle, value);
				GC.KeepAlive (this);
				if (_builder != null)
					_builder.FillType = value;
			}
		}

		/// <summary>Gets the path's convexity.</summary>
		/// <value>One of the enumeration values that specifies the path's convexity.</value>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public SKPathConvexity Convexity { get { return IsConvex ? SKPathConvexity.Convex : SKPathConvexity.Concave; } }

		/// <summary>Gets a value indicating whether the path is convex.</summary>
		/// <value><see langword="true" /> if the path is convex; otherwise, <see langword="false" />.</value>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public bool IsConvex { get { var r = SkiaApi.sk_path_is_convex (Handle); GC.KeepAlive (this); return r; } }

		/// <summary>Gets a value indicating whether the path is concave.</summary>
		/// <value><see langword="true" /> if the path is concave; otherwise, <see langword="false" />.</value>
		/// <remarks>If it is currently unknown, then this function will attempt to compute the convexity (and cache the result).</remarks>
		public bool IsConcave => !IsConvex;

		/// <summary>Gets a value indicating whether or not the path is empty (contains no lines or curves).</summary>
		/// <value><see langword="true" /> if the path is empty; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsEmpty => VerbCount == 0;

		/// <summary>Gets a value indicating whether the path is a single oval or circle.</summary>
		/// <value><see langword="true" /> if the path is a single oval or circle; otherwise, <see langword="false" />.</value>
		/// <remarks>See also <see cref="M:SkiaSharp.SKPath.GetOvalBounds" />.</remarks>
		public bool IsOval { get { var r = SkiaApi.sk_path_is_oval (Handle, null); GC.KeepAlive (this); return r; } }

		/// <summary>Gets a value indicating whether the path is a single, round rectangle.</summary>
		/// <value><see langword="true" /> if the path is a single, round rectangle; otherwise, <see langword="false" />.</value>
		/// <remarks>See also <see cref="M:SkiaSharp.SKPath.GetRoundRect" />.</remarks>
		public bool IsRoundRect { get { var r = SkiaApi.sk_path_is_rrect (Handle, IntPtr.Zero); GC.KeepAlive (this); return r; } }

		/// <summary>Gets a value indicating whether the path is a single, straight line.</summary>
		/// <value><see langword="true" /> if the path is a single, straight line; otherwise, <see langword="false" />.</value>
		/// <remarks>See also <see cref="M:SkiaSharp.SKPath.GetLine" />.</remarks>
		public bool IsLine { get { var r = SkiaApi.sk_path_is_line (Handle, null); GC.KeepAlive (this); return r; } }

		/// <summary>Gets a value indicating whether the path is a single rectangle.</summary>
		/// <value><see langword="true" /> if the path is a single rectangle; otherwise, <see langword="false" />.</value>
		/// <remarks>See also <see cref="M:SkiaSharp.SKPath.GetRect" /> and <see cref="M:SkiaSharp.SKPath.GetRect(System.Boolean@,SkiaSharp.SKPathDirection@)" />.</remarks>
		public bool IsRect { get { var r = SkiaApi.sk_path_is_rect (Handle, null, null, null); GC.KeepAlive (this); return r; } }

		/// <summary>Gets a set of flags indicating if the path contains one or more segments of that type.</summary>
		/// <value>A set of flags indicating the segment types contained in the path.</value>
		/// <remarks />
		public SKPathSegmentMask SegmentMasks { get { var r = (SKPathSegmentMask)SkiaApi.sk_path_get_segment_masks (Handle); GC.KeepAlive (this); return r; } }

		/// <summary>Gets the number of verbs in the path.</summary>
		/// <value>The number of verbs in the path.</value>
		/// <remarks />
		public int VerbCount { get { var r = SkiaApi.sk_path_count_verbs (Handle); GC.KeepAlive (this); return r; } }

		/// <summary>Gets the number of points on the path.</summary>
		/// <value>The number of points on the path.</value>
		/// <remarks />
		public int PointCount { get { var r = SkiaApi.sk_path_count_points (Handle); GC.KeepAlive (this); return r; } }

		/// <summary>Gets the point at the specified index.</summary>
		/// <param name="index">The index of the point to get.</param>
		/// <value>The point at the specified index.</value>
		/// <remarks>If the index is out of range (i.e. is not 0 &lt;= index &lt; <see cref="P:SkiaSharp.SKPath.PointCount" />), then the returned coordinates will be (0, 0).</remarks>
		public SKPoint this[int index] => GetPoint (index);

		/// <summary>Gets all the points in the path.</summary>
		/// <value>The array of points in the path.</value>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// The number of points will be <xref:SkiaSharp.SKPath.PointCount>, To only
		/// return a subset of the points in the path, use
		/// <xref:SkiaSharp.SKPath.GetPoints%2A>.
		/// ]]></format></remarks>
		public SKPoint[] Points { get { return GetPoints (PointCount); } }

		/// <summary>Returns the last point on the path. If no points have been added, (0, 0) is returned.</summary>
		/// <value>The last point on the path.</value>
		/// <remarks />
		public SKPoint LastPoint {
			get {
				SKPoint point;
				SkiaApi.sk_path_get_last_point (Handle, &point);
				GC.KeepAlive (this);
				return point;
			}
		}

		/// <summary>Gets the bounds of the path's points. If the path contains zero points/verbs, this will return the empty rectangle.</summary>
		/// <value>Gets the bounds of the path's points.</value>
		/// <remarks>This bounds may be larger than the actual shape, since curves do not extend as far as their control points. Additionally this bound encompasses all points, even isolated MoveTo either preceding or following the last non-degenerate contour.</remarks>
		public SKRect Bounds {
			get {
				SKRect rect;
				SkiaApi.sk_path_get_bounds (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		/// <summary>Gets the "tight" bounds of the path. Unlike <see cref="P:SkiaSharp.SKPath.Bounds" />, the control points of curves are excluded.</summary>
		/// <value>The tight bounds of the path.</value>
		/// <remarks />
		public SKRect TightBounds {
			get {
				if (GetTightBounds (out var rect)) {
					return rect;
				} else {
					return SKRect.Empty;
				}
			}
		}

		/// <summary>Returns the oval bounds of the path.</summary>
		/// <returns>Returns the oval bounds of the path.</returns>
		/// <remarks>If the path is not a single oval or circle, then an empty rectangle is returned. See also <see cref="P:SkiaSharp.SKPath.IsOval" />.</remarks>
		public SKRect GetOvalBounds ()
		{
			SKRect bounds;
			var isOval = SkiaApi.sk_path_is_oval (Handle, &bounds);
			GC.KeepAlive (this);
			if (isOval) {
				return bounds;
			} else {
				return SKRect.Empty;
			}
		}

		/// <summary>Returns the round rectangle of the path.</summary>
		/// <returns>Returns the round rectangle of the path.</returns>
		/// <remarks>If the path is not a single round rectangle, then <see langword="null" /> is returned. See also <see cref="P:SkiaSharp.SKPath.IsRoundRect" />.</remarks>
		public SKRoundRect GetRoundRect ()
		{
			var rrect = new SKRoundRect ();
			var result = SkiaApi.sk_path_is_rrect (Handle, rrect.Handle);
			GC.KeepAlive (this);
			if (result) {
				return rrect;
			} else {
				rrect.Dispose ();
				return null;
			}
		}

		/// <summary>Returns the two points of the path.</summary>
		/// <returns>Returns the two points of the path.</returns>
		/// <remarks>If the path is not a single, straight line, then <see langword="null" /> is returned. See also <see cref="P:SkiaSharp.SKPath.IsLine" />.</remarks>
		public SKPoint[] GetLine ()
		{
			var temp = new SKPoint[2];
			fixed (SKPoint* t = temp) {
				var result = SkiaApi.sk_path_is_line (Handle, t);
				GC.KeepAlive (this);
				if (result) {
					return temp;
				} else {
					return null;
				}
			}
		}

		/// <summary>Returns the rectangle of the path.</summary>
		/// <returns>Returns the rectangle of the path.</returns>
		/// <remarks>If the path is not a single rectangle, then an empty rectangle is returned. See also <see cref="P:SkiaSharp.SKPath.IsRect" />.</remarks>
		public SKRect GetRect () =>
			GetRect (out var isClosed, out var direction);

		/// <summary>Returns the rectangle of the path.</summary>
		/// <param name="isClosed"><see langword="true" /> if the rectangle is closed; otherwise, <see langword="false" />.</param>
		/// <param name="direction">The direction of the rectangle.</param>
		/// <returns>Returns the rectangle of the path.</returns>
		/// <remarks>If the path is not a single rectangle, then an empty rectangle is returned. See also <see cref="P:SkiaSharp.SKPath.IsRect" />.</remarks>
		public SKRect GetRect (out bool isClosed, out SKPathDirection direction)
		{
			byte c;
			fixed (SKPathDirection* d = &direction) {
				SKRect rect;
				var result = SkiaApi.sk_path_is_rect (Handle, &rect, &c, d);
				GC.KeepAlive (this);
				isClosed = c > 0;
				if (result) {
					return rect;
				} else {
					return SKRect.Empty;
				}
			}
		}

		/// <summary>Returns the point at the specified index.</summary>
		/// <param name="index">The index of the point to return.</param>
		/// <returns>The point at the specified index.</returns>
		/// <remarks>If the index is out of range (i.e. is not 0 &lt;= index &lt; <see cref="P:SkiaSharp.SKPath.PointCount" />), then the returned coordinates will be (0, 0).</remarks>
		public SKPoint GetPoint (int index)
		{
			if (index < 0 || index >= PointCount)
				throw new ArgumentOutOfRangeException (nameof (index));

			SKPoint point;
			SkiaApi.sk_path_get_point (Handle, index, &point);
			GC.KeepAlive (this);
			return point;
		}

		/// <summary>Returns a subset of points in the path. Up to max points are copied.</summary>
		/// <param name="max">The maximum number of points to copy into points.</param>
		/// <returns>Returns the requested set of points.</returns>
		/// <remarks />
		public SKPoint[] GetPoints (int max)
		{
			var points = new SKPoint[max];
			GetPoints (points, max);
			return points;
		}

		/// <summary>Returns a subset of points in the path. Up to max points are copied.</summary>
		/// <param name="points">The array to hold the points.</param>
		/// <param name="max">The maximum number of points to copy into points.</param>
		/// <returns>Returns the actual number of points in the path.</returns>
		/// <remarks />
		public int GetPoints (SKPoint[] points, int max)
		{
			fixed (SKPoint* p = points) {
				var r = SkiaApi.sk_path_get_points (Handle, p, max);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Determines whether the point (x, y) is contained by the path, taking into account the <see cref="P:SkiaSharp.SKPath.FillType" />.</summary>
		/// <param name="x">The x-coordinate to check.</param>
		/// <param name="y">The y-coordinate to check.</param>
		/// <returns><see langword="true" /> if the point (x, y) is contained by the path; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (float x, float y)
		{
			var r = SkiaApi.sk_path_contains (Handle, x, y);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Sets the beginning of the next contour to the point.</summary>
		/// <param name="offset">The amount to offset the entire path.</param>
		/// <remarks />
		public void Offset (SKPoint offset) =>
			Offset (offset.X, offset.Y);

		/// <summary>Offset the path by the specified distance.</summary>
		/// <param name="dx">The amount in the x-direction to offset the entire path.</param>
		/// <param name="dy">The amount in the y-direction to offset the entire path.</param>
		/// <remarks />
		public void Offset (float dx, float dy)
		{
			var matrix = SKMatrix.CreateTranslation (dx, dy);
			Transform (in matrix);
		}

		/// <summary>Clear any lines and curves from the path, making it empty.</summary>
		/// <remarks>This frees up internal storage associated with those segments.</remarks>
		public void Reset ()
		{
			if (_builder != null) {
				_builder.Dispose ();
				_builder = null;
			}
			SkiaApi.sk_path_reset (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Returns the bounds of the path's points.</summary>
		/// <param name="rect">The bounds, if the path contains any points.</param>
		/// <returns><see langword="true" /> if the path is not empty; otherwise, <see langword="false" />.</returns>
		/// <remarks>This bounds may be larger than the actual shape, since curves do not extend as far as their control points. Additionally this bound encompasses all points, even isolated MoveTo either preceding or following the last non-degenerate contour.</remarks>
		public bool GetBounds (out SKRect rect)
		{
			var isEmpty = IsEmpty;
			if (isEmpty) {
				rect = SKRect.Empty;
			} else {
				fixed (SKRect* r = &rect) {
					SkiaApi.sk_path_get_bounds (Handle, r);
					GC.KeepAlive (this);
				}
			}
			return !isEmpty;
		}

		/// <summary>Computes a bounds that is conservatively "snug" around the path.</summary>
		/// <returns>Returns the bounds.</returns>
		/// <remarks><para>This assumes that the path will be filled.</para><para></para><para>It does not attempt to collapse away contours that are logically empty (e.g. MoveTo(x, y) + LineTo(x, y)) but will include them in the calculation.</para></remarks>
		public SKRect ComputeTightBounds ()
		{
			SKRect rect;
			SkiaApi.sk_path_compute_tight_bounds (Handle, &rect);
			GC.KeepAlive (this);
			return rect;
		}

		/// <summary>Applies a transformation matrix to all elements in the path.</summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		/// <remarks />
		public void Transform (in SKMatrix matrix)
		{
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_path_transform (Handle, m);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Applies a transformation matrix to all elements and stores the result in the destination path.</summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		/// <param name="destination">The path that will receive the transformed result.</param>
		/// <remarks />
		public void Transform (in SKMatrix matrix, SKPath destination)
		{
			if (destination == null)
				throw new ArgumentNullException (nameof (destination));

			fixed (SKMatrix* m = &matrix)
				SkiaApi.sk_path_transform_to_dest (Handle, m, destination.Handle);
			GC.KeepAlive (destination);
			GC.KeepAlive (this);
		}

		/// <summary>Applies a transformation matrix to all elements in the path.</summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		/// <remarks />
		[Obsolete("Use Transform(in SKMatrix) instead.", true)]
		public void Transform (SKMatrix matrix) =>
			Transform (in matrix);

		/// <summary>Applies a transformation matrix to all elements and stores the result in the destination path.</summary>
		/// <param name="matrix">The matrix to use for transformation.</param>
		/// <param name="destination">The path that will receive the transformed result.</param>
		/// <remarks />
		[Obsolete("Use Transform(in SKMatrix matrix, SKPath destination) instead.", true)]
		public void Transform (SKMatrix matrix, SKPath destination) =>
			Transform (in matrix, destination);

		/// <summary>Creates an iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.</summary>
		/// <param name="forceClose">When <paramref name="forceClose" /> is <see langword="true" />, each contour (as defined by a new starting move command) will be completed with a close verb regardless of the contour's contents.</param>
		/// <returns>Returns an object that can be used to iterate over the various elements of the path.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// This iterator is able to clean up the path as the values are returned. If you
		/// do not desire to get verbs that have been cleaned up, use the
		/// <xref:SkiaSharp.SKPath.CreateRawIterator%2A> method instead.
		/// ]]></format></remarks>
		public Iterator CreateIterator (bool forceClose)
		{
			return new Iterator (this, forceClose);
		}

		/// <summary>Creates a raw iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.</summary>
		/// <returns>Returns an object that can be used to iterate over the various elements of the path.</returns>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Unlike the <xref:SkiaSharp.SKPath.CreateIterator%2A> method, this iterator
		/// does not clean up or normalize the values in the path. It returns the raw
		/// elements contained in the path.
		/// ]]></format></remarks>
		public RawIterator CreateRawIterator ()
		{
			return new RawIterator (this);
		}

		/// <summary>Compute the result of a logical operation on two paths.</summary>
		/// <param name="other">The second operand.</param>
		/// <param name="op">The logical operator.</param>
		/// <param name="result">The path that will be used to set the result to. The current path will be <see cref="M:SkiaSharp.SKPath.Reset" />.</param>
		/// <returns><see langword="true" /> if the operation was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Op (SKPath other, SKPathOp op, SKPath result)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			var success = SkiaApi.sk_pathop_op (Handle, other.Handle, op, result.Handle);
			GC.KeepAlive (other);
			GC.KeepAlive (result);
			GC.KeepAlive (this);
			return success;
		}

		/// <summary>Compute the result of a logical operation on two paths.</summary>
		/// <param name="other">The second operand.</param>
		/// <param name="op">The logical operator.</param>
		/// <returns>Returns the resulting path if the operation was successful, otherwise <see langword="null" />.</returns>
		/// <remarks />
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

		/// <summary>Simplifies the current path.</summary>
		/// <param name="result">The path to store the simplified path data. If simplification failed, then this is unmodified.</param>
		/// <returns><see langword="true" /> if simplification was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks>The curve order is reduced where possible so that cubics may be turned into quadratics, and quadratics maybe turned into lines.</remarks>
		public bool Simplify (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			var success = SkiaApi.sk_pathop_simplify (Handle, result.Handle);
			GC.KeepAlive (result);
			GC.KeepAlive (this);
			return success;
		}

		/// <summary>Returns a simplified copy of the current path.</summary>
		/// <returns>Returns the new path if simplification was successful, or <see langword="null" /> otherwise.</returns>
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

		/// <summary>Gets the "tight" bounds of the path. Unlike <see cref="M:SkiaSharp.SKPath.GetBounds(SkiaSharp.SKRect@)" />, the control points of curves are excluded.</summary>
		/// <param name="result">The tight bounds of the path.</param>
		/// <returns><see langword="true" /> if the bounds could be computed; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool GetTightBounds (out SKRect result)
		{
			fixed (SKRect* r = &result) {
				var success = SkiaApi.sk_pathop_tight_bounds (Handle, r);
				GC.KeepAlive (this);
				return success;
			}
		}

		/// <summary>Converts this path to a winding fill type and stores the result in the specified path.</summary>
		/// <param name="result">The path that will receive the winding version of this path.</param>
		/// <returns><see langword="true" /> if the conversion was successful; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool ToWinding (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			var success = SkiaApi.sk_pathop_as_winding (Handle, result.Handle);
			GC.KeepAlive (result);
			GC.KeepAlive (this);
			return success;
		}

		/// <summary>Creates a new path with the fill type set to winding.</summary>
		/// <returns>A new path with winding fill type, or <see langword="null" /> if the conversion fails.</returns>
		/// <remarks />
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

		/// <summary>Returns a SVG path data representation of the current path.</summary>
		/// <returns>The SVG path data string.</returns>
		/// <remarks />
		public string ToSvgPathData ()
		{
			using var str = new SKString ();
			SkiaApi.sk_path_to_svg_string (Handle, str.Handle);
			GC.KeepAlive (this);
			return (string)str;
		}

		/// <summary>Creates a path based on the SVG path data string.</summary>
		/// <param name="svgPath">The SVG path data.</param>
		/// <returns>Returns the new path if successful, otherwise <see langword="null" />.</returns>
		/// <remarks />
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

		/// <summary>Chop a conic into a number of quads.</summary>
		/// <param name="p0">The coordinates of the starting point of the conic curve.</param>
		/// <param name="p1">The coordinates of the control point of the conic curve.</param>
		/// <param name="p2">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <param name="pow2">The tolerance to use (1 &lt;&lt; pow2).</param>
		/// <returns>Returns the collection of points that make up the conic curve.</returns>
		/// <remarks />
		public static SKPoint[] ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2)
		{
			ConvertConicToQuads (p0, p1, p2, w, out var pts, pow2);
			return pts;
		}

		/// <summary>Chop a conic into a number of quads.</summary>
		/// <param name="p0">The coordinates of the starting point of the conic curve.</param>
		/// <param name="p1">The coordinates of the control point of the conic curve.</param>
		/// <param name="p2">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <param name="pts">The collection of points.</param>
		/// <param name="pow2">The tolerance to use (1 &lt;&lt; pow2).</param>
		/// <returns>Returns the number of quads.</returns>
		/// <remarks />
		public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2)
		{
			var quadCount = 1 << pow2;
			var ptCount = 2 * quadCount + 1;
			pts = new SKPoint[ptCount];
			return ConvertConicToQuads (p0, p1, p2, w, pts, pow2);
		}

		/// <summary>Chop a conic into a number of quads.</summary>
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

		// Lazy builder support

		private void EnsureBuilder ()
		{
			if (_builder == null)
				_builder = new SKPathBuilder (this);
		}

		private void FlushBuilder ()
		{
			if (_builder == null)
				return;

			var newHandle = SkiaApi.sk_pathbuilder_detach_path (_builder.Handle);
			_builder.Dispose ();
			_builder = null;
			SkiaApi.sk_path_delete (Handle);
			GC.KeepAlive (this);
			Handle = newHandle;
		}

		internal void ReplaceFromBuilder (SKPathBuilder builder)
		{
			if (_builder != null) {
				_builder.Dispose ();
				_builder = null;
			}
			var newHandle = SkiaApi.sk_pathbuilder_detach_path (builder.Handle);
			GC.KeepAlive (builder);
			SkiaApi.sk_path_delete (Handle);
			GC.KeepAlive (this);
			Handle = newHandle;
		}

		#region Deprecated Mutation Methods

		// Move

		/// <summary>Sets the beginning of the next contour to the point.</summary>
		/// <param name="point">The coordinates of the start of a new contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void MoveTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.MoveTo (point);
		}

		/// <summary>Sets the beginning of the next contour to the point.</summary>
		/// <param name="x">The x-coordinate of the start of a new contour.</param>
		/// <param name="y">The y-coordinate of the start of a new contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void MoveTo (float x, float y)
		{
			EnsureBuilder ();
			_builder.MoveTo (x, y);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="point">The amount to add to the coordinates of the last point on this contour, to specify the start of a new contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RMoveTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.RMoveTo (point);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.MoveTo(System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="dx">The amount to add to the x-coordinate of the last point on this contour, to specify the start of a new contour.</param>
		/// <param name="dy">The amount to add to the x-coordinate of the last point on this contour, to specify the start of a new contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RMoveTo (float dx, float dy)
		{
			EnsureBuilder ();
			_builder.RMoveTo (dx, dy);
		}

		// Line

		/// <summary>Adds a line from the last point to the specified point (x, y).</summary>
		/// <param name="point">The coordinates of the end of a line.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void LineTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.LineTo (point);
		}

		/// <summary>Adds a line from the last point to the specified point (x, y).</summary>
		/// <param name="x">The x-coordinate of the end of a line.</param>
		/// <param name="y">The y-coordinate of the end of a line.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void LineTo (float x, float y)
		{
			EnsureBuilder ();
			_builder.LineTo (x, y);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.LineTo(SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="point">The amount to add to the coordinates of the last point on this contour, to specify the end of a line.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RLineTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.RLineTo (point);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.LineTo(System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="dx">The amount to add to the x-coordinate of the last point on this contour, to specify the end of a line.</param>
		/// <param name="dy">The amount to add to the y-coordinate of the last point on this contour, to specify the end of a line.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RLineTo (float dx, float dy)
		{
			EnsureBuilder ();
			_builder.RLineTo (dx, dy);
		}

		// Quad

		/// <summary>Adds a quadratic bezier from the last point.</summary>
		/// <param name="point0">The coordinates of the control point on a quadratic curve.</param>
		/// <param name="point1">The coordinates of the end point on a quadratic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Adds a quadratic bezier from the last point, approaching control point
		/// (`point0`), and ending at `point1`.
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void QuadTo (SKPoint point0, SKPoint point1)
		{
			EnsureBuilder ();
			_builder.QuadTo (point0, point1);
		}

		/// <summary>Adds a quadratic bezier from the last point.</summary>
		/// <param name="x0">The x-coordinate of the control point on a quadratic curve.</param>
		/// <param name="y0">The y-coordinate of the control point on a quadratic curve.</param>
		/// <param name="x1">The x-coordinate of the end point on a quadratic curve.</param>
		/// <param name="y1">The y-coordinate of the end point on a quadratic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Adds a quadratic bezier from the last point, approaching control point
		/// (`x0`, `y0`), and ending at (`x1`, `y1`).
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			EnsureBuilder ();
			_builder.QuadTo (x0, y0, x1, y1);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.QuadTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RQuadTo (SKPoint point0, SKPoint point1)
		{
			EnsureBuilder ();
			_builder.RQuadTo (point0, point1);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.QuadTo(System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the control point on a quadratic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify end point on a quadratic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			EnsureBuilder ();
			_builder.RQuadTo (dx0, dy0, dx1, dy1);
		}

		// Conic

		/// <summary>Adds a conic path from the last point.</summary>
		/// <param name="point0">The coordinates of the control point of the conic curve.</param>
		/// <param name="point1">The coordinates of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ConicTo (SKPoint point0, SKPoint point1, float w)
		{
			EnsureBuilder ();
			_builder.ConicTo (point0, point1, w);
		}

		/// <summary>Adds a conic path from the last point.</summary>
		/// <param name="x0">The x-coordinate of the control point of the conic curve.</param>
		/// <param name="y0">The y-coordinate of the control point of the conic curve.</param>
		/// <param name="x1">The x-coordinate of the end point of the conic curve.</param>
		/// <param name="y1">The y-coordinate of the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			EnsureBuilder ();
			_builder.ConicTo (x0, y0, x1, y1, w);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.ConicTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RConicTo (SKPoint point0, SKPoint point1, float w)
		{
			EnsureBuilder ();
			_builder.RConicTo (point0, point1, w);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.ConicTo(System.Single,System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the control point of the conic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify the end point of the conic curve.</param>
		/// <param name="w">The weight of the conic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			EnsureBuilder ();
			_builder.RConicTo (dx0, dy0, dx1, dy1, w);
		}

		// Cubic

		/// <summary>Adds a cubic bezier from the last point.</summary>
		/// <param name="point0">The coordinates of the 1st control point on a cubic curve.</param>
		/// <param name="point1">The coordinates of the 2nd control point on a cubic curve.</param>
		/// <param name="point2">The coordinates of the end point on a cubic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			EnsureBuilder ();
			_builder.CubicTo (point0, point1, point2);
		}

		/// <summary>Adds a cubic bezier from the last point.</summary>
		/// <param name="x0">The x-coordinate of the 1st control point on a cubic curve.</param>
		/// <param name="y0">The y-coordinate of the 1st control point on a cubic curve.</param>
		/// <param name="x1">The x-coordinate of the 2nd control point on a cubic curve.</param>
		/// <param name="y1">The y-coordinate of the 2nd control point on a cubic curve.</param>
		/// <param name="x2">The x-coordinate of the end point on a cubic curve.</param>
		/// <param name="y2">The y-coordinate of the end point on a cubic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			EnsureBuilder ();
			_builder.CubicTo (x0, y0, x1, y1, x2, y2);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.CubicTo(SkiaSharp.SKPoint,SkiaSharp.SKPoint,SkiaSharp.SKPoint)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="point0">The amount to add to the coordinates of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="point1">The amount to add to the coordinates of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="point2">The amount to add to the coordinates of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			EnsureBuilder ();
			_builder.RCubicTo (point0, point1, point2);
		}

		/// <summary>Same as <see cref="M:SkiaSharp.SKPath.CubicTo(System.Single,System.Single,System.Single,System.Single,System.Single,System.Single)" /> but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="dx0">The amount to add to the x-coordinate of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="dy0">The amount to add to the y-coordinate of the last point on this contour, to specify the 1st control point on a cubic curve.</param>
		/// <param name="dx1">The amount to add to the x-coordinate of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="dy1">The amount to add to the y-coordinate of the last point on this contour, to specify the 2nd control point on a cubic curve.</param>
		/// <param name="dx2">The amount to add to the x-coordinate of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <param name="dy2">The amount to add to the y-coordinate of the last point on this contour, to specify the end point on a cubic curve.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If no <xref:SkiaSharp.SKPath.MoveTo%2A> call has been made for this contour,
		/// the first point is automatically set to (0, 0).
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			EnsureBuilder ();
			_builder.RCubicTo (dx0, dy0, dx1, dy1, dx2, dy2);
		}

		// Arc

		/// <summary>Appends an elliptical arc from the current point in the format used by SVG.</summary>
		/// <param name="r">The radius.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="xy">The destination coordinate.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			EnsureBuilder ();
			_builder.ArcTo (r, xAxisRotate, largeArc, sweep, xy);
		}

		/// <summary>Appends an elliptical arc from the current point in the format used by SVG.</summary>
		/// <param name="rx">The radius in the x-direction.</param>
		/// <param name="ry">The radius in the y-direction.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="x">The destination x-coordinate.</param>
		/// <param name="y">The destination y-coordinate.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			EnsureBuilder ();
			_builder.ArcTo (rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		/// <summary>Appends the specified arc to the path.</summary>
		/// <param name="oval">The bounding oval defining the shape and size of the arc.</param>
		/// <param name="startAngle">The starting angle (in degrees) where the arc begins.</param>
		/// <param name="sweepAngle">The sweep angle (in degrees) measured clockwise.</param>
		/// <param name="forceMoveTo">Whether to always begin a new contour with the arc.</param>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// If the start of the arc is different from the path's current last point, then
		/// an automatic <xref:SkiaSharp.SKPath.LineTo%2A> is added to connect the current
		/// contour to the start of the arc. However, if the path is empty, then we call
		/// <xref:SkiaSharp.SKPath.MoveTo%2A> with the first point of the arc.
		/// ]]></format></remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo)
		{
			EnsureBuilder ();
			_builder.ArcTo (oval, startAngle, sweepAngle, forceMoveTo);
		}

		/// <summary>Appends a line and arc to the current path.</summary>
		/// <param name="point1">The corner coordinates.</param>
		/// <param name="point2">The destination coordinates.</param>
		/// <param name="radius">The corner radius.</param>
		/// <remarks>This is the same as the PostScript call "arct".</remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKPoint point1, SKPoint point2, float radius)
		{
			EnsureBuilder ();
			_builder.ArcTo (point1, point2, radius);
		}

		/// <summary>Appends a line and arc to the current path.</summary>
		/// <param name="x1">The corner x-coordinate.</param>
		/// <param name="y1">The corner y-coordinate.</param>
		/// <param name="x2">The destination x-coordinate.</param>
		/// <param name="y2">The destination y-coordinate.</param>
		/// <param name="radius">The corner radius.</param>
		/// <remarks>This is the same as the PostScript call "arct".</remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (float x1, float y1, float x2, float y2, float radius)
		{
			EnsureBuilder ();
			_builder.ArcTo (x1, y1, x2, y2, radius);
		}

		/// <summary>The same as <see cref="M:SkiaSharp.SKPath.ArcTo(SkiaSharp.SKPoint,System.Single,SkiaSharp.SKPathArcSize,SkiaSharp.SKPathDirection,SkiaSharp.SKPoint)" />, but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="r">The radius.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="xy">The destination coordinates relative to the last point.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			EnsureBuilder ();
			_builder.RArcTo (r, xAxisRotate, largeArc, sweep, xy);
		}

		/// <summary>The same as <see cref="M:SkiaSharp.SKPath.ArcTo(System.Single,System.Single,System.Single,SkiaSharp.SKPathArcSize,SkiaSharp.SKPathDirection,System.Single,System.Single)" />, but the coordinates are considered relative to the last point on this contour.</summary>
		/// <param name="rx">The radius in the x-direction.</param>
		/// <param name="ry">The radius in the y-direction.</param>
		/// <param name="xAxisRotate">The angle in degrees relative to the x-axis.</param>
		/// <param name="largeArc">Determines whether the smallest or largest arc possible is drawn.</param>
		/// <param name="sweep">Determines if the arc should be swept in an anti-clockwise or clockwise direction.</param>
		/// <param name="x">The destination x-coordinate relative to the last point.</param>
		/// <param name="y">The destination y-coordinate relative to the last point.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			EnsureBuilder ();
			_builder.RArcTo (rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		// Close

		/// <summary>Closes the current contour.</summary>
		/// <remarks>If the current point is not equal to the first point of the contour, a line segment is automatically added.</remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void Close ()
		{
			EnsureBuilder ();
			_builder.Close ();
		}

		// Rewind

		/// <summary>Clear any lines and curves from the path, making it empty.</summary>
		/// <remarks>Any internal storage for those lines/curves is retained, making reuse of the path potentially faster.</remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void Rewind ()
		{
			if (_builder != null) {
				_builder.Reset ();
			} else {
				_builder = new SKPathBuilder ();
			}
		}

		// Add shapes

		/// <summary>Adds a closed rectangle contour to the path.</summary>
		/// <param name="rect">The rectangle to add as a closed contour to the path.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRect (rect, direction);
		}

		/// <summary>Adds a closed rectangle contour to the path.</summary>
		/// <param name="rect">The rectangle to add as a closed contour to the path.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <param name="startIndex">Initial point of the contour (initial <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" />), expressed as a corner index, starting in the upper-left position, clock-wise. Must be in the range of 0..3.</param>
		/// <remarks>Add a closed rectangle contour to the path with an initial point of the contour (startIndex) expressed as a corner index.</remarks>
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			EnsureBuilder ();
			_builder.AddRect (rect, direction, startIndex);
		}

		/// <summary>Adds a closed rectangle with rounded corners to the current path.</summary>
		/// <param name="rect">The rounded rectangle.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, direction);
		}

		/// <summary>Adds a closed rectangle with rounded corners to the current path.</summary>
		/// <param name="rect">The rounded rectangle.</param>
		/// <param name="direction">The direction to wind the rectangle's contour.</param>
		/// <param name="startIndex">Initial point of the contour (initial <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" />), expressed as an index of the radii minor/major points, ordered clock-wise. Must be in the range of 0..7.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, direction, startIndex);
		}

		/// <summary>Adds a closed rectangle with rounded corners to the current path.</summary>
		/// <param name="rect">The bounds of a the rounded rectangle.</param>
		/// <param name="rx">The x-radius of the rounded corners.</param>
		/// <param name="ry">The y-radius of the rounded corners.</param>
		/// <param name="dir">The direction to wind the rectangle's contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, rx, ry, dir);
		}

		/// <summary>Adds a closed oval contour to the path.</summary>
		/// <param name="rect">The bounding oval to add as a closed contour to the path.</param>
		/// <param name="direction">The direction to wind the oval's contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddOval (rect, direction);
		}

		/// <summary>Adds the specified arc to the path as a new contour.</summary>
		/// <param name="oval">The bounds of oval used to define the size of the arc.</param>
		/// <param name="startAngle">Starting angle (in degrees) where the arc begins.</param>
		/// <param name="sweepAngle">Sweep angle (in degrees) measured clockwise.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			EnsureBuilder ();
			_builder.AddArc (oval, startAngle, sweepAngle);
		}

		/// <summary>Adds a closed circle contour to the path.</summary>
		/// <param name="x">The x-coordinate of the center of the circle.</param>
		/// <param name="y">The y-coordinate of the center of the circle.</param>
		/// <param name="radius">The radius of the circle.</param>
		/// <param name="dir">The direction to wind the circle's contour.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddCircle (x, y, radius, dir);
		}

		/// <summary>Adds a new contour made of just lines.</summary>
		/// <param name="points">The points that make up the polygon.</param>
		/// <param name="close"><see langword="true" /> to close the path; otherwise, <see langword="false" />.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPoly (SKPoint[] points, bool close = true)
		{
			EnsureBuilder ();
			_builder.AddPoly (points, close);
		}

		/// <summary>Adds a polygon contour to the path.</summary>
		/// <param name="points">The points defining the polygon vertices.</param>
		/// <param name="close">If <see langword="true" />, closes the polygon by connecting the last point to the first.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			EnsureBuilder ();
			_builder.AddPoly (points, close);
		}

		// Add path

		/// <summary>Extends the current path with the path elements from another path offset by (<paramref name="dx" />, <paramref name="dy" />), using the specified extension mode.</summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="dx">The amount to translate the path in X as it is added.</param>
		/// <param name="dy">The amount to translate the path in Y as it is added.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="F:SkiaSharp.SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="F:SkiaSharp.SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, dx, dy, mode);
		}

		/// <summary>Extends the current path with the path elements from another path, by applying the specified transformation matrix, using the specified extension mode.</summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="matrix">Transformation matrix applied to the <paramref name="other" /> path.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="F:SkiaSharp.SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="F:SkiaSharp.SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, in matrix, mode);
		}

		/// <summary>Extends the current path with the path elements from another path, using the specified extension mode.</summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <param name="mode">Determines how the <paramref name="other" /> path contours are added to the path. On <see cref="F:SkiaSharp.SKPathAddMode.Append" /> mode, contours are added as new contours. On <see cref="F:SkiaSharp.SKPathAddMode.Extend" /> mode, the last contour of the path is extended with the first contour of the <paramref name="other" /> path.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, mode);
		}

		/// <summary>Extends the current path with the path elements from another path in reverse order.</summary>
		/// <param name="other">The path containing the elements to be added to the current path.</param>
		/// <remarks />
		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPathReverse (SKPath other)
		{
			if (other == null)
				throw new ArgumentNullException (nameof (other));

			EnsureBuilder ();
			_builder.ReverseAddPath (other);
		}

		#endregion

		//

		/// <summary>Iterator object to scan the all of the segments (lines, quadratics, cubics) of each contours in a path.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Iterators are created by calling the
		/// <xref:SkiaSharp.SKPath.CreateIterator%2A?displayProperty=nameWithType> method.
		/// ]]></format></remarks>
		public class Iterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKPath path;

			internal Iterator (SKPath path, bool forceClose)
				: base (SkiaApi.sk_path_create_iter (path.Handle, forceClose ? 1 : 0), true)
			{
				this.path = path;
			}

			/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPath.Iterator" /> and optionally releases the managed resources.</summary>
			/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
			/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPath.Iterator" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_path_iter_destroy (Handle);

			/// <summary>Returns the next verb in this iteration of the path.</summary>
			/// <param name="points">The array to receive the points for the current segment. Must have at least 4 elements.</param>
			/// <returns>The verb of the current segment, or <see cref="F:SkiaSharp.SKPathVerb.Done" /> when finished.</returns>
			/// <remarks />
			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

			/// <summary>Returns the next verb in this iteration of the path.</summary>
			/// <param name="points">The span to receive the points for the current segment. Must have at least 4 elements.</param>
			/// <returns>The verb of the current segment, or <see cref="F:SkiaSharp.SKPathVerb.Done" /> when finished.</returns>
			/// <remarks />
			public SKPathVerb Next (Span<SKPoint> points)
			{
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements.", nameof (points));

				fixed (SKPoint* p = points) {
					var r = SkiaApi.sk_path_iter_next (Handle, p);
					GC.KeepAlive (this);
					return r;
				}
			}

			/// <summary>Returns the weight for the current conic.</summary>
			/// <returns>The conic weight for the current segment.</returns>
			/// <remarks><format type="text/markdown"><![CDATA[
			/// ## Remarks
			///
			/// Only valid if the current segment return by
			/// <xref:SkiaSharp.SKPath.Iterator.Next%2A> was <xref:SkiaSharp.SKPathVerb.Conic>.
			/// ]]></format></remarks>
			public float ConicWeight ()
			{
				var r = SkiaApi.sk_path_iter_conic_weight (Handle);
				GC.KeepAlive (this);
				return r;
			}

			/// <summary>Returns a value indicating whether the last call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> returns a line which was the result of a <see cref="M:SkiaSharp.SKPath.Close" /> command.</summary>
			/// <returns><see langword="true" /> if the last call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> returned a line which was the result of a <see cref="M:SkiaSharp.SKPath.Close" /> command.</returns>
			/// <remarks>If the call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> returned a different value than <see cref="F:SkiaSharp.SKPathVerb.Line" />, the result is undefined.</remarks>
			public bool IsCloseLine ()
			{
				var r = SkiaApi.sk_path_iter_is_close_line (Handle) != 0;
				GC.KeepAlive (this);
				return r;
			}

			/// <summary>Returns a value indicating whether the current contour is closed.</summary>
			/// <returns><see langword="true" /> if the current contour is closed (has a <see cref="F:SkiaSharp.SKPathVerb.Close" />).</returns>
			/// <remarks />
			public bool IsCloseContour ()
			{
				var r = SkiaApi.sk_path_iter_is_closed_contour (Handle) != 0;
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Iterator object to scan through the verbs in the path, providing the associated points.</summary>
		/// <remarks><format type="text/markdown"><![CDATA[
		/// ## Remarks
		///
		/// Iterators are created by calling the
		/// <xref:SkiaSharp.SKPath.CreateRawIterator%2A?displayProperty=nameWithType>
		/// method.
		/// ]]></format></remarks>
		public class RawIterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKPath path;

			internal RawIterator (SKPath path)
				: base (SkiaApi.sk_path_create_rawiter (path.Handle), true)
			{
				this.path = path;
			}

			/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPath.RawIterator" /> and optionally releases the managed resources.</summary>
			/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
			/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPath.RawIterator" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_path_rawiter_destroy (Handle);

			/// <summary>Returns the next verb in this iteration of the path.</summary>
			/// <param name="points">The storage for the points representing the current verb and/or segment. Should be an array of four points.</param>
			/// <returns>The verb of the current segment.</returns>
			/// <remarks />
			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

			/// <summary>Returns the next verb in this iteration of the path.</summary>
			/// <param name="points">The storage for the points representing the current verb and/or segment. Should be a span of at least four points.</param>
			/// <returns>The verb of the current segment.</returns>
			/// <remarks />
			public SKPathVerb Next (Span<SKPoint> points)
			{
				if (points.Length != 4)
					throw new ArgumentException ("Must be an array of four elements.", nameof (points));
				fixed (SKPoint* p = points) {
					var r = SkiaApi.sk_path_rawiter_next (Handle, p);
					GC.KeepAlive (this);
					return r;
				}
			}

			/// <summary>Returns the weight for the current conic.</summary>
			/// <returns>The conic weight for the current segment.</returns>
			/// <remarks>Only valid if the current segment returned by <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> was <see cref="F:SkiaSharp.SKPathVerb.Conic" />.</remarks>
			public float ConicWeight ()
			{
				var r = SkiaApi.sk_path_rawiter_conic_weight (Handle);
				GC.KeepAlive (this);
				return r;
			}

			/// <summary>Returns what the next verb will be, but do not visit the next segment.</summary>
			/// <returns>Returns the verb for the next segment.</returns>
			/// <remarks />
			public SKPathVerb Peek ()
			{
				var r = SkiaApi.sk_path_rawiter_peek (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Perform a series of path operations, optimized for unioning many paths together.</summary>
		/// <remarks />
		public class OpBuilder : SKObject, ISKSkipObjectRegistration
		{
			/// <summary>Creates an instance of <see cref="T:SkiaSharp.SKPath.OpBuilder" />.</summary>
			/// <remarks />
			public OpBuilder ()
				: base (SkiaApi.sk_opbuilder_new (), true)
			{
			}

			/// <summary>Add one or more paths and their operand.</summary>
			/// <param name="path">The second operand.</param>
			/// <param name="op">The operator to apply to the existing and supplied paths.</param>
			/// <remarks>The builder is empty before the first path is added, so the result of a single add is ("empty-path" OP "path").</remarks>
			public void Add (SKPath path, SKPathOp op)
			{
				SkiaApi.sk_opbuilder_add (Handle, path.Handle, op);
				GC.KeepAlive (path);
				GC.KeepAlive (this);
			}

			/// <summary>Computes the sum of all paths and operands, and resets the builder to its initial state.</summary>
			/// <param name="result">The product of the operands.</param>
			/// <returns><see langword="true" /> if the operation succeeded; otherwise, <see langword="false" />.</returns>
			/// <remarks />
			public bool Resolve (SKPath result)
			{
				if (result == null)
					throw new ArgumentNullException (nameof (result));

				var success = SkiaApi.sk_opbuilder_resolve (Handle, result.Handle);
				GC.KeepAlive (result);
				GC.KeepAlive (this);
				return success;
			}

			/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKPath.OpBuilder" /> and optionally releases the managed resources.</summary>
			/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
			/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKPath.OpBuilder" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
			protected override void Dispose (bool disposing) =>
				base.Dispose (disposing);

			/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_opbuilder_destroy (Handle);
		}
	}
}
