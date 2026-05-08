# Issue Triage Report — #1223

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T17:44:06Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.75 (75%)) |

**Issue Summary:** Reporter experiences an AccessViolationException when drawing polygons and text concurrently using DrawPath and DrawTextOnPath across multiple threads in release mode on SkiaSharp 1.68.

**Analysis:** Access violation from GC prematurely collecting SkiaSharp objects (SKTypeface, SKPath, SKPaint) while they are still referenced by in-flight native calls in a multithreaded, release-optimized build. Maintainer linked this to issue #1220 (same crash class), which was fixed in v1.68.2 by adding GC lifecycle guards.

**Recommendations:** **needs-info** — Maintainer linked the crash to GC issue #1220 (fixed in v1.68.2) and suggested a preview build. Reporter never confirmed whether the fix resolved it. Ask the reporter to verify on current release before closing.

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

1. Use SkiaSharp 1.68 in a multithreaded application
2. Create SKPath objects and draw them using DrawPath in multiple threads simultaneously
3. Mix polygon drawing (DrawPath) and text-on-path drawing (DrawTextOnPath) in the same concurrent workload
4. Run in Release configuration (not Debug)
5. Observe AccessViolationException — more likely with dense data

**Environment:** SkiaSharp 1.68, release mode, multithreaded polygon + text drawing

**Related issues:** #1220

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1220 — Related multithreaded GC crash with SKShaper/SKTypeface — fixed in v1.68.2

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Access violation exception when calling DrawPath/DrawTextOnPath in multithreaded release build |
| Repro quality | none |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Maintainer attributed the crash class to GC collections. Companion issue #1220 (same symptom class) was fixed in v1.68.2. Current codebase is significantly newer (v3.x era) and has GC.KeepAlive guards in several areas. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.72 (72%) |
| Reason | Maintainer explicitly linked to #1220 as the likely culprit. Issue #1220 was closed as completed in milestone v1.68.2. Maintainer suggested preview.55 had the fix. Reporter never confirmed outcome. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 1.68.2 |

## Analysis

### Technical Summary

Access violation from GC prematurely collecting SkiaSharp objects (SKTypeface, SKPath, SKPaint) while they are still referenced by in-flight native calls in a multithreaded, release-optimized build. Maintainer linked this to issue #1220 (same crash class), which was fixed in v1.68.2 by adding GC lifecycle guards.

### Rationale

Reporter describes a hard crash (access violation) only in release mode — the classic .NET JIT optimizer allowing the GC to finalize objects whose managed handles were extracted before the native call completed. Maintainer corroborated: 'often due to some GC collections — I believe we have the general cases fixed in the new previews.' Related issue #1220 with identical symptom pattern (multithreaded, typeface handle nulled mid-call) was closed as fixed in v1.68.2. Reporter was on 1.68 and never confirmed whether updating resolved it.

### Key Signals

