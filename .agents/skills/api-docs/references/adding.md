# Adding docs for new APIs

Use this when new APIs have shipped and their doc files contain `To be added.` placeholders (typically
triggered by the daily `auto-api-docs-writer` workflow after a NuGet/CI update). This is the **direct-XML**
pipeline — no extract/merge JSON round-trip.

You run this yourself, end to end — one agent, no sub-agent fan-out. Accuracy comes from reading the C#
source first; prose comes from the .NET conventions in the reference files. You write for developers who
copy your examples into real code, so every claim must be true and every example must compile.

## Required reading (first)

1. [`patterns.md`](patterns.md) — .NET XML doc syntax, verb conventions, summary/param/return patterns.
2. [`skia-patterns.md`](skia-patterns.md) — SkiaSharp/HarfBuzz domain facts (color layouts, struct
   defaults, standard-based enums, caller-owned vs parent-owned).
3. [`obsolete-api-map.md`](obsolete-api-map.md) — members that must never appear in an example, and
   replacements.

Apply these facts; do not restate them in the docs. If a fact is in a reference file, trust it over your
own recollection.

## Procedure

1. **Regenerate stubs (only if new APIs were added).** Skip if you are editing existing docs or the
   automated workflow already ran this as a pre-step.
   ```bash
   dotnet tool restore
   dotnet cake --target=docs-download-output   # latest NuGets from the CI feed
   dotnet cake --target=update-docs            # mdoc update + format → "To be added." placeholders
   ```

2. **Resolve scope** (see [`scope-resolution.md`](scope-resolution.md)). New members are easiest to target
   with the `new` selector:
   ```bash
   pwsh .agents/skills/api-docs/scripts/docs-tool.ps1 resolve-scope new
   ```
   Shard the result into ~25–40-file batches.

3. **Write (per file).** A field is **in scope to fill** when it is empty, self-closing, or still a
   placeholder (`To be added.`, or a bracketed remarks scaffold like `[Describe …]`). Do not rewrite
   already-written prose.
   1. **Read the C# source first.** From the filename, locate the type in `binding/` and read it. Build a
      fact sheet: constructors, method overloads, property accessors (`{ get; }` vs `{ get; set; }`),
      validation behavior (throws? clamps? pads? truncates?), numeric constants, and defaults. The source
      is authoritative — never document from the member name alone.
   2. **Open the `.xml` and locate each `<Docs>` block.** Each `<Member>`/type carries a
      `MemberSignature[@Language='DocId']` you use as the stable id. Fill the in-scope children:
      `<summary>`, `<param>`, `<returns>`, `<value>`, `<typeparam>`, `<exception>`, and `<remarks>`.
   3. **Match the accessor verb to the signature**, not to intuition: `{ get; set; }` → "Gets or sets …",
      `{ get; }` → "Gets …". Many struct properties look read-only but are settable — check the signature.
   4. **Defaults come from the source.** A struct property with no field initializer defaults to
      `0`/`null`/`false`; do not copy a "typical" sibling constant.
   5. **Standard-citing enum members:** read the C/C++ header where the enum is defined and verify the
      number AND the behavior against the member name.
   6. **Remarks:** type-level entries get a real `<remarks>` (description + disposal note if applicable +
      one compiling example). Simple members get self-closing `<remarks />`. Inside CDATA remarks use
      `<xref:Bare.Uid>` with **no** `T:`/`M:`/`P:` prefix; `<see cref>` (with prefix) is for non-CDATA prose.
   7. **Examples must compile and be self-contained:** declare every variable; never use an obsolete member
      (check the obsolete map); never `using`/`Dispose` a parent-owned object (e.g. `SKSurface.Canvas`).
   8. **Save the file**, preserving CDATA and all signature elements. Change only `<Docs>` content.

4. **Review** the files just written with the review checks ([`reviewing.md`](reviewing.md) §Checks), then
   fix CRITICAL findings by editing the XML directly.

5. **Validate & format** ([`validation.md`](validation.md)): structural validator → linter →
   `docs-format-docs`.

6. **Land:** commit on a `dev/...` branch in the `docs` submodule and open a PR (the submodule protects
   `main`).

## Output

After all files, emit a compact manifest — one line per file:

```
WROTE | <file> | summaries:<n> params:<n> returns:<n> remarks:<n> | source:<binding path or NONE>
```

Then list any field you intentionally left as a placeholder (ran out of certainty/time) so the next run
re-detects it:

```
DEFERRED | <file> | <docId> | <field> | <reason>
```

## Boundaries

- Edit only the in-scope `.xml` files, and only `<Docs>` content — never touch `MemberSignature`,
  `TypeSignature`, or generated files (`index.xml`, `ns-*.xml`, `_filter.xml`, `FrameworksIndex/`).
- Never invent an API, overload, or numeric value. If you cannot verify it, leave the field deferred.
- The writer only fills in-scope (empty/placeholder) fields; it does not rewrite existing prose.
- If a large type runs out of certainty, leave its placeholder intact (`DEFERRED`) so the next run
  re-detects it — the file stays clean and well-formed either way.
