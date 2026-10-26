# Fuzzing

SkiaSharp uses coverage-guided fuzzing (SharpFuzz + libFuzzer) both in the internal CI pipeline (via OneFuzz) and locally.

## Architecture

| Component | Path | Purpose |
|-----------|------|---------|
| **Fuzzing Project** | `tests/SkiaSharp.Fuzzing` | Defines `IFuzzer`, individual fuzzer classes in `Fuzzers/`, starting seed corpora in `seeds/<name>/`, and a top-level `Program.cs` (`--prepare <dir> [--instrument <sharpfuzz>]` and `<fuzzer-name> [input]`). |
| **Cake Targets** | `scripts/infra/security/fuzzing/fuzzing.cake` | Provides `fuzzing-prepare` (CI drop preparation) and `fuzzing` (`Default`: prepare + local libFuzzer execution). |
| **Docker Helper** | `scripts/infra/security/fuzzing/docker/run-local.sh` | Runs `dotnet cake --target=fuzzing` inside an Azure Linux 3.0 container (for macOS or containerized Linux runs). |
| **OneFuzz CLI** | `scripts/infra/security/fuzzing/onefuzz.ps1` | Cross-platform wrapper that downloads the internal OneFuzz CLI into `externals/onefuzz-cli` and forwards arguments. |

### Available Fuzzers

| Name | Class | What It Exercises |
|------|-------|-------------------|
| `managed-streams` | `ManagedStreamsFuzzer` | `SKCodec`, `SKBitmap`, `SKPicture`, and `SKFontManager` over seekable and non-seekable managed streams (normal reads, short reads, and early end-of-stream). |
| `picture-deserialize` | `PictureDeserializeFuzzer` | `SKPicture.Deserialize` from `ReadOnlySpan<byte>`, `SKData`, and `Stream`, followed by canvas playback. |
| `harfbuzz-managed-callbacks` | `HarfBuzzManagedCallbacksFuzzer` | `HarfBuzzSharp.Face` managed table callbacks (normal tables, truncated tables, missing tags, and disposal ordering). |
| `skottie-resource-providers` | `SkottieResourceProvidersFuzzer` | `FileResourceProvider`, `DataUriResourceProvider`, `CachingResourceProvider`, and Skottie animation seeking/rendering. |

---

## Running Fuzzers Locally

Ensure native binaries exist in `./output/native` first (`dotnet cake --target=externals-download` or a local native build).

### Windows & Linux x64 (Native)

On Windows and Linux x64, `fuzzing-download` downloads the prebuilt `libfuzzer-dotnet` (`libfuzzer-dotnet-windows.exe` or `libfuzzer-dotnet-debian`) and `sharpfuzz` into `externals/fuzzing`:

```bash
# Fuzz a target for 60 seconds
dotnet cake --target=fuzzing --fuzzerTarget=managed-streams --fuzzerMaxTime=60

# Replay a single crash input file
dotnet cake --target=fuzzing --fuzzerTarget=managed-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123
```

### macOS / Docker

`libfuzzer-dotnet` and `SharpFuzz` only support Windows and Linux (LLVM's Darwin `libFuzzer` runtime stubs out `ExtraCountersBegin`/`ExtraCountersEnd` as null, and `SharpFuzz` uses Linux `libc` `shmat`). On macOS (or Linux arm64), use `run-local.sh` to run inside an Azure Linux container (which builds `/usr/local/bin/libfuzzer-dotnet` and sets `FUZZER_EXE`):

```bash
# Fuzz a target for 60 seconds in Docker
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=managed-streams --fuzzerMaxTime=60

# Replay a crash input in Docker
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=managed-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123
```

### Cake Targets & Arguments

- `fuzzing-download`: installs `SharpFuzz.CommandLine` and downloads `libfuzzer-dotnet` into `externals/fuzzing` (skipped when `FUZZER_EXE` / `--fuzzer` is provided).
- `fuzzing-prepare`: publishes `SkiaSharp.Fuzzing` and stages the per-platform drops (`windows`, `ubuntu`, `azurelinux`), each with its own `OneFuzzConfig.json`, in `output/fuzzing/drop/<platform>/`.
- `fuzzing` (`Default`: `fuzzing-download` + `fuzzing-prepare` + `fuzzing-run`): instruments the target assemblies and runs `libfuzzer-dotnet`.

| Argument | Maps To | Description |
|----------|---------|-------------|
| `--fuzzerTarget=<name>` | `--target_arg=<name>` | Required for `fuzzing`. Which `IFuzzer` to run. |
| `--fuzzer=<path>` | `FUZZER_EXE` | Optional path to a custom `libfuzzer-dotnet` binary. |
| `--fuzzerInput=<path>` | `<path>` | Run a specific crash file (or directory) once and exit. |
| `--fuzzerMaxTime=<sec>` | `-max_total_time=<sec>` | Stop fuzzing after `<sec>` seconds (default: run until stopped or crash). |
| `--fuzzerRuns=<count>` | `-runs=<count>` | Stop after `<count>` test iterations. |
| `--fuzzerTimeout=<sec>` | `-timeout=<sec>` | Per-input hang timeout in seconds (default: `60`). |

Local fuzzing outputs are written to `output/fuzzing/local/`:
- `output/fuzzing/local/bin/`: instrumented assemblies and runner binary
- `output/fuzzing/local/corpus/<target>/`: accumulated corpus across local runs
- `output/fuzzing/local/crashes/`: crash inputs saved by libFuzzer

### Debugging a Crash Input Directly in .NET

To debug a managed crash in Visual Studio / VS Code without `libfuzzer-dotnet`, pass the input file or folder directly to `SkiaSharp.Fuzzing`:

```bash
dotnet run --project tests/SkiaSharp.Fuzzing -- managed-streams output/fuzzing/local/crashes/crash-123
```

---

## Adding a New Fuzzer

1. Add a class implementing `IFuzzer` under `tests/SkiaSharp.Fuzzing/Fuzzers/`:

```csharp
namespace SkiaSharp.Fuzzing.Fuzzers;

public sealed class MyFeatureFuzzer : IFuzzer
{
    public static string Name => "my-feature";

    public static void FuzzTarget(ReadOnlySpan<byte> bytes)
    {
        // Exercise the target APIs on bytes
    }
}
```

2. Add one or more starting seed files under `tests/SkiaSharp.Fuzzing/seeds/my-feature/`.
3. Run `dotnet cake --target=fuzzing-prepare` to verify the seed smoke check and config generation. `SkiaSharp.Fuzzing` discovers the new `IFuzzer` via reflection automatically.

---

## Preparing the Cloud Drop & Using the OneFuzz CLI

CI runs `dotnet cake --target=fuzzing-prepare` to populate `output/fuzzing/drop/<platform>/` and submits each platform directory via `onefuzz-task@0`.

To inspect cloud submissions, jobs, or crash containers with the OneFuzz CLI (requires PowerShell 7+, .NET 10, and `az login` with access to `fuzzfest/Onefuzz`):

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 utils status
./scripts/infra/security/fuzzing/onefuzz.ps1 jobs get --job-id <guid>
./scripts/infra/security/fuzzing/onefuzz.ps1 containers download --name <crashes-container> -o output/fuzzing/local/crashes
```
