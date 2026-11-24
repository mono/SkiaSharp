using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// gr_backend_t
	internal enum GRBackendNative {
		// OPENGL_GR_BACKEND = 0
		OpenGL = 0,
		// VULKAN_GR_BACKEND = 1
		Vulkan = 1,
		// METAL_GR_BACKEND = 2
		Metal = 2,
		// DIRECT3D_GR_BACKEND = 3
		Direct3D = 3,
		// UNSUPPORTED_GR_BACKEND = 5
		Unsupported = 5,
	}
}
