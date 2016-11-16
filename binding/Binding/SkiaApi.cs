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

using GRBackendObject = System.IntPtr;
using GRBackendContext = System.IntPtr;

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
using sk_wstream_t = System.IntPtr;
using sk_wstream_dynamicmemorystream_t = System.IntPtr;
using sk_wstream_filestream_t = System.IntPtr;
using sk_bitmap_t = System.IntPtr;
using sk_codec_t = System.IntPtr;
using sk_imagefilter_croprect_t = System.IntPtr;
using sk_imagefilter_t = System.IntPtr;
using sk_colorfilter_t = System.IntPtr;
using sk_document_t = System.IntPtr;
using sk_colorspace_t = System.IntPtr;
using sk_path_iterator_t = System.IntPtr;
using sk_path_effect_t = System.IntPtr;
using sk_pathmeasure_t = System.IntPtr;
using sk_pixelref_factory_t = System.IntPtr;
using sk_colortable_t = System.IntPtr;
using gr_context_t = System.IntPtr;
using gr_glinterface_t = System.IntPtr;
using sk_opbuilder_t = System.IntPtr;
using sk_region_t = System.IntPtr;

namespace SkiaSharp
{
	internal static class SkiaApi
	{
#if __TVOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __IOS__ && __UNIFIED__
		private const string SKIA = "@rpath/libSkiaSharp.framework/libSkiaSharp";
#elif __ANDROID__
		private const string SKIA = "libSkiaSharp.so";
#elif __MACOS__
		private const string SKIA = "libSkiaSharp.dylib";
#elif DESKTOP
		private const string SKIA = "libSkiaSharp.dll"; // redirected using .dll.config to 'libSkiaSharp.dylib' on OS X
#elif WINDOWS_UWP
		private const string SKIA = "libSkiaSharp.dll";
#else
		private const string SKIA = "libSkiaSharp";
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
		public extern static sk_surface_t sk_surface_new_backend_render_target (gr_context_t context, ref GRBackendRenderTargetDesc desc, ref SKSurfaceProps props);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_backend_render_target (gr_context_t context, ref GRBackendRenderTargetDesc desc, IntPtr propsZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_backend_texture (gr_context_t context, ref GRBackendTextureDesc desc, ref SKSurfaceProps props);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_backend_texture (gr_context_t context, ref GRBackendTextureDesc desc, IntPtr propsZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_backend_texture_as_render_target (gr_context_t context, ref GRBackendTextureDesc desc, ref SKSurfaceProps props);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_backend_texture_as_render_target (gr_context_t context, ref GRBackendTextureDesc desc, IntPtr propsZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_render_target (gr_context_t context, bool budgeted, ref SKImageInfo info, int sampleCount, ref SKSurfaceProps props);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_surface_t sk_surface_new_render_target (gr_context_t context, bool budgeted, ref SKImageInfo info, int sampleCount, IntPtr propsZero);


		// Canvas


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
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_canvas_quick_reject(sk_canvas_t t, ref SKRect rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_rect(sk_canvas_t t, ref SKRect rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_path(sk_canvas_t t, sk_path_t p);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_paint(sk_canvas_t t, sk_paint_t p);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_region(sk_canvas_t t, sk_region_t region, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_rect(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_round_rect (sk_canvas_t t, ref SKRect rect, float rx, float ry, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_oval(sk_canvas_t t, ref SKRect rect, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_circle(sk_canvas_t t, float cx, float cy, float radius, sk_paint_t paint);
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
		public extern static void sk_canvas_draw_color(sk_canvas_t t, SKColor color, SKBlendMode mode);
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
		public extern static void sk_canvas_clip_rect_with_operation(sk_canvas_t t, ref SKRect crect, SKClipOperation op, bool doAA);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_path_with_operation(sk_canvas_t t, sk_path_t cpath, SKClipOperation op, bool doAA);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_clip_region(sk_canvas_t t, sk_region_t region, SKClipOperation op);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_canvas_get_clip_device_bounds(sk_canvas_t t, ref SKRectI cbounds);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_canvas_get_clip_bounds(sk_canvas_t t, ref SKRect cbounds);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_canvas_t sk_canvas_new_from_bitmap(sk_bitmap_t bitmap);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_flush (sk_canvas_t canvas);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_bitmap_lattice(sk_canvas_t t, sk_bitmap_t bitmap, ref SKLatticeInternal lattice, ref SKRect dst, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_draw_image_lattice(sk_canvas_t t, sk_image_t image, ref SKLatticeInternal lattice, ref SKRect dst, sk_paint_t paint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_canvas_unref(sk_canvas_t canvas);

		// Paint

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_paint_t sk_paint_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_delete(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_antialias(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_antialias(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_dither(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_dither(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_verticaltext(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_verticaltext(sk_paint_t t, bool v);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static UInt32 sk_paint_get_color(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_color(sk_paint_t t, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPaintStyle sk_paint_get_style(sk_paint_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_style(sk_paint_t t, SKPaintStyle style);
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
		public extern static void sk_paint_set_blendmode(sk_paint_t t, SKBlendMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKBlendMode sk_paint_get_blendmode(sk_paint_t t);
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
		public extern static float sk_paint_get_fontmetrics(sk_paint_t t, IntPtr fontMetricsZero, float scale);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_paint_get_path_effect(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_path_effect(sk_paint_t cpaint, sk_path_effect_t effect);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_linear_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_linear_text(sk_paint_t cpaint, bool linearText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_subpixel_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_subpixel_text(sk_paint_t cpaint, bool subpixelText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_lcd_render_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_lcd_render_text(sk_paint_t cpaint, bool lcdText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_embedded_bitmap_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_embedded_bitmap_text(sk_paint_t cpaint, bool useEmbeddedBitmapText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_autohinted(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_autohinted(sk_paint_t cpaint, bool useAutohinter);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPaintHinting sk_paint_get_hinting(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_hinting(sk_paint_t cpaint, SKPaintHinting hintingLevel);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_underline_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_underline_text(sk_paint_t cpaint, bool underlineText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_strikethru_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_strikethru_text(sk_paint_t cpaint, bool strikeThruText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_fake_bold_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_fake_bold_text(sk_paint_t cpaint, bool fakeBoldText);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_is_dev_kern_text(sk_paint_t cpaint);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_paint_set_dev_kern_text(sk_paint_t cpaint, bool devKernText);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_get_fill_path(sk_paint_t paint, sk_path_t src, sk_path_t dst, ref SKRect cullRect, float resScale);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_paint_get_fill_path(sk_paint_t paint, sk_path_t src, sk_path_t dst, IntPtr cullRectZero, float resScale);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_paint_t sk_paint_clone(sk_paint_t cpaint);


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
		public extern static sk_image_t sk_image_new_from_bitmap(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_image_unref(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_image_get_width(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_image_get_height(sk_image_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static uint sk_image_get_unique_id(sk_image_t t);


		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_path_contains (sk_path_t cpath, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_path_get_last_point (sk_path_t cpath, out SKPoint point);
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
		public extern static void sk_path_rewind(sk_path_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_reset(sk_path_t t);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rect(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rect_start(sk_path_t t, ref SKRect rect, SKPathDirection direction, uint startIndex);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_oval(sk_path_t t, ref SKRect rect, SKPathDirection direction);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_arc(sk_path_t t, ref SKRect rect, float startAngle, float sweepAngle);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_path_offset (sk_path_t t, sk_path_t other, float dx, float dy, SKPath.AddMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_path_matrix (sk_path_t t, sk_path_t other, ref SKMatrix matrix, SKPath.AddMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_path (sk_path_t t, sk_path_t other, SKPath.AddMode mode);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_path_reverse (sk_path_t t, sk_path_t other);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_path_get_bounds(sk_path_t t, out SKRect rect);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPathFillType sk_path_get_filltype (sk_path_t t);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_set_filltype (sk_path_t t, SKPathFillType filltype);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_path_clone (sk_path_t t);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_t sk_path_transform (sk_path_t t, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_arc_to (sk_path_t t, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rarc_to (sk_path_t t, float rx, float ry, float xAxisRotate, SKPathArcSize largeArc, SKPathDirection sweep, float x, float y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_arc_to_with_oval (sk_path_t t, ref SKRect oval, float startAngle, float sweepAngle, bool forceMoveTo);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_arc_to_with_points (sk_path_t t, float x1, float y1, float x2, float y2, float radius);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_rounded_rect (sk_path_t t, ref SKRect rect, float rx, float ry, SKPathDirection dir);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_add_circle (sk_path_t t, float x, float y, float radius, SKPathDirection dir);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_count_verbs (sk_path_t path);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_count_points (sk_path_t path);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_get_point (sk_path_t path, int index, out SKPoint point);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_get_points (sk_path_t path, [Out] SKPoint[] points, int max);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPathConvexity sk_path_get_convexity (sk_path_t cpath);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_set_convexity (sk_path_t cpath, SKPathConvexity convexity);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_path_parse_svg_string (sk_path_t cpath, [MarshalAs(UnmanagedType.LPStr)] string str);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_to_svg_string (sk_path_t cpath, sk_string_t str);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_convert_conic_to_quads(ref SKPoint p0, ref SKPoint p1, ref SKPoint p2, float w, [Out] SKPoint[] pts, int pow2);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_pathmeasure_t sk_pathmeasure_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_pathmeasure_t sk_pathmeasure_new_with_path(sk_path_t path, bool forceClosed, float resScale);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_pathmeasure_destroy(sk_pathmeasure_t pathMeasure);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_pathmeasure_set_path(sk_pathmeasure_t pathMeasure, sk_path_t path, bool forceClosed);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_pathmeasure_get_length(sk_pathmeasure_t pathMeasure);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t pathMeasure, float distance, out SKPoint position, out SKPoint tangent);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t pathMeasure, float distance, IntPtr positionZero, out SKPoint tangent);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_get_pos_tan(sk_pathmeasure_t pathMeasure, float distance, out SKPoint position, IntPtr tangentZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_get_matrix(sk_pathmeasure_t pathMeasure, float distance, out SKMatrix matrix, SKPathMeasure.MatrixFlags flags);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_get_segment(sk_pathmeasure_t pathMeasure, float start, float stop, sk_path_t dst, bool startWithMoveTo);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_is_closed(sk_pathmeasure_t pathMeasure);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathmeasure_next_contour(sk_pathmeasure_t pathMeasure);

		// path ops
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathop_op(sk_path_t one, sk_path_t two, SKPathOp op, sk_path_t result);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathop_simplify(sk_path_t path, sk_path_t result);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_pathop_tight_bounds(sk_path_t path, out SKRect result);
		
		// path op builder
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_opbuilder_t sk_opbuilder_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_opbuilder_destroy(sk_opbuilder_t builder);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_opbuilder_add(sk_opbuilder_t builder, sk_path_t path, SKPathOp op);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_opbuilder_resolve(sk_opbuilder_t builder, sk_path_t result);

		// iterator
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_iterator_t sk_path_create_iter (sk_path_t path, int forceClose);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPath.Verb sk_path_iter_next (sk_path_iterator_t iterator, [Out] SKPoint [] points, int doConsumeDegenerates, int exact);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_path_iter_conic_weight (sk_path_iterator_t iterator);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_iter_is_close_line (sk_path_iterator_t iterator);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_path_iter_is_closed_contour (sk_path_iterator_t iterator);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_iter_destroy (sk_path_t path);

		// Raw iterator
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_iterator_t sk_path_create_rawiter (sk_path_t path);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPath.Verb sk_path_rawiter_next (sk_path_iterator_t iterator, [Out] SKPoint [] points);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKPath.Verb sk_path_rawiter_peek (sk_path_iterator_t iterator);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_path_rawiter_conic_weight (sk_path_iterator_t iterator);
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_path_rawiter_destroy (sk_path_t path);
								   
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
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_tile(ref SKRect src, ref SKRect dst, sk_imagefilter_t input);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_xfermode(SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_imagefilter_t sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground /*NULL*/, sk_imagefilter_croprect_t cropRect /*NULL*/);

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
		public extern static sk_colorfilter_t sk_colorfilter_new_gamma(float gamma);
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
		public extern static sk_canvas_t sk_picture_get_recording_canvas(sk_picture_recorder_t r);
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
		public extern static sk_shader_t sk_shader_new_linear_gradient([In] SKPoint[] points, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_linear_gradient([In] SKPoint[] points, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, IntPtr matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_radial_gradient(ref SKPoint center, float radius, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_radial_gradient(ref SKPoint center, float radius, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, IntPtr matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_sweep_gradient(ref SKPoint center, [In] SKColor[] colors, IntPtr colorPosZero, int count, IntPtr matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_sweep_gradient(ref SKPoint center, [In] SKColor[] colors, IntPtr colorPosZero, int count, ref SKMatrix matrixZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_two_point_conical_gradient(ref SKPoint start, float startRadius, ref SKPoint end, float endRadius, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_shader_t sk_shader_new_two_point_conical_gradient(ref SKPoint start, float startRadius, ref SKPoint end, float endRadius, [In] SKColor[] colors, IntPtr colorPosZero, int count, SKShaderTileMode mode, IntPtr matrixZero);

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
		public extern static sk_typeface_t sk_typeface_create_from_name([MarshalAs(UnmanagedType.LPStr)] string str, SKTypefaceStyle style);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_name_with_font_style([MarshalAs(UnmanagedType.LPStr)] string familyName, int weight, int width, SKFontStyleSlant slant);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_typeface(sk_typeface_t typeface, SKTypefaceStyle style);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_typeface_t sk_typeface_create_from_file([MarshalAs(UnmanagedType.LPStr)] string path, int index);
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
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_typeface_get_font_weight(sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_typeface_get_font_width(sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKFontStyleSlant sk_typeface_get_font_slant(sk_typeface_t typeface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKTypefaceStyle sk_typeface_get_style(sk_typeface_t typeface);

		// Streams
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_memorystream_destroy(sk_stream_memorystream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_filestream_destroy(sk_stream_filestream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_stream_asset_destroy(sk_stream_assetstream_t stream);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_read(sk_stream_t stream, IntPtr buffer, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_skip(sk_stream_t stream, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
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
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_read_bool(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_rewind(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_has_position(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_get_position(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_seek(sk_stream_t stream, IntPtr position);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_move(sk_stream_t stream, long offset);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_stream_has_length(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_stream_get_length(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_filestream_t sk_filestream_new([MarshalAs(UnmanagedType.LPStr)] string path);
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

		// Managed streams
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_managedstream_t sk_managedstream_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_managedstream_set_delegates(IntPtr pRead, IntPtr pPeek, IntPtr pIsAtEnd, IntPtr pRewind, IntPtr pGetPosition, IntPtr pSeek, IntPtr pMove, IntPtr pGetLength, IntPtr pCreateNew, IntPtr pDestroy);

		// Writeable streams
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_filewstream_destroy(sk_wstream_filestream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_dynamicmemorywstream_destroy(sk_wstream_dynamicmemorystream_t cstream);

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_wstream_filestream_t sk_filewstream_new([MarshalAs(UnmanagedType.LPStr)] string path);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_wstream_dynamicmemorystream_t sk_dynamicmemorywstream_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_data_t sk_dynamicmemorywstream_copy_to_data(sk_wstream_dynamicmemorystream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_stream_assetstream_t sk_dynamicmemorywstream_detach_as_stream(sk_wstream_dynamicmemorystream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write(sk_wstream_t cstream, IntPtr buffer, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write(sk_wstream_t cstream, byte[] buffer, IntPtr size);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_wstream_newline(sk_wstream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_wstream_flush(sk_wstream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_wstream_bytes_written(sk_wstream_t cstream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_8(sk_wstream_t cstream, Byte value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_16(sk_wstream_t cstream, UInt16 value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_32(sk_wstream_t cstream, UInt32 value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_text(sk_wstream_t cstream, [MarshalAs(UnmanagedType.LPStr)] string value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_dec_as_text(sk_wstream_t cstream, Int32 value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_bigdec_as_text(sk_wstream_t cstream, Int64 value, int minDigits);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_hex_as_text(sk_wstream_t cstream, UInt32 value, int minDigits);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_scalar_as_text(sk_wstream_t cstream, float value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_bool(sk_wstream_t cstream, bool value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_scalar(sk_wstream_t cstream, float value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_packed_uint(sk_wstream_t cstream, IntPtr value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_wstream_write_stream(sk_wstream_t cstream, sk_stream_t input, IntPtr length);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_wstream_get_size_of_packed_uint(IntPtr value);

		// Document
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_document_unref(sk_document_t document);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_document_t sk_document_create_pdf_from_stream(sk_wstream_t stream, float dpi);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_document_t sk_document_create_pdf_from_filename([MarshalAs(UnmanagedType.LPStr)] string path, float dpi);
		//[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		//public extern static sk_document_t sk_document_create_xps_from_stream(sk_wstream_t stream, float dpi);
		//[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		//public extern static sk_document_t sk_document_create_xps_from_filename([MarshalAs(UnmanagedType.LPStr)] string path, float dpi);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_canvas_t sk_document_begin_page(sk_document_t document, float width, float height, ref SKRect content);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_canvas_t sk_document_begin_page(sk_document_t document, float width, float height, IntPtr content);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_document_end_page(sk_document_t document);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_document_close(sk_document_t document);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_document_abort(sk_document_t document);

		// Codec
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static IntPtr sk_codec_min_buffered_bytes_needed();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_codec_t sk_codec_new_from_stream(sk_stream_t stream);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_codec_t sk_codec_new_from_data(sk_data_t data);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_codec_destroy(sk_codec_t codec);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_codec_get_info(sk_codec_t codec, out SKImageInfo info);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecOrigin sk_codec_get_origin(sk_codec_t codec);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_codec_get_scaled_dimensions(sk_codec_t codec, float desiredScale, out SKSizeI dimensions);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_codec_get_valid_subset(sk_codec_t codec, ref SKRectI desiredSubset);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKEncodedFormat sk_codec_get_encoded_format(sk_codec_t codec);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_get_pixels(sk_codec_t codec, ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, ref SKCodecOptionsInternal options, IntPtr ctable, ref int ctableCount);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_get_pixels_using_defaults(sk_codec_t codec, ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_start_incremental_decode(sk_codec_t codec, ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, ref SKCodecOptionsInternal options, IntPtr ctable, ref int ctableCount);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_start_incremental_decode(sk_codec_t codec, ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, ref SKCodecOptionsInternal options, IntPtr ctableZero, IntPtr ctableCountZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_start_incremental_decode(sk_codec_t codec, ref SKImageInfo info, IntPtr pixels, IntPtr rowBytes, IntPtr optionsZero, IntPtr ctableZero, IntPtr ctableCountZero);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKCodecResult sk_codec_incremental_decode(sk_codec_t codec, out int rowsDecoded);

		// Bitmap
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_bitmap_t sk_bitmap_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_destructor(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_get_info(sk_bitmap_t b, out SKImageInfo info);
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
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_is_null(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_is_immutable(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_immutable(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_is_volatile(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_volatile(sk_bitmap_t b, bool value);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_erase(sk_bitmap_t cbitmap, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_erase_rect(sk_bitmap_t cbitmap, SKColor color, ref SKRectI rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static UInt32 sk_bitmap_get_pixel_color(sk_bitmap_t cbitmap, int x, int y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static UInt32 sk_bitmap_get_index8_color(sk_bitmap_t cbitmap, int x, int y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_pixel_color(sk_bitmap_t cbitmap, int x, int y, SKColor color);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_ready_to_draw(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_copy_pixels_to(sk_bitmap_t cbitmap, IntPtr dst, IntPtr dstSize, IntPtr dstRowBytes, bool preserveDstPad);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_copy(sk_bitmap_t cbitmap, sk_bitmap_t dst, SKColorType ct);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_can_copy_to(sk_bitmap_t cbitmap, SKColorType ct);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_lock_pixels(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_unlock_pixels(sk_bitmap_t b);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_install_pixels(sk_bitmap_t cbitmap, ref SKImageInfo cinfo, IntPtr pixels, IntPtr rowBytes, sk_colortable_t ctable, IntPtr releaseProc, IntPtr context);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_try_alloc_pixels(sk_bitmap_t cbitmap, ref SKImageInfo requestedInfo, IntPtr rowBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool sk_bitmap_try_alloc_pixels_with_color_table(sk_bitmap_t cbitmap, ref SKImageInfo requestedInfo, sk_pixelref_factory_t factory, sk_colortable_t ctable);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colortable_t sk_bitmap_get_colortable(sk_bitmap_t cbitmap);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_bitmap_set_pixels(sk_bitmap_t cbitmap, IntPtr pixels, sk_colortable_t ctable);

		// Matrix

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_matrix_try_invert(ref SKMatrix matrix, out SKMatrix result);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_concat(ref SKMatrix target, ref SKMatrix first, ref SKMatrix second);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_pre_concat(ref SKMatrix target, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_post_concat(ref SKMatrix target, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_map_rect(ref SKMatrix matrix, out SKRect dest, ref SKRect source);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_map_points (ref SKMatrix matrix, IntPtr dst, IntPtr src, int count);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_map_vectors (ref SKMatrix matrix, IntPtr dst, IntPtr src, int count);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_map_xy (ref SKMatrix matrix, float x, float y, out SKPoint result);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_matrix_map_vector (ref SKMatrix matrix, float x, float y, out SKPoint result);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static float sk_matrix_map_radius (ref SKMatrix matrix, float radius);

		// Path Effect

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]  
		public extern static void sk_path_effect_unref (sk_path_effect_t effect); 
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_compose(sk_path_effect_t outer, sk_path_effect_t inner);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_sum(sk_path_effect_t first, sk_path_effect_t second);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_discrete(float segLength, float deviation, UInt32 seedAssist /*0*/);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_corner(float radius);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_1d_path(sk_path_t path, float advance, float phase, SKPath1DPathEffectStyle style);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_2d_line(float width, ref SKMatrix matrix);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_2d_path(ref SKMatrix matrix, sk_path_t path);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_path_effect_t sk_path_effect_create_dash(float[] intervals, int count, float phase);

		// Color Table

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_colortable_unref (sk_colortable_t ctable);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_colortable_t sk_colortable_new ([In] SKColor[] colors, int count);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int sk_colortable_count (sk_colortable_t ctable);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_colortable_read_colors (sk_colortable_t ctable, [Out] SKColor[] colors);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void sk_colortable_read_colors (sk_colortable_t ctable, out IntPtr colors);

		// GRContext

		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_context_t gr_context_create (GRBackend backend, GRBackendContext backendContext, ref GRContextOptions options);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_context_t gr_context_create_with_defaults (GRBackend backend, GRBackendContext backendContext);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_unref (gr_context_t context);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_abandon_context (gr_context_t context);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_release_resources_and_abandon_context (gr_context_t context);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_get_resource_cache_limits (gr_context_t context, out int maxResources, out IntPtr maxResourceBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_set_resource_cache_limits (gr_context_t context, int maxResources, IntPtr maxResourceBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_get_resource_cache_usage (gr_context_t context, out int maxResources, out IntPtr maxResourceBytes);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static int gr_context_get_recommended_sample_count (gr_context_t context, GRPixelConfig config, float dpi);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_context_flush (gr_context_t context);
		
		// GLInterface
		
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_assemble_interface (IntPtr ctx, IntPtr get);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_assemble_gl_interface (IntPtr ctx, IntPtr get);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_assemble_gles_interface (IntPtr ctx, IntPtr get);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_default_interface ();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_create_native_interface ();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static void gr_glinterface_unref (gr_glinterface_t glInterface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static gr_glinterface_t gr_glinterface_clone (gr_glinterface_t glInterface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool gr_glinterface_validate (gr_glinterface_t glInterface);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)]
		public extern static bool gr_glinterface_has_extension (gr_glinterface_t glInterface, [MarshalAs(UnmanagedType.LPStr)] string extension);
		
		// SKRegion
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_region_t sk_region_new();
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static sk_region_t sk_region_new2(sk_region_t r);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_contains(sk_region_t r, sk_region_t region);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_contains2(sk_region_t r, int x, int y);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_intersects(sk_region_t r, sk_region_t src);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_intersects(sk_region_t r, SKRectI rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_set_region(sk_region_t r, sk_region_t src);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_set_rect(sk_region_t r, ref SKRectI rect);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_set_path(sk_region_t r, sk_path_t t, sk_region_t clip);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_op(sk_region_t r, int left, int top, int right, int bottom, SKRegionOperation op);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.I1)] 
		public extern static bool sk_region_op2(sk_region_t r, sk_region_t src, SKRegionOperation op);
		[DllImport(SKIA, CallingConvention = CallingConvention.Cdecl)]
		public extern static SKRectI sk_region_get_bounds(sk_region_t r);
	}
}

