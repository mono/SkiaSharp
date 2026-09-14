using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_runtimeeffect_uniform_flags_t
	[Flags]
	internal enum SKRuntimeEffectUniformFlagsNative {
		// NONE_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x00
		None = 0,
		// ARRAY_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x01
		Array = 1,
		// COLOR_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x02
		Color = 2,
		// VERTEX_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x04
		Vertex = 4,
		// FRAGMENT_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x08
		Fragment = 8,
		// HALF_PRECISION_SK_RUNTIMEEFFECT_UNIFORM_FLAGS = 0x10
		HalfPrecision = 16,
	}
}
