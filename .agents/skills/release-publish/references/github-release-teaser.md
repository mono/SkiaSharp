# GitHub Release Teaser

> **Scope:** this file is **only** for cleaning up the **GitHub Release** body. It is
> unrelated to the website release notes (the separate `release-notes` skill, which owns
> `documentation/docfx/releases/` + `TEMPLATE.md`). This teaser never reads those pages.

Build a short, scannable **teaser** for the GitHub Release that surfaces only what a
package consumer cares about, then fold GitHub's full auto-generated PR list beneath it.

## Principles

- **One input: the generated release log.** The teaser is built *only* from the
  generated-notes draft body downloaded by `create-release-draft.py`. Do **not** read
  `documentation/docfx/releases/*`, do **not** run git commands, and do **not** wait on
  the website release-notes workflow or its `bot/release-notes` PR.
- **Keep the auto-gen list.** It already carries every PR number and author handle for
  free. We never hand-rebuild it — we just **fold it into a `<details>` block** under the
  teaser.
- **Credit every promoted change inline.** Each teaser bullet keeps its PR author as
  `by @author (#NNNN)`. Use **one PR per bullet** so every contributor is named; you may
  combine PRs into a single bullet **only for one closely-related change**, and then
  **credit every author** (`by @a, @b (#NNNN, #MMMM)`). Include the handle even for
  maintainers — inline credit is factual attribution. The `Thanks to our contributors`
  line is a separate community-only roll-up and stays.
- **Essential-only, customer-first.** The teaser highlights features, consumer-visible bug
  fixes, breaking changes, and notable native-dependency updates. Plumbing (CI, build, tests,
  build-tooling bumps, docs automation, backport bookkeeping, internal refactors) stays in the
  folded log.
- **Don't fold consumer bug fixes.** A `Fix…`/`Fixed…` PR that touches a public
  SkiaSharp/HarfBuzzSharp type or a user-facing scenario (wrong rendering, a crash, wrong
  output) is exactly what a consumer wants to see — it goes under 🐛 Fixes, **not** the folded
  log. Only fixes to CI/build/tests/docs/skills/samples (and vague `[skia-sync]` upstream
  merges) stay folded.
- **The Skia engine milestone is a headline feature.** A `Update skia to milestone N` /
  `[skia-sync]` milestone bump is the marquee ✨ What's New item ("Updated Skia engine to
  milestone N"), **not** a 📦 dependency bump. Routine `[skia-sync] Merge upstream … bug
  fixes` PRs are engine-sync plumbing already represented by that milestone bullet — keep
  them folded.
- **Read every PR.** Scan the whole log, including older PRs near the top of a long list —
  age doesn't turn a feature or a consumer-facing fix into plumbing.
- **Never advertise security.** Do **not** call anything a "security fix", title a section
  "Security", or name a CVE in the teaser — we deliberately avoid signaling which bundled
  component was vulnerable so attackers get no roadmap. Surface bundled-dependency bumps
  neutrally as `Updated <dep> to <version>`; the upstream PR title stays verbatim only
  inside the folded log.
- **Section emojis only.** Use ⚠️ / ✨ / 🐛 / 📦 on the section headers. There is **no**
  per-PR platform-emoji decoration (that older scheme is retired).
- **Link out for the full story.** The script links to the categorized website
  release notes using the stable base-version permalink. The teaser never reads
  or waits on that page.

---

## Process

`create-release-draft.py` execution writes:

| File | Owner |
|------|-------|
| `output/release/{tag}/generated-log.md` | Script; never edit. This is the only classification input. |
| `output/release/{tag}/teaser.md` | Agent; replace the subtitle placeholder and add selected sections. |

1. **Read every entry** in `generated-log.md`.
2. **Edit `teaser.md`** using the teaser-input template below. Drop plumbing,
   classify customer-visible changes into ⚠️ / ✨ / 🐛 / 📦, and credit each
   promoted change.
3. Preserve exactly one `<!-- RELEASE_LINKS -->` marker. Do not add NuGet,
   website-notes, or changelog URLs yourself.
4. Run `publish-release.py --teaser-file ... --dry-run`. The script inserts
   the exact links, strips duplicate generated headings, counts PRs, folds the
   generated log, and reports the expected final-body SHA.
5. After final approval, `publish-release.py` uploads `release-body.md` and
   publishes the GitHub draft.

---

## Teaser-input template

Edit `teaser.md` into exactly this shape. Sections with no qualifying items are
omitted entirely (no "*None.*" placeholders). The script owns everything below
the teaser, including links and the folded log.

```markdown
{one-line, plain-language subtitle of the release}

