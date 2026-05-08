# Issue Triage Report — #1164

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T18:19:25Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.92 (92%)) |
| Suggested action | needs-investigation (0.88 (88%)) |

**Issue Summary:** SKImage.ToTextureImage causes a native SIGSEGV crash on macOS when called outside of the GPU draw context (e.g., from a button click handler) because OpenGL/Metal operations require the view's rendering context to be current.

**Analysis:** When SKImage.ToTextureImage is called outside of the GPU rendering context (e.g., from a button-click handler rather than the PaintSurface callback), the native Skia code attempts to invoke OpenGL functions (glGenTextures) without a current GL context, triggering a SIGSEGV. The C# wrapper in SKImage.cs (lines 572-578) validates only that context != null but does not check whether the GRContext is currently active or whether the calling thread has a bound GL context. The maintainer confirmed this root cause and provided a workaround. A fix should either throw a managed exception when the context is invalid, or document the constraint clearly.

**Recommendations:** **needs-investigation** — Root cause is well-understood (no GL context bound when ToTextureImage is called outside the render callback) and confirmed by a maintainer. A workaround exists. Needs investigation to design and implement the proper fix (managed guard or native improvement).

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/macOS |
| Backends | backend/OpenGL |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/nor0x/SkiaSharp_PerfomanceExperiments/tree/master/Xamarin.Forms — Reproduction repository
- https://github.com/mono/SkiaSharp/issues/1984 — Related issue: same crash with Metal backend on macOS
- https://github.com/mono/SkiaSharp/issues/1188 — Related closed issue: macOS native crashes in gr_backendrendertarget_get_gl_framebufferinfo

**Code snippets:**

```csharp
var im = SKImage.FromEncodedData(_buffer.ElementAt(index)).ToTextureImage(SkiaView.GRContext);
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Got a SIGSEGV while executing native code |
| Repro quality | complete |
| Target frameworks | — |

**Stack trace:**

```text
glGenTextures -> gr_backendrendertarget_get_gl_framebufferinfo -> sk_image_make_texture_image
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.2-preview.29 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | ToTextureImage still exists in current codebase with no null context guard or thread/context check; related issue #1984 filed in 2022 shows the same crash with the Metal backend. |

## Analysis

### Technical Summary

When SKImage.ToTextureImage is called outside of the GPU rendering context (e.g., from a button-click handler rather than the PaintSurface callback), the native Skia code attempts to invoke OpenGL functions (glGenTextures) without a current GL context, triggering a SIGSEGV. The C# wrapper in SKImage.cs (lines 572-578) validates only that context != null but does not check whether the GRContext is currently active or whether the calling thread has a bound GL context. The maintainer confirmed this root cause and provided a workaround. A fix should either throw a managed exception when the context is invalid, or document the constraint clearly.

### Rationale

