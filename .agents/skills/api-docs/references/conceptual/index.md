# Conceptual documentation route

Use this route for Markdown under `documentation/docfx/guides/`. A conceptual article should help a
reader understand a model, choose an approach, complete a task, migrate working code, or resolve a
specific symptom. It should complement API reference rather than narrate members one by one.

## Route the work

Choose both a **change size** and an **article type** before reading the remaining references.

### Change size

| Size | Typical work | Route |
|---|---|---|
| Focused fix | One fact, broken link, typo, or small sample correction | Read [`../technical-fact-checking.md`](../technical-fact-checking.md), [`fact-checking.md`](fact-checking.md), and [`validation.md`](validation.md); add [`code-samples.md`](code-samples.md) for code or [`structure-and-style.md`](structure-and-style.md) for prose/metadata |
| Substantive review | Several claims, a complete example, platform coverage, or article structure | Read [`reviewing.md`](reviewing.md), then the matching blueprint and relevant shared references |
| New article or major rewrite | New reader journey, changed article type, or broad restructuring | Read [`authoring.md`](authoring.md), then the matching blueprint and relevant shared references |

Review is report-only unless the user asks to fix or rewrite the article. When they do, apply the
corrections and run the authoring validation pass.

### Article type

| Reader intent | Use when the reader needs to... | Blueprint |
|---|---|---|
| Overview or decision | Compare choices and find the right next task | [`templates/overview.md`](templates/overview.md) |
| Concept | Understand a model, lifecycle, or relationship | [`templates/concept.md`](templates/concept.md) |
| How-to | Complete one concrete task | [`templates/how-to.md`](templates/how-to.md) |
| Migration | Move working code from one supported approach to another | [`templates/migration.md`](templates/migration.md) |
| Troubleshooting | Diagnose and fix a named symptom or error | [`templates/troubleshooting.md`](templates/troubleshooting.md) |

Do not combine independent reader intents merely because they share APIs. Split the material when a
reader who needs one outcome would have to skip large sections written for another.

## Shared references

Load only the references the task needs:

- [`../technical-fact-checking.md`](../technical-fact-checking.md) — shared public-contract boundary,
  evidence hierarchy, and cross-layer verification used by API reference and conceptual docs.
- [`fact-checking.md`](fact-checking.md) — conceptual claim ledger, version/platform checks, external
  sources, and review evidence.
- [`code-samples.md`](code-samples.md) — snippet versus sample decisions, source verification, failure
  handling, ownership, async lifetimes, and intentionally incorrect code.
- [`structure-and-style.md`](structure-and-style.md) — metadata, introductions, headings, procedures,
  voice, global readiness, formatting, links, alerts, and accessible images.
- [`validation.md`](validation.md) — code, links, DocFX, rendered output, and validation reporting.
- [`microsoft-contribute-sources.md`](microsoft-contribute-sources.md) — provenance and the Microsoft
  Learn infrastructure rules intentionally not copied into SkiaSharp. Read this only when maintaining
  the skill.

## Quality contract

Whatever the article type:

1. Start from a real reader, starting state, and outcome.
2. Verify consequential claims against the closest source of truth.
3. Make code honest about what is complete, illustrative, platform-specific, or intentionally wrong.
4. Put prerequisites, constraints, and recovery guidance before the point where the reader needs them.
5. Make the path to success scannable and verifiable.
6. State uncertainty instead of turning an assumption into documentation.

The MicrosoftDocs/Contribute guidance supplies the editorial system. SkiaSharp source, tests, native
code, and platform implementations supply the technical truth.
