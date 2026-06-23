# GitHub Release Teaser

> **Scope:** this file is **only** for cleaning up the **GitHub Release** body. It is
> unrelated to the website release notes (the separate `release-notes` skill, which owns
> `documentation/docfx/releases/` + `TEMPLATE.md`). This teaser never reads those pages.

Build a short, scannable **teaser** for the GitHub Release that surfaces only what a
package consumer cares about, then fold GitHub's full auto-generated PR list beneath it.

## Principles

- **One input: the generated release log.** The teaser is built *only* from the
  `--generate-notes` body created in Step 6 of the publish skill. Do **not** read
  `documentation/docfx/releases/*`, do **not** run git commands, and do **not** wait on
  the website release-notes workflow or its `bot/release-notes` PR.
- **Keep the auto-gen list.** It already carries every PR number and author handle for
  free. We never hand-rebuild it — we just **fold it into a `<details>` block** under the
  teaser.
- **Essential-only, customer-first.** The teaser highlights features, breaking changes,
  and security fixes. Plumbing (CI, build, tests, dependency bumps, docs automation,
  backport bookkeeping, internal refactors) stays in the folded log, not the teaser.
- **Section emojis only.** Use ✨ / ⚠️ / 🛡️ on the section headers. There is **no**
  per-PR platform-emoji decoration (that older scheme is retired).
- **Link out for the full story.** The teaser links to the categorized website release
  notes; that page is produced separately (Step 5) and the link is a stable permalink
  that resolves once Pages publishes — we link it but never read or wait on it.

---

## Process

```bash
# 1. Capture the generated log produced by Step 6 (the ONLY input).
mkdir -p /tmp/skiasharp/release
gh release view {tag} --json body -q '.body' > /tmp/skiasharp/release/generated-log.md
```

