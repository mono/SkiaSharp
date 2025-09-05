#nullable disable

using System;

namespace SkiaSharp
{
	/// <summary>
	/// The base class for objects in the <see cref="SKPaint" /> that affect the geometry of a drawing primitive before it is transformed by the canvas' matrix and drawn.
	/// </summary>
	public unsafe class SKPathEffect : SKObject, ISKReferenceCounted
	{
		internal SKPathEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		protected override void Dispose (bool disposing) =>
			base.Dispose (disposing);

		/// <summary>
		/// Creates a compound path effect.
		/// </summary>
		/// <param name="outer">The outer (second) path effect to apply.</param>
		/// <param name="inner">The inner (first) path effect to apply.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		/// <remarks>The effect is to apply first the inner path effect and the the outer path effect (e.g. outer(inner(path))).</remarks>
		public static SKPathEffect CreateCompose(SKPathEffect outer, SKPathEffect inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject(SkiaApi.sk_path_effect_create_compose(outer.Handle, inner.Handle));
		}

		/// <summary>
		/// Creates a compound path effect.
		/// </summary>
		/// <param name="first">The first path effect to apply.</param>
		/// <param name="second">The second path effect to apply.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		/// <remarks>The effect is to apply one path after the other.</remarks>
		public static SKPathEffect CreateSum(SKPathEffect first, SKPathEffect second)
		{
			if (first == null)
				throw new ArgumentNullException(nameof(first));
			if (second == null)
				throw new ArgumentNullException(nameof(second));
			return GetObject(SkiaApi.sk_path_effect_create_sum(first.Handle, second.Handle));
		}

		/// <summary>
		/// Creates a "jitter" path effect by chopping a path into discrete segments, and randomly displacing them.
		/// </summary>
		/// <param name="segLength">The length of the segments to break the path into.</param>
		/// <param name="deviation">The maximum distance to move the point away from the original path.</param>
		/// <param name="seedAssist">The randomizer seed to use.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		public static SKPathEffect CreateDiscrete(float segLength, float deviation, UInt32 seedAssist = 0)
		{
			return GetObject(SkiaApi.sk_path_effect_create_discrete(segLength, deviation, seedAssist));
		}

		/// <summary>
		/// Creates a path effect that can turn sharp corners into various treatments (e.g. rounded corners).
		/// </summary>
		/// <param name="radius">The radius to use, must be &gt; 0 to have an effect.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		public static SKPathEffect CreateCorner(float radius)
		{
			return GetObject(SkiaApi.sk_path_effect_create_corner(radius));
		}

		/// <summary>
		/// Creates a dash path effect by replicating the specified path.
		/// </summary>
		/// <param name="path">The path to replicate (dash).</param>
		/// <param name="advance">The space between instances of path.</param>
		/// <param name="phase">The distance (mod advance) along path for its initial position.</param>
		/// <param name="style">How to transform path at each point (based on the current position and tangent).</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		public static SKPathEffect Create1DPath(SKPath path, float advance, float phase, SKPath1DPathEffectStyle style)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return GetObject(SkiaApi.sk_path_effect_create_1d_path(path.Handle, advance, phase, style));
		}

		/// <summary>
		/// Creates a dash path effect by replacing the path with a solid line.
		/// </summary>
		/// <param name="width">The width of the line.</param>
		/// <param name="matrix">The matrix.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		public static SKPathEffect Create2DLine(float width, SKMatrix matrix)
		{
			return GetObject(SkiaApi.sk_path_effect_create_2d_line(width, &matrix));
		}

		/// <summary>
		/// Stamp the specified path to fill the shape, using the matrix to define the latice.
		/// </summary>
		/// <param name="matrix">The matrix.</param>
		/// <param name="path">The path.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		public static SKPathEffect Create2DPath(SKMatrix matrix, SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return GetObject(SkiaApi.sk_path_effect_create_2d_path(&matrix, path.Handle));
		}

		/// <summary>
		/// Creates a dash path effect by specifying the dash intervals.
		/// </summary>
		/// <param name="intervals">The definition of the dash pattern via an even number of entries.</param>
		/// <param name="phase">The offset into the intervals array. (mod the sum of all of the intervals).</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		/// <remarks><para>The dash pattern is defined via an array containing an even number of entries (&gt;=2), with the even indices specifying the length of "on" intervals, and the odd indices specifying the length of "off" intervals.</para><para>For example: if the intervals = new [] { 10, 20 } and the phase = 25, then the dash pattern will be: 5 pixels off, 10 pixels on, 20 pixels off, 10 pixels on, 20 pixels off, etc. A phase of -5, 25, 55, 85, etc. would all result in the same path, because the sum of all the intervals is 30.</para></remarks>
		public static SKPathEffect CreateDash(float[] intervals, float phase)
		{
			if (intervals == null)
				throw new ArgumentNullException(nameof(intervals));
			if (intervals.Length % 2 != 0)
				throw new ArgumentException("The intervals must have an even number of entries.", nameof(intervals));
			fixed (float* i = intervals) {
				return GetObject (SkiaApi.sk_path_effect_create_dash (i, intervals.Length, phase));
			}
		}

		/// <summary>
		/// Creates a path effect that trims the path.
		/// </summary>
		/// <param name="start">The start path offset between [0, 1] - inclusive.</param>
		/// <param name="stop">The stop path offset between [0, 1] - inclusive.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		/// <remarks>If either the start or stop values are outside the [0, 1] range (inclusive), they will be pinned to the nearest legal value.</remarks>
		public static SKPathEffect CreateTrim(float start, float stop)
		{
			return CreateTrim(start, stop, SKTrimPathEffectMode.Normal);
		}

		/// <summary>
		/// Creates a path effect that trims the path.
		/// </summary>
		/// <param name="start">The start path offset between [0, 1] - inclusive.</param>
		/// <param name="stop">The stop path offset between [0, 1] - inclusive.</param>
		/// <param name="mode">The trim mode to use.</param>
		/// <returns>Returns the new <see cref="SKPathEffect" />, or null on error.</returns>
		/// <remarks>If either the start or stop values are outside the [0, 1] range (inclusive), they will be pinned to the nearest legal value.</remarks>
		public static SKPathEffect CreateTrim(float start, float stop, SKTrimPathEffectMode mode)
		{
			return GetObject(SkiaApi.sk_path_effect_create_trim(start, stop, mode));
		}

		internal static SKPathEffect GetObject (IntPtr handle) =>
			GetOrAddObject (handle, (h, o) => new SKPathEffect (h, o));
	}
}

