using System;

namespace SkiaSharp
{
	public unsafe class SKRoundRect : SKObject, ISKSkipObjectRegistration
	{
		internal SKRoundRect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKRoundRect ()
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetEmpty ();
		}

		public SKRoundRect (SKRect rect)
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetRect (rect);
		}

		public SKRoundRect (SKRect rect, float radius)
			: this (rect, radius, radius)
		{
		}

		public SKRoundRect (SKRect rect, float xRadius, float yRadius)
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetRect (rect, xRadius, yRadius);
		}

		public SKRoundRect (SKRoundRect rrect)
			: this (SkiaApi.sk_rrect_new_copy (rrect.Handle), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_rrect_delete (Handle);

		public SKRect Rect {
			get {
				SKRect rect;
				SkiaApi.sk_rrect_get_rect (Handle, &rect);
				return rect;
			}
		}

		public SKPoint[] Radii => new[] {
			GetRadii(SKRoundRectCorner.UpperLeft),
			GetRadii(SKRoundRectCorner.UpperRight),
			GetRadii(SKRoundRectCorner.LowerRight),
			GetRadii(SKRoundRectCorner.LowerLeft),
		};

		public SKRoundRectType Type => SkiaApi.sk_rrect_get_type (Handle);

		public float Width => SkiaApi.sk_rrect_get_width (Handle);

		public float Height => SkiaApi.sk_rrect_get_height (Handle);

		public bool IsValid => SkiaApi.sk_rrect_is_valid (Handle);

		public bool AllCornersCircular => CheckAllCornersCircular (Utils.NearlyZero);

		public bool CheckAllCornersCircular (float tolerance)
		{
			var ul = GetRadii (SKRoundRectCorner.UpperLeft);
			var ur = GetRadii (SKRoundRectCorner.UpperRight);
			var lr = GetRadii (SKRoundRectCorner.LowerRight);
			var ll = GetRadii (SKRoundRectCorner.LowerLeft);

			return
				Utils.NearlyEqual (ul.X, ul.Y, tolerance) &&
				Utils.NearlyEqual (ur.X, ur.Y, tolerance) &&
				Utils.NearlyEqual (lr.X, lr.Y, tolerance) &&
				Utils.NearlyEqual (ll.X, ll.Y, tolerance);
		}

		public void SetEmpty ()
		{
			SkiaApi.sk_rrect_set_empty (Handle);
		}

		public void SetRect (SKRect rect)
		{
			SkiaApi.sk_rrect_set_rect (Handle, &rect);
		}

		public void SetRect (SKRect rect, float xRadius, float yRadius)
		{
			SkiaApi.sk_rrect_set_rect_xy (Handle, &rect, xRadius, yRadius);
		}

		public void SetOval (SKRect rect)
		{
			SkiaApi.sk_rrect_set_oval (Handle, &rect);
		}

		public void SetNinePatch (SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius)
		{
			SkiaApi.sk_rrect_set_nine_patch (Handle, &rect, leftRadius, topRadius, rightRadius, bottomRadius);
		}

		public void SetRectRadii (SKRect rect, SKPoint[] radii)
		{
			if (radii == null)
				throw new ArgumentNullException (nameof (radii));
			if (radii.Length != 4)
				throw new ArgumentException ("Radii must have a length of 4.", nameof (radii));

			fixed (SKPoint* r = radii) {
				SkiaApi.sk_rrect_set_rect_radii (Handle, &rect, r);
			}
		}

		public bool Contains (SKRect rect)
		{
			return SkiaApi.sk_rrect_contains (Handle, &rect);
		}

		public SKPoint GetRadii (SKRoundRectCorner corner)
		{
			SKPoint radii;
			SkiaApi.sk_rrect_get_radii (Handle, corner, &radii);
			return radii;
		}

		public void Deflate (SKSize size)
		{
			Deflate (size.Width, size.Height);
		}

		public void Deflate (float dx, float dy)
		{
			SkiaApi.sk_rrect_inset (Handle, dx, dy);
		}

		public void Inflate (SKSize size)
		{
			Inflate (size.Width, size.Height);
		}

		public void Inflate (float dx, float dy)
		{
			SkiaApi.sk_rrect_outset (Handle, dx, dy);
		}

		public void Offset (SKPoint pos)
		{
			Offset (pos.X, pos.Y);
		}

		public void Offset (float dx, float dy)
		{
			SkiaApi.sk_rrect_offset (Handle, dx, dy);
		}

		public bool TryTransform (SKMatrix matrix, out SKRoundRect transformed)
		{
			var destHandle = SkiaApi.sk_rrect_new ();
			if (SkiaApi.sk_rrect_transform (Handle, &matrix, destHandle)) {
				transformed = new SKRoundRect (destHandle, true);
				return true;
			}
			SkiaApi.sk_rrect_delete (destHandle);
			transformed = null;
			return false;
		}

		public SKRoundRect Transform (SKMatrix matrix)
		{
			if (TryTransform (matrix, out var transformed)) {
				return transformed;
			}
			return null;
		}
	}
}
