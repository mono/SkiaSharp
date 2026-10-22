# Golden-image tests

Visual tests render each scene through every backend required by the host's
[GPU policy](gpu-test-policy.md). Sample tests compare application output and
browser screenshots. Both use reviewed PNG references and the same deterministic
key resolver; each consumer owns its file/resource enumeration and image loading.

## Reference paths

```text
tests/Goldens/{Samples|Visual}/{profile}/{version-or-base}/{case}[.variant][.architecture].png
```

Profiles, cases, variants and architectures are lowercase alphanumeric/kebab
segments. Versions are actual numeric product/platform versions, optionally
ending in `.rN` for an image revision. `base` means explicitly unversioned.
Architectures use .NET architecture names (`x64`, `arm64`, `wasm`, etc.);
those tokens are reserved and cannot also name functional variants.
`latest` is input-only: a caller must resolve it to an actual version before
creating a request or capture key.

Examples:

```text
Visual/mac/base/visual-diagonal-lines.ganesh-gl.arm64.png
Visual/portable/base/visual-filled-circle.raster.png
Samples/mac/base/browser-wasm-page.png
Samples/docker-linux/base/console.png
```

The existing visual and sample references remain `base` references. This does
not infer a product version from an old screenshot. Sample host profiles are
`mac`, `linux` and `windows`; container profiles are `docker-linux` and
`docker-windows`, so container and host output cannot accidentally share a
baseline.

## Lookup rules

`GoldenRequest` carries the profile, actual version or `base`, case,
architecture, optional functional variant, version scope, ordered fallback
profiles and an explicit neutral-variant policy.

For each eligible version, candidate filenames are ordered:

```text
case.variant.architecture.png
case.variant.png
case.architecture.png          # only if neutral-variant fallback is enabled
case.png                      # only if neutral-variant fallback is enabled
```

Without a variant, only the architecture-specific and neutral filenames apply.
A named variant never borrows a sibling variant; another architecture is never
substituted. Visual tests use the renderer as the variant and disallow neutral
variants, so a missing GPU reference cannot resolve to raster pixels.

Versioned lookup tries the exact version, its explicit minor/family baselines,
then available older captures in descending numeric order, then `base`.
`Major` scope bounds older captures to the same major; `Minor` bounds them to
the same major/minor and never tries a major-only baseline. Choose scope by
product family: for example, Ubuntu `22.04` and `22.10` are separate minor
families, as are macOS `10.14` and `10.15`. Future captures are never eligible.
`Unversioned` scope tries only `base`.

The whole primary profile precedes fallback profiles. Fallbacks are caller-owned,
ordered and **base-only**; there is no inferred cross-product/platform fallback.
Visual tests preserve their existing platform-to-`portable` lookup, including
`nanoserver` to `windows` to `portable`. Sample tests have no profile fallback.

`GoldenResolution` reports the selected key/location, ordered attempted
candidates and a readable trace. Missing references fail with that trace; lookup
never creates or approves a reference.

## Packaging

`tests/GoldenAssets.targets` packages the domain selected by a host's
`GoldenRoot`. Desktop tests copy files to `Goldens/{profile}/...`; device/browser
visual hosts set `EmbedGoldens=true` and use exact logical resource names:

```text
SkiaSharp.Goldens/{profile}/{version-or-base}/{case}[.variant][.architecture].png
```

The visual consumer probes the output directory and `TestConfig.PathRoot`, then
the entry, configured host and shared test assemblies. It does not scan all
loaded assemblies or rely on MSBuild's directory-name mangling. Images are loaded
and decoded by the consumer, not cached by shared resolution code.

## Comparison policy

This naming/lookup model does not set tolerances, resize/crop images or change
renderers. Visual `GoldenTolerance` remains renderer-specific:

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
##SKIA-GOLDEN## role=actual key=Visual/mac/base/visual-text.raster.arm64.png size=256x256 base64=...
```

Roles are `actual`, `reference` and `diff`. The key always describes the capture
request, not a selected shared baseline; the readable lookup trace names that
baseline separately.

```sh
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx --dry-run
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx --images-out output/golden-images
```

The exporter validates paths, PNG integrity and dimensions. Default output is
`output/golden-proposals/{Samples|Visual}/...`, never the committed reference
tree. Conflicting captures fail rather than silently selecting a winner.
Diagnostic export retains each run, role and retry separately.

Review pixels before promoting a proposal into `tests/Goldens`; never bless a
mismatch automatically. Re-run affected profiles after promotion. Visual
failures also retain `_visualfailures/{renderer}/{scene}.actual.png` and
`.diff.png` when the host has a writable output directory.

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
