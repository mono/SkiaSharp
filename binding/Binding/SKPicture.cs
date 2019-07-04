using System;

namespace SkiaSharp
{
	public class SKPicture : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKPicture (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public uint UniqueId => SkiaApi.sk_picture_get_unique_id (Handle);

		public SKRect CullRect {
			get {
				SkiaApi.sk_picture_get_cull_rect (Handle, out var rect);
				return rect;
			}
		}
	}
}
