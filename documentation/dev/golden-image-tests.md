# Golden-image tests

Visual tests render each scene through every backend required by the host's
[GPU policy](gpu-test-policy.md). Sample tests compare application output and
browser screenshots. Both use reviewed PNG references and the non-shipping
`SkiaSharp.Tests.Shared` project for requests, platform policy, disk/resource
lookup, byte loading, PNG handling, comparison results, diagnostic markers and
`SKPixelComparer`. `GoldenVerifier` gives visual and sample tests the same
resolve, require, compare, retry, diff and marker flow. Consumers keep only their
request factories, policy choices and renderer/capture operations.

## Reference paths

```text
tests/Goldens/{Samples|Visual}/{platform}/[{max-version}/]{case/path}[.variant][.architecture].png
```

Profiles, variants and architectures are safe lowercase alphanumeric/kebab
segments. Cases are safe relative paths with one or more segments, preserving
case (for example `Console/output` or `Maui/home`). The containing folder already
identifies the platform, so groups distinguish test families such as `Maui/*` and `Native/*`
rather than repeating `Android` or `iOS`. Rooted paths,
backslashes, empty segments, dot segments and reserved device names are rejected.
Optional version folders use `System.Version`, normalized to at least three
numeric components; fourth components retain normal `System.Version` ordering.
Stored directories use this canonical numeric form, without leading zeros.
The platform root is the current/latest reference.
Architectures use .NET architecture names (`x64`, `arm64`, `wasm`, etc.);
those tokens are reserved and cannot also name functional variants.
There are no symbolic versions, image revision suffixes or version scopes.

Examples:

```text
Visual/mac/DiagonalLines.ganesh-gl.arm64.png
Visual/portable/FilledCircle.raster.png
Samples/mac/Browser/wasm-page.png
Samples/docker-linux/Console/output.png
```

Existing visual and sample references live directly under their platform because
they are the current references. Migrating their paths does not change any PNG
bytes or infer a product version. Sample host profiles are
`mac`, `linux` and `windows`; container profiles are `docker-linux` and
`docker-windows`, so container and host output cannot accidentally share a
baseline.

## Lookup rules

`GoldenRequest` carries the platform, optional `Version`, case path, optional architecture,
and optional functional variant.

For each eligible version, candidate filenames are ordered:

```text
case.variant.architecture.png
case.variant.png
```

Without a variant, only the architecture-specific and neutral filenames apply.
A named variant never borrows a sibling variant; another architecture is never
substituted, and a named variant never falls back to an unqualified image.
Visual tests use the renderer as the variant, so a missing GPU reference cannot
resolve to raster pixels.

Version folders are inclusive upper-bound snapshots. For a runtime version,
lookup selects the smallest stored version greater than or equal to it. With
`18.2.0/`, `26.1.0/`, and a root reference, runtimes through 18.2 use the first
snapshot, later runtimes through 26.1 use the second, and newer runtimes use the
root. An unversioned request uses the root directly. Lookup never synthesizes
intermediate versions.

The whole primary platform precedes fallback platforms, which use the same version
search. `GoldenPlatform` normalizes `macos` to `mac`. Every platform except
`portable` falls back to `portable`; the sole special chain is
`nanoserver -> windows -> portable`. Platforms are deduplicated. Docker platforms
also fall back to `portable` (there currently are no portable sample references).
Callers cannot supply arbitrary fallback lists.

`GoldenResolution` reports the selected key/location, ordered attempted
candidates, a `ReadBytes()` method and a readable trace. Missing results retain
the trace. Lookup never creates or approves a reference.

## Packaging

`tests/GoldenAssets.targets` packages the source selected by a host's
`GoldenRoot`. Each consumer project has its own output directory and copies its
selected references to `Goldens/{platform}/...`; device/browser
visual hosts set `EmbedGoldens=true` and use exact logical resource names:

```text
SkiaSharp.Goldens/{platform}/[{max-version}/]{case/path}[.variant][.architecture].png
```

`GoldenStore` accepts ordered disk roots and resource assemblies. Disk roots
precede embedded resources; the first source containing a key wins. Enumeration
within each source is ordinal and deterministic. The visual consumer configures
the output directory and `TestConfig.PathRoot`, then the entry, configured host
and test assemblies. Samples configure their packaged output directory.
Neither scans all loaded assemblies or relies on directory-name mangling.
Selected bytes are read fresh, without a shared byte cache.

## Comparison policy

The shared `GoldenComparisonOptions` and `GoldenVerifier` standardize image
decoding, dimensions, threshold evaluation and difference generation. Consumers
still choose policy. Visual comparisons remain renderer-specific:

| Renderer | Per-channel tolerance | Allowed outlier fraction |
| --- | --- | --- |
| `raster` | 2 | 0.002 |
| GPU renderers | 12 | 0.03 |

Sample comparisons retain zero channel tolerance, **0.075% differing pixels**
by default, exact dimensions and the existing **6% mean RGBA error** option for
animated GPU pages. See [sample guidance](building-samples.md).

## Running and reviewing captures

For managed-only work, bootstrap unchanged natives, then run the full console
solution. Native changes require building natives from source instead.

```sh
dotnet cake --target=externals-download
dotnet test tests/SkiaSharp.Tests.Console.slnx -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0

# Focused diagnostics; these do not replace the unfiltered solution gate.
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class "*GoldenReferenceTests"
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 -p:EmbedGoldens=true \
  -- --filter-class "*GoldenResourceTests"
python3 -m unittest discover -s scripts/infra/tests -p test_golden_images.py -v
```

Each image is emitted as one unversioned marker in the test output/TRX:

```text
##SKIA-GOLDEN## role=actual key=mac/Text.raster.arm64.png size=256x256 base64=...
```

Roles are `actual`, `reference` and `diff`. The key is relative to the consumer's
configured `GoldenRoot`; sample and visual runs already have different roots.
It describes the capture request, not a selected shared reference; the readable
lookup trace names that reference separately.

```sh
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx \
  --out output/golden-proposals/Visual --dry-run
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx \
  --out output/golden-proposals/Visual
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx --images-out output/golden-images
```

The exporter validates paths, PNG integrity and dimensions. Run it against one
consumer's results and choose that consumer's proposal root (`Visual` or
`Samples`). Proposals remain outside the committed reference tree. Conflicting
captures fail rather than silently selecting a winner.
Diagnostic export retains each run, role and retry separately.

Review pixels before promoting a proposal into `tests/Goldens`; never bless a
mismatch automatically. Re-run affected profiles after promotion. File-backed
captures retain a sibling `.diff.png`; in-memory captures attach the difference
directly to the test result.

## Extending the visual matrix

Add deterministic, parameterless `ISkiaScene` implementations under
`tests/Tests/SkiaSharp/Visual/Scenes/`. Avoid system fonts, clocks and randomness.
Renderer implementations live under `Visual/Renderers/`, desktop-only GL under
`Renderers/Desktop/`, and Vulkan renderers under `tests/VulkanTests/Visual/`.
Keep constructors cheap; do GPU work in `RenderAsync`. Register required
backends in `GpuPolicy` and serialize their rendering collection.

The matrix catalogs discover scenes/renderers only; they are not golden
catalogs. New `(renderer, scene)` pairs fail as unseeded until a reviewed
reference is committed.
