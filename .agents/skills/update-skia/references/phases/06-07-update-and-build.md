# 06–07 — Update and native build

## Phase 06 — update version surfaces

Hydrate the dependencies recorded by the merged Skia `DEPS` before inspecting their
semantic-version evidence:

```bash
cd "${SKIA_SYNC_WORKSPACE:-${GITHUB_WORKSPACE:-$PWD}}"
python3 externals/skia/tools/git-sync-deps
```

This synchronizes dependency source only; it does not build native binaries. Then run the
maintained version helper from the parent repository:

```bash
python3 "${SKIA_SYNC_SKILL_DIR:-.agents/skills/update-skia}/scripts/update_versions.py" \
  --repo-root "${SKIA_SYNC_WORKSPACE:-${GITHUB_WORKSPACE:-$PWD}}"
```

The helper updates and validates `scripts/VERSIONS.txt`, the Skia registrations in
`cgmanifest.json`, `scripts/azure-templates-variables.yml`, and the native C API version. It
reads the workflow-resolved `SKIA_SYNC_*` values in automation and implements all modes:

- Milestone bump: advance milestone and package/native versions.
- Same-milestone bug-fix: keep versions and advance Skia hashes.
- Upstream `main`: keep versions and advance the submodule/hash while still checking APIs.

It also compares exact fork-base and working-tree `DEPS`, writes
`$ARTIFACT_DIR/skia-dependency-changes.json`, and mechanically synchronizes each tracked
registration's `skia_dependency.revision` while recording old/new URLs and SHAs in the artifact.
A changed tracked dependency intentionally
causes the helper to fail until you:

1. Read the checked-out dependency's authoritative version constant, header, CMake project,
   changelog, README, or tag.
2. Update `component.other.version` when needed.
3. Set `skia_dependency.version_reviewed_identity` to the final DEPS `URL@revision`.
4. Set `skia_dependency.version_source` to the exact source and value inspected.
5. Update the corresponding `skia-dependency-decisions.md` row.
6. Rerun the helper and require its gate to pass.

A revision-only roll may retain the same semantic version, but still requires the final reviewed
identity and source evidence. The helper rejects a manifest version bump when that dependency's
DEPS identity did not change. It is idempotent; rerun it after every final DEPS/native adaptation
and after the final mono/skia fix commit so the parent records the exact tested state.
Every tracked registration, including an unchanged legacy entry, must have non-empty
`version_source` evidence. Backfill missing `skia_dependency` evidence from the hydrated source so
supported branches become compliant over time without changing semantic versions unnecessarily.
The script proves coverage and consistency; the independent review must re-read each cited source
to validate the agent's semantic-version claim.

## Phase 07 — build the updated native source

Never use `externals-download` during a Skia update. Restore repository tools, then build the local
platform from source:

| Host | Command |
|---|---|
| Linux x64 | `dotnet cake --target=externals-linux --arch=x64` |
| Linux arm64 | `dotnet cake --target=externals-linux --arch=arm64` |
| Windows x64 | `dotnet cake --target=externals-windows --arch=x64` |
| macOS arm64 | `dotnet cake --target=externals-macos --arch=arm64` |
| macOS x64 | `dotnet cake --target=externals-macos --arch=x64` |

```bash
cd "${SKIA_SYNC_WORKSPACE:-${GITHUB_WORKSPACE:-$PWD}}"
dotnet tool restore
dotnet cake --target="externals-{PLATFORM}" --arch="{ARCH}"
```

Run the build as one foreground shell invocation and wait for that invocation to return. Do not
wrap it in `nohup`, append `&`, or start progress-polling commands; the shell tool can remain active
for the full build.

Automation may provide a built/cached base tree so GN, Ninja, and unchanged dependencies can be
reused. That is only an optimization; the merged-target source build remains mandatory.

### Evidence-driven build loop

Treat each distinct failure as a new gate:

1. Capture the first real compiler/linker/GN/dependency error.
2. Trace it to target source, fork customization, dependency state, or durable build configuration.
3. Make one coherent fix for that cause.
4. Rebuild and prove that failure is gone before investigating a different one.
5. If the exact failure repeats, prove the changed code/configuration was actually compiled.

| Failure class | Investigation direction |
|---|---|
| Missing or renamed C++ API | Adapt the C API include/call/type to target source |
| Struct/static assert | Reconcile C representation and managed mapping |
| Missing dependency API/source | Reopen the dependency decision and hydrated revision |
| Obsolete GN arg | Update durable `native/**/build.cake` configuration |
| Newly optional module | Follow existing fork policy; otherwise add its supported dependency or report the product decision |
| Undefined symbol | Check feature guards, defines, and explicit target dependencies |

Read only the matching build/C API section of
[../known-gotchas.md](../known-gotchas.md). Do not add a one-off `--gnArgs` value or alter
compiler/linker flags merely to silence one host. A genuinely required GN choice belongs in every
affected platform's `native/**/build.cake` and must be reported for cross-platform review.

When evidence disproves an earlier dependency or risk conclusion, update
`skia-dependency-decisions.md`, `skia-breaking-change-analysis.md`, and the validation-review
disposition together by replacing the provisional entry; do not append a contradictory "final"
section. Commit each proven post-merge dependency/C API adaptation in mono/skia as a separate
explanatory commit.

After every mono/skia adaptation, rerun `audit_fork_patches.py` with the Phase 05 arguments. Fill
new or changed rows and require `--validate` to pass again. Reuse the exact Phase 05
`python3 "${SKIA_SYNC_SKILL_DIR:-.agents/skills/update-skia}/scripts/audit_fork_patches.py"`
command rather than searching
for another copy of the helper.

Before Phase 08, rerun `update_versions.py` against final `DEPS`, then reconcile every row in
`skia-dependency-changes.json` with the main decision table and the built source. Rewrite affected
rows and delete superseded provisional text; do not leave contradictory
preserve/accept/compatibility conclusions. The helper must pass with no unverified tracked
dependency.

## Gate

- Version helper passes for the selected mode.
- Updated target native library builds from source.
- Every build failure is resolved rather than bypassed.
- Dependency and analysis artifacts match the built tree.
- `update_versions.py` passes: every tracked changed dependency has source-backed semantic-version
  evidence, and no unrelated Component Governance version changed.
- The refreshed fork-patch audit validates.
