# Issue Triage Report — #3378

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-30T13:50:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp.HarfBuzz (0.95 (95%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** Memory leak in BlobExtensions.ToHarfBuzzBlob() — when GetMemoryBase() returns zero, the SKStreamAsset is read into unmanaged memory but never disposed, causing a leak every time SKShaper is constructed on affected platforms.

**Analysis:** In BlobExtensions.ToHarfBuzzBlob() (source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs), when asset.GetMemoryBase() returns IntPtr.Zero, the code allocates unmanaged memory, copies the asset data via asset.Read(), and creates a Blob with a destructor that frees the unmanaged pointer — but the asset itself is never disposed in this branch. In the non-zero path (line 24) the blob destructor does call asset.Dispose(), so that path is correct. The SKShaper constructor (SKShaper.cs line 20) calls Typeface.OpenStream(...).ToHarfBuzzBlob() and this pattern leaks the SKStreamAsset on any platform where GetMemoryBase returns zero.

**Recommendations:** **ready-to-fix** — Root cause is confirmed in BlobExtensions.cs else branch. Fix is obvious and low-risk.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp.HarfBuzz |
| Platforms | os/Android, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

**Code snippets:**

```csharp
class TextSegmentShaper : IDisposable { ... Shaper = new SKShaper(typeface); ... }
```

```csharp
BlobExtensions.ToHarfBuzzBlob: else branch reads asset but never disposes it
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | memory-leak |
| Error message | — |
| Repro quality | partial |
| Target frameworks | — |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The else branch in BlobExtensions.ToHarfBuzzBlob still does not dispose the asset in current code. |

## Analysis

### Technical Summary

In BlobExtensions.ToHarfBuzzBlob() (source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs), when asset.GetMemoryBase() returns IntPtr.Zero, the code allocates unmanaged memory, copies the asset data via asset.Read(), and creates a Blob with a destructor that frees the unmanaged pointer — but the asset itself is never disposed in this branch. In the non-zero path (line 24) the blob destructor does call asset.Dispose(), so that path is correct. The SKShaper constructor (SKShaper.cs line 20) calls Typeface.OpenStream(...).ToHarfBuzzBlob() and this pattern leaks the SKStreamAsset on any platform where GetMemoryBase returns zero.

### Rationale

The bug is confirmed by reading BlobExtensions.cs: the else branch on line 28-30 does not dispose the asset. The title mentions 'SKShader' but the actual leak is in SKShaper via ToHarfBuzzBlob(). Area is SkiaSharp.HarfBuzz (not SkiaSharp). Backend/Metal label from prior triage is incorrect — this is a memory management bug unrelated to any rendering backend. The fix is straightforward: dispose the asset after reading its data in the else branch.

### Key Signals

- "I think the bug is in ToHarfBuzzBlob() extension - it doesn't dispose SKStreamAsset in all paths." — **issue body** (Reporter correctly identifies the root cause in BlobExtensions.)
- "First observed bug on Android, but seems to be a problem also on Windows." — **issue body** (Cross-platform — occurs wherever GetMemoryBase returns zero.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs` | 27-30 | direct | In the else branch (GetMemoryBase returns IntPtr.Zero), asset.Read() is called and Marshal.AllocCoTaskMem memory is used, but asset.Dispose() is never called. The Blob destructor only frees the unmanaged ptr, not the asset. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs` | 23-24 | direct | In the if branch (GetMemoryBase returns non-zero), the Blob destructor lambda calls asset.Dispose() correctly. |
| `source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs` | 20 | related | SKShaper constructor calls Typeface.OpenStream(out index).ToHarfBuzzBlob(), which is the call site that triggers the leak whenever the else branch is taken. |

### Resolution Proposals

**Hypothesis:** Add asset.Dispose() after asset.Read() in the else branch of ToHarfBuzzBlob(), or wrap it in a using statement before creating the Blob.

1. **Dispose asset in else branch of ToHarfBuzzBlob** — fix, cost/xs, validated=yes
   - In BlobExtensions.ToHarfBuzzBlob, after reading the asset data in the else branch, dispose the asset immediately since the data has been copied to unmanaged memory.

```csharp
var ptr = Marshal.AllocCoTaskMem(size);
asset.Read(ptr, size);
asset.Dispose(); // fix: dispose after reading
blob = new Blob(ptr, size, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));
```

**Recommended proposal:** fix-1

**Why:** One-line fix that mirrors the intent of the if-branch (asset disposed when no longer needed), fully resolves the leak.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is confirmed in BlobExtensions.cs else branch. Fix is obvious and low-risk. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Update labels: correct area to area/SkiaSharp.HarfBuzz, keep type/bug and tenet/reliability | labels=type/bug, area/SkiaSharp.HarfBuzz, os/Android, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.92 (92%) | Acknowledge the confirmed bug with the fix location | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear bug report! The root cause is confirmed in `BlobExtensions.ToHarfBuzzBlob()` ([BlobExtensions.cs, lines 28-30](https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs#L28-L30)): when `GetMemoryBase()` returns `IntPtr.Zero`, the asset data is copied to unmanaged memory via `Marshal.AllocCoTaskMem` but the `SKStreamAsset` itself is never disposed. The fix is to call `asset.Dispose()` after `asset.Read(ptr, size)` in that else branch.

Note: the title says "SKShader" but the issue is in `SKShaper` → `ToHarfBuzzBlob()`.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3378,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-30T13:50:00Z"
  },
  "summary": "Memory leak in BlobExtensions.ToHarfBuzzBlob() — when GetMemoryBase() returns zero, the SKStreamAsset is read into unmanaged memory but never disposed, causing a leak every time SKShaper is constructed on affected platforms.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp.HarfBuzz",
      "confidence": 0.95
    },
    "platforms": [
      "os/Android",
      "os/Windows-Classic"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "memory-leak",
      "reproQuality": "partial"
    },
    "reproEvidence": {
      "codeSnippets": [
        "class TextSegmentShaper : IDisposable { ... Shaper = new SKShaper(typeface); ... }",
        "BlobExtensions.ToHarfBuzzBlob: else branch reads asset but never disposes it"
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The else branch in BlobExtensions.ToHarfBuzzBlob still does not dispose the asset in current code."
    }
  },
  "analysis": {
    "summary": "In BlobExtensions.ToHarfBuzzBlob() (source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs), when asset.GetMemoryBase() returns IntPtr.Zero, the code allocates unmanaged memory, copies the asset data via asset.Read(), and creates a Blob with a destructor that frees the unmanaged pointer — but the asset itself is never disposed in this branch. In the non-zero path (line 24) the blob destructor does call asset.Dispose(), so that path is correct. The SKShaper constructor (SKShaper.cs line 20) calls Typeface.OpenStream(...).ToHarfBuzzBlob() and this pattern leaks the SKStreamAsset on any platform where GetMemoryBase returns zero.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs",
        "lines": "27-30",
        "finding": "In the else branch (GetMemoryBase returns IntPtr.Zero), asset.Read() is called and Marshal.AllocCoTaskMem memory is used, but asset.Dispose() is never called. The Blob destructor only frees the unmanaged ptr, not the asset.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs",
        "lines": "23-24",
        "finding": "In the if branch (GetMemoryBase returns non-zero), the Blob destructor lambda calls asset.Dispose() correctly.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/SKShaper.cs",
        "lines": "20",
        "finding": "SKShaper constructor calls Typeface.OpenStream(out index).ToHarfBuzzBlob(), which is the call site that triggers the leak whenever the else branch is taken.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "I think the bug is in ToHarfBuzzBlob() extension - it doesn't dispose SKStreamAsset in all paths.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the root cause in BlobExtensions."
      },
      {
        "text": "First observed bug on Android, but seems to be a problem also on Windows.",
        "source": "issue body",
        "interpretation": "Cross-platform — occurs wherever GetMemoryBase returns zero."
      }
    ],
    "rationale": "The bug is confirmed by reading BlobExtensions.cs: the else branch on line 28-30 does not dispose the asset. The title mentions 'SKShader' but the actual leak is in SKShaper via ToHarfBuzzBlob(). Area is SkiaSharp.HarfBuzz (not SkiaSharp). Backend/Metal label from prior triage is incorrect — this is a memory management bug unrelated to any rendering backend. The fix is straightforward: dispose the asset after reading its data in the else branch.",
    "resolution": {
      "hypothesis": "Add asset.Dispose() after asset.Read() in the else branch of ToHarfBuzzBlob(), or wrap it in a using statement before creating the Blob.",
      "proposals": [
        {
          "title": "Dispose asset in else branch of ToHarfBuzzBlob",
          "description": "In BlobExtensions.ToHarfBuzzBlob, after reading the asset data in the else branch, dispose the asset immediately since the data has been copied to unmanaged memory.",
          "category": "fix",
          "effort": "cost/xs",
          "validated": "yes",
          "codeSnippet": "var ptr = Marshal.AllocCoTaskMem(size);\nasset.Read(ptr, size);\nasset.Dispose(); // fix: dispose after reading\nblob = new Blob(ptr, size, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));"
        }
      ],
      "recommendedProposal": "fix-1",
      "recommendedReason": "One-line fix that mirrors the intent of the if-branch (asset disposed when no longer needed), fully resolves the leak."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is confirmed in BlobExtensions.cs else branch. Fix is obvious and low-risk.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Update labels: correct area to area/SkiaSharp.HarfBuzz, keep type/bug and tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/SkiaSharp.HarfBuzz",
          "os/Android",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the confirmed bug with the fix location",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the clear bug report! The root cause is confirmed in `BlobExtensions.ToHarfBuzzBlob()` ([BlobExtensions.cs, lines 28-30](https://github.com/mono/SkiaSharp/blob/main/source/SkiaSharp.HarfBuzz/SkiaSharp.HarfBuzz/BlobExtensions.cs#L28-L30)): when `GetMemoryBase()` returns `IntPtr.Zero`, the asset data is copied to unmanaged memory via `Marshal.AllocCoTaskMem` but the `SKStreamAsset` itself is never disposed. The fix is to call `asset.Dispose()` after `asset.Read(ptr, size)` in that else branch.\n\nNote: the title says \"SKShader\" but the issue is in `SKShaper` → `ToHarfBuzzBlob()`."
      }
    ]
  }
}
```

</details>
