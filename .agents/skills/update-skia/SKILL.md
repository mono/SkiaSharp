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

Use this skill as the source of truth for both a developer-run update and the automated sync.
The workflow only supplies resolved inputs, a prepared host, and automated PR delivery.

`Skia C++ -> SkiaSharp C API -> generated P/Invoke -> managed wrappers`

Run from the mono/SkiaSharp repository root. Each phase lives in a separate reference so only
the current work enters context.

## Start state

Automation exports the `SKIA_SYNC_*` variables below. A local run resolves them in Phase 01.
Never replace supplied automation values with assumptions about `main`, `skiasharp`, or branch
names.

| Runtime value | Environment variable | Meaning |
|---|---|---|
| `{CURRENT}` | `SKIA_SYNC_CURRENT` | Milestone on the parent base |
| `{TARGET}` | `SKIA_SYNC_TARGET` | Requested target milestone |
| `{UPSTREAM_REF}` | `SKIA_SYNC_UPSTREAM_REF` | `chrome/m{TARGET}` or `main` |
| `{BASE_BRANCH}` | `SKIA_SYNC_BASE_BRANCH` | Parent PR base |
| `{PARENT_BASE_SHA}` | `SKIA_SYNC_PARENT_BASE_SHA` | Exact parent base commit used for metadata comparison |
| `{SKIA_BASE_BRANCH}` | `SKIA_SYNC_SKIA_BASE_BRANCH` | mono/skia PR base |
| `{SKIA_BASE_SHA}` | `SKIA_SYNC_SKIA_BASE_SHA` | Exact mono/skia commit recorded by the parent base |
| `{HEAD_BRANCH}` | `SKIA_SYNC_HEAD_BRANCH` | Feature branch used in both repositories |
| `{IS_RELEASE}` | `SKIA_SYNC_IS_RELEASE` | Whether the selected base is a release line |
| `{BASE_UPSTREAM_SHA}` | `SKIA_SYNC_BASE_UPSTREAM_SHA` | Exact upstream commit recorded by the parent base |
| `{TARGET_UPSTREAM_SHA}` | `SKIA_SYNC_TARGET_UPSTREAM_SHA` | Exact fetched target upstream commit |
| `{PARENT_REMOTE_HEAD}` | `SKIA_SYNC_PARENT_REMOTE_HEAD` | Parent sync-branch SHA observed before the agent, or `none` |
| `{SKIA_REMOTE_HEAD}` | `SKIA_SYNC_SKIA_REMOTE_HEAD` | mono/skia sync-branch SHA observed before the agent, or `none` |
| `{PLATFORM}` | `SKIA_SYNC_PLATFORM` | Native Cake target suffix |
| `{ARCH}` | `SKIA_SYNC_ARCH` | Native architecture |

`SKIA_SYNC_AUTOMATION=1` selects the automation handoff in Phase 11. Without it, use the
repository PR templates and normal developer pushes.

Initialize the artifact directory once:

```bash
ARTIFACT_DIR="${SKIA_SYNC_ARTIFACT_DIR:-${TMPDIR:-/tmp}/skia-sync-agent}"
mkdir -p "$ARTIFACT_DIR"
```

## Definition of success

An update is complete only when:

- The authoritative old-upstream-to-target range was analyzed before merging.
- The mono/skia result is a genuine two-parent merge with every fork patch and dependency
  decision accounted for.
- The updated native source builds; downloaded old native artifacts were never substituted.
- Bindings were regenerated and every required managed wrapper was reviewed.
- The final **unfiltered** `tests/SkiaSharp.Tests.Console.slnx` run passes every host, including
  every GPU backend required by `GpuPolicy` on the validation host.
- Named Ganesh and Graphite Vulkan context tests execute without skips, and the deterministic
  evidence validator accepts the initial, final, and focused logs.
- The parent points to the exact tested mono/skia commit.
- Both PR descriptions identify untested platforms and are ready for human review.
- Automated publication uses verified Git bundles and never exposes a write token to the agent or
  to a runner that executes updated repository code.

## Working rules

