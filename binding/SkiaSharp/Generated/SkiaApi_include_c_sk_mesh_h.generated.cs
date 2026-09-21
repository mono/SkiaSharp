using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_mesh.h

		// void sk_mesh_delete(sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_delete (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_delete (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_delete (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_delete sk_mesh_delete_delegate;
		internal static void sk_mesh_delete (sk_mesh_t mesh) =>
			(sk_mesh_delete_delegate ??= GetSymbol<Delegates.sk_mesh_delete> ("sk_mesh_delete")).Invoke (mesh);
		#endif

		// bool sk_mesh_get_is_valid(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_mesh_get_is_valid (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mesh_get_is_valid (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_mesh_get_is_valid (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_is_valid sk_mesh_get_is_valid_delegate;
		internal static bool sk_mesh_get_is_valid (sk_mesh_t mesh) =>
			(sk_mesh_get_is_valid_delegate ??= GetSymbol<Delegates.sk_mesh_get_is_valid> ("sk_mesh_get_is_valid")).Invoke (mesh);
		#endif

		// sk_mesh_index_buffer_t* sk_mesh_index_buffer_copy(const sk_mesh_index_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_mesh_index_buffer_t sk_mesh_index_buffer_copy (sk_mesh_index_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_index_buffer_t sk_mesh_index_buffer_copy (sk_mesh_index_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_index_buffer_t sk_mesh_index_buffer_copy (sk_mesh_index_buffer_t buffer);
		}
		private static Delegates.sk_mesh_index_buffer_copy sk_mesh_index_buffer_copy_delegate;
		internal static sk_mesh_index_buffer_t sk_mesh_index_buffer_copy (sk_mesh_index_buffer_t buffer) =>
			(sk_mesh_index_buffer_copy_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_copy> ("sk_mesh_index_buffer_copy")).Invoke (buffer);
		#endif

		// size_t sk_mesh_index_buffer_get_size(const sk_mesh_index_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_mesh_index_buffer_get_size (sk_mesh_index_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_index_buffer_get_size (sk_mesh_index_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_index_buffer_get_size (sk_mesh_index_buffer_t buffer);
		}
		private static Delegates.sk_mesh_index_buffer_get_size sk_mesh_index_buffer_get_size_delegate;
		internal static /* size_t */ IntPtr sk_mesh_index_buffer_get_size (sk_mesh_index_buffer_t buffer) =>
			(sk_mesh_index_buffer_get_size_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_get_size> ("sk_mesh_index_buffer_get_size")).Invoke (buffer);
		#endif

		// sk_mesh_index_buffer_t* sk_mesh_index_buffer_make(const void* data, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_mesh_index_buffer_t sk_mesh_index_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_index_buffer_t sk_mesh_index_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_index_buffer_t sk_mesh_index_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_index_buffer_make sk_mesh_index_buffer_make_delegate;
		internal static sk_mesh_index_buffer_t sk_mesh_index_buffer_make (IntPtr data, /* size_t */ IntPtr size) =>
			(sk_mesh_index_buffer_make_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_make> ("sk_mesh_index_buffer_make")).Invoke (data, size);
		#endif

		// sk_mesh_t* sk_mesh_new()
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_mesh_t sk_mesh_new ();
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_t sk_mesh_new ();
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_t sk_mesh_new ();
		}
		private static Delegates.sk_mesh_new sk_mesh_new_delegate;
		internal static sk_mesh_t sk_mesh_new () =>
			(sk_mesh_new_delegate ??= GetSymbol<Delegates.sk_mesh_new> ("sk_mesh_new")).Invoke ();
		#endif

		// void sk_mesh_set_bounds(sk_mesh_t* mesh, const sk_rect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_bounds (sk_mesh_t mesh, IntPtr bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_bounds (sk_mesh_t mesh, IntPtr bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_bounds (sk_mesh_t mesh, IntPtr bounds);
		}
		private static Delegates.sk_mesh_set_bounds sk_mesh_set_bounds_delegate;
		internal static void sk_mesh_set_bounds (sk_mesh_t mesh, IntPtr bounds) =>
			(sk_mesh_set_bounds_delegate ??= GetSymbol<Delegates.sk_mesh_set_bounds> ("sk_mesh_set_bounds")).Invoke (mesh, bounds);
		#endif

		// void sk_mesh_set_children(sk_mesh_t* mesh, sk_flattenable_t** children, size_t childCount)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_children (sk_mesh_t mesh, IntPtr children, /* size_t */ IntPtr childCount);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_children (sk_mesh_t mesh, IntPtr children, /* size_t */ IntPtr childCount);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_children (sk_mesh_t mesh, IntPtr children, /* size_t */ IntPtr childCount);
		}
		private static Delegates.sk_mesh_set_children sk_mesh_set_children_delegate;
		internal static void sk_mesh_set_children (sk_mesh_t mesh, IntPtr children, /* size_t */ IntPtr childCount) =>
			(sk_mesh_set_children_delegate ??= GetSymbol<Delegates.sk_mesh_set_children> ("sk_mesh_set_children")).Invoke (mesh, children, childCount);
		#endif

		// void sk_mesh_set_index_buffer(sk_mesh_t* mesh, sk_mesh_index_buffer_t* ib, size_t indexCount, size_t indexOffset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_index_buffer (sk_mesh_t mesh, sk_mesh_index_buffer_t ib, /* size_t */ IntPtr indexCount, /* size_t */ IntPtr indexOffset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_index_buffer (sk_mesh_t mesh, sk_mesh_index_buffer_t ib, /* size_t */ IntPtr indexCount, /* size_t */ IntPtr indexOffset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_index_buffer (sk_mesh_t mesh, sk_mesh_index_buffer_t ib, /* size_t */ IntPtr indexCount, /* size_t */ IntPtr indexOffset);
		}
		private static Delegates.sk_mesh_set_index_buffer sk_mesh_set_index_buffer_delegate;
		internal static void sk_mesh_set_index_buffer (sk_mesh_t mesh, sk_mesh_index_buffer_t ib, /* size_t */ IntPtr indexCount, /* size_t */ IntPtr indexOffset) =>
			(sk_mesh_set_index_buffer_delegate ??= GetSymbol<Delegates.sk_mesh_set_index_buffer> ("sk_mesh_set_index_buffer")).Invoke (mesh, ib, indexCount, indexOffset);
		#endif

		// void sk_mesh_set_mode(sk_mesh_t* mesh, sk_mesh_mode_t mode)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_mode (sk_mesh_t mesh, SKMeshMode mode);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_mode (sk_mesh_t mesh, SKMeshMode mode);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_mode (sk_mesh_t mesh, SKMeshMode mode);
		}
		private static Delegates.sk_mesh_set_mode sk_mesh_set_mode_delegate;
		internal static void sk_mesh_set_mode (sk_mesh_t mesh, SKMeshMode mode) =>
			(sk_mesh_set_mode_delegate ??= GetSymbol<Delegates.sk_mesh_set_mode> ("sk_mesh_set_mode")).Invoke (mesh, mode);
		#endif

		// void sk_mesh_set_spec(sk_mesh_t* mesh, sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_spec (sk_mesh_t mesh, sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_spec (sk_mesh_t mesh, sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_spec (sk_mesh_t mesh, sk_meshspecification_t spec);
		}
		private static Delegates.sk_mesh_set_spec sk_mesh_set_spec_delegate;
		internal static void sk_mesh_set_spec (sk_mesh_t mesh, sk_meshspecification_t spec) =>
			(sk_mesh_set_spec_delegate ??= GetSymbol<Delegates.sk_mesh_set_spec> ("sk_mesh_set_spec")).Invoke (mesh, spec);
		#endif

		// void sk_mesh_set_uniforms(sk_mesh_t* mesh, sk_data_t* uniforms)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_uniforms (sk_mesh_t mesh, sk_data_t uniforms);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_uniforms (sk_mesh_t mesh, sk_data_t uniforms);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_uniforms (sk_mesh_t mesh, sk_data_t uniforms);
		}
		private static Delegates.sk_mesh_set_uniforms sk_mesh_set_uniforms_delegate;
		internal static void sk_mesh_set_uniforms (sk_mesh_t mesh, sk_data_t uniforms) =>
			(sk_mesh_set_uniforms_delegate ??= GetSymbol<Delegates.sk_mesh_set_uniforms> ("sk_mesh_set_uniforms")).Invoke (mesh, uniforms);
		#endif

		// void sk_mesh_set_vertex_buffer(sk_mesh_t* mesh, sk_mesh_vertex_buffer_t* vb, size_t vertexCount, size_t vertexOffset)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_mesh_set_vertex_buffer (sk_mesh_t mesh, sk_mesh_vertex_buffer_t vb, /* size_t */ IntPtr vertexCount, /* size_t */ IntPtr vertexOffset);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_set_vertex_buffer (sk_mesh_t mesh, sk_mesh_vertex_buffer_t vb, /* size_t */ IntPtr vertexCount, /* size_t */ IntPtr vertexOffset);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_set_vertex_buffer (sk_mesh_t mesh, sk_mesh_vertex_buffer_t vb, /* size_t */ IntPtr vertexCount, /* size_t */ IntPtr vertexOffset);
		}
		private static Delegates.sk_mesh_set_vertex_buffer sk_mesh_set_vertex_buffer_delegate;
		internal static void sk_mesh_set_vertex_buffer (sk_mesh_t mesh, sk_mesh_vertex_buffer_t vb, /* size_t */ IntPtr vertexCount, /* size_t */ IntPtr vertexOffset) =>
			(sk_mesh_set_vertex_buffer_delegate ??= GetSymbol<Delegates.sk_mesh_set_vertex_buffer> ("sk_mesh_set_vertex_buffer")).Invoke (mesh, vb, vertexCount, vertexOffset);
		#endif

		// bool sk_mesh_validate(sk_mesh_t* mesh, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_mesh_validate (sk_mesh_t mesh, sk_string_t error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mesh_validate (sk_mesh_t mesh, sk_string_t error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_mesh_validate (sk_mesh_t mesh, sk_string_t error);
		}
		private static Delegates.sk_mesh_validate sk_mesh_validate_delegate;
		internal static bool sk_mesh_validate (sk_mesh_t mesh, sk_string_t error) =>
			(sk_mesh_validate_delegate ??= GetSymbol<Delegates.sk_mesh_validate> ("sk_mesh_validate")).Invoke (mesh, error);
		#endif

		// sk_mesh_vertex_buffer_t* sk_mesh_vertex_buffer_copy(const sk_mesh_vertex_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy (sk_mesh_vertex_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy (sk_mesh_vertex_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy (sk_mesh_vertex_buffer_t buffer);
		}
		private static Delegates.sk_mesh_vertex_buffer_copy sk_mesh_vertex_buffer_copy_delegate;
		internal static sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy (sk_mesh_vertex_buffer_t buffer) =>
			(sk_mesh_vertex_buffer_copy_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_copy> ("sk_mesh_vertex_buffer_copy")).Invoke (buffer);
		#endif

		// size_t sk_mesh_vertex_buffer_get_size(const sk_mesh_vertex_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_mesh_vertex_buffer_get_size (sk_mesh_vertex_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_vertex_buffer_get_size (sk_mesh_vertex_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_vertex_buffer_get_size (sk_mesh_vertex_buffer_t buffer);
		}
		private static Delegates.sk_mesh_vertex_buffer_get_size sk_mesh_vertex_buffer_get_size_delegate;
		internal static /* size_t */ IntPtr sk_mesh_vertex_buffer_get_size (sk_mesh_vertex_buffer_t buffer) =>
			(sk_mesh_vertex_buffer_get_size_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_get_size> ("sk_mesh_vertex_buffer_get_size")).Invoke (buffer);
		#endif

		// sk_mesh_vertex_buffer_t* sk_mesh_vertex_buffer_make(const void* data, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (IntPtr data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_vertex_buffer_make sk_mesh_vertex_buffer_make_delegate;
		internal static sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (IntPtr data, /* size_t */ IntPtr size) =>
			(sk_mesh_vertex_buffer_make_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_make> ("sk_mesh_vertex_buffer_make")).Invoke (data, size);
		#endif

		// void sk_meshspecification_get_child_from_index(const sk_meshspecification_t* spec, int index, sk_runtimeeffect_child_t* cchild)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_get_child_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectChildNative* cchild);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_get_child_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectChildNative* cchild);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_get_child_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectChildNative* cchild);
		}
		private static Delegates.sk_meshspecification_get_child_from_index sk_meshspecification_get_child_from_index_delegate;
		internal static void sk_meshspecification_get_child_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectChildNative* cchild) =>
			(sk_meshspecification_get_child_from_index_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_child_from_index> ("sk_meshspecification_get_child_from_index")).Invoke (spec, index, cchild);
		#endif

		// void sk_meshspecification_get_child_name(const sk_meshspecification_t* spec, int index, sk_string_t* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_get_child_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_get_child_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_get_child_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		}
		private static Delegates.sk_meshspecification_get_child_name sk_meshspecification_get_child_name_delegate;
		internal static void sk_meshspecification_get_child_name (sk_meshspecification_t spec, Int32 index, sk_string_t name) =>
			(sk_meshspecification_get_child_name_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_child_name> ("sk_meshspecification_get_child_name")).Invoke (spec, index, name);
		#endif

		// size_t sk_meshspecification_get_children_size(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_meshspecification_get_children_size (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_meshspecification_get_children_size (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_meshspecification_get_children_size (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_get_children_size sk_meshspecification_get_children_size_delegate;
		internal static /* size_t */ IntPtr sk_meshspecification_get_children_size (sk_meshspecification_t spec) =>
			(sk_meshspecification_get_children_size_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_children_size> ("sk_meshspecification_get_children_size")).Invoke (spec);
		#endif

		// size_t sk_meshspecification_get_stride(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_meshspecification_get_stride (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_meshspecification_get_stride (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_meshspecification_get_stride (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_get_stride sk_meshspecification_get_stride_delegate;
		internal static /* size_t */ IntPtr sk_meshspecification_get_stride (sk_meshspecification_t spec) =>
			(sk_meshspecification_get_stride_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_stride> ("sk_meshspecification_get_stride")).Invoke (spec);
		#endif

		// size_t sk_meshspecification_get_uniform_byte_size(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_meshspecification_get_uniform_byte_size (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_meshspecification_get_uniform_byte_size (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_meshspecification_get_uniform_byte_size (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_get_uniform_byte_size sk_meshspecification_get_uniform_byte_size_delegate;
		internal static /* size_t */ IntPtr sk_meshspecification_get_uniform_byte_size (sk_meshspecification_t spec) =>
			(sk_meshspecification_get_uniform_byte_size_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_uniform_byte_size> ("sk_meshspecification_get_uniform_byte_size")).Invoke (spec);
		#endif

		// void sk_meshspecification_get_uniform_from_index(const sk_meshspecification_t* spec, int index, sk_runtimeeffect_uniform_t* cuniform)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_get_uniform_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_get_uniform_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_get_uniform_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectUniformNative* cuniform);
		}
		private static Delegates.sk_meshspecification_get_uniform_from_index sk_meshspecification_get_uniform_from_index_delegate;
		internal static void sk_meshspecification_get_uniform_from_index (sk_meshspecification_t spec, Int32 index, SKRuntimeEffectUniformNative* cuniform) =>
			(sk_meshspecification_get_uniform_from_index_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_uniform_from_index> ("sk_meshspecification_get_uniform_from_index")).Invoke (spec, index, cuniform);
		#endif

		// void sk_meshspecification_get_uniform_name(const sk_meshspecification_t* spec, int index, sk_string_t* name)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_get_uniform_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_get_uniform_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_get_uniform_name (sk_meshspecification_t spec, Int32 index, sk_string_t name);
		}
		private static Delegates.sk_meshspecification_get_uniform_name sk_meshspecification_get_uniform_name_delegate;
		internal static void sk_meshspecification_get_uniform_name (sk_meshspecification_t spec, Int32 index, sk_string_t name) =>
			(sk_meshspecification_get_uniform_name_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_uniform_name> ("sk_meshspecification_get_uniform_name")).Invoke (spec, index, name);
		#endif

		// size_t sk_meshspecification_get_uniforms_size(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial /* size_t */ IntPtr sk_meshspecification_get_uniforms_size (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_meshspecification_get_uniforms_size (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_meshspecification_get_uniforms_size (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_get_uniforms_size sk_meshspecification_get_uniforms_size_delegate;
		internal static /* size_t */ IntPtr sk_meshspecification_get_uniforms_size (sk_meshspecification_t spec) =>
			(sk_meshspecification_get_uniforms_size_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_uniforms_size> ("sk_meshspecification_get_uniforms_size")).Invoke (spec);
		#endif

		// sk_meshspecification_t* sk_meshspecification_make(const sk_meshspecification_attribute_t* attributes, size_t attributeCount, size_t vertexStride, const sk_meshspecification_varying_t* varyings, size_t varyingCount, const sk_string_t* vs, const sk_string_t* fs, sk_colorspace_t* cs, sk_alphatype_t at, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial sk_meshspecification_t sk_meshspecification_make (IntPtr attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, IntPtr varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_meshspecification_t sk_meshspecification_make (IntPtr attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, IntPtr varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_meshspecification_t sk_meshspecification_make (IntPtr attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, IntPtr varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		}
		private static Delegates.sk_meshspecification_make sk_meshspecification_make_delegate;
		internal static sk_meshspecification_t sk_meshspecification_make (IntPtr attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, IntPtr varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error) =>
			(sk_meshspecification_make_delegate ??= GetSymbol<Delegates.sk_meshspecification_make> ("sk_meshspecification_make")).Invoke (attributes, attributeCount, vertexStride, varyings, varyingCount, vs, fs, cs, at, error);
		#endif

		// void sk_meshspecification_ref(sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_ref (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_ref (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_ref (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_ref sk_meshspecification_ref_delegate;
		internal static void sk_meshspecification_ref (sk_meshspecification_t spec) =>
			(sk_meshspecification_ref_delegate ??= GetSymbol<Delegates.sk_meshspecification_ref> ("sk_meshspecification_ref")).Invoke (spec);
		#endif

		// void sk_meshspecification_unref(sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_meshspecification_unref (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_meshspecification_unref (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_meshspecification_unref (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_unref sk_meshspecification_unref_delegate;
		internal static void sk_meshspecification_unref (sk_meshspecification_t spec) =>
			(sk_meshspecification_unref_delegate ??= GetSymbol<Delegates.sk_meshspecification_unref> ("sk_meshspecification_unref")).Invoke (spec);
		#endif

		#endregion

	}
}
