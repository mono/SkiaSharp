# Phases 6–7: Update and native build

## Phase 6 — update version surfaces

From the parent repository:

```bash
python3 .agents/skills/update-skia/scripts/update_versions.py \
  --current {CURRENT} \
  --target {TARGET} \
  --upstream-ref {UPSTREAM_REF}
```

The helper updates and validates `VERSIONS.txt`, `cgmanifest.json`,
`azure-templates-variables.yml`, and `SK_C_INCREMENT`. It is idempotent: rerun it after the
final submodule fix commit so `cgmanifest.json` points at the tested submodule tip.

Normal milestone updates advance versions. Release bug-fix and upstream-tip syncs retain them.

## Phase 7 — build updated source

Never use `externals-download`. In automation, the base tree has already been built and cached;
this command remains mandatory and incrementally rebuilds the merged target:

```bash
dotnet tool restore
dotnet cake --target=externals-{platform} --arch={arch}
```

Fix errors at their source:

| Failure | Direction |
|---|---|
| Missing/renamed C++ API | Adapt the C API call/include/type |
| Struct/static assert | Update the C representation and managed mapping |
| Missing dependency API | Revisit the dependency compatibility decision |
| Obsolete GN arg | Remove/update the durable `native/**/build.cake` argument |
| New required optional dependency | Use the supported GN switch in every affected platform |
| Undefined symbol | Check feature guards and explicit target dependencies |

Read relevant build/C API sections of [../known-gotchas.md](../known-gotchas.md) only when a
matching failure appears.

Do not add one-off command-line GN flags or modify compiler flags to hide an error. Durable GN
arguments belong in affected `native/**/build.cake` files.

After any native edit, rebuild from source. If the exact failure repeats, prove the changed code
was compiled/linked before forming a second theory.

When a build disproves an earlier dependency or risk conclusion, update the dependency decisions,
primary analysis, and validation-review disposition together so the final artifacts agree.

Do not commit dependency or C API fixes yet.

## Gate

- Version helper passes.
- Updated target native library builds successfully.
- Dependency decisions reflect any compatibility discovery.
