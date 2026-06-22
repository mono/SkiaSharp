---
name: api-docs
description: >
  Write and review XML API documentation for SkiaSharp following .NET guidelines.
  
  Triggers: "document class", "add XML docs", "write XML documentation", "add triple-slash comments",
  "review documentation quality", "check docs for errors", "fix doc issues", "fill in missing docs",
  "remove To be added placeholders", API documentation requests.
---

# API Documentation

Write and review XML API documentation for SkiaSharp.

## Key Concepts

- The `docs/` directory is a **Git submodule** pointing to [`mono/SkiaSharp-API-docs`](https://github.com/mono/SkiaSharp-API-docs)
- XML files are **generated from NuGet assemblies** using `mdoc` — if new APIs were added, run Phase 1 before editing
- For existing APIs, you can edit the XML files directly without regenerating (start at Phase 2)

## File Locations

```
docs/SkiaSharpAPI/
├── SkiaSharp/              # Main namespace
│   ├── SKCanvas.xml        # One XML file per type
│   ├── SKPaint.xml
│   ├── SKImage.xml
│   └── ...
├── HarfBuzzSharp/          # HarfBuzz namespace
├── SkiaSharp.Views.*/      # Platform-specific views
└── index.xml               # Extension methods (auto-synced, don't edit)
```

## Phases

### Phase 1: Regenerate Stubs (optional)

When new APIs have been added, XML doc files must be regenerated before documenting them.

```bash
dotnet tool restore
dotnet cake --target=docs-download-output   # Download latest NuGets from CI feed
dotnet cake --target=update-docs            # Regenerate XML docs (mdoc update + format)
```

New members appear with "To be added." placeholders. **Skip this phase** if:
- You're editing existing docs (no new APIs)
- The automated workflow already ran stub generation as a pre-agent step

### Phase 2: Extract

Extract placeholders to JSON using the docs tool:
```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 extract docs/SkiaSharpAPI/ -Output output/docs-work/
```
This produces one JSON file per XML file, containing only members with "To be added." placeholders. The `output/` directory is gitignored so local runs don't pollute the repo.

### Phase 3: Discover (Orchestrator)

This phase is lightweight — you are an **orchestrator**, not a writer. Read only what you need to plan the work, then delegate immediately.

1. **Read the manifest** — `output/docs-work/manifest.json` tells you how many files, types, and fields need documentation. Use this to plan the write phase.

2. **Read the reference docs** — these will be passed to writer agents:
   - [references/patterns.md](references/patterns.md) — .NET XML documentation formatting rules
   - [references/skia-patterns.md](references/skia-patterns.md) — SkiaSharp/HarfBuzz domain knowledge

3. **Do NOT pre-read JSON files or source code** — the writer agents will do their own discovery. Pre-reading is wasted work that duplicates what writers do.

### Phase 4: Write (1 Agent)

Launch **one** background `general-purpose` agent to write documentation for all files in `manifest.json`.

**Writer agent prompt:**

```
You are an API DOCUMENTATION WRITER. Fill all placeholder fields in the JSON
files with accurate, well-written .NET XML documentation.

STEPS:
1. Read the patterns file at .agents/skills/api-docs/references/patterns.md
2. Read the domain knowledge at .agents/skills/api-docs/references/skia-patterns.md
3. For each JSON file listed in output/docs-work/manifest.json:
   a. Read the JSON file
   b. Find and READ the corresponding C# source in binding/ (ESSENTIAL for accuracy)
   c. Fill all "To be added." fields following the rules below
   d. Write the completed JSON back to the same path

JSON INTEGRITY RULES (NON-NEGOTIABLE):
- NEVER add new keys to the JSON that are not already present in the file
- NEVER add new entries (members) to the JSON that were not extracted
- Only modify values of EXISTING keys — the extract tool determines scope
- The "_extractedKeys" array in each entry lists exactly which fields you may fill
- If a member has only "remarks" extracted, fill ONLY "remarks" — do NOT add
  "summary", "params", or other fields even if you think they could be improved

WRITING RULES:
- NEVER invent API calls — verify every method/overload exists by reading C# source
- NEVER guess numeric values — read MemberValue from JSON, cross-reference source
- Fill ALL params in ALL overloads — each overload independently complete
- Accessor verb comes from the entry's "signature" field, not from intuition:
  "{ get; set; }" → "Gets or sets ..."; "{ get; }" → "Gets ...". The signature is
  already in the JSON — read it, don't guess from the member's purpose. (Many struct
  properties look read-only but are actually settable, e.g. SKFontVariationAxis.)
- Constructor summaries MUST use the exact .NET phrase: "Initializes a new instance of
  the <see cref="T:..." /> class." Use "struct" instead of "class" when the type's
  "signature" field says "struct" (e.g. "public readonly struct ..."). Do not shorten
  to "Initializes a new <see cref> from ..." — keep "Initializes a new instance of the".
- Use <see langword="null" /> not <see langword="default" /> for nullable params
- Use <see langword="true" /> and <see langword="false" /> for boolean literals in prose
- Struct property defaults are whatever zero-init produces (0/null/false) UNLESS the
  source has a field initializer. Do NOT copy a "default" from a sibling constant
  (e.g. SKDocumentXpsOptions.Dpi defaults to 0, NOT to SKDocument.DefaultRasterDpi = 72).
- Code examples must be SELF-CONTAINED and compile: declare every variable you reference,
  and never put `using`/Dispose on a parent-owned object (the canvas from
  SKDocument.BeginPage and SKSurface.Canvas are owned by their parent). See
  skia-patterns.md "Caller-owned vs parent-owned".

OBSOLETE MEMBERS (avoid broken examples):
- Entries with an "obsolete" field are deprecated. When the text contains "true"
  (i.e. [Obsolete("...", true)]) the member is a COMPILE ERROR if used — examples that
  call it will not build. Never use an obsolete member in a code example, and never
  recommend it as the way to do something.
- The obsolete message names the replacement — use it. The most common SkiaSharp trap:
  text rendering moved from SKPaint to SKFont. See skia-patterns.md "Obsolete APIs".
- You still must document the obsolete member's own summary/value when it is in scope —
  just describe it factually; the [Obsolete] attribute already carries the warning.

STANDARD VALUES (CICP, Vulkan, OpenType, etc.):
- For enum members referencing external standards, you MUST read the C/C++ header
  in externals/skia/ where the enum is defined and verify numeric values
- Do NOT rely on your own knowledge of standards for specific numeric values
  (gamma values, bit depths, transfer function parameters, etc.)
- The MEMBER NAME usually encodes the exact standard (SmpteRp4312 = SMPTE RP 431-2;
  SmpteSt4281 = SMPTE ST 428-1). Match the summary's standard number AND its described
  behavior to the name: RP 431-2 is DCI-P3 (not "432-2"), and ST 428-1 is a gamma-2.6
  transfer (not "linear" — a separate Linear member exists). See skia-patterns.md
  "Standard-Based Enums".
- If you cannot find the header, state "value from standard" without inventing
  specific numbers (e.g., say "assumed display gamma" not "assumed gamma 2.2")

REMARKS RULES:
- Type-level entries (memberType=type) have remarksRequired=true with a CDATA template.
  Complete it fully — never leave [bracketed placeholders]. Include description,
  disposal note if applicable, and a code example.
- Member-level: set remarks to "" for simple members, or write rich markdown for
  important methods (factory methods, Draw*, etc.)

CONTENT FORMAT:
- XML refs: <see cref="T:..." />, <paramref name="..." />, <see langword="null" />
- In CDATA remarks: use <xref:...> instead of <see cref>. The DocId prefix (T:/M:/P:)
  belongs ONLY to <see cref>. An xref uses the bare DocFX UID with NO prefix:
    ✅ <xref:SkiaSharp.SKPath>   ✅ <xref:SkiaSharp.SKPathBuilder.MoveTo(System.Single,System.Single)>
    ❌ <xref:T:SkiaSharp.SKPath> ❌ <xref:P:SkiaSharp.SKPaint.Color>
  A T:/M:/P: prefix inside an xref produces a broken link.
- Set remarks to "" for self-closing <remarks />

TRUST HIERARCHY for native type facts (bit layouts, byte orders):
1. Native C/C++ header in repo (if you can find and read it) — AUTHORITATIVE
2. skia-patterns.md reference file — PRE-VERIFIED, trust it
3. Your own knowledge — DO NOT USE for byte layouts. Never invent macro expansions.

Do all work directly. Do NOT launch sub-agents or delegate.
```

Wait for the writer agent to complete before proceeding to Phase 5.

### Phase 5: Review (3 Independent Agents)

Launch **three** background `general-purpose` agents in parallel to independently validate the documentation. Each reviewer reads the source code fresh — they did NOT write the docs and have no blind spots from the writing process.

> **IMPORTANT — adversarial mindset:** AI documentation writers typically introduce
> 15–30 factual errors per batch of ~800 fields. A review that finds 0 issues
> almost certainly skimmed. Each agent MUST read source code and provide evidence.

**Agent 1: Factual Claim Verifier** — cross-references every factual claim against source:

```
You are a FACTUAL CLAIM VERIFIER. Your ONLY job is to find documentation claims
that contradict the actual source code.

ADVERSARIAL CONTEXT: AI doc writers confidently state "facts" without checking
source. Common errors include: wrong parameter constraints, wrong channel names,
wrong byte layouts, and invented API overloads. Assume errors exist.

Do all work directly. Do NOT launch sub-agents or delegate.

FIRST: Read .agents/skills/api-docs/references/skia-patterns.md — this is your
reference for domain-specific facts (byte layouts, naming conventions, type
categories). You will use this to verify claims about native types.

SOURCE-FIRST PROTOCOL — for each JSON file in output/docs-work/:
1. Identify the type name from the filename
2. Find and READ the corresponding C# source file in binding/ BEFORE reading the JSON
3. Build a fact sheet: constructors, methods, property accessors (get vs get+set),
   validation logic (throws? clamps? pads? truncates?), numeric constants, defaults
4. NOW read the JSON and compare every claim against your fact sheet

SPECIFIC CHECKS:
- Parameter constraints ("exactly N", "must be", "cannot be"): does the source
  actually validate/reject, or silently accept? Read the METHOD BODY, not just signature.
- Data format claims (bit layouts, channel names, byte orders): verify against
  skia-patterns.md and the native C/C++ header if available in the repo.
- Default value claims: find the actual default in source. A struct property with NO
  field initializer defaults to 0/null/false — "default 72" for a struct DPI is wrong
  (72 is SKDocument.DefaultRasterDpi, a sibling constant used by scalar overloads, not
  the struct's default). Recurring trap: SKDocumentXpsOptions.Dpi.
- "Gets or sets" vs "Gets": the entry's "signature" field shows the accessor verbatim
  ({ get; } vs { get; set; }) — compare the verb against it. Don't trust the prose alone.
- Cross-library: SkiaSharp and HarfBuzzSharp are DIFFERENT libraries with different conventions.

STANDARD VALUE VERIFICATION (CRITICAL):
- For enum members that cite external standards (CICP, ITU-T, Vulkan, OpenType),
  you MUST locate and read the C/C++ header where the enum is defined (check
  externals/skia/include/ or binding/ generated files)
- Cross-reference the MemberValue in JSON against the enum constant in source
- The MEMBER NAME usually encodes the standard (SmpteRp4312 = SMPTE RP 431-2;
  SmpteSt4281 = SMPTE ST 428-1). Verify the summary's standard number AND its described
  behavior against the name: RP 431-2 ≠ "432-2"; ST 428-1 is gamma ~2.6, NOT "linear"
  (a separate Linear member exists for the actual linear transfer).
- If documentation claims a specific numeric property of a standard (e.g., "gamma 2.2",
  "10-bit precision", "64-bit packed"), verify it is consistent:
  - Does the bit math add up? (e.g., "64-bit with 10+10+10+10 packed" = only 40 bits → ERROR)
  - Does the gamma match the correct CICP value number?
- If you cannot find the header, flag the claim as UNVERIFIED rather than assuming correct
- Provide file path + line number for every standard value you verify

TRUST HIERARCHY for native type facts (bit layouts, byte orders, macro expansions):
1. Native C/C++ header in repo (if you can find and read it) — AUTHORITATIVE
2. skia-patterns.md reference file — PRE-VERIFIED, trust it if header unavailable
3. Your own knowledge — DO NOT USE for byte layouts. Never invent macro expansions.

If you cannot locate the native header for a type, you MUST defer to
skia-patterns.md. Do NOT "correct" a claim that matches skia-patterns.md
based on your own reasoning about how a macro "should" expand. The reference
file was verified against the actual source.

OUTPUT FORMAT — you MUST include a verification trace per file:
  [filename.json] SOURCE: binding/SkiaSharp/SKFoo.cs (read lines 1-85)
    Claim: "exactly four characters" (docId: M:SkiaSharp.SKFoo.Parse)
      → Source line 22: if (s.Length < 4) s = s.PadRight(4); → SILENTLY PADS
      → CRITICAL: should say "padded with spaces if shorter than 4 characters"
    Claim: "Gets or sets the width" (docId: P:SkiaSharp.SKFoo.Width)
      → Source line 45: public int Width { get; } → GET ONLY
      → CRITICAL: should say "Gets"
  ISSUES: [list]

A file review with NO source file reads is INCOMPLETE and will be rejected.
Finding 0 issues across all files should be rare — state your confidence level.
```

**Agent 2: Code Example Verifier** — verifies every code example uses real APIs:

```
You are a CODE EXAMPLE VERIFIER. Your ONLY job is to verify that every code
example in the documentation uses real APIs with correct signatures.

ADVERSARIAL CONTEXT: AI doc writers frequently invent method overloads, use
wrong parameter types, and write examples with C# syntax errors. In past runs,
we found examples using reserved keywords as variable names (var override = ...)
and constructing structs that no public API actually accepts. Assume errors exist.

Do all work directly. Do NOT launch sub-agents or delegate.

For each JSON file in output/docs-work/:
1. Find every code block in remarks fields (look for ```csharp in CDATA sections)
2. For each code block:
   a. Extract every method call, constructor call, and property access
   b. grep binding/ source code to verify it exists with that exact signature
   c. Check: does the overload accept those parameter types?
   d. Check for C# reserved keywords used as variable names:
      override, base, event, class, struct, delegate, abstract, virtual, etc.
   e. Check null safety: if a method returns nullable (SKData?), does the
      example handle null before using the result?
   f. Check for OBSOLETE APIs: grep the member's source for [Obsolete. A member marked
      [Obsolete("...", true)] is a COMPILE ERROR and must never appear in an example.
      The "obsolete" field in the JSON entry also flags these. Watch especially for the
      legacy text API: SKPaint.TextSize/TextScaleX/Typeface/TextAlign and the
      SKCanvas.DrawText(string,float,float,SKPaint) overload are obsolete-error — the
      modern form uses SKFont (see skia-patterns.md "Obsolete APIs").
   g. Check the example is SELF-CONTAINED: every variable/identifier referenced must be
      declared within the snippet. An undefined local (e.g. using `bitmap2` when only
      `bitmap` was created) is a compile error. This is easy to miss in multi-statement
      examples — track each identifier back to its declaration.
   h. Check disposal OWNERSHIP: never `using`/Dispose an object owned by a parent. The
      canvas from SKDocument.BeginPage and SKSurface.Canvas are parent-owned (owns:false);
      wrapping them in `using` is wrong (see skia-patterns.md "Caller-owned vs parent-owned").

