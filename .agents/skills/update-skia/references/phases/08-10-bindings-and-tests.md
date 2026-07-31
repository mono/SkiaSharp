# 08–10 — Bindings, managed build, and tests

## Phase 08 — regenerate bindings

The native build must run first so dependency headers exist:

```bash
python3 .agents/skills/update-skia/scripts/regenerate_bindings.py
dotnet build binding/SkiaSharp/SkiaSharp.csproj
```

The helper runs every maintained generator configuration, restores HarfBuzz, reports generated
changes, and lists new native functions. Never edit a generated file manually.

## Phase 09 — review the managed surface

For every newly generated native function:

```bash
git diff "origin/{BASE_BRANCH}" -- binding/SkiaSharp/SkiaApi.generated.cs |
  grep '^+.*internal static'
grep -rn "<native-function>" binding/SkiaSharp --exclude='*.generated.cs'
```

Add or adapt hand-written wrappers and tests when existing behavior or the requested API requires
them. Keep public ABI additive. Constructors throw on failure; factories follow existing nullable
behavior. An unrelated new upstream API can be deferred, but a changed behavior used by an
existing wrapper cannot.

## Phase 10 — execute the full validation loop

### Prepare the runtime

Use a host that can execute every maintained test host and every backend `GpuPolicy` requires.
Verify required runtime prerequisites before running tests. In deterministic automation, pin
software implementations where practical instead of relying on opportunistic hardware discovery.

Failed GPU bring-up is a test failure. If a local host cannot execute every backend required by
`GpuPolicy` for that platform, use the provisioned workflow instead of claiming successful
validation from incomplete coverage.

### Initial full solution

```bash
dotnet build binding/SkiaSharp/SkiaSharp.csproj
set -o pipefail
dotnet test tests/SkiaSharp.Tests.Console.slnx \
  -p:TargetFramework=net10.0 \
  -p:TargetFrameworks=net10.0 \
  2>&1 | tee "$ARTIFACT_DIR/initial-test-output.txt"
```

Run the solution unfiltered. Confirm every maintained test host in the solution reported results.
The initial run exists to expose failures for the diagnostic loop below. A required host that runs
zero tests or only skips tests is an infrastructure failure; test failures are expected inputs to
diagnosis and must be fixed before the final run.

### Focused diagnostics

After the solution identifies a failure, filter only its owning host:

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

Microsoft.Testing.Platform fails solution projects with zero filter matches, so never append a
single-test filter to the `.slnx`.

Diagnose the failing behavior before editing:

- Trace the first failing call through its direct managed/native implementation and preconditions,
  then diff that path over `DIFF_RANGE`. Broaden the investigation only when evidence rules it out.
- Prefer compatibility changes in fork-owned `src/c`, `include/c`, managed wrappers, or durable
  build targets. Before adding a new patch to upstream-owned implementation, inspect analogous
  C shims and prove the behavior cannot be preserved at the binding boundary.
- When adapting from repository prior art, account for its private types and feature guards before
  rebuilding.
- After native changes, rebuild from source and prove the changed library is loaded.
- If the C API surface changed, regenerate bindings and repeat Phase 09.

A focused run proves a candidate fix only. Rerun the full unfiltered solution after every failure
is fixed; only that final run satisfies the gate. Capture the complete final command output—not a
manually written summary—in the canonical artifact:

```bash
rm -f "$ARTIFACT_DIR/test-output.txt"
rm -f "$ARTIFACT_DIR/test-exit-code.txt"
(
  set +e
  set -o pipefail
  dotnet test tests/SkiaSharp.Tests.Console.slnx \
    -p:TargetFramework=net10.0 \
    -p:TargetFrameworks=net10.0 \
    2>&1 | tee "$ARTIFACT_DIR/test-output.txt"
  TEST_EXIT=${PIPESTATUS[0]}
  printf '%s\n' "$TEST_EXIT" > "$ARTIFACT_DIR/test-exit-code.txt"
  exit "$TEST_EXIT"
)
```

### Finalize tested commits

After the final full solution passes:

1. Ensure every post-merge mono/skia adaptation is committed and the worktree is clean.
2. Rerun `update_versions.py` so `cgmanifest.json` records the final tested submodule tip.
3. Commit version, binding, wrapper, test, and submodule changes in the parent.
4. Verify no build-time side effects or unrelated files are staged.
5. Verify the parent gitlink equals the mono/skia commit used by the green run.
6. Refresh and validate `skia-fork-patch-audit.md` against the final mono/skia tip.

## Gate

- Binding helper and managed build pass.
- No required native function lacks a managed decision.
- Final unfiltered solution passes every host.
- Every `GpuPolicy`-required backend initializes and executes with zero failures.
- Parent points to the exact tested mono/skia commit.
- Every final fork-delta change has one non-contradictory evidence-backed disposition.
