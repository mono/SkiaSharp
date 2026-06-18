---
name: release-notes
description: >
  Generate or regenerate SkiaSharp's website release notes AND API changelogs as one
  coherent set under documentation/docfx/releases/. Two phases: (1) Prepare — one script
  (scripts/generate.sh) regenerates everything deterministic (the API-diff tree +
  co-release-map sidecar from the published NuGet feed, and each version page's raw-data
  block + page→API-diff links from git history); (2) Polish — the AI rewrites ONLY the
  prose of the pages the script flagged as changed.

  Use this skill whenever the user asks to:
  - Generate or regenerate release notes for a version ("write release notes for 3.119.2")
  - Regenerate or refresh the API changelogs / API diff ("update the api diff", "regen changelogs")
  - Refresh the whole releases/ set after publishing packages
  - Manually fix or update release notes that the automated workflow got wrong

  Triggers: "release notes for X", "regenerate release notes", "update API changelogs",
  "api diff", "changelog", "update website release notes", "write release notes",
  "refresh release notes".

    NOTE: The full set is normally regenerated automatically by the `update-release-notes`
  agentic workflow when code lands on main, release branches, or tags are pushed. In that
  automated run the Prepare script has already been run for you, so you skip it and only do
  the Polish phase. Run manually for regeneration or corrections, in which case you run the
  Prepare script yourself first and then polish. Either way, the pages to polish are listed
  in `output/files-to-polish.txt`.
---

# Release Notes & API Changelogs Skill

Generate SkiaSharp's website release notes **and** API changelogs as one coherent set.

This skill is used both by the `update-release-notes` agentic workflow (automatically
on push to `main`, `release/*` branches, and tags) and manually when regenerating,
correcting, or bulk-processing the release pages.

## How it works: prepare, then polish

The skill runs **one of two ways**, and both end at the same place — a list of pages to
polish in `output/files-to-polish.txt`:

- **Manually** (regeneration or corrections): you run the Prepare script yourself; it
  writes `output/files-to-polish.txt`, then you read it and polish those pages.
- **Automatically** (the `update-release-notes` workflow): Prepare has **already run** for
  you, so you **skip it**; `output/files-to-polish.txt` is already on disk — read it and
  polish those pages.

The skill is just **two phases**:

1. **Prepare** — run one script, `scripts/generate.sh`, which regenerates everything
   deterministic (the API-diff tree, each page's raw-data block, the page→API-diff
   links) and writes the "Files to polish" list to a file
   (`output/files-to-polish.txt`). No AI.
2. **Polish** — you (the AI) rewrite only the *prose* of the pages the script flagged as
   changed.

So the whole skill is: **run the script before you do anything, then do your one job —
polish.** `generate.sh` is the single entry point for the Prepare phase — you never need
to know what it runs internally.

