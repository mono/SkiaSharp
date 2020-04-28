using System;

namespace SkiaSharp
{
	public unsafe class SKPicture : SKObject, ISKReferenceCounted
	{
		internal SKPicture (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		public uint UniqueId =>
			SkiaApi.sk_picture_get_unique_id (Handle);

		public SKRect CullRect {
			get {
				SKRect rect;
				SkiaApi.sk_picture_get_cull_rect (Handle, &rect);
				return rect;
			}
		}

		// ToShader

		public SKShader ToShader () =>
			ToShader (SKShaderTileMode.Clamp, SKShaderTileMode.Clamp);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy) =>
			SKShader.GetObject (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, null, null));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile) =>
			SKShader.GetObject (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, null, &tile));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile) =>
			SKShader.GetObject (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, &localMatrix, &tile));

		//

		internal static SKPicture GetObject (IntPtr handle, bool owns = true, bool unrefExisting = true) =>
			GetOrAddObject (handle, owns, unrefExisting, (h, o) => new SKPicture (h, o));
	}
}
