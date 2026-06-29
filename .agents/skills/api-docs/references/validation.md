# Validation & formatting

Agents edit the `.xml` directly. **One** Cake target does both the formatting and the deterministic,
no-LLM checks — run it before opening a PR:

```bash
dotnet cake --target=docs-format-docs
```

It walks **every** type file in `docs/`, normalizes whitespace/attribute order (so diffs stay minimal and
reviewable), and runs the content checks below. The same target also runs during full doc regeneration, so
these checks guard regenerated stubs too.

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
- **Obsolete members used in a `csharp` fence**, matched against [`obsolete-api-map.md`](obsolete-api-map.md).
- Repeated words ("the the") and common misspellings (dictionary, not an LLM).

These are advisory: a fresh regen full of placeholders is just noisy, not broken.

## What FAILS the build (errors)

Two classes mean the XML itself is broken and would break the published Learn site. They are logged as
**errors** and make the target exit non-zero:

- `malformed-xml` — the file will not parse.
- `broken-cdata` — a `csharp`/`xref` CDATA block was destroyed (e.g. `<` escaped to `&lt;xref:`), which
  silently corrupts the rendered remarks.

Fix every `[docs] malformed-xml` / `[docs] broken-cdata` error before landing — the build will not pass
otherwise. This is the guarantee that a direct XML edit cannot ship site-breaking markup.

> There is no separate `docs-lint`/`docs-validate` target and no git-baseline "only `<Docs>` changed"
> structural check anymore. Signatures are owned by mdoc regeneration and are visible in the PR diff; the
> broken-XML error above is what keeps the site safe.