- "I use Draw path to draw polygons with multithreading... I get an access violation exception. This only happens in release and not in debug." — **issue body** (Release-only crash is the defining signal for GC optimizer interaction — debug builds disable many JIT optimizations that allow object liveness to be shortened.)
- "We have noticed a few of these - often due to some GC collections. I believe we have the general cases fixed in the new previews." — **comment by mattleibow** (Maintainer confirms root cause class (GC) and implies fix in 1.68.2-preview.)
- "The text... Might be https://github.com/mono/SkiaSharp/issues/1220" — **comment by mattleibow** (Maintainer directly links this to #1220 (multithreaded GC crash with SKTypeface), which was fixed in v1.68.2.)
- "It is quite difficult to produce a minimal case, as it is data specific." — **comment by reporter** (No minimal repro available — makes verification impossible without reporter confirming the fix.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKCanvas.cs` | 405-412 | direct | DrawPath extracts path.Handle and paint.Handle directly and passes them to the native call. No GC.KeepAlive guard. In release mode with aggressive JIT, the GC can consider the parameter objects dead after their .Handle property is read. Contrast: SKData, SKImage, and SKBitmap all have explicit GC.KeepAlive guards in similar patterns. |
| `binding/SkiaSharp/SKObject.cs` | 259-276 | direct | Dispose(bool) sets Handle = IntPtr.Zero after releasing native memory. If a GC finalizer runs concurrently while a native call holds the pointer value, the pointer value is already passed but the finalizer can free the underlying memory, causing the access violation. |
| `binding/SkiaSharp/SKData.cs` | 167-191 | related | SKData uses GC.KeepAlive(stream) explicitly to prevent premature GC collection of managed objects passed to native code — confirms this is a known pattern in the codebase and the fix class for #1220. |
| `binding/SkiaSharp/SKCanvas.cs` | 697-716 | related | DrawTextOnPath (the API the reporter also mentions) internally calls DrawPath, so it shares the same potential GC vulnerability for the path and font objects. |

### Workarounds

- Upgrade to SkiaSharp 1.68.2 or later where GC lifecycle fixes were applied.
- Ensure each thread creates and disposes its own SKCanvas, SKPath, and SKPaint objects independently — never share mutable SkiaSharp objects across threads.
- Hold explicit references to all SkiaSharp objects for the lifetime of the native operation (avoid letting objects go out of scope before the drawing call returns).
- In extreme cases, wrap drawing loops with GC.KeepAlive(path) and GC.KeepAlive(paint) after each DrawPath call as a local guard.

### Next Questions

- Did the reporter verify whether 1.68.2 or later resolved the crash?
- Is the crash still reproducible on current SkiaSharp (v2.x/v3.x)?
- Were any objects being shared across threads (e.g., a shared SKPaint or SKPath), or was each thread truly independent?

### Resolution Proposals

**Hypothesis:** GC collected SKPath or SKPaint objects mid-draw in a multithreaded release build. The 1.68.2 GC lifecycle fixes (same fix that resolved #1220) likely addressed the general case.

1. **Confirm resolution via 1.68.2+** — investigation, confidence 0.72 (72%), cost/xs, validated=untested
   - Ask the reporter to confirm whether upgrading to SkiaSharp 1.68.2 or later resolved the crash. Current codebase is v3.x and well past the fix.
2. **Workaround: thread-local SkiaSharp objects** — workaround, confidence 0.90 (90%), cost/s, validated=untested
   - Each thread must own and manage its own SKCanvas, SKPath, SKPaint, and SKFont objects. Do not share mutable SkiaSharp objects across threads.

**Recommended proposal:** Confirm resolution via 1.68.2+

**Why:** Maintainer already pointed at the fix in preview builds. Highest-value action is confirming the reporter is unblocked.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.75 (75%) |
| Reason | Maintainer linked the crash to GC issue #1220 (fixed in v1.68.2) and suggested a preview build. Reporter never confirmed whether the fix resolved it. Ask the reporter to verify on current release before closing. |
| Suggested repro platform | linux |

### Missing Info

- Confirmation that SkiaSharp 1.68.2+ (or current v2/v3 release) still exhibits the crash
- Whether objects were being shared across threads or each thread had its own independent objects
- Stack trace or crash log from the access violation

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply bug, core SkiaSharp, and reliability tenet labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.75 (75%) | Ask reporter to confirm whether the 1.68.2 GC fix resolved their issue, and provide threading guidance | — |
| link-related | low | 0.92 (92%) | Cross-reference the related GC fix issue #1220 | linkedIssue=#1220 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The symptoms here (release-only crash during multithreaded drawing) match a class of GC lifetime bugs that were addressed in **SkiaSharp 1.68.2** — see the related fix in #1220.

Could you confirm:
1. Did upgrading to 1.68.2 (or any later version) resolve the crash for you?
2. Are each of your threads creating their own independent `SKCanvas`, `SKPath`, and `SKPaint` objects, or are any of these shared between threads?

As a general threading rule: **SkiaSharp objects are not thread-safe**. Each thread must own and manage its own drawing objects. Sharing a `SKPaint` or `SKPath` across threads without synchronization can cause exactly this kind of crash.

If the issue still occurs on a current release (v2.x or v3.x), please share a stack trace or minimal repro so we can investigate further.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1223,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T17:44:06Z"
  },
  "summary": "Reporter experiences an AccessViolationException when drawing polygons and text concurrently using DrawPath and DrawTextOnPath across multiple threads in release mode on SkiaSharp 1.68.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Access violation exception when calling DrawPath/DrawTextOnPath in multithreaded release build",
      "reproQuality": "none",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Use SkiaSharp 1.68 in a multithreaded application",
        "Create SKPath objects and draw them using DrawPath in multiple threads simultaneously",
        "Mix polygon drawing (DrawPath) and text-on-path drawing (DrawTextOnPath) in the same concurrent workload",
        "Run in Release configuration (not Debug)",
        "Observe AccessViolationException — more likely with dense data"
      ],
      "environmentDetails": "SkiaSharp 1.68, release mode, multithreaded polygon + text drawing",
      "relatedIssues": [
        1220
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1220",
          "description": "Related multithreaded GC crash with SKShaper/SKTypeface — fixed in v1.68.2"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Maintainer attributed the crash class to GC collections. Companion issue #1220 (same symptom class) was fixed in v1.68.2. Current codebase is significantly newer (v3.x era) and has GC.KeepAlive guards in several areas."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.72,
      "reason": "Maintainer explicitly linked to #1220 as the likely culprit. Issue #1220 was closed as completed in milestone v1.68.2. Maintainer suggested preview.55 had the fix. Reporter never confirmed outcome.",
      "relatedPRs": [],
      "fixedInVersion": "1.68.2"
    }
  },
  "analysis": {
    "summary": "Access violation from GC prematurely collecting SkiaSharp objects (SKTypeface, SKPath, SKPaint) while they are still referenced by in-flight native calls in a multithreaded, release-optimized build. Maintainer linked this to issue #1220 (same crash class), which was fixed in v1.68.2 by adding GC lifecycle guards.",
    "rationale": "Reporter describes a hard crash (access violation) only in release mode — the classic .NET JIT optimizer allowing the GC to finalize objects whose managed handles were extracted before the native call completed. Maintainer corroborated: 'often due to some GC collections — I believe we have the general cases fixed in the new previews.' Related issue #1220 with identical symptom pattern (multithreaded, typeface handle nulled mid-call) was closed as fixed in v1.68.2. Reporter was on 1.68 and never confirmed whether updating resolved it.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "405-412",
        "finding": "DrawPath extracts path.Handle and paint.Handle directly and passes them to the native call. No GC.KeepAlive guard. In release mode with aggressive JIT, the GC can consider the parameter objects dead after their .Handle property is read. Contrast: SKData, SKImage, and SKBitmap all have explicit GC.KeepAlive guards in similar patterns.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "259-276",
        "finding": "Dispose(bool) sets Handle = IntPtr.Zero after releasing native memory. If a GC finalizer runs concurrently while a native call holds the pointer value, the pointer value is already passed but the finalizer can free the underlying memory, causing the access violation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "167-191",
        "finding": "SKData uses GC.KeepAlive(stream) explicitly to prevent premature GC collection of managed objects passed to native code — confirms this is a known pattern in the codebase and the fix class for #1220.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKCanvas.cs",
        "lines": "697-716",
        "finding": "DrawTextOnPath (the API the reporter also mentions) internally calls DrawPath, so it shares the same potential GC vulnerability for the path and font objects.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I use Draw path to draw polygons with multithreading... I get an access violation exception. This only happens in release and not in debug.",
        "source": "issue body",
        "interpretation": "Release-only crash is the defining signal for GC optimizer interaction — debug builds disable many JIT optimizations that allow object liveness to be shortened."
      },
      {
        "text": "We have noticed a few of these - often due to some GC collections. I believe we have the general cases fixed in the new previews.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirms root cause class (GC) and implies fix in 1.68.2-preview."
      },
      {
        "text": "The text... Might be https://github.com/mono/SkiaSharp/issues/1220",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer directly links this to #1220 (multithreaded GC crash with SKTypeface), which was fixed in v1.68.2."
      },
      {
        "text": "It is quite difficult to produce a minimal case, as it is data specific.",
        "source": "comment by reporter",
        "interpretation": "No minimal repro available — makes verification impossible without reporter confirming the fix."
      }
    ],
    "workarounds": [
      "Upgrade to SkiaSharp 1.68.2 or later where GC lifecycle fixes were applied.",
      "Ensure each thread creates and disposes its own SKCanvas, SKPath, and SKPaint objects independently — never share mutable SkiaSharp objects across threads.",
      "Hold explicit references to all SkiaSharp objects for the lifetime of the native operation (avoid letting objects go out of scope before the drawing call returns).",
      "In extreme cases, wrap drawing loops with GC.KeepAlive(path) and GC.KeepAlive(paint) after each DrawPath call as a local guard."
    ],
    "nextQuestions": [
      "Did the reporter verify whether 1.68.2 or later resolved the crash?",
      "Is the crash still reproducible on current SkiaSharp (v2.x/v3.x)?",
      "Were any objects being shared across threads (e.g., a shared SKPaint or SKPath), or was each thread truly independent?"
    ],
    "resolution": {
      "hypothesis": "GC collected SKPath or SKPaint objects mid-draw in a multithreaded release build. The 1.68.2 GC lifecycle fixes (same fix that resolved #1220) likely addressed the general case.",
      "proposals": [
        {
          "title": "Confirm resolution via 1.68.2+",
          "description": "Ask the reporter to confirm whether upgrading to SkiaSharp 1.68.2 or later resolved the crash. Current codebase is v3.x and well past the fix.",
          "category": "investigation",
          "confidence": 0.72,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: thread-local SkiaSharp objects",
          "description": "Each thread must own and manage its own SKCanvas, SKPath, SKPaint, and SKFont objects. Do not share mutable SkiaSharp objects across threads.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Confirm resolution via 1.68.2+",
      "recommendedReason": "Maintainer already pointed at the fix in preview builds. Highest-value action is confirming the reporter is unblocked."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.75,
      "reason": "Maintainer linked the crash to GC issue #1220 (fixed in v1.68.2) and suggested a preview build. Reporter never confirmed whether the fix resolved it. Ask the reporter to verify on current release before closing.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Confirmation that SkiaSharp 1.68.2+ (or current v2/v3 release) still exhibits the crash",
      "Whether objects were being shared across threads or each thread had its own independent objects",
      "Stack trace or crash log from the access violation"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core SkiaSharp, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter to confirm whether the 1.68.2 GC fix resolved their issue, and provide threading guidance",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the report! The symptoms here (release-only crash during multithreaded drawing) match a class of GC lifetime bugs that were addressed in **SkiaSharp 1.68.2** — see the related fix in #1220.\n\nCould you confirm:\n1. Did upgrading to 1.68.2 (or any later version) resolve the crash for you?\n2. Are each of your threads creating their own independent `SKCanvas`, `SKPath`, and `SKPaint` objects, or are any of these shared between threads?\n\nAs a general threading rule: **SkiaSharp objects are not thread-safe**. Each thread must own and manage its own drawing objects. Sharing a `SKPaint` or `SKPath` across threads without synchronization can cause exactly this kind of crash.\n\nIf the issue still occurs on a current release (v2.x or v3.x), please share a stack trace or minimal repro so we can investigate further."
      },
      {
        "type": "link-related",
        "description": "Cross-reference the related GC fix issue #1220",
        "risk": "low",
        "confidence": 0.92,
        "linkedIssue": 1220
      }
    ]
  }
}
```

</details>
