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

		public uint UniqueId => SkiaApi.sk_picture_get_unique_id (Handle);

		public SKRect CullRect {
			get {
				SKRect rect;
				SkiaApi.sk_picture_get_cull_rect (Handle, &rect);
				return rect;
			}
		}
	}
}
