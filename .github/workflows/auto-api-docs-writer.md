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

# -- Custom jobs -------------------------------------------------------
# Stub regeneration requires Windows (mdoc.exe is .NET Framework).
# Results are uploaded as artifacts and available to the agent.
jobs:
  regenerate-stubs:
    runs-on: windows-latest
    if: github.event.inputs.skip_regeneration != 'true'
    steps:
      - name: Checkout SkiaSharp
        uses: actions/checkout@v4
        with:
          fetch-depth: 0
          submodules: recursive
      - name: Align docs submodule to latest main
        run: |
          cd docs
          git fetch origin main
          git checkout origin/main
          cd ..
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'
      - name: Install GTK# 2
        shell: pwsh
        run: |
          $msiUrl = "https://github.com/mono/gtk-sharp/releases/download/2.12.45/gtk-sharp-2.12.45.msi"
          $msiPath = "$env:RUNNER_TEMP\gtk-sharp.msi"
          Invoke-WebRequest -Uri $msiUrl -OutFile $msiPath
          Start-Process msiexec.exe -ArgumentList "/i", $msiPath, "/quiet", "/norestart" -Wait -NoNewWindow
      - name: Restore tools
        run: dotnet tool restore
      - name: Download latest NuGet packages
        run: dotnet cake --target=docs-download-output
      - name: Regenerate API docs
        run: dotnet cake --target=update-docs
      - name: Package regenerated docs
        shell: bash
        run: |
          mkdir -p /tmp/gh-aw/agent
          tar czf /tmp/gh-aw/agent/docs-regenerated.tar.gz -C docs SkiaSharpAPI

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
  - name: Align docs submodule to working branch
    run: |
      cd docs
      git fetch origin main
      git checkout -B automation/write-api-docs origin/main
      cd ..
      echo "docs submodule on automation/write-api-docs at docs main"

pre-agent-steps:
  - name: Apply regenerated stubs
    run: |
      if [ -f /tmp/gh-aw/agent/docs-regenerated.tar.gz ]; then
        echo "Applying regenerated stubs from Windows job..."
        tar xzf /tmp/gh-aw/agent/docs-regenerated.tar.gz -C docs
        echo "Stubs applied"
      else
        echo "No regenerated stubs artifact — using docs main as-is"
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

- **Phase 1 is pre-computed** — stub regeneration already ran on Windows. Skip it.
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
