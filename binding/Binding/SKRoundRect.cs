using System;

namespace SkiaSharp
{
	public class SKRoundRect : SKObject
	{
		[Preserve]
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_rrect_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public SKRect Rect {
			get {
				SkiaApi.sk_rrect_get_rect (Handle, out var rect);
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
			SkiaApi.sk_rrect_set_rect (Handle, ref rect);
		}

		public void SetRect (SKRect rect, float xRadius, float yRadius)
		{
			SkiaApi.sk_rrect_set_rect_xy (Handle, ref rect, xRadius, yRadius);
		}

		public void SetOval (SKRect rect)
		{
			SkiaApi.sk_rrect_set_oval (Handle, ref rect);
		}

		public void SetNinePatch (SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius)
		{
			SkiaApi.sk_rrect_set_nine_patch (Handle, ref rect, leftRadius, topRadius, rightRadius, bottomRadius);
		}

		public void SetRectRadii (SKRect rect, SKPoint[] radii)
		{
			if (radii == null)
				throw new ArgumentNullException (nameof (radii));
			if (radii.Length != 4)
				throw new ArgumentException ("Radii must have a length of 4.", nameof (radii));

			SkiaApi.sk_rrect_set_rect_radii (Handle, ref rect, radii);
		}

		public bool Contains (SKRect rect)
		{
			return SkiaApi.sk_rrect_contains (Handle, ref rect);
		}

		public SKPoint GetRadii (SKRoundRectCorner corner)
		{
			SkiaApi.sk_rrect_get_radii (Handle, corner, out var radii);
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

		public SKRoundRect Transform (SKMatrix matrix)
		{
			var destHandle = SkiaApi.sk_rrect_new ();
			if (SkiaApi.sk_rrect_transform (Handle, ref matrix, destHandle)) {
				return new SKRoundRect (destHandle, true);
			}
			return null;
		}
	}
}
