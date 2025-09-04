# API Docs Migration Guide

This guide is for agents migrating API documentation from the legacy `docs/SkiaSharpAPI` into inline XML docs in the C# codebase. Use this together with `API.Docs.Migration.Checklist.md`.

Quick start
- Open `API.Docs.Migration.Checklist.md` and pick the next unchecked type from the top (within its namespace).
- Open the XML at `docs/SkiaSharpAPI/<Namespace>/<Type>.xml` and the C# source file under `binding/**` or `source/**` that defines that type.
- Copy the XML docs verbatim into the code as XML documentation comments.
- Mark the type and each migrated member in the checklist as checked.

Rules
- Source of truth: Only use the XML files in `docs/SkiaSharpAPI`. Do not invent, paraphrase, or summarize; copy text verbatim (including "To be added.").
- Placement style: Place XML docs immediately before declarations.
  - Types: place `///` docs outside and immediately above the `class/struct/interface/enum` declaration.
  - Members: place docs immediately above the member declaration.
- XML tag formatting: The opening and closing tags for `summary` and `remarks` must each be on their own line.
  - Example:
    - Correct:
      /// <summary>
      /// Represents a text buffer in memory.
      /// </summary>
      /// <remarks>
      /// Additional details here.
      /// </remarks>
    - Incorrect: `/// <summary>Represents a text buffer in memory.</summary>`
- Map DocIds correctly:
  - `T:` type, `M:` method/constructor, `P:` property, `F:` field, `E:` event.
  - Match overload signatures exactly before pasting docs.
- Coverage: Migrate all members that exist in code and are present in the XML. If the XML contains a DocId that is not present in code, do not add code. Add a note under the type in the checklist: `Missing in code: <DocId>` and leave it unchecked.
- Fidelity: Preserve wording, punctuation, and simple formatting. Use appropriate XML doc tags: `summary`, `remarks`, `param`, `returns`, `value`, `exception` when present.
- Tag ordering, make sure to follow the order of: `summary`, `value`, `param`, `returns`, `exception`, `remarks`.
- No API changes: Do not alter public APIs to make docs fit. If there’s a mismatch, note it in the checklist and continue.

Workflow
1) Select the next unchecked type in `API.Docs.Migration.Checklist.md`.
2) Open its XML and corresponding C# file.
3) Insert type docs above the type; then migrate each member’s docs by matching DocIds to members. Keep order and text verbatim.
4) If a DocId isn’t found in code, note it in the checklist as `Missing in code: <DocId>`.
5) Save the file, sanity-check XML doc syntax.
6) Tick off the type and migrated member lines in the checklist.
7) Repeat for the next type.

Helpers
- Regenerate the checklist from the docs folder (prints to stdout):
  - PowerShell:
    - `pwsh -NoProfile -File scripts/Generate-ApiDocsChecklist.ps1 -DocsRoot docs/SkiaSharpAPI > API.Docs.Migration.Checklist.md`
- Searching code: find the C# type by fully-qualified name or by filename in `binding/**` or `source/**`.

Definition of done (per type)
- Type documentation is placed immediately before the type declaration.
- All existing members with DocIds in the XML are documented verbatim.
- Missing DocIds are noted under the type in the checklist.
- The type and migrated members are checked off in `API.Docs.Migration.Checklist.md`.

Notes
- Many entries in the XML say "To be added." Keep them verbatim.
- For nested types, place docs above the nested declaration, still using the outside-before style.
