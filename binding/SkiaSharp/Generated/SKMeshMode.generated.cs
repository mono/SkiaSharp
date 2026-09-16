using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_mesh_mode_t
	/// <summary>Specifies how mesh vertices are assembled into triangles.</summary>
	/// <remarks />
	public enum SKMeshMode {
		// TRIANGLES_SK_MESH_MODE = 0
		/// <summary>Every three vertices form an independent triangle.</summary>
		Triangles = 0,
		// TRIANGLE_STRIP_SK_MESH_MODE = 1
		/// <summary>Each vertex after the first two forms a triangle with the previous two.</summary>
		TriangleStrip = 1,
	}
}
