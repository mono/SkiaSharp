<!-- Reference asset of the release-notes skill. Two uses: (1) the Polish agent applies this to
     each page before saving (self-review, fix, re-check); (2) run it standalone against any
     finished <version>.md — locally or as a fresh review pass — to score a page. Not part of the
     DocFX site. -->

# Release-notes reviewer

Review ONE polished `<version>.md`. Apply every check. A page **PASSES** only when all checks
pass; otherwise list the specific fixes and correct them.

To run it standalone, read the page, work the checklist top to bottom, and emit:

```
VERDICT: PASS | FAIL   (score N/12)
- <check-id>: PASS | FAIL — <detail / exact fix>
```

## The 12 checks

1. **banner-shape** — The blockquote under `# Version X.Y.Z` is one of exactly:
   - Stable: `> **<theme>** · Released <Month D, YYYY> · [NuGet](url) · [GitHub Release](url)`
   - Preview: `> **<theme>** · Preview only · [NuGet](preview-url) · [GitHub Release](url)`
   - Unreleased: `> **Upcoming release** · In development · Not yet available on NuGet`
   - HarfBuzz: `> **<theme>** · Ships with SkiaSharp <X> · [NuGet](hb-url) · [GitHub Release](url)`
   The script scaffolds the stable banner with the date + both links and a `<THEME>` token; the
   AI must replace `<THEME>` with a real 2-4 word phrase. FAIL if a literal `<THEME>` survives
   (`grep -c '<THEME>' <version>.md` must be 0), if it's a bare `> [NuGet](…)`, if `<theme>` is
   missing/generic-placeholder, or if a stable page has no `Released <date>` or no GitHub Release
   link.

2. **script-owned-lines-verbatim** — The `> **API changes** · …` line, any `> **Supersedes …**`
   / `> **… Superseded by …**` line, and the whole `<!-- RAW PR DATA … -->` comment are
   byte-identical to what the raw-data block declares. FAIL if any was re-authored.

3. **no-bare-handles** — Every `@name` in the rendered body is a `[@name](https://github.com/name)`
   link. FAIL on any bare `@name`, backticked `` `@name` ``, `by @name`, or `contributed by @name`.
   Grep: `grep -nE 'contributed by @|by @[A-Za-z0-9_-]+|(^|[^`[/])@[A-Za-z0-9_-]+' <version>.md | grep -v 'RAW PR DATA'`.

4. **no-bot-credit** — No `❤️`, Contributors row, or "by" is given to a bot: `github-actions[bot]`,
   `Copilot` (the coding agent — links to `github.com/apps/copilot-swe-agent`), `dependabot[bot]`,
   `dotnet-policy-service[bot]`, any `*[bot]`. Their PRs may be listed, but with no attribution
   (Prepare already strips the `[community ✨]` marker from bot lines, so a bot credit means Polish
   invented one). Grep: `grep -nE '@github-actions|@dependabot|@[Cc]opilot|apps/copilot|\[bot\]' <version>.md`.

5. **product-only** — No bullet describes an `[internal]` PR (CI/build, `.agents/**` skills,
   workflows, docs-site, PR-staging, milestone automation, tests, samples). The one collapse
   line *"Plus various CI, documentation, and internal tooling improvements."* is allowed once.
   Each `[mixed]` PR (build config only) is either surfaced as a genuine shipping change or
   folded into that collapse line — never left as a raw build/infra bullet.

6. **grouping-density** — Count top-level product bullets across `##` category sections (exclude
   previews and the Contributors table). Target **8–15**, and **at most ~4 per category section**
   (`## Breaking Changes` is exempt — one bullet per distinct break is fine). FAIL if any
   non-breaking category has > 5 bullets with obvious mergeable adjacencies (same subsystem,
   same dependency set, same overload shape). Grep: `grep -c '^- \*\*' <version>.md`.

7. **highlights-heading-and-length** — The literal heading `## Highlights` is present (FAIL if it
   was dropped or replaced by a bare unlabelled lead paragraph — `grep -c '^## Highlights'
   <version>.md` must be 1). Its body is **≤ 100 words** (target ~80) AND ≤ 3 short sentences: no
   bullet list, no PR/issue links, no per-item parentheticals, no contributor enumeration (at
   most one standout handle), no "Compared to X: A, B, C, …" comma-run. Count it:
   `awk '/^## Highlights/{f=1;next} /^## /{f=0} f' <version>.md | wc -w` must be ≤ 100. FAIL if a
   reader could reconstruct the category sections from Highlights alone.

8. **breaking-present** — `## Breaking Changes` heading exists (it is scaffolded into the
   skeleton — never delete it). When there is no `.breaking.md`/`.notes.md` companion the body is
   the scaffolded *"None in this release."* / *"None in this preview line."* line, kept verbatim.
   When a companion is listed, the section has a proportional handful of summarized bullets (never
   a dump, never empty) and no leftover `TODO(polish)`. FAIL if the heading was dropped or a
   `TODO(polish)` marker remains.

9. **contributor-table-complete** — The `## Community Contributors ❤️` table has **exactly one row
   per entry in the raw-data `contributors:` roster** — none missing, none invented. Cross-check:
   every `@login` in the roster appears as a row, and no row exists for a login absent from the
   roster (so no `mattleibow`, no bot). Column 1 `Contributor` is a plain `[@user](url)` with
   **no ❤️**; column 2 `What They Did` is a **single line** of prose summary followed by the PR
   links in one parenthetical. FAIL on a missing roster member, ❤️ in the cell, a cell that is
   only `[#N]` links, or a row for `mattleibow`/any bot.
   Grep the roster: `grep -A99 'contributors:' <version>.md | grep -oE '@[A-Za-z0-9-]+'`.

10. **inline-attribution-shape** — Every inline community credit is
    `❤️ [@user](url) ([#NNN](url))` with `❤️` immediately before the linked handle. FAIL on
    `contributed by @user`, `by @user`, a bare `@user`, `❤️ @user` (no link), `@user ❤️` (heart
    after the handle), or a maintainer/bot bullet carrying `❤️`.

11. **no-scaffold-markers** — No `TODO(polish)` and no `<THEME>` survive anywhere.
    Grep: `grep -nE 'TODO\(polish\)|<THEME>' <version>.md` must be empty.

12. **previews-verbatim** — Each trailing `## <label> (<date>)` matches a raw-data `preview
    milestones` entry, verbatim label and date; each is one sentence + a Full Changelog link. No
    invented previews or dates.

13. **links-minimal** — `## Links` on a released page has only `Full Changelog` + `NuGet Package`.
    No hand-authored API-diff or HarfBuzz link (those are script-owned in the banner).

## Fix, don't just flag

When self-reviewing during Polish: on any FAIL, correct the page and re-check. Cap at one
re-review; if something still fails, note it briefly in the PR body so a human sees it.
