---
name: release-notes
description: Write the polished prose for a SkiaSharp release-notes page. Use whenever the release-notes workflow asks you to fill in a version's notes, when you see a `data.json` for a release under `documentation/docfx/releases/_sources/`, or when a user asks to draft, polish, or regenerate release notes / a changelog for a SkiaSharp version. You produce ONE small JSON file of prose (`prose.json`); a deterministic renderer builds the page.
---

# Release notes — writing the prose

You are writing the human prose for one release-notes page. **You do not build the
page.** A script (`render-notes.py`) owns every heading, table, banner, `@handle`,
❤️, and PR link. Your entire job is to fill a small set of prose *slots*, and the
renderer assembles the page from those plus the facts in `data.json`.

This split exists on purpose: the parts that used to break — dropped headings,
bare handles, missing contributors, malformed links — are now impossible because
you never type them. Spend your effort on the one thing only a human-quality
writer can do: turn a raw activity log into a changelog a **NuGet consumer** wants
to read.

## The one test for everything you write

> Would a consumer notice this change without looking inside our repo?

If yes, write about it. If no (CI tweaks, internal refactors, doc/workflow
plumbing, test infra), leave it out — the renderer already collapses that noise.
`data.json` tags every PR `product` / `mixed` / `internal`; treat `internal` as
invisible unless it changed shipped behaviour, and for `mixed` judge from the
title.

## Running the full pipeline (prepare → write prose → render)

Producing release notes is three steps. Two are scripts you run; the middle one is
the writing this skill is about.

```
prepare.sh   →   (you write prose.json per page)   →   render.sh
 (network)              (this skill)                     (offline)
```

1. **`.agents/skills/release-notes/scripts/prepare.sh`** — regenerates the API diffs
   (Cake), the per-page `_sources/<version>.data.json` facts, and `_sources/index.json`,
   and writes the list of pages needing prose to `output/files-to-polish.txt`.
2. **You** read each listed page's `data.json` and write its `prose.json` (below).
3. **`.agents/skills/release-notes/scripts/render.sh`** — renders every page from
   `data.json` + `prose.json` and rebuilds `TOC.yml` + `index.md`. It fails loudly on
   invalid prose.

Both scripts take the **same three flags** — `--force`, `--min-version`, `--max-version`
— and nothing else. Choose them from what was asked:
- "regenerate the release notes **for 4.151.0**" → `--min-version 4.151.0 --max-version 4.151.0`
- "regenerate the release notes" (everything) → no flags
- after changing the **api-diff tools or the page format** → add `--force` (rebuilds even
  cached api diffs / unchanged pages)

Everything is incremental: an unforced run skips work whose output already exists (a
shipped version's api diff never changes), so a routine run is cheap — there is no
"notes-only" mode to reach for.

**Running locally** (needs `dotnet`, `python3`, `git`, `gh`):
```bash
# one version, end to end
.agents/skills/release-notes/scripts/prepare.sh --min-version 4.151.0 --max-version 4.151.0
#   … you write documentation/docfx/releases/_sources/4.151.0.prose.json …
.agents/skills/release-notes/scripts/render.sh  --min-version 4.151.0 --max-version 4.151.0

# everything
.agents/skills/release-notes/scripts/prepare.sh
.agents/skills/release-notes/scripts/render.sh
```

**In CI** a separate `prepare` job runs step 1, and you (the agent) do steps 2 and 3 —
write each page's prose, then run `render-notes.py --all` to finalize (the workflow's
tool allowlist permits `python3` for exactly this).

## How to work

You are given a list of pages to write (in CI, `output/files-to-polish.txt`; one
`documentation/docfx/releases/<version>.md` path per line). Every input for a
page lives in a `_sources/` folder beside it — for a page `releases/<version>.md`
the inputs are `releases/_sources/<version>.data.json`,
`releases/_sources/<version>.prose.json` (what you write), and an optional
`releases/_sources/<version>.notes.md`. HarfBuzzSharp is not a separate page — it
ships inside each SkiaSharp release, so it renders as a `## HarfBuzzSharp X.Y.Z`
section on the SkiaSharp page (see `harfbuzz_summary` below). For **each** page:

