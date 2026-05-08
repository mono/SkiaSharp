# Issue Triage Report — #2366

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T09:07:28Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** Feature request to have SkiaSharp classes that expose a Clone() method (such as SKPaint and SKTypeface) implement the ICloneable interface to enable generic extension methods and interoperability with code that depends on ICloneable.

**Analysis:** SKPaint exposes a strongly-typed Clone() method (returning SKPaint) but does not implement ICloneable. Adding ICloneable to SKPaint (and any other classes with Clone()) would be a small, non-breaking change that enables generic extension patterns.

**Recommendations:** **keep-open** — Valid, low-effort enhancement. The reporter has a workaround and there is no bug. The issue should stay open as a tracked enhancement request.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
public static T With<T>(this T receiver, Action<T> method) where T : ICloneable {
    var ret = (T) receiver.Clone();
    method.Invoke(receiver);
    return ret;
}

var p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };
var p2 = p1.With(p => p.Color = SKColors.Magenta);
```

## Analysis

### Technical Summary

SKPaint exposes a strongly-typed Clone() method (returning SKPaint) but does not implement ICloneable. Adding ICloneable to SKPaint (and any other classes with Clone()) would be a small, non-breaking change that enables generic extension patterns.

### Rationale

This is type/enhancement because ICloneable would improve an existing Clone() method's interface contract without adding new functionality. The area is area/SkiaSharp since SKPaint is a core binding class. Tenet compatibility applies since ICloneable would improve generic interoperability with .NET patterns.

### Key Signals

- "Is there any reason it shouldn't implement ICloneable?" — **issue body** (Reporter is asking whether there is a design reason preventing ICloneable adoption.)
- "I have implemented a specialized version of the method for now." — **issue body** (Reporter has a working workaround but wants the cleaner generic approach.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 302-303 | direct | SKPaint.Clone() exists and returns SKPaint, wrapping sk_compatpaint_clone. The class does not implement ICloneable. |
| `binding/SkiaSharp/SKTypeface.cs` | — | related | SKTypeface has several Clone overloads (Clone(ReadOnlySpan), Clone(int), Clone(SKFontArguments)) but none implement ICloneable. |

### Workarounds

- Define a non-generic extension method or a wrapper specific to SKPaint: var p2 = p1.Clone(); p2.Color = SKColors.Magenta;
- Use a local delegate or helper function constrained to SKPaint instead of requiring ICloneable.

### Resolution Proposals

**Hypothesis:** Add ICloneable to SKPaint and other classes that already expose a Clone() method. The explicit interface implementation would return object (required by ICloneable) while the existing strongly-typed Clone() method remains unchanged.

1. **Manual clone without ICloneable constraint** — workaround, cost/xs, validated=yes
   - Clone SKPaint directly and set properties without generic machinery.

```csharp
var p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };
var p2 = p1.Clone();
p2.Color = SKColors.Magenta;
```
2. **Implement ICloneable on SKPaint (and other Clone-capable classes)** — fix, cost/s, validated=untested
   - Add ICloneable to SKPaint, SKTypeface, and any other SkiaSharp class with a Clone method. Use explicit interface implementation so the strongly-typed Clone() signature is preserved.

**Recommended proposal:** workaround-1

**Why:** The workaround is trivially available today. The fix is straightforward but requires a minor API surface change and maintainer decision.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Valid, low-effort enhancement. The reporter has a workaround and there is no bug. The issue should stay open as a tracked enhancement request. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/enhancement, area/SkiaSharp, and tenet/compatibility labels. | labels=type/enhancement, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Acknowledge the request, confirm the workaround, and indicate the fix is feasible but awaits maintainer decision. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request!

You can achieve the same result today with a direct clone:

```csharp
var p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };
var p2 = p1.Clone();
p2.Color = SKColors.Magenta;
```

Implementing `ICloneable` on `SKPaint` (and other classes that already have a `Clone()` method) would be a small, non-breaking change. An explicit interface implementation preserving the strongly-typed `Clone()` is the natural approach.

Tracking this as an enhancement for maintainer evaluation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2366,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T09:07:28Z"
  },
  "summary": "Feature request to have SkiaSharp classes that expose a Clone() method (such as SKPaint and SKTypeface) implement the ICloneable interface to enable generic extension methods and interoperability with code that depends on ICloneable.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "public static T With<T>(this T receiver, Action<T> method) where T : ICloneable {\n    var ret = (T) receiver.Clone();\n    method.Invoke(receiver);\n    return ret;\n}\n\nvar p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };\nvar p2 = p1.With(p => p.Color = SKColors.Magenta);"
      ]
    }
  },
  "analysis": {
    "summary": "SKPaint exposes a strongly-typed Clone() method (returning SKPaint) but does not implement ICloneable. Adding ICloneable to SKPaint (and any other classes with Clone()) would be a small, non-breaking change that enables generic extension patterns.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "lines": "302-303",
        "finding": "SKPaint.Clone() exists and returns SKPaint, wrapping sk_compatpaint_clone. The class does not implement ICloneable.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "finding": "SKTypeface has several Clone overloads (Clone(ReadOnlySpan), Clone(int), Clone(SKFontArguments)) but none implement ICloneable.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "Is there any reason it shouldn't implement ICloneable?",
        "source": "issue body",
        "interpretation": "Reporter is asking whether there is a design reason preventing ICloneable adoption."
      },
      {
        "text": "I have implemented a specialized version of the method for now.",
        "source": "issue body",
        "interpretation": "Reporter has a working workaround but wants the cleaner generic approach."
      }
    ],
    "rationale": "This is type/enhancement because ICloneable would improve an existing Clone() method's interface contract without adding new functionality. The area is area/SkiaSharp since SKPaint is a core binding class. Tenet compatibility applies since ICloneable would improve generic interoperability with .NET patterns.",
    "workarounds": [
      "Define a non-generic extension method or a wrapper specific to SKPaint: var p2 = p1.Clone(); p2.Color = SKColors.Magenta;",
      "Use a local delegate or helper function constrained to SKPaint instead of requiring ICloneable."
    ],
    "resolution": {
      "hypothesis": "Add ICloneable to SKPaint and other classes that already expose a Clone() method. The explicit interface implementation would return object (required by ICloneable) while the existing strongly-typed Clone() method remains unchanged.",
      "proposals": [
        {
          "title": "Manual clone without ICloneable constraint",
          "category": "workaround",
          "description": "Clone SKPaint directly and set properties without generic machinery.",
          "codeSnippet": "var p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };\nvar p2 = p1.Clone();\np2.Color = SKColors.Magenta;",
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Implement ICloneable on SKPaint (and other Clone-capable classes)",
          "category": "fix",
          "description": "Add ICloneable to SKPaint, SKTypeface, and any other SkiaSharp class with a Clone method. Use explicit interface implementation so the strongly-typed Clone() signature is preserved.",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "workaround-1",
      "recommendedReason": "The workaround is trivially available today. The fix is straightforward but requires a minor API surface change and maintainer decision."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Valid, low-effort enhancement. The reporter has a workaround and there is no bug. The issue should stay open as a tracked enhancement request.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/enhancement, area/SkiaSharp, and tenet/compatibility labels.",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the request, confirm the workaround, and indicate the fix is feasible but awaits maintainer decision.",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the feature request!\n\nYou can achieve the same result today with a direct clone:\n\n```csharp\nvar p1 = new SKPaint { StrokeWidth = 2, Color = SKColors.Cyan };\nvar p2 = p1.Clone();\np2.Color = SKColors.Magenta;\n```\n\nImplementing `ICloneable` on `SKPaint` (and other classes that already have a `Clone()` method) would be a small, non-breaking change. An explicit interface implementation preserving the strongly-typed `Clone()` is the natural approach.\n\nTracking this as an enhancement for maintainer evaluation."
      }
    ]
  }
}
```

</details>
