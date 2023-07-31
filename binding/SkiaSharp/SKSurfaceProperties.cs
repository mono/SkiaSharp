using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public class SKSurfaceProperties : SKObject
	{
		internal SKSurfaceProperties (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		public SKSurfaceProperties (SKPixelGeometry pixelGeometry)
			: this ((uint)0, pixelGeometry)
		{
		}

		public SKSurfaceProperties (uint flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new (flags, pixelGeometry), true)
		{
		}

		public SKSurfaceProperties (SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry)
			: this (SkiaApi.sk_surfaceprops_new ((uint)flags, pixelGeometry), true)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		protected override void DisposeNative () =>
			SkiaApi.sk_surfaceprops_delete (Handle);

		public SKSurfacePropsFlags Flags =>
			(SKSurfacePropsFlags)SkiaApi.sk_surfaceprops_get_flags (Handle);

		public SKPixelGeometry PixelGeometry =>
			SkiaApi.sk_surfaceprops_get_pixel_geometry (Handle);

		public bool IsUseDeviceIndependentFonts =>
			Flags.HasFlag (SKSurfacePropsFlags.UseDeviceIndependentFonts);

		internal static SKSurfaceProperties GetObject (IntPtr handle, bool owns = true) =>
			GetOrAddObject (handle, owns, (h, o) => new SKSurfaceProperties (h, o));
	}
}
