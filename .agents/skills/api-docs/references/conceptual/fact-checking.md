# Fact-checking conceptual documentation

Conceptual prose can be fluent and still be false. Build a claim inventory before writing or reviewing,
then verify each claim against the closest source of truth.

Apply the shared evidence hierarchy, public managed contract boundary, and cross-layer procedure in
[`../technical-fact-checking.md`](../technical-fact-checking.md). This file adds the claim-management,
platform-matrix, and external-source checks needed by conceptual articles.

## Build a claim ledger

Track consequential claims rather than every sentence:

```text
CLAIM | <exact claim> | <scope/version/platform> | <evidence> | VERIFIED / QUALIFIED / UNVERIFIED
```

Include claims that affect whether a reader's code compiles, runs, remains safe, or selects a supported
path:

- API and overload existence.
- Failure behavior and return values.
- Defaults, limits, and units.
- Ownership and disposal.
- Async/callback lifetime and ordering.
- Thread/context requirements.
- Platform/backend availability.
- Version-sensitive behavior.

Record `QUALIFIED` when the claim is true only under a stated condition. Record `UNVERIFIED` when the
available evidence is insufficient; do not convert it into a warning or absolute statement.

## Verify platform matrices row by row

For every named backend/platform pair:

1. Find the target-specific handler, renderer, or project configuration.
2. Identify the actual backend selected there.
3. Check compile-time and runtime availability gates.
4. Record unsupported targets and the failure mode.
5. Distinguish "not implemented in this integration" from "the native API cannot support it."

Do not combine two backends into one platform cell or sentence unless their verified platform sets are
identical. OpenGL and Vulkan, for example, remain separate rows when one includes a target that the other
does not. A citation to a matrix is not enough: compare every emitted tuple with the cited row.

Keep required CI coverage distinct from availability. A `RequiredOn`/test-policy row proves where the
repository requires that backend, not that every omitted platform is unsupported. Before making a
negative or exhaustive availability claim, inspect the platform's native build flags and optional
integration path as well as the managed API.

Avoid broad claims such as "cross-platform," "works everywhere," or "identical" unless every listed
implementation supports the same behavior.

## Handle external and time-sensitive facts

- Prefer first-party product documentation or source.
- Capture the relevant version or publication state when behavior can change.
- Link to a stable conceptual page rather than a transient search result.
- Explain whether SkiaSharp exposes the native capability today; native support alone does not establish
  managed support.
- Remove an external claim that is not needed for the reader's outcome and cannot be verified.

## Evidence in reviews

Every CRITICAL or IMPORTANT factual finding needs one of:

- A repository `path:line`.
- A focused test and its observed result.
- A current first-party URL with the relevant condition quoted or summarized.

No citation means the lead remains `UNVERIFIED`. This prevents confident false positives from becoming
documentation churn.
