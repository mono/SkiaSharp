using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_vertices.h

		// sk_vertices_t* sk_vertices_make_copy(sk_vertices_vertex_mode_t vmode, int vertexCount, const sk_point_t* positions, const sk_point_t* texs, const sk_color_t* colors, int indexCount, const uint16_t* indices)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices);
		}
		private static Delegates.sk_vertices_make_copy sk_vertices_make_copy_delegate;
		internal static IntPtr sk_vertices_make_copy (SKVertexMode vmode, Int32 vertexCount, SKPoint* positions, SKPoint* texs, UInt32* colors, Int32 indexCount, UInt16* indices) =>
			(sk_vertices_make_copy_delegate ??= GetSymbol<Delegates.sk_vertices_make_copy> ("sk_vertices_make_copy")).Invoke (vmode, vertexCount, positions, texs, colors, indexCount, indices);
		#endif

		// void sk_vertices_ref(sk_vertices_t* cvertices)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_vertices_ref (IntPtr cvertices);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_ref (IntPtr cvertices);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_vertices_ref (IntPtr cvertices);
		}
		private static Delegates.sk_vertices_ref sk_vertices_ref_delegate;
		internal static void sk_vertices_ref (IntPtr cvertices) =>
			(sk_vertices_ref_delegate ??= GetSymbol<Delegates.sk_vertices_ref> ("sk_vertices_ref")).Invoke (cvertices);
		#endif

		// void sk_vertices_unref(sk_vertices_t* cvertices)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_vertices_unref (IntPtr cvertices);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_vertices_unref (IntPtr cvertices);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_vertices_unref (IntPtr cvertices);
		}
		private static Delegates.sk_vertices_unref sk_vertices_unref_delegate;
		internal static void sk_vertices_unref (IntPtr cvertices) =>
			(sk_vertices_unref_delegate ??= GetSymbol<Delegates.sk_vertices_unref> ("sk_vertices_unref")).Invoke (cvertices);
		#endif

		#endregion

	}
}
