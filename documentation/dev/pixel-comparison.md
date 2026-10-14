# Pixel comparison API

`SkiaSharp.Testing` is a small, consumer-facing comparison API: two decoded images
in, measurements and a match decision out, with difference images generated only
when requested. It does not capture screenshots, edit images, manage baselines,
retry tests, or depend on a test framework.

The library project and its implementation live together in
`source/SkiaSharp.Testing/`. The desktop test host, shared device/browser test
library, package-integration host and Samples test host use normal project
references to this library; none compiles its implementation directly.
**They are not in `SkiaSharp.dll`. This change prepares a separate experimental NuGet
package; publishing still requires the normal release approval.**
There is no requirement to preserve the separately published Extended comparer's
API shape.

## Terminology

| Term | Meaning |
| --- | --- |
| Difference | An observed absolute channel difference before acceptance is evaluated |
| Mismatch | A compared pixel exceeds at least one enabled channel's effective allowance |
| Error | A conventional statistical measurement, such as mean absolute or squared error |
| Match | The complete comparison passes its configured policy, even if a permitted number of pixels mismatch |

The API uses mismatch names for counts, fractions, bounds and budgets,
difference names for channel magnitudes and diagnostic images, and established
error names for MAE/MSE/RMS statistics. `IsMatch` does not mean encoded-byte
identity or necessarily zero raw difference.

## MVP scope and readiness

The useful MVP is the comparison kernel below, shared by SkiaSharp and exposed
through a separate portable library for external consumers. It covers Extended's
general capabilities: decoded image inputs, pixel counts, channel differences,
tolerance maps, error statistics, and binary/color diagnostics. It improves the
defaults and units rather than preserving Extended's signatures.

| Required capability | Current implementation |
| --- | --- |
| Exact/tolerant RGB, RGBA, or individual-channel comparison | Scalar `SKPixelTolerance` on four nullable options properties |
| Per-pixel tolerance image | `ToleranceMask`, combined with global allowances |
| Explicitly excluded pixels | `IgnoreMask`, with separate compared and ignored pixel counts |
| Mismatch count, fraction, bounds and largest channel difference | `SKPixelComparisonResult` |
| Absolute/SSE, MAE/MSE/RMSE/normalized RMS/PSNR | Separate raw and thresholded metric groups |
| Whole-image acceptance | Optional count, fraction, raw-RMS and normalized raw-MAE limits |
| Diagnostic PNG-ready images | Binary mask, raw RGB channel difference, threshold overlay |
| Standalone consumer library | `source/SkiaSharp.Testing` with a separate package and experimental API contract |

Two boundaries matter. The library requires equal dimensions and readable CPU
pixels; GPU readback, screenshot targeting and resizing belong to the caller.
It measures normalized 8-bit samples, not arbitrary HDR values, perceptual
similarity, or encoded-file identity. Fonts, profiles, scale and animation
determinism must be controlled by the test producer.

The public surface is implemented, but external adoption is not yet a completed
package release or a verified replacement for every Magick input. In particular,
MAUI's pinned red-RMS conversion needs oracle fixtures. Current Samples uses the
built-in normalized raw-MAE acceptance property when its caller selects that
replacement policy. Ignore-mask support does not authorize excluded regions in
any existing consumer.

### API shape

The complete public surface is shown below. These are declaration-only
signatures, not executable implementation code; implicit constructors are
listed explicitly. Private/internal implementation members are omitted.

