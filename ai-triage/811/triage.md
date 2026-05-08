# Issue Triage Report — #811

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T17:44:45Z |
| Type | type/enhancement (0.90 (90%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** Request to have SKPixmap detect when its pixel source (SKSurface) has been disposed and throw ObjectDisposedException instead of returning a dangling pointer that causes AccessViolationException.

**Analysis:** SKPixmap already holds a `pixelSource` field (set to the SKSurface by PeekPixels) as a GC-liveness reference, but this reference is never checked for disposal before pixel access. Adding a guard that checks `pixelSource?.IsDisposed == true` in GetPixels and related methods would convert silent AccessViolationException crashes into a catchable ObjectDisposedException.

**Recommendations:** **needs-investigation** — Well-specified enhancement with a clear implementation path. Code investigation confirms the mechanism (pixelSource + IsDisposed) is already in place. Needs a small code change and test to validate.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/enhancement |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/enhancement, area/SkiaSharp, type/feature-request |

## Evidence

### Reproduction

1. Create an SKSurface
2. Call PeekPixels() to obtain an SKPixmap
3. Dispose the SKSurface (e.g., via using block)
4. Call pixmap.GetPixels() on the now-dangling pixmap
5. Pass the returned IntPtr to Marshal.Copy or similar — observe AccessViolationException

**Environment:** Unity Editor, any platform. Large images (1000x1000) reproduce reliably; smaller (500x500) reproduce intermittently depending on GC timing.

**Repository links:**
- https://devdiv.visualstudio.com/DevDiv/_workitems/edit/820075 — VS internal tracking item referenced in issue body

## Analysis

### Technical Summary

SKPixmap already holds a `pixelSource` field (set to the SKSurface by PeekPixels) as a GC-liveness reference, but this reference is never checked for disposal before pixel access. Adding a guard that checks `pixelSource?.IsDisposed == true` in GetPixels and related methods would convert silent AccessViolationException crashes into a catchable ObjectDisposedException.

### Rationale

The request is an enhancement: the current behavior is technically correct (the pixmap points to the surface's pixel buffer), but there is no safety net when the user disposes the source surface prematurely. The `pixelSource` field and the `SKObject.IsDisposed` property both exist already, making this a low-risk, well-scoped improvement. Classified as type/enhancement rather than type/bug because the crash results from user error (disposing a live dependency), not from a bug in SkiaSharp itself.

### Key Signals

- "I ended up with an AccessViolationException sometimes when using .GetPixels after disposing the Surface via a using block." — **issue body** (Classic use-after-free pattern: pixmap retains a raw pointer to surface memory that is freed on Dispose.)
- "It would be nice if the SKPixmap class could detect this condition and throw an exception instead of returning a pointer to freed memory." — **issue body** (Reporter explicitly requests defensive validation, not a behavior change — clearly an enhancement.)
- "the AccessViolationException causes Unity Editor to hard crash. Not fun!" — **issue body** (Practical severity: AV in Unity is unrecoverable, making a catchable ObjectDisposedException a meaningful reliability improvement.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPixmap.cs` | 11 | direct | SKPixmap.pixelSource is set to the SKSurface in PeekPixels() and cleared on pixmap disposal, but it is never checked before pixel access — so calling GetPixels() after the source is disposed silently returns a freed pointer. |
| `binding/SkiaSharp/SKSurface.cs` | 300-309 | direct | SKSurface.PeekPixels() sets pixmap.pixelSource = this to prevent premature GC collection, but offers no disposal guard. Once the user explicitly calls surface.Dispose(), the reference remains (as a disposed object) but the native memory is freed. |
| `binding/SkiaSharp/SKObject.cs` | 242 | related | SKObject.IsDisposed is a protected internal property — accessible from SKPixmap to check whether pixelSource has been disposed. |

### Workarounds

- Ensure the SKSurface outlives all SKPixmap instances obtained from it via PeekPixels — do not dispose the surface inside a using block while still using the pixmap.
- Use SKSurface.ReadPixels() instead of PeekPixels()+GetPixels(): ReadPixels copies the data immediately and does not retain a pointer to the surface's backing memory.

### Next Questions

- Should the disposal guard apply to all SKPixmap pixel-access methods (GetPixels, GetPixelSpan, ReadPixels, Encode, etc.) or only GetPixels?
- Is there a way to also guard against the source being disposed on a different thread while the pixmap is being read?

### Resolution Proposals

**Hypothesis:** Add a disposal guard to SKPixmap that checks pixelSource.IsDisposed before any pixel access method and throws ObjectDisposedException.

1. **Add disposal guard in SKPixmap pixel access methods** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Check if pixelSource is non-null and disposed before returning pixel pointers. Throw ObjectDisposedException with a helpful message explaining that the source surface must remain alive.
2. **Document the lifetime requirement in XML docs** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Add XML documentation to PeekPixels() explaining that the caller must keep the SKSurface alive for as long as the returned SKPixmap is in use. Pair with the workaround of using ReadPixels() for a safe copy.

**Recommended proposal:** Add disposal guard in SKPixmap pixel access methods

**Why:** Converts an unrecoverable crash into a catchable exception with no behavioral change for correct usage. Low risk and well-scoped, leveraging existing infrastructure (pixelSource + IsDisposed).

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Well-specified enhancement with a clear implementation path. Code investigation confirms the mechanism (pixelSource + IsDisposed) is already in place. Needs a small code change and test to validate. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Correct labels: keep type/enhancement, area/SkiaSharp, add tenet/reliability | labels=type/enhancement, area/SkiaSharp, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Acknowledge enhancement request and provide workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for identifying the `pixelSource` hook!

This is a valid enhancement: `SKPixmap` already holds a reference to the source `SKSurface` via its internal `pixelSource` field, but doesn't currently check whether that source has been disposed before returning pixel pointers.

**Workaround until this is fixed:**

Use `SKSurface.ReadPixels()` instead of `PeekPixels()` + `GetPixels()` — `ReadPixels` copies the pixel data immediately and does not retain a pointer to the surface's backing memory:

```csharp
// Safe: copies pixels before surface is disposed
using var surface = SKSurface.Create(info);
var pixels = new byte[info.BytesSize];
GCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);
try {
    surface.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, 0, 0);
} finally {
    handle.Free();
}
```

Alternatively, make sure the `SKSurface` outlives any `SKPixmap` obtained from `PeekPixels()` — do not dispose the surface in a `using` block while still holding the pixmap reference.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 811,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T17:44:45Z",
    "currentLabels": [
      "type/enhancement",
      "area/SkiaSharp",
      "type/feature-request"
    ]
  },
  "summary": "Request to have SKPixmap detect when its pixel source (SKSurface) has been disposed and throw ObjectDisposedException instead of returning a dangling pointer that causes AccessViolationException.",
  "classification": {
    "type": {
      "value": "type/enhancement",
      "confidence": 0.9
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create an SKSurface",
        "Call PeekPixels() to obtain an SKPixmap",
        "Dispose the SKSurface (e.g., via using block)",
        "Call pixmap.GetPixels() on the now-dangling pixmap",
        "Pass the returned IntPtr to Marshal.Copy or similar — observe AccessViolationException"
      ],
      "environmentDetails": "Unity Editor, any platform. Large images (1000x1000) reproduce reliably; smaller (500x500) reproduce intermittently depending on GC timing.",
      "repoLinks": [
        {
          "url": "https://devdiv.visualstudio.com/DevDiv/_workitems/edit/820075",
          "description": "VS internal tracking item referenced in issue body"
        }
      ]
    }
  },
  "analysis": {
    "summary": "SKPixmap already holds a `pixelSource` field (set to the SKSurface by PeekPixels) as a GC-liveness reference, but this reference is never checked for disposal before pixel access. Adding a guard that checks `pixelSource?.IsDisposed == true` in GetPixels and related methods would convert silent AccessViolationException crashes into a catchable ObjectDisposedException.",
    "rationale": "The request is an enhancement: the current behavior is technically correct (the pixmap points to the surface's pixel buffer), but there is no safety net when the user disposes the source surface prematurely. The `pixelSource` field and the `SKObject.IsDisposed` property both exist already, making this a low-risk, well-scoped improvement. Classified as type/enhancement rather than type/bug because the crash results from user error (disposing a live dependency), not from a bug in SkiaSharp itself.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPixmap.cs",
        "lines": "11",
        "finding": "SKPixmap.pixelSource is set to the SKSurface in PeekPixels() and cleared on pixmap disposal, but it is never checked before pixel access — so calling GetPixels() after the source is disposed silently returns a freed pointer.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKSurface.cs",
        "lines": "300-309",
        "finding": "SKSurface.PeekPixels() sets pixmap.pixelSource = this to prevent premature GC collection, but offers no disposal guard. Once the user explicitly calls surface.Dispose(), the reference remains (as a disposed object) but the native memory is freed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "242",
        "finding": "SKObject.IsDisposed is a protected internal property — accessible from SKPixmap to check whether pixelSource has been disposed.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I ended up with an AccessViolationException sometimes when using .GetPixels after disposing the Surface via a using block.",
        "source": "issue body",
        "interpretation": "Classic use-after-free pattern: pixmap retains a raw pointer to surface memory that is freed on Dispose."
      },
      {
        "text": "It would be nice if the SKPixmap class could detect this condition and throw an exception instead of returning a pointer to freed memory.",
        "source": "issue body",
        "interpretation": "Reporter explicitly requests defensive validation, not a behavior change — clearly an enhancement."
      },
      {
        "text": "the AccessViolationException causes Unity Editor to hard crash. Not fun!",
        "source": "issue body",
        "interpretation": "Practical severity: AV in Unity is unrecoverable, making a catchable ObjectDisposedException a meaningful reliability improvement."
      }
    ],
    "workarounds": [
      "Ensure the SKSurface outlives all SKPixmap instances obtained from it via PeekPixels — do not dispose the surface inside a using block while still using the pixmap.",
      "Use SKSurface.ReadPixels() instead of PeekPixels()+GetPixels(): ReadPixels copies the data immediately and does not retain a pointer to the surface's backing memory."
    ],
    "nextQuestions": [
      "Should the disposal guard apply to all SKPixmap pixel-access methods (GetPixels, GetPixelSpan, ReadPixels, Encode, etc.) or only GetPixels?",
      "Is there a way to also guard against the source being disposed on a different thread while the pixmap is being read?"
    ],
    "resolution": {
      "hypothesis": "Add a disposal guard to SKPixmap that checks pixelSource.IsDisposed before any pixel access method and throws ObjectDisposedException.",
      "proposals": [
        {
          "title": "Add disposal guard in SKPixmap pixel access methods",
          "description": "Check if pixelSource is non-null and disposed before returning pixel pointers. Throw ObjectDisposedException with a helpful message explaining that the source surface must remain alive.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Document the lifetime requirement in XML docs",
          "description": "Add XML documentation to PeekPixels() explaining that the caller must keep the SKSurface alive for as long as the returned SKPixmap is in use. Pair with the workaround of using ReadPixels() for a safe copy.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Add disposal guard in SKPixmap pixel access methods",
      "recommendedReason": "Converts an unrecoverable crash into a catchable exception with no behavioral change for correct usage. Low risk and well-scoped, leveraging existing infrastructure (pixelSource + IsDisposed)."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Well-specified enhancement with a clear implementation path. Code investigation confirms the mechanism (pixelSource + IsDisposed) is already in place. Needs a small code change and test to validate.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: keep type/enhancement, area/SkiaSharp, add tenet/reliability",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/enhancement",
          "area/SkiaSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge enhancement request and provide workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report and for identifying the `pixelSource` hook!\n\nThis is a valid enhancement: `SKPixmap` already holds a reference to the source `SKSurface` via its internal `pixelSource` field, but doesn't currently check whether that source has been disposed before returning pixel pointers.\n\n**Workaround until this is fixed:**\n\nUse `SKSurface.ReadPixels()` instead of `PeekPixels()` + `GetPixels()` — `ReadPixels` copies the pixel data immediately and does not retain a pointer to the surface's backing memory:\n\n```csharp\n// Safe: copies pixels before surface is disposed\nusing var surface = SKSurface.Create(info);\nvar pixels = new byte[info.BytesSize];\nGCHandle handle = GCHandle.Alloc(pixels, GCHandleType.Pinned);\ntry {\n    surface.ReadPixels(info, handle.AddrOfPinnedObject(), info.RowBytes, 0, 0);\n} finally {\n    handle.Free();\n}\n```\n\nAlternatively, make sure the `SKSurface` outlives any `SKPixmap` obtained from `PeekPixels()` — do not dispose the surface in a `using` block while still holding the pixmap reference."
      }
    ]
  }
}
```

</details>
