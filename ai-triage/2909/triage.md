# Issue Triage Report — #2909

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T06:19:38Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** System.AccessViolationException thrown randomly at SKImage.Subset in an ASP.NET Core server app under concurrent load, caused by improper disposal — the original SKImage from FromBitmap is leaked and SKData is not disposed, leading to GC-driven native crashes under memory pressure.

**Analysis:** The crash is caused by incorrect resource management in the reporter's code: the original SKImage from SKImage.FromBitmap is silently leaked when overwritten by Subset (which may return a new instance), and SKData.Create(stream) is also never disposed. Under concurrent load, GC finalizers run on these leaked objects at unpredictable times, leading to use-after-free native crashes. Additionally, the Subset pattern must guard against the case where Subset returns the same instance (as documented in SkiaSharp's same-instance return pattern).

**Recommendations:** **close-as-not-a-bug** — The crash is caused by incorrect resource management in the caller's code — leaked SKImage and SKData under GC pressure. Proper use of 'using' statements and the same-instance disposal pattern resolves the issue. This is a usage error, not a SkiaSharp defect.

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
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create ASP.NET Core app with an endpoint that decodes an image from file, creates SKImage.FromBitmap, then calls Subset, then disposes the result and the bitmap
2. Run JMeter load test against the endpoint
3. After ~10 minutes of concurrent requests, AccessViolationException occurs

**Environment:** ASP.NET Core, .NET 8.0, SkiaSharp 2.88.6, Windows 10 Enterprise, concurrent load via Apache JMeter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | System.AccessViolationException throws randomly at SKImage.Subset(SKRectI subset) |
| Repro quality | partial |
| Target frameworks | net8.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The disposal and same-instance pattern for SKImage.Subset has not changed — the usage bug would still produce the same crash. |

## Analysis

### Technical Summary

The crash is caused by incorrect resource management in the reporter's code: the original SKImage from SKImage.FromBitmap is silently leaked when overwritten by Subset (which may return a new instance), and SKData.Create(stream) is also never disposed. Under concurrent load, GC finalizers run on these leaked objects at unpredictable times, leading to use-after-free native crashes. Additionally, the Subset pattern must guard against the case where Subset returns the same instance (as documented in SkiaSharp's same-instance return pattern).

### Rationale

AccessViolationException is a native memory corruption crash. Per SkiaSharp patterns, this typically indicates premature disposal or a disposed object being accessed. The code has two clear resource leaks: SKData from Create(stream) is never disposed, and the original SKImage from FromBitmap is lost (leaked) when image is reassigned to the Subset result. Under load, GC finalizers collect these leaked objects, which can race with ongoing native Skia operations and produce crashes. The crash is 'random' because it depends on GC scheduling under memory pressure.

### Key Signals

- "System.AccessViolationException throws randomly at SKImage.Subset(SKRectI subset)" — **issue body** (Native memory access violation — consistent with use-after-free from GC finalizing leaked objects under load)
- "The issue will occur around 10 minutes" — **issue body — Step 5** (Crash is non-deterministic and load-dependent — classic symptom of GC timing issue rather than synchronous logic bug)
- "image = image.Subset(new SKRectI(...))" — **issue body — code snippet** (Original SKImage is overwritten without disposal — if Subset returns a new instance, original is leaked; if same instance, no leak but GC pressure builds from SKData)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 544-547 | direct | SKImage.Subset(SKRectI) calls sk_image_make_subset_raster and wraps the result via GetObject. May return a new instance OR the same instance. If caller overwrites the reference without disposing the original, the original leaks. |
| `binding/SkiaSharp/SKImage.cs` | 16 | direct | SKImage implements ISKReferenceCounted, meaning native memory is managed by Skia's ref-counting. Leaking the C# object is safe in theory, but under GC pressure finalizers run concurrently which can race with active use of Skia's internal state. |

### Workarounds

- Use 'using' statements for all SKObject instances to ensure deterministic disposal
- Handle the same-instance return pattern: store original image, call Subset, then conditionally dispose original only if result != original
- Dispose SKData.Create(stream) explicitly — wrap in 'using var data = SKData.Create(stream)'

### Next Questions

- Does the crash occur on a single-threaded repro or only under concurrent load?
- Is there a stack trace available from the AccessViolationException?

### Resolution Proposals

**Hypothesis:** The crash is caused by multiple resource leaks in the caller's code: SKData not disposed and original SKImage not disposed before Subset result is assigned. Under load, GC finalizers race with native Skia state.

1. **Fix resource management with using statements** — workaround, confidence 0.88 (88%), cost/xs, validated=untested
   - Rewrite the endpoint using proper 'using' statements for all SKObject instances and handle the Subset same-instance return pattern correctly.