```csharp
namespace SkiaSharp.Testing;

/// <summary>Compares borrowed decoded inputs synchronously and creates independently owned diagnostic images.</summary>
public static class SKPixelComparer
{
    /// <summary>Compares images; null options select exact RGBA with unpremultiplied normalization.</summary>
    public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKPixelComparerOptions? options = null);
    /// <summary>Compares bitmaps without mutating them; null options select exact RGBA.</summary>
    public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options = null);
    /// <summary>Compares pixmaps respecting source row stride; null options select exact RGBA.</summary>
    public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options = null);

    /// <summary>Creates a caller-owned opaque image in the chosen style; limits do not change its pixel classification.</summary>
    public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null);
    /// <summary>Creates a caller-owned diagnostic from bitmaps; the default style is the threshold overlay.</summary>
    public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null);
    /// <summary>Creates a caller-owned diagnostic from pixmaps; the default style is the threshold overlay.</summary>
    public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelDifferenceStyle style = SKPixelDifferenceStyle.ThresholdOverlay, SKPixelComparerOptions? options = null);
}

/// <summary>Represents one channel's inclusive difference allowance, expressed in effective byte units.</summary>
public readonly struct SKPixelTolerance : IEquatable<SKPixelTolerance>
{
    /// <summary>Creates the default scalar value, equivalent to Exact.</summary>
    public SKPixelTolerance();
    /// <summary>Gets a zero allowance: the selected channel must match exactly.</summary>
    public static SKPixelTolerance Exact { get; }
    /// <summary>Creates an allowance of 0 through 255 byte levels.</summary>
    public static SKPixelTolerance Absolute(int value);
    /// <summary>Creates an allowance of 0 through 100 percentage points of the full 255 range, without integer rounding.</summary>
    public static SKPixelTolerance Percent(double value);
    /// <summary>Gets the effective allowance in byte units, including fractional values such as 1.275.</summary>
    public double Value { get; }
    /// <summary>Tests whether a finite nonnegative absolute channel difference is within this allowance.</summary>
    public bool Allows(double absoluteDifference);
    /// <summary>Tests equality of effective byte allowances, regardless of which factory created them.</summary>
    public bool Equals(SKPixelTolerance other);
    /// <summary>Tests whether the object is a scalar tolerance with the same effective allowance.</summary>
    public override bool Equals(object? obj);
    /// <summary>Returns a hash consistent with equality of effective allowances.</summary>
    public override int GetHashCode();
    /// <summary>Tests whether two scalar allowances are equal.</summary>
    public static bool operator ==(SKPixelTolerance left, SKPixelTolerance right);
    /// <summary>Tests whether two scalar allowances differ.</summary>
    public static bool operator !=(SKPixelTolerance left, SKPixelTolerance right);
}

/// <summary>Configures channel allowances, normalization and optional whole-image acceptance limits.</summary>
public sealed class SKPixelComparerOptions
{
    /// <summary>Creates exact RGBA settings, unpremultiplied normalization, no masks and no image-wide limits.</summary>
    public SKPixelComparerOptions();
    /// <summary>Gets or sets the red allowance; defaults to Exact, with null excluding red from comparison and metrics.</summary>
    public SKPixelTolerance? RedTolerance { get; set; }
    /// <summary>Gets or sets the green allowance; defaults to Exact, with null excluding green.</summary>
    public SKPixelTolerance? GreenTolerance { get; set; }
    /// <summary>Gets or sets the blue allowance; defaults to Exact, with null excluding blue.</summary>
    public SKPixelTolerance? BlueTolerance { get; set; }
    /// <summary>Gets or sets the alpha allowance; defaults to Exact, with null excluding alpha but not undoing premultiplication.</summary>
    public SKPixelTolerance? AlphaTolerance { get; set; }
    /// <summary>Gets or sets the normalized representation: Unpremul by default, or Premul; this does not select channels.</summary>
    public SKAlphaType AlphaType { get; set; }
    /// <summary>Gets or sets a borrowed same-size mask that widens enabled channel allowances; alpha is a threshold, not coverage.</summary>
    public SKImage? ToleranceMask { get; set; }
    /// <summary>Gets or sets a borrowed same-size mask whose nonzero decoded RGB excludes whole pixels; alpha is not coverage.</summary>
    public SKImage? IgnoreMask { get; set; }
    /// <summary>Gets or sets the count/percentage budget over compared pixels; all configured gates combine with AND.</summary>
    public SKPixelMismatchBudget? MismatchBudget { get; set; }
    /// <summary>Gets or sets a raw difference-magnitude limit from 0 to 1, independent of tolerance; null disables this RMS gate.</summary>
    public double? MaxNormalizedRootMeanSquaredError { get; set; }
    /// <summary>Gets or sets an inclusive raw normalized MAE limit from 0 to 1, independent of tolerance; null disables this MAE gate.</summary>
    public double? MaxNormalizedMeanAbsoluteError { get; set; }
}

/// <summary>Contains immutable measurements and a match decision without retaining native inputs.</summary>
public sealed class SKPixelComparisonResult
{
    /// <summary>Gets whether all configured limits pass, or zero pixels mismatch when no limits are configured.</summary>
    public bool IsMatch { get; }
    /// <summary>Gets the complete image area, including ignored pixels.</summary>
    public int TotalPixels { get; }
    /// <summary>Gets pixels included in comparison, including matches and tolerated differences; metric denominators use this count.</summary>
    public int ComparedPixelCount { get; }
    /// <summary>Gets pixels excluded by IgnoreMask, equal to TotalPixels minus ComparedPixelCount.</summary>
    public int IgnoredPixelCount { get; }
    /// <summary>Gets enabled channels per pixel, from one to four; identical in both metric groups.</summary>
    public int ChannelCount { get; }
    /// <summary>Gets the number of pixels exceeding at least one enabled channel's effective allowance.</summary>
    public int MismatchedPixelCount { get; }
    /// <summary>Gets mismatched pixels divided by compared pixels, as a fraction from 0 to 1.</summary>
    public double MismatchedPixelFraction { get; }
    /// <summary>Gets the largest raw difference on an enabled channel, including tolerated differences.</summary>
    public int MaxChannelDifference { get; }
    /// <summary>Gets the bounding rectangle of mismatched pixels, or null when none mismatch.</summary>
    public SKRectI? MismatchBounds { get; }
    /// <summary>Gets enabled-channel errors before tolerance filtering, excluding ignored pixels.</summary>
    public SKPixelComparisonMetrics RawMetrics { get; }
    /// <summary>Gets full exceeding channel differences with tolerated ones replaced by zero; the denominator matches RawMetrics.</summary>
    public SKPixelComparisonMetrics ThresholdedMetrics { get; }
}

/// <summary>Contains raw or thresholded error measurements using the same compared-pixel and enabled-channel denominator.</summary>
public sealed class SKPixelComparisonMetrics
{
    /// <summary>Gets total compared pixels times enabled channels; tolerated samples remain in this denominator.</summary>
    public long SampleCount { get; }
    /// <summary>Gets the 64-bit absolute-difference sum; thresholded metrics replace tolerated channel differences with zero.</summary>
    public long SumAbsoluteError { get; }
    /// <summary>Gets the 64-bit squared-difference sum; thresholded metrics replace tolerated channel differences with zero.</summary>
    public long SumSquaredError { get; }
    /// <summary>Gets SumAbsoluteError divided by SampleCount, in byte-channel units.</summary>
    public double MeanAbsoluteError { get; }
    /// <summary>Gets mean absolute error normalized to 0-1 by the 255 channel range, using the selected sample denominator.</summary>
    public double NormalizedMeanAbsoluteError { get; }
    /// <summary>Gets SumSquaredError divided by SampleCount, in squared byte-channel units.</summary>
    public double MeanSquaredError { get; }
    /// <summary>Gets the square root of MeanSquaredError, in byte-channel units.</summary>
    public double RootMeanSquaredError { get; }
    /// <summary>Gets RootMeanSquaredError divided by 255, from 0 to 1.</summary>
    public double NormalizedRootMeanSquaredError { get; }
    /// <summary>Gets PSNR in decibels, or positive infinity when this group's squared error is zero.</summary>
    public double PeakSignalToNoiseRatio { get; }
}

/// <summary>Selects the diagnostic visualization without changing numerical comparison or acceptance.</summary>
public enum SKPixelDifferenceStyle
{
    /// <summary>Uses opaque white for mismatched pixels and black for matching, tolerated or ignored pixels.</summary>
    BinaryMask = 0,
    /// <summary>Uses opaque raw RGB differences, including disabled RGB channels; ignored pixels are black and alpha is not visualized.</summary>
    ChannelDifference = 1,
    /// <summary>Uses red for mismatches, amber for tolerated differences, dimmed actual RGB for matches and neutral gray for ignored pixels.</summary>
    ThresholdOverlay = 2,
}

/// <summary>Represents an inclusive mismatched-pixel count cap, percentage cap, or both.</summary>
public readonly struct SKPixelMismatchBudget : IEquatable<SKPixelMismatchBudget>
{
    /// <summary>Creates the default budget, equivalent to Exact.</summary>
    public SKPixelMismatchBudget();
    /// <summary>Gets a budget allowing zero mismatched pixels.</summary>
    public static SKPixelMismatchBudget Exact { get; }
    /// <summary>Creates an inclusive nonnegative mismatched-pixel count cap.</summary>
    public static SKPixelMismatchBudget Pixels(long maximum);
    /// <summary>Creates a mismatched-pixel percentage cap from 0 through 100 percentage points.</summary>
    public static SKPixelMismatchBudget Percent(double maximum);
    /// <summary>Creates simultaneous count and percentage caps; both must pass.</summary>
    public static SKPixelMismatchBudget Both(long pixels, double percent);
    /// <summary>Gets the count cap, or null for a percentage-only budget.</summary>
    public long? MaxPixels { get; }
    /// <summary>Gets the cap in percentage points, or null for a count-only budget.</summary>
    public double? MaxPercent { get; }
    /// <summary>Tests valid mismatched/compared counts against the configured caps, with inclusive floor-based percentage acceptance.</summary>
    public bool Allows(long mismatchedPixels, long comparedPixels);
    /// <summary>Tests equality of configured budget caps.</summary>
    public bool Equals(SKPixelMismatchBudget other);
    /// <summary>Tests whether an object is an equal error budget.</summary>
    public override bool Equals(object? obj);
    /// <summary>Returns a hash consistent with equality of configured caps.</summary>
    public override int GetHashCode();
    /// <summary>Tests whether two budgets are equal.</summary>
    public static bool operator ==(SKPixelMismatchBudget left, SKPixelMismatchBudget right);
    /// <summary>Tests whether two budgets differ.</summary>
    public static bool operator !=(SKPixelMismatchBudget left, SKPixelMismatchBudget right);
}
```

