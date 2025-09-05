#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// Encapsulates the geometric region used to specify clipping areas for drawing.
	/// </summary>
	public unsafe class SKRegion : SKObject, ISKSkipObjectRegistration
	{
		internal SKRegion (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>
		/// Creates an empty region.
		/// </summary>
		public SKRegion ()
			: this (SkiaApi.sk_region_new (), true)
		{
		}

		/// <summary>
		/// Creates a new region by copying an existing region.
		/// </summary>
		/// <param name="region">The region to copy.</param>
		public SKRegion (SKRegion region)
			: this ()
		{
			SetRegion (region);
		}

		/// <summary>
		/// Creates a new region using the area described by the rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to use as the region.</param>
		public SKRegion (SKRectI rect)
			: this ()
		{
			SetRect (rect);
		}

		/// <summary>
		/// Creates a new region using the area described by the path.
		/// </summary>
		/// <param name="path">The path to use as the region.</param>
		public SKRegion (SKPath path)
			: this ()
		{
			SetPath (path);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_region_delete (Handle);

		// properties

		public bool IsEmpty =>
			SkiaApi.sk_region_is_empty (Handle);

		public bool IsRect =>
			SkiaApi.sk_region_is_rect (Handle);

		public bool IsComplex =>
			SkiaApi.sk_region_is_complex (Handle);

		/// <summary>
		/// Gets the bounds of this region.
		/// </summary>
		/// <remarks>If the region is empty, returns an empty rectangle.</remarks>
		public SKRectI Bounds {
			get {
				SKRectI rect;
				SkiaApi.sk_region_get_bounds (Handle, &rect);
				return rect;
			}
		}

		// GetBoundaryPath

		public SKPath GetBoundaryPath ()
		{
			var path = new SKPath ();
			if (!SkiaApi.sk_region_get_boundary_path (Handle, path.Handle)) {
				path.Dispose ();
				path = null;
			}
			return path;
		}

		// Contains

		/// <summary>
		/// Check to see if the specified path is completely inside the current region.
		/// </summary>
		/// <param name="path">The path to check with.</param>
		/// <returns>Returns true if the specified path is completely inside the current region, otherwise false.</returns>
		/// <remarks>This works for simple (rectangular) and complex path, and always returns the correct result. If either the path or the region is empty, this method returns false.</remarks>
		public bool Contains (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Contains (pathRegion);
		}

		/// <summary>
		/// Check to see if the specified region is completely inside the current region.
		/// </summary>
		/// <param name="src">The region to check with.</param>
		/// <returns>Returns true if the specified region is completely inside the current region, otherwise false.</returns>
		/// <remarks>This works for simple (rectangular) and complex regions, and always returns the correct result. If either region is empty, this method returns false.</remarks>
		public bool Contains (SKRegion src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_region_contains (Handle, src.Handle);
		}

		/// <summary>
		/// Check to see if the specified coordinates are completely inside the current region.
		/// </summary>
		/// <param name="xy">The coordinates to check with.</param>
		/// <returns>Returns true if the specified coordinates are completely inside the current region, otherwise false.</returns>
		public bool Contains (SKPointI xy) =>
			SkiaApi.sk_region_contains_point (Handle, xy.X, xy.Y);

		/// <summary>
		/// Check to see if the specified coordinates are completely inside the current region.
		/// </summary>
		/// <param name="x">The x-coordinate to check with.</param>
		/// <param name="y">The y-coordinate to check with.</param>
		/// <returns>Returns true if the specified coordinates are completely inside the current region, otherwise false.</returns>
		public bool Contains (int x, int y) =>
			SkiaApi.sk_region_contains_point (Handle, x, y);

		/// <summary>
		/// Check to see if the specified rect is completely inside the current region.
		/// </summary>
		/// <param name="rect">The rect to check with.</param>
		/// <returns>Returns true if the specified rect is completely inside the current region, otherwise false.</returns>
		/// <remarks>If either the rect or the region is empty, this method returns false.</remarks>
		public bool Contains (SKRectI rect) =>
			SkiaApi.sk_region_contains_rect (Handle, &rect);

		// QuickContains

		/// <param name="rect"></param>
		public bool QuickContains (SKRectI rect) =>
			SkiaApi.sk_region_quick_contains (Handle, &rect);

		// QuickReject

		/// <param name="rect"></param>
		public bool QuickReject (SKRectI rect) =>
			SkiaApi.sk_region_quick_reject_rect (Handle, &rect);

		/// <param name="region"></param>
		public bool QuickReject (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_quick_reject (Handle, region.Handle);
		}

		/// <param name="path"></param>
		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return QuickReject (pathRegion);
		}

		// Intersects

		/// <summary>
		/// Check to see if the specified path intersects with the current region.
		/// </summary>
		/// <param name="path">The path to check with.</param>
		/// <returns>Returns true if the specified path has a non-empty intersection with the current region.</returns>
		public bool Intersects (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Intersects (pathRegion);
		}

		/// <summary>
		/// Check to see if the specified region intersects with the current region.
		/// </summary>
		/// <param name="region">The region to check with.</param>
		/// <returns>Returns true if the specified region has a non-empty intersection with the current region.</returns>
		public bool Intersects (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_intersects (Handle, region.Handle);
		}

		/// <summary>
		/// Check to see if the specified rectangle intersects with the current region.
		/// </summary>
		/// <param name="rect">The rectangle to check with.</param>
		/// <returns>Returns true if the specified rectangle has a non-empty intersection with the current region.</returns>
		public bool Intersects (SKRectI rect) =>
			SkiaApi.sk_region_intersects_rect (Handle, &rect);

		// Set*

		public void SetEmpty () =>
			SkiaApi.sk_region_set_empty (Handle);

		/// <summary>
		/// Set this region to the specified region.
		/// </summary>
		/// <param name="region">The replacement region.</param>
		/// <returns>Return true if the resulting region is non-empty.</returns>
		public bool SetRegion (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_set_region (Handle, region.Handle);
		}

		/// <summary>
		/// Set this region to the specified rectangle.
		/// </summary>
		/// <param name="rect">The replacement rectangle.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		public bool SetRect (SKRectI rect) =>
			SkiaApi.sk_region_set_rect (Handle, &rect);

		public bool SetRects (ReadOnlySpan<SKRectI> rects)
		{
			if (rects == null)
				throw new ArgumentNullException (nameof (rects));

			fixed (SKRectI* r = rects) {
				return SkiaApi.sk_region_set_rects (Handle, r, rects.Length);
			}
		}

		/// <summary>
		/// Set this region to the area described by the path, clipped.
		/// </summary>
		/// <param name="path">The replacement path.</param>
		/// <param name="clip">The clipping region.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		/// <remarks>This produces a region that is identical to the pixels that would be drawn by the path (with no anti-aliasing) with the specified clip.</remarks>
		public bool SetPath (SKPath path, SKRegion clip)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (clip == null)
				throw new ArgumentNullException (nameof (clip));

			return SkiaApi.sk_region_set_path (Handle, path.Handle, clip.Handle);
		}

		/// <summary>
		/// Set this region to the area described by the path, clipped to the current region.
		/// </summary>
		/// <param name="path">The replacement path.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		/// <remarks>This produces a region that is identical to the pixels that would be drawn by the path (with no anti-aliasing) with the current region as the clip.</remarks>
		public bool SetPath (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var clip = new SKRegion ();
			var rect = SKRectI.Ceiling (path.Bounds);
			if (!rect.IsEmpty)
				clip.SetRect (rect);

			return SkiaApi.sk_region_set_path (Handle, path.Handle, clip.Handle);
		}

		// Translate

		/// <param name="x"></param>
		/// <param name="y"></param>
		public void Translate (int x, int y) =>
			SkiaApi.sk_region_translate (Handle, x, y);

		// Op

		/// <summary>
		/// Set this region to the result of applying the operation to this region and the specified rectangle.
		/// </summary>
		/// <param name="rect">The rectangle to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		public bool Op (SKRectI rect, SKRegionOperation op) =>
			SkiaApi.sk_region_op_rect (Handle, &rect, op);

		/// <summary>
		/// Set this region to the result of applying the operation to this region and the specified rectangle.
		/// </summary>
		/// <param name="left">The x-coordinate to apply the operator on.</param>
		/// <param name="top">The y-coordinate to apply the operator on.</param>
		/// <param name="right">The right-coordinate to apply the operator on.</param>
		/// <param name="bottom">The bottom-coordinate to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		public bool Op (int left, int top, int right, int bottom, SKRegionOperation op) =>
			Op (new SKRectI (left, top, right, bottom), op);

		/// <summary>
		/// Set this region to the result of applying the operation to this region and the specified region.
		/// </summary>
		/// <param name="region">The region to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		public bool Op (SKRegion region, SKRegionOperation op) =>
			SkiaApi.sk_region_op (Handle, region.Handle, op);

		/// <summary>
		/// Set this region to the result of applying the operation to this region and the specified path.
		/// </summary>
		/// <param name="path">The path to apply the operator on.</param>
		/// <param name="op">The operator to apply.</param>
		/// <returns>Returns true if the resulting region is non-empty.</returns>
		public bool Op (SKPath path, SKRegionOperation op)
		{
			using var pathRegion = new SKRegion (path);
			return Op (pathRegion, op);
		}

		// Iterators

		public RectIterator CreateRectIterator () =>
			new RectIterator (this);

		/// <param name="clip"></param>
		public ClipIterator CreateClipIterator (SKRectI clip) =>
			new ClipIterator (this, clip);

		/// <param name="y"></param>
		/// <param name="left"></param>
		/// <param name="right"></param>
		public SpanIterator CreateSpanIterator (int y, int left, int right) =>
			new SpanIterator (this, y, left, right);

		// classes

		public class RectIterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKRegion region;

			internal RectIterator (SKRegion region)
				: base (SkiaApi.sk_region_iterator_new (region.Handle), true)
			{
				this.region = region;
			}

			protected override void DisposeNative () =>
				SkiaApi.sk_region_iterator_delete (Handle);

			/// <param name="rect"></param>
			public bool Next (out SKRectI rect)
			{
				if (SkiaApi.sk_region_iterator_done (Handle)) {
					rect = SKRectI.Empty;
					return false;
				}

				fixed (SKRectI* r = &rect) {
					SkiaApi.sk_region_iterator_rect (Handle, r);
				}

				SkiaApi.sk_region_iterator_next (Handle);

				return true;
			}
		}

		public class ClipIterator : SKObject, ISKSkipObjectRegistration
		{
			private readonly SKRegion region;
			private readonly SKRectI clip;

			internal ClipIterator (SKRegion region, SKRectI clip)
				: base (SkiaApi.sk_region_cliperator_new (region.Handle, &clip), true)
			{
				this.region = region;
				this.clip = clip;
			}

			protected override void DisposeNative () =>
				SkiaApi.sk_region_cliperator_delete (Handle);

			/// <param name="rect"></param>
			public bool Next (out SKRectI rect)
			{
				if (SkiaApi.sk_region_cliperator_done (Handle)) {
					rect = SKRectI.Empty;
					return false;
				}

				fixed (SKRectI* r = &rect) {
					SkiaApi.sk_region_iterator_rect (Handle, r);
				}

				SkiaApi.sk_region_cliperator_next (Handle);

				return true;
			}
		}

		public class SpanIterator : SKObject, ISKSkipObjectRegistration
		{
			internal SpanIterator (SKRegion region, int y, int left, int right)
				: base (SkiaApi.sk_region_spanerator_new (region.Handle, y, left, right), true)
			{
			}

			protected override void DisposeNative () =>
				SkiaApi.sk_region_spanerator_delete (Handle);

			/// <param name="left"></param>
			/// <param name="right"></param>
			public bool Next (out int left, out int right)
			{
				int l;
				int r;
				if (SkiaApi.sk_region_spanerator_next (Handle, &l, &r)) {
					left = l;
					right = r;
					return true;
				}

				left = 0;
				right = 0;
				return false;
			}
		}
	}
}
