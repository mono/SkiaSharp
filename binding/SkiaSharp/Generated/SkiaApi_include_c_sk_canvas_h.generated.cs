using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_canvas.h

		// void sk_canvas_clear(sk_canvas_t* ccanvas, sk_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clear (sk_canvas_t ccanvas, UInt32 color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clear (sk_canvas_t ccanvas, UInt32 color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clear (sk_canvas_t ccanvas, UInt32 color);
		}
		private static Delegates.sk_canvas_clear sk_canvas_clear_delegate;
		internal static void sk_canvas_clear (sk_canvas_t ccanvas, UInt32 color) =>
			(sk_canvas_clear_delegate ??= GetSymbol<Delegates.sk_canvas_clear> ("sk_canvas_clear")).Invoke (ccanvas, color);
		#endif

		// void sk_canvas_clear_color4f(sk_canvas_t* ccanvas, sk_color4f_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clear_color4f (sk_canvas_t ccanvas, SKColorF color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clear_color4f (sk_canvas_t ccanvas, SKColorF color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clear_color4f (sk_canvas_t ccanvas, SKColorF color);
		}
		private static Delegates.sk_canvas_clear_color4f sk_canvas_clear_color4f_delegate;
		internal static void sk_canvas_clear_color4f (sk_canvas_t ccanvas, SKColorF color) =>
			(sk_canvas_clear_color4f_delegate ??= GetSymbol<Delegates.sk_canvas_clear_color4f> ("sk_canvas_clear_color4f")).Invoke (ccanvas, color);
		#endif

		// void sk_canvas_clip_path_with_operation(sk_canvas_t* ccanvas, const sk_path_t* cpath, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clip_path_with_operation (sk_canvas_t ccanvas, sk_path_t cpath, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_path_with_operation (sk_canvas_t ccanvas, sk_path_t cpath, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_path_with_operation (sk_canvas_t ccanvas, sk_path_t cpath, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_path_with_operation sk_canvas_clip_path_with_operation_delegate;
		internal static void sk_canvas_clip_path_with_operation (sk_canvas_t ccanvas, sk_path_t cpath, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_path_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_path_with_operation> ("sk_canvas_clip_path_with_operation")).Invoke (ccanvas, cpath, op, doAA);
		#endif

		// void sk_canvas_clip_rect_with_operation(sk_canvas_t* ccanvas, const sk_rect_t* crect, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clip_rect_with_operation (sk_canvas_t ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rect_with_operation (sk_canvas_t ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_rect_with_operation (sk_canvas_t ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_rect_with_operation sk_canvas_clip_rect_with_operation_delegate;
		internal static void sk_canvas_clip_rect_with_operation (sk_canvas_t ccanvas, SKRect* crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_rect_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_rect_with_operation> ("sk_canvas_clip_rect_with_operation")).Invoke (ccanvas, crect, op, doAA);
		#endif

		// void sk_canvas_clip_region(sk_canvas_t* ccanvas, const sk_region_t* region, sk_clipop_t op)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clip_region (sk_canvas_t ccanvas, sk_region_t region, SKClipOperation op);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_region (sk_canvas_t ccanvas, sk_region_t region, SKClipOperation op);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_region (sk_canvas_t ccanvas, sk_region_t region, SKClipOperation op);
		}
		private static Delegates.sk_canvas_clip_region sk_canvas_clip_region_delegate;
		internal static void sk_canvas_clip_region (sk_canvas_t ccanvas, sk_region_t region, SKClipOperation op) =>
			(sk_canvas_clip_region_delegate ??= GetSymbol<Delegates.sk_canvas_clip_region> ("sk_canvas_clip_region")).Invoke (ccanvas, region, op);
		#endif

		// void sk_canvas_clip_rrect_with_operation(sk_canvas_t* ccanvas, const sk_rrect_t* crect, sk_clipop_t op, bool doAA)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_clip_rrect_with_operation (sk_canvas_t ccanvas, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_clip_rrect_with_operation (sk_canvas_t ccanvas, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_clip_rrect_with_operation (sk_canvas_t ccanvas, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA);
		}
		private static Delegates.sk_canvas_clip_rrect_with_operation sk_canvas_clip_rrect_with_operation_delegate;
		internal static void sk_canvas_clip_rrect_with_operation (sk_canvas_t ccanvas, sk_rrect_t crect, SKClipOperation op, [MarshalAs (UnmanagedType.I1)] bool doAA) =>
			(sk_canvas_clip_rrect_with_operation_delegate ??= GetSymbol<Delegates.sk_canvas_clip_rrect_with_operation> ("sk_canvas_clip_rrect_with_operation")).Invoke (ccanvas, crect, op, doAA);
		#endif

		// void sk_canvas_concat(sk_canvas_t* ccanvas, const sk_matrix44_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_concat (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_concat (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_concat (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		}
		private static Delegates.sk_canvas_concat sk_canvas_concat_delegate;
		internal static void sk_canvas_concat (sk_canvas_t ccanvas, SKMatrix44* cmatrix) =>
			(sk_canvas_concat_delegate ??= GetSymbol<Delegates.sk_canvas_concat> ("sk_canvas_concat")).Invoke (ccanvas, cmatrix);
		#endif

		// void sk_canvas_destroy(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_destroy (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_destroy (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_destroy (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_destroy sk_canvas_destroy_delegate;
		internal static void sk_canvas_destroy (sk_canvas_t ccanvas) =>
			(sk_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_canvas_destroy> ("sk_canvas_destroy")).Invoke (ccanvas);
		#endif

		// void sk_canvas_discard(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_discard (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_discard (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_discard (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_discard sk_canvas_discard_delegate;
		internal static void sk_canvas_discard (sk_canvas_t ccanvas) =>
			(sk_canvas_discard_delegate ??= GetSymbol<Delegates.sk_canvas_discard> ("sk_canvas_discard")).Invoke (ccanvas);
		#endif

		// void sk_canvas_draw_annotation(sk_canvas_t* t, const sk_rect_t* rect, const char* key, sk_data_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_annotation sk_canvas_draw_annotation_delegate;
		internal static void sk_canvas_draw_annotation (sk_canvas_t t, SKRect* rect, /* char */ void* key, sk_data_t value) =>
			(sk_canvas_draw_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_annotation> ("sk_canvas_draw_annotation")).Invoke (t, rect, key, value);
		#endif

		// void sk_canvas_draw_arc(sk_canvas_t* ccanvas, const sk_rect_t* oval, float startAngle, float sweepAngle, bool useCenter, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_arc sk_canvas_draw_arc_delegate;
		internal static void sk_canvas_draw_arc (sk_canvas_t ccanvas, SKRect* oval, Single startAngle, Single sweepAngle, [MarshalAs (UnmanagedType.I1)] bool useCenter, sk_paint_t paint) =>
			(sk_canvas_draw_arc_delegate ??= GetSymbol<Delegates.sk_canvas_draw_arc> ("sk_canvas_draw_arc")).Invoke (ccanvas, oval, startAngle, sweepAngle, useCenter, paint);
		#endif

		// void sk_canvas_draw_atlas(sk_canvas_t* ccanvas, const sk_image_t* atlas, const sk_rsxform_t* xform, const sk_rect_t* tex, const sk_color_t* colors, int count, sk_blendmode_t mode, const sk_sampling_options_t* sampling, const sk_rect_t* cullRect, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_atlas sk_canvas_draw_atlas_delegate;
		internal static void sk_canvas_draw_atlas (sk_canvas_t ccanvas, sk_image_t atlas, SKRotationScaleMatrix* xform, SKRect* tex, UInt32* colors, Int32 count, SKBlendMode mode, SKSamplingOptions* sampling, SKRect* cullRect, sk_paint_t paint) =>
			(sk_canvas_draw_atlas_delegate ??= GetSymbol<Delegates.sk_canvas_draw_atlas> ("sk_canvas_draw_atlas")).Invoke (ccanvas, atlas, xform, tex, colors, count, mode, sampling, cullRect, paint);
		#endif

		// void sk_canvas_draw_circle(sk_canvas_t* ccanvas, float cx, float cy, float rad, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_circle (sk_canvas_t ccanvas, Single cx, Single cy, Single rad, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_circle (sk_canvas_t ccanvas, Single cx, Single cy, Single rad, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_circle (sk_canvas_t ccanvas, Single cx, Single cy, Single rad, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_circle sk_canvas_draw_circle_delegate;
		internal static void sk_canvas_draw_circle (sk_canvas_t ccanvas, Single cx, Single cy, Single rad, sk_paint_t cpaint) =>
			(sk_canvas_draw_circle_delegate ??= GetSymbol<Delegates.sk_canvas_draw_circle> ("sk_canvas_draw_circle")).Invoke (ccanvas, cx, cy, rad, cpaint);
		#endif

		// void sk_canvas_draw_color(sk_canvas_t* ccanvas, sk_color_t color, sk_blendmode_t cmode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode cmode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode cmode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode cmode);
		}
		private static Delegates.sk_canvas_draw_color sk_canvas_draw_color_delegate;
		internal static void sk_canvas_draw_color (sk_canvas_t ccanvas, UInt32 color, SKBlendMode cmode) =>
			(sk_canvas_draw_color_delegate ??= GetSymbol<Delegates.sk_canvas_draw_color> ("sk_canvas_draw_color")).Invoke (ccanvas, color, cmode);
		#endif

		// void sk_canvas_draw_color4f(sk_canvas_t* ccanvas, sk_color4f_t color, sk_blendmode_t cmode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode cmode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode cmode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode cmode);
		}
		private static Delegates.sk_canvas_draw_color4f sk_canvas_draw_color4f_delegate;
		internal static void sk_canvas_draw_color4f (sk_canvas_t ccanvas, SKColorF color, SKBlendMode cmode) =>
			(sk_canvas_draw_color4f_delegate ??= GetSymbol<Delegates.sk_canvas_draw_color4f> ("sk_canvas_draw_color4f")).Invoke (ccanvas, color, cmode);
		#endif

		// void sk_canvas_draw_drawable(sk_canvas_t* ccanvas, sk_drawable_t* cdrawable, const sk_matrix_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_drawable (sk_canvas_t ccanvas, sk_drawable_t cdrawable, SKMatrix* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drawable (sk_canvas_t ccanvas, sk_drawable_t cdrawable, SKMatrix* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_drawable (sk_canvas_t ccanvas, sk_drawable_t cdrawable, SKMatrix* cmatrix);
		}
		private static Delegates.sk_canvas_draw_drawable sk_canvas_draw_drawable_delegate;
		internal static void sk_canvas_draw_drawable (sk_canvas_t ccanvas, sk_drawable_t cdrawable, SKMatrix* cmatrix) =>
			(sk_canvas_draw_drawable_delegate ??= GetSymbol<Delegates.sk_canvas_draw_drawable> ("sk_canvas_draw_drawable")).Invoke (ccanvas, cdrawable, cmatrix);
		#endif

		// void sk_canvas_draw_drrect(sk_canvas_t* ccanvas, const sk_rrect_t* outer, const sk_rrect_t* inner, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_drrect sk_canvas_draw_drrect_delegate;
		internal static void sk_canvas_draw_drrect (sk_canvas_t ccanvas, sk_rrect_t outer, sk_rrect_t inner, sk_paint_t paint) =>
			(sk_canvas_draw_drrect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_drrect> ("sk_canvas_draw_drrect")).Invoke (ccanvas, outer, inner, paint);
		#endif

		// void sk_canvas_draw_image(sk_canvas_t* ccanvas, const sk_image_t* cimage, float x, float y, const sk_sampling_options_t* sampling, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_image (sk_canvas_t ccanvas, sk_image_t cimage, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image (sk_canvas_t ccanvas, sk_image_t cimage, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image (sk_canvas_t ccanvas, sk_image_t cimage, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_image sk_canvas_draw_image_delegate;
		internal static void sk_canvas_draw_image (sk_canvas_t ccanvas, sk_image_t cimage, Single x, Single y, SKSamplingOptions* sampling, sk_paint_t cpaint) =>
			(sk_canvas_draw_image_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image> ("sk_canvas_draw_image")).Invoke (ccanvas, cimage, x, y, sampling, cpaint);
		#endif

		// void sk_canvas_draw_image_lattice(sk_canvas_t* ccanvas, const sk_image_t* image, const sk_lattice_t* lattice, const sk_rect_t* dst, sk_filter_mode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_image_lattice (sk_canvas_t ccanvas, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_lattice (sk_canvas_t ccanvas, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_lattice (sk_canvas_t ccanvas, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_image_lattice sk_canvas_draw_image_lattice_delegate;
		internal static void sk_canvas_draw_image_lattice (sk_canvas_t ccanvas, sk_image_t image, SKLatticeInternal* lattice, SKRect* dst, SKFilterMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_image_lattice_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_lattice> ("sk_canvas_draw_image_lattice")).Invoke (ccanvas, image, lattice, dst, mode, paint);
		#endif

		// void sk_canvas_draw_image_nine(sk_canvas_t* ccanvas, const sk_image_t* image, const sk_irect_t* center, const sk_rect_t* dst, sk_filter_mode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_image_nine (sk_canvas_t ccanvas, sk_image_t image, SKRectI* center, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_nine (sk_canvas_t ccanvas, sk_image_t image, SKRectI* center, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_nine (sk_canvas_t ccanvas, sk_image_t image, SKRectI* center, SKRect* dst, SKFilterMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_image_nine sk_canvas_draw_image_nine_delegate;
		internal static void sk_canvas_draw_image_nine (sk_canvas_t ccanvas, sk_image_t image, SKRectI* center, SKRect* dst, SKFilterMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_image_nine_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_nine> ("sk_canvas_draw_image_nine")).Invoke (ccanvas, image, center, dst, mode, paint);
		#endif

		// void sk_canvas_draw_image_rect(sk_canvas_t* ccanvas, const sk_image_t* cimage, const sk_rect_t* csrcR, const sk_rect_t* cdstR, const sk_sampling_options_t* sampling, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_image_rect (sk_canvas_t ccanvas, sk_image_t cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_image_rect (sk_canvas_t ccanvas, sk_image_t cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_image_rect (sk_canvas_t ccanvas, sk_image_t cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_image_rect sk_canvas_draw_image_rect_delegate;
		internal static void sk_canvas_draw_image_rect (sk_canvas_t ccanvas, sk_image_t cimage, SKRect* csrcR, SKRect* cdstR, SKSamplingOptions* sampling, sk_paint_t cpaint) =>
			(sk_canvas_draw_image_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_image_rect> ("sk_canvas_draw_image_rect")).Invoke (ccanvas, cimage, csrcR, cdstR, sampling, cpaint);
		#endif

		// void sk_canvas_draw_line(sk_canvas_t* ccanvas, float x0, float y0, float x1, float y1, sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_line sk_canvas_draw_line_delegate;
		internal static void sk_canvas_draw_line (sk_canvas_t ccanvas, Single x0, Single y0, Single x1, Single y1, sk_paint_t cpaint) =>
			(sk_canvas_draw_line_delegate ??= GetSymbol<Delegates.sk_canvas_draw_line> ("sk_canvas_draw_line")).Invoke (ccanvas, x0, y0, x1, y1, cpaint);
		#endif

		// void sk_canvas_draw_link_destination_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_link_destination_annotation sk_canvas_draw_link_destination_annotation_delegate;
		internal static void sk_canvas_draw_link_destination_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value) =>
			(sk_canvas_draw_link_destination_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_link_destination_annotation> ("sk_canvas_draw_link_destination_annotation")).Invoke (t, rect, value);
		#endif

		// void sk_canvas_draw_named_destination_annotation(sk_canvas_t* t, const sk_point_t* point, sk_data_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_named_destination_annotation sk_canvas_draw_named_destination_annotation_delegate;
		internal static void sk_canvas_draw_named_destination_annotation (sk_canvas_t t, SKPoint* point, sk_data_t value) =>
			(sk_canvas_draw_named_destination_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_named_destination_annotation> ("sk_canvas_draw_named_destination_annotation")).Invoke (t, point, value);
		#endif

		// void sk_canvas_draw_oval(sk_canvas_t* ccanvas, const sk_rect_t* crect, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_oval (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_oval (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_oval (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_oval sk_canvas_draw_oval_delegate;
		internal static void sk_canvas_draw_oval (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint) =>
			(sk_canvas_draw_oval_delegate ??= GetSymbol<Delegates.sk_canvas_draw_oval> ("sk_canvas_draw_oval")).Invoke (ccanvas, crect, cpaint);
		#endif

		// void sk_canvas_draw_paint(sk_canvas_t* ccanvas, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_paint (sk_canvas_t ccanvas, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_paint (sk_canvas_t ccanvas, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_paint (sk_canvas_t ccanvas, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_paint sk_canvas_draw_paint_delegate;
		internal static void sk_canvas_draw_paint (sk_canvas_t ccanvas, sk_paint_t cpaint) =>
			(sk_canvas_draw_paint_delegate ??= GetSymbol<Delegates.sk_canvas_draw_paint> ("sk_canvas_draw_paint")).Invoke (ccanvas, cpaint);
		#endif

		// void sk_canvas_draw_patch(sk_canvas_t* ccanvas, const sk_point_t* cubics, const sk_color_t* colors, const sk_point_t* texCoords, sk_blendmode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_patch sk_canvas_draw_patch_delegate;
		internal static void sk_canvas_draw_patch (sk_canvas_t ccanvas, SKPoint* cubics, UInt32* colors, SKPoint* texCoords, SKBlendMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_patch_delegate ??= GetSymbol<Delegates.sk_canvas_draw_patch> ("sk_canvas_draw_patch")).Invoke (ccanvas, cubics, colors, texCoords, mode, paint);
		#endif

		// void sk_canvas_draw_path(sk_canvas_t* ccanvas, const sk_path_t* cpath, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_path (sk_canvas_t ccanvas, sk_path_t cpath, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_path (sk_canvas_t ccanvas, sk_path_t cpath, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_path (sk_canvas_t ccanvas, sk_path_t cpath, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_path sk_canvas_draw_path_delegate;
		internal static void sk_canvas_draw_path (sk_canvas_t ccanvas, sk_path_t cpath, sk_paint_t cpaint) =>
			(sk_canvas_draw_path_delegate ??= GetSymbol<Delegates.sk_canvas_draw_path> ("sk_canvas_draw_path")).Invoke (ccanvas, cpath, cpaint);
		#endif

		// void sk_canvas_draw_picture(sk_canvas_t* ccanvas, const sk_picture_t* cpicture, const sk_matrix_t* cmatrix, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_picture (sk_canvas_t ccanvas, sk_picture_t cpicture, SKMatrix* cmatrix, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_picture (sk_canvas_t ccanvas, sk_picture_t cpicture, SKMatrix* cmatrix, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_picture (sk_canvas_t ccanvas, sk_picture_t cpicture, SKMatrix* cmatrix, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_picture sk_canvas_draw_picture_delegate;
		internal static void sk_canvas_draw_picture (sk_canvas_t ccanvas, sk_picture_t cpicture, SKMatrix* cmatrix, sk_paint_t cpaint) =>
			(sk_canvas_draw_picture_delegate ??= GetSymbol<Delegates.sk_canvas_draw_picture> ("sk_canvas_draw_picture")).Invoke (ccanvas, cpicture, cmatrix, cpaint);
		#endif

		// void sk_canvas_draw_point(sk_canvas_t* ccanvas, float x, float y, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_point (sk_canvas_t ccanvas, Single x, Single y, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_point (sk_canvas_t ccanvas, Single x, Single y, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_point (sk_canvas_t ccanvas, Single x, Single y, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_point sk_canvas_draw_point_delegate;
		internal static void sk_canvas_draw_point (sk_canvas_t ccanvas, Single x, Single y, sk_paint_t cpaint) =>
			(sk_canvas_draw_point_delegate ??= GetSymbol<Delegates.sk_canvas_draw_point> ("sk_canvas_draw_point")).Invoke (ccanvas, x, y, cpaint);
		#endif

		// void sk_canvas_draw_points(sk_canvas_t* ccanvas, sk_point_mode_t pointMode, size_t count, const sk_point_t[-1] points, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_points (sk_canvas_t ccanvas, SKPointMode pointMode, /* size_t */ IntPtr count, SKPoint* points, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_points (sk_canvas_t ccanvas, SKPointMode pointMode, /* size_t */ IntPtr count, SKPoint* points, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_points (sk_canvas_t ccanvas, SKPointMode pointMode, /* size_t */ IntPtr count, SKPoint* points, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_points sk_canvas_draw_points_delegate;
		internal static void sk_canvas_draw_points (sk_canvas_t ccanvas, SKPointMode pointMode, /* size_t */ IntPtr count, SKPoint* points, sk_paint_t cpaint) =>
			(sk_canvas_draw_points_delegate ??= GetSymbol<Delegates.sk_canvas_draw_points> ("sk_canvas_draw_points")).Invoke (ccanvas, pointMode, count, points, cpaint);
		#endif

		// void sk_canvas_draw_rect(sk_canvas_t* ccanvas, const sk_rect_t* crect, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_rect (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rect (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_rect (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_rect sk_canvas_draw_rect_delegate;
		internal static void sk_canvas_draw_rect (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint) =>
			(sk_canvas_draw_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_rect> ("sk_canvas_draw_rect")).Invoke (ccanvas, crect, cpaint);
		#endif

		// void sk_canvas_draw_region(sk_canvas_t* ccanvas, const sk_region_t* cregion, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_region (sk_canvas_t ccanvas, sk_region_t cregion, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_region (sk_canvas_t ccanvas, sk_region_t cregion, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_region (sk_canvas_t ccanvas, sk_region_t cregion, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_region sk_canvas_draw_region_delegate;
		internal static void sk_canvas_draw_region (sk_canvas_t ccanvas, sk_region_t cregion, sk_paint_t cpaint) =>
			(sk_canvas_draw_region_delegate ??= GetSymbol<Delegates.sk_canvas_draw_region> ("sk_canvas_draw_region")).Invoke (ccanvas, cregion, cpaint);
		#endif

		// void sk_canvas_draw_round_rect(sk_canvas_t* ccanvas, const sk_rect_t* crect, float rx, float ry, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_round_rect (sk_canvas_t ccanvas, SKRect* crect, Single rx, Single ry, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_round_rect (sk_canvas_t ccanvas, SKRect* crect, Single rx, Single ry, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_round_rect (sk_canvas_t ccanvas, SKRect* crect, Single rx, Single ry, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_round_rect sk_canvas_draw_round_rect_delegate;
		internal static void sk_canvas_draw_round_rect (sk_canvas_t ccanvas, SKRect* crect, Single rx, Single ry, sk_paint_t cpaint) =>
			(sk_canvas_draw_round_rect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_round_rect> ("sk_canvas_draw_round_rect")).Invoke (ccanvas, crect, rx, ry, cpaint);
		#endif

		// void sk_canvas_draw_rrect(sk_canvas_t* ccanvas, const sk_rrect_t* crect, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_rrect (sk_canvas_t ccanvas, sk_rrect_t crect, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_rrect (sk_canvas_t ccanvas, sk_rrect_t crect, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_rrect (sk_canvas_t ccanvas, sk_rrect_t crect, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_rrect sk_canvas_draw_rrect_delegate;
		internal static void sk_canvas_draw_rrect (sk_canvas_t ccanvas, sk_rrect_t crect, sk_paint_t cpaint) =>
			(sk_canvas_draw_rrect_delegate ??= GetSymbol<Delegates.sk_canvas_draw_rrect> ("sk_canvas_draw_rrect")).Invoke (ccanvas, crect, cpaint);
		#endif

		// void sk_canvas_draw_simple_text(sk_canvas_t* ccanvas, const void* text, size_t byte_length, sk_text_encoding_t encoding, float x, float y, const sk_font_t* cfont, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_simple_text sk_canvas_draw_simple_text_delegate;
		internal static void sk_canvas_draw_simple_text (sk_canvas_t ccanvas, void* text, /* size_t */ IntPtr byte_length, SKTextEncoding encoding, Single x, Single y, sk_font_t cfont, sk_paint_t cpaint) =>
			(sk_canvas_draw_simple_text_delegate ??= GetSymbol<Delegates.sk_canvas_draw_simple_text> ("sk_canvas_draw_simple_text")).Invoke (ccanvas, text, byte_length, encoding, x, y, cfont, cpaint);
		#endif

		// void sk_canvas_draw_text_blob(sk_canvas_t* ccanvas, sk_textblob_t* text, float x, float y, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_text_blob (sk_canvas_t ccanvas, sk_textblob_t text, Single x, Single y, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_text_blob (sk_canvas_t ccanvas, sk_textblob_t text, Single x, Single y, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_text_blob (sk_canvas_t ccanvas, sk_textblob_t text, Single x, Single y, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_draw_text_blob sk_canvas_draw_text_blob_delegate;
		internal static void sk_canvas_draw_text_blob (sk_canvas_t ccanvas, sk_textblob_t text, Single x, Single y, sk_paint_t cpaint) =>
			(sk_canvas_draw_text_blob_delegate ??= GetSymbol<Delegates.sk_canvas_draw_text_blob> ("sk_canvas_draw_text_blob")).Invoke (ccanvas, text, x, y, cpaint);
		#endif

		// void sk_canvas_draw_url_annotation(sk_canvas_t* t, const sk_rect_t* rect, sk_data_t* value)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value);
		}
		private static Delegates.sk_canvas_draw_url_annotation sk_canvas_draw_url_annotation_delegate;
		internal static void sk_canvas_draw_url_annotation (sk_canvas_t t, SKRect* rect, sk_data_t value) =>
			(sk_canvas_draw_url_annotation_delegate ??= GetSymbol<Delegates.sk_canvas_draw_url_annotation> ("sk_canvas_draw_url_annotation")).Invoke (t, rect, value);
		#endif

		// void sk_canvas_draw_vertices(sk_canvas_t* ccanvas, const sk_vertices_t* vertices, sk_blendmode_t mode, const sk_paint_t* paint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint);
		}
		private static Delegates.sk_canvas_draw_vertices sk_canvas_draw_vertices_delegate;
		internal static void sk_canvas_draw_vertices (sk_canvas_t ccanvas, sk_vertices_t vertices, SKBlendMode mode, sk_paint_t paint) =>
			(sk_canvas_draw_vertices_delegate ??= GetSymbol<Delegates.sk_canvas_draw_vertices> ("sk_canvas_draw_vertices")).Invoke (ccanvas, vertices, mode, paint);
		#endif

		// bool sk_canvas_get_device_clip_bounds(sk_canvas_t* ccanvas, sk_irect_t* cbounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_canvas_get_device_clip_bounds (sk_canvas_t ccanvas, SKRectI* cbounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_device_clip_bounds (sk_canvas_t ccanvas, SKRectI* cbounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_get_device_clip_bounds (sk_canvas_t ccanvas, SKRectI* cbounds);
		}
		private static Delegates.sk_canvas_get_device_clip_bounds sk_canvas_get_device_clip_bounds_delegate;
		internal static bool sk_canvas_get_device_clip_bounds (sk_canvas_t ccanvas, SKRectI* cbounds) =>
			(sk_canvas_get_device_clip_bounds_delegate ??= GetSymbol<Delegates.sk_canvas_get_device_clip_bounds> ("sk_canvas_get_device_clip_bounds")).Invoke (ccanvas, cbounds);
		#endif

		// bool sk_canvas_get_local_clip_bounds(sk_canvas_t* ccanvas, sk_rect_t* cbounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_canvas_get_local_clip_bounds (sk_canvas_t ccanvas, SKRect* cbounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_get_local_clip_bounds (sk_canvas_t ccanvas, SKRect* cbounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_get_local_clip_bounds (sk_canvas_t ccanvas, SKRect* cbounds);
		}
		private static Delegates.sk_canvas_get_local_clip_bounds sk_canvas_get_local_clip_bounds_delegate;
		internal static bool sk_canvas_get_local_clip_bounds (sk_canvas_t ccanvas, SKRect* cbounds) =>
			(sk_canvas_get_local_clip_bounds_delegate ??= GetSymbol<Delegates.sk_canvas_get_local_clip_bounds> ("sk_canvas_get_local_clip_bounds")).Invoke (ccanvas, cbounds);
		#endif

		// void sk_canvas_get_matrix(sk_canvas_t* ccanvas, sk_matrix44_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_get_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_get_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_get_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		}
		private static Delegates.sk_canvas_get_matrix sk_canvas_get_matrix_delegate;
		internal static void sk_canvas_get_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix) =>
			(sk_canvas_get_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_get_matrix> ("sk_canvas_get_matrix")).Invoke (ccanvas, cmatrix);
		#endif

		// int sk_canvas_get_save_count(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_canvas_get_save_count (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_get_save_count (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_get_save_count (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_get_save_count sk_canvas_get_save_count_delegate;
		internal static Int32 sk_canvas_get_save_count (sk_canvas_t ccanvas) =>
			(sk_canvas_get_save_count_delegate ??= GetSymbol<Delegates.sk_canvas_get_save_count> ("sk_canvas_get_save_count")).Invoke (ccanvas);
		#endif

		// bool sk_canvas_is_clip_empty(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_is_clip_empty sk_canvas_is_clip_empty_delegate;
		internal static bool sk_canvas_is_clip_empty (sk_canvas_t ccanvas) =>
			(sk_canvas_is_clip_empty_delegate ??= GetSymbol<Delegates.sk_canvas_is_clip_empty> ("sk_canvas_is_clip_empty")).Invoke (ccanvas);
		#endif

		// bool sk_canvas_is_clip_rect(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_is_clip_rect sk_canvas_is_clip_rect_delegate;
		internal static bool sk_canvas_is_clip_rect (sk_canvas_t ccanvas) =>
			(sk_canvas_is_clip_rect_delegate ??= GetSymbol<Delegates.sk_canvas_is_clip_rect> ("sk_canvas_is_clip_rect")).Invoke (ccanvas);
		#endif

		// sk_canvas_t* sk_canvas_new_from_bitmap(const sk_bitmap_t* bitmap)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap);
		}
		private static Delegates.sk_canvas_new_from_bitmap sk_canvas_new_from_bitmap_delegate;
		internal static sk_canvas_t sk_canvas_new_from_bitmap (sk_bitmap_t bitmap) =>
			(sk_canvas_new_from_bitmap_delegate ??= GetSymbol<Delegates.sk_canvas_new_from_bitmap> ("sk_canvas_new_from_bitmap")).Invoke (bitmap);
		#endif

		// sk_canvas_t* sk_canvas_new_from_raster(const sk_imageinfo_t* cinfo, void* pixels, size_t rowBytes, const sk_surfaceprops_t* props)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_canvas_t sk_canvas_new_from_raster (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t props);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_canvas_t sk_canvas_new_from_raster (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t props);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_canvas_t sk_canvas_new_from_raster (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t props);
		}
		private static Delegates.sk_canvas_new_from_raster sk_canvas_new_from_raster_delegate;
		internal static sk_canvas_t sk_canvas_new_from_raster (SKImageInfoNative* cinfo, void* pixels, /* size_t */ IntPtr rowBytes, sk_surfaceprops_t props) =>
			(sk_canvas_new_from_raster_delegate ??= GetSymbol<Delegates.sk_canvas_new_from_raster> ("sk_canvas_new_from_raster")).Invoke (cinfo, pixels, rowBytes, props);
		#endif

		// bool sk_canvas_quick_reject(sk_canvas_t* ccanvas, const sk_rect_t* crect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_canvas_quick_reject (sk_canvas_t ccanvas, SKRect* crect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_canvas_quick_reject (sk_canvas_t ccanvas, SKRect* crect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_canvas_quick_reject (sk_canvas_t ccanvas, SKRect* crect);
		}
		private static Delegates.sk_canvas_quick_reject sk_canvas_quick_reject_delegate;
		internal static bool sk_canvas_quick_reject (sk_canvas_t ccanvas, SKRect* crect) =>
			(sk_canvas_quick_reject_delegate ??= GetSymbol<Delegates.sk_canvas_quick_reject> ("sk_canvas_quick_reject")).Invoke (ccanvas, crect);
		#endif

		// void sk_canvas_reset_matrix(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_reset_matrix (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_reset_matrix (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_reset_matrix (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_reset_matrix sk_canvas_reset_matrix_delegate;
		internal static void sk_canvas_reset_matrix (sk_canvas_t ccanvas) =>
			(sk_canvas_reset_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_reset_matrix> ("sk_canvas_reset_matrix")).Invoke (ccanvas);
		#endif

		// void sk_canvas_restore(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_restore (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_restore (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_restore sk_canvas_restore_delegate;
		internal static void sk_canvas_restore (sk_canvas_t ccanvas) =>
			(sk_canvas_restore_delegate ??= GetSymbol<Delegates.sk_canvas_restore> ("sk_canvas_restore")).Invoke (ccanvas);
		#endif

		// void sk_canvas_restore_to_count(sk_canvas_t* ccanvas, int saveCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_restore_to_count (sk_canvas_t ccanvas, Int32 saveCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_restore_to_count (sk_canvas_t ccanvas, Int32 saveCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_restore_to_count (sk_canvas_t ccanvas, Int32 saveCount);
		}
		private static Delegates.sk_canvas_restore_to_count sk_canvas_restore_to_count_delegate;
		internal static void sk_canvas_restore_to_count (sk_canvas_t ccanvas, Int32 saveCount) =>
			(sk_canvas_restore_to_count_delegate ??= GetSymbol<Delegates.sk_canvas_restore_to_count> ("sk_canvas_restore_to_count")).Invoke (ccanvas, saveCount);
		#endif

		// void sk_canvas_rotate_degrees(sk_canvas_t* ccanvas, float degrees)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_rotate_degrees (sk_canvas_t ccanvas, Single degrees);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_degrees (sk_canvas_t ccanvas, Single degrees);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_rotate_degrees (sk_canvas_t ccanvas, Single degrees);
		}
		private static Delegates.sk_canvas_rotate_degrees sk_canvas_rotate_degrees_delegate;
		internal static void sk_canvas_rotate_degrees (sk_canvas_t ccanvas, Single degrees) =>
			(sk_canvas_rotate_degrees_delegate ??= GetSymbol<Delegates.sk_canvas_rotate_degrees> ("sk_canvas_rotate_degrees")).Invoke (ccanvas, degrees);
		#endif

		// void sk_canvas_rotate_radians(sk_canvas_t* ccanvas, float radians)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_rotate_radians (sk_canvas_t ccanvas, Single radians);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_rotate_radians (sk_canvas_t ccanvas, Single radians);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_rotate_radians (sk_canvas_t ccanvas, Single radians);
		}
		private static Delegates.sk_canvas_rotate_radians sk_canvas_rotate_radians_delegate;
		internal static void sk_canvas_rotate_radians (sk_canvas_t ccanvas, Single radians) =>
			(sk_canvas_rotate_radians_delegate ??= GetSymbol<Delegates.sk_canvas_rotate_radians> ("sk_canvas_rotate_radians")).Invoke (ccanvas, radians);
		#endif

		// int sk_canvas_save(sk_canvas_t* ccanvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_canvas_save (sk_canvas_t ccanvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save (sk_canvas_t ccanvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_save (sk_canvas_t ccanvas);
		}
		private static Delegates.sk_canvas_save sk_canvas_save_delegate;
		internal static Int32 sk_canvas_save (sk_canvas_t ccanvas) =>
			(sk_canvas_save_delegate ??= GetSymbol<Delegates.sk_canvas_save> ("sk_canvas_save")).Invoke (ccanvas);
		#endif

		// int sk_canvas_save_layer(sk_canvas_t* ccanvas, const sk_rect_t* crect, const sk_paint_t* cpaint)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_canvas_save_layer (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save_layer (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_save_layer (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint);
		}
		private static Delegates.sk_canvas_save_layer sk_canvas_save_layer_delegate;
		internal static Int32 sk_canvas_save_layer (sk_canvas_t ccanvas, SKRect* crect, sk_paint_t cpaint) =>
			(sk_canvas_save_layer_delegate ??= GetSymbol<Delegates.sk_canvas_save_layer> ("sk_canvas_save_layer")).Invoke (ccanvas, crect, cpaint);
		#endif

		// int sk_canvas_save_layer_rec(sk_canvas_t* ccanvas, const sk_canvas_savelayerrec_t* crec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial Int32 sk_canvas_save_layer_rec (sk_canvas_t ccanvas, SKCanvasSaveLayerRecNative* crec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern Int32 sk_canvas_save_layer_rec (sk_canvas_t ccanvas, SKCanvasSaveLayerRecNative* crec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate Int32 sk_canvas_save_layer_rec (sk_canvas_t ccanvas, SKCanvasSaveLayerRecNative* crec);
		}
		private static Delegates.sk_canvas_save_layer_rec sk_canvas_save_layer_rec_delegate;
		internal static Int32 sk_canvas_save_layer_rec (sk_canvas_t ccanvas, SKCanvasSaveLayerRecNative* crec) =>
			(sk_canvas_save_layer_rec_delegate ??= GetSymbol<Delegates.sk_canvas_save_layer_rec> ("sk_canvas_save_layer_rec")).Invoke (ccanvas, crec);
		#endif

		// void sk_canvas_scale(sk_canvas_t* ccanvas, float sx, float sy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_scale (sk_canvas_t ccanvas, Single sx, Single sy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_scale (sk_canvas_t ccanvas, Single sx, Single sy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_scale (sk_canvas_t ccanvas, Single sx, Single sy);
		}
		private static Delegates.sk_canvas_scale sk_canvas_scale_delegate;
		internal static void sk_canvas_scale (sk_canvas_t ccanvas, Single sx, Single sy) =>
			(sk_canvas_scale_delegate ??= GetSymbol<Delegates.sk_canvas_scale> ("sk_canvas_scale")).Invoke (ccanvas, sx, sy);
		#endif

		// void sk_canvas_set_matrix(sk_canvas_t* ccanvas, const sk_matrix44_t* cmatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix);
		}
		private static Delegates.sk_canvas_set_matrix sk_canvas_set_matrix_delegate;
		internal static void sk_canvas_set_matrix (sk_canvas_t ccanvas, SKMatrix44* cmatrix) =>
			(sk_canvas_set_matrix_delegate ??= GetSymbol<Delegates.sk_canvas_set_matrix> ("sk_canvas_set_matrix")).Invoke (ccanvas, cmatrix);
		#endif

		// void sk_canvas_skew(sk_canvas_t* ccanvas, float sx, float sy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_skew (sk_canvas_t ccanvas, Single sx, Single sy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_skew (sk_canvas_t ccanvas, Single sx, Single sy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_skew (sk_canvas_t ccanvas, Single sx, Single sy);
		}
		private static Delegates.sk_canvas_skew sk_canvas_skew_delegate;
		internal static void sk_canvas_skew (sk_canvas_t ccanvas, Single sx, Single sy) =>
			(sk_canvas_skew_delegate ??= GetSymbol<Delegates.sk_canvas_skew> ("sk_canvas_skew")).Invoke (ccanvas, sx, sy);
		#endif

		// void sk_canvas_translate(sk_canvas_t* ccanvas, float dx, float dy)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_canvas_translate (sk_canvas_t ccanvas, Single dx, Single dy);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_canvas_translate (sk_canvas_t ccanvas, Single dx, Single dy);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_canvas_translate (sk_canvas_t ccanvas, Single dx, Single dy);
		}
		private static Delegates.sk_canvas_translate sk_canvas_translate_delegate;
		internal static void sk_canvas_translate (sk_canvas_t ccanvas, Single dx, Single dy) =>
			(sk_canvas_translate_delegate ??= GetSymbol<Delegates.sk_canvas_translate> ("sk_canvas_translate")).Invoke (ccanvas, dx, dy);
		#endif

		// gr_recording_context_t* sk_get_recording_context(sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial gr_recording_context_t sk_get_recording_context (sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern gr_recording_context_t sk_get_recording_context (sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate gr_recording_context_t sk_get_recording_context (sk_canvas_t canvas);
		}
		private static Delegates.sk_get_recording_context sk_get_recording_context_delegate;
		internal static gr_recording_context_t sk_get_recording_context (sk_canvas_t canvas) =>
			(sk_get_recording_context_delegate ??= GetSymbol<Delegates.sk_get_recording_context> ("sk_get_recording_context")).Invoke (canvas);
		#endif

		// sk_surface_t* sk_get_surface(sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_surface_t sk_get_surface (sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_surface_t sk_get_surface (sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_surface_t sk_get_surface (sk_canvas_t canvas);
		}
		private static Delegates.sk_get_surface sk_get_surface_delegate;
		internal static sk_surface_t sk_get_surface (sk_canvas_t canvas) =>
			(sk_get_surface_delegate ??= GetSymbol<Delegates.sk_get_surface> ("sk_get_surface")).Invoke (canvas);
		#endif

		// void sk_nodraw_canvas_destroy(sk_nodraw_canvas_t* t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t t);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t t);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t t);
		}
		private static Delegates.sk_nodraw_canvas_destroy sk_nodraw_canvas_destroy_delegate;
		internal static void sk_nodraw_canvas_destroy (sk_nodraw_canvas_t t) =>
			(sk_nodraw_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_nodraw_canvas_destroy> ("sk_nodraw_canvas_destroy")).Invoke (t);
		#endif

		// sk_nodraw_canvas_t* sk_nodraw_canvas_new(int width, int height)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height);
		}
		private static Delegates.sk_nodraw_canvas_new sk_nodraw_canvas_new_delegate;
		internal static sk_nodraw_canvas_t sk_nodraw_canvas_new (Int32 width, Int32 height) =>
			(sk_nodraw_canvas_new_delegate ??= GetSymbol<Delegates.sk_nodraw_canvas_new> ("sk_nodraw_canvas_new")).Invoke (width, height);
		#endif

		// void sk_nway_canvas_add_canvas(sk_nway_canvas_t* t, sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nway_canvas_add_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_add_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_add_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		}
		private static Delegates.sk_nway_canvas_add_canvas sk_nway_canvas_add_canvas_delegate;
		internal static void sk_nway_canvas_add_canvas (sk_nway_canvas_t t, sk_canvas_t canvas) =>
			(sk_nway_canvas_add_canvas_delegate ??= GetSymbol<Delegates.sk_nway_canvas_add_canvas> ("sk_nway_canvas_add_canvas")).Invoke (t, canvas);
		#endif

		// void sk_nway_canvas_destroy(sk_nway_canvas_t* t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nway_canvas_destroy (sk_nway_canvas_t t);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_destroy (sk_nway_canvas_t t);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_destroy (sk_nway_canvas_t t);
		}
		private static Delegates.sk_nway_canvas_destroy sk_nway_canvas_destroy_delegate;
		internal static void sk_nway_canvas_destroy (sk_nway_canvas_t t) =>
			(sk_nway_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_nway_canvas_destroy> ("sk_nway_canvas_destroy")).Invoke (t);
		#endif

		// sk_nway_canvas_t* sk_nway_canvas_new(int width, int height)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height);
		}
		private static Delegates.sk_nway_canvas_new sk_nway_canvas_new_delegate;
		internal static sk_nway_canvas_t sk_nway_canvas_new (Int32 width, Int32 height) =>
			(sk_nway_canvas_new_delegate ??= GetSymbol<Delegates.sk_nway_canvas_new> ("sk_nway_canvas_new")).Invoke (width, height);
		#endif

		// void sk_nway_canvas_remove_all(sk_nway_canvas_t* t)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nway_canvas_remove_all (sk_nway_canvas_t t);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_all (sk_nway_canvas_t t);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_remove_all (sk_nway_canvas_t t);
		}
		private static Delegates.sk_nway_canvas_remove_all sk_nway_canvas_remove_all_delegate;
		internal static void sk_nway_canvas_remove_all (sk_nway_canvas_t t) =>
			(sk_nway_canvas_remove_all_delegate ??= GetSymbol<Delegates.sk_nway_canvas_remove_all> ("sk_nway_canvas_remove_all")).Invoke (t);
		#endif

		// void sk_nway_canvas_remove_canvas(sk_nway_canvas_t* t, sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_nway_canvas_remove_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_nway_canvas_remove_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_nway_canvas_remove_canvas (sk_nway_canvas_t t, sk_canvas_t canvas);
		}
		private static Delegates.sk_nway_canvas_remove_canvas sk_nway_canvas_remove_canvas_delegate;
		internal static void sk_nway_canvas_remove_canvas (sk_nway_canvas_t t, sk_canvas_t canvas) =>
			(sk_nway_canvas_remove_canvas_delegate ??= GetSymbol<Delegates.sk_nway_canvas_remove_canvas> ("sk_nway_canvas_remove_canvas")).Invoke (t, canvas);
		#endif

		// void sk_overdraw_canvas_destroy(sk_overdraw_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas);
		}
		private static Delegates.sk_overdraw_canvas_destroy sk_overdraw_canvas_destroy_delegate;
		internal static void sk_overdraw_canvas_destroy (sk_overdraw_canvas_t canvas) =>
			(sk_overdraw_canvas_destroy_delegate ??= GetSymbol<Delegates.sk_overdraw_canvas_destroy> ("sk_overdraw_canvas_destroy")).Invoke (canvas);
		#endif

		// sk_overdraw_canvas_t* sk_overdraw_canvas_new(sk_canvas_t* canvas)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas);
		}
		private static Delegates.sk_overdraw_canvas_new sk_overdraw_canvas_new_delegate;
		internal static sk_overdraw_canvas_t sk_overdraw_canvas_new (sk_canvas_t canvas) =>
			(sk_overdraw_canvas_new_delegate ??= GetSymbol<Delegates.sk_overdraw_canvas_new> ("sk_overdraw_canvas_new")).Invoke (canvas);
		#endif

		#endregion

	}
}
