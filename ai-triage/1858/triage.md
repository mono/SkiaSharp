# Issue Triage Report — #1858

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T04:46:47Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | close-as-external (0.80 (80%)) |

**Issue Summary:** Invalid clipping on Apple Silicon and some Nvidia 3000-series GPUs caused by an upstream Skia stencil buffer bug (skia#12164), with a workaround available via GRContextOptions.AvoidStencilBuffers.

**Analysis:** The clipping bug is rooted in upstream Skia's handling of stencil buffers on Apple Silicon (Metal backend) and certain Nvidia 3000-series GPUs (OpenGL backend). The upstream Skia bug (skia#12164) was fixed in a later Skia milestone; until SkiaSharp upgrades to that milestone, the issue persists. A known workaround is setting GRContextOptions.AvoidStencilBuffers = true. Setting SKPaint.IsAntiAliased = false also resolves the visual artifact but degrades quality.

**Recommendations:** **close-as-external** — Root cause is upstream Skia bug skia#12164 affecting stencil buffer clipping on Apple Silicon and certain Nvidia GPUs. Fix requires a Skia milestone upgrade. A documented workaround exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/macOS |
| Backends | backend/Metal, backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://bugs.chromium.org/p/skia/issues/detail?id=12164 — Upstream Skia bug for the stencil buffer clipping issue
- https://github.com/AvaloniaUI/Avalonia/issues/6144 — AvaloniaUI report demonstrating the same clipping bug

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

The clipping bug is rooted in upstream Skia's handling of stencil buffers on Apple Silicon (Metal backend) and certain Nvidia 3000-series GPUs (OpenGL backend). The upstream Skia bug (skia#12164) was fixed in a later Skia milestone; until SkiaSharp upgrades to that milestone, the issue persists. A known workaround is setting GRContextOptions.AvoidStencilBuffers = true. Setting SKPaint.IsAntiAliased = false also resolves the visual artifact but degrades quality.

### Rationale

This is a type/bug in the native Skia layer (area/libSkiaSharp.native). The root cause is an upstream Skia GPU bug affecting stencil buffer clipping on Apple Silicon Metal and certain Nvidia OpenGL drivers. A usable workaround exists (AvoidStencilBuffers = true). The suggested action is close-as-external because the fix requires an upstream Skia milestone upgrade, not a SkiaSharp code change.

### Key Signals

- "This only happens on apple silicon gpus, and some nvidia 3000 series GPUs." — **issue body** (Platform-specific GPU rendering issue, likely stencil buffer handling in Metal/OpenGL backends.)
- "It looks like the fix wasnt merged into skia until July so probably we cant fix this until m9x." — **issue body** (Reporter acknowledges the fix is upstream and SkiaSharp must wait for a milestone bump.)
- "new GRContextOptions { AvoidStencilBuffers = true }" — **comment by danwalmsley** (Confirmed workaround that avoids the stencil buffer path entirely.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/GRContextOptions.cs` | — | direct | GRContextOptions exposes AvoidStencilBuffers property (default false) which maps to the native GrContextOptions::fAvoidStencilBuffers field. Setting it to true disables stencil buffer use and avoids the GPU bug. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | Generated struct includes fAvoidStencilBuffers as a Byte field, confirming the binding correctly passes this option to the native Skia layer. |

### Workarounds

- Set GRContextOptions.AvoidStencilBuffers = true when creating the GRContext to avoid the stencil buffer code path.
- Set SKPaint.IsAntiAliased = false to eliminate the clipping artifact at the cost of rendering quality.

### Resolution Proposals

**Hypothesis:** The bug is in upstream Skia's stencil buffer handling for Metal/OpenGL on affected GPU hardware. No SkiaSharp-side fix is possible until the upstream Skia milestone containing the fix (skia#12164) is adopted.

1. **Use AvoidStencilBuffers workaround** — workaround, cost/xs, validated=yes
   - Set GRContextOptions.AvoidStencilBuffers = true when creating the GPU context to bypass the stencil buffer path.

```csharp
var options = new GRContextOptions { AvoidStencilBuffers = true };
var grContext = GRContext.CreateGl(grInterface, options);
```

**Recommended proposal:** Use AvoidStencilBuffers workaround

**Why:** Confirmed by reporter, low effort, immediately actionable.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-external |
| Confidence | 0.80 (80%) |
| Reason | Root cause is upstream Skia bug skia#12164 affecting stencil buffer clipping on Apple Silicon and certain Nvidia GPUs. Fix requires a Skia milestone upgrade. A documented workaround exists. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, os/macOS, backend/Metal, backend/OpenGL, tenet/reliability | labels=type/bug, area/libSkiaSharp.native, os/macOS, backend/Metal, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge the upstream root cause, share workarounds, and note that a fix requires a Skia milestone bump. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

This is caused by an upstream Skia bug ([skia#12164](https://bugs.chromium.org/p/skia/issues/detail?id=12164)) affecting stencil buffer clipping on Apple Silicon (Metal) and some Nvidia 3000-series GPUs (OpenGL). Since the fix lives in the upstream Skia library, SkiaSharp cannot patch this independently — it requires upgrading to the Skia milestone that includes the fix.

**Workarounds available now:**

1. **Disable stencil buffers** (recommended):
   ```csharp
   var options = new GRContextOptions { AvoidStencilBuffers = true };
   var grContext = GRContext.CreateGl(grInterface, options);
   ```

2. **Disable antialiasing** (degrades quality):
   ```csharp
   paint.IsAntiAliased = false;
   ```

We're tracking the upstream Skia milestone upgrade and will resolve this issue when the fix is incorporated.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1858,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T04:46:47Z"
  },
  "summary": "Invalid clipping on Apple Silicon and some Nvidia 3000-series GPUs caused by an upstream Skia stencil buffer bug (skia#12164), with a workaround available via GRContextOptions.AvoidStencilBuffers.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/Metal",
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "wrong-output",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "repoLinks": [
        {
          "url": "https://bugs.chromium.org/p/skia/issues/detail?id=12164",
          "description": "Upstream Skia bug for the stencil buffer clipping issue"
        },
        {
          "url": "https://github.com/AvaloniaUI/Avalonia/issues/6144",
          "description": "AvaloniaUI report demonstrating the same clipping bug"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The clipping bug is rooted in upstream Skia's handling of stencil buffers on Apple Silicon (Metal backend) and certain Nvidia 3000-series GPUs (OpenGL backend). The upstream Skia bug (skia#12164) was fixed in a later Skia milestone; until SkiaSharp upgrades to that milestone, the issue persists. A known workaround is setting GRContextOptions.AvoidStencilBuffers = true. Setting SKPaint.IsAntiAliased = false also resolves the visual artifact but degrades quality.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/GRContextOptions.cs",
        "finding": "GRContextOptions exposes AvoidStencilBuffers property (default false) which maps to the native GrContextOptions::fAvoidStencilBuffers field. Setting it to true disables stencil buffer use and avoids the GPU bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "Generated struct includes fAvoidStencilBuffers as a Byte field, confirming the binding correctly passes this option to the native Skia layer.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "This only happens on apple silicon gpus, and some nvidia 3000 series GPUs.",
        "source": "issue body",
        "interpretation": "Platform-specific GPU rendering issue, likely stencil buffer handling in Metal/OpenGL backends."
      },
      {
        "text": "It looks like the fix wasnt merged into skia until July so probably we cant fix this until m9x.",
        "source": "issue body",
        "interpretation": "Reporter acknowledges the fix is upstream and SkiaSharp must wait for a milestone bump."
      },
      {
        "text": "new GRContextOptions { AvoidStencilBuffers = true }",
        "source": "comment by danwalmsley",
        "interpretation": "Confirmed workaround that avoids the stencil buffer path entirely."
      }
    ],
    "rationale": "This is a type/bug in the native Skia layer (area/libSkiaSharp.native). The root cause is an upstream Skia GPU bug affecting stencil buffer clipping on Apple Silicon Metal and certain Nvidia OpenGL drivers. A usable workaround exists (AvoidStencilBuffers = true). The suggested action is close-as-external because the fix requires an upstream Skia milestone upgrade, not a SkiaSharp code change.",
    "workarounds": [
      "Set GRContextOptions.AvoidStencilBuffers = true when creating the GRContext to avoid the stencil buffer code path.",
      "Set SKPaint.IsAntiAliased = false to eliminate the clipping artifact at the cost of rendering quality."
    ],
    "resolution": {
      "hypothesis": "The bug is in upstream Skia's stencil buffer handling for Metal/OpenGL on affected GPU hardware. No SkiaSharp-side fix is possible until the upstream Skia milestone containing the fix (skia#12164) is adopted.",
      "proposals": [
        {
          "title": "Use AvoidStencilBuffers workaround",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "yes",
          "description": "Set GRContextOptions.AvoidStencilBuffers = true when creating the GPU context to bypass the stencil buffer path.",
          "codeSnippet": "var options = new GRContextOptions { AvoidStencilBuffers = true };\nvar grContext = GRContext.CreateGl(grInterface, options);"
        }
      ],
      "recommendedProposal": "Use AvoidStencilBuffers workaround",
      "recommendedReason": "Confirmed by reporter, low effort, immediately actionable."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-external",
      "confidence": 0.8,
      "reason": "Root cause is upstream Skia bug skia#12164 affecting stencil buffer clipping on Apple Silicon and certain Nvidia GPUs. Fix requires a Skia milestone upgrade. A documented workaround exists.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/macOS, backend/Metal, backend/OpenGL, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/macOS",
          "backend/Metal",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the upstream root cause, share workarounds, and note that a fix requires a Skia milestone bump.",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report!\n\nThis is caused by an upstream Skia bug ([skia#12164](https://bugs.chromium.org/p/skia/issues/detail?id=12164)) affecting stencil buffer clipping on Apple Silicon (Metal) and some Nvidia 3000-series GPUs (OpenGL). Since the fix lives in the upstream Skia library, SkiaSharp cannot patch this independently — it requires upgrading to the Skia milestone that includes the fix.\n\n**Workarounds available now:**\n\n1. **Disable stencil buffers** (recommended):\n   ```csharp\n   var options = new GRContextOptions { AvoidStencilBuffers = true };\n   var grContext = GRContext.CreateGl(grInterface, options);\n   ```\n\n2. **Disable antialiasing** (degrades quality):\n   ```csharp\n   paint.IsAntiAliased = false;\n   ```\n\nWe're tracking the upstream Skia milestone upgrade and will resolve this issue when the fix is incorporated."
      }
    ]
  }
}
```

</details>
