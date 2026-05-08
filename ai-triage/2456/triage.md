# Issue Triage Report — #2456

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T09:01:41Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-investigation (0.78 (78%)) |

**Issue Summary:** Reporter receives an AccessViolationException when creating and copying an SKBitmap inside async/multithreaded MAUI code on Windows and Android with SkiaSharp 2.88.3; making the code synchronous eliminates the crash.

**Analysis:** The crash is almost certainly a threading violation: SKBitmap (and all Skia objects) are not thread-safe, and async/await continuations in .NET can resume on any thread pool thread. The reporter's own follow-up confirms this — making the code synchronous eliminates the error entirely. Larger bitmaps expose the race more readily because allocation takes longer. The root cause is likely that a single SKBitmap instance is accessed concurrently across async continuation threads, or that a bitmap is disposed on one thread while a pixel-level copy operation is still in progress on another.

**Recommendations:** **needs-investigation** — Crash is real and reproducible, but root cause needs confirmation: is this pure user error (threading violation) or does SkiaSharp's async/disposal path have a genuine bug that could be fixed with guards?

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/Android |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Launch MAUI app targeting Windows
2. Click 'Click me' button to open coordinate system dialog
3. Select '4326 WGS-84' and click OK — image displays
4. Repeat steps 2-3; second time triggers AccessViolationException

**Environment:** SkiaSharp 2.88.3, .NET MAUI, Windows 11 / Android (Samsung), Visual Studio

**Repository links:**
- https://github.com/gktval/SkiaSharpError/tree/main/SkiaSharpAccessViolation — Reporter's reproduction project

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | AccessViolationException during SKBitmap.Copy() in async/multithreaded context |
| Repro quality | partial |
| Target frameworks | net7.0-windows10.0.19041, net7.0-android32 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKBitmap threading model has not changed; the underlying Skia threading constraints are the same across 2.x versions. |

## Analysis

### Technical Summary

The crash is almost certainly a threading violation: SKBitmap (and all Skia objects) are not thread-safe, and async/await continuations in .NET can resume on any thread pool thread. The reporter's own follow-up confirms this — making the code synchronous eliminates the error entirely. Larger bitmaps expose the race more readily because allocation takes longer. The root cause is likely that a single SKBitmap instance is accessed concurrently across async continuation threads, or that a bitmap is disposed on one thread while a pixel-level copy operation is still in progress on another.

### Rationale

The crash (AccessViolationException) maps squarely to the documented SkiaSharp threading anti-pattern: sharing mutable Skia objects across threads. The reporter's comment that removing `await` fixes the issue is a strong signal. SKBitmap.Copy() calls PeekPixels() to access raw pixel memory — concurrent reads/writes to that memory from different threads will cause native-level AV. Classification is type/bug rather than type/question because there may be a genuine SkiaSharp issue with safe disposal under concurrent async access, but investigation is needed to distinguish user error from library bug.

### Key Signals

