#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>Describes the format and sampling properties of a Graphite backend texture in a backend-agnostic way.</summary>
	/// <remarks><format type="text/markdown"><![CDATA[
	/// ## Remarks
	///
	/// Create an instance from a backend-specific descriptor, such as <xref:SkiaSharp.SKGraphiteTextureInfo.CreateVulkan(SkiaSharp.SKGraphiteVkTextureInfo)>, and pass it to <xref:SkiaSharp.SKGraphiteRecorder.CreateBackendTexture(System.Int32,System.Int32,SkiaSharp.SKGraphiteTextureInfo)>.
	///
	/// This type wraps a native Skia resource and implements `IDisposable`. Dispose it when it is no longer needed.
	/// ]]></format></remarks>
	public unsafe class SKGraphiteTextureInfo : SKObject
	{
		internal SKGraphiteTextureInfo (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		/// <summary>Creates a texture info from a Vulkan texture descriptor.</summary>
		/// <param name="info">The Vulkan texture descriptor to wrap.</param>
		/// <returns>A new <see cref="T:SkiaSharp.SKGraphiteTextureInfo" />, or <see langword="null" /> if it could not be created.</returns>
		/// <remarks />
		public static SKGraphiteTextureInfo CreateVulkan (SKGraphiteVkTextureInfo info)
		{
			IntPtr handle = SkiaApi.sk_graphite_vk_texture_info_new (&info);
			return handle == IntPtr.Zero ? null : new SKGraphiteTextureInfo (handle, true);
		}

		/// <summary>Releases the native resources used by the texture info.</summary>
		/// <remarks />
		protected override void DisposeNative () =>
			SkiaApi.sk_graphite_texture_info_delete (Handle);

		/// <summary>Gets a value indicating whether the texture info describes a valid texture.</summary>
		/// <value><see langword="true" /> if the texture info is valid; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool              IsValid     => SkiaApi.sk_graphite_texture_info_is_valid (Handle);
		/// <summary>Gets the graphics backend that this texture info is for.</summary>
		/// <value>One of the enumeration values that indicates the backend.</value>
		/// <remarks />
		public SKGraphiteBackend Backend     => SkiaApi.sk_graphite_texture_info_get_backend (Handle);
		/// <summary>Gets the number of samples per pixel of the texture.</summary>
		/// <value>The sample count.</value>
		/// <remarks />
		public int               SampleCount => SkiaApi.sk_graphite_texture_info_get_sample_count (Handle);
		/// <summary>Gets a value indicating whether the texture has mipmaps.</summary>
		/// <value><see langword="true" /> if the texture has mipmaps; otherwise, <see langword="false" />.</value>
		/// <remarks />
		public bool              Mipmapped   => SkiaApi.sk_graphite_texture_info_get_mipmapped (Handle);
	}
}
