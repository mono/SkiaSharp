# Issue Triage Report — #3643

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-28T05:36:50Z |
| Type | type/bug (0.85 (85%)) |
| Area | area/SkiaSharp.Views (0.95 (95%)) |
| Suggested action | ready-to-fix (0.82 (82%)) |

**Issue Summary:** SKControl and SKGLControl only check DesignMode on themselves, missing ancestor design mode detection; controls embedded in a UserControl being edited in the WinForms designer will attempt to render with Skia/OpenGL instead of being suppressed.

**Analysis:** Both SKControl and SKGLControl check only Control.DesignMode in OnPaint to suppress rendering in the WinForms designer. DesignMode returns true only when the control is the root of the designer surface; when an SKControl is embedded inside a UserControl that is being edited, DesignMode is false and Skia/OpenGL rendering fires, which can crash the designer process. The fix is to also check Control.IsAncestorSiteInDesignMode (available since .NET 6; needs polyfill on older TFMs).

**Recommendations:** **ready-to-fix** — Root cause is confirmed by code investigation, fix approach is well-understood, and a community workaround validates the approach. Only open question is polyfill strategy for older TFMs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a WinForms UserControl that contains an SKControl or SKGLControl
2. Open the parent Form that contains that UserControl in the VS designer
3. Observe that DesignMode on the SKControl returns false even though it is inside a design-time host
4. The control attempts to render with Skia/OpenGL instead of showing a design-time placeholder

**Environment:** Windows Classic, Visual Studio WinForms designer, any .NET version

**Related issues:** #2033

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3614#issuecomment-4182840423 — Original comment from @xackus identifying the issue in PR #3614

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net6.0-windows, net462 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The DesignMode-only check is still present in current source at SKControl.cs:28 and SKGLControl.cs:74; no fix has been merged. |

## Analysis

### Technical Summary

Both SKControl and SKGLControl check only Control.DesignMode in OnPaint to suppress rendering in the WinForms designer. DesignMode returns true only when the control is the root of the designer surface; when an SKControl is embedded inside a UserControl that is being edited, DesignMode is false and Skia/OpenGL rendering fires, which can crash the designer process. The fix is to also check Control.IsAncestorSiteInDesignMode (available since .NET 6; needs polyfill on older TFMs).

### Rationale

The issue is clearly a behavioral bug: the design-mode guard is incomplete by design of the .NET Control.DesignMode property. Code investigation confirms both affected files still use only DesignMode. The proposed fix (IsAncestorSiteInDesignMode) is well-known and the issue has been acknowledged by the maintainer (mattleibow opened it). This is ready to fix once the TFM polyfill strategy is decided.

### Key Signals

