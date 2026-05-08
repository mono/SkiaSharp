# Issue Triage Report — #1684

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T19:18:33Z |
| Type | type/question (0.75 (75%)) |
| Area | area/SkiaSharp.Views (0.85 (85%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** Reporter gets CS0672/CS0809 compiler warning 'Obsolete member overrides non-obsolete member' when overriding OnSizeChanged in a custom class that inherits from SKCanvasView on Android, after upgrading to newer Xamarin.AndroidX packages.

**Analysis:** The reporter receives a compiler warning about OnSizeChanged when their custom class overrides it from SKCanvasView. The SkiaSharp source shows SKCanvasView.OnSizeChanged is NOT marked [Obsolete], but newer AndroidX bindings may have deprecated the underlying Android View.OnSizeChanged, causing the warning to propagate through the inheritance chain. The difference between CircularGauge (affected) and SemiCircularGauge (unaffected) suggests user code may differ between the two, but no code was provided to confirm.

**Recommendations:** **needs-info** — No code was shared to confirm whether this is a SkiaSharp issue or user code; the difference between CircularGauge and SemiCircularGauge needs explanation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp.Views |
| Platforms | os/Android |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Upgrade Xamarin.AndroidX.AppCompat to 1.2.0.7 and Xamarin.AndroidX.Navigation.Fragment to 2.3.3.1
2. Create a custom class (e.g. CircularGauge) inheriting from SKCanvasView
3. Override OnSizeChanged in that custom class
4. Observe compiler warning 'Obsolete member overrides non-obsolete member'

**Environment:** Android 7.1 to 11, SkiaSharp.Views 2.80.2, Xamarin.AndroidX.AppCompat 1.2.0.7, Xamarin.AndroidX.Navigation.Fragment 2.3.3.1

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | SkiaSharp.Views has been updated since 2.80.2; unclear if the issue still manifests with current versions. |

## Analysis

### Technical Summary

The reporter receives a compiler warning about OnSizeChanged when their custom class overrides it from SKCanvasView. The SkiaSharp source shows SKCanvasView.OnSizeChanged is NOT marked [Obsolete], but newer AndroidX bindings may have deprecated the underlying Android View.OnSizeChanged, causing the warning to propagate through the inheritance chain. The difference between CircularGauge (affected) and SemiCircularGauge (unaffected) suggests user code may differ between the two, but no code was provided to confirm.

### Rationale

This is best classified as a question rather than a bug because: (1) SkiaSharp source code confirms OnSizeChanged is not marked [Obsolete]; (2) the warning only affects CircularGauge and not SemiCircularGauge, suggesting user code differences; (3) no repro code was shared to confirm whether this is a SkiaSharp issue or user code; (4) if newer AndroidX bindings deprecated View.OnSizeChanged, SkiaSharp may need to add a pragma or [Obsolete] relay, but this cannot be verified without code.

### Key Signals

- "only on the CircularGauge, I get the message in the heading for OnSizeChanged" — **issue body** (Issue is specific to CircularGauge vs SemiCircularGauge — suggests user code difference, not a blanket SkiaSharp issue.)
- "the method isn't marked as an obsolete/deprecated member if the first place" — **issue body** (Reporter confirms SKCanvasView.OnSizeChanged is not marked [Obsolete] in their view — consistent with source code findings.)
- "I recently upgraded to Xamarin.AndroidX.AppCompat 1.2.0.7" — **issue body** (AndroidX upgrade is the trigger — newer bindings may have deprecated View.OnSizeChanged, causing the propagation warning.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 119-125 | direct | SKCanvasView.OnSizeChanged overrides Android View.OnSizeChanged without [Obsolete] attribute. If AndroidX bindings mark View.OnSizeChanged as deprecated, this override may propagate a compiler warning to subclasses. |
| `source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs` | 1-11 | context | SKCanvasView inherits from Android.Views.View. No [Obsolete] attributes found anywhere in the Android SKCanvasView implementation. |

### Workarounds

- Add #pragma warning disable CS0672 (or CS0809) around the OnSizeChanged override to suppress the warning
- Remove the OnSizeChanged override and use PaintSurface event instead to respond to canvas size changes

### Next Questions

- Can the reporter share the CircularGauge class definition showing the OnSizeChanged override?
- Does the SemiCircularGauge class also override OnSizeChanged, or is it absent?
- What is the exact compiler warning code (CS0672 or CS0809)?

### Resolution Proposals

**Hypothesis:** The CircularGauge class has OnSizeChanged marked [Obsolete] somewhere in its own inheritance chain (perhaps user-defined), OR the AndroidX upgrade marked View.OnSizeChanged deprecated and SKCanvasView's override is missing a [Obsolete] relay.

1. **Suppress the warning with pragma** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - If the user needs to override OnSizeChanged, add #pragma warning disable CS0672 (or appropriate code) around the method.
2. **Use PaintSurface event for sizing logic** — alternative, confidence 0.75 (75%), cost/s, validated=untested
   - Instead of overriding OnSizeChanged, handle sizing within the PaintSurface event handler where SKPaintSurfaceEventArgs.Info.Size gives current dimensions.

**Recommended proposal:** Suppress the warning with pragma

**Why:** Minimal change that unblocks the reporter while they await more investigation.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | No code was shared to confirm whether this is a SkiaSharp issue or user code; the difference between CircularGauge and SemiCircularGauge needs explanation. |
| Suggested repro platform | linux |

### Missing Info

- CircularGauge and SemiCircularGauge class definitions (especially the OnSizeChanged override)
- Exact compiler warning code (CS0672 or CS0809)
- Whether SemiCircularGauge also overrides OnSizeChanged

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply question, views, and Android labels | labels=type/question, area/SkiaSharp.Views, os/Android |
| add-comment | medium | 0.80 (80%) | Request more info and provide workaround options | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. The SkiaSharp source shows `SKCanvasView.OnSizeChanged` is not marked `[Obsolete]`, so the warning is likely related to how the newer AndroidX bindings expose the underlying Android `View.OnSizeChanged`.

To help diagnose this:
1. Can you share the `CircularGauge` class definition, particularly the `OnSizeChanged` override?
2. Does `SemiCircularGauge` also override `OnSizeChanged`? If so, does it get the same warning?
3. What is the exact compiler warning code shown (CS0672 or CS0809)?

In the meantime, you can suppress the warning with:
```csharp
#pragma warning disable CS0672
protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
{
    base.OnSizeChanged(w, h, oldw, oldh);
    // your sizing logic
}
#pragma warning restore CS0672
```

Alternatively, you can avoid overriding `OnSizeChanged` entirely and handle sizing within the `PaintSurface` event handler, where `e.Info.Size` gives you the current canvas dimensions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1684,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T19:18:33Z"
  },
  "summary": "Reporter gets CS0672/CS0809 compiler warning 'Obsolete member overrides non-obsolete member' when overriding OnSizeChanged in a custom class that inherits from SKCanvasView on Android, after upgrading to newer Xamarin.AndroidX packages.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.75
    },
    "area": {
      "value": "area/SkiaSharp.Views",
      "confidence": 0.85
    },
    "platforms": [
      "os/Android"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Upgrade Xamarin.AndroidX.AppCompat to 1.2.0.7 and Xamarin.AndroidX.Navigation.Fragment to 2.3.3.1",
        "Create a custom class (e.g. CircularGauge) inheriting from SKCanvasView",
        "Override OnSizeChanged in that custom class",
        "Observe compiler warning 'Obsolete member overrides non-obsolete member'"
      ],
      "environmentDetails": "Android 7.1 to 11, SkiaSharp.Views 2.80.2, Xamarin.AndroidX.AppCompat 1.2.0.7, Xamarin.AndroidX.Navigation.Fragment 2.3.3.1"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "SkiaSharp.Views has been updated since 2.80.2; unclear if the issue still manifests with current versions."
    }
  },
  "analysis": {
    "summary": "The reporter receives a compiler warning about OnSizeChanged when their custom class overrides it from SKCanvasView. The SkiaSharp source shows SKCanvasView.OnSizeChanged is NOT marked [Obsolete], but newer AndroidX bindings may have deprecated the underlying Android View.OnSizeChanged, causing the warning to propagate through the inheritance chain. The difference between CircularGauge (affected) and SemiCircularGauge (unaffected) suggests user code may differ between the two, but no code was provided to confirm.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "119-125",
        "finding": "SKCanvasView.OnSizeChanged overrides Android View.OnSizeChanged without [Obsolete] attribute. If AndroidX bindings mark View.OnSizeChanged as deprecated, this override may propagate a compiler warning to subclasses.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views/Platform/Android/SKCanvasView.cs",
        "lines": "1-11",
        "finding": "SKCanvasView inherits from Android.Views.View. No [Obsolete] attributes found anywhere in the Android SKCanvasView implementation.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "only on the CircularGauge, I get the message in the heading for OnSizeChanged",
        "source": "issue body",
        "interpretation": "Issue is specific to CircularGauge vs SemiCircularGauge — suggests user code difference, not a blanket SkiaSharp issue."
      },
      {
        "text": "the method isn't marked as an obsolete/deprecated member if the first place",
        "source": "issue body",
        "interpretation": "Reporter confirms SKCanvasView.OnSizeChanged is not marked [Obsolete] in their view — consistent with source code findings."
      },
      {
        "text": "I recently upgraded to Xamarin.AndroidX.AppCompat 1.2.0.7",
        "source": "issue body",
        "interpretation": "AndroidX upgrade is the trigger — newer bindings may have deprecated View.OnSizeChanged, causing the propagation warning."
      }
    ],
    "rationale": "This is best classified as a question rather than a bug because: (1) SkiaSharp source code confirms OnSizeChanged is not marked [Obsolete]; (2) the warning only affects CircularGauge and not SemiCircularGauge, suggesting user code differences; (3) no repro code was shared to confirm whether this is a SkiaSharp issue or user code; (4) if newer AndroidX bindings deprecated View.OnSizeChanged, SkiaSharp may need to add a pragma or [Obsolete] relay, but this cannot be verified without code.",
    "workarounds": [
      "Add #pragma warning disable CS0672 (or CS0809) around the OnSizeChanged override to suppress the warning",
      "Remove the OnSizeChanged override and use PaintSurface event instead to respond to canvas size changes"
    ],
    "nextQuestions": [
      "Can the reporter share the CircularGauge class definition showing the OnSizeChanged override?",
      "Does the SemiCircularGauge class also override OnSizeChanged, or is it absent?",
      "What is the exact compiler warning code (CS0672 or CS0809)?"
    ],
    "resolution": {
      "hypothesis": "The CircularGauge class has OnSizeChanged marked [Obsolete] somewhere in its own inheritance chain (perhaps user-defined), OR the AndroidX upgrade marked View.OnSizeChanged deprecated and SKCanvasView's override is missing a [Obsolete] relay.",
      "proposals": [
        {
          "title": "Suppress the warning with pragma",
          "description": "If the user needs to override OnSizeChanged, add #pragma warning disable CS0672 (or appropriate code) around the method.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use PaintSurface event for sizing logic",
          "description": "Instead of overriding OnSizeChanged, handle sizing within the PaintSurface event handler where SKPaintSurfaceEventArgs.Info.Size gives current dimensions.",
          "category": "alternative",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Suppress the warning with pragma",
      "recommendedReason": "Minimal change that unblocks the reporter while they await more investigation."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "No code was shared to confirm whether this is a SkiaSharp issue or user code; the difference between CircularGauge and SemiCircularGauge needs explanation.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "CircularGauge and SemiCircularGauge class definitions (especially the OnSizeChanged override)",
      "Exact compiler warning code (CS0672 or CS0809)",
      "Whether SemiCircularGauge also overrides OnSizeChanged"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, views, and Android labels",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/question",
          "area/SkiaSharp.Views",
          "os/Android"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request more info and provide workaround options",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed report. The SkiaSharp source shows `SKCanvasView.OnSizeChanged` is not marked `[Obsolete]`, so the warning is likely related to how the newer AndroidX bindings expose the underlying Android `View.OnSizeChanged`.\n\nTo help diagnose this:\n1. Can you share the `CircularGauge` class definition, particularly the `OnSizeChanged` override?\n2. Does `SemiCircularGauge` also override `OnSizeChanged`? If so, does it get the same warning?\n3. What is the exact compiler warning code shown (CS0672 or CS0809)?\n\nIn the meantime, you can suppress the warning with:\n```csharp\n#pragma warning disable CS0672\nprotected override void OnSizeChanged(int w, int h, int oldw, int oldh)\n{\n    base.OnSizeChanged(w, h, oldw, oldh);\n    // your sizing logic\n}\n#pragma warning restore CS0672\n```\n\nAlternatively, you can avoid overriding `OnSizeChanged` entirely and handle sizing within the `PaintSurface` event handler, where `e.Info.Size` gives you the current canvas dimensions."
      }
    ]
  }
}
```

</details>
