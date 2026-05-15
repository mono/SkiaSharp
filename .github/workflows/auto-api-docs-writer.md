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
# Shallow checkout for SkiaSharp + skia submodule (source reference only).
# Docs repo checked out separately so create-pull-request can target it.
checkout:
  - fetch-depth: 1
    submodules: recursive
  - repository: mono/SkiaSharp-API-docs
    path: docs
    fetch-depth: 1
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

# -- Safe outputs ------------------------------------------------------
safe-outputs:
  create-pull-request:
    target-repo: "mono/SkiaSharp-API-docs"
    draft: false
    base-branch: main
    preserve-branch-name: true
    recreate-ref: true

# -- Pre-agent steps (host) -------------------------------------------
steps:
  - name: Set up agent output directory
    run: |
      mkdir -p /tmp/gh-aw/agent

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
---

# Auto API Docs Writer

**Read `.agents/skills/api-docs/SKILL.md` and follow Phases 3–5.** Overrides for this workflow:

- **Phases 1–2 are pre-computed** — stub regeneration and JSON extraction already ran. Skip them.
- **First thing**: run `dotnet tool restore` (pre-agent-steps can't carry this into the chroot).
- **Do NOT edit XML files directly** — edit only the JSON files in `output/docs-work/`.

Your workflow:
1. **Phase 3 (Discover)** — read patterns, study existing good docs, read source code
2. **Phase 4 (Write)** — fill JSON files with documentation
3. **Phase 5 (Review)** — launch two background agents, fix issues, repeat until clean
4. **Finalize** — merge, validate, commit, and create the PR (see below)

## Finalize — Merge, Validate, and Create PR

After completing Phases 3–5:

1. **Merge** JSON changes back into XML:
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
   ```

2. **Format** the XML docs:
   ```bash
   dotnet cake --target=docs-format-docs || true
   ```

3. **Safety check** — verify no `MemberSignature` or `TypeSignature` lines were deleted:
   ```bash
   cd docs && git diff -- '*.xml' | grep '^-.*<\(MemberSignature\|TypeSignature\)' | head -20
   ```
   If any deletions appear, revert those files with `git checkout HEAD -- <file>` and re-run merge.

4. **Commit** in the docs directory:
   ```bash
   cd docs
   git add -A
   git commit -m "Fill API documentation placeholders"
   ```

5. **Create PR** — use the `create_pull_request` tool:
   - Branch: `automation/write-api-docs`
   - Title: `Fill API documentation placeholders`
   - Body: `Automated AI-generated documentation for XML API docs with 'To be added.' placeholders.`

If there are no documentation changes after merging, skip the commit and PR.
