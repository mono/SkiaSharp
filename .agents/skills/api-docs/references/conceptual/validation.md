# Validating conceptual documentation

Validation proves both technical correctness and publishing quality. Run the smallest checks that cover
the change, then build the complete DocFX site. Inspect the results, correct defects, and repeat the
applicable checks until the selected wave is trustworthy; a green build alone is not a correctness
verdict.

## 1. Recheck source-backed claims

- Re-read every changed API call against current source.
- Confirm every CRITICAL/IMPORTANT correction still matches its cited evidence.
- Recheck platform rows and version qualifiers.
- Search for stale absolutes, placeholders, and renamed terms.

## 2. Validate code

For complete examples:

- Compile them in an existing test/sample project or a focused temporary project using the repository's
  existing toolchain.
- For core SkiaSharp API workflows, prefer the existing
  `tests\SkiaSharp.Tests.Console\SkiaSharp.Tests.Console.csproj` when it is an appropriate host.
- Run them when the article promises runtime output and the host supports the feature.
- Use existing tests or platform renderers as substitute evidence when GPU hardware or another platform
  is unavailable.

For illustrative snippets, verify signatures, variables, ownership, and control flow against source even
when a complete host cannot be built.

If a repository build requires native binaries, follow the root `AGENTS.md` bootstrap rule. Never use
`externals-download` after native changes.

## 3. Validate navigation and links

- Add or update the appropriate `TOC.yml` entry.
- Check relative file paths and every changed heading fragment.
- Confirm xref UIDs and case.
- Check that prerequisite and next-step articles exist.
- Search for references to renamed or moved pages.

## 4. Build DocFX

Restore the repository's existing tools, then build:

```powershell
dotnet tool restore
dotnet docfx documentation\docfx\docfx.json
```

Run warnings as errors when the baseline is clean:

```powershell
dotnet docfx documentation\docfx\docfx.json --warningsAsErrors
```

If the repository already has unrelated warnings, do not hide or "fix" them as part of the article.
Capture the baseline and prove that no warning references a changed file or link. Report the pre-existing
warning count and paths explicitly.

## 5. Inspect rendered output

Inspect the built page whenever headings, tables, lists, alerts, code, images, or navigation changed.
Keep the build log and representative rendered page outside committed content until semantic review is
complete so claims about warnings and rendered structure remain independently checkable.
Check:

- Heading hierarchy and TOC placement.
- Wrapping and readability of tables/code at narrow widths.
- Alert choice and spacing.
- Image size, alt text, and nearby explanation.
- Link targets and fragment navigation.

Derive any reported heading, table, code-block, alert, warning, or changed-file count from the retained
artifact/log and check the arithmetic before reporting it; do not rely on a remembered or hand-enumerated
total.

A successful build does not prove that a page is readable.

## 6. Report validation

Use:

```text
VALIDATED | source:<pass/fail> snippets:<pass/fail/not-run> links:<pass/fail> docfx:<pass/fail> rendered:<checked/not-checked>
BASELINE | warnings:<n> | changed-file warnings:<n> | <paths or none>
LIMITATION | <check not run> | <reason and substitute evidence>
```

Do not claim a complete validation when a code sample was only inspected or a rendered page was not
opened.