The seven public types are `SKPixelComparer`, `SKPixelComparerOptions`,
`SKPixelTolerance`, `SKPixelComparisonResult`, `SKPixelComparisonMetrics`,
`SKPixelDifferenceStyle`, and `SKPixelMismatchBudget`. Results/metrics have no public constructors.
Settings have a normal public constructor and writable properties; comparison
snapshots them without retaining native inputs.

## Given two images

```csharp
using SkiaSharp.Testing;

var result = SKPixelComparer.Compare(expected, actual);
Assert.True(result.IsMatch);
```

The default is exact **RGBA**, including alpha, using unpremultiplied BGRA8888
normalized samples. A transparent-pixel readback is validated before drawing into
the normalized bitmap; invisible hidden RGB does not become a new difference.

All settings belong to one options object:

```csharp
var options = new SKPixelComparerOptions
{
    RedTolerance = SKPixelTolerance.Absolute(10),
    GreenTolerance = SKPixelTolerance.Absolute(20),
    BlueTolerance = SKPixelTolerance.Absolute(2),
    AlphaTolerance = SKPixelTolerance.Absolute(2),
    MismatchBudget = SKPixelMismatchBudget.Percent(0.2),
};

var result = SKPixelComparer.Compare(expected, actual, options);
Assert.True(result.IsMatch);

using var diff = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, SKPixelDifferenceStyle.ThresholdOverlay, options);
```

The same overload families accept `SKImage`, `SKBitmap`, or `SKPixmap`. There are
no positional tolerance or mask arguments, filename overloads, channels enum,
or summed-tolerance mode. In this independent testing API, `options = null`
selects the library's default settings. Difference style defaults to
`ThresholdOverlay`; use `options: options` to supply settings without a style.

## Tolerance: how different may a sample be?

