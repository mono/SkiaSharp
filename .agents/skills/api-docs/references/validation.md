# Workflow: Validation

Agents edit the `.xml` directly, so safety comes from two non-LLM gates that
run **after** any edit. Run both before opening a PR.

## 1. Deterministic linter (objective defects, no model)

```bash
dotnet cake --target=docs-lint --scope=<files-or-scope>
```

Catches the things that never need judgment:

- Malformed XML; unescaped `&`, `<`, `>`; mismatched/missing tags.
- `<see cref>` with a missing/wrong DocId prefix; `<xref:T:/M:/P:…>` **inside CDATA** (prefix is only for
  `<see cref>`).
- Empty `<summary/>` / `<value/>` / `<returns/>` (note `<remarks/>` is allowed).
- Accessor-verb mismatch vs the `MemberSignature` (`{ get; }` documented as "Gets or sets").
- **Obsolete members used in a `csharp` fence**, matched against `references/obsolete-api-map.md`.
- Repeated words ("the the") and spelling (dictionary/cspell with a Skia allowlist — not an LLM).

The linter emits the shared finding contract so its output merges with the reviewers':

```
SEVERITY | <class> | <file> | <docId> | <message>
```

## 2. Structural validator (edit-safety, no model)

```bash
dotnet cake --target=docs-validate --scope=<files>
```

Replaces the old `merge` guards now that edits are in place. For each changed file it asserts:

1. The XML is **well-formed**.
2. `MemberSignature` and `TypeSignature` element counts are **unchanged** vs the pre-edit version (no
   member added, lost, or renamed).
3. **Only `<Docs>` content changed** — signatures, attributes, and structure are byte-identical outside
   `<Docs>`.

Any violation fails the run. This is the guarantee that a direct XML edit cannot silently corrupt the
API surface.

## 3. Formatting

```bash
dotnet cake --target=docs-format-docs
```

Normalizes whitespace/attribute order so diffs stay minimal and reviewable.

## Order of operations

`docs-resolve-scope` → edit (writer or fix step) → **`docs-validate`** (structural) → **`docs-lint`**
(objective) → `docs-format-docs` → PR. Reviews that only *report* (no edits) skip the validate/format
steps and just run `docs-lint` alongside the LLM reviewers.
