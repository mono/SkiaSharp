---
description: "Daily upstream Skia milestone sync - merges new commits, resolves conflicts, builds, tests, and creates PRs."

# -- Engine ------------------------------------------------------------
# Pin GPT-5.6 Sol for the primary update work so scheduled runs never fall back
# to a lower default model.
engine:
  id: copilot
model: gpt-5.6-sol

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
  # skip=true output = nothing to sync; skip=false = verified work to do.
  # A missing output never activates the agent.
  # Outputs are available in the prompt via ${{ needs.pre_activation.outputs.* }}.
  steps:
    - name: Check out detection scripts
      uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
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
  # `needs` is additive for compiler-managed jobs: keep the generated gates and make
  # pre_activation directly visible wherever the staged safe-output config is evaluated.
  # Keep the supported hyphenated alias: gh-aw v0.87.10 normalizes it in the lockfile, while
  # the underscored spelling is misclassified during pre-activation discovery and forms a cycle.
  agent:
    needs: [pre-activation]
  safe_outputs:
    needs: [pre-activation]
  conclusion:
    # The compiler-generated conclusion job is the fresh downstream trust
    # boundary: it already needs agent, detection, and safe_outputs.
    pre-steps:
      - name: Authorize downstream delivery
        id: delivery_gate
        if: >-
          needs.agent.result == 'success' &&
          needs.detection.result == 'success' &&
          needs.detection.outputs.detection_success == 'true' &&
          needs.safe_outputs.result == 'success' &&
          needs.safe_outputs.outputs.process_safe_outputs_status == 'success' &&
          needs.safe_outputs.outputs.process_safe_outputs_processed_count == '1' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_succeeded == '1' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_failed == '0' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_skipped == '0' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_warnings == '0' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_cancelled == '0' &&
          needs.safe_outputs.outputs.process_safe_outputs_items_deferred == '0'
        shell: /bin/sh -e {0}
        run: printf 'authorized=true\n' >> "$GITHUB_OUTPUT"
      - name: Check out trusted delivery code
        if: steps.delivery_gate.outputs.authorized == 'true'
        uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
        with:
          persist-credentials: false
          sparse-checkout: |
            .agents/skills/update-skia
            .github/scripts/skia-sync-detect.sh
            .github/scripts/skia-sync-push-prs.sh
      - name: Download immutable delivery package
        if: steps.delivery_gate.outputs.authorized == 'true'
        uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1
        with:
          name: skia-sync-delivery
          path: ${{ runner.temp }}/skia-sync-delivery-artifact
      - name: Validate delivery package and remote bases
        if: steps.delivery_gate.outputs.authorized == 'true'
        shell: /bin/sh -e {0}
        env:
          BASH_ENV: /dev/null
          ENV: /dev/null
          GH_TOKEN: ${{ github.token }}
          INPUT_BASE_BRANCH: ${{ github.event.inputs.base_branch }}
          INPUT_TARGET: ${{ github.event.inputs.target }}
          LD_AUDIT: ""
          LD_LIBRARY_PATH: ""
          LD_PRELOAD: ""
        run: |
          DELIVERY_HOME=$(mktemp -d "$RUNNER_TEMP/skia-sync-delivery-home.XXXXXX")
          VERIFIED_ROOT=$(mktemp -d "$RUNNER_TEMP/skia-sync-verified.XXXXXX")
          chmod 700 "$DELIVERY_HOME" "$VERIFIED_ROOT"
          {
            printf 'SKIA_SYNC_DELIVERY_HOME=%s\n' "$DELIVERY_HOME"
            printf 'SKIA_SYNC_VERIFIED_ROOT=%s\n' "$VERIFIED_ROOT"
          } >> "$GITHUB_ENV"

          RESOLVED_ENV="$DELIVERY_HOME/resolved.env"
          /usr/bin/env -i \
            HOME="$DELIVERY_HOME" \
            PATH=/usr/bin:/bin \
            GIT_CONFIG_GLOBAL=/dev/null \
            GIT_CONFIG_NOSYSTEM=1 \
            GIT_NO_REPLACE_OBJECTS=1 \
            GIT_TERMINAL_PROMPT=0 \
            GH_TOKEN="$GH_TOKEN" \
            GITHUB_REF="$GITHUB_REF" \
            GITHUB_REPOSITORY="$GITHUB_REPOSITORY" \
            GITHUB_RUN_NUMBER="$GITHUB_RUN_NUMBER" \
            GITHUB_SHA="$GITHUB_SHA" \
            /bin/bash --noprofile --norc \
              "$GITHUB_WORKSPACE/.github/scripts/skia-sync-detect.sh" \
              --resolve-only \
              --target "$INPUT_TARGET" \
              --base-branch "$INPUT_BASE_BRANCH" \
              --output "$RESOLVED_ENV"
          /usr/bin/python3 -I - "$RESOLVED_ENV" "$DELIVERY_HOME/resolved.sh" "$GITHUB_ENV" <<'PY'
          import shlex
          import sys
          import unicodedata

          source, shell_output, github_env = sys.argv[1:]
          names = {
              "base_branch": "SKIA_SYNC_BASE_BRANCH",
              "current": "SKIA_SYNC_CURRENT",
              "head_branch": "SKIA_SYNC_HEAD_BRANCH",
              "is_release": "SKIA_SYNC_IS_RELEASE",
              "mode": "SKIA_SYNC_MODE",
              "skia_base_branch": "SKIA_SYNC_SKIA_BASE_BRANCH",
              "target": "SKIA_SYNC_TARGET",
              "upstream_ref": "SKIA_SYNC_UPSTREAM_REF",
          }
          values = {}
          with open(source, encoding="utf-8") as stream:
              for raw_line in stream:
                  key, separator, value = raw_line.rstrip("\n").partition("=")
                  if not separator or key not in names or key in values:
                      raise SystemExit("::error::Resolved delivery metadata is malformed.")
                  if not value or any(unicodedata.category(character) == "Cc" for character in value):
                      raise SystemExit("::error::Resolved delivery metadata contains an invalid value.")
                  values[key] = value
          if set(values) != set(names):
              raise SystemExit("::error::Resolved delivery metadata is incomplete.")
          with open(shell_output, "w", encoding="utf-8") as shell_stream, \
                  open(github_env, "a", encoding="utf-8") as env_stream:
              for key, env_name in sorted(names.items()):
                  value = values[key]
                  shell_stream.write(f"export {env_name}={shlex.quote(value)}\n")
                  env_stream.write(f"{env_name}={value}\n")
          PY
          # shellcheck disable=SC1090
          . "$DELIVERY_HOME/resolved.sh"
          unset GH_TOKEN

          resolve_remote_head() {
            repo="$1"
            branch="$2"
            output=$(mktemp "$RUNNER_TEMP/skia-sync-remote-head.XXXXXX")
            /usr/bin/env -i \
              HOME="$DELIVERY_HOME" \
              PATH=/usr/bin:/bin \
              GIT_CONFIG_GLOBAL=/dev/null \
              GIT_CONFIG_NOSYSTEM=1 \
              GIT_NO_REPLACE_OBJECTS=1 \
              GIT_TERMINAL_PROMPT=0 \
              /usr/bin/git \
                -c core.hooksPath=/dev/null \
                -c core.fsmonitor=false \
                -c credential.helper= \
                ls-remote --exit-code --heads "https://github.com/${repo}.git" \
                "refs/heads/${branch}" >"$output"
            /usr/bin/python3 -I - "$output" "refs/heads/${branch}" <<'PY'
          import re
          import sys

          content = open(sys.argv[1], "rb").read()
          match = re.fullmatch(rb"([0-9a-f]{40})\t" + re.escape(sys.argv[2].encode()) + rb"\n?", content)
          if not match:
              raise SystemExit("::error::Remote base branch did not resolve to exactly one immutable commit.")
          print(match.group(1).decode())
          PY
            rm -f -- "$output"
          }

          PARENT_BASE_SHA=$(resolve_remote_head mono/SkiaSharp "$SKIA_SYNC_BASE_BRANCH")
          SKIA_BASE_SHA=$(resolve_remote_head mono/skia "$SKIA_SYNC_SKIA_BASE_BRANCH")
          {
            printf 'SKIA_SYNC_EXPECTED_PARENT_BASE_SHA=%s\n' "$PARENT_BASE_SHA"
            printf 'SKIA_SYNC_EXPECTED_SKIA_BASE_SHA=%s\n' "$SKIA_BASE_SHA"
          } >> "$GITHUB_ENV"

          /usr/bin/env -i \
            HOME="$DELIVERY_HOME" \
            PATH=/usr/bin:/bin \
            GIT_CONFIG_GLOBAL=/dev/null \
            GIT_CONFIG_NOSYSTEM=1 \
            GIT_NO_REPLACE_OBJECTS=1 \
            GITHUB_SHA="$GITHUB_SHA" \
            SKIA_SYNC_BASE_BRANCH="$SKIA_SYNC_BASE_BRANCH" \
            SKIA_SYNC_CURRENT="$SKIA_SYNC_CURRENT" \
            SKIA_SYNC_DELIVERY_ENV_FILE="$GITHUB_ENV" \
            SKIA_SYNC_DELIVERY_PACKAGE_DIR="$RUNNER_TEMP/skia-sync-delivery-artifact" \
            SKIA_SYNC_EXPECTED_PARENT_BASE_SHA="$PARENT_BASE_SHA" \
            SKIA_SYNC_EXPECTED_SKIA_BASE_SHA="$SKIA_BASE_SHA" \
            SKIA_SYNC_HEAD_BRANCH="$SKIA_SYNC_HEAD_BRANCH" \
            SKIA_SYNC_IS_RELEASE="$SKIA_SYNC_IS_RELEASE" \
            SKIA_SYNC_SKIA_BASE_BRANCH="$SKIA_SYNC_SKIA_BASE_BRANCH" \
            SKIA_SYNC_TARGET="$SKIA_SYNC_TARGET" \
            SKIA_SYNC_UPSTREAM_REF="$SKIA_SYNC_UPSTREAM_REF" \
            SKIA_SYNC_VERIFIED_ROOT="$VERIFIED_ROOT" \
            /bin/bash --noprofile --norc \
              "$GITHUB_WORKSPACE/.github/scripts/skia-sync-push-prs.sh" \
              --verify-delivery-package
      - name: Push branches and create PRs
        if: steps.delivery_gate.outputs.authorized == 'true'
        shell: /bin/sh -e {0}
        env:
          BASH_ENV: /dev/null
          ENV: /dev/null
          GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
          LD_AUDIT: ""
          LD_LIBRARY_PATH: ""
          LD_PRELOAD: ""
        run: |
          exec /usr/bin/env -i \
            HOME="$SKIA_SYNC_DELIVERY_HOME" \
            PATH=/usr/bin:/bin \
            GIT_CONFIG_GLOBAL=/dev/null \
            GIT_CONFIG_NOSYSTEM=1 \
            GIT_NO_REPLACE_OBJECTS=1 \
            GH_TOKEN="$GH_TOKEN" \
            GITHUB_REPOSITORY="$GITHUB_REPOSITORY" \
            RUNNER_TEMP="$RUNNER_TEMP" \
            SKIA_SYNC_ARTIFACT_DIR="$SKIA_SYNC_ARTIFACT_DIR" \
            SKIA_SYNC_BASE_BRANCH="$SKIA_SYNC_BASE_BRANCH" \
            SKIA_SYNC_BASE_UPSTREAM_SHA="$SKIA_SYNC_BASE_UPSTREAM_SHA" \
            SKIA_SYNC_COMPLETION_SIGNAL_FILE="$SKIA_SYNC_COMPLETION_SIGNAL_FILE" \
            SKIA_SYNC_CURRENT="$SKIA_SYNC_CURRENT" \
            SKIA_SYNC_HEAD_BRANCH="$SKIA_SYNC_HEAD_BRANCH" \
            SKIA_SYNC_IS_RELEASE="$SKIA_SYNC_IS_RELEASE" \
            SKIA_SYNC_PARENT_BASE_SHA="$SKIA_SYNC_PARENT_BASE_SHA" \
            SKIA_SYNC_PARENT_REPO_DIR="$SKIA_SYNC_PARENT_REPO_DIR" \
            SKIA_SYNC_RUNTIME_DIR="$GITHUB_WORKSPACE" \
            SKIA_SYNC_SKIA_BASE_BRANCH="$SKIA_SYNC_SKIA_BASE_BRANCH" \
            SKIA_SYNC_SKIA_BASE_SHA="$SKIA_SYNC_SKIA_BASE_SHA" \
            SKIA_SYNC_SKIA_REPO_DIR="$SKIA_SYNC_SKIA_REPO_DIR" \
            SKIA_SYNC_SKILL_DIR="$GITHUB_WORKSPACE/.agents/skills/update-skia" \
            SKIA_SYNC_TARGET="$SKIA_SYNC_TARGET" \
            SKIA_SYNC_TARGET_UPSTREAM_SHA="$SKIA_SYNC_TARGET_UPSTREAM_SHA" \
            SKIA_SYNC_UPSTREAM_REF="$SKIA_SYNC_UPSTREAM_REF" \
            /bin/bash --noprofile --norc \
              "$GITHUB_WORKSPACE/.github/scripts/skia-sync-push-prs.sh"
      - name: Clean delivery workspace
        if: always()
        shell: /bin/sh -e {0}
        env:
          BASH_ENV: /dev/null
          ENV: /dev/null
          LD_AUDIT: ""
          LD_LIBRARY_PATH: ""
          LD_PRELOAD: ""
        run: |
          case "${SKIA_SYNC_DELIVERY_HOME:-}" in
            "$RUNNER_TEMP"/skia-sync-delivery-home.*) rm -rf -- "$SKIA_SYNC_DELIVERY_HOME" ;;
          esac
          case "${SKIA_SYNC_VERIFIED_ROOT:-}" in
            "$RUNNER_TEMP"/skia-sync-verified.*) rm -rf -- "$SKIA_SYNC_VERIFIED_ROOT" ;;
          esac

