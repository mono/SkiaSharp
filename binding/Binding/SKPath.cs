//
// Bindings for SKPath
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public class SKPath : IDisposable
	{
		internal IntPtr handle;

		public SKPath ()
		{
			handle = SkiaApi.sk_path_new ();
		}
		
		public void Dispose ()
		{
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		protected virtual void Dispose (bool disposing)
		{
			if (handle != IntPtr.Zero) {
				SkiaApi.sk_path_delete (handle);
				handle = IntPtr.Zero;
			}
		}

		~SKPath()
		{
			Dispose (false);
		}

		public void MoveTo (float x, float y)
		{
			SkiaApi.sk_path_move_to (handle, x, y);
		}

		public void LineTo (float x, float y)
		{
			SkiaApi.sk_path_line_to (handle, x, y);
		}

		public void QuadTo (float x0, float y0, float x1, float y1)
		{
			SkiaApi.sk_path_quad_to (handle, x0, y0, x1, y1);
		}

		public void ConicTo (float x0, float y0, float x1, float y1, float w)
		{
			SkiaApi.sk_path_conic_to (handle, x0, y0, x1, y1, w);
		}

		public void CubicTo (float x0, float y0, float x1, float y1, float x2, float y2)
		{
			SkiaApi.sk_path_cubic_to (handle, x0, y0, x1, y1, x2, y2);
		}

		public void Close ()
		{
			SkiaApi.sk_path_close (handle);
		}

		public void AddRect (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_rect (handle, ref rect, direction);
		}

		public void AddOval (SKRect rect, SKPathDirection direction)
		{
			SkiaApi.sk_path_add_oval (handle, ref rect, direction);
		}

		public bool GetBounds (out SKRect rect)
		{
			return SkiaApi.sk_path_get_bounds (handle, out rect);
		}
	}
}