OUTPUT FORMAT — you MUST use this structure for each file:
  [filename.json] CHECKED N code examples
    Example 1 (docId): "var surface = SKSurface.Create(...)"
      → grep binding/SkiaSharp/SKSurface.cs: found Create(SKImageInfo) at line 42 ✓
      → variable names OK ✓
    Example 2 (docId): "paint.TextSize = 24f"
      → CRITICAL: TextSize is [Obsolete(..., true)] — won't compile; use font.Size
  ISSUES: [list] or NONE

A review that checks 0 code examples is INCOMPLETE. Finding 0 issues across
all files should be rare — state your confidence level if you find nothing.
```

**Agent 3: Quality Reviewer** — catches style, completeness, and naming issues:

```
You are a documentation QUALITY REVIEWER. Read the checklist at
.agents/skills/api-docs/references/checklist.md, the patterns at
.agents/skills/api-docs/references/patterns.md, and the domain knowledge at
.agents/skills/api-docs/references/skia-patterns.md for guidelines.

Do all work directly. Do NOT launch sub-agents or delegate.

For each JSON file in output/docs-work/:
1. Check for remaining "To be added." values that should have been filled
2. Check type-level entries (memberType=type) have real remarks content,
   not template placeholders like [Describe...] or [Show...]
3. Check summaries add value beyond just restating the member name
4. Check <see cref> references use correct prefix (T: M: P: F:)
5. Check CDATA <xref:...> links use NO DocId prefix — <xref:SkiaSharp.SKPath> is correct,
   <xref:T:SkiaSharp.SKPath> / <xref:M:...> / <xref:P:...> are broken (prefix is for <see cref> only)