`SKPixelTolerance` is an immutable **single-channel** allowance. Its default
value and `Exact` both mean zero allowance. Channel selection belongs to the four
nullable options properties, which each default to `Exact`:

```csharp
var exact = SKPixelTolerance.Exact;
var options = new SKPixelComparerOptions
{
    RedTolerance = SKPixelTolerance.Absolute(10),
    GreenTolerance = SKPixelTolerance.Percent(0.5),
    BlueTolerance = SKPixelTolerance.Exact,
    AlphaTolerance = null, // Ignore alpha.
};
var rgbOnly = new SKPixelComparerOptions { AlphaTolerance = null };
var redOnly = new SKPixelComparerOptions
{
    GreenTolerance = null, BlueTolerance = null, AlphaTolerance = null,
};
var alphaOnly = new SKPixelComparerOptions
{
    RedTolerance = null, GreenTolerance = null, BlueTolerance = null,
};
```

**Zero means exact; a `null` options property disables its channel.** Disabling
all four channels is invalid when comparison or difference generation starts.
Disabled channels contribute neither rejection nor raw/thresholded
metrics nor maximum channel difference, so red-only RMS has a one-channel denominator.
This encodes selection without another options flag or enum.

`Absolute` accepts integer byte-channel allowances from 0 through 255.
`Percent` accepts percentage points from 0 through 100, not fractions. It
converts each value to a percentage of the full channel range without rounding:
0.5% of 255 is 1.275, so a difference of 1 passes and 2 fails.
The scalar's `Value` property exposes the effective byte-unit allowance.
`ChannelCount` belongs to the comparison result, not the scalar tolerance or
individual metric groups.
There is no uniform-tolerance constructor or competing aggregate `Tolerance`
property; set the four named properties when a uniform allowance is needed.

A selected channel rejects a pixel only when its absolute difference is
**greater than** its allowance. Equality at the boundary is tolerated. A pixel
mismatches if any enabled channel exceeds its allowance. An absolute allowance is not
named `Exact(10, ...)`, since a nonzero allowance is not exact equality.

`tolerance.Allows(absoluteDifference)` owns this inclusive boundary and rejects
invalid negative/non-finite differences. A disabled channel is excluded by
options; it is not represented by an infinite tolerance.

## Limits: how much whole-image error is acceptable?

Tolerance classifies individual samples. Limits evaluate the complete
comparison. They are related but are not interchangeable.

The optional properties on `SKPixelComparerOptions` are:

| Property | Units and decision |
| --- | --- |
| `MismatchBudget` | Inclusive mismatched-pixel count, percentage, or simultaneous count and percentage caps |
| `MaxNormalizedRootMeanSquaredError` | Raw selected-channel normalized RMS, 0-1; measured value must be no greater |
| `MaxNormalizedMeanAbsoluteError` | Raw selected-channel normalized MAE, 0-1; measured value must be no greater |

All configured limits must pass. With no limits, `IsMatch` means zero mismatched
pixels under the selected tolerance. An RMS-only or MAE-only limit does not
secretly impose zero mismatched pixels.

```csharp
var count = SKPixelMismatchBudget.Pixels(20);
var percentage = SKPixelMismatchBudget.Percent(0.3); // 0.3% of compared pixels.
var both = SKPixelMismatchBudget.Both(pixels: 20, percent: 0.3);
bool allowed = both.Allows(mismatchedPixels: 3, comparedPixels: 1000);
var options = new SKPixelComparerOptions { MismatchBudget = both };
```

Count and percentage caps can differ when compared coverage changes. `Both(20, 0.3)`
allows 3 mismatched pixels in a 1,000-pixel image and 20 in a 10,000-pixel image:
the stricter cap wins. Default budget and `Exact` allow zero mismatched pixels.
Budget `Percent` uses 0-100 percentage points of compared pixels, not the 0-1
fraction used by RMS/MAE or the channel range used by tolerance percentages.
`MaxPixels` and `MaxPercent` expose the configured caps; `Allows` receives both
counts because percentage acceptance depends on the denominator. Invalid or
zero comparison coverage is rejected, not treated as a vacuous match.

Percentage budgets floor using the percentage's round-trip decimal
representation, so `Percent(0.3)` allows exactly 3 of 1,000 pixels rather than
losing a pixel to the binary representation of 0.3. The calculation also avoids
rounding count limits upward for very large denominators. Channel tolerance
still retains its effective floating-point byte threshold; it does not floor it.

`MismatchBudget = null` leaves only the configured RMS and/or MAE gates; with all
three settings unset, comparison is strict under the channel tolerances.
`MismatchBudget = Exact` explicitly requires zero mismatched pixels even alongside
RMS or MAE. Budget, RMS and MAE gates combine with AND; none changes raw measurements.

MAE is the average absolute selected-channel difference divided by 255.
`MaxNormalizedMeanAbsoluteError = 0.01` allows an average of 2.55 byte-channel
units, not 1% mismatched pixels or a per-sample allowance of 2.55. RMS gives
large differences more weight than MAE. Both limits use raw measurements;
raising tolerances does not lower them. An ignore mask changes their coverage
explicitly.

```csharp
var options = new SKPixelComparerOptions
{
    MaxNormalizedMeanAbsoluteError = 0.01,
};
var result = SKPixelComparer.Compare(expected, actual, options);
Assert.True(result.IsMatch);
```

