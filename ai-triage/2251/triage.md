# Issue Triage Report — #2251

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T02:51:44Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.85 (85%)) |

**Issue Summary:** Feature request to add SKColor.FromName(string) factory method that resolves a named color string (e.g. 'blue') to an SKColor value.

**Analysis:** SKColor has Parse/TryParse for hex strings and FromHsl/FromHsv factories, but no color-name lookup. SKColors has ~140 static named color fields that could back such a lookup. Upstream Skia C API has no color-by-name functionality, so this would be a pure C# layer addition. A reflection-based workaround already exists in the community.

**Recommendations:** **keep-open** — Valid, low-effort feature request. A workaround exists but a first-class API would be better. Suitable for a community PR.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Call SKColor.FromName("blue") — method does not exist

**Environment:** SkiaSharp (any version), any platform

**Repository links:**
- https://github.com/mono/SkiaSharp/discussions/2250 — Initial discussion about this feature

## Analysis

### Technical Summary

SKColor has Parse/TryParse for hex strings and FromHsl/FromHsv factories, but no color-name lookup. SKColors has ~140 static named color fields that could back such a lookup. Upstream Skia C API has no color-by-name functionality, so this would be a pure C# layer addition. A reflection-based workaround already exists in the community.

### Rationale

This is a genuine feature gap: SKColor has no way to go from a human-readable color name (like 'blue') to an SKColor value without either hardcoding or using reflection. The feature is low complexity — it would be a dictionary lookup against the SKColors static fields. It does not require C API or native changes.

### Key Signals

- "var color = SKColor.FromName(inputClrName);" — **issue body** (Reporter wants a named color factory method on SKColor.)
- "Is this function part of Skia? If not then it shouldn't be added." — **comment #1 by charlesroddie** (Community debate about whether the feature belongs in SkiaSharp vs. Skia upstream.)
- "System.Drawing.Color causes major problems in linux due to its utilization of libgdiplus" — **comment #2 by uxmanz** (The System.Drawing.Color workaround is problematic on Linux, motivating a native SkiaSharp solution.)
- "var skColor = typeof(SKColors).GetField(colorName); return (SKColor)skColor.GetValue(null);" — **comment #4 by nchabelengmc** (A community workaround using reflection on SKColors exists and received 3 thumbs-up.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKColor.cs` | 123-223 | direct | SKColor has Parse(string hexString) and TryParse(string, out SKColor) for hex strings, and FromHsl/FromHsv factories. No FromName or TryParseName method exists. |
| `binding/SkiaSharp/SKColors.cs` | 1-153 | direct | SKColors is a struct with ~140 static SKColor fields (PascalCase names like 'Blue', 'AliceBlue'). These could back a name-lookup dictionary without any native/C API changes. |

### Workarounds

- Use reflection: var field = typeof(SKColors).GetField(colorName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase); return field != null ? (SKColor)field.GetValue(null) : SKColors.Black;
- Use System.Drawing.Color.FromName() and convert to SKColor (not recommended on Linux due to libgdiplus dependency)

### Next Questions

- Should the lookup be case-insensitive?
- Should it accept CSS color names (which match the SKColors names)?
- Should there be a TryFromName overload returning bool?

### Resolution Proposals

**Hypothesis:** Add SKColor.TryParse overload or new SKColor.FromName / TryFromName methods that look up named colors from SKColors static fields using a pre-built dictionary.

1. **Add SKColor.TryParse overload with color name fallback** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Extend the existing TryParse to also accept named colors (e.g. 'blue', 'Blue', 'BLUE') by building a static case-insensitive dictionary from SKColors fields. Add a separate TryFromName(string, out SKColor) method following the existing TryParse pattern.
2. **Reflection-based workaround (user-side)** — workaround, confidence 0.90 (90%), cost/xs, validated=untested
   - Use reflection on SKColors to look up by name. Already shared in comments with 3 upvotes. Works today without any SkiaSharp changes.

**Recommended proposal:** Add SKColor.TryParse overload with color name fallback

**Why:** Pure C# change, follows existing TryParse pattern, no native changes needed. Low effort (cost/s).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.85 (85%) |
| Reason | Valid, low-effort feature request. A workaround exists but a first-class API would be better. Suitable for a community PR. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request and area/SkiaSharp labels | labels=type/feature-request, area/SkiaSharp, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Acknowledge request, share workaround, and invite a community PR | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! There's no `SKColor.FromName()` built into SkiaSharp yet.

In the meantime, you can use reflection against `SKColors` as a workaround:

```csharp
private static bool TryGetColorByName(string name, out SKColor color)
{
    var field = typeof(SKColors).GetField(name,
        System.Reflection.BindingFlags.Public |
        System.Reflection.BindingFlags.Static |
        System.Reflection.BindingFlags.IgnoreCase);
    if (field != null)
    {
        color = (SKColor)field.GetValue(null)!;
        return true;
    }
    color = SKColors.Empty;
    return false;
}
```

