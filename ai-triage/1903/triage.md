# Issue Triage Report — #1903

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-02T19:22:13Z |
| Type | type/question (0.80 (80%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.80 (80%)) |

**Issue Summary:** User asks whether asyncRescaleAndReadPixels methods will be exposed in SkiaSharp, and whether there is a way to access the underlying OpenGL context for async PBO-based pixel readback.

**Analysis:** The user wants non-blocking GPU pixel readback. Skia exposes asyncRescaleAndReadPixels on SkImage and SkSurface, but SkiaSharp does not wrap these methods. The synchronous ReadPixels on SKImage is exposed. The user also asks about accessing the native GL context to use PBOs directly, which is not a supported SkiaSharp pattern. This is a two-part question: (1) feature gap in the binding (asyncRescaleAndReadPixels not exposed), and (2) workaround question about native GL access.

**Recommendations:** **close-as-not-a-bug** — Both questions can be answered: async readback is not bound (gap, not a bug) and a workaround exists. The issue does not describe broken behavior.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/OpenGL |
| Tenets | tenet/performance |
| Partner | — |

## Evidence

### Reproduction

**Related issues:** #1381

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/1381 — Related issue: SKSurface.ReadPixels is super slow — same performance concern about synchronous pixel readback

## Analysis

### Technical Summary

The user wants non-blocking GPU pixel readback. Skia exposes asyncRescaleAndReadPixels on SkImage and SkSurface, but SkiaSharp does not wrap these methods. The synchronous ReadPixels on SKImage is exposed. The user also asks about accessing the native GL context to use PBOs directly, which is not a supported SkiaSharp pattern. This is a two-part question: (1) feature gap in the binding (asyncRescaleAndReadPixels not exposed), and (2) workaround question about native GL access.

### Rationale

Issue title says [QUESTION] and both asks are phrased as questions rather than a formal feature request. The async readback API is a genuine gap in the SkiaSharp bindings, but the user is asking about it rather than requesting it. Workarounds (offload sync read to a background Task, or request the binding be added) can be provided. Closing as answered/not-a-bug is appropriate once the explanation is given.

### Key Signals

- "asyncRescaleAndReadPixels … seemingly not currently implemented in SkiaSharp" — **issue body** (User correctly identifies the gap — Skia has these APIs but SkiaSharp does not expose them.)
- "Is there any plan to implement these methods in SkiaSharp?" — **issue body** (Roadmap question — no public timeline exists.)
- "Is there anyway at the moment that I can get access to the underlying OpenGL Context and use the native ReadPixels into two Pixel Buffer objects" — **issue body** (Workaround question — SkiaSharp does not expose the raw GL handle, but the user could marshal their own GL calls alongside SkiaSharp if they manage the GL context themselves.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | — | direct | SKImage exposes multiple synchronous ReadPixels overloads (by SKImageInfo+IntPtr and by SKPixmap). No asyncRescaleAndReadPixels or equivalent is present anywhere in the binding. |
| `binding/SkiaSharp/SKSurface.cs` | — | direct | SKSurface.ReadPixels (SKImageInfo, IntPtr, int, int, int) is the only readback method on SKSurface — no async variant exists. |

### Workarounds

- Run SKImage.ReadPixels on a background thread via Task.Run to avoid stalling the render thread — the copy itself is synchronous but can be offloaded.
- If you control the GL context (e.g., using OpenTK or Silk.NET), you can call glReadPixels / glReadBuffer + PBO directly after SkiaSharp finishes rendering to the surface, since both share the same GL context.

### Next Questions

- Is the user creating the GRContext themselves (i.e., do they control the GL context)?
- Which platform/framework are they targeting? WASM, desktop, mobile?

### Resolution Proposals

**Hypothesis:** asyncRescaleAndReadPixels is not bound in SkiaSharp. The workaround is either running the synchronous read on a background thread or using native GL calls directly if the user owns the GL context.

1. **Offload synchronous ReadPixels to a background thread** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Call SKImage.ReadPixels inside Task.Run to avoid blocking the render thread. Data is still copied synchronously, but the render thread is freed.
2. **Use native GL PBOs directly alongside SkiaSharp** — workaround, confidence 0.70 (70%), cost/m, validated=untested
   - If you create and own the GL context (e.g., with OpenTK/Silk.NET), you can call glBindBuffer(GL_PIXEL_PACK_BUFFER, pbo) and glReadPixels after SkiaSharp flushes to the surface. SkiaSharp does not expose the raw context handle, but since both share the same native GL context, native GL calls interleave correctly.
3. **Add asyncRescaleAndReadPixels binding to SkiaSharp** — investigation, confidence 0.90 (90%), cost/l, validated=untested
   - File a feature-request issue specifically asking to expose SkImage::asyncRescaleAndReadPixels. This would require a new C API shim + C# binding.

**Recommended proposal:** Offload synchronous ReadPixels to a background thread

**Why:** Lowest friction workaround that works today without any binding changes or native interop.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.80 (80%) |
| Reason | Both questions can be answered: async readback is not bound (gap, not a bug) and a workaround exists. The issue does not describe broken behavior. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, SkiaSharp area, OpenGL backend, and performance tenet labels | labels=type/question, area/SkiaSharp, backend/OpenGL, tenet/performance |
| add-comment | medium | 0.80 (80%) | Answer the two questions with workarounds and suggest filing a feature request for async binding | — |
| close-issue | medium | 0.75 (75%) | Close as answered — both questions have responses and workarounds | stateReason=completed |
| link-related | low | 0.85 (85%) | Cross-reference related performance issue about slow ReadPixels | linkedIssue=#1381 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the question!

**1. asyncRescaleAndReadPixels in SkiaSharp**

You're correct — Skia's `SkImage::asyncRescaleAndReadPixels` (and the YUV420 variant) are not currently exposed in SkiaSharp. There is no public timeline for adding these. If this is important to you, please open a separate feature-request issue so it can be tracked and prioritised.

**2. Workaround: offloading the synchronous read**

The simplest workaround today is to run `SKImage.ReadPixels` on a background thread to avoid blocking the render thread:

```csharp
await Task.Run(() =>
{
    bool ok = image.ReadPixels(dstInfo, dstPixels, rowBytes, 0, 0);
});
```

**3. Workaround: native GL PBOs**

If you own the GL context (e.g., using OpenTK or Silk.NET), you can call `glReadPixels` + PBOs directly alongside SkiaSharp after calling `surface.Flush()`. SkiaSharp does not expose the raw GL handle, but native GL calls share the same underlying context.

Related performance discussion: #1381.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1903,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-02T19:22:13Z"
  },
  "summary": "User asks whether asyncRescaleAndReadPixels methods will be exposed in SkiaSharp, and whether there is a way to access the underlying OpenGL context for async PBO-based pixel readback.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.8
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/performance"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "relatedIssues": [
        1381
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1381",
          "description": "Related issue: SKSurface.ReadPixels is super slow — same performance concern about synchronous pixel readback"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The user wants non-blocking GPU pixel readback. Skia exposes asyncRescaleAndReadPixels on SkImage and SkSurface, but SkiaSharp does not wrap these methods. The synchronous ReadPixels on SKImage is exposed. The user also asks about accessing the native GL context to use PBOs directly, which is not a supported SkiaSharp pattern. This is a two-part question: (1) feature gap in the binding (asyncRescaleAndReadPixels not exposed), and (2) workaround question about native GL access.",
    "rationale": "Issue title says [QUESTION] and both asks are phrased as questions rather than a formal feature request. The async readback API is a genuine gap in the SkiaSharp bindings, but the user is asking about it rather than requesting it. Workarounds (offload sync read to a background Task, or request the binding be added) can be provided. Closing as answered/not-a-bug is appropriate once the explanation is given.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "finding": "SKImage exposes multiple synchronous ReadPixels overloads (by SKImageInfo+IntPtr and by SKPixmap). No asyncRescaleAndReadPixels or equivalent is present anywhere in the binding.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "finding": "SKSurface.ReadPixels (SKImageInfo, IntPtr, int, int, int) is the only readback method on SKSurface — no async variant exists.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "asyncRescaleAndReadPixels … seemingly not currently implemented in SkiaSharp",
        "source": "issue body",
        "interpretation": "User correctly identifies the gap — Skia has these APIs but SkiaSharp does not expose them."
      },
      {
        "text": "Is there any plan to implement these methods in SkiaSharp?",
        "source": "issue body",
        "interpretation": "Roadmap question — no public timeline exists."
      },
      {
        "text": "Is there anyway at the moment that I can get access to the underlying OpenGL Context and use the native ReadPixels into two Pixel Buffer objects",
        "source": "issue body",
        "interpretation": "Workaround question — SkiaSharp does not expose the raw GL handle, but the user could marshal their own GL calls alongside SkiaSharp if they manage the GL context themselves."
      }
    ],
    "workarounds": [
      "Run SKImage.ReadPixels on a background thread via Task.Run to avoid stalling the render thread — the copy itself is synchronous but can be offloaded.",
      "If you control the GL context (e.g., using OpenTK or Silk.NET), you can call glReadPixels / glReadBuffer + PBO directly after SkiaSharp finishes rendering to the surface, since both share the same GL context."
    ],
    "nextQuestions": [
      "Is the user creating the GRContext themselves (i.e., do they control the GL context)?",
      "Which platform/framework are they targeting? WASM, desktop, mobile?"
    ],
    "resolution": {
      "hypothesis": "asyncRescaleAndReadPixels is not bound in SkiaSharp. The workaround is either running the synchronous read on a background thread or using native GL calls directly if the user owns the GL context.",
      "proposals": [
        {
          "title": "Offload synchronous ReadPixels to a background thread",
          "description": "Call SKImage.ReadPixels inside Task.Run to avoid blocking the render thread. Data is still copied synchronously, but the render thread is freed.",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use native GL PBOs directly alongside SkiaSharp",
          "description": "If you create and own the GL context (e.g., with OpenTK/Silk.NET), you can call glBindBuffer(GL_PIXEL_PACK_BUFFER, pbo) and glReadPixels after SkiaSharp flushes to the surface. SkiaSharp does not expose the raw context handle, but since both share the same native GL context, native GL calls interleave correctly.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/m",
          "validated": "untested"
        },
        {
          "title": "Add asyncRescaleAndReadPixels binding to SkiaSharp",
          "description": "File a feature-request issue specifically asking to expose SkImage::asyncRescaleAndReadPixels. This would require a new C API shim + C# binding.",
          "category": "investigation",
          "confidence": 0.9,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Offload synchronous ReadPixels to a background thread",
      "recommendedReason": "Lowest friction workaround that works today without any binding changes or native interop."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.8,
      "reason": "Both questions can be answered: async readback is not bound (gap, not a bug) and a workaround exists. The issue does not describe broken behavior.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, SkiaSharp area, OpenGL backend, and performance tenet labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/SkiaSharp",
          "backend/OpenGL",
          "tenet/performance"
        ]
      },
      {
        "type": "add-comment",
        "description": "Answer the two questions with workarounds and suggest filing a feature request for async binding",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the question!\n\n**1. asyncRescaleAndReadPixels in SkiaSharp**\n\nYou're correct — Skia's `SkImage::asyncRescaleAndReadPixels` (and the YUV420 variant) are not currently exposed in SkiaSharp. There is no public timeline for adding these. If this is important to you, please open a separate feature-request issue so it can be tracked and prioritised.\n\n**2. Workaround: offloading the synchronous read**\n\nThe simplest workaround today is to run `SKImage.ReadPixels` on a background thread to avoid blocking the render thread:\n\n```csharp\nawait Task.Run(() =>\n{\n    bool ok = image.ReadPixels(dstInfo, dstPixels, rowBytes, 0, 0);\n});\n```\n\n**3. Workaround: native GL PBOs**\n\nIf you own the GL context (e.g., using OpenTK or Silk.NET), you can call `glReadPixels` + PBOs directly alongside SkiaSharp after calling `surface.Flush()`. SkiaSharp does not expose the raw GL handle, but native GL calls share the same underlying context.\n\nRelated performance discussion: #1381."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — both questions have responses and workarounds",
        "risk": "medium",
        "confidence": 0.75,
        "stateReason": "completed"
      },
      {
        "type": "link-related",
        "description": "Cross-reference related performance issue about slow ReadPixels",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 1381
      }
    ]
  }
}
```

</details>