For example, a tolerance of 2 ignores small rounding changes at any pixel. An
error budget of `Percent(0.2)` additionally allows up to 0.2% of pixels to exceed that
tolerance. This is different from allowing a per-channel difference of 0.2% of
255. Increasing tolerance does not lower the raw RMS measurement.

Negative, out-of-range, NaN, and infinite settings are invalid. The result's
PSNR may legitimately be positive infinity when squared error is zero.

## Measurements and diagnostics

`SKPixelComparisonResult` provides:

- `IsMatch`, full-image `TotalPixels`, `ComparedPixelCount`, `IgnoredPixelCount`,
  and `ChannelCount`.
- `MismatchedPixelCount` and `MismatchedPixelFraction`, with an explicit 0-1 fraction.
- Raw selected-channel `MaxChannelDifference`, including tolerated differences.
- `MismatchBounds`, the bounding rectangle of mismatched pixels, or `null` when none.
- `RawMetrics` and `ThresholdedMetrics`.

Each `SKPixelComparisonMetrics` group has wide absolute/SSE numerators and
MAE, MSE, RMSE, normalized RMSE, and PSNR with a selected-channel denominator.
Raw metrics include every selected normalized difference at nonignored pixels
before tolerance.
Thresholded metrics include only individual channel differences that exceed
their thresholds, not the difference minus the allowance. Both groups divide by
all compared pixels times enabled channels.

Changing only tolerance leaves `RawMetrics` unchanged. This prevents a tolerated
image from being reported as having zero raw RMS merely because the classifier
ignored its differences. Result and metric objects have controlled construction
and retain no native inputs.

Pixel coverage and `ChannelCount` are exposed once on the comparison result.
Both metric groups have the same `SampleCount`,
`ComparedPixelCount * ChannelCount`. Without an ignore mask,
`ComparedPixelCount == TotalPixels`. For 100 compared pixels
with four enabled channels, both groups divide by 400,
even if only one channel sample exceeds tolerance. Thresholded MAE/MSE measure
remaining error across the whole image, not average severity among mismatched
samples. No member reports the count of exceeding channel samples.

### Helpers that avoid manual arithmetic

`SampleCount` already contains compared pixels times enabled channels, so metrics
do not need redundant `TotalPixels` or `ChannelCount` properties to calculate
averages. Prefer the existing get-only measurements:

| Need | Member |
| --- | --- |
| Mean channel difference in byte units | `metrics.MeanAbsoluteError` |
| Mean channel difference normalized to 0-1 | `metrics.NormalizedMeanAbsoluteError` |
| RMS magnitude in byte units | `metrics.RootMeanSquaredError` |
| RMS magnitude normalized to 0-1 | `metrics.NormalizedRootMeanSquaredError` |
| Share of pixels exceeding tolerance | `result.MismatchedPixelFraction` |
| Complete acceptance decision | `result.IsMatch` |

The new normalized-MAE helper calculates
`SumAbsoluteError / (SampleCount * 255.0)`, with zero for zero samples. It
automatically handles RGB, RGBA or individual channels and avoids caller-side
hard-coded `4` and `255`. `SampleImage` uses it for reporting and configures the
built-in raw-MAE acceptance gate; raw versus thresholded measurement remains
explicit.

Possible later shortcuts include normalized MSE, maximum channel difference
normalized to 0-1, or mismatch percentage in 0-100 units. Add them only when a
caller needs them; prefer explicit units and derived getters over duplicate
metadata or a generic calculation framework. A percentage helper would not
change the existing 0-1 fraction's meaning.

```csharp
using var binary = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, SKPixelDifferenceStyle.BinaryMask, options);
using var overlay = SKPixelComparer.GenerateDifferenceImage(expected, actual, options: options);
using var difference = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, SKPixelDifferenceStyle.ChannelDifference, options);
```

The default difference image is `ThresholdOverlay`, so alpha-only rejections are
visible. Red means mismatched, amber means a selected difference within tolerance,
and matching pixels show dimmed actual RGB. Ignored pixels are opaque neutral
gray (128, 128, 128). `BinaryMask` uses white/black, with ignored pixels black.
`ChannelDifference` is an opaque raw RGB diagnostic, including RGB channels
disabled in classification. Ignored pixels are black; it does not claim to
display alpha-only changes.

Limits affect `IsMatch`, not which pixels are colored red. Every comparison and
binary/overlay diagnostic uses the same classification settings.

## Tolerance maps and representation

```csharp
var options = new SKPixelComparerOptions
{
    RedTolerance = SKPixelTolerance.Absolute(2),
    GreenTolerance = SKPixelTolerance.Absolute(2),
    BlueTolerance = SKPixelTolerance.Absolute(2),
    AlphaTolerance = SKPixelTolerance.Absolute(2),
    ToleranceMask = toleranceMask,
};

var result = SKPixelComparer.Compare(expected, actual, options);
```

The borrowed mask must have the same dimensions as the inputs. For each enabled
channel it can widen, but not tighten, the global allowance:
`max(global allowance, normalized mask channel)`. It cannot re-enable a
`null`-disabled channel.

This is not an excluded-pixel mask: tolerated pixels remain in both metric
denominators. Mask alpha is a channel threshold, not coverage; an opaque mask's
alpha value of 255 tolerates all alpha differences there.

### Explicit ignore coverage

