# Pixel comparison API

`SkiaSharp.Testing` is a small, consumer-facing comparison API: two decoded images
in, measurements and a match decision out, with difference images generated only
when requested. It does not capture screenshots, edit images, manage baselines,
retry tests, or depend on a test framework.

The public declarations currently live in
`tests/TestUtilities/PixelComparison/` and are source-shared by the desktop test
host, shared device/browser test library, package-integration host, and Samples
test host.
**They are not in `SkiaSharp.dll`, and no package is published by this change.**
There is no requirement to preserve the separately published Extended comparer's
API shape.

## MVP scope and readiness

The useful MVP is the comparison kernel below, shared by SkiaSharp and exposed
through a separate portable library for external consumers. It covers Extended's
general capabilities: decoded image inputs, pixel counts, channel differences,
tolerance maps, error statistics, and binary/color diagnostics. It improves the
defaults and units rather than preserving Extended's signatures.

| Required capability | Current implementation |
| --- | --- |
| Exact/tolerant RGB, RGBA, or individual-channel comparison | `SKPixelTolerance`, with nullable channel allowances |
| Per-pixel tolerance image | `ToleranceMask`, combined with global allowances |
| Error count, fraction, bounds and largest channel error | `SKPixelComparisonResult` |
| Absolute/SSE, MAE/MSE/RMSE/normalized RMS/PSNR | Separate raw and thresholded metric groups |
| Whole-image acceptance | Optional count, fraction and raw-RMS budgets |
| Diagnostic PNG-ready images | Binary mask, raw RGB delta, threshold overlay |
| Standalone consumer library | Nonshipping `SkiaSharp.Testing` prototype; publication remains separate |

Two boundaries matter. The library requires equal dimensions and readable CPU
pixels; GPU readback, screenshot targeting and resizing belong to the caller.
It measures normalized 8-bit samples, not arbitrary HDR values, perceptual
similarity, or encoded-file identity. Fonts, profiles, scale and animation
determinism must be controlled by the test producer.

The public surface is implemented, but external adoption is not yet a completed
package release or a verified replacement for every Magick input. In particular,
MAUI's pinned red-RMS conversion needs oracle fixtures. Current Samples also uses
normalized mean absolute error; the measurement is already available, but a
built-in MAE acceptance property is an additive follow-up, not implemented today.

### API shape

The complete public surface is shown below. These are declaration-only
signatures, not executable implementation code; implicit constructors are
listed explicitly. Private/internal implementation members are omitted.

```csharp
namespace SkiaSharp.Testing;

public static class SKPixelComparer
{
    public static SKPixelComparisonResult Compare(SKImage first, SKImage second);
    public static SKPixelComparisonResult Compare(SKImage first, SKImage second, SKPixelComparerOptions? options);
    public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second);
    public static SKPixelComparisonResult Compare(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options);
    public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second);
    public static SKPixelComparisonResult Compare(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options);

    public static SKImage GenerateDifferenceImage(SKImage first, SKImage second);
    public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelComparerOptions? options);
    public static SKImage GenerateDifferenceImage(SKImage first, SKImage second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style);
    public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second);
    public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options);
    public static SKImage GenerateDifferenceImage(SKBitmap first, SKBitmap second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style);
    public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second);
    public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options);
    public static SKImage GenerateDifferenceImage(SKPixmap first, SKPixmap second, SKPixelComparerOptions? options, SKPixelDifferenceStyle style);
}

public readonly struct SKPixelTolerance : IEquatable<SKPixelTolerance>
{
    public SKPixelTolerance();
    public static SKPixelTolerance Exact { get; }
    public static SKPixelTolerance Absolute(int value);
    public static SKPixelTolerance Absolute(int? red, int? green, int? blue, int? alpha);
    public static SKPixelTolerance Percent(double value);
    public static SKPixelTolerance Percent(double? red, double? green, double? blue, double? alpha);
    public double? Red { get; }
    public double? Green { get; }
    public double? Blue { get; }
    public double? Alpha { get; }
    public int ChannelCount { get; }
    public bool Equals(SKPixelTolerance other);
    public override bool Equals(object? obj);
    public override int GetHashCode();
    public static bool operator ==(SKPixelTolerance left, SKPixelTolerance right);
    public static bool operator !=(SKPixelTolerance left, SKPixelTolerance right);
}

public sealed class SKPixelComparerOptions
{
    public SKPixelComparerOptions();
    public SKPixelTolerance Tolerance { get; set; }
    public SKAlphaType AlphaType { get; set; }
    public SKImage? ToleranceMask { get; set; }
    public long? MaxErrorPixels { get; set; }
    public double? MaxErrorPixelFraction { get; set; }
    public double? MaxNormalizedRootMeanSquaredError { get; set; }
}

public sealed class SKPixelComparisonResult
{
    public bool IsMatch { get; }
    public int TotalPixels { get; }
    public int ChannelCount { get; }
    public int ErrorPixelCount { get; }
    public double ErrorPixelFraction { get; }
    public int MaxChannelDelta { get; }
    public SKRectI? ErrorBounds { get; }
    public SKPixelComparisonMetrics RawMetrics { get; }
    public SKPixelComparisonMetrics ThresholdedMetrics { get; }
}

public sealed class SKPixelComparisonMetrics
{
    public int TotalPixels { get; }
    public int ChannelCount { get; }
    public long SampleCount { get; }
    public long AbsoluteError { get; }
    public long SumSquaredError { get; }
    public double MeanAbsoluteError { get; }
    public double MeanSquaredError { get; }
    public double RootMeanSquaredError { get; }
    public double NormalizedRootMeanSquaredError { get; }
    public double PeakSignalToNoiseRatio { get; }
}

public enum SKPixelDifferenceStyle
{
    BinaryMask = 0,
    ChannelDelta = 1,
    ThresholdOverlay = 2,
}
```

