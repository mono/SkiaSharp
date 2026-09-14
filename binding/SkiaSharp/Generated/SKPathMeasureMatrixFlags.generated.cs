using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pathmeasure_matrixflags_t
	/// <summary>Flags to indicate how to compute a matrix from a position along a path.</summary>
	/// <remarks>This is used with <see cref="M:SkiaSharp.SKPathMeasure.GetMatrix(System.Single,SkiaSharp.SKMatrix@,SkiaSharp.SKPathMeasureMatrixFlags)" />.</remarks>
	[Flags]
	public enum SKPathMeasureMatrixFlags {
		// GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS = 0x01
		/// <summary>Use the coordinates of the point along the path.</summary>
		GetPosition = 1,
		// GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS = 0x02
		/// <summary>Use the coordinates of the tangent along the path.</summary>
		GetTangent = 2,
		// GET_POS_AND_TAN_SK_PATHMEASURE_MATRIXFLAGS = GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS | GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS
		/// <summary>Use the coordinates of the point and the tangent along the path.</summary>
		GetPositionAndTangent = 3,
	}
}
