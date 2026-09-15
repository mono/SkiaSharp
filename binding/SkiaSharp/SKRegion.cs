#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Encapsulates the geometric region used to specify clipping areas for drawing.</summary>
	/// <remarks />
	public unsafe class SKRegion : SKObject, ISKSkipObjectRegistration
	{
		internal SKRegion (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates an empty region.</summary>
		/// <remarks />
		public SKRegion ()
			: this (SkiaApi.sk_region_new (), true)
		{
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRegion" /> class by copying an existing region.</summary>
		/// <param name="region">The region to copy.</param>
		/// <remarks />
		public SKRegion (SKRegion region)
			: this ()
		{
			SetRegion (region);
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRegion" /> class using the area described by the rectangle.</summary>
		/// <param name="rect">The rectangle to use as the region.</param>
		/// <remarks />
		public SKRegion (SKRectI rect)
			: this ()
		{
			SetRect (rect);
		}

		/// <summary>Initializes a new instance of the <see cref="T:SkiaSharp.SKRegion" /> class using the area described by the path.</summary>
		/// <param name="path">The path to use as the region.</param>
		/// <remarks />
		public SKRegion (SKPath path)
			: this ()
		{
			SetPath (path);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKRegion" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKRegion" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_region_delete (Handle);

		// properties

		/// <summary>Gets a value indicating whether the region is empty.</summary>
		/// <value><see langword="true" /> if the region is empty; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsEmpty {
			get {
				var result = SkiaApi.sk_region_is_empty (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether the region is a single rectangle.</summary>
		/// <value><see langword="true" /> if the region is a single rectangle; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsRect {
			get {
				var result = SkiaApi.sk_region_is_rect (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether the region is complex (more than a single rectangle).</summary>
		/// <value><see langword="true" /> if the region is complex; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsComplex {
			get {
				var result = SkiaApi.sk_region_is_complex (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the bounds of this region.</summary>
		/// <value>The bounding rectangle of the region.</value>
		/// <remarks>If the region is empty, returns an empty rectangle.</remarks>
		public SKRectI Bounds {
			get {
				SKRectI rect;
				SkiaApi.sk_region_get_bounds (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		// GetBoundaryPath

		/// <summary>Returns a path that describes the boundary of the region.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKPath" /> representing the region boundary, or <see langword="null" /> if the region is empty.</returns>
		/// <remarks />
		public SKPath GetBoundaryPath ()
		{
			var path = new SKPath ();
			if (!SkiaApi.sk_region_get_boundary_path (Handle, path.Handle)) {
				path.Dispose ();
				path = null;
			}
			GC.KeepAlive (this);
			return path;
		}

		// Contains

		/// <summary>Check to see if the specified path is completely inside the current region.</summary>
		/// <param name="path">The path to check with.</param>
		/// <returns><see langword="true" /> if the specified path is completely inside the current region; otherwise, <see langword="false" />.</returns>
		/// <remarks>This works for simple (rectangular) and complex path, and always returns the correct result. If either the path or the region is empty, this method returns <see langword="false" />.</remarks>
		public bool Contains (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Contains (pathRegion);
		}

		/// <summary>Check to see if the specified region is completely inside the current region.</summary>
		/// <param name="src">The region to check with.</param>
		/// <returns><see langword="true" /> if the specified region is completely inside the current region; otherwise, <see langword="false" />.</returns>
		/// <remarks>This works for simple (rectangular) and complex regions, and always returns the correct result. If either region is empty, this method returns <see langword="false" />.</remarks>
		public bool Contains (SKRegion src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			var result = SkiaApi.sk_region_contains (Handle, src.Handle);
			GC.KeepAlive (src);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Check to see if the specified coordinates are completely inside the current region.</summary>
		/// <param name="xy">The coordinates to check with.</param>
		/// <returns><see langword="true" /> if the specified coordinates are completely inside the current region; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (SKPointI xy)
		{
			var result = SkiaApi.sk_region_contains_point (Handle, xy.X, xy.Y);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Check to see if the specified coordinates are completely inside the current region.</summary>
		/// <param name="x">The x-coordinate to check with.</param>
		/// <param name="y">The y-coordinate to check with.</param>
		/// <returns><see langword="true" /> if the specified coordinates are completely inside the current region; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (int x, int y)
		{
			var result = SkiaApi.sk_region_contains_point (Handle, x, y);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Check to see if the specified rect is completely inside the current region.</summary>
		/// <param name="rect">The rect to check with.</param>
		/// <returns><see langword="true" /> if the specified rect is completely inside the current region; otherwise, <see langword="false" />.</returns>
		/// <remarks>If either the rect or the region is empty, this method returns <see langword="false" />.</remarks>
		public bool Contains (SKRectI rect)
		{
			var result = SkiaApi.sk_region_contains_rect (Handle, &rect);
			GC.KeepAlive (this);
			return result;
		}

		// QuickContains

		/// <summary>Quickly checks if the specified rectangle is completely inside the region.</summary>
		/// <param name="rect">The rectangle to check.</param>
		/// <returns><see langword="true" /> if the rectangle is completely contained; otherwise, <see langword="false" />.</returns>
		/// <remarks>This is an optimized check that may return <see langword="false" /> even when the rectangle is contained, but never returns <see langword="true" /> if the rectangle is not contained.</remarks>
		public bool QuickContains (SKRectI rect)
		{
			var result = SkiaApi.sk_region_quick_contains (Handle, &rect);
			GC.KeepAlive (this);
			return result;
		}

		// QuickReject

		/// <summary>Quickly checks if the specified rectangle does not intersect the region.</summary>
		/// <param name="rect">The rectangle to check.</param>
		/// <returns><see langword="true" /> if the rectangle definitely does not intersect; otherwise, <see langword="false" />.</returns>
		/// <remarks>This is an optimized check that may return <see langword="false" /> even when there is no intersection, but never returns <see langword="true" /> if there is an intersection.</remarks>
		public bool QuickReject (SKRectI rect)
		{
			var result = SkiaApi.sk_region_quick_reject_rect (Handle, &rect);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Quickly checks if the specified region does not intersect the current region.</summary>
		/// <param name="region">The region to check.</param>
		/// <returns><see langword="true" /> if the region definitely does not intersect; otherwise, <see langword="false" />.</returns>
		/// <remarks>This is an optimized check that may return <see langword="false" /> even when there is no intersection, but never returns <see langword="true" /> if there is an intersection.</remarks>
		public bool QuickReject (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			var result = SkiaApi.sk_region_quick_reject (Handle, region.Handle);
			GC.KeepAlive (region);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Quickly checks if the specified path does not intersect the region.</summary>
		/// <param name="path">The path to check.</param>
		/// <returns><see langword="true" /> if the path definitely does not intersect; otherwise, <see langword="false" />.</returns>
		/// <remarks>This is an optimized check that may return <see langword="false" /> even when there is no intersection, but never returns <see langword="true" /> if there is an intersection.</remarks>
		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return QuickReject (pathRegion);
		}

		// Intersects

		/// <summary>Check to see if the specified path intersects with the current region.</summary>
		/// <param name="path">The path to check with.</param>
		/// <returns><see langword="true" /> if the specified path has a non-empty intersection with the current region.</returns>
		/// <remarks />
		public bool Intersects (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Intersects (pathRegion);
		}

		/// <summary>Check to see if the specified region intersects with the current region.</summary>
		/// <param name="region">The region to check with.</param>
		/// <returns><see langword="true" /> if the specified region has a non-empty intersection with the current region.</returns>
		/// <remarks />
		public bool Intersects (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			var result = SkiaApi.sk_region_intersects (Handle, region.Handle);
			GC.KeepAlive (region);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Check to see if the specified rectangle intersects with the current region.</summary>
		/// <param name="rect">The rectangle to check with.</param>
		/// <returns><see langword="true" /> if the specified rectangle has a non-empty intersection with the current region.</returns>
		/// <remarks />
		public bool Intersects (SKRectI rect)
		{
			var result = SkiaApi.sk_region_intersects_rect (Handle, &rect);
			GC.KeepAlive (this);
			return result;
		}

		// Set*

		/// <summary>Sets the region to be empty.</summary>
		/// <remarks />
		public void SetEmpty ()
		{
			SkiaApi.sk_region_set_empty (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this region to the specified region.</summary>
		/// <param name="region">The replacement region.</param>
		/// <returns>Returns <see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool SetRegion (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			var result = SkiaApi.sk_region_set_region (Handle, region.Handle);
			GC.KeepAlive (region);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Sets this region to the specified rectangle.</summary>
		/// <param name="rect">The replacement rectangle.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool SetRect (SKRectI rect)
		{
			var result = SkiaApi.sk_region_set_rect (Handle, &rect);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Sets the region to the union of the specified rectangles.</summary>
		/// <param name="rects">The span of rectangles to set the region to.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty; otherwise, <see langword="false" />.</returns>
		/// <remarks />
		public bool SetRects (ReadOnlySpan<SKRectI> rects)
		{
			fixed (SKRectI* r = rects) {
				var result = SkiaApi.sk_region_set_rects (Handle, r, rects.Length);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Sets this region to the area described by the path, clipped.</summary>
		/// <param name="path">The replacement path.</param>
		/// <param name="clip">The clipping region.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks>This produces a region that is identical to the pixels that would be drawn by the path (with no anti-aliasing) with the specified clip.</remarks>
		public bool SetPath (SKPath path, SKRegion clip)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (clip == null)
				throw new ArgumentNullException (nameof (clip));

			var result = SkiaApi.sk_region_set_path (Handle, path.Handle, clip.Handle);
			GC.KeepAlive (path);
			GC.KeepAlive (clip);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Sets this region to the area described by the path, clipped to the current region.</summary>
		/// <param name="path">The replacement path.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks>This produces a region that is identical to the pixels that would be drawn by the path (with no anti-aliasing) with the current region as the clip.</remarks>
		public bool SetPath (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var clip = new SKRegion ();
			var rect = SKRectI.Ceiling (path.Bounds);
			if (!rect.IsEmpty)
				clip.SetRect (rect);

			var result = SkiaApi.sk_region_set_path (Handle, path.Handle, clip.Handle);
			GC.KeepAlive (path);
			GC.KeepAlive (this);
			return result;
		}

		// Translate

		/// <summary>Translates the region by the specified offset.</summary>
		/// <param name="x">The horizontal distance to translate.</param>
		/// <param name="y">The vertical distance to translate.</param>
		/// <remarks />
		public void Translate (int x, int y)
		{
			SkiaApi.sk_region_translate (Handle, x, y);
			GC.KeepAlive (this);
		}

		// Op

		/// <summary>Sets this region to the result of applying the operation to this region and the specified rectangle.</summary>
		/// <param name="rect">The rectangle to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool Op (SKRectI rect, SKRegionOperation op)
		{
			var result = SkiaApi.sk_region_op_rect (Handle, &rect, op);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Sets this region to the result of applying the operation to this region and the specified rectangle.</summary>
		/// <param name="left">The x-coordinate to apply the operator on.</param>
		/// <param name="top">The y-coordinate to apply the operator on.</param>
		/// <param name="right">The right-coordinate to apply the operator on.</param>
		/// <param name="bottom">The bottom-coordinate to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool Op (int left, int top, int right, int bottom, SKRegionOperation op) =>
			Op (new SKRectI (left, top, right, bottom), op);

		/// <summary>Sets this region to the result of applying the operation to this region and the specified region.</summary>
		/// <param name="region">The region to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool Op (SKRegion region, SKRegionOperation op)
		{
			var result = SkiaApi.sk_region_op (Handle, region.Handle, op);
			GC.KeepAlive (region);
			GC.KeepAlive (this);
			return result;
		}

		/// <summary>Sets this region to the result of applying the operation to this region and the specified path.</summary>
		/// <param name="path">The path to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns><see langword="true" /> if the resulting region is non-empty.</returns>
		/// <remarks />
		public bool Op (SKPath path, SKRegionOperation op)
		{
			using var pathRegion = new SKRegion (path);
			return Op (pathRegion, op);
		}

		// Iterators

		/// <summary>Creates an iterator that returns the rectangles that make up the region.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKRegion.RectIterator" /> for iterating over the rectangles.</returns>
		/// <remarks />
		public RectIterator CreateRectIterator () =>
			new RectIterator (this);

		/// <summary>Creates an iterator that returns the rectangles of the region clipped to the specified rectangle.</summary>
		/// <param name="clip">The clipping rectangle to intersect with the region.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRegion.ClipIterator" /> for iterating over the clipped rectangles.</returns>
		/// <remarks />
		public ClipIterator CreateClipIterator (SKRectI clip) =>
			new ClipIterator (this, clip);

		/// <summary>Creates an iterator that returns the horizontal spans of the region intersected with the specified line.</summary>
		/// <param name="y">The y-coordinate of the horizontal line to iterate.</param>
		/// <param name="left">The left bound of the horizontal line.</param>
		/// <param name="right">The right bound of the horizontal line.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKRegion.SpanIterator" /> for iterating over the horizontal spans.</returns>
		/// <remarks />
		public SpanIterator CreateSpanIterator (int y, int left, int right) =>
			new SpanIterator (this, y, left, right);

		// classes

		/// <summary>Iterates over the rectangles that make up a region.</summary>
		/// <remarks />
		public class RectIterator : SKObject, ISKSkipObjectRegistration
		{
			internal RectIterator (SKRegion region)
				: base (SkiaApi.sk_region_iterator_new (region.Handle), true)
			{
				Referenced (this, region);
			}

			/// <summary>This member supports the infrastructure and is not intended to be used directly from your code.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_region_iterator_delete (Handle);

			/// <summary>Advances to the next rectangle and returns it.</summary>
			/// <param name="rect">When this method returns, contains the current rectangle.</param>
			/// <returns><see langword="true" /> if a rectangle was available; otherwise, <see langword="false" />.</returns>
			/// <remarks />
			public bool Next (out SKRectI rect)
			{
				if (SkiaApi.sk_region_iterator_done (Handle)) {
					rect = SKRectI.Empty;
					GC.KeepAlive (this);
					return false;
				}

				fixed (SKRectI* r = &rect) {
					SkiaApi.sk_region_iterator_rect (Handle, r);
				}

				SkiaApi.sk_region_iterator_next (Handle);
				GC.KeepAlive (this);

				return true;
			}
		}

		/// <summary>Iterates over the rectangles of a region clipped to a bounding rectangle.</summary>
		/// <remarks />
		public class ClipIterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKRectI clip;

			internal ClipIterator (SKRegion region, SKRectI clip)
				: base (SkiaApi.sk_region_cliperator_new (region.Handle, &clip), true)
			{
				Referenced (this, region);
				this.clip = clip;
			}

			/// <summary>This member supports the infrastructure and is not intended to be used directly from your code.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_region_cliperator_delete (Handle);

			/// <summary>Advances to the next rectangle and returns it.</summary>
			/// <param name="rect">When this method returns, contains the current rectangle.</param>
			/// <returns><see langword="true" /> if a rectangle was available; otherwise, <see langword="false" />.</returns>
			/// <remarks />
			public bool Next (out SKRectI rect)
			{
				if (SkiaApi.sk_region_cliperator_done (Handle)) {
					rect = SKRectI.Empty;
					GC.KeepAlive (this);
					return false;
				}

				fixed (SKRectI* r = &rect) {
					SkiaApi.sk_region_iterator_rect (Handle, r);
				}

				SkiaApi.sk_region_cliperator_next (Handle);
				GC.KeepAlive (this);

				return true;
			}
		}

		/// <summary>Iterates over the horizontal spans of a region at a specific y-coordinate.</summary>
		/// <remarks />
		public class SpanIterator : SKObject, ISKSkipObjectRegistration
		{
			internal SpanIterator (SKRegion region, int y, int left, int right)
				: base (SkiaApi.sk_region_spanerator_new (region.Handle, y, left, right), true)
			{
				Referenced (this, region);
			}

			/// <summary>This member supports the infrastructure and is not intended to be used directly from your code.</summary>
			/// <remarks />
			protected override void DisposeNative () =>
				SkiaApi.sk_region_spanerator_delete (Handle);

			/// <summary>Advances to the next horizontal span and returns its boundaries.</summary>
			/// <param name="left">When this method returns, contains the left edge of the current span.</param>
			/// <param name="right">When this method returns, contains the right edge of the current span.</param>
			/// <returns><see langword="true" /> if a span was available; otherwise, <see langword="false" />.</returns>
			/// <remarks />
			public bool Next (out int left, out int right)
			{
				int l;
				int r;
				if (SkiaApi.sk_region_spanerator_next (Handle, &l, &r)) {
					left = l;
					right = r;
					GC.KeepAlive (this);
					return true;
				}

				left = 0;
				right = 0;
				GC.KeepAlive (this);
				return false;
			}
		}
	}
}
