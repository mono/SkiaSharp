# Issue Triage Report — #2967

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T08:05:56Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/SkiaSharp.Views (0.92 (92%)) |
| Suggested action | ready-to-fix (0.80 (80%)) |

**Issue Summary:** When multiple WinUI TabView tabs are removed simultaneously, a known WinUI bug fires spurious Unloaded events on the wrong FrameworkElement, causing SKXamlCanvas to call FreeBitmap() even though the control is still loaded, resulting in a blank canvas.

**Analysis:** The SKXamlCanvas uses a `loadUnloadCounter` to guard against duplicate Loaded/Unloaded events (fix for #1118). However, WinUI's TabView has a separate bug (microsoft/microsoft-ui-xaml#9880) where extra Unloaded events fire on the wrong element while it is still loaded (IsLoaded == true). The counter decrements to 0 on such a spurious Unloaded event, causing FreeBitmap() to be called and the canvas background to be cleared while the control is still visible.

**Recommendations:** **ready-to-fix** — Root cause is clear from code analysis and reporter diagnosis. The fix is small (add IsLoaded check in OnUnloaded). Upstream WinUI bug is documented and unlikely to be fixed in all supported WinUI versions.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-WinUI |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a WinUI app with a TabView containing multiple tabs, each hosting an SKXamlCanvas
2. Remove multiple tabs simultaneously
3. Observe that the content of the currently selected tab becomes blank

**Environment:** WinUI TabView, SkiaSharp 2.88.3, Visual Studio on Windows

**Repository links:**
- https://github.com/microsoft/microsoft-ui-xaml/issues/9880 — WinUI bug: multiple Unloaded events triggered on wrong FrameworkElement
- https://github.com/microsoft/microsoft-ui-xaml/issues/1900 — WinUI erratic Load/Unload behavior
- https://github.com/microsoft/microsoft-ui-xaml/issues/8342 — WinUI erratic Load/Unload behavior (another instance)
- https://github.com/mono/SkiaSharp/issues/1118 — Prior related fix that introduced loadUnloadCounter

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKXamlCanvas content disappears when removing multiple WinUI TabView tabs |
| Repro quality | partial |
| Target frameworks | net8.0-windows10.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The loadUnloadCounter pattern in SKXamlCanvas.cs has not changed — it is still vulnerable to spurious Unloaded events from WinUI. |

## Analysis

### Technical Summary

The SKXamlCanvas uses a `loadUnloadCounter` to guard against duplicate Loaded/Unloaded events (fix for #1118). However, WinUI's TabView has a separate bug (microsoft/microsoft-ui-xaml#9880) where extra Unloaded events fire on the wrong element while it is still loaded (IsLoaded == true). The counter decrements to 0 on such a spurious Unloaded event, causing FreeBitmap() to be called and the canvas background to be cleared while the control is still visible.

### Rationale

Confirmed real bug: code reading shows OnUnloaded decrements counter and calls FreeBitmap() when counter reaches 0, with no check of IsLoaded. Reporter's diagnosis matches code behavior exactly. WinUI upstream issue is documented and well-known. The existing loadUnloadCounter mitigates double-Loaded but not spurious Unloaded from a different element.

### Key Signals

- "when the SKXamlCanvas receives incorrect Unloaded events it actually has IsLoaded == true" — **issue body** (Reporter observed that the element is still loaded when the spurious Unloaded fires — this can be used as a guard.)
- "the background is cleared on Unloaded events, so that's why my content disappears" — **issue body** (Confirms FreeBitmap() path is the root cause of visible blank canvas.)
- "Maybe the current fix with the loadUnloadCounter could be replaced with a private isLoaded flag which can be set in OnLoaded, and then prevent multiple Loaded events to be processed? The flag should then only get reset in OnUnloaded if IsLoaded actually is false." — **issue body** (Reporter suggests a concrete fix: check IsLoaded in OnUnloaded before clearing.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 46 | direct | loadUnloadCounter is an integer counter used to prevent double-subscription to DPI events. Incremented on Loaded, decremented on Unloaded. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 125-140 | direct | OnLoaded increments counter and only proceeds if counter == 1. Guards against duplicate Loaded events but does not inspect FrameworkElement.IsLoaded. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs` | 142-159 | direct | OnUnloaded decrements counter and calls FreeBitmap() when counter reaches 0. No check of IsLoaded property — vulnerable to spurious Unloaded events fired by WinUI TabView on controls that are still loaded. |

### Workarounds

- Avoid removing multiple TabView tabs simultaneously — remove them one at a time.
- After tabs are removed, call canvas.Invalidate() on the remaining tab's SKXamlCanvas to force a repaint.

### Next Questions

- Does the same issue occur with the UWP (non-WINDOWS) variant of SKXamlCanvas?
- Is the fix needed only in SKXamlCanvas or also in SKSwapChainPanel/SKGLWinUIElement?
- What WinUI/Windows App SDK version first introduced the TabView multi-tab removal bug?

### Resolution Proposals

**Hypothesis:** The fix is to guard FreeBitmap() in OnUnloaded by checking IsLoaded == false before calling it, since WinUI fires spurious Unloaded events on elements that remain loaded.

1. **Check IsLoaded before calling FreeBitmap()** — fix, confidence 0.82 (82%), cost/xs, validated=untested
   - In OnUnloaded, add a check: if (IsLoaded) return; before decrementing the counter or before calling FreeBitmap(). This prevents clearing the canvas when the WinUI runtime fires a spurious Unloaded on an element that is still actually loaded.
2. **Replace counter with IsLoaded-based tracking** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Replace loadUnloadCounter with a private bool _isLoaded field. Set it true in OnLoaded (skip if already true). Reset it false in OnUnloaded only when IsLoaded == false. This is the approach suggested by the reporter.

**Recommended proposal:** Check IsLoaded before calling FreeBitmap()

**Why:** Minimal change with the highest confidence; directly guards the problematic code path without restructuring the existing counter logic.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.80 (80%) |
| Reason | Root cause is clear from code analysis and reporter diagnosis. The fix is small (add IsLoaded check in OnUnloaded). Upstream WinUI bug is documented and unlikely to be fixed in all supported WinUI versions. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply WinUI, views, and reliability labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-WinUI, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Acknowledge the bug, explain root cause, suggest the IsLoaded guard fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed investigation and links to the upstream WinUI bugs!

I've confirmed the root cause in `SKXamlCanvas.cs`: `OnUnloaded` decrements `loadUnloadCounter` and calls `FreeBitmap()` when it reaches 0, without checking `IsLoaded`. When WinUI's TabView fires a spurious `Unloaded` event on a still-loaded element (per [microsoft-ui-xaml#9880](https://github.com/microsoft/microsoft-ui-xaml/issues/9880)), the counter hits 0 and the canvas is cleared incorrectly.

Your suggested fix — checking `IsLoaded` in `OnUnloaded` before clearing — looks like the right approach. The simplest change would be:

```csharp
private void OnUnloaded(object sender, RoutedEventArgs e)
{
    if (IsLoaded)
        return;
    // ... rest of the existing cleanup
}
```

**Workaround until fixed:** After removing multiple tabs, call `canvas.Invalidate()` on the surviving tab's `SKXamlCanvas` to force a repaint.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2967,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T08:05:56Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "When multiple WinUI TabView tabs are removed simultaneously, a known WinUI bug fires spurious Unloaded events on the wrong FrameworkElement, causing SKXamlCanvas to call FreeBitmap() even though the control is still loaded, resulting in a blank canvas.",
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
      "os/Windows-WinUI"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKXamlCanvas content disappears when removing multiple WinUI TabView tabs",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net8.0-windows10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinUI app with a TabView containing multiple tabs, each hosting an SKXamlCanvas",
        "Remove multiple tabs simultaneously",
        "Observe that the content of the currently selected tab becomes blank"
      ],
      "environmentDetails": "WinUI TabView, SkiaSharp 2.88.3, Visual Studio on Windows",
      "repoLinks": [
        {
          "url": "https://github.com/microsoft/microsoft-ui-xaml/issues/9880",
          "description": "WinUI bug: multiple Unloaded events triggered on wrong FrameworkElement"
        },
        {
          "url": "https://github.com/microsoft/microsoft-ui-xaml/issues/1900",
          "description": "WinUI erratic Load/Unload behavior"
        },
        {
          "url": "https://github.com/microsoft/microsoft-ui-xaml/issues/8342",
          "description": "WinUI erratic Load/Unload behavior (another instance)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1118",
          "description": "Prior related fix that introduced loadUnloadCounter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The loadUnloadCounter pattern in SKXamlCanvas.cs has not changed — it is still vulnerable to spurious Unloaded events from WinUI."
    }
  },
  "analysis": {
    "summary": "The SKXamlCanvas uses a `loadUnloadCounter` to guard against duplicate Loaded/Unloaded events (fix for #1118). However, WinUI's TabView has a separate bug (microsoft/microsoft-ui-xaml#9880) where extra Unloaded events fire on the wrong element while it is still loaded (IsLoaded == true). The counter decrements to 0 on such a spurious Unloaded event, causing FreeBitmap() to be called and the canvas background to be cleared while the control is still visible.",
    "rationale": "Confirmed real bug: code reading shows OnUnloaded decrements counter and calls FreeBitmap() when counter reaches 0, with no check of IsLoaded. Reporter's diagnosis matches code behavior exactly. WinUI upstream issue is documented and well-known. The existing loadUnloadCounter mitigates double-Loaded but not spurious Unloaded from a different element.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "46",
        "finding": "loadUnloadCounter is an integer counter used to prevent double-subscription to DPI events. Incremented on Loaded, decremented on Unloaded.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "125-140",
        "finding": "OnLoaded increments counter and only proceeds if counter == 1. Guards against duplicate Loaded events but does not inspect FrameworkElement.IsLoaded.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WinUI/SKXamlCanvas.cs",
        "lines": "142-159",
        "finding": "OnUnloaded decrements counter and calls FreeBitmap() when counter reaches 0. No check of IsLoaded property — vulnerable to spurious Unloaded events fired by WinUI TabView on controls that are still loaded.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "when the SKXamlCanvas receives incorrect Unloaded events it actually has IsLoaded == true",
        "source": "issue body",
        "interpretation": "Reporter observed that the element is still loaded when the spurious Unloaded fires — this can be used as a guard."
      },
      {
        "text": "the background is cleared on Unloaded events, so that's why my content disappears",
        "source": "issue body",
        "interpretation": "Confirms FreeBitmap() path is the root cause of visible blank canvas."
      },
      {
        "text": "Maybe the current fix with the loadUnloadCounter could be replaced with a private isLoaded flag which can be set in OnLoaded, and then prevent multiple Loaded events to be processed? The flag should then only get reset in OnUnloaded if IsLoaded actually is false.",
        "source": "issue body",
        "interpretation": "Reporter suggests a concrete fix: check IsLoaded in OnUnloaded before clearing."
      }
    ],
    "workarounds": [
      "Avoid removing multiple TabView tabs simultaneously — remove them one at a time.",
      "After tabs are removed, call canvas.Invalidate() on the remaining tab's SKXamlCanvas to force a repaint."
    ],
    "nextQuestions": [
      "Does the same issue occur with the UWP (non-WINDOWS) variant of SKXamlCanvas?",
      "Is the fix needed only in SKXamlCanvas or also in SKSwapChainPanel/SKGLWinUIElement?",
      "What WinUI/Windows App SDK version first introduced the TabView multi-tab removal bug?"
    ],
    "resolution": {
      "hypothesis": "The fix is to guard FreeBitmap() in OnUnloaded by checking IsLoaded == false before calling it, since WinUI fires spurious Unloaded events on elements that remain loaded.",
      "proposals": [
        {
          "title": "Check IsLoaded before calling FreeBitmap()",
          "description": "In OnUnloaded, add a check: if (IsLoaded) return; before decrementing the counter or before calling FreeBitmap(). This prevents clearing the canvas when the WinUI runtime fires a spurious Unloaded on an element that is still actually loaded.",
          "category": "fix",
          "confidence": 0.82,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Replace counter with IsLoaded-based tracking",
          "description": "Replace loadUnloadCounter with a private bool _isLoaded field. Set it true in OnLoaded (skip if already true). Reset it false in OnUnloaded only when IsLoaded == false. This is the approach suggested by the reporter.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Check IsLoaded before calling FreeBitmap()",
      "recommendedReason": "Minimal change with the highest confidence; directly guards the problematic code path without restructuring the existing counter logic."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.8,
      "reason": "Root cause is clear from code analysis and reporter diagnosis. The fix is small (add IsLoaded check in OnUnloaded). Upstream WinUI bug is documented and unlikely to be fixed in all supported WinUI versions.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply WinUI, views, and reliability labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-WinUI",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the bug, explain root cause, suggest the IsLoaded guard fix",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed investigation and links to the upstream WinUI bugs!\n\nI've confirmed the root cause in `SKXamlCanvas.cs`: `OnUnloaded` decrements `loadUnloadCounter` and calls `FreeBitmap()` when it reaches 0, without checking `IsLoaded`. When WinUI's TabView fires a spurious `Unloaded` event on a still-loaded element (per [microsoft-ui-xaml#9880](https://github.com/microsoft/microsoft-ui-xaml/issues/9880)), the counter hits 0 and the canvas is cleared incorrectly.\n\nYour suggested fix — checking `IsLoaded` in `OnUnloaded` before clearing — looks like the right approach. The simplest change would be:\n\n```csharp\nprivate void OnUnloaded(object sender, RoutedEventArgs e)\n{\n    if (IsLoaded)\n        return;\n    // ... rest of the existing cleanup\n}\n```\n\n**Workaround until fixed:** After removing multiple tabs, call `canvas.Invalidate()` on the surviving tab's `SKXamlCanvas` to force a repaint."
      }
    ]
  }
}
```

</details>