Classified as type/bug because a well-formed API call causes a native crash instead of returning null or throwing a managed exception. Platform is macOS (OpenGL backend exhibited here; Metal also affected per #1984). Severity is high because it is a hard crash with no managed fallback, though it requires incorrect usage (calling outside the draw callback). A workaround exists (call inside PaintSurface). Not closing as external because SkiaSharp should guard against this rather than crash.

### Key Signals

- "Got a SIGSEGV while executing native code ... glGenTextures" — **issue body** (GL function called without active OpenGL context)
- "the crash is because macOS doesn't do the correct checks that it should ... you have to make sure that you have set the view to be in control" — **maintainer comment #642686538** (Root cause confirmed: no GL context bound at the call site)
- "Obviously, we should look into the crash part and see if we can either throw a native exception or return null" — **maintainer comment #642686538** (Maintainer acknowledges a fix is needed in SkiaSharp itself)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKImage.cs` | 572-578 | direct | ToTextureImage only checks context != null before calling the native sk_image_make_texture_image. There is no validation that the GPU context is current or that the GL context is bound on the calling thread. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | — | direct | sk_image_make_texture_image is a direct P/Invoke to the native library; no error handling wrapper around the call. A null return would be surfaced via GetObject returning null, but a SIGSEGV in native code bypasses all managed exception handling. |

### Workarounds

- Call ToTextureImage inside the SKGLView/SKMetalView PaintSurface handler where the GPU context is guaranteed to be current, not in event handlers like button clicks.
- Check img.Image.IsTextureBacked before calling ToTextureImage; perform the conversion lazily inside the draw callback: if (!img.Image.IsTextureBacked) img.Image = img.Image.ToTextureImage(context);

### Next Questions

- Can we add a GRContext.IsCurrent property or check in the C# layer to guard ToTextureImage and throw a descriptive InvalidOperationException instead of crashing?
- Does the same crash reproduce on the latest stable build (2.88.x / 3.x)?
- Is a similar guard needed for other GRContext-dependent APIs?

### Resolution Proposals

**Hypothesis:** Add a guard in SKImage.ToTextureImage (or in the native C API) that returns null / throws a managed exception when the GPU context is not current, rather than crashing with SIGSEGV.

1. **Queue texture upload to next draw callback** — workaround, cost/xs, validated=yes
   - Store images as CPU SKImage objects; inside PaintSurface, lazily convert to texture using ToTextureImage before drawing. This ensures the GL/Metal context is always current.

```csharp
// In button click / load handler:
var cpuImage = SKImage.FromEncodedData(buffer);

// In PaintSurface / Draw handler:
if (!cpuImage.IsTextureBacked)
    cpuImage = cpuImage.ToTextureImage(grContext);
canvas.DrawImage(cpuImage, destination, paint);
```
2. **Add managed guard to ToTextureImage** — fix, cost/s, validated=untested
   - Check context.IsValid or equivalent before calling native sk_image_make_texture_image; throw InvalidOperationException with a descriptive message if the context is not current.

**Recommended proposal:** Queue texture upload to next draw callback

**Why:** Immediate workaround, no code changes required, confirmed by maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.88 (88%) |
| Reason | Root cause is well-understood (no GL context bound when ToTextureImage is called outside the render callback) and confirmed by a maintainer. A workaround exists. Needs investigation to design and implement the proper fix (managed guard or native improvement). |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type, area, platform, backend, tenet labels | labels=type/bug, area/SkiaSharp, os/macOS, backend/OpenGL, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Share root cause analysis and workaround with the reporter | — |
| link-related | low | 0.95 (95%) | Cross-reference related issue #1984 (same crash, Metal backend) | linkedIssue=#1984 |
| link-related | low | 0.85 (85%) | Cross-reference closed related issue #1188 | linkedIssue=#1188 |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! The crash occurs because `ToTextureImage` internally calls OpenGL/Metal functions that require the GPU context to be **current on the calling thread**. When invoked from a button click (outside of the `PaintSurface`/`Draw` callback), the GL context is not bound, causing the native SIGSEGV.

**Workaround (confirmed working):**

Defer the GPU upload to the draw callback where the context is always current:

```csharp
// Load/decode as CPU image (safe to call from anywhere)
var cpuImage = SKImage.FromEncodedData(buffer);

// Inside PaintSurface / Draw handler – context is current here
if (!cpuImage.IsTextureBacked)
    cpuImage = cpuImage.ToTextureImage(grContext);
canvas.DrawImage(cpuImage, destination, paint);
```

We're tracking a fix to make SkiaSharp throw a descriptive managed exception (or return `null`) instead of crashing when the context is not current. Related: #1984.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1164,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T18:19:25Z"
  },
  "summary": "SKImage.ToTextureImage causes a native SIGSEGV crash on macOS when called outside of the GPU draw context (e.g., from a button click handler) because OpenGL/Metal operations require the view's rendering context to be current.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.92
    },
    "platforms": [
      "os/macOS"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "Got a SIGSEGV while executing native code",
      "stackTrace": "glGenTextures -> gr_backendrendertarget_get_gl_framebufferinfo -> sk_image_make_texture_image",
      "reproQuality": "complete"
    },
    "reproEvidence": {
      "codeSnippets": [
        "var im = SKImage.FromEncodedData(_buffer.ElementAt(index)).ToTextureImage(SkiaView.GRContext);"
      ],
      "attachments": [],
      "repoLinks": [
        {
          "url": "https://github.com/nor0x/SkiaSharp_PerfomanceExperiments/tree/master/Xamarin.Forms",
          "description": "Reproduction repository"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1984",
          "description": "Related issue: same crash with Metal backend on macOS"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1188",
          "description": "Related closed issue: macOS native crashes in gr_backendrendertarget_get_gl_framebufferinfo"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.2-preview.29"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "ToTextureImage still exists in current codebase with no null context guard or thread/context check; related issue #1984 filed in 2022 shows the same crash with the Metal backend."
    }
  },
  "analysis": {
    "summary": "When SKImage.ToTextureImage is called outside of the GPU rendering context (e.g., from a button-click handler rather than the PaintSurface callback), the native Skia code attempts to invoke OpenGL functions (glGenTextures) without a current GL context, triggering a SIGSEGV. The C# wrapper in SKImage.cs (lines 572-578) validates only that context != null but does not check whether the GRContext is currently active or whether the calling thread has a bound GL context. The maintainer confirmed this root cause and provided a workaround. A fix should either throw a managed exception when the context is invalid, or document the constraint clearly.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "572-578",
        "finding": "ToTextureImage only checks context != null before calling the native sk_image_make_texture_image. There is no validation that the GPU context is current or that the GL context is bound on the calling thread.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "finding": "sk_image_make_texture_image is a direct P/Invoke to the native library; no error handling wrapper around the call. A null return would be surfaced via GetObject returning null, but a SIGSEGV in native code bypasses all managed exception handling.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Got a SIGSEGV while executing native code ... glGenTextures",
        "source": "issue body",
        "interpretation": "GL function called without active OpenGL context"
      },
      {
        "text": "the crash is because macOS doesn't do the correct checks that it should ... you have to make sure that you have set the view to be in control",
        "source": "maintainer comment #642686538",
        "interpretation": "Root cause confirmed: no GL context bound at the call site"
      },
      {
        "text": "Obviously, we should look into the crash part and see if we can either throw a native exception or return null",
        "source": "maintainer comment #642686538",
        "interpretation": "Maintainer acknowledges a fix is needed in SkiaSharp itself"
      }
    ],
    "rationale": "Classified as type/bug because a well-formed API call causes a native crash instead of returning null or throwing a managed exception. Platform is macOS (OpenGL backend exhibited here; Metal also affected per #1984). Severity is high because it is a hard crash with no managed fallback, though it requires incorrect usage (calling outside the draw callback). A workaround exists (call inside PaintSurface). Not closing as external because SkiaSharp should guard against this rather than crash.",
    "workarounds": [
      "Call ToTextureImage inside the SKGLView/SKMetalView PaintSurface handler where the GPU context is guaranteed to be current, not in event handlers like button clicks.",
      "Check img.Image.IsTextureBacked before calling ToTextureImage; perform the conversion lazily inside the draw callback: if (!img.Image.IsTextureBacked) img.Image = img.Image.ToTextureImage(context);"
    ],
    "nextQuestions": [
      "Can we add a GRContext.IsCurrent property or check in the C# layer to guard ToTextureImage and throw a descriptive InvalidOperationException instead of crashing?",
      "Does the same crash reproduce on the latest stable build (2.88.x / 3.x)?",
      "Is a similar guard needed for other GRContext-dependent APIs?"
    ],
    "resolution": {
      "hypothesis": "Add a guard in SKImage.ToTextureImage (or in the native C API) that returns null / throws a managed exception when the GPU context is not current, rather than crashing with SIGSEGV.",
      "proposals": [
        {
          "title": "Queue texture upload to next draw callback",
          "category": "workaround",
          "description": "Store images as CPU SKImage objects; inside PaintSurface, lazily convert to texture using ToTextureImage before drawing. This ensures the GL/Metal context is always current.",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "// In button click / load handler:\nvar cpuImage = SKImage.FromEncodedData(buffer);\n\n// In PaintSurface / Draw handler:\nif (!cpuImage.IsTextureBacked)\n    cpuImage = cpuImage.ToTextureImage(grContext);\ncanvas.DrawImage(cpuImage, destination, paint);"
        },
        {
          "title": "Add managed guard to ToTextureImage",
          "category": "fix",
          "description": "Check context.IsValid or equivalent before calling native sk_image_make_texture_image; throw InvalidOperationException with a descriptive message if the context is not current.",
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Queue texture upload to next draw callback",
      "recommendedReason": "Immediate workaround, no code changes required, confirmed by maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.88,
      "reason": "Root cause is well-understood (no GL context bound when ToTextureImage is called outside the render callback) and confirmed by a maintainer. A workaround exists. Needs investigation to design and implement the proper fix (managed guard or native improvement).",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type, area, platform, backend, tenet labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/macOS",
          "backend/OpenGL",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share root cause analysis and workaround with the reporter",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for reporting this! The crash occurs because `ToTextureImage` internally calls OpenGL/Metal functions that require the GPU context to be **current on the calling thread**. When invoked from a button click (outside of the `PaintSurface`/`Draw` callback), the GL context is not bound, causing the native SIGSEGV.\n\n**Workaround (confirmed working):**\n\nDefer the GPU upload to the draw callback where the context is always current:\n\n```csharp\n// Load/decode as CPU image (safe to call from anywhere)\nvar cpuImage = SKImage.FromEncodedData(buffer);\n\n// Inside PaintSurface / Draw handler – context is current here\nif (!cpuImage.IsTextureBacked)\n    cpuImage = cpuImage.ToTextureImage(grContext);\ncanvas.DrawImage(cpuImage, destination, paint);\n```\n\nWe're tracking a fix to make SkiaSharp throw a descriptive managed exception (or return `null`) instead of crashing when the context is not current. Related: #1984."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issue #1984 (same crash, Metal backend)",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 1984
      },
      {
        "type": "link-related",
        "description": "Cross-reference closed related issue #1188",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 1188
      }
    ]
  }
}
```

</details>
