# Phases 8–10: Bindings, managed build, and tests

## Phase 8 — regenerate bindings

Native build must run first so dependency headers exist.

```bash
python3 .agents/skills/update-skia/scripts/regenerate_bindings.py
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

The helper runs all maintained configs, restores HarfBuzz, reports binding changes, and lists
new native functions.

## Phase 9 — managed wrapper review

For each newly generated native function:

```bash
git diff "origin/{BASE_BRANCH}" -- binding/SkiaSharp/SkiaApi.generated.cs |
  grep '^+.*internal static'
grep -rn "<native-function>" binding/SkiaSharp --exclude='*.generated.cs'
```

Add wrappers/tests where required. Keep managed ABI additive; factories return null on native
failure. New upstream APIs may be deferred only when no existing behavior depends on them.

## Phase 10 — initial full solution

Build native once more if managed/C API work changed it, then:

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
set -o pipefail
dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 \
  -p:TargetFrameworks=net10.0 \
  2>&1 | tee "$ARTIFACT_DIR/test-output.txt"
tail -10 "$ARTIFACT_DIR/test-output.txt"
```

Do not append a filter to the solution. Every host must report results.

## Focused diagnostics

After the solution identifies a failure, filter only the owning project:

| Host | Diagnostic project |
|---|---|
| Core | `tests/SkiaSharp.Tests.Console/SkiaSharp.Tests.Console.csproj` |
| Singleton | `tests/SkiaSharp.Tests.SingletonInit.Console/SkiaSharp.Tests.SingletonInit.Console.csproj` |
| Vulkan | `tests/SkiaSharp.Vulkan.Tests.Console/SkiaSharp.Vulkan.Tests.Console.csproj` |
| Direct3D | `tests/SkiaSharp.Direct3D.Tests.Console/SkiaSharp.Direct3D.Tests.Console.csproj` |

```bash
dotnet test <owning-project> \
  -p:TargetFramework=net10.0 \
  -p:TargetFrameworks=net10.0 \
  -- --filter-method "*SpecificTest*"
```

For a null managed factory, trace managed wrapper -> C API -> native factory and diff that
implementation before following warnings. If the same failure repeats after a fix, verify the
changed code is in the built and loaded library.

Rebuild after native changes. Focused runs are diagnostic only.

## Final full solution and commits

Rerun the unfiltered solution after every failure is fixed. Only that run satisfies the gate.
Confirm base, singleton, Vulkan, and Direct3D hosts all pass.

After the final green run:

1. Commit all post-merge dependency/C API fixes in mono/skia.
2. Rerun `update_versions.py` so the parent hash points to that final submodule commit.
3. Commit version/binding/wrapper/test/submodule changes in the parent.
4. Verify no unrelated build side effects are staged.

## Gate

- Binding helper and managed build pass.
- No required native function lacks a wrapper.
- Final unfiltered solution passes every host.
- Vulkan tests execute and pass on the provisioned automation host.
- Parent points to the exact tested submodule commit.