1. Read its `_sources/<version>.data.json`. It has:
   `prs` (title, author, community, tag), `previews` (each with its PR list),
   `contributors` (the authoritative roster), `breaking_candidates`, `tallies`,
   and the banner/link facts.
2. Read the breaking sources it points at, if present: the version's
   `*.breaking.md` API diff and any `_sources/<version>.notes.md` sidecar. These
   are your material for the `breaking` slot — the API diff gives signature
   removals, the notes sidecar gives *behavioural* breaks (same signature, new
   runtime behaviour) that no diff can detect.
3. Write `documentation/docfx/releases/_sources/<version>.prose.json`
   (schema: `.agents/skills/release-notes/scripts/schema/prose.schema.json`).
4. Render the page:
   `python3 .agents/skills/release-notes/scripts/render-notes.py _sources/<version>.data.json _sources/<version>.prose.json <version>.md`
   (use the full `documentation/docfx/releases/` paths). If it prints
   `PROSE VALIDATION FAILED`, read the errors, fix that slot, and re-run. A clean
   render — the `.md` written — is the bar.

You never hand-edit the `.md`. The per-page render above is just to validate your
prose as you go; **`render.sh` does the authoritative final pass** — it re-renders
every page and rebuilds `TOC.yml` + `index.md` from the committed JSON. Commit the
`_sources/<version>.prose.json` and the rendered `.md` together (the
`_sources/<version>.data.json` is already produced by the Prepare phase).

## The slots

Each slot below lists its purpose, the cap the renderer enforces, and one good +
one bad example. Caps are hard: the renderer rejects an over-long highlight, a
missing contributor, or an unknown category. Stay well under and you never see an
error. Where a slot is nullable or optional, the note says so — reach for `null`
rather than padding.

