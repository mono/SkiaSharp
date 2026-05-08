# Issue Triage Report — #2908

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T17:51:44Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.93 (93%)) |

**Issue Summary:** SEHException (E_FAIL) thrown when calling DrawPaint with an SKRuntimeEffect-created shader on a bitmap canvas in SkiaSharp 2.88.8 on Windows; duplicate of #2657 and fixed in v3.

**Analysis:** The reporter calls the deprecated SKRuntimeEffect.Create() API in SkiaSharp 2.88.8, then uses the returned shader to DrawPaint on a bitmap canvas, triggering an SEHException (E_FAIL). This is a known crash in the 2.x series when using SKSL runtime effects on raster/bitmap canvases on Windows. Community comment correctly identifies this as a duplicate of #2657, which was closed as completed. The fix is to upgrade to SkiaSharp v3 and use the renamed CreateShader() API.

**Recommendations:** **close-as-duplicate** — Community comment and code investigation confirm this is a duplicate of #2657 (SKRuntimeEffect crash on Bitmap Canvas). Issue #2657 was closed as completed (fixed in v3). The fix path is clear: upgrade to v3 and use CreateShader().

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | backend/Raster |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2657 — Canonical duplicate: SKRuntimeEffect doesn't work on Bitmap Canvas, closed as completed (fixed in v3)
- https://github.com/mono/SkiaSharp/issues/1822 — Earlier related issue referenced in #2657

**Code snippets:**

```csharp
SKRuntimeEffect runtimeEffect = SKRuntimeEffect.Create(sksl, out error);
SKShader shader = runtimeEffect.ToShader(true);
SKPaint paint = new SKPaint() { Shader = shader };
...
canvas.DrawPaint(paint); // SEHException thrown here
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | crash |
| Error message | SEHException: E_FAIL (0x80004005) when calling canvas.DrawPaint(paint) with an SKRuntimeEffect shader on bitmap canvas |
| Repro quality | complete |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | This issue is a known crash fixed in SkiaSharp v3. The reporter is using the deprecated SKRuntimeEffect.Create() API which no longer exists in v3; v3 uses CreateShader/CreateColorFilter/CreateBlender. |

## Analysis

### Technical Summary

The reporter calls the deprecated SKRuntimeEffect.Create() API in SkiaSharp 2.88.8, then uses the returned shader to DrawPaint on a bitmap canvas, triggering an SEHException (E_FAIL). This is a known crash in the 2.x series when using SKSL runtime effects on raster/bitmap canvases on Windows. Community comment correctly identifies this as a duplicate of #2657, which was closed as completed. The fix is to upgrade to SkiaSharp v3 and use the renamed CreateShader() API.

### Rationale

This is a confirmed duplicate of #2657 (SKRuntimeEffect crash on Bitmap Canvas on Windows). The crash exists in the 2.x series; the fix is upgrading to SkiaSharp v3 which renames the API to CreateShader() and fixes the underlying Skia crash. The reporter's sample also uses the old API signature, so the corrected API usage with v3 is the proper resolution.

### Key Signals

- "Duplicate #2657, try upgrading your SkiaSharp to v3-preview." — **Community comment by nomi-san** (A community member already identified this as a duplicate of the canonical issue and provided the fix.)
- "SEHException: E_FAIL (0x80004005) thrown at canvas.DrawPaint(paint)" — **Issue body** (Matches exactly the pattern seen in #2657 — SKRuntimeEffect on bitmap canvas crashes in v2.88.x on Windows.)
- "SKRuntimeEffect.Create(sksl, out error) / runtimeEffect.ToShader(true)" — **Reporter code** (Reporter is using the old v2 API. The v3 API is CreateShader() / ToShader() (no bool arg). In v2.88.x this combination triggered a crash on Windows raster surfaces.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKRuntimeEffect.cs` | [22, 53] | direct | The current codebase has CreateShader(), CreateColorFilter(), and CreateBlender() static methods, but no Create() method. The reporter is calling SKRuntimeEffect.Create() which is an old v2 API renamed in v3. In v3, ToShader() no longer takes a boolean argument — it takes optional uniforms/children. |
| `binding/SkiaSharp/SKRuntimeEffect.cs` | [22, 31] | direct | The Create() API that the reporter uses was removed/renamed as part of the v3 refactor. The new API SKRuntimeEffect.CreateShader(sksl, out errors) and .ToShader() without the boolean argument is the correct approach in v3. |