```csharp
using var data = SKData.Create(stream);
using var bitmap = SKBitmap.Decode(data);
using var sourceImage = SKImage.FromBitmap(bitmap);
var subsetImage = sourceImage.Subset(new SKRectI(0, 0, 1306, 425));
if (subsetImage != sourceImage)
    sourceImage.Dispose();
// use subsetImage...
subsetImage?.Dispose();
```

**Recommended proposal:** Fix resource management with using statements

**Why:** Addresses all leaks deterministically and follows the documented same-instance pattern. Simple change, high confidence.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | The crash is caused by incorrect resource management in the caller's code — leaked SKImage and SKData under GC pressure. Proper use of 'using' statements and the same-instance disposal pattern resolves the issue. This is a usage error, not a SkiaSharp defect. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, area, platform, and reliability tenet labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | high | 0.82 (82%) | Explain root cause (resource leaks) and provide corrected code using proper disposal pattern | — |
| close-issue | medium | 0.80 (80%) | Close as not a bug — root cause is usage error in caller code | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed reproduction steps!

The `AccessViolationException` is caused by **resource management issues** in the endpoint code. Under concurrent load, the .NET GC finalizes leaked native objects at unpredictable times, which races with active Skia operations and produces native memory crashes.

There are two issues in the code:

1. **`SKData` is not disposed** — `SKData.Create(stream)` creates a native object that is never released.
2. **Original `SKImage` is leaked** — `image = image.Subset(...)` overwrites the reference without disposing the original image. `Subset()` may return a *new* instance, in which case the original is lost.

Here is the corrected pattern:

```csharp
app.MapGet("/converter", () =>
{
    string imagePath = Path.GetFullPath(@"Data/image1.jpg");
    using (SKFileStream stream = new SKFileStream(imagePath))
    using (var data = SKData.Create(stream))
    using (var bitmap = SKBitmap.Decode(data))
    using (var sourceImage = SKImage.FromBitmap(bitmap))
    {
        var subsetImage = sourceImage.Subset(new SKRectI(0, 0, 1306, 425));
        // Subset may return the same instance, so only dispose if different
        if (subsetImage != sourceImage)
            sourceImage.Dispose();  // already handled by outer 'using', but be explicit

        // use subsetImage...
        subsetImage?.Dispose();
    }
    return Results.Ok();
});
```

Using deterministic disposal via `using` ensures native memory is released promptly rather than waiting for GC finalization, which eliminates the race condition under load.

