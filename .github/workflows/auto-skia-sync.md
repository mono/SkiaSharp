---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

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
        description: "Which milestone to track"
        required: false
        type: choice
        default: next
        options: [current, next, latest]
      milestone:
        description: "Exact milestone number (e.g. 148) — overrides mode if set"
        required: false
        type: string

  # -- Pre-activation step -------------------------------------------
  # Runs BEFORE the agent job. Detects the target milestone.
  # Exit 1 = hard failure (explicit milestone input doesn't exist).
  # skip=true output = nothing to sync (graceful skip, workflow shows green).
  # Outputs are available in the prompt via ${{ needs.pre_activation.outputs.* }}.
  steps:
    - name: Detect milestone
      id: detect
      env:
        INPUT_MODE: ${{ github.event.inputs.mode }}
        INPUT_MILESTONE: ${{ github.event.inputs.milestone }}
        SCHEDULE: ${{ github.event.schedule }}
        GH_TOKEN: ${{ github.token }}
      run: |
        if [ -n "$INPUT_MODE" ]; then
          MODE="$INPUT_MODE"
        elif [ "$SCHEDULE" = "0 7 * * *" ]; then
          MODE=current
        elif [ "$SCHEDULE" = "0 17 * * *" ]; then
          MODE=latest
        else
          MODE=next
        fi
        echo "mode=$MODE" >> "$GITHUB_OUTPUT"

        # main's milestone — the basis for `current`/`next` mode math.
        MAIN_MS=$(gh api "repos/$GITHUB_REPOSITORY/contents/scripts/VERSIONS.txt?ref=$GITHUB_REF" \
          --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}')

        NEXT=$((MAIN_MS + 1))
        echo "next=$NEXT" >> "$GITHUB_OUTPUT"

        LATEST=$(git ls-remote --heads https://github.com/google/skia.git 'refs/heads/chrome/m*' \
          | sed -n 's|.*refs/heads/chrome/m\([0-9]*\)$|\1|p' \
          | sort -n | tail -1)
        echo "latest=$LATEST" >> "$GITHUB_OUTPUT"

        # Explicit milestone input overrides mode
        if [ -n "$INPUT_MILESTONE" ]; then
          TARGET="$INPUT_MILESTONE"
          MODE="explicit"
        elif [ "$MODE" = "latest" ]; then
          TARGET="$LATEST"
        elif [ "$MODE" = "current" ]; then
          TARGET="$MAIN_MS"
        else
          TARGET="$NEXT"
        fi
        echo "target=$TARGET" >> "$GITHUB_OUTPUT"

        # -- Resolve the target line: `main` (newest, in-development) or a release line.
        # A maintenance line lives as `release/<major>.<TARGET>.x` in BOTH mono/SkiaSharp
        # and mono/skia. We only look for one when TARGET is older than main's milestone —
        # the newest line is always served by `main`. The release branch itself is created
        # by the release process (release-branch skill), NOT this sync; if the mono/skia
        # side is missing we fail so branch ownership stays with the release process.
        IS_RELEASE=false
        BASE_BRANCH=main
        SKIA_BASE_BRANCH=skiasharp
        HEAD_BRANCH="skia-sync/m${TARGET}"
        RELEASE_BRANCH=""
        if [ "$TARGET" != "$MAIN_MS" ]; then
          RELEASE_BRANCH=$(git ls-remote --heads "https://github.com/${GITHUB_REPOSITORY}.git" "refs/heads/release/*.${TARGET}.x" \
            | sed -n 's|.*refs/heads/\(release/[0-9][0-9.]*\.x\)$|\1|p' | head -1)
        fi
        if [ -n "$RELEASE_BRANCH" ]; then
          IS_RELEASE=true
          BASE_BRANCH="$RELEASE_BRANCH"
          SKIA_BASE_BRANCH="$RELEASE_BRANCH"
          HEAD_BRANCH="skia-sync/${RELEASE_BRANCH//\//-}"

          # The matching mono/skia release branch MUST already exist.
          SKIA_BASE_SHA=$(git ls-remote --heads https://github.com/mono/skia.git "refs/heads/${SKIA_BASE_BRANCH}" | awk '{print $1}')
          if [ -z "$SKIA_BASE_SHA" ]; then
            echo "::error::mono/skia branch '${SKIA_BASE_BRANCH}' does not exist. Release branches are owned by the release process (release-branch skill) — create it before running a release sync for m${TARGET}."
            exit 1
          fi
        fi
        echo "is_release=$IS_RELEASE" >> "$GITHUB_OUTPUT"
        echo "base_branch=$BASE_BRANCH" >> "$GITHUB_OUTPUT"
        echo "skia_base_branch=$SKIA_BASE_BRANCH" >> "$GITHUB_OUTPUT"
        echo "head_branch=$HEAD_BRANCH" >> "$GITHUB_OUTPUT"

        # `current` is the milestone of the BASE branch we're syncing INTO:
        #  - main line → main's milestone
        #  - release   → that line's milestone (== TARGET ⇒ a bug-fix-only sync)
        if [ "$IS_RELEASE" = true ]; then
          CURRENT=$(gh api "repos/$GITHUB_REPOSITORY/contents/scripts/VERSIONS.txt?ref=${BASE_BRANCH}" \
            --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}')
        else
          CURRENT="$MAIN_MS"
        fi
        echo "current=$CURRENT" >> "$GITHUB_OUTPUT"

        UPSTREAM_SHA=$(git ls-remote https://github.com/google/skia.git "refs/heads/chrome/m${TARGET}" | awk '{print $1}')
        if [ -z "$UPSTREAM_SHA" ]; then
          echo "::notice::upstream/chrome/m${TARGET} does not exist yet"
          if [ "$MODE" = "explicit" ]; then
            exit 1
          fi
          echo "skip=true" >> "$GITHUB_OUTPUT"
          exit 0
        fi

        # Check whether there is anything new to merge from upstream. Compare upstream
        # HEAD against the existing sync branch if present; otherwise, for release lines,
        # against the release base branch. This avoids spinning up the expensive agent
        # job when there's nothing new. (For a main milestone bump the sync branch won't
        # exist yet and there is always new work, so we proceed.)
        SYNC_SHA=$(git ls-remote https://github.com/mono/skia.git "refs/heads/${HEAD_BRANCH}" | awk '{print $1}')
        COMPARE_REF=""
        if [ -n "$SYNC_SHA" ]; then
          COMPARE_REF="$HEAD_BRANCH"
        elif [ "$IS_RELEASE" = true ]; then
          COMPARE_REF="$SKIA_BASE_BRANCH"
        fi
        if [ -n "$COMPARE_REF" ]; then
          BEHIND=$(gh api "repos/mono/skia/compare/${UPSTREAM_SHA}...${COMPARE_REF}" \
            --jq '.behind_by' 2>/dev/null || echo "unknown")
          if [ "$BEHIND" = "0" ]; then
            echo "::notice::chrome/m${TARGET} already fully merged into ${COMPARE_REF} (upstream HEAD: ${UPSTREAM_SHA:0:12}) — skipping"
            echo "skip=true" >> "$GITHUB_OUTPUT"
            exit 0
          fi
          echo "${COMPARE_REF} exists but is ${BEHIND} commits behind upstream"
        fi

        echo "Will process: m${TARGET} → ${BASE_BRANCH} (mode=${MODE}, base milestone=m${CURRENT}, latest=m${LATEST}, head=${HEAD_BRANCH})"

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
    env:
      INPUT_MODE: ${{ github.event.inputs.mode }}
      INPUT_MILESTONE: ${{ github.event.inputs.milestone }}
      SCHEDULE: ${{ github.event.schedule }}
      GH_TOKEN: ${{ github.token }}
    run: |
      # Re-derive the sync base branch here (same rules as the pre-activation
      # `Detect milestone` step, which stays the source of truth). We can't read
      # its outputs: this step runs in the agent job, which only `needs:`
      # activation, and the cheap pre-activation job has no checkout to share a
      # script from. github.event inputs ARE available in any job, so we redo the
      # small base-branch resolution from them.
      if [ -n "$INPUT_MODE" ]; then MODE="$INPUT_MODE";
      elif [ "$SCHEDULE" = "0 7 * * *" ]; then MODE=current;
      elif [ "$SCHEDULE" = "0 17 * * *" ]; then MODE=latest;
      else MODE=next; fi
      MAIN_MS=$(gh api "repos/$GITHUB_REPOSITORY/contents/scripts/VERSIONS.txt?ref=$GITHUB_REF" \
        --jq '.content' | base64 -d | grep '^libSkiaSharp.*milestone' | awk '{print $NF}')
      if [ -n "$INPUT_MILESTONE" ]; then TARGET="$INPUT_MILESTONE";
      elif [ "$MODE" = "latest" ]; then
        TARGET=$(git ls-remote --heads https://github.com/google/skia.git 'refs/heads/chrome/m*' \
          | sed -n 's|.*refs/heads/chrome/m\([0-9]*\)$|\1|p' | sort -n | tail -1);
      elif [ "$MODE" = "current" ]; then TARGET="$MAIN_MS";
      else TARGET="$((MAIN_MS + 1))"; fi

      BASE_BRANCH=main
      SKIA_BASE_BRANCH=skiasharp
      if [ "$TARGET" != "$MAIN_MS" ]; then
        RELEASE_BRANCH=$(git ls-remote --heads "https://github.com/${GITHUB_REPOSITORY}.git" "refs/heads/release/*.${TARGET}.x" \
          | sed -n 's|.*refs/heads/\(release/[0-9][0-9.]*\.x\)$|\1|p' | head -1)
        if [ -n "$RELEASE_BRANCH" ]; then
          BASE_BRANCH="$RELEASE_BRANCH"
          SKIA_BASE_BRANCH="$RELEASE_BRANCH"
        fi
      fi

      # The checkout uses the workflow branch (main), so the submodule may be at a
      # different SHA than the base branch expects. Fix it before the agent runs.
      # The submodule tracks `${SKIA_BASE_BRANCH}` in mono/skia (skiasharp for a
      # main sync, release/<major>.<milestone>.x for a release sync), so the
      # base-branch submodule SHA should be a commit on that branch.
      echo "Aligning submodule to origin/${BASE_BRANCH} (mono/skia ${SKIA_BASE_BRANCH})"
      git fetch origin "$BASE_BRANCH" 2>&1 || true
      BASE_SUB_SHA=$(git ls-tree "origin/${BASE_BRANCH}" -- externals/skia | awk '{print $3}')
      echo "origin/${BASE_BRANCH} submodule SHA: $BASE_SUB_SHA"
      git -C externals/skia fetch origin "$SKIA_BASE_BRANCH" 2>&1
      git -C externals/skia checkout "$BASE_SUB_SHA" 2>&1
      echo "Verifying SHA is on ${SKIA_BASE_BRANCH} branch:"
      git -C externals/skia branch -r --contains "$BASE_SUB_SHA" | grep -q "origin/${SKIA_BASE_BRANCH}" \
        && echo "  ✅ SHA is on origin/${SKIA_BASE_BRANCH}" \
        || echo "  ⚠️ SHA is NOT on origin/${SKIA_BASE_BRANCH} — submodule pointer may be stale"
  - name: Copy push script for post-step
    run: |
      cp .github/scripts/skia-sync-push-prs.sh /tmp/gh-aw/skia-sync-push-prs.sh

# -- Pre-agent steps ---------------------------------------------------
# Run on the host before the agent starts. Packages installed here are visible
# in the AWF chroot (shared host filesystem), but dotnet tool restore does NOT
# carry into the chroot — the agent must run it itself.
pre-agent-steps:
  - name: Install native build dependencies
    run: |
      sudo apt-get update -qq
      sudo apt-get install -y clang fontconfig libfontconfig1-dev ninja-build fonts-dejavu-core ttf-ancient-fonts
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

**Read `.agents/skills/update-skia/SKILL.md` and follow Phases 2-10.** Notes specific to this automated workflow:

- **Phase 1 is pre-computed** (above). Skip it — but you still need to add the `upstream` remote
  and fetch `chrome/m${{ needs.pre_activation.outputs.target }}` (Phase 1 step 4) since Phase 5 depends on it.
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