### Resolution Proposals

**Hypothesis:** The SEHException is caused by a known defect in SkiaSharp 2.88.x where SKRuntimeEffect shaders crash when used on raster bitmap canvases on Windows. The defect is fixed in v3.

1. **Upgrade to SkiaSharp v3 and use CreateShader()** — workaround, cost/xs, validated=yes
   - Upgrade the NuGet package to SkiaSharp v3 and replace SKRuntimeEffect.Create() with SKRuntimeEffect.CreateShader(), and ToShader(true) with ToShader().

```csharp
// v3 API
SKRuntimeEffect runtimeEffect = SKRuntimeEffect.CreateShader(sksl, out string error);
SKShader shader = runtimeEffect.ToShader();
SKPaint paint = new SKPaint() { Shader = shader };
```

**Recommended proposal:** proposal-1

**Why:** Upgrade is the canonical fix per the maintainer closing #2657 as completed in v3.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.93 (93%) |
| Reason | Community comment and code investigation confirm this is a duplicate of #2657 (SKRuntimeEffect crash on Bitmap Canvas). Issue #2657 was closed as completed (fixed in v3). The fix path is clear: upgrade to v3 and use CreateShader(). |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Add type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability |
| link-duplicate | medium | 0.93 (93%) | Mark as duplicate of #2657 | linkedIssue=#2657 |
| add-comment | medium | 0.93 (93%) | Post workaround pointing to v3 upgrade and the new CreateShader() API | — |

**Comment draft for `add-comment`:**