The six public types are `SKPixelComparer`, `SKPixelComparerOptions`,
`SKPixelTolerance`, `SKPixelComparisonResult`, `SKPixelComparisonMetrics`,
and `SKPixelDifferenceStyle`. Results/metrics have no public constructors.
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
    Tolerance = SKPixelTolerance.Absolute(10, 20, 2, 2),
    MaxErrorPixelFraction = 0.002,
};

var result = SKPixelComparer.Compare(expected, actual, options);
Assert.True(result.IsMatch);

using var diff = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, options, SKPixelDifferenceStyle.ThresholdOverlay);
```

The same overload families accept `SKImage`, `SKBitmap`, or `SKPixmap`. There are
no positional tolerance or mask arguments, filename overloads, optional
parameters, channels enum, or summed-tolerance mode.

## Tolerance: how different may a sample be?

`SKPixelTolerance` is an immutable value. The default value and `Exact` both
enable all four channels with zero allowance:

```csharp
var exact = SKPixelTolerance.Exact;
var allChannels = SKPixelTolerance.Absolute(2);
var perChannel = SKPixelTolerance.Absolute(10, 20, 2, 2);
var rgbOnly = SKPixelTolerance.Absolute(0, 0, 0, null);
var redOnly = SKPixelTolerance.Absolute(0, null, null, null);
var alphaOnly = SKPixelTolerance.Absolute(null, null, null, 0);
var percentages = SKPixelTolerance.Percent(0.5, 0.2, 1, null);
```

**Zero means exact; `null` disables a channel.** Disabling all channels is
invalid. Disabled channels contribute neither rejection nor raw/thresholded
metrics nor maximum delta, so red-only RMS has a one-channel denominator.
This encodes selection without another options flag or enum.

`Absolute` accepts integer byte-channel allowances from 0 through 255.
`Percent` accepts percentage points from 0 through 100, not fractions. It
converts each value to a percentage of the full channel range without rounding:
0.5% of 255 is 1.275, so a difference of 1 passes and 2 fails.
The nullable `Red`, `Green`, `Blue`, and `Alpha` properties expose these effective
byte-unit allowances; `ChannelCount` reports the number of enabled channels.

A selected channel rejects a pixel only when its absolute difference is
**greater than** its allowance. Equality at the boundary is tolerated. A pixel
is rejected if any enabled channel rejects it. An absolute allowance is not
named `Exact(10, ...)`, since a nonzero allowance is not exact equality.

## Limits: how much whole-image error is acceptable?

Tolerance classifies individual samples. Limits evaluate the complete
comparison. They are related but are not interchangeable.

The optional properties on `SKPixelComparerOptions` are:

| Property | Units and decision |
| --- | --- |
| `MaxErrorPixels` | Nonnegative count; rejected count must be no greater |
| `MaxErrorPixelFraction` | Fraction 0-1; rejected count must be no greater than `floor(totalPixels * fraction)` |
| `MaxNormalizedRootMeanSquaredError` | Raw selected-channel normalized RMS, 0-1; measured value must be no greater |

All configured limits must pass. With no limits, `IsMatch` means zero rejected
pixels under the selected tolerance. An RMS-only limit does not secretly impose
zero rejected pixels.

For example, a tolerance of 2 ignores small rounding changes at any pixel. A
fraction limit of 0.002 additionally allows up to 0.2% of pixels to exceed that
tolerance. This is different from allowing a per-channel difference of 0.2% of
255. Increasing tolerance does not lower the raw RMS measurement.

Negative, out-of-range, NaN, and infinite settings are invalid. The result's
PSNR may legitimately be positive infinity when squared error is zero.

## Measurements and diagnostics

`SKPixelComparisonResult` provides:

- `IsMatch`, `TotalPixels`, and `ChannelCount`.
- `ErrorPixelCount` and `ErrorPixelFraction`, with an explicit 0-1 fraction.
- Raw selected-channel `MaxChannelDelta`, including tolerated differences.
- `ErrorBounds`, the bounding rectangle of rejected pixels, or `null` when none.
- `RawMetrics` and `ThresholdedMetrics`.

Each `SKPixelComparisonMetrics` group has wide absolute/SSE numerators and
MAE, MSE, RMSE, normalized RMSE, and PSNR with a selected-channel denominator.
Raw metrics include every selected normalized difference before tolerance.
Thresholded metrics include only individual channel differences that exceed
their thresholds, not the difference minus the allowance. Both groups divide by
all compared pixels times enabled channels.

Changing only tolerance leaves `RawMetrics` unchanged. This prevents a tolerated
image from being reported as having zero raw RMS merely because the classifier
ignored its differences. Result and metric objects have controlled construction
and retain no native inputs.

```csharp
using var binary = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, options, SKPixelDifferenceStyle.BinaryMask);
using var overlay = SKPixelComparer.GenerateDifferenceImage(expected, actual, options);
using var delta = SKPixelComparer.GenerateDifferenceImage(
    expected, actual, options, SKPixelDifferenceStyle.ChannelDelta);
