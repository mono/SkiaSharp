using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public class SKSurfaceProperties : SKObject
	{
		private SKSurfaceProperties (IntPtr handle, bool owns = true, bool registerHandle = true)
			: base (handle, owns, registerHandle)
		{
		}

		[EditorBrowsable (EditorBrowsableState.Never)]
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
			: base (SkiaApi.sk_surfaceprops_new (flags, pixelGeometry))
		{
		}

		public SKSurfaceProperties (SKSurfacePropsFlags flags, SKPixelGeometry pixelGeometry)
			: base (SkiaApi.sk_surfaceprops_new ((uint)flags, pixelGeometry))
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
			GetOrAddObject (handle, owns, (h, o) => new SKSurfaceProperties (h, o, false));
	}
}
