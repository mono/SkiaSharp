# Issue Triage Report — #1853

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-20T05:22:29Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | close-as-fixed (0.75 (75%)) |

**Issue Summary:** On macOS, moving a window containing SKGLView between displays with different BackingScaleFactor (retina vs non-retina) causes the rendered content to be displayed at double or half size instead of adjusting to the new display scale.

**Analysis:** SKGLView did not update its GL surface dimensions when the window was moved between displays with different BackingScaleFactor values. The fix — tracking lastBackingScaleFactor and calling Reshape() on change — is already present in the current codebase.

**Recommendations:** **close-as-fixed** — The current SKGLView.cs already contains explicit BackingScaleFactor change detection and reshape logic (lines 106-126) that directly addresses the reported issue; the bug was in version 2.80.2 which predates this code.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | — |
| Perf | — |
| Partner | — |

## Evidence

### Reproduction

1. Draw content on a SKGLView (e.g. a circle)
2. Move the window from a retina display to a non-retina display
3. Observe content displayed at double size
4. Move back to retina display — observe content at half size

**Environment:** SkiaSharp 2.80.2, macOS 12.0, MacBook Pro (14" 2021), Visual Studio for Mac

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net6.0-macos |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Current SKGLView.cs already tracks lastBackingScaleFactor and calls Reshape() when it changes, explicitly handling this scenario. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.80 (80%) |
| Reason | source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs lines 106-126 track lastBackingScaleFactor and call Reshape() + NeedsDisplay when BackingScaleFactor changes between draws — this directly addresses the reported behavior. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

SKGLView did not update its GL surface dimensions when the window was moved between displays with different BackingScaleFactor values. The fix — tracking lastBackingScaleFactor and calling Reshape() on change — is already present in the current codebase.

### Rationale

The reporter describes wrong rendering output (wrong scale) on macOS when dragging between retina/non-retina displays. Code investigation shows the current SKGLView.cs explicitly handles this case at lines 106-126 with BackingScaleFactor change detection and forced reshape, which was not present in v2.80.2.

### Key Signals

- "Moving from a retina display to a non-retina display causes everything to be displayed at double size." — **issue body** (GL surface was sized for the old BackingScaleFactor, so pixels map at wrong ratio.)
- "Version with issue: 2.80.2 / Last known good version: None" — **issue body** (No regression — bug was always present in SKGLView; SKCanvasView was not affected because it re-reads BackingScaleFactor on every DrawRect call.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 106-126 | direct | lastBackingScaleFactor field is updated on each DrawRect; when it differs from Window.BackingScaleFactor the code calls Reshape() to recompute newSize and queues NeedsDisplay = true, then returns early to prevent drawing at the old scale. This exactly addresses the reported bug. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs` | 97-103 | direct | Reshape() calls ConvertSizeToBacking(Bounds.Size) to obtain the physical pixel size at the current BackingScaleFactor and stores it in newSize. Without the scale-change detection above, Reshape() would only be called on bounds changes, not on scale changes. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKCanvasView.cs` | 70 | related | SKCanvasView passes Window.BackingScaleFactor directly to drawable.CreateSurface() on every DrawRect call, so it never has stale scale information — explaining why SKCanvasView was unaffected. |

### Next Questions

- Confirm the BackingScaleFactor tracking code was introduced after v2.80.2 to close this issue.
- Verify behavior on macOS 13/14 with ProMotion displays that also vary backing scale.

### Resolution Proposals

**Hypothesis:** The bug was that SKGLView's Reshape() was only triggered by bounds changes, not by display scale changes. The current code tracks lastBackingScaleFactor and forces a reshape when it changes.

1. **Verify fix and close** — investigation, confidence 0.80 (80%), cost/xs, validated=untested
   - The BackingScaleFactor tracking code present in the current SKGLView.cs (lines 106-126) appears to fully address the reported issue. Verify on a multi-display Mac setup and close if confirmed.

**Recommended proposal:** Verify fix and close

**Why:** The fix is already in the codebase; only confirmation is needed before closing.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.75 (75%) |
| Reason | The current SKGLView.cs already contains explicit BackingScaleFactor change detection and reshape logic (lines 106-126) that directly addresses the reported issue; the bug was in version 2.80.2 which predates this code. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, macOS, OpenGL labels | labels=type/bug, area/SkiaSharp.Views, os/macOS, backend/OpenGL |
| add-comment | high | 0.75 (75%) | Inform reporter that fix is present in current builds and request confirmation | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the detailed report! Looking at the current source code, `SKGLView` on macOS now tracks `BackingScaleFactor` changes between draws and forces a reshape when the display scale changes (this code was added after v2.80.2). Could you confirm whether this is still reproducible with the latest SkiaSharp release? If the fix is confirmed, we'll close this issue as resolved.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1853,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-20T05:22:29Z"
  },
  "summary": "On macOS, moving a window containing SKGLView between displays with different BackingScaleFactor (retina vs non-retina) causes the rendered content to be displayed at double or half size instead of adjusting to the new display scale.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.92
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-macos"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Draw content on a SKGLView (e.g. a circle)",
        "Move the window from a retina display to a non-retina display",
        "Observe content displayed at double size",
        "Move back to retina display — observe content at half size"
      ],
      "environmentDetails": "SkiaSharp 2.80.2, macOS 12.0, MacBook Pro (14\" 2021), Visual Studio for Mac"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Current SKGLView.cs already tracks lastBackingScaleFactor and calls Reshape() when it changes, explicitly handling this scenario."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.8,
      "reason": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs lines 106-126 track lastBackingScaleFactor and call Reshape() + NeedsDisplay when BackingScaleFactor changes between draws — this directly addresses the reported behavior."
    }
  },
  "analysis": {
    "summary": "SKGLView did not update its GL surface dimensions when the window was moved between displays with different BackingScaleFactor values. The fix — tracking lastBackingScaleFactor and calling Reshape() on change — is already present in the current codebase.",
    "rationale": "The reporter describes wrong rendering output (wrong scale) on macOS when dragging between retina/non-retina displays. Code investigation shows the current SKGLView.cs explicitly handles this case at lines 106-126 with BackingScaleFactor change detection and forced reshape, which was not present in v2.80.2.",
    "keySignals": [
      {
        "text": "Moving from a retina display to a non-retina display causes everything to be displayed at double size.",
        "source": "issue body",
        "interpretation": "GL surface was sized for the old BackingScaleFactor, so pixels map at wrong ratio."
      },
      {
        "text": "Version with issue: 2.80.2 / Last known good version: None",
        "source": "issue body",
        "interpretation": "No regression — bug was always present in SKGLView; SKCanvasView was not affected because it re-reads BackingScaleFactor on every DrawRect call."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "106-126",
        "finding": "lastBackingScaleFactor field is updated on each DrawRect; when it differs from Window.BackingScaleFactor the code calls Reshape() to recompute newSize and queues NeedsDisplay = true, then returns early to prevent drawing at the old scale. This exactly addresses the reported bug.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKGLView.cs",
        "lines": "97-103",
        "finding": "Reshape() calls ConvertSizeToBacking(Bounds.Size) to obtain the physical pixel size at the current BackingScaleFactor and stores it in newSize. Without the scale-change detection above, Reshape() would only be called on bounds changes, not on scale changes.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/macOS/SKCanvasView.cs",
        "lines": "70",
        "finding": "SKCanvasView passes Window.BackingScaleFactor directly to drawable.CreateSurface() on every DrawRect call, so it never has stale scale information — explaining why SKCanvasView was unaffected.",
        "relevance": "related"
      }
    ],
    "nextQuestions": [
      "Confirm the BackingScaleFactor tracking code was introduced after v2.80.2 to close this issue.",
      "Verify behavior on macOS 13/14 with ProMotion displays that also vary backing scale."
    ],
    "resolution": {
      "hypothesis": "The bug was that SKGLView's Reshape() was only triggered by bounds changes, not by display scale changes. The current code tracks lastBackingScaleFactor and forces a reshape when it changes.",
      "proposals": [
        {
          "title": "Verify fix and close",
          "description": "The BackingScaleFactor tracking code present in the current SKGLView.cs (lines 106-126) appears to fully address the reported issue. Verify on a multi-display Mac setup and close if confirmed.",
          "category": "investigation",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify fix and close",
      "recommendedReason": "The fix is already in the codebase; only confirmation is needed before closing."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.75,
      "reason": "The current SKGLView.cs already contains explicit BackingScaleFactor change detection and reshape logic (lines 106-126) that directly addresses the reported issue; the bug was in version 2.80.2 which predates this code.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, macOS, OpenGL labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/macOS",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that fix is present in current builds and request confirmation",
        "risk": "high",
        "confidence": 0.75,
        "comment": "Thank you for the detailed report! Looking at the current source code, `SKGLView` on macOS now tracks `BackingScaleFactor` changes between draws and forces a reshape when the display scale changes (this code was added after v2.80.2). Could you confirm whether this is still reproducible with the latest SkiaSharp release? If the fix is confirmed, we'll close this issue as resolved."
      }
    ]
  }
}
```

</details>
