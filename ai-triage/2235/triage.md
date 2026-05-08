# Issue Triage Report — #2235

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T07:37:06Z |
| Type | type/question (0.97 (97%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter asks whether SkiaSharp supports DirectX 12 natively (or at minimum DirectX 11) for use as a 2D UI renderer inside a native DirectX 12 application.

**Analysis:** SkiaSharp does support Direct3D 12 as a GPU backend via GRContext.CreateDirect3D() and the companion GRD3DBackendContext class, with a high-level Vortice interop package (SkiaSharp.Direct3D.Vortice) also available. The question can be fully answered from the existing codebase.

**Recommendations:** **close-as-not-a-bug** — SkiaSharp already supports Direct3D 12 via GRContext.CreateDirect3D() and GRD3DBackendContext. The question is fully answered by existing public API.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Direct3D |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

**Environment:** Native DirectX 12 application, wants to embed a 2D UI renderer

## Analysis

### Technical Summary

SkiaSharp does support Direct3D 12 as a GPU backend via GRContext.CreateDirect3D() and the companion GRD3DBackendContext class, with a high-level Vortice interop package (SkiaSharp.Direct3D.Vortice) also available. The question can be fully answered from the existing codebase.

### Rationale

The title explicitly marks this as a question ('[QUESTION]'). The reporter asks about DirectX 12 support for embedding SkiaSharp into a native D3D12 application. Code investigation confirms that Direct3D 12 support exists through GRBackend.Direct3D, GRContext.CreateDirect3D(), GRD3DBackendContext, and the SkiaSharp.Direct3D.Vortice NuGet package. No bug or missing feature is present; the functionality the reporter seeks already exists.

### Key Signals

- "I wasn't able to find any information about whether SkiaSharp supports DirectX12 natively." — **issue body** (Reporter lacks discoverability of the Direct3D 12 backend support — a docs/discoverability gap, not a missing feature.)
- "I am writing a custom 2D UI renderer for my native DirectX12 application." — **comment #1** (Use case is clearly D3D12 interop — exactly what GRD3DBackendContext and GRContext.CreateDirect3D() are designed for.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/EnumMappings.cs` | 14-28 | direct | GRBackend enum includes 'Direct3D = 4' with full round-trip mapping to the native GRBackendNative.Direct3D value, confirming the backend is registered and routed. |
| `binding/SkiaSharp/GRContext.cs` | 66-70 | direct | GRContext.CreateDirect3D(GRD3DBackendContext) and its options overload are public factory methods that create a Direct3D 12 GPU context, directly answering the reporter's question. |
| `binding/SkiaSharp/GRD3DBackendContext.cs` | 1-43 | direct | GRD3DBackendContext exposes Adapter, Device, and Queue IntPtr properties matching the D3D12 object model (IDXGIAdapter, ID3D12Device, ID3D12CommandQueue). |
| `source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DBackendContext.cs` | 1-47 | related | GRVorticeD3DBackendContext wraps GRD3DBackendContext with strongly-typed Vortice.Direct3D12 types (IDXGIAdapter1, ID3D12Device2, ID3D12CommandQueue), providing a convenient higher-level entry point for Direct3D 12 interop. |

### Workarounds

- Use GRContext.CreateDirect3D(new GRD3DBackendContext { Adapter = ..., Device = ..., Queue = ... }) with raw IntPtr handles from your D3D12 objects.
- Use the SkiaSharp.Direct3D.Vortice NuGet package which provides GRVorticeD3DBackendContext with strongly-typed Vortice.Direct3D12 properties for a cleaner integration.

### Resolution Proposals

**Hypothesis:** SkiaSharp already supports Direct3D 12 via GRContext.CreateDirect3D() — the reporter simply did not find the API because documentation discoverability is low.

1. **Use GRContext.CreateDirect3D with raw handles** — workaround, confidence 0.90 (90%), cost/m, validated=yes
   - Create a GRD3DBackendContext with your D3D12 adapter, device, and command queue pointers, then call GRContext.CreateDirect3D() to get a SkiaSharp GPU context backed by Direct3D 12.

```csharp
var backendContext = new GRD3DBackendContext
{
    Adapter = adapterPtr,  // IDXGIAdapter1*
    Device  = devicePtr,   // ID3D12Device*
    Queue   = queuePtr     // ID3D12CommandQueue*
};
using var grContext = GRContext.CreateDirect3D(backendContext);
```
2. **Use SkiaSharp.Direct3D.Vortice package** — alternative, confidence 0.88 (88%), cost/s, validated=yes
   - Add the SkiaSharp.Direct3D.Vortice NuGet package and use GRVorticeD3DBackendContext for strongly-typed Vortice.Direct3D12 interop instead of raw IntPtrs.

```csharp
var backendContext = new GRVorticeD3DBackendContext
{
    Adapter = dxgiAdapter,   // IDXGIAdapter1
    Device  = d3d12Device,   // ID3D12Device2
    Queue   = commandQueue   // ID3D12CommandQueue
};
using var grContext = GRContext.CreateDirect3D(backendContext);
```

**Recommended proposal:** Use GRContext.CreateDirect3D with raw handles

**Why:** Works with any D3D12 interop library without additional NuGet dependencies.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | SkiaSharp already supports Direct3D 12 via GRContext.CreateDirect3D() and GRD3DBackendContext. The question is fully answered by existing public API. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply question, SkiaSharp area, Windows, and Direct3D backend labels | labels=type/question, area/SkiaSharp, os/Windows-Classic, backend/Direct3D |
| add-comment | medium | 0.90 (90%) | Answer the question explaining Direct3D 12 support | — |
| close-issue | medium | 0.88 (88%) | Close as answered — feature exists | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Yes, SkiaSharp supports **Direct3D 12** natively!

You can create a Direct3D 12 GPU context using `GRContext.CreateDirect3D()` together with `GRD3DBackendContext`, which accepts raw pointers to your existing `IDXGIAdapter`, `ID3D12Device`, and `ID3D12CommandQueue` objects:

```csharp
var backendContext = new GRD3DBackendContext
{
    Adapter = adapterPtr,  // IDXGIAdapter1*
    Device  = devicePtr,   // ID3D12Device*
    Queue   = queuePtr     // ID3D12CommandQueue*
};
using var grContext = GRContext.CreateDirect3D(backendContext);
```

If you are using the [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) bindings, the `SkiaSharp.Direct3D.Vortice` NuGet package provides the strongly-typed `GRVorticeD3DBackendContext` wrapper that avoids raw `IntPtr` marshalling:

```csharp
var backendContext = new GRVorticeD3DBackendContext
{
    Adapter = dxgiAdapter,
    Device  = d3d12Device,
    Queue   = commandQueue
};
using var grContext = GRContext.CreateDirect3D(backendContext);
```

Note that Direct3D 11 is **not** directly exposed — Skia's backend targets D3D12. If your application uses D3D11 you can still use it via the `DirectX11On12` compatibility layer as you already noted.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2235,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T07:37:06Z"
  },
  "summary": "Reporter asks whether SkiaSharp supports DirectX 12 natively (or at minimum DirectX 11) for use as a 2D UI renderer inside a native DirectX 12 application.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Direct3D"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Native DirectX 12 application, wants to embed a 2D UI renderer",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "SkiaSharp does support Direct3D 12 as a GPU backend via GRContext.CreateDirect3D() and the companion GRD3DBackendContext class, with a high-level Vortice interop package (SkiaSharp.Direct3D.Vortice) also available. The question can be fully answered from the existing codebase.",
    "rationale": "The title explicitly marks this as a question ('[QUESTION]'). The reporter asks about DirectX 12 support for embedding SkiaSharp into a native D3D12 application. Code investigation confirms that Direct3D 12 support exists through GRBackend.Direct3D, GRContext.CreateDirect3D(), GRD3DBackendContext, and the SkiaSharp.Direct3D.Vortice NuGet package. No bug or missing feature is present; the functionality the reporter seeks already exists.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/EnumMappings.cs",
        "lines": "14-28",
        "finding": "GRBackend enum includes 'Direct3D = 4' with full round-trip mapping to the native GRBackendNative.Direct3D value, confirming the backend is registered and routed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "66-70",
        "finding": "GRContext.CreateDirect3D(GRD3DBackendContext) and its options overload are public factory methods that create a Direct3D 12 GPU context, directly answering the reporter's question.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRD3DBackendContext.cs",
        "lines": "1-43",
        "finding": "GRD3DBackendContext exposes Adapter, Device, and Queue IntPtr properties matching the D3D12 object model (IDXGIAdapter, ID3D12Device, ID3D12CommandQueue).",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Direct3D/SkiaSharp.Direct3D.Vortice/GRVorticeD3DBackendContext.cs",
        "lines": "1-47",
        "finding": "GRVorticeD3DBackendContext wraps GRD3DBackendContext with strongly-typed Vortice.Direct3D12 types (IDXGIAdapter1, ID3D12Device2, ID3D12CommandQueue), providing a convenient higher-level entry point for Direct3D 12 interop.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I wasn't able to find any information about whether SkiaSharp supports DirectX12 natively.",
        "source": "issue body",
        "interpretation": "Reporter lacks discoverability of the Direct3D 12 backend support — a docs/discoverability gap, not a missing feature."
      },
      {
        "text": "I am writing a custom 2D UI renderer for my native DirectX12 application.",
        "source": "comment #1",
        "interpretation": "Use case is clearly D3D12 interop — exactly what GRD3DBackendContext and GRContext.CreateDirect3D() are designed for."
      }
    ],
    "workarounds": [
      "Use GRContext.CreateDirect3D(new GRD3DBackendContext { Adapter = ..., Device = ..., Queue = ... }) with raw IntPtr handles from your D3D12 objects.",
      "Use the SkiaSharp.Direct3D.Vortice NuGet package which provides GRVorticeD3DBackendContext with strongly-typed Vortice.Direct3D12 properties for a cleaner integration."
    ],
    "resolution": {
      "hypothesis": "SkiaSharp already supports Direct3D 12 via GRContext.CreateDirect3D() — the reporter simply did not find the API because documentation discoverability is low.",
      "proposals": [
        {
          "title": "Use GRContext.CreateDirect3D with raw handles",
          "description": "Create a GRD3DBackendContext with your D3D12 adapter, device, and command queue pointers, then call GRContext.CreateDirect3D() to get a SkiaSharp GPU context backed by Direct3D 12.",
          "category": "workaround",
          "codeSnippet": "var backendContext = new GRD3DBackendContext\n{\n    Adapter = adapterPtr,  // IDXGIAdapter1*\n    Device  = devicePtr,   // ID3D12Device*\n    Queue   = queuePtr     // ID3D12CommandQueue*\n};\nusing var grContext = GRContext.CreateDirect3D(backendContext);",
          "confidence": 0.9,
          "effort": "cost/m",
          "validated": "yes"
        },
        {
          "title": "Use SkiaSharp.Direct3D.Vortice package",
          "description": "Add the SkiaSharp.Direct3D.Vortice NuGet package and use GRVorticeD3DBackendContext for strongly-typed Vortice.Direct3D12 interop instead of raw IntPtrs.",
          "category": "alternative",
          "codeSnippet": "var backendContext = new GRVorticeD3DBackendContext\n{\n    Adapter = dxgiAdapter,   // IDXGIAdapter1\n    Device  = d3d12Device,   // ID3D12Device2\n    Queue   = commandQueue   // ID3D12CommandQueue\n};\nusing var grContext = GRContext.CreateDirect3D(backendContext);",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use GRContext.CreateDirect3D with raw handles",
      "recommendedReason": "Works with any D3D12 interop library without additional NuGet dependencies."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "SkiaSharp already supports Direct3D 12 via GRContext.CreateDirect3D() and GRD3DBackendContext. The question is fully answered by existing public API.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp area, Windows, and Direct3D backend labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Direct3D"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the question explaining Direct3D 12 support",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Yes, SkiaSharp supports **Direct3D 12** natively!\n\nYou can create a Direct3D 12 GPU context using `GRContext.CreateDirect3D()` together with `GRD3DBackendContext`, which accepts raw pointers to your existing `IDXGIAdapter`, `ID3D12Device`, and `ID3D12CommandQueue` objects:\n\n```csharp\nvar backendContext = new GRD3DBackendContext\n{\n    Adapter = adapterPtr,  // IDXGIAdapter1*\n    Device  = devicePtr,   // ID3D12Device*\n    Queue   = queuePtr     // ID3D12CommandQueue*\n};\nusing var grContext = GRContext.CreateDirect3D(backendContext);\n```\n\nIf you are using the [Vortice.Windows](https://github.com/amerkoleci/Vortice.Windows) bindings, the `SkiaSharp.Direct3D.Vortice` NuGet package provides the strongly-typed `GRVorticeD3DBackendContext` wrapper that avoids raw `IntPtr` marshalling:\n\n```csharp\nvar backendContext = new GRVorticeD3DBackendContext\n{\n    Adapter = dxgiAdapter,\n    Device  = d3d12Device,\n    Queue   = commandQueue\n};\nusing var grContext = GRContext.CreateDirect3D(backendContext);\n```\n\nNote that Direct3D 11 is **not** directly exposed — Skia's backend targets D3D12. If your application uses D3D11 you can still use it via the `DirectX11On12` compatibility layer as you already noted."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — feature exists",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
