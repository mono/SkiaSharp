---
name: release-notes
description: >
  Generate or regenerate SkiaSharp's website release notes AND API diffs as one
  coherent set under documentation/docfx/releases/. Two phases: (1) Prepare — one script
  (scripts/generate.sh) regenerates everything deterministic (the API-diff tree +
  co-release-map sidecar from the published NuGet feed, and each version page's raw-data
  block + page→API-diff links from git history); (2) Polish — the AI rewrites ONLY the
  prose of the pages the script flagged as changed.

  Use this skill whenever the user asks to:
  - Generate or regenerate release notes for a version ("write release notes for 3.119.2")
  - Regenerate or refresh the API diffs ("update the api diff", "regen the api diffs")
  - Refresh the whole releases/ set after publishing packages
  - Manually fix or update release notes that the automated workflow got wrong

  Triggers: "release notes for X", "regenerate release notes", "update API diffs",
  "api diff", "update website release notes", "write release notes",
  "refresh release notes". Also matches the legacy term "changelog" / "API changelog".

    NOTE: The full set is normally regenerated automatically by the `update-release-notes`
  agentic workflow when code lands on main, release branches, or tags are pushed. In that
  automated run the Prepare script has already been run for you, so you skip it and only do
  the Polish phase. Run manually for regeneration or corrections, in which case you run the
  Prepare script yourself first and then polish. Either way, the pages to polish are listed
  in `output/files-to-polish.txt`.
---

# Release Notes & API Diffs Skill

Generate SkiaSharp's website release notes **and** API diffs as one coherent set.

This skill is used both by the `update-release-notes` agentic workflow (automatically
on push to `main`, `release/*` branches, and tags) and manually when regenerating,
correcting, or bulk-processing the release pages.

## Purpose — what these release notes are for

These pages exist for **one audience: developers who consume the SkiaSharp and
HarfBuzzSharp NuGet packages.** They open a version page to answer, in order:

1. **Should I upgrade?** — Is something broken; is there a security or crash fix that hits me?
2. **What changes if I do?** — New or changed APIs, new behavior, new platforms, new
   native-dependency versions in the binary I ship.
3. **What breaks if I do?** — Signature, behavioral, or platform removals.
4. **Who do I thank?** — Community contributors.

They are **not** a repository activity log, a project changelog, or a contributor timeline.
A PR that only changes how *we* build, test, ship, or document the library is invisible from
the consumer's side — their compiled app is byte-identical whether or not we merged it. Those
PRs **do not appear in the notes**, however much work they were.

### The one test — apply it to every PR

> **Would a developer who just consumes the NuGet package notice this change without looking
> at our repository?**

- **Yes → include it**, in the right category. (New/changed API, behavior change, bug fix,
  native-dependency bump, new RID/TFM, a native build flag that ships in the binary.)
