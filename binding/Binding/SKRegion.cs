using System;

namespace SkiaSharp
{
	public unsafe class SKRegion : SKObject, ISKSkipObjectRegistration
	{
		internal SKRegion (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKRegion ()
			: this (SkiaApi.sk_region_new (), true)
		{
		}

		public SKRegion (SKRegion region)
			: this ()
		{
			SetRegion (region);
		}

		public SKRegion (SKRectI rect)
			: this ()
		{
			SetRect (rect);
		}

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

		public bool Contains (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Contains (pathRegion);
		}

		public bool Contains (SKRegion src)
		{
			if (src == null)
				throw new ArgumentNullException (nameof (src));

			return SkiaApi.sk_region_contains (Handle, src.Handle);
		}

		public bool Contains (SKPointI xy) =>
			SkiaApi.sk_region_contains_point (Handle, xy.X, xy.Y);

		public bool Contains (int x, int y) =>
			SkiaApi.sk_region_contains_point (Handle, x, y);

		public bool Contains (SKRectI rect) =>
			SkiaApi.sk_region_contains_rect (Handle, &rect);

		// QuickContains

		public bool QuickContains (SKRectI rect) =>
			SkiaApi.sk_region_quick_contains (Handle, &rect);

		// QuickReject

		public bool QuickReject (SKRectI rect) =>
			SkiaApi.sk_region_quick_reject_rect (Handle, &rect);

		public bool QuickReject (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_quick_reject (Handle, region.Handle);
		}

		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return QuickReject (pathRegion);
		}

		// Intersects

		public bool Intersects (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			using var pathRegion = new SKRegion (path);
			return Intersects (pathRegion);
		}

		public bool Intersects (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_intersects (Handle, region.Handle);
		}

		public bool Intersects (SKRectI rect) =>
			SkiaApi.sk_region_intersects_rect (Handle, &rect);

		// Set*

		public void SetEmpty () =>
			SkiaApi.sk_region_set_empty (Handle);

		public bool SetRegion (SKRegion region)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			return SkiaApi.sk_region_set_region (Handle, region.Handle);
		}

		public bool SetRect (SKRectI rect) =>
			SkiaApi.sk_region_set_rect (Handle, &rect);

		public bool SetRects (SKRectI[] rects)
		{
			if (rects == null)
				throw new ArgumentNullException (nameof (rects));

			fixed (SKRectI* r = rects) {
				return SkiaApi.sk_region_set_rects (Handle, r, rects.Length);
			}
		}

		public bool SetPath (SKPath path, SKRegion clip)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (clip == null)
				throw new ArgumentNullException (nameof (clip));

			return SkiaApi.sk_region_set_path (Handle, path.Handle, clip.Handle);
		}

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

		public void Translate (int x, int y) =>
			SkiaApi.sk_region_translate (Handle, x, y);

		// Op

		public bool Op (SKRectI rect, SKRegionOperation op) =>
			SkiaApi.sk_region_op_rect (Handle, &rect, op);

		public bool Op (int left, int top, int right, int bottom, SKRegionOperation op) =>
			Op (new SKRectI (left, top, right, bottom), op);

		public bool Op (SKRegion region, SKRegionOperation op) =>
			SkiaApi.sk_region_op (Handle, region.Handle, op);

		public bool Op (SKPath path, SKRegionOperation op)
		{
			using var pathRegion = new SKRegion (path);
			return Op (pathRegion, op);
		}

		// Iterators

		public RectIterator CreateRectIterator () =>
			new RectIterator (this);

		public ClipIterator CreateClipIterator (SKRectI clip) =>
			new ClipIterator (this, clip);

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
