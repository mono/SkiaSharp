//
// Bindings for SKCanvas
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	// No dispose, the Canvas is only valid while the Surface is valid.
	public class SKCanvas
	{
		internal IntPtr handle;
		internal SKCanvas (IntPtr ptr)
		{
			handle = ptr;
		}

		public void Save ()
		{
			if (handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SKCanvas");
			SkiaApi.sk_canvas_save (handle);
		}

		public void SaveLayer (SKRect limit, SKPaint paint)
		{
			SkiaApi.sk_canvas_save_layer (handle, ref limit, paint == null ? IntPtr.Zero : paint.handle);
		}

		public void SaveLayer (SKPaint paint)
		{
			SkiaApi.sk_canvas_save_layer (handle, IntPtr.Zero, paint == null ? IntPtr.Zero : paint.handle);
		}

		public void DrawColor (SKColor color, SKXferMode mode = SKXferMode.Src)
		{
			SkiaApi.sk_canvas_draw_color (handle, color, mode);
		}

		public void DrawLine (float x0, float y0, float x1, float y1, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			SkiaApi.sk_canvas_draw_line (handle, x0, y0, x1, y1, paint.handle);
		}

		public void Clear ()
		{
			DrawColor (SKColors.Empty, SKXferMode.Src);
		}

		public void Restore ()
		{
			SkiaApi.sk_canvas_restore (handle);
		}

		public void Translate (float dx, float dy)
		{
			SkiaApi.sk_canvas_translate (handle, dx, dy);
		}

		public void Scale (float sx, float sy)
		{
			SkiaApi.sk_canvas_scale (handle, sx, sy);
		}

		public void RotateDegrees (float degrees)
		{
			SkiaApi.sk_canvas_rotate_degrees (handle, degrees);
		}

		public void RotateRadians (float radians)
		{
			SkiaApi.sk_canvas_rotate_radians (handle, radians);
		}

		public void Skew (float sx, float sy)
		{
			SkiaApi.sk_canvas_skew (handle, sx, sy);
		}

		public void Concat (ref SKMatrix m)
		{
			SkiaApi.sk_canvas_concat (handle, ref m);
		}

		public void ClipRect (SKRect rect)
		{
			SkiaApi.sk_canvas_clip_rect (handle, ref rect);
		}

		public void ClipPath (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException ("path");
			
			SkiaApi.sk_canvas_clip_path (handle, path.handle);
		}

		public void DrawPaint (SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			SkiaApi.sk_canvas_draw_paint (handle, paint.handle);
		}

		public void DrawRect (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			SkiaApi.sk_canvas_draw_rect (handle, ref rect, paint.handle);
		}

		public void DrawOval (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			SkiaApi.sk_canvas_draw_oval (handle, ref rect, paint.handle);
		}
		
		public void DrawPath (SKPath path, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (path == null)
				throw new ArgumentNullException ("path");
			SkiaApi.sk_canvas_draw_path (handle, path.handle, paint.handle);
		}

		public void DrawPoints (SKPointMode mode, SKPoint [] points, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (points == null)
				throw new ArgumentNullException ("points");
			SkiaApi.sk_canvas_draw_points (handle, mode, (IntPtr)points.Length, points, paint.handle);
		}

		public void DrawPoint (float x, float y, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			SkiaApi.sk_canvas_draw_point (handle, x, y, paint.handle);
		}

		public void DrawPoint (float x, float y, SKColor color)
		{
			SkiaApi.sk_canvas_draw_point_color (handle, x, y, color);
		}
		
		public void DrawImage (SKPath path, SKImage image, float x, float y, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (image == null)
				throw new ArgumentNullException ("image");
			if (path == null)
				throw new ArgumentNullException ("path");
			SkiaApi.sk_canvas_draw_image (handle, image.handle, x, y, paint.handle);
		}

		public void DrawImageScaled (SKPath path, SKImage image, SKRect dest, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (image == null)
				throw new ArgumentNullException ("image");
			if (path == null)
				throw new ArgumentNullException ("path");
			SkiaApi.sk_canvas_draw_image_rect (handle, image.handle, IntPtr.Zero, ref dest, paint.handle);
		}

		public void DrawImageScaled (SKPath path, SKImage image, SKRect source, SKRect dest, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (image == null)
				throw new ArgumentNullException ("image");
			if (path == null)
				throw new ArgumentNullException ("path");
			SkiaApi.sk_canvas_draw_image_rect (handle, image.handle, ref source, ref dest, paint.handle);
		}

		public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint)
		{
			if (picture == null)
				throw new ArgumentNullException ("picture");
			SkiaApi.sk_canvas_draw_picture (handle, picture.handle, ref matrix, paint == null ? IntPtr.Zero : paint.handle);
		}

		public void DrawPicture (SKPicture picture, SKPaint paint)
		{
			if (picture == null)
				throw new ArgumentNullException ("picture");
			SkiaApi.sk_canvas_draw_picture (handle, picture.handle, IntPtr.Zero, paint == null ? IntPtr.Zero : paint.handle);
		}

		public void DrawText (string text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			if (paint == null)
				throw new ArgumentNullException ("paint");

			var bytes = System.Text.Encoding.UTF8.GetBytes (text);
			SkiaApi.sk_canvas_draw_text (handle, bytes, bytes.Length, x, y, paint.handle);
		}

		public void DrawText (string text, SKPoint [] points, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (points == null)
				throw new ArgumentNullException ("points");

			var bytes = System.Text.Encoding.UTF8.GetBytes (text);
			SkiaApi.sk_canvas_draw_pos_text (handle, bytes, bytes.Length, points, paint.handle);
		}

		public void DrawText (string text, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException ("text");
			if (paint == null)
				throw new ArgumentNullException ("paint");
			if (paint == null)
				throw new ArgumentNullException ("paint");

			var bytes = System.Text.Encoding.UTF8.GetBytes (text);
			SkiaApi.sk_canvas_draw_text_on_path (handle, bytes, bytes.Length, path.handle, hOffset, vOffset, paint.handle);
		}
	}
}

