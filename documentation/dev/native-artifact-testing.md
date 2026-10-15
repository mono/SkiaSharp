# Native artifact tests

The Python suite in `scripts/infra/native/tests` checks native binaries independently of the
managed xUnit tests. Install its dependencies with:

```sh
python3 -m pip install -r scripts/infra/native/tests/requirements.txt
```

## Artifact inspection

Linux builds call the suite through `CheckLinuxDependencies` after each library is copied to
`output/native`. Inspection uses GNU `readelf` and never loads the target, so it also works for
cross-built architectures and libc variants:

```sh
python3 -m pytest scripts/infra/native/tests -q --lane=linux \
  --library="$(pwd)/output/native/linux/x64/libHarfBuzzSharp.so" \
  --max-glibc=2.27 --excluded=fontconfig \
  --junitxml=output/logs/native-tests/harfbuzz-linux-x64.xml
```

The suite preserves the existing dependency inclusion/exclusion and GLIBC-ceiling checks.
HarfBuzz inspection additionally checks required probe entry points and the `hb_*` export
boundary, and rejects dynamic relocations that could preempt calls to defined `hb_*` functions.
This is not a complete ABI-baseline comparison. Undefined external symbols and data symbols are
not treated as internal function calls.

Windows builds use the same entry point through `CheckWindowsDependencies`, with
`--lane=windows --dumpbin=<path>`. The suite preserves hard dependency, delay-load and PDB
runtime-marker checks. The `objr_spectre`/`objd_spectre` PDB marker identifies a runtime build
marker only: it does not prove compiler mitigation, code generation or complete Spectre protection.
CFG (`/guard:cf`) is a separate mechanism.

## Isolated runtime regression

The runtime lane compiles a small C executable and conflicting shared library, then loads the
conflicting functions globally before the produced HarfBuzz library. Every probe is a separate,
time-bounded process. A deliberately preemptible negative control must fail with exit code 86
before the real artifact is tested.

Run on Linux with the **same architecture and libc** as the artifacts. Missing tools, binaries,
or a mismatched host fail rather than skip. A C compiler and GNU `readelf` are required.

```sh
# Test both linux and linuxnodeps artifacts for the current host architecture.
dotnet cake --target=tests-native-runtime --nativePlatform=linux --nativeLibc=glibc

# On Alpine, test alpine and alpinenodeps instead.
dotnet cake --target=tests-native-runtime --nativePlatform=alpine --nativeLibc=musl
```

Use the dedicated images under `scripts/infra/native/runtime/{azurelinux,alpine}` when running
locally in Docker. For example, with already-built matching artifacts:

```sh
docker build -t skiasharp-native-tests scripts/infra/native/runtime/azurelinux
docker run --rm --volume "$(pwd):/work" -w /work skiasharp-native-tests \
  /bin/sh -c "dotnet tool restore && dotnet cake --target=tests-native-runtime --nativePlatform=linux --nativeLibc=glibc"
```

Build changed native sources with `externals-<platform>`; never use `externals-download` to
validate a native change.

## CI and suite maintenance

Linux matrix and Windows native build jobs gate on inspection and publish JUnit files from
`output/logs/native-tests`. Native cache keys include the suite; a cache hit restores results
produced with the same check revision. Two additional jobs in the Tests stage run the actual
merged native artifacts on matching Azure Linux/glibc and Alpine/musl hosts, including their
nodeps variants. They inherit the existing Linux test-agent pool; currently those CI hosts are
x64. Other architectures receive inspection coverage, not mismatched runtime execution.

The dedicated runtime images install compilers; the existing bare nodeps managed-test images
remain unchanged. Failed probes fail the job, and results are published even on failure.

Run the suite's own parser and harness tests without native build outputs:

```sh
python3 -m pytest scripts/infra/native/tests/unit -q
```
