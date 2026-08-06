# Validation & formatting

Agents edit the `.xml` directly. **One** Cake target does both the formatting and the deterministic,
no-LLM checks — run it before opening a PR:

```bash
dotnet cake --target=docs-format-docs
```

It walks **every** type file in `docs/`, normalizes whitespace/attribute order (so diffs stay minimal and
reviewable), and runs the content checks below. The same target also runs during full doc regeneration, so
these checks guard regenerated stubs too.

The target is a structural/deterministic gate, not proof that the prose is technically correct. A run can
report zero deterministic findings while docs still invent native behavior, omit a nullable callback
failure, or refer to a private identifier. Complete the source-backed authoring/review pass as well.

## What it reports (warnings — never fail the build)

Findings use the shared contract, logged as Cake **warnings**:

```
[docs] <class> | <file> | <docId> | <message>
```

- **Missing docs** — any `To be added.` placeholder left unfilled (also summarized per type as a
  "Docs missing on …" warning).
- `<see cref>` with a missing/wrong DocId prefix; `<xref:T:/M:/P:…>` **inside CDATA** (the prefix is only
  for `<see cref>`).
- Empty `<summary/>` / `<value/>` / `<returns/>` (note `<remarks/>` is allowed).
- Accessor-verb mismatch vs the `MemberSignature` (`{ get; }` documented as "Gets or sets").
- Repeated words ("the the") and common misspellings (dictionary, not an LLM).

> **Not checked here:** obsolete members in examples. Distinguishing an obsolete overload from a modern one
> (e.g. `SKCanvas.DrawText` with vs without `SKFont`) needs signature awareness a name match can't do, so
> that is a reviewer judgement — see [`obsolete-api-map.md`](obsolete-api-map.md) and `reviewing.md` Check B.

These are advisory: a fresh regen full of placeholders is just noisy, not broken.

For each touched type file, separately search for `To be added.`, TODO, empty required tags, and bracketed
scaffolds. Every remaining in-scope field must be filled or listed as `DEFERRED`; do not call a file or type
complete merely because the Cake target exits successfully.

Before landing, also reject:

- A BCL DocId incorrectly nested under SkiaSharp, such as `T:SkiaSharp.System.*`.
- Any unresolved `cref`, including a plausible framework target that is absent from the target framework
  reference set.
- A `<Docs>` block whose prose describes another member, or whose shape contradicts its own signature
  (`<returns>` on `void`, `<value>` on a method, missing parameter docs, or a mismatched DocId).
- A `WROTE` row without real managed source ranges, final member/exception counts, or the expected number
  of `NATIVE` rows.
- A native-backed status, ownership, callback, backend, or lifetime claim without a pinned native
  path-and-line citation.
- A zero-finding report that has not reconciled deterministic exceptions for every touched member.
- Selection accounting where the frozen selected DocIds differ from `EVIDENCE`, per-file `TRACE checked:`
  totals; where `WROTE` differs from the final semantic diff; or where `UNSELECTED` uses a wildcard or
  approximate count instead of one exact row per file.

These completion checks depend on the evidence block and source audit; `docs-format-docs` does not perform
them. Treat missing evidence or inconsistent counts as validation failure, not as a warning. They do not
replace human/source review of whether a claim or exception list is complete.

## What FAILS the build (errors)

Two things stop a doc from parsing or rendering on the published Learn site, so both fail the target:

- **Unparseable XML** — `docs-format-docs` loads every file with `XDocument.Load`; a file that is not
  well-formed throws immediately (with its name) and aborts the run before anything else is checked.
- `broken-cdata` — a `csharp`/`xref` CDATA block was destroyed (e.g. `<` escaped to `&lt;xref:`), which
  silently corrupts the rendered remarks. Logged as a `[docs] broken-cdata` **error**.

Fix any parse failure and every `[docs] broken-cdata` error before landing — the build will not pass
otherwise. This is the guarantee that a direct XML edit cannot ship site-breaking markup.

> There is no separate `docs-lint`/`docs-validate` target and no git-baseline "only `<Docs>` changed"
> structural check anymore. The checks run in the same pass that formats each file, on the tree it already
> loaded. Signatures are owned by mdoc regeneration and are visible in the PR diff; the broken-XML failures
> above are what keep the site safe.

## Diff boundary

- Without stub regeneration, hand edits belong only in `<Docs>` content; non-doc signature/assembly changes
  are unexpected.
- With stub regeneration, preserve mdoc's generated structural changes, but do not hand-edit them. Report
  generated-only files separately from files whose `<Docs>` content you authored.
- Never stage `index.xml`, `ns-*.xml`, `_filter.xml`, or `FrameworksIndex/`.
