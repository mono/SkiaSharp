---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

# -- Engine ------------------------------------------------------------
# Use Claude Opus (instead of the default Sonnet) for this workflow: the
# upstream merge/conflict-resolution and build-fix reasoning is hard and
# benefits from the stronger model, despite the higher AI-credit cost.
engine:
  id: copilot
model: claude-opus-4.8

# -- Triggers ----------------------------------------------------------
# One fuzzy schedule every 6h. Scheduled runs pass no target, so the detector ROTATES:
# it picks ONE supported line from versions.json per run, round-robin
# (see .github/scripts/skia-sync-detect.sh / rotate_select). Manual dispatch may pin a
# specific `target` (a milestone number, or `main` for the upstream tip).
on:
  schedule: every 6h
  workflow_dispatch:
    inputs:
      target:
        description: "What to sync. Empty = rotate over the supported versions.json lines (the scheduled default). Or a milestone number (e.g. 151), or `main` for the very tip of upstream Skia (google/skia main HEAD — bleeding edge, NOT a version bump)."
        required: false
        type: string

  # -- Pre-activation step -------------------------------------------
  # Runs BEFORE the agent job. Detects the target milestone + branch line.
  # All resolution logic lives in the committed .github/scripts/skia-sync-detect.sh
  # (the single source of truth, sparse-checked-out below); --gate adds the
  # "is there anything to sync?" check.
  # Exit 1 = hard failure (explicit milestone input doesn't exist / branch missing).
  # skip=true output = nothing to sync (graceful skip, workflow shows green).
  # Outputs are available in the prompt via ${{ needs.pre_activation.outputs.* }}.
  steps:
    - name: Check out detection scripts
      uses: actions/checkout@v7
      with:
        sparse-checkout: .github/scripts
    - name: Detect milestone
      id: detect
      # Scheduled runs pass an empty target — the detector then ROTATES: it reads
      # versions.json's `support` block and picks one supported line per run, round-robin
      # by GITHUB_RUN_NUMBER (stable across jobs, so this gate and the agent-job align step
      # resolve the SAME target). Manual dispatch passes a milestone number or `main`.
      # Staged into an env var rather than interpolated into `run:`, so the free-form input
      # can't inject shell — the script consumes it as a real --target arg.
      env:
        SYNC_TARGET: ${{ github.event.inputs.target }}
        GH_TOKEN: ${{ github.token }}
      run: bash .github/scripts/skia-sync-detect.sh --gate --target "$SYNC_TARGET"

# -- Pre-activation outputs ------------------------------------------
# Expose detect step outputs for use in the prompt and other jobs.
# Required: without this, ${{ needs.pre_activation.outputs.target }} is empty.
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      target: ${{ steps.detect.outputs.target }}
      upstream_ref: ${{ steps.detect.outputs.upstream_ref }}
      mode: ${{ steps.detect.outputs.mode }}
      skip: ${{ steps.detect.outputs.skip }}
      is_release: ${{ steps.detect.outputs.is_release }}
      base_branch: ${{ steps.detect.outputs.base_branch }}
      skia_base_branch: ${{ steps.detect.outputs.skia_base_branch }}
      head_branch: ${{ steps.detect.outputs.head_branch }}

# -- Agent job gate --------------------------------------------------
# Only run the agent if pre-activation succeeded and there's work to do.
if: needs.pre_activation.outputs.detect_result == 'success' && needs.pre_activation.outputs.skip != 'true'

# -- Checkout --------------------------------------------------------
checkout:
  - fetch-depth: 0
    submodules: recursive
timeout-minutes: 120
concurrency:
  group: skia-upstream-sync-${{ github.event.inputs.target || github.event.schedule || 'manual' }}
  cancel-in-progress: true

# -- Agent tools -----------------------------------------------------
tools:
  github:
    toolsets: [repos, pull_requests]
    allowed-repos: ["mono/skia", "mono/skiasharp"]
    min-integrity: none
  bash: ["*"]
  edit:

# -- Network allowlist -----------------------------------------------
# Skia build fetches deps from *.googlesource.com and GN from storage/cipd.
network:
  allowed:
    - defaults
    - github
    - dotnet
    - "chromium.googlesource.com"
    - "skia.googlesource.com"
    - "android.googlesource.com"
    - "dawn.googlesource.com"
    - "swiftshader.googlesource.com"
    - "chrome-infra-packages.appspot.com"
    - "gn.googlesource.com"
    - dart