- Create feature branches in both repositories before changes; never commit to protected branches.
- Use a genuine two-parent merge in mono/skia; never use a tree-override merge.
- Preserve every fork patch unless upstream contains an equivalent or improved form.
- Mark a removed patch `upstreamed` only after enumerating every independent behavior in its old
  delta and locating each one in the target; one upstreamed hunk does not cover another lost hunk.
- Classify dependency revisions against the **fork base**, not only the prior upstream milestone.
- Treat final `DEPS` as ground truth: every enabled revision that differs from the fork base must
  have a matching final decision, exact-SHA evidence, and reconciled Component Governance metadata.
  A dependency recorded as preserved must still equal the fork-base revision.
- Let `update_versions.py` identify dependency changes from exact base/final DEPS. For every tracked
  changed dependency, derive the semantic version from checked-out source and complete the
  `skia_dependency` verification fields before the helper can pass. Never update an unrelated
  manifest version when its DEPS identity did not change.
- Components not sourced from Skia DEPS, including ANGLE and its dependencies, are updated in
  separate dependency PRs rather than bundled into a Skia upstream sync.
- Never use `externals-download` after a submodule/native/C API change.
- Never hand-edit `*.generated.cs`; regenerate it.
- Keep public managed ABI additive.
- A focused project test is diagnostic only; it never satisfies the final gate.
- GPU bring-up failures are test failures. The validation environment must provide every backend
  required by `GpuPolicy` for its platform.
- Diagnose failures from repository evidence. Do not add one-off compiler/GN flags, skip tests,
  weaken assertions, or encode milestone-specific answers to make one run green.
- Execute the update phases in this agent. Do not delegate the full update, or any mutating,
  build, test, or delivery phase, to a general-purpose/background agent. Only Phase 03's explicitly
  read-only discrepancy review may be delegated.
- Run long builds and full test suites in the foreground as one shell invocation. Do not use
  `nohup`, `&`, or agent-turn progress polling. If the shell tool keeps a command running, block on
  that existing session and inspect its output once after it exits.
- Do not create PRs, write automation handoff files, or report completion while any gate fails.
- In automation, no-work is handled before the agent starts. A started agent that cannot
  complete must fail rather than return success-shaped output.

## Modes

| Mode | Version behavior |
|---|---|
| Normal milestone | Advance milestone, soname, assembly/file, and package versions |
| Release-line bug-fix | Keep versions; advance Skia hashes only |
| Upstream `main` tip | Keep versions; still regenerate, build, and test because APIs may change |

`CURRENT == TARGET` means bug-fix behavior only when `{UPSTREAM_REF} != main`.

## Phase router

Read **only the current phase file**, complete its gate, then move to the next row.

| Phases | Read when starting | Required outcome |
|---|---|---|
| 01–03 Resolve & research | [references/phases/01-03-research.md](references/phases/01-03-research.md) | Authoritative runtime state, diff analysis, and independent review |
| 04–05 Branch & merge | [references/phases/04-05-branch-and-merge.md](references/phases/04-05-branch-and-merge.md) | Correct branches and audited two-parent upstream merge |
| 06–07 Update & native build | [references/phases/06-07-update-and-build.md](references/phases/06-07-update-and-build.md) | Version files consistent and updated native source builds |
| 08–10 Bindings & tests | [references/phases/08-10-bindings-and-tests.md](references/phases/08-10-bindings-and-tests.md) | Bindings reviewed and final unfiltered solution green |
| 11–11 Ship | [references/phases/11-11-ship.md](references/phases/11-11-ship.md) | Local PRs or automation handoff; no merge without approval |

Do not preload all phase files. The current phase file names any narrower reference section
needed for that phase.

## Deterministic helpers

- `scripts/update_versions.py` updates and validates version surfaces, Skia hashes, and deterministic
  DEPS-to-Component-Governance identity/review signals.
- `scripts/regenerate_bindings.py` runs every binding configuration, restores HarfBuzz,
  and reports new native functions.
- `scripts/audit_fork_patches.py` compares the old and new fork deltas and fails while any
  added, removed, or changed patch lacks a final evidence-backed disposition.
- `scripts/validate_test_output.py` verifies every full-solution host passed and requires explicit,
  non-skipped Ganesh and Graphite Vulkan evidence.

These scripts are idempotent and are the source of truth for their phases. Do not manually
recreate their behavior.

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
