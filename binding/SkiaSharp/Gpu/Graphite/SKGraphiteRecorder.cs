#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// A short-lived recording context vended by a <see cref="SKGraphiteContext"/>.
	/// Drawing commands target a recorder; <see cref="Snap"/> produces a Recording for
	/// later submission. Single-thread-affine; multiple recorders may be used concurrently
	/// on different threads against the same parent context.
	/// </summary>
	public unsafe class SKGraphiteRecorder : SKObject
	{
		// Optional ImageProvider attached at CreateRecorder time. Holds the managed
		// FindOrCreate delegate alive (via its GCHandle) for as long as the recorder
		// can dispatch into it. Freed in DisposeNative AFTER the native recorder is
		// destroyed (which drops the last sp ref to the underlying FfiImageProvider).
		private SKGraphiteImageProvider attachedImageProvider;

		internal SKGraphiteRecorder (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		internal void AttachImageProvider (SKGraphiteImageProvider provider)
		{
			if (provider != null) {
				provider.TransferOwnership ();
				attachedImageProvider = provider;
			}
		}

		protected override void DisposeNative ()
		{
			// Drain the provider's cache BEFORE the native recorder dies — the cached
			// graphite-backed SkImages hold sk_sp<TextureProxy>s that reference the
			// recorder's resource manager. Tearing those down post-destroy is UB.
			attachedImageProvider?.DrainCacheBeforeRecorderDispose ();

			SkiaApi.sk_graphite_recorder_delete (Handle);

			if (attachedImageProvider != null) {
				attachedImageProvider.FreeAfterContextDispose ();
				attachedImageProvider = null;
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
		/// Allocate a fresh Skia-managed GPU texture matching the supplied
		/// <paramref name="info"/>. Pair the resulting <see cref="SKGraphiteBackendTexture"/>
		/// with <see cref="DeleteBackendTexture"/> (here on the recorder, OR
		/// <see cref="SKGraphiteContext.DeleteBackendTexture"/> on the context) so
		/// Skia releases the underlying GPU resource. Returns null on failure.
		/// </summary>
		public SKGraphiteBackendTexture CreateBackendTexture (int width, int height, SKGraphiteTextureInfo info)
		{
			if (info == null)
				throw new ArgumentNullException (nameof (info));
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
