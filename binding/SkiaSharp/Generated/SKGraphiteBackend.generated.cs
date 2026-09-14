using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_backend_t
	public enum SKGraphiteBackend {
		// DAWN_SK_GRAPHITE_BACKEND = 0
		Dawn = 0,
		// METAL_SK_GRAPHITE_BACKEND = 1
		Metal = 1,
		// VULKAN_SK_GRAPHITE_BACKEND = 2
		Vulkan = 2,
		// UNKNOWN_SK_GRAPHITE_BACKEND = -1
		Unknown = -1,
	}
}
