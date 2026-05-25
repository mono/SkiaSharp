#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>
	/// A short-lived recording context vended by a <see cref="SKGraphiteContext"/>.
	/// Drawing commands target a recorder; <see cref="Snap"/> produces a Recording for
	/// later submission. Single-thread-affine; multiple recorders may be used concurrently
	/// on different threads against the same parent context.
	/// </summary>
	public unsafe class SKGraphiteRecorder : SKObject
	{
		// Pin keeping the user's image-upload callback alive while Skia's FfiImageProvider
		// can dispatch into it. Freed in DisposeNative AFTER the native recorder is destroyed.
		private GCHandle pinnedImageCallback;

		// Optional cleanup hook for whatever state the callback's closure captured
		// (typically an SKGraphiteImageCache). Runs BEFORE the native recorder is
		// destroyed — graphite-backed images cached against this recorder are only
		// safe to release while the recorder is still alive.
		private Action imageCallbackDispose;

		internal SKGraphiteRecorder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal void AttachImageCallback (GCHandle pinned, Action onDispose)
		{
			pinnedImageCallback = pinned;
			imageCallbackDispose = onDispose;
		}

		protected override void DisposeNative ()
		{
			imageCallbackDispose?.Invoke ();
			imageCallbackDispose = null;

			SkiaApi.sk_graphite_recorder_delete (Handle);

			if (pinnedImageCallback.IsAllocated) {
				pinnedImageCallback.Free ();
				pinnedImageCallback = default;
			}
		}

		public SKGraphiteBackend Backend =>
			SkiaApi.sk_graphite_recorder_get_backend (Handle);

		public int MaxTextureSize =>
			SkiaApi.sk_graphite_recorder_get_max_texture_size (Handle);

		/// <summary>
		/// Produce a Recording from the work queued on this recorder. Returns null when there
		/// are no draws to snap. After being inserted via <see cref="SKGraphiteContext.InsertRecording(SKGraphiteRecording)"/>,
		/// a recording is consumed and should be disposed.
		/// </summary>
		public SKGraphiteRecording Snap ()
		{
			IntPtr handle = SkiaApi.sk_graphite_recorder_snap (Handle);
			return handle == IntPtr.Zero ? null : new SKGraphiteRecording (handle, true);
		}

		/// <summary>
		/// Allocate a fresh Skia-managed GPU texture. Pair the result with
		/// <see cref="DeleteBackendTexture"/> (or <see cref="SKGraphiteContext.DeleteBackendTexture"/>).
		/// Returns null on failure.
		/// </summary>
		public SKGraphiteBackendTexture CreateBackendTexture (int width, int height, SKGraphiteTextureInfo info)
		{
			if (info == null)
				throw new ArgumentNullException (nameof (info));
			if (width <= 0)
				throw new ArgumentOutOfRangeException (nameof (width), width, "Must be positive.");
			if (height <= 0)
				throw new ArgumentOutOfRangeException (nameof (height), height, "Must be positive.");
			IntPtr handle = SkiaApi.sk_graphite_recorder_create_backend_texture (Handle, width, height, info.Handle);
			return handle == IntPtr.Zero ? null : new SKGraphiteBackendTexture (handle, true);
		}

		/// <summary>
		/// Schedule the underlying GPU texture for release. The
		/// <see cref="SKGraphiteBackendTexture"/> wrapper itself must still
		/// be disposed separately (or via <c>using</c>).
		/// </summary>
		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_recorder_delete_backend_texture (Handle, backendTexture.Handle);
		}
	}
}
