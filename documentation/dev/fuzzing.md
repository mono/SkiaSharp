# Fuzzing

SkiaSharp uses coverage-guided fuzzing (SharpFuzz + libFuzzer) both in the internal CI pipeline (via OneFuzz) and locally.

## Architecture

| Component | Path | Purpose |
|-----------|------|---------|
| **Fuzzing Project** | `tests/SkiaSharp.Fuzzing` | Defines `IFuzzer`, individual fuzzer classes in `Fuzzers/`, starting seed corpora in `seeds/<name>/`, and a top-level `Program.cs` (`--prepare <dir> [--instrument]` and `<fuzzer-name> [input]`). |
| **Cake Targets** | `scripts/infra/security/fuzzing/fuzzing.cake` | Downloads or selects the native driver, prepares cloud drops, and instruments and runs local fuzzing. Cloud preparation and local execution are independent. |
| **Docker Helper** | `scripts/infra/security/fuzzing/docker/run-local.sh` | Runs `dotnet cake --target=fuzzing` inside an Azure Linux 3.0 container (for macOS or containerized Linux runs). |
| **OneFuzz CLI** | `scripts/infra/security/fuzzing/onefuzz.ps1` | Cross-platform wrapper that downloads the internal OneFuzz CLI into `externals/onefuzz-cli` and forwards arguments. |

---

## Running Fuzzers Locally

Ensure native binaries exist in `./output/native` first (`dotnet cake --target=externals-download` or a local native build).
Use `externals-download` only for managed-only changes. After native/C API or Skia submodule changes, build the matching natives from source instead.

### Windows & Linux x64 (Native)

On Windows and Linux x64, `fuzzing-download` downloads the prebuilt `libfuzzer-dotnet` (`libfuzzer-dotnet-windows.exe` or `libfuzzer-dotnet-debian`) into `externals/fuzzing`. `SharpFuzz.CommandLine` is pinned in `.config/dotnet-tools.json` (`dotnet tool restore`).

```bash
# Fuzz a target for 60 seconds
dotnet cake --target=fuzzing --fuzzerTarget=image-streams --fuzzerMaxTime=60

# Replay a single crash input file
dotnet cake --target=fuzzing --fuzzerTarget=image-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123
```

### macOS / Docker

On macOS (or Linux arm64), use `run-local.sh` to run inside an Azure Linux container (which builds `/usr/local/bin/libfuzzer-dotnet` in a builder stage and sets `FUZZER_EXE`):

```bash
# Fuzz a target for 60 seconds in Docker
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=image-streams --fuzzerMaxTime=60

# Replay a crash input in Docker
./scripts/infra/security/fuzzing/docker/run-local.sh --fuzzerTarget=image-streams --fuzzerInput=output/fuzzing/local/crashes/crash-123
```

### Cake Targets & Arguments

- `fuzzing`: downloads `libfuzzer-dotnet` into `externals/fuzzing` (skipped when `FUZZER_EXE` / `--fuzzer` is provided), runs `SkiaSharp.Fuzzing --prepare output/fuzzing/local/bin --instrument` to stage and instrument the local assemblies, and runs `libfuzzer-dotnet`.
- `fuzzing-prepare`: runs `SkiaSharp.Fuzzing --prepare output/fuzzing/drop` to stage the per-platform cloud drops (`windows` and `linux`), each with its own `OneFuzzConfig.json`, in `output/fuzzing/drop/<platform>/`.

| Argument | Maps To | Description |
|----------|---------|-------------|
| `--fuzzerTarget=<name>` | `--target_arg=<name>` | Required for `fuzzing`. Which `IFuzzer` to run. |
| `--fuzzer=<path>` | `FUZZER_EXE` | Optional path to a custom `libfuzzer-dotnet` binary. |
| `--fuzzerInput=<path>` | `<path>` | Run a specific crash file (or directory) once and exit. |
| `--fuzzerMaxTime=<sec>` | `-max_total_time=<sec>` | Stop fuzzing after `<sec>` seconds (default: run until stopped or crash). |
| `--fuzzerRuns=<count>` | `-runs=<count>` | Stop after `<count>` test iterations. |
| `--fuzzerTimeout=<sec>` | `-timeout=<sec>` | Per-input hang timeout in seconds (default: `60`). |

Local fuzzing outputs are written to `output/fuzzing/local/`:
- `output/fuzzing/local/bin/`: staged + instrumented assemblies and runner binary
- `output/fuzzing/local/corpus/<target>/`: accumulated corpus across local runs
- `output/fuzzing/local/crashes/`: crash inputs saved by libFuzzer

### Debugging a Crash Input Directly in .NET

To debug a managed crash in Visual Studio / VS Code without `libfuzzer-dotnet`, pass the input file or folder directly to `SkiaSharp.Fuzzing`:

```bash
dotnet run --project tests/SkiaSharp.Fuzzing -- image-streams output/fuzzing/local/crashes/crash-123
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
3. Create and populate the OneFuzz seed corpus container (`skiasharp-my-feature-seeds`) as shown below.
4. Run `dotnet cake --target=fuzzing-prepare` to stage the drops and generate configs (`SkiaSharp.Fuzzing` discovers the new `IFuzzer` via reflection automatically).

---

## Preparing the Cloud Drop & Using the OneFuzz CLI

CI runs `dotnet cake --target=fuzzing-prepare` to populate `output/fuzzing/drop/{windows,linux}/` and submits each platform directory via `onefuzz-task@0`.

Each drop contains the managed assemblies and `.pdb`s, that platform's native libraries, `OneFuzzConfig.json`, and `assets/` (deployed with `PreservePathsJobDependencies: ["./assets/**/*"]`).

Cloud jobs read their initial seeds from the `skiasharp-<target>-seeds` storage container (`SeedCorpusContainer`), shared by the `windows` and `linux` submissions:

```bash
# Create and upload a target's seed corpus container
./scripts/infra/security/fuzzing/onefuzz.ps1 containers create --name skiasharp-image-streams-seeds
./scripts/infra/security/fuzzing/onefuzz.ps1 containers files upload-dir skiasharp-image-streams-seeds tests/SkiaSharp.Fuzzing/seeds/image-streams
```

To inspect cloud submissions, jobs, or crash containers with the OneFuzz CLI (requires PowerShell 7+, .NET 10, and `az login` with access to `fuzzfest/Onefuzz`):

```bash
./scripts/infra/security/fuzzing/onefuzz.ps1 utils status
./scripts/infra/security/fuzzing/onefuzz.ps1 jobs get --job-id <guid>
./scripts/infra/security/fuzzing/onefuzz.ps1 containers files download-dir <crashes-container> output/fuzzing/local/crashes
```
