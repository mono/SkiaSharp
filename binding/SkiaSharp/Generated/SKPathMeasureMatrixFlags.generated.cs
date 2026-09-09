using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_pathmeasure_matrixflags_t
	[Flags]
	public enum SKPathMeasureMatrixFlags {
		// GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS = 0x01
		GetPosition = 1,
		// GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS = 0x02
		GetTangent = 2,
		// GET_POS_AND_TAN_SK_PATHMEASURE_MATRIXFLAGS = GET_POSITION_SK_PATHMEASURE_MATRIXFLAGS | GET_TANGENT_SK_PATHMEASURE_MATRIXFLAGS
		GetPositionAndTangent = 3,
	}
}
