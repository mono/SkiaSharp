# Issue Triage Report — #1227

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:30:00Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-fixed (0.78 (78%)) |

**Issue Summary:** Random unit test crashes (access violations) on Windows with many parallel threads in the dev/update-skia branch, caused by GC premature collection of P/Invoke objects and reference counting bugs in HandleDictionary and SKColorSpaceStatic.

**Analysis:** Multiple root causes for random test crashes under high parallelism: (1) GC premature collection of managed wrappers during active P/Invoke calls (e.g., SKTextBlobBuilder.Build()); (2) incorrect reference counting for singleton SKColorSpaceStatic objects in HandleDictionary; (3) a C API bug where SKColorSpace ref was not incremented when passing ImageInfo into native code in m80 vs m68. All three were fixed by the maintainer.

**Recommendations:** **close-as-fixed** — Maintainer confirmed specific fixes were committed. Code investigation confirms GC.KeepAlive added to SKTextBlobBuilder.Build(), SKColorSpaceStatic ownership corrected to owns=false, and C API ref counting patched. Issue is from 2020 and the specific bugs have been addressed.

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
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Build dev/update-skia branch on Windows
2. Run dotnet test with many parallel threads (32 hyperthreads on AMD 3950x)
3. Observe random access violations or InvalidOperationException about unreferencing objects

**Environment:** Windows 10, VS2019, AMD Ryzen 3950x (32 hyperthreads)

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/1237 — PR fixing reference counting in HandleDictionary for SKColorSpaceStatic

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | About to unreference an object that has no references. H: 1bf6db1a700 Type: SkiaSharp.SKColorSpace+SKColorSpaceStatic |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Fixes were applied: GC.KeepAlive added to SKTextBlobBuilder.Build() and other methods, SKColorSpaceStatic now uses owns=false, and the C API ref counting bug in sk_types_priv.h was corrected by the maintainer. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.80 (80%) |
| Reason | Maintainer explicitly stated fixes were committed in 1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1 and mono/skia@47de7965. Code investigation confirms GC.KeepAlive(this) added to SKTextBlobBuilder.Build() and SKColorSpaceStatic now correctly uses owns=false. |
| Related PRs | — |
| Related commits | 1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1, 47de7965be076311e824589b0464a4e1e08d54eb |
| Fixed in version | — |

## Analysis

### Technical Summary

Multiple root causes for random test crashes under high parallelism: (1) GC premature collection of managed wrappers during active P/Invoke calls (e.g., SKTextBlobBuilder.Build()); (2) incorrect reference counting for singleton SKColorSpaceStatic objects in HandleDictionary; (3) a C API bug where SKColorSpace ref was not incremented when passing ImageInfo into native code in m80 vs m68. All three were fixed by the maintainer.

### Rationale

Issue is clearly a threading/GC race condition triggered during parallel test execution. Multiple specific root causes were identified and fixed: GC.KeepAlive added to affected methods, SKColorSpaceStatic ownership corrected to owns=false, and C API ref counting in sk_types_priv.h was patched. The issue should be closeable as fixed.

### Key Signals

