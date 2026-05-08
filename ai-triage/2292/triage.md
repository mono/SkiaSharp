# Issue Triage Report — #2292

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T15:46:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp.HarfBuzz (0.90 (90%)) |
| Suggested action | close-as-fixed (0.78 (78%)) |

**Issue Summary:** iOS app crashes with MissingMethodException (SKRunBuffer.GetGlyphSpan not found) on simulator and ExecutionEngineException (JIT in AOT-only mode) on physical device when calling DrawShapedText from SkiaSharp.HarfBuzz, triggered by a VS for Mac 8.9.1 toolchain update.

**Analysis:** A version mismatch between SkiaSharp.HarfBuzz and the core SkiaSharp NuGet caused two distinct failures on iOS: MissingMethodException because the HarfBuzz extension called GetGlyphSpan() which wasn't in the installed core, and ExecutionEngineException because the AOT compiler could not JIT the extension method on the device. The root cause has been resolved — current CanvasExtensions.cs uses run.Glyphs directly without calling GetGlyphSpan().

**Recommendations:** **close-as-fixed** — Root cause (calling GetGlyphSpan() which was absent in the paired SkiaSharp core) is gone — current CanvasExtensions.cs uses run.Glyphs directly. Issue is from 2021 with old Xamarin toolchain; no open reproduction against modern releases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Upgrade to Visual Studio for Mac 8.9.1 (build 34) or later
2. Build a Xamarin.iOS app that calls DrawShapedText from SkiaSharp.HarfBuzz
3. Run on iPhone Simulator — MissingMethodException for GetGlyphSpan()
4. Run on physical iPhone device — ExecutionEngineException attempting JIT in AOT mode

**Environment:** Xamarin.iOS 14.14.2.5, VS for Mac 8.9.2 (build 0), Mono 6.12.0.125, iOS 14.4, iPhone 6s / iPhone 12 Pro Max Simulator

**Repository links:**
- https://developercommunity.visualstudio.com/t/The-application-throws-MissingMethodExc/1376338 — Original Developer Community ticket

**Code snippets:**

```csharp
canvas.DrawShapedText(shaper, text, x, y, paint)
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | exception |
| Error message | Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan() |
| Repro quality | partial |
| Target frameworks | xamarin.ios |

**Stack trace:**

```text
System.MissingMethodException at ILive.iOS.Views.Debugging.TimelineDebugView.CanvasView_PaintSurface -> SkiaSharp.Views.iOS.SKCanvasView.Draw
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 8.8.7 (build 18), 8.9.1 (build 34) |
| Worked in | VS for Mac 8.8.7 (build 18) |
| Broke in | VS for Mac 8.9.1 (build 34) |
| Current relevance | unlikely |
| Relevance reason | Current CanvasExtensions.cs uses run.Glyphs directly and no longer calls GetGlyphSpan(); the version mismatch that triggered this bug is resolved in modern SkiaSharp releases. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.88 (88%) |
| Reason | Reporter explicitly states it worked in VS for Mac 8.8.7 and broke in 8.9.1. The VS update changed the bundled SkiaSharp version, creating a mismatch where SkiaSharp.HarfBuzz expected GetGlyphSpan() on SKRunBuffer but the installed SkiaSharp core didn't have it yet (or vice versa). |
| Worked in version | VS for Mac 8.8.7 (build 18) |
| Broke in version | VS for Mac 8.9.1 (build 34) |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.82 (82%) |
| Reason | Current CanvasExtensions.cs (DrawShapedText implementation) accesses run.Glyphs property directly and does not call GetGlyphSpan(). The GetGlyphSpan() method is now marked [Obsolete] in SKRunBuffer.cs, confirming the API was added and then superseded. Modern packages ship aligned versions of SkiaSharp and SkiaSharp.HarfBuzz so the mismatch cannot recur. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

A version mismatch between SkiaSharp.HarfBuzz and the core SkiaSharp NuGet caused two distinct failures on iOS: MissingMethodException because the HarfBuzz extension called GetGlyphSpan() which wasn't in the installed core, and ExecutionEngineException because the AOT compiler could not JIT the extension method on the device. The root cause has been resolved — current CanvasExtensions.cs uses run.Glyphs directly without calling GetGlyphSpan().