```markdown
Hi @martinrhan! This is a known issue in SkiaSharp 2.88.x — using `SKRuntimeEffect` shaders on bitmap/raster canvases crashes on Windows. It's been tracked and fixed in SkiaSharp v3.

This issue is a duplicate of #2657.

**Fix:** Upgrade to SkiaSharp v3 and use the updated API:

```csharp
// SkiaSharp v3 API
SKRuntimeEffect runtimeEffect = SKRuntimeEffect.CreateShader(sksl, out string error);
SKShader shader = runtimeEffect.ToShader();
SKPaint paint = new SKPaint() { Shader = shader };
```

Key changes from v2:
- `SKRuntimeEffect.Create()` → `SKRuntimeEffect.CreateShader()`
- `ToShader(bool)` → `ToShader()` (no boolean argument)
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2908,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T17:51:44Z"
  },
  "summary": "SEHException (E_FAIL) thrown when calling DrawPaint with an SKRuntimeEffect-created shader on a bitmap canvas in SkiaSharp 2.88.8 on Windows; duplicate of #2657 and fixed in v3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/Raster"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "SEHException: E_FAIL (0x80004005) when calling canvas.DrawPaint(paint) with an SKRuntimeEffect shader on bitmap canvas",
      "reproQuality": "complete",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "codeSnippets": [
        "SKRuntimeEffect runtimeEffect = SKRuntimeEffect.Create(sksl, out error);\nSKShader shader = runtimeEffect.ToShader(true);\nSKPaint paint = new SKPaint() { Shader = shader };\n...\ncanvas.DrawPaint(paint); // SEHException thrown here"
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2657",
          "description": "Canonical duplicate: SKRuntimeEffect doesn't work on Bitmap Canvas, closed as completed (fixed in v3)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/1822",
          "description": "Earlier related issue referenced in #2657"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "This issue is a known crash fixed in SkiaSharp v3. The reporter is using the deprecated SKRuntimeEffect.Create() API which no longer exists in v3; v3 uses CreateShader/CreateColorFilter/CreateBlender."
    }
  },
  "analysis": {
    "summary": "The reporter calls the deprecated SKRuntimeEffect.Create() API in SkiaSharp 2.88.8, then uses the returned shader to DrawPaint on a bitmap canvas, triggering an SEHException (E_FAIL). This is a known crash in the 2.x series when using SKSL runtime effects on raster/bitmap canvases on Windows. Community comment correctly identifies this as a duplicate of #2657, which was closed as completed. The fix is to upgrade to SkiaSharp v3 and use the renamed CreateShader() API.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "finding": "The current codebase has CreateShader(), CreateColorFilter(), and CreateBlender() static methods, but no Create() method. The reporter is calling SKRuntimeEffect.Create() which is an old v2 API renamed in v3. In v3, ToShader() no longer takes a boolean argument — it takes optional uniforms/children.",
        "relevance": "direct",
        "lines": "[22, 53]"
      },
      {
        "file": "binding/SkiaSharp/SKRuntimeEffect.cs",
        "finding": "The Create() API that the reporter uses was removed/renamed as part of the v3 refactor. The new API SKRuntimeEffect.CreateShader(sksl, out errors) and .ToShader() without the boolean argument is the correct approach in v3.",
        "relevance": "direct",
        "lines": "[22, 31]"
      }
    ],
    "keySignals": [
      {
        "text": "Duplicate #2657, try upgrading your SkiaSharp to v3-preview.",
        "source": "Community comment by nomi-san",
        "interpretation": "A community member already identified this as a duplicate of the canonical issue and provided the fix."
      },
      {
        "text": "SEHException: E_FAIL (0x80004005) thrown at canvas.DrawPaint(paint)",
        "source": "Issue body",
        "interpretation": "Matches exactly the pattern seen in #2657 — SKRuntimeEffect on bitmap canvas crashes in v2.88.x on Windows."
      },
      {
        "text": "SKRuntimeEffect.Create(sksl, out error) / runtimeEffect.ToShader(true)",
        "source": "Reporter code",
        "interpretation": "Reporter is using the old v2 API. The v3 API is CreateShader() / ToShader() (no bool arg). In v2.88.x this combination triggered a crash on Windows raster surfaces."
      }
    ],
    "rationale": "This is a confirmed duplicate of #2657 (SKRuntimeEffect crash on Bitmap Canvas on Windows). The crash exists in the 2.x series; the fix is upgrading to SkiaSharp v3 which renames the API to CreateShader() and fixes the underlying Skia crash. The reporter's sample also uses the old API signature, so the corrected API usage with v3 is the proper resolution.",
    "resolution": {
      "hypothesis": "The SEHException is caused by a known defect in SkiaSharp 2.88.x where SKRuntimeEffect shaders crash when used on raster bitmap canvases on Windows. The defect is fixed in v3.",
      "proposals": [
        {
          "title": "Upgrade to SkiaSharp v3 and use CreateShader()",
          "category": "workaround",
          "description": "Upgrade the NuGet package to SkiaSharp v3 and replace SKRuntimeEffect.Create() with SKRuntimeEffect.CreateShader(), and ToShader(true) with ToShader().",
          "codeSnippet": "// v3 API\nSKRuntimeEffect runtimeEffect = SKRuntimeEffect.CreateShader(sksl, out string error);\nSKShader shader = runtimeEffect.ToShader();\nSKPaint paint = new SKPaint() { Shader = shader };",
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "proposal-1",
      "recommendedReason": "Upgrade is the canonical fix per the maintainer closing #2657 as completed in v3."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.93,
      "reason": "Community comment and code investigation confirm this is a duplicate of #2657 (SKRuntimeEffect crash on Bitmap Canvas). Issue #2657 was closed as completed (fixed in v3). The fix path is clear: upgrade to v3 and use CreateShader().",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Add type/bug, area/SkiaSharp, os/Windows-Classic, backend/Raster, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "backend/Raster",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #2657",
        "risk": "medium",
        "confidence": 0.93,
        "linkedIssue": 2657
      },
      {
        "type": "add-comment",
        "description": "Post workaround pointing to v3 upgrade and the new CreateShader() API",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "Hi @martinrhan! This is a known issue in SkiaSharp 2.88.x — using `SKRuntimeEffect` shaders on bitmap/raster canvases crashes on Windows. It's been tracked and fixed in SkiaSharp v3.\n\nThis issue is a duplicate of #2657.\n\n**Fix:** Upgrade to SkiaSharp v3 and use the updated API:\n\n```csharp\n// SkiaSharp v3 API\nSKRuntimeEffect runtimeEffect = SKRuntimeEffect.CreateShader(sksl, out string error);\nSKShader shader = runtimeEffect.ToShader();\nSKPaint paint = new SKPaint() { Shader = shader };\n```\n\nKey changes from v2:\n- `SKRuntimeEffect.Create()` → `SKRuntimeEffect.CreateShader()`\n- `ToShader(bool)` → `ToShader()` (no boolean argument)"
      }
    ]
  }
}
```

</details>