6. Check constructor summaries use the full phrase "Initializes a new instance of the
   <see cref> class" (or "struct" for value types) — not a shortened "Initializes a new <see cref>"
7. Check property summaries match the accessor in the entry's "signature" field:
   "Gets or sets" for { get; set; }, "Gets" for { get; }
8. Check boolean wording: parameters use "true to...", while returns AND property
   `<value>` use "true if..." (a property value describes a state, like a return)
9. Check nullable params use <see langword="null" /> not "default"
10. Check remarks don't make false comparisons with other types
    (e.g., "Unlike X, which is immutable" — verify before accepting)
11. Check enum member descriptions accurately describe the member's
    specific value, not a similar-looking sibling enum member. For standard-based enums,
    the member NAME encodes the standard (SmpteRp4312 = SMPTE RP 431-2; SmpteSt4281 =
    SMPTE ST 428-1) — verify the summary's standard number AND behavior match it
    (RP 431-2 not "432-2"; ST 428-1 is gamma 2.6, not "linear"). See skia-patterns.md
    "Standard-Based Enums".
12. Check domain facts against skia-patterns.md (naming conventions, byte layouts,
    type categories). If documentation matches the reference, it is correct.
13. For native byte layout claims, compare against skia-patterns.md. If the
    documentation matches the reference file, it is CORRECT — do not override.
    Never invent C macro expansions to "disprove" the reference.

