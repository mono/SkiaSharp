# Golden-image tests

Visual and sample tests compare rendered output with reviewed PNG references.
The shared, non-shipping implementation lives in `SkiaSharp.Tests.Shared`.

## Using a golden

Resolve the test case, then verify the form of image the test already has:

```csharp
var golden = store.Resolve(new(
    profile: "mac",
    casePath: "DiagonalLines",
    architecture: "arm64",
    variant: "raster"));

golden.Verify(
    GoldenInput.Path(actualPath),
    new(expectedSize: new(256, 256), channelTolerance: 2));

golden.Verify(GoldenInput.Bytes(encodedImage, "actual.png"));
golden.Verify(GoldenInput.Pixels(pixels, imageInfo));

await golden.VerifyAsync(
    GoldenInput.Capture(CaptureScreenshot),
    TimeSpan.FromSeconds(10),
    cancellationToken);
```

Use the static form for a direct comparison without reference lookup:

```csharp
Golden.Verify(GoldenInput.Path(actualPath), GoldenInput.Path(expectedPath));
```

`GoldenInput.Bytes(...)` accepts any format Skia can decode. Diagnostics,
attachments and generated differences are normalized to PNG.

## References

References use:

```text
tests/Goldens/{Samples|Visual}/{profile}/[{max-version}/]{case/path}[.variant][.architecture].png
```

The profile root is the current reference. Optional version directories are
inclusive upper-bound snapshots.

Lookup tries the functional variant with the requested architecture, then the
same variant without an architecture. Named variants never borrow sibling or
unqualified references.

The requested profile is exhausted before fallback:

- `nanoserver -> windows -> portable`
- other non-portable profiles -> `portable`

`tests/GoldenAssets.targets` copies references for normal hosts or embeds them
as deterministic `SkiaSharp.Goldens/...` resources for device and browser hosts.

## Comparison policy

Visual tests use:

| Renderer | Channel tolerance | Allowed differing pixels |
| --- | --- | --- |
| `raster` | 2 | 0.2% |
| GPU renderers | 12 | 3% |

Sample tests use zero channel tolerance and allow 0.075% differing pixels.
Animated GPU sample pages may instead use a 6% mean RGBA error limit.

## Running and reviewing

For managed-only changes:

```sh
dotnet cake --target=externals-download
dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0
```

Focused checks:

```sh
dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -- --filter-class "*GoldenReferenceTests"

dotnet test tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj \
  -p:TargetFramework=net10.0 -p:TargetFrameworks=net10.0 \
  -p:EmbedGoldens=true -- --filter-class "*GoldenResourceTests"
```

Resolved verifications emit `##SKIA-GOLDEN##` records to the test result.
Extract proposals outside the committed reference tree:

```sh
python3 scripts/infra/tests/extract-visual-goldens.py path/to/results.trx \
  --out output/golden-proposals/Visual
```

Review proposed pixels before moving them into `tests/Goldens`; references are
never accepted automatically. File-backed failures retain a sibling
`.diff.png`; in-memory failures attach the difference to the test result.

## Adding visual coverage

Add deterministic `ISkiaScene` implementations under
`tests/Tests/SkiaSharp/Visual/Scenes/`. Avoid system fonts, clocks and randomness.
Register required backends in `GpuPolicy`. New renderer/scene pairs fail until a
reviewed reference is committed.
