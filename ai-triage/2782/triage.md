# Issue Triage Report — #2782

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T20:05:28Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** SKShader.WithLocalMatrix (and CreateLocalMatrix) incorrectly cancels a native ref-count bump when Skia returns the same pointer for an identity matrix, causing premature disposal of the original shader.

**Analysis:** When Skia's sk_shader_with_local_matrix is called with an identity matrix it returns the same sk_sp<SkShader> with a bumped ref count (same native pointer). SKShader.GetObject calls GetOrAddObject with unrefExisting=true, which finds the already-cached managed instance and calls SafeUnRef() to cancel the extra ref. This leaves the ref count unchanged, so when the temp wrapper is disposed the ref count hits 0 and both wrappers are invalidated, even though the original shader is still in scope.

**Recommendations:** **ready-to-fix** — Root cause is fully understood, fix path is clear (remove identity fast path in C API shim), and maintainer has already confirmed the approach.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create an SKShader: s1 = SKShader.CreateColor(...)
2. Call s2 = s1.WithLocalMatrix(SKMatrix.CreateIdentity())
3. Dispose s2
4. Observe that s1.Handle is now IntPtr.Zero even though s1 has not been disposed

**Environment:** SkiaSharp 2.88+; any platform. Regression from 1.x behavior.

**Repository links:**
- https://github.com/mono/SkiaSharp/discussions/2652#discussioncomment-7324838 — Original discussion where the reporter first identified the issue

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | After disposing temp wrapper with identity local matrix, original shader handle becomes IntPtr.Zero prematurely |
| Repro quality | complete |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.1, 2.88 |
| Worked in | 1.60.1 |
| Broke in | 2.88 |
| Current relevance | likely |
| Relevance reason | The GetOrAddObject unrefExisting=true path and the Skia identity-matrix fast path are both still present in the current codebase. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.90 (90%) |
| Reason | Reporter confirms the behavior worked in v1.60.1 where a new wrapper was always created. Changed when the managed object cache (HandleDictionary) was introduced alongside the ref-counted unrefExisting logic. |
| Worked in version | 1.60.1 |
| Broke in version | 2.88 |

## Analysis

### Technical Summary

When Skia's sk_shader_with_local_matrix is called with an identity matrix it returns the same sk_sp<SkShader> with a bumped ref count (same native pointer). SKShader.GetObject calls GetOrAddObject with unrefExisting=true, which finds the already-cached managed instance and calls SafeUnRef() to cancel the extra ref. This leaves the ref count unchanged, so when the temp wrapper is disposed the ref count hits 0 and both wrappers are invalidated, even though the original shader is still in scope.

### Rationale

This is a confirmed memory-management bug: the unrefExisting=true logic in GetOrAddObject is designed to handle the case where the native side bumped the count for a new owner, but when the same handle is returned with no new logical owner (identity-matrix fast path), the unref leaves the count incorrect. The maintainer has already diagnosed and proposed a fix: remove the identity-matrix fast path so a new native object is always returned.

### Key Signals