This covers all the named colors in `SKColors` (AliceBlue, Blue, etc.) and is case-insensitive. We'd welcome a PR that adds `SKColor.TryParse(string, out SKColor)` overload or a dedicated `SKColor.TryFromName(string, out SKColor)` method backed by a pre-built static dictionary.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2251,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T02:51:44Z"
  },
  "summary": "Feature request to add SKColor.FromName(string) factory method that resolves a named color string (e.g. 'blue') to an SKColor value.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKColor.FromName(\"blue\") — method does not exist"
      ],
      "environmentDetails": "SkiaSharp (any version), any platform",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/discussions/2250",
          "description": "Initial discussion about this feature"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKColor has Parse/TryParse for hex strings and FromHsl/FromHsv factories, but no color-name lookup. SKColors has ~140 static named color fields that could back such a lookup. Upstream Skia C API has no color-by-name functionality, so this would be a pure C# layer addition. A reflection-based workaround already exists in the community.",
    "rationale": "This is a genuine feature gap: SKColor has no way to go from a human-readable color name (like 'blue') to an SKColor value without either hardcoding or using reflection. The feature is low complexity — it would be a dictionary lookup against the SKColors static fields. It does not require C API or native changes.",
    "keySignals": [
      {
        "text": "var color = SKColor.FromName(inputClrName);",
        "source": "issue body",
        "interpretation": "Reporter wants a named color factory method on SKColor."
      },
      {
        "text": "Is this function part of Skia? If not then it shouldn't be added.",
        "source": "comment #1 by charlesroddie",
        "interpretation": "Community debate about whether the feature belongs in SkiaSharp vs. Skia upstream."
      },
      {
        "text": "System.Drawing.Color causes major problems in linux due to its utilization of libgdiplus",
        "source": "comment #2 by uxmanz",
        "interpretation": "The System.Drawing.Color workaround is problematic on Linux, motivating a native SkiaSharp solution."
      },
      {
        "text": "var skColor = typeof(SKColors).GetField(colorName); return (SKColor)skColor.GetValue(null);",
        "source": "comment #4 by nchabelengmc",
        "interpretation": "A community workaround using reflection on SKColors exists and received 3 thumbs-up."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKColor.cs",
        "lines": "123-223",
        "finding": "SKColor has Parse(string hexString) and TryParse(string, out SKColor) for hex strings, and FromHsl/FromHsv factories. No FromName or TryParseName method exists.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKColors.cs",
        "lines": "1-153",
        "finding": "SKColors is a struct with ~140 static SKColor fields (PascalCase names like 'Blue', 'AliceBlue'). These could back a name-lookup dictionary without any native/C API changes.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Use reflection: var field = typeof(SKColors).GetField(colorName, BindingFlags.Public | BindingFlags.Static | BindingFlags.IgnoreCase); return field != null ? (SKColor)field.GetValue(null) : SKColors.Black;",
      "Use System.Drawing.Color.FromName() and convert to SKColor (not recommended on Linux due to libgdiplus dependency)"
    ],
    "nextQuestions": [
      "Should the lookup be case-insensitive?",
      "Should it accept CSS color names (which match the SKColors names)?",
      "Should there be a TryFromName overload returning bool?"
    ],
    "resolution": {
      "hypothesis": "Add SKColor.TryParse overload or new SKColor.FromName / TryFromName methods that look up named colors from SKColors static fields using a pre-built dictionary.",
      "proposals": [
        {
          "title": "Add SKColor.TryParse overload with color name fallback",
          "description": "Extend the existing TryParse to also accept named colors (e.g. 'blue', 'Blue', 'BLUE') by building a static case-insensitive dictionary from SKColors fields. Add a separate TryFromName(string, out SKColor) method following the existing TryParse pattern.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Reflection-based workaround (user-side)",
          "description": "Use reflection on SKColors to look up by name. Already shared in comments with 3 upvotes. Works today without any SkiaSharp changes.",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add SKColor.TryParse overload with color name fallback",
      "recommendedReason": "Pure C# change, follows existing TryParse pattern, no native changes needed. Low effort (cost/s)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.85,
      "reason": "Valid, low-effort feature request. A workaround exists but a first-class API would be better. Suitable for a community PR.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and area/SkiaSharp labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request, share workaround, and invite a community PR",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the feature request! There's no `SKColor.FromName()` built into SkiaSharp yet.\n\nIn the meantime, you can use reflection against `SKColors` as a workaround:\n\n```csharp\nprivate static bool TryGetColorByName(string name, out SKColor color)\n{\n    var field = typeof(SKColors).GetField(name,\n        System.Reflection.BindingFlags.Public |\n        System.Reflection.BindingFlags.Static |\n        System.Reflection.BindingFlags.IgnoreCase);\n    if (field != null)\n    {\n        color = (SKColor)field.GetValue(null)!;\n        return true;\n    }\n    color = SKColors.Empty;\n    return false;\n}\n```\n\nThis covers all the named colors in `SKColors` (AliceBlue, Blue, etc.) and is case-insensitive. We'd welcome a PR that adds `SKColor.TryParse(string, out SKColor)` overload or a dedicated `SKColor.TryFromName(string, out SKColor)` method backed by a pre-built static dictionary."
      }
    ]
  }
}
```

</details>
