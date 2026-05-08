# Issue Triage Report — #1953

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T01:09:00Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-info (0.88 (88%)) |

**Issue Summary:** Reporter claims SKShader.CreateLinearGradient returns null for certain parameters, contradicting the XML documentation claim that the function never returns null.

**Analysis:** The SKShader.CreateLinearGradient implementation passes the native function result directly through GetObject, which returns null when the native sk_shader_new_linear_gradient returns a null/zero pointer. This can occur for degenerate inputs (e.g., empty colors array with count=0 or NaN coordinates). The XML documentation incorrectly states 'This function never returns null', creating a discrepancy between the documented contract and actual behavior. Without a code snippet it is impossible to confirm the exact trigger, but the code path that can yield null is confirmed.

**Recommendations:** **needs-info** — Maintainer already requested a code snippet 3 months after filing with no response. The bug may be valid (GetObject can return null) but requires the exact parameters to confirm and fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Call SKShader.CreateLinearGradient with certain parameters (screenshot only, no code provided)
2. Observe that the return value is null

**Environment:** SkiaSharp 2.80.3, IDE: Rider (platform unspecified)

**Repository links:**
- https://user-images.githubusercontent.com/29846655/153790590-2b53ff62-4e5d-4abe-8bc1-67f121ea809b.png — Screenshot of parameters passed to CreateLinearGradient (exact values not readable without image)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | SKShader.CreateLinearGradient returns null |
| Repro quality | none |
| Target frameworks | netstandard |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The SKShader.CreateLinearGradient binding has not substantively changed since 2.80.3; GetObject still returns null for a zero native handle. |

## Analysis

### Technical Summary

The SKShader.CreateLinearGradient implementation passes the native function result directly through GetObject, which returns null when the native sk_shader_new_linear_gradient returns a null/zero pointer. This can occur for degenerate inputs (e.g., empty colors array with count=0 or NaN coordinates). The XML documentation incorrectly states 'This function never returns null', creating a discrepancy between the documented contract and actual behavior. Without a code snippet it is impossible to confirm the exact trigger, but the code path that can yield null is confirmed.

### Rationale

Classified as type/bug because the documented contract ('never returns null') conflicts with the observed behavior and the code shows a clear path to null return via GetObject(IntPtr.Zero). Area is area/SkiaSharp since the issue is in the core shader binding. Severity is medium: it only manifests for edge-case parameter values and doesn't crash the process, but violates the API contract. Cannot reproduce without a code snippet — suggestedAction is needs-info since the maintainer already requested a snippet three months prior with no response.

### Key Signals

