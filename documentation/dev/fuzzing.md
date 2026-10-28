# Fuzzing

SkiaSharp is fuzzed with coverage-guided fuzzing (SharpFuzz + libFuzzer), both locally and in the internal CI pipeline
through OneFuzz. This guide covers how to run and debug a fuzzer, why we have our own fuzzers, how to write a good one,
and how the cloud pipeline works.

- [Quickstart](#quickstart)
- [What is fuzzing, and why fuzz SkiaSharp?](#what-is-fuzzing-and-why-fuzz-skiasharp)
- [Writing a good fuzzer](#writing-a-good-fuzzer)
- [Cloud fuzzing with OneFuzz](#cloud-fuzzing-with-onefuzz)
- [Further reading](#further-reading)

---

## Quickstart

Native binaries must exist in `output/native/` before anything runs. For managed-only changes use
`dotnet cake --target=externals-download`. After any native, C API, or Skia submodule change, build the natives from
source instead (see [AGENTS.md](../../AGENTS.md)); never use `externals-download` in that case.

### Run a fuzzer locally

**Windows or Linux x64** (runs natively; `libfuzzer-dotnet` is downloaded into `externals/fuzzing`):

```bash
dotnet tool restore   # SharpFuzz.CommandLine is pinned in .config/dotnet-tools.json
dotnet cake --target=fuzzing --fuzzerTarget=image-streams --fuzzerMaxTime=60
```

**macOS (or Linux arm64)** — `libfuzzer-dotnet` has no macOS build, so run inside an Azure Linux container. The script
builds the image, mounts the repo, and runs the same Cake target inside it:

```bash
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=image-streams --fuzzerMaxTime=60
```

Without `--fuzzerMaxTime` or `--fuzzerRuns` it fuzzes until you stop it or it finds a crash.

### Replay and debug a crash input

Crashes are saved to `output/fuzzing/local/crashes/`. To debug in Visual Studio / VS Code, bypass libFuzzer and run the
fuzzer on the file (or a folder of files) directly in .NET. This uses the normal, un-instrumented build:

```bash
dotnet run --project tests/SkiaSharp.Fuzzing -- image-streams output/fuzzing/local/crashes/crash-123
```

To replay through libFuzzer instead (instrumented, same as the fuzz run), pass `--fuzzerInput`. The path is relative to
the repo root and can be a file or a directory:

```bash
# Windows or Linux x64
dotnet cake --target=fuzzing --fuzzerTarget=image-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123

# macOS / Docker
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=image-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123
```

Each fuzzer lives in `tests/SkiaSharp.Fuzzing/Fuzzers/` with its starting corpus in `tests/SkiaSharp.Fuzzing/seeds/<name>/`.
Run `dotnet run --project tests/SkiaSharp.Fuzzing` with no arguments to list all discovered fuzzers.

### Cake targets and arguments

| Target | What it does |
|--------|--------------|
| `fuzzing` | Local fuzzing. Downloads `libfuzzer-dotnet` (skipped when `--fuzzer`/`FUZZER_EXE` is set), stages and instruments the assemblies into `output/fuzzing/local/bin/`, then runs `libfuzzer-dotnet`. |
| `fuzzing-prepare` | Cloud drop. Stages un-instrumented per-platform drops with a `OneFuzzConfig.json` into `output/fuzzing/drop/{windows,linux}/`. Used by CI; see [below](#cloud-fuzzing-with-onefuzz). |

| Argument | Maps to | Description |
|----------|---------|-------------|
| `--fuzzerTarget=<name>` | `--target_arg=<name>` | **Required** for `fuzzing`. Which `IFuzzer` to run. |
| `--fuzzer=<path>` | `FUZZER_EXE` | Use your own `libfuzzer-dotnet` binary instead of downloading one. |
| `--fuzzerInput=<path>` | positional input | Run a crash file or directory once and exit instead of fuzzing. |
| `--fuzzerMaxTime=<sec>` | `-max_total_time` | Stop after this many seconds. Default: run until stopped. |
| `--fuzzerRuns=<count>` | `-runs` | Stop after this many iterations. |
| `--fuzzerTimeout=<sec>` | `-timeout` | Per-input hang timeout. Default: `60`. |

Local outputs, all under `output/fuzzing/local/`:

| Path | Contents |
|------|----------|
| `bin/` | Staged, SharpFuzz-instrumented assemblies and the runner. |
| `corpus/<target>/` | Corpus that accumulates across local runs. |
| `crashes/` | Crash, hang, and OOM inputs saved by libFuzzer (`crash-*`, `timeout-*`, `oom-*`). |

---

## What is fuzzing, and why fuzz SkiaSharp?

### How it works here

A coverage-guided fuzzer mutates inputs, watches which code paths each input reaches, and keeps the inputs that reach
something new. Over millions of cheap iterations it finds the odd inputs a human would never write.

For .NET that takes two pieces:

- **[SharpFuzz](https://github.com/Metalnem/sharpfuzz)** rewrites the managed assemblies at the IL level so each branch
  reports coverage. The `fuzzing` target instruments `SkiaSharp.dll`, `HarfBuzzSharp.dll`, `SkiaSharp.Skottie.dll`,
  `SkiaSharp.Resources.dll`, and `SkiaSharp.SceneGraph.dll`.
- **[libfuzzer-dotnet](https://github.com/Metalnem/libfuzzer-dotnet)** bridges LLVM's libFuzzer mutation engine to the
  managed process. It runs our harness as a child and hands each input over shared memory to
  `FuzzTarget(ReadOnlySpan<byte> bytes)`, in-process, with no process spawn per iteration.

Coverage is only collected from managed code. The native libraries (`libSkiaSharp`, `libHarfBuzzSharp`) are not
instrumented, so a fuzzer finds native bugs only by driving them from C# and seeing a crash or hang, not by guiding on
native branches.

A finding is anything libFuzzer reports: an unhandled managed exception, a native crash or access violation, a hang
(over the timeout), or an out-of-memory condition.

### Why we fuzz when OSS-Fuzz already fuzzes Skia

Google's OSS-Fuzz continuously fuzzes native Skia and HarfBuzz (see `externals/skia/fuzz/oss_fuzz/`). That does not
cover what SkiaSharp adds on top. Those C++ fuzzers hand an `SkMemoryStream` or raw pointer straight to the engine, so
they never touch:

- SkiaSharp's own **C# parsers**, such as `SKColor.TryParse`, `SKPath.ParseSvgPathData`, and the HarfBuzz `Tag`/`Feature`
  parsers.
- **Span and pointer marshaling**: pinning, lengths, and buffer ownership at the P/Invoke boundary.
- **Object lifetime and GC rooting**: handles, finalizers, `owns:` flags, and anything the native side keeps alive.
- **Managed ↔ native callback proxies**: `SKManagedStream`, `SKFrontBufferedManagedStream`, `SKManagedWStream`, and the
  HarfBuzzSharp delegates, where native code calls back into C# while parsing untrusted data.

Driving the native decoders through those managed adapters exercises both layers together. It also reaches code the C++
fuzzers do not: native codecs take different paths for seekable and non-seekable streams, so a managed stream that
claims it cannot seek triggers behavior a memory stream never will. This is why the stream fuzzers run every input in
both modes.

### Fuzzers vs. unit tests

| | Unit test | Fuzzer |
|---|-----------|--------|
| Input | One known input you chose | Open-ended space explored by mutation |
| Asserts | A specific expected output | "Nothing bad happens" |
| Good for | Edge cases you can name: null or empty, disposal order, missing files | Bugs you cannot predict: unhandled exceptions, native crashes, hangs, OOM |
| Runs | Every CI build, in seconds | Continuously, for hours or days |

They complement each other. **Every bug a fuzzer finds should leave two things behind:**

1. A deterministic regression unit test in `tests/Tests/` that reproduces it without any fuzzing tooling.
2. The minimized crashing input added as a seed in `tests/SkiaSharp.Fuzzing/seeds/<target>/`, so the fuzzer keeps
   exploring around it.

---

## Writing a good fuzzer

These rules combine Google's
[What makes a good fuzz target](https://github.com/google/fuzzing/blob/master/docs/good-fuzz-target.md) and
[Splitting inputs](https://github.com/google/fuzzing/blob/master/docs/split-inputs.md), the conventions in
[`dotnet/runtime` fuzzing](https://github.com/dotnet/runtime/blob/main/src/libraries/Fuzzing/README.md), and the
patterns in upstream Skia's `externals/skia/fuzz/oss_fuzz/`. The snippets below are from the real fuzzers.

### The contract

A fuzzer is a class implementing `IFuzzer`. `Program.cs` finds it by reflection, so there is no registration step:

```csharp
public interface IFuzzer
{
    static abstract string Name { get; }
    static abstract void FuzzTarget(ReadOnlySpan<byte> bytes);
}
```

`FuzzTarget` runs millions of times in one process. Any unhandled exception, native crash, or hang is a finding. So
**expected failures must not throw**: factories that return `null` for bad input are fine; wrapping the body in
`try/catch` to hide real bugs is not.

### One fuzzer per input format

Do not build one multiplexer fuzzer that sniffs the input and feeds PNGs, pictures, and fonts to different code. Mutating
a PNG header is meaningful; mutating the "which format" selector and the PNG at once is not. Split by format family, as
we do with `image-streams`, `picture-streams`, and `typeface-streams`. Each gets a seed corpus of valid files that stay
valid for that target.

### Pass standard formats through unmodified

If the input is a standard format (PNG, SKP, TTF, Lottie JSON), hand `bytes` to the parser as-is. Do not steal header or
trailer bytes as mode flags or selectors; that corrupts the format and breaks every seed.

To cover a small number of modes, such as seekable vs. non-seekable streams, run the same input through each mode
deterministically. `PictureStreamsFuzzer` is the canonical shape:

```csharp
public static void FuzzTarget(ReadOnlySpan<byte> bytes)
{
    if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
        return;

    var payload = bytes.ToArray();

    Fuzz(payload, seekable: true);
    Fuzz(payload, seekable: false);
}

private static void Fuzz(byte[] payload, bool seekable)
{
    using var stream = new FuzzStream(payload, seekable);
    using var picture = SKPicture.Deserialize(stream);
    if (picture is null)
        return;

    using var bitmap = new SKBitmap(64, 64);
    using var canvas = new SKCanvas(bitmap);
    canvas.DrawPicture(picture);
    picture.Playback(canvas);

    using var output = new FuzzWStream(seekable);
    picture.Serialize(output);
}
```

`FuzzStream` and `FuzzWStream` (`FuzzStreams.cs`) are plain .NET `Stream`s over a byte array that can pretend to be
non-seekable. Reuse them for any new stream-based fuzzer.

The same idea applies when a mode really is a different API shape rather than a flag in the data:
`HarfBuzzShapingFuzzer` shapes the same slice as UTF-8, UTF-16, and UTF-32 by calling a helper per encoding.

### Drive deferred work to completion

Parsing a file often does not touch the interesting code. Decoders and deserializers are frequently lazy, so a fuzzer
that only creates the object tests a fraction of the surface.

- `PictureStreamsFuzzer` calls `DrawPicture` and `Playback`. `SKPicture.Deserialize` reads the stream eagerly, but the
  embedded images and drawing opcodes are not decoded or executed until the picture is played back.
- `ImageStreamsFuzzer` calls `GetPixels` (and `DrawImage` for `SKImage`) so the codec actually decodes pixels, walks
  animation frames, and re-encodes the result, rather than only reading the header.

Ask of every new fuzzer: what happens after parse that a real caller would do, and does the fuzzer do it?

### Don't guess at what the parsed object contains

When probing a parsed object, query what the fuzzed input actually provides rather than a hardcoded sample. A fixed
string like `"Hello"` is useless against a mutated font whose `cmap` no longer maps those characters, so the fuzzer
exercises nothing. `TypefaceStreamsFuzzer` follows Skia's `FuzzCOLRv1.cpp`, which walks glyph IDs directly:

```csharp
var glyphCount = Math.Min(typeface.GlyphCount, 16);
if (glyphCount <= 0)
    return;

Span<ushort> glyphs = stackalloc ushort[glyphCount];
for (var i = 0; i < glyphCount; i++)
    glyphs[i] = (ushort)i;

using var font = new SKFont(typeface, 16f);
font.GetGlyphWidths(glyphs, widths, Span<SKRect>.Empty);
font.GetGlyphPositions(glyphs, positions);
```

### Keep iterations fast, deterministic, and bounded

Throughput is the whole game. A slow or flaky iteration costs coverage for every run that follows.

- **Reject bad sizes up front.** Every fuzzer returns early for empty input or input over a per-fuzzer
  `MaximumInputBytes`.
- **Cap what the input can make you allocate.** A 50-byte header can claim a `65535×65535` image. `ImageStreamsFuzzer`
  checks the decoded size before allocating anything, so a crafted header can't OOM the runner:

  ```csharp
  var info = codec.Info;
  if (info.Width <= 0 || info.Height <= 0 ||
      info.Width > MaximumDimension || info.Height > MaximumDimension ||
      info.BytesSize64 <= 0 || info.BytesSize64 > MaximumDecodedBytes)
      return;
  ```

  Output surfaces are small and fixed (`64×64`, `32×32`) for the same reason.
- **No disk I/O per iteration.** If a fuzzer needs a fixed scaffold asset, load it once. `HarfBuzzShapingFuzzer` shapes
  fuzzed text against `assets/font.ttf` and reads it through a `Lazy<byte[]>`:

  ```csharp
  private static readonly Lazy<byte[]> FontBytes = new(LoadFontBytes);
  ```
- **No randomness, time, or global state** that makes the same input behave differently between runs. A crash you cannot
  replay is not a bug report.
- **Keep seeds tiny**: hundreds of bytes to a few KB. libFuzzer is much faster on a 16×16 image than a 512×512 photo,
  and mutates it more usefully.
- **Dispose everything** with `using`. A leak per iteration becomes an OOM an hour in and hides the real findings.

### Adding a new fuzzer

1. Add a class implementing `IFuzzer` in `tests/SkiaSharp.Fuzzing/Fuzzers/` (use an existing fuzzer as the template).
   Pick a kebab-case `Name`; it becomes the Cake `--fuzzerTarget`, the seed folder, and the cloud seed container name.

   ```csharp
   namespace SkiaSharp.Fuzzing.Fuzzers;

   public sealed class MyFeatureFuzzer : IFuzzer
   {
       private const int MaximumInputBytes = 1 * 1024 * 1024;

       public static string Name => "my-feature";

       public static void FuzzTarget(ReadOnlySpan<byte> bytes)
       {
           if (bytes.IsEmpty || bytes.Length > MaximumInputBytes)
               return;

           // Parse bytes, then drive the result to completion.
       }
   }
   ```

2. Add a few small, valid seed files to `tests/SkiaSharp.Fuzzing/seeds/my-feature/`.
3. Smoke test the seeds directly in .NET (fast, debuggable, no libFuzzer needed):

   ```bash
   dotnet run --project tests/SkiaSharp.Fuzzing -- my-feature tests/SkiaSharp.Fuzzing/seeds/my-feature
   ```

4. Fuzz it for a minute locally and fix anything it finds:

   ```bash
   dotnet cake --target=fuzzing --fuzzerTarget=my-feature --fuzzerMaxTime=60
   ```

5. Run `dotnet cake --target=fuzzing-prepare` and check that the new entry appears in both generated
   `OneFuzzConfig.json` files (see [validation](#validate-the-drop-locally)).
6. Create and populate the `skiasharp-my-feature-seeds` OneFuzz container
   (see [seed containers](#seed-containers)). The cloud job reads its seeds from there, not from the repo.

---

## Cloud fuzzing with OneFuzz

In CI the internal `skiasharp-package` pipeline has a `fuzzing` stage (`scripts/azure-templates-stages-fuzzing.yml`)
with two jobs:

1. **Prepare** runs `dotnet cake --target=fuzzing-prepare`, which builds the harness and stages one drop per platform in
   `output/fuzzing/drop/{windows,linux}/`. The drop is published as a pipeline artifact.
2. **Submit** runs on a fresh agent that never builds anything. It downloads the drop and submits each platform
   directory with `onefuzz-task@0`.

Each platform directory contains the un-instrumented managed assemblies and `.pdb`s, that platform's native libraries,
`assets/`, and `OneFuzzConfig.json`, which has one entry per discovered `IFuzzer`. OneFuzz instruments the target
assemblies itself on its workers. Each entry points at its seed container, `skiasharp-<target>-seeds`, shared by the
`windows` and `linux` submissions.

### Validate the drop locally

`fuzzing-prepare` runs on any OS, so you can generate and check a drop before CI does:

```bash
dotnet cake --target=fuzzing-prepare

./scripts/infra/security/fuzzing/onefuzz.ps1 onefuzzconfig validate \
    --config output/fuzzing/drop/linux/OneFuzzConfig.json \
    --drop-path output/fuzzing/drop/linux
```

Without `--drop-path` it only validates the schema against the service. With it, it also checks that every file in each
entry's `JobDependencies` exists inside the drop.

### The OneFuzz CLI wrapper

`scripts/infra/security/fuzzing/onefuzz.ps1` runs the internal OneFuzz CLI on any platform and forwards all arguments to
it. The first run downloads the CLI into `externals/onefuzz-cli`. Delete that folder or use the CLI's own
`onefuzz update` to refresh it. It requires PowerShell 7+, the .NET 10 runtime, and `az login` with access to the
`fuzzfest/Onefuzz` Azure DevOps feed. Run `onefuzz.ps1 --help` or `<command> --help` for the full option list.

### Seed containers

Seeds are uploaded once per target and read by every submission. Re-upload after adding or changing seeds.

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 containers create --name skiasharp-image-streams-seeds
./scripts/infra/security/fuzzing/onefuzz.ps1 containers files upload-dir skiasharp-image-streams-seeds tests/SkiaSharp.Fuzzing/seeds/image-streams
```

### Submit a drop from the CLI

Normally CI submits for you. To submit by hand, for example to try a change before merging, point `submit` at a platform
directory. `--platform` is `azurelinux3-x64` for the `linux` drop and `windows-server-2026-x64` for the `windows` drop.
Start with `--dry-run`, which checks the artifacts, config, and connectivity without uploading:

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 submit \
    --platform azurelinux3-x64 \
    --artifacts-path output/fuzzing/drop/linux \
    --branch <your-branch> \
    --dry-run
```

Remove `--dry-run` to submit for real; add `--wait-for-submission` to poll until it finishes and `--force` to bypass
de-duplication of identical submissions.

### Inspect submissions, jobs, and crashes

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 utils status
./scripts/infra/security/fuzzing/onefuzz.ps1 submissions --help
./scripts/infra/security/fuzzing/onefuzz.ps1 jobs get --job-id <guid>
./scripts/infra/security/fuzzing/onefuzz.ps1 containers files download-dir <crashes-container> output/fuzzing/local/crashes
```

Once a crash is downloaded to `output/fuzzing/local/crashes/`, replay it with the
[commands in the Quickstart](#replay-and-debug-a-crash-input), then follow the
[fuzzers vs. unit tests](#fuzzers-vs-unit-tests) rule: add a regression test and a seed.

---

## Further reading

- [Google Fuzzing: What makes a good fuzz target](https://github.com/google/fuzzing/blob/master/docs/good-fuzz-target.md)
- [Google Fuzzing: Splitting inputs](https://github.com/google/fuzzing/blob/master/docs/split-inputs.md)
- [LLVM libFuzzer documentation](https://llvm.org/docs/LibFuzzer.html)
- [`dotnet/runtime` fuzzing documentation](https://github.com/dotnet/runtime/blob/main/src/libraries/Fuzzing/README.md)
- [Skia fuzzing documentation](https://skia.org/docs/dev/testing/fuzz/) and the upstream OSS-Fuzz targets in
  [`externals/skia/fuzz/oss_fuzz/`](../../externals/skia/fuzz/oss_fuzz)
- [SharpFuzz](https://github.com/Metalnem/sharpfuzz) and
  [libfuzzer-dotnet](https://github.com/Metalnem/libfuzzer-dotnet)
