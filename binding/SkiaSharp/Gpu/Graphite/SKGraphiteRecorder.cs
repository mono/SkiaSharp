#nullable disable

using System;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
#if THROW_OBJECT_EXCEPTIONS
	using GCHandle = SkiaSharp.GCHandleProxy;
#endif

	public unsafe class SKGraphiteRecorder : SKObject
	{
		private SKCanvas deferredCanvas;

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
			deferredCanvas?.Dispose ();
			deferredCanvas = null;

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

		public SKGraphiteRecording Snap ()
		{
			deferredCanvas?.Dispose ();
			deferredCanvas = null;

			IntPtr handle = SkiaApi.sk_graphite_recorder_snap (Handle);
			return handle == IntPtr.Zero ? null : new SKGraphiteRecording (handle, true);
		}

		public SKCanvas CreateDeferredCanvas (SKImageInfo info, SKGraphiteTextureInfo textureInfo)
		{
			if (textureInfo == null)
				throw new ArgumentNullException (nameof (textureInfo));
			if (info.Width <= 0)
				throw new ArgumentOutOfRangeException (
					nameof (info), info.Width, "Width must be positive.");
			if (info.Height <= 0)
				throw new ArgumentOutOfRangeException (
					nameof (info), info.Height, "Height must be positive.");
			if (info.Width > MaxTextureSize || info.Height > MaxTextureSize)
				throw new ArgumentOutOfRangeException (
					nameof (info), info.Size, "Dimensions exceed the recorder's maximum texture size.");
			if (info.ColorType == SKColorType.Unknown)
				throw new ArgumentException ("Color type must be specified.", nameof (info));
			if (info.AlphaType == SKAlphaType.Unknown)
				throw new ArgumentException ("Alpha type must be specified.", nameof (info));
			if (!textureInfo.IsValid)
				throw new ArgumentException ("Texture information must be valid.", nameof (textureInfo));
			if (textureInfo.Backend != Backend)
				throw new ArgumentException (
					"Texture information must use the recorder's backend.",
					nameof (textureInfo));

			var cinfo = SKImageInfoNative.FromManaged (ref info);
			IntPtr handle = SkiaApi.sk_graphite_recorder_make_deferred_canvas (
				Handle, &cinfo, textureInfo.Handle);
			GC.KeepAlive (textureInfo);
			GC.KeepAlive (info.ColorSpace);
			if (handle == IntPtr.Zero)
				return null;

			deferredCanvas = SKCanvas.GetObject (handle, false, unrefExisting: false);
			return deferredCanvas;
		}

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

		public void DeleteBackendTexture (SKGraphiteBackendTexture backendTexture)
		{
			if (backendTexture == null)
				throw new ArgumentNullException (nameof (backendTexture));
			SkiaApi.sk_graphite_recorder_delete_backend_texture (Handle, backendTexture.Handle);
		}
	}
}
