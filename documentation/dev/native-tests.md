# Native artifact tests

`tests/SkiaSharp.Tests.Native` inspects the built native libraries for every platform: imports and dependencies, symbols, glibc versions, page alignment, Apple/WASM/Tizen packaging, and so on. Each case is a PowerShell script in `tests/SkiaSharp.Tests.Native/Cases/`. The Cake target `tests-native` builds and runs the suite and writes the results to `output/logs/testlogs/SkiaSharp.Tests.Native/`.

Windows checks cover hard/delay imports, CRT policy, and symbols. They do not replace Windows App Certification Kit (WACK) certification or application suspend/resume testing.

## Native artifacts

The suite checks the complete native set and does not download anything. Put the artifacts from a CI build in the same layout CI uses:

| CI artifact | Location |
|---|---|
| `native` | `output/native/` |
| `native_msvc` | `output/msvc/native/` |

## Running locally

Requires the repository's .NET SDK, PowerShell 7.4 or later, and the LLVM tools (`llvm-readobj`, `llvm-nm`, `llvm-objdump`, `llvm-ar`, `llvm-pdbutil`) on `PATH`. A missing tool fails the test; the suite never looks for an alternative.

Windows native compilation uses the LLVM component in the selected Visual Studio installation (`VC\Tools\Llvm\x64`), configured by `scripts/infra/native/windows/select-llvm.ps1`. That component does not necessarily include the artifact-inspection tools above. Install the full [LLVM toolchain](https://releases.llvm.org/) for these tests and explicitly add its `bin` directory to `PATH`; do not replace the compilation `LLVM_HOME`. For the default standalone Windows installation:

```powershell
$env:PATH = "C:\Program Files\LLVM\bin;$env:PATH"
dotnet tool restore
dotnet cake --target=tests-native
```

`HarfBuzzSymbolBinding` builds and runs a small C probe against the Linux library, so it needs a Linux glibc host with `gcc`. On other hosts it is reported as skipped.

## Running in Docker

This runs the suite in the same Linux image as CI (`scripts/infra/tests/docker/native`), so every case runs on any host that has Docker with Linux containers:

```powershell
pwsh .\scripts\infra\tests\docker\native\run-local.ps1
```

The image installs LLVM, `gcc`, and the .NET SDK explicitly. The script builds the image, mounts the repository, and runs `tests-native` inside the container. Any extra arguments are passed on to Cake (for example `--configuration=Debug`). Stop local builds before running it: the container uses the same `bin`, `obj`, and `output` directories.