# -- Environment -----------------------------------------------------
# Clang is required for the Linux native build (retpoline flag).
env:
  CC: clang
  CXX: clang++
  SKIA_SYNC_ARTIFACT_DIR: /tmp/gh-aw/agent
  SKIASHARP_REQUIRE_VULKAN: "1"
permissions:
  contents: read
  pull-requests: read

# -- Safe outputs ------------------------------------------------------
# All real GitHub writes (push, both PRs) are done in the post-step via bash with
# SKIASHARP_AUTOBUMP_TOKEN — gh-aw can't create the mono/skia PR (the submodule's merge
# commits live in a nested repo gh-aw sees only as a gitlink) and `staged: true` keeps the
# agent from creating anything directly.
#
# `create-pull-request` is declared ONLY as an honest completion signal: it is kept STAGED
# (preview-only — NO real PR is created), so a successful sync registers as a pull-request
# output instead of being mislabeled a "no-op". The agent calls it when work was done and
# `noop` only when there genuinely was none.
safe-outputs:
  staged: true
  create-pull-request:
    staged: true
    if-no-changes: ignore
  # report-as-issue defaults to true, but this workflow has no `issues: write` and a real sync
  # is NOT a no-op — disable the no-op→issue posting so genuine no-work runs don't try (and fail)
  # to file a "no-op runs" issue.
  noop:
    report-as-issue: false

# -- Sandbox -----------------------------------------------------------
# Mount host fontconfig config AND font files into the AWF chroot.
# /etc/fonts provides fonts.conf; /usr/share/fonts provides the actual .ttf files.
# Without BOTH, fontconfig resolves 0 fonts and SKTypeface tests fail (61 failures).
sandbox:
  agent:
    mounts:
      - "/etc/fonts:/etc/fonts:ro"
      - "/usr/share/fonts:/usr/share/fonts:ro"

# -- Pre-agent steps (host) ------------------------------------------
# Both steps: and pre-agent-steps: run on the HOST, not inside the AWF container.
# The agent runs in an AWF chroot. sandbox.agent.mounts handles /etc/fonts.
steps:
  - name: Set up agent output directory
    run: |
      mkdir -p /tmp/gh-aw/agent
  - name: Align submodule to the base branch
    # Same target resolution as the pre_activation detect step (see there). The agent job
    # can't read pre_activation's outputs (it only `needs:` activation), so re-run the
    # same committed detector to recover base_branch / skia_base_branch, then align the
    # submodule. For rotation runs (empty target) the detector picks the SAME line as
    # pre_activation because the round-robin index is GITHUB_RUN_NUMBER (identical across
    # jobs) and main's config is read at the immutable $GITHUB_SHA. skia-sync-detect.sh is
    # the single source of truth.
    env:
      SYNC_TARGET: ${{ github.event.inputs.target }}
      GH_TOKEN: ${{ github.token }}
    run: |
      OUT=$(mktemp)
      SKIA_SYNC_OUT="$OUT" bash .github/scripts/skia-sync-detect.sh --target "$SYNC_TARGET"
      set -a; . "$OUT"; set +a
      bash .github/scripts/skia-sync-align-submodule.sh
      bash .github/scripts/skia-sync-prefetch-upstream.sh
  - name: Copy post-step assets
    run: |
      cp .github/scripts/skia-sync-push-prs.sh /tmp/gh-aw/skia-sync-push-prs.sh
      cp .github/scripts/skia-sync-render-template.py /tmp/gh-aw/skia-sync-render-template.py
      cp .github/scripts/skia-sync-pr-skia.md /tmp/gh-aw/skia-sync-pr-skia.md
      cp .github/scripts/skia-sync-pr-skiasharp.md /tmp/gh-aw/skia-sync-pr-skiasharp.md

