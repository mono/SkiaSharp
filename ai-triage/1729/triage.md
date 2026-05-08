# Issue Triage Report — #1729

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T07:02:51Z |
| Type | type/feature-request (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Feature request to expose path equality comparison as SKPath.Equals(SKPath), similar to SkPath::operator== in native Skia, to allow comparing two paths without slow C# iteration.

**Analysis:** No SKPath.Equals(SKPath) or equivalent exists in SkiaSharp. The C API layer (SkiaApi.generated.cs) contains no sk_path_equals binding. Skia C++ exposes SkPath::operator==, and PathKit (WASM) exposes equals(). Implementing this would require adding a C API shim and a C# wrapper method.

**Recommendations:** **needs-investigation** — Valid feature request with clear upstream support. Needs investigation of C++ SkPath equality API availability and structural vs geometric semantics before implementation.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** SkiaSharp — no specific version or platform mentioned

**Repository links:**
- https://skia.org/docs/user/modules/pathkit/#equalsotherpath — PathKit documentation for equals(otherPath)

## Analysis

### Technical Summary

No SKPath.Equals(SKPath) or equivalent exists in SkiaSharp. The C API layer (SkiaApi.generated.cs) contains no sk_path_equals binding. Skia C++ exposes SkPath::operator==, and PathKit (WASM) exposes equals(). Implementing this would require adding a C API shim and a C# wrapper method.

### Rationale

The issue clearly describes a new API addition (path equality comparison) that does not currently exist in the SkiaSharp binding. The reporter references the upstream Skia PathKit equals() function and notes the current C# workaround (iterating over path verbs/points) is considerably slower. No implementation of this feature was found in the codebase.

### Key Signals

- "Skia has an `equals(otherPath)` method that would be good to expose" — **issue body** (Reporter is aware of upstream Skia capability and requesting a binding to it.)
- "Iterating over the paths in C# and looking for differences, but that would be considerably slower" — **issue body** (Current workaround is performance-impacting; the native method would be more efficient.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPath.cs` | — | direct | SKPath class has no Equals(SKPath) method or operator== overload. Standard Object.Equals is not overridden for structural path comparison. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | No sk_path_equals C API binding exists in the generated P/Invoke layer. The existing path methods are: sk_path_new, sk_path_clone, sk_path_delete, sk_path_count_points, sk_path_count_verbs, etc. — no equality function. |

### Workarounds

- Iterate verb-by-verb using SKPath.GetVerbs() / GetPoints() and compare manually (slower but functional for correctness checks).
- Serialize both paths to SVG strings via SKPath.ToSvgPathData() and compare strings (approximation — may differ if constructed differently but represent same shape).

### Next Questions

- Does Skia C++ expose this as SkPath::operator== and/or an explicit equals method in the public C++ headers?
- Should the method compare geometrically (same shape) or structurally (same sequence of verbs/points)?

### Resolution Proposals

**Hypothesis:** Expose SkPath's native equality check by adding a C API shim (sk_path_equals) and a C# wrapper method SKPath.Equals(SKPath other).

1. **Expose path equality via C API and C# wrapper** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add sk_path_equals(sk_path_t a, sk_path_t b) C API function wrapping SkPath::operator== (or SkPath::equals if available), regenerate bindings, then add SKPath.Equals(SKPath other) in the C# wrapper.
2. **Workaround: compare SVG string serializations** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Use SKPath.ToSvgPathData() on both paths and compare the resulting strings. This works if paths were constructed identically but will not detect geometric equivalence of differently-constructed paths.

**Recommended proposal:** Expose path equality via C API and C# wrapper

**Why:** This is the correct long-term solution — leverage the native SkPath equality which is structural, fast, and accurate.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request with clear upstream support. Needs investigation of C++ SkPath equality API availability and structural vs geometric semantics before implementation. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply feature-request and SkiaSharp area labels | labels=type/feature-request, area/SkiaSharp |
| add-comment | medium | 0.82 (82%) | Acknowledge request and share workaround and implementation notes | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the feature request! You're right that native Skia exposes path equality (SkPath::operator==) and this isn't currently exposed in SkiaSharp.

As a workaround in the meantime, you can compare paths by iterating verbs and points:

```csharp
static bool PathsEqual(SKPath a, SKPath b)
{
    if (a.VerbCount != b.VerbCount || a.PointCount != b.PointCount)
        return false;
    var aVerbs = a.GetVerbs();
    var bVerbs = b.GetVerbs();
    var aPoints = a.GetPoints(a.PointCount);
    var bPoints = b.GetPoints(b.PointCount);
    for (int i = 0; i < aVerbs.Length; i++)
        if (aVerbs[i] != bVerbs[i]) return false;
    for (int i = 0; i < aPoints.Length; i++)
        if (aPoints[i] != bPoints[i]) return false;
    return true;
}
```

Adding a native `SKPath.Equals(SKPath other)` is feasible by wrapping `SkPath::operator==` through the C API — contributions welcome!
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1729,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T07:02:51Z"
  },
  "summary": "Feature request to expose path equality comparison as SKPath.Equals(SKPath), similar to SkPath::operator== in native Skia, to allow comparing two paths without slow C# iteration.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "SkiaSharp — no specific version or platform mentioned",
      "repoLinks": [
        {
          "url": "https://skia.org/docs/user/modules/pathkit/#equalsotherpath",
          "description": "PathKit documentation for equals(otherPath)"
        }
      ]
    }
  },
  "analysis": {
    "summary": "No SKPath.Equals(SKPath) or equivalent exists in SkiaSharp. The C API layer (SkiaApi.generated.cs) contains no sk_path_equals binding. Skia C++ exposes SkPath::operator==, and PathKit (WASM) exposes equals(). Implementing this would require adding a C API shim and a C# wrapper method.",
    "rationale": "The issue clearly describes a new API addition (path equality comparison) that does not currently exist in the SkiaSharp binding. The reporter references the upstream Skia PathKit equals() function and notes the current C# workaround (iterating over path verbs/points) is considerably slower. No implementation of this feature was found in the codebase.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "finding": "SKPath class has no Equals(SKPath) method or operator== overload. Standard Object.Equals is not overridden for structural path comparison.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "No sk_path_equals C API binding exists in the generated P/Invoke layer. The existing path methods are: sk_path_new, sk_path_clone, sk_path_delete, sk_path_count_points, sk_path_count_verbs, etc. — no equality function.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Skia has an `equals(otherPath)` method that would be good to expose",
        "source": "issue body",
        "interpretation": "Reporter is aware of upstream Skia capability and requesting a binding to it."
      },
      {
        "text": "Iterating over the paths in C# and looking for differences, but that would be considerably slower",
        "source": "issue body",
        "interpretation": "Current workaround is performance-impacting; the native method would be more efficient."
      }
    ],
    "workarounds": [
      "Iterate verb-by-verb using SKPath.GetVerbs() / GetPoints() and compare manually (slower but functional for correctness checks).",
      "Serialize both paths to SVG strings via SKPath.ToSvgPathData() and compare strings (approximation — may differ if constructed differently but represent same shape)."
    ],
    "nextQuestions": [
      "Does Skia C++ expose this as SkPath::operator== and/or an explicit equals method in the public C++ headers?",
      "Should the method compare geometrically (same shape) or structurally (same sequence of verbs/points)?"
    ],
    "resolution": {
      "hypothesis": "Expose SkPath's native equality check by adding a C API shim (sk_path_equals) and a C# wrapper method SKPath.Equals(SKPath other).",
      "proposals": [
        {
          "title": "Expose path equality via C API and C# wrapper",
          "description": "Add sk_path_equals(sk_path_t a, sk_path_t b) C API function wrapping SkPath::operator== (or SkPath::equals if available), regenerate bindings, then add SKPath.Equals(SKPath other) in the C# wrapper.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround: compare SVG string serializations",
          "description": "Use SKPath.ToSvgPathData() on both paths and compare the resulting strings. This works if paths were constructed identically but will not detect geometric equivalence of differently-constructed paths.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Expose path equality via C API and C# wrapper",
      "recommendedReason": "This is the correct long-term solution — leverage the native SkPath equality which is structural, fast, and accurate."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Valid feature request with clear upstream support. Needs investigation of C++ SkPath equality API availability and structural vs geometric semantics before implementation.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request and SkiaSharp area labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request and share workaround and implementation notes",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the feature request! You're right that native Skia exposes path equality (SkPath::operator==) and this isn't currently exposed in SkiaSharp.\n\nAs a workaround in the meantime, you can compare paths by iterating verbs and points:\n\n```csharp\nstatic bool PathsEqual(SKPath a, SKPath b)\n{\n    if (a.VerbCount != b.VerbCount || a.PointCount != b.PointCount)\n        return false;\n    var aVerbs = a.GetVerbs();\n    var bVerbs = b.GetVerbs();\n    var aPoints = a.GetPoints(a.PointCount);\n    var bPoints = b.GetPoints(b.PointCount);\n    for (int i = 0; i < aVerbs.Length; i++)\n        if (aVerbs[i] != bVerbs[i]) return false;\n    for (int i = 0; i < aPoints.Length; i++)\n        if (aPoints[i] != bPoints[i]) return false;\n    return true;\n}\n```\n\nAdding a native `SKPath.Equals(SKPath other)` is feasible by wrapping `SkPath::operator==` through the C API — contributions welcome!"
      }
    ]
  }
}
```

</details>
