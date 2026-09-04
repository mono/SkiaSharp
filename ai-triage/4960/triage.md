# Issue Triage Report — #4960

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-09-04T04:27:08Z |
| Type | type/enhancement (0.85 (85%)) |
| Area | area/Build (0.75 (75%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Tracking issue to refactor SkiaSharp's automation/tooling to derive repository identity from GitHub context and .gitmodules so the mono→dotnet org transfer only requires a small set of edits.

**Analysis:** This is a large, well-scoped infrastructure/tooling tracking issue (sub-issue of #4959, the mono→dotnet org transfer epic) with a detailed checklist covering identity plumbing, gh-aw workflow guards, Skia sync tooling, submodule/docs metadata, release/milestone tooling, and public link portability. It is not a code bug: it asks for repository-identity literals (mono/SkiaSharp, mono/skia) to be replaced with values derived from GitHub Actions context and the existing .gitmodules/cgmanifest.json configuration, so that the actual org transfer only requires a handful of source edits. Work is already substantially in progress via PR #4964, which the comment thread shows going through multiple remediation passes (URL rewriting, cache key handling, drift scanning, DocFX/Pages base URL, Backport workflow scope split into #4965). The most recent comment reports the PR's automation/tooling and site-build checks green, with only the required Azure macOS/iOS/Mac Catalyst/tvOS aggregate pending an available build worker (external CI capacity issue, unrelated to the change itself).

**Recommendations:** **keep-open** — This is an actively-worked infrastructure tracking issue with an in-progress, near-complete implementation PR (#4964); it should remain open until that PR merges and the externally-blocked Azure CI leg clears.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Perf | — |
| Partner | — |

## Evidence

## Analysis

### Technical Summary

This is a large, well-scoped infrastructure/tooling tracking issue (sub-issue of #4959, the mono→dotnet org transfer epic) with a detailed checklist covering identity plumbing, gh-aw workflow guards, Skia sync tooling, submodule/docs metadata, release/milestone tooling, and public link portability. It is not a code bug: it asks for repository-identity literals (mono/SkiaSharp, mono/skia) to be replaced with values derived from GitHub Actions context and the existing .gitmodules/cgmanifest.json configuration, so that the actual org transfer only requires a handful of source edits. Work is already substantially in progress via PR #4964, which the comment thread shows going through multiple remediation passes (URL rewriting, cache key handling, drift scanning, DocFX/Pages base URL, Backport workflow scope split into #4965). The most recent comment reports the PR's automation/tooling and site-build checks green, with only the required Azure macOS/iOS/Mac Catalyst/tvOS aggregate pending an available build worker (external CI capacity issue, unrelated to the change itself).

### Rationale

Classified as type/enhancement (improves existing automation/tooling, no new user-facing feature) with area/Build since the scope is entirely CI/tooling/submodule-metadata rather than the SkiaSharp API surface. No bugSignals since this is not a functional defect. suggestedAction is keep-open: the linked PR #4964 already implements and iteratively remediates the full checklist and is passing all controllable checks; the issue should stay open until #4964 merges and the remaining external Azure macOS worker-availability blocker clears.

### Key Signals

- "Refactor SkiaSharp's in-repository automation and tooling so the repository can move from mono/SkiaSharp to dotnet/SkiaSharp without a broad transfer-day edit." — **issue body** (Primary goal: centralize repository-identity resolution ahead of an org transfer.)
- "PR #4964 head 881540f1681 is pushed and clean... The required aggregate is externally blocked: Native macOS iOS, macOS, Mac Catalyst, and tvOS remain pending with no assigned worker" — **issue comment** (Implementation is essentially complete; remaining blocker is CI capacity, not code.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `.gitmodules` | — | direct | externals/skia is pinned to https://github.com/mono/skia.git (branch skiasharp) and docs is pinned to https://github.com/mono/SkiaSharp-API-docs (branch master), confirming these are exactly the literal paired-repository URLs the issue wants centrally resolved instead of hard-coded in tooling. |
| `cgmanifest.json` | — | direct | Contains a registration entry with a "url" field for the Skia submodule that duplicates the .gitmodules URL, matching the issue's statement that cgmanifest.json is 'the one required duplicate of the Skia submodule URL' needing validation. |

### Next Questions

- Has PR #4964 merged, and does it close out all checklist items in this issue?
- Is the required Azure macOS/iOS/Mac Catalyst/tvOS build leg unblocked (worker availability), allowing the aggregate check to complete?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | This is an actively-worked infrastructure tracking issue with an in-progress, near-complete implementation PR (#4964); it should remain open until that PR merges and the externally-blocked Azure CI leg clears. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply classification labels to categorize this tracking issue. | labels=type/enhancement, area/Build, tenet/compatibility |
| link-related | low | 0.90 (90%) | Cross-reference the parent migration epic and related PRs/issues already mentioned in the thread. | linkedIssue=#4959 |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 4960,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-09-04T04:27:08Z"
  },
  "summary": "Tracking issue to refactor SkiaSharp's automation/tooling to derive repository identity from GitHub context and .gitmodules so the mono→dotnet org transfer only requires a small set of edits.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.85
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.75
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {},
  "analysis": {
    "summary": "This is a large, well-scoped infrastructure/tooling tracking issue (sub-issue of #4959, the mono→dotnet org transfer epic) with a detailed checklist covering identity plumbing, gh-aw workflow guards, Skia sync tooling, submodule/docs metadata, release/milestone tooling, and public link portability. It is not a code bug: it asks for repository-identity literals (mono/SkiaSharp, mono/skia) to be replaced with values derived from GitHub Actions context and the existing .gitmodules/cgmanifest.json configuration, so that the actual org transfer only requires a handful of source edits. Work is already substantially in progress via PR #4964, which the comment thread shows going through multiple remediation passes (URL rewriting, cache key handling, drift scanning, DocFX/Pages base URL, Backport workflow scope split into #4965). The most recent comment reports the PR's automation/tooling and site-build checks green, with only the required Azure macOS/iOS/Mac Catalyst/tvOS aggregate pending an available build worker (external CI capacity issue, unrelated to the change itself).",
    "codeInvestigation": [
      {
        "file": ".gitmodules",
        "finding": "externals/skia is pinned to https://github.com/mono/skia.git (branch skiasharp) and docs is pinned to https://github.com/mono/SkiaSharp-API-docs (branch master), confirming these are exactly the literal paired-repository URLs the issue wants centrally resolved instead of hard-coded in tooling.",
        "relevance": "direct"
      },
      {
        "file": "cgmanifest.json",
        "finding": "Contains a registration entry with a \"url\" field for the Skia submodule that duplicates the .gitmodules URL, matching the issue's statement that cgmanifest.json is 'the one required duplicate of the Skia submodule URL' needing validation.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Refactor SkiaSharp's in-repository automation and tooling so the repository can move from mono/SkiaSharp to dotnet/SkiaSharp without a broad transfer-day edit.",
        "source": "issue body",
        "interpretation": "Primary goal: centralize repository-identity resolution ahead of an org transfer."
      },
      {
        "text": "PR #4964 head 881540f1681 is pushed and clean... The required aggregate is externally blocked: Native macOS iOS, macOS, Mac Catalyst, and tvOS remain pending with no assigned worker",
        "source": "issue comment",
        "interpretation": "Implementation is essentially complete; remaining blocker is CI capacity, not code."
      }
    ],
    "rationale": "Classified as type/enhancement (improves existing automation/tooling, no new user-facing feature) with area/Build since the scope is entirely CI/tooling/submodule-metadata rather than the SkiaSharp API surface. No bugSignals since this is not a functional defect. suggestedAction is keep-open: the linked PR #4964 already implements and iteratively remediates the full checklist and is passing all controllable checks; the issue should stay open until #4964 merges and the remaining external Azure macOS worker-availability blocker clears.",
    "nextQuestions": [
      "Has PR #4964 merged, and does it close out all checklist items in this issue?",
      "Is the required Azure macOS/iOS/Mac Catalyst/tvOS build leg unblocked (worker availability), allowing the aggregate check to complete?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "This is an actively-worked infrastructure tracking issue with an in-progress, near-complete implementation PR (#4964); it should remain open until that PR merges and the externally-blocked Azure CI leg clears."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels to categorize this tracking issue.",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/enhancement",
          "area/Build",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference the parent migration epic and related PRs/issues already mentioned in the thread.",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 4959
      }
    ]
  }
}
```

</details>