# -- Pre-agent steps ---------------------------------------------------
# Run on the host before the agent starts. Packages installed here are visible
# in the AWF chroot (shared host filesystem), but dotnet tool restore does NOT
# carry into the chroot — the agent must run it itself.
pre-agent-steps:
  - name: Install native build dependencies
    # These run on the HOST and are visible to the agent's AWF chroot via the shared
    # filesystem. The agent itself CANNOT apt-install anything (no apt inside the chroot,
    # and the firewall blocks the Ubuntu archives), so every native build dependency must
    # be installed here.
    #
    # libc++: native/linux/build.cake builds libSkiaSharp/libHarfBuzzSharp with
    # `-stdlib=libc++` (clang's LLVM C++ runtime). On the real CI this comes from the
    # .NET cross-compilation image; for this host build we must install libc++-dev +
    # libc++abi-dev or the compile fails with "cannot find <libc++ headers>". Keep this in
    # sync with native/linux/build.cake's extra_cflags/extra_ldflags.
    run: |
      sudo apt-get update -qq
      sudo apt-get install -y clang libc++-dev libc++abi-dev fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts mesa-vulkan-drivers vulkan-tools
      LAVAPIPE_ICD=$(dpkg -L mesa-vulkan-drivers | grep -E '/lvp_icd(\.x86_64)?\.json$' | head -n 1)
      if [ -z "$LAVAPIPE_ICD" ]; then
        echo "::error::mesa-vulkan-drivers did not install a lavapipe ICD manifest."
        exit 1
      fi
      export VK_ICD_FILENAMES="$LAVAPIPE_ICD"
      export VK_DRIVER_FILES="$LAVAPIPE_ICD"
      {
        echo "VK_ICD_FILENAMES=$LAVAPIPE_ICD"
        echo "VK_DRIVER_FILES=$LAVAPIPE_ICD"
      } >> "$GITHUB_ENV"
      fc-cache -f
      dotnet workload install android --skip-sign-check
    env:
      DEBIAN_FRONTEND: noninteractive
  - name: Verify Mesa lavapipe
    run: |
      set -euo pipefail
      if [ ! -r "$VK_DRIVER_FILES" ]; then
        echo "::error::Mesa lavapipe ICD manifest is missing or unreadable: $VK_DRIVER_FILES"
        exit 1
      fi

      VULKANINFO_OUTPUT=$(mktemp)
      if ! vulkaninfo --summary >"$VULKANINFO_OUTPUT" 2>&1; then
        cat "$VULKANINFO_OUTPUT"
        echo "::error::vulkaninfo could not initialize the pinned Mesa lavapipe ICD."
        exit 1
      fi
      cat "$VULKANINFO_OUTPUT"

      if ! grep -Eiq '(deviceName|driverName).*(llvmpipe|lavapipe)' "$VULKANINFO_OUTPUT"; then
        echo "::error::The pinned Vulkan ICD did not expose Mesa lavapipe/llvmpipe."
        exit 1
      fi
      echo "Verified deterministic software Vulkan through $VK_DRIVER_FILES"

# -- Post-agent steps -----------------------------------------------
# Run AFTER the AI finishes. Pushes branches and creates/updates PRs
# using the SKIASHARP_AUTOBUMP_TOKEN (has write access to mono/skia).
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash /tmp/gh-aw/skia-sync-push-prs.sh
---

# Sync - Skia Upstream

Base milestone (current): `m${{ needs.pre_activation.outputs.current }}`.  
Target milestone: `m${{ needs.pre_activation.outputs.target }}`.  
Upstream ref (merge from): `${{ needs.pre_activation.outputs.upstream_ref }}` (google/skia) — `main` means the bleeding-edge tip.  
Base branch (SkiaSharp): `${{ needs.pre_activation.outputs.base_branch }}` — mono/skia base: `${{ needs.pre_activation.outputs.skia_base_branch }}`.  
Sync (head) branch: `${{ needs.pre_activation.outputs.head_branch }}` (same name in both repos).  
Release-line sync: `${{ needs.pre_activation.outputs.is_release }}`.

