namespace SkiaSharp.Tests.Samples.Utils;

/// <summary>Specifies dimensions and error limits for sample image comparisons.</summary>
internal sealed record SampleImageOptions
{
    /// <summary>The default allowed fraction of differing pixels: 0.075%.</summary>
    internal const decimal DefaultMaxErrorPixelFraction = 0.00075m;

    /// <summary>Gets the optional required output size. Actual and expected dimensions must always match.</summary>
    internal SKSizeI? ExpectedSize { get; init; }

    /// <summary>
    /// Gets the fraction of pixels allowed to differ by any RGBA channel value.
    /// The default is 0.00075 (0.075%). This limit applies when <see cref="MaxAverageColorErrorFraction"/> is not set.
    /// </summary>
    internal decimal MaxErrorPixelFraction { get; init; } = DefaultMaxErrorPixelFraction;

    /// <summary>
    /// Gets the optional mean absolute RGBA channel error limit, normalized by 255.
    /// When set, replaces <see cref="MaxErrorPixelFraction"/> rather than adding a second check.
    /// Small changes across many pixels can pass; use only for animated GPU smoke checks.
    /// </summary>
    internal double? MaxAverageColorErrorFraction { get; init; }
}