`IgnoreMask` is a separate borrowed, same-size image. It is read as
unpremultiplied BGRA8888 **data**, without drawing or compositing it over a
background. Comparison `AlphaType` does not change this mask's interpretation.
Black RGB means compare; any nonzero red, green or blue byte means ignore the
entire pixel. White is the usual ignore value. Mask alpha does not independently
mark coverage, and comparison channel selection does not disable mask channels.
Use opaque black/white masks to make their content easy to inspect.

```csharp
var options = new SKPixelComparerOptions
{
    IgnoreMask = ignoreMask, // Caller-owned, same dimensions as expected/actual.
    MismatchBudget = SKPixelMismatchBudget.Percent(0.3),
};
var result = SKPixelComparer.Compare(expected, actual, options);
Assert.Equal(result.TotalPixels,
    result.ComparedPixelCount + result.IgnoredPixelCount);
```

Mask coordinates correspond directly to image coordinates: no resizing,
cropping, interpolation or alignment. Ignored pixels contribute neither raw nor
thresholded error, mismatches, maximum channel difference, nor mismatch bounds.
Both metric denominators and percentage budgets use only `ComparedPixelCount`.
For example, ignoring 900 pixels in a 1,000-pixel image leaves 100 compared
pixels; a 1% budget permits one mismatch, not ten.

`TotalPixels` remains the full image area. A mask excluding every pixel is an
`ArgumentException` in both comparison and difference generation, never a
successful empty comparison. Invalid/disposed masks, wrong dimensions and failed
readback also fail explicitly. Null or all-black masks retain existing results
and diagnostics. Tolerance and ignore masks can be supplied together; tolerance
applies only at included pixels. Results retain neither mask.

`AlphaType` defaults to `SKAlphaType.Unpremul` and also permits
`SKAlphaType.Premul`. Representation and channel selection are independent:
RGB-only can still compare premultiplied RGB bytes.

Inputs and masks are borrowed and must remain alive and unmodified for each
synchronous call. Options are snapshotted at entry. Numeric results own no native
resources; returned difference images are caller-owned and survive disposal of
temporary normalization buffers. Invalid/disposed/empty/unreadable inputs,
inconsistent sizes, and invalid settings fail explicitly.

## Existing consumer policies

The new defaults do not silently change the owning harnesses' established gates.

| Consumer | Explicit settings and gate |
| --- | --- |
| Core bitmap assertion | `AlphaTolerance = null`, other channels exact, premultiplied; external strict fraction comparison `< 10^-precision` |
| Renderer goldens | Four `Absolute(ChannelTolerance)` properties, premultiplied, configured maximum error fraction with the same floor-based budget |
| Package integration | Exact RGB, premultiplied; existing crop/resize adapter and similarity at least 95% remain |
| Samples | Exact RGBA, premultiplied; decimal pixel fraction 0.00075 by default, or the caller-selected normalized raw-MAE replacement gate |
| MAUI Graphics / Resizetizer | Exact RGB, legacy representation; fraction limits 0.07 / 0.27 |
| MAUI screenshot metric | Exact red only, raw normalized RMS limit 0.005; verify pinned Magick Q8 conversion parity before replacement |

```csharp
var result = SKPixelComparer.Compare(expected, actual,
    new SKPixelComparerOptions
    {
        GreenTolerance = null,
        BlueTolerance = null,
        AlphaTolerance = null,
        MaxNormalizedRootMeanSquaredError = 0.005,
    });
```

