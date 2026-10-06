#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	/// <summary>Records drawing commands issued against Graphite-backed surfaces and snapshots them into recordings for insertion into a <see cref="T:SkiaSharp.SKGraphiteContext" />.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Create a recorder from a context with <xref:SkiaSharp.SKGraphiteContext.CreateRecorder(System.Int64)>. Draw into Graphite-backed surfaces created for the recorder, call <xref:SkiaSharp.SKGraphiteRecorder.Snap> to capture the work as a <xref:SkiaSharp.SKGraphiteRecording>, then insert and submit the recording through the context.
	///
	/// A recorder is not thread-safe; use one recorder per thread. This type wraps a native Skia resource and implements `IDisposable`.
	/// ]]></format></remarks>
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

		/// <summary>Releases the native resources used by the recorder.</summary>
		/// <remarks />
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

		/// <summary>Gets the graphics backend that this recorder uses.</summary>
		/// <value>One of the enumeration values that indicates the backend.</value>
		/// <remarks />
		public SKGraphiteBackend Backend =>
			SkiaApi.sk_graphite_recorder_get_backend (Handle);

		/// <summary>Gets the maximum texture dimension supported by the recorder's backend, in pixels.</summary>
		/// <value>The maximum texture size, in pixels.</value>
		/// <remarks />
		public int MaxTextureSize =>
			SkiaApi.sk_graphite_recorder_get_max_texture_size (Handle);

		/// <summary>Captures the commands recorded so far into a new recording and resets the recorder for further recording.</summary>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteRecording" />, or <see langword="null" /> if there was nothing to record.</returns>
		/// <remarks />
		public SKGraphiteRecording Snap ()
		{
			IntPtr handle = SkiaApi.sk_graphite_recorder_snap (Handle);
			return handle == IntPtr.Zero ? null : new SKGraphiteRecording (handle, true);
		}

		/// <summary>Creates a new backend texture that is owned by the recorder's context.</summary>
		/// <param name="width">The width of the texture, in pixels.</param>
		/// <param name="height">The height of the texture, in pixels.</param>
		/// <param name="info">The texture descriptor.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteBackendTexture" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
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

		/// <summary>Deletes a backend texture that was created by this recorder.</summary>
		/// <param name="backendTexture">The backend texture to delete.</param>
		/// <remarks />
		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_recorder_delete_backend_texture (Handle, backendTexture.Handle);
		}
	}
}
