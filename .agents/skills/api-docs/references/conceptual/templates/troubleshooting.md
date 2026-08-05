# Troubleshooting blueprint

Use a troubleshooting article when readers arrive with a named symptom, error, or unexpected result.
Lead with diagnosis and restoration, not a general architecture lesson.

## Plan

Define:

```text
Exact symptom/error:
Affected versions/platforms:
Fast discriminating checks:
Likely causes in priority order:
Fix for each cause:
Verification:
Prevention:
When to escalate:
```

If several unrelated failures share only a broad product area, split them into focused articles or use an
index that routes by symptom.

## Suggested shape

```markdown
---
title: "Troubleshoot <specific symptom> in SkiaSharp"
description: "<Symptom, likely scope, and the result of following the article>"
---

# Troubleshoot <specific symptom> in SkiaSharp

<Repeat the observable symptom in the reader's language, state the most likely scope, and give the fastest
safe route to confirmation.>

## Confirm the symptom

1. <Check that distinguishes this issue from a similar one.>
2. <Capture the relevant status/error/backend/version.>

## Diagnose the cause

| Observation | Likely cause | Go to |
|---|---|---|
| <Signal> | <Cause> | [<Fix heading>](#fragment) |

## Fix <cause one>

<Explain why it causes the symptom.>

<If seeing the failing pattern helps diagnosis, mark it as incorrect in prose and code:>

```csharp
// Incorrect: <why this produces the symptom>.
// <Dangerous or non-compiling line remains commented out.>
```

```csharp
// Corrected pattern.
```

## Fix <cause two>

...

## Verify the fix

<Expected output/status and a check that the original symptom is gone.>

## Prevent recurrence

<Test, validation, or design practice if one exists.>

## Get more help

<Diagnostic details to collect and the appropriate issue/support path.>
```

## Quality checks

- The title and opening use the symptom a reader is likely to search for.
- Diagnostic checks narrow causes before the article asks for invasive changes.
- Causes are ordered by likelihood and impact.
- Each fix explains the causal link and includes a verification step.
- Workarounds are labeled as workarounds and do not replace a supported fix without explanation.
- Escalation asks for actionable diagnostics without exposing credentials or personal data.
