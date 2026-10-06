// Based on SkiaSharp.Extended's SKPixelComparerOptions at
// https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64
// Copyright (c) 2015-2016 Xamarin, Inc.
// Copyright (c) 2017-2020 Microsoft Corporation. Licensed under the MIT license.
#nullable enable

namespace SkiaSharp.Testing;

/// <summary>Mutable comparison configuration, snapshotted at the start of each synchronous operation.</summary>
/// <remarks>Do not mutate these settings or their borrowed mask during a call. No native input or mask ownership is transferred.</remarks>
public sealed class SKPixelComparerOptions
{
	/// <summary>Per-channel allowance and selection; defaults to strict RGBA.</summary>
	public SKPixelTolerance Tolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Normalization output alpha type (Unpremul by default); only Unpremul or Premul is supported.</summary>
	public SKAlphaType AlphaType { get; set; } = SKAlphaType.Unpremul;

	/// <summary>Optional borrowed image whose normalized channel bytes widen the corresponding enabled channel's allowance at each pixel.</summary>
	/// <remarks>Mask alpha is an alpha-channel threshold, not opacity coverage. The mask never enables an excluded channel, marks a pixel ignored, or changes metric denominators. The caller retains and must not mutate or dispose it during the call.</remarks>
	public SKImage? ToleranceMask { get; set; }

	/// <summary>Optional inclusive maximum count of rejected pixels for <see cref="SKPixelComparisonResult.IsMatch"/>.</summary>
	/// <remarks>Must be nonnegative. Budgets combine with AND; unset budgets impose no restriction.</remarks>
	public long? MaxErrorPixels { get; set; }

	/// <summary>Optional inclusive fraction of rejected pixels (0 through 1), applied as floor(total pixels × fraction).</summary>
	public double? MaxErrorPixelFraction { get; set; }

	/// <summary>Optional inclusive maximum raw selected-channel normalized RMS error (0 through 1), independent of tolerance.</summary>
	public double? MaxNormalizedRootMeanSquaredError { get; set; }
}
