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
			SKShader.CreatePicture (this, tmx, tmy);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKRect tile) =>
			SKShader.CreatePicture (this, tmx, tmy, tile);

		public SKShader ToShader (SKShaderTileMode tmx, SKShaderTileMode tmy, SKMatrix localMatrix, SKRect tile) =>
			SKShader.CreatePicture (this, tmx, tmy, localMatrix, tile);

		internal static SKPicture GetObject (IntPtr ptr, bool owns = true, bool unrefExisting = true)
		{
			if (GetInstance<SKPicture> (ptr, out var instance)) {
				if (unrefExisting && instance is ISKReferenceCounted refcnt) {
#if THROW_OBJECT_EXCEPTIONS
					if (refcnt.GetReferenceCount () == 1)
						throw new InvalidOperationException (
							$"About to unreference an object that has no references. " +
							$"H: {ptr:x} Type: {instance.GetType ()}");
#endif
					refcnt.SafeUnRef ();
				}
				return instance;
			}

			return new SKPicture (ptr, owns);
		}
	}
}
