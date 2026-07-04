<!-- This file is the template for all SkiaSharp AND HarfBuzzSharp release notes pages.
     It is a reference asset of the release-notes skill
     (.agents/skills/release-notes/references/) and is not part of the DocFX site.

     There are two page families (see release-notes-and-api-diffs.md §1.5):
     - SkiaSharp family: title "# Version X", lives at releases/<line>.md
     - HarfBuzz family:  title "# HarfBuzzSharp X", lives at releases/harfbuzzsharp/<hb-line>.md
     Both families use the SAME structure below. The HarfBuzz page only differs in its
     title, its status banner (it "Ships with SkiaSharp X" — where X is the canonical
     introducing SkiaSharp release, the one whose "Bump HarfBuzz" PR created the line —
     and is dated by that release rather than having its own date, since HarfBuzz never
     releases on its own), and its API-changes link target. A HarfBuzz page lists only
     HarfBuzz-touching changes (the script already filtered them).

     AI reads this file when formatting release notes for any version:
     - Stable release: has NuGet link, release date, GitHub Release link
     - Preview-only: use "Preview only" instead of date, NuGet links to preview package
     - Upcoming/unreleased: use "In development", no NuGet/release links, omit Links section

     Adapt the example below based on the version's status and family. The worked example
     is ENTIRELY FICTIONAL — the version (9.990.0), PR numbers, handles, API names, and
     dependency versions are all placeholders — and is a category-rich stable rollup so
     every section is shown at least once. A smaller release will naturally have fewer
     sections. Never copy the placeholders into a real page; use the real raw-data. Omit
     sections that don't apply, and never force empty categories.

     "No changes" pages (a HarfBuzz line with no HarfBuzz-touching changes) are written
     in full by the GENERATOR and are NOT in your "Files to polish" list — never author
     or edit one.

     COMPANION FILES (see release-notes-and-api-diffs.md §4.7): the raw-data block at the
     top of a page may list "companions — open and read these during polish":
       - the manual additions sidecar (<stem>.notes.md) — freeform maintainer notes;
       - the full API diff — <line>/index.md indexes every per-assembly diff and flags
         the breaking ones; open it, then follow the assemblies that matter; and
       - the API breaking diff (*.breaking.md) — one per broken assembly, present only
         when signatures broke (a big release can list many; summarize across them).
     OPEN and READ each listed companion, then SUMMARIZE — never paste it verbatim or dump
     the whole diff. Weave the manual notes into Highlights / Breaking Changes; summarize
     breaking changes from the breaking diff. You may READ these files but NEVER edit them,
     and never touch git or the gh API.
-->

# Version 9.990.0

> **Engine upgrade & API cleanup** · Released January 15, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/9.990.0) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v9.990.0)