- **No → drop it** — or, when there is a lot of it, fold it into a single trailing
  *"Plus various CI, documentation, and internal tooling improvements."* line (never a bullet
  per change). (CI/build pipelines and caching, GitHub Actions/workflows, our own `.agents/**`
  skills of any name — including `security-audit`, `ci-status`, the release-notes/docs tooling
  — the docs website, PR-staging, sample-publishing, milestone/label automation, test-infra,
  the package's own version bump.)

**You do not have to make this call from scratch.** The Prepare script pre-tags every
raw-data PR line **`[product]`**, **`[mixed]`**, or **`[internal]`** by the files the PR
changed:

- **`[product]`** — touches shipped code (`binding/`, `externals/`, `source/`). A real change
  a consumer can see. **Write it up.**
- **`[internal]`** — touches none of the above (CI, workflows, our `.agents/**` skills, the docs
  site, tests, samples, build/meta files). **Drop it** into the single collapse line.
- **`[mixed]`** — touches only build config (`native/`): it *might* change the shipped binary via
  a compile flag (e.g. a rasteriser define), or it *might* be pure infra (a Docker image or SDK
  pin). **Take a best guess from the title/context in the block — no need to open the PR.**
  Surface it as a bullet only when it plausibly changes what ships (a rendering/behaviour fix,
  a crash fix, a new platform); otherwise fold it into the collapse line.

**Trust the tag.** The one test above is only the tie-breaker for the rare `[product]`/`[internal]`
line the tag clearly got wrong.

**The PR title is not evidence — test what shipped, not what it is called.** *"Chrome Releases
blog integration in security-audit skill"* is an internal skill change even though it says
"security"; *"ci-status daily CI health dashboard"* is internal even though it says "dashboard";
*"Publish-samples workflow"* is a workflow change even though "samples" sounds product-shaped.

This is the single most important thing the polish gets right or wrong: a page with a perfect
contributors table and thirty internal-tooling bullets is **worse** than one with a small
formatting slip and zero leaked bullets. The first buries the signal a consumer came for.

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
| `generate.sh` (no args) | **Both** — the API diffs **and** the release-notes pages, for every branch. *(The default, and what the workflow uses.)* |
| `generate.sh --api-only` | Only the machine-generated **API-diff** tree under `documentation/docfx/releases/`. |
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
that file's raw-data block **plus the companion files that block references** (see
[Companion files](#companion-files-open-and-read-them) below), and nothing else. Do **not**
create files, rename files, compute diff ranges, or reason about released-vs-unreleased or
rollup-vs-delta — if you catch yourself doing any of that, stop: the script already did it.
**Never edit the script, and never edit a companion file.** If a page you expect is missing
(or an unexpected one exists, or any data looks wrong), **stop and report it** — do not work
around it and do not touch the script. A maintainer decides whether the script needs fixing;
the Polish phase only polishes.

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

#### Companion files (open and read them)

The raw-data block is still your **primary** input, and it remains the reason you never touch
git or the gh API — it carries the PR titles, authors, and numbers. On top of that, the block
may list a **companions** manifest: extra files, already on disk, that you should **open and
read** while polishing, then **summarize**. There are up to three, all referenced by
page-relative path (see `release-notes-and-api-diffs.md` §4.7):

| Companion | What it is | Use it for |
| --- | --- | --- |
| `<stem>.notes.md` | **Manual additions sidecar** — freeform Markdown the maintainer wrote to bring something out (not always breaking). | Weave its editorial points into Highlights; surface any behavioral breaking notes under Breaking Changes. |
| `<line>/index.md` (indexes every per-assembly diff; flags breaking) | The **full public-API diff** landing page — one door to the whole folder. | Open it, follow the assemblies that matter, draw richer/accurate highlights. Do **not** paste it — summarize. |
| `<line>/…/*.breaking.md` (**one per broken assembly**) | The **API breaking diff** — present only where signatures actually broke; a big release lists many. | The "what" of Breaking Changes. **Open every listed file** and **summarize the changes as a few bullets** — name the affected types/areas, link the API diff for the full list. |

Rules for companions:

- **Open and read** each one the manifest lists, then **summarize** — never paste a diff
  verbatim or dump the whole file. A short human summary ("obsoleted several APIs in `SKPaint`
  and `SKFont`"; "`SKFooBar` was removed — use `SKBaz`") with a small migration example where
  it helps is the goal, for a bulk sweep as much as a curated set.
- **Merge `.notes.md` neatly** — editorial "bring this out" notes into Highlights; behavioral
  and interop breaks under Breaking Changes. It is the **only** channel for breaks the signature
  diff can't see: behavioral changes (same signature, different runtime behavior, e.g.
  `new SKFont()` carrying an empty typeface) and interop / native structs (e.g. removed
  `GRVkBackendContextNative` fields).
- **Summarize `.breaking.md` into a few bullets** under `## Breaking Changes`; never drop the
  section. Readers follow the API-diff link for the exhaustive list.
- You may **read** these files but **never edit them** — they are inputs/artifacts. Still no
  git, no gh API, no other files. If the manifest lists a companion but the file is missing (or
  vice-versa), **stop and report it** — do not work around it.

**IMPORTANT:** The list of files to polish is **always** at `output/files-to-polish.txt`
(one repo-relative path per line, nothing else). For a **manual** run the Prepare script
writes it there (and echoes the same paths to the log); for an **automated** run it has
already been placed there for you. The file is a plain list, e.g.:

```
documentation/docfx/releases/4.147.0.md
documentation/docfx/releases/3.119.5-unreleased.md
documentation/docfx/releases/4.148.0-unreleased.md
```

You MUST polish **every file** in the "Files to polish" list — not just the first one.
Read each file to get its raw data and metadata, then rewrite it with polished content.

#### Read the template

Read `.agents/skills/release-notes/references/TEMPLATE.md`. This is a representative
(fictional) example of a polished release notes page — its version, PR numbers, handles,
and API names are placeholders. Match its structure, tone, and formatting exactly, but
never copy its placeholder content; write from the page's real raw-data.

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

The final file structure should be (omit any section with nothing to report — never force
an empty category):

```markdown
<!-- RAW PR DATA — Do not remove this comment block. ... -->

<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->

# Version X.Y.Z

> **<theme>** · Released <Month D, YYYY> · [NuGet](url) · [GitHub Release](url)    ← rule 7

## Highlights            (ALWAYS present, literal heading; ~80 words, ≤100 — rule 2)
## Breaking Changes      (always present; "None in this release." when none — rule 3)
## <Category>            (one or more: Engine, GPU & Rendering, API Surface, Text & Fonts,
                          Bug Fixes, Lifecycle & Internals, Platform, Security — rules 4-5;
                          [product] items + surfaced [mixed] items — rule 1)
## Community Contributors ❤️   (linked table — one row per `contributors:` roster entry — rule 6)
## Links                 (Full Changelog + NuGet only — rule 11)
## <Preview label> (date)      (one trailing section per preview bucket, newest first — rule 10)
```

Each file has its own HTML comment block with version, status, branch, and diff range.

**CRITICAL: Each file is independent.** Only use the raw PR data that is IN THAT FILE.
Do NOT combine data from other files. For example, if `3.119.4-unreleased.md` has 4 PRs
and `3.119.4.md` has 101 PRs, the unreleased file should only cover those 4 PRs — it is
NOT a rollup of the version file.

Follow these rules, **in priority order — earlier rules trump later ones.** If a later rule
would tempt you to include an internal PR, rule 1 wins.

> **References (read the two that carry the detail):**
> [`references/grouping.md`](references/grouping.md) — how to merge related PRs into a few
> thematic bullets (rule 5); [`references/review-checklist.md`](references/review-checklist.md)
> — the 12-point self-review you run before saving; [`references/TEMPLATE.md`](references/TEMPLATE.md)
> — the canonical page shape (banner, sections, worked example).

1. **Focus on the product, not the project — drop internal work.** This is the rule that makes
   the page useful (see the **Purpose** section above). Every raw-data
   PR line is tagged `[product]`, `[mixed]`, or `[internal]` by the script:
   - **`[internal]` → do not write a bullet for it.** Roll ALL internal PRs into a single
     trailing line — *"Plus various CI, documentation, and internal tooling improvements."* — or
     omit even that when there were none. Never one bullet per internal change.
   - **`[product]` → keep it**, categorized below.
   - **`[mixed]` (build config only) → guess from the title/context in the block** (no need to
     open the PR). Give it a bullet only when it plausibly changes what ships — a rendering or
     behaviour fix, a crash fix, a new platform/RID; otherwise fold it into the collapse line
     with the internal work. When unsure, prefer the collapse line.
   Apply the *one test* (see Purpose) only as a tie-breaker for a line the tag clearly got wrong.
   **Dev-cycle and `-unreleased` pages are often mostly `[internal]`** — that is expected; such a
   page may legitimately be just Highlights + the single collapse line.

2. **Highlights — a hook, not a summary (MANDATORY heading, target ~80 words, HARD CAP 100).**
   The section is **always present**: write the literal heading **`## Highlights`** right after the
   banner / `> **API changes**` block, then two or three **short** sentences — **aim for ~80
   words, never exceed 100** (count them). Never drop the heading and never replace it with a
   bare unlabelled lead paragraph. Name only the **3-4 biggest** things (the engine jump, the one
   headline feature, the fact that there are breaking changes) in plain prose. This is the single
   most-violated rule, so the bans are explicit — in Highlights, **do NOT**:
   - enumerate APIs, dependency bumps, or fixes (the categories below carry the full list);
   - list contributors one by one (the Community Contributors table does that) — at most name the
     *one* standout by linked handle;
   - use per-item parentheticals, PR/issue links, or `code` API names as a checklist;
   - write "Compared to X, v4 delivers: A, B, C, D, E, F …" — that comma-run **is** the
     enumeration this rule forbids.
   If a reader could reconstruct the category sections from your Highlights, it is too long —
   cut it back to the hook. The bigger the release, the more **selective** Highlights get, never
   longer.

   > **Too long (what not to do):** *"4.148.0 is the shipped v4 stable… Compared to 3.119.x, v4
   > delivers: variable font support (`SKFontArguments`), color font palettes, animated WebP
   > encoding (`SKWebpEncoder`), `SKStream.GetData()` zero-copy, `SKSamplingOptions` overloads…
   > Behavioral hardening: default typeface resolution moves… `new SKFont()`… Standout community
   > work: @ramezgerges (16 PRs — …), @4Darmygeometry (…), @SimonvBez (…), @ebariche (…)…"*
   > (228 words — the whole page crammed into the hook).
   >
   > **Right (keep the heading):**
   > ```markdown
   > ## Highlights
   >
   > The first stable v4 release upgrades the engine to Skia m148 and lands the big v4 feature
   > wave — variable fonts, color font palettes, and animated WebP — alongside a set of
   > behavioural breaking changes to review before upgrading. Community work from
   > [@ramezgerges](https://github.com/ramezgerges) anchored the release.
   > ```
   > (~50 words, under the heading.)

3. **Breaking changes** — Always include a `## Breaking Changes` section (*"None in this
   release."* when there are none — never drop the heading). If the raw block lists a
   `.breaking.md` and/or `.notes.md` companion, open them and **summarize the breaks as a few
   bullets**: name the affected types/areas, add a migration snippet where it helps, and link
   the API diff for the full list. Behavioral & interop breaks come only from `.notes.md`.
   Summarize, don't dump. (See [Companion files](#companion-files-open-and-read-them).)

4. **Skia engine first** — If a "Bump skia" or "milestone" PR appears in the raw data,
   list it first under an **Engine** category.

5. **Group and categorize — curate, don't enumerate.** Merge related `[product]` PRs into a few
   **thematic bullets** (target **8–15 total across the whole page, at most ~4 per category
   section** — count them; `## Breaking Changes` is the only exception and may list one per
   distinct break). **Never one bullet per PR.** Read
   **[`references/grouping.md`](references/grouping.md)** for the countable cap, the clustering
   axes, and a worked example. Group under top-level `##` categories (Engine & native
   dependencies, New APIs, GPU & Rendering, Text & Fonts, Views & platforms, Fixes, Lifecycle &
   Internals, Security). Each grouped bullet:
   `**impact-statement title** — one line on why it matters. ❤️ [@a](url), [@b](url) ([#N](url), [#M](url))`
   - The PR link(s) come **last**, in one parenthetical.
   - Community contributors are credited with `❤️ [@user](url)` before the PR links. The
     maintainer (@mattleibow) and **all bot accounts** (`github-actions[bot]`, `copilot[bot]`,
     `dependabot[bot]`, any `*[bot]`) are **never** credited — their PRs may be listed but carry
     no `❤️` and no "by".
   - **Never** a bare, backticked, or `by @handle` — every handle is a `[@user](https://github.com/user)`
     link, never the sentence's subject (the raw-data `by @user` is source data, not output).

6. **Community Contributors table — render the roster, completely.** The raw-data block ends
   with an authoritative **`contributors:`** roster: every external (non-maintainer, non-bot)
   author to credit, with their PR numbers. **Render one table row per roster entry — never omit
   one, never invent one.** (Reconstructing this list from the body prose kept silently dropping
   real contributors whose PRs were folded into thematic bullets — the roster is the fix.) Emit
   the table whenever the roster is non-empty:
   `## Community Contributors ❤️`, **one row per contributor, one line each**:
   `| [@user](https://github.com/user) | <short prose summary of their work> ([#N](url), [#M](url)) |`
   - Column 1 `Contributor` is a **plain `[@user](url)` — no ❤️** (the heart wraps badly here; it
     lives only on the inline bullets).
   - Column 2 `What They Did` is a **single line**: a prose summary (Feature A, bug B, thing C)
     built from that contributor's PR titles in the block, then their PR links in **one**
     parenthetical. **Not** a bare list of `[#N]` links, and **not** `[#N] — note` fragments.
   - A contributor appears **even if their work was internal/samples-only** — the table thanks
     everyone whose PRs shipped, so summarize whatever they did.
   - **Never** a row for @mattleibow or any bot (the roster already excludes them).

7. **Banner — themed, dated, linked.** The blockquote directly under `# Version X.Y.Z` is
   **never** a bare `> [NuGet](…)`. Write exactly one of these shapes:
   - **Stable:** `> **<theme>** · Released <date> · [NuGet](https://www.nuget.org/packages/SkiaSharp/<v>) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v<v>)`
     — `<date>` is the raw-data block's `released:` field **verbatim** (e.g. *December 2, 2024*);
     `<theme>` is a 2–4 word editorial phrase you write from the Highlights (e.g. *First stable
     3.x release*, *Engine upgrade & API cleanup*).
   - **Preview-only:** `> **<theme>** · Preview only · [NuGet](<preview-url>) · [GitHub Release](url)`
   - **Unreleased:** `> **Upcoming release** · In development · Not yet available on NuGet`
   - **HarfBuzz:** `> **<theme>** · Ships with SkiaSharp <X> · [NuGet](<hb-url>) · [GitHub Release](url)`
   If the existing page already has a bare banner, **replace it** — do not preserve it. The
   script-owned `> **Supersedes …**` and `> **API changes** …` lines stay verbatim below it.

8. **PR links** — Every item links to its PR.

9. **Rollup at top** — Aggregate ALL `[product]` changes across all previews into the main
   sections.

10. **Previews are minimal** — One sentence + Full Changelog link each, at the bottom.
    Render one trailing `## <label> (<date>)` section per entry in the data-block's
    `preview milestones` list (newest first), using that entry's compare link. Do not invent
    previews or dates — the list is authoritative (sourced from published prerelease tags).

11. **Links section** — Full Changelog and NuGet Package only. **Do not add an API-diff
    or HarfBuzz link** — the script injects a `> **API changes**` line under the banner
    when a diff folder exists (gated on existence, so it never dangles). Keep that line
    verbatim and author no API links yourself (see TEMPLATE.md "SCRIPT-OWNED API LINKS").

12. **Generation timestamp** — Always include `<!-- Generated: YYYY-MM-DDTHH:MM:SSZ by {model-name} -->`
    as the first line AFTER the raw data comment block (before the `# Version` heading).
    Use the current UTC time and your model name.

## Before you save each page — self-review

Apply **[`references/review-checklist.md`](references/review-checklist.md)** to the page you just
wrote — it is the 12-point rubric (also runnable standalone to grade any finished page). Fix
every FAIL and re-check once. The checks that go wrong most often, watch these first:

1. **Banner shape** — `> **<theme>** · Released <Month D, YYYY> · [NuGet](url) · [GitHub Release](url)`
   on a stable page (never a bare `> [NuGet](…)`); `Preview only` / `In development` variants per
   TEMPLATE.md. The release date comes from the raw-data `released:` line.
2. **Curated, not enumerated** — 8–15 grouped product bullets total, **at most ~4 per category
   section** (Breaking Changes exempt); not one per PR
   (see [`references/grouping.md`](references/grouping.md)).
3. **Product test** — no `[internal]` PR gets a bullet; they collapse into the single
   *"Plus various CI, documentation, and internal tooling improvements."* line. Each `[mixed]` PR
   is either surfaced as a shipping change or folded into that same line — never left as a raw
   build/infra bullet.
4. **No bare `@handle`, no bot credits** — every handle is `[@user](url)`; no `❤️`/row/"by" for
   any `*[bot]`.
5. **Contributors table complete** — **one row per `contributors:` roster entry**, none missing,
   none invented; each row is plain `[@user](url)` · prose summary + PR links, no ❤️ in the cell.
6. **`## Highlights` heading present** (never a bare lead paragraph) and **≤ 100 words** (target
   ~80 — count with `awk`/`wc -w`), ≤ 3 short sentences, no enumeration; **`## Breaking Changes`
   present**; raw-data comment intact.

## Parallelization

When regenerating multiple versions, process them in parallel — each version is independent.
Fetch all raw data in one script call, then launch one agent per version to write the polished page.

> This applies to **interactive/manual** use where sub-agents are available. When running
> unattended (e.g. the automated workflow), just polish the listed files **sequentially** in
> the one agent — do not try to spawn sub-agents.
