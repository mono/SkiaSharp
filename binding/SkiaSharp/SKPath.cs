#nullable disable

using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public enum SKPathConvexity
	{
		Unknown = 0,
		Convex = 1,
		Concave = 2,
	}

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
		public override IntPtr Handle {
			get {
				if (!IsDisposed)
					FlushBuilder ();
				return base.Handle;
			}
			protected set => base.Handle = value;
		}

		public SKPath ()
			: this (SkiaApi.sk_path_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPath instance.");
			}
		}

		public SKPath (SKPath path)
			: this (SkiaApi.sk_path_clone (path.Handle), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKPath instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative ()
		{
			_builder?.Dispose ();
			_builder = null;
			SkiaApi.sk_path_delete (Handle);
		}

		public SKPathFillType FillType {
			get => SkiaApi.sk_path_get_filltype (Handle);
			set {
				SkiaApi.sk_path_set_filltype (Handle, value);
				if (_builder != null)
					_builder.FillType = value;
			}
		}

		public SKPathConvexity Convexity { get { return IsConvex ? SKPathConvexity.Convex : SKPathConvexity.Concave; } }

		public bool IsConvex { get { return SkiaApi.sk_path_is_convex (Handle); } }

		public bool IsConcave => !IsConvex;

		public bool IsEmpty => VerbCount == 0;

		public bool IsOval { get { return SkiaApi.sk_path_is_oval (Handle, null); } }

		public bool IsRoundRect { get { return SkiaApi.sk_path_is_rrect (Handle, IntPtr.Zero); } }

		public bool IsLine { get { return SkiaApi.sk_path_is_line (Handle, null); } }

		public bool IsRect { get { return SkiaApi.sk_path_is_rect (Handle, null, null, null); } }

		public SKPathSegmentMask SegmentMasks { get { return (SKPathSegmentMask)SkiaApi.sk_path_get_segment_masks (Handle); } }

		public int VerbCount { get { return SkiaApi.sk_path_count_verbs (Handle); } }

		public int PointCount { get { return SkiaApi.sk_path_count_points (Handle); } }

		public SKPoint this[int index] => GetPoint (index);

		public SKPoint[] Points { get { return GetPoints (PointCount); } }

		public SKPoint LastPoint {
			get {
				SKPoint point;
				SkiaApi.sk_path_get_last_point (Handle, &point);
				return point;
			}
		}

		public SKRect Bounds {
			get {
				SKRect rect;
				SkiaApi.sk_path_get_bounds (Handle, &rect);
				return rect;
			}
		}

		public SKRect TightBounds {
			get {
				if (GetTightBounds (out var rect)) {
					return rect;
				} else {
					return SKRect.Empty;
				}
			}
		}

		public SKRect GetOvalBounds ()
		{
			SKRect bounds;
			if (SkiaApi.sk_path_is_oval (Handle, &bounds)) {
				return bounds;
			} else {
				return SKRect.Empty;
			}
		}

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

		public SKRect GetRect () =>
			GetRect (out var isClosed, out var direction);

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

		public SKPoint GetPoint (int index)
		{
			if (index < 0 || index >= PointCount)
				throw new ArgumentOutOfRangeException (nameof (index));

			SKPoint point;
			SkiaApi.sk_path_get_point (Handle, index, &point);
			return point;
		}

		public SKPoint[] GetPoints (int max)
		{
			var points = new SKPoint[max];
			GetPoints (points, max);
			return points;
		}

		public int GetPoints (SKPoint[] points, int max)
		{
			fixed (SKPoint* p = points) {
				return SkiaApi.sk_path_get_points (Handle, p, max);
			}
		}

		public bool Contains (float x, float y)
		{
			return SkiaApi.sk_path_contains (Handle, x, y);
		}

		public void Offset (SKPoint offset) =>
			Offset (offset.X, offset.Y);

		public void Offset (float dx, float dy)
		{
			var matrix = SKMatrix.CreateTranslation (dx, dy);
			Transform (in matrix);
		}

		public void Reset ()
		{
			if (_builder != null) {
				_builder.Dispose ();
				_builder = null;
			}
			SkiaApi.sk_path_reset (Handle);
		}

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

		[Obsolete("Use Transform(in SKMatrix) instead.", true)]
		public void Transform (SKMatrix matrix) =>
			Transform (in matrix);

		[Obsolete("Use Transform(in SKMatrix matrix, SKPath destination) instead.", true)]
		public void Transform (SKMatrix matrix, SKPath destination) =>
			Transform (in matrix, destination);

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

		public bool Simplify (SKPath result)
		{
			if (result == null)
				throw new ArgumentNullException (nameof (result));

			return SkiaApi.sk_pathop_simplify (Handle, result.Handle);
		}

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

		public bool GetTightBounds (out SKRect result)
		{
			fixed (SKRect* r = &result) {
				return SkiaApi.sk_pathop_tight_bounds (Handle, r);
			}
		}

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

		public string ToSvgPathData ()
		{
			using var str = new SKString ();
			SkiaApi.sk_path_to_svg_string (Handle, str.Handle);
			return (string)str;
		}

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

		public static SKPoint[] ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, int pow2)
		{
			ConvertConicToQuads (p0, p1, p2, w, out var pts, pow2);
			return pts;
		}

		public static int ConvertConicToQuads (SKPoint p0, SKPoint p1, SKPoint p2, float w, out SKPoint[] pts, int pow2)
		{
			var quadCount = 1 << pow2;
			var ptCount = 2 * quadCount + 1;
			pts = new SKPoint[ptCount];
			return ConvertConicToQuads (p0, p1, p2, w, pts, pow2);
		}

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
			Handle = newHandle;
		}

		internal void ReplaceFromBuilder (SKPathBuilder builder)
		{
			if (_builder != null) {
				_builder.Dispose ();
				_builder = null;
			}
			var newHandle = SkiaApi.sk_pathbuilder_detach_path (builder.Handle);
			SkiaApi.sk_path_delete (Handle);
			Handle = newHandle;
		}

		#region Deprecated Mutation Methods

		// Move

		[Obsolete ("Use SKPathBuilder instead.")]
		public void MoveTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.MoveTo (point);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void MoveTo (float x, float y)
		{
			EnsureBuilder ();
			_builder.MoveTo (x, y);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RMoveTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.RMoveTo (point);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RMoveTo (float dx, float dy)
		{
			EnsureBuilder ();
			_builder.RMoveTo (dx, dy);
		}

		// Line

		[Obsolete ("Use SKPathBuilder instead.")]
		public void LineTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.LineTo (point);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void LineTo (float x, float y)
		{
			EnsureBuilder ();
			_builder.LineTo (x, y);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RLineTo (SKPoint point)
		{
			EnsureBuilder ();
			_builder.RLineTo (point);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RLineTo (float dx, float dy)
		{
			EnsureBuilder ();
			_builder.RLineTo (dx, dy);
		}

		// Quad

		[Obsolete ("Use SKPathBuilder instead.")]
		public void QuadTo (SKPoint point0, SKPoint point1)
		{
			EnsureBuilder ();
			_builder.QuadTo (point0, point1);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			EnsureBuilder ();
			_builder.QuadTo (x0, y0, x1, y1);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RQuadTo (SKPoint point0, SKPoint point1)
		{
			EnsureBuilder ();
			_builder.RQuadTo (point0, point1);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			EnsureBuilder ();
			_builder.RQuadTo (dx0, dy0, dx1, dy1);
		}

		// Conic

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ConicTo (SKPoint point0, SKPoint point1, float w)
		{
			EnsureBuilder ();
			_builder.ConicTo (point0, point1, w);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			EnsureBuilder ();
			_builder.ConicTo (x0, y0, x1, y1, w);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RConicTo (SKPoint point0, SKPoint point1, float w)
		{
			EnsureBuilder ();
			_builder.RConicTo (point0, point1, w);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			EnsureBuilder ();
			_builder.RConicTo (dx0, dy0, dx1, dy1, w);
		}

		// Cubic

		[Obsolete ("Use SKPathBuilder instead.")]
		public void CubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			EnsureBuilder ();
			_builder.CubicTo (point0, point1, point2);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			EnsureBuilder ();
			_builder.CubicTo (x0, y0, x1, y1, x2, y2);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RCubicTo (SKPoint point0, SKPoint point1, SKPoint point2)
		{
			EnsureBuilder ();
			_builder.RCubicTo (point0, point1, point2);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			EnsureBuilder ();
			_builder.RCubicTo (dx0, dy0, dx1, dy1, dx2, dy2);
		}

		// Arc

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			EnsureBuilder ();
			_builder.ArcTo (r, xAxisRotate, largeArc, sweep, xy);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			EnsureBuilder ();
			_builder.ArcTo (rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo)
		{
			EnsureBuilder ();
			_builder.ArcTo (oval, startAngle, sweepAngle, forceMoveTo);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (SKPoint point1, SKPoint point2, float radius)
		{
			EnsureBuilder ();
			_builder.ArcTo (point1, point2, radius);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void ArcTo (float x1, float y1, float x2, float y2, float radius)
		{
			EnsureBuilder ();
			_builder.ArcTo (x1, y1, x2, y2, radius);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RArcTo (SKPoint r, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, SKPoint xy)
		{
			EnsureBuilder ();
			_builder.RArcTo (r, xAxisRotate, largeArc, sweep, xy);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void RArcTo (float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y)
		{
			EnsureBuilder ();
			_builder.RArcTo (rx, ry, xAxisRotate, largeArc, sweep, x, y);
		}

		// Close

		[Obsolete ("Use SKPathBuilder instead.")]
		public void Close ()
		{
			EnsureBuilder ();
			_builder.Close ();
		}

		// Rewind

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

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRect (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRect (rect, direction);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			EnsureBuilder ();
			_builder.AddRect (rect, direction, startIndex);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, direction);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRoundRect rect, SKPathDirection direction, uint startIndex)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, direction, startIndex);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddRoundRect (SKRect rect, float rx, float ry, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddRoundRect (rect, rx, ry, dir);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddOval (SKRect rect, SKPathDirection direction = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddOval (rect, direction);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			EnsureBuilder ();
			_builder.AddArc (oval, startAngle, sweepAngle);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddCircle (float x, float y, float radius, SKPathDirection dir = SKPathDirection.Clockwise)
		{
			EnsureBuilder ();
			_builder.AddCircle (x, y, radius, dir);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPoly (SKPoint[] points, bool close = true)
		{
			EnsureBuilder ();
			_builder.AddPoly (points, close);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPoly (ReadOnlySpan<SKPoint> points, bool close = true)
		{
			EnsureBuilder ();
			_builder.AddPoly (points, close);
		}

		// Add path

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, float dx, float dy, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, dx, dy, mode);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, in SKMatrix matrix, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, in matrix, mode);
		}

		[Obsolete ("Use SKPathBuilder instead.")]
		public void AddPath (SKPath other, SKPathAddMode mode = SKPathAddMode.Append)
		{
			EnsureBuilder ();
			_builder.AddPath (other, mode);
		}

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

			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

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

			public float ConicWeight () =>
				SkiaApi.sk_path_iter_conic_weight (Handle);

			public bool IsCloseLine () =>
				SkiaApi.sk_path_iter_is_close_line (Handle) != 0;

			public bool IsCloseContour () =>
				SkiaApi.sk_path_iter_is_closed_contour (Handle) != 0;
		}

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

			public SKPathVerb Next (SKPoint[] points) =>
				Next (new Span<SKPoint> (points));

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

			public float ConicWeight () =>
				SkiaApi.sk_path_rawiter_conic_weight (Handle);

			public SKPathVerb Peek () =>
				SkiaApi.sk_path_rawiter_peek (Handle);
		}

		public class OpBuilder : SKObject, ISKSkipObjectRegistration
		{
			public OpBuilder ()
				: base (SkiaApi.sk_opbuilder_new (), true)
			{
			}

			public void Add (SKPath path, SKPathOp op) =>
				SkiaApi.sk_opbuilder_add (Handle, path.Handle, op);

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
