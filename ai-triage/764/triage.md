# Issue Triage Report — #764

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T11:02:49Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.82 (82%)) |

**Issue Summary:** User asks whether SkiaSharp can draw into a shared GPU resource (DirectX or other 3D API handle) for use in 3D applications.

**Analysis:** The reporter wants to know if they can create a shared GPU context/surface in SkiaSharp and pass its handle (e.g. D3D device/queue) to a 3D API. The current SkiaSharp codebase provides GRContext.CreateDirect3D(GRD3DBackendContext), CreateVulkan, CreateMetal, and CreateGl factory methods that accept native GPU handles, enabling exactly this use case. The maintainer pointed to issues #688 and #755 which contain code examples for offscreen GPU rendering patterns.

**Recommendations:** **close-as-not-a-bug** — This is a how-to question that has already been answered by the maintainer pointing to related issues #688 and #755. The feature the reporter wants (shared GPU surface via Direct3D/Vulkan handles) is supported through GRContext.CreateDirect3D, CreateVulkan, and CreateMetal APIs.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/Direct3D, backend/OpenGL, backend/Vulkan |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/688 — Related issue discussing offscreen GPU rendering with code examples
- https://github.com/mono/SkiaSharp/issues/755 — Related issue on shared GPU rendering context

## Analysis

### Technical Summary

The reporter wants to know if they can create a shared GPU context/surface in SkiaSharp and pass its handle (e.g. D3D device/queue) to a 3D API. The current SkiaSharp codebase provides GRContext.CreateDirect3D(GRD3DBackendContext), CreateVulkan, CreateMetal, and CreateGl factory methods that accept native GPU handles, enabling exactly this use case. The maintainer pointed to issues #688 and #755 which contain code examples for offscreen GPU rendering patterns.

### Rationale

This is a how-to question. The maintainer's comment links to two issues (#688, #755) that demonstrate the pattern. The current codebase (GRContext.CreateDirect3D, CreateVulkan, CreateMetal) directly supports sharing GPU handles with 3D APIs. No code bug or missing feature — the question has a documented answer.

### Key Signals