- "SKShader.CreateLinearGradient returns null at these parameters" — **issue title and body** (Reporter observes null return from a function the docs say cannot be null.)
- "Returns a new , or an empty shader on error. This function never returns null." — **XML documentation quoted in issue** (Documentation claims cannot return null, but GetObject(IntPtr.Zero) returns null if the native Skia function fails for degenerate inputs.)
- "Could you attach a code snippet or repro sample?" — **maintainer comment (#1129051110)** (Maintainer already identified that more details are needed; no response from reporter.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKShader.cs` | 125-137 | direct | CreateLinearGradient validates null/length mismatch but does NOT guard against colors.Length == 0. It then calls GetObject(SkiaApi.sk_shader_new_linear_gradient(...)) — GetObject returns null when the native handle is IntPtr.Zero. |
| `binding/SkiaSharp/SKObject.cs` | 100-107 | direct | GetOrAddObject returns null explicitly when handle == IntPtr.Zero, so any native API returning nullptr will produce null in C#. |

### Workarounds

- Always check the return value of CreateLinearGradient for null and fall back to SKShader.CreateColor() or SKShader.CreateEmpty() to avoid NullReferenceException
- Ensure colors array has at least 2 elements; Skia requires count >= 1 but gradients are most useful with >= 2 colors
- Use SKShader.CreateColor(colors[0]) as a fallback if only one color is needed

### Next Questions

- What exact parameter values were passed (start, end, colors array, colorPos, mode)?
- Was colors an empty array (length 0)?
- What platform/OS was the reporter using?
- Does the issue still reproduce on 2.88.x?

### Resolution Proposals

**Hypothesis:** Native sk_shader_new_linear_gradient returns nullptr when called with invalid inputs (likely colors.Length == 0 or degenerate coordinates), and the C# wrapper passes that null through rather than throwing or returning an empty shader as the docs promise.

1. **Request minimal repro from reporter** — investigation, confidence 0.95 (95%), cost/xs, validated=untested
   - Ask reporter to provide the exact parameter values or a minimal code snippet showing the null return. This is needed before any code change can be made.
2. **Add input validation for colors.Length == 0** — fix, confidence 0.70 (70%), cost/xs, validated=untested
   - Add a check for colors.Length == 0 and throw ArgumentException, matching the behavior documented. This prevents passing count=0 to the native function which may return null.
3. **Update XML documentation to reflect nullable return** — fix, confidence 0.90 (90%), cost/xs, validated=untested
   - Remove or correct the 'never returns null' claim in the XML docs to accurately reflect that null may be returned for invalid parameters.

**Recommended proposal:** Request minimal repro from reporter

**Why:** Cannot confirm the exact trigger without a code snippet. The maintainer already asked once; a follow-up with guidance on what to include is the right next step.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.88 (88%) |
| Reason | Maintainer already requested a code snippet 3 months after filing with no response. The bug may be valid (GetObject can return null) but requires the exact parameters to confirm and fix. |
| Suggested repro platform | linux |

### Missing Info

- Exact parameters passed to CreateLinearGradient (colors array content, start/end points, tile mode)
- Platform and operating system
- Whether the issue reproduces on SkiaSharp 2.88.x

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/bug, area/SkiaSharp, tenet/reliability labels | labels=type/bug, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Follow up requesting code snippet and platform, noting the null return is technically possible for degenerate inputs | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! To help us investigate, could you share the exact code you're using to call `SKShader.CreateLinearGradient`? In particular:

- The `colors` array contents (how many colors, their values)
- The `start` and `end` point values
- The `colorPos` array (if used)
- Your platform / OS

Looking at the code, `CreateLinearGradient` can return `null` when the underlying native Skia function returns a null pointer — this can happen for edge-case inputs such as an empty `colors` array. A minimal repro snippet would let us confirm the trigger and determine whether this needs a fix (validation guard) or a documentation correction.

As a temporary workaround, you can check for `null` and fall back to a solid color:
```csharp
var shader = SKShader.CreateLinearGradient(start, end, colors, mode);
if (shader == null)
    shader = SKShader.CreateColor(colors.Length > 0 ? colors[0] : SKColors.Transparent);
```
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1953,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T01:09:00Z"
  },
  "summary": "Reporter claims SKShader.CreateLinearGradient returns null for certain parameters, contradicting the XML documentation claim that the function never returns null.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "errorMessage": "SKShader.CreateLinearGradient returns null",
      "reproQuality": "none",
      "targetFrameworks": [
        "netstandard"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call SKShader.CreateLinearGradient with certain parameters (screenshot only, no code provided)",
        "Observe that the return value is null"
      ],
      "environmentDetails": "SkiaSharp 2.80.3, IDE: Rider (platform unspecified)",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/29846655/153790590-2b53ff62-4e5d-4abe-8bc1-67f121ea809b.png",
          "description": "Screenshot of parameters passed to CreateLinearGradient (exact values not readable without image)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The SKShader.CreateLinearGradient binding has not substantively changed since 2.80.3; GetObject still returns null for a zero native handle."
    }
  },
  "analysis": {
    "summary": "The SKShader.CreateLinearGradient implementation passes the native function result directly through GetObject, which returns null when the native sk_shader_new_linear_gradient returns a null/zero pointer. This can occur for degenerate inputs (e.g., empty colors array with count=0 or NaN coordinates). The XML documentation incorrectly states 'This function never returns null', creating a discrepancy between the documented contract and actual behavior. Without a code snippet it is impossible to confirm the exact trigger, but the code path that can yield null is confirmed.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKShader.cs",
        "lines": "125-137",
        "finding": "CreateLinearGradient validates null/length mismatch but does NOT guard against colors.Length == 0. It then calls GetObject(SkiaApi.sk_shader_new_linear_gradient(...)) — GetObject returns null when the native handle is IntPtr.Zero.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "100-107",
        "finding": "GetOrAddObject returns null explicitly when handle == IntPtr.Zero, so any native API returning nullptr will produce null in C#.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "SKShader.CreateLinearGradient returns null at these parameters",
        "source": "issue title and body",
        "interpretation": "Reporter observes null return from a function the docs say cannot be null."
      },
      {
        "text": "Returns a new , or an empty shader on error. This function never returns null.",
        "source": "XML documentation quoted in issue",
        "interpretation": "Documentation claims cannot return null, but GetObject(IntPtr.Zero) returns null if the native Skia function fails for degenerate inputs."
      },
      {
        "text": "Could you attach a code snippet or repro sample?",
        "source": "maintainer comment (#1129051110)",
        "interpretation": "Maintainer already identified that more details are needed; no response from reporter."
      }
    ],
    "rationale": "Classified as type/bug because the documented contract ('never returns null') conflicts with the observed behavior and the code shows a clear path to null return via GetObject(IntPtr.Zero). Area is area/SkiaSharp since the issue is in the core shader binding. Severity is medium: it only manifests for edge-case parameter values and doesn't crash the process, but violates the API contract. Cannot reproduce without a code snippet — suggestedAction is needs-info since the maintainer already requested a snippet three months prior with no response.",
    "nextQuestions": [
      "What exact parameter values were passed (start, end, colors array, colorPos, mode)?",
      "Was colors an empty array (length 0)?",
      "What platform/OS was the reporter using?",
      "Does the issue still reproduce on 2.88.x?"
    ],
    "workarounds": [
      "Always check the return value of CreateLinearGradient for null and fall back to SKShader.CreateColor() or SKShader.CreateEmpty() to avoid NullReferenceException",
      "Ensure colors array has at least 2 elements; Skia requires count >= 1 but gradients are most useful with >= 2 colors",
      "Use SKShader.CreateColor(colors[0]) as a fallback if only one color is needed"
    ],
    "resolution": {
      "hypothesis": "Native sk_shader_new_linear_gradient returns nullptr when called with invalid inputs (likely colors.Length == 0 or degenerate coordinates), and the C# wrapper passes that null through rather than throwing or returning an empty shader as the docs promise.",
      "proposals": [
        {
          "title": "Request minimal repro from reporter",
          "description": "Ask reporter to provide the exact parameter values or a minimal code snippet showing the null return. This is needed before any code change can be made.",
          "category": "investigation",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add input validation for colors.Length == 0",
          "description": "Add a check for colors.Length == 0 and throw ArgumentException, matching the behavior documented. This prevents passing count=0 to the native function which may return null.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Update XML documentation to reflect nullable return",
          "description": "Remove or correct the 'never returns null' claim in the XML docs to accurately reflect that null may be returned for invalid parameters.",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request minimal repro from reporter",
      "recommendedReason": "Cannot confirm the exact trigger without a code snippet. The maintainer already asked once; a follow-up with guidance on what to include is the right next step."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.88,
      "reason": "Maintainer already requested a code snippet 3 months after filing with no response. The bug may be valid (GetObject can return null) but requires the exact parameters to confirm and fix.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Exact parameters passed to CreateLinearGradient (colors array content, start/end points, tile mode)",
      "Platform and operating system",
      "Whether the issue reproduces on SkiaSharp 2.88.x"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Follow up requesting code snippet and platform, noting the null return is technically possible for degenerate inputs",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the report! To help us investigate, could you share the exact code you're using to call `SKShader.CreateLinearGradient`? In particular:\n\n- The `colors` array contents (how many colors, their values)\n- The `start` and `end` point values\n- The `colorPos` array (if used)\n- Your platform / OS\n\nLooking at the code, `CreateLinearGradient` can return `null` when the underlying native Skia function returns a null pointer — this can happen for edge-case inputs such as an empty `colors` array. A minimal repro snippet would let us confirm the trigger and determine whether this needs a fix (validation guard) or a documentation correction.\n\nAs a temporary workaround, you can check for `null` and fall back to a solid color:\n```csharp\nvar shader = SKShader.CreateLinearGradient(start, end, colors, mode);\nif (shader == null)\n    shader = SKShader.CreateColor(colors.Length > 0 ? colors[0] : SKColors.Transparent);\n```"
      }
    ]
  }
}
```

</details>
