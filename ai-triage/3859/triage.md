# Issue Triage Report — #3859

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-08T19:30:00Z |
| Type | type/enhancement (0.92 (92%)) |
| Area | area/SkiaSharp (0.97 (97%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** Performance enhancement request to reduce unnecessary allocations in SKRuntimeEffectUniforms.ToData() and SKRuntimeEffectChildren.ToArray() which are called on every invocation of ToShader, ToColorFilter, ToBlender, and ToImageFilter in hot rendering paths.

**Analysis:** Every call to ToShader/ToColorFilter/ToBlender/ToImageFilter incurs two unnecessary allocations: SKData.CreateCopy in ToData() copies the entire uniform buffer, and children.ToArray() allocates a new SKObject[] copy. The fix requires investigating whether the C++ native side copies or takes ownership of the data, then exposing an internal path that skips the redundant copies.

**Recommendations:** **needs-investigation** — The issue is well-specified by a collaborator with clear code locations and investigation questions. The key open question (does native makeShader copy uniforms?) must be answered before implementing the fix. This is ready for the issue-repro / issue-fix pipeline once confirmed.

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

**Related issues:** #3680

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3680 — Parent tracking issue: [v4] Evaluate and wrap new Skia APIs

## Analysis

### Technical Summary

Every call to ToShader/ToColorFilter/ToBlender/ToImageFilter incurs two unnecessary allocations: SKData.CreateCopy in ToData() copies the entire uniform buffer, and children.ToArray() allocates a new SKObject[] copy. The fix requires investigating whether the C++ native side copies or takes ownership of the data, then exposing an internal path that skips the redundant copies.

### Rationale

This is a type/enhancement because it improves the performance of existing functionality without adding new APIs. The reporter (a collaborator) has clearly identified the allocations, their locations, and proposed investigation paths. The tenet/performance label applies because the change targets hot rendering paths. No platform specificity — this is pure managed C# wrapper code.

### Key Signals

- "uniforms.ToData() — calls SKData.CreateCopy which copies the entire uniform buffer into a new SKData" — **issue body** (Confirmed by code inspection: ToData() at line 317 explicitly calls SKData.CreateCopy. This is an avoidable allocation if the native makeShader already copies the data.)
- "children.ToArray() — allocates a new SKObject[] copy" — **issue body** (Confirmed: line 405 calls children.ToArray() which copies the internal array. Since RentHandlesArray accepts an array, the internal field could be passed directly.)
- "Part of #3680" — **issue body** (This is a sub-task of the v4 API/performance tracking issue, confirming it's planned work for the v4 release.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKRuntimeEffect.cs` | 312-318 | direct | SKRuntimeEffectUniforms.ToData() calls SKData.CreateCopy on every invocation, allocating a new native SKData buffer containing the entire uniform buffer. The internal 'data' field already holds a valid SKData that could be passed directly if the native side copies it. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | 404-405 | direct | SKRuntimeEffectChildren.ToArray() calls children.ToArray() which allocates a new SKObject[] on every call. The internal 'children' field is an SKObject[] that could be passed directly to RentHandlesArray without copying. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | 123-143 | direct | ToShader public overloads call uniforms.ToData() and children.ToArray(), then pass to private ToShader(SKData, SKObject[], SKMatrix*). The private method already uses Utils.RentHandlesArray which is allocation-friendly. If ToData/ToArray allocations are eliminated, the hot path would be near-zero-allocation. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | 150-191 | related | ToColorFilter and ToBlender have the same pattern as ToShader — same allocations via ToData()/ToArray() on every call. No ImageFilter overload exists here yet, but the same optimization applies. |

### Next Questions

- Does sk_runtimeeffect_make_shader / makeShader copy the uniform data or take ownership? If it copies, ToData can return the internal data directly (AsData pattern) without calling CreateCopy.
- Can RentHandlesArray accept the internal SKObject[] directly from SKRuntimeEffectChildren, or does it need to be a copy?
- Should an internal AsData() method be added to SKRuntimeEffectUniforms that returns the backing SKData without copying, for use only within the same call frame?

### Resolution Proposals

**Hypothesis:** The native Skia side copies uniform data during makeShader/makeColorFilter/makeBlender calls, so passing the backing SKData directly (via an internal AsData() accessor) would be safe and eliminate the copy. Similarly, the internal children[] can be passed directly to RentHandlesArray.

1. **Add internal AsData() to SKRuntimeEffectUniforms** — fix, confidence 0.80 (80%), cost/s, validated=untested
   - Add an internal AsData() method that returns the backing data field directly without copying. Update all To* methods to call AsData() instead of ToData() internally. Preserve ToData() as public API for callers who need an owned copy.
2. **Expose internal children array to RentHandlesArray** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Change SKRuntimeEffectChildren.ToArray() to an internal accessor that returns the backing array directly. Update all To* methods to use this internal accessor. Preserve ToArray() as public API.

**Recommended proposal:** Add internal AsData() to SKRuntimeEffectUniforms

**Why:** Eliminating the CreateCopy allocation is the larger win since uniforms can be large buffers. Combining both proposals gives a near-zero-allocation hot path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | The issue is well-specified by a collaborator with clear code locations and investigation questions. The key open question (does native makeShader copy uniforms?) must be answered before implementing the fix. This is ready for the issue-repro / issue-fix pipeline once confirmed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply enhancement, SkiaSharp core, and performance labels | labels=type/enhancement, area/SkiaSharp, tenet/performance |
| add-comment | medium | 0.88 (88%) | Acknowledge and outline investigation plan | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for filing this with such clear detail! The allocations in `ToData()` and `ToArray()` are confirmed by code inspection.

**Investigation plan:**
1. Verify whether `sk_runtimeeffect_make_shader` (and the other `make*` functions) copy the uniform data or take ownership — if they copy, we can safely pass the backing `SKData` directly via an internal `AsData()` method.
2. Confirm `RentHandlesArray` can safely accept the internal `SKObject[]` directly without a defensive copy.
3. Implement the zero-copy path internally while preserving the existing public `ToData()` and `ToArray()` APIs for callers who need an owned copy.

This is a good candidate for the `issue-fix` skill once the native ownership semantics are confirmed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3859,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-08T19:30:00Z"
  },
  "summary": "Performance enhancement request to reduce unnecessary allocations in SKRuntimeEffectUniforms.ToData() and SKRuntimeEffectChildren.ToArray() which are called on every invocation of ToShader, ToColorFilter, ToBlender, and ToImageFilter in hot rendering paths.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.97
    },
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        3680
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3680",
          "description": "Parent tracking issue: [v4] Evaluate and wrap new Skia APIs"
        }
      ]
    }
  },
  "analysis": {
    "summary": "Every call to ToShader/ToColorFilter/ToBlender/ToImageFilter incurs two unnecessary allocations: SKData.CreateCopy in ToData() copies the entire uniform buffer, and children.ToArray() allocates a new SKObject[] copy. The fix requires investigating whether the C++ native side copies or takes ownership of the data, then exposing an internal path that skips the redundant copies.",
    "rationale": "This is a type/enhancement because it improves the performance of existing functionality without adding new APIs. The reporter (a collaborator) has clearly identified the allocations, their locations, and proposed investigation paths. The tenet/performance label applies because the change targets hot rendering paths. No platform specificity — this is pure managed C# wrapper code.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "lines": "312-318",
        "finding": "SKRuntimeEffectUniforms.ToData() calls SKData.CreateCopy on every invocation, allocating a new native SKData buffer containing the entire uniform buffer. The internal 'data' field already holds a valid SKData that could be passed directly if the native side copies it.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "lines": "404-405",
        "finding": "SKRuntimeEffectChildren.ToArray() calls children.ToArray() which allocates a new SKObject[] on every call. The internal 'children' field is an SKObject[] that could be passed directly to RentHandlesArray without copying.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "lines": "123-143",
        "finding": "ToShader public overloads call uniforms.ToData() and children.ToArray(), then pass to private ToShader(SKData, SKObject[], SKMatrix*). The private method already uses Utils.RentHandlesArray which is allocation-friendly. If ToData/ToArray allocations are eliminated, the hot path would be near-zero-allocation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "lines": "150-191",
        "finding": "ToColorFilter and ToBlender have the same pattern as ToShader — same allocations via ToData()/ToArray() on every call. No ImageFilter overload exists here yet, but the same optimization applies.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "uniforms.ToData() — calls SKData.CreateCopy which copies the entire uniform buffer into a new SKData",
        "source": "issue body",
        "interpretation": "Confirmed by code inspection: ToData() at line 317 explicitly calls SKData.CreateCopy. This is an avoidable allocation if the native makeShader already copies the data."
      },
      {
        "text": "children.ToArray() — allocates a new SKObject[] copy",
        "source": "issue body",
        "interpretation": "Confirmed: line 405 calls children.ToArray() which copies the internal array. Since RentHandlesArray accepts an array, the internal field could be passed directly."
      },
      {
        "text": "Part of #3680",
        "source": "issue body",
        "interpretation": "This is a sub-task of the v4 API/performance tracking issue, confirming it's planned work for the v4 release."
      }
    ],
    "nextQuestions": [
      "Does sk_runtimeeffect_make_shader / makeShader copy the uniform data or take ownership? If it copies, ToData can return the internal data directly (AsData pattern) without calling CreateCopy.",
      "Can RentHandlesArray accept the internal SKObject[] directly from SKRuntimeEffectChildren, or does it need to be a copy?",
      "Should an internal AsData() method be added to SKRuntimeEffectUniforms that returns the backing SKData without copying, for use only within the same call frame?"
    ],
    "resolution": {
      "hypothesis": "The native Skia side copies uniform data during makeShader/makeColorFilter/makeBlender calls, so passing the backing SKData directly (via an internal AsData() accessor) would be safe and eliminate the copy. Similarly, the internal children[] can be passed directly to RentHandlesArray.",
      "proposals": [
        {
          "title": "Add internal AsData() to SKRuntimeEffectUniforms",
          "description": "Add an internal AsData() method that returns the backing data field directly without copying. Update all To* methods to call AsData() instead of ToData() internally. Preserve ToData() as public API for callers who need an owned copy.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Expose internal children array to RentHandlesArray",
          "description": "Change SKRuntimeEffectChildren.ToArray() to an internal accessor that returns the backing array directly. Update all To* methods to use this internal accessor. Preserve ToArray() as public API.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add internal AsData() to SKRuntimeEffectUniforms",
      "recommendedReason": "Eliminating the CreateCopy allocation is the larger win since uniforms can be large buffers. Combining both proposals gives a near-zero-allocation hot path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "The issue is well-specified by a collaborator with clear code locations and investigation questions. The key open question (does native makeShader copy uniforms?) must be answered before implementing the fix. This is ready for the issue-repro / issue-fix pipeline once confirmed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply enhancement, SkiaSharp core, and performance labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge and outline investigation plan",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for filing this with such clear detail! The allocations in `ToData()` and `ToArray()` are confirmed by code inspection.\n\n**Investigation plan:**\n1. Verify whether `sk_runtimeeffect_make_shader` (and the other `make*` functions) copy the uniform data or take ownership — if they copy, we can safely pass the backing `SKData` directly via an internal `AsData()` method.\n2. Confirm `RentHandlesArray` can safely accept the internal `SKObject[]` directly without a defensive copy.\n3. Implement the zero-copy path internally while preserving the existing public `ToData()` and `ToArray()` APIs for callers who need an owned copy.\n\nThis is a good candidate for the `issue-fix` skill once the native ownership semantics are confirmed."
      }
    ]
  }
}
```

</details>
