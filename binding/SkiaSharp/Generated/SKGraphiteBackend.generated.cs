using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_graphite_backend_t
	/// <summary>Specifies the graphics backend that drives a Graphite context.</summary>
	/// <remarks />
	public enum SKGraphiteBackend {
		// DAWN_SK_GRAPHITE_BACKEND = 0
		/// <summary>The Dawn (WebGPU) backend.</summary>
		Dawn = 0,
		// METAL_SK_GRAPHITE_BACKEND = 1
		/// <summary>The Apple Metal backend.</summary>
		Metal = 1,
		// VULKAN_SK_GRAPHITE_BACKEND = 2
		/// <summary>The Vulkan backend.</summary>
		Vulkan = 2,
		// UNKNOWN_SK_GRAPHITE_BACKEND = -1
		/// <summary>An unknown or unsupported backend.</summary>
		Unknown = -1,
	}
}