2. **Extract** the teaser by running the [extraction prompt](#extraction-prompt) over
   `generated-log.md`. It returns just the teaser markdown (the part above the folded log).
3. **Assemble** the final body using the [teaser template](#teaser-template): the extracted
   teaser on top, then the captured log folded into `<details>`, then the `**Full
   Changelog**` line. Write it to `/tmp/skiasharp/release/release-body.md`.

```bash
# 4. Update the release in place.
gh release edit {tag} --notes-file /tmp/skiasharp/release/release-body.md
```

---

## Teaser template

Emit the final release body in **exactly** this shape. Sections with no qualifying items
are **omitted entirely** (no "*None.*" placeholders). The release **title** is already set
by `gh release create --title`, so the body has no top-level `#` heading.

```markdown
{one-line, plain-language subtitle of the release} · 📦 [NuGet](https://www.nuget.org/packages/SkiaSharp/{version}) · 📖 [Full release notes](https://mono.github.io/SkiaSharp/docs/releases/{version}.html)

## ✨ What's New
- {customer-facing improvement, ≤ ~15 words} (#{pr})
- {…}

## ⚠️ Breaking Changes
- {what changed and what a consumer must do} (#{pr})

## 🛡️ Security
- {vulnerability fixed; name the CVE if known} (#{pr})

Thanks to our contributors: @{handle}, @{handle}

---

<details><summary>All changes ({N} pull requests)</summary>

{the verbatim generated-log.md — What's Changed, New Contributors, Full Changelog}

</details>

**Full Changelog**: {compare-url}
```

Notes on the placeholders:

- `{version}` — the release version **without** the leading `v` (e.g. tag `v3.119.2` →
  `3.119.2`). For previews use the full preview version if its page exists; otherwise the
  link still resolves once that page publishes.
- `{N}` — count of `* … by @… in …` lines in the generated log.
- `{compare-url}` — copy the `**Full Changelog**: …/compare/…` URL from the generated log.
  It also remains inside the folded `<details>`; surfacing it once at the bottom is the
  quick link. Omit the outer line only if the generated log has none.
- Drop any section header whose list is empty. `Thanks to our contributors` may be omitted
  if the only authors are maintainers/bots.

---

## Extraction prompt

Feed the captured `generated-log.md` to this prompt (it produces only the teaser portion —
everything above the `---` / `<details>` in the template):

> You are writing a concise, **customer-facing** GitHub Release summary for **SkiaSharp
> {version}**. Your only input is the auto-generated release log below — one line per
> merged pull request, each with a title, `#PR`, and `@author`.
>
> **Goal:** surface only what a consumer of the SkiaSharp / HarfBuzzSharp NuGet packages
> would care about. **Ignore** CI/build/test changes, dependency-bump bookkeeping, version
> bumps, release-notes/docs automation, backport plumbing, and internal refactors — unless
> they visibly affect package consumers.
>
> **Classify** the remaining PRs into:
> - **✨ What's New** — features, new APIs, performance, behavior improvements, new
>   platform/runtime support.
> - **⚠️ Breaking Changes** — anything that could require consumer code or config changes:
>   removed/renamed/retyped public APIs, changed defaults, minimum-version bumps, dropped
>   target frameworks or runtimes.
> - **🛡️ Security** — CVEs and vulnerability fixes. Name the CVE if it appears in the title.
>
> Write **2–5 short bullets** per non-empty section, each **≤ ~15 words**, plain language,
> no internal jargon. Keep the `(#NNNN)` reference on each bullet. **Omit any section that
> has no qualifying PRs** — do not emit empty headers or "None".
>
> Then add one line: `Thanks to our contributors: ` followed by the **unique** `@author`
> handles from the log, excluding maintainers and bots (`@github-actions`, `@dependabot`,
> `@github-actions[bot]`). Omit the line if no community authors remain.
>
> Begin with a single plain-language subtitle line summarizing the release in one phrase.
>
> Output **only** the teaser markdown (subtitle line + the sections above). Do **not**
> invent changes that aren't in the log. Do **not** emit the `<details>` block, the
> horizontal rule, or the `**Full Changelog**` line — those are added afterward.
>
> Release log:
> ```
> {contents of generated-log.md}
> ```

### Classification quick-reference

| Bucket | Promote to teaser when the PR… | Examples of what to ignore |
|--------|--------------------------------|----------------------------|
| ✨ What's New | adds an API/feature, new platform/runtime, perf or behavior improvement | refactors, test-only, sample tweaks |
| ⚠️ Breaking Changes | removes/renames/retypes public API, changes a default, bumps a min version, drops a TFM/runtime | internal API changes, private helpers |
| 🛡️ Security | fixes a CVE or vulnerability (often a native-dep bump with a CVE) | routine dependency refreshes with no CVE |
| (folded only) | CI, build, pipeline, version bump, docs/notes automation, backport plumbing | — |

---

## Worked example

**Generated log (input):**

```
## What's Changed
* Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* Add riscv64 build support by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3201
* Bump libpng to 1.6.44 (CVE-2024-XXACK) by @mattleibow in https://github.com/mono/SkiaSharp/pull/3402
* Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
* Bump to the next version by @mattleibow in https://github.com/mono/SkiaSharp/pull/3410

## New Contributors
* @kasperk81 made their first contribution in https://github.com/mono/SkiaSharp/pull/3201

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2
```

**Assembled release body (output):**

```markdown
Adds tvOS Metal support and a new RISC-V build, plus a security fix. · 📦 [NuGet](https://www.nuget.org/packages/SkiaSharp/3.119.2) · 📖 [Full release notes](https://mono.github.io/SkiaSharp/docs/releases/3.119.2.html)

## ✨ What's New
- Support `SKMetalView` on tvOS (#3114)
- Add riscv64 build support (#3201)

## 🛡️ Security
- Bump libpng to 1.6.44, fixing CVE-2024-XXACK (#3402)

Thanks to our contributors: @MartinZikmund, @kasperk81

---

<details><summary>All changes (5 pull requests)</summary>

## What's Changed
* Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* Add riscv64 build support by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3201
* Bump libpng to 1.6.44 (CVE-2024-XXACK) by @mattleibow in https://github.com/mono/SkiaSharp/pull/3402
* Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
* Bump to the next version by @mattleibow in https://github.com/mono/SkiaSharp/pull/3410

## New Contributors
* @kasperk81 made their first contribution in https://github.com/mono/SkiaSharp/pull/3201

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2

</details>

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2
```

Note what dropped out of the teaser: the AI-docs PR and the version bump are plumbing, so
they live only in the folded log. There was no breaking change, so that section is omitted.
