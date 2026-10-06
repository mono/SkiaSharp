# Pixel comparison API

`SkiaSharp.Testing` is a small, consumer-facing comparison API: two decoded images
in, measurements and a match decision out, with difference images generated only
when requested. It does not capture screenshots, edit images, manage baselines,
retry tests, or depend on a test framework.

The public declarations currently live in
`tests/TestUtilities/PixelComparison/` and are source-shared by the desktop test
host, shared device/browser test library, and package-integration host.
**They are not in `SkiaSharp.dll`, and no package is published by this change.**
There is no requirement to preserve the separately published Extended comparer's
API shape.

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
using var binary = SKPixelComparer.GenerateDifferenceMask(expected, actual, options);
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

The standalone, signed class-library prototype has been compiled against
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

## Small future additions, if needed

A distinct one-byte excluded-pixel mask and normalized `ReadOnlySpan<byte>`
input could be added when a real caller needs them. Excluded pixels would leave
the metric denominator, unlike tolerance-map pixels. Zero comparison coverage
must not look like a successful match.

Anti-alias detection, perceptual thresholds, window-density scores, SSIM,
correlations, EXIF comparison, and PDF/SVG processing are deliberately excluded.
Current pixelmatch main and published `v7.2.0` use different perceptual
algorithms; any future borrowing must name and pin its algorithm rather than
pretend its threshold is a byte-channel tolerance.
