using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_surfaceprops_flags_t
	[Flags]
	public enum SKSurfacePropsFlags {
		// NONE_SK_SURFACE_PROPS_FLAGS = 0
		None = 0,
		// USE_DEVICE_INDEPENDENT_FONTS_SK_SURFACE_PROPS_FLAGS = 1 << 0
		UseDeviceIndependentFonts = 1,
	}
}
