---
name: update-skia
description: >
  Update the Skia graphics library to a new Chrome milestone in SkiaSharp's mono/skia fork.
  Handles upstream merge, fork-patch preservation, dependency compatibility, C API adaptation,
  binding regeneration, full backend testing, and coordinated dual-repo PRs.

  Use whenever the user asks to update/bump Skia, merge upstream Skia, update the Skia
  submodule, sync a milestone or release line, merge upstream main, or check the current
  Skia milestone/version. For an individual dependency update, use native-dependency-update.
---

# Update Skia

Skia updates cross two repositories and four implementation layers:

`Skia C++ -> SkiaSharp C API -> generated P/Invoke -> managed wrappers`

The workflow is intentionally gated, but each phase lives in a separate reference so only
the current work enters context.

## Runtime state

Resolve or receive these values before Phase 1:

| Variable | Meaning |
|---|---|
| `{CURRENT}` | Milestone on the parent base branch |
| `{TARGET}` | Requested target milestone |
| `{UPSTREAM_REF}` | `chrome/m{TARGET}` or `main` |
| `{BASE_BRANCH}` | Parent target: `main` or a release branch |
| `{SKIA_BASE_BRANCH}` | mono/skia target: `skiasharp` or matching release branch |
| `{HEAD_BRANCH}` | Shared feature branch in both repositories |

The automated workflow supplies all six values. Do not re-derive or replace them there.

Initialize the artifact directory once:

```bash
ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-${TMPDIR:-/tmp}/skia-sync-agent}"
mkdir -p "$ARTIFACT_DIR"
```

## Non-negotiable invariants

- Create feature branches in both repositories before changes; never commit to protected branches.
- Use a genuine two-parent merge in mono/skia; never use a tree-override merge.
- Preserve every fork patch unless upstream contains an equivalent or improved form.
- Classify dependency revisions against the **fork base**, not only the prior upstream milestone.
- Never use `externals-download` after a submodule/native/C API change.
- Never hand-edit `*.generated.cs`; regenerate it.
- Keep public managed ABI additive.
- Build native from the updated source, build managed code, and finish with an unfiltered
  `tests/SkiaSharp.Tests.Console.slnx` run in which every host passes.
- A focused project test is diagnostic only; it never satisfies the final gate.
- Do not write PR handoff files, create PRs, or report completion while any gate fails.
- In automation, no-work is handled before the agent starts. A started agent that cannot
  complete must fail rather than return `noop`.

## Phase router

Read **only the current phase file**, complete its gate, then move to the next row.

| Phases | Read when starting | Required outcome |
|---|---|---|
| 1–3 Research | [references/phases/research.md](references/phases/research.md) | Authoritative diff analysis plus independent discrepancy review |
| 4–5 Branch & merge | [references/phases/merge.md](references/phases/merge.md) | Correct branches and audited two-parent upstream merge |
| 6–7 Update & native build | [references/phases/build.md](references/phases/build.md) | Version files consistent and updated native source builds |
| 8–10 Bindings, managed build, tests | [references/phases/verify.md](references/phases/verify.md) | Bindings reviewed and final unfiltered solution green |
| 11 Ship | [references/phases/ship.md](references/phases/ship.md) | Complete, cross-linked PRs; no merge without approval |

Do not preload all phase files. The current phase file names any narrower reference section
needed for that phase.

## Deterministic helpers

- `scripts/update_versions.py` updates and validates version surfaces and Skia hashes.
- `scripts/regenerate_bindings.py` runs every binding configuration, restores HarfBuzz,
  and reports new native functions.

These scripts are idempotent and are the source of truth for their phases. Do not manually
recreate their behavior.

## Modes

| Mode | Version behavior |
|---|---|
| Normal milestone | Advance milestone, soname, assembly/file, and package versions |
| Release-line bug-fix | Keep versions; advance Skia hashes only |
| Upstream `main` tip | Keep versions; still regenerate/build/test because APIs may change |

`CURRENT == TARGET` means bug-fix behavior only when `{UPSTREAM_REF} != main`.

## Additional references

- [references/known-gotchas.md](references/known-gotchas.md) — read only the phase-specific
  sections directed by a phase file.
- [references/breaking-changes-checklist.md](references/breaking-changes-checklist.md) —
  detailed Phase 2 audit checklist.
- [references/validation-prompt.md](references/validation-prompt.md) — independent Phase 3 prompt.
- [documentation/dev/dependencies.md](../../../documentation/dev/dependencies.md) —
  dependency and `cgmanifest.json` model.

## Completion

Report the upstream ref/SHA, fork-patch and dependency decisions, C API/binding changes,
exact build and per-host test results, both PR links, and unresolved cross-platform review.
Do not merge either PR without explicit approval.
