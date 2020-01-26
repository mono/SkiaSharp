using System;

namespace SkiaSharp
{
	public unsafe class SKPicture : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKPicture (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

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

		public SKShader ToShader (SKShaderTileMode tmx = SKShaderTileMode.Clamp, SKShaderTileMode tmy = SKShaderTileMode.Clamp) =>
			GetObject<SKShader> (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, null, null));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile) =>
			GetObject<SKShader> (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, null, &tile));

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, in SKMatrix localMatrix, SKRect tile)
		{
			fixed (SKMatrix* m = &localMatrix) {
				return GetObject<SKShader> (SkiaApi.sk_picture_make_shader (Handle, tmx, tmy, m, &tile));
			}
		}
	}
}