- "The current code only prevents painting if SKControl itself is being edited in the WinForms designer." — **issue body (from @xackus in PR #3614)** (Confirms the guard is incomplete; ancestor design mode is not detected.)
- "You might want to use Control.IsAncestorSiteInDesignMode (needs to be polyfilled on older targets)" — **issue body** (Proposed fix is already identified; needs polyfill work for pre-.NET 6 targets.)
- "I have overridden the OnPaint method to check the IsDesignMode property, allowing SKGLControl to function correctly within the designer." — **comment by @sueastward** (Community confirms the problem is real and workaround is feasible per-control.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs` | 28-29 | direct | OnPaint checks only 'if (DesignMode) return;' — misses ancestor design mode when SKControl is nested inside a UserControl being designed. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 73-78 | direct | OnPaint checks only 'if (DesignMode)' and clears background — same incomplete check as SKControl; OpenGL calls will fire when nested in a designer host. |

### Workarounds

- Override OnPaint in a subclass and check IsDesignMode (GLControl property for SKGLControl) before calling base.OnPaint, as shown by @sueastward in the comments.
- For SKControl subclasses, override OnPaint and add: if (DesignMode || this.IsAncestorSiteInDesignMode()) return; before calling base.OnPaint (requires .NET 6+).

### Next Questions

- What TFMs does SkiaSharp.Views.WindowsForms currently target — does it need a polyfill for net462/netstandard?
- Should SKGLControl use GLControl.IsDesignMode (OpenTK property) or Control.IsAncestorSiteInDesignMode?
- Are there other SKControl subclasses (e.g., in Uno WinForms) with the same incomplete guard?

### Resolution Proposals

**Hypothesis:** Replace the simple DesignMode check in both SKControl.OnPaint and SKGLControl.OnPaint with a helper that also checks IsAncestorSiteInDesignMode, polyfilled for older TFMs.

1. **Use IsAncestorSiteInDesignMode with polyfill** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Add an IsInDesignMode() helper that returns DesignMode || (IsAncestorSiteInDesignMode available ? IsAncestorSiteInDesignMode : false). Use this helper in both SKControl and SKGLControl OnPaint.
2. **Subclass workaround (user-side)** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Users can subclass SKControl/SKGLControl and override OnPaint to add the IsAncestorSiteInDesignMode check themselves, as shown in the community comment.

**Recommended proposal:** Use IsAncestorSiteInDesignMode with polyfill

**Why:** Fixes the root cause for all users without requiring subclassing. Small effort, low risk.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.82 (82%) |
| Reason | Root cause is confirmed by code investigation, fix approach is well-understood, and a community workaround validates the approach. Only open question is polyfill strategy for older TFMs. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, views, windows-classic labels | labels=type/bug, area/SkiaSharp.Views, os/Windows-Classic |
| add-comment | medium | 0.82 (82%) | Confirm root cause, note TFM polyfill consideration, and link community workaround | — |

**Comment draft for `add-comment`:**

```markdown
Confirmed: both `SKControl.OnPaint` and `SKGLControl.OnPaint` use `DesignMode` which only returns `true` when the control is the root designer surface. When either control is hosted inside a `UserControl` that is being designed, `DesignMode` is `false` and Skia/OpenGL rendering fires.

**Fix:** Replace the `DesignMode` check with one that also covers `Control.IsAncestorSiteInDesignMode` (available since .NET 6). For older TFMs a polyfill walking `Site?.DesignMode` up the parent chain is needed.

**Workaround until fixed:** Subclass `SKGLControl` and override `OnPaint`:

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    if (IsDesignMode) // GLControl property, checks self + ancestors via OpenTK
    {
        e.Graphics.Clear(BackColor);
        return;
    }
    base.OnPaint(e);
}
```

For `SKControl`, the same pattern applies using `DesignMode || IsAncestorSiteInDesignMode` (.NET 6+).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3643,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-28T05:36:50Z"
  },
  "summary": "SKControl and SKGLControl only check DesignMode on themselves, missing ancestor design mode detection; controls embedded in a UserControl being edited in the WinForms designer will attempt to render with Skia/OpenGL instead of being suppressed.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.85
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0-windows",
        "net462"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a WinForms UserControl that contains an SKControl or SKGLControl",
        "Open the parent Form that contains that UserControl in the VS designer",
        "Observe that DesignMode on the SKControl returns false even though it is inside a design-time host",
        "The control attempts to render with Skia/OpenGL instead of showing a design-time placeholder"
      ],
      "environmentDetails": "Windows Classic, Visual Studio WinForms designer, any .NET version",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3614#issuecomment-4182840423",
          "description": "Original comment from @xackus identifying the issue in PR #3614"
        }
      ],
      "relatedIssues": [
        2033
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [],
      "currentRelevance": "likely",
      "relevanceReason": "The DesignMode-only check is still present in current source at SKControl.cs:28 and SKGLControl.cs:74; no fix has been merged."
    }
  },
  "analysis": {
    "summary": "Both SKControl and SKGLControl check only Control.DesignMode in OnPaint to suppress rendering in the WinForms designer. DesignMode returns true only when the control is the root of the designer surface; when an SKControl is embedded inside a UserControl that is being edited, DesignMode is false and Skia/OpenGL rendering fires, which can crash the designer process. The fix is to also check Control.IsAncestorSiteInDesignMode (available since .NET 6; needs polyfill on older TFMs).",
    "rationale": "The issue is clearly a behavioral bug: the design-mode guard is incomplete by design of the .NET Control.DesignMode property. Code investigation confirms both affected files still use only DesignMode. The proposed fix (IsAncestorSiteInDesignMode) is well-known and the issue has been acknowledged by the maintainer (mattleibow opened it). This is ready to fix once the TFM polyfill strategy is decided.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKControl.cs",
        "lines": "28-29",
        "finding": "OnPaint checks only 'if (DesignMode) return;' — misses ancestor design mode when SKControl is nested inside a UserControl being designed.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "73-78",
        "finding": "OnPaint checks only 'if (DesignMode)' and clears background — same incomplete check as SKControl; OpenGL calls will fire when nested in a designer host.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The current code only prevents painting if SKControl itself is being edited in the WinForms designer.",
        "source": "issue body (from @xackus in PR #3614)",
        "interpretation": "Confirms the guard is incomplete; ancestor design mode is not detected."
      },
      {
        "text": "You might want to use Control.IsAncestorSiteInDesignMode (needs to be polyfilled on older targets)",
        "source": "issue body",
        "interpretation": "Proposed fix is already identified; needs polyfill work for pre-.NET 6 targets."
      },
      {
        "text": "I have overridden the OnPaint method to check the IsDesignMode property, allowing SKGLControl to function correctly within the designer.",
        "source": "comment by @sueastward",
        "interpretation": "Community confirms the problem is real and workaround is feasible per-control."
      }
    ],
    "workarounds": [
      "Override OnPaint in a subclass and check IsDesignMode (GLControl property for SKGLControl) before calling base.OnPaint, as shown by @sueastward in the comments.",
      "For SKControl subclasses, override OnPaint and add: if (DesignMode || this.IsAncestorSiteInDesignMode()) return; before calling base.OnPaint (requires .NET 6+)."
    ],
    "nextQuestions": [
      "What TFMs does SkiaSharp.Views.WindowsForms currently target — does it need a polyfill for net462/netstandard?",
      "Should SKGLControl use GLControl.IsDesignMode (OpenTK property) or Control.IsAncestorSiteInDesignMode?",
      "Are there other SKControl subclasses (e.g., in Uno WinForms) with the same incomplete guard?"
    ],
    "resolution": {
      "hypothesis": "Replace the simple DesignMode check in both SKControl.OnPaint and SKGLControl.OnPaint with a helper that also checks IsAncestorSiteInDesignMode, polyfilled for older TFMs.",
      "proposals": [
        {
          "title": "Use IsAncestorSiteInDesignMode with polyfill",
          "description": "Add an IsInDesignMode() helper that returns DesignMode || (IsAncestorSiteInDesignMode available ? IsAncestorSiteInDesignMode : false). Use this helper in both SKControl and SKGLControl OnPaint.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Subclass workaround (user-side)",
          "description": "Users can subclass SKControl/SKGLControl and override OnPaint to add the IsAncestorSiteInDesignMode check themselves, as shown in the community comment.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use IsAncestorSiteInDesignMode with polyfill",
      "recommendedReason": "Fixes the root cause for all users without requiring subclassing. Small effort, low risk."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.82,
      "reason": "Root cause is confirmed by code investigation, fix approach is well-understood, and a community workaround validates the approach. Only open question is polyfill strategy for older TFMs.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, views, windows-classic labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm root cause, note TFM polyfill consideration, and link community workaround",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Confirmed: both `SKControl.OnPaint` and `SKGLControl.OnPaint` use `DesignMode` which only returns `true` when the control is the root designer surface. When either control is hosted inside a `UserControl` that is being designed, `DesignMode` is `false` and Skia/OpenGL rendering fires.\n\n**Fix:** Replace the `DesignMode` check with one that also covers `Control.IsAncestorSiteInDesignMode` (available since .NET 6). For older TFMs a polyfill walking `Site?.DesignMode` up the parent chain is needed.\n\n**Workaround until fixed:** Subclass `SKGLControl` and override `OnPaint`:\n\n```csharp\nprotected override void OnPaint(PaintEventArgs e)\n{\n    if (IsDesignMode) // GLControl property, checks self + ancestors via OpenTK\n    {\n        e.Graphics.Clear(BackColor);\n        return;\n    }\n    base.OnPaint(e);\n}\n```\n\nFor `SKControl`, the same pattern applies using `DesignMode || IsAncestorSiteInDesignMode` (.NET 6+)."
      }
    ]
  }
}
```

</details>
