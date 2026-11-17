using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_meshspecification_varying_type_t
	/// <summary>Specifies the type of a value passed from a mesh vertex program to its fragment program.</summary>
	/// <remarks />
	public enum SKMeshSpecificationVaryingType {
		// FLOAT_SK_MESHSPECIFICATION_VARYING_TYPE = 0
		/// <summary>A single 32-bit floating-point value.</summary>
		Float = 0,
		// FLOAT2_SK_MESHSPECIFICATION_VARYING_TYPE = 1
		/// <summary>Two 32-bit floating-point values.</summary>
		Float2 = 1,
		// FLOAT3_SK_MESHSPECIFICATION_VARYING_TYPE = 2
		/// <summary>Three 32-bit floating-point values.</summary>
		Float3 = 2,
		// FLOAT4_SK_MESHSPECIFICATION_VARYING_TYPE = 3
		/// <summary>Four 32-bit floating-point values.</summary>
		Float4 = 3,
		// HALF_SK_MESHSPECIFICATION_VARYING_TYPE = 4
		/// <summary>A single 16-bit floating-point value.</summary>
		Half = 4,
		// HALF2_SK_MESHSPECIFICATION_VARYING_TYPE = 5
		/// <summary>Two 16-bit floating-point values.</summary>
		Half2 = 5,
		// HALF3_SK_MESHSPECIFICATION_VARYING_TYPE = 6
		/// <summary>Three 16-bit floating-point values.</summary>
		Half3 = 6,
		// HALF4_SK_MESHSPECIFICATION_VARYING_TYPE = 7
		/// <summary>Four 16-bit floating-point values.</summary>
		Half4 = 7,
	}
}
