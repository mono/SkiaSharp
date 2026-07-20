#nullable disable

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
			GC.KeepAlive (this);
		}

		public unsafe SKRect Bounds {
			get {
				SKRect rect;
				SceneGraphApi.sksg_invalidation_controller_get_bounds (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		public unsafe void Begin ()
		{
			SceneGraphApi.sksg_invalidation_controller_begin (Handle);
			GC.KeepAlive (this);
		}

		public unsafe void End ()
		{
			SceneGraphApi.sksg_invalidation_controller_end (Handle);
			GC.KeepAlive (this);
		}

		public unsafe void Reset ()
		{
			SceneGraphApi.sksg_invalidation_controller_reset (Handle);
			GC.KeepAlive (this);
		}
	}
}