```

The default difference image is `ThresholdOverlay`, so alpha-only rejections are
visible. Red means rejected, amber means a selected difference within tolerance,
and matching pixels show dimmed actual RGB. `BinaryMask` uses white/black.
`ChannelDelta` is an opaque raw RGB diagnostic and does not claim to display
alpha-only changes.

Limits affect `IsMatch`, not which pixels are colored red. Every comparison and
binary/overlay diagnostic uses the same classification settings.

## Tolerance maps and representation

```csharp
var options = new SKPixelComparerOptions
{
    Tolerance = SKPixelTolerance.Absolute(2),
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
| Core bitmap assertion | `Absolute(0, 0, 0, null)`, premultiplied; external strict fraction comparison `< 10^-precision` |
| Renderer goldens | RGBA `Absolute(ChannelTolerance)`, premultiplied, configured maximum error fraction with the same floor-based budget |
| Package integration | Exact RGB, premultiplied; existing crop/resize adapter and similarity at least 95% remain |
| Samples | Exact RGBA, premultiplied; decimal pixel fraction 0.00075 by default, or the caller-selected normalized raw-MAE replacement gate |
| MAUI Graphics / Resizetizer | Exact RGB, legacy representation; fraction limits 0.07 / 0.27 |
| MAUI screenshot metric | Exact red only, raw normalized RMS limit 0.005; verify pinned Magick Q8 conversion parity before replacement |

```csharp
var result = SKPixelComparer.Compare(expected, actual,
    new SKPixelComparerOptions
    {
        Tolerance = SKPixelTolerance.Absolute(0, null, null, null),
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
var normalizedMae =
    result.RawMetrics.AbsoluteError / (result.TotalPixels * 4.0 * 255);
```

It is neither NRMSE nor the share of rejected pixels. Browser animated-GPU smoke
checks currently use 0.06; a broad, slight change can pass this criterion while
many pixels differ. Do not substitute the RMS limit or add the pixel-count gate
as a second requirement. Existing infrastructure tests cover both policies.
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

## Packaging for MAUI: proposed, not published

The natural distribution is **one small `SkiaSharp.Testing` class-library
package**, not an addition to `SkiaSharp.dll` or a dependency on the Extended
feature set. Its public namespace and types already form that facade. A
[nonshipping class-library prototype](../../tests/SkiaSharp.Testing/SkiaSharp.Testing.csproj)
builds the same sources against MAUI's existing SkiaSharp dependency floor.

| Decision | Recommendation |
| --- | --- |
| Initial frameworks | `netstandard2.0` for broad .NET/legacy consumption, plus `net10.0` for the current runtime |
| Runtime dependency | Only `SkiaSharp`, with the lowest API version actually compiled and validated; target MAUI's existing `4.150.1` floor rather than forcing a milestone upgrade |
| Native assets | Add no new native-assets package or bundled native binary; use the consumer's existing SkiaSharp platform setup |
| Test frameworks | No xUnit, NUnit, Microsoft.Testing.Platform, Appium, or Playwright dependency |
| Other libraries | No HarfBuzz, Magick.NET, ImageSharp, or metadata/document conversion dependency |
| Assembly | Signed where required by repository/legacy consumers; compiler XML next to `lib/` and `ref/` assemblies |
| Package/version | A separate test-tool package/release decision; no release-manifest or publication changes in this PR |

The eventual repository project can live in `source/SkiaSharp.Testing/` and use
the normal versioning/signing/packaging conventions. Canonical implementation
files should move there once the binary library is adopted. The current shared
source import can bridge that transition; it should not become permanently
duplicated code in a separate repository.

A project sketch for a **later** packaging change is:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFrameworks>netstandard2.0;net10.0</TargetFrameworks>
    <AssemblyName>SkiaSharp.Testing</AssemblyName>
    <RootNamespace>SkiaSharp.Testing</RootNamespace>
    <PackageId>SkiaSharp.Testing</PackageId>
    <PackagingGroup>SkiaSharp.Testing</PackagingGroup>
    <GenerateDocumentationFile>true</GenerateDocumentationFile>
    <IsPackable>false</IsPackable>
    <IsShipping>false</IsShipping>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="SkiaSharp" Version="[4.150.1,5.0.0)" />
  </ItemGroup>
</Project>
```

The floor and upper bound are a proposal requiring package-compatibility
validation, not an untested promise. A source-built CI binary may instead
ProjectReference the matching local binding. The distributed NuGet dependency
range is a separate choice; it must not pull a new native milestone into MAUI
just to compare images.

The standalone, signed class-library prototype was compiled before the current rebase against
`SkiaSharp 4.150.1` for both `netstandard2.0` and `net10.0`, with generated compiler
XML and reference assemblies and zero documentation warnings. It uses these
public sources without referencing a test assembly. This verifies the compile
floor and portable facade, not every native/runtime combination or Magick
conversion parity. `IsPackable` and `IsShipping` are both false; the prototype
is not included in release manifests and produces no published package.

Package-integration tests must continue linking the comparer source, or use a
separate helper build mode that references their exact selected BAR package.
They must not reference a helper binary that transitively loads the locally
built binding and invalidates the package-provenance check.

MAUI can initially replace the Graphics/Resizetizer assertion implementation with
this package while retaining NUnit, reference naming and attachments in MAUI.
A thin `VisualTestUtils.SkiaSharp` adapter can implement MAUI's existing
comparer/diff/editor interfaces in that repository; it does not belong in the
comparison package. The red RMS predicate is straightforward, but its
color/alpha decoding parity and Mac screenshot mask/resampling require separate
verification.

Before any publication: validate the dependency floor and framework matrix,
review the public API and XML documentation, add the project to the appropriate
shipping/package manifests, and make an explicit version/publication decision.
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

**First recommendation: normalized MAE acceptance and ignored-region support.**
MAE is needed by a current consumer; excluded regions are common in screenshot
tools and prevent volatile UI from forcing broad global tolerances.
Improve diagnostics next. Do not introduce a general algorithm/plugin pipeline
before there is a proven requirement.

The following signatures are **proposals only**, not available APIs:

| Priority | Additive surface | Required contract |
| --- | --- | --- |
| Next | `double? SKPixelComparerOptions.MaxNormalizedMeanAbsoluteError` | Raw selected-channel MAE / 255; finite 0-1; joins configured budgets with AND. Samples sets this alone when its MAE policy replaces its fraction gate |
| Next | `SKImage? SKPixelComparerOptions.IgnoreMask` | Dedicated coverage map, separate from tolerance: explicit nonzero coverage excludes a pixel. Add `ComparedPixelCount`/`IgnoredPixelCount`; reject zero coverage |
| Next | `SKPixelDifferenceOptions` plus a new difference-generation overload | Configurable highlight/tolerated/background colors and optional transparent diagnostic output; existing style overload defaults unchanged |
| Later | Normalized `ReadOnlySpan<byte>` input overloads with image info and row bytes | Borrowed buffers, explicit layout/stride/alpha, equivalent metrics; no decode or pointer ownership surprises |
| Later | Named perceptual/AA comparison entry point returning a distinct result | Versioned metric, neighborhood semantics and counted/excluded AA pixels; never reinterpret existing byte tolerance |
| Optional | Region/tile metrics or maximum window error | Useful for compact regressions hidden by global averages; separate opt-in measurements, not a new default gate |
| Demand-driven | SSIM/perceptual hash, alignment search, metadata or document rendering | Separate capabilities/packages as needed; none required for the current image-testing MVP |

Ignoring pixels is a deliberate coverage change. Preserve `TotalPixels` as image
area, but use `ComparedPixelCount` for error fractions, metric denominators and
count-derived fraction budgets when an ignore mask is explicitly supplied.
With no ignore mask, results must be identical. Specify mask size, format,
coordinates, ignored diff appearance and raw-stat exclusion before implementation.
Do not use tolerance-mask alpha to mean coverage.

### Preserve both binary API and behavioral compatibility

Before shipping, continue refining the unpublished surface rather than keeping
obsolete internal aliases. After shipping, retain signatures and getters,
add overloads/properties rather than changing their types, and never rely on
changed optional-parameter defaults at callers' compile time.

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
