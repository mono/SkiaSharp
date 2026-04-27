# Release Notes Template

This template defines the format for SkiaSharp website release notes. AI uses this
as reference when reformatting raw release data into polished pages.

## Workflow

1. **Fetch raw data** — `python3 scripts/generate-release-notes.py --last 5` downloads
   GitHub release bodies to a temp directory
2. **AI reformats** — Read the raw files + this template → write polished markdown
   to `documentation/docfx/releases/{version}.md`
3. **Update TOC** — Add new versions to `documentation/docfx/releases/TOC.yml`

## Template Structure

```markdown
# Version {X.Y.Z}

> **{one-line theme}** · Released {month day, year} · [NuGet](https://www.nuget.org/packages/SkiaSharp/{X.Y.Z})

## Highlights

{1-3 sentence summary of the release. What's the story? What should users know?
Mention the most impactful changes and how many community contributors if any.}

## Breaking Changes

{If none: "*None in this release.*"}

{If present, group by topic with sub-headers:}

### {Topic}

- **`OldThing` removed/deprecated** — {Why and what to use instead.}

{Link to migration guide if one exists.}

## New Features

{Group features by category. Use sub-headers. Each feature gets a bold title,
a sentence of context, and a PR link. Add ❤️ for community contributions.}

### {Category: Engine, GPU & Rendering, API Surface, Text, Images, Platform, etc.}

- **{Feature name}** — {What it does and why it matters.} {❤️ @contributor} ([#NNN](url))

## Security

{Only include if there are security-related changes.}

- {Description} ([#NNN](url))

## Bug Fixes

- {Description of fix} ([#NNN](url))

## Platform Support

| Platform | What's New |
|----------|-----------|
| 🍎 Apple | {summary} |
| 🪟 Windows | {summary} |
| 🐧 Linux | {summary} |
| 🤖 Android | {summary} |
| 🌐 WebAssembly | {summary} |

{Only include platforms with changes. Omit rows with nothing new.}

## Community Contributors ❤️

{Only include if there are community contributors (not @mattleibow).}

| Contributor | What They Did |
|-------------|--------------|
| [@username](https://github.com/username) | {Brief description} |

## Links

- [Full Changelog](https://github.com/mono/SkiaSharp/compare/{prev-tag}...{tag})
- [NuGet Package](https://www.nuget.org/packages/SkiaSharp/{X.Y.Z})
- [API Diff](https://github.com/mono/SkiaSharp/tree/main/changelogs/SkiaSharp/{X.Y.Z})

---

## Preview {N} ({month day, year})

{One sentence summary of what this preview added.}

[Full Changelog](https://github.com/mono/SkiaSharp/compare/{prev-tag}...{tag})

---

## Preview {N-1} ({month day, year})

{One sentence summary.}

[Full Changelog](url)
```

## Rules

1. **Rollup at top** — The top section aggregates ALL changes across all previews
   into one polished summary. Don't just describe what the stable tag added.

2. **Previews are minimal** — Each preview section is a one-sentence summary +
   changelog link. The detail lives in the rollup.

3. **Omit internal/CI-only PRs** — Don't list version bumps, CI fixes, doc updates,
   skill changes, or other items that don't affect users. Mention them in the preview
   summary if relevant ("CI and documentation improvements") but don't itemize.

4. **Community ❤️** — Always credit community contributors (anyone not @mattleibow)
   with ❤️ inline and in the Contributors table.

5. **Categorize features** — Group by what they affect: Engine, GPU & Rendering,
   API Surface, Text, Images & Documents, Platform, Security, etc. Pick categories
   that fit the release — don't force empty categories.

6. **Platform table** — Quick at-a-glance summary. Only include platforms with changes.

7. **Breaking changes first** — If there are breaking changes, they appear right
   after Highlights so users see them immediately.

8. **Adapt to content** — A security-focused release (like 3.119.2) should lead with
   security. A feature-heavy release (like 1.68.0) should lead with features. The
   template sections are guidelines, not rigid requirements — omit empty sections,
   add new ones if the release warrants it.

9. **Links are relative** — API diff links use relative paths when possible
   (e.g., to changelogs in the same repo).

10. **Version in filename** — Each file is named `{X.Y.Z}.md` (e.g., `3.119.2.md`).

## Section Reference

| Section | When to Include | Required? |
|---------|----------------|-----------|
| Highlights | Always | Yes |
| Breaking Changes | Always (say "None" if empty) | Yes |
| New Features | When there are features | If applicable |
| Security | When there are security changes | If applicable |
| Bug Fixes | When there are bug fixes | If applicable |
| Platform Support | When multiple platforms affected | If applicable |
| Community Contributors | When community contributed | If applicable |
| Links | Always | Yes |
| Preview sections | When there are previews | If applicable |

## Platform Emoji Reference

| Emoji | Platform |
|-------|----------|
| 🍎 | Apple (iOS, macOS, tvOS, Mac Catalyst) |
| 🪟 | Windows |
| 🐧 | Linux |
| 🤖 | Android |
| 🌐 | WebAssembly / Blazor |
| 🎨 | Core API |
| 🏗️ | Build system / CI |
| 📦 | General |
