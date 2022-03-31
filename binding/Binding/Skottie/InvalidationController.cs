using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.Skottie
{
	public unsafe class InvalidationController : SKObject
	{
		public InvalidationController ()
			: this (IntPtr.Zero, true)
		{
			Handle = SkiaApi.sksg_invalidation_controller_new ();
		}

		internal InvalidationController (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// SK_C_API void sksg_invalidation_controller_inval (sksg_invalidation_controller_t* instance, sk_rect_t* rect, sk_matrix_t* matrix);
		public unsafe void Inval(SKRect rect, SKMatrix matrix)
		{
			SkiaApi.sksg_invalidation_controller_inval (Handle, &rect, &matrix);
		}

		// SK_C_API void sksg_invalidation_controller_get_bounds (sksg_invalidation_controller_t* instance, sk_rect_t* bounds);
		public unsafe SKRect Bounds
		{
			get {
				SKRect rect;
				SkiaApi.sksg_invalidation_controller_get_bounds (Handle, &rect);
				return rect;
			}
		}

		// SK_C_API void sksg_invalidation_controller_begin (sksg_invalidation_controller_t* instance);
		public unsafe void Begin ()
		{
			SkiaApi.sksg_invalidation_controller_begin (Handle);
		}

		// SK_C_API void sksg_invalidation_controller_end (sksg_invalidation_controller_t* instance);
		public unsafe void End ()
		{
			SkiaApi.sksg_invalidation_controller_end (Handle);
		}

		// SK_C_API void sksg_invalidation_controller_reset (sksg_invalidation_controller_t* instance);
		public unsafe void Reset ()
		{
			SkiaApi.sksg_invalidation_controller_reset (Handle);
		}
	}
}
