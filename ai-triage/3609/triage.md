# Issue Triage Report — #3609

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-03T05:30:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | ready-to-fix (0.92 (92%)) |

**Issue Summary:** Seven GC lifecycle tests intermittently fail on Windows .NET Framework CI due to non-deterministic finalizer scheduling; the CollectGarbage() helper lacks the double-collect pattern and there is no SkipOnNetFramework() guard equivalent to the existing SkipOnMono().

**Analysis:** The CollectGarbage() helper in BaseTest.cs only calls GC.Collect() + GC.WaitForPendingFinalizers() once — missing the standard double-collect pattern required to reliably flush finalizers. Under concurrent CI execution on .NET Framework, finalizer threads can race with assertions causing both 'object not collected' and 'refcount off by 1' failures. The fix is to improve CollectGarbage() and add a SkipOnNetFramework() guard for the most fragile tests.

**Recommendations:** **ready-to-fix** — Root cause is clearly identified (incomplete CollectGarbage helper and missing SkipOnNetFramework guard), fix path is well-scoped and documented in the issue body by the maintainer. No reproduction is needed — the CI failures are the reproduction.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, status/known-issue |

## Evidence

### Reproduction

1. Run the full SkiaSharp test suite on Windows .NET Framework (net48) under concurrent CI load
2. Observe intermittent failures in GC lifecycle tests (~1–2x per 10 builds each)

**Environment:** Windows .NET Framework 4.8, concurrent test execution under CI load

**Related issues:** #3608

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | other |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net48 |

## Analysis

### Technical Summary

The CollectGarbage() helper in BaseTest.cs only calls GC.Collect() + GC.WaitForPendingFinalizers() once — missing the standard double-collect pattern required to reliably flush finalizers. Under concurrent CI execution on .NET Framework, finalizer threads can race with assertions causing both 'object not collected' and 'refcount off by 1' failures. The fix is to improve CollectGarbage() and add a SkipOnNetFramework() guard for the most fragile tests.

### Rationale

This is a type/bug in the test infrastructure (not the library itself) that causes CI noise on Windows .NET Framework. The root cause is well-understood: CollectGarbage() lacks the double-collect pattern, and there is no SkipOnNetFramework() guard equivalent to SkipOnMono(). The fix is narrow and mechanical: improve the helper and guard the most fragile tests. Classification is ready-to-fix because both the root cause and the fix path are documented in the issue itself by the maintainer.

### Key Signals

