# Issue Triage Report — #1199

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T19:24:35Z |
| Type | type/enhancement (0.88 (88%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** Performance enhancement request to optimize SKTypeface.FamilyName by avoiding an intermediate SKString allocation and two extra native round-trips when retrieving the family name string.

**Analysis:** SKTypeface.FamilyName currently calls sk_typeface_get_family_name to get an sk_string_t handle, wraps it in a managed SKString object, then calls back to native twice (sk_string_get_c_str + sk_string_get_size) to marshal the UTF-8 content into a managed string. The reporter suggests having the native side return a char* directly to avoid the intermediate allocation and extra native calls. The same inefficiency exists in PostScriptName.

**Recommendations:** **keep-open** — Valid performance enhancement acknowledged by maintainer. The fix is clear but requires C API changes for the full optimization. Worth tracking for a future performance pass.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Filed 2020-03-29, maintainer acknowledged as valid

**Code snippets:**

```csharp
public string FamilyName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));
```

## Analysis

### Technical Summary

SKTypeface.FamilyName currently calls sk_typeface_get_family_name to get an sk_string_t handle, wraps it in a managed SKString object, then calls back to native twice (sk_string_get_c_str + sk_string_get_size) to marshal the UTF-8 content into a managed string. The reporter suggests having the native side return a char* directly to avoid the intermediate allocation and extra native calls. The same inefficiency exists in PostScriptName.

### Rationale

The title says BUG but the description is a performance optimization suggestion. The current behavior is functionally correct; the issue is purely about efficiency. The maintainer (mattleibow) confirmed it as a smart improvement. Classified as type/enhancement because it improves existing working functionality. No platform specificity — this affects all targets equally.

### Key Signals

- "Currently SKTypeface.FamilyName is retrieving a SKString from the native side and is then always cast to a string that calls back to the native side to retrieve a UTF8 encoded string" — **issue body** (Redundant native calls: one for the SKString allocation, then two more (get_c_str + get_size) to extract the string content.)
- "Thanks for this issue. Actually pretty smart thing to do." — **comment by mattleibow** (Maintainer confirmation that the optimization is valid and desirable.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKTypeface.cs` | 103 | direct | FamilyName property still uses the old pattern: (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle)) — allocates an SKString object, then calls back to native twice via sk_string_get_c_str and sk_string_get_size before marshaling to a managed string. Issue is still present in current code. |
| `binding/SkiaSharp/SKTypeface.cs` | 123 | related | PostScriptName has the same inefficiency: (string)SKString.GetObject(SkiaApi.sk_typeface_get_post_script_name(Handle)). Fixing FamilyName should be paired with fixing PostScriptName. |
| `binding/SkiaSharp/SKString.cs` | 48-53 | direct | SKString.ToString() calls sk_string_get_c_str(Handle) and sk_string_get_size(Handle) — two extra P/Invoke calls per string retrieval. Replacing with a direct buffer-based C API would eliminate these plus the intermediate object allocation. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | context | sk_typeface_get_family_name returns sk_string_t (an owned native heap allocation). A more efficient alternative would be a new C API that writes into a caller-supplied buffer or returns the internal const char* pointer directly, avoiding any heap allocation. |

### Resolution Proposals

**Hypothesis:** Add a new C API function that writes the family name into a caller-supplied UTF-8 buffer (avoiding the sk_string_t allocation), then update the C# binding to use a stack-allocated or pooled buffer path.

1. **Use SKString with proper disposal (C#-only, minimal change)** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - Wrap the existing call in a using block so the native SKString is disposed immediately after string extraction. Reduces GC pressure but does not eliminate the extra native calls.
2. **Add buffer-based C API + update binding (full fix)** — fix, confidence 0.72 (72%), cost/s, validated=untested
   - Add a new C API sk_typeface_get_family_name_buffer(typeface, char* buf, size_t bufLen) or use sk_string_get_c_str on an output-parameter sk_string to allow zero-allocation retrieval. Update SkiaApi binding and FamilyName / PostScriptName to use it.

**Recommended proposal:** Use SKString with proper disposal (C#-only, minimal change)

**Why:** The C#-only fix can be done without C API changes and is ABI-safe. It reduces GC pressure immediately. The full buffer-based fix requires C API changes and native rebuilds, which is a larger scope and should be done separately.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid performance enhancement acknowledged by maintainer. The fix is clear but requires C API changes for the full optimization. Worth tracking for a future performance pass. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply enhancement, core area, and performance tenet labels | labels=type/enhancement, area/SkiaSharp, tenet/performance |

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1199,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T19:24:35Z"
  },
  "summary": "Performance enhancement request to optimize SKTypeface.FamilyName by avoiding an intermediate SKString allocation and two extra native round-trips when retrieving the family name string.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "codeSnippets": [
        "public string FamilyName => (string)SKString.GetObject (SkiaApi.sk_typeface_get_family_name (Handle));"
      ],
      "environmentDetails": "Filed 2020-03-29, maintainer acknowledged as valid"
    }
  },
  "analysis": {
    "summary": "SKTypeface.FamilyName currently calls sk_typeface_get_family_name to get an sk_string_t handle, wraps it in a managed SKString object, then calls back to native twice (sk_string_get_c_str + sk_string_get_size) to marshal the UTF-8 content into a managed string. The reporter suggests having the native side return a char* directly to avoid the intermediate allocation and extra native calls. The same inefficiency exists in PostScriptName.",
    "rationale": "The title says BUG but the description is a performance optimization suggestion. The current behavior is functionally correct; the issue is purely about efficiency. The maintainer (mattleibow) confirmed it as a smart improvement. Classified as type/enhancement because it improves existing working functionality. No platform specificity — this affects all targets equally.",
    "keySignals": [
      {
        "text": "Currently SKTypeface.FamilyName is retrieving a SKString from the native side and is then always cast to a string that calls back to the native side to retrieve a UTF8 encoded string",
        "source": "issue body",
        "interpretation": "Redundant native calls: one for the SKString allocation, then two more (get_c_str + get_size) to extract the string content."
      },
      {
        "text": "Thanks for this issue. Actually pretty smart thing to do.",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmation that the optimization is valid and desirable."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "103",
        "finding": "FamilyName property still uses the old pattern: (string)SKString.GetObject(SkiaApi.sk_typeface_get_family_name(Handle)) — allocates an SKString object, then calls back to native twice via sk_string_get_c_str and sk_string_get_size before marshaling to a managed string. Issue is still present in current code.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "123",
        "finding": "PostScriptName has the same inefficiency: (string)SKString.GetObject(SkiaApi.sk_typeface_get_post_script_name(Handle)). Fixing FamilyName should be paired with fixing PostScriptName.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKString.cs",
        "lines": "48-53",
        "finding": "SKString.ToString() calls sk_string_get_c_str(Handle) and sk_string_get_size(Handle) — two extra P/Invoke calls per string retrieval. Replacing with a direct buffer-based C API would eliminate these plus the intermediate object allocation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_typeface_get_family_name returns sk_string_t (an owned native heap allocation). A more efficient alternative would be a new C API that writes into a caller-supplied buffer or returns the internal const char* pointer directly, avoiding any heap allocation.",
        "relevance": "context"
      }
    ],
    "resolution": {
      "hypothesis": "Add a new C API function that writes the family name into a caller-supplied UTF-8 buffer (avoiding the sk_string_t allocation), then update the C# binding to use a stack-allocated or pooled buffer path.",
      "proposals": [
        {
          "title": "Use SKString with proper disposal (C#-only, minimal change)",
          "description": "Wrap the existing call in a using block so the native SKString is disposed immediately after string extraction. Reduces GC pressure but does not eliminate the extra native calls.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Add buffer-based C API + update binding (full fix)",
          "description": "Add a new C API sk_typeface_get_family_name_buffer(typeface, char* buf, size_t bufLen) or use sk_string_get_c_str on an output-parameter sk_string to allow zero-allocation retrieval. Update SkiaApi binding and FamilyName / PostScriptName to use it.",
          "category": "fix",
          "confidence": 0.72,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use SKString with proper disposal (C#-only, minimal change)",
      "recommendedReason": "The C#-only fix can be done without C API changes and is ABI-safe. It reduces GC pressure immediately. The full buffer-based fix requires C API changes and native rebuilds, which is a larger scope and should be done separately."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid performance enhancement acknowledged by maintainer. The fix is clear but requires C API changes for the full optimization. Worth tracking for a future performance pass.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, core area, and performance tenet labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      }
    ]
  }
}
```

</details>
