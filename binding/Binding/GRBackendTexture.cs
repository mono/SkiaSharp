using System;

namespace SkiaSharp
{
	public class GRBackendTexture : SKObject
	{
		[Preserve]
		internal GRBackendTexture (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		public GRBackendTexture (GRGlBackendTextureDesc desc)
			: this (IntPtr.Zero, true)
		{
			var glInfo = new GRGlTextureInfo (desc.TextureHandle.Target, desc.TextureHandle.Id, desc.TextureHandle.Format);
			CreateGl (desc.Width, desc.Height, false, glInfo);
		}

		public GRBackendTexture (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
			: this (IntPtr.Zero, true)
		{
			CreateGl (width, height, mipmapped, glInfo);
		}

		private void CreateGl (int width, int height, bool mipmapped, GRGlTextureInfo glInfo)
		{
			Handle = SkiaApi.gr_backendtexture_new_gl (width, height, mipmapped, ref glInfo);

			if (Handle == IntPtr.Zero) {
				throw new InvalidOperationException ("Unable to create a new GRBackendTexture instance.");
			}
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.gr_backendtexture_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public bool IsValid => SkiaApi.gr_backendtexture_is_valid (Handle);
		public int Width => SkiaApi.gr_backendtexture_get_width (Handle);
		public int Height => SkiaApi.gr_backendtexture_get_height (Handle);
		public bool HasMipMaps => SkiaApi.gr_backendtexture_has_mipmaps (Handle);
		public GRBackend Backend => SkiaApi.gr_backendtexture_get_backend (Handle);

		public GRGlTextureInfo GetGlTextureInfo ()
		{
			if (GetGlTextureInfo (out var info))
				return info;
			return default (GRGlTextureInfo);
		}
		public bool GetGlTextureInfo (out GRGlTextureInfo glInfo)
		{
			return SkiaApi.gr_backendtexture_get_gl_textureinfo (Handle, out glInfo);
		}
	}
}
