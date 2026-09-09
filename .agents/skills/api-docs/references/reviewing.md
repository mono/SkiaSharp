# Reviewing existing docs

Audit **existing** SkiaSharp docs for accuracy, freshness, examples, and hygiene — by scope. This is the
dual of [`adding.md`](adding.md) (which fills blanks); review improves what is already filled. Review is
**report-only by default**; fixing is a separate, gated step.

You run this yourself, end to end — resolve scope, read source, compare, report, and (if approved) fix.
One agent does the whole pass. Work in batches of ~25–40 files so each pass stays auditable and resumable.

## Required reading (first)

1. [`skia-patterns.md`](skia-patterns.md) — pre-verified domain facts (color layouts, struct defaults,
   standard-based enums, caller-owned vs parent-owned). If a doc claim matches this file, it is correct —
   do **not** "correct" it from your own reasoning about how a macro "should" expand.
2. [`checklist.md`](checklist.md) — the CRITICAL/IMPORTANT/MINOR severity taxonomy you classify against.
3. [`patterns.md`](patterns.md) — .NET XML doc syntax, verb conventions, `cref` vs `xref` rules.
4. [`obsolete-api-map.md`](obsolete-api-map.md) — obsolete members and the modern API to use instead.
   §1 lists members that are always obsolete; §2 lists methods whose name also exists on the modern API
   (disambiguate by signature). Using an obsolete member in an example is a compile failure → CRITICAL.

Apply these facts; do not restate them in the docs.

## Adversarial framing

AI doc writers confidently state "facts" without checking source — wrong parameter constraints, wrong
channel names, wrong byte layouts, invented overloads, wrong defaults — and frequently write examples
that call obsolete members or undeclared variables. **Assume errors exist.** A review that finds 0 issues
across many files almost certainly skimmed. Every factual finding must cite source (`path:line`).

## Procedure

1. **Pick which docs to review.** The docs live at `docs/SkiaSharpAPI/<Namespace>/<Type>.xml`; list them
   directly and map each to its source at `binding/<Namespace>/<Type>.cs` (`grep` if the guess is wrong).
   The user describes the target in plain language. For a theme ("the font docs"), scan the list and
   **select the matching files yourself** — e.g. expand "font" to `SKFont`, `SKTypeface`, `SKFontMetrics`,
   `SKTextBlob`, and the text APIs on `SKPaint`/`SKCanvas`, judging by each type's purpose not just its
   filename. For "whatever changed" use `git -C docs diff --name-only origin/main...HEAD`; for the whole
   library review every file. Shard into ~25–40-file batches; review is incremental against the
   `last-reviewed` marker.

2. **Run the deterministic checks** on the batch with `docs-format-docs` ([`validation.md`](validation.md)).
   It finds objective defects with no model cost and emits findings in the shared contract.

3. **Review each file (source first).** From the filename, find and **READ the C# source in `binding/`
   BEFORE reading the XML** and build a fact sheet: constructors, overloads, accessor kind per property
   (`{ get; }` vs `{ get; set; }`), validation logic (throws/clamps/pads/truncates), numeric constants,
   defaults. Then read each `<Docs>` block and run the checks below. A file reviewed with **no source read
   is incomplete**.

4. **Collect and dedupe findings** in the shared contract. Deduplicate by `(file, docId, class)` plus
   fuzzy message match; when the linter and your own review report the same defect, keep one row. On a
   severity disagreement, take the **highest**.

5. **(Gated) Fix.** If fixing is approved, edit the XML directly for CRITICAL (and chosen IMPORTANT)
   findings, and expand examples where types are example-poor — port the `SKCanvas`/`SKShader` bar to
   barren types like `SKFont`, `SKImageFilter`.

6. **Validate & format** — only if step 5 made edits: run `docs-format-docs` and fix any build-failing
   broken-XML errors ([`validation.md`](validation.md)).

7. **Land** per-wave PRs on `dev/...` branches in the `docs` submodule.

## Checks

### A. Factual — claims vs source (cite `path:line`)

- **Parameter constraints** ("exactly N", "must be", "cannot be"): does the method body actually
  validate/reject, or silently accept/pad/truncate? Read the body, not just the signature.
- **Accessor verb** vs the `MemberSignature` (`{ get; }` → "Gets", `{ get; set; }` → "Gets or sets").
- **Defaults:** only assert a documented default is *wrong* when you have **read the source** and seen the
  field initializer — or confirmed there is none (a struct/auto-property field with no initializer is
  `0`/`null`/`false`; recurring trap: `SKDocumentXpsOptions.Dpi` is 0, not 72). **If you cannot read the
  source, do not assume `0`** — a stated default you can't verify is `UNVERIFIED`, not a finding.
  Inventing "should be 0" for a default you never checked in source is a false positive.
- **Data-format claims** (bit layouts, channel order, byte order): verify against `skia-patterns.md` and
  the native header if present; check the bit math adds up.
- **Standard-citing enums:** the member name encodes the standard — verify the number AND the behavior
  (RP 431-2 ≠ "432-2"; ST 428-1 transfer is gamma ~2.6, not "linear"). Provide header path + line.