This expresses the
[actual MAUI red-channel RMS call](https://github.com/dotnet/maui/blob/7a5a5d610cc056e6fbd0f3dfe3930ea6de1f28bc/src/TestUtils/src/VisualTestUtils.MagickNet/MagickNetVisualComparer.cs).
It is not a claim of general ImageMagick parity. Exception-message percentage
parsing, screenshot preparation, and baseline acceptance stay outside this API.

### Current SampleImage and browser/Appium boundaries

Canonical `SampleImage` now accepts files or saved PNG bytes plus
`SampleImageOptions`. It validates optional expected size and always requires
actual/reference dimensions to match. Its default is exact premultiplied RGBA
with a decimal `MaxErrorPixelFraction` of `0.00075m`. The adapter keeps that
decimal count gate rather than translating it through a binary-floating fraction.

When `MaxAverageColorErrorFraction` is set, it **replaces** the pixel-fraction
gate. It is normalized raw MAE:

```csharp
var comparisonOptions = new SKPixelComparerOptions
{
    AlphaType = SKAlphaType.Premul,
    MaxNormalizedMeanAbsoluteError = 0.06,
};
var result = SKPixelComparer.Compare(expected, actual, comparisonOptions);
Assert.True(result.IsMatch);
```

It is neither NRMSE nor the share of mismatched pixels. Browser animated-GPU smoke
checks currently use 0.06; a broad, slight change can pass this criterion while
many pixels differ. Do not substitute the RMS limit or add the pixel-count gate
as a second requirement. The MAE branch uses `result.IsMatch`; the default
fraction branch retains its original decimal count comparison. Existing
infrastructure tests cover both policies.
`CountPixels` is an independent region/content assertion, not a golden metric.

Browser capture uses full-page PNGs with device scale factor 1 and OS-specific
references. These are not interchangeable with GPU-surface or element captures.
The comparer owns neither browser timing nor HTML/UI stabilization.

The [published Appium sample work](https://github.com/mono/SkiaSharp/pull/5338)
was inspected at commit `737646a6b82a929fb25768403c39627ad33cbd6b`.
It captures target elements through Appium and stores references by application
family, platform and device profile. Its `SampleImage` adapter additionally
re-captures within a bounded timeout, distinguishes dimension mismatches while
waiting, and emits final failure attachments. Its explicit reference-capture
mode records candidates, not a successful baseline validation.
Some animated pages capture diagnostics without golden comparison.
These are consumer responsibilities, not new comparer overloads.

The current Appium contract uses zero channel tolerance, matching dimensions
and the same strict 0.075% differing-pixel budget. No crop, mask or relaxed
budget is authorized by that contract. An OS home-indicator or other system-UI
change can therefore legitimately fail the capture comparison. Future ignored
regions require a separate consumer decision and coverage review; they are not
a workaround to turn those current failures green.

Neither this branch nor that reference requires a general screenshot editor in
the kernel. Any future crop/scale/background-composite helpers should be separate,
explicit operations with recorded preparation metadata. Keep raw screenshot,
prepared actual, reference, options and diff associated with the same attempt;
never retry only the comparison against an unrelated earlier capture.

## Library, tests and experimental package

[`source/SkiaSharp.Testing`](../../source/SkiaSharp.Testing/SkiaSharp.Testing.csproj)
owns the library and all seven public types. It follows the normal source
project conventions: shared `source/Directory.Build.*`, `BasicTargetFrameworks`,
signing, compiler XML/reference assemblies, and a normal project reference to the
local SkiaSharp binding. There are no project-local build overrides, conditional
package/binding dependency modes, or shared-source manifests.

[`tests/SkiaSharp.Testing.Tests`](../../tests/SkiaSharp.Testing.Tests/SkiaSharp.Testing.Tests.csproj)
is its dedicated signed xUnit/Microsoft.Testing.Platform host. Metrics, acceptance
budgets, tolerance, ignore coverage, diagnostics, normalization and validation
have focused test files, with one shared fixture helper. They are not compiled
into the main SkiaSharp test assembly. Internal construction is available only
to this friend test assembly. The host participates in the normal unfiltered
desktop test solution and can also be built independently.

```bash
dotnet build source/SkiaSharp.Testing/SkiaSharp.Testing.csproj
dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

The distribution is a real **`SkiaSharp.Testing` NuGet package**, separate from
`SkiaSharp.dll` and Extended. It contains the helper assembly, reference assembly
and compiler documentation, not a test runner or native binary. Its only product
dependency is the corresponding SkiaSharp package; test frameworks, HarfBuzz,
Magick.NET, Appium and Playwright are not library dependencies.

### Experimental API contract

The package follows the normal SkiaSharp family version, including its existing
preview, RC or PR/build suffix. For example, stable `SkiaSharp 4.156.0` produces
`SkiaSharp.Testing 4.156.0`; there is no separate preview counter or versioning
exception. The **API**, not the package version, is marked experimental:

```csharp
[assembly: Experimental("SKIA5001")]
```

The assembly-level attribute covers all seven public types. Supported C#
compilers report `SKIA5001` when consumers use this evolving API. Opt in
explicitly in the consuming project:

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);SKIA5001</NoWarn>
</PropertyGroup>
```

Repository test consumers already include that specific opt-in. It does not
disable other diagnostics or require a preview SDK, runtime, language version,
or `<EnablePreviewFeatures>`. For framework targets before .NET 8, an internal
compatibility attribute emits the same experimental metadata without adding a
public helper type or package dependency. Older compilers that do not recognize
this attribute cannot enforce the opt-in; the documented experimental contract
still applies.

`SKIA5001` identifies the testing library's experimental comparison API, not every
future experiment. Other independently opted-in product APIs or views receive
their own IDs under the shared [experimental API policy and registry](experimental-apis.md).

Experimental APIs may change between releases even when the package has a stable
NuGet version. This does not relax the ABI contract of `SkiaSharp.dll` or any
other existing package. See [.NET preview APIs](https://learn.microsoft.com/dotnet/fundamentals/runtime-libraries/preview-apis).

Repository test consumers use ordinary project references. Exact-package
integration consumes the produced `SkiaSharp.Testing` package from the selected
BAR feed, not a project reference that would introduce locally built SkiaSharp.
The helper and SkiaSharp package must belong to the same producing artifact
cohort. Older artifact sets without the new helper cannot silently substitute a
local library or a different package.

The source solution and all three platform filters include the library.
Publication still requires normal package verification and protected release
approval; this change does not publish anything. The earlier isolated prototype's
successful `4.150.1` compile is historical evidence only, not the dependency
contract of this normal, current-line package. Existing MAUI consumers must check
their SkiaSharp package/version compatibility before adopting it.

MAUI can initially replace the Graphics/Resizetizer assertion implementation with
this package while retaining NUnit, reference naming and attachments in MAUI.
A thin `VisualTestUtils.SkiaSharp` adapter can implement MAUI's existing
comparer/diff/editor interfaces in that repository; it does not belong in the
comparison package. The red RMS predicate is straightforward, but its
color/alpha decoding parity and Mac screenshot mask/resampling require separate
verification.

Before publication: validate the framework and dependency matrix, the exact
produced package graph, experimental metadata/consumer opt-in, compiler XML/reference payload
and public API. Publish only with the normal explicit release approval.
The separately published Extended package is not changed or deprecated here.

## Competitor overlap and recommended next increments

The strongest common ground is explicit thresholds, mismatch counts, numerical
metrics and useful diff images, not a screenshot framework. Those are already
the MVP. Comparison criteria must retain their units: ImageMagick fuzz, RGB-byte
tolerance, perceptual distance, MAE, RMS and mismatch fraction are not synonyms.

| Reference | Useful capability | Recommendation |
| --- | --- | --- |
| [pixelmatch](https://github.com/mapbox/pixelmatch/blob/v7.2.0/README.md) | Perceptual threshold, optional AA exclusion, configurable/transparent diff | Borrow diagnostic ergonomics first; perceptual/AA classification must be opt-in |
| [Playwright](https://playwright.dev/docs/test-snapshots) | Count/ratio limits, stable capture, platform baselines | Limits belong here; capture stabilization and baseline lookup stay in adapters |
| [Magick.NET](https://github.com/dlemstra/Magick.NET) | Channel-selective MAE/RMS, masks, metrics and diff colors | Prove MAUI's narrow metric subset, not full ImageMagick parity |
| [Codeuctivity.SkiaSharpCompare](https://github.com/Codeuctivity/SkiaSharp.Compare) | Tolerance maps, alpha, optional resizing, convenience inputs | Maps/alpha are covered; keep resizing explicit and outside strict comparison |
| [Codeuctivity.ImageSharpCompare](https://github.com/Codeuctivity/ImageSharp.Compare) | Pixel fraction and editable difference/tolerance maps | Keep map meaning explicit; do not inherit differing percentage units or another engine |
| [Verify.ImageMagick](https://github.com/VerifyTests/Verify.ImageMagick) | Thin framework comparers, review/acceptance tooling | Offer adapter examples, not a Verify/NUnit/xUnit dependency in the package |

Normalized MAE acceptance and ignored-region support are implemented.
MAE is used by Samples; ignored-region support is opt-in and has not been enabled
in existing screenshot/golden consumers. Improve diagnostics next. Do not
introduce a general algorithm/plugin pipeline before there is a proven requirement.

The following signatures are **proposals only**, not available APIs:

| Priority | Additive surface | Required contract |
| --- | --- | --- |
| Next | `SKPixelDifferenceOptions` plus a new difference-generation overload | Configurable highlight/tolerated/background colors and optional transparent diagnostic output; existing style overload defaults unchanged |
| Later | Normalized `ReadOnlySpan<byte>` input overloads with image info and row bytes | Borrowed buffers, explicit layout/stride/alpha, equivalent metrics; no decode or pointer ownership surprises |
| Later | Named perceptual/AA comparison entry point returning a distinct result | Versioned metric, neighborhood semantics and counted/excluded AA pixels; never reinterpret existing byte tolerance |
| Optional | Region/tile metrics or maximum window error | Useful for compact regressions hidden by global averages; separate opt-in measurements, not a new default gate |
| Demand-driven | SSIM/perceptual hash, alignment search, metadata or document rendering | Separate capabilities/packages as needed; none required for the current image-testing MVP |

Ignoring pixels is a deliberate coverage change. Preserve `TotalPixels` as image
area, but use `ComparedPixelCount` for error fractions, metric denominators and
count-derived fraction budgets when an ignore mask is explicitly supplied.
These contracts are specified above. Do not use tolerance-mask alpha to mean
coverage, or enable excluded regions in a consumer without a separate decision.

### Preserve both binary API and behavioral compatibility

The testing API is explicitly experimental while it evolves; consumers opt into
that contract. Before making it stable, finalize signatures and behavior rather
than keeping obsolete internal aliases. Once stable, retain signatures and
getters, add overloads/properties rather than changing their types, and never rely
on changed optional-parameter defaults at callers' compile time. Existing
SkiaSharp/Extended stable API contracts remain unchanged.

New options must default to today's behavior: new budgets unset, no ignored
pixels, unchanged channel representation and opaque red/amber overlay.
The readonly tolerance keeps its existing `Exact`/`Absolute`/`Percent` meaning;
do not add perceptual units or change percentage points to fractions.
Results remain library-constructed; additional getters can be added without
changing existing construction signatures.

Extended's summed-channel threshold is not retained as an MVP mode: it is a
different classifier, not another spelling of per-channel tolerance. If a real
consumer needs that exact criterion, add a separately named comparison overload
or criterion with explicit units and characterization fixtures. Binary/colored
diffs, masks, alpha and existing metrics already preserve the useful general
feature set without that historical overload ambiguity.

Pin any borrowed perceptual algorithm. In the research snapshot, pixelmatch main
had newer OKLab/ignore-mask/window ideas than its published `v7.2.0` YIQ release;
main-branch code is not proof of a released feature. AA exclusion can hide stroke
regressions, and global MAE/RMS can dilute small catastrophic changes. Keep strict
pixel counts alongside richer metrics where coverage needs it.

### Release gates for a useful external MVP

Require exact math/boundary/channel tests, wide accumulators, raw-stat invariance,
mask/diff consistency, padded input parity, invalid-input and native-lifetime
tests, portable XML/reference builds and consumer package-graph validation.
Characterize opaque, transparent and color-managed fixtures against the actual
MAUI Magick Q8 version before claiming replacement parity. Decide and document
color-space conversion and fully transparent hidden-RGB behavior; matching
normalized samples is not encoded-byte equality.

Retain caller ownership of baseline review, missing-reference failure,
capture-only provenance, device identity and scale, bounded re-capture,
attachments, GPU capability policy and test-framework assertions. Publication
needs its own version/release approval; a green compile prototype does not
publish a package or establish every runtime/native combination.