<!-- RELEASE_LINKS -->

## ⚠️ Breaking Changes
- {what changed and what a consumer must do} by @{author} (#{pr})

## ✨ What's New
- {new feature, API, platform/runtime, perf win, or Skia milestone bump, ≤ ~15 words} by @{author} (#{pr})
- {…}

## 🐛 Fixes
- {consumer-visible bug fixed — wrong rendering, crash, wrong output, ≤ ~15 words} by @{author} (#{pr})

## 📦 Dependency Updates
- Updated {dependency} to {version} by @{author} (#{pr})

Thanks to our contributors: @{handle}, @{handle}
```

The script derives and validates:

- NuGet version from the exact public SkiaSharp package.
- Website page from the numeric release version.
- Compare URL from GitHub's generated `**Full Changelog**:` line.
- Pull-request count from the generated `What's Changed` list only; New
  Contributors entries are not double-counted.
- Folded content from the generated log, preserving its raw PR bullets and New
  Contributors block.

Notes on teaser content:

- Every teaser bullet ends with `by @{author} (#{pr})`. One PR per bullet so each author is
  credited; combine only closely-related PRs and credit every author as
  `by @a, @b (#{pr1}, #{pr2})`.
- Drop any section header whose list is empty. `Thanks to our contributors` may be omitted
  if the only authors are maintainers/bots.

---

## Classifying the PRs

Work only from the captured `generated-log.md` — one line per merged PR (title, `#PR`,
`@author`). Surface only what a consumer of the SkiaSharp / HarfBuzzSharp NuGet packages
would care about. **Ignore** (leave folded) CI/build/test changes, build-tooling dependency
bumps (GitHub Actions, internal package references), the SkiaSharp version-number bump,
release-notes/docs automation, backport plumbing, and internal refactors — unless they
visibly affect package consumers. Review **every** PR, including older ones near the top of a
long list — age doesn't turn a feature or a consumer-facing fix into plumbing.

Classify the remaining PRs into these sections, emitted **in this order** (breaking changes
first — that's what consumers most need to see); omit any section with no qualifying PRs:

- **⚠️ Breaking Changes** — anything that could require consumer code or config changes:
  removed/renamed/retyped public APIs, newly `[Obsolete]`/deprecated public APIs or obsolete
  members promoted to warnings or errors (consumers see new build diagnostics), changed
  defaults, minimum-version bumps, dropped target frameworks or runtimes.
- **✨ What's New** — new features, new APIs, performance wins, new platform/runtime support,
  and the **Skia engine milestone bump** (`Update skia to milestone N` → "Updated Skia engine
  to milestone N"). The milestone bump is a headline here, **not** a dependency update.
- **🐛 Fixes** — **consumer-visible bug fixes**: wrong rendering, a crash, wrong
  output/values, or wrong behavior involving a public SkiaSharp/HarfBuzzSharp type or a
  concrete user-facing scenario. A `Fix…`/`Fixed…` PR **still qualifies** if a package
  consumer could have hit the bug — don't fold it just because it's a fix. **Keep folding**
  fixes to CI/build/tests/docs/skills/samples and vague engine-sync fixes
  (`[skia-sync] Merge upstream … bug fixes`, covered by the milestone bullet).
- **📦 Dependency Updates** — bumps of bundled **native** graphics-stack libraries (libpng,
  libexpat, freetype, harfbuzz, zlib, libjpeg-turbo, brotli, …), written as `Updated <dep> to
  <version>`. The Skia engine milestone is **not** here (it's ✨ What's New). **Never** write
  "security"/"vulnerability" or name a CVE, even when the PR title does.

Write **2–5 bullets** per non-empty section, each **≤ ~15 words**, plain language, no internal
jargon, ending with **`by @author (#NNNN)`** (one PR per bullet; combine only one closely-
related change, crediting every author). Open the body with a single neutral subtitle line
summarizing the release; never frame it as a "security" release. Close the teaser with
`Thanks to our contributors: ` + the **unique** community `@author` handles (exclude
maintainers and bots like `@github-actions[bot]`, `@dependabot`); omit the line if none
remain.

### Classification quick-reference

| Bucket | Promote to teaser when the PR… | Examples of what to ignore |
|--------|--------------------------------|----------------------------|
| ⚠️ Breaking Changes | removes/renames/retypes public API, **obsoletes/deprecates a public API**, changes a default, bumps a min version, drops a TFM/runtime | internal API changes, private helpers |
| ✨ What's New | adds an API/feature, new platform/runtime, perf win, or the Skia engine milestone bump | refactors, test-only, sample tweaks |
| 🐛 Fixes | fixes a consumer-visible bug (wrong rendering, crash, wrong output/behavior) on a public type or user-facing scenario | `[skia-sync]` upstream-merge fixes, CI/build/test/docs/sample fixes |
| 📦 Dependency Updates | bumps a bundled native library (libpng, libexpat, freetype, harfbuzz, zlib, libjpeg-turbo, brotli, …), stated neutrally as a version update — never as a CVE/security fix; the Skia engine milestone goes under ✨ | CI/build-tooling bumps, GitHub Action bumps, internal package refs |
| (folded only) | CI, build, pipeline, version bump, docs/notes automation, backport plumbing | — |

---

## Worked example

**Generated log (input):**

```
## What's Changed
* Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* Add riscv64 build support by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3201
* Fix SKBitmap.Encode returning empty data for WBMP images by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3401
* Bump libpng to 1.6.44 (CVE-2024-XXACK) by @mattleibow in https://github.com/mono/SkiaSharp/pull/3402
* Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
* Bump to the next version by @mattleibow in https://github.com/mono/SkiaSharp/pull/3410

## New Contributors
* @kasperk81 made their first contribution in https://github.com/mono/SkiaSharp/pull/3201

**Full Changelog**: https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2
```

**Script-assembled release body (output):**

```markdown
Adds tvOS Metal support and a RISC-V build, and fixes WBMP encoding.

📦 [NuGet](https://www.nuget.org/packages/SkiaSharp/3.119.2) · 📖 [Release notes](https://mono.github.io/SkiaSharp/docs/releases/3.119.2.html) · 🔀 [Full changelog](https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2)

## ✨ What's New
- Support `SKMetalView` on tvOS by @MartinZikmund (#3114)
- Add riscv64 build support by @kasperk81 (#3201)

## 🐛 Fixes
- Fix `SKBitmap.Encode` returning empty data for WBMP images by @kasperk81 (#3401)

## 📦 Dependency Updates
- Updated libpng to 1.6.44 by @mattleibow (#3402)

Thanks to our contributors: @MartinZikmund, @kasperk81

---

<details><summary>All changes (6 pull requests)</summary>

* Support SKMetalView on tvOS by @MartinZikmund in https://github.com/mono/SkiaSharp/pull/3114
* Add riscv64 build support by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3201
* Fix SKBitmap.Encode returning empty data for WBMP images by @kasperk81 in https://github.com/mono/SkiaSharp/pull/3401
* Bump libpng to 1.6.44 (CVE-2024-XXACK) by @mattleibow in https://github.com/mono/SkiaSharp/pull/3402
* Adding the initial set of AI docs by @mattleibow in https://github.com/mono/SkiaSharp/pull/3406
* Bump to the next version by @mattleibow in https://github.com/mono/SkiaSharp/pull/3410

## New Contributors
* @kasperk81 made their first contribution in https://github.com/mono/SkiaSharp/pull/3201

</details>
```

Note what dropped out of the teaser: the AI-docs PR and the version bump are plumbing, so
they live only in the folded log. There was no breaking change, so that section is omitted.
The folded log keeps each upstream PR title verbatim — including the `(CVE-2024-XXACK)` text
— but the 📦 Dependency Updates bullet stays neutral (`Updated libpng to 1.6.44`); we never
re-advertise a vulnerability in the teaser.
