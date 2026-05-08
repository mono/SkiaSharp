# Issue Triage Report — #2506

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T22:39:00Z |
| Type | type/bug (0.82 (82%)) |
| Area | area/SkiaSharp.Views.Maui (0.95 (95%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** Overriding MeasureOverride on a derived SKCanvasView in MAUI (2.88.3) does not propagate DesiredSize.Height when the control returns a non-zero height from MeasureOverride.

**Analysis:** The MAUI SKCanvasView derives from Microsoft.Maui.Controls.View but does not override MeasureOverride; it simply uses the default MAUI measure path. When a subclass returns a custom Size from MeasureOverride, MAUI's layout pipeline should propagate that size to DesiredSize, but reporters observe DesiredSize.Height staying at zero for horizontal orientations. This may be caused by MAUI's internal layout pipeline ignoring partial size returns or a constraint applied by the handler's platform view (SKXamlCanvas on Windows). No SkiaSharp-specific MeasureOverride logic exists in the codebase.

**Recommendations:** **needs-info** — The report lacks a minimal repro project and platform version details, making investigation difficult. The behavior warrants investigation once a repro is available.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Maui |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | partner/maui |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3239 — Related issue by the same author: InvalidateMeasure doesn't call MeasureOverride after the first call (MAUI SKCanvasView measuring)

**Code snippets:**

```csharp
Size measured = child.Measure(availableWidth, availableHeight);
Assert child.DesiredSize.Height > 0
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | net7.0-windows |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKCanvasView MAUI control does not override MeasureOverride or apply any size constraint fix; the issue is likely a MAUI framework integration concern still present in current versions. |

## Analysis

### Technical Summary

The MAUI SKCanvasView derives from Microsoft.Maui.Controls.View but does not override MeasureOverride; it simply uses the default MAUI measure path. When a subclass returns a custom Size from MeasureOverride, MAUI's layout pipeline should propagate that size to DesiredSize, but reporters observe DesiredSize.Height staying at zero for horizontal orientations. This may be caused by MAUI's internal layout pipeline ignoring partial size returns or a constraint applied by the handler's platform view (SKXamlCanvas on Windows). No SkiaSharp-specific MeasureOverride logic exists in the codebase.

### Rationale

Classified type/bug because the reported behavior (DesiredSize.Height not reflecting MeasureOverride return value) is an incorrect layout contract violation. Area is SkiaSharp.Views.Maui because the Views.Maui handler is the integration point between MAUI layouts and the native canvas. Severity medium because the layout is broken (zero height) but users can workaround via explicit HeightRequest. Repro quality is partial — no standalone minimal repro is attached. suggestedAction is needs-info because no minimal repro project is provided and no detailed version info was pasted.

### Key Signals

- "When Orientation is Horizontal, MeasureOverride returns a Size.Height = Thickness and DesiredSize.Height is always zero." — **issue body** (Only Height fails, Width works. This asymmetry suggests the native platform view or MAUI handler is clamping the Height but not Width.)
- "I've confirmed this behavior with StackLayout, AbsoluteLayout and my own custom LayoutManager." — **issue body** (Not a specific layout bug — the issue is in the SKCanvasView MAUI integration itself, not the caller layout.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs` | — | direct | SKCanvasView derives from Microsoft.Maui.Controls.View and does not override MeasureOverride or apply any custom size constraints. There is no SkiaSharp code interfering with the measure pass. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs` | — | direct | The Windows handler wraps SKXamlCanvas (a WinUI Canvas-derived element). WinUI Canvas by default stretches to fill available space and may not propagate a desired height back through the MAUI handler's measure path, potentially causing DesiredSize.Height to remain zero. |
| `source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs` | — | related | The handler uses the default ViewHandler.ViewMapper with no custom measure/layout mapper entries, confirming no SkiaSharp-level measure override is in place. |

### Next Questions

- Can the reporter provide a minimal standalone repro project?
- Does the issue reproduce on Android or iOS as well, or only Windows?
- What MAUI version is being used alongside SkiaSharp 2.88.3?

### Resolution Proposals

1. **Set HeightRequest explicitly as a workaround** — workaround, cost/xs, validated=untested
   - Until the MAUI integration is fixed, set HeightRequest in the overriding subclass to the thickness value returned by MeasureOverride.
2. **Investigate if MAUI ViewHandler cross-platform measure needs to be overridden** — investigation, cost/m, validated=untested
   - Check whether the SKCanvasView handler needs to override the platform-specific measure/layout methods to forward the virtual view's DesiredSize from MeasureOverride back to the native platform view, similar to how other MAUI custom view handlers implement cross-measure.

**Recommended proposal:** investigation-1

**Why:** The root cause is not yet confirmed — it may be a MAUI framework gap or a SkiaSharp handler gap. Investigation is needed before implementing a fix.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The report lacks a minimal repro project and platform version details, making investigation difficult. The behavior warrants investigation once a repro is available. |
| Suggested repro platform | windows |

### Missing Info

- Minimal standalone repro project (link to repo or attached zip)
- Full version info (MAUI version, .NET version, Windows version)
- Confirmation whether the issue also happens on Android or iOS

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp.Views.Maui, os/Windows-Classic, tenet/reliability, partner/maui | labels=type/bug, area/SkiaSharp.Views.Maui, os/Windows-Classic, tenet/reliability, partner/maui |
| add-comment | medium | 0.80 (80%) | Ask for a minimal repro project and MAUI version info; provide the HeightRequest workaround. | — |

**Comment draft for `add-comment`:**

```markdown
Thank you for the report!

We've investigated the SkiaSharp.Views.Maui code and confirmed that `SKCanvasView` does not override `MeasureOverride` itself, so the behavior you're seeing may be related to how MAUI's layout pipeline propagates the height from a derived `MeasureOverride` through the Windows platform handler.

To help us reproduce and investigate further, could you please provide:
1. A minimal standalone repro project (link to a GitHub repo or attached zip)
2. Your MAUI version and .NET version
3. Whether the issue also reproduces on Android or iOS

**In the meantime**, a known workaround is to set `HeightRequest` explicitly in your `MeasureOverride` override to match the `Thickness` value you're returning:

```csharp
protected override Size MeasureOverride(double widthConstraint, double heightConstraint)
{
    if (Orientation == Orientation.Horizontal)
    {
        HeightRequest = Thickness;
        return new Size(widthConstraint, Thickness);
    }
    else
    {
        WidthRequest = Thickness;
        return new Size(Thickness, heightConstraint);
    }
}
```

This is a workaround only and the underlying issue should still be fixed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2506,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T22:39:00Z"
  },
  "summary": "Overriding MeasureOverride on a derived SKCanvasView in MAUI (2.88.3) does not propagate DesiredSize.Height when the control returns a non-zero height from MeasureOverride.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.82
    },
    "area": {
      "value": "area/SkiaSharp.Views.Maui",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ],
    "partner": "partner/maui"
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0-windows"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "Size measured = child.Measure(availableWidth, availableHeight);\nAssert child.DesiredSize.Height > 0"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3239",
          "description": "Related issue by the same author: InvalidateMeasure doesn't call MeasureOverride after the first call (MAUI SKCanvasView measuring)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKCanvasView MAUI control does not override MeasureOverride or apply any size constraint fix; the issue is likely a MAUI framework integration concern still present in current versions."
    }
  },
  "analysis": {
    "summary": "The MAUI SKCanvasView derives from Microsoft.Maui.Controls.View but does not override MeasureOverride; it simply uses the default MAUI measure path. When a subclass returns a custom Size from MeasureOverride, MAUI's layout pipeline should propagate that size to DesiredSize, but reporters observe DesiredSize.Height staying at zero for horizontal orientations. This may be caused by MAUI's internal layout pipeline ignoring partial size returns or a constraint applied by the handler's platform view (SKXamlCanvas on Windows). No SkiaSharp-specific MeasureOverride logic exists in the codebase.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Controls/SKCanvasView.cs",
        "finding": "SKCanvasView derives from Microsoft.Maui.Controls.View and does not override MeasureOverride or apply any custom size constraints. There is no SkiaSharp code interfering with the measure pass.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.Windows.cs",
        "finding": "The Windows handler wraps SKXamlCanvas (a WinUI Canvas-derived element). WinUI Canvas by default stretches to fill available space and may not propagate a desired height back through the MAUI handler's measure path, potentially causing DesiredSize.Height to remain zero.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views.Maui/SkiaSharp.Views.Maui.Core/Handlers/SKCanvasView/SKCanvasViewHandler.cs",
        "finding": "The handler uses the default ViewHandler.ViewMapper with no custom measure/layout mapper entries, confirming no SkiaSharp-level measure override is in place.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "When Orientation is Horizontal, MeasureOverride returns a Size.Height = Thickness and DesiredSize.Height is always zero.",
        "source": "issue body",
        "interpretation": "Only Height fails, Width works. This asymmetry suggests the native platform view or MAUI handler is clamping the Height but not Width."
      },
      {
        "text": "I've confirmed this behavior with StackLayout, AbsoluteLayout and my own custom LayoutManager.",
        "source": "issue body",
        "interpretation": "Not a specific layout bug — the issue is in the SKCanvasView MAUI integration itself, not the caller layout."
      }
    ],
    "rationale": "Classified type/bug because the reported behavior (DesiredSize.Height not reflecting MeasureOverride return value) is an incorrect layout contract violation. Area is SkiaSharp.Views.Maui because the Views.Maui handler is the integration point between MAUI layouts and the native canvas. Severity medium because the layout is broken (zero height) but users can workaround via explicit HeightRequest. Repro quality is partial — no standalone minimal repro is attached. suggestedAction is needs-info because no minimal repro project is provided and no detailed version info was pasted.",
    "resolution": {
      "proposals": [
        {
          "title": "Set HeightRequest explicitly as a workaround",
          "description": "Until the MAUI integration is fixed, set HeightRequest in the overriding subclass to the thickness value returned by MeasureOverride.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Investigate if MAUI ViewHandler cross-platform measure needs to be overridden",
          "description": "Check whether the SKCanvasView handler needs to override the platform-specific measure/layout methods to forward the virtual view's DesiredSize from MeasureOverride back to the native platform view, similar to how other MAUI custom view handlers implement cross-measure.",
          "category": "investigation",
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "investigation-1",
      "recommendedReason": "The root cause is not yet confirmed — it may be a MAUI framework gap or a SkiaSharp handler gap. Investigation is needed before implementing a fix."
    },
    "nextQuestions": [
      "Can the reporter provide a minimal standalone repro project?",
      "Does the issue reproduce on Android or iOS as well, or only Windows?",
      "What MAUI version is being used alongside SkiaSharp 2.88.3?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The report lacks a minimal repro project and platform version details, making investigation difficult. The behavior warrants investigation once a repro is available.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Minimal standalone repro project (link to repo or attached zip)",
      "Full version info (MAUI version, .NET version, Windows version)",
      "Confirmation whether the issue also happens on Android or iOS"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp.Views.Maui, os/Windows-Classic, tenet/reliability, partner/maui",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp.Views.Maui",
          "os/Windows-Classic",
          "tenet/reliability",
          "partner/maui"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for a minimal repro project and MAUI version info; provide the HeightRequest workaround.",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thank you for the report!\n\nWe've investigated the SkiaSharp.Views.Maui code and confirmed that `SKCanvasView` does not override `MeasureOverride` itself, so the behavior you're seeing may be related to how MAUI's layout pipeline propagates the height from a derived `MeasureOverride` through the Windows platform handler.\n\nTo help us reproduce and investigate further, could you please provide:\n1. A minimal standalone repro project (link to a GitHub repo or attached zip)\n2. Your MAUI version and .NET version\n3. Whether the issue also reproduces on Android or iOS\n\n**In the meantime**, a known workaround is to set `HeightRequest` explicitly in your `MeasureOverride` override to match the `Thickness` value you're returning:\n\n```csharp\nprotected override Size MeasureOverride(double widthConstraint, double heightConstraint)\n{\n    if (Orientation == Orientation.Horizontal)\n    {\n        HeightRequest = Thickness;\n        return new Size(widthConstraint, Thickness);\n    }\n    else\n    {\n        WidthRequest = Thickness;\n        return new Size(Thickness, heightConstraint);\n    }\n}\n```\n\nThis is a workaround only and the underlying issue should still be fixed."
      }
    ]
  }
}
```

</details>
