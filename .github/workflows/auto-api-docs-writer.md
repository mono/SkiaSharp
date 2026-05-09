---
description: "Daily API documentation pipeline — regenerates XML stubs from CI NuGets, then AI fills 'To be added.' placeholders."

# -- Triggers ----------------------------------------------------------
on:
  schedule:
    - cron: "0 8 * * *"
  workflow_dispatch:
    inputs:
      skip_regeneration:
        description: "Skip stub regeneration (Phase 1) — use when stubs are already up to date"
        required: false
        type: boolean
        default: false
      max_files:
        description: "Maximum number of files to process (0 = unlimited)"
        required: false
        type: string
        default: "0"

  # -- Pre-activation step -------------------------------------------
  # Lightweight check: clone docs repo and count existing placeholders.
  # Stub regeneration runs later in pre-agent-steps (once, not twice).
  # Exit 1 = skip the agent (nothing to do).
  steps:
    - name: Check for existing placeholders
      id: check
      run: |
        # Shallow clone to check current state (public repo, no auth)
        git clone --depth 1 https://github.com/mono/SkiaSharp-API-docs.git /tmp/docs-check
        PLACEHOLDER_COUNT=$(grep -rc "To be added" /tmp/docs-check/SkiaSharpAPI/ 2>/dev/null | awk -F: '{s+=$2} END {print s+0}')
        FILE_COUNT=$(grep -rl "To be added" /tmp/docs-check/SkiaSharpAPI/ 2>/dev/null | wc -l | tr -d ' ')
        rm -rf /tmp/docs-check
        echo "placeholder_count=$PLACEHOLDER_COUNT" >> "$GITHUB_OUTPUT"
        echo "file_count=$FILE_COUNT" >> "$GITHUB_OUTPUT"
        echo "Existing placeholders: $PLACEHOLDER_COUNT across $FILE_COUNT files"

        if [ "$PLACEHOLDER_COUNT" -eq 0 ]; then
          echo "::notice::No 'To be added.' placeholders found — nothing to do"
          exit 1
        fi

jobs:
  pre-activation:
    outputs:
      placeholder_count: ${{ steps.check.outputs.placeholder_count }}
      file_count: ${{ steps.check.outputs.file_count }}

# -- Agent gate --------------------------------------------------------
if: needs.pre_activation.outputs.check_result == 'success'

# -- Checkout ----------------------------------------------------------
checkout:
  - fetch-depth: 0
    submodules: recursive
timeout-minutes: 120
concurrency:
  group: auto-api-docs-writer
  cancel-in-progress: true

# -- Agent tools -------------------------------------------------------
tools:
  github:
    toolsets: [repos]
    allowed-repos: ["mono/skiasharp", "mono/skiasharp-api-docs"]
    min-integrity: none
  bash: ["*"]
  edit:

# -- Network allowlist -------------------------------------------------
network:
  allowed:
    - defaults
    - github
    - dotnet

# -- Permissions -------------------------------------------------------
permissions:
  contents: read

# -- Pre-agent steps (host) -------------------------------------------
steps:
  - name: Set up agent output directory
    run: |
      mkdir -p /tmp/gh-aw/agent
  - name: Align docs submodule to latest main
    run: |
      cd docs
      git fetch origin main
      git checkout origin/main
      cd ..
      echo "docs submodule aligned to docs main"

pre-agent-steps:
  - name: Restore tools and regenerate stubs
    env:
      SKIP_REGENERATION: ${{ github.event.inputs.skip_regeneration }}
    run: |
      dotnet tool restore
      if [ "$SKIP_REGENERATION" != "true" ]; then
        dotnet cake --target=docs-download-output
        dotnet cake --target=update-docs
      fi

  - name: Copy push script for post-step
    run: |
      cp .github/scripts/api-docs-push-pr.sh /tmp/gh-aw/api-docs-push-pr.sh

# -- Post-agent steps --------------------------------------------------
post-steps:
  - name: Push branch and create PR
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: bash /tmp/gh-aw/api-docs-push-pr.sh
---

# Auto API Docs Writer

There are **${{ needs.pre_activation.outputs.placeholder_count }}** "To be added." placeholders across **${{ needs.pre_activation.outputs.file_count }}** files.

**Read `.agents/skills/api-docs/SKILL.md` and follow Phases 2–5.** Overrides for this workflow:

- **Phase 1 is pre-computed** — stub regeneration already ran. Skip it.
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't carry this into the chroot).
- **Phase 5 branch**: use `automation/write-api-docs` as the branch name.
- **Max files**: ${{ github.event.inputs.max_files || '0' }} (0 = unlimited — process all files).

After Phase 5, write the signal file for the post-step:

```bash
mkdir -p /tmp/gh-aw/agent
cat > /tmp/gh-aw/agent/api-docs-env.sh << 'EOF'
DOCS_BRANCH=automation/write-api-docs
EOF
```

Also write `/tmp/gh-aw/agent/api-docs-summary.md` with a summary of files processed, placeholders filled, and any issues encountered.

**IMPORTANT:** Do NOT push branches or create PRs — the post-step handles that.
Do NOT call `create_pull_request`. Just commit locally.

If there are no changes after processing, do NOT write `api-docs-env.sh`.
The post-step will detect its absence and skip.
