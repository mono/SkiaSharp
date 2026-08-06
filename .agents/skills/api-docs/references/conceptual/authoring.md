# Authoring conceptual articles

Use this procedure for a new article or a major rewrite. Read the matching blueprint from
[`index.md`](index.md), then load [`../technical-fact-checking.md`](../technical-fact-checking.md),
[`fact-checking.md`](fact-checking.md),
[`structure-and-style.md`](structure-and-style.md), and [`code-samples.md`](code-samples.md) when the
article contains code.

## 1. Define the reader contract

Write one planning sentence before drafting:

```text
For <reader> who already has <starting state>, this <article type> helps them <outcome>, within <scope>.
```

Then record:

- The prerequisite knowledge, packages, workloads, devices, graphics contexts, or permissions.
- The observable completion condition.
- The important exclusions. Link elsewhere instead of growing a second task inside the article.
- The supported versions, platforms, and backends when the outcome is not universal.

If the sentence contains multiple unrelated outcomes, split the article or select one dominant outcome.

## 2. Build an evidence ledger

Before writing technical prose, follow the shared [`technical-fact-checking.md`](../technical-fact-checking.md)
contract and the conceptual claim guidance in [`fact-checking.md`](fact-checking.md). Record each
consequential claim, its evidence, and its status:

```text
CLAIM | <claim> | <source path:line or first-party URL> | VERIFIED / QUALIFIED / UNVERIFIED
```

Include API signatures, return/failure behavior, defaults, ownership, callback lifetime, threading,
platform/backend support, and external setup. Do not draft a warning from an unverified assumption.
Represent a platform matrix as exact backend/platform tuples; do not merge backends whose verified target
sets differ.

## 3. Design the reader journey

Start from the matching blueprint. Preserve its reader logic, not its placeholder headings.

- Put prerequisites and decision-changing limitations before the first dependent step.
- Give the reader enough context to understand *why* an action is required, but keep reference detail
  out of the task flow.
- For procedures, order actions exactly as the reader performs them and include the expected result.
- For branching paths, explain the choice once, then separate the paths with specific headings.
- End with verification and only the next links needed to continue.

Update the section TOC when adding, moving, or renaming an article.

## 4. Draft the introduction

The opening should answer, in a short paragraph:

1. What can the reader accomplish?
2. When should they use this approach?
3. What constraint most affects success or choice?

Do not repeat the title, begin with product history, or spend the first screen defining terms that the
reader does not yet need.

## 5. Write source-backed examples

Apply [`code-samples.md`](code-samples.md). In particular:

- Verify every SkiaSharp member and overload in current source.
- Declare what host-specific values the reader must supply.
- Emit the shared `RESOURCE` and `RESULT` ledgers, and check every meaningful nullable factory,
  `bool`, or status result before dependent use, including later operations such as encode, submit, flush,
  insert, and readback. Reconcile one `RESULT` row per call-site occurrence against the final code; repeated
  calls to the same method each need their own check.
- Dispose caller-owned native wrappers and keep parent-owned objects alive.
- Model pinning, callbacks, asynchronous work, and GPU cleanup for their complete lifetimes.
- Pair intentionally incorrect code with an explicit explanation and a corrected version.

An illustrative fragment must say what it omits. A complete workflow must compile and reach the stated
result.

## 6. Apply editorial and accessibility passes

Use [`structure-and-style.md`](structure-and-style.md) as separate passes rather than trying to fix
everything while drafting:

1. Metadata, title, introduction, and heading hierarchy.
2. Procedure order and scannability.
3. Voice, terminology, global readiness, and inclusive language.
4. Links, xrefs, alerts, formatting, and image accessibility.

Separate passes catch inconsistencies that disappear when prose and code are reviewed together.

## 7. Validate and self-review

Run [`validation.md`](validation.md), then review the article against its blueprint:

- Does the opening promise the same outcome the article delivers?
- Can the intended reader complete or verify that outcome?
- Does every limitation include a recovery path, supported alternative, or explicit boundary?
- Are technical claims still supported by the evidence ledger?
- Are all changed links, heading fragments, and TOC entries valid?

Inspect the source-backed claim ledger, final Markdown diff, built page, and sample results together.
Mechanically compare platform/backend tuples with their evidence and reconcile reported rendered counts
(headings, tables, code blocks, alerts) with the retained rendered artifact rather than counting from
memory. Generate the counts from the artifact and verify that the prose label and enumerated items agree
before reporting them.
Correct every factual, structural, navigation, ownership, or sample defect and repeat the validation and
self-review pass until the selected article wave is trustworthy. A green DocFX build proves structure,
not that a reader can safely complete the task.

## Output

After editing, report:

```text
WROTE | <file> | type:<type> | outcome:<short outcome>
VERIFIED | <claim> | <source path:line or URL>
QUALIFIED | <claim> | <condition> | <source path:line or URL>
DEFERRED | <unverified claim or section> | <reason>
VALIDATED | docfx:<pass/fail> snippets:<pass/fail/not-run> rendered:<checked/not-checked>
```

`DEFERRED` is the output form of an `UNVERIFIED` ledger entry that could not be resolved or safely
removed. Do not hide unresolved claims behind fluent prose; leave them qualified or deferred so review
can focus on the remaining risk.