### `theme` — 2-6 words
What *this* release is about, shown bold in the banner. No punctuation.
- Good: `First stable v4 release`
- Bad: `Version 4.148.0` (that's the title, not a theme) · `Lots of fixes and new APIs` (vague)

### `highlights_headline` — one sentence, ≤20 words
The single most important thing about the release. **Not a list.** Decide it from
the `product`-tagged PRs, the Skia milestone bump, and whether there are breaking
changes — the one thing a consumer would care about most, in a sentence. You are
not summarising every PR here.
- Good: `SkiaSharp 4.148.0 is the first stable v4 release, built on Skia m148.`
- Bad: `This release adds WebP, SKStream.GetData, singleton lifecycle, pixel fixes, WinUI fixes, and more.` (enumeration)

### `highlights_body` — optional, ≤60 words, or `null`
Name the biggest themes to draw the reader in. Prose is best, but a short feature
list is fine for a big release — just keep the whole Highlights block (headline +
body) under ~100 words so it stays a lead-in, not the changelog. No PR links, no
`@handles`. If the headline already says enough, use `null`.
- Good: `It adds variable fonts and animated WebP, and reworks the singleton lifecycle. This is a breaking release — check the changes below before upgrading.`
- Bad: `Includes #4125, #3771, #3772, #4080, #4068 and fixes from @ramezgerges.` (links + handles, and it's just PR numbers, not themes)

### `breaking` — array, one entry per change a consumer must act on
Merge from two sources: signature removals in the `*.breaking.md` diff, and
behavioural breaks described in `breaking_candidates` / the notes sidecar. Empty
array is fine and renders "None in this release." Give each a `title`, a `body`
that says what changed **and what to do**, and the `prs` it came from. Only write
what you can substantiate: a `breaking_candidate` carries a `hint` and sometimes
`prs`, but when its companion file isn't on disk and it lists no concrete change,
fall back to the PR titles in `prs` you can actually read — never invent a removal
you can't point at.
- Good: `{"title": "SKPaint no longer exposes legacy text state", "body": "The paint text/font members obsoleted in v3 are now compile errors — move typeface and text size onto SKFont.", "prs": [4068, 4114]}`
- Bad: `{"title": "Refactoring", "body": "Various changes."}` (no action, not consumer-facing)

### `categories` — array of `{heading, bullets}`
The body of the page. **`heading` must be exactly one of these six** (the renderer
rejects anything else — this is the closed list, in the order they render):

| Heading | What belongs here |
|---|---|
| `Engine` | The Skia milestone bump and upstream engine syncs; bundled-engine changes a consumer would feel. |
| `API Surface` | New or changed public APIs — added types, methods, overloads, options. |
| `Bug Fixes` | Corrected behaviour, crashes, wrong output — even when platform-specific. |
| `Lifecycle & Internals` | Disposal, finalizers, initialization, singleton/handle lifecycle — consumer-visible runtime behaviour, not build plumbing. |
| `Platform` | Platform-**support** changes: a target added or dropped, new native assets, TFM realignment. |
| `Security` | Bundled native-dependency refreshes and security fixes. |

You choose which of the six to include — a section appears only when it has a real
product-facing bullet, and you may use as few as one. Prefer fewer, denser
sections over many one-bullet ones. **Curate, don't enumerate:** each bullet
MERGES related PRs into one product theme (aim 3-5 bullets per section on a big
release; 1-2 is perfectly fine on a servicing release — never merge distinct areas
just to hit a count), with a `lead` (bold summary) + `detail` (what it means for
the consumer) + the `prs`. The renderer adds the PR links and the ❤️ community
credit — never write those yourself. A change with a migration usually belongs in
`breaking`; don't also give it its own thin category section unless it has
independent product value. Placement rule of thumb: ordinary fixes go under **Bug
Fixes** even when platform-specific; use **Platform** only for platform-support
additions or removals.
- Good: `{"heading": "Bug Fixes", "bullets": [{"lead": "Pixel access corrected", "detail": "GetPixelSpan now uses RowBytes for stride and the right axis for offsets.", "prs": [4148, 4128]}]}` (two PRs → one theme)
- Bad: `{"heading": "Bugfixes", …}` (not one of the six) · one bullet per PR restating its title · a section that lists 20 internal PRs.

### `contributor_summaries` — one line per roster login
`data.contributors` is authoritative — every login there needs an entry (the
renderer fails otherwise) and no one else gets one. Summarise that person's work
in prose; the renderer adds their `@handle` and PR links. This is the one place
`internal` work is worth naming — a contributor's sample or CI work still deserves
credit even though it never became a category bullet.
- Good: `"ramezgerges": "Singleton lifecycle rework, the SKPath finalizer fix, and Uno sample updates"`
- Bad: `"ramezgerges": "#4080, #4068, #3796"` (that's data, not a summary)

### `preview_summaries` — one line per preview key
`data.previews` lists each preview/RC with the PRs that first shipped in it. Give
each `key` a 1-2 sentence summary of what that milestone delivered. When a preview
only carried internal work, describe the milestone itself (e.g. "opened the line"
or "cut the release candidate") rather than forcing a product story.
- Good: `"p2": "Preview 2 added animated WebP encoding and the SKPath finalizer fix."`
- Bad: leaving a preview key out (the renderer fails), or restating every PR.

### `harfbuzz_summary` — one short paragraph, or `null`
HarfBuzzSharp ships **inside** each SkiaSharp release, so its notes are a
`## HarfBuzzSharp X.Y.Z` section on this page, not a separate page. `data.harfbuzz`
gives the version and `prs` — the PRs in this release that touched the HarfBuzz
binding (a subset of the page's PRs, so you have already written about most of them
above). Summarise the HarfBuzz-facing story in 1-2 sentences; the renderer adds the
heading, the ❤️ credit and the PR links.
- Required only when `data.harfbuzz.prs` is non-empty. When it is empty the renderer
  writes "No HarfBuzzSharp binding changes shipped…" itself — set `harfbuzz_summary`
  to `null`. When `data.harfbuzz` is absent (e.g. an unreleased head), omit it.
- Good: `"Adds variable-font shaping and an HBColor value type, and refreshes the bundled HarfBuzz to 8.3.0."`
- Bad: re-listing every PR, or repeating the SkiaSharp highlights verbatim.

## Why this is short

There is no separate template, grouping guide, or checklist to reconcile — the
renderer is the checklist, and this file is the only instructions. If a rule
isn't here, it's because the renderer already guarantees it. Write the prose;
let the script build the page.