> **Running unattended from the `update-release-notes` workflow?** The **Prepare** phase
> has **already been run for you** — the API-diff tree and the raw-data pages are already
> on disk. **Skip Prepare entirely** and go straight to
> [Polish the prose](#polish-the-prose), polishing exactly the files listed in
> `output/files-to-polish.txt` (one repo-relative path per line; an empty file means
> nothing changed — make no edits).

## Process

### Prepare — run the script first

> **In the automated workflow this already ran for you** — **skip this whole phase** and
> jump to [Polish the prose](#polish-the-prose). Run it yourself only for **manual**
> regeneration.

**Decide scope, then run it.** Ask the user which version(s) to generate (or infer from
context) and pick the matching invocation from the table below. From anywhere in the repo,
with no arguments, it does a full regeneration of everything:

```bash
.agents/skills/release-notes/scripts/generate.sh
```

By default it regenerates **both** artifacts for every branch and writes the
**"Files to polish"** list to `output/files-to-polish.txt` (also echoed to the log).
The flags narrow what it touches:

| Invocation | What it regenerates |
| --- | --- |
| `generate.sh` (no args) | **Both** — the API changelogs **and** the release-notes pages, for every branch. *(The default, and what the workflow uses.)* |
| `generate.sh --api-only` | Only the machine-generated **API-diff changelog** tree under `documentation/docfx/releases/`. |
| `generate.sh --notes-only` | Only the release-notes **pages'** raw-data blocks (+ `TOC.yml`/`index.md`). |
| `generate.sh <scope args>` | Same as default but limited to the given scope, e.g. `--branch main`, `--branch release/4.147.0-preview.1`, or a version like `3.119.2`. |

How it produces those files (which engine runs, the on-disk layout, diff ranges,
supersession, which pages are skipped because nothing changed) is **implementation detail
owned by the script** — you don't need it to run the skill. The only output you act on is
the **"Files to polish"** list the script writes to `output/files-to-polish.txt`: those
are the pages whose raw data changed and that you polish next. You never create, rename,
or delete pages — the script owns that.

**Requirements.** The script needs the .NET SDK and `python3`, plus network access to
nuget.org and the GitHub API. For **manual** runs, if a required tool or the network is
missing the script stops with a clear error — **ask the user to install it / restore
connectivity**; never work around it or hand-write API-diff links to compensate.

### Polish the prose

This is the only phase you (the AI) own. **The script structures everything; you only
polish prose.**

**The script owns everything structural and deterministic.** It decides — entirely on its
own — every filename (`{version}.md` vs `{version}-unreleased.md`), every diff range, which
branch produces which page, whether a page is released or unreleased, which previews roll
up, supersession banners, and which stale pages to delete. The exact rules live **in the
script** (module + function docstrings in `generate-release-notes.py`) and are intentionally
NOT restated here, so this skill can never drift from the code.

**Your job is narrow: rewrite the body of each file in the "Files to polish" list**, using
that file's embedded raw-data block, and nothing else. Do **not** create files, rename
files, compute diff ranges, or reason about released-vs-unreleased or rollup-vs-delta — if
you catch yourself doing any of that, stop: the script already did it. **Never edit the
script.** If a page you expect is missing (or an unexpected one exists, or any data looks
wrong), **stop and report it** — do not work around it and do not touch the script. A
maintainer decides whether the script needs fixing; the Polish phase only polishes.

The one structural fact you **consume** (never compute) while polishing: when a page rolls
up tagged previews, the script delivers the PRs already **grouped into per-preview buckets**
inside the raw-data block — each PR under the `## <label> (<date>) · <compare-url>` of the
preview it first shipped in, newest first, and the buckets together cover every PR in the
range. Render one trailing `## Preview N (date)` section per bucket per TEMPLATE.md
(verbatim label/date/compare-link), summarizing that bucket's PRs, and **merge all buckets**
for the top-level Highlights. A page with no previews (an unreleased delta or a plain stable
patch) instead carries a single flat PR list — summarize it as one set. Never re-bucket,
reorder, or recompute any of this; it is the script's output.

The file starts with an HTML comment block containing both metadata (version, status, branch,
diff range, PR count) AND the raw PR list. Below the comment is a skeleton heading with a
placeholder for polished content. The raw data comment must be preserved in the final file.

**IMPORTANT:** The list of files to polish is **always** at `output/files-to-polish.txt`
(one repo-relative path per line). For a **manual** run the Prepare script writes it there
(and echoes it to the log); for an **automated** run it has already been placed there for
you. The echoed summary looks like:

```
========================================
Files to polish:
  - documentation/docfx/releases/4.147.0.md
  - documentation/docfx/releases/3.119.5-unreleased.md
  - documentation/docfx/releases/4.148.0-unreleased.md
========================================
```

You MUST polish **every file** in the "Files to polish" list — not just the first one.
Read each file to get its raw data and metadata, then rewrite it with polished content.

#### Read the template

Read `documentation/docfx/releases/TEMPLATE.md`. This is a real example of a polished
release notes page. Match its structure, tone, and formatting exactly.

Determine the version's status from the HTML comment block in the file (`status: unreleased`, `status: preview`, or `status: stable`):
- **Stable**: header uses `Released {date}` + NuGet link + GitHub Release link
- **Preview**: header uses `Preview only` + preview NuGet link + GitHub Release link
- **Unreleased**: header uses `> **Upcoming release** · In development · Not yet available on NuGet`
- **Superseded** (a `superseded:` line is present in the comment block): the version was a
  preview that will never ship as stable. Keep the script-generated
  `> **Preview only** · Superseded by [X.Y.Z](...) · Never released as stable …` header and
  add a short note in Highlights that the work rolled up into the superseding version.
- **Successor** (a `supersedes:` line is present in the comment block): this version rolls
  up one or more skipped preview-only versions. Keep the script-generated
  `> **Supersedes [X.Y.Z](...)** · Rolls up preview-only work …` note and mention in
  Highlights that the skipped preview work is rolled up cumulatively. This is the back-link
  that makes the supersede relationship **two-way** (the superseded page points forward, the
  successor points back).

#### Skipped / superseded minors (preview-only versions)

Occasionally a minor ships previews but is **skipped** before going stable — e.g. `4.147`
was previewed but abandoned in favour of `4.148`. **The script handles all of this for you**
and records the outcome in the file's data-block; you only render what's there:

- The diff base is already chosen and baked into the `from..to` range, so a skipped line is
  rolled up automatically (e.g. `4.148`'s data already covers all the `4.147` work). You never
  pick a base yourself — just summarise the PRs in the file.
- A `superseded:` line means the version was a preview that never shipped stable. Render the
  script-generated *"Preview only · Superseded by …"* header (kept verbatim).
- A `supersedes:` line is the two-way back-link on the successor page. Render the
  script-generated *"Supersedes …"* note (kept verbatim).

> You never compute supersession or base selection — just render whatever markers the file
> contains. The mechanics (and the optional `scripts/infra/docs/versions.json` overrides) are script
> internals.

When polishing a superseded page, keep the script-generated *"Preview only · Superseded by …"*
header and add a one-line note in Highlights that the work rolled up into the successor.
When polishing a **successor** page, keep the script-generated *"Supersedes …"* note and add a
one-line note in Highlights that the skipped preview work is rolled up cumulatively.

#### Write the polished pages

For **every file** listed in the script's "Files to polish" output, write polished release
notes **below the raw data HTML comment block**. 

**CRITICAL: Preserve the raw data comment.** The `<!-- RAW PR DATA ... -->` comment block at
the top of the file must remain intact. Replace everything AFTER it (the skeleton heading and
placeholder) with polished content. When you write the polished content, start with the
`<!-- Generated: ... -->` timestamp line, then the `# Version X.Y.Z` heading, then the rest.

The final file structure should be:

```markdown
<!-- RAW PR DATA — Do not remove this comment block. ... -->

<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->

# Version X.Y.Z

> **theme** · status · links

## Highlights
...
```

Each file has its own HTML comment block with version, status, branch, and diff range.

**CRITICAL: Each file is independent.** Only use the raw PR data that is IN THAT FILE.
Do NOT combine data from other files. For example, if `3.119.4-unreleased.md` has 4 PRs
and `3.119.4.md` has 101 PRs, the unreleased file should only cover those 4 PRs — it is
NOT a rollup of the version file.

Follow these rules:

1. **Highlights** — 1-3 sentences. What's the story? Lead with the biggest changes.
   Mention community contributors by linked name.

2. **Skia engine** — If a "Bump skia" or "milestone" PR appears in the raw data,
   list it first under an **Engine** category.

3. **Categorize features** — Group by what they affect. Use sub-headers:
   Engine, GPU & Rendering, API Surface, Text & Fonts, Platform, Security, etc.
   Each item: **bold title** — description. ❤️ [@contributor](https://github.com/contributor) ([#NNN](url))

4. **Community contributors** — Anyone not @mattleibow. Mark with ❤️ inline AND
   in a Contributors table. **ALWAYS** link: `[@user](https://github.com/user)`.
   Never write bare `@user` anywhere in the file.

5. **Omit noise** — Skip version bumps, CI-only fixes, doc updates, workflow/skill changes.
   If many, mention as: "Plus several CI and documentation improvements."

6. **Breaking changes** — If any, list under `### ⚠️ Breaking Changes` after Highlights.

7. **PR links** — Every item links to its PR.

8. **Generation timestamp** — Always include `<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->`
   as the first line AFTER the raw data comment block (before the `# Version` heading).
   Use the current UTC time and your model name.

9. **Rollup at top** — Aggregate ALL changes across all previews into the main sections.

10. **Previews are minimal** — One sentence + changelog link each, at the bottom.
    Render one trailing `## <label> (<date>)` section per entry in the data-block's
    `preview milestones` list (newest first), using that entry's compare link. Do not invent
    previews or dates — the list is authoritative (sourced from published prerelease tags).

11. **Links section** — Full Changelog and NuGet Package only. **Do not add an API-diff
    or HarfBuzz link** — the script injects a `> **API changes**` line under the banner
    when a diff folder exists (gated on existence, so it never dangles). Keep that line
    verbatim and author no API links yourself (see TEMPLATE.md "SCRIPT-OWNED API LINKS").

## Parallelization

When regenerating multiple versions, process them in parallel — each version is independent.
Fetch all raw data in one script call, then launch one agent per version to write the polished page.

> This applies to **interactive/manual** use where sub-agents are available. When running
> unattended (e.g. the automated workflow), just polish the listed files **sequentially** in
> the one agent — do not try to spawn sub-agents.
