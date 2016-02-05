//
// Bindings for SKPicture
//
// Author:
//   Miguel de Icaza
//
// Copyright 2016 Xamarin Inc
//

using System;

namespace SkiaSharp
{
	public class SKPicture : SKObject
	{
		[Preserve]
		internal SKPicture (IntPtr h)
			: base (h)
		{
		}
		
		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero) {
				SkiaApi.sk_picture_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public uint UniqueId => SkiaApi.sk_picture_get_unique_id (Handle);
		public SKRect Bounds => SkiaApi.sk_picture_get_bounds (Handle);
	}
}