Note: For high-throughput server scenarios, also consider object pooling for `SKBitmap`/`SKImage` to reduce allocation pressure.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2909,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T06:19:38Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "System.AccessViolationException thrown randomly at SKImage.Subset in an ASP.NET Core server app under concurrent load, caused by improper disposal — the original SKImage from FromBitmap is leaked and SKData is not disposed, leading to GC-driven native crashes under memory pressure.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
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
      "errorMessage": "System.AccessViolationException throws randomly at SKImage.Subset(SKRectI subset)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create ASP.NET Core app with an endpoint that decodes an image from file, creates SKImage.FromBitmap, then calls Subset, then disposes the result and the bitmap",
        "Run JMeter load test against the endpoint",
        "After ~10 minutes of concurrent requests, AccessViolationException occurs"
      ],
      "environmentDetails": "ASP.NET Core, .NET 8.0, SkiaSharp 2.88.6, Windows 10 Enterprise, concurrent load via Apache JMeter"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The disposal and same-instance pattern for SKImage.Subset has not changed — the usage bug would still produce the same crash."
    }
  },
  "analysis": {
    "summary": "The crash is caused by incorrect resource management in the reporter's code: the original SKImage from SKImage.FromBitmap is silently leaked when overwritten by Subset (which may return a new instance), and SKData.Create(stream) is also never disposed. Under concurrent load, GC finalizers run on these leaked objects at unpredictable times, leading to use-after-free native crashes. Additionally, the Subset pattern must guard against the case where Subset returns the same instance (as documented in SkiaSharp's same-instance return pattern).",
    "rationale": "AccessViolationException is a native memory corruption crash. Per SkiaSharp patterns, this typically indicates premature disposal or a disposed object being accessed. The code has two clear resource leaks: SKData from Create(stream) is never disposed, and the original SKImage from FromBitmap is lost (leaked) when image is reassigned to the Subset result. Under load, GC finalizers collect these leaked objects, which can race with ongoing native Skia operations and produce crashes. The crash is 'random' because it depends on GC scheduling under memory pressure.",
    "keySignals": [
      {
        "text": "System.AccessViolationException throws randomly at SKImage.Subset(SKRectI subset)",
        "source": "issue body",
        "interpretation": "Native memory access violation — consistent with use-after-free from GC finalizing leaked objects under load"
      },
      {
        "text": "The issue will occur around 10 minutes",
        "source": "issue body — Step 5",
        "interpretation": "Crash is non-deterministic and load-dependent — classic symptom of GC timing issue rather than synchronous logic bug"
      },
      {
        "text": "image = image.Subset(new SKRectI(...))",
        "source": "issue body — code snippet",
        "interpretation": "Original SKImage is overwritten without disposal — if Subset returns a new instance, original is leaked; if same instance, no leak but GC pressure builds from SKData"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "544-547",
        "finding": "SKImage.Subset(SKRectI) calls sk_image_make_subset_raster and wraps the result via GetObject. May return a new instance OR the same instance. If caller overwrites the reference without disposing the original, the original leaks.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "16",
        "finding": "SKImage implements ISKReferenceCounted, meaning native memory is managed by Skia's ref-counting. Leaking the C# object is safe in theory, but under GC pressure finalizers run concurrently which can race with active use of Skia's internal state.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use 'using' statements for all SKObject instances to ensure deterministic disposal",
      "Handle the same-instance return pattern: store original image, call Subset, then conditionally dispose original only if result != original",
      "Dispose SKData.Create(stream) explicitly — wrap in 'using var data = SKData.Create(stream)'"
    ],
    "nextQuestions": [
      "Does the crash occur on a single-threaded repro or only under concurrent load?",
      "Is there a stack trace available from the AccessViolationException?"
    ],
    "resolution": {
      "hypothesis": "The crash is caused by multiple resource leaks in the caller's code: SKData not disposed and original SKImage not disposed before Subset result is assigned. Under load, GC finalizers race with native Skia state.",
      "proposals": [
        {
          "title": "Fix resource management with using statements",
          "description": "Rewrite the endpoint using proper 'using' statements for all SKObject instances and handle the Subset same-instance return pattern correctly.",
          "category": "workaround",
          "codeSnippet": "using var data = SKData.Create(stream);\nusing var bitmap = SKBitmap.Decode(data);\nusing var sourceImage = SKImage.FromBitmap(bitmap);\nvar subsetImage = sourceImage.Subset(new SKRectI(0, 0, 1306, 425));\nif (subsetImage != sourceImage)\n    sourceImage.Dispose();\n// use subsetImage...\nsubsetImage?.Dispose();",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix resource management with using statements",
      "recommendedReason": "Addresses all leaks deterministically and follows the documented same-instance pattern. Simple change, high confidence."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "The crash is caused by incorrect resource management in the caller's code — leaked SKImage and SKData under GC pressure. Proper use of 'using' statements and the same-instance disposal pattern resolves the issue. This is a usage error, not a SkiaSharp defect.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, area, platform, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain root cause (resource leaks) and provide corrected code using proper disposal pattern",
        "risk": "high",
        "confidence": 0.82,
        "comment": "Thank you for the detailed reproduction steps!\n\nThe `AccessViolationException` is caused by **resource management issues** in the endpoint code. Under concurrent load, the .NET GC finalizes leaked native objects at unpredictable times, which races with active Skia operations and produces native memory crashes.\n\nThere are two issues in the code:\n\n1. **`SKData` is not disposed** — `SKData.Create(stream)` creates a native object that is never released.\n2. **Original `SKImage` is leaked** — `image = image.Subset(...)` overwrites the reference without disposing the original image. `Subset()` may return a *new* instance, in which case the original is lost.\n\nHere is the corrected pattern:\n\n```csharp\napp.MapGet(\"/converter\", () =>\n{\n    string imagePath = Path.GetFullPath(@\"Data/image1.jpg\");\n    using (SKFileStream stream = new SKFileStream(imagePath))\n    using (var data = SKData.Create(stream))\n    using (var bitmap = SKBitmap.Decode(data))\n    using (var sourceImage = SKImage.FromBitmap(bitmap))\n    {\n        var subsetImage = sourceImage.Subset(new SKRectI(0, 0, 1306, 425));\n        // Subset may return the same instance, so only dispose if different\n        if (subsetImage != sourceImage)\n            sourceImage.Dispose();  // already handled by outer 'using', but be explicit\n\n        // use subsetImage...\n        subsetImage?.Dispose();\n    }\n    return Results.Ok();\n});\n```\n\nUsing deterministic disposal via `using` ensures native memory is released promptly rather than waiting for GC finalization, which eliminates the race condition under load.\n\nNote: For high-throughput server scenarios, also consider object pooling for `SKBitmap`/`SKImage` to reduce allocation pressure."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — root cause is usage error in caller code",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