- "Temp has the same Handle, and ref count=1 (!)" — **issue body** (Confirms that GetOrAddObject's SafeUnRef() cancelled the sk_ref_sp bump, leaving the count at 1.)
- "the createLocalMatrix does bump the ref count to 2, but that the managed wrapper does an unref() before returning. That happens in GetOrAddObject() which is called with unrefExisting=true" — **issue body** (Reporter directly identified the root cause in the call stack.)
- "I think I am just going to remove the fast path and skip the identity check" — **comment #1 (mattleibow)** (Maintainer confirmed bug and proposed fix: bypass the Skia identity-matrix optimization at the C API shim layer.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKShader.cs` | 30-31 | direct | WithLocalMatrix calls GetObject(sk_shader_with_local_matrix(Handle, &localMatrix)). When localMatrix is identity, Skia returns the same pointer with ref count+1. |
| `binding/SkiaSharp/SKShader.cs` | 458-459 | direct | GetObject delegates to GetOrAddObject(handle, (h,o) => new SKShader(h,o)) without overriding unrefExisting, so the default true is used. |
| `binding/SkiaSharp/SKObject.cs` | 102-104 | direct | SKObject.GetOrAddObject(handle, factory) calls HandleDictionary.GetOrAddObject(handle, owns:true, unrefExisting:true, factory) — hardcoded unrefExisting=true. |
| `binding/SkiaSharp/HandleDictionary.cs` | 76-89 | direct | When an existing managed instance is found for the handle, if unrefExisting=true it calls SafeUnRef() on it. This cancels the Skia ref-count bump, leaving the count at 1 instead of 2 — so the first dispose prematurely frees the native object. |

### Next Questions

- Does WithColorFilter have the same issue when a null/no-op filter is passed?
- Are there other C API methods that may return the same pointer due to similar optimizations?

### Resolution Proposals

**Hypothesis:** Skia's identity-matrix fast path returns the same native pointer, but GetOrAddObject's unref logic treats this as 'we own a new ref and should discard it' — which is incorrect when the same logical owner is still active.

1. **Remove identity-matrix fast path in C API shim** — fix, confidence 0.90 (90%), cost/s, validated=untested
   - Override the identity check in the C API shim (externals/skia/src/c/sk_shader.cpp) so sk_shader_with_local_matrix always creates a new SkLocalMatrixShader, even for identity matrices. This guarantees a fresh native pointer with ref count 1 is returned.
2. **Special-case same-pointer return in GetObject** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - In SKShader.GetObject (or WithLocalMatrix), detect when the returned handle equals the current Handle and return 'this' directly without going through GetOrAddObject, preserving the ref count.

**Recommended proposal:** Remove identity-matrix fast path in C API shim

**Why:** Maintainer's preferred approach. Simpler to reason about: always return a new object with a fresh ref. Avoids the subtle same-pointer aliasing problem throughout the managed wrapper layer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | Root cause is fully understood, fix path is clear (remove identity fast path in C API shim), and maintainer has already confirmed the approach. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp, tenet/reliability | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Summarize diagnosis and proposed fix path for visibility | — |

**Comment draft for `add-comment`:**

```markdown
**Root cause confirmed.** When `WithLocalMatrix` is called with an identity matrix, Skia's C++ returns the same `sk_sp<SkShader>` (with ref count bumped). The managed `GetOrAddObject` path then calls `SafeUnRef()` on the cached instance, cancelling that bump. The result is ref count stays at 1, so disposing the second wrapper prematurely destroys the native object that the first wrapper still points to.

The planned fix is to remove the identity-matrix fast path in the C API shim so that `sk_shader_with_local_matrix` always returns a new native object, giving each managed wrapper exclusive ownership of its ref.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2782,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T20:05:28Z"
  },
  "summary": "SKShader.WithLocalMatrix (and CreateLocalMatrix) incorrectly cancels a native ref-count bump when Skia returns the same pointer for an identity matrix, causing premature disposal of the original shader.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "After disposing temp wrapper with identity local matrix, original shader handle becomes IntPtr.Zero prematurely",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKShader: s1 = SKShader.CreateColor(...)",
        "Call s2 = s1.WithLocalMatrix(SKMatrix.CreateIdentity())",
        "Dispose s2",
        "Observe that s1.Handle is now IntPtr.Zero even though s1 has not been disposed"
      ],
      "environmentDetails": "SkiaSharp 2.88+; any platform. Regression from 1.x behavior.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/discussions/2652#discussioncomment-7324838",
          "description": "Original discussion where the reporter first identified the issue"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.1",
        "2.88"
      ],
      "workedIn": "1.60.1",
      "brokeIn": "2.88",
      "currentRelevance": "likely",
      "relevanceReason": "The GetOrAddObject unrefExisting=true path and the Skia identity-matrix fast path are both still present in the current codebase."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.9,
      "reason": "Reporter confirms the behavior worked in v1.60.1 where a new wrapper was always created. Changed when the managed object cache (HandleDictionary) was introduced alongside the ref-counted unrefExisting logic.",
      "workedInVersion": "1.60.1",
      "brokeInVersion": "2.88"
    }
  },
  "analysis": {
    "summary": "When Skia's sk_shader_with_local_matrix is called with an identity matrix it returns the same sk_sp<SkShader> with a bumped ref count (same native pointer). SKShader.GetObject calls GetOrAddObject with unrefExisting=true, which finds the already-cached managed instance and calls SafeUnRef() to cancel the extra ref. This leaves the ref count unchanged, so when the temp wrapper is disposed the ref count hits 0 and both wrappers are invalidated, even though the original shader is still in scope.",
    "rationale": "This is a confirmed memory-management bug: the unrefExisting=true logic in GetOrAddObject is designed to handle the case where the native side bumped the count for a new owner, but when the same handle is returned with no new logical owner (identity-matrix fast path), the unref leaves the count incorrect. The maintainer has already diagnosed and proposed a fix: remove the identity-matrix fast path so a new native object is always returned.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "30-31",
        "finding": "WithLocalMatrix calls GetObject(sk_shader_with_local_matrix(Handle, &localMatrix)). When localMatrix is identity, Skia returns the same pointer with ref count+1.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "458-459",
        "finding": "GetObject delegates to GetOrAddObject(handle, (h,o) => new SKShader(h,o)) without overriding unrefExisting, so the default true is used.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "102-104",
        "finding": "SKObject.GetOrAddObject(handle, factory) calls HandleDictionary.GetOrAddObject(handle, owns:true, unrefExisting:true, factory) — hardcoded unrefExisting=true.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "76-89",
        "finding": "When an existing managed instance is found for the handle, if unrefExisting=true it calls SafeUnRef() on it. This cancels the Skia ref-count bump, leaving the count at 1 instead of 2 — so the first dispose prematurely frees the native object.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Temp has the same Handle, and ref count=1 (!)",
        "source": "issue body",
        "interpretation": "Confirms that GetOrAddObject's SafeUnRef() cancelled the sk_ref_sp bump, leaving the count at 1."
      },
      {
        "text": "the createLocalMatrix does bump the ref count to 2, but that the managed wrapper does an unref() before returning. That happens in GetOrAddObject() which is called with unrefExisting=true",
        "source": "issue body",
        "interpretation": "Reporter directly identified the root cause in the call stack."
      },
      {
        "text": "I think I am just going to remove the fast path and skip the identity check",
        "source": "comment #1 (mattleibow)",
        "interpretation": "Maintainer confirmed bug and proposed fix: bypass the Skia identity-matrix optimization at the C API shim layer."
      }
    ],
    "nextQuestions": [
      "Does WithColorFilter have the same issue when a null/no-op filter is passed?",
      "Are there other C API methods that may return the same pointer due to similar optimizations?"
    ],
    "resolution": {
      "hypothesis": "Skia's identity-matrix fast path returns the same native pointer, but GetOrAddObject's unref logic treats this as 'we own a new ref and should discard it' — which is incorrect when the same logical owner is still active.",
      "proposals": [
        {
          "title": "Remove identity-matrix fast path in C API shim",
          "description": "Override the identity check in the C API shim (externals/skia/src/c/sk_shader.cpp) so sk_shader_with_local_matrix always creates a new SkLocalMatrixShader, even for identity matrices. This guarantees a fresh native pointer with ref count 1 is returned.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Special-case same-pointer return in GetObject",
          "description": "In SKShader.GetObject (or WithLocalMatrix), detect when the returned handle equals the current Handle and return 'this' directly without going through GetOrAddObject, preserving the ref count.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Remove identity-matrix fast path in C API shim",
      "recommendedReason": "Maintainer's preferred approach. Simpler to reason about: always return a new object with a fresh ref. Avoids the subtle same-pointer aliasing problem throughout the managed wrapper layer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "Root cause is fully understood, fix path is clear (remove identity fast path in C API shim), and maintainer has already confirmed the approach.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, tenet/reliability",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize diagnosis and proposed fix path for visibility",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "**Root cause confirmed.** When `WithLocalMatrix` is called with an identity matrix, Skia's C++ returns the same `sk_sp<SkShader>` (with ref count bumped). The managed `GetOrAddObject` path then calls `SafeUnRef()` on the cached instance, cancelling that bump. The result is ref count stays at 1, so disposing the second wrapper prematurely destroys the native object that the first wrapper still points to.\n\nThe planned fix is to remove the identity-matrix fast path in the C API shim so that `sk_shader_with_local_matrix` always returns a new native object, giving each managed wrapper exclusive ownership of its ref."
      }
    ]
  }
}
```

</details>
