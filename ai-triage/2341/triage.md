# Issue Triage Report — #2341

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T23:37:30Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views.Maui (0.80 (80%)) |
| Suggested action | close-as-fixed (0.75 (75%)) |

**Issue Summary:** In a WinUI app using MAUI's SKCanvasView, rapidly resizing the window causes canvas content to appear incorrectly scaled for some frames (flickering), traced to a dispatcher-based Invalidate() call introduced in PR #1804; a February 2025 community member confirms the issue is resolved by PR #2922.

**Analysis:** PR #1804 removed the ScaleTransform and replaced it with a dispatcher-queued Invalidate() in OnSizeChanged. This created a race: during the brief delay between the SizeChanged event and the dispatcher executing DoInvalidate(), the existing bitmap (sized for the previous window dimensions) was displayed at the new window size without the compensating scale, causing a single-frame visual glitch that appears as flickering during resize. The reporter correctly identified both root cause and fix. Current code already implements Solution 1 (Stretch.None + ScaleTransform in UpdateBrushScale()), suggesting PR #2922 applied the fix.

**Recommendations:** **close-as-fixed** — Community member @Mangepange confirmed the issue is resolved in February 2025, attributing the fix to PR #2922. Code inspection confirms Stretch.None + ScaleTransform are present in current SKXamlCanvas.cs, matching reporter's Solution 1.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create a MAUI app with SKCanvasView drawing text
2. Run on WinUI (Windows) platform
3. Continuously resize the application window
4. Observe canvas content appears incorrectly scaled for some frames

**Environment:** SkiaSharp 2.88.3, Visual Studio, WinUI, Windows

**Related issues:** #2295

**Repository links:**
- https://github.com/mono/SkiaSharp/files/10235785/SkiaScaleWinUI.zip — Minimal WinUI MAUI repro project provided by reporter
- https://github.com/mono/SkiaSharp/pull/1804 — PR that introduced the regression: removed ScaleTransform, added dispatcher-based Invalidate() in OnSizeChanged
- https://github.com/mono/SkiaSharp/issues/2295 — Related issue: WinUI canvas stretching on size change — same root cause, filed by same community member

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | complete |
| Target frameworks | WinUI |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | Community member @Mangepange confirmed in February 2025 the issue is resolved, attributing the fix to PR #2922. Current SKXamlCanvas.cs code uses Stretch.None with a ScaleTransform, matching reporter's suggested Solution 1. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.75 (75%) |
| Reason | Community member @Mangepange commented in February 2025: 'This works perfectly for me now. I think it was #2922 that solved it.' Code inspection of SKXamlCanvas.cs confirms Stretch.None + ScaleTransform are now in place, exactly matching reporter's Solution 1. |
| Related PRs | #2922 |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

PR #1804 removed the ScaleTransform and replaced it with a dispatcher-queued Invalidate() in OnSizeChanged. This created a race: during the brief delay between the SizeChanged event and the dispatcher executing DoInvalidate(), the existing bitmap (sized for the previous window dimensions) was displayed at the new window size without the compensating scale, causing a single-frame visual glitch that appears as flickering during resize. The reporter correctly identified both root cause and fix. Current code already implements Solution 1 (Stretch.None + ScaleTransform in UpdateBrushScale()), suggesting PR #2922 applied the fix.

### Rationale

The reporter provided an accurate root cause analysis pointing to PR #1804 and suggested two solutions. Code inspection of the current SKXamlCanvas.cs confirms Solution 1 is implemented: the ImageBrush uses Stretch.None and UpdateBrushScale() sets a ScaleTransform (1/Dpi). A community member confirmed in February 2025 the issue is resolved. Classification as close-as-fixed with medium confidence because maintainer-level confirmation of the specific PR is not present, but code + community evidence is strong.

### Key Signals

