# Native tests

Native scenarios use the same xUnit v3 runner and TRX reporting as the managed tests.
They live in `tests/SkiaSharp.Tests.Native/Cases`: **add a script or folder to add a test**.
The project copies `Cases` to its output, and one theory discovers `Name.ps1` and `Name/test.ps1`.
No C# registration or manifest is needed.

Use a single `Name.ps1` for standalone checks, or a `Name/test.ps1` folder when a case needs
C/C++ sources or other data. The runner invokes PowerShell with the script's directory as its working directory. Exit zero means pass; a nonzero
exit fails the theory with stdout/stderr. Missing scripts also fail. The runner gives
each case five minutes and terminates its process tree on timeout.
The script's first argument is the checkout's fixed `output/native` directory, resolved
automatically by the runner; no environment variable or path setup is needed.
The checkout root is supplied through MSBuild's `NativeTest.RepositoryDirectory`
runtime configuration value, following the sample and MSBuild test projects.

Cases import `Helpers.psm1` for common artifact layouts, file/tool checks and
ELF/PE inspection helpers. The shared file lives directly in `Cases`, so all native test
scripts and supporting files are copied together.
Keep scenario assertions in the case script, with short section comments and blank lines.

## Running

The suite requires Linux/glibc, PowerShell 7.4 or later, a C compiler, GNU binutils
and LLVM's `llvm-readobj`. PowerShell is included in the .NET SDK test image.
It tests existing native outputs; it does not build or download Skia.

```sh
dotnet test tests/SkiaSharp.Tests.Native/SkiaSharp.Tests.Native.csproj

# Dedicated solution; kept separate from the cross-platform managed console suite.
dotnet test tests/SkiaSharp.Tests.Native.slnx

# Cake writes TRX under output/logs/testlogs/SkiaSharp.Tests.Native.
dotnet cake --target=tests-native
```

The runtime case, `HarfBuzzSymbolBinding`, checks the matching host architecture's `linux`
and `linuxnodeps` artifacts. It compiles a standalone loader and conflicting functions,
verifies a deliberately preemptible negative control, then tests the produced HarfBuzz
library. Conflicting libraries are never loaded into the xUnit process.

The remaining cases inspect artifacts without executing them, including cross-built targets:

| Case | Checks |
|---|---|
| `ArtifactCoverage` | Every required artifact exists, aliases match, and every native binary in the final collection is claimed by an inspection case. |
| `LinuxDependencies` | Fontconfig dependency policy for glibc, musl, nodeps and Bionic builds. |
| `LinuxGlibcCompatibility` | Both libraries and nodeps variants stay within the per-architecture GLIBC ceilings declared in the script. |
| `AndroidPageAlignment` | Every LOAD segment supports 16 KB pages for both libraries and all four Android ABIs. |
| `AndroidDebugSymbols` | Matching nonempty GNU build IDs and the exact `.gnu_debuglink` filename for each `.so`/`.so.dbg` pair. |
| `WindowsDependencies` | No hard `VCRUNTIME` or `MSVCP` dependencies in desktop, Nano Server and WinUI artifacts. |
| `WindowsDelayLoads` | Desktop Skia DLLs require D3D12 and D3DCompiler as delay imports, not hard imports. |
| `WindowsRuntimeMarker` | Native Windows PDBs contain the existing `objr_spectre`/`objd_spectre` runtime marker. |
| `WindowsSymbols` | Native/portable PDB identities match their DLLs, and WinUI metadata is readable. |
| `AppleArtifacts` | Mach-O slices, public symbols, dSYM UUIDs and zipped framework contents match the expected Apple outputs. |
| `TizenArtifacts` | ELF architecture and public entry points match the Tizen outputs. |
| `WasmArtifacts` | Every static-archive member is wasm32 and the expected public entry points are defined. |

`ArtifactCoverage` compares the complete on-disk collection with the shared inventory.
An added library, architecture, platform or binary inside a bundled package fails until
the inventory and an appropriate inspection are updated. Binary signatures also catch
extensionless framework/DWARF files and binaries with unfamiliar suffixes. Versioned
ELF filenames and retained WASM archive names are checked against their inspected aliases.
The separately produced `native_msvc` collection is inspected too, so MSVC builds cannot
escape the Windows checks or the unclaimed-artifact guard.

The PDB marker is not evidence of compiler mitigation, code generation, CFG or complete
Spectre protection. The managed WinUI projection is excluded from that marker check,
matching the previous build gate. These migrated artifact assertions live in the scenarios rather
than Cake; build scripts still compile, copy, strip and prepare debug symbols. Required artifacts and tools must
exist: absent inputs fail rather than silently reducing coverage.

A case can also run without .NET:

```sh
pwsh -NoLogo -NoProfile -File tests/SkiaSharp.Tests.Native/Cases/HarfBuzzSymbolBinding/test.ps1 "$PWD/output/native"
```

## CI

The Native Tests stage runs immediately after native artifact merging as a leaf stage
in the Complete pipeline. Packaging does not wait for tests. The Package pipeline is
build-only and does not run native tests.
In the standalone Tests pipeline it runs after
preparation against the source build's native artifacts. Its dedicated Linux job consumes
the merged `native` artifact and `native_msvc` under `output/msvc`, runs `tests-native`
and publishes TRX results even on failure. Its small image
is independent of native build images and minimal managed-runtime images:

```sh
docker build -t skiasharp-native-tests scripts/infra/tests/docker/native
docker run --rm --volume "$(pwd):/work" -w /work skiasharp-native-tests \
  /bin/sh -c "dotnet tool restore && dotnet cake --target=tests-native"
```

The current CI host is x64. Artifacts must match the runtime architecture and libc;
missing tools/artifacts or mismatches fail, never silently skip.
All artifact inspection runs in this same Linux job; it does not need Windows, Android,
Apple or Tizen runtime hosts. Static ABI and symbol checks do not imply runtime coverage
on those platforms. Only the HarfBuzz case executes target code on its matching glibc host.
Cake retains toolchain and build-input validation, but does not duplicate these artifact assertions.

Build modified native sources using `externals-<platform>`, never `externals-download`.
