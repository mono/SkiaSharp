using System;
using System.Diagnostics;

namespace SkiaSharp
{
	public static class TextOnPath
	{
		private static void MorphPoints (
					Span<SKPoint> dst,
					Span<SKPoint> src,
					int count,
					SKPathMeasure meas,
					in SKMatrix matrix)
		{
			for (int i = 0; i < count; i++) {
				SKPoint s = matrix.MapPoint (src[i].X, src[i].Y);

				if (!meas.GetPositionAndTangent (s.X, out var p, out var t)) {
					// set to 0 if the measure failed, so that we just set dst == pos
					t = SKPoint.Empty;
				}

				// y-offset the point in the direction of the normal vector on the path.
				dst[i] = new SKPoint (p.X - t.Y * s.Y, p.Y + t.X * s.Y);
			}
		}

		/*  TODO: Need differentially more subdivisions when the follow-path is curvy. Not sure how to
		 determine that, but we need it. I guess a cheap answer is let the caller tell us,
		 but that seems like a cop-out. Another answer is to get Skia's Rob Johnson to figure it out.
		 */
		private static void MorphPath (SKPath dst, SKPath src, SKPathMeasure meas, in SKMatrix matrix)
		{
			using var it = src.CreateIterator (false);

			SKPathVerb verb;

			Span<SKPoint> srcP = stackalloc SKPoint[4];
			Span<SKPoint> dstP = stackalloc SKPoint[4];

			while ((verb = it.Next (srcP)) != SKPathVerb.Done) {
				switch (verb) {
					case SKPathVerb.Move:
						MorphPoints (dstP, srcP, 1, meas, matrix);
						dst.MoveTo (dstP[0]);
						break;
					case SKPathVerb.Line:
						// turn lines into quads to look bendy
						srcP[0].X = (srcP[0].X + srcP[1].X) * 0.5f;
						srcP[0].Y = (srcP[0].Y + srcP[1].Y) * 0.5f;
						MorphPoints (dstP, srcP, 2, meas, matrix);
						dst.QuadTo (dstP[0], dstP[1]);
						break;
					case SKPathVerb.Quad:
						MorphPoints (dstP, srcP.Slice (1, 2), 2, meas, matrix);
						dst.QuadTo (dstP[0], dstP[1]);
						break;
					case SKPathVerb.Conic:
						MorphPoints (dstP, srcP.Slice (1, 2), 2, meas, matrix);
						dst.ConicTo (dstP[0], dstP[1], it.ConicWeight ());
						break;
					case SKPathVerb.Cubic:
						MorphPoints (dstP, srcP.Slice (1, 3), 3, meas, matrix);
						dst.CubicTo (dstP[0], dstP[1], dstP[2]);
						break;
					case SKPathVerb.Close:
						dst.Close ();
						break;
					default:
						Debug.Fail ("unknown verb");
						break;
				}
			}
		}

		/// <summary>
		/// Get a geometric path from text on a path, also warping (aka bending) the glyph geometries.
		/// </summary>
		public static SKPath GetPathFromTextWarpedOnPath (this SKFont font, ReadOnlySpan<ushort> glyphs, ReadOnlySpan<float> glyphWidths, ReadOnlySpan<SKPoint> glyphOffsets, SKPath path, float alignment)
		{
			var warpedPath = new SKPath ();

			if (glyphs.Length == 0)
				return warpedPath;

			var textLength = glyphOffsets[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];

			using var pathMeasure = new SKPathMeasure (path);

			var contourLength = pathMeasure.Length;

			var startOffset = glyphOffsets[0].X + (contourLength - textLength) * alignment;

			using var glyphPathCache = new GlyphPathCache (font);

			// TODO: Deal with multiple contours?
			for (var index = 0; index < glyphOffsets.Length; index++) {
				var glyphOffset = glyphOffsets[index];
				var gw = glyphWidths[index];
				var x0 = startOffset + glyphOffset.X;
				var x1 = x0 + gw;

				if (x1 >= 0 && x0 <= contourLength) {
					var glyphId = glyphs[index];
					var glyphPath = glyphPathCache.GetPath (glyphId);

					var transformation = SKMatrix.CreateTranslation (x0, glyphOffset.Y);
					MorphPath (warpedPath, glyphPath, pathMeasure, transformation);
				}
			}

			return warpedPath;
		}

		/// <summary>
		/// Get a text blob from placing text on a path, not warping the glyph geometries, just spacing the glyphs
		/// </summary>
		public static SKTextBlob GetBlobFromTextPlacedOnPath (
			this SKFont font,
			ReadOnlySpan<ushort> glyphs,
			ReadOnlySpan<float> glyphWidths,
			ReadOnlySpan<SKPoint> glyphOffsets,
			SKPath path,
			float alignment)
		{
			using var textBlobBuilder = new SKTextBlobBuilder ();

			if (glyphs.Length == 0)
				return textBlobBuilder.Build ();

			var glyphTransforms = Utils.RentArray<SKRotationScaleMatrix> (glyphs.Length);

			var textLength = glyphOffsets[glyphs.Length - 1].X + glyphWidths[glyphs.Length - 1];

			using var pathMeasure = new SKPathMeasure (path);

			var contourLength = pathMeasure.Length;

			var startOffset = glyphOffsets[0].X + (contourLength - textLength) * alignment;

			var firstGlyphIndex = 0;
			var pathGlyphCount = 0;

			// TODO: Deal with multiple contours?
			for (var index = 0; index < glyphOffsets.Length; index++) {
				var glyphOffset = glyphOffsets[index];
				var halfWidth = glyphWidths[index] * 0.5f;
				var pathOffset = startOffset + glyphOffset.X + halfWidth;

				// TODO: Clip glyphs on both ends of paths
				if (pathOffset >= 0 &&
					pathOffset < contourLength &&
					pathMeasure.GetPositionAndTangent (pathOffset, out var position, out var tangent)) {
					if (pathGlyphCount == 0)
						firstGlyphIndex = index;

					var tx = tangent.X;
					var ty = tangent.Y;

					var px = position.X;
					var py = position.Y;

					// Horizontally offset the position using the tangent vector
					px -= tx * halfWidth;
					py -= ty * halfWidth;

					// Vertically offset the position using the normal vector  (-ty, tx)
					var dy = glyphOffset.Y;
					px -= dy * ty;
					py += dy * tx;

					glyphTransforms.Span[pathGlyphCount++] = new SKRotationScaleMatrix (tx, ty, px, py);
				}
			}

			if (pathGlyphCount == 0)
				return textBlobBuilder.Build ();

			textBlobBuilder.AddRotationScaleRun (
				glyphs.Slice (firstGlyphIndex, pathGlyphCount),
				font,
				glyphTransforms.Span.Slice (0, pathGlyphCount));

			return textBlobBuilder.Build ();
		}
	}
}
