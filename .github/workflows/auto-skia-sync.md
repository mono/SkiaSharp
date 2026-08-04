---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

# -- Engine ------------------------------------------------------------
# Use Claude Opus for the primary update work. A controlled Sonnet 5 evaluation
# was cheaper when successful, but did not achieve the required reliability.
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
      base_branch:
        description: "Optional mono/SkiaSharp base branch override for manual workflow validation. Empty uses normal main/release detection."
        required: false
        type: string

  # -- Pre-activation step -------------------------------------------
  # Runs BEFORE the agent job. Detects the target milestone + branch line.
  # All resolution and work-detection logic lives in the committed
  # .github/scripts/skia-sync-detect.sh (the single source of truth,
  # sparse-checked-out below).
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
      # by GITHUB_RUN_NUMBER (stable across jobs, so this gate and the agent-job prepare step
      # resolve the SAME target). Manual dispatch passes a milestone number or `main`.
      # Staged into an env var rather than interpolated into `run:`, so the free-form input
      # can't inject shell — the script consumes it as a real --target arg.
      env:
        SYNC_TARGET: ${{ github.event.inputs.target }}
        SYNC_BASE_BRANCH: ${{ github.event.inputs.base_branch }}
        GH_TOKEN: ${{ github.token }}
      run: bash .github/scripts/skia-sync-detect.sh --output "$GITHUB_OUTPUT" --target "$SYNC_TARGET" --base-branch "$SYNC_BASE_BRANCH"

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
max-ai-credits: 2000
concurrency:
  group: skia-upstream-sync-${{ github.event.inputs.base_branch || 'auto' }}-${{ github.event.inputs.target || github.event.schedule || 'manual' }}
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
  - name: Prepare Skia checkout
    # Same target resolution as the pre_activation detect step (see there). The agent job
    # can't read pre_activation's outputs (it only `needs:` activation), so re-run the
    # same committed detector to recover base_branch / skia_base_branch, then prepare the
    # submodule and exact upstream analysis range. For rotation runs (empty target), the
    # detector picks the SAME line as pre_activation because the round-robin index is
    # GITHUB_RUN_NUMBER (identical across jobs) and main's config is read at the immutable
    # $GITHUB_SHA. skia-sync-detect.sh is the single source of truth.
    env:
      SYNC_TARGET: ${{ github.event.inputs.target }}
      SYNC_BASE_BRANCH: ${{ github.event.inputs.base_branch }}
      GH_TOKEN: ${{ github.token }}
    run: |
      OUT=$(mktemp)
      bash .github/scripts/skia-sync-detect.sh --resolve-only --output "$OUT" --target "$SYNC_TARGET" --base-branch "$SYNC_BASE_BRANCH"
      set -a; . "$OUT"; set +a
      bash .github/scripts/skia-sync-prepare-skia.sh
      {
        printf 'SKIA_SYNC_AUTOMATION=1\n'
        printf 'SKIA_SYNC_CURRENT=%s\n' "$current"
        printf 'SKIA_SYNC_TARGET=%s\n' "$target"
        printf 'SKIA_SYNC_UPSTREAM_REF=%s\n' "$upstream_ref"
        printf 'SKIA_SYNC_IS_RELEASE=%s\n' "$is_release"
        printf 'SKIA_SYNC_BASE_BRANCH=%s\n' "$base_branch"
        printf 'SKIA_SYNC_SKIA_BASE_BRANCH=%s\n' "$skia_base_branch"
        printf 'SKIA_SYNC_HEAD_BRANCH=%s\n' "$head_branch"
        printf 'SKIA_SYNC_PLATFORM=linux\n'
        printf 'SKIA_SYNC_ARCH=x64\n'
        printf 'SKIA_SYNC_SKIA_BASE_SHA=%s\n' "$(git -C externals/skia rev-parse HEAD)"
      } >> "$GITHUB_ENV"
  - name: Copy post-step assets
    run: |
      RUNTIME_DIR="$RUNNER_TEMP/skia-sync-runtime"
      mkdir -p "$RUNTIME_DIR"
      cp .github/scripts/skia-sync-push-prs.sh "$RUNTIME_DIR/skia-sync-push-prs.sh"
      cp .agents/skills/update-skia/scripts/update_versions.py "$RUNTIME_DIR/update_versions.py"
      cp .agents/skills/update-skia/scripts/audit_fork_patches.py "$RUNTIME_DIR/audit-fork-patches.py"
      chmod -R a-w "$RUNTIME_DIR"

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
      sudo apt-get install -y clang libc++-dev libc++abi-dev fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts xvfb mesa-utils libgl1-mesa-dri libglx-mesa0 mesa-vulkan-drivers vulkan-tools
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
  - name: Verify Mesa software OpenGL
    run: |
      set -euo pipefail
      nohup Xvfb :99 -screen 0 1280x1024x24 > /tmp/xvfb.log 2>&1 &
      sleep 3
      {
        echo "DISPLAY=:99"
        echo "LIBGL_ALWAYS_SOFTWARE=1"
        echo "GALLIUM_DRIVER=softpipe"
      } >> "$GITHUB_ENV"

      DISPLAY=:99 LIBGL_ALWAYS_SOFTWARE=1 GALLIUM_DRIVER=softpipe \
        glxinfo -B | tee /tmp/glxinfo.txt
      if ! grep -Eiq 'renderer string:.*softpipe' /tmp/glxinfo.txt; then
        cat /tmp/xvfb.log
        echo "::error::Mesa software OpenGL did not initialize with softpipe."
        exit 1
      fi
      echo "Verified deterministic software OpenGL through Mesa softpipe on Xvfb."
