---
description: "Daily API documentation pipeline — regenerates XML stubs from CI NuGets, then AI fills 'To be added.' placeholders."

# -- Triggers ----------------------------------------------------------
on:
  schedule:
    - cron: "0 8 * * *"
  pull_request:
    paths:
      - .github/workflows/auto-api-docs-writer.md
      - .github/workflows/auto-api-docs-writer.lock.yml
      - .github/scripts/api-docs-push-pr.sh
      - .agents/skills/api-docs/**
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

  # No pre-activation gate — stub regeneration in pre-agent-steps may
  # create new placeholders that don't exist yet on docs main.
  # The agent checks for placeholders itself and exits early if none.

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
      git checkout -B automation/write-api-docs origin/main
      cd ..
      echo "docs submodule on automation/write-api-docs at docs main"

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
