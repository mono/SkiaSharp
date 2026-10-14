---
title: "Compare images for testing"
description: "Use SkiaSharp.Testing to compare rendered images, set tolerances and generate failure diagnostics."
---

# Compare images for testing

`SkiaSharp.Testing` compares two decoded `SKImage`, `SKBitmap` or `SKPixmap`
objects. It reports pixel differences, evaluates acceptance limits and can
generate a diagnostic image. Screenshot capture, resizing, baseline selection
and test-framework assertions remain the caller's responsibility.

## Set up

Install `SkiaSharp.Testing` alongside the matching SkiaSharp release and retain
your application's existing native-assets setup:

```bash
dotnet add package SkiaSharp.Testing
```

> [!IMPORTANT]
> This API is experimental and may change between releases, including releases
> with a stable package version. Opt in to diagnostic `SKIA4001` in your test
> project; no preview SDK or runtime is required.

```xml
<PropertyGroup>
  <NoWarn>$(NoWarn);SKIA4001</NoWarn>
</PropertyGroup>
```

## Compare two images

```csharp
using System;
using System.IO;
using SkiaSharp;
using SkiaSharp.Testing;

using var expected = SKImage.FromEncodedData("expected.png")
    ?? throw new InvalidOperationException("Unable to decode the reference.");
using var actual = SKImage.FromEncodedData("actual.png")
    ?? throw new InvalidOperationException("Unable to decode the actual image.");

var result = SKPixelComparer.Compare(expected, actual);
if (!result.IsMatch)
{
    using var difference = SKPixelComparer.GenerateDifferenceImage(expected, actual);
    using var png = difference.Encode(SKEncodedImageFormat.Png, 100)
        ?? throw new InvalidOperationException("Unable to encode the difference.");
    File.WriteAllBytes("difference.png", png.ToArray());
    throw new InvalidOperationException(
        $"{result.MismatchedPixelCount} pixels differ.");
}
```

The default is exact RGBA comparison after normalization to unpremultiplied
8-bit pixels. Inputs must have equal, nonempty dimensions and readable pixels.
Normalize capture size, scale, fonts and animation timing before comparing;
the library does not align or resize images.

## Allow small differences

Channel tolerances classify individual pixels. A pixel mismatches when any
enabled channel exceeds its allowance. A mismatch budget controls how many such
pixels the whole image may contain:

```csharp
var options = new SKPixelComparerOptions
{
    RedTolerance = SKPixelTolerance.Absolute(2),
    GreenTolerance = SKPixelTolerance.Absolute(2),
    BlueTolerance = SKPixelTolerance.Absolute(2),
    AlphaTolerance = null,
    MismatchBudget = SKPixelMismatchBudget.Percent(0.2),
};
var result = SKPixelComparer.Compare(expected, actual, options);
```

`null` disables a channel; zero requires an exact match. `Absolute` uses byte
levels, while tolerance `Percent` uses percentage points of the full 255 range.
Budget `Percent(0.2)` means 0.2% of compared pixels, not a fraction of 0.2.
`Pixels(n)` sets a count cap; `Both(n, percent)` requires both caps to pass.
Boundaries are inclusive, and percentage budgets floor to a whole-pixel limit.

For an average difference-magnitude gate instead, configure
`MaxNormalizedMeanAbsoluteError` or `MaxNormalizedRootMeanSquaredError` from
0 to 1. For example, a normalized MAE limit of `0.01` allows an average absolute
channel difference of 2.55 byte levels. It does not mean 1% mismatched pixels.
All explicitly configured gates combine with AND; an MAE-only or RMS-only gate
does not also require zero mismatches. With no gates, zero mismatches are allowed.

## Read measurements and diagnostics

`RawMetrics` measures enabled-channel differences before tolerance filtering.
`ThresholdedMetrics` replaces tolerated channel differences with zero; exceeding
differences retain their full magnitude. Both use
`ComparedPixelCount * ChannelCount` samples, so thresholding never shrinks the
denominator. Prefer helpers such as `NormalizedMeanAbsoluteError` and
`NormalizedRootMeanSquaredError` over hard-coded channel counts or ranges.

`MismatchBounds` locates mismatched pixels, while `MaxChannelDifference` includes
tolerated differences. `IsMatch` can be true even when some pixels mismatch.

Pass a `SKPixelDifferenceStyle` to `GenerateDifferenceImage`:

| Style | Output |
| --- | --- |
| `ThresholdOverlay` (default) | Red mismatches, amber tolerated differences, dimmed actual RGB matches |
| `BinaryMask` | White mismatches, black otherwise |
| `ChannelDifference` | Raw absolute RGB differences, including disabled RGB channels; alpha-only differences are not visible |

Limits affect acceptance, not diagnostic classification. Use the same options
for comparison and difference generation.

## Tolerance and ignore masks

Both masks are optional, caller-owned `SKImage` objects with the input dimensions:

- `ToleranceMask` widens each enabled channel's allowance using that mask byte.
  Its alpha byte is an alpha-channel allowance, not coverage. Pixels remain in
  raw measurements and all denominators.
- `IgnoreMask` excludes entire pixels: black RGB compares; any nonzero decoded
  RGB ignores, independently of mask alpha or enabled comparison channels.
  Ignore masks are read as unpremultiplied data without drawing or compositing.

Ignored pixels contribute no errors, mismatches, maxima or bounds.
`TotalPixels` remains the image area; `ComparedPixelCount` and `IgnoredPixelCount`
report coverage, and fractions/budgets use only compared pixels. Ignoring every
pixel is invalid. Ignored diagnostics are black in binary/channel-difference
styles and neutral gray in overlays.

Inputs and masks must stay alive and unmodified during each synchronous call.
Results retain no native inputs; returned difference images are independently
owned and must be disposed. Set `AlphaType = SKAlphaType.Premul` when comparing
that representation intentionally; disabling alpha does not undo opacity's
effect on premultiplied RGB.
