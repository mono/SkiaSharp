using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_verb_t
	public enum SKPathVerb {
		// MOVE_SK_PATH_VERB = 0
		Move = 0,
		// LINE_SK_PATH_VERB = 1
		Line = 1,
		// QUAD_SK_PATH_VERB = 2
		Quad = 2,
		// CONIC_SK_PATH_VERB = 3
		Conic = 3,
		// CUBIC_SK_PATH_VERB = 4
		Cubic = 4,
		// CLOSE_SK_PATH_VERB = 5
		Close = 5,
		// DONE_SK_PATH_VERB = 6
		Done = 6,
	}
}
