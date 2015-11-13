using System;
using System.Runtime.InteropServices;

using sk_surface_t = System.IntPtr;
using sk_canvas_t = System.IntPtr;
using sk_image_t = System.IntPtr;
using sk_paint_t = System.IntPtr;
using sk_shader_t = System.IntPtr;
using sk_maskfilter_t = System.IntPtr;
using sk_path_t = System.IntPtr;
using sk_picture_t = System.IntPtr;

namespace SkiaSharp
{
	internal static class SkiaApi
	{
		const string SKIA = "/tmp/libskia.dylib";

		[DllImport (SKIA)] public extern static void         sk_surface_unref             (sk_surface_t t);
		[DllImport (SKIA)] public extern static sk_surface_t sk_surface_new_raster        (ref SKImageInfo info, ref SKSurfaceProps pros);
		[DllImport (SKIA)] public extern static sk_surface_t sk_surface_new_raster        (ref SKImageInfo info, IntPtr propsZero);
		[DllImport (SKIA)] public extern static sk_surface_t sk_surface_new_raster_direct (ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, ref SKSurfaceProps props);
		[DllImport (SKIA)] public extern static sk_surface_t sk_surface_new_raster_direct (ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, IntPtr propsZero);
		[DllImport (SKIA)] public extern static sk_canvas_t  sk_surface_get_canvas        (sk_surface_t t);
		[DllImport (SKIA)] public extern static sk_image_t   sk_surface_new_image_snapshot(sk_surface_t t);

		[DllImport (SKIA)] public extern static void sk_canvas_save           (sk_canvas_t t);
		[DllImport (SKIA)] public extern static void sk_canvas_save_layer     (sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_save_layer     (sk_canvas_t t, IntPtr rectZero, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_restore        (sk_canvas_t t);
		[DllImport (SKIA)] public extern static void sk_canvas_translate      (sk_canvas_t t, float dx, float dy);
		[DllImport (SKIA)] public extern static void sk_canvas_scale          (sk_canvas_t t, float sx, float sy);
		[DllImport (SKIA)] public extern static void sk_canvas_rotate_degrees (sk_canvas_t t, float degrees);
		[DllImport (SKIA)] public extern static void sk_canvas_rotate_radians (sk_canvas_t t, float radians);
		[DllImport (SKIA)] public extern static void sk_canvas_skew           (sk_canvas_t t, float sx, float sy);
		[DllImport (SKIA)] public extern static void sk_canvas_concat         (sk_canvas_t t, ref SKMatrix m);
		[DllImport (SKIA)] public extern static void sk_canvas_clip_rect      (sk_canvas_t t, ref SKRect rect);
		[DllImport (SKIA)] public extern static void sk_canvas_clip_path      (sk_canvas_t t, sk_path_t p);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_paint     (sk_canvas_t t, sk_paint_t p);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_rect      (sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_oval      (sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_path      (sk_canvas_t t, sk_path_t path, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_image     (sk_canvas_t t, sk_image_t image, float x, float y, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_image_rect(sk_canvas_t t, sk_image_t image, ref SKRect src, ref SKRect dest, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_image_rect(sk_canvas_t t, sk_image_t image, IntPtr srcZero, ref SKRect dest, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_picture   (sk_canvas_t t, sk_picture_t pict, ref SKMatrix mat, sk_paint_t paint);
		[DllImport (SKIA)] public extern static void sk_canvas_draw_picture   (sk_canvas_t t, sk_picture_t pict, IntPtr matZero, sk_paint_t paint);

		[DllImport (SKIA)] public extern static sk_paint_t   sk_paint_new               ();
		[DllImport (SKIA)] public extern static void         sk_paint_delete            (sk_paint_t t);
		[DllImport (SKIA)] public extern static bool         sk_paint_is_antialias      (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_antialias     (sk_paint_t t, bool v);
		[DllImport (SKIA)] public extern static SKColor      sk_paint_get_color         (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_color         (sk_paint_t t, SKColor color);
		[DllImport (SKIA)] public extern static bool         sk_paint_is_stroke         (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_stroke        (sk_paint_t t, bool v);
		[DllImport (SKIA)] public extern static float        sk_paint_get_stroke_width  (sk_paint_t paint);
		[DllImport (SKIA)] public extern static void         sk_paint_set_stroke_width  (sk_paint_t t, float width);
		[DllImport (SKIA)] public extern static float        sk_paint_get_stroke_miter  (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_stroke_miter  (sk_paint_t t, float miter);
		[DllImport (SKIA)] public extern static SKStrokeCap  sk_paint_get_stroke_cap    (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_stroke_cap    (sk_paint_t t, SKStrokeCap cap);
		[DllImport (SKIA)] public extern static SKStrokeJoin sk_paint_get_stroke_join   (sk_paint_t t);
		[DllImport (SKIA)] public extern static void         sk_paint_set_stroke_join   (sk_paint_t t, SKStrokeJoin join);
		[DllImport (SKIA)] public extern static void         sk_paint_set_shader        (sk_paint_t t, sk_shader_t shader);
		[DllImport (SKIA)] public extern static void         sk_paint_set_maskfilter    (sk_paint_t t, sk_maskfilter_t filt);
		[DllImport (SKIA)] public extern static void         sk_paint_set_xfermode_mode (sk_paint_t t, SKXferMode mode);

		[DllImport (SKIA)] public extern static sk_path_t    sk_path_new();
		[DllImport (SKIA)] public extern static void         sk_path_delete     (sk_path_t t);
		[DllImport (SKIA)] public extern static void         sk_path_move_to    (sk_path_t t, float x, float y);
		[DllImport (SKIA)] public extern static void         sk_path_line_to    (sk_path_t t, float x, float y);
		[DllImport (SKIA)] public extern static void         sk_path_quad_to    (sk_path_t t, float x0, float y0, float x1, float y1);
		[DllImport (SKIA)] public extern static void         sk_path_conic_to   (sk_path_t t, float x0, float y0, float x1, float y1, float w);
		[DllImport (SKIA)] public extern static void         sk_path_cubic_to   (sk_path_t t, float x0, float y0, float x1, float y1, float x2, float y2);
		[DllImport (SKIA)] public extern static void         sk_path_close      (sk_path_t t);
		[DllImport (SKIA)] public extern static void         sk_path_add_rect   (sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport (SKIA)] public extern static void         sk_path_add_oval   (sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport (SKIA)] public extern static bool         sk_path_get_bounds (sk_path_t t, out SKRect rect);

		[DllImport (SKIA)] public extern static void         sk_maskfilter_unref        (sk_maskfilter_t t);
		[DllImport (SKIA)] public extern static sk_maskfilter_t sk_maskfilter_new_blur  (SKBlurStyle style, float sigma);

		[DllImport (SKIA)] public extern static void         sk_shader_unref (sk_shader_t t);
		[DllImport (SKIA)] public extern static void         sk_image_unref (sk_image_t t);
		[DllImport (SKIA)] public extern static void         sk_picture_unref (sk_image_t t);


	}
}

