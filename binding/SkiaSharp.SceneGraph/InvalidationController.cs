#nullable disable

using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace SkiaSharp.SceneGraph
{
	/// <summary>Controls invalidation tracking for scene graph animations.</summary>
	/// <remarks>This class is used with Skottie animations to track which regions of the scene graph need to be redrawn. Call <see cref="M:SkiaSharp.SceneGraph.InvalidationController.Begin" /> before seeking/ticking an animation, then <see cref="M:SkiaSharp.SceneGraph.InvalidationController.End" /> afterward to capture the invalidated bounds.</remarks>
	public unsafe class InvalidationController : SKObject, ISKSkipObjectRegistration
	{
		/// <summary>Creates a new instance of <see cref="T:SkiaSharp.SceneGraph.InvalidationController" />.</summary>
		/// <remarks />
		public InvalidationController ()
			: this (SceneGraphApi.sksg_invalidation_controller_new (), true)
		{
		}

		internal InvalidationController (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Releases the native resources associated with this object.</summary>
		/// <remarks />
		protected override void DisposeNative ()
		{
			SceneGraphApi.sksg_invalidation_controller_delete (Handle);
		}

		/// <summary>Marks a rectangular region as invalidated.</summary>
		/// <param name="rect">The local-space rectangle to mark as invalidated.</param>
		/// <param name="matrix">The transformation matrix to apply to the rectangle.</param>
		/// <remarks>The rectangle is transformed by the specified matrix before being added to the accumulated invalidated bounds.</remarks>
		public unsafe void Invalidate (SKRect rect, SKMatrix matrix)
		{
			SceneGraphApi.sksg_invalidation_controller_inval (Handle, &rect, &matrix);
			GC.KeepAlive (this);
		}

		/// <summary>Gets the bounding rectangle encompassing all invalidated regions.</summary>
		/// <value>The accumulated bounds of all invalidated areas since the last <see cref="M:SkiaSharp.SceneGraph.InvalidationController.Reset" /> or construction.</value>
		/// <remarks />
		public unsafe SKRect Bounds {
			get {
				SKRect rect;
				SceneGraphApi.sksg_invalidation_controller_get_bounds (Handle, &rect);
				GC.KeepAlive (this);
				return rect;
			}
		}

		/// <summary>Begins an invalidation tracking session.</summary>
		/// <remarks>Call this method before seeking or ticking an animation to start tracking invalidated regions. Call <see cref="M:SkiaSharp.SceneGraph.InvalidationController.End" /> when finished.</remarks>
		public unsafe void Begin ()
		{
			SceneGraphApi.sksg_invalidation_controller_begin (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Ends an invalidation tracking session.</summary>
		/// <remarks>Call this method after seeking or ticking an animation to finalize the invalidated region tracking. The <see cref="P:SkiaSharp.SceneGraph.InvalidationController.Bounds" /> property will contain the accumulated invalidated area.</remarks>
		public unsafe void End ()
		{
			SceneGraphApi.sksg_invalidation_controller_end (Handle);
			GC.KeepAlive (this);
		}

		/// <summary>Clears all accumulated invalidation state.</summary>
		/// <remarks>After calling this method, <see cref="P:SkiaSharp.SceneGraph.InvalidationController.Bounds" /> will return an empty rectangle until new regions are invalidated.</remarks>
		public unsafe void Reset ()
		{
			SceneGraphApi.sksg_invalidation_controller_reset (Handle);
			GC.KeepAlive (this);
		}
	}
}
