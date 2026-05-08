# Issue Triage Report — #2409

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T13:35:00Z |
| Type | type/feature-request (0.82 (82%)) |
| Area | area/libSkiaSharp.native (0.85 (85%)) |
| Suggested action | keep-open (0.78 (78%)) |

**Issue Summary:** WebAssembly build of SkiaSharp requires 'unsafe-eval' in the Content Security Policy (CSP) due to Emscripten-generated eval usage; reporter requests CSP-compatible WASM output and asks about .NET 7 JS import statement support.

**Analysis:** SkiaSharp's WebAssembly native library is compiled with Emscripten. Older Emscripten versions generate JavaScript glue code that uses eval(), requiring 'unsafe-eval' in the CSP. The reporter wants SkiaSharp's WASM build to support stricter CSP policies, possibly by adopting .NET 7 JS import interop which avoids eval. This is a build/packaging enhancement request for the native WASM artifacts.

**Recommendations:** **keep-open** — Valid feature request requiring native WASM build investigation. No quick fix available; needs design and upstream Emscripten/runtime coordination.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/libSkiaSharp.native |
| Platforms | os/WASM |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | partner/unoplatform |

## Evidence

### Reproduction

**Environment:** Uno Platform WebAssembly application. Version: latest (at time of filing, ~2023-03-07). TFM: WebAssembly.

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | latest |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The WASM native assets still build with Emscripten and the eval concern is tied to the Emscripten version and compilation flags, not SkiaSharp C# code. The feature for CSP-safe WASM output has not been explicitly added. |

## Analysis

### Technical Summary

SkiaSharp's WebAssembly native library is compiled with Emscripten. Older Emscripten versions generate JavaScript glue code that uses eval(), requiring 'unsafe-eval' in the CSP. The reporter wants SkiaSharp's WASM build to support stricter CSP policies, possibly by adopting .NET 7 JS import interop which avoids eval. This is a build/packaging enhancement request for the native WASM artifacts.

### Rationale

The issue is titled [BUG] but describes a feature/platform limitation: the WASM native binary uses Emscripten-generated JS that includes eval(), which blocks strict CSP policies. This is not a regression (N/A for 'last known good version') but a missing capability — SkiaSharp's WASM build has never supported strict CSP. The request for .NET 7 JS import statement support confirms this is a forward-looking feature request. Area is libSkiaSharp.native because the change must be in how the native WASM binary is compiled/linked. The partner tag is unoplatform because the reporter is using Uno Platform.

### Key Signals