- "what you basically need for that is the GPU handle to create a surface in DirectX or other 3d API's" — **issue body** (Reporter wants to pass an existing D3D/Vulkan device handle to SkiaSharp, not create a fresh context.)
- "I think this is exactly what @tcuthill and @pathw0rk3r are doing in #688 and #755" — **maintainer comment** (The maintainer confirmed the question was answered by existing issues with code examples.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | 66-96 | direct | GRContext.CreateDirect3D(GRD3DBackendContext) and CreateVulkan/CreateMetal/CreateGl factory methods exist, accepting native GPU backend contexts. |
| `binding/SkiaSharp/GRD3DBackendContext.cs` | 7-37 | direct | GRD3DBackendContext exposes Adapter, Device, and Queue as nint properties accepting IDXGIAdapter, ID3D12Device, and ID3D12CommandQueue COM pointers from a D3D12 application. |

### Resolution Proposals

1. **Answer with existing API information** — workaround, cost/xs, validated=untested
   - Point reporter to GRContext.CreateDirect3D / CreateVulkan / CreateMetal which accept native GPU handles, and cross-reference issues #688 and #755.

**Recommended proposal:** Answer with existing API information

**Why:** The question is answered by existing APIs and related issues.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.82 (82%) |
| Reason | This is a how-to question that has already been answered by the maintainer pointing to related issues #688 and #755. The feature the reporter wants (shared GPU surface via Direct3D/Vulkan handles) is supported through GRContext.CreateDirect3D, CreateVulkan, and CreateMetal APIs. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply classification labels | labels=type/question, area/SkiaSharp |
| add-comment | medium | 0.82 (82%) | Answer the question by explaining available GPU context factories and pointing to examples | — |
| close-issue | medium | — | Close as answered — the question has been addressed | stateReason=completed |
| link-related | low | 0.95 (95%) | Cross-reference #688 which has code examples for GPU context sharing | linkedIssue=#688 |
| link-related | low | 0.90 (90%) | Cross-reference #755 for shared GPU rendering | linkedIssue=#755 |

**Comment draft for `add-comment`:**

```markdown
Yes, this is supported! SkiaSharp exposes `GRContext.CreateDirect3D(GRD3DBackendContext)`, `GRContext.CreateVulkan(GRVkBackendContext)`, and `GRContext.CreateMetal(GRMtlBackendContext)` factory methods. Each backend context type accepts native GPU handles (e.g. `GRD3DBackendContext.Device` takes an `ID3D12Device` pointer as `nint`).

You can see working examples of sharing GPU contexts in #688 and #755. Those issues demonstrate creating an offscreen GPU surface that can be used alongside a 3D application's device.

Closing as answered — please reopen if you have more specific questions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 764,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T11:02:49Z"
  },
  "summary": "User asks whether SkiaSharp can draw into a shared GPU resource (DirectX or other 3D API handle) for use in 3D applications.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "backends": [
      "backend/Direct3D",
      "backend/OpenGL",
      "backend/Vulkan"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/688",
          "description": "Related issue discussing offscreen GPU rendering with code examples"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/755",
          "description": "Related issue on shared GPU rendering context"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The reporter wants to know if they can create a shared GPU context/surface in SkiaSharp and pass its handle (e.g. D3D device/queue) to a 3D API. The current SkiaSharp codebase provides GRContext.CreateDirect3D(GRD3DBackendContext), CreateVulkan, CreateMetal, and CreateGl factory methods that accept native GPU handles, enabling exactly this use case. The maintainer pointed to issues #688 and #755 which contain code examples for offscreen GPU rendering patterns.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "finding": "GRContext.CreateDirect3D(GRD3DBackendContext) and CreateVulkan/CreateMetal/CreateGl factory methods exist, accepting native GPU backend contexts.",
        "relevance": "direct",
        "lines": "66-96"
      },
      {
        "file": "binding/SkiaSharp/GRD3DBackendContext.cs",
        "finding": "GRD3DBackendContext exposes Adapter, Device, and Queue as nint properties accepting IDXGIAdapter, ID3D12Device, and ID3D12CommandQueue COM pointers from a D3D12 application.",
        "relevance": "direct",
        "lines": "7-37"
      }
    ],
    "keySignals": [
      {
        "text": "what you basically need for that is the GPU handle to create a surface in DirectX or other 3d API's",
        "source": "issue body",
        "interpretation": "Reporter wants to pass an existing D3D/Vulkan device handle to SkiaSharp, not create a fresh context."
      },
      {
        "text": "I think this is exactly what @tcuthill and @pathw0rk3r are doing in #688 and #755",
        "source": "maintainer comment",
        "interpretation": "The maintainer confirmed the question was answered by existing issues with code examples."
      }
    ],
    "rationale": "This is a how-to question. The maintainer's comment links to two issues (#688, #755) that demonstrate the pattern. The current codebase (GRContext.CreateDirect3D, CreateVulkan, CreateMetal) directly supports sharing GPU handles with 3D APIs. No code bug or missing feature — the question has a documented answer.",
    "resolution": {
      "proposals": [
        {
          "title": "Answer with existing API information",
          "description": "Point reporter to GRContext.CreateDirect3D / CreateVulkan / CreateMetal which accept native GPU handles, and cross-reference issues #688 and #755.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Answer with existing API information",
      "recommendedReason": "The question is answered by existing APIs and related issues."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.82,
      "reason": "This is a how-to question that has already been answered by the maintainer pointing to related issues #688 and #755. The feature the reporter wants (shared GPU surface via Direct3D/Vulkan handles) is supported through GRContext.CreateDirect3D, CreateVulkan, and CreateMetal APIs.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply classification labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/question",
          "area/SkiaSharp"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question by explaining available GPU context factories and pointing to examples",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Yes, this is supported! SkiaSharp exposes `GRContext.CreateDirect3D(GRD3DBackendContext)`, `GRContext.CreateVulkan(GRVkBackendContext)`, and `GRContext.CreateMetal(GRMtlBackendContext)` factory methods. Each backend context type accepts native GPU handles (e.g. `GRD3DBackendContext.Device` takes an `ID3D12Device` pointer as `nint`).\n\nYou can see working examples of sharing GPU contexts in #688 and #755. Those issues demonstrate creating an offscreen GPU surface that can be used alongside a 3D application's device.\n\nClosing as answered — please reopen if you have more specific questions."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — the question has been addressed",
        "risk": "medium",
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Cross-reference #688 which has code examples for GPU context sharing",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 688
      },
      {
        "type": "link-related",
        "description": "Cross-reference #755 for shared GPU rendering",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 755
      }
    ]
  }
}
```

</details>
