using System;

namespace SkiaSharp
{
	// TODO: carefully consider the `PeekPixels`, `ReadPixels`

	public unsafe class SKCanvas : SKObject
	{
		private const int PatchCornerCount = 4;
		private const int PatchCubicsCount = 12;

		internal SKCanvas (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public SKCanvas (SKBitmap bitmap)
			: this (IntPtr.Zero, true)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			Handle = SkiaApi.sk_canvas_new_from_bitmap (bitmap.Handle);
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_canvas_destroy (Handle);

		public void Discard () =>
			SkiaApi.sk_canvas_discard (Handle);

		// QuickReject

		public bool QuickReject (SKRect rect)
		{
			return SkiaApi.sk_canvas_quick_reject (Handle, &rect);
		}

		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			return path.IsEmpty || QuickReject (path.Bounds);
		}

		// Save*

		public int Save ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SKCanvas");
			return SkiaApi.sk_canvas_save (Handle);
		}

		public int SaveLayer (SKRect limit, SKPaint paint)
		{
			return SkiaApi.sk_canvas_save_layer (Handle, &limit, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public int SaveLayer (SKPaint paint)
		{
			return SkiaApi.sk_canvas_save_layer (Handle, null, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public int SaveLayer () =>
			SaveLayer (null);

		// DrawColor

		public void DrawColor (SKColor color, SKBlendMode mode = SKBlendMode.Src)
		{
			SkiaApi.sk_canvas_draw_color (Handle, (uint)color, mode);
		}

		// DrawLine

		public void DrawLine (SKPoint p0, SKPoint p1, SKPaint paint)
		{
			DrawLine (p0.X, p0.Y, p1.X, p1.Y, paint);
		}

		public void DrawLine (float x0, float y0, float x1, float y1, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_line (Handle, x0, y0, x1, y1, paint.Handle);
		}

		// Clear

		public void Clear ()
		{
			DrawColor (SKColors.Empty, SKBlendMode.Src);
		}

		public void Clear (SKColor color)
		{
			DrawColor (color, SKBlendMode.Src);
		}

		// Restore*

		public void Restore ()
		{
			SkiaApi.sk_canvas_restore (Handle);
		}

		public void RestoreToCount (int count)
		{
			SkiaApi.sk_canvas_restore_to_count (Handle, count);
		}

		// Translate

		public void Translate (float dx, float dy)
		{
			SkiaApi.sk_canvas_translate (Handle, dx, dy);
		}

		public void Translate (SKPoint point)
		{
			SkiaApi.sk_canvas_translate (Handle, point.X, point.Y);
		}

		// Scale

		public void Scale (float s)
		{
			SkiaApi.sk_canvas_scale (Handle, s, s);
		}

		public void Scale (float sx, float sy)
		{
			SkiaApi.sk_canvas_scale (Handle, sx, sy);
		}

		public void Scale (SKPoint size)
		{
			SkiaApi.sk_canvas_scale (Handle, size.X, size.Y);
		}

		public void Scale (float sx, float sy, float px, float py)
		{
			Translate (px, py);
			Scale (sx, sy);
			Translate (-px, -py);
		}

		// Rotate*

		public void RotateDegrees (float degrees)
		{
			SkiaApi.sk_canvas_rotate_degrees (Handle, degrees);
		}

		public void RotateRadians (float radians)
		{
			SkiaApi.sk_canvas_rotate_radians (Handle, radians);
		}

		public void RotateDegrees (float degrees, float px, float py)
		{
			Translate (px, py);
			RotateDegrees (degrees);
			Translate (-px, -py);
		}

		public void RotateRadians (float radians, float px, float py)
		{
			Translate (px, py);
			RotateRadians (radians);
			Translate (-px, -py);
		}

		// Skew

		public void Skew (float sx, float sy)
		{
			SkiaApi.sk_canvas_skew (Handle, sx, sy);
		}

		public void Skew (SKPoint skew)
		{
			SkiaApi.sk_canvas_skew (Handle, skew.X, skew.Y);
		}

		// Concat

		public void Concat (ref SKMatrix m)
		{
			fixed (SKMatrix* ptr = &m) {
				SkiaApi.sk_canvas_concat (Handle, ptr);
			}
		}

		// Clip*

		public void ClipRect (SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			SkiaApi.sk_canvas_clip_rect_with_operation (Handle, &rect, operation, antialias);
		}

		public void ClipRoundRect (SKRoundRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));

			SkiaApi.sk_canvas_clip_rrect_with_operation (Handle, rect.Handle, operation, antialias);
		}

		public void ClipPath (SKPath path, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			SkiaApi.sk_canvas_clip_path_with_operation (Handle, path.Handle, operation, antialias);
		}

		public void ClipRegion (SKRegion region, SKClipOperation operation = SKClipOperation.Intersect)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));

