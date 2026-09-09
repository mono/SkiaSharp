# Reviewing conceptual articles

Review conceptual documentation for reader success, technical correctness, safe examples, platform
accuracy, structure, accessibility, and maintenance risk. Review is report-only unless the user asks to
apply fixes.

Read [`../technical-fact-checking.md`](../technical-fact-checking.md),
[`fact-checking.md`](fact-checking.md), the matching article blueprint from [`index.md`](index.md), and
[`code-samples.md`](code-samples.md) when the scope contains code.

## Choose the review depth

This is a second axis after the top-level change-size routing in [`index.md`](index.md). Classify how
deeply the selected review scope needs to be examined:

| Review depth | Examples | Checks |
|---|---|---|
| Focused | Typo, one broken link, one factual correction | Verify the changed claim and its immediate context |
| Substantive | New article, changed task flow, platform matrix, complete sample | Run every review pass below |
| Draft | Incomplete structure or intentionally partial content | Focus on direction, missing evidence, and blockers before polishing |

Resolve the scope to an explicit file list. For a PR, use the parent repository diff; for a theme, include
the index/TOC and every article needed to evaluate the reader journey.

## Review passes

### 1. Reader outcome

- Identify the intended reader, starting state, and promised outcome from the article itself.
- Confirm the article type matches that intent.
- Check that prerequisites appear before dependent actions and that the completion condition is visible.
- Flag scope that combines independent tasks or omits a required step.

### 2. Technical facts

Work claim by claim using the shared [`technical-fact-checking.md`](../technical-fact-checking.md)
contract and [`fact-checking.md`](fact-checking.md):

- Verify signatures, overloads, nullability, defaults, validation, and result values in managed source.
- Verify ownership, disposal order, pinning, callbacks, and threading through wrappers, tests, and native
  contracts as needed.
- Verify every platform/backend tuple from its implementation or build configuration. Do not accept a
  combined row until the named backends have identical verified target sets.
- Qualify version-sensitive and external claims with current first-party evidence.

No source means `UNVERIFIED`, not "wrong."

### 3. Code and commands

Use [`code-samples.md`](code-samples.md):

- Determine whether each block claims to be a complete sample or an illustrative snippet.
- Confirm members, overloads, variables, imports, the shared `RESOURCE`/`RESULT` ledgers, every meaningful
  nullable/Boolean/status check, ownership, and lifetimes. Reconcile ledger rows to call-site occurrences,
  including repeated calls to the same method.
- Check that deliberately wrong code is unmistakably marked in prose and code.
- Confirm commands match the repository and target platform.
- Run or compile representative complete samples when practical.

### 4. Article-type structure

Compare the article to its blueprint:

- Overview: clear choice criteria, comparison, and routed next tasks.
- Concept: accurate mental model, relationships/lifecycle, constraints, and applied example.
- How-to: prerequisites, ordered procedure, expected results, and verification.
- Migration: supported starting/target states, mapping, before/after, what stays the same, and rollback or
  fallback.
- Troubleshooting: symptom-first opening, diagnosis, causes, resolution, verification, and escalation.

Flag a structural issue only when it makes the article harder to use; blueprints are reader models, not
mandatory boilerplate.

### 5. Editorial, inclusive language, and accessibility

Apply [`structure-and-style.md`](structure-and-style.md):

- One H1, specific sentence-case headings, concise metadata, and a result-oriented introduction.
- Active, direct prose with consistent terms and no unnecessary idioms or future tense.
- Inclusive language that does not assume gender, ability, expertise, or a preferred platform.
- Correct code/UI/new-term formatting.
- Descriptive links, exact-case xrefs, valid fragments, and first-party external sources.
- Sparse, correctly chosen alerts that are not stacked.
- Useful alt text and a text explanation for complex visuals; do not use screenshots to present code.

### 6. Maintenance and validation

- Check TOC placement and neighboring article links.
- Look for repeated facts that should link to a canonical article instead.
- Identify time-sensitive claims without a version or source.
- Run the applicable checks in [`validation.md`](validation.md).

## Severity

Use reader impact, not writing preference:

- **CRITICAL** — The task cannot succeed; code does not compile; an API/member is fabricated; guidance
  can crash, leak, corrupt data, free parent-owned memory, or violate a native lifetime; or the article
  directs readers through an unsupported path with no warning.
- **IMPORTANT** — A factual/default/platform claim is wrong; a required prerequisite, failure check, or
  recovery path is missing; a core link is broken; or the structure is likely to produce the wrong
  implementation.
- **MINOR** — Terminology, metadata, repetition, formatting, accessibility wording, or scannability can
  improve without changing the technical outcome.

Examples:

```text
CRITICAL | example | guide.md | Create the surface | Disposes SKSurface.Canvas, which is parent-owned; SKSurface.cs:... shows the surface owns it, so later draws can access a released native object.
IMPORTANT | platform | guide.md | Supported platforms | Claims Direct3D support on Linux, but the Direct3D context is Windows-only in <source path:line>; readers will choose an unavailable backend.
MINOR | structure | guide.md | Overview | The heading does not describe the decision made in this section, so it is hard to scan.
```

Every CRITICAL or IMPORTANT finding needs a repository `path:line`, focused test, or current first-party
source. For a structural finding, cite the applicable blueprint/reference path and line range. For an
unsupported-advice or absence finding, record the searched source scope and cite the closest contract
paths/ranges that show the supported behavior; "no source basis" alone is not a complete evidence record.
Deduplicate overlapping symptoms into the root finding.

## Output

Emit one machine-readable line per finding:

```text
SEVERITY | class | <file> | <heading-or-line> | <claim, evidence, reader impact, and fix direction>
```

Then provide a compact report:

```markdown
# Conceptual documentation review — <scope>

## Summary
- Files reviewed: <n>
- Findings: CRITICAL <n>, IMPORTANT <n>, MINOR <n>
- Unverified claims: <n>
- Verdict: Ready to publish / Needs fixes / Major rework

## Findings
...

## Evidence gaps
...
```

If the user asks for fixes, correct the root causes, preserve unrelated prose, and run the full validation
procedure. Inspect the changed article, evidence ledger, sample results, and rendered page; then repeat
the review and validation passes until no unresolved CRITICAL/IMPORTANT defect remains in the selected
wave. Do not leave staged review comments for issues already fixed in the branch unless the user
specifically wants review comments.