<!-- For preview-only versions:
> **{theme}** · Preview only · [NuGet](https://www.nuget.org/packages/SkiaSharp/{version}-preview.N.B) · [GitHub Release](url)

     For upcoming/unreleased versions:
> **Upcoming release** · In development · Not yet available on NuGet

     For a HarfBuzz family page (title would be "# HarfBuzzSharp 9.9.0"):
> **{theme}** · Ships with SkiaSharp 9.990.0 · [NuGet](https://www.nuget.org/packages/HarfBuzzSharp/9.9.0) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v9.990.0)

     For an in-flight (unreleased) HarfBuzz page — a new HarfBuzz line an upcoming
     SkiaSharp release declares but that has not published yet:
> **Upcoming release** · In development · Ships with the upcoming SkiaSharp 9.991.0 · Not yet available on NuGet
-->

<!-- SCRIPT-OWNED STRUCTURAL LINES — do not author, move, or narrate these.
     The script injects them right here (immediately under the status banner) and owns
     them completely; keep any it emits VERBATIM and write none of your own.

     - Supersession banner (only on a stable rollup that supersedes preview-only lines):
       > **Supersedes [9.989.0](9.989.0.md)** · Rolls up preview-only work ...
       Keep it verbatim; add a one-line note in Highlights that the work rolled up.

     - API-changes line (every emitted page whose line has an API-diff folder): on a
       SkiaSharp page it points at this line's API-diff index (<line>/index.md), plus a
       link to the co-shipped HarfBuzz hub page when the release co-ships HarfBuzzSharp;
       on a HarfBuzz page it points at this HarfBuzz line's index
       (harfbuzzsharp/<hb-line>/index.md) and back at its canonical SkiaSharp release.
       The diff is the whole story — do NOT write prose about API diffs, HarfBuzz
       versions, or cross-family links. If the line is absent (e.g. an unreleased head
       page with no diff folder yet) there is simply no folder — do NOT add one.

     HarfBuzz-page API-changes example:
> **API changes** · [HarfBuzzSharp API diff](9.9.0/index.md) · Ships with [SkiaSharp 9.990.0](../9.990.0.md) -->
> **Supersedes [9.989.0](9.989.0.md)** · Rolls up preview-only work that was never released as stable — those changes are included cumulatively below.

> **API changes** · [SkiaSharp API diff](9.990.0/index.md) · [HarfBuzzSharp 9.9.0](harfbuzzsharp/9.9.0.md)

## Highlights

<!-- Short and impact-first — HARD CAP: at most 2-3 sentences, no matter how big the release.
     Name only the 3-4 biggest / coolest things (the engine jump, the headline feature, a
     breaking change to know about). A hook, not a table of contents — the categories below
     carry the full list, so never enumerate dependency bumps, individual fixes, or every new
     API here; the bigger the release, the more SELECTIVE (not longer) it gets. Mention only
     standout community contributors by linked name. If the manual additions sidecar
     (<stem>.notes.md, see §4.7) has editorial points the maintainer wants brought out, weave
     them in here in your own words. On a supersedes rollup, note in one line that the skipped
     preview work is included cumulatively. -->

The first engine bump of the 9.x line: Skia m998 lands with variable-font support and animated encoding, alongside a major-version API cleanup that promotes long-deprecated members to compile errors — check Breaking Changes before upgrading. HarfBuzz 9.9.0 ships too ([HarfBuzzSharp 9.9.0](harfbuzzsharp/9.9.0.md)). Standout community work from [@octocat](https://github.com/octocat) and [@monalisa](https://github.com/monalisa).

## Breaking Changes

<!-- ALWAYS include this heading — never drop it, and place it right after Highlights.
     If the raw-data block lists companions (§4.7), open them and SUMMARIZE the breaks as
     a few bullets:
     - the API breaking diff (*.breaking.md) — open every listed file (one per broken assembly)
       and name the affected types/areas in a few bullets, with a small migration snippet where
       it helps. Summarize, don't dump — readers follow the API-diff link for the full list.
     - the manual additions sidecar (<stem>.notes.md) — behavioral breaks (same signature,
       different runtime behavior) and interop / native-struct breaks (e.g. fields removed from
       a native backend-context struct) that never appear in a signature diff. Fold them in here.
     If NEITHER companion has anything, write "None in this release." -->

- **Legacy obsoletes are now compile errors** — Members deprecated in a previous major (including the old font-on-`SKBar` APIs) are now compile errors, and obsolete enum members are trimmed from the reference assembly. Move to `SKFoo` for text, and to the replacements flagged by the earlier obsolete warnings. See the [API diff](9.990.0/index.md) for the full list. ([#1201](https://github.com/mono/SkiaSharp/pull/1201), [#1202](https://github.com/mono/SkiaSharp/pull/1202))
- **`new SKFoo()` default state** — A parameterless `SKFoo` now carries an empty `SKBaz` instead of the default one. Pass it explicitly when you need the default:

  ```csharp
  // Before: implicitly the default SKBaz
  var foo = new SKFoo();
  // After: pass the value you want
  var foo = new SKFoo(SKBaz.Default);
  ```
- **Removed `SKFooContext` fields** — Several native interop fields were removed from the backend-context struct; rebuild any code that populated them directly. See the [API diff](9.990.0/index.md) for the full list.

## Engine

<!-- Category sections are top-level `##` headings — NOT nested under a "New Features" parent.
     Group items by what they affect; pick the categories that fit the release and never force
     an empty one. Common categories: Engine, GPU & Rendering, API Surface, Text & Fonts,
     Bug Fixes, Lifecycle & Internals, Platform, Images & Documents, Security.
     Each item: **bold title** — description. ❤️ for community, then the PR link.
     If a "Bump skia"/milestone PR is in the data, list it first here under Engine. -->

- **Skia milestone m999** — Upgrades the native Skia engine to Chrome milestone m999. ([#1203](https://github.com/mono/SkiaSharp/pull/1203))
- **Skia milestone m998** — Included from the 9.989.0 rollup. ❤️ [@octocat](https://github.com/octocat) ([#1204](https://github.com/mono/SkiaSharp/pull/1204))

## API Surface

- **Variable font support** — Variation-axis and font-palette APIs across SkiaSharp and HarfBuzzSharp (`HBFoo`/`HBBar` axes, color palettes for emoji fonts). ❤️ [@octocat](https://github.com/octocat) ([#1205](https://github.com/mono/SkiaSharp/pull/1205))
- **`SKFooEncoder` — animated output** — New encoder for multi-frame animated files. ([#1206](https://github.com/mono/SkiaSharp/pull/1206))
- **`SKBar.GetData()`** — Zero-copy stream-to-`SKData` conversion. ([#1207](https://github.com/mono/SkiaSharp/pull/1207))
- **`SKQuxOptions` on `SKQux.Draw`** — Filtering-quality overloads. ❤️ [@octobot](https://github.com/octobot) ([#1208](https://github.com/mono/SkiaSharp/pull/1208))

## Bug Fixes

<!-- Only include if there are bug fixes. -->

- **Fixed `SKFooMap`/`SKBar.GetPixelSpan` stride** — `GetPixelSpan` now uses `RowBytes` for stride instead of width × bytes-per-pixel, fixing incorrect results on padded buffers. ([#1209](https://github.com/mono/SkiaSharp/pull/1209))
- **Fixed `SKFooPath` finalizer crash** — Resolves a crash when the builder is collected before the object it created. ❤️ [@octocat](https://github.com/octocat) ([#1210](https://github.com/mono/SkiaSharp/pull/1210))
- **Fixed Android `SKFooView` rendering after MAUI tab switch** ❤️ [@hubot](https://github.com/hubot) ([#1211](https://github.com/mono/SkiaSharp/pull/1211))

## Lifecycle & Internals

<!-- Correctness/lifetime/build-plumbing work that isn't a user-facing feature or a plain bug fix. -->

- **Reworked singleton lifecycle** — Singleton native objects (default paints, fonts, etc.) now use a proper ref-counted lifecycle, preventing use-after-free crashes. ❤️ [@octocat](https://github.com/octocat) ([#1212](https://github.com/mono/SkiaSharp/pull/1212))
- **Moved the native-compat gate to a static constructor** — The native library version check now runs at startup, giving a clearer error when the wrong binary is loaded. ([#1213](https://github.com/mono/SkiaSharp/pull/1213))

## Platform

- **Linux Bionic native assets** ❤️ [@monalisa](https://github.com/monalisa) ([#1214](https://github.com/mono/SkiaSharp/pull/1214))
- **Tizen x64 and ARM64 native builds** ([#1215](https://github.com/mono/SkiaSharp/pull/1215))
- **WASM: dropped pre-.NET 8 Emscripten builds** ([#1216](https://github.com/mono/SkiaSharp/pull/1216))
- **Fixed WinUI Projection DLL not found for .NET 9** ([#1217](https://github.com/mono/SkiaSharp/pull/1217))
- **Fixed Apple platform TFMs** — `.26.0` for libraries, unversioned for apps. ([#1223](https://github.com/mono/SkiaSharp/pull/1223))
- **C# 13 support on legacy TFMs** ❤️ [@monalisa](https://github.com/monalisa) ([#1224](https://github.com/mono/SkiaSharp/pull/1224))

## Security

<!-- Only include if there are security-related changes (usually native dependency bumps).
     Use the real dependency names from the raw data; the versions shown here are placeholders. -->

- **libexpat updated to 9.9.9** ([#1218](https://github.com/mono/SkiaSharp/pull/1218))
- **libjpeg-turbo updated to 9.9.9** ([#1219](https://github.com/mono/SkiaSharp/pull/1219))
- **freetype updated to 9.9.9** ([#1220](https://github.com/mono/SkiaSharp/pull/1220))
- **zlib updated to 9.9.9** ([#1221](https://github.com/mono/SkiaSharp/pull/1221))
- **libpng updated to 9.9.9** ([#1222](https://github.com/mono/SkiaSharp/pull/1222))

## Community Contributors ❤️

<!-- Only include if there are community contributors (anyone not @mattleibow).
     ALWAYS link usernames: [@user](https://github.com/user) — never bare @user anywhere.
     TWO columns: "Contributor" is the PLAIN link with NO ❤️ (the heart belongs only on the
     inline category bullets above; in this table it just wraps badly on long rows). "What They
     Did" is a SHORT PROSE summary of everything they landed this release — NEVER just a list of
     PR numbers. One row per contributor. -->

| Contributor | What They Did |
|-------------|--------------|
| [@octocat](https://github.com/octocat) | Skia m998 bump; variable fonts; obsoletes promotion; singleton lifecycle; `SKFooPath` crash fix |
| [@monalisa](https://github.com/monalisa) | Linux Bionic native assets; C# 13 support on legacy TFMs |
| [@hubot](https://github.com/hubot) | Fixed Android `SKFooView` rendering after MAUI tab switch |
| [@octobot](https://github.com/octobot) | `SKQuxOptions` overloads for `SKQux.Draw` |

## Links

<!-- For released versions only. Omit entirely for upcoming/unreleased.
     Author ONLY the changelog + NuGet links here from the raw data. Do NOT add
     an API-diff link — the script already injects the "> **API changes**" line
     near the top (see the SCRIPT-OWNED note above). -->

- [Full Changelog](https://github.com/mono/SkiaSharp/compare/v9.988.0...v9.990.0)
- [NuGet Package](https://www.nuget.org/packages/SkiaSharp/9.990.0)

---

## Release Candidate 1 (January 8, 2026)

<!-- One sentence per preview/RC — the rollup above carries the detail. Render one trailing
     "## <label> (<date>)" section per entry in the data-block's preview list, newest first. -->

Upgraded Skia to m999, reworked singleton lifecycles, promoted the remaining legacy obsoletes to compile errors, and fixed `SKFooMap` stride and coordinate calculations.

[Full Changelog](https://github.com/mono/SkiaSharp/compare/v9.989.0-preview.3.1...v9.990.0-rc.1.2)

---

## Preview 3 (December 20, 2025)

Added a new managed color struct, dependency updates, API wrappers and naming fixes, and dropped pre-.NET 8 WASM Emscripten support.

[Full Changelog](https://github.com/mono/SkiaSharp/compare/v9.989.0-preview.2.1...v9.989.0-preview.3.1)

---

## Preview 1 (December 2, 2025)

Variable font support, animated encoding, and the first engine bump of the line.

[Full Changelog](https://github.com/mono/SkiaSharp/compare/v9.988.0...v9.989.0-preview.1.1)

<!-- FORMATTING RULES (priority order — product-focus first):
     - Product over project (RULE 1): write for people USING the library. Every raw-data PR line
       is pre-tagged [product] or [internal] by the script (product = touches binding/ native/
       externals/ source/; everything else = internal). DROP every [internal] line — never a
       bullet per internal change; roll them all into ONE trailing "Plus various CI,
       documentation, and internal tooling improvements." line (or omit it if none). Write up
       only [product] lines. The title is not evidence (a security-audit skill change is not a
       library security fix). Keep native-dep bumps, native build flags that ship in the binary
       (Spectre mitigation, new RIDs/TFMs), API changes, behavior/bug fixes, packaging.
     - Highlights: short and impact-first — HARD CAP at 2-3 sentences, no matter how big the
       release. Name only the 3-4 biggest / coolest things; a hook, not a table of contents.
       Never enumerate dependency bumps, individual fixes, or every new API here — the bigger
       the release, the more selective (not longer) it gets.
     - Rollup at top: aggregate ALL [product] changes across all previews into one polished summary
     - Categories are top-level `##` sections (Engine, API Surface, Bug Fixes, Platform, …),
       NOT nested under a "New Features" parent; pick the ones that fit and never force empties
     - Previews/RC are minimal: one sentence + changelog link each
     - Attribution: item shape is `**title** — desc. ❤️ [@user](url) ([#NNN](url))`. Credit &
       link every community contributor (anyone not @mattleibow); the maintainer's own PRs end
       with just the `([#NNN](url))` link (no ❤️). NEVER write `by @handle`, and never emit a
       bare @user anywhere in the rendered body. In the Community Contributors TABLE the
       "Contributor" cell is a plain `[@user](url)` with NO ❤️ (the heart is only on the inline
       bullets above) and "What They Did" is a short PROSE summary, never a bare list of PRs.
     - Adapt to content: security release leads with security, feature release leads with features
     - Skia engine: if a "Bump skia" PR is in the data, list it first under Engine
     - Breaking Changes: always emit the `## Breaking Changes` heading right after Highlights
       ("None in this release." when there are none)
     - HarfBuzz pages: same structure, title "# HarfBuzzSharp X", banner "Ships with
       SkiaSharp Y"; the data is already filtered to HarfBuzz-touching changes — don't
       pull in SkiaSharp-only items. Never author a generator-written "No changes" page.
     - API links: never hand-author, move, or narrate API-diff, HarfBuzz, or cross-family
       links. The script injects the "> **API changes**" line under the banner on every
       emitted page; keep it verbatim and add nothing. No block means no folder yet.
     - Companions (§4.7): if the raw-data block lists "companions", open and read each one
       (manual notes sidecar, full API diff, breaking diff) and SUMMARIZE. Weave manual
       notes into Highlights/Breaking Changes; summarize breaking changes from the breaking
       diff with a small migration example where it helps. Never dump a diff verbatim, and
       never edit a companion file.
-->