- "If I make the method synchronous by taking out the await, then I cannot reproduce the error" — **comment #1** (Strong indicator of a threading violation — async continuation resumes on different thread, causing concurrent native object access.)
- "I can more easily reproduce the error if I create larger bitmaps (such as 1200x1000 as compared to 256x256)" — **comment #1** (Larger allocations take longer, widening the race window between threads.)
- "I have had the problem occur with both Windows and Android" — **issue body** (Cross-platform crash rules out a platform-specific driver or window-system bug; points to native Skia memory access violation.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKBitmap.cs` | 160-173 | direct | SKBitmap.Copy() creates a new SKBitmap and calls CopyTo(), which calls PeekPixels() to get a direct pointer to pixel memory. No thread synchronization is present. If a second thread accesses or frees the source bitmap concurrently, the native pixel pointer becomes dangling. |
| `binding/SkiaSharp/SKBitmap.cs` | 183-200 | direct | CopyTo() uses `using var srcPixmap = PeekPixels()` — the pixmap holds a raw pointer into the bitmap's pixel buffer. If the source bitmap is modified or disposed on another thread while this executes, an AV can occur. |

### Workarounds

- Wrap all SKBitmap creation, copy, and drawing operations in a single non-async method or synchronize access with a lock/SemaphoreSlim to prevent concurrent native object access.
- Avoid sharing SKBitmap instances across async continuations; create a fresh SKBitmap on each use and dispose immediately after.
- Use ConfigureAwait(true) to ensure async continuations resume on the original (UI) thread when manipulating Skia objects.

### Next Questions

- Is the same SKBitmap instance being accessed from multiple threads, or are separate instances being created per-thread?
- Is there a finalizer-triggered disposal racing with the async Copy() call?
- Does the crash reproduce with the latest 2.88.x release?

### Resolution Proposals

**Hypothesis:** A single SKBitmap instance is being accessed concurrently from multiple thread-pool threads due to async/await continuations, causing a native-level race condition in pixel memory access inside SKBitmap.CopyTo().

1. **Synchronize SKBitmap access with SemaphoreSlim** — workaround, confidence 0.85 (85%), cost/s, validated=untested
   - Wrap all Skia operations that touch the same bitmap in a SemaphoreSlim(1,1) critical section so only one thread accesses the native object at a time.
2. **Don't share SKBitmap across async boundaries** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Ensure SKBitmap is created, used, and disposed entirely within a single synchronous code path; pass data (e.g., byte arrays) across async boundaries instead of the Skia object itself.

**Recommended proposal:** Don't share SKBitmap across async boundaries

**Why:** Highest confidence, avoids threading complexity altogether, aligns with Skia's documented non-thread-safe design.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.78 (78%) |
| Reason | Crash is real and reproducible, but root cause needs confirmation: is this pure user error (threading violation) or does SkiaSharp's async/disposal path have a genuine bug that could be fixed with guards? |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, area, platform, and tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, os/Android, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Post analysis noting threading root cause and suggest workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed follow-up — the key clue is that **removing `await` fixes the crash**. This strongly points to a threading violation: Skia (and therefore SkiaSharp) objects are **not thread-safe**, and async/await continuations in .NET can resume on any thread-pool thread. When `SKBitmap.Copy()` runs concurrently with another thread that also touches the same bitmap (or disposes it), the native pixel buffer access becomes a race condition that causes an `AccessViolationException`.

**Immediate workarounds:**
1. Don't share `SKBitmap` instances across async boundaries. Create, use, and dispose a bitmap entirely within a single synchronous call.
2. If you must use async, protect all Skia object access with a `SemaphoreSlim(1,1)` lock.
3. Pass raw data (e.g., `byte[]`) across async boundaries rather than live Skia objects.

We'll investigate whether SkiaSharp can add defensive checks or documentation to make this failure mode clearer.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2456,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T09:01:41Z"
  },
  "summary": "Reporter receives an AccessViolationException when creating and copying an SKBitmap inside async/multithreaded MAUI code on Windows and Android with SkiaSharp 2.88.3; making the code synchronous eliminates the crash.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic",
      "os/Android"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "AccessViolationException during SKBitmap.Copy() in async/multithreaded context",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-windows10.0.19041",
        "net7.0-android32"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Launch MAUI app targeting Windows",
        "Click 'Click me' button to open coordinate system dialog",
        "Select '4326 WGS-84' and click OK — image displays",
        "Repeat steps 2-3; second time triggers AccessViolationException"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET MAUI, Windows 11 / Android (Samsung), Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/gktval/SkiaSharpError/tree/main/SkiaSharpAccessViolation",
          "description": "Reporter's reproduction project"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKBitmap threading model has not changed; the underlying Skia threading constraints are the same across 2.x versions."
    }
  },
  "analysis": {
    "summary": "The crash is almost certainly a threading violation: SKBitmap (and all Skia objects) are not thread-safe, and async/await continuations in .NET can resume on any thread pool thread. The reporter's own follow-up confirms this — making the code synchronous eliminates the error entirely. Larger bitmaps expose the race more readily because allocation takes longer. The root cause is likely that a single SKBitmap instance is accessed concurrently across async continuation threads, or that a bitmap is disposed on one thread while a pixel-level copy operation is still in progress on another.",
    "rationale": "The crash (AccessViolationException) maps squarely to the documented SkiaSharp threading anti-pattern: sharing mutable Skia objects across threads. The reporter's comment that removing `await` fixes the issue is a strong signal. SKBitmap.Copy() calls PeekPixels() to access raw pixel memory — concurrent reads/writes to that memory from different threads will cause native-level AV. Classification is type/bug rather than type/question because there may be a genuine SkiaSharp issue with safe disposal under concurrent async access, but investigation is needed to distinguish user error from library bug.",
    "keySignals": [
      {
        "text": "If I make the method synchronous by taking out the await, then I cannot reproduce the error",
        "source": "comment #1",
        "interpretation": "Strong indicator of a threading violation — async continuation resumes on different thread, causing concurrent native object access."
      },
      {
        "text": "I can more easily reproduce the error if I create larger bitmaps (such as 1200x1000 as compared to 256x256)",
        "source": "comment #1",
        "interpretation": "Larger allocations take longer, widening the race window between threads."
      },
      {
        "text": "I have had the problem occur with both Windows and Android",
        "source": "issue body",
        "interpretation": "Cross-platform crash rules out a platform-specific driver or window-system bug; points to native Skia memory access violation."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "160-173",
        "finding": "SKBitmap.Copy() creates a new SKBitmap and calls CopyTo(), which calls PeekPixels() to get a direct pointer to pixel memory. No thread synchronization is present. If a second thread accesses or frees the source bitmap concurrently, the native pixel pointer becomes dangling.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKBitmap.cs",
        "lines": "183-200",
        "finding": "CopyTo() uses `using var srcPixmap = PeekPixels()` — the pixmap holds a raw pointer into the bitmap's pixel buffer. If the source bitmap is modified or disposed on another thread while this executes, an AV can occur.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Wrap all SKBitmap creation, copy, and drawing operations in a single non-async method or synchronize access with a lock/SemaphoreSlim to prevent concurrent native object access.",
      "Avoid sharing SKBitmap instances across async continuations; create a fresh SKBitmap on each use and dispose immediately after.",
      "Use ConfigureAwait(true) to ensure async continuations resume on the original (UI) thread when manipulating Skia objects."
    ],
    "nextQuestions": [
      "Is the same SKBitmap instance being accessed from multiple threads, or are separate instances being created per-thread?",
      "Is there a finalizer-triggered disposal racing with the async Copy() call?",
      "Does the crash reproduce with the latest 2.88.x release?"
    ],
    "resolution": {
      "hypothesis": "A single SKBitmap instance is being accessed concurrently from multiple thread-pool threads due to async/await continuations, causing a native-level race condition in pixel memory access inside SKBitmap.CopyTo().",
      "proposals": [
        {
          "title": "Synchronize SKBitmap access with SemaphoreSlim",
          "description": "Wrap all Skia operations that touch the same bitmap in a SemaphoreSlim(1,1) critical section so only one thread accesses the native object at a time.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Don't share SKBitmap across async boundaries",
          "description": "Ensure SKBitmap is created, used, and disposed entirely within a single synchronous code path; pass data (e.g., byte arrays) across async boundaries instead of the Skia object itself.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Don't share SKBitmap across async boundaries",
      "recommendedReason": "Highest confidence, avoids threading complexity altogether, aligns with Skia's documented non-thread-safe design."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.78,
      "reason": "Crash is real and reproducible, but root cause needs confirmation: is this pure user error (threading violation) or does SkiaSharp's async/disposal path have a genuine bug that could be fixed with guards?",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/Android",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis noting threading root cause and suggest workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed follow-up — the key clue is that **removing `await` fixes the crash**. This strongly points to a threading violation: Skia (and therefore SkiaSharp) objects are **not thread-safe**, and async/await continuations in .NET can resume on any thread-pool thread. When `SKBitmap.Copy()` runs concurrently with another thread that also touches the same bitmap (or disposes it), the native pixel buffer access becomes a race condition that causes an `AccessViolationException`.\n\n**Immediate workarounds:**\n1. Don't share `SKBitmap` instances across async boundaries. Create, use, and dispose a bitmap entirely within a single synchronous call.\n2. If you must use async, protect all Skia object access with a `SemaphoreSlim(1,1)` lock.\n3. Pass raw data (e.g., `byte[]`) across async boundaries rather than live Skia objects.\n\nWe'll investigate whether SkiaSharp can add defensive checks or documentation to make this failure mode clearer."
      }
    ]
  }
}
```

</details>