# -- Post-agent steps -----------------------------------------------
# Run AFTER the AI finishes. Finalize mechanical metadata without credentials,
# then push branches and create/update PRs with the write credential.
post-steps:
  - name: Finalize sync metadata
    env:
      SKIA_SYNC_RUNTIME_DIR: ${{ runner.temp }}/skia-sync-runtime
    run: |
      set -euo pipefail
      test "$(git branch --show-current)" = "$SKIA_SYNC_HEAD_BRANCH"
      test "$(git -C externals/skia branch --show-current)" = "$SKIA_SYNC_HEAD_BRANCH"

      UNEXPECTED_CHANGES=$(
        {
          git diff --name-only
          git diff --cached --name-only
        } | sort -u | grep -Ev '^(cgmanifest\.json|scripts/VERSIONS\.txt|scripts/azure-templates-variables\.yml|externals/skia|externals/depot_tools)$' || true
      )
      if [ -n "$UNEXPECTED_CHANGES" ]; then
        echo "::error::The agent left uncommitted semantic changes:"
        echo "$UNEXPECTED_CHANGES"
        exit 1
      fi

      python3 "$SKIA_SYNC_RUNTIME_DIR/update_versions.py" --repo-root "$GITHUB_WORKSPACE"

      if ! git -C externals/skia diff --quiet || ! git -C externals/skia diff --cached --quiet; then
        echo "::error::The finalizer changed mono/skia; the agent did not commit its native work."
        exit 1
      fi

      git add cgmanifest.json scripts/VERSIONS.txt scripts/azure-templates-variables.yml externals/skia
      if ! git diff --cached --quiet; then
        git config user.name "SkiaSharp Sync"
        git config user.email "devnull@localhost"
        git commit -m "[skia-sync] Finalize deterministic metadata"
      fi
      git diff --quiet -- . ':(exclude)externals/depot_tools'
      git diff --cached --quiet
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
      SKIA_SYNC_RUNTIME_DIR: ${{ runner.temp }}/skia-sync-runtime
    run: bash "$SKIA_SYNC_RUNTIME_DIR/skia-sync-push-prs.sh"
---

# Sync - Skia Upstream

Read `.agents/skills/update-skia/SKILL.md` and use it as the complete engineering process.
Load one numbered phase reference at a time.

## 1. Resolved runtime state

| Value | Resolved setting |
|---|---|
| Current milestone | `${{ needs.pre_activation.outputs.current }}` |
| Target milestone | `${{ needs.pre_activation.outputs.target }}` |
| Upstream ref | `${{ needs.pre_activation.outputs.upstream_ref }}` |
| Parent base | `${{ needs.pre_activation.outputs.base_branch }}` |
| mono/skia base | `${{ needs.pre_activation.outputs.skia_base_branch }}` |
| Shared head branch | `${{ needs.pre_activation.outputs.head_branch }}` |
| Release-line mode | `${{ needs.pre_activation.outputs.is_release }}` |
| Build target | `linux / x64` |

These values are exported as `SKIA_SYNC_*`. Phase 01 target resolution and no-work detection are
already complete; verify the supplied refs and begin its research work. Do not re-derive or replace
the supplied values.

## 2. Provisioned environment

- The selected parent base is fetched, the submodule is aligned to that base's exact recorded
  pointer, and the target upstream ref and recorded base-upstream SHA are fetched.
- No native tree is prebuilt. The first mandatory merged-target source build starts cold and normally
  takes 10–20 minutes. Wait for it to finish; do not treat the expected quiet compile period as a hang,
  cancel it, or restart it solely because of its duration.
- Clang, libc++, fontconfig/fonts, Ninja, Android workload, Xvfb, and Mesa software GL/Vulkan are
  installed. Mesa softpipe and the lavapipe ICD are pinned so every Linux backend required by
  `GpuPolicy` executes deterministically.
- The sandbox cannot install host packages. Diagnose update failures in source, dependencies, or
  durable repository configuration; do not alter flags to mask a missing prerequisite.

## 3. Execution contract

- Complete Phases 02–10 in order. Phase 03 must finish before either feature branch is created.
- In Phase 04, create or check out the parent feature branch from the resolved parent base and the
  submodule feature branch from its exact recorded pointer before merging or building. Never build
  from the workflow checkout when it differs from the resolved parent base.
- The agent job starts only after upstream work is detected. It must complete or fail; never return
  `noop` or human-review output for an unresolved build/test failure.
- Build and test failures are work to diagnose and fix. The final gate is the unfiltered solution
  with every maintained host and every `GpuPolicy`-required backend executing successfully, exactly
  as defined by the skill.
- For a deterministic failure, trace the failing call through its direct implementation and
  preconditions before widening the search. Expand to surrounding logs or broader history only when
  evidence rules out that path.
- Commit only to `${{ needs.pre_activation.outputs.head_branch }}` in both local repositories.

## 4. Automated delivery

After every gate passes, follow Phase 11's **Automated delivery** contract. Do not push branches or
create real PRs/issues from the agent. Write the required artifacts under
`/tmp/gh-aw/agent`, then invoke staged `create_pull_request` once as the completion signal.
The deterministic post-step performs guarded pushes and creates the two cross-linked draft PRs.
