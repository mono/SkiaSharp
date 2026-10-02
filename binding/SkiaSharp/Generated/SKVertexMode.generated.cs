using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_vertices_vertex_mode_t
	/// <summary>Various mode with which to interpret triangles when invoking <see cref="M:SkiaSharp.SKCanvas.DrawVertices(SkiaSharp.SKVertexMode,SkiaSharp.SKPoint[],SkiaSharp.SKColor[],SkiaSharp.SKPaint)" />.</summary>
	/// <remarks />
	public enum SKVertexMode {
		// TRIANGLES_SK_VERTICES_VERTEX_MODE = 0
		/// <summary>The vertices are a triangle list.</summary>
		Triangles = 0,
		// TRIANGLE_STRIP_SK_VERTICES_VERTEX_MODE = 1
		/// <summary>The vertices are a triangle strip.</summary>
		TriangleStrip = 1,
		// TRIANGLE_FAN_SK_VERTICES_VERTEX_MODE = 2
		/// <summary>The vertices are a triangle fan.</summary>
		TriangleFan = 2,
	}
}
