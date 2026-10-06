# Pixel comparison for tests

The comparer is an **unpublished test utility**, not part of `SkiaSharp.dll` or
a new NuGet package. Its implementation lives in
`tests/TestUtilities/PixelComparison/` and is source-shared by the desktop test
host, shared device/browser test library, and package-integration test project.
Package integration still compiles against its exact selected SkiaSharp package;
it does not reference the locally built bindings to obtain the comparer.

This document separates the implemented API from a proposal for small future
improvements. Neither layer needs screenshot capture, image editing, baseline
storage, a test framework, or a comparison pipeline.

## Implemented API

The starting point is the current
[Extended comparer](https://github.com/mono/SkiaSharp.Extended/tree/579c974196199962dc1cb7c22bc68fe32e9a5b64/source/SkiaSharp.Extended/Comparer),
with its RGB/RGBA options, tolerance maps, and error metrics. The port keeps
decoded `SKImage`, `SKBitmap`, and `SKPixmap` inputs.

The filename overloads are omitted: callers already own decoding and its errors.
The bare integer-tolerance overload is also omitted. In current Extended it
means a summed RGB threshold; in SkiaSharp's old test copy it meant a maximum
RGBA channel delta. Passing the options explicitly removes that ambiguity.
The mask overload likewise takes explicit options, avoiding a third-argument
`null` that could mean either a mask or options.

The following is a signature summary, not implementation code. Each comparison
family also accepts matching bitmap or pixmap inputs:

```csharp
SKPixelComparisonResult Compare(SKImage first, SKImage second);
SKPixelComparisonResult Compare(
    SKImage first, SKImage second, SKPixelComparerOptions? options);
SKPixelComparisonResult Compare(
    SKImage first, SKImage second, int tolerance,
    SKPixelComparerOptions? options);
SKPixelComparisonResult Compare(
    SKImage first, SKImage second, SKImage mask,
    SKPixelComparerOptions? options);
```

### Given two images

Inside the test projects:

```csharp
using SkiaSharp.Testing;

var result = SKPixelComparer.Compare(expected, actual);

Assert.Equal(0, result.ErrorPixelCount);
```

The default compares RGB in BGRA8888 **unpremultiplied** representation, with
zero tolerance. It deliberately keeps current Extended's defaults rather than
changing alpha behavior during the move.

Normalization retains the original draw-based behavior after validating that
the source pixels can be read. Fully transparent hidden RGB therefore does not
become a new visible difference merely because the implementation was moved.

For RGBA comparison with a per-channel allowance:

```csharp
var options = new SKPixelComparerOptions
{
    CompareAlpha = true,
    TolerancePerChannel = true,
};

var result = SKPixelComparer.Compare(expected, actual, 2, options);
Assert.True(result.ErrorPixelPercentage <= 0.002);
```

`ErrorPixelPercentage` is the existing name, but its value is a **fraction from
0 to 1**, not percentage points. A delta equal to tolerance is accepted.

For summed tolerance, set `TolerancePerChannel = false`. For example, deltas
`(2, 2, 2)` with tolerance `3` pass a per-channel comparison but fail a summed
comparison. Alpha contributes to the sum only when `CompareAlpha` is enabled.

### Compatibility with existing tests

`AlphaType` selects the comparison representation. Its default is
`SKAlphaType.Unpremul`; `SKAlphaType.Premul` retains the old test helper's
premultiplied-byte comparisons. This is independent of whether alpha is among
the channels compared.

| Consumer | Explicit options and existing gate |
| --- | --- |
| Core bitmap assertions | Premultiplied RGB; error fraction strictly below `10^-precision` |
| Renderer goldens | Premultiplied RGBA, per-channel tolerance 2 or 12; count no greater than `floor(totalPixels * allowedFraction)` |
| Package integration | Premultiplied RGB; existing resize adapter and similarity at least 95% |

These policies remain at their call sites. The comparer does not choose a GPU
policy, accept a missing baseline, or resize images to make them comparable.

### Tolerance maps

```csharp
var result = SKPixelComparer.Compare(expected, actual, toleranceMask, options);
```

The mask has the same dimensions as the images. In per-channel mode, its channel
values are individual tolerance thresholds. In summed mode, their selected
channel sum is the threshold.

This is **not an excluded-pixel mask**. A white RGB mask pixel tolerates all RGB
differences, but that pixel remains in the metric denominator. When comparing
alpha, an opaque mask's alpha value of 255 also tolerates all alpha differences
at that pixel. Do not confuse mask opacity with coverage.

### Measurements

The result exposes total/error pixel counts, the error fraction, 64-bit
`AbsoluteError` and `SumSquaredError`, channel count, and the maximum observed
selected-channel delta. Derived measurements are MAE, MSE, RMSE, normalized RMSE,
and PSNR.

The aggregate error metrics keep Extended's **threshold-filtered** semantics:
per-channel mode excludes tolerated channels from the numerators; summed mode
includes all selected-channel deltas only for a rejected pixel. The divisor is
all image pixels times the selected channel count. `MaxChannelDelta` is raw,
including differences that were tolerated.

Using `long` avoids the old absolute-error overflow: a 3840x2160 black/white RGB
comparison requires `6,345,216,000` absolute error. PSNR is positive infinity
when the measured squared error is zero.

### Difference images

The two existing conveniences remain:

```csharp
using var mask = SKPixelComparer.GenerateDifferenceMask(expected, actual);
using var delta = SKPixelComparer.GenerateDifferenceImage(expected, actual);
```

The first is a strict binary mask. The second is Extended's raw RGB
channel-delta image, not a threshold heatmap. Options overloads select alpha and
representation where applicable.

For the renderer golden diagnostic:

```csharp
var options = new SKPixelComparerOptions
{
    AlphaType = SKAlphaType.Premul,
    CompareAlpha = true,
    TolerancePerChannel = true,
};

using var diff = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, SKPixelDifferenceStyle.ThresholdOverlay, 2, options);
```

The explicit styles are `BinaryMask`, `ChannelDelta`, and `ThresholdOverlay`.
The overlay uses the comparison predicate: rejected pixels are red, tolerated
differences are amber, and matches show dimmed actual pixels. Alpha-only
rejections are visible in the overlay; the channel-delta convenience continues
to show RGB deltas.

Inputs are borrowed and must remain valid and unmodified for the synchronous
call. Numeric results do not retain the images. Returned difference images are
caller-owned. Unsupported/disposed inputs, unreadable pixels, invalid tolerance,
and inconsistent dimensions are errors, not successful comparisons with empty
measurements.

## Proposed next API: a few small ideas

**The remainder is a proposal, not an implemented or published contract.**
The goal is still two images in, measurements out, with optional acceptance limits
and difference output. There is no assertion framework or baseline manager.

### One comparison-options shape

Keep decoded-image conveniences, but move tolerance into one options object:

```csharp
var options = new SKPixelComparisonOptions
{
    Channels = SKPixelComparisonChannels.Rgba,
    AlphaType = SKAlphaType.Unpremul,
    Tolerance = 2,
    ToleranceMode = SKPixelToleranceMode.PerChannel,
    Limits = new SKPixelComparisonLimits
    {
        MaxErrorPixelFraction = 0.002,
    },
};

var result = SKPixelComparer.Compare(expected, actual, options);
Assert.True(result.IsMatch);

using var diff = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, options, SKPixelDifferenceStyle.ThresholdOverlay);
```

`SKImage`, `SKBitmap`, and `SKPixmap` retain equivalent entry points. Use
overloads rather than default parameters. A plain two-image overload would
select strict comparison; **RGBA is the recommended future default**, with old
RGB callers migrated explicitly. That default is not changed by the current
port.

### Useful result shape

```csharp
// Proposed get-only result members:
bool IsMatch;
int TotalPixels;
int ComparedPixelCount;
int IgnoredPixelCount;
int ErrorPixelCount;
double ErrorPixelFraction;
int MaxChannelDelta;
SKRectI? ErrorBounds;
SKPixelComparisonMetrics RawMetrics;
SKPixelComparisonMetrics ThresholdedMetrics;
```

Both metric groups carry wide absolute/SSE numerators and clearly named
MAE/MSE/RMSE/normalized RMSE/PSNR. Raw metrics describe the selected samples
before tolerance; thresholded metrics retain the current Extended behavior.
Pixel selection and exclusion precede both groups.

This avoids reporting zero raw RMSE merely because tolerance hid every
difference. The result has controlled construction and no native ownership;
creating a difference image remains an explicit operation on the input images.
Error bounds are simply the bounding rectangle of rejected pixels, useful for
failure diagnostics without adding any editing API.

### Explicit limits, with unambiguous units

The small limits object needs only:

```csharp
long? MaxErrorPixels;
double? MaxErrorPixelFraction;
double? MaxNormalizedRootMeanSquaredError;
SKPixelComparisonBoundary Boundary; // Inclusive or Exclusive
```

All configured limits must pass. With no limits, `IsMatch` means zero rejected
pixels under the selected tolerance. When a metric-only limit is supplied,
there is no hidden additional zero-error-pixel condition. RMS limits use raw
metrics by default; an explicitly named metric-source choice can request the
thresholded group.

Fractions use 0-1; normalized RMS uses 0-1 but is **not a pixel fraction**.
Inclusive fraction budgets use `floor(comparedPixelCount * fraction)`, matching
the renderer harness. Exclusive boundaries support the core helper's existing
strict comparison. Invalid/NaN/infinite limits fail validation.

### Concrete calls for the existing consumers

| Use case | Proposed settings |
| --- | --- |
| Exact image equality | Zero tolerance; no limits |
| Existing Extended RGB behavior | RGB channels; explicit premultiplied or unpremultiplied representation appropriate to the consumer |
| Core bitmap assertion | RGB, premultiplied, `MaxErrorPixelFraction = 10^-precision`, exclusive boundary |
| Renderer golden | RGBA, premultiplied, per-channel tolerance, existing maximum error fraction |
| Package integration | RGB, premultiplied, maximum error fraction 0.05; resizing remains outside comparison |
| MAUI Graphics / Resizetizer | RGB, legacy representation, maximum error fraction 0.07 / 0.27 |
| MAUI screenshot compatibility | Red channel, raw normalized RMS maximum 0.005, inclusive boundary; prove Magick Q8 conversion parity first |

For example, the proposed MAUI numeric call is small:

```csharp
var result = SKPixelComparer.Compare(expected, actual,
    new SKPixelComparisonOptions
    {
        Channels = SKPixelComparisonChannels.Red,
        Limits = new SKPixelComparisonLimits
        {
            MaxNormalizedRootMeanSquaredError = 0.005,
        },
    });
```

This models the
[actual red-only MAUI comparison](https://github.com/dotnet/maui/blob/7a5a5d610cc056e6fbd0f3dfe3930ea6de1f28bc/src/TestUtils/src/VisualTestUtils.MagickNet/MagickNetVisualComparer.cs),
not a blanket claim of ImageMagick compatibility. MAUI's rounded error-message
parsing should not become a comparer feature.

### Small additions worth considering

| Idea | Source / benefit | Proposed boundary |
| --- | --- | --- |
| Channel selection | Magick.NET / MAUI; red, RGB, RGBA and alpha-only measurements | An explicit managed flags enum, not a general image-processing API |
| Count and fraction limits | [Playwright](https://playwright.dev/docs/test-snapshots); readable acceptance budgets | A small value/configuration object; no retries, capture or baseline logic |
| Raw versus thresholded metrics | Prevents current filtered-metric ambiguity | Two clearly named numerical groups |
| Excluded-pixel mask | [pixelmatch](https://github.com/mapbox/pixelmatch/blob/0cbe435beb0dbff093889c0f049a355ab3fa50a3/README.md); volatile regions | A separate one-byte-per-pixel mask; 0 compares, nonzero excludes; report coverage and reject zero compared pixels |
| Difference bounds and on-demand output | Helps diagnose a small changed region; avoids generating images callers do not request | Rectangle and existing diff styles, not an editor |
| Normalized span input | Goldens already have packed pixels | `ReadOnlySpan<byte>` plus explicit image info/layout; no decoding or pointer lifetime escaping the call |

The new byte mask must not reuse tolerance-mask alpha. Its excluded pixels leave
the fraction/metric denominator; tolerance-map pixels do not. Input spans and
padded pixmaps must agree on channel ordering, alpha representation and error
measurements.

Current pixelmatch main has newer OKLab, ignore-mask and window-density ideas
than its published `v7.2.0` YIQ implementation. Any future algorithm borrowing
must pin and name the algorithm, not treat those thresholds as RGB tolerance.
Anti-alias detection, perceptual thresholds, window-density scores, SSIM,
correlations, EXIF comparison and PDF/SVG processing are **not** part of this
proposal's first increment.

## Adoption order

The current change replaces the existing Extended dependency and local copy
with one tested, internal source implementation. Next, channel selection,
structured acceptance limits and raw metrics can be added independently,
followed by a separate ignore mask or normalized span convenience if a caller
needs them.

Keep the utility unpublished until its defaults, units and externally useful
surface are agreed. A future API review can choose whether to retain old names
as aliases; it must not remove or change signatures in the separately published
Extended package as a side effect of this repository change.
