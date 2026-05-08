# Issue Triage Report — #1848

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-06T22:25:00Z |
| Type | type/question (0.95 (95%)) |
| Area | area/Docs (0.80 (80%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** Reporter asks whether SkiaSharp's interop type mappings (e.g., C void* to C# nint or JavaScript) are documented.

**Analysis:** The reporter is asking whether the data type mappings used in SkiaSharp's interop layer (e.g., C void* → C# nint, JS nint) are publicly documented. The question is brief and context-free, referencing a standup demo. The documentation/dev/architecture.md file does contain a type mapping table covering C++ ↔ C API ↔ C# P/Invoke ↔ C# Wrapper, but it is internal developer documentation and may not be easily discoverable. No specific version or repro is involved.

**Recommendations:** **needs-info** — The issue is a brief, vague question referencing an unspecified standup demo with no version, platform, or specific scenario context. Clarification is needed before a complete answer can be provided.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Docs |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

**Environment:** No version or platform specified. Issue filed 2021-10-29.

## Analysis

### Technical Summary

The reporter is asking whether the data type mappings used in SkiaSharp's interop layer (e.g., C void* → C# nint, JS nint) are publicly documented. The question is brief and context-free, referencing a standup demo. The documentation/dev/architecture.md file does contain a type mapping table covering C++ ↔ C API ↔ C# P/Invoke ↔ C# Wrapper, but it is internal developer documentation and may not be easily discoverable. No specific version or repro is involved.

### Rationale

This is a documentation question, not a bug. The reporter references a standup demo and asks about documentation availability. The architecture.md already has a type-mapping table but it is internal developer docs. The question is sufficiently vague (no version, no platform, no specific scenario) to warrant a needs-info response asking for more context, but the answer can be partially given by pointing to existing docs.

### Key Signals

- "With most interop approaches, usually the rub is in the data type mappings. For instance, in the standup demo this week, C void* to CSharp and/or JS nint." — **issue body** (Reporter is referencing a .NET Community Standup demo that featured SkiaSharp interop; they want to know if there's documentation on how C types map to C# and JS types.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `documentation/dev/architecture.md` | 47-73 | direct | File contains a 'Type Mappings' table showing C++ → C API → C# P/Invoke → C# Wrapper mappings (e.g., SkCanvas* → sk_canvas_t* → IntPtr → SKCanvas). Also documents type conversion macros (AsCanvas, ToCanvas, etc.). This is the closest thing to interop type documentation in the repo. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | related | Generated P/Invoke declarations show actual C# interop signatures, using IntPtr for opaque handles and blittable structs (SKRect, etc.) for value types. void* is surfaced as void* in unsafe contexts. |

### Workarounds

- Refer to documentation/dev/architecture.md which contains a type mapping table for C++ ↔ C API ↔ C# P/Invoke ↔ C# Wrapper.
- The generated SkiaApi.generated.cs contains all actual P/Invoke signatures showing how each native type maps to C#.

### Next Questions

- What specific interop scenario is the reporter working on? (C# P/Invoke, JavaScript interop, WASM, etc.)
- Is this about consuming SkiaSharp from JavaScript (Blazor WASM) or about extending SkiaSharp with custom C++ code?
- Which standup demo is being referenced?

### Resolution Proposals

1. **Point to existing architecture documentation** — workaround, cost/xs, validated=untested
   - Respond with a pointer to documentation/dev/architecture.md which contains the type mapping table, and to SkiaApi.generated.cs for actual P/Invoke signatures.
2. **Request clarification on specific interop scenario** — investigation, cost/xs, validated=untested
   - Ask the reporter what specific interop context they are working in (C# P/Invoke, WASM/JS interop, custom C++ extensions) since the question is too vague to give a precise answer.

**Recommended proposal:** Request clarification on specific interop scenario

**Why:** The question is too vague to give a complete answer. Asking for the specific scenario will allow a targeted, useful response.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | The issue is a brief, vague question referencing an unspecified standup demo with no version, platform, or specific scenario context. Clarification is needed before a complete answer can be provided. |
| Suggested repro platform | linux |

### Missing Info

- Which specific interop scenario is the reporter working on? (C# P/Invoke, Blazor/WASM JavaScript interop, custom native extensions, etc.)
- Which standup demo is being referenced?
- What SkiaSharp version and target platform are they using?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply type/question, area/Docs, tenet/compatibility labels | labels=type/question, area/Docs, tenet/compatibility |
| add-comment | medium | 0.80 (80%) | Ask for clarification on specific interop scenario and point to existing docs | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question! SkiaSharp does have some interop documentation in [`documentation/dev/architecture.md`](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/architecture.md), which includes a type mapping table showing how C++ types map through the C API layer to C# P/Invoke types and the high-level C# wrapper types.

Could you provide a bit more context so we can give a more targeted answer?

1. **Which interop scenario are you working on?** For example:
   - C# consuming SkiaSharp via P/Invoke directly?
   - Blazor/WASM JavaScript interop (calling SkiaSharp from JS)?
   - Writing custom C/C++ extensions that interact with SkiaSharp's native layer?
2. **Which standup demo are you referring to?** A link would help us understand the specific use case.
3. **Which SkiaSharp version and target platform are you using?**

With that context we can point you to the right documentation or create new documentation if it's missing.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1848,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-06T22:25:00Z"
  },
  "summary": "Reporter asks whether SkiaSharp's interop type mappings (e.g., C void* to C# nint or JavaScript) are documented.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.95
    },
    "area": {
      "value": "area/Docs",
      "confidence": 0.8
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "No version or platform specified. Issue filed 2021-10-29."
    }
  },
  "analysis": {
    "summary": "The reporter is asking whether the data type mappings used in SkiaSharp's interop layer (e.g., C void* → C# nint, JS nint) are publicly documented. The question is brief and context-free, referencing a standup demo. The documentation/dev/architecture.md file does contain a type mapping table covering C++ ↔ C API ↔ C# P/Invoke ↔ C# Wrapper, but it is internal developer documentation and may not be easily discoverable. No specific version or repro is involved.",
    "codeInvestigation": [
      {
        "file": "documentation/dev/architecture.md",
        "finding": "File contains a 'Type Mappings' table showing C++ → C API → C# P/Invoke → C# Wrapper mappings (e.g., SkCanvas* → sk_canvas_t* → IntPtr → SKCanvas). Also documents type conversion macros (AsCanvas, ToCanvas, etc.). This is the closest thing to interop type documentation in the repo.",
        "relevance": "direct",
        "lines": "47-73"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "Generated P/Invoke declarations show actual C# interop signatures, using IntPtr for opaque handles and blittable structs (SKRect, etc.) for value types. void* is surfaced as void* in unsafe contexts.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "With most interop approaches, usually the rub is in the data type mappings. For instance, in the standup demo this week, C void* to CSharp and/or JS nint.",
        "source": "issue body",
        "interpretation": "Reporter is referencing a .NET Community Standup demo that featured SkiaSharp interop; they want to know if there's documentation on how C types map to C# and JS types."
      }
    ],
    "rationale": "This is a documentation question, not a bug. The reporter references a standup demo and asks about documentation availability. The architecture.md already has a type-mapping table but it is internal developer docs. The question is sufficiently vague (no version, no platform, no specific scenario) to warrant a needs-info response asking for more context, but the answer can be partially given by pointing to existing docs.",
    "workarounds": [
      "Refer to documentation/dev/architecture.md which contains a type mapping table for C++ ↔ C API ↔ C# P/Invoke ↔ C# Wrapper.",
      "The generated SkiaApi.generated.cs contains all actual P/Invoke signatures showing how each native type maps to C#."
    ],
    "nextQuestions": [
      "What specific interop scenario is the reporter working on? (C# P/Invoke, JavaScript interop, WASM, etc.)",
      "Is this about consuming SkiaSharp from JavaScript (Blazor WASM) or about extending SkiaSharp with custom C++ code?",
      "Which standup demo is being referenced?"
    ],
    "resolution": {
      "proposals": [
        {
          "title": "Point to existing architecture documentation",
          "category": "workaround",
          "description": "Respond with a pointer to documentation/dev/architecture.md which contains the type mapping table, and to SkiaApi.generated.cs for actual P/Invoke signatures.",
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Request clarification on specific interop scenario",
          "category": "investigation",
          "description": "Ask the reporter what specific interop context they are working in (C# P/Invoke, WASM/JS interop, custom C++ extensions) since the question is too vague to give a precise answer.",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Request clarification on specific interop scenario",
      "recommendedReason": "The question is too vague to give a complete answer. Asking for the specific scenario will allow a targeted, useful response."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "The issue is a brief, vague question referencing an unspecified standup demo with no version, platform, or specific scenario context. Clarification is needed before a complete answer can be provided.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Which specific interop scenario is the reporter working on? (C# P/Invoke, Blazor/WASM JavaScript interop, custom native extensions, etc.)",
      "Which standup demo is being referenced?",
      "What SkiaSharp version and target platform are they using?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/question, area/Docs, tenet/compatibility labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/Docs",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask for clarification on specific interop scenario and point to existing docs",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the question! SkiaSharp does have some interop documentation in [`documentation/dev/architecture.md`](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/architecture.md), which includes a type mapping table showing how C++ types map through the C API layer to C# P/Invoke types and the high-level C# wrapper types.\n\nCould you provide a bit more context so we can give a more targeted answer?\n\n1. **Which interop scenario are you working on?** For example:\n   - C# consuming SkiaSharp via P/Invoke directly?\n   - Blazor/WASM JavaScript interop (calling SkiaSharp from JS)?\n   - Writing custom C/C++ extensions that interact with SkiaSharp's native layer?\n2. **Which standup demo are you referring to?** A link would help us understand the specific use case.\n3. **Which SkiaSharp version and target platform are you using?**\n\nWith that context we can point you to the right documentation or create new documentation if it's missing."
      }
    ]
  }
}
```

</details>
