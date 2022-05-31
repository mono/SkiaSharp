using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.SceneGraph
{
	public unsafe class InvalidationController : SKObject, ISKSkipObjectRegistration
	{
		public InvalidationController ()
			: this (SceneGraphApi.sksg_invalidation_controller_new (), true)
		{
		}

		internal InvalidationController (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void DisposeNative ()
		{
			SceneGraphApi.sksg_invalidation_controller_delete (Handle);
		}

		public unsafe void Invalidate (SKRect rect, SKMatrix matrix)
		{
			SceneGraphApi.sksg_invalidation_controller_inval (Handle, &rect, &matrix);
		}

		public unsafe SKRect Bounds {
			get {
				SKRect rect;
				SceneGraphApi.sksg_invalidation_controller_get_bounds (Handle, &rect);
				return rect;
			}
		}

		public unsafe void Begin ()
		{
			SceneGraphApi.sksg_invalidation_controller_begin (Handle);
		}

		public unsafe void End ()
		{
			SceneGraphApi.sksg_invalidation_controller_end (Handle);
		}

		public unsafe void Reset ()
		{
			SceneGraphApi.sksg_invalidation_controller_reset (Handle);
		}
	}
}