- "the WebAssembly version of SkiaSharp makes use of eval" — **issue body** (Reporter identified that eval comes from the WASM/Emscripten build, not application code.)
- "Is there are roadmap to supporting .NET7 js import statements?" — **issue body** (Reporter is asking about a future feature (JS import interop), confirming this is a feature request not a regression.)
- "requiring the removal of unsafe-inline / unsafe-eval expressions from the Content-Security-Policy" — **issue body** (Security requirement from a client — this is a real-world enterprise constraint, not a minor request.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets` | — | direct | WASM targets configure native linking via Emscripten flags (-s USE_WEBGL2=1, OFFSCREEN_FRAMEBUFFER) and reference pre-compiled libSkiaSharp.a static libraries for net6, net7, net8, and net9+ runtimes. No explicit CSP-related flags or JS import settings found. |
| `native/wasm/build.cake` | — | related | The WASM native build pipeline exists in native/wasm/. The eval dependency is a characteristic of the Emscripten-generated JavaScript glue code, not SkiaSharp's own code. Upgrading Emscripten or changing its flags (e.g., -s MODULARIZE=1, -s EXPORT_ES6=1) could eliminate eval usage. |

### Next Questions

- Which Emscripten version is currently used to build the WASM libSkiaSharp.a?
- Has the .NET WASM runtime (net8/net9) addressed eval usage at the runtime level so only SkiaSharp's own JS glue needs updating?
- Does the Blazor WASM variant of SkiaSharp have the same CSP issue?

### Resolution Proposals

**Hypothesis:** Updating the Emscripten build flags for WASM to use ES6 modules (-s EXPORT_ES6=1, -s MODULARIZE=1) or a newer runtime that uses JS import instead of eval would eliminate the CSP requirement.

1. **Update Emscripten build flags for CSP compliance** — investigation, confidence 0.65 (65%), cost/l, validated=untested
   - Investigate updating the WASM native build in native/wasm/ to use Emscripten flags that avoid eval (e.g., MODULARIZE, EXPORT_ES6, or newer Emscripten with JS import support). This may also require updating the .targets integration.
2. **Track .NET WASM runtime eval elimination** — investigation, confidence 0.60 (60%), cost/xs, validated=untested
   - Monitor dotnet/runtime progress on eliminating eval from the WASM runtime itself. If the .NET runtime resolves this at the host level, SkiaSharp may benefit automatically.

**Recommended proposal:** Update Emscripten build flags for CSP compliance

**Why:** Direct fix targeting the root cause. The eval usage originates in compiled Emscripten JS glue, and modern Emscripten supports CSP-safe output modes.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.78 (78%) |
| Reason | Valid feature request requiring native WASM build investigation. No quick fix available; needs design and upstream Emscripten/runtime coordination. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Apply feature-request, native WASM, compatibility labels | labels=type/feature-request, area/libSkiaSharp.native, os/WASM, tenet/compatibility, partner/unoplatform |
| add-comment | medium | 0.78 (78%) | Acknowledge request, explain root cause, and note investigation needed | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for raising this! The `unsafe-eval` CSP requirement comes from Emscripten-generated JavaScript glue code used to load the native WebAssembly binary — it is not generated by SkiaSharp's own C# code.

Modern Emscripten versions support CSP-safe output via flags like `-s EXPORT_ES6=1` / `-s MODULARIZE=1`, and .NET 7+ introduced JS import interop (`[JSImport]`/`[JSExport]`) which avoids eval. We need to investigate updating the WASM native build pipeline in SkiaSharp to use these newer approaches.

This is a valid enhancement request. We'll track it here. In the meantime, as a short-term workaround, you may be able to configure your CSP to apply `unsafe-eval` only to the specific WASM worker scope rather than globally, reducing the security exposure.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2409,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T13:35:00Z"
  },
  "summary": "WebAssembly build of SkiaSharp requires 'unsafe-eval' in the Content Security Policy (CSP) due to Emscripten-generated eval usage; reporter requests CSP-compatible WASM output and asks about .NET 7 JS import statement support.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.82
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.85
    },
    "platforms": [
      "os/WASM"
    ],
    "tenets": [
      "tenet/compatibility"
    ],
    "partner": "partner/unoplatform"
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Uno Platform WebAssembly application. Version: latest (at time of filing, ~2023-03-07). TFM: WebAssembly.",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "latest"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The WASM native assets still build with Emscripten and the eval concern is tied to the Emscripten version and compilation flags, not SkiaSharp C# code. The feature for CSP-safe WASM output has not been explicitly added."
    }
  },
  "analysis": {
    "summary": "SkiaSharp's WebAssembly native library is compiled with Emscripten. Older Emscripten versions generate JavaScript glue code that uses eval(), requiring 'unsafe-eval' in the CSP. The reporter wants SkiaSharp's WASM build to support stricter CSP policies, possibly by adopting .NET 7 JS import interop which avoids eval. This is a build/packaging enhancement request for the native WASM artifacts.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.WebAssembly/buildTransitive/SkiaSharp.targets",
        "finding": "WASM targets configure native linking via Emscripten flags (-s USE_WEBGL2=1, OFFSCREEN_FRAMEBUFFER) and reference pre-compiled libSkiaSharp.a static libraries for net6, net7, net8, and net9+ runtimes. No explicit CSP-related flags or JS import settings found.",
        "relevance": "direct"
      },
      {
        "file": "native/wasm/build.cake",
        "finding": "The WASM native build pipeline exists in native/wasm/. The eval dependency is a characteristic of the Emscripten-generated JavaScript glue code, not SkiaSharp's own code. Upgrading Emscripten or changing its flags (e.g., -s MODULARIZE=1, -s EXPORT_ES6=1) could eliminate eval usage.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "the WebAssembly version of SkiaSharp makes use of eval",
        "source": "issue body",
        "interpretation": "Reporter identified that eval comes from the WASM/Emscripten build, not application code."
      },
      {
        "text": "Is there are roadmap to supporting .NET7 js import statements?",
        "source": "issue body",
        "interpretation": "Reporter is asking about a future feature (JS import interop), confirming this is a feature request not a regression."
      },
      {
        "text": "requiring the removal of unsafe-inline / unsafe-eval expressions from the Content-Security-Policy",
        "source": "issue body",
        "interpretation": "Security requirement from a client — this is a real-world enterprise constraint, not a minor request."
      }
    ],
    "rationale": "The issue is titled [BUG] but describes a feature/platform limitation: the WASM native binary uses Emscripten-generated JS that includes eval(), which blocks strict CSP policies. This is not a regression (N/A for 'last known good version') but a missing capability — SkiaSharp's WASM build has never supported strict CSP. The request for .NET 7 JS import statement support confirms this is a forward-looking feature request. Area is libSkiaSharp.native because the change must be in how the native WASM binary is compiled/linked. The partner tag is unoplatform because the reporter is using Uno Platform.",
    "nextQuestions": [
      "Which Emscripten version is currently used to build the WASM libSkiaSharp.a?",
      "Has the .NET WASM runtime (net8/net9) addressed eval usage at the runtime level so only SkiaSharp's own JS glue needs updating?",
      "Does the Blazor WASM variant of SkiaSharp have the same CSP issue?"
    ],
    "resolution": {
      "hypothesis": "Updating the Emscripten build flags for WASM to use ES6 modules (-s EXPORT_ES6=1, -s MODULARIZE=1) or a newer runtime that uses JS import instead of eval would eliminate the CSP requirement.",
      "proposals": [
        {
          "title": "Update Emscripten build flags for CSP compliance",
          "description": "Investigate updating the WASM native build in native/wasm/ to use Emscripten flags that avoid eval (e.g., MODULARIZE, EXPORT_ES6, or newer Emscripten with JS import support). This may also require updating the .targets integration.",
          "category": "investigation",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        },
        {
          "title": "Track .NET WASM runtime eval elimination",
          "description": "Monitor dotnet/runtime progress on eliminating eval from the WASM runtime itself. If the .NET runtime resolves this at the host level, SkiaSharp may benefit automatically.",
          "category": "investigation",
          "confidence": 0.6,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Update Emscripten build flags for CSP compliance",
      "recommendedReason": "Direct fix targeting the root cause. The eval usage originates in compiled Emscripten JS glue, and modern Emscripten supports CSP-safe output modes."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.78,
      "reason": "Valid feature request requiring native WASM build investigation. No quick fix available; needs design and upstream Emscripten/runtime coordination.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, native WASM, compatibility labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/feature-request",
          "area/libSkiaSharp.native",
          "os/WASM",
          "tenet/compatibility",
          "partner/unoplatform"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge request, explain root cause, and note investigation needed",
        "risk": "medium",
        "confidence": 0.78,
        "comment": "Thanks for raising this! The `unsafe-eval` CSP requirement comes from Emscripten-generated JavaScript glue code used to load the native WebAssembly binary — it is not generated by SkiaSharp's own C# code.\n\nModern Emscripten versions support CSP-safe output via flags like `-s EXPORT_ES6=1` / `-s MODULARIZE=1`, and .NET 7+ introduced JS import interop (`[JSImport]`/`[JSExport]`) which avoids eval. We need to investigate updating the WASM native build pipeline in SkiaSharp to use these newer approaches.\n\nThis is a valid enhancement request. We'll track it here. In the meantime, as a short-term workaround, you may be able to configure your CSP to apply `unsafe-eval` only to the specific WASM worker scope rather than globally, reducing the security exposure."
      }
    ]
  }
}
```

</details>
