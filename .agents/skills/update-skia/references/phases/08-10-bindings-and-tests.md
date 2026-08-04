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
Run each full solution command as one foreground invocation and wait for it to return; do not
background or poll it.
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

### Final deterministic reconciliation

After the final full solution passes, complete every deterministic gate against the exact tested
mono/skia tree before moving to Phase 11:

1. Ensure every post-merge mono/skia adaptation is committed and the worktree is clean.
2. From the parent root, run the metadata finalizer:

   ```bash
   python3 .agents/skills/update-skia/scripts/update_versions.py
   ```

   If it fails, reconcile every `skia-dependency-changes.json` row with checked-out source,
   `cgmanifest.json`, and `skia-dependency-decisions.md`, then rerun until it passes.
3. Refresh the fork audit against the **current final mono/skia HEAD**:

   ```bash
   python3 .agents/skills/update-skia/scripts/audit_fork_patches.py \
     --old-upstream "$SKIA_SYNC_BASE_UPSTREAM_SHA" \
     --new-upstream "$SKIA_SYNC_TARGET_UPSTREAM_SHA" \
     --fork-base "$SKIA_SYNC_SKIA_BASE_SHA" \
     --merged-head HEAD \
     --output "$ARTIFACT_DIR/skia-fork-patch-audit.md" \
     --validate
   ```

   The command rewrites stale fingerprints as `TODO` before validating. If it fails, inspect the
   final old/new patches for every regenerated row, replace each `TODO` with a valid disposition
   and concrete evidence, and rerun this exact command until it passes.
4. Confirm `skia-dependency-changes.json`, `cgmanifest.json`, and
   `skia-dependency-decisions.md` agree on every changed URL/SHA, semantic version, version source,
   and manifest action.
5. Confirm `skia-fork-patch-audit.md` has no `TODO` and describes the exact current mono/skia HEAD.
6. Commit version, binding, wrapper, test, and submodule changes in the parent.
7. Verify no build-time side effects or unrelated files are staged.
8. Verify the parent gitlink equals the mono/skia commit used by the green run.

Any subsequent mono/skia or dependency change invalidates this reconciliation: return to step 1.
Do not read Phase 11 until both commands above pass against final state.

## Gate

- Binding helper and managed build pass.
- No required native function lacks a managed decision.
- Final unfiltered solution passes every host.
- Every `GpuPolicy`-required backend initializes and executes with zero failures.
- Parent points to the exact tested mono/skia commit.
- The deterministic dependency metadata gate passes with source-backed version verification for
  every tracked DEPS change and no version-only manifest drift.
- `audit_fork_patches.py --validate` passes against final mono/skia HEAD, and every final fork-delta
  change has one non-contradictory evidence-backed disposition.
