---
description: "Daily API documentation pipeline — regenerates XML stubs from CI NuGets, then AI fills 'To be added.' placeholders."

# -- Triggers ----------------------------------------------------------
on:
  schedule:
    - cron: "0 8 * * *"
  workflow_dispatch:

# -- Custom jobs -------------------------------------------------------
# Stub regeneration requires Windows (mdoc.exe is .NET Framework).
# Checks out docs main, runs mdoc, uploads result as artifact.
jobs:
  regenerate-stubs:
    runs-on: windows-latest
    steps:
      - name: Checkout SkiaSharp
        uses: actions/checkout@v4
        with:
          fetch-depth: 0
          submodules: recursive
      - name: Align docs to latest main
        shell: bash
        run: |
          cd docs
          git fetch origin main
          git checkout -B automation/write-api-docs origin/main
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
      - name: Upload regenerated docs
        uses: actions/upload-artifact@v4
        with:
          name: docs-regenerated
          path: docs/SkiaSharpAPI/
          retention-days: 1

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
  - name: Set up docs branch
    run: |
      cd docs
      git fetch origin main
      git checkout -B automation/write-api-docs origin/main
      cd ..

pre-agent-steps:
  - name: Download regenerated docs
    uses: actions/download-artifact@v4
    with:
      name: docs-regenerated
      path: docs/SkiaSharpAPI/

  - name: Install lxml and extract placeholders
    run: |
      
      mkdir -p output/docs-work
      pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 extract docs/SkiaSharpAPI/ -Output output/docs-work/

  - name: Copy push script for post-step
    run: |
      cp .github/scripts/api-docs-push-pr.sh /tmp/gh-aw/api-docs-push-pr.sh

# -- Post-agent steps --------------------------------------------------
post-steps:
  - name: Merge docs and push
    env:
      GH_TOKEN: ${{ secrets.SKIASHARP_AUTOBUMP_TOKEN }}
    run: |
      pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
      dotnet tool restore
      dotnet cake --target=docs-format-docs || true
      bash /tmp/gh-aw/api-docs-push-pr.sh
---

# Auto API Docs Writer

**Read `.agents/skills/api-docs/SKILL.md` and follow Phases 3–5.** Overrides for this workflow:

- **Phases 1–2 are pre-computed** — stub regeneration and JSON extraction already ran. Skip them.
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't carry this into the chroot).
- **Do NOT edit XML files directly** — edit only the JSON files in `output/docs-work/`.
- **Phase 6 (merge) is handled by the post-step** — do NOT merge or run docs-format-docs yourself.

Your workflow:
1. **Phase 3 (Discover)** — read patterns, study existing good docs, read source code
2. **Phase 4 (Write)** — fill JSON files with documentation
3. **Phase 5 (Review)** — launch two background agents, fix issues, repeat until clean

Invalid docs are worse than no docs. Only validated, accurate documentation should remain in the JSON files when you finish.
