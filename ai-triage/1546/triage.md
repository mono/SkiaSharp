# Issue Triage Report — #1546

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T17:40:30Z |
| Type | type/feature-request (0.88 (88%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | keep-open (0.82 (82%)) |

**Issue Summary:** Reporter requests `SkSurface::getTextureHandle` (get underlying OpenGL texture handle from a GPU surface) be exposed in SkiaSharp, and asks for a working sample for creating a surface on top of an existing `GRBackendTexture`.

**Analysis:** The `SkSurface::getBackendTexture()` / `getTextureHandle()` API is not exposed in SkiaSharp. There is an explicit `// TODO: GetTextureHandle` comment in `SKImage.cs` acknowledging the gap. A workaround exists: create the surface on top of a user-owned `GRBackendTexture`, so the caller already knows the texture ID.

**Recommendations:** **keep-open** — Valid feature request with a TODO already tracked in SKImage.cs. A workaround exists but the missing `GetBackendTexture` binding is a legitimate gap for OpenGL interop use cases.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/feature-request |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | backend/OpenGL |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Create a GPU-backed SKSurface using GRContext and a GRBackendRenderTarget
2. Attempt to retrieve the underlying OpenGL texture handle from the surface
3. No such API exists in SkiaSharp (no GetTextureHandle or GetBackendTexture on SKSurface)

**Environment:** OpenGL integration scenario, platform unspecified

## Analysis

### Technical Summary

The `SkSurface::getBackendTexture()` / `getTextureHandle()` API is not exposed in SkiaSharp. There is an explicit `// TODO: GetTextureHandle` comment in `SKImage.cs` acknowledging the gap. A workaround exists: create the surface on top of a user-owned `GRBackendTexture`, so the caller already knows the texture ID.

### Rationale

This is a feature request because the `getTextureHandle` / `GetBackendTexture` method is explicitly missing from SkiaSharp bindings, as confirmed by the TODO in SKImage.cs. The secondary question about GRBackendTexture null return is likely a usage issue (mismatched color space/format), addressable with documentation. The core ask — retrieving the GL texture handle from an SKSurface — is a valid OpenGL interop use case not yet covered by the bindings.

### Key Signals

- "i need the method `SkSurface::getTextureHandle`" — **issue body** (Reporter wants to retrieve the native GL texture ID from a GPU-backed surface for use in their own OpenGL rendering pipeline.)
- "a (working) sample how to use `GRBackendTexture` would be a acceptable alternative" — **issue body** (Reporter would accept a usage example of the reverse workflow: wrapping an existing texture as a surface instead.)
- "When calling `SKSurface.Create(...)` i always get a null object" — **issue body** (Reporter is also struggling with the SKSurface.Create overload that takes a GRBackendTexture — likely a parameter mismatch (color space, color type, or texture format).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 13 | direct | Explicit TODO comment `// TODO: GetTextureHandle` confirms the API is known to be missing from the bindings. |
| `binding/SkiaSharp/GRBackendTexture.cs` | 87-95 | related | `GRBackendTexture.GetGlTextureInfo()` exists and returns `GRGlTextureInfo` (with `Id`, `Target`, `Format`) — so the GL texture ID IS retrievable from a GRBackendTexture, but there is no way to obtain a GRBackendTexture from an existing SKSurface. |
| `binding/SkiaSharp/SKSurface.cs` | 123-176 | related | `SKSurface.Create(GRContext, GRBackendTexture, ...)` overloads exist to create a surface WRAPPING a user-provided backend texture. However there is no inverse method to retrieve the backend texture FROM an existing surface. |

### Workarounds

- Create the surface by wrapping a user-owned texture: `var backendTex = new GRBackendTexture(w, h, false, new GRGlTextureInfo { Id = myTexId, Target = GL_TEXTURE_2D, Format = GL_RGBA8 }); var surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft, 0, SKColorType.Rgba8888);` — the caller already knows `myTexId`.
- Alternatively, snapshot to an SKImage and use `SKImage.FromAdoptedTexture` or inspect the GRBackendTexture used to build the surface.

### Next Questions

- Is the reporter trying to read BACK the texture from a GRContext-allocated surface, or from one they created themselves?
- What Skia version / SkiaSharp version is being used? `getTextureHandle` was renamed to `getBackendTexture` in newer Skia.

### Resolution Proposals

**Hypothesis:** Expose `SKSurface.GetBackendTexture(bool flushPendingGrContextIO)` wrapping `SkSurface::getBackendTexture()`. As a near-term workaround, users should own the GRBackendTexture themselves.

1. **Use user-owned GRBackendTexture for two-way interop** — workaround, confidence 0.85 (85%), cost/xs, validated=untested
   - Create the surface from a user-created GL texture so the texture ID is already known. Call Flush() before using the texture in external GL code.

```csharp
// Create a GL texture manually
int texId = GL.GenTexture();
GL.BindTexture(TextureTarget.Texture2D, texId);
GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,
    PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

// Wrap it in SkiaSharp
var texInfo = new GRGlTextureInfo
{
    Id = (uint)texId,
    Target = (uint)TextureTarget.Texture2D,
    Format = (uint)SizedInternalFormat.Rgba8
};
var backendTex = new GRBackendTexture(width, height, false, texInfo);
var surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft, 0,
    SKColorType.Rgba8888, SKColorSpace.CreateSrgb());

// Draw into surface...
grContext.Flush();
// Now use texId in your own OpenGL code
```
2. **Expose SKSurface.GetBackendTexture()** — fix, confidence 0.80 (80%), cost/m, validated=untested
   - Bind `SkSurface::getBackendTexture()` in the C API and expose as `GRBackendTexture SKSurface.GetBackendTexture(bool flushPendingGrContextIO)` in C#. Callers can then call `GetGlTextureInfo()` on the returned object.

**Recommended proposal:** Use user-owned GRBackendTexture for two-way interop

**Why:** Requires no code changes. Callers that need GL texture interop should own the texture themselves — this is the standard OpenGL interop pattern.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.82 (82%) |
| Reason | Valid feature request with a TODO already tracked in SKImage.cs. A workaround exists but the missing `GetBackendTexture` binding is a legitimate gap for OpenGL interop use cases. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply feature-request, SkiaSharp core, and OpenGL backend labels | labels=type/feature-request, area/SkiaSharp, backend/OpenGL |
| add-comment | medium | 0.82 (82%) | Provide workaround using user-owned GRBackendTexture and explain why SKSurface.Create returns null | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The `SkSurface::getBackendTexture()` method isn't currently exposed in SkiaSharp (there's an internal TODO tracking this).

**Workaround — own the texture yourself:**

For GL interop, the recommended pattern is to create the GL texture yourself and then wrap it, so you already know the texture ID:

```csharp
// Create GL texture
int texId = GL.GenTexture();
GL.BindTexture(TextureTarget.Texture2D, texId);
GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,
    width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);

// Wrap in SkiaSharp
var texInfo = new GRGlTextureInfo
{
    Id = (uint)texId,
    Target = (uint)TextureTarget.Texture2D,
    Format = (uint)SizedInternalFormat.Rgba8  // must match TexImage2D format
};
var backendTex = new GRBackendTexture(width, height, false, texInfo);
var surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft,
    0, SKColorType.Rgba8888, SKColorSpace.CreateSrgb());

// Draw into surface, then flush before using in external GL code
grContext.Flush();
// texId is ready to use in your own GL pipeline
```

**Why does `SKSurface.Create(...)` return null?** The most common cause is a mismatch between the `GRGlTextureInfo.Format` (sized internal format, e.g. `GL_RGBA8`) and the `SKColorType` passed to `Create`. Make sure you use `SizedInternalFormat.Rgba8` (not `PixelInternalFormat.Rgba`) and `SKColorType.Rgba8888`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1546,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T17:40:30Z"
  },
  "summary": "Reporter requests `SkSurface::getTextureHandle` (get underlying OpenGL texture handle from a GPU surface) be exposed in SkiaSharp, and asks for a working sample for creating a surface on top of an existing `GRBackendTexture`.",
  "classification": {
    "type": {
      "value": "type/feature-request",
      "confidence": 0.88
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
    },
    "backends": [
      "backend/OpenGL"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a GPU-backed SKSurface using GRContext and a GRBackendRenderTarget",
        "Attempt to retrieve the underlying OpenGL texture handle from the surface",
        "No such API exists in SkiaSharp (no GetTextureHandle or GetBackendTexture on SKSurface)"
      ],
      "environmentDetails": "OpenGL integration scenario, platform unspecified",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "The `SkSurface::getBackendTexture()` / `getTextureHandle()` API is not exposed in SkiaSharp. There is an explicit `// TODO: GetTextureHandle` comment in `SKImage.cs` acknowledging the gap. A workaround exists: create the surface on top of a user-owned `GRBackendTexture`, so the caller already knows the texture ID.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "13",
        "finding": "Explicit TODO comment `// TODO: GetTextureHandle` confirms the API is known to be missing from the bindings.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRBackendTexture.cs",
        "lines": "87-95",
        "finding": "`GRBackendTexture.GetGlTextureInfo()` exists and returns `GRGlTextureInfo` (with `Id`, `Target`, `Format`) — so the GL texture ID IS retrievable from a GRBackendTexture, but there is no way to obtain a GRBackendTexture from an existing SKSurface.",
        "relevance": "related"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "123-176",
        "finding": "`SKSurface.Create(GRContext, GRBackendTexture, ...)` overloads exist to create a surface WRAPPING a user-provided backend texture. However there is no inverse method to retrieve the backend texture FROM an existing surface.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "i need the method `SkSurface::getTextureHandle`",
        "source": "issue body",
        "interpretation": "Reporter wants to retrieve the native GL texture ID from a GPU-backed surface for use in their own OpenGL rendering pipeline."
      },
      {
        "text": "a (working) sample how to use `GRBackendTexture` would be a acceptable alternative",
        "source": "issue body",
        "interpretation": "Reporter would accept a usage example of the reverse workflow: wrapping an existing texture as a surface instead."
      },
      {
        "text": "When calling `SKSurface.Create(...)` i always get a null object",
        "source": "issue body",
        "interpretation": "Reporter is also struggling with the SKSurface.Create overload that takes a GRBackendTexture — likely a parameter mismatch (color space, color type, or texture format)."
      }
    ],
    "rationale": "This is a feature request because the `getTextureHandle` / `GetBackendTexture` method is explicitly missing from SkiaSharp bindings, as confirmed by the TODO in SKImage.cs. The secondary question about GRBackendTexture null return is likely a usage issue (mismatched color space/format), addressable with documentation. The core ask — retrieving the GL texture handle from an SKSurface — is a valid OpenGL interop use case not yet covered by the bindings.",
    "workarounds": [
      "Create the surface by wrapping a user-owned texture: `var backendTex = new GRBackendTexture(w, h, false, new GRGlTextureInfo { Id = myTexId, Target = GL_TEXTURE_2D, Format = GL_RGBA8 }); var surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft, 0, SKColorType.Rgba8888);` — the caller already knows `myTexId`.",
      "Alternatively, snapshot to an SKImage and use `SKImage.FromAdoptedTexture` or inspect the GRBackendTexture used to build the surface."
    ],
    "nextQuestions": [
      "Is the reporter trying to read BACK the texture from a GRContext-allocated surface, or from one they created themselves?",
      "What Skia version / SkiaSharp version is being used? `getTextureHandle` was renamed to `getBackendTexture` in newer Skia."
    ],
    "resolution": {
      "hypothesis": "Expose `SKSurface.GetBackendTexture(bool flushPendingGrContextIO)` wrapping `SkSurface::getBackendTexture()`. As a near-term workaround, users should own the GRBackendTexture themselves.",
      "proposals": [
        {
          "title": "Use user-owned GRBackendTexture for two-way interop",
          "description": "Create the surface from a user-created GL texture so the texture ID is already known. Call Flush() before using the texture in external GL code.",
          "category": "workaround",
          "codeSnippet": "// Create a GL texture manually\nint texId = GL.GenTexture();\nGL.BindTexture(TextureTarget.Texture2D, texId);\nGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, width, height, 0,\n    PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);\n\n// Wrap it in SkiaSharp\nvar texInfo = new GRGlTextureInfo\n{\n    Id = (uint)texId,\n    Target = (uint)TextureTarget.Texture2D,\n    Format = (uint)SizedInternalFormat.Rgba8\n};\nvar backendTex = new GRBackendTexture(width, height, false, texInfo);\nvar surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft, 0,\n    SKColorType.Rgba8888, SKColorSpace.CreateSrgb());\n\n// Draw into surface...\ngrContext.Flush();\n// Now use texId in your own OpenGL code",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Expose SKSurface.GetBackendTexture()",
          "description": "Bind `SkSurface::getBackendTexture()` in the C API and expose as `GRBackendTexture SKSurface.GetBackendTexture(bool flushPendingGrContextIO)` in C#. Callers can then call `GetGlTextureInfo()` on the returned object.",
          "category": "fix",
          "confidence": 0.8,
          "effort": "cost/m",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use user-owned GRBackendTexture for two-way interop",
      "recommendedReason": "Requires no code changes. Callers that need GL texture interop should own the texture themselves — this is the standard OpenGL interop pattern."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.82,
      "reason": "Valid feature request with a TODO already tracked in SKImage.cs. A workaround exists but the missing `GetBackendTexture` binding is a legitimate gap for OpenGL interop use cases.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply feature-request, SkiaSharp core, and OpenGL backend labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/feature-request",
          "area/SkiaSharp",
          "backend/OpenGL"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide workaround using user-owned GRBackendTexture and explain why SKSurface.Create returns null",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the report! The `SkSurface::getBackendTexture()` method isn't currently exposed in SkiaSharp (there's an internal TODO tracking this).\n\n**Workaround — own the texture yourself:**\n\nFor GL interop, the recommended pattern is to create the GL texture yourself and then wrap it, so you already know the texture ID:\n\n```csharp\n// Create GL texture\nint texId = GL.GenTexture();\nGL.BindTexture(TextureTarget.Texture2D, texId);\nGL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8,\n    width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);\n\n// Wrap in SkiaSharp\nvar texInfo = new GRGlTextureInfo\n{\n    Id = (uint)texId,\n    Target = (uint)TextureTarget.Texture2D,\n    Format = (uint)SizedInternalFormat.Rgba8  // must match TexImage2D format\n};\nvar backendTex = new GRBackendTexture(width, height, false, texInfo);\nvar surface = SKSurface.Create(grContext, backendTex, GRSurfaceOrigin.TopLeft,\n    0, SKColorType.Rgba8888, SKColorSpace.CreateSrgb());\n\n// Draw into surface, then flush before using in external GL code\ngrContext.Flush();\n// texId is ready to use in your own GL pipeline\n```\n\n**Why does `SKSurface.Create(...)` return null?** The most common cause is a mismatch between the `GRGlTextureInfo.Format` (sized internal format, e.g. `GL_RGBA8`) and the `SKColorType` passed to `Create`. Make sure you use `SizedInternalFormat.Rgba8` (not `PixelInternalFormat.Rgba`) and `SKColorType.Rgba8888`."
      }
    ]
  }
}
```

</details>
