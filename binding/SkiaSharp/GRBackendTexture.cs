﻿using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace SkiaSharp
{
	public unsafe class GRBackendTexture : SKObject, ISKSkipObjectRegistration
	{
		internal GRBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
		public GRBackendTexture (GRGlBackendTextureDesc desc)
			: this (IntPtr.Zero, true)
		{
			var handle = desc.TextureHandle;
			if (handle.Format == 0) {
				handle.Format = desc.Config.ToGlSizedFormat ();
			}
			CreateGl (desc.Width, desc.Height, false, handle);
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
		[Obsolete ("Use GRBackendTexture(int, int, bool, GRGlTextureInfo) instead.")]
		public GRBackendTexture (GRBackendTextureDesc desc)
			: this (IntPtr.Zero, true)
		{
			var handlePtr = desc.TextureHandle;
			var oldHandle = Marshal.PtrToStructure<GRTextureInfoObsolete> (handlePtr);

			var handle = new GRGlTextureInfo (oldHandle.fTarget, oldHandle.fID, desc.Config.ToGlSizedFormat ());
			CreateGl (desc.Width, desc.Height, false, handle);
		}

		public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, mipmapped, glInfo);
		}

		public GRBackendTexture (int width, int height, GRVkImageInfo vkInfo)
			: this (IntPtr.Zero, true)
		{
			CreateVulkan (width, height, vkInfo);
		}

#if __IOS__ || __MACOS__

		public GRBackendTexture (int width, int height, bool mipmapped, GRMtlTextureInfo mtlInfo)
			: this (IntPtr.Zero, true)
		{
			var info = mtlInfo.ToNative ();
			Handle = SkiaApi.gr_backendtexture_new_metal (width, height, mipmapped, &info);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

#endif

		private void CreateGl (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
		{
			Handle = SkiaApi.gr_backendtexture_new_gl (width, height, mipmapped, &glInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		private void CreateVulkan (int width, int height, GRVkImageInfo vkInfo)
		{
			Handle = SkiaApi.gr_backendtexture_new_vulkan (width, height, &vkInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.gr_backendtexture_delete (Handle);

		public bool IsValid => SkiaApi.gr_backendtexture_is_valid (Handle);
		public int Width => SkiaApi.gr_backendtexture_get_width (Handle);
		public int Height => SkiaApi.gr_backendtexture_get_height (Handle);
		public bool HasMipMaps => SkiaApi.gr_backendtexture_has_mipmaps (Handle);
		public GRBackend Backend => SkiaApi.gr_backendtexture_get_backend (Handle).FromNative ();
		public SKSizeI Size => new SKSizeI (Width, Height);
		public SKRectI Rect => new SKRectI (0, 0, Width, Height);

		public GRGlTextureInfo GetGlTextureInfo () =>
			GetGlTextureInfo (out var info) ? info : default;

		public bool GetGlTextureInfo (out GRGlTextureInfo glInfo)
		{
			fixed (GRGlTextureInfo* g = &glInfo) {
				return SkiaApi.gr_backendtexture_get_gl_textureinfo (Handle, g);
			}
		}

		[Obsolete]
		[StructLayout (LayoutKind.Sequential)]
		internal struct GRTextureInfoObsolete
		{
			public uint fTarget;
			public uint fID;
		}
	}
}