> **Mode (resolved above — don't re-derive).** `is_release == true` ⇒ a **release-line bug-fix
> sync** (`current == target`): do NOT bump milestone/soname/nuget versions; only `cgmanifest.json`'s
> hash changes. `upstream_ref == main` ⇒ **`main`/tip mode** (head `skia-sync/main`): also not a
> version bump, but it MAY carry new APIs (regenerate + build + test as normal). Everything else is a
> normal milestone bump. See **[skill Phase 1](.agents/skills/update-skia/SKILL.md)** for what each
> mode means; always use the base/head values above, never hardcode `main`/`skiasharp`/`skia-sync/m{target}`.
>
> **Tip merges are large and conflict-heavy** (the submodule base is well behind `main`), so the
> **verify-upstream-or-reapply** policy is mandatory — for every conflicted file, classify each fork
> patch as *upstreamed* (take upstream's refined form) or *not upstreamed* (re-apply on top), never a
> blanket `--theirs`/`--ours`, never a silent drop. Full procedure + the mandatory before-merge
> snapshot/audit: **skill Phase 5** and [gotcha #15](.agents/skills/update-skia/references/known-gotchas.md).

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-10.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it. The host setup has already added the
  `upstream` remote, fetched `${{ needs.pre_activation.outputs.upstream_ref }}` and the current
  milestone branch, and exported the exact prior upstream commit as `SKIA_BASE_UPSTREAM_SHA`.
  Verify those refs; do not re-derive the mode or substitute a milestone-name range.
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't do this for the chroot).
- The update-skia orchestration scripts are Python. Use those maintained scripts rather
  than recreating their versioning or binding-generation logic.
- **Phases 2 and 3 are NOT pre-computed.** Complete both before branching or merging.
  Write the primary analysis to `/tmp/gh-aw/agent/skia-breaking-change-analysis.md`, invoke
  the independent reviewer with the available `task` tool exactly as Phase 3 requires, and
  write its findings to `/tmp/gh-aw/agent/skia-validation-review.md`.
- **Phase 4**: The parent base branch is `origin/${{ needs.pre_activation.outputs.base_branch }}` and the
  submodule base branch is mono/skia `${{ needs.pre_activation.outputs.skia_base_branch }}` — use these in
  place of `origin/main` / `skiasharp` for every step. The sync branch in BOTH repos is
  `${{ needs.pre_activation.outputs.head_branch }}`. Before creating a fresh branch, check if
  `origin/${{ needs.pre_activation.outputs.head_branch }}` already exists; if so, check it out — the
  pre-activation step already verified new upstream commits exist. Even when current == target there may
  be new upstream bug-fix commits — a matching milestone does NOT mean no work.
  **Skip Phase 4 step 5** (submodule SHA alignment) — the pre-agent step already aligned the submodule to
  the base branch's pointer (on mono/skia `${{ needs.pre_activation.outputs.skia_base_branch }}`).
  Branch from the current HEAD when creating the submodule feature branch in step 6.
- **Phase 6 (version files)**: When `current == target` and the upstream ref is `chrome/m<N>`,
  this is a bug-fix-only sync (including release-line syncs) — keep milestone/soname/nuget
  versions unchanged; only the Skia hashes in `cgmanifest.json` advance. Do NOT advance the
  milestone. A `main` tip sync also has `current == target`, but may carry API/binding changes
  and still updates the submodule.
  Pass the ref you actually merged to `update_versions.py` so `cgmanifest.json`'s `upstream_merge_commit`
  resolves to a real SHA: **`--upstream-ref "${{ needs.pre_activation.outputs.upstream_ref }}"`**
  (this is `chrome/m<N>` for a milestone sync, or `main` for a tip sync — the script defaults to
  `chrome/m{target}`, which does NOT exist on a `main`-tip merge).
- **Phase 5 dependency decisions**: write
  `/tmp/gh-aw/agent/skia-dependency-decisions.md` before committing the merge. Classify every
  changed active `DEPS` entry; preserve fork state/pins by default, but accept a compatible
  upstream revision when target source code demonstrably uses API absent from the fork pin.
- **Build platform**: use Linux x64 (`dotnet cake --target=externals-linux --arch=x64`). Clang is pre-configured via env vars.
  Phase 10 always rebuilds native before building C# and running tests.
- **Native build environment is provisioned by the host workflow** (clang, `libc++-dev`/`libc++abi-dev`,
  fontconfig, ninja, and Mesa lavapipe discovered from the installed package manifest and pinned
  through both `VK_ICD_FILENAMES` and `VK_DRIVER_FILES`). You CANNOT install packages (no apt/sudo in the sandbox; the firewall blocks OS
  mirrors). Do NOT hack compiler/linker flags (e.g. `-stdlib=libc++`) or `scripts/infra/native/**`
  to silence a failure; diagnose and fix the actual update.
- **A genuinely required new upstream gn arg** (e.g. `skia_use_partition_alloc=false`, when a new
  dependency our `DEPS` doesn't vendor forces it) goes in `native/**/build.cake`, NOT a one-off
  `--gnArgs` flag — see **[skill Phase 7](.agents/skills/update-skia/SKILL.md) / [gotcha #23](.agents/skills/update-skia/references/known-gotchas.md)**.
  Since you only build Linux x64 here, apply it to every clang platform's `build.cake` that needs it and
  flag the change in BOTH PR summaries for cross-platform human review.
- **NEVER run `externals-download`** in this workflow — not even for debugging or baseline comparison. Build from source only.
- **Phase 9 reminder**: a green C# build is NOT sufficient - run the new-function diff check from Phase 9 step 1.
- **Phase 10 is a hard gate**: build native, build C#, then run the full `.slnx`. ALL builds and
  ALL tests must pass. If anything fails, diagnose it, fix it, rebuild, and rerun the full solution.
  Do NOT skip or disable tests. Do NOT classify a build/test failure as "human attention," stage it
  for review, write the output files below, or signal completion. The fact that this is an automated
  workflow does not permit creating a failing PR.
  `SKIASHARP_REQUIRE_VULKAN=1` is set deliberately: failure to initialize the pinned lavapipe
  runtime is a test failure, not an allowed runtime skip. The two platform-specific SharpVk
  skips on Linux remain expected.
  Before testing, assert that `SKIASHARP_REQUIRE_VULKAN` is exactly `1` and
  `SKIA_SYNC_ARTIFACT_DIR` is exactly `/tmp/gh-aw/agent`; stop if either is missing.
- **Phase 11 — do NOT execute it.** Replace it entirely with the file writes below.
  Do NOT push branches or create *real* PRs/issues — every GitHub artifact is created by the
  post-step (it pushes both repos and opens both PRs with the autobump token). Just commit locally.
  Do NOT call `create_issue`. Your completion signal is `create_pull_request` (staged) when you did
  work, or `noop` when you did not — see "Completion signal" at the end of this prompt.
- **No-work handling is owned by pre-activation.** If the agent job starts, work was detected.
  Do not convert an incomplete run into `noop`; stop with a failure signal and leave
  `skia-sync-env.sh` absent. The post-step treats a missing handoff as an error.

After Phase 10 has passed completely, confirm all three analysis files above exist and are
non-empty, then write these files:

1. `/tmp/gh-aw/agent/skia-sync-env.sh` — **required** for the post-step to know what to push:
   ```bash
   mkdir -p /tmp/gh-aw/agent
   cat > /tmp/gh-aw/agent/skia-sync-env.sh << EOF
   TARGET=${{ needs.pre_activation.outputs.target }}
   CURRENT=${{ needs.pre_activation.outputs.current }}
   UPSTREAM_REF=${{ needs.pre_activation.outputs.upstream_ref }}
   IS_RELEASE=${{ needs.pre_activation.outputs.is_release }}
   BASE_BRANCH=${{ needs.pre_activation.outputs.base_branch }}
   SKIA_BASE_BRANCH=${{ needs.pre_activation.outputs.skia_base_branch }}
   HEAD_BRANCH=${{ needs.pre_activation.outputs.head_branch }}
   EOF
   ```

2. `/tmp/gh-aw/agent/skia-sync-skia-summary.md` - the automated report inserted into the
   dedicated mono/skia PR template:
   - Use the exact headings `## Changes`, `## Testing`, and `## Human review`.
   - Include upstream merge details, every conflict/dependency decision, C API fixes,
     validation results, and remaining cross-platform review.

3. `/tmp/gh-aw/agent/skia-sync-skiasharp-summary.md` - the automated report inserted into the
   dedicated mono/SkiaSharp PR template:
   - Use the exact headings `## Changes`, `## Testing`, and `## Human review`.
   - Include the breaking-change analysis, version/binding/C# changes, exact host test
     results, and remaining cross-platform review.

The post-step renders the complete, repository-specific PR bodies from
`.github/scripts/skia-sync-pr-skia.md` and
`.github/scripts/skia-sync-pr-skiasharp.md`; do not reproduce contributor templates in the
summary fragments. All files written to `/tmp/gh-aw/agent/` are automatically uploaded as workflow artifacts.
For Phase 10, write the test-output log to `/tmp/gh-aw/agent/test-output.txt` (in place of the
skill's default path) so it's uploaded as an artifact and failures can be inspected after the run.

Commit submodule changes inside `externals/skia` on `${{ needs.pre_activation.outputs.head_branch }}`.
Commit parent repo changes on `${{ needs.pre_activation.outputs.head_branch }}` in the parent.

## Completion signal

When the sync is done — you have committed locally and written `skia-sync-env.sh` plus both
summaries — invoke staged `create_pull_request` **once** through the `safeoutputs` CLI preferred
by the injected system instructions. Build its JSON input under `/tmp/gh-aw/agent/`, include the
actual current branch from `git branch --show-current`, and pass it with
`safeoutputs create_pull_request . < INPUT.json`. This is preview-only: it creates no real PR and
pushes nothing. The post-step opens both real PRs with the autobump token. Use the `[skia-sync] …`
title for this mode and a one-line body pointing at the two summary files. Do this instead of
`noop`; do not call the raw MCP function when the CLI is available.

Do not call `noop` from the agent job. Runs with no upstream work are skipped before the agent
starts; an agent run without `skia-sync-env.sh` is incomplete and must fail.
