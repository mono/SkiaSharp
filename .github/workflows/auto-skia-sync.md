---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

# -- Engine ------------------------------------------------------------
# Use Claude Opus (instead of the default Sonnet) for this workflow: the
# upstream merge/conflict-resolution and build-fix reasoning is hard and
# benefits from the stronger model, despite the higher AI-credit cost.
engine:
  id: copilot
  model: claude-opus-4.7

# -- Triggers ----------------------------------------------------------
# Three daily crons: current (7 AM), next (12 PM), latest (5 PM UTC).
# Manual dispatch with mode selector.
on:
  schedule:
    - cron: "0 7 * * *"
    - cron: "0 12 * * *"
    - cron: "0 17 * * *"
  workflow_dispatch:
    inputs:
      mode:
        description: "Which milestone to track. `main` syncs the very tip of upstream Skia (google/skia main HEAD) — dispatch-only, bleeding edge, NOT a version bump."
        required: false
        type: choice
        default: next
        options: [current, next, latest, main]
      milestone:
        description: "Exact milestone number (e.g. 148) — overrides mode if set"
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
      uses: actions/checkout@v4
      with:
        sparse-checkout: .github/scripts
    - name: Detect milestone
      id: detect
      # The cron→mode mapping lives HERE, next to the cron declarations:
      # 7 AM → current, 5 PM → latest, everything else (12 PM cron + the
      # workflow_dispatch default) → next. Staged into env vars rather than
      # interpolated straight into `run:`, so the free-form `milestone` input
      # can't inject shell — the script consumes them as real --mode/--milestone args.
      env:
        MODE: >-
          ${{ github.event.inputs.mode
              || (github.event.schedule == '0 7 * * *' && 'current')
              || (github.event.schedule == '0 17 * * *' && 'latest')
              || 'next' }}
        MILESTONE: ${{ github.event.inputs.milestone }}
        GH_TOKEN: ${{ github.token }}
      run: bash .github/scripts/skia-sync-detect.sh --gate --mode "$MODE" --milestone "$MILESTONE"

# -- Pre-activation outputs ------------------------------------------
# Expose detect step outputs for use in the prompt and other jobs.
# Required: without this, ${{ needs.pre_activation.outputs.target }} is empty.
jobs:
  pre-activation:
    outputs:
      current: ${{ steps.detect.outputs.current }}
      next: ${{ steps.detect.outputs.next }}
      latest: ${{ steps.detect.outputs.latest }}
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
  group: skia-upstream-sync-${{ github.event.inputs.mode || github.event.schedule || 'manual' }}
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
    - "storage.googleapis.com"

# -- Environment -----------------------------------------------------
# Clang is required for the Linux native build (retpoline flag).
env:
  CC: clang
  CXX: clang++
permissions:
  contents: read
  pull-requests: read

# -- Safe outputs ------------------------------------------------------
# All GitHub writes (push, PR) are done in post-steps via bash.
# Stage all safe outputs so the agent can't create issues/PRs directly.
safe-outputs:
  staged: true

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
    # Same cron→mode resolution as the pre_activation detect step (see there). The
    # agent job can't read pre_activation's outputs (it only `needs:` activation), so
    # re-run the same committed detector to recover base_branch / skia_base_branch,
    # then align the submodule. skia-sync-detect.sh is the single source of truth.
    env:
      MODE: >-
        ${{ github.event.inputs.mode
            || (github.event.schedule == '0 7 * * *' && 'current')
            || (github.event.schedule == '0 17 * * *' && 'latest')
            || 'next' }}
      MILESTONE: ${{ github.event.inputs.milestone }}
      GH_TOKEN: ${{ github.token }}
    run: |
      OUT=$(mktemp)
      SKIA_SYNC_OUT="$OUT" bash .github/scripts/skia-sync-detect.sh --mode "$MODE" --milestone "$MILESTONE"
      set -a; . "$OUT"; set +a
      bash .github/scripts/skia-sync-align-submodule.sh
  - name: Copy push script for post-step
    run: |
      cp .github/scripts/skia-sync-push-prs.sh /tmp/gh-aw/skia-sync-push-prs.sh

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
      sudo apt-get install -y clang libc++-dev libc++abi-dev fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts
      fc-cache -f
      dotnet workload install android --skip-sign-check
    env:
      DEBIAN_FRONTEND: noninteractive

# -- Post-agent steps -----------------------------------------------
# Run AFTER the AI finishes. Pushes branches and creates/updates PRs
# using the SKIASHARP_AUTOBUMP_TOKEN (has write access to mono/skia).
post-steps:
  - name: Push branches and create PRs
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash /tmp/gh-aw/skia-sync-push-prs.sh
---

# Skia Upstream Sync

