# Native tests

Native scenarios use the same xUnit v3 runner and TRX reporting as the managed tests.
They live in `tests/SkiaSharp.Tests.Native/Cases`: **add a folder to add a test**.
The project copies every case folder to its output, and one theory enumerates those folders.
No C# registration or manifest is needed.

Each folder contains `test.sh` and any C/C++ sources or data it needs. The runner invokes
`bash test.sh` with that folder as its working directory. Exit zero means pass; a nonzero
exit fails the theory with stdout/stderr. Missing scripts also fail. The runner gives
each case five minutes and terminates its process tree on timeout.
The script's first argument is the checkout's fixed `output/native` directory, resolved
automatically by the runner; no environment variable or path setup is needed.

## Running

The initial suite requires Linux/glibc, Bash, a C compiler, GNU binutils and coreutils.
It tests existing native outputs; it does not build or download Skia.

```sh
dotnet test tests/SkiaSharp.Tests.Native/SkiaSharp.Tests.Native.csproj

# Dedicated solution; kept separate from the cross-platform managed console suite.
dotnet test tests/SkiaSharp.Tests.Native.slnx

# Cake writes TRX under output/logs/testlogs/SkiaSharp.Tests.Native.
dotnet cake --target=tests-native
```

The first case, `HarfBuzzSymbolBinding`, checks the matching host architecture's `linux`
and `linuxnodeps` artifacts. It compiles a standalone loader and conflicting functions,
verifies a deliberately preemptible negative control, then tests the produced HarfBuzz
library. Conflicting libraries are never loaded into the xUnit process.

A case can also run without .NET:

```sh
bash tests/SkiaSharp.Tests.Native/Cases/HarfBuzzSymbolBinding/test.sh "$PWD/output/native"
```

## CI

The Native Tests stage runs immediately after native artifact merging and gates packaging
in the Complete pipeline. The Package pipeline is build-only and does not run native tests.
In the standalone Tests pipeline it runs after
preparation against the source build's native artifacts. Its dedicated Linux job consumes
the merged `native` artifact, runs `tests-native` and publishes TRX results even on failure. Its small image
is independent of native build images and minimal managed-runtime images:

```sh
docker build -t skiasharp-native-tests scripts/infra/tests/docker/native
docker run --rm --volume "$(pwd):/work" -w /work skiasharp-native-tests \
  /bin/sh -c "dotnet tool restore && dotnet cake --target=tests-native"
```

The current CI host is x64. Artifacts must match the runtime architecture and libc;
missing tools/artifacts or mismatches fail, never silently skip.
Alpine, Windows and macOS scenarios/jobs are not part of this initial suite.
Existing Cake dependency and platform checks remain unchanged in native builds.

Build modified native sources using `externals-<platform>`, never `externals-download`.