Report CRITICAL and IMPORTANT issues only. Include file, docId, and fix.
```

**After all 3 reviewers complete:** Fix all CRITICAL issues reported by any reviewer. Edit the JSON files directly to correct each issue. Then proceed to Phase 6.

### Phase 6: Merge

After fixing all CRITICAL review issues, merge the JSON back into XML:

```bash
pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 merge output/docs-work/
```

The merge tool has built-in safety checks:
- **Signature preservation** — counts `MemberSignature` and `TypeSignature` elements before and after merging each file and aborts with a fatal error if any were lost
- **Extract guard** — rejects any field not listed in `_extractedKeys` (prevents agents from overwriting existing documentation with "improved" versions). Rejected fields emit a warning.
- **XML validation** — validates the output XML is well-formed after save

Then run formatting to clean up:

```bash
dotnet cake --target=docs-format-docs
```

## Resources

- [references/patterns.md](references/patterns.md) — .NET XML documentation syntax, verb conventions, and formatting
- [references/skia-patterns.md](references/skia-patterns.md) — SkiaSharp/HarfBuzz domain knowledge (color types, naming, threading)
- [references/checklist.md](references/checklist.md) — Review severity criteria (CRITICAL / IMPORTANT / MINOR)
- [documentation/dev/writing-docs.md](../../../documentation/dev/writing-docs.md) — Full pipeline docs, cake targets, local generation