- "About to unreference an object that has no references. H: 1bf6db1a700 Type: SkiaSharp.SKColorSpace+SKColorSpaceStatic" — **comment by ziriax** (Singleton ColorSpace object reference count dropping to zero unexpectedly — reference counting bug in HandleDictionary.)
- "Fixed in 1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1 and https://github.com/mono/skia/commit/47de7965be076311e824589b0464a4e1e08d54eb" — **comment by mattleibow** (Maintainer confirmed the specific C API and managed code fixes were committed.)
- "the `this` object sometimes gets collected while a method is running" — **comment by mattleibow** (GC can collect managed wrapper while P/Invoke is executing — missing GC.KeepAlive pattern.)
- "m80: NO REF on the way in — sk_sp<SkColorSpace>(AsColorSpace(info->colorspace)))" — **comment by mattleibow** (C API regression between m68 and m80: colorspace ref no longer incremented when passing ImageInfo into native, causing double-free.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTextBlob.cs` | 282-287 | direct | SKTextBlobBuilder.Build() now includes GC.KeepAlive(this) after the P/Invoke call, preventing premature GC collection of the builder object while native code executes. |
| `binding/SkiaSharp/SKColorSpace.cs` | 164-172 | direct | SKColorSpaceStatic constructor now uses base(x, false) — owns=false — which prevents incorrect reference count decrements for these static singleton objects. |
| `binding/SkiaSharp/HandleDictionary.cs` | 74-98 | related | GetOrAddObject still calls SafeUnRef on existing ISKReferenceCounted instances when unrefExisting=true. With THROW_OBJECT_EXCEPTIONS enabled, a check prevents unreferencing when count is already 1. The core logic is unchanged but the singleton ownership fix prevents the problematic path. |

### Next Questions

- Are there other native wrapper methods still missing GC.KeepAlive(this) after P/Invoke calls?
- Is the wglChoosePixelFormatARB deadlock (reported by ziriax) a known Windows/driver limitation or should it be separately tracked?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.78 (78%) |
| Reason | Maintainer confirmed specific fixes were committed. Code investigation confirms GC.KeepAlive added to SKTextBlobBuilder.Build(), SKColorSpaceStatic ownership corrected to owns=false, and C API ref counting patched. Issue is from 2020 and the specific bugs have been addressed. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Existing labels are correct — type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability already applied | labels=type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability |
| add-comment | high | 0.78 (78%) | Comment summarizing the fixes applied and asking reporter to confirm resolution | — |
| close-issue | medium | 0.75 (75%) | Close as fixed since root causes were addressed by maintainer commits | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
The specific root causes identified in this issue have been addressed in the codebase:

1. **GC premature collection**: `GC.KeepAlive(this)` was added to `SKTextBlobBuilder.Build()` and other affected methods to prevent the managed wrapper from being collected during P/Invoke execution.
2. **SKColorSpaceStatic reference counting**: `SKColorSpaceStatic` now uses `owns=false`, preventing incorrect reference count decrements for singleton color space objects.
3. **C API ref counting**: The m80 C API bug in `sk_types_priv.h` where `SKColorSpace` was not ref-incremented when passing `ImageInfo` into native code was fixed.

If you are still experiencing crashes on a current release, please reopen with your SkiaSharp version and updated repro steps.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1227,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:30:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "Random unit test crashes (access violations) on Windows with many parallel threads in the dev/update-skia branch, caused by GC premature collection of P/Invoke objects and reference counting bugs in HandleDictionary and SKColorSpaceStatic.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "About to unreference an object that has no references. H: 1bf6db1a700 Type: SkiaSharp.SKColorSpace+SKColorSpaceStatic",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Build dev/update-skia branch on Windows",
        "Run dotnet test with many parallel threads (32 hyperthreads on AMD 3950x)",
        "Observe random access violations or InvalidOperationException about unreferencing objects"
      ],
      "environmentDetails": "Windows 10, VS2019, AMD Ryzen 3950x (32 hyperthreads)",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1237",
          "description": "PR fixing reference counting in HandleDictionary for SKColorSpaceStatic"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "unlikely",
      "relevanceReason": "Fixes were applied: GC.KeepAlive added to SKTextBlobBuilder.Build() and other methods, SKColorSpaceStatic now uses owns=false, and the C API ref counting bug in sk_types_priv.h was corrected by the maintainer."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.8,
      "reason": "Maintainer explicitly stated fixes were committed in 1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1 and mono/skia@47de7965. Code investigation confirms GC.KeepAlive(this) added to SKTextBlobBuilder.Build() and SKColorSpaceStatic now correctly uses owns=false.",
      "relatedCommits": [
        "1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1",
        "47de7965be076311e824589b0464a4e1e08d54eb"
      ]
    }
  },
  "analysis": {
    "summary": "Multiple root causes for random test crashes under high parallelism: (1) GC premature collection of managed wrappers during active P/Invoke calls (e.g., SKTextBlobBuilder.Build()); (2) incorrect reference counting for singleton SKColorSpaceStatic objects in HandleDictionary; (3) a C API bug where SKColorSpace ref was not incremented when passing ImageInfo into native code in m80 vs m68. All three were fixed by the maintainer.",
    "rationale": "Issue is clearly a threading/GC race condition triggered during parallel test execution. Multiple specific root causes were identified and fixed: GC.KeepAlive added to affected methods, SKColorSpaceStatic ownership corrected to owns=false, and C API ref counting in sk_types_priv.h was patched. The issue should be closeable as fixed.",
    "keySignals": [
      {
        "text": "About to unreference an object that has no references. H: 1bf6db1a700 Type: SkiaSharp.SKColorSpace+SKColorSpaceStatic",
        "source": "comment by ziriax",
        "interpretation": "Singleton ColorSpace object reference count dropping to zero unexpectedly — reference counting bug in HandleDictionary."
      },
      {
        "text": "Fixed in 1590d7e1e81581e1c4ab1252b7d3dcb563d1a8e1 and https://github.com/mono/skia/commit/47de7965be076311e824589b0464a4e1e08d54eb",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmed the specific C API and managed code fixes were committed."
      },
      {
        "text": "the `this` object sometimes gets collected while a method is running",
        "source": "comment by mattleibow",
        "interpretation": "GC can collect managed wrapper while P/Invoke is executing — missing GC.KeepAlive pattern."
      },
      {
        "text": "m80: NO REF on the way in — sk_sp<SkColorSpace>(AsColorSpace(info->colorspace)))",
        "source": "comment by mattleibow",
        "interpretation": "C API regression between m68 and m80: colorspace ref no longer incremented when passing ImageInfo into native, causing double-free."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTextBlob.cs",
        "lines": "282-287",
        "finding": "SKTextBlobBuilder.Build() now includes GC.KeepAlive(this) after the P/Invoke call, preventing premature GC collection of the builder object while native code executes.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColorSpace.cs",
        "lines": "164-172",
        "finding": "SKColorSpaceStatic constructor now uses base(x, false) — owns=false — which prevents incorrect reference count decrements for these static singleton objects.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/HandleDictionary.cs",
        "lines": "74-98",
        "finding": "GetOrAddObject still calls SafeUnRef on existing ISKReferenceCounted instances when unrefExisting=true. With THROW_OBJECT_EXCEPTIONS enabled, a check prevents unreferencing when count is already 1. The core logic is unchanged but the singleton ownership fix prevents the problematic path.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Are there other native wrapper methods still missing GC.KeepAlive(this) after P/Invoke calls?",
      "Is the wglChoosePixelFormatARB deadlock (reported by ziriax) a known Windows/driver limitation or should it be separately tracked?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.78,
      "reason": "Maintainer confirmed specific fixes were committed. Code investigation confirms GC.KeepAlive added to SKTextBlobBuilder.Build(), SKColorSpaceStatic ownership corrected to owns=false, and C API ref counting patched. Issue is from 2020 and the specific bugs have been addressed.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Existing labels are correct — type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability already applied",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "os/Windows-Classic",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Comment summarizing the fixes applied and asking reporter to confirm resolution",
        "risk": "high",
        "confidence": 0.78,
        "comment": "The specific root causes identified in this issue have been addressed in the codebase:\n\n1. **GC premature collection**: `GC.KeepAlive(this)` was added to `SKTextBlobBuilder.Build()` and other affected methods to prevent the managed wrapper from being collected during P/Invoke execution.\n2. **SKColorSpaceStatic reference counting**: `SKColorSpaceStatic` now uses `owns=false`, preventing incorrect reference count decrements for singleton color space objects.\n3. **C API ref counting**: The m80 C API bug in `sk_types_priv.h` where `SKColorSpace` was not ref-incremented when passing `ImageInfo` into native code was fixed.\n\nIf you are still experiencing crashes on a current release, please reopen with your SkiaSharp version and updated repro steps."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed since root causes were addressed by maintainer commits",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