Base milestone (current): `m${{ needs.pre_activation.outputs.current }}`.  
Target milestone: `m${{ needs.pre_activation.outputs.target }}`.  
Upstream ref (merge from): `${{ needs.pre_activation.outputs.upstream_ref }}` (google/skia) — `main` means the bleeding-edge tip.  
Base branch (SkiaSharp): `${{ needs.pre_activation.outputs.base_branch }}` — mono/skia base: `${{ needs.pre_activation.outputs.skia_base_branch }}`.  
Sync (head) branch: `${{ needs.pre_activation.outputs.head_branch }}` (same name in both repos).  
Release-line sync: `${{ needs.pre_activation.outputs.is_release }}`.

> **About the base branch.** Most syncs target `main` (the newest, in-development line) with
> `skiasharp` as the mono/skia base. When an older milestone is requested AND a matching
> `release/<major>.<milestone>.x` branch exists, this is a **release-line sync**: both PRs target
> that release branch instead (mono/skia base = the same `release/<major>.<milestone>.x` branch,
> which the release process guarantees exists). In that case `current == target`, so it is always a
> **bug-fix-only sync** — do NOT bump the milestone, soname, or nuget versions; only the upstream
> merge + `cgmanifest.json` commit hash change. Use the base/head branch values above everywhere
> instead of hardcoding `main`/`skiasharp`/`skia-sync/m{target}`.
>
> **`main` (tip) mode.** When the upstream ref above is `main` (head branch `skia-sync/main`), this is a
> **bleeding-edge sync from the very tip of upstream Skia** (google/skia `main` HEAD), not a `chrome/m<N>`
> milestone branch. It targets the newest line (`main`/`skiasharp`) and `current == target`, so it is
> **NOT a version bump** — keep the milestone, soname, and nuget versions unchanged. It may still include
> new APIs / binding changes (regenerate + build + test as normal), and a tip merge can be large and
> conflict-heavy because the submodule base is well behind google/skia main; resolve what you reasonably
> can and record anything unresolved under "items needing human attention".

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-10.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it — but you still need to add the `upstream` remote
  and fetch the upstream ref `${{ needs.pre_activation.outputs.upstream_ref }}` (Phase 1 step 4) since Phase 5 depends on it.
  For every mode except `main` this is a `chrome/m${{ needs.pre_activation.outputs.target }}` milestone branch;
  for `main` (tip) mode it is google/skia's `main` HEAD (bleeding edge — may include new APIs/binding changes,
  but it is NOT a version bump, and the head branch is `skia-sync/main`).
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't do this for the chroot).
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
- **Phase 6 (version files)**: For a release-line sync (`is_release == true`, so `current == target`) this is a
  bug-fix-only sync — keep the release line's milestone/soname/nuget versions unchanged; the only expected
  parent-repo change is `cgmanifest.json`'s commit hash. Do NOT advance the milestone.
- **Build platform**: use Linux x64 (`dotnet cake --target=externals-linux --arch=x64`). Clang is pre-configured via env vars.
  This also applies to Phase 10 if a native rebuild is needed.
- **Native build environment is fully provisioned by the workflow** (clang, `libc++-dev`/`libc++abi-dev`,
  fontconfig, ninja). Do NOT modify any native build files (`native/**/build.cake`, `scripts/infra/native/**`)
  and do NOT change compiler/linker flags (e.g. `-stdlib=libc++`) to work around a build error. You also CANNOT
  install packages (no apt/sudo inside the sandbox, and the firewall blocks OS package mirrors). If a native
  build genuinely fails because a dependency is missing from the host, that is a workflow bug: STOP, do not hack
  the build, and record it in the mono/SkiaSharp summary under "items needing human attention" so the
  `Install native build dependencies` step can be fixed.
- **NEVER run `externals-download`** in this workflow — not even for debugging or baseline comparison. Build from source only.
- **Phase 9 reminder**: a green C# build is NOT sufficient - run the new-function diff check from Phase 9 step 1.
- **Phase 11 — do NOT execute it.** Replace it entirely with the file writes below.
  Do NOT push branches, create PRs, or create issues — all GitHub artifacts are handled by the post-step.
  Just commit locally. Do NOT call `create_issue` or `create_pull_request`.
- **"No work" signal**: the pre-activation step skips the workflow when there are no new upstream commits,
  so this should not happen. If somehow it does, do NOT write `skia-sync-env.sh` and stop.

After Phase 10, write these files:

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

2. `/tmp/gh-aw/agent/skia-sync-skia-summary.md` - for the mono/skia PR:
   - Upstream merge details, conflicts resolved, C API fixes, items needing human attention

3. `/tmp/gh-aw/agent/skia-sync-skiasharp-summary.md` - for the mono/SkiaSharp PR:
   - Breaking change analysis, version/binding updates, C# changes, build/test results, items needing human attention

All files written to `/tmp/gh-aw/agent/` are automatically uploaded as workflow artifacts.
Write test output there too (`/tmp/gh-aw/agent/test-output.txt`) so failures can be inspected after the run.

Commit submodule changes inside `externals/skia` on `${{ needs.pre_activation.outputs.head_branch }}`.
Commit parent repo changes on `${{ needs.pre_activation.outputs.head_branch }}` in the parent.