### Rationale

This is a type/bug in area/SkiaSharp.HarfBuzz: the extension method called an API (GetGlyphSpan) not present in the SkiaSharp version shipped alongside it, causing a runtime MissingMethodException. The current source has resolved this by using run.Glyphs directly. GetGlyphSpan() is now present (marked Obsolete) in the core, so even older mismatched binaries would no longer fail. The suggestedAction is close-as-fixed because the root cause is gone in modern releases.

### Key Signals

- "Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan()" — **issue body — iPhone Simulator exception** (SkiaSharp.HarfBuzz compiled against a SkiaSharp version that had GetGlyphSpan(), but runtime SkiaSharp core did not — classic NuGet version skew.)
- "Attempting to JIT compile method 'void SkiaSharp.HarfBuzz.CanvasExtensions:DrawShapedText...' while running in aot-only mode" — **issue body — physical iPhone exception** (Extension method not AOT-compiled on device build; likely a linker/AOT issue with the version mismatch or missing linker preserve hint.)
- "worked-in:8.8.7 (build 18)" — **issue body tag** (Confirms this is a regression introduced at VS for Mac 8.9.1 toolchain boundary.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs` | 58-103 | direct | Current DrawShapedText(SKShaper, ...) implementation accesses run.Glyphs (line 80) and run.Positions (line 81) directly — does NOT call GetGlyphSpan(). The version that shipped with VS for Mac 8.9.1 called GetGlyphSpan() which was absent from the paired SkiaSharp core. |
| `binding/SkiaSharp/SKRunBuffer.cs` | 25-26 | direct | GetGlyphSpan() is present and marked [Obsolete("Use Glyphs instead.")], confirming it was added later to provide backward compatibility but is no longer the primary access path. Its presence resolves the MissingMethodException for any still-mismatched setup. |

### Workarounds

- Ensure SkiaSharp and SkiaSharp.HarfBuzz NuGet versions are identical in the project (both same version); the mismatch is the core trigger.
- Upgrade to current SkiaSharp — current code path uses run.Glyphs, eliminating the missing method.

### Next Questions

- What exact SkiaSharp NuGet version was pinned when the reporter hit this? Knowing the version helps confirm fix boundary.
- Was the AOT ExecutionEngineException a separate linker issue or purely a consequence of the version mismatch?

### Resolution Proposals

**Hypothesis:** The VS for Mac 8.9.1 toolchain updated the embedded SkiaSharp.HarfBuzz but not the SkiaSharp core (or vice versa), creating a version mismatch where the extension called an API not yet present on device.

1. **Close as fixed — current code no longer calls GetGlyphSpan()** — fix, confidence 0.82 (82%), cost/xs, validated=untested
   - The current DrawShapedText implementation uses run.Glyphs directly. GetGlyphSpan() is now present in SKRunBuffer as [Obsolete]. Both fixes land in modern SkiaSharp releases.

**Recommended proposal:** Close as fixed — current code no longer calls GetGlyphSpan()

**Why:** Source evidence confirms the call path that caused the MissingMethodException no longer exists in the current codebase.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.78 (78%) |
| Reason | Root cause (calling GetGlyphSpan() which was absent in the paired SkiaSharp core) is gone — current CanvasExtensions.cs uses run.Glyphs directly. Issue is from 2021 with old Xamarin toolchain; no open reproduction against modern releases. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/SkiaSharp.HarfBuzz, os/iOS, tenet/compatibility, tenet/reliability | labels=type/bug, area/SkiaSharp.HarfBuzz, os/iOS, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.78 (78%) | Post analysis noting likely fix in current releases | — |
| close-issue | medium | 0.78 (78%) | Close as fixed — current code resolves the root cause | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report (migrated from Developer Community).

After investigating the current source, the root cause of the `MissingMethodException: Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan()` appears to have been a **version mismatch** between `SkiaSharp.HarfBuzz` and the core `SkiaSharp` NuGet — the HarfBuzz extension called `GetGlyphSpan()` which was not present in the SkiaSharp version your app was running against at the time.

In the current codebase:
- `CanvasExtensions.DrawShapedText` now accesses `run.Glyphs` directly and does **not** call `GetGlyphSpan()`.
- `GetGlyphSpan()` has been added to `SKRunBuffer` (marked `[Obsolete]`) to prevent this mismatch going forward.

If you are still seeing this issue with a **current version** of SkiaSharp (2.x or 3.x), please re-open with the exact NuGet versions you're using. If you've already upgraded and no longer see this, we'll close as fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2292,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T15:46:00Z"
  },
  "summary": "iOS app crashes with MissingMethodException (SKRunBuffer.GetGlyphSpan not found) on simulator and ExecutionEngineException (JIT in AOT-only mode) on physical device when calling DrawShapedText from SkiaSharp.HarfBuzz, triggered by a VS for Mac 8.9.1 toolchain update.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "exception",
      "errorMessage": "Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan()",
      "stackTrace": "System.MissingMethodException at ILive.iOS.Views.Debugging.TimelineDebugView.CanvasView_PaintSurface -> SkiaSharp.Views.iOS.SKCanvasView.Draw",
      "reproQuality": "partial",
      "targetFrameworks": [
        "xamarin.ios"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Upgrade to Visual Studio for Mac 8.9.1 (build 34) or later",
        "Build a Xamarin.iOS app that calls DrawShapedText from SkiaSharp.HarfBuzz",
        "Run on iPhone Simulator — MissingMethodException for GetGlyphSpan()",
        "Run on physical iPhone device — ExecutionEngineException attempting JIT in AOT mode"
      ],
      "codeSnippets": [
        "canvas.DrawShapedText(shaper, text, x, y, paint)"
      ],
      "environmentDetails": "Xamarin.iOS 14.14.2.5, VS for Mac 8.9.2 (build 0), Mono 6.12.0.125, iOS 14.4, iPhone 6s / iPhone 12 Pro Max Simulator",
      "repoLinks": [
        {
          "url": "https://developercommunity.visualstudio.com/t/The-application-throws-MissingMethodExc/1376338",
          "description": "Original Developer Community ticket"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "8.8.7 (build 18)",
        "8.9.1 (build 34)"
      ],
      "workedIn": "VS for Mac 8.8.7 (build 18)",
      "brokeIn": "VS for Mac 8.9.1 (build 34)",
      "currentRelevance": "unlikely",
      "relevanceReason": "Current CanvasExtensions.cs uses run.Glyphs directly and no longer calls GetGlyphSpan(); the version mismatch that triggered this bug is resolved in modern SkiaSharp releases."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.88,
      "reason": "Reporter explicitly states it worked in VS for Mac 8.8.7 and broke in 8.9.1. The VS update changed the bundled SkiaSharp version, creating a mismatch where SkiaSharp.HarfBuzz expected GetGlyphSpan() on SKRunBuffer but the installed SkiaSharp core didn't have it yet (or vice versa).",
      "workedInVersion": "VS for Mac 8.8.7 (build 18)",
      "brokeInVersion": "VS for Mac 8.9.1 (build 34)"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.82,
      "reason": "Current CanvasExtensions.cs (DrawShapedText implementation) accesses run.Glyphs property directly and does not call GetGlyphSpan(). The GetGlyphSpan() method is now marked [Obsolete] in SKRunBuffer.cs, confirming the API was added and then superseded. Modern packages ship aligned versions of SkiaSharp and SkiaSharp.HarfBuzz so the mismatch cannot recur."
    }
  },
  "analysis": {
    "summary": "A version mismatch between SkiaSharp.HarfBuzz and the core SkiaSharp NuGet caused two distinct failures on iOS: MissingMethodException because the HarfBuzz extension called GetGlyphSpan() which wasn't in the installed core, and ExecutionEngineException because the AOT compiler could not JIT the extension method on the device. The root cause has been resolved — current CanvasExtensions.cs uses run.Glyphs directly without calling GetGlyphSpan().",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/CanvasExtensions.cs",
        "lines": "58-103",
        "finding": "Current DrawShapedText(SKShaper, ...) implementation accesses run.Glyphs (line 80) and run.Positions (line 81) directly — does NOT call GetGlyphSpan(). The version that shipped with VS for Mac 8.9.1 called GetGlyphSpan() which was absent from the paired SkiaSharp core.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRunBuffer.cs",
        "lines": "25-26",
        "finding": "GetGlyphSpan() is present and marked [Obsolete(\"Use Glyphs instead.\")], confirming it was added later to provide backward compatibility but is no longer the primary access path. Its presence resolves the MissingMethodException for any still-mismatched setup.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan()",
        "source": "issue body — iPhone Simulator exception",
        "interpretation": "SkiaSharp.HarfBuzz compiled against a SkiaSharp version that had GetGlyphSpan(), but runtime SkiaSharp core did not — classic NuGet version skew."
      },
      {
        "text": "Attempting to JIT compile method 'void SkiaSharp.HarfBuzz.CanvasExtensions:DrawShapedText...' while running in aot-only mode",
        "source": "issue body — physical iPhone exception",
        "interpretation": "Extension method not AOT-compiled on device build; likely a linker/AOT issue with the version mismatch or missing linker preserve hint."
      },
      {
        "text": "worked-in:8.8.7 (build 18)",
        "source": "issue body tag",
        "interpretation": "Confirms this is a regression introduced at VS for Mac 8.9.1 toolchain boundary."
      }
    ],
    "rationale": "This is a type/bug in area/SkiaSharp.HarfBuzz: the extension method called an API (GetGlyphSpan) not present in the SkiaSharp version shipped alongside it, causing a runtime MissingMethodException. The current source has resolved this by using run.Glyphs directly. GetGlyphSpan() is now present (marked Obsolete) in the core, so even older mismatched binaries would no longer fail. The suggestedAction is close-as-fixed because the root cause is gone in modern releases.",
    "workarounds": [
      "Ensure SkiaSharp and SkiaSharp.HarfBuzz NuGet versions are identical in the project (both same version); the mismatch is the core trigger.",
      "Upgrade to current SkiaSharp — current code path uses run.Glyphs, eliminating the missing method."
    ],
    "nextQuestions": [
      "What exact SkiaSharp NuGet version was pinned when the reporter hit this? Knowing the version helps confirm fix boundary.",
      "Was the AOT ExecutionEngineException a separate linker issue or purely a consequence of the version mismatch?"
    ],
    "resolution": {
      "hypothesis": "The VS for Mac 8.9.1 toolchain updated the embedded SkiaSharp.HarfBuzz but not the SkiaSharp core (or vice versa), creating a version mismatch where the extension called an API not yet present on device.",
      "proposals": [
        {
          "title": "Close as fixed — current code no longer calls GetGlyphSpan()",
          "description": "The current DrawShapedText implementation uses run.Glyphs directly. GetGlyphSpan() is now present in SKRunBuffer as [Obsolete]. Both fixes land in modern SkiaSharp releases.",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Close as fixed — current code no longer calls GetGlyphSpan()",
      "recommendedReason": "Source evidence confirms the call path that caused the MissingMethodException no longer exists in the current codebase."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.78,
      "reason": "Root cause (calling GetGlyphSpan() which was absent in the paired SkiaSharp core) is gone — current CanvasExtensions.cs uses run.Glyphs directly. Issue is from 2021 with old Xamarin toolchain; no open reproduction against modern releases.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.HarfBuzz, os/iOS, tenet/compatibility, tenet/reliability",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "os/iOS",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post analysis noting likely fix in current releases",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for the detailed report (migrated from Developer Community).\n\nAfter investigating the current source, the root cause of the `MissingMethodException: Method not found: System.Span`1 SkiaSharp.SKRunBuffer.GetGlyphSpan()` appears to have been a **version mismatch** between `SkiaSharp.HarfBuzz` and the core `SkiaSharp` NuGet — the HarfBuzz extension called `GetGlyphSpan()` which was not present in the SkiaSharp version your app was running against at the time.\n\nIn the current codebase:\n- `CanvasExtensions.DrawShapedText` now accesses `run.Glyphs` directly and does **not** call `GetGlyphSpan()`.\n- `GetGlyphSpan()` has been added to `SKRunBuffer` (marked `[Obsolete]`) to prevent this mismatch going forward.\n\nIf you are still seeing this issue with a **current version** of SkiaSharp (2.x or 3.x), please re-open with the exact NuGet versions you're using. If you've already upgraded and no longer see this, we'll close as fixed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — current code resolves the root cause",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
