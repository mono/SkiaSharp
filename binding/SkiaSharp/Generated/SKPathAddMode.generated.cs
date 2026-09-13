using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_add_mode_t
	/// <summary>Controls how a path is added to another path.</summary>
	/// <remarks />
	public enum SKPathAddMode {
		// APPEND_SK_PATH_ADD_MODE = 0
		/// <summary>Source path contours are added as new contours.</summary>
		Append = 0,
		// EXTEND_SK_PATH_ADD_MODE = 1
		/// <summary>The path is added by extending the last contour of the destination path with the first contour of the source path. If the last contour of the destination path is closed, then it will not be extended. Instead, the start of source path will be extended by a straight line to the end point of the destination path.</summary>
		Extend = 1,
	}
}
