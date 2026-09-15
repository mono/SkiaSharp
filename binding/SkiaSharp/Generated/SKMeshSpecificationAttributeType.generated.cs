using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_meshspecification_attribute_type_t
	/// <summary>Specifies the type of a per-vertex attribute declared by a mesh specification.</summary>
	/// <remarks />
	public enum SKMeshSpecificationAttributeType {
		// FLOAT_SK_MESHSPECIFICATION_ATTRIBUTE_TYPE = 0
		/// <summary>A single 32-bit floating-point value.</summary>
		Float = 0,
		// FLOAT2_SK_MESHSPECIFICATION_ATTRIBUTE_TYPE = 1
		/// <summary>Two 32-bit floating-point values.</summary>
		Float2 = 1,
		// FLOAT3_SK_MESHSPECIFICATION_ATTRIBUTE_TYPE = 2
		/// <summary>Three 32-bit floating-point values.</summary>
		Float3 = 2,
		// FLOAT4_SK_MESHSPECIFICATION_ATTRIBUTE_TYPE = 3
		/// <summary>Four 32-bit floating-point values.</summary>
		Float4 = 3,
		// UBYTE4_UNORM_SK_MESHSPECIFICATION_ATTRIBUTE_TYPE = 4
		/// <summary>Four unsigned bytes, normalized to the range 0 to 1.</summary>
		Ubyte4Unorm = 4,
	}
}
