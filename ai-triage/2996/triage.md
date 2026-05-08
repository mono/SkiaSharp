# Issue Triage Report — #2996

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-29T10:30:56Z |
| Type | type/feature-request (0.95 (95%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-fixed (0.92 (92%)) |

**Issue Summary:** Reporter requests DirectX (D3D11) and Metal GRContext support in SkiaSharp, unaware that GRContext.CreateDirect3D() (D3D12) and GRContext.CreateMetal() already exist in the codebase and are available in 3.119 preview.

**Analysis:** The requested feature (DirectX and Metal GRContext) is already implemented. GRContext.CreateDirect3D() wraps the D3D12 backend via GRD3DBackendContext (IDXGIAdapter, ID3D12Device, ID3D12CommandQueue). GRContext.CreateMetal() wraps the Metal backend via GRMtlBackendContext. Note: D3D11 is not supported by Skia's Direct3D backend — only D3D12. The reporter's assumption that the API was missing is incorrect.

**Recommendations:** **close-as-fixed** — GRContext.CreateDirect3D() and GRContext.CreateMetal() already exist in SkiaSharp. Related issue #2817 was closed as completed in March 2025. A contributor confirmed availability in 3.119 preview.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic, os/macOS, os/iOS |
| Backends | backend/Direct3D, backend/Metal |
| Tenets | — |
| Partner | — |
| Current labels | type/feature-request |

## Evidence

### Reproduction

**Environment:** Windows (implied by DirectX reference); macOS/iOS (implied by Metal reference)

**Related issues:** #2817

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2817 — Issue #2817: [FEATURE] Support Direct3D — closed as completed March 2025

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.95 (95%) |
| Reason | GRContext.CreateDirect3D() and GRContext.CreateMetal() both exist in binding/SkiaSharp/GRContext.cs. A contributor confirmed DX12 and Metal are available in 3.119 preview. Related issue #2817 was closed as completed. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | 3.119.0-preview |

## Analysis

### Technical Summary

The requested feature (DirectX and Metal GRContext) is already implemented. GRContext.CreateDirect3D() wraps the D3D12 backend via GRD3DBackendContext (IDXGIAdapter, ID3D12Device, ID3D12CommandQueue). GRContext.CreateMetal() wraps the Metal backend via GRMtlBackendContext. Note: D3D11 is not supported by Skia's Direct3D backend — only D3D12. The reporter's assumption that the API was missing is incorrect.

### Rationale

The issue title and body request GRContext.CreateDX (DirectX) and Metal context creation. Code inspection shows both GRContext.CreateDirect3D() and GRContext.CreateMetal() are present in the public API. The related issue #2817 was closed as completed. A contributor comment on this very issue confirms availability in 3.119 preview. This is a close-as-fixed scenario where the reporter was unaware of the existing API.

### Key Signals

- "it seems like there's no GRContext.CreateDX..." — **issue body** (Reporter was unaware of GRContext.CreateDirect3D() which already exists.)
- "You can use DX12 and Metal in 3.119 preview - Skia itself doesn't support DX11 as a backend (but you can do it via ANGLE)." — **comment by jeremy-visionaid** (Contributor confirms feature is available in 3.119 preview. Also clarifies D3D11 is not supported natively but D3D12 is.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContext.cs` | 66-79 | direct | GRContext.CreateDirect3D(GRD3DBackendContext) and CreateDirect3D(GRD3DBackendContext, GRContextOptions) are public static factory methods — the D3D12 backend is fully bound |
| `binding/SkiaSharp/GRContext.cs` | 82-99 | direct | GRContext.CreateMetal(GRMtlBackendContext) and CreateMetal(GRMtlBackendContext, GRContextOptions) are public static factory methods — Metal backend is fully bound |
| `binding/SkiaSharp/GRD3DBackendContext.cs` | 1-37 | direct | GRD3DBackendContext exposes Adapter (IDXGIAdapter), Device (ID3D12Device), Queue (ID3D12CommandQueue) — this is D3D12, not D3D11. Reporter requested D3D11 which Skia does not support natively. |
| `binding/SkiaSharp/GRMtlBackendContext.cs` | 1-65 | direct | GRMtlBackendContext exposes IMTLDevice and IMTLCommandQueue on Apple platforms (iOS, macOS, tvOS) — Metal backend is fully bound |

### Resolution Proposals

**Hypothesis:** The feature already exists in SkiaSharp via GRContext.CreateDirect3D() (D3D12) and GRContext.CreateMetal(). The reporter should use these APIs. D3D11 is not supported by Skia natively; ANGLE can be used as a bridge if D3D11 interop is needed.

1. **Redirect reporter to existing Direct3D 12 and Metal APIs** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Inform reporter that GRContext.CreateDirect3D() using GRD3DBackendContext (with IDXGIAdapter, ID3D12Device, ID3D12CommandQueue) and GRContext.CreateMetal() using GRMtlBackendContext are available in SkiaSharp 3.119 preview. Note that D3D11 is not supported by Skia — D3D12 is required. ANGLE can bridge D3D11/D3D12.

**Recommended proposal:** Redirect reporter to existing Direct3D 12 and Metal APIs

**Why:** The feature already exists. Reporter needs to know the correct API names and that D3D12 (not D3D11) is required.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.92 (92%) |
| Reason | GRContext.CreateDirect3D() and GRContext.CreateMetal() already exist in SkiaSharp. Related issue #2817 was closed as completed in March 2025. A contributor confirmed availability in 3.119 preview. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply feature-request, area/SkiaSharp, Windows, Direct3D, Metal labels | labels=type/feature-request, area/SkiaSharp, os/Windows-Classic, os/macOS, os/iOS, backend/Direct3D, backend/Metal |
| add-comment | high | 0.92 (92%) | Inform reporter that DirectX (D3D12) and Metal GRContext support already exist in SkiaSharp | — |
| close-issue | medium | 0.90 (90%) | Close as fixed — feature already implemented | stateReason=completed |
| link-related | low | 0.95 (95%) | Link related issue #2817 (same Direct3D feature request, closed as completed) | linkedIssue=#2817 |

**Comment draft for `add-comment`:**

```markdown
Hi @raphm4044! The feature you're requesting is already available in SkiaSharp.

**Direct3D 12 (Windows):**
```csharp
using var backendContext = new GRD3DBackendContext
{
    Adapter = adapter.NativePointer,  // IDXGIAdapter1*
    Device  = device.NativePointer,  // ID3D12Device*
    Queue   = queue.NativePointer,   // ID3D12CommandQueue*
};
using var grContext = GRContext.CreateDirect3D(backendContext);
```

**Metal (macOS / iOS):**
```csharp
using var backendContext = new GRMtlBackendContext
{
    Device = mtlDevice,
    Queue  = mtlCommandQueue,
};
using var grContext = GRContext.CreateMetal(backendContext);
```

Note: SkiaSharp uses **Direct3D 12** (not D3D11). If you need D3D11 interop you can use ANGLE as a bridge.

Both APIs are available in SkiaSharp 3.119 preview. See also #2817 which tracks the same request and was closed as completed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2996,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-29T10:30:56Z",
    "currentLabels": [
      "type/feature-request"
    ]
  },
  "summary": "Reporter requests DirectX (D3D11) and Metal GRContext support in SkiaSharp, unaware that GRContext.CreateDirect3D() (D3D12) and GRContext.CreateMetal() already exist in the codebase and are available in 3.119 preview.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.95
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic",
      "os/macOS",
      "os/iOS"
    ],
    "backends": [
      "backend/Direct3D",
      "backend/Metal"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Windows (implied by DirectX reference); macOS/iOS (implied by Metal reference)",
      "relatedIssues": [
        2817
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2817",
          "description": "Issue #2817: [FEATURE] Support Direct3D — closed as completed March 2025"
        }
      ]
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.95,
      "reason": "GRContext.CreateDirect3D() and GRContext.CreateMetal() both exist in binding/SkiaSharp/GRContext.cs. A contributor confirmed DX12 and Metal are available in 3.119 preview. Related issue #2817 was closed as completed.",
      "relatedPRs": [],
      "fixedInVersion": "3.119.0-preview"
    }
  },
  "analysis": {
    "summary": "The requested feature (DirectX and Metal GRContext) is already implemented. GRContext.CreateDirect3D() wraps the D3D12 backend via GRD3DBackendContext (IDXGIAdapter, ID3D12Device, ID3D12CommandQueue). GRContext.CreateMetal() wraps the Metal backend via GRMtlBackendContext. Note: D3D11 is not supported by Skia's Direct3D backend — only D3D12. The reporter's assumption that the API was missing is incorrect.",
    "rationale": "The issue title and body request GRContext.CreateDX (DirectX) and Metal context creation. Code inspection shows both GRContext.CreateDirect3D() and GRContext.CreateMetal() are present in the public API. The related issue #2817 was closed as completed. A contributor comment on this very issue confirms availability in 3.119 preview. This is a close-as-fixed scenario where the reporter was unaware of the existing API.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "66-79",
        "finding": "GRContext.CreateDirect3D(GRD3DBackendContext) and CreateDirect3D(GRD3DBackendContext, GRContextOptions) are public static factory methods — the D3D12 backend is fully bound",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "82-99",
        "finding": "GRContext.CreateMetal(GRMtlBackendContext) and CreateMetal(GRMtlBackendContext, GRContextOptions) are public static factory methods — Metal backend is fully bound",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRD3DBackendContext.cs",
        "lines": "1-37",
        "finding": "GRD3DBackendContext exposes Adapter (IDXGIAdapter), Device (ID3D12Device), Queue (ID3D12CommandQueue) — this is D3D12, not D3D11. Reporter requested D3D11 which Skia does not support natively.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRMtlBackendContext.cs",
        "lines": "1-65",
        "finding": "GRMtlBackendContext exposes IMTLDevice and IMTLCommandQueue on Apple platforms (iOS, macOS, tvOS) — Metal backend is fully bound",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "it seems like there's no GRContext.CreateDX...",
        "source": "issue body",
        "interpretation": "Reporter was unaware of GRContext.CreateDirect3D() which already exists."
      },
      {
        "text": "You can use DX12 and Metal in 3.119 preview - Skia itself doesn't support DX11 as a backend (but you can do it via ANGLE).",
        "source": "comment by jeremy-visionaid",
        "interpretation": "Contributor confirms feature is available in 3.119 preview. Also clarifies D3D11 is not supported natively but D3D12 is."
      }
    ],
    "resolution": {
      "hypothesis": "The feature already exists in SkiaSharp via GRContext.CreateDirect3D() (D3D12) and GRContext.CreateMetal(). The reporter should use these APIs. D3D11 is not supported by Skia natively; ANGLE can be used as a bridge if D3D11 interop is needed.",
      "proposals": [
        {
          "title": "Redirect reporter to existing Direct3D 12 and Metal APIs",
          "description": "Inform reporter that GRContext.CreateDirect3D() using GRD3DBackendContext (with IDXGIAdapter, ID3D12Device, ID3D12CommandQueue) and GRContext.CreateMetal() using GRMtlBackendContext are available in SkiaSharp 3.119 preview. Note that D3D11 is not supported by Skia — D3D12 is required. ANGLE can bridge D3D11/D3D12.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Redirect reporter to existing Direct3D 12 and Metal APIs",
      "recommendedReason": "The feature already exists. Reporter needs to know the correct API names and that D3D12 (not D3D11) is required."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.92,
      "reason": "GRContext.CreateDirect3D() and GRContext.CreateMetal() already exist in SkiaSharp. Related issue #2817 was closed as completed in March 2025. A contributor confirmed availability in 3.119 preview.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, area/SkiaSharp, Windows, Direct3D, Metal labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "os/macOS",
          "os/iOS",
          "backend/Direct3D",
          "backend/Metal"
        ]
      },
      {
        "type": "add-comment",
        "description": "Inform reporter that DirectX (D3D12) and Metal GRContext support already exist in SkiaSharp",
        "risk": "high",
        "confidence": 0.92,
        "comment": "Hi @raphm4044! The feature you're requesting is already available in SkiaSharp.\n\n**Direct3D 12 (Windows):**\n```csharp\nusing var backendContext = new GRD3DBackendContext\n{\n    Adapter = adapter.NativePointer,  // IDXGIAdapter1*\n    Device  = device.NativePointer,  // ID3D12Device*\n    Queue   = queue.NativePointer,   // ID3D12CommandQueue*\n};\nusing var grContext = GRContext.CreateDirect3D(backendContext);\n```\n\n**Metal (macOS / iOS):**\n```csharp\nusing var backendContext = new GRMtlBackendContext\n{\n    Device = mtlDevice,\n    Queue  = mtlCommandQueue,\n};\nusing var grContext = GRContext.CreateMetal(backendContext);\n```\n\nNote: SkiaSharp uses **Direct3D 12** (not D3D11). If you need D3D11 interop you can use ANGLE as a bridge.\n\nBoth APIs are available in SkiaSharp 3.119 preview. See also #2817 which tracks the same request and was closed as completed."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — feature already implemented",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Link related issue #2817 (same Direct3D feature request, closed as completed)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2817
      }
    ]
  }
}
```

</details>
