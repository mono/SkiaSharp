#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Represents a rounded rectangle with a potentially different radii for each corner.</summary>
	/// <remarks>If either of a corner's radii are 0 the corner will be square and negative radii are not allowed (they are clamped to zero).</remarks>
	public unsafe class SKRoundRect : SKObject, ISKSkipObjectRegistration
	{
		internal SKRoundRect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKRoundRect" /> with all values initialized to 0.</summary>
		/// <remarks />
		public SKRoundRect ()
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetEmpty ();
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKRoundRect" /> with all radii set to 0.</summary>
		/// <param name="rect">The bounds of the new rectangle.</param>
		/// <remarks />
		public SKRoundRect (SKRect rect)
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetRect (rect);
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKRoundRect" /> with the same radii for all four corners.</summary>
		/// <param name="rect">The bounds of the new rectangle.</param>
		/// <param name="radius">The radii of the corners.</param>
		/// <remarks />
		public SKRoundRect (SKRect rect, float radius)
			: this (rect, radius, radius)
		{
		}

		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SKRoundRect" /> with the same radii for all four corners.</summary>
		/// <param name="rect">The bounds of the new rectangle.</param>
		/// <param name="xRadius">The radii of the corners along the x-axis.</param>
		/// <param name="yRadius">The radii of the corners along the y-axis.</param>
		/// <remarks />
		public SKRoundRect (SKRect rect, float xRadius, float yRadius)
			: this (SkiaApi.sk_rrect_new (), true)
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKRoundRect instance.");
			}
			SetRect (rect, xRadius, yRadius);
		}

		/// <summary>Creates a copy of a <see cref="T:SkiaSharp.SKRoundRect" />.</summary>
		/// <param name="rrect">The rounded rectangle to copy.</param>
		/// <remarks />
		public SKRoundRect (SKRoundRect rrect)
			: this (SkiaApi.sk_rrect_new_copy (rrect.Handle), true)
		{
			GC.KeepAlive (rrect);
		}

		/// <summary>Releases the unmanaged resources used by the <see cref="T:SkiaSharp.SKRoundRect" /> and optionally releases the managed resources.</summary>
		/// <param name="disposing"><see langword="true" /> to release both managed and unmanaged resources; <see langword="false" /> to release only unmanaged resources.</param>
		/// <remarks>Always dispose the object before you release your last reference to the <see cref="T:SkiaSharp.SKRoundRect" />. Otherwise, the resources it is using will not be freed until the garbage collector calls the finalizer.</remarks>
		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>Implemented by derived <see cref="T:SkiaSharp.SKObject" /> types to destroy any native objects.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_rrect_delete (Handle);

		/// <summary>Gets the rectangle bounds of the rounded rectangle.</summary>
		/// <value>The rectangle bounds.</value>
		/// <remarks />
		public SKRect Rect {
			get {
				SKRect rect;
				SkiaApi.sk_rrect_get_rect (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		/// <summary>Gets the radii of the corners.</summary>
		/// <value>An array of corner radii.</value>
		/// <remarks>The order of the corners are clockwise from the top left: Top Left, Top Right, Bottom Right, Bottom Left.</remarks>
		public SKPoint[] Radii => new[] {
			GetRadii(SKRoundRectCorner.UpperLeft),
			GetRadii(SKRoundRectCorner.UpperRight),
			GetRadii(SKRoundRectCorner.LowerRight),
			GetRadii(SKRoundRectCorner.LowerLeft),
		};

		/// <summary>Gets a value indicating what sub-type of rounded rectangle this instance is.</summary>
		/// <value>The type of the rounded rectangle.</value>
		/// <remarks />
		public SKRoundRectType Type {
			get {
				var r = SkiaApi.sk_rrect_get_type (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the width of the rectangle.</summary>
		/// <value>The width of the rectangle.</value>
		/// <remarks />
		public float Width {
			get {
				var r = SkiaApi.sk_rrect_get_width (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets the height of the rectangle.</summary>
		/// <value>The height of the rectangle.</value>
		/// <remarks />
		public float Height {
			get {
				var r = SkiaApi.sk_rrect_get_height (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating whether the rectangle has a valid bounds, radii and type.</summary>
		/// <value><see langword="true" /> if the rectangle is valid; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool IsValid {
			get {
				var r = SkiaApi.sk_rrect_is_valid (Handle);
				GC.KeepAlive (this);
				return r;
			}
		}

		/// <summary>Gets a value indicating whether all four corners are circular (with the x- and y-axis equal).</summary>
		/// <value><see langword="true" /> if all four corners are circular; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool AllCornersCircular => CheckAllCornersCircular (Utils.NearlyZero);

		/// <summary>Check to see whether all four corners are circular (with the x- and y-axis equal).</summary>
		/// <param name="tolerance">The difference in the axis allowed before the corners are no longer circular.</param>
		/// <returns><see langword="true" /> if all corners are circular within the tolerance; otherwise, <see langword="false" />.</returns>
		/// <remarks />
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

		/// <summary>Sets this rounded rectangle to an empty rectangle (with all values 0).</summary>
		/// <remarks />
		public void SetEmpty ()
		{
			SkiaApi.sk_rrect_set_empty (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this rectangle to be a simple rectangle.</summary>
		/// <param name="rect">The simple rectangle.</param>
		/// <remarks />
		public void SetRect (SKRect rect)
		{
			SkiaApi.sk_rrect_set_rect (Handle, &rect);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this rectangle to be a simple rounded rectangle.</summary>
		/// <param name="rect">The simple rectangle.</param>
		/// <param name="xRadius">The radii of the corners along the x-axis.</param>
		/// <param name="yRadius">The radii of the corners along the y-axis.</param>
		/// <remarks />
		public void SetRect (SKRect rect, float xRadius, float yRadius)
		{
			SkiaApi.sk_rrect_set_rect_xy (Handle, &rect, xRadius, yRadius);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this rectangle to be an oval.</summary>
		/// <param name="rect">The outer bounds of the oval.</param>
		/// <remarks />
		public void SetOval (SKRect rect)
		{
			SkiaApi.sk_rrect_set_oval (Handle, &rect);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this rounded rectangle to a nine-patch rectangle.</summary>
		/// <param name="rect">The interior rectangle.</param>
		/// <param name="leftRadius">The radii along the x-axis on the left side of the rectangle.</param>
		/// <param name="topRadius">The radii along the y-axis on the top of the rectangle.</param>
		/// <param name="rightRadius">The radii along the x-axis on the right side of the rectangle.</param>
		/// <param name="bottomRadius">The radii along the y-axis on the bottom of the rectangle.</param>
		/// <remarks />
		public void SetNinePatch (SKRect rect, float leftRadius, float topRadius, float rightRadius, float bottomRadius)
		{
			SkiaApi.sk_rrect_set_nine_patch (Handle, &rect, leftRadius, topRadius, rightRadius, bottomRadius);
			GC.KeepAlive (this);
		}

		/// <summary>Sets this rectangle to be a rounded rectangle.</summary>
		/// <param name="rect">The rectangle.</param>
		/// <param name="radii">The corner radii.</param>
		/// <remarks />
		public void SetRectRadii (SKRect rect, SKPoint[] radii)
		{
			if (radii == null)
				throw new ArgumentNullException (nameof (radii));

			SetRectRadii(rect, radii.AsSpan());
		}

		/// <summary>Sets the rectangle with individual corner radii specified as a span of points.</summary>
		/// <param name="rect">The bounds of the rounded rectangle.</param>
		/// <param name="radii">An array of four points representing the radii for each corner (upper-left, upper-right, lower-right, lower-left).</param>
		/// <remarks>If the radii span does not contain exactly four elements, an exception is thrown.</remarks>
		public void SetRectRadii (SKRect rect, ReadOnlySpan<SKPoint> radii)
		{
			if (radii.Length != 4)
				throw new ArgumentException ("Radii must have a length of 4.", nameof (radii));

			fixed (SKPoint* r = radii) {
				SkiaApi.sk_rrect_set_rect_radii (Handle, &rect, r);
				GC.KeepAlive (this);
			}
		}

		/// <summary>Determines whether the specified rectangle is wholly contained within the rounded rectangle.</summary>
		/// <param name="rect">The rectangle.</param>
		/// <returns>Returns <see langword="true" /> if the specified rectangle is inside the rounded rectangle, otherwise <see langword="false" />.</returns>
		/// <remarks />
		public bool Contains (SKRect rect)
		{
			var r = SkiaApi.sk_rrect_contains (Handle, &rect);
			GC.KeepAlive (this);
			return r;
		}

		/// <summary>Retrieves the radii of the specified corner.</summary>
		/// <param name="corner">The corner to retrieve.</param>
		/// <returns>Returns the radii of the specified corner.</returns>
		/// <remarks />
		public SKPoint GetRadii (SKRoundRectCorner corner)
		{
			SKPoint radii;
			SkiaApi.sk_rrect_get_radii (Handle, corner, &radii);
			GC.KeepAlive (this);
			return radii;
		}

		/// <summary>Deflate the rectangle by the specified amount.</summary>
		/// <param name="size">The amount to deflate the rectangle by.</param>
		/// <remarks>The corner radii are adjusted by the amount of the deflation if they are round.</remarks>
		public void Deflate (SKSize size)
		{
			Deflate (size.Width, size.Height);
		}

		/// <summary>Deflate the rectangle by the specified amount.</summary>
		/// <param name="dx">The amount to deflate the rectangle by along the x-axis.</param>
		/// <param name="dy">The amount to deflate the rectangle by along the y-axis.</param>
		/// <remarks>The corner radii are adjusted by the amount of the deflation if they are round.</remarks>
		public void Deflate (float dx, float dy)
		{
			SkiaApi.sk_rrect_inset (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		/// <summary>Inflate the rectangle by the specified amount.</summary>
		/// <param name="size">The amount to inflate the rectangle by.</param>
		/// <remarks>The corner radii are adjusted by the amount of the inflation if they are round.</remarks>
		public void Inflate (SKSize size)
		{
			Inflate (size.Width, size.Height);
		}

		/// <summary>Inflate the rectangle by the specified amount.</summary>
		/// <param name="dx">The amount to inflate the rectangle by along the x-axis.</param>
		/// <param name="dy">The amount to inflate the rectangle by along the y-axis.</param>
		/// <remarks>The corner radii are adjusted by the amount of the inflation if they are round.</remarks>
		public void Inflate (float dx, float dy)
		{
			SkiaApi.sk_rrect_outset (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		/// <summary>Translate the rectangle by the specified amount.</summary>
		/// <param name="pos">The amount to translate the rectangle by.</param>
		/// <remarks />
		public void Offset (SKPoint pos)
		{
			Offset (pos.X, pos.Y);
		}

		/// <summary>Translate the rectangle by the specified amount.</summary>
		/// <param name="dx">The amount to translate the rectangle by along the x-axis.</param>
		/// <param name="dy">The amount to translate the rectangle by along the y-axis.</param>
		/// <remarks />
		public void Offset (float dx, float dy)
		{
			SkiaApi.sk_rrect_offset (Handle, dx, dy);
			GC.KeepAlive (this);
		}

		/// <summary>Create a new rounded rectangle that is transformed by the specified matrix.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <param name="transformed">The transformed rounded rectangle.</param>
		/// <returns>Returns <see langword="true" /> if the transformation was successful, otherwise <see langword="false" />.</returns>
		/// <remarks>The transformation matrix must be a scale and/or translation matrix.</remarks>
		public bool TryTransform (SKMatrix matrix, out SKRoundRect transformed)
		{
			var destHandle = SkiaApi.sk_rrect_new ();
			var success = SkiaApi.sk_rrect_transform (Handle, &matrix, destHandle);
			GC.KeepAlive (this);
			if (success) {
				transformed = new SKRoundRect (destHandle, true);
				return true;
			}
			SkiaApi.sk_rrect_delete (destHandle);
			transformed = null;
			return false;
		}

		/// <summary>Create a new rounded rectangle that is transformed by the specified matrix.</summary>
		/// <param name="matrix">The transformation matrix.</param>
		/// <returns>Returns a new, transformed rectangle if the matrix was valid, or <see langword="null" /> otherwise.</returns>
		/// <remarks>The transformation matrix must be a scale and/or translation matrix.</remarks>
		public SKRoundRect Transform (SKMatrix matrix)
		{
			if (TryTransform (matrix, out var transformed)) {
				return transformed;
			}
			return null;
		}
	}
}
