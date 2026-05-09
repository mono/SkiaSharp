---
description: "Daily AI-powered API documentation writer — fills 'To be added.' placeholders in mono/SkiaSharp-API-docs."

# -- Triggers ----------------------------------------------------------
on:
  schedule:
    - cron: "0 8 * * *"
  workflow_dispatch:
    inputs:
      max_files:
        description: "Maximum number of files to process (0 = unlimited)"
        required: false
        type: string
        default: "0"

  # -- Pre-activation step -------------------------------------------
  # Check whether there are any "To be added." placeholders in the docs repo.
  # Exit 1 = skip the entire workflow (nothing to do).
  steps:
    - name: Check for placeholders
      id: check
      run: |
        # Get the latest commit SHA on docs repo main (public repo, no auth needed)
        DOCS_SHA=$(gh api repos/mono/SkiaSharp-API-docs/git/ref/heads/main --jq '.object.sha')
        echo "docs_sha=$DOCS_SHA" >> "$GITHUB_OUTPUT"
        echo "Docs repo main SHA: $DOCS_SHA"

        # Clone the docs repo shallowly to count placeholders (public, no auth needed)
        git clone --depth 1 https://github.com/mono/SkiaSharp-API-docs.git /tmp/docs-check
        PLACEHOLDER_COUNT=$(grep -r "To be added" /tmp/docs-check/SkiaSharpAPI/ 2>/dev/null | wc -l | tr -d ' ')
        echo "placeholder_count=$PLACEHOLDER_COUNT" >> "$GITHUB_OUTPUT"
        echo "Placeholder count: $PLACEHOLDER_COUNT"

        if [ "$PLACEHOLDER_COUNT" -eq 0 ]; then
          echo "::notice::No 'To be added.' placeholders found — nothing to do"
          exit 1
        fi

        # Count affected files
        FILE_COUNT=$(grep -rl "To be added" /tmp/docs-check/SkiaSharpAPI/ 2>/dev/null | wc -l | tr -d ' ')
        echo "file_count=$FILE_COUNT" >> "$GITHUB_OUTPUT"
        echo "Files with placeholders: $FILE_COUNT, Total placeholders: $PLACEHOLDER_COUNT"

        rm -rf /tmp/docs-check

jobs:
  pre-activation:
    outputs:
      docs_sha: ${{ steps.check.outputs.docs_sha }}
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
      DOCS_SHA="${GH_AW_NEEDS_PRE_ACTIVATION_OUTPUTS_DOCS_SHA:-${{ needs.pre_activation.outputs.docs_sha }}}"
      echo "Checking out docs submodule at $DOCS_SHA (docs main HEAD)"
      cd docs
      git fetch origin main
      git checkout "$DOCS_SHA"
      cd ..
      echo "docs submodule aligned to docs main: $DOCS_SHA"
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

There are **${{ needs.pre_activation.outputs.placeholder_count }}** "To be added." placeholders across **${{ needs.pre_activation.outputs.file_count }}** files in `mono/SkiaSharp-API-docs`.

Your job is to fill in these placeholders with proper API documentation following .NET guidelines.

## Step 1 — Read the documentation skill

Read these files to understand documentation patterns and quality criteria:

- `.agents/skills/api-docs/SKILL.md`
- `.agents/skills/api-docs/references/patterns.md`
- `.agents/skills/api-docs/references/checklist.md`

These contain the XML patterns, verb conventions, parameter/return docs rules, and quality checklist.

## Step 2 — Set up the docs submodule

The pre-agent step already aligned the `docs/` submodule to the latest `main`. Create a working branch:

```bash
cd docs
git checkout -b automation/write-api-docs
cd ..
```

## Step 3 — Find files with placeholders

Search for "To be added." across `docs/SkiaSharpAPI/`:

```bash
grep -rl "To be added" docs/SkiaSharpAPI/ | sort
```

Prioritize files in this order:
1. `docs/SkiaSharpAPI/SkiaSharp/` — core namespace (most important)
2. `docs/SkiaSharpAPI/HarfBuzzSharp/` — text shaping
3. Other namespaces (Views, etc.)

**Max files**: ${{ github.event.inputs.max_files || '0' }} (0 = unlimited — process all files).

## Step 4 — Write documentation for each file

For each file with placeholders:

1. **Read the XML file** to understand the type and its members
2. **Read the corresponding C# source** from `binding/` to understand what each API does. The type name maps to the source:
   - `docs/SkiaSharpAPI/SkiaSharp/SKCanvas.xml` → search in `binding/SkiaSharp/` for `SKCanvas`
   - `docs/SkiaSharpAPI/HarfBuzzSharp/Buffer.xml` → search in `binding/HarfBuzzSharp/` for `Buffer`
3. **Replace "To be added." placeholders** with proper documentation following the patterns from Step 1
4. **Validate the XML** after editing each file:
   ```bash
   xmllint --noout docs/SkiaSharpAPI/<Namespace>/<TypeName>.xml
   ```

Key rules (from the skill):
- Summaries start with third-person present-tense verb
- Properties: "Gets or sets..." / "Gets..." / "Gets a value indicating whether..."
- Constructors: "Initializes a new instance of the..."
- Parameters: noun phrase starting with article (The, A, An)
- Boolean params: "true to..." (NOT "true if...")
- Boolean returns: "true if..." (NOT "true to...")
- Use `<see cref="T:..." />`, `<see langword="null" />`, `<paramref name="..." />`
- Don't just repeat the member name — add meaningful context

## Step 5 — Format and validate

After processing all files, run the formatting target:

```bash
dotnet tool restore
dotnet cake --target=docs-format-docs
```

This validates the XML and reports documentation coverage. Fix any errors it reports.

## Step 6 — Commit and write output

Commit all changes inside the `docs/` submodule:

```bash
cd docs
git add -A
git diff --cached --quiet && echo "No changes" && exit 0
git config user.name "github-actions[bot]"
git config user.email "41898282+github-actions[bot]@users.noreply.github.com"
git commit -m "Fill API documentation placeholders

AI-generated documentation for XML API docs with 'To be added.' placeholders.
Follows .NET API documentation guidelines."
cd ..
```

Then write the signal file and summary for the post-step:

```bash
mkdir -p /tmp/gh-aw/agent
cat > /tmp/gh-aw/agent/api-docs-env.sh << 'EOF'
DOCS_BRANCH=automation/write-api-docs
EOF
```

Also write `/tmp/gh-aw/agent/api-docs-summary.md` with:
- Number of files processed
- Number of placeholders filled
- Any files skipped and why
- Any validation issues encountered

**IMPORTANT:** Do NOT push branches or create PRs — the post-step handles that.

If there are no changes after processing (all "To be added." were in generated files or already filled), do NOT write `api-docs-env.sh`. The post-step will detect its absence and skip.
