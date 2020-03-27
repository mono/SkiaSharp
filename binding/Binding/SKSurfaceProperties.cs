using System;
using System.ComponentModel;

namespace SkiaSharp
{
	public class SKSurfaceProperties : SKObject
	{
		[Preserve]
		internal SKSurfaceProperties (IntPtr h, bool owns)
			: base (h, owns)
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

		internal static SKSurfaceProperties GetObject (IntPtr ptr, bool owns = true, bool unrefExisting = true)
		{
			if (GetInstance<SKSurfaceProperties> (ptr, out var instance)) {
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

			return new SKSurfaceProperties (ptr, owns);
		}
	}
}
