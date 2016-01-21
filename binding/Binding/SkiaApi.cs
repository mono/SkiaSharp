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
using sk_picture_recorder_t = System.IntPtr;
using sk_typeface_t = System.IntPtr;
using sk_stream_t = System.IntPtr;
using sk_stream_filestream_t = System.IntPtr;
using sk_stream_memorystream_t = System.IntPtr;
using sk_stream_assetstream_t = System.IntPtr;
using sk_stream_managedstream_t = System.IntPtr;
using sk_stream_streamrewindable_t = System.IntPtr;

namespace SkiaSharp
{
	internal static class SkiaApi
	{
#if MONOTOUCH
		const string SKIA = "@rpath/libskia_ios.framework/libskia_ios";
#elif __ANDROID__
		const string SKIA = "libskia_android.so";
#elif XAMARIN_MAC
		const string SKIA = "liblibskia_osx.dylib";
#elif WINDOWS_DESKTOP
		const string SKIA = "libskia_windows.dll";
#else
		const string SKIA = "/tmp/libskia.dylib";
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
		public extern static void sk_canvas_save(sk_canvas_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_save_layer(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_save_layer(sk_canvas_t t, IntPtr rectZero, sk_paint_t paint);
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

		// These can be overloaded to take an IntPtr which points to some utf8 text.
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text(sk_canvas_t t, byte[] text, int len, float x, float y, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_pos_text(sk_canvas_t t, byte[] text, int len, [In] SKPoint[] points, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_text_on_path(sk_canvas_t t, byte[] text, int len, sk_path_t path, float hOffset, float vOffset, sk_paint_t paint);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_paint_t sk_paint_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_delete(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_paint_is_antialias(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_antialias(sk_paint_t t, bool v);
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
		public extern static void sk_paint_set_maskfilter(sk_paint_t t, sk_maskfilter_t filt);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_xfermode_mode(sk_paint_t t, SKXferMode mode);
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


		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_image_new_raster_copy(ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_image_t sk_image_new_from_encoded(sk_data_t encoded, ref SKRectI subset);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_image_encode(sk_image_t t);
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
		public extern static void sk_path_line_to(sk_path_t t, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_quad_to(sk_path_t t, float x0, float y0, float x1, float y1);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_conic_to(sk_path_t t, float x0, float y0, float x1, float y1, float w);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_cubic_to(sk_path_t t, float x0, float y0, float x1, float y1, float x2, float y2);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_close(sk_path_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rect(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_oval(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static bool sk_path_get_bounds(sk_path_t t, out SKRect rect);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_maskfilter_unref(sk_maskfilter_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_maskfilter_t sk_maskfilter_new_blur(SKBlurStyle style, float sigma);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_shader_unref(sk_shader_t t);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_empty();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_with_copy(IntPtr src, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_from_malloc(IntPtr malloc, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_data_new_subset(sk_data_t src, IntPtr offset, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_data_unref(sk_data_t d);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_data_get_size(sk_data_t d);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_data_get_data(sk_data_t d);

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

		// Streams
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
		public extern static void sk_managedstream_set_delegates(IntPtr pRead, IntPtr pIsAtEnd, IntPtr pRewind, IntPtr pGetPosition, IntPtr pSeek, IntPtr pMove, IntPtr pGetLength, IntPtr pCreateNew, IntPtr pDestroy);
	}
}