# -- Agent job gate --------------------------------------------------
# Only run the agent if pre-activation succeeded and explicitly found work to do.
if: needs.pre_activation.outputs.detect_result == 'success' && needs.pre_activation.outputs.skip == 'false'

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
# All real GitHub writes (push, both PRs) are done in the downstream delivery job.
# SKIASHARP_AUTOBUMP_TOKEN never enters the agent job. gh-aw can't create the mono/skia PR
# (the submodule's merge commits live in a nested repo gh-aw sees only as a gitlink), and
# `staged: true` keeps the agent from creating anything directly.
#
# `create-pull-request` is declared ONLY as an honest completion signal: it is kept STAGED
# (preview-only — NO real PR is created), so a successful sync registers as a pull-request
# output instead of being mislabeled a "no-op". The agent calls it when work was done and
# `noop` only when there genuinely was none. Pin both the effective base and its override
# allowlist to the detector's resolved branch so release/manual runs emit matching provenance.
safe-outputs:
  staged: true
  create-pull-request:
    staged: true
    if-no-changes: ignore
    base-branch: ${{ needs.pre_activation.outputs.base_branch }}
    allowed-base-branches:
      - ${{ needs.pre_activation.outputs.base_branch }}
  # report-as-issue defaults to true, but this workflow has no `issues: write` and a real sync
  # is NOT a no-op — disable the no-op→issue posting so genuine no-work runs don't try (and fail)
  # to file a "no-op runs" issue.
  noop:
    report-as-issue: false
  threat-detection:
    steps:
      - name: Check out trusted detector attestation code
        uses: actions/checkout@3d3c42e5aac5ba805825da76410c181273ba90b1 # v7.0.1
        with:
          persist-credentials: false
          sparse-checkout: .github/scripts/skia-sync-push-prs.sh
      - name: Download immutable detector attestation
        uses: actions/download-artifact@3e5f45b2cfb9172054b4087a40e8e0b5a5461e7c # v8.0.1
        with:
          name: skia-sync-delivery
          path: ${{ runner.temp }}/skia-sync-detector-artifact
      - name: Bind threat detection to immutable delivery objects
        shell: /bin/sh -e {0}
        env:
          BASH_ENV: /dev/null
          ENV: /dev/null
          LD_AUDIT: ""
          LD_LIBRARY_PATH: ""
          LD_PRELOAD: ""
        run: |
          ATTESTATION_HOME=$(mktemp -d "$RUNNER_TEMP/skia-sync-attestation-home.XXXXXX")
          chmod 700 "$ATTESTATION_HOME"
          exec /usr/bin/env -i \
            HOME="$ATTESTATION_HOME" \
            PATH=/usr/bin:/bin \
            GIT_CONFIG_GLOBAL=/dev/null \
            GIT_CONFIG_NOSYSTEM=1 \
            GIT_NO_REPLACE_OBJECTS=1 \
            RUNNER_TEMP="$RUNNER_TEMP" \
            SKIA_SYNC_DELIVERY_PACKAGE_DIR="$RUNNER_TEMP/skia-sync-detector-artifact" \
            SKIA_SYNC_THREAT_DETECTION_DIR=/tmp/gh-aw/threat-detection \
            /bin/bash --noprofile --norc \
              "$GITHUB_WORKSPACE/.github/scripts/skia-sync-push-prs.sh" \
              --verify-detection-attestation

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
    # Same target resolution as the pre_activation detect step (see there). The direct job
    # dependency makes those outputs available to the staged safe-output config; this step
    # re-runs the committed detector to materialize all resolved values as shell environment
    # and prepare the submodule plus exact upstream analysis range. For rotation runs (empty
    # target), both resolutions select the SAME line because the round-robin index is
    # GITHUB_RUN_NUMBER (identical across jobs) and main's config is read at the immutable
    # $GITHUB_SHA. skia-sync-detect.sh is the single source of truth.
    env:
      SYNC_TARGET: ${{ github.event.inputs.target }}
      SYNC_BASE_BRANCH: ${{ github.event.inputs.base_branch }}
      GH_TOKEN: ${{ github.token }}
    run: |
      OUT=$(mktemp)
      bash .github/scripts/skia-sync-detect.sh --resolve-only --output "$OUT" --target "$SYNC_TARGET" --base-branch "$SYNC_BASE_BRANCH"
      set -a
      # shellcheck disable=SC1090
      . "$OUT"
      set +a
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
  - name: Stage immutable workflow assets
    run: |
      RUNTIME_DIR="$RUNNER_TEMP/gh-aw/skia-sync-runtime"
      mkdir -p "$RUNTIME_DIR"
      cp -a .agents/skills/update-skia "$RUNTIME_DIR/update-skia"
      cp -a .github/scripts/skia-sync-push-prs.sh "$RUNTIME_DIR/skia-sync-push-prs.sh"
      {
        printf 'SKIA_SYNC_RUNTIME_DIR=%s\n' "$RUNTIME_DIR"
        printf 'SKIA_SYNC_SKILL_DIR=%s\n' "$RUNTIME_DIR/update-skia"
      } >> "$GITHUB_ENV"
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
# Run AFTER the AI finishes. Verify that deterministic finalization happened before
# the terminal safe-output call, then stage an immutable allowlisted handoff.
post-steps:
  - name: Verify finalized sync metadata
    shell: /bin/sh -e {0}
    env:
      BASH_ENV: /dev/null
      ENV: /dev/null
      LD_AUDIT: ""
      LD_LIBRARY_PATH: ""
      LD_PRELOAD: ""
      SKIA_SYNC_RUNTIME_DIR: ${{ runner.temp }}/gh-aw/skia-sync-runtime
    run: |
      /usr/bin/env -i \
        HOME="$RUNNER_TEMP/skia-sync-finalizer-home" \
        PATH=/usr/bin:/bin \
        GIT_CONFIG_GLOBAL=/dev/null \
        GIT_CONFIG_NOSYSTEM=1 \
        GIT_NO_REPLACE_OBJECTS=1 \
        GITHUB_WORKSPACE="$GITHUB_WORKSPACE" \
        SKIA_SYNC_HEAD_BRANCH="$SKIA_SYNC_HEAD_BRANCH" \
        SKIA_SYNC_SKILL_DIR="$SKIA_SYNC_SKILL_DIR" \
        /bin/bash --noprofile --norc -euo pipefail <<'FINALIZE'
      safe_git() {
        command git \
          -c core.hooksPath=/dev/null \
          -c core.fsmonitor=false \
          -c credential.helper= \
          -c commit.gpgSign=false \
          "$@"
      }

      reject_unsafe_repository() {
        local repo="$1"
        local git_dir
        local local_config
        local unsafe_config
        local replace_refs

        git_dir=$(safe_git -C "$repo" rev-parse --absolute-git-dir)
        if [[ -e "$git_dir/config.worktree" || -L "$git_dir/config.worktree" ]]; then
          echo "::error::Worktree Git configuration is forbidden in $repo."
          exit 1
        fi
        local_config=$(safe_git -C "$repo" config --local --no-includes --name-only --list)
        unsafe_config=$(printf '%s\n' "$local_config" | tr '[:upper:]' '[:lower:]' | grep -E \
          '^(extensions\.worktreeconfig|include(if\..*)?\.path|core\.(alternaterefscommand|attributesfile|editor|fsmonitor|hookspath|sshcommand|worktree)|diff\.(external|.*\.(command|textconv))|filter\..*\.(clean|smudge|process)|credential(\..*)?\.helper|remote\..*\.uploadpack|uploadpack\.packobjectshook|url\..*\.(insteadof|pushinsteadof)|commit\.gpgsign|gpg\..*|sequence\.editor)$' || true)
        if [[ -n "$unsafe_config" ]]; then
          echo "::error::Unsafe local Git configuration is forbidden in $repo: $unsafe_config"
          exit 1
        fi
        replace_refs=$(safe_git -C "$repo" for-each-ref --format='%(refname)' refs/replace)
        if [[ -n "$replace_refs" ]]; then
          echo "::error::Replacement refs are forbidden in $repo."
          exit 1
        fi
      }

      reject_unsafe_repository "$GITHUB_WORKSPACE"
      reject_unsafe_repository "$GITHUB_WORKSPACE/externals/skia"
      test "$(safe_git branch --show-current)" = "$SKIA_SYNC_HEAD_BRANCH"
      test "$(safe_git -C externals/skia branch --show-current)" = "$SKIA_SYNC_HEAD_BRANCH"

      UNEXPECTED_CHANGES=$(
        {
          safe_git diff --no-ext-diff --no-textconv --name-only
          safe_git diff --cached --no-ext-diff --no-textconv --name-only
        } | sort -u | grep -Ev '^(cgmanifest\.json|scripts/VERSIONS\.txt|scripts/azure-templates-variables\.yml|externals/skia)$' || true
      )
      if [[ -n "$UNEXPECTED_CHANGES" ]]; then
        echo "::error::The agent left uncommitted semantic changes:"
        echo "$UNEXPECTED_CHANGES"
        exit 1
      fi

      command python3 -I "$SKIA_SYNC_SKILL_DIR/scripts/update_versions.py" --repo-root "$GITHUB_WORKSPACE"

      if ! safe_git -C externals/skia diff --no-ext-diff --no-textconv --quiet ||
         ! safe_git -C externals/skia diff --cached --no-ext-diff --no-textconv --quiet; then
        echo "::error::The finalizer changed mono/skia; the agent did not commit its native work."
        exit 1
      fi

      if ! safe_git diff --no-ext-diff --no-textconv --quiet ||
         ! safe_git diff --cached --no-ext-diff --no-textconv --quiet; then
        echo "::error::The agent must finalize and commit deterministic metadata before create_pull_request."
        exit 1
      fi
      FINALIZE
  - name: Stage immutable delivery package
    shell: /bin/sh -e {0}
    env:
      BASH_ENV: /dev/null
      ENV: /dev/null
      LD_AUDIT: ""
      LD_LIBRARY_PATH: ""
      LD_PRELOAD: ""
      SKIA_SYNC_COMPLETION_SIGNAL_FILE: ${{ runner.temp }}/gh-aw/safeoutputs/outputs.jsonl
      SKIA_SYNC_RUNTIME_DIR: ${{ runner.temp }}/gh-aw/skia-sync-runtime
    run: |
      PACKAGE_DIR=$(mktemp -d "$RUNNER_TEMP/skia-sync-delivery-package.XXXXXX")
      chmod 700 "$PACKAGE_DIR"
      trap 'rm -rf -- "$PACKAGE_DIR"' EXIT
      /usr/bin/env -i \
        HOME="$RUNNER_TEMP/skia-sync-package-home" \
        PATH=/usr/bin:/bin \
        GIT_CONFIG_GLOBAL=/dev/null \
        GIT_CONFIG_NOSYSTEM=1 \
        GIT_NO_REPLACE_OBJECTS=1 \
        GITHUB_SHA="$GITHUB_SHA" \
        GITHUB_WORKSPACE="$GITHUB_WORKSPACE" \
        RUNNER_TEMP="$RUNNER_TEMP" \
        SKIA_SYNC_ARTIFACT_DIR="$SKIA_SYNC_ARTIFACT_DIR" \
        SKIA_SYNC_BASE_BRANCH="$SKIA_SYNC_BASE_BRANCH" \
        SKIA_SYNC_BASE_UPSTREAM_SHA="$SKIA_SYNC_BASE_UPSTREAM_SHA" \
        SKIA_SYNC_COMPLETION_SIGNAL_FILE="$SKIA_SYNC_COMPLETION_SIGNAL_FILE" \
        SKIA_SYNC_CURRENT="$SKIA_SYNC_CURRENT" \
        SKIA_SYNC_HEAD_BRANCH="$SKIA_SYNC_HEAD_BRANCH" \
        SKIA_SYNC_IS_RELEASE="$SKIA_SYNC_IS_RELEASE" \
        SKIA_SYNC_PARENT_BASE_SHA="$SKIA_SYNC_PARENT_BASE_SHA" \
        SKIA_SYNC_RUNTIME_DIR="$SKIA_SYNC_RUNTIME_DIR" \
        SKIA_SYNC_SKIA_BASE_BRANCH="$SKIA_SYNC_SKIA_BASE_BRANCH" \
        SKIA_SYNC_SKIA_BASE_SHA="$SKIA_SYNC_SKIA_BASE_SHA" \
        SKIA_SYNC_SKILL_DIR="$SKIA_SYNC_SKILL_DIR" \
        SKIA_SYNC_TARGET="$SKIA_SYNC_TARGET" \
        SKIA_SYNC_TARGET_UPSTREAM_SHA="$SKIA_SYNC_TARGET_UPSTREAM_SHA" \
        SKIA_SYNC_THREAT_DETECTION_SOURCE_DIR=/tmp/gh-aw \
        SKIA_SYNC_UPSTREAM_REF="$SKIA_SYNC_UPSTREAM_REF" \
        SKIA_SYNC_DELIVERY_PACKAGE_DIR="$PACKAGE_DIR" \
        "$SKIA_SYNC_RUNTIME_DIR/skia-sync-push-prs.sh" \
          --stage-delivery-package
      trap - EXIT
      printf 'SKIA_SYNC_DELIVERY_PACKAGE_DIR=%s\n' "$PACKAGE_DIR" >> "$GITHUB_ENV"
  - name: Upload immutable delivery package
    uses: actions/upload-artifact@043fb46d1a93c77aae656e7c1c64a875d1fc6a0a # v7.0.1
    with:
      name: skia-sync-delivery
      path: ${{ env.SKIA_SYNC_DELIVERY_PACKAGE_DIR }}
      if-no-files-found: error
      retention-days: 1
  - name: Clean staged delivery package
    if: always()
    shell: /bin/sh -e {0}
    env:
      BASH_ENV: /dev/null
      ENV: /dev/null
      LD_AUDIT: ""
      LD_LIBRARY_PATH: ""
      LD_PRELOAD: ""
    run: |
      case "${SKIA_SYNC_DELIVERY_PACKAGE_DIR:-}" in
        "$RUNNER_TEMP"/skia-sync-delivery-package.*)
          rm -rf -- "$SKIA_SYNC_DELIVERY_PACKAGE_DIR"
          ;;
      esac
---

# Sync - Skia Upstream

Read `$SKIA_SYNC_SKILL_DIR/SKILL.md` and use that immutable staged copy as the complete
engineering process. Load one numbered phase reference at a time from the same directory.

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
- The exact update-skia skill from the triggering workflow revision is staged read-only at
  `$SKIA_SYNC_SKILL_DIR`. It remains authoritative after the product checkout switches branches.
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
`/tmp/gh-aw/agent`, run the deterministic metadata finalizer, commit the final parent and nested
heads, and only then invoke staged `create_pull_request` once as the terminal completion signal.
Nothing may mutate either repository after that signal. The downstream conclusion job delivers
only the exact detector-attested objects and creates the two cross-linked draft PRs.
