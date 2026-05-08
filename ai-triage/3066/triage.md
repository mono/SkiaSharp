# Issue Triage Report — #3066

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T16:02:53Z |
| Type | type/bug (0.72 (72%)) |
| Area | area/SkiaSharp.Views.Blazor (0.98 (98%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** SkiaSharp Blazor views construct DOM element identifiers using `ElementReference.Id` prefixed with `_bl_`, a property documented as not intended for user code and potentially unstable across Blazor versions.

**Analysis:** SkiaSharp Blazor views use ElementReference.Id prefixed with _bl_ to construct a CSS attribute selector for locating canvas elements in JavaScript interop. The Blazor ElementReference.Id property is documented as 'public to support JSON serialization and should not be used by user code.' This pattern appears in both SKHtmlCanvasInterop.cs (line 46) and SizeWatcherInterop.cs (line 43). A maintainer comment notes that ASP.NET Core's own JS runtime does the same thing, suggesting it is de-facto stable, but no official contract guarantees this will not change. The fix path is to ensure the ElementReference is always passed directly via JSObject interop (already supported on .NET 7+) rather than relying on the ID-based fallback.

**Recommendations:** **needs-investigation** — The issue is valid — SkiaSharp uses an undocumented Blazor internal. While currently functional and mirroring what ASP.NET Core does internally, it should be investigated to determine if direct ElementReference passing can eliminate the reliance on the undocumented ID scheme.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.Views.Blazor |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Components/Web.JS/src/Rendering/ElementReferenceCapture.ts#L10-L17 — ASP.NET Core itself uses _bl_-prefixed attributes in its rendering engine ElementReferenceCapture, suggesting de-facto stability.

**Code snippets:**

```csharp
htmlElementId = "_bl_" + element.Id; // SKHtmlCanvasInterop.cs:46
```

```csharp
htmlElementId = "_bl_" + element.Id; // SizeWatcherInterop.cs:43
```

```csharp
element = element || document.querySelector('[' + elementId + ']'); // SKHtmlCanvas.ts:48
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | other |
| Error message | — |
| Repro quality | none |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.118.0-preview.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The _bl_ prefix usage is still present in current source code in both SKHtmlCanvasInterop.cs and SizeWatcherInterop.cs. |

## Analysis

### Technical Summary

SkiaSharp Blazor views use ElementReference.Id prefixed with _bl_ to construct a CSS attribute selector for locating canvas elements in JavaScript interop. The Blazor ElementReference.Id property is documented as 'public to support JSON serialization and should not be used by user code.' This pattern appears in both SKHtmlCanvasInterop.cs (line 46) and SizeWatcherInterop.cs (line 43). A maintainer comment notes that ASP.NET Core's own JS runtime does the same thing, suggesting it is de-facto stable, but no official contract guarantees this will not change. The fix path is to ensure the ElementReference is always passed directly via JSObject interop (already supported on .NET 7+) rather than relying on the ID-based fallback.

### Rationale

Classified as type/bug with moderate confidence because the behavior is currently functional but relies on an undocumented implementation detail of Blazor. The risk is low since ASP.NET Core itself uses the same convention internally, but a future Blazor update could change the _bl_ scheme without a breaking-change notice. The area is clearly SkiaSharp.Views.Blazor and affects WASM-based Blazor deployments. Severity is low because there is no current failure. The recommended fix is to ensure direct ElementReference passing is always used, avoiding the ID-based fallback entirely on .NET 7+.

### Key Signals

- "The Id is unique at least within the scope of a given user/circuit. This property is public to support Json serialization and should not be used by user code." — **Blazor ElementReference XML documentation (referenced in issue body)** (Microsoft explicitly states this property is not for user code — using it risks breakage if Blazor changes its internal ID scheme.)
- "Probably not too crazy as we are doing the same thing as the SDK: https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Components/Web.JS/src/Rendering/ElementReferenceCapture.ts#L10-L17" — **Issue comment by mattleibow** (ASP.NET Core itself constructs _bl_-prefixed attributes in its rendering engine, making the pattern a de-facto stable convention even if undocumented.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SKHtmlCanvasInterop.cs` | 46 | direct | htmlElementId = "_bl_" + element.Id; — constructs the DOM attribute name using the undocumented _bl_ prefix plus the ElementReference.Id for later use as a CSS attribute selector in JS. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SizeWatcherInterop.cs` | 43 | direct | htmlElementId = "_bl_" + element.Id; — same pattern used in the size watcher interop for observing resize events on the canvas. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.ts` | 48 | direct | element = element || document.querySelector('[' + elementId + ']'); — JS falls back to a CSS attribute selector using the _bl_-prefixed ID when the element reference is not passed directly. |
| `source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SizeWatcher.ts` | — | related | SizeWatcher JavaScript also accepts elementId for observing, using the same _bl_-prefixed ID pattern as SKHtmlCanvas. |

### Next Questions

- Can the JS interop be updated to always use the passed ElementReference (JSObject on NET7+) and drop the document.querySelector fallback path?
- Is there a stable Blazor API to obtain a reliable user-safe element identifier from an ElementReference?
- Does Microsoft consider the _bl_ prefix a stable contract despite the disclaimer on ElementReference.Id?

### Resolution Proposals

**Hypothesis:** The _bl_-prefixed ID is used as a key in a JS Map and as a CSS attribute selector fallback. Since .NET 7+ already supports passing JSObject element references directly, the fix is to always pass the element reference and remove the ID-based fallback path.

1. **Use ElementReference directly instead of ID lookup** — fix, cost/m, validated=untested
   - On .NET 7+, the JSImport path already accepts a JSObject? element parameter. Ensure the ElementReference is always marshalled as JSObject and passed directly, eliminating the need to construct or use the _bl_-prefixed elementId for DOM lookup. The Map key in JS can be replaced with a WeakRef or WeakMap keyed on the element object itself.
2. **Keep current approach with explicit documentation** — workaround, cost/xs, validated=untested
   - Since ASP.NET Core itself uses the same _bl_ pattern, document the assumption explicitly in code comments with a link to the aspnetcore source, and add a test/check that fails fast if the ID format changes. This is a minimal-risk workaround until a cleaner solution is available.

**Recommended proposal:** Use ElementReference directly instead of ID lookup

**Why:** Removing the dependency on ElementReference.Id entirely is the correct long-term fix. The .NET 7+ JSImport path already passes the element reference directly; the fix focuses on ensuring the ID-based fallback is no longer needed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | The issue is valid — SkiaSharp uses an undocumented Blazor internal. While currently functional and mirroring what ASP.NET Core does internally, it should be investigated to determine if direct ElementReference passing can eliminate the reliance on the undocumented ID scheme. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility labels. | labels=area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.82 (82%) | Acknowledge the valid concern and outline the investigation path. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for flagging this!

You're right that `ElementReference.Id` is documented as not intended for user code. The current approach of constructing `_bl_` + `Id` as a CSS attribute selector works today because the Blazor rendering engine assigns that same attribute to DOM elements, and ASP.NET Core's own JS runtime uses the same convention. However, since Microsoft has not guaranteed this will remain stable, we should investigate whether we can remove the dependency entirely.

For the .NET 7+ `JSImport` path, the interop methods already accept a `JSObject?` element parameter — we should ensure the `ElementReference` is always marshalled and passed directly, making the `elementId` fallback path unnecessary. For older targets, a similar approach may be possible.

Tracking this for investigation.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3066,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T16:02:53Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "SkiaSharp Blazor views construct DOM element identifiers using `ElementReference.Id` prefixed with `_bl_`, a property documented as not intended for user code and potentially unstable across Blazor versions.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.72
    },
    "area": {
      "value": "area/SkiaSharp.Views.Blazor",
      "confidence": 0.98
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "other",
      "reproQuality": "none"
    },
    "reproEvidence": {
      "codeSnippets": [
        "htmlElementId = \"_bl_\" + element.Id; // SKHtmlCanvasInterop.cs:46",
        "htmlElementId = \"_bl_\" + element.Id; // SizeWatcherInterop.cs:43",
        "element = element || document.querySelector('[' + elementId + ']'); // SKHtmlCanvas.ts:48"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Components/Web.JS/src/Rendering/ElementReferenceCapture.ts#L10-L17",
          "description": "ASP.NET Core itself uses _bl_-prefixed attributes in its rendering engine ElementReferenceCapture, suggesting de-facto stability."
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.118.0-preview.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The _bl_ prefix usage is still present in current source code in both SKHtmlCanvasInterop.cs and SizeWatcherInterop.cs."
    }
  },
  "analysis": {
    "summary": "SkiaSharp Blazor views use ElementReference.Id prefixed with _bl_ to construct a CSS attribute selector for locating canvas elements in JavaScript interop. The Blazor ElementReference.Id property is documented as 'public to support JSON serialization and should not be used by user code.' This pattern appears in both SKHtmlCanvasInterop.cs (line 46) and SizeWatcherInterop.cs (line 43). A maintainer comment notes that ASP.NET Core's own JS runtime does the same thing, suggesting it is de-facto stable, but no official contract guarantees this will not change. The fix path is to ensure the ElementReference is always passed directly via JSObject interop (already supported on .NET 7+) rather than relying on the ID-based fallback.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SKHtmlCanvasInterop.cs",
        "lines": "46",
        "finding": "htmlElementId = \"_bl_\" + element.Id; — constructs the DOM attribute name using the undocumented _bl_ prefix plus the ElementReference.Id for later use as a CSS attribute selector in JS.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/Internal/SizeWatcherInterop.cs",
        "lines": "43",
        "finding": "htmlElementId = \"_bl_\" + element.Id; — same pattern used in the size watcher interop for observing resize events on the canvas.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SKHtmlCanvas.ts",
        "lines": "48",
        "finding": "element = element || document.querySelector('[' + elementId + ']'); — JS falls back to a CSS attribute selector using the _bl_-prefixed ID when the element reference is not passed directly.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.Blazor/wwwroot/SizeWatcher.ts",
        "finding": "SizeWatcher JavaScript also accepts elementId for observing, using the same _bl_-prefixed ID pattern as SKHtmlCanvas.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "The Id is unique at least within the scope of a given user/circuit. This property is public to support Json serialization and should not be used by user code.",
        "source": "Blazor ElementReference XML documentation (referenced in issue body)",
        "interpretation": "Microsoft explicitly states this property is not for user code — using it risks breakage if Blazor changes its internal ID scheme."
      },
      {
        "text": "Probably not too crazy as we are doing the same thing as the SDK: https://github.com/dotnet/aspnetcore/blob/v9.0.0/src/Components/Web.JS/src/Rendering/ElementReferenceCapture.ts#L10-L17",
        "source": "Issue comment by mattleibow",
        "interpretation": "ASP.NET Core itself constructs _bl_-prefixed attributes in its rendering engine, making the pattern a de-facto stable convention even if undocumented."
      }
    ],
    "rationale": "Classified as type/bug with moderate confidence because the behavior is currently functional but relies on an undocumented implementation detail of Blazor. The risk is low since ASP.NET Core itself uses the same convention internally, but a future Blazor update could change the _bl_ scheme without a breaking-change notice. The area is clearly SkiaSharp.Views.Blazor and affects WASM-based Blazor deployments. Severity is low because there is no current failure. The recommended fix is to ensure direct ElementReference passing is always used, avoiding the ID-based fallback entirely on .NET 7+.",
    "nextQuestions": [
      "Can the JS interop be updated to always use the passed ElementReference (JSObject on NET7+) and drop the document.querySelector fallback path?",
      "Is there a stable Blazor API to obtain a reliable user-safe element identifier from an ElementReference?",
      "Does Microsoft consider the _bl_ prefix a stable contract despite the disclaimer on ElementReference.Id?"
    ],
    "resolution": {
      "hypothesis": "The _bl_-prefixed ID is used as a key in a JS Map and as a CSS attribute selector fallback. Since .NET 7+ already supports passing JSObject element references directly, the fix is to always pass the element reference and remove the ID-based fallback path.",
      "proposals": [
        {
          "title": "Use ElementReference directly instead of ID lookup",
          "description": "On .NET 7+, the JSImport path already accepts a JSObject? element parameter. Ensure the ElementReference is always marshalled as JSObject and passed directly, eliminating the need to construct or use the _bl_-prefixed elementId for DOM lookup. The Map key in JS can be replaced with a WeakRef or WeakMap keyed on the element object itself.",
          "category": "fix",
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Keep current approach with explicit documentation",
          "description": "Since ASP.NET Core itself uses the same _bl_ pattern, document the assumption explicitly in code comments with a link to the aspnetcore source, and add a test/check that fails fast if the ID format changes. This is a minimal-risk workaround until a cleaner solution is available.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use ElementReference directly instead of ID lookup",
      "recommendedReason": "Removing the dependency on ElementReference.Id entirely is the correct long-term fix. The .NET 7+ JSImport path already passes the element reference directly; the fix focuses on ensuring the ID-based fallback is no longer needed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "The issue is valid — SkiaSharp uses an undocumented Blazor internal. While currently functional and mirroring what ASP.NET Core does internally, it should be investigated to determine if direct ElementReference passing can eliminate the reliance on the undocumented ID scheme.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply area/SkiaSharp.Views.Blazor, os/WASM, tenet/reliability, tenet/compatibility labels.",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "area/SkiaSharp.Views.Blazor",
          "os/WASM",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the valid concern and outline the investigation path.",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for flagging this!\n\nYou're right that `ElementReference.Id` is documented as not intended for user code. The current approach of constructing `_bl_` + `Id` as a CSS attribute selector works today because the Blazor rendering engine assigns that same attribute to DOM elements, and ASP.NET Core's own JS runtime uses the same convention. However, since Microsoft has not guaranteed this will remain stable, we should investigate whether we can remove the dependency entirely.\n\nFor the .NET 7+ `JSImport` path, the interop methods already accept a `JSObject?` element parameter — we should ensure the `ElementReference` is always marshalled and passed directly, making the `elementId` fallback path unnecessary. For older targets, a similar approach may be possible.\n\nTracking this for investigation."
      }
    ]
  }
}
```

</details>
