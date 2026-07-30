# 06–07 — Update and native build

## Phase 06 — update version surfaces

From the parent repository, run the maintained helper:

```bash
python3 .agents/skills/update-skia/scripts/update_versions.py \
  --current "{CURRENT}" \
  --target "{TARGET}" \
  --upstream-ref "{UPSTREAM_REF}"
```

The helper updates and validates `scripts/VERSIONS.txt`, `cgmanifest.json`,
`scripts/azure-templates-variables.yml`, and the native C API version. It implements all modes:

- Milestone bump: advance milestone and package/native versions.
- Same-milestone bug-fix: keep versions and advance Skia hashes.
- Upstream `main`: keep versions and advance the submodule/hash while still checking APIs.

It is idempotent. Run it again after the final mono/skia fix commit so the parent records the exact
tested submodule tip.

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
dotnet tool restore
dotnet cake --target="externals-{PLATFORM}" --arch="{ARCH}"
```

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
| Newly optional module | Add the supported target dependency/feature configuration |
| Undefined symbol | Check feature guards, defines, and explicit target dependencies |

Read only the matching build/C API section of
[../known-gotchas.md](../known-gotchas.md). Do not add a one-off `--gnArgs` value or alter
compiler/linker flags merely to silence one host. A genuinely required GN choice belongs in every
affected platform's `native/**/build.cake` and must be reported for cross-platform review.

When evidence disproves an earlier dependency or risk conclusion, update
`skia-dependency-decisions.md`, `skia-breaking-change-analysis.md`, and the validation-review
disposition together. Commit each proven post-merge dependency/C API adaptation in mono/skia as a
separate explanatory commit.

## Gate

- Version helper passes for the selected mode.
- Updated target native library builds from source.
- Every build failure is resolved rather than bypassed.
- Dependency and analysis artifacts match the built tree.
