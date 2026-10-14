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
	/// <summary>Red-channel byte allowance, or null to exclude red from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? RedTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Green-channel byte allowance, or null to exclude green from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? GreenTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Blue-channel byte allowance, or null to exclude blue from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? BlueTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Alpha-channel byte allowance, or null to exclude alpha from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? AlphaTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Normalization output alpha type (Unpremul by default); only Unpremul or Premul is supported.</summary>
	public SKAlphaType AlphaType { get; set; } = SKAlphaType.Unpremul;

	/// <summary>Optional borrowed image whose normalized channel bytes widen the corresponding enabled channel's allowance at each pixel.</summary>
	/// <remarks>Mask alpha is an alpha-channel threshold, not opacity coverage. The mask never enables an excluded channel, marks a pixel ignored, or changes metric denominators. The caller retains and must not mutate or dispose it during the call.</remarks>
	public SKImage? ToleranceMask { get; set; }

	/// <summary>Optional inclusive rejected-pixel budget for <see cref="SKPixelComparisonResult.IsMatch"/>; null imposes no count restriction when RMS is configured.</summary>
	public SKPixelErrorBudget? ErrorBudget { get; set; }

	/// <summary>Optional inclusive maximum raw selected-channel normalized RMS error (0 through 1), independent of tolerance.</summary>
	public double? MaxNormalizedRootMeanSquaredError { get; set; }
}
