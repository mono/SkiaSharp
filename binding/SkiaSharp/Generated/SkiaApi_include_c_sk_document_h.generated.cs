using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{
	internal unsafe partial class SkiaApi
	{
		#region sk_document.h

		// void sk_document_abort(sk_document_t* document)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_document_abort (IntPtr document);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_abort (IntPtr document);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_abort (IntPtr document);
		}
		private static Delegates.sk_document_abort sk_document_abort_delegate;
		internal static void sk_document_abort (IntPtr document) =>
			(sk_document_abort_delegate ??= GetSymbol<Delegates.sk_document_abort> ("sk_document_abort")).Invoke (document);
		#endif

		// sk_canvas_t* sk_document_begin_page(sk_document_t* document, float width, float height, const sk_rect_t* content)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_document_begin_page (IntPtr document, Single width, Single height, SKRect* content);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_document_begin_page (IntPtr document, Single width, Single height, SKRect* content);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_document_begin_page (IntPtr document, Single width, Single height, SKRect* content);
		}
		private static Delegates.sk_document_begin_page sk_document_begin_page_delegate;
		internal static IntPtr sk_document_begin_page (IntPtr document, Single width, Single height, SKRect* content) =>
			(sk_document_begin_page_delegate ??= GetSymbol<Delegates.sk_document_begin_page> ("sk_document_begin_page")).Invoke (document, width, height, content);
		#endif

		// void sk_document_close(sk_document_t* document)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_document_close (IntPtr document);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_close (IntPtr document);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_close (IntPtr document);
		}
		private static Delegates.sk_document_close sk_document_close_delegate;
		internal static void sk_document_close (IntPtr document) =>
			(sk_document_close_delegate ??= GetSymbol<Delegates.sk_document_close> ("sk_document_close")).Invoke (document);
		#endif

		// sk_document_t* sk_document_create_pdf_from_stream(sk_wstream_t* stream)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_document_create_pdf_from_stream (IntPtr stream);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_document_create_pdf_from_stream (IntPtr stream);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_document_create_pdf_from_stream (IntPtr stream);
		}
		private static Delegates.sk_document_create_pdf_from_stream sk_document_create_pdf_from_stream_delegate;
		internal static IntPtr sk_document_create_pdf_from_stream (IntPtr stream) =>
			(sk_document_create_pdf_from_stream_delegate ??= GetSymbol<Delegates.sk_document_create_pdf_from_stream> ("sk_document_create_pdf_from_stream")).Invoke (stream);
		#endif

		// sk_document_t* sk_document_create_pdf_from_stream_with_metadata(sk_wstream_t* stream, const sk_document_pdf_metadata_t* metadata)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_document_create_pdf_from_stream_with_metadata (IntPtr stream, SKDocumentPdfMetadataInternal* metadata);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_document_create_pdf_from_stream_with_metadata (IntPtr stream, SKDocumentPdfMetadataInternal* metadata);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_document_create_pdf_from_stream_with_metadata (IntPtr stream, SKDocumentPdfMetadataInternal* metadata);
		}
		private static Delegates.sk_document_create_pdf_from_stream_with_metadata sk_document_create_pdf_from_stream_with_metadata_delegate;
		internal static IntPtr sk_document_create_pdf_from_stream_with_metadata (IntPtr stream, SKDocumentPdfMetadataInternal* metadata) =>
			(sk_document_create_pdf_from_stream_with_metadata_delegate ??= GetSymbol<Delegates.sk_document_create_pdf_from_stream_with_metadata> ("sk_document_create_pdf_from_stream_with_metadata")).Invoke (stream, metadata);
		#endif

		// sk_document_t* sk_document_create_xps_from_stream(sk_wstream_t* stream, float dpi)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_document_create_xps_from_stream (IntPtr stream, Single dpi);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_document_create_xps_from_stream (IntPtr stream, Single dpi);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_document_create_xps_from_stream (IntPtr stream, Single dpi);
		}
		private static Delegates.sk_document_create_xps_from_stream sk_document_create_xps_from_stream_delegate;
		internal static IntPtr sk_document_create_xps_from_stream (IntPtr stream, Single dpi) =>
			(sk_document_create_xps_from_stream_delegate ??= GetSymbol<Delegates.sk_document_create_xps_from_stream> ("sk_document_create_xps_from_stream")).Invoke (stream, dpi);
		#endif

		// sk_document_t* sk_document_create_xps_from_stream_with_options(sk_wstream_t* stream, const sk_document_xps_options_t* options)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial IntPtr sk_document_create_xps_from_stream_with_options (IntPtr stream, SKDocumentXpsOptions* options);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern IntPtr sk_document_create_xps_from_stream_with_options (IntPtr stream, SKDocumentXpsOptions* options);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate IntPtr sk_document_create_xps_from_stream_with_options (IntPtr stream, SKDocumentXpsOptions* options);
		}
		private static Delegates.sk_document_create_xps_from_stream_with_options sk_document_create_xps_from_stream_with_options_delegate;
		internal static IntPtr sk_document_create_xps_from_stream_with_options (IntPtr stream, SKDocumentXpsOptions* options) =>
			(sk_document_create_xps_from_stream_with_options_delegate ??= GetSymbol<Delegates.sk_document_create_xps_from_stream_with_options> ("sk_document_create_xps_from_stream_with_options")).Invoke (stream, options);
		#endif

		// void sk_document_end_page(sk_document_t* document)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_document_end_page (IntPtr document);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_end_page (IntPtr document);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_end_page (IntPtr document);
		}
		private static Delegates.sk_document_end_page sk_document_end_page_delegate;
		internal static void sk_document_end_page (IntPtr document) =>
			(sk_document_end_page_delegate ??= GetSymbol<Delegates.sk_document_end_page> ("sk_document_end_page")).Invoke (document);
		#endif

		// void sk_document_unref(sk_document_t* document)
		#if !USE_DELEGATES
		#if USE_LIBRARY_IMPORT
		[LibraryImport (SKIA)]
		internal static partial void sk_document_unref (IntPtr document);
		#else // !USE_LIBRARY_IMPORT
		[DllImport (SKIA, CallingConvention = CallingConvention.Cdecl)]
		internal static extern void sk_document_unref (IntPtr document);
		#endif
		#else
		private partial class Delegates {
			[UnmanagedFunctionPointer (CallingConvention.Cdecl)]
			internal delegate void sk_document_unref (IntPtr document);
		}
		private static Delegates.sk_document_unref sk_document_unref_delegate;
		internal static void sk_document_unref (IntPtr document) =>
			(sk_document_unref_delegate ??= GetSymbol<Delegates.sk_document_unref> ("sk_document_unref")).Invoke (document);
		#endif

		#endregion

	}
}
