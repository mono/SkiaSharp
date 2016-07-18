﻿//
// Bindings for SKPath
//
// Author:
//   Matthew Leibowitz
//
// Copyright 2016 Xamarin Inc
//
using System;

namespace SkiaSharp
{
	public enum SkPath1DPathEffectStyle
	{
		Translate,
		Rotate,
		Morph,
	}

	public class SKPathEffect : SKObject
	{
		[Preserve]
		internal SKPathEffect (IntPtr handle)
			: base (handle)
		{
		}

		protected override void Dispose (bool disposing)
		{
			if (Handle != IntPtr.Zero && OwnsHandle) {
				SkiaApi.sk_path_effect_unref (Handle);
			}

			base.Dispose (disposing);
		}
		
		public static SKPathEffect CreateCompose(SKPathEffect outer, SKPathEffect inner)
		{
			if (outer == null)
				throw new ArgumentNullException(nameof(outer));
			if (inner == null)
				throw new ArgumentNullException(nameof(inner));
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_compose(outer.Handle, inner.Handle));
		}

		public static SKPathEffect CreateSum(SKPathEffect first, SKPathEffect second)
		{
			if (first == null)
				throw new ArgumentNullException(nameof(first));
			if (second == null)
				throw new ArgumentNullException(nameof(second));
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_sum(first.Handle, second.Handle));
		}

		public static SKPathEffect CreateDiscrete(float segLength, float deviation, UInt32 seedAssist = 0)
		{
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_discrete(segLength, deviation, seedAssist));
		}

		public static SKPathEffect CreateCorner(float radius)
		{
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_corner(radius));
		}

		public static SKPathEffect Create1DPath(SKPath path, float advance, float phase, SkPath1DPathEffectStyle style)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_1d_path(path.Handle, advance, phase, style));
		}

		public static SKPathEffect Create2DLine(float width, SKMatrix matrix)
		{
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_2d_line(width, matrix));
		}

		public static SKPathEffect Create2DPath(SKMatrix matrix, SKPath path)
		{
			if (path == null)
				throw new ArgumentNullException(nameof(path));
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_2d_path(matrix, path.Handle));
		}

		public static SKPathEffect CreateDash(float[] intervals, float phase)
		{
			if (intervals == null)
				throw new ArgumentNullException(nameof(intervals));
			return GetObject<SKPathEffect>(SkiaApi.sk_path_effect_create_dash(intervals, intervals.Length, phase));
		}

	}
}

