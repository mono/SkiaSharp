using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_imagefilter.h

		// sk_imagefilter_t* sk_imagefilter_new_arithmetic(float k1, float k2, float k3, float k4, bool enforcePMColor, const sk_imagefilter_t* background, const sk_imagefilter_t* foreground, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_arithmetic sk_imagefilter_new_arithmetic_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_arithmetic (Single k1, Single k2, Single k3, Single k4, [MarshalAs (UnmanagedType.I1)] bool enforcePMColor, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect) =>
			(sk_imagefilter_new_arithmetic_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_arithmetic> ("sk_imagefilter_new_arithmetic")).Invoke (k1, k2, k3, k4, enforcePMColor, background, foreground, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_blend(sk_blendmode_t mode, const sk_imagefilter_t* background, const sk_imagefilter_t* foreground, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_blend (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_blend (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_blend (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_blend sk_imagefilter_new_blend_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_blend (SKBlendMode mode, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect) =>
			(sk_imagefilter_new_blend_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_blend> ("sk_imagefilter_new_blend")).Invoke (mode, background, foreground, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_blender(sk_blender_t* blender, const sk_imagefilter_t* background, const sk_imagefilter_t* foreground, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_blender (sk_blender_t blender, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_blender (sk_blender_t blender, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_blender (sk_blender_t blender, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_blender sk_imagefilter_new_blender_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_blender (sk_blender_t blender, sk_imagefilter_t background, sk_imagefilter_t foreground, SKRect* cropRect) =>
			(sk_imagefilter_new_blender_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_blender> ("sk_imagefilter_new_blender")).Invoke (blender, background, foreground, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_blur(float sigmaX, float sigmaY, sk_shader_tilemode_t tileMode, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_blur sk_imagefilter_new_blur_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_blur (Single sigmaX, Single sigmaY, SKShaderTileMode tileMode, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_blur_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_blur> ("sk_imagefilter_new_blur")).Invoke (sigmaX, sigmaY, tileMode, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_color_filter(sk_colorfilter_t* cf, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_color_filter sk_imagefilter_new_color_filter_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_color_filter (sk_colorfilter_t cf, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_color_filter_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_color_filter> ("sk_imagefilter_new_color_filter")).Invoke (cf, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_compose(const sk_imagefilter_t* outer, const sk_imagefilter_t* inner)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner);
		}
		private static Delegates.sk_imagefilter_new_compose sk_imagefilter_new_compose_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_compose (sk_imagefilter_t outer, sk_imagefilter_t inner) =>
			(sk_imagefilter_new_compose_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_compose> ("sk_imagefilter_new_compose")).Invoke (outer, inner);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_crop(const sk_rect_t* rect, sk_shader_tilemode_t tileMode, const sk_imagefilter_t* input)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_crop (SKRect* rect, SKShaderTileMode tileMode, sk_imagefilter_t input);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_crop (SKRect* rect, SKShaderTileMode tileMode, sk_imagefilter_t input);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_crop (SKRect* rect, SKShaderTileMode tileMode, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_crop sk_imagefilter_new_crop_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_crop (SKRect* rect, SKShaderTileMode tileMode, sk_imagefilter_t input) =>
			(sk_imagefilter_new_crop_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_crop> ("sk_imagefilter_new_crop")).Invoke (rect, tileMode, input);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_dilate(float radiusX, float radiusY, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_dilate sk_imagefilter_new_dilate_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_dilate (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_dilate_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_dilate> ("sk_imagefilter_new_dilate")).Invoke (radiusX, radiusY, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_displacement_map_effect(sk_color_channel_t xChannelSelector, sk_color_channel_t yChannelSelector, float scale, const sk_imagefilter_t* displacement, const sk_imagefilter_t* color, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_displacement_map_effect sk_imagefilter_new_displacement_map_effect_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_displacement_map_effect (SKColorChannel xChannelSelector, SKColorChannel yChannelSelector, Single scale, sk_imagefilter_t displacement, sk_imagefilter_t color, SKRect* cropRect) =>
			(sk_imagefilter_new_displacement_map_effect_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_displacement_map_effect> ("sk_imagefilter_new_displacement_map_effect")).Invoke (xChannelSelector, yChannelSelector, scale, displacement, color, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_diffuse(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float kd, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_distant_lit_diffuse sk_imagefilter_new_distant_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_distant_lit_diffuse (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_distant_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_distant_lit_diffuse> ("sk_imagefilter_new_distant_lit_diffuse")).Invoke (direction, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_distant_lit_specular(const sk_point3_t* direction, sk_color_t lightColor, float surfaceScale, float ks, float shininess, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_distant_lit_specular sk_imagefilter_new_distant_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_distant_lit_specular (SKPoint3* direction, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_distant_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_distant_lit_specular> ("sk_imagefilter_new_distant_lit_specular")).Invoke (direction, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_drop_shadow(float dx, float dy, float sigmaX, float sigmaY, sk_color_t color, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_drop_shadow sk_imagefilter_new_drop_shadow_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_drop_shadow (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_drop_shadow_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_drop_shadow> ("sk_imagefilter_new_drop_shadow")).Invoke (dx, dy, sigmaX, sigmaY, color, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_drop_shadow_only(float dx, float dy, float sigmaX, float sigmaY, sk_color_t color, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_drop_shadow_only sk_imagefilter_new_drop_shadow_only_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_drop_shadow_only (Single dx, Single dy, Single sigmaX, Single sigmaY, UInt32 color, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_drop_shadow_only_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_drop_shadow_only> ("sk_imagefilter_new_drop_shadow_only")).Invoke (dx, dy, sigmaX, sigmaY, color, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_empty ();
		}
		private static Delegates.sk_imagefilter_new_empty sk_imagefilter_new_empty_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_empty () =>
			(sk_imagefilter_new_empty_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_empty> ("sk_imagefilter_new_empty")).Invoke ();
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_erode(float radiusX, float radiusY, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_erode sk_imagefilter_new_erode_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_erode (Single radiusX, Single radiusY, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_erode_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_erode> ("sk_imagefilter_new_erode")).Invoke (radiusX, radiusY, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_image(sk_image_t* image, const sk_rect_t* srcRect, const sk_rect_t* dstRect, const sk_sampling_options_t* sampling)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_image (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_image (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling);
		}
		private static Delegates.sk_imagefilter_new_image sk_imagefilter_new_image_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_image (sk_image_t image, SKRect* srcRect, SKRect* dstRect, SKSamplingOptions* sampling) =>
			(sk_imagefilter_new_image_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_image> ("sk_imagefilter_new_image")).Invoke (image, srcRect, dstRect, sampling);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_image_simple(sk_image_t* image, const sk_sampling_options_t* sampling)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_image_simple (sk_image_t image, SKSamplingOptions* sampling);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_image_simple (sk_image_t image, SKSamplingOptions* sampling);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_image_simple (sk_image_t image, SKSamplingOptions* sampling);
		}
		private static Delegates.sk_imagefilter_new_image_simple sk_imagefilter_new_image_simple_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_image_simple (sk_image_t image, SKSamplingOptions* sampling) =>
			(sk_imagefilter_new_image_simple_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_image_simple> ("sk_imagefilter_new_image_simple")).Invoke (image, sampling);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_magnifier(const sk_rect_t* lensBounds, float zoomAmount, float inset, const sk_sampling_options_t* sampling, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* lensBounds, Single zoomAmount, Single inset, SKSamplingOptions* sampling, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* lensBounds, Single zoomAmount, Single inset, SKSamplingOptions* sampling, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* lensBounds, Single zoomAmount, Single inset, SKSamplingOptions* sampling, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_magnifier sk_imagefilter_new_magnifier_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_magnifier (SKRect* lensBounds, Single zoomAmount, Single inset, SKSamplingOptions* sampling, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_magnifier_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_magnifier> ("sk_imagefilter_new_magnifier")).Invoke (lensBounds, zoomAmount, inset, sampling, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_matrix_convolution(const sk_isize_t* kernelSize, const float[-1] kernel, float gain, float bias, const sk_ipoint_t* kernelOffset, sk_shader_tilemode_t ctileMode, bool convolveAlpha, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_matrix_convolution sk_imagefilter_new_matrix_convolution_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_matrix_convolution (SKSizeI* kernelSize, Single* kernel, Single gain, Single bias, SKPointI* kernelOffset, SKShaderTileMode ctileMode, [MarshalAs (UnmanagedType.I1)] bool convolveAlpha, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_matrix_convolution_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_matrix_convolution> ("sk_imagefilter_new_matrix_convolution")).Invoke (kernelSize, kernel, gain, bias, kernelOffset, ctileMode, convolveAlpha, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_matrix_transform(const sk_matrix_t* cmatrix, const sk_sampling_options_t* sampling, const sk_imagefilter_t* input)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_matrix_transform (SKMatrix* cmatrix, SKSamplingOptions* sampling, sk_imagefilter_t input);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_matrix_transform (SKMatrix* cmatrix, SKSamplingOptions* sampling, sk_imagefilter_t input);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_matrix_transform (SKMatrix* cmatrix, SKSamplingOptions* sampling, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_matrix_transform sk_imagefilter_new_matrix_transform_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_matrix_transform (SKMatrix* cmatrix, SKSamplingOptions* sampling, sk_imagefilter_t input) =>
			(sk_imagefilter_new_matrix_transform_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_matrix_transform> ("sk_imagefilter_new_matrix_transform")).Invoke (cmatrix, sampling, input);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_merge(const sk_imagefilter_t*[-1] cfilters, int count, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* cfilters, Int32 count, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* cfilters, Int32 count, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* cfilters, Int32 count, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_merge sk_imagefilter_new_merge_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_merge (sk_imagefilter_t* cfilters, Int32 count, SKRect* cropRect) =>
			(sk_imagefilter_new_merge_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_merge> ("sk_imagefilter_new_merge")).Invoke (cfilters, count, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_merge_simple(const sk_imagefilter_t* first, const sk_imagefilter_t* second, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_merge_simple (sk_imagefilter_t first, sk_imagefilter_t second, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_merge_simple (sk_imagefilter_t first, sk_imagefilter_t second, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_merge_simple (sk_imagefilter_t first, sk_imagefilter_t second, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_merge_simple sk_imagefilter_new_merge_simple_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_merge_simple (sk_imagefilter_t first, sk_imagefilter_t second, SKRect* cropRect) =>
			(sk_imagefilter_new_merge_simple_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_merge_simple> ("sk_imagefilter_new_merge_simple")).Invoke (first, second, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_offset(float dx, float dy, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_offset sk_imagefilter_new_offset_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_offset (Single dx, Single dy, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_offset_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_offset> ("sk_imagefilter_new_offset")).Invoke (dx, dy, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_picture(const sk_picture_t* picture)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture);
		}
		private static Delegates.sk_imagefilter_new_picture sk_imagefilter_new_picture_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_picture (sk_picture_t picture) =>
			(sk_imagefilter_new_picture_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_picture> ("sk_imagefilter_new_picture")).Invoke (picture);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_picture_with_rect(const sk_picture_t* picture, const sk_rect_t* targetRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_picture_with_rect (sk_picture_t picture, SKRect* targetRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_picture_with_rect (sk_picture_t picture, SKRect* targetRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_picture_with_rect (sk_picture_t picture, SKRect* targetRect);
		}
		private static Delegates.sk_imagefilter_new_picture_with_rect sk_imagefilter_new_picture_with_rect_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_picture_with_rect (sk_picture_t picture, SKRect* targetRect) =>
			(sk_imagefilter_new_picture_with_rect_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_picture_with_rect> ("sk_imagefilter_new_picture_with_rect")).Invoke (picture, targetRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_diffuse(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float kd, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_point_lit_diffuse sk_imagefilter_new_point_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_point_lit_diffuse (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_point_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_point_lit_diffuse> ("sk_imagefilter_new_point_lit_diffuse")).Invoke (location, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_point_lit_specular(const sk_point3_t* location, sk_color_t lightColor, float surfaceScale, float ks, float shininess, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_point_lit_specular sk_imagefilter_new_point_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_point_lit_specular (SKPoint3* location, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_point_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_point_lit_specular> ("sk_imagefilter_new_point_lit_specular")).Invoke (location, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_shader(const sk_shader_t* shader, bool dither, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_shader (sk_shader_t shader, [MarshalAs (UnmanagedType.I1)] bool dither, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_shader (sk_shader_t shader, [MarshalAs (UnmanagedType.I1)] bool dither, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_shader (sk_shader_t shader, [MarshalAs (UnmanagedType.I1)] bool dither, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_shader sk_imagefilter_new_shader_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_shader (sk_shader_t shader, [MarshalAs (UnmanagedType.I1)] bool dither, SKRect* cropRect) =>
			(sk_imagefilter_new_shader_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_shader> ("sk_imagefilter_new_shader")).Invoke (shader, dither, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_diffuse(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float kd, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_spot_lit_diffuse sk_imagefilter_new_spot_lit_diffuse_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_spot_lit_diffuse (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single kd, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_spot_lit_diffuse_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_spot_lit_diffuse> ("sk_imagefilter_new_spot_lit_diffuse")).Invoke (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, kd, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_spot_lit_specular(const sk_point3_t* location, const sk_point3_t* target, float specularExponent, float cutoffAngle, sk_color_t lightColor, float surfaceScale, float ks, float shininess, const sk_imagefilter_t* input, const sk_rect_t* cropRect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect);
		}
		private static Delegates.sk_imagefilter_new_spot_lit_specular sk_imagefilter_new_spot_lit_specular_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_spot_lit_specular (SKPoint3* location, SKPoint3* target, Single specularExponent, Single cutoffAngle, UInt32 lightColor, Single surfaceScale, Single ks, Single shininess, sk_imagefilter_t input, SKRect* cropRect) =>
			(sk_imagefilter_new_spot_lit_specular_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_spot_lit_specular> ("sk_imagefilter_new_spot_lit_specular")).Invoke (location, target, specularExponent, cutoffAngle, lightColor, surfaceScale, ks, shininess, input, cropRect);
		#endif

		// sk_imagefilter_t* sk_imagefilter_new_tile(const sk_rect_t* src, const sk_rect_t* dst, const sk_imagefilter_t* input)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input);
		}
		private static Delegates.sk_imagefilter_new_tile sk_imagefilter_new_tile_delegate;
		internal static sk_imagefilter_t sk_imagefilter_new_tile (SKRect* src, SKRect* dst, sk_imagefilter_t input) =>
			(sk_imagefilter_new_tile_delegate ??= GetSymbol<Delegates.sk_imagefilter_new_tile> ("sk_imagefilter_new_tile")).Invoke (src, dst, input);
		#endif

		// void sk_imagefilter_unref(sk_imagefilter_t* cfilter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_imagefilter_unref (sk_imagefilter_t cfilter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_imagefilter_unref (sk_imagefilter_t cfilter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_imagefilter_unref (sk_imagefilter_t cfilter);
		}
		private static Delegates.sk_imagefilter_unref sk_imagefilter_unref_delegate;
		internal static void sk_imagefilter_unref (sk_imagefilter_t cfilter) =>
			(sk_imagefilter_unref_delegate ??= GetSymbol<Delegates.sk_imagefilter_unref> ("sk_imagefilter_unref")).Invoke (cfilter);
		#endif

		#endregion

	}
}