			SkiaApi.sk_canvas_clip_region (Handle, region.Handle, operation);
		}

		public SKRect LocalClipBounds {
			get {
				GetLocalClipBounds (out var bounds);
				return bounds;
			}
		}

		public SKRectI DeviceClipBounds {
			get {
				GetDeviceClipBounds (out var bounds);
				return bounds;
			}
		}

		public bool IsClipEmpty => SkiaApi.sk_canvas_is_clip_empty (Handle);

		public bool IsClipRect => SkiaApi.sk_canvas_is_clip_rect (Handle);

		public bool GetLocalClipBounds (out SKRect bounds)
		{
			fixed (SKRect* b = &bounds) {
				return SkiaApi.sk_canvas_get_local_clip_bounds (Handle, b);
			}
		}

		public bool GetDeviceClipBounds (out SKRectI bounds)
		{
			fixed (SKRectI* b = &bounds) {
				return SkiaApi.sk_canvas_get_device_clip_bounds (Handle, b);
			}
		}

		// DrawPaint

		public void DrawPaint (SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_paint (Handle, paint.Handle);
		}

		// DrawRegion

		public void DrawRegion (SKRegion region, SKPaint paint)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_region (Handle, region.Handle, paint.Handle);
		}

		// DrawRect

		public void DrawRect (float x, float y, float w, float h, SKPaint paint)
		{
			DrawRect (SKRect.Create (x, y, w, h), paint);
		}

		public void DrawRect (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_rect (Handle, &rect, paint.Handle);
		}

		// DrawRoundRect

		public void DrawRoundRect (SKRoundRect rect, SKPaint paint)
		{
			if (rect == null)
				throw new ArgumentNullException (nameof (rect));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_rrect (Handle, rect.Handle, paint.Handle);
		}

		public void DrawRoundRect (float x, float y, float w, float h, float rx, float ry, SKPaint paint)
		{
			DrawRoundRect (SKRect.Create (x, y, w, h), rx, ry, paint);
		}

		public void DrawRoundRect (SKRect rect, float rx, float ry, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_round_rect (Handle, &rect, rx, ry, paint.Handle);
		}

		public void DrawRoundRect (SKRect rect, SKSize r, SKPaint paint)
		{
			DrawRoundRect (rect, r.Width, r.Height, paint);
		}

		// DrawOval

		public void DrawOval (float cx, float cy, float rx, float ry, SKPaint paint)
		{
			DrawOval (new SKRect (cx - rx, cy - ry, cx + rx, cy + ry), paint);
		}

		public void DrawOval (SKPoint c, SKSize r, SKPaint paint)
		{
			DrawOval (c.X, c.Y, r.Width, r.Height, paint);
		}

		public void DrawOval (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_oval (Handle, &rect, paint.Handle);
		}

		// DrawCircle

		public void DrawCircle (float cx, float cy, float radius, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_circle (Handle, cx, cy, radius, paint.Handle);
		}

		public void DrawCircle (SKPoint c, float radius, SKPaint paint)
		{
			DrawCircle (c.X, c.Y, radius, paint);
		}

		// DrawPath

		public void DrawPath (SKPath path, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			SkiaApi.sk_canvas_draw_path (Handle, path.Handle, paint.Handle);
		}

		// DrawPoints

		public void DrawPoints (SKPointMode mode, SKPoint[] points, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			fixed (SKPoint* p = points) {
				SkiaApi.sk_canvas_draw_points (Handle, mode, (IntPtr)points.Length, p, paint.Handle);
			}
		}

		// DrawPoint

		public void DrawPoint (SKPoint p, SKPaint paint)
		{
			DrawPoint (p.X, p.Y, paint);
		}

		public void DrawPoint (float x, float y, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_point (Handle, x, y, paint.Handle);
		}

		public void DrawPoint (SKPoint p, SKColor color)
		{
			DrawPoint (p.X, p.Y, color);
		}

		public void DrawPoint (float x, float y, SKColor color)
		{
			using (var paint = new SKPaint { Color = color }) {
				DrawPoint (x, y, paint);
			}
		}

		// DrawImage

		public void DrawImage (SKImage image, SKPoint p, SKPaint paint = null)
		{
			DrawImage (image, p.X, p.Y, paint);
		}

		public void DrawImage (SKImage image, float x, float y, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image (Handle, image.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawImage (SKImage image, SKRect dest, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image_rect (Handle, image.Handle, null, &dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawImage (SKImage image, SKRect source, SKRect dest, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image_rect (Handle, image.Handle, &source, &dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// DrawPicture

		public void DrawPicture (SKPicture picture, float x, float y, SKPaint paint = null)
		{
			var matrix = SKMatrix.MakeTranslation (x, y);
			DrawPicture (picture, ref matrix, paint);
		}

		public void DrawPicture (SKPicture picture, SKPoint p, SKPaint paint = null)
		{
			DrawPicture (picture, p.X, p.Y, paint);
		}

		public void DrawPicture (SKPicture picture, ref SKMatrix matrix, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, m, paint == null ? IntPtr.Zero : paint.Handle);
			}
		}

		public void DrawPicture (SKPicture picture, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));
			SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, null, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// DrawDrawable

		public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			fixed (SKMatrix* m = &matrix) {
				SkiaApi.sk_canvas_draw_drawable (Handle, drawable.Handle, m);
			}
		}

		public void DrawDrawable (SKDrawable drawable, float x, float y)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			var matrix = SKMatrix.MakeTranslation (x, y);
			DrawDrawable (drawable, ref matrix);
		}

		public void DrawDrawable (SKDrawable drawable, SKPoint p)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			var matrix = SKMatrix.MakeTranslation (p.X, p.Y);
			DrawDrawable (drawable, ref matrix);
		}

		// DrawBitmap

		public void DrawBitmap (SKBitmap bitmap, SKPoint p, SKPaint paint = null)
		{
			DrawBitmap (bitmap, p.X, p.Y, paint);
		}

		public void DrawBitmap (SKBitmap bitmap, float x, float y, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			SkiaApi.sk_canvas_draw_bitmap (Handle, bitmap.Handle, x, y, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawBitmap (SKBitmap bitmap, SKRect dest, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			SkiaApi.sk_canvas_draw_bitmap_rect (Handle, bitmap.Handle, null, &dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawBitmap (SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			SkiaApi.sk_canvas_draw_bitmap_rect (Handle, bitmap.Handle, &source, &dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// DrawSurface

		public void DrawSurface (SKSurface surface, SKPoint p, SKPaint paint = null)
		{
			DrawSurface (surface, p.X, p.Y, paint);
		}

		public void DrawSurface (SKSurface surface, float x, float y, SKPaint paint = null)
		{
			if (surface == null)
				throw new ArgumentNullException (nameof (surface));

			surface.Draw (this, x, y, paint);
		}

		// DrawText (SKTextBlob)

		public void DrawText (SKTextBlob text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text_blob (Handle, text.Handle, x, y, paint.Handle);
		}

		// DrawText

		public void DrawText (string text, SKPoint p, SKPaint paint)
		{
			DrawText (text, p.X, p.Y, paint);
		}

		public void DrawText (string text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			var bytes = StringUtilities.GetEncodedText (text, paint.TextEncoding);
			DrawText (bytes, x, y, paint);
		}

		public void DrawText (byte[] text, SKPoint p, SKPaint paint)
		{
			DrawText (text, p.X, p.Y, paint);
		}

		public void DrawText (byte[] text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			fixed (byte* t = text) {
				SkiaApi.sk_canvas_draw_text (Handle, t, (IntPtr)text.Length, x, y, paint.Handle);
			}
		}

		public void DrawText (IntPtr buffer, int length, SKPoint p, SKPaint paint)
		{
			DrawText (buffer, length, p.X, p.Y, paint);
		}

		public void DrawText (IntPtr buffer, int length, float x, float y, SKPaint paint)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text (Handle, (void*)buffer, (IntPtr)length, x, y, paint.Handle);
		}

		// DrawPositionedText

		public void DrawPositionedText (string text, SKPoint[] points, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			var bytes = StringUtilities.GetEncodedText (text, paint.TextEncoding);
			DrawPositionedText (bytes, points, paint);
		}

		public void DrawPositionedText (byte[] text, SKPoint[] points, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			fixed (byte* t = text)
			fixed (SKPoint* p = points) {
				SkiaApi.sk_canvas_draw_pos_text (Handle, t, (IntPtr)text.Length, p, paint.Handle);
			}
		}

		public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			fixed (SKPoint* p = points) {
				SkiaApi.sk_canvas_draw_pos_text (Handle, (void*)buffer, (IntPtr)length, p, paint.Handle);
			}
		}

		// DrawTextOnPath

		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint)
		{
			DrawTextOnPath (text, path, offset.X, offset.Y, paint);
		}

		public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			var bytes = StringUtilities.GetEncodedText (text, paint.TextEncoding);
			DrawTextOnPath (bytes, path, hOffset, vOffset, paint);
		}

		public void DrawTextOnPath (byte[] text, SKPath path, SKPoint offset, SKPaint paint)
		{
			DrawTextOnPath (text, path, offset.X, offset.Y, paint);
		}

		public void DrawTextOnPath (byte[] text, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			fixed (byte* t = text) {
				SkiaApi.sk_canvas_draw_text_on_path (Handle, t, (IntPtr)text.Length, path.Handle, hOffset, vOffset, paint.Handle);
			}
		}

		public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint)
		{
			DrawTextOnPath (buffer, length, path, offset.X, offset.Y, paint);
		}

		public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text_on_path (Handle, (void*)buffer, (IntPtr)length, path.Handle, hOffset, vOffset, paint.Handle);
		}

		// Flush

		public void Flush ()
		{
			SkiaApi.sk_canvas_flush (Handle);
		}

		// Draw*Annotation

		public void DrawAnnotation (SKRect rect, string key, SKData value)
		{
			var bytes = StringUtilities.GetEncodedText (key, SKTextEncoding.Utf8);
			fixed (byte* b = bytes) {
				SkiaApi.sk_canvas_draw_annotation (base.Handle, &rect, b, value == null ? IntPtr.Zero : value.Handle);
			}
		}

		public void DrawUrlAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_url_annotation (Handle, &rect, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawUrlAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawUrlAnnotation (rect, data);
			return data;
		}

		public void DrawNamedDestinationAnnotation (SKPoint point, SKData value)
		{
			SkiaApi.sk_canvas_draw_named_destination_annotation (Handle, &point, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawNamedDestinationAnnotation (SKPoint point, string value)
		{
			var data = SKData.FromCString (value);
			DrawNamedDestinationAnnotation (point, data);
			return data;
		}

		public void DrawLinkDestinationAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_link_destination_annotation (Handle, &rect, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawLinkDestinationAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawLinkDestinationAnnotation (rect, data);
			return data;
		}

		// Draw*NinePatch

		public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			// the "center" rect must fit inside the bitmap "rect"
			if (!SKRect.Create (bitmap.Info.Size).Contains (center))
				throw new ArgumentException ("Center rectangle must be contained inside the bitmap bounds.", nameof (center));

			SkiaApi.sk_canvas_draw_bitmap_nine (Handle, bitmap.Handle, &center, &dst, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			// the "center" rect must fit inside the image "rect"
			if (!SKRect.Create (image.Width, image.Height).Contains (center))
				throw new ArgumentException ("Center rectangle must be contained inside the image bounds.", nameof (center));

			SkiaApi.sk_canvas_draw_image_nine (Handle, image.Handle, &center, &dst, paint == null ? IntPtr.Zero : paint.Handle);
		}

		// Draw*Lattice

		public void DrawBitmapLattice (SKBitmap bitmap, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
		{
			var lattice = new SKLattice {
				XDivs = xDivs,
				YDivs = yDivs
			};
			DrawBitmapLattice (bitmap, lattice, dst, paint);
		}

		public void DrawImageLattice (SKImage image, int[] xDivs, int[] yDivs, SKRect dst, SKPaint paint = null)
		{
			var lattice = new SKLattice {
				XDivs = xDivs,
				YDivs = yDivs
			};
			DrawImageLattice (image, lattice, dst, paint);
		}

		public void DrawBitmapLattice (SKBitmap bitmap, SKLattice lattice, SKRect dst, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			if (lattice.XDivs == null)
				throw new ArgumentNullException (nameof (lattice.XDivs));
			if (lattice.YDivs == null)
				throw new ArgumentNullException (nameof (lattice.YDivs));

			fixed (int* x = lattice.XDivs)
			fixed (int* y = lattice.YDivs)
			fixed (SKLatticeRectType* r = lattice.RectTypes)
			fixed (SKColor* c = lattice.Colors) {
				var nativeLattice = new SKLatticeInternal {
					fBounds = null,
					fRectTypes = r,
					fXCount = lattice.XDivs.Length,
					fXDivs = x,
					fYCount = lattice.YDivs.Length,
					fYDivs = y,
					fColors = (uint*)c,
				};
				if (lattice.Bounds != null) {
					var bounds = lattice.Bounds.Value;
					nativeLattice.fBounds = &bounds;
				}
				SkiaApi.sk_canvas_draw_bitmap_lattice (Handle, bitmap.Handle, &nativeLattice, &dst, paint == null ? IntPtr.Zero : paint.Handle);
			}
		}

		public void DrawImageLattice (SKImage image, SKLattice lattice, SKRect dst, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			if (lattice.XDivs == null)
				throw new ArgumentNullException (nameof (lattice.XDivs));
			if (lattice.YDivs == null)
				throw new ArgumentNullException (nameof (lattice.YDivs));

			fixed (int* x = lattice.XDivs)
			fixed (int* y = lattice.YDivs)
			fixed (SKLatticeRectType* r = lattice.RectTypes)
			fixed (SKColor* c = lattice.Colors) {
				var nativeLattice = new SKLatticeInternal {
					fBounds = null,
					fRectTypes = r,
					fXCount = lattice.XDivs.Length,
					fXDivs = x,
					fYCount = lattice.YDivs.Length,
					fYDivs = y,
					fColors = (uint*)c,
				};
				if (lattice.Bounds != null) {
					var bounds = lattice.Bounds.Value;
					nativeLattice.fBounds = &bounds;
				}
				SkiaApi.sk_canvas_draw_image_lattice (Handle, image.Handle, &nativeLattice, &dst, paint == null ? IntPtr.Zero : paint.Handle);
			}
		}

		// *Matrix

		public void ResetMatrix ()
		{
			SkiaApi.sk_canvas_reset_matrix (Handle);
		}

		public void SetMatrix (SKMatrix matrix)
		{
			SkiaApi.sk_canvas_set_matrix (Handle, &matrix);
		}

		public SKMatrix TotalMatrix {
			get {
				SKMatrix matrix;
				SkiaApi.sk_canvas_get_total_matrix (Handle, &matrix);
				return matrix;
			}
		}

		// SaveCount

		public int SaveCount => SkiaApi.sk_canvas_get_save_count (Handle);

		// DrawVertices

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, colors);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, UInt16[] indices, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors, indices);
			DrawVertices (vert, SKBlendMode.Modulate, paint);
		}

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKBlendMode mode, UInt16[] indices, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy (vmode, vertices, texs, colors, indices);
			DrawVertices (vert, mode, paint);
		}

		public void DrawVertices (SKVertices vertices, SKBlendMode mode, SKPaint paint)
		{
			if (vertices == null)
				throw new ArgumentNullException (nameof (vertices));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_vertices (Handle, vertices.Handle, mode, paint.Handle);
		}

		// DrawArc

		public void DrawArc (SKRect oval, float startAngle, float sweepAngle, bool useCenter, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_arc (Handle, &oval, startAngle, sweepAngle, useCenter, paint.Handle);
		}

		// DrawRoundRectDifference

		public void DrawRoundRectDifference (SKRoundRect outer, SKRoundRect inner, SKPaint paint)
		{
			if (outer == null)
				throw new ArgumentNullException (nameof (outer));
			if (inner == null)
				throw new ArgumentNullException (nameof (inner));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_drrect (Handle, outer.Handle, inner.Handle, paint.Handle);
		}

		// DrawAtlas

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKPaint paint) =>
			DrawAtlas (atlas, sprites, transforms, null, SKBlendMode.Dst, null, paint);

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKPaint paint) =>
			DrawAtlas (atlas, sprites, transforms, colors, mode, null, paint);

		public void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect cullRect, SKPaint paint) =>
			DrawAtlas (atlas, sprites, transforms, colors, mode, &cullRect, paint);

		private void DrawAtlas (SKImage atlas, SKRect[] sprites, SKRotationScaleMatrix[] transforms, SKColor[] colors, SKBlendMode mode, SKRect* cullRect, SKPaint paint)
		{
			if (atlas == null)
				throw new ArgumentNullException (nameof (atlas));
			if (sprites == null)
				throw new ArgumentNullException (nameof (sprites));
			if (transforms == null)
				throw new ArgumentNullException (nameof (transforms));

			if (transforms.Length != sprites.Length)
				throw new ArgumentException ("The number of transforms must match the number of sprites.", nameof (transforms));
			if (colors != null && colors.Length != sprites.Length)
				throw new ArgumentException ("The number of colors must match the number of sprites.", nameof (colors));

			fixed (SKRect* s = sprites)
			fixed (SKRotationScaleMatrix* t = transforms)
			fixed (SKColor* c = colors) {
				SkiaApi.sk_canvas_draw_atlas (Handle, atlas.Handle, t, s, (uint*)c, transforms.Length, mode, cullRect, paint.Handle);
			}
		}

		// DrawPatch

		public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKPaint paint) =>
			DrawPatch (cubics, colors, texCoords, SKBlendMode.Modulate, paint);

		public void DrawPatch (SKPoint[] cubics, SKColor[] colors, SKPoint[] texCoords, SKBlendMode mode, SKPaint paint)
		{
			if (cubics == null)
				throw new ArgumentNullException (nameof (cubics));
			if (cubics.Length != PatchCubicsCount)
				throw new ArgumentException ($"Cubics must have a length of {PatchCubicsCount}.", nameof (cubics));

			if (colors != null && colors.Length != PatchCornerCount)
				throw new ArgumentException ($"Colors must have a length of {PatchCornerCount}.", nameof (colors));

			if (texCoords != null && texCoords.Length != PatchCornerCount)
				throw new ArgumentException ($"Texture coordinates must have a length of {PatchCornerCount}.", nameof (texCoords));

			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			fixed (SKPoint* cubes = cubics)
			fixed (SKColor* cols = colors)
			fixed (SKPoint* coords = texCoords) {
				SkiaApi.sk_canvas_draw_patch (Handle, cubes, (uint*)cols, coords, mode, paint.Handle);
			}
		}

		internal static SKCanvas GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKCanvas (h, o));
	}

	public class SKAutoCanvasRestore : IDisposable
	{
		private SKCanvas canvas;
		private readonly int saveCount;

		public SKAutoCanvasRestore (SKCanvas canvas)
			: this (canvas, true)
		{
		}

		public SKAutoCanvasRestore (SKCanvas canvas, bool doSave)
		{
			this.canvas = canvas;
			this.saveCount = 0;

			if (canvas != null) {
				saveCount = canvas.SaveCount;
				if (doSave) {
					canvas.Save ();
				}
			}
		}

		public void Dispose ()
		{
			Restore ();
		}

		/// <summary>
		/// Perform the restore now, instead of waiting for the Dispose.
		/// Will only do this once.
		/// </summary>
		public void Restore ()
		{
			if (canvas != null) {
				canvas.RestoreToCount (saveCount);
				canvas = null;
			}
		}
	}
}
