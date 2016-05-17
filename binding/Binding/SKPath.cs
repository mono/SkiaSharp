//
// Bindings for SKPath
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKPath : SKObject
	{
		[Preserve]
		internal SKPath (IntPtr handle)
			: base (handle)
		{
		}

		public SKPath ()
			: this (SkiaApi.sk_path_new ())
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new SKPath instance.");
			}
		}

		public SKPath(SKPath path)
			: this (SkiaApi.sk_path_clone(path.Handle))
		{
			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to copy the SKPath instance.");
			}
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				SkiaApi.sk_path_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public SKPathFillType FillType {
			get {
				return SkiaApi.sk_path_get_filltype (Handle);
			}
			set {
				SkiaApi.sk_path_set_filltype (Handle, value);
			}
		}

		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_path_move_to (Handle, x, y);
		}

		public void RMoveTo (float dx, float dy)
		{
			SkiaApi.sk_path_rmove_to (Handle, dx, dy);
		}

		public void LineTo (float x, float y)
		{
			SkiaApi.sk_path_line_to (Handle, x, y);
		}

		public void RLineTo (float dx, float dy)
		{
			SkiaApi.sk_path_rline_to (Handle, dx, dy);
		}

		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_path_quad_to (Handle, x0, y0, x1, y1);
		}

		public void RQuadTo (float dx0, float dy0, float dx1, float dy1)
		{
			SkiaApi.sk_path_rquad_to (Handle, dx0, dy0, dx1, dy1);
		}

		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_path_conic_to (Handle, x0, y0, x1, y1, w);
		}

		public void RConicTo (float dx0, float dy0, float dx1, float dy1, float w)
		{
			SkiaApi.sk_path_rconic_to (Handle, dx0, dy0, dx1, dy1, w);
		}

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_path_cubic_to (Handle, x0, y0, x1, y1, x2, y2);
		}

		public void RCubicTo (float dx0, float dy0, float dx1, float dy1, float dx2, float dy2)
		{
			SkiaApi.sk_path_rcubic_to (Handle, dx0, dy0, dx1, dy1, dx2, dy2);
		}

		public void Close ()
		{
			SkiaApi.sk_path_close (Handle);
		}

		public void AddRect (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_rect (Handle, ref rect, direction);
		}

		public void AddRect (SKRect rect, SKPathDirection direction, uint startIndex)
		{
			if (startIndex > 3)
				throw new ArgumentOutOfRangeException ("startIndex", "startIndex must be 0 - 3");

			SkiaApi.sk_path_add_rect_start (Handle, ref rect, direction, startIndex);
		}

		public void AddOval (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_oval (Handle, ref rect, direction);
		}

		public void AddArc (SKRect oval, float startAngle, float sweepAngle)
		{
			SkiaApi.sk_path_add_arc (Handle, ref oval, startAngle, sweepAngle);
		}

		public bool GetBounds (out SKRect rect)
		{
			return SkiaApi.sk_path_get_bounds (Handle, out rect);
		}

		public void Transform (SKMatrix matrix)
		{
			SkiaApi.sk_path_transform (Handle, ref matrix);
		}
	}
}

