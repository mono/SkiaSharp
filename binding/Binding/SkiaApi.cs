//
// Low-level P/Invoke declarations
//
// Author:
//   Miguel de Icaza
//
// Copyright 2015 Xamarin Inc
//
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
using sk_data_t = System.IntPtr;
using sk_string_t = System.IntPtr;
using sk_picture_recorder_t = System.IntPtr;
using sk_typeface_t = System.IntPtr;
using sk_font_table_tag_t = System.UInt32;
using sk_stream_t = System.IntPtr;
using sk_stream_filestream_t = System.IntPtr;
using sk_stream_memorystream_t = System.IntPtr;
using sk_stream_assetstream_t = System.IntPtr;
using sk_stream_managedstream_t = System.IntPtr;
using sk_stream_streamrewindable_t = System.IntPtr;
using sk_bitmap_t = System.IntPtr;
using sk_imagedecoder_t = System.IntPtr;
using sk_imagefilter_croprect_t = System.IntPtr;
using sk_imagefilter_t = System.IntPtr;
using sk_colorfilter_t = System.IntPtr;

namespace SkiaSharp
{
	internal static class SkiaApi
	{
#if MONOTOUCH
		const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __ANDROID__
		const string SKIA = "libSkiaSharp.so";
#elif XAMARIN_MAC
		const string SKIA = "libSkiaSharp.dylib";
#elif DESKTOP
		const string SKIA = "libSkiaSharp.dll"; // redirected using .dll.config to 'libSkiaSharp.dylib' on OS X
#elif WINDOWS_UWP
		const string SKIA = "libSkiaSharp.dll";
#else
		const string SKIA = "libSkiaSharp";
#endif

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_surface_unref(sk_surface_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_raster(ref SKImageInfo info, ref SKSurfaceProps pros);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_raster(ref SKImageInfo info, IntPtr propsZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_raster_direct(ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, ref SKSurfaceProps props);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_raster_direct(ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, IntPtr propsZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_canvas_t sk_surface_get_canvas(sk_surface_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_surface_new_image_snapshot(sk_surface_t t);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_canvas_save(sk_canvas_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_canvas_save_layer(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_canvas_save_layer(sk_canvas_t t, IntPtr rectZero, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_restore(sk_canvas_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_canvas_get_save_count(sk_canvas_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_restore_to_count(sk_canvas_t t, int saveCount);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_translate(sk_canvas_t t, float dx, float dy);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_scale(sk_canvas_t t, float sx, float sy);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_rotate_degrees(sk_canvas_t t, float degrees);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_rotate_radians(sk_canvas_t t, float radians);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_skew(sk_canvas_t t, float sx, float sy);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_concat(sk_canvas_t t, ref SKMatrix m);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_rect(sk_canvas_t t, ref SKRect rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_path(sk_canvas_t t, sk_path_t p);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_paint(sk_canvas_t t, sk_paint_t p);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_rect(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_round_rect (sk_canvas_t t, ref SKRect rect, float rx, float ry, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_oval(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_path(sk_canvas_t t, sk_path_t path, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_image(sk_canvas_t t, sk_image_t image, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_image_rect(sk_canvas_t t, sk_image_t image, ref SKRect src, ref SKRect dest, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_image_rect(sk_canvas_t t, sk_image_t image, IntPtr srcZero, ref SKRect dest, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_picture(sk_canvas_t t, sk_picture_t pict, ref SKMatrix mat, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_picture(sk_canvas_t t, sk_picture_t pict, IntPtr matZero, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_color(sk_canvas_t t, SKColor color, SKXferMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_points(sk_canvas_t t, SKPointMode mode, IntPtr count, [In] SKPoint[] points, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_point(sk_canvas_t t, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_point_color(sk_canvas_t t, float x, float y, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_line(sk_canvas_t t, float x0, float y0, float x1, float y1, sk_paint_t paint);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text(sk_canvas_t t, byte[] text, int len, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_pos_text(sk_canvas_t t, byte[] text, int len, [In] SKPoint[] points, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text_on_path(sk_canvas_t t, byte[] text, int len, sk_path_t path, float hOffset, float vOffset, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text(sk_canvas_t t, IntPtr text, int len, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_pos_text(sk_canvas_t t, IntPtr text, int len, [In] SKPoint[] points, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text_on_path(sk_canvas_t t, IntPtr text, int len, sk_path_t path, float hOffset, float vOffset, sk_paint_t paint);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_bitmap(sk_canvas_t t, sk_bitmap_t bitmap, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_bitmap_rect(sk_canvas_t t, sk_bitmap_t bitmap, ref SKRect src, ref SKRect dest, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_bitmap_rect(sk_canvas_t t, sk_bitmap_t bitmap, IntPtr srcZero, ref SKRect dest, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static  void sk_canvas_reset_matrix(sk_canvas_t canvas);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static  void sk_canvas_set_matrix(sk_canvas_t canvas, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_get_total_matrix(sk_canvas_t canvas, ref SKMatrix matrix);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_rect_with_operation(sk_canvas_t t, ref SKRect crect, SKRegionOperation op, bool doAA);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_path_with_operation(sk_canvas_t t, sk_path_t cpath, SKRegionOperation op, bool doAA);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_canvas_get_clip_device_bounds(sk_canvas_t t, ref SKRectI cbounds);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_canvas_get_clip_bounds(sk_canvas_t t, ref SKRect cbounds);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_paint_t sk_paint_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_delete(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_paint_is_antialias(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_antialias(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_paint_is_dither(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_dither(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_paint_is_verticaltext(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_verticaltext(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKColor sk_paint_get_color(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_color(sk_paint_t t, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_paint_is_stroke(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_stroke(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_stroke_width(sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_stroke_width(sk_paint_t t, float width);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_stroke_miter(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_stroke_miter(sk_paint_t t, float miter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKStrokeCap sk_paint_get_stroke_cap(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_stroke_cap(sk_paint_t t, SKStrokeCap cap);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKStrokeJoin sk_paint_get_stroke_join(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_stroke_join(sk_paint_t t, SKStrokeJoin join);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_shader(sk_paint_t t, sk_shader_t shader);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_paint_get_shader(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_maskfilter(sk_paint_t t, sk_maskfilter_t filter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_paint_get_maskfilter(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_colorfilter(sk_paint_t t, sk_colorfilter_t filter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_paint_get_colorfilter(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_imagefilter(sk_paint_t t, sk_imagefilter_t filter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_paint_get_imagefilter(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_xfermode_mode(sk_paint_t t, SKXferMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKXferMode sk_paint_get_xfermode_mode(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_filter_quality(sk_paint_t t, SKFilterQuality filterQuality);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKFilterQuality sk_paint_get_filter_quality(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_paint_get_typeface(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_typeface(sk_paint_t t, sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_textsize(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_textsize(sk_paint_t t, float size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKTextAlign sk_paint_get_text_align(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_text_align(sk_paint_t t, SKTextAlign align);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKTextEncoding sk_paint_get_text_encoding(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_text_encoding(sk_paint_t t, SKTextEncoding encoding);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_text_scale_x(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_text_scale_x(sk_paint_t t, float scale);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_text_skew_x(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_text_skew_x(sk_paint_t t, float skew);

		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_measure_text (sk_paint_t t, byte [] text, IntPtr length, ref SKRect bounds);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_measure_text (sk_paint_t t, byte [] text, IntPtr length, IntPtr boundsZero);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_measure_text(sk_paint_t t, IntPtr text, IntPtr length, ref SKRect bounds);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_measure_text(sk_paint_t t, IntPtr text, IntPtr length, IntPtr boundsZero);


		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_paint_break_text(sk_paint_t t, IntPtr text, IntPtr length, float maxWidth, out float measuredWidth);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_paint_break_text (sk_paint_t t, byte [] text, IntPtr length, float maxWidth, out float measuredWidth);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_paint_get_text_path(sk_paint_t t, IntPtr text, IntPtr length, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_paint_get_text_path(sk_paint_t t, byte [] text, IntPtr length, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_paint_get_pos_text_path(sk_paint_t t, IntPtr text, IntPtr length, [In] SKPoint[] points);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_paint_get_pos_text_path(sk_paint_t t, byte [] text, IntPtr length, [In] SKPoint[] points);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_paint_get_fontmetrics(sk_paint_t t, out SKFontMetrics fontMetrics, float scale);


		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_image_new_raster_copy(ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_image_new_from_encoded(sk_data_t encoded, ref SKRectI subset);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_image_new_from_encoded(sk_data_t encoded, IntPtr subsetPtr);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_image_encode(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_image_encode_specific(sk_image_t t, SKImageEncodeFormat format, int quality);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_image_unref(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_image_get_width(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_image_get_height(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint sk_image_get_unique_id(sk_image_t t);


		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_path_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_delete(sk_path_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_move_to(sk_path_t t, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rmove_to(sk_path_t t, float dx, float dy);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_line_to(sk_path_t t, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rline_to(sk_path_t t, float dx, float dy);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_quad_to(sk_path_t t, float x0, float y0, float x1, float y1);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rquad_to(sk_path_t t, float dx0, float dy0, float dx1, float dy1);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_conic_to(sk_path_t t, float x0, float y0, float x1, float y1, float w);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rconic_to(sk_path_t t, float dx0, float dy0, float dx1, float dy1, float w);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_cubic_to(sk_path_t t, float x0, float y0, float x1, float y1, float x2, float y2);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rcubic_to(sk_path_t t, float dx0, float dy0, float dx1, float dy1, float dx2, float dy2);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_close(sk_path_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rect(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rect_start(sk_path_t t, ref SKRect rect, SKPathDirection direction, uint startIndex);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_oval(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_arc(sk_path_t t, ref SKRect rect, float startAngle, float sweepAngle);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_path_get_bounds(sk_path_t t, out SKRect rect);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPathFillType sk_path_get_filltype (sk_path_t t);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_set_filltype (sk_path_t t, SKPathFillType filltype);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_path_clone (sk_path_t t);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_path_transform (sk_path_t t, ref SKMatrix matrix);

		// SkMaskFilter
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_maskfilter_unref(sk_maskfilter_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_blur(SKBlurStyle style, float sigma);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_emboss(float blurSigma, float[] direction, float ambient, float specular);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_table(byte[] table /*[256]*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_gamma(float gamma);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_clip(byte min, byte max);

		// SkImageFilter::CropRect
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_croprect_t sk_imagefilter_croprect_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_croprect_t sk_imagefilter_croprect_new_with_rect(ref SKRect rect, SKCropRectFlags flags);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagefilter_croprect_destructor(sk_imagefilter_croprect_t cropRect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagefilter_croprect_get_rect(sk_imagefilter_croprect_t cropRect, out SKRect rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCropRectFlags sk_imagefilter_croprect_get_flags(sk_imagefilter_croprect_t cropRect);

		// SkImageFilter
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagefilter_unref(sk_imagefilter_t filter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_matrix(ref SKMatrix matrix, SKFilterQuality quality, sk_imagefilter_t input /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_alpha_threshold(ref SKRectI region, float innerThreshold, float outerThreshold, sk_imagefilter_t input /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_blur(float sigmaX, float sigmaY, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_color_filter(sk_colorfilter_t cf, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_compose(sk_imagefilter_t outer, sk_imagefilter_t inner);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_displacement_map_effect(SKDisplacementMapEffectChannelSelectorType xChannelSelector, SKDisplacementMapEffectChannelSelectorType yChannelSelector, float scale, sk_imagefilter_t displacement, sk_imagefilter_t color /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_downsample(float scale, sk_imagefilter_t input /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, SKColor color, SKDropShadowImageFilterShadowMode shadowMode, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse(ref SKPoint3 direction, SKColor lightColor, float surfaceScale, float kd, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse(ref SKPoint3 location, SKColor lightColor, float surfaceScale, float kd, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse(ref SKPoint3 location, ref SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float kd, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_distant_lit_specular(ref SKPoint3 direction, SKColor lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_point_lit_specular(ref SKPoint3 location, SKColor lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_spot_lit_specular(ref SKPoint3 location, ref SKPoint3 target, float specularExponent, float cutoffAngle, SKColor lightColor, float surfaceScale, float ks, float shininess, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_magnifier(ref SKRect src, float inset, sk_imagefilter_t input /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_matrix_convolution(ref SKSizeI kernelSize, float[] kernel, float gain, float bias, ref SKPointI kernelOffset, SKMatrixConvolutionTileMode tileMode, bool convolveAlpha, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_merge(sk_imagefilter_t[] filters, int count, SKXferMode[] modes /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_dilate(int radiusX, int radiusY, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_erode(int radiusX, int radiusY, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_offset(float dx, float dy, sk_imagefilter_t input /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_picture(sk_picture_t picture);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_picture_with_croprect(sk_picture_t picture, ref SKRect cropRect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_picture_for_localspace(sk_picture_t picture, ref SKRect cropRect, SKFilterQuality filterQuality);

		// SkColorFilter

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_colorfilter_unref(sk_colorfilter_t filter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_mode(SKColor c, SKXferMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_lighting(SKColor mul, SKColor add);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_compose(sk_colorfilter_t outer, sk_colorfilter_t inner);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_color_cube(sk_data_t cubeData, int cubeDimension);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_color_matrix(float[] array/*[20]*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_luma_color();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_table(byte[] table/*[256]*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colorfilter_t sk_colorfilter_new_table_argb(byte[] tableA/*[256]*/, byte[] tableR/*[256]*/, byte[] tableG/*[256]*/, byte[] tableB/*[256]*/);

		// SkData
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_empty();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_with_copy(IntPtr src, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_with_copy(byte[] src, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_from_malloc(IntPtr malloc, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_from_malloc(byte[] malloc, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_subset(sk_data_t src, IntPtr offset, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_from_file(string path);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_from_stream(sk_stream_t stream, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_data_unref(sk_data_t d);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_data_get_size(sk_data_t d);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_data_get_data(sk_data_t d);

		// SkString
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_string_t sk_string_new_empty();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_string_t sk_string_new_with_copy(byte[] src, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_string_destructor(sk_string_t skstring);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_string_get_size(sk_string_t skstring);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_string_get_c_str(sk_string_t skstring);

		// SkPicture
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_picture_recorder_delete(sk_picture_recorder_t r);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_picture_recorder_t sk_picture_recorder_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_canvas_t sk_picture_recorder_begin_recording(sk_picture_recorder_t r, ref SKRect rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_picture_t sk_picture_recorder_end_recording(sk_picture_recorder_t r);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_picture_unref(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint sk_picture_get_unique_id(sk_picture_t p);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKRect sk_picture_get_bounds(sk_picture_t p);

		// SkShader
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_shader_unref(sk_shader_t t);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_empty();

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_color(SKColor color);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_local_matrix(sk_shader_t proxy, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_color_filter(sk_shader_t proxy, sk_colorfilter_t filter);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_bitmap(sk_bitmap_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_bitmap(sk_bitmap_t src, SKShaderTileMode tmx, SKShaderTileMode tmy, IntPtr matrixZero);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_linear_gradient([In] SKPoint[] points, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_linear_gradient([In] SKPoint[] points, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, IntPtr matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_radial_gradient(ref SKPoint center, float radius, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_radial_gradient(ref SKPoint center, float radius, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, IntPtr matrixZero);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_sweep_gradient(ref SKPoint center, [In] SKColor[] colors, float[] colorPos, int count, IntPtr matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_sweep_gradient(ref SKPoint center, [In] SKColor[] colors, float[] colorPos, int count, ref SKMatrix matrixZero);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_two_point_conical_gradient(ref SKPoint start, float startRadius, ref SKPoint end, float endRadius, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_two_point_conical_gradient(ref SKPoint start, float startRadius, ref SKPoint end, float endRadius, [In] SKColor[] colors, float[] colorPos, int count, SKShaderTileMode mode, IntPtr matrixZero);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, IntPtr tileSizeZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, ref SKPointI tileSize);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, IntPtr tileSizeZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, ref SKPointI tileSize);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_compose(sk_shader_t shaderA, sk_shader_t shaderB);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_compose_with_mode(sk_shader_t shaderA, sk_shader_t shaderB, SKXferMode mode);

		// Typeface
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_name(string str, SKTypefaceStyle style);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_typeface(IntPtr typeface, SKTypefaceStyle style);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_file(string path, int index);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_stream(sk_stream_assetstream_t stream, int index);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_typeface_unref(sk_typeface_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_typeface_chars_to_glyphs(sk_typeface_t t, IntPtr chars, SKEncoding encoding, IntPtr glyphPtr, int glyphCount);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_string_t sk_typeface_get_family_name(sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_typeface_count_tables(sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_typeface_get_table_tags(sk_typeface_t typeface, sk_font_table_tag_t[] tags);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_typeface_get_table_size(sk_typeface_t typeface, sk_font_table_tag_t tag);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_typeface_get_table_data(sk_typeface_t typeface, sk_font_table_tag_t tag, IntPtr offset, IntPtr length, byte[] data);

		// Streams
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_read(sk_stream_t stream, IntPtr buffer, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_skip(sk_stream_t stream, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_is_at_end(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SByte sk_stream_read_s8(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static Int16 sk_stream_read_s16(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static Int32 sk_stream_read_s32(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static Byte sk_stream_read_u8(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static UInt16 sk_stream_read_u16(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static UInt32 sk_stream_read_u32(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_read_bool(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_rewind(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_has_position(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_get_position(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_seek(sk_stream_t stream, IntPtr position);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_move(sk_stream_t stream, long offset);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_stream_has_length(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_get_length(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_filestream_t sk_filestream_new(string path);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_memorystream_t sk_memorystream_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_memorystream_t sk_memorystream_new_with_length(IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_memorystream_t sk_memorystream_new_with_data(IntPtr data, IntPtr length, bool copyData);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_memorystream_t sk_memorystream_new_with_data(byte[] data, IntPtr length, bool copyData);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_memorystream_t sk_memorystream_new_with_skdata(SKData data);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_memorystream_set_memory(sk_stream_memorystream_t s, IntPtr data, IntPtr length, bool copyData);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_memorystream_set_memory(sk_stream_memorystream_t s, byte[] data, IntPtr length, bool copyData);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_managedstream_t sk_managedstream_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_managedstream_set_delegates(IntPtr pRead, IntPtr pPeek, IntPtr pIsAtEnd, IntPtr pRewind, IntPtr pGetPosition, IntPtr pSeek, IntPtr pMove, IntPtr pGetLength, IntPtr pCreateNew, IntPtr pDestroy);

		// Bitmap
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_bitmap_t sk_bitmap_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_destructor(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_get_info(sk_bitmap_t b, out SKImageInfo info);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)] 
		public extern static IntPtr sk_bitmap_get_pixels(sk_bitmap_t b, out IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_get_pixel_colors(sk_bitmap_t b, [Out] SKColor[] colors);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_pixel_colors(sk_bitmap_t b, [In] SKColor[] colors);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_reset(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_bitmap_get_row_bytes(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_bitmap_get_byte_count(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_is_null(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_is_immutable(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_immutable(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_is_volatile(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_volatile(sk_bitmap_t b, bool value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_erase(sk_bitmap_t cbitmap, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_erase_rect(sk_bitmap_t cbitmap, SKColor color, ref SKRectI rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKColor sk_bitmap_get_pixel_color(sk_bitmap_t cbitmap, int x, int y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_pixel_color(sk_bitmap_t cbitmap, int x, int y, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_copy(sk_bitmap_t cbitmap, sk_bitmap_t dst, SKColorType ct);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_can_copy_to(sk_bitmap_t cbitmap, SKColorType ct);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_lock_pixels(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_unlock_pixels(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_bitmap_try_alloc_pixels(sk_bitmap_t cbitmap, ref SKImageInfo requestedInfo, IntPtr rowBytes);

		// Image Decoder
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_destructor(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKImageDecoderFormat sk_imagedecoder_get_decoder_format(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKImageDecoderFormat sk_imagedecoder_get_stream_format(sk_stream_streamrewindable_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static string sk_imagedecoder_get_format_name_from_format(SKImageDecoderFormat cformat);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static string sk_imagedecoder_get_format_name_from_decoder(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_get_skip_writing_zeros(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_set_skip_writing_zeros(sk_imagedecoder_t cdecoder, bool skip);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_get_dither_image(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_set_dither_image(sk_imagedecoder_t cdecoder, bool dither);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_get_prefer_quality_over_speed(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_set_prefer_quality_over_speed(sk_imagedecoder_t cdecoder, bool qualityOverSpeed);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_get_require_unpremultiplied_colors(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_set_require_unpremultiplied_colors(sk_imagedecoder_t cdecoder, bool request);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_imagedecoder_get_sample_size(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_set_sample_size(sk_imagedecoder_t cdecoder, int size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_imagedecoder_cancel_decode(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_should_cancel_decode(sk_imagedecoder_t cdecoder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKImageDecoderResult sk_imagedecoder_decode(sk_imagedecoder_t cdecoder, sk_stream_t cstream, sk_bitmap_t bitmap, SKColorType pref, SKImageDecoderMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagedecoder_t sk_imagedecoder_factory(sk_stream_streamrewindable_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_decode_file(string file, sk_bitmap_t bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_decode_memory(byte[] buffer, IntPtr size, sk_bitmap_t bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_decode_memory(IntPtr buffer, IntPtr size, sk_bitmap_t bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_imagedecoder_decode_stream(sk_stream_streamrewindable_t cstream, sk_bitmap_t bitmap, SKColorType pref, SKImageDecoderMode mode, ref SKImageDecoderFormat format);
	}
}

