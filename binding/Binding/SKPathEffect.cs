using System;

namespace SkiaSharp
{
	public unsafe class SKPathEffect : SKObject, ISKReferenceCounted
	{
		[Preserve]
		internal SKPathEffect (IntPtr handle, bool owns)
			: base (handle, owns)
		{
		}

		// CreateCompose

		public static SKPathEffect CreateCompose (SKPathEffect outer, SKPathEffect inner)
		{
			if (outer == null)
				throw new ArgumentNullException (nameof (outer));
			if (inner == null)
				throw new ArgumentNullException (nameof (inner));

			return GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_compose (outer.Handle, inner.Handle));
		}

		// CreateSum

		public static SKPathEffect CreateSum (SKPathEffect first, SKPathEffect second)
		{
			if (first == null)
				throw new ArgumentNullException (nameof (first));
			if (second == null)
				throw new ArgumentNullException (nameof (second));

			return GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_sum (first.Handle, second.Handle));
		}

		// CreateDiscrete

		public static SKPathEffect CreateDiscrete (float segLength, float deviation, uint seedAssist = 0) =>
			GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_discrete (segLength, deviation, seedAssist));

		// CreateCorner

		public static SKPathEffect CreateCorner (float radius) =>
			GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_corner (radius));

		// Create1DPath

		public static SKPathEffect Create1DPath (SKPath path, float advance, float phase, SKPath1DPathEffectStyle style)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			return GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_1d_path (path.Handle, advance, phase, style));
		}

		// Create2DLine

		public static SKPathEffect Create2DLine (float width, SKMatrix matrix) =>
			GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_2d_line (width, &matrix));

		// Create2DPath

		public static SKPathEffect Create2DPath (SKMatrix matrix, SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException (nameof (path));

			return GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_2d_path (&matrix, path.Handle));
		}

		// CreateDash

		public static SKPathEffect CreateDash (float[] intervals, float phase) =>
			CreateDash (intervals.AsSpan (), phase);

		public static SKPathEffect CreateDash (ReadOnlySpan<float> intervals, float phase)
		{
			if (intervals.Length % 2 != 0)
				throw new ArgumentException ("The intervals must have an even number of entries.", nameof (intervals));

			fixed (float* i = intervals) {
				return GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_dash (i, intervals.Length, phase));
			}
		}

		// CreateTrim

		public static SKPathEffect CreateTrim (float start, float stop, SKTrimPathEffectMode mode = SKTrimPathEffectMode.Normal) =>
			GetObject<SKPathEffect> (SkiaApi.sk_path_effect_create_trim (start, stop, mode));
	}
}
