<!-- This file is the template for all SkiaSharp release notes pages.
     It is excluded from the DocFX build.

     AI reads this file when formatting release notes for any version:
     - Stable release: has NuGet link, release date, GitHub Release link
     - Preview-only: use "Preview only" instead of date, NuGet links to preview package
     - Upcoming/unreleased: use "In development", no NuGet/release links, omit Links section

     Adapt the example below based on the version's status.
     Omit sections that don't apply. Never force empty categories.
-->

# Version 3.119.2

> **Security hardening release** · Released February 7, 2026 · [NuGet](https://www.nuget.org/packages/SkiaSharp/3.119.2) · [GitHub Release](https://github.com/mono/SkiaSharp/releases/tag/v3.119.2)

<!-- For preview-only versions:
> **{theme}** · Preview only · [NuGet](https://www.nuget.org/packages/SkiaSharp/{version}-preview.N.B) · [GitHub Release](url)

     For upcoming/unreleased versions:
> **Upcoming release** · In development · Not yet available on NuGet
-->

## Highlights

<!-- 1-3 sentences. Lead with what matters most. Mention community contributors. -->

This release focuses on hardening the native libraries against common exploit techniques on Windows and Linux. Spectre mitigations, Control Flow Guard, and BufferSecurityCheck are now enabled across all Windows and Linux native builds. Three community contributors drove every user-facing change in this release.

## Breaking Changes

<!-- Always include. If none, say so explicitly. -->

*None in this release.*

## New Features

<!-- Group by category. Pick categories that fit — don't force empty ones.
     Common: Engine, GPU & Rendering, API Surface, Text & Fonts, Platform, Images & Documents
     Each item: bold title, sentence of context, ❤️ for community, PR link. -->

### Platform

- **tvOS `SKSurface.Create` overloads** — Adds the missing overloads for tvOS, bringing parity with other Apple platforms. ❤️ [@MartinZikmund](https://github.com/MartinZikmund) ([#3342](https://github.com/mono/SkiaSharp/pull/3342))

## Security

<!-- Only include if there are security-related changes. -->

- **Spectre mitigation for Windows** — Enables `/Qspectre` for `libSkiaSharp.dll`. ❤️ [@sshumakov](https://github.com/sshumakov) ([#3496](https://github.com/mono/SkiaSharp/pull/3496))
- **Control Flow Guard (CFG)** — Enables CFG for all Windows native DLLs. ❤️ [@Aguilex](https://github.com/Aguilex) ([#3397](https://github.com/mono/SkiaSharp/pull/3397))

## Bug Fixes

<!-- Only include if there are bug fixes. -->

- **Fixed the frobnitz crash** — Description of what was wrong and what the fix does. ([#NNN](https://github.com/mono/SkiaSharp/pull/NNN))

## Platform Support

<!-- Quick at-a-glance table. Only include platforms with changes. Omit rows with nothing new. -->

| Platform | What's New |
|----------|-----------|
| 🍎 Apple | tvOS `SKSurface.Create` overloads |
| 🪟 Windows | Spectre mitigation, Control Flow Guard |
| 🐧 Linux | Spectre mitigation |

<!-- Platform emoji reference: 🍎 Apple · 🪟 Windows · 🐧 Linux · 🤖 Android · 🌐 WebAssembly · 🎨 Core API · 🏗️ Build/CI · 📦 General -->

## Community Contributors ❤️

<!-- Only include if there are community contributors (anyone not @mattleibow).
     ALWAYS link usernames: [@user](https://github.com/user) — never bare @user anywhere. -->

| Contributor | What They Did |
|-------------|--------------|
| [@MartinZikmund](https://github.com/MartinZikmund) | Added missing tvOS `SKSurface.Create` overloads |
| [@Aguilex](https://github.com/Aguilex) | Enabled Control Flow Guard and BufferSecurityCheck on Windows |

## Links

<!-- For released versions only. Omit entirely for upcoming/unreleased. -->

- [Full Changelog](https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2)
- [NuGet Package](https://www.nuget.org/packages/SkiaSharp/3.119.2)
- [API Diff](https://github.com/mono/SkiaSharp/tree/main/changelogs/SkiaSharp/3.119.2)

---

## Preview 3 (February 5, 2026)

<!-- One sentence per preview. The rollup above has the detail. -->

Added Spectre mitigation for Windows native libraries.

[Full Changelog](https://github.com/mono/SkiaSharp/compare/v3.119.2-preview.2.3...v3.119.2-preview.3.1)

---

## Preview 1 (January 26, 2026)

Added missing tvOS `SKSurface.Create` overloads, plus build system improvements.

[Full Changelog](https://github.com/mono/SkiaSharp/compare/v3.119.1...v3.119.2-preview.1.2)

<!-- FORMATTING RULES:
     - Rollup at top: aggregate ALL changes across all previews into one polished summary
     - Previews are minimal: one sentence + changelog link each
     - Omit noise: don't itemize version bumps, CI fixes, doc/skill/workflow changes
     - Community ❤️: always credit and link anyone not @mattleibow
     - Adapt to content: security release leads with security, feature release leads with features
     - Skia engine: if a "Bump skia" PR is in the data, list it first under Engine category
-->
