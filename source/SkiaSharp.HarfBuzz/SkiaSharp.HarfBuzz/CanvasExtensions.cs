using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SkiaSharp.HarfBuzz
{
	public static class CanvasExtensions
	{
		[Obsolete("Use DrawShapedText(string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, paint.TextAlign, paint.GetFont(), paint);

		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			canvas.DrawShapedText(text, p.X, p.Y, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public static void DrawShapedText(this SKCanvas canvas, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(text, p.X, p.Y, textAlign, font, paint);

		[Obsolete("Use DrawShapedText(string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKPaint paint) =>
			canvas.DrawShapedText(text, x, y, paint.TextAlign, paint.GetFont(), paint);

		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			canvas.DrawShapedText(text, x, y, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public static void DrawShapedText(this SKCanvas canvas, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			var shaper = GetShaper(font.Typeface);
			canvas.DrawShapedText(shaper, text, x, y, textAlign, font, paint);
			if (cacheDuration == 0)
				shaper.Dispose();
		}

		[Obsolete("Use DrawShapedText(SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, paint.TextAlign, paint.GetFont(), paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			canvas.DrawShapedText(shaper, text, p.X, p.Y, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, SKPoint p, SKTextAlign textAlign, SKFont font, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, p.X, p.Y, textAlign, font, paint);

		[Obsolete("Use DrawShapedText(SKShaper shaper, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint) instead.")]
		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKPaint paint) =>
			canvas.DrawShapedText(shaper, text, x, y, paint.TextAlign, paint.GetFont(), paint);

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKFont font, SKPaint paint) =>
#pragma warning disable CS0618 // Type or member is obsolete (TODO: replace paint.TextAlign with SKTextAlign.Left)
			canvas.DrawShapedText(shaper, text, x, y, paint.TextAlign, font, paint);
#pragma warning restore CS0618 // Type or member is obsolete

		public static void DrawShapedText(this SKCanvas canvas, SKShaper shaper, string text, float x, float y, SKTextAlign textAlign, SKFont font, SKPaint paint)
		{
			if (string.IsNullOrEmpty(text))
				return;

			if (canvas == null)
				throw new ArgumentNullException(nameof(canvas));
			if (shaper == null)
				throw new ArgumentNullException(nameof(shaper));
			if (paint == null)
				throw new ArgumentNullException(nameof(paint));

			font.Typeface = shaper.Typeface;

			// shape the text
			var result = GetShapeResult(shaper, text, font);

			// create the text blob
			using var builder = new SKTextBlobBuilder();
			var run = builder.AllocateRawPositionedRun(font, result.Codepoints.Length, null);

			// copy the glyphs
			var g = run.Glyphs;
			var p = run.Positions;
			for (var i = 0; i < result.Codepoints.Length; i++)
			{
				g[i] = (ushort)result.Codepoints[i];
				p[i] = result.Points[i];
			}

			// build
			using var textBlob = builder.Build();

			// adjust alignment
			var xOffset = 0f;
			if (textAlign != SKTextAlign.Left)
			{
				var width = result.Width;
				if (textAlign == SKTextAlign.Center)
					width *= 0.5f;
				xOffset -= width;
			}

			// draw the text
			canvas.DrawText(textBlob, x + xOffset, y, paint);

			if (clearCacheTimer is null && cacheDuration > 0)
				clearCacheTimer = new Timer(_ => ClearCache(), null, 0, cacheDuration);
		}

		private static uint cacheDuration = 0;

		public static void SetShaperCacheDuration(this SKCanvas canvas, uint milliseconds) => SetShaperCacheDuration(milliseconds);
		public static void SetShaperCacheDuration(uint milliseconds)
		{
			cacheDuration = milliseconds;

			clearCacheTimer?.Change(0, cacheDuration);
		}

		private static readonly Dictionary<int, (SKShaper shaper, DateTime cachedAt)> shaperCache = new();
		private static readonly Dictionary<int, (SKShaper.Result shapeResult, DateTime cachedAt)> shapeResultCache = new();

		private static SKShaper GetShaper(SKTypeface typeface)
		{
			if (cacheDuration == 0)
				return new SKShaper(typeface);

			var key = HashCode.Combine(typeface.FamilyName, typeface.IsBold, typeface.IsItalic);

			SKShaper shaper;
			lock (shaperCache)
			{
				shaper = shaperCache.TryGetValue(key, out var value)
					? value.shaper
					: new SKShaper(typeface);

				shaperCache[key] = (shaper, DateTime.Now);      // update timestamp
			}
			return shaper;
		}

		private static SKShaper.Result GetShapeResult(SKShaper shaper, string text, SKFont font)
		{
			if (cacheDuration == 0)
				return shaper.Shape(text, 0, 0, font);

			var key = HashCode.Combine(font.Typeface.FamilyName, font.Size, font.Typeface.IsBold, font.Typeface.IsItalic, text);

			SKShaper.Result result;
			lock (shapeResultCache)
			{
				result = shapeResultCache.TryGetValue(key, out var value)
					? value.shapeResult
					: shaper.Shape(text, 0, 0, font);

				shapeResultCache[key] = (result, DateTime.Now);             // update timestamp
			}
			return result;
		}

		private static Timer clearCacheTimer = null;

		private static void ClearCache()
		{
			var outdated = DateTime.Now - TimeSpan.FromMilliseconds(cacheDuration);

			foreach (var kv in shaperCache.AsEnumerable())
			{
				if (kv.Value.cachedAt < outdated)
				{
					if (shaperCache.Remove(kv.Key))
						kv.Value.shaper.Dispose();
				}
			}

			foreach (var kv in shapeResultCache.AsEnumerable())
			{
				if (kv.Value.cachedAt < outdated)
					shapeResultCache.Remove(kv.Key);
			}

			if ((shaperCache.Count == 0 && shapeResultCache.Count == 0) || cacheDuration == 0)
			{
				clearCacheTimer?.Dispose();
				clearCacheTimer = null;
			}
		}
	}
}
