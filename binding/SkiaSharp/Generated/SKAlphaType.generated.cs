using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_alphatype_t
	/// <summary>Describes how to interpret the alpha component of a pixel.</summary>
	/// <remarks />
	public enum SKAlphaType {
		// UNKNOWN_SK_ALPHATYPE = 0
		/// <summary>The alpha type is uninitialized.</summary>
		Unknown = 0,
		// OPAQUE_SK_ALPHATYPE = 1
		/// <summary>All pixels are stored as opaque.</summary>
		Opaque = 1,
		// PREMUL_SK_ALPHATYPE = 2
		/// <summary><para>All pixels have their alpha premultiplied in their color components.</para><para>This is the natural format for the rendering target pixels.</para></summary>
		Premul = 2,
		// UNPREMUL_SK_ALPHATYPE = 3
		/// <summary><para>All pixels have their color components stored without any regard to the alpha. e.g. this is the default configuration for PNG images.</para><para>This alpha-type is ONLY supported for input images. Rendering cannot generate this on output.</para></summary>
		Unpremul = 3,
	}
}