- "Invalidate invokes DoInvalidate, but from within a dispatcher which causes the inaccurate rendering of the frames." — **issue body** (Reporter correctly identified the race condition: dispatcher delay between SizeChanged and actual repaint causes old mis-sized bitmap to briefly appear.)
- "This works perfectly for me now. I think it was #2922 that solved it." — **comment by @Mangepange, February 2025** (Community-confirmed fix in a recent release. Strong evidence this is already resolved.)
- "It would be neat if SKXamlCanvas.OnSizeChanged was virtual, just like SKXamlCanvas.OnPaintSurface." — **comment by @Mangepange, April 2024** (Same user who later confirmed the fix was previously interested in a workaround — their 2025 comment is meaningful because they were actively tracking this issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 237-255 | direct | CreateBitmap() creates ImageBrush with Stretch.None and calls UpdateBrushScale() — reporter's Solution 1 is implemented. The brush does not visually stretch during the dispatcher delay on resize. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 257-269 | direct | UpdateBrushScale() applies a ScaleTransform of 1/Dpi to the ImageBrush, compensating for the pixel-to-DIP ratio. This ensures the bitmap appears at the correct visual size even before DoInvalidate runs. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 120-123 | related | OnSizeChanged still calls Invalidate() which dispatches via DispatcherQueue, not DoInvalidate() directly. However, since the brush is Stretch.None with ScaleTransform, the visual glitch described in the issue no longer occurs. |

### Workarounds

- Upgrade to the latest SkiaSharp NuGet package (which includes PR #2922) — community-confirmed fix.

### Next Questions

- Confirm which SkiaSharp NuGet version shipped PR #2922
- Verify whether the fix also applies to the UWP path (non-WINDOWS #if branch)

### Resolution Proposals

**Hypothesis:** Bug was introduced in PR #1804 by removing ScaleTransform and using dispatcher-queued Invalidate(). Fixed (likely in PR #2922) by restoring Stretch.None + ScaleTransform so the old bitmap is not visually mis-scaled during the dispatch delay.

1. **Upgrade to latest SkiaSharp** — workaround, confidence 0.75 (75%), cost/xs
   - Update the SkiaSharp NuGet package to the latest version. PR #2922 added back Stretch.None + ScaleTransform to SKXamlCanvas (Reporter's Solution 1), which prevents the bitmap from stretching during the dispatcher delay on window resize.

**Recommended proposal:** Upgrade to latest SkiaSharp

**Why:** Community member confirmed the issue is resolved in the current codebase. Upgrading the NuGet package is the path of least resistance.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.75 (75%) |
| Reason | Community member @Mangepange confirmed the issue is resolved in February 2025, attributing the fix to PR #2922. Code inspection confirms Stretch.None + ScaleTransform are present in current SKXamlCanvas.cs, matching reporter's Solution 1. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, WinUI platform, MAUI views area, and reliability tenet labels | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-WinUI, tenet/reliability |
| add-comment | high | 0.75 (75%) | Notify reporter that issue appears fixed in a recent release | — |
| close-issue | medium | 0.75 (75%) | Close as fixed — community-confirmed fix in recent release, current code confirms the fix is in place | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and root cause analysis! Based on a community comment from February 2025 (by @Mangepange), this issue appears to have been resolved — likely by PR #2922. The current `SKXamlCanvas.cs` uses `Stretch.None` with a `ScaleTransform` that compensates for DPI, which matches your suggested Solution 1.

If you're still experiencing this with the latest SkiaSharp release, please reopen this issue or file a new one with the version details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2341,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T23:37:30Z"
  },
  "summary": "In a WinUI app using MAUI's SKCanvasView, rapidly resizing the window causes canvas content to appear incorrectly scaled for some frames (flickering), traced to a dispatcher-based Invalidate() call introduced in PR #1804; a February 2025 community member confirms the issue is resolved by PR #2922.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.8
    },
    "platforms": [
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "complete",
      "targetFrameworks": [
        "WinUI"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a MAUI app with SKCanvasView drawing text",
        "Run on WinUI (Windows) platform",
        "Continuously resize the application window",
        "Observe canvas content appears incorrectly scaled for some frames"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, Visual Studio, WinUI, Windows",
      "relatedIssues": [
        2295
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/10235785/SkiaScaleWinUI.zip",
          "description": "Minimal WinUI MAUI repro project provided by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1804",
          "description": "PR that introduced the regression: removed ScaleTransform, added dispatcher-based Invalidate() in OnSizeChanged"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2295",
          "description": "Related issue: WinUI canvas stretching on size change — same root cause, filed by same community member"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "Community member @Mangepange confirmed in February 2025 the issue is resolved, attributing the fix to PR #2922. Current SKXamlCanvas.cs code uses Stretch.None with a ScaleTransform, matching reporter's suggested Solution 1."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.75,
      "reason": "Community member @Mangepange commented in February 2025: 'This works perfectly for me now. I think it was #2922 that solved it.' Code inspection of SKXamlCanvas.cs confirms Stretch.None + ScaleTransform are now in place, exactly matching reporter's Solution 1.",
      "relatedPRs": [
        2922
      ]
    }
  },
  "analysis": {
    "summary": "PR #1804 removed the ScaleTransform and replaced it with a dispatcher-queued Invalidate() in OnSizeChanged. This created a race: during the brief delay between the SizeChanged event and the dispatcher executing DoInvalidate(), the existing bitmap (sized for the previous window dimensions) was displayed at the new window size without the compensating scale, causing a single-frame visual glitch that appears as flickering during resize. The reporter correctly identified both root cause and fix. Current code already implements Solution 1 (Stretch.None + ScaleTransform in UpdateBrushScale()), suggesting PR #2922 applied the fix.",
    "rationale": "The reporter provided an accurate root cause analysis pointing to PR #1804 and suggested two solutions. Code inspection of the current SKXamlCanvas.cs confirms Solution 1 is implemented: the ImageBrush uses Stretch.None and UpdateBrushScale() sets a ScaleTransform (1/Dpi). A community member confirmed in February 2025 the issue is resolved. Classification as close-as-fixed with medium confidence because maintainer-level confirmation of the specific PR is not present, but code + community evidence is strong.",
    "keySignals": [
      {
        "text": "Invalidate invokes DoInvalidate, but from within a dispatcher which causes the inaccurate rendering of the frames.",
        "source": "issue body",
        "interpretation": "Reporter correctly identified the race condition: dispatcher delay between SizeChanged and actual repaint causes old mis-sized bitmap to briefly appear."
      },
      {
        "text": "This works perfectly for me now. I think it was #2922 that solved it.",
        "source": "comment by @Mangepange, February 2025",
        "interpretation": "Community-confirmed fix in a recent release. Strong evidence this is already resolved."
      },
      {
        "text": "It would be neat if SKXamlCanvas.OnSizeChanged was virtual, just like SKXamlCanvas.OnPaintSurface.",
        "source": "comment by @Mangepange, April 2024",
        "interpretation": "Same user who later confirmed the fix was previously interested in a workaround — their 2025 comment is meaningful because they were actively tracking this issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "237-255",
        "finding": "CreateBitmap() creates ImageBrush with Stretch.None and calls UpdateBrushScale() — reporter's Solution 1 is implemented. The brush does not visually stretch during the dispatcher delay on resize.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "257-269",
        "finding": "UpdateBrushScale() applies a ScaleTransform of 1/Dpi to the ImageBrush, compensating for the pixel-to-DIP ratio. This ensures the bitmap appears at the correct visual size even before DoInvalidate runs.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "120-123",
        "finding": "OnSizeChanged still calls Invalidate() which dispatches via DispatcherQueue, not DoInvalidate() directly. However, since the brush is Stretch.None with ScaleTransform, the visual glitch described in the issue no longer occurs.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Upgrade to the latest SkiaSharp NuGet package (which includes PR #2922) — community-confirmed fix."
    ],
    "nextQuestions": [
      "Confirm which SkiaSharp NuGet version shipped PR #2922",
      "Verify whether the fix also applies to the UWP path (non-WINDOWS #if branch)"
    ],
    "resolution": {
      "hypothesis": "Bug was introduced in PR #1804 by removing ScaleTransform and using dispatcher-queued Invalidate(). Fixed (likely in PR #2922) by restoring Stretch.None + ScaleTransform so the old bitmap is not visually mis-scaled during the dispatch delay.",
      "proposals": [
        {
          "title": "Upgrade to latest SkiaSharp",
          "description": "Update the SkiaSharp NuGet package to the latest version. PR #2922 added back Stretch.None + ScaleTransform to SKXamlCanvas (Reporter's Solution 1), which prevents the bitmap from stretching during the dispatcher delay on window resize.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs"
        }
      ],
      "recommendedProposal": "Upgrade to latest SkiaSharp",
      "recommendedReason": "Community member confirmed the issue is resolved in the current codebase. Upgrading the NuGet package is the path of least resistance."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.75,
      "reason": "Community member @Mangepange confirmed the issue is resolved in February 2025, attributing the fix to PR #2922. Code inspection confirms Stretch.None + ScaleTransform are present in current SKXamlCanvas.cs, matching reporter's Solution 1.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, WinUI platform, MAUI views area, and reliability tenet labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that issue appears fixed in a recent release",
        "risk": "high",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report and root cause analysis! Based on a community comment from February 2025 (by @Mangepange), this issue appears to have been resolved — likely by PR #2922. The current `SKXamlCanvas.cs` uses `Stretch.None` with a `ScaleTransform` that compensates for DPI, which matches your suggested Solution 1.\n\nIf you're still experiencing this with the latest SkiaSharp release, please reopen this issue or file a new one with the version details."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — community-confirmed fix in recent release, current code confirms the fix is in place",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