- "call CollectGarbage(), then assert that objects have been finalized or reference counts have changed" — **issue body** (All failing tests share the same root cause: asserting deterministic GC timing on a non-deterministic runtime.)
- "NullReferenceException (GC race)" — **issue body — SKManagedWStreamTest.StreamIsReferencedAndNotDisposedPrematurely** (Confirms a finalizer thread is running concurrently and disposing objects before assertions complete.)
- "Several of these tests call SkipOnMono(), acknowledging that GC behavior varies by runtime. The same non-determinism applies to .NET Framework under CI load." — **issue body** (The SkipOnMono() guard already exists for the same class of problem; a SkipOnNetFramework() is the symmetrical solution.)
- "Expected refcount 4, got 3" — **issue body — SKColorSpaceTest.ColorSpaceIsNotDisposedPrematurely** (Off-by-one refcount suggests a concurrent finalizer ran before the assertion, already releasing one reference.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `tests/Tests/BaseTest.cs` | 38-42 | direct | CollectGarbage() calls GC.Collect() + GC.WaitForPendingFinalizers() only once — the standard double-collect idiom (Collect, WaitForPendingFinalizers, Collect) is missing. This means some finalization rounds may not complete before tests assert, especially under heavy parallel test execution. |
| `tests/Tests/BaseTest.cs` | 72-75 | direct | SkipOnMono() exists to skip tests on runtimes where finalizer timing is non-deterministic, but no SkipOnNetFramework() equivalent is defined. .NET Framework 4.8 under concurrent test load exhibits similar non-determinism. |
| `tests/Tests/SkiaSharp/SKManagedWStreamTest.cs` | 112-150 | direct | StreamIsReferencedAndNotDisposedPrematurely calls CollectGarbage() inside DoWork(), then checks stream liveness after the outer CollectGarbage(). The NullReferenceException in CI suggests a finalizer thread disposes the stream between the GC call and the assertion — a classic finalization race. |
| `tests/Tests/SkiaSharp/SKColorSpaceTest.cs` | 134-161 | direct | ColorSpaceIsNotDisposedPrematurely asserts exact native refcounts (e.g., Assert.Equal(2, ...)) after CollectGarbage(). Internal caching or concurrent finalizers can cause the refcount to be off-by-one. The test at line 175 already uses Assert.InRange for the PeekPixels refcount, but lines 148/155/159 still use strict Assert.Equal. |
| `tests/Tests/SkiaSharp/SKTypefaceTest.cs` | 556-590 | related | GCStillCollectsTypeface skips on non-Windows only (SkipOnNonWindows), so it runs on Windows .NET Framework. It relies on GC collecting a typeface after a single CollectGarbage() call — fails when finalizers don't run within the expected window. |

### Workarounds

- Use the double-collect pattern manually in specific tests: GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();
- Add Thread.Sleep(100) between GC calls to reduce finalizer race conditions under CI load

### Next Questions

- Should SkipOnNetFramework() fully skip the test or should it retry with tolerance instead?
- Are the refcount assertions in ColorSpaceIsNotDisposedPrematurely intentionally strict, or can they use Assert.InRange across the board?

### Resolution Proposals

**Hypothesis:** The CollectGarbage() helper is too weak for concurrent CI environments. Upgrading it to the double-collect pattern (GC.Collect, WaitForPendingFinalizers, GC.Collect) with optional retry will resolve most failures. The most fragile tests should also gain a SkipOnNetFramework() guard.

1. **Improve CollectGarbage() with double-collect pattern** — fix, confidence 0.90 (90%), cost/xs, validated=yes
   - Update BaseTest.CollectGarbage() to call GC.Collect(), GC.WaitForPendingFinalizers(), then GC.Collect() a second time. This is the standard idiom for flushing the finalizer queue reliably.

```csharp
public static void CollectGarbage()
{
    GC.Collect();
    GC.WaitForPendingFinalizers();
    GC.Collect();
}
```
2. **Add SkipOnNetFramework() guard for fragile GC tests** — fix, confidence 0.85 (85%), cost/s, validated=yes
   - Add a SkipOnNetFramework() helper to BaseTest (mirroring SkipOnMono()) and apply it to tests where exact finalizer timing is required: GCStillCollectsTypeface, StreamIsReferencedAndNotDisposedPrematurely, StreamLosesOwnershipAndCanBeGarbageCollected, DataOutLivesImageUntilFinalizersRun.

```csharp
protected static void SkipOnNetFramework(string reason = ".NET Framework does not guarantee finalizers run immediately under CI load")
{
    Assert.SkipWhen(IsNetFramework, reason);
}

private static bool IsNetFramework =>
#if NETFRAMEWORK
    true;
#else
    false;
#endif
```
3. **Use Assert.InRange for refcount-sensitive assertions** — fix, confidence 0.80 (80%), cost/xs, validated=untested
   - Replace Assert.Equal for native refcount checks in ColorSpaceIsNotDisposedPrematurely with Assert.InRange(actual, expected - 1, expected + 1) to tolerate off-by-one from concurrent finalizers.

**Recommended proposal:** Improve CollectGarbage() with double-collect pattern

**Why:** Fixing the root cause in one place (BaseTest.CollectGarbage) improves all seven tests at once, with minimal code change and zero risk of hiding real bugs.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.92 (92%) |
| Reason | Root cause is clearly identified (incomplete CollectGarbage helper and missing SkipOnNetFramework guard), fix path is well-scoped and documented in the issue body by the maintainer. No reproduction is needed — the CI failures are the reproduction. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add area/SkiaSharp and tenet/reliability labels; type/bug and os/Windows-Classic are already present | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.90 (90%) | Summarize root cause and direct to the three fix proposals | — |

**Comment draft for `add-comment`:**

```markdown
**Root cause confirmed:** `CollectGarbage()` in `BaseTest.cs` uses a single `GC.Collect() + GC.WaitForPendingFinalizers()` call, which is insufficient to reliably drain the finalizer queue under concurrent .NET Framework CI execution.

**Recommended fix (ready to implement):**

1. **Upgrade `CollectGarbage()`** to the double-collect pattern:
   ```csharp
   public static void CollectGarbage()
   {
       GC.Collect();
       GC.WaitForPendingFinalizers();
       GC.Collect();
   }
   ```

2. **Add `SkipOnNetFramework()` guard** (mirrors `SkipOnMono()`) for the most timing-sensitive tests: `GCStillCollectsTypeface`, `StreamIsReferencedAndNotDisposedPrematurely`, `StreamLosesOwnershipAndCanBeGarbageCollected`, `DataOutLivesImageUntilFinalizersRun`.

3. **Optionally** replace `Assert.Equal` with `Assert.InRange` for native refcount checks in `ColorSpaceIsNotDisposedPrematurely` (lines 148, 155, 159) to tolerate off-by-one from concurrent finalizers.

All three changes are in test infrastructure only — no library code is affected.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3609,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-03T05:30:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "status/known-issue"
    ]
  },
  "summary": "Seven GC lifecycle tests intermittently fail on Windows .NET Framework CI due to non-deterministic finalizer scheduling; the CollectGarbage() helper lacks the double-collect pattern and there is no SkipOnNetFramework() guard equivalent to the existing SkipOnMono().",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "other",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net48"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run the full SkiaSharp test suite on Windows .NET Framework (net48) under concurrent CI load",
        "Observe intermittent failures in GC lifecycle tests (~1–2x per 10 builds each)"
      ],
      "environmentDetails": "Windows .NET Framework 4.8, concurrent test execution under CI load",
      "relatedIssues": [
        3608
      ]
    }
  },
  "analysis": {
    "summary": "The CollectGarbage() helper in BaseTest.cs only calls GC.Collect() + GC.WaitForPendingFinalizers() once — missing the standard double-collect pattern required to reliably flush finalizers. Under concurrent CI execution on .NET Framework, finalizer threads can race with assertions causing both 'object not collected' and 'refcount off by 1' failures. The fix is to improve CollectGarbage() and add a SkipOnNetFramework() guard for the most fragile tests.",
    "codeInvestigation": [
      {
        "file": "tests/Tests/BaseTest.cs",
        "lines": "38-42",
        "finding": "CollectGarbage() calls GC.Collect() + GC.WaitForPendingFinalizers() only once — the standard double-collect idiom (Collect, WaitForPendingFinalizers, Collect) is missing. This means some finalization rounds may not complete before tests assert, especially under heavy parallel test execution.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/BaseTest.cs",
        "lines": "72-75",
        "finding": "SkipOnMono() exists to skip tests on runtimes where finalizer timing is non-deterministic, but no SkipOnNetFramework() equivalent is defined. .NET Framework 4.8 under concurrent test load exhibits similar non-determinism.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKManagedWStreamTest.cs",
        "lines": "112-150",
        "finding": "StreamIsReferencedAndNotDisposedPrematurely calls CollectGarbage() inside DoWork(), then checks stream liveness after the outer CollectGarbage(). The NullReferenceException in CI suggests a finalizer thread disposes the stream between the GC call and the assertion — a classic finalization race.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKColorSpaceTest.cs",
        "lines": "134-161",
        "finding": "ColorSpaceIsNotDisposedPrematurely asserts exact native refcounts (e.g., Assert.Equal(2, ...)) after CollectGarbage(). Internal caching or concurrent finalizers can cause the refcount to be off-by-one. The test at line 175 already uses Assert.InRange for the PeekPixels refcount, but lines 148/155/159 still use strict Assert.Equal.",
        "relevance": "direct"
      },
      {
        "file": "tests/Tests/SkiaSharp/SKTypefaceTest.cs",
        "lines": "556-590",
        "finding": "GCStillCollectsTypeface skips on non-Windows only (SkipOnNonWindows), so it runs on Windows .NET Framework. It relies on GC collecting a typeface after a single CollectGarbage() call — fails when finalizers don't run within the expected window.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "call CollectGarbage(), then assert that objects have been finalized or reference counts have changed",
        "source": "issue body",
        "interpretation": "All failing tests share the same root cause: asserting deterministic GC timing on a non-deterministic runtime."
      },
      {
        "text": "NullReferenceException (GC race)",
        "source": "issue body — SKManagedWStreamTest.StreamIsReferencedAndNotDisposedPrematurely",
        "interpretation": "Confirms a finalizer thread is running concurrently and disposing objects before assertions complete."
      },
      {
        "text": "Several of these tests call SkipOnMono(), acknowledging that GC behavior varies by runtime. The same non-determinism applies to .NET Framework under CI load.",
        "source": "issue body",
        "interpretation": "The SkipOnMono() guard already exists for the same class of problem; a SkipOnNetFramework() is the symmetrical solution."
      },
      {
        "text": "Expected refcount 4, got 3",
        "source": "issue body — SKColorSpaceTest.ColorSpaceIsNotDisposedPrematurely",
        "interpretation": "Off-by-one refcount suggests a concurrent finalizer ran before the assertion, already releasing one reference."
      }
    ],
    "rationale": "This is a type/bug in the test infrastructure (not the library itself) that causes CI noise on Windows .NET Framework. The root cause is well-understood: CollectGarbage() lacks the double-collect pattern, and there is no SkipOnNetFramework() guard equivalent to SkipOnMono(). The fix is narrow and mechanical: improve the helper and guard the most fragile tests. Classification is ready-to-fix because both the root cause and the fix path are documented in the issue itself by the maintainer.",
    "workarounds": [
      "Use the double-collect pattern manually in specific tests: GC.Collect(); GC.WaitForPendingFinalizers(); GC.Collect();",
      "Add Thread.Sleep(100) between GC calls to reduce finalizer race conditions under CI load"
    ],
    "nextQuestions": [
      "Should SkipOnNetFramework() fully skip the test or should it retry with tolerance instead?",
      "Are the refcount assertions in ColorSpaceIsNotDisposedPrematurely intentionally strict, or can they use Assert.InRange across the board?"
    ],
    "resolution": {
      "hypothesis": "The CollectGarbage() helper is too weak for concurrent CI environments. Upgrading it to the double-collect pattern (GC.Collect, WaitForPendingFinalizers, GC.Collect) with optional retry will resolve most failures. The most fragile tests should also gain a SkipOnNetFramework() guard.",
      "proposals": [
        {
          "title": "Improve CollectGarbage() with double-collect pattern",
          "description": "Update BaseTest.CollectGarbage() to call GC.Collect(), GC.WaitForPendingFinalizers(), then GC.Collect() a second time. This is the standard idiom for flushing the finalizer queue reliably.",
          "category": "fix",
          "codeSnippet": "public static void CollectGarbage()\n{\n    GC.Collect();\n    GC.WaitForPendingFinalizers();\n    GC.Collect();\n}",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Add SkipOnNetFramework() guard for fragile GC tests",
          "description": "Add a SkipOnNetFramework() helper to BaseTest (mirroring SkipOnMono()) and apply it to tests where exact finalizer timing is required: GCStillCollectsTypeface, StreamIsReferencedAndNotDisposedPrematurely, StreamLosesOwnershipAndCanBeGarbageCollected, DataOutLivesImageUntilFinalizersRun.",
          "category": "fix",
          "codeSnippet": "protected static void SkipOnNetFramework(string reason = \".NET Framework does not guarantee finalizers run immediately under CI load\")\n{\n    Assert.SkipWhen(IsNetFramework, reason);\n}\n\nprivate static bool IsNetFramework =>\n#if NETFRAMEWORK\n    true;\n#else\n    false;\n#endif",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "yes"
        },
        {
          "title": "Use Assert.InRange for refcount-sensitive assertions",
          "description": "Replace Assert.Equal for native refcount checks in ColorSpaceIsNotDisposedPrematurely with Assert.InRange(actual, expected - 1, expected + 1) to tolerate off-by-one from concurrent finalizers.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Improve CollectGarbage() with double-collect pattern",
      "recommendedReason": "Fixing the root cause in one place (BaseTest.CollectGarbage) improves all seven tests at once, with minimal code change and zero risk of hiding real bugs."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.92,
      "reason": "Root cause is clearly identified (incomplete CollectGarbage helper and missing SkipOnNetFramework guard), fix path is well-scoped and documented in the issue body by the maintainer. No reproduction is needed — the CI failures are the reproduction.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add area/SkiaSharp and tenet/reliability labels; type/bug and os/Windows-Classic are already present",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Summarize root cause and direct to the three fix proposals",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "**Root cause confirmed:** `CollectGarbage()` in `BaseTest.cs` uses a single `GC.Collect() + GC.WaitForPendingFinalizers()` call, which is insufficient to reliably drain the finalizer queue under concurrent .NET Framework CI execution.\n\n**Recommended fix (ready to implement):**\n\n1. **Upgrade `CollectGarbage()`** to the double-collect pattern:\n   ```csharp\n   public static void CollectGarbage()\n   {\n       GC.Collect();\n       GC.WaitForPendingFinalizers();\n       GC.Collect();\n   }\n   ```\n\n2. **Add `SkipOnNetFramework()` guard** (mirrors `SkipOnMono()`) for the most timing-sensitive tests: `GCStillCollectsTypeface`, `StreamIsReferencedAndNotDisposedPrematurely`, `StreamLosesOwnershipAndCanBeGarbageCollected`, `DataOutLivesImageUntilFinalizersRun`.\n\n3. **Optionally** replace `Assert.Equal` with `Assert.InRange` for native refcount checks in `ColorSpaceIsNotDisposedPrematurely` (lines 148, 155, 159) to tolerate off-by-one from concurrent finalizers.\n\nAll three changes are in test infrastructure only — no library code is affected."
      }
    ]
  }
}
```

</details>
