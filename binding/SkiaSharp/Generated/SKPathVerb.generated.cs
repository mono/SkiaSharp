using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#region Namespaces


#endregion

namespace SkiaSharp
{

	// sk_path_verb_t
	/// <summary>Verbs contained in an <see cref="T:SkiaSharp.SKPath" />.</summary>
	/// <remarks>In the description below, the number of points returned represents the number of valid entries on the return array of points that is passed to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" />.</remarks>
	public enum SKPathVerb {
		// MOVE_SK_PATH_VERB = 0
		/// <summary>Move command, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return a single point.</summary>
		Move = 0,
		// LINE_SK_PATH_VERB = 1
		/// <summary>Line path, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return two points.</summary>
		Line = 1,
		// QUAD_SK_PATH_VERB = 2
		/// <summary>Quad command, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return three points.</summary>
		Quad = 2,
		// CONIC_SK_PATH_VERB = 3
		/// <summary>Conic path, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return three points, plus the <see cref="M:SkiaSharp.SKPath.RawIterator.ConicWeight" /> point.</summary>
		Conic = 3,
		// CUBIC_SK_PATH_VERB = 4
		/// <summary>Cubic path, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return four points.</summary>
		Cubic = 4,
		// CLOSE_SK_PATH_VERB = 5
		/// <summary>Close path, a call to <see cref="M:SkiaSharp.SKPath.Iterator.Next(SkiaSharp.SKPoint[])" /> or <see cref="M:SkiaSharp.SKPath.RawIterator.Next(SkiaSharp.SKPoint[])" /> will return one point (contour's <see cref="M:SkiaSharp.SKPath.MoveTo(SkiaSharp.SKPoint)" /> point).</summary>
		Close = 5,
		// DONE_SK_PATH_VERB = 6
		/// <summary>The path is completed, points will not contain any data.</summary>
		Done = 6,
	}
}
