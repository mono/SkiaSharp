using System;

namespace SkiaSharp
{
	public class SKSurfaceProperties : SKObject
	{
		[Preserve]
		internal SKSurfaceProperties (IntPtr h, bool owns)
			: base (h, owns)
		{
		}

		[Obsolete]
		public SKSurfaceProperties (SKSurfaceProps props)
			: this (props.Flags, props.PixelGeometry)
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

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_surfaceprops_delete (Handle);
			}

			base.Dispose (disposing);
		}

		public SKSurfacePropsFlags Flags =>
			(SKSurfacePropsFlags)SkiaApi.sk_surfaceprops_get_flags (Handle);

		public SKPixelGeometry PixelGeometry =>
			SkiaApi.sk_surfaceprops_get_pixel_geometry (Handle);

		public bool IsUseDeviceIndependentFonts =>
			Flags.HasFlag (SKSurfacePropsFlags.UseDeviceIndependentFonts);
	}
}
