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
		internal SKPath (IntPtr handle)
			: base (handle)
		{
		}

		public SKPath ()
			: this (SkiaApi.sk_path_new ())
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				SkiaApi.sk_path_delete (Handle);
			}

			base.Dispose (disposing);
		}
		
		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_path_move_to (Handle, x, y);
		}

		public void LineTo (float x, float y)
		{
			SkiaApi.sk_path_line_to (Handle, x, y);
		}

		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_path_quad_to (Handle, x0, y0, x1, y1);
		}

		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_path_conic_to (Handle, x0, y0, x1, y1, w);
		}

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_path_cubic_to (Handle, x0, y0, x1, y1, x2, y2);
		}

		public void Close ()
		{
			SkiaApi.sk_path_close (Handle);
		}

		public void AddRect (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_rect (Handle, ref rect, direction);
		}

		public void AddOval (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_oval (Handle, ref rect, direction);
		}

		public bool GetBounds (out SKRect rect)
		{
			return SkiaApi.sk_path_get_bounds (Handle, out rect);
		}
	}
}

