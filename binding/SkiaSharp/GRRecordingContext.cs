#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>A context for recording GPU operations that can be replayed later.</summary>
	/// <remarks />
	public unsafe class GRRecordingContext : SKObject, ISKReferenceCounted
	{
		internal GRRecordingContext (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		/// <summary>Gets the GPU backend type for this context.</summary>
		/// <value>The GPU backend type.</value>
		/// <remarks />
		public virtual GRBackend Backend {
			get {
				var result = SkiaApi.gr_recording_context_get_backend (Handle).FromNative ();
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets a value indicating whether the context has been abandoned.</summary>
		/// <value><see langword="true" /> if the context has been abandoned; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public virtual bool IsAbandoned {
			get {
				var result = SkiaApi.gr_recording_context_is_abandoned (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the maximum supported texture size.</summary>
		/// <value>The maximum texture size in pixels.</value>
		/// <remarks />
		public int MaxTextureSize {
			get {
				var result = SkiaApi.gr_recording_context_max_texture_size (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the maximum supported render target size.</summary>
		/// <value>The maximum render target size in pixels.</value>
		/// <remarks />
		public int MaxRenderTargetSize {
			get {
				var result = SkiaApi.gr_recording_context_max_render_target_size (Handle);
				GC.KeepAlive (this);
				return result;
			}
		}

		/// <summary>Gets the maximum supported sample count for a surface with the specified color type.</summary>
		/// <param name="colorType">The color type to check.</param>
		/// <returns>The maximum sample count, or 0 if the color type is not supported.</returns>
		/// <remarks />
		public int GetMaxSurfaceSampleCount (SKColorType colorType)
		{
			var result = SkiaApi.gr_recording_context_get_max_surface_sample_count_for_color_type (Handle, colorType.ToNative ());
			GC.KeepAlive (this);
			return result;
		}

		internal static GRRecordingContext GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true)
		{
			var directContext = SkiaApi.gr_recording_context_get_direct_context (handle);
			if (directContext != IntPtr.Zero) {
				return GRContext.GetObject (directContext, owns: false, unrefExisting: false);
			}

			return GetOrAddObject (handle, owns, unrefExisting, (h, o) => new GRRecordingContext (h, o));
		}
	}
}
