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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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

		// void sk_mesh_get_bounds(const sk_mesh_t* mesh, sk_rect_t* bounds)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial void sk_mesh_get_bounds (sk_mesh_t mesh, SKRect* bounds);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_mesh_get_bounds (sk_mesh_t mesh, SKRect* bounds);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_mesh_get_bounds (sk_mesh_t mesh, SKRect* bounds);
		}
		private static Delegates.sk_mesh_get_bounds sk_mesh_get_bounds_delegate;
		internal static void sk_mesh_get_bounds (sk_mesh_t mesh, SKRect* bounds) =>
			(sk_mesh_get_bounds_delegate ??= GetSymbol<Delegates.sk_mesh_get_bounds> ("sk_mesh_get_bounds")).Invoke (mesh, bounds);
		#endif

		// size_t sk_mesh_get_index_count(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial /* size_t */ IntPtr sk_mesh_get_index_count (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_get_index_count (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_get_index_count (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_index_count sk_mesh_get_index_count_delegate;
		internal static /* size_t */ IntPtr sk_mesh_get_index_count (sk_mesh_t mesh) =>
			(sk_mesh_get_index_count_delegate ??= GetSymbol<Delegates.sk_mesh_get_index_count> ("sk_mesh_get_index_count")).Invoke (mesh);
		#endif

		// size_t sk_mesh_get_index_offset(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial /* size_t */ IntPtr sk_mesh_get_index_offset (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_get_index_offset (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_get_index_offset (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_index_offset sk_mesh_get_index_offset_delegate;
		internal static /* size_t */ IntPtr sk_mesh_get_index_offset (sk_mesh_t mesh) =>
			(sk_mesh_get_index_offset_delegate ??= GetSymbol<Delegates.sk_mesh_get_index_offset> ("sk_mesh_get_index_offset")).Invoke (mesh);
		#endif

		// bool sk_mesh_get_is_valid(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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

		// sk_mesh_mode_t sk_mesh_get_mode(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial SKMeshMode sk_mesh_get_mode (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern SKMeshMode sk_mesh_get_mode (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate SKMeshMode sk_mesh_get_mode (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_mode sk_mesh_get_mode_delegate;
		internal static SKMeshMode sk_mesh_get_mode (sk_mesh_t mesh) =>
			(sk_mesh_get_mode_delegate ??= GetSymbol<Delegates.sk_mesh_get_mode> ("sk_mesh_get_mode")).Invoke (mesh);
		#endif

		// size_t sk_mesh_get_vertex_count(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial /* size_t */ IntPtr sk_mesh_get_vertex_count (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_get_vertex_count (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_get_vertex_count (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_vertex_count sk_mesh_get_vertex_count_delegate;
		internal static /* size_t */ IntPtr sk_mesh_get_vertex_count (sk_mesh_t mesh) =>
			(sk_mesh_get_vertex_count_delegate ??= GetSymbol<Delegates.sk_mesh_get_vertex_count> ("sk_mesh_get_vertex_count")).Invoke (mesh);
		#endif

		// size_t sk_mesh_get_vertex_offset(const sk_mesh_t* mesh)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial /* size_t */ IntPtr sk_mesh_get_vertex_offset (sk_mesh_t mesh);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern /* size_t */ IntPtr sk_mesh_get_vertex_offset (sk_mesh_t mesh);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate /* size_t */ IntPtr sk_mesh_get_vertex_offset (sk_mesh_t mesh);
		}
		private static Delegates.sk_mesh_get_vertex_offset sk_mesh_get_vertex_offset_delegate;
		internal static /* size_t */ IntPtr sk_mesh_get_vertex_offset (sk_mesh_t mesh) =>
			(sk_mesh_get_vertex_offset_delegate ??= GetSymbol<Delegates.sk_mesh_get_vertex_offset> ("sk_mesh_get_vertex_offset")).Invoke (mesh);
		#endif

		// sk_mesh_index_buffer_t* sk_mesh_index_buffer_copy(const sk_mesh_index_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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

		// sk_mesh_index_buffer_t* sk_mesh_index_buffer_copy_ganesh(gr_direct_context_t* context, const sk_mesh_index_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_index_buffer_t sk_mesh_index_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_index_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_index_buffer_t sk_mesh_index_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_index_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_index_buffer_t sk_mesh_index_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_index_buffer_t buffer);
		}
		private static Delegates.sk_mesh_index_buffer_copy_ganesh sk_mesh_index_buffer_copy_ganesh_delegate;
		internal static sk_mesh_index_buffer_t sk_mesh_index_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_index_buffer_t buffer) =>
			(sk_mesh_index_buffer_copy_ganesh_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_copy_ganesh> ("sk_mesh_index_buffer_copy_ganesh")).Invoke (context, buffer);
		#endif

		// size_t sk_mesh_index_buffer_get_size(const sk_mesh_index_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_index_buffer_t sk_mesh_index_buffer_make (void* data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_index_buffer_t sk_mesh_index_buffer_make (void* data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_index_buffer_t sk_mesh_index_buffer_make (void* data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_index_buffer_make sk_mesh_index_buffer_make_delegate;
		internal static sk_mesh_index_buffer_t sk_mesh_index_buffer_make (void* data, /* size_t */ IntPtr size) =>
			(sk_mesh_index_buffer_make_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_make> ("sk_mesh_index_buffer_make")).Invoke (data, size);
		#endif

		// sk_mesh_index_buffer_t* sk_mesh_index_buffer_make_ganesh(gr_direct_context_t* context, const void* data, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_index_buffer_t sk_mesh_index_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_index_buffer_t sk_mesh_index_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_index_buffer_t sk_mesh_index_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_index_buffer_make_ganesh sk_mesh_index_buffer_make_ganesh_delegate;
		internal static sk_mesh_index_buffer_t sk_mesh_index_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size) =>
			(sk_mesh_index_buffer_make_ganesh_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_make_ganesh> ("sk_mesh_index_buffer_make_ganesh")).Invoke (context, data, size);
		#endif

		// bool sk_mesh_index_buffer_update(sk_mesh_index_buffer_t* buffer, gr_direct_context_t* context, const void* data, size_t offset, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_mesh_index_buffer_update (sk_mesh_index_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mesh_index_buffer_update (sk_mesh_index_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_mesh_index_buffer_update (sk_mesh_index_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_index_buffer_update sk_mesh_index_buffer_update_delegate;
		internal static bool sk_mesh_index_buffer_update (sk_mesh_index_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size) =>
			(sk_mesh_index_buffer_update_delegate ??= GetSymbol<Delegates.sk_mesh_index_buffer_update> ("sk_mesh_index_buffer_update")).Invoke (buffer, context, data, offset, size);
		#endif

		// sk_mesh_t* sk_mesh_make(sk_meshspecification_t* spec, const sk_mesh_make_args_t* args, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_t sk_mesh_make (sk_meshspecification_t spec, SKMeshMakeArgsNative* args, sk_string_t error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_t sk_mesh_make (sk_meshspecification_t spec, SKMeshMakeArgsNative* args, sk_string_t error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_t sk_mesh_make (sk_meshspecification_t spec, SKMeshMakeArgsNative* args, sk_string_t error);
		}
		private static Delegates.sk_mesh_make sk_mesh_make_delegate;
		internal static sk_mesh_t sk_mesh_make (sk_meshspecification_t spec, SKMeshMakeArgsNative* args, sk_string_t error) =>
			(sk_mesh_make_delegate ??= GetSymbol<Delegates.sk_mesh_make> ("sk_mesh_make")).Invoke (spec, args, error);
		#endif

		// sk_mesh_t* sk_mesh_make_indexed(sk_meshspecification_t* spec, const sk_mesh_make_indexed_args_t* args, sk_string_t* error)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_t sk_mesh_make_indexed (sk_meshspecification_t spec, SKMeshMakeIndexedArgsNative* args, sk_string_t error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_t sk_mesh_make_indexed (sk_meshspecification_t spec, SKMeshMakeIndexedArgsNative* args, sk_string_t error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_t sk_mesh_make_indexed (sk_meshspecification_t spec, SKMeshMakeIndexedArgsNative* args, sk_string_t error);
		}
		private static Delegates.sk_mesh_make_indexed sk_mesh_make_indexed_delegate;
		internal static sk_mesh_t sk_mesh_make_indexed (sk_meshspecification_t spec, SKMeshMakeIndexedArgsNative* args, sk_string_t error) =>
			(sk_mesh_make_indexed_delegate ??= GetSymbol<Delegates.sk_mesh_make_indexed> ("sk_mesh_make_indexed")).Invoke (spec, args, error);
		#endif

		// sk_mesh_vertex_buffer_t* sk_mesh_vertex_buffer_copy(const sk_mesh_vertex_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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

		// sk_mesh_vertex_buffer_t* sk_mesh_vertex_buffer_copy_ganesh(gr_direct_context_t* context, const sk_mesh_vertex_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_vertex_buffer_t buffer);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_vertex_buffer_t buffer);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_vertex_buffer_t buffer);
		}
		private static Delegates.sk_mesh_vertex_buffer_copy_ganesh sk_mesh_vertex_buffer_copy_ganesh_delegate;
		internal static sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_copy_ganesh (gr_direct_context_t context, sk_mesh_vertex_buffer_t buffer) =>
			(sk_mesh_vertex_buffer_copy_ganesh_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_copy_ganesh> ("sk_mesh_vertex_buffer_copy_ganesh")).Invoke (context, buffer);
		#endif

		// size_t sk_mesh_vertex_buffer_get_size(const sk_mesh_vertex_buffer_t* buffer)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (void* data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (void* data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (void* data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_vertex_buffer_make sk_mesh_vertex_buffer_make_delegate;
		internal static sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make (void* data, /* size_t */ IntPtr size) =>
			(sk_mesh_vertex_buffer_make_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_make> ("sk_mesh_vertex_buffer_make")).Invoke (data, size);
		#endif

		// sk_mesh_vertex_buffer_t* sk_mesh_vertex_buffer_make_ganesh(gr_direct_context_t* context, const void* data, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_vertex_buffer_make_ganesh sk_mesh_vertex_buffer_make_ganesh_delegate;
		internal static sk_mesh_vertex_buffer_t sk_mesh_vertex_buffer_make_ganesh (gr_direct_context_t context, void* data, /* size_t */ IntPtr size) =>
			(sk_mesh_vertex_buffer_make_ganesh_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_make_ganesh> ("sk_mesh_vertex_buffer_make_ganesh")).Invoke (context, data, size);
		#endif

		// bool sk_mesh_vertex_buffer_update(sk_mesh_vertex_buffer_t* buffer, gr_direct_context_t* context, const void* data, size_t offset, size_t size)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static partial bool sk_mesh_vertex_buffer_update (sk_mesh_vertex_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		[return: MarshalAs (UnmanagedType.I1)]
		internal static extern bool sk_mesh_vertex_buffer_update (sk_mesh_vertex_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			[return: MarshalAs (UnmanagedType.I1)]
			internal delegate bool sk_mesh_vertex_buffer_update (sk_mesh_vertex_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size);
		}
		private static Delegates.sk_mesh_vertex_buffer_update sk_mesh_vertex_buffer_update_delegate;
		internal static bool sk_mesh_vertex_buffer_update (sk_mesh_vertex_buffer_t buffer, gr_direct_context_t context, void* data, /* size_t */ IntPtr offset, /* size_t */ IntPtr size) =>
			(sk_mesh_vertex_buffer_update_delegate ??= GetSymbol<Delegates.sk_mesh_vertex_buffer_update> ("sk_mesh_vertex_buffer_update")).Invoke (buffer, context, data, offset, size);
		#endif

		// void sk_meshspecification_get_child_from_index(const sk_meshspecification_t* spec, int index, sk_runtimeeffect_child_t* cchild)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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

		// sk_colorspace_t* sk_meshspecification_get_colorspace(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_colorspace_t sk_meshspecification_get_colorspace (sk_meshspecification_t spec);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_colorspace_t sk_meshspecification_get_colorspace (sk_meshspecification_t spec);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_colorspace_t sk_meshspecification_get_colorspace (sk_meshspecification_t spec);
		}
		private static Delegates.sk_meshspecification_get_colorspace sk_meshspecification_get_colorspace_delegate;
		internal static sk_colorspace_t sk_meshspecification_get_colorspace (sk_meshspecification_t spec) =>
			(sk_meshspecification_get_colorspace_delegate ??= GetSymbol<Delegates.sk_meshspecification_get_colorspace> ("sk_meshspecification_get_colorspace")).Invoke (spec);
		#endif

		// size_t sk_meshspecification_get_stride(const sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
		internal static partial sk_meshspecification_t sk_meshspecification_make (SKMeshSpecificationAttributeNative* attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, SKMeshSpecificationVaryingNative* varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern sk_meshspecification_t sk_meshspecification_make (SKMeshSpecificationAttributeNative* attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, SKMeshSpecificationVaryingNative* varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate sk_meshspecification_t sk_meshspecification_make (SKMeshSpecificationAttributeNative* attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, SKMeshSpecificationVaryingNative* varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error);
		}
		private static Delegates.sk_meshspecification_make sk_meshspecification_make_delegate;
		internal static sk_meshspecification_t sk_meshspecification_make (SKMeshSpecificationAttributeNative* attributes, /* size_t */ IntPtr attributeCount, /* size_t */ IntPtr vertexStride, SKMeshSpecificationVaryingNative* varyings, /* size_t */ IntPtr varyingCount, sk_string_t vs, sk_string_t fs, sk_colorspace_t cs, SKAlphaType at, sk_string_t error) =>
			(sk_meshspecification_make_delegate ??= GetSymbol<Delegates.sk_meshspecification_make> ("sk_meshspecification_make")).Invoke (attributes, attributeCount, vertexStride, varyings, varyingCount, vs, fs, cs, at, error);
		#endif

		// void sk_meshspecification_ref(sk_meshspecification_t* spec)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
		[UnmanagedCallConv (CallConvs = new[] { typeof (CallConvCdecl) })]
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