- **Cross-library:** SkiaSharp (Skia/C++) and HarfBuzzSharp (HarfBuzz/C) have different conventions —
  never assume one behaves like the other.
- **Trust hierarchy for native facts:** native header in repo > `skia-patterns.md` > your own knowledge
  (never use the last for byte layouts). Cannot find the header → flag `UNVERIFIED`, not wrong.

Classes: `factual` (a stated fact contradicts source), `fabricated-member` (docs/example reference a
type, member, or overload that does **not** exist in source).

### B. Examples — compile, real APIs, no obsolete members

For every ```` ```csharp ```` block in `<remarks>` CDATA, extract each constructor call, method call, and
property access, and:

- `grep` the C# source in `binding/` to confirm it exists **with that exact signature/overload**, and
  that the overload accepts those argument types.
- Flag C# reserved words used as identifiers (`override`, `base`, `event`, `class`, `struct`, …).
- Null safety: if a call returns nullable (`SKData?`, `SKTypeface.FromFamilyName`), is null handled
  before use?
- **Obsolete check:** match every member against [`obsolete-api-map.md`](obsolete-api-map.md) AND grep
  its source for `[Obsolete(...)]`. A member from the map's §1 (or any `[Obsolete(..., error: true)]`
  member) in an example is CRITICAL — it won't compile; a soft-obsolete (`[Obsolete]` warning-only)
  member is IMPORTANT.
  - **Disambiguate overloads — same name ≠ obsolete.** Text methods like `DrawText`/`MeasureText` exist
    in both an obsolete and a modern form (map §2). Judge by *receiver and signature*, not by name: the
    deprecated forms are on `SKPaint` or are the `SKCanvas` overloads with no `SKFont` argument; the modern
    forms are on `SKFont` or take an `SKFont`. `font.MeasureText(...)` and
    `canvas.DrawText(..., SKTextAlign, font, paint)` are **correct** — never rewrite a valid modern call.
    Only flag `paint.MeasureText(...)` and `canvas.DrawText(string, x, y, paint)`.
- Self-contained: every identifier referenced must be declared in the snippet (using `bitmap2` when only
  `bitmap` was declared is a compile error).
- Ownership: never `using`/`Dispose` a parent-owned object (the canvas from `SKDocument.BeginPage` and
  `SKSurface.Canvas` are parent-owned).

Class: `example`.

### C. Quality — .NET conventions, completeness, style

- Remaining placeholders (`To be added.`, TODO, bracketed remarks scaffolds like `[Describe …]`).
- Type-level `<remarks>` has real content, not a template blank.
- Summaries add value beyond restating the member name.
- `<see cref>` uses the correct prefix (`T:`/`M:`/`P:`/`F:`); CDATA `<xref:…>` uses the **bare UID, no
  prefix** (`<xref:SkiaSharp.SKPath>` correct; `<xref:T:SkiaSharp.SKPath>` broken).
- Constructor summaries use the full "Initializes a new instance of the `<see cref>` class" (or "struct"
  for value types).
- Property summaries match the accessor verb in the signature.
- Boolean wording: parameters "true to…"; returns and property `<value>` "true if…".
- Nullable params use `<see langword="null" />`, not "default".
- Remarks make no false cross-type comparisons ("Unlike X, which is immutable…" — verify first).
- Empty tags (`<summary />`, `<value />`, `<returns />`) that should have content (`<remarks />` is OK).
- Spelling and repeated words — the linter also catches these, so only add ones it would miss (domain-word
  misuse). Do not flag a domain fact that matches `skia-patterns.md`.

Class: `quality`.

## Output

One finding per line, machine-parseable:

```
SEVERITY | class | <file> | <docId> | <message — what it says vs what source says (path:line)>
```

`SEVERITY` ∈ `CRITICAL`/`IMPORTANT`/`MINOR` per `checklist.md`. Then a per-file trace proving you read
source (so a skim is detectable):

```
TRACE | <file> | source:<binding/...cs lines a-b or NONE> | checked:<n> | issues:<n>
```

After deduplication, write a single Markdown report plus the machine block (one line per deduped finding)
to `output/docs-review/` (gitignored). Nothing in `docs/` changes unless the gated fix step runs.

```
# Review report — <scope> (<n> files)

## Summary
- Files reviewed: <n>   Findings: CRITICAL <n>, IMPORTANT <n>, MINOR <n>
- Coverage gaps: <files with no source read, or "none">
- Assessment: Ready for release / Needs fixes / Major issues

## CRITICAL / IMPORTANT / MINOR
- `<file>` · `<docId>` — <message> → <fix>

## Extra findings (uncorroborated leads)
...

FINDING | <severity> | <class> | <file> | <docId> | <message>
```

## Boundaries

- Report only by default — fixing is a separate, gated step.
- Cite a source `path:line` for every factual contradiction. No citation → not a finding.
- Never flag a domain fact that matches `skia-patterns.md` — the reference is pre-verified.
- Never invent a finding; never downgrade a CRITICAL to make a report look cleaner.
