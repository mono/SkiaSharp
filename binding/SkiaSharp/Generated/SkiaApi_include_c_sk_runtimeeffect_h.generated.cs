using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_runtimeeffect.h

		// void sk_runtimeeffect_get_child_from_index(const sk_runtimeeffect_t* effect, int index, sk_runtimeeffect_child_t* cchild)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_child_from_index (IntPtr effect, Int32 index, SKRuntimeEffectChildNative* cchild);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_child_from_index (IntPtr effect, Int32 index, SKRuntimeEffectChildNative* cchild);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_child_from_index (IntPtr effect, Int32 index, SKRuntimeEffectChildNative* cchild);
		}
		private static Delegates.sk_runtimeeffect_get_child_from_index sk_runtimeeffect_get_child_from_index_delegate;
		internal static void sk_runtimeeffect_get_child_from_index (IntPtr effect, Int32 index, SKRuntimeEffectChildNative* cchild) =>
			(sk_runtimeeffect_get_child_from_index_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_child_from_index> ("sk_runtimeeffect_get_child_from_index")).Invoke (effect, index, cchild);
		#endif

		// void sk_runtimeeffect_get_child_from_name(const sk_runtimeeffect_t* effect, const char* name, size_t len, sk_runtimeeffect_child_t* cchild)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_child_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectChildNative* cchild);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_child_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectChildNative* cchild);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_child_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectChildNative* cchild);
		}
		private static Delegates.sk_runtimeeffect_get_child_from_name sk_runtimeeffect_get_child_from_name_delegate;
		internal static void sk_runtimeeffect_get_child_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectChildNative* cchild) =>
			(sk_runtimeeffect_get_child_from_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_child_from_name> ("sk_runtimeeffect_get_child_from_name")).Invoke (effect, name, len, cchild);
		#endif

		// void sk_runtimeeffect_get_child_name(const sk_runtimeeffect_t* effect, int index, sk_string_t* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_child_name (IntPtr effect, Int32 index, IntPtr name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_child_name (IntPtr effect, Int32 index, IntPtr name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_child_name (IntPtr effect, Int32 index, IntPtr name);
		}
		private static Delegates.sk_runtimeeffect_get_child_name sk_runtimeeffect_get_child_name_delegate;
		internal static void sk_runtimeeffect_get_child_name (IntPtr effect, Int32 index, IntPtr name) =>
			(sk_runtimeeffect_get_child_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_child_name> ("sk_runtimeeffect_get_child_name")).Invoke (effect, index, name);
		#endif

		// size_t sk_runtimeeffect_get_children_size(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_runtimeeffect_get_children_size (IntPtr effect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_children_size (IntPtr effect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_children_size (IntPtr effect);
		}
		private static Delegates.sk_runtimeeffect_get_children_size sk_runtimeeffect_get_children_size_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_children_size (IntPtr effect) =>
			(sk_runtimeeffect_get_children_size_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_children_size> ("sk_runtimeeffect_get_children_size")).Invoke (effect);
		#endif

		// size_t sk_runtimeeffect_get_uniform_byte_size(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_runtimeeffect_get_uniform_byte_size (IntPtr effect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_uniform_byte_size (IntPtr effect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_uniform_byte_size (IntPtr effect);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_byte_size sk_runtimeeffect_get_uniform_byte_size_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_uniform_byte_size (IntPtr effect) =>
			(sk_runtimeeffect_get_uniform_byte_size_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_byte_size> ("sk_runtimeeffect_get_uniform_byte_size")).Invoke (effect);
		#endif

		// void sk_runtimeeffect_get_uniform_from_index(const sk_runtimeeffect_t* effect, int index, sk_runtimeeffect_uniform_t* cuniform)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_uniform_from_index (IntPtr effect, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_uniform_from_index (IntPtr effect, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_uniform_from_index (IntPtr effect, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_from_index sk_runtimeeffect_get_uniform_from_index_delegate;
		internal static void sk_runtimeeffect_get_uniform_from_index (IntPtr effect, Int32 index, SKRuntimeEffectUniformNative* cuniform) =>
			(sk_runtimeeffect_get_uniform_from_index_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_index> ("sk_runtimeeffect_get_uniform_from_index")).Invoke (effect, index, cuniform);
		#endif

		// void sk_runtimeeffect_get_uniform_from_name(const sk_runtimeeffect_t* effect, const char* name, size_t len, sk_runtimeeffect_uniform_t* cuniform)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_uniform_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectUniformNative* cuniform);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_uniform_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectUniformNative* cuniform);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_uniform_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectUniformNative* cuniform);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_from_name sk_runtimeeffect_get_uniform_from_name_delegate;
		internal static void sk_runtimeeffect_get_uniform_from_name (IntPtr effect, /* char */ void* name, /* size_t */ IntPtr len, SKRuntimeEffectUniformNative* cuniform) =>
			(sk_runtimeeffect_get_uniform_from_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_from_name> ("sk_runtimeeffect_get_uniform_from_name")).Invoke (effect, name, len, cuniform);
		#endif

		// void sk_runtimeeffect_get_uniform_name(const sk_runtimeeffect_t* effect, int index, sk_string_t* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_get_uniform_name (IntPtr effect, Int32 index, IntPtr name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_get_uniform_name (IntPtr effect, Int32 index, IntPtr name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_get_uniform_name (IntPtr effect, Int32 index, IntPtr name);
		}
		private static Delegates.sk_runtimeeffect_get_uniform_name sk_runtimeeffect_get_uniform_name_delegate;
		internal static void sk_runtimeeffect_get_uniform_name (IntPtr effect, Int32 index, IntPtr name) =>
			(sk_runtimeeffect_get_uniform_name_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniform_name> ("sk_runtimeeffect_get_uniform_name")).Invoke (effect, index, name);
		#endif

		// size_t sk_runtimeeffect_get_uniforms_size(const sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_size (IntPtr effect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_size (IntPtr effect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_size (IntPtr effect);
		}
		private static Delegates.sk_runtimeeffect_get_uniforms_size sk_runtimeeffect_get_uniforms_size_delegate;
		internal static /* size_t */ IntPtr sk_runtimeeffect_get_uniforms_size (IntPtr effect) =>
			(sk_runtimeeffect_get_uniforms_size_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_get_uniforms_size> ("sk_runtimeeffect_get_uniforms_size")).Invoke (effect);
		#endif

		// sk_blender_t* sk_runtimeeffect_make_blender(sk_runtimeeffect_t* effect, sk_data_t* uniforms, sk_flattenable_t** children, size_t childCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_blender (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_blender (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_blender (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		}
		private static Delegates.sk_runtimeeffect_make_blender sk_runtimeeffect_make_blender_delegate;
		internal static IntPtr sk_runtimeeffect_make_blender (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount) =>
			(sk_runtimeeffect_make_blender_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_blender> ("sk_runtimeeffect_make_blender")).Invoke (effect, uniforms, children, childCount);
		#endif

		// sk_colorfilter_t* sk_runtimeeffect_make_color_filter(sk_runtimeeffect_t* effect, sk_data_t* uniforms, sk_flattenable_t** children, size_t childCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_color_filter (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_color_filter (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_color_filter (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount);
		}
		private static Delegates.sk_runtimeeffect_make_color_filter sk_runtimeeffect_make_color_filter_delegate;
		internal static IntPtr sk_runtimeeffect_make_color_filter (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount) =>
			(sk_runtimeeffect_make_color_filter_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_color_filter> ("sk_runtimeeffect_make_color_filter")).Invoke (effect, uniforms, children, childCount);
		#endif

		// sk_runtimeeffect_t* sk_runtimeeffect_make_for_blender(sk_string_t* sksl, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_for_blender (IntPtr sksl, IntPtr error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_for_blender (IntPtr sksl, IntPtr error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_for_blender (IntPtr sksl, IntPtr error);
		}
		private static Delegates.sk_runtimeeffect_make_for_blender sk_runtimeeffect_make_for_blender_delegate;
		internal static IntPtr sk_runtimeeffect_make_for_blender (IntPtr sksl, IntPtr error) =>
			(sk_runtimeeffect_make_for_blender_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_for_blender> ("sk_runtimeeffect_make_for_blender")).Invoke (sksl, error);
		#endif

		// sk_runtimeeffect_t* sk_runtimeeffect_make_for_color_filter(sk_string_t* sksl, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_for_color_filter (IntPtr sksl, IntPtr error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_for_color_filter (IntPtr sksl, IntPtr error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_for_color_filter (IntPtr sksl, IntPtr error);
		}
		private static Delegates.sk_runtimeeffect_make_for_color_filter sk_runtimeeffect_make_for_color_filter_delegate;
		internal static IntPtr sk_runtimeeffect_make_for_color_filter (IntPtr sksl, IntPtr error) =>
			(sk_runtimeeffect_make_for_color_filter_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_for_color_filter> ("sk_runtimeeffect_make_for_color_filter")).Invoke (sksl, error);
		#endif

		// sk_runtimeeffect_t* sk_runtimeeffect_make_for_shader(sk_string_t* sksl, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_for_shader (IntPtr sksl, IntPtr error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_for_shader (IntPtr sksl, IntPtr error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_for_shader (IntPtr sksl, IntPtr error);
		}
		private static Delegates.sk_runtimeeffect_make_for_shader sk_runtimeeffect_make_for_shader_delegate;
		internal static IntPtr sk_runtimeeffect_make_for_shader (IntPtr sksl, IntPtr error) =>
			(sk_runtimeeffect_make_for_shader_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_for_shader> ("sk_runtimeeffect_make_for_shader")).Invoke (sksl, error);
		#endif

		// sk_shader_t* sk_runtimeeffect_make_shader(sk_runtimeeffect_t* effect, sk_data_t* uniforms, sk_flattenable_t** children, size_t childCount, const sk_matrix_t* localMatrix)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_runtimeeffect_make_shader (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_runtimeeffect_make_shader (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_runtimeeffect_make_shader (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix);
		}
		private static Delegates.sk_runtimeeffect_make_shader sk_runtimeeffect_make_shader_delegate;
		internal static IntPtr sk_runtimeeffect_make_shader (IntPtr effect, IntPtr uniforms, IntPtr* children, /* size_t */ IntPtr childCount, SKMatrix* localMatrix) =>
			(sk_runtimeeffect_make_shader_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_make_shader> ("sk_runtimeeffect_make_shader")).Invoke (effect, uniforms, children, childCount, localMatrix);
		#endif

		// void sk_runtimeeffect_unref(sk_runtimeeffect_t* effect)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_runtimeeffect_unref (IntPtr effect);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_runtimeeffect_unref (IntPtr effect);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_runtimeeffect_unref (IntPtr effect);
		}
		private static Delegates.sk_runtimeeffect_unref sk_runtimeeffect_unref_delegate;
		internal static void sk_runtimeeffect_unref (IntPtr effect) =>
			(sk_runtimeeffect_unref_delegate ??= GetSymbol<Delegates.sk_runtimeeffect_unref> ("sk_runtimeeffect_unref")).Invoke (effect);
		#endif

		#endregion

	}
}
