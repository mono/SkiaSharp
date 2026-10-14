// Copyright (c) Microsoft Corporation. Licensed under the MIT license.
#nullable enable

namespace SkiaSharp.Testing;

/// <summary>Mutable comparison configuration, snapshotted at the start of each synchronous operation.</summary>
/// <remarks>Do not mutate these settings or their borrowed masks during a call. No native input or mask ownership is transferred.</remarks>
public sealed class SKPixelComparerOptions
{
	/// <summary>Red-channel byte allowance, or null to exclude red from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? RedTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Green-channel byte allowance, or null to exclude green from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? GreenTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Blue-channel byte allowance, or null to exclude blue from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	public SKPixelTolerance? BlueTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Alpha-channel byte allowance, or null to exclude alpha from comparisons and statistics; defaults to <see cref="SKPixelTolerance.Exact"/>.</summary>
	/// <remarks>Excluding alpha does not undo premultiplication: when AlphaType is Premul, opacity can still affect compared RGB samples.</remarks>
	public SKPixelTolerance? AlphaTolerance { get; set; } = SKPixelTolerance.Exact;

	/// <summary>Normalization output alpha type (Unpremul by default); only Unpremul or Premul is supported.</summary>
	public SKAlphaType AlphaType { get; set; } = SKAlphaType.Unpremul;

	/// <summary>Optional borrowed image whose normalized channel bytes widen the corresponding enabled channel's allowance at each pixel.</summary>
	/// <remarks>Mask alpha is an alpha-channel threshold, not opacity coverage. The mask never enables an excluded channel, marks a pixel ignored, or changes metric denominators. The caller retains and must not mutate or dispose it during the call.</remarks>
	public SKImage? ToleranceMask { get; set; }

	/// <summary>Optional borrowed same-size image whose nonblack decoded RGB pixels exclude corresponding image coordinates from comparison.</summary>
	/// <remarks>Read as BGRA8888 Unpremul data without drawing: any nonzero RGB byte ignores the entire pixel, regardless of alpha. Black pixels remain compared. Ignored pixels contribute no errors, bounds, maxima, or metric samples; fraction budgets use remaining coverage. Both comparison and diagnostics throw ArgumentException for IgnoreMask when every pixel is ignored. The caller retains and must not mutate or dispose the mask during the call.</remarks>
	public SKImage? IgnoreMask { get; set; }

	/// <summary>Optional inclusive mismatched-pixel budget over nonignored pixels; null imposes no count restriction when RMS or MAE is configured.</summary>
	/// <remarks>All configured budget, RMS, and MAE restrictions combine with AND. With all unset, IsMatch requires zero mismatched pixels; an explicit Exact budget retains that requirement alongside magnitude limits.</remarks>
	public SKPixelMismatchBudget? MismatchBudget { get; set; }

	/// <summary>Optional inclusive maximum raw selected-channel normalized RMS error over nonignored pixels, finite and from 0 through 1.</summary>
	/// <remarks>This is a raw difference-magnitude limit, not a mismatched-pixel fraction. Increasing tolerance or mask allowances does not reduce it; null imposes no RMS restriction.</remarks>
	public double? MaxNormalizedRootMeanSquaredError { get; set; }

	/// <summary>Optional inclusive maximum raw selected-channel normalized mean absolute error, finite and from 0 through 1.</summary>
	/// <remarks>Uses RawMetrics.NormalizedMeanAbsoluteError over nonignored pixels, independently of tolerance or tolerance-mask allowances. All configured gates combine with AND; MAE alone does not require zero mismatched pixels. Null imposes no MAE restriction.</remarks>
	public double? MaxNormalizedMeanAbsoluteError { get; set; }
}
