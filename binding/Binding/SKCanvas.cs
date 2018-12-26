using System;

namespace SkiaSharp
{
	// TODO: carefully consider the `PeekPixels`, `ReadPixels`
	// TODO: `ClipRRect` may be useful
	// TODO: `DrawRRect` may be useful
	// TODO: `DrawDRRect` may be useful
	// TODO: add the `DrawArc` variants
	// TODO: add `DrawTextBlob` variants if/when we bind `SKTextBlob`
	// TODO: add `DrawPatch` variants
	// TODO: add `DrawAtlas` variants
	// TODO: add `DrawDrawable` variants if/when we bind `SKDrawable`
	// TODO: add `IsClipEmpty` and `IsClipRect`

	public class SKCanvas : SKObject
	{
		[Preserve]
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_canvas_destroy (Handle);
			}

			base.Dispose (disposing);
		}

		public bool QuickReject (SKRect rect)
		{
			return SkiaApi.sk_canvas_quick_reject (Handle, ref rect);
		}

		public bool QuickReject (SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			return path.IsEmpty || QuickReject (path.Bounds);
		}

		public int Save ()
		{
			if (Handle == IntPtr.Zero)
				throw new ObjectDisposedException ("SKCanvas");
			return SkiaApi.sk_canvas_save (Handle);
		}

		public int SaveLayer (SKRect limit, SKPaint paint)
		{
			return SkiaApi.sk_canvas_save_layer (Handle, ref limit, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public int SaveLayer (SKPaint paint)
		{
			return SkiaApi.sk_canvas_save_layer (Handle, IntPtr.Zero, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawColor (SKColor color, SKBlendMode mode = SKBlendMode.Src)
		{
			SkiaApi.sk_canvas_draw_color (Handle, color, mode);
		}

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

		public void Clear ()
		{
			DrawColor (SKColors.Empty, SKBlendMode.Src);
		}

		public void Clear (SKColor color)
		{
			DrawColor (color, SKBlendMode.Src);
		}

		public void Restore ()
		{
			SkiaApi.sk_canvas_restore (Handle);
		}

		public void RestoreToCount (int count)
		{
			SkiaApi.sk_canvas_restore_to_count (Handle, count);
		}

		public void Translate (float dx, float dy)
		{
			SkiaApi.sk_canvas_translate (Handle, dx, dy);
		}

		public void Translate (SKPoint point)
		{
			SkiaApi.sk_canvas_translate (Handle, point.X, point.Y);
		}
		
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

		public void RotateRadians(float radians, float px, float py)
		{
			Translate (px, py);
			RotateRadians (radians);
			Translate (-px, -py);
		}

		public void Skew (float sx, float sy)
		{
			SkiaApi.sk_canvas_skew (Handle, sx, sy);
		}

		public void Skew (SKPoint skew)
		{
			SkiaApi.sk_canvas_skew (Handle, skew.X, skew.Y);
		}

		public void Concat (ref SKMatrix m)
		{
			SkiaApi.sk_canvas_concat (Handle, ref m);
		}

		public void ClipRect (SKRect rect, SKClipOperation operation = SKClipOperation.Intersect, bool antialias = false)
		{
			SkiaApi.sk_canvas_clip_rect_with_operation (Handle, ref rect, operation, antialias);
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

		public bool GetLocalClipBounds (out SKRect bounds)
		{
			return SkiaApi.sk_canvas_get_local_clip_bounds (Handle, out bounds);
		}

		public bool GetDeviceClipBounds (out SKRectI bounds)
		{
			return SkiaApi.sk_canvas_get_device_clip_bounds (Handle, out bounds);
		}

		public void DrawPaint (SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_paint (Handle, paint.Handle);
		}

		public void DrawRegion (SKRegion region, SKPaint paint)
		{
			if (region == null)
				throw new ArgumentNullException (nameof (region));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_region (Handle, region.Handle, paint.Handle);
		}

		public void DrawRect (float x, float y, float w, float h, SKPaint paint)
		{
			DrawRect (SKRect.Create (x, y, w, h), paint);
		}

		public void DrawRect (SKRect rect, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			SkiaApi.sk_canvas_draw_rect (Handle, ref rect, paint.Handle);
		}

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
			SkiaApi.sk_canvas_draw_round_rect (Handle, ref rect, rx, ry, paint.Handle);
		}

		public void DrawRoundRect (SKRect rect, SKSize r, SKPaint paint)
		{
			DrawRoundRect (rect, r.Width, r.Height, paint);
		}

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
			SkiaApi.sk_canvas_draw_oval (Handle, ref rect, paint.Handle);
		}

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
		
		public void DrawPath (SKPath path, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (path == null)
				throw new ArgumentNullException (nameof (path));
			SkiaApi.sk_canvas_draw_path (Handle, path.Handle, paint.Handle);
		}

		public void DrawPoints (SKPointMode mode, SKPoint [] points, SKPaint paint)
		{
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			SkiaApi.sk_canvas_draw_points (Handle, mode, (IntPtr)points.Length, points, paint.Handle);
		}

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
			SkiaApi.sk_canvas_draw_image_rect (Handle, image.Handle, IntPtr.Zero, ref dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawImage (SKImage image, SKRect source, SKRect dest, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			SkiaApi.sk_canvas_draw_image_rect (Handle, image.Handle, ref source, ref dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

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
			SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, ref matrix, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawPicture (SKPicture picture, SKPaint paint = null)
		{
			if (picture == null)
				throw new ArgumentNullException (nameof (picture));
			SkiaApi.sk_canvas_draw_picture (Handle, picture.Handle, IntPtr.Zero, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawDrawable (SKDrawable drawable, ref SKMatrix matrix)
		{
			if (drawable == null)
				throw new ArgumentNullException (nameof (drawable));
			SkiaApi.sk_canvas_draw_drawable (Handle, drawable.Handle, ref matrix);
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
			SkiaApi.sk_canvas_draw_bitmap_rect (Handle, bitmap.Handle, IntPtr.Zero, ref dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawBitmap (SKBitmap bitmap, SKRect source, SKRect dest, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			SkiaApi.sk_canvas_draw_bitmap_rect (Handle, bitmap.Handle, ref source, ref dest, paint == null ? IntPtr.Zero : paint.Handle);
		}

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

		public void DrawText (SKTextBlob text, float x, float y, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text_blob (Handle, text.Handle, x, y, paint.Handle);
		}

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

			SkiaApi.sk_canvas_draw_text (Handle, text, text.Length, x, y, paint.Handle);
		}

		public void DrawPositionedText (string text, SKPoint [] points, SKPaint paint)
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

		public void DrawPositionedText (byte[] text, SKPoint [] points, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));

			SkiaApi.sk_canvas_draw_pos_text (Handle, text, text.Length, points, paint.Handle);
		}

		public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, SKPoint offset, SKPaint paint)
		{
			DrawTextOnPath (buffer, length, path, offset.X, offset.Y, paint);
		}

		public void DrawTextOnPath (IntPtr buffer, int length, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			
			SkiaApi.sk_canvas_draw_text_on_path (Handle, buffer, length, path.Handle, hOffset, vOffset, paint.Handle);
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
			
			SkiaApi.sk_canvas_draw_text (Handle, buffer, length, x, y, paint.Handle);
		}

		public void DrawPositionedText (IntPtr buffer, int length, SKPoint[] points, SKPaint paint)
		{
			if (buffer == IntPtr.Zero && length != 0)
				throw new ArgumentNullException (nameof (buffer));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (points == null)
				throw new ArgumentNullException (nameof (points));
			
			SkiaApi.sk_canvas_draw_pos_text (Handle, buffer, length, points, paint.Handle);
		}

		public void DrawTextOnPath (string text, SKPath path, SKPoint offset, SKPaint paint)
		{
			DrawTextOnPath (text, path, offset.X, offset.Y, paint);
		}

		public void DrawTextOnPath (string text, SKPath path, float hOffset, float vOffset, SKPaint paint)
		{
			if (text == null)
				throw new ArgumentNullException (nameof (text));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
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
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));
			if (paint == null)
				throw new ArgumentNullException (nameof (paint));

			SkiaApi.sk_canvas_draw_text_on_path (Handle, text, text.Length, path.Handle, hOffset, vOffset, paint.Handle);
		}

		public void Flush ()
		{
			SkiaApi.sk_canvas_flush (Handle);
		}

		public void DrawAnnotation (SKRect rect, string key, SKData value)
		{
			SkiaApi.sk_canvas_draw_annotation (Handle, ref rect, StringUtilities.GetEncodedText (key, SKTextEncoding.Utf8), value == null ? IntPtr.Zero : value.Handle);
		}

		public void DrawUrlAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_url_annotation (Handle, ref rect, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawUrlAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawUrlAnnotation (rect, data);
			return data;
		}

		public void DrawNamedDestinationAnnotation (SKPoint point, SKData value)
		{
			SkiaApi.sk_canvas_draw_named_destination_annotation (Handle, ref point, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawNamedDestinationAnnotation (SKPoint point, string value)
		{
			var data = SKData.FromCString (value);
			DrawNamedDestinationAnnotation (point, data);
			return data;
		}

		public void DrawLinkDestinationAnnotation (SKRect rect, SKData value)
		{
			SkiaApi.sk_canvas_draw_link_destination_annotation (Handle, ref rect, value == null ? IntPtr.Zero : value.Handle);
		}

		public SKData DrawLinkDestinationAnnotation (SKRect rect, string value)
		{
			var data = SKData.FromCString (value);
			DrawLinkDestinationAnnotation (rect, data);
			return data;
		}

		public void DrawBitmapNinePatch (SKBitmap bitmap, SKRectI center, SKRect dst, SKPaint paint = null)
		{
			if (bitmap == null)
				throw new ArgumentNullException (nameof (bitmap));
			// the "center" rect must fit inside the bitmap "rect"
			if (!SKRect.Create (bitmap.Info.Size).Contains (center))
				throw new ArgumentException ("Center rectangle must be contained inside the bitmap bounds.", nameof (center));

			SkiaApi.sk_canvas_draw_bitmap_nine (Handle, bitmap.Handle, ref center, ref dst, paint == null ? IntPtr.Zero : paint.Handle);
		}

		public void DrawImageNinePatch (SKImage image, SKRectI center, SKRect dst, SKPaint paint = null)
		{
			if (image == null)
				throw new ArgumentNullException (nameof (image));
			// the "center" rect must fit inside the image "rect"
			if (!SKRect.Create (image.Width, image.Height).Contains (center))
				throw new ArgumentException ("Center rectangle must be contained inside the image bounds.", nameof (center));

			SkiaApi.sk_canvas_draw_image_nine (Handle, image.Handle, ref center, ref dst, paint == null ? IntPtr.Zero : paint.Handle);
		}

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

			unsafe {
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
						fColors = c,
					};
					if (lattice.Bounds != null) {
						var bounds = lattice.Bounds.Value;
						nativeLattice.fBounds = &bounds;
					}
					SkiaApi.sk_canvas_draw_bitmap_lattice (Handle, bitmap.Handle, ref nativeLattice, ref dst, paint == null ? IntPtr.Zero : paint.Handle);
				}
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
			
			unsafe {
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
						fColors = c,
					};
					if (lattice.Bounds != null) {
						var bounds = lattice.Bounds.Value;
						nativeLattice.fBounds = &bounds;
					}
					SkiaApi.sk_canvas_draw_image_lattice (Handle, image.Handle, ref nativeLattice, ref dst, paint == null ? IntPtr.Zero : paint.Handle);
				}
			}
		}

		public void ResetMatrix ()
		{
			SkiaApi.sk_canvas_reset_matrix (Handle);
		}

		public void SetMatrix (SKMatrix matrix)
		{
			SkiaApi.sk_canvas_set_matrix (Handle, ref matrix);
		}

		public SKMatrix TotalMatrix {
			get {
				SKMatrix matrix = new SKMatrix();
				SkiaApi.sk_canvas_get_total_matrix (Handle, ref matrix);
				return matrix;
			}
		}

		public int SaveCount => SkiaApi.sk_canvas_get_save_count (Handle);

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy(vmode, vertices, colors);
			DrawVertices(vert, SKBlendMode.Modulate, paint);
		}

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy(vmode, vertices, texs, colors);
			DrawVertices(vert, SKBlendMode.Modulate, paint);
		}

		public void DrawVertices (SKVertexMode vmode, SKPoint[] vertices, SKPoint[] texs, SKColor[] colors, UInt16[] indices, SKPaint paint)
		{
			var vert = SKVertices.CreateCopy(vmode, vertices, texs, colors, indices);
			DrawVertices(vert, SKBlendMode.Modulate, paint);
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

