using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_shader.h

		// sk_shader_t* sk_shader_new_blend(sk_blendmode_t mode, const sk_shader_t* dst, const sk_shader_t* src)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_blend (SKBlendMode mode, IntPtr dst, IntPtr src);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_blend (SKBlendMode mode, IntPtr dst, IntPtr src);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_blend (SKBlendMode mode, IntPtr dst, IntPtr src);
		}
		private static Delegates.sk_shader_new_blend sk_shader_new_blend_delegate;
		internal static IntPtr sk_shader_new_blend (SKBlendMode mode, IntPtr dst, IntPtr src) =>
			(sk_shader_new_blend_delegate ??= GetSymbol<Delegates.sk_shader_new_blend> ("sk_shader_new_blend")).Invoke (mode, dst, src);
		#endif

		// sk_shader_t* sk_shader_new_blender(sk_blender_t* blender, const sk_shader_t* dst, const sk_shader_t* src)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_blender (IntPtr blender, IntPtr dst, IntPtr src);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_blender (IntPtr blender, IntPtr dst, IntPtr src);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_blender (IntPtr blender, IntPtr dst, IntPtr src);
		}
		private static Delegates.sk_shader_new_blender sk_shader_new_blender_delegate;
		internal static IntPtr sk_shader_new_blender (IntPtr blender, IntPtr dst, IntPtr src) =>
			(sk_shader_new_blender_delegate ??= GetSymbol<Delegates.sk_shader_new_blender> ("sk_shader_new_blender")).Invoke (blender, dst, src);
		#endif

		// sk_shader_t* sk_shader_new_color(sk_color_t color)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_color (UInt32 color);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_color (UInt32 color);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_color (UInt32 color);
		}
		private static Delegates.sk_shader_new_color sk_shader_new_color_delegate;
		internal static IntPtr sk_shader_new_color (UInt32 color) =>
			(sk_shader_new_color_delegate ??= GetSymbol<Delegates.sk_shader_new_color> ("sk_shader_new_color")).Invoke (color);
		#endif

		// sk_shader_t* sk_shader_new_color4f(const sk_color4f_t* color, const sk_colorspace_t* colorspace)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_color4f (SKColorF* color, IntPtr colorspace);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_color4f (SKColorF* color, IntPtr colorspace);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_color4f (SKColorF* color, IntPtr colorspace);
		}
		private static Delegates.sk_shader_new_color4f sk_shader_new_color4f_delegate;
		internal static IntPtr sk_shader_new_color4f (SKColorF* color, IntPtr colorspace) =>
			(sk_shader_new_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_color4f> ("sk_shader_new_color4f")).Invoke (color, colorspace);
		#endif

		// sk_shader_t* sk_shader_new_empty()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_empty ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_empty ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_empty ();
		}
		private static Delegates.sk_shader_new_empty sk_shader_new_empty_delegate;
		internal static IntPtr sk_shader_new_empty () =>
			(sk_shader_new_empty_delegate ??= GetSymbol<Delegates.sk_shader_new_empty> ("sk_shader_new_empty")).Invoke ();
		#endif

		// sk_shader_t* sk_shader_new_linear_gradient(const sk_point_t[2] points = 2, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_linear_gradient sk_shader_new_linear_gradient_delegate;
		internal static IntPtr sk_shader_new_linear_gradient (SKPoint* points, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_linear_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_linear_gradient> ("sk_shader_new_linear_gradient")).Invoke (points, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_linear_gradient_color4f(const sk_point_t[2] points = 2, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_linear_gradient_color4f sk_shader_new_linear_gradient_color4f_delegate;
		internal static IntPtr sk_shader_new_linear_gradient_color4f (SKPoint* points, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_linear_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_linear_gradient_color4f> ("sk_shader_new_linear_gradient_color4f")).Invoke (points, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_perlin_noise_fractal_noise(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		}
		private static Delegates.sk_shader_new_perlin_noise_fractal_noise sk_shader_new_perlin_noise_fractal_noise_delegate;
		internal static IntPtr sk_shader_new_perlin_noise_fractal_noise (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize) =>
			(sk_shader_new_perlin_noise_fractal_noise_delegate ??= GetSymbol<Delegates.sk_shader_new_perlin_noise_fractal_noise> ("sk_shader_new_perlin_noise_fractal_noise")).Invoke (baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
		#endif

		// sk_shader_t* sk_shader_new_perlin_noise_turbulence(float baseFrequencyX, float baseFrequencyY, int numOctaves, float seed, const sk_isize_t* tileSize)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize);
		}
		private static Delegates.sk_shader_new_perlin_noise_turbulence sk_shader_new_perlin_noise_turbulence_delegate;
		internal static IntPtr sk_shader_new_perlin_noise_turbulence (Single baseFrequencyX, Single baseFrequencyY, Int32 numOctaves, Single seed, SKSizeI* tileSize) =>
			(sk_shader_new_perlin_noise_turbulence_delegate ??= GetSymbol<Delegates.sk_shader_new_perlin_noise_turbulence> ("sk_shader_new_perlin_noise_turbulence")).Invoke (baseFrequencyX, baseFrequencyY, numOctaves, seed, tileSize);
		#endif

		// sk_shader_t* sk_shader_new_radial_gradient(const sk_point_t* center, float radius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_radial_gradient sk_shader_new_radial_gradient_delegate;
		internal static IntPtr sk_shader_new_radial_gradient (SKPoint* center, Single radius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_radial_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_radial_gradient> ("sk_shader_new_radial_gradient")).Invoke (center, radius, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_radial_gradient_color4f(const sk_point_t* center, float radius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_radial_gradient_color4f sk_shader_new_radial_gradient_color4f_delegate;
		internal static IntPtr sk_shader_new_radial_gradient_color4f (SKPoint* center, Single radius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_radial_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_radial_gradient_color4f> ("sk_shader_new_radial_gradient_color4f")).Invoke (center, radius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_sweep_gradient(const sk_point_t* center, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_sweep_gradient sk_shader_new_sweep_gradient_delegate;
		internal static IntPtr sk_shader_new_sweep_gradient (SKPoint* center, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix) =>
			(sk_shader_new_sweep_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_sweep_gradient> ("sk_shader_new_sweep_gradient")).Invoke (center, colors, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_sweep_gradient_color4f(const sk_point_t* center, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, float startAngle, float endAngle, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_sweep_gradient_color4f sk_shader_new_sweep_gradient_color4f_delegate;
		internal static IntPtr sk_shader_new_sweep_gradient_color4f (SKPoint* center, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, Single startAngle, Single endAngle, SKMatrix* localMatrix) =>
			(sk_shader_new_sweep_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_sweep_gradient_color4f> ("sk_shader_new_sweep_gradient_color4f")).Invoke (center, colors, colorspace, colorPos, colorCount, tileMode, startAngle, endAngle, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_two_point_conical_gradient(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color_t[-1] colors, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_two_point_conical_gradient sk_shader_new_two_point_conical_gradient_delegate;
		internal static IntPtr sk_shader_new_two_point_conical_gradient (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, UInt32* colors, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_two_point_conical_gradient_delegate ??= GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient> ("sk_shader_new_two_point_conical_gradient")).Invoke (start, startRadius, end, endRadius, colors, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// sk_shader_t* sk_shader_new_two_point_conical_gradient_color4f(const sk_point_t* start, float startRadius, const sk_point_t* end, float endRadius, const sk_color4f_t* colors, const sk_colorspace_t* colorspace, const float[-1] colorPos, int colorCount, sk_shader_tilemode_t tileMode, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_new_two_point_conical_gradient_color4f sk_shader_new_two_point_conical_gradient_color4f_delegate;
		internal static IntPtr sk_shader_new_two_point_conical_gradient_color4f (SKPoint* start, Single startRadius, SKPoint* end, Single endRadius, SKColorF* colors, IntPtr colorspace, Single* colorPos, Int32 colorCount, SKShaderTileMode tileMode, SKMatrix* localMatrix) =>
			(sk_shader_new_two_point_conical_gradient_color4f_delegate ??= GetSymbol<Delegates.sk_shader_new_two_point_conical_gradient_color4f> ("sk_shader_new_two_point_conical_gradient_color4f")).Invoke (start, startRadius, end, endRadius, colors, colorspace, colorPos, colorCount, tileMode, localMatrix);
		#endif

		// void sk_shader_ref(sk_shader_t* shader)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_shader_ref (IntPtr shader);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_ref (IntPtr shader);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_shader_ref (IntPtr shader);
		}
		private static Delegates.sk_shader_ref sk_shader_ref_delegate;
		internal static void sk_shader_ref (IntPtr shader) =>
			(sk_shader_ref_delegate ??= GetSymbol<Delegates.sk_shader_ref> ("sk_shader_ref")).Invoke (shader);
		#endif

		// void sk_shader_unref(sk_shader_t* shader)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_shader_unref (IntPtr shader);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_shader_unref (IntPtr shader);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_shader_unref (IntPtr shader);
		}
		private static Delegates.sk_shader_unref sk_shader_unref_delegate;
		internal static void sk_shader_unref (IntPtr shader) =>
			(sk_shader_unref_delegate ??= GetSymbol<Delegates.sk_shader_unref> ("sk_shader_unref")).Invoke (shader);
		#endif

		// sk_shader_t* sk_shader_with_color_filter(const sk_shader_t* shader, const sk_colorfilter_t* filter)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_with_color_filter (IntPtr shader, IntPtr filter);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_with_color_filter (IntPtr shader, IntPtr filter);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_with_color_filter (IntPtr shader, IntPtr filter);
		}
		private static Delegates.sk_shader_with_color_filter sk_shader_with_color_filter_delegate;
		internal static IntPtr sk_shader_with_color_filter (IntPtr shader, IntPtr filter) =>
			(sk_shader_with_color_filter_delegate ??= GetSymbol<Delegates.sk_shader_with_color_filter> ("sk_shader_with_color_filter")).Invoke (shader, filter);
		#endif

		// sk_shader_t* sk_shader_with_local_matrix(const sk_shader_t* shader, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_shader_with_local_matrix (IntPtr shader, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_shader_with_local_matrix (IntPtr shader, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_shader_with_local_matrix (IntPtr shader, SKMatrix* localMatrix);
		}
		private static Delegates.sk_shader_with_local_matrix sk_shader_with_local_matrix_delegate;
		internal static IntPtr sk_shader_with_local_matrix (IntPtr shader, SKMatrix* localMatrix) =>
			(sk_shader_with_local_matrix_delegate ??= GetSymbol<Delegates.sk_shader_with_local_matrix> ("sk_shader_with_local_matrix")).Invoke (shader, localMatrix);
		#endif

		#endregion

	}
}
