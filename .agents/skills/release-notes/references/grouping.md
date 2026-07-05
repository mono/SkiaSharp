<!-- Reference asset of the release-notes skill. The Polish agent reads this to turn a flat
     PR list into a curated, grouped page. Not part of the DocFX site. -->

# Grouping & categorizing the changes

Release notes are **curated**, not exhaustive. The single biggest quality failure is rendering
**one bullet per PR** — a wall of 50+ line items. A good page merges related PRs into a handful
of **thematic bullets**, each telling one product story. This file is how.

## The density budget

Across **all** category sections combined, aim for **8–15 product bullets total** — not one per
PR. A single bullet carries **2–8 related PRs**. Reserve a solo bullet only for a genuine
standalone landmark (the engine bump, one headline API, a breaking-only change).

**The countable rule: at most ~4 bullets per category section.** After you draft a category,
count its bullets — if a `##` section (other than `## Breaking Changes`, which is one bullet per
distinct break) has **more than ~4**, you are enumerating: find the same-kind items and merge
them into one grouped bullet. Concretely, within a category:

- Several **fixes to the same subsystem** (all Uno/XAML, all MAUI, all memory/leak) → **one**
  "Fixed … regressions" bullet listing them, main-style (see the example below).
- Several **native-dependency bumps** (libpng, libwebp, brotli, expat) → **one** bullet.
- Several **overloads of the same shape** (all the `Span<T>` additions) → **one** bullet.

`## Breaking Changes` is the exception — it may carry one bullet per distinct break (a v4-style
release legitimately has ~10). Every *other* category is curated down to a few grouped bullets.

A 130-PR release and a 12-PR release should have Highlights and category sections of *similar
length*. The big release is more *selective*, not longer.

## How to cluster (three axes, in order of preference)

1. **Same API / feature area.** All `Span<T>` overloads across `SKMatrix.MapPoints`/`MapVectors`,
   `SKPath.AddPoly`/`SetRectRadii`, and the color filters → **one** bullet.
2. **Same platform / target.** All Uno / XAML view fixes → one bullet; all Blazor / WASM fixes →
   one; all "Metal on Apple" work → one.
3. **Same symptom / mechanism.** All view-leak / memory fixes → one; all packaging/nuget-layout
   changes → one.

Only the `[product]` lines are grouped — every `[internal]` line is dropped into the single
collapse line (see the writing rules). Bot-authored PRs may be grouped but never credited.

## Group titles are impact statements, not PR titles

- The **title** is the shipping impact — what the consumer gets — **not** a concatenation of PR
  titles: `` **`Span<T>` overloads across the point / path / color-filter APIs** ``.
- The **description** is one line on *why the consumer cares*, not a list of the PRs.
- All the group's PR links go in **one parenthetical at the end**, with each community
  contributor credited once: `❤️ [@a](url), [@b](url) ([#1](url), [#2](url), [#3](url))`.

## Categories (top-level `##` sections)

Pick the ones that fit; never force an empty category. Order by importance for the release.

`Engine & native dependencies` · `New APIs` · `GPU & Rendering` · `Text & Fonts` ·
`Views & platforms` · `Fixes` · `Lifecycle & Internals` · `Security`

The **Skia engine bump** (a "Bump skia" / "milestone" PR) always leads, under
`Engine & native dependencies`.

## Worked example — 3.116.0

The raw data lists these four `[product]` PRs separately:

```
[product] Add SKMatrix.MapPoints/MapVectors, SKPath.AddPoly overloads with Span<SKPoint> (#3030) @alexandrvslv
[product] Provide Span overload for SetRectRadii (#2949) @Youssef1313
[product] Add spans to the color filters (#2879) @mattleibow
[product] Added GetPixelSpan() with offsets (#2609) @mattleibow
```

❌ **Wrong (one bullet per PR):**

```markdown
- **Span<SKPoint> for SKMatrix.MapPoints/MapVectors and SKPath.AddPoly** — … ❤️ [@alexandrvslv](https://github.com/alexandrvslv) ([#3030](…))
- **Span<T> overload for SKPath.SetRectRadii** — … ❤️ [@Youssef1313](https://github.com/Youssef1313) ([#2949](…))
- **New color-filter span APIs** — … ([#2879](…))
- **GetPixelSpan() overloads with offset** — ([#2609](…))
```

✅ **Right (one grouped bullet):**

```markdown
- **`Span<T>` overloads across the point / path / color-filter APIs** — Zero-copy interop with stack-allocated buffers, for `SKMatrix.MapPoints`/`MapVectors`, `SKPath.AddPoly`/`SetRectRadii`, the color filters, and `GetPixelSpan`. ❤️ [@alexandrvslv](https://github.com/alexandrvslv), [@Youssef1313](https://github.com/Youssef1313) ([#3030](https://github.com/mono/SkiaSharp/pull/3030), [#2949](https://github.com/mono/SkiaSharp/pull/2949), [#2879](https://github.com/mono/SkiaSharp/pull/2879), [#2609](https://github.com/mono/SkiaSharp/pull/2609))
```

More groupings from the same release, so the page reads as ~18 bullets instead of ~56:

| PRs | One grouped bullet |
|---|---|
| #2829, #2885, #3026 | **Skia updated to milestone m116** (with in-milestone refreshes) |
| #3093, #3089, #3081, #3087, #3086 | **.NET 9 Blazor / WASM fixes** |
| #2918, #2854, #2720, #2563, #2559, #2612 | **Uno / XAML view fixes** |
| #2598, #2733, #2317 | **New accelerated views** (`SKGLView`, WinUI, WPF `SKGLElement`) |
| #2747, #2788, #2804 | **Metal on Mac Catalyst** (default backend + common Metal APIs) |
| #2830, #2883, #2889, #2748, #2630 | **New drawing APIs** (`SKBlender`, `SKPicture` R-Tree, `ToRawShader`, Skottie builder) |
