# Issue Triage Report — #3472

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T18:33:00Z |
| Type | type/bug (0.98 (98%)) |
| Area | area/HarfBuzzSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** HarfBuzzSharp.Blob.FromStream uses a managed byte array pinned with `fixed`, but the pin is released when the method returns while the native blob retains the pointer — GC can then move the array, causing memory corruption or crashes.

**Analysis:** The FromStream method pins a managed byte array with `fixed`, passes the pointer to `hb_blob_create`, then returns the Blob — but the `fixed` pin is scoped to the block and is released when the method exits. The GC is then free to relocate the managed array, leaving the native HarfBuzz blob with an invalid pointer. This is a latent memory-safety bug that manifests non-deterministically whenever GC runs after Blob creation. The fix is to allocate unmanaged memory (Marshal.AllocCoTaskMem or GCHandle.Alloc Pinned) and free it in the release callback.

**Recommendations:** **ready-to-fix** — Root cause is confirmed by code inspection, fix is clear and low-effort, workaround exists for users. Related issue #2323 has been open since 2022 without a fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/HarfBuzzSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/reliability |

## Evidence

### Reproduction

1. Call HarfBuzzSharp.Blob.FromStream(stream) with any stream
2. Trigger a GC collection after the Blob is created
3. Use the Blob (e.g., create a Face from it) — observe corrupted glyph info or AccessViolationException

**Environment:** Visual Studio on Windows, SkiaSharp 3.116.0, all platforms affected

**Related issues:** #2323

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2323 — Prior report of the same GC-safety issue in Blob.FromStream — confirmed with AccessViolationException

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | GC moves managed array after fixed block exits, native HarfBuzz blob holds stale pointer |
| Repro quality | partial |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The FromStream method code is unchanged between 2.88.x and 3.116.x — the bug has always been present. The 'last known good' version may reflect inconsistent GC timing rather than a real code change. |

## Analysis

### Technical Summary

The FromStream method pins a managed byte array with `fixed`, passes the pointer to `hb_blob_create`, then returns the Blob — but the `fixed` pin is scoped to the block and is released when the method exits. The GC is then free to relocate the managed array, leaving the native HarfBuzz blob with an invalid pointer. This is a latent memory-safety bug that manifests non-deterministically whenever GC runs after Blob creation. The fix is to allocate unmanaged memory (Marshal.AllocCoTaskMem or GCHandle.Alloc Pinned) and free it in the release callback.

### Rationale

The bug is confirmed by direct code inspection (binding/HarfBuzzSharp/Blob.cs line 79): a `fixed` pointer is passed to native code that outlives the `fixed` scope. This is a well-known anti-pattern documented in CLAUDE.md and confirmed by related issue #2323 where a commenter reports actual AccessViolationException. The fix is straightforward and well-understood.

### Key Signals

- "fixed (byte* dataPtr = data) { return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ()); }" — **binding/HarfBuzzSharp/Blob.cs lines 79-81** (The fixed pin is released the instant this method returns, but the Blob holds the pointer for its entire lifetime — use-after-unpin.)
- "Can confirm this is actually a problem. At best all codepoints in Buffer.GlyphInfos becomes 0, at worst AccessViolationException will be thrown." — **issue #2323 comment by TJYSunset** (Independently confirmed — real crashes and silent data corruption in the wild.)
- "This should also improve performance since it removes the redundant copy" — **issue #3472 comment by jeremy-visionaid** (Reporter notes the fix also removes a copy, improving performance.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/Blob.cs` | 71-82 | direct | FromStream pins managed array with `fixed`, passes the pointer to hb_blob_create via Blob constructor, then returns while the fixed scope ends — the native blob retains the pointer after the GC pin is released. |
| `binding/HarfBuzzSharp/Blob.cs` | 84-89 | direct | The Create helper calls DelegateProxies.Create for the release callback and passes it to hb_blob_create — the callback fires when HarfBuzz destroys the blob. The release callback disposes MemoryStream, not the byte array, so the array is never freed or unpinned by the callback. |
| `binding/SkiaSharp/SKImage.cs` | 30 | related | SKImage uses Marshal.AllocCoTaskMem for a similar pattern of passing pointer to native code — the correct approach already used elsewhere in the codebase. |

### Workarounds

- Manually allocate unmanaged memory with Marshal.AllocHGlobal or Marshal.AllocCoTaskMem, copy data into it, then construct Blob directly with: var ptr = Marshal.AllocCoTaskMem(data.Length); Marshal.Copy(data, 0, ptr, data.Length); var blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));
- Pin the managed array for the Blob lifetime using GCHandle: var gch = GCHandle.Alloc(data, GCHandleType.Pinned); var blob = new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());

### Next Questions

- Does the same GC-safety issue affect any other FromStream or similar methods in HarfBuzzSharp or SkiaSharp that pass fixed pointers to native objects outliving the fixed scope?
- The reporter's suggested fix uses stream.Length which fails for non-seekable streams — should the fix use MemoryStream+copy or GCHandle.Alloc approach instead?

### Resolution Proposals

**Hypothesis:** Replace the `fixed` pin with unmanaged memory allocation so the pointer remains valid for the entire Blob lifetime, or use GCHandle.Alloc(Pinned) and free it in the release callback.

1. **Use Marshal.AllocCoTaskMem for unmanaged copy** — fix, confidence 0.92 (92%), cost/xs, validated=yes
   - Replace fixed block with Marshal.AllocCoTaskMem, copy bytes into unmanaged memory, pass to Blob, free in release callback. This is safe for non-seekable streams and matches the pattern used in SKImage.cs.

```csharp
public static unsafe Blob FromStream(Stream stream)
{
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    var data = ms.ToArray();
    var ptr = Marshal.AllocCoTaskMem(data.Length);
    Marshal.Copy(data, 0, ptr, data.Length);
    return new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));
}
```
2. **Use GCHandle.Alloc(Pinned) to pin managed array** — fix, confidence 0.88 (88%), cost/xs, validated=yes
   - Allocate a GCHandle to pin the managed array for the Blob's lifetime and free it in the release callback. Avoids extra copy but keeps managed memory alive longer.

```csharp
public static unsafe Blob FromStream(Stream stream)
{
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    var data = ms.ToArray();
    var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
    return new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());
}
```

**Recommended proposal:** Use Marshal.AllocCoTaskMem for unmanaged copy

**Why:** Matches the existing pattern in SKImage.cs, avoids long-term GC heap fragmentation from pinned managed arrays, and works correctly for all stream types including non-seekable ones.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | Root cause is confirmed by code inspection, fix is clear and low-effort, workaround exists for users. Related issue #2323 has been open since 2022 without a fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Correct labels: fix area to HarfBuzzSharp, remove incorrect os/Windows-Classic (cross-platform bug), keep type/bug and tenet/reliability | labels=type/bug, area/HarfBuzzSharp, tenet/reliability |
| link-related | low | 0.95 (95%) | Cross-reference prior report of the same bug | linkedIssue=#2323 |
| add-comment | medium | 0.92 (92%) | Confirm the bug, provide workaround, and indicate the fix path | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed analysis! This is a confirmed GC-safety bug — the `fixed` pin in `FromStream` is scoped to the block and is released when the method returns, but HarfBuzz retains the pointer for the blob's lifetime. Once unpinned, the GC can relocate the managed array, corrupting the blob data (also reported in #2323).

**Workaround (until fixed):** Allocate unmanaged memory yourself and construct `Blob` directly:
```csharp
using var ms = new MemoryStream();
stream.CopyTo(ms);
var data = ms.ToArray();
var ptr = Marshal.AllocCoTaskMem(data.Length);
Marshal.Copy(data, 0, ptr, data.Length);
var blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));
```

The fix in `FromStream` should use `Marshal.AllocCoTaskMem` + copy instead of the `fixed` block, matching the existing pattern in `SKImage.cs`. We'll also update the TODO comment about avoiding the second copy.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3472,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T18:33:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/reliability"
    ]
  },
  "summary": "HarfBuzzSharp.Blob.FromStream uses a managed byte array pinned with `fixed`, but the pin is released when the method returns while the native blob retains the pointer — GC can then move the array, causing memory corruption or crashes.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.98
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "crash",
      "errorMessage": "GC moves managed array after fixed block exits, native HarfBuzz blob holds stale pointer",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Call HarfBuzzSharp.Blob.FromStream(stream) with any stream",
        "Trigger a GC collection after the Blob is created",
        "Use the Blob (e.g., create a Face from it) — observe corrupted glyph info or AccessViolationException"
      ],
      "environmentDetails": "Visual Studio on Windows, SkiaSharp 3.116.0, all platforms affected",
      "relatedIssues": [
        2323
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2323",
          "description": "Prior report of the same GC-safety issue in Blob.FromStream — confirmed with AccessViolationException"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The FromStream method code is unchanged between 2.88.x and 3.116.x — the bug has always been present. The 'last known good' version may reflect inconsistent GC timing rather than a real code change."
    }
  },
  "analysis": {
    "summary": "The FromStream method pins a managed byte array with `fixed`, passes the pointer to `hb_blob_create`, then returns the Blob — but the `fixed` pin is scoped to the block and is released when the method exits. The GC is then free to relocate the managed array, leaving the native HarfBuzz blob with an invalid pointer. This is a latent memory-safety bug that manifests non-deterministically whenever GC runs after Blob creation. The fix is to allocate unmanaged memory (Marshal.AllocCoTaskMem or GCHandle.Alloc Pinned) and free it in the release callback.",
    "rationale": "The bug is confirmed by direct code inspection (binding/HarfBuzzSharp/Blob.cs line 79): a `fixed` pointer is passed to native code that outlives the `fixed` scope. This is a well-known anti-pattern documented in CLAUDE.md and confirmed by related issue #2323 where a commenter reports actual AccessViolationException. The fix is straightforward and well-understood.",
    "keySignals": [
      {
        "text": "fixed (byte* dataPtr = data) { return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ()); }",
        "source": "binding/HarfBuzzSharp/Blob.cs lines 79-81",
        "interpretation": "The fixed pin is released the instant this method returns, but the Blob holds the pointer for its entire lifetime — use-after-unpin."
      },
      {
        "text": "Can confirm this is actually a problem. At best all codepoints in Buffer.GlyphInfos becomes 0, at worst AccessViolationException will be thrown.",
        "source": "issue #2323 comment by TJYSunset",
        "interpretation": "Independently confirmed — real crashes and silent data corruption in the wild."
      },
      {
        "text": "This should also improve performance since it removes the redundant copy",
        "source": "issue #3472 comment by jeremy-visionaid",
        "interpretation": "Reporter notes the fix also removes a copy, improving performance."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "71-82",
        "finding": "FromStream pins managed array with `fixed`, passes the pointer to hb_blob_create via Blob constructor, then returns while the fixed scope ends — the native blob retains the pointer after the GC pin is released.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "84-89",
        "finding": "The Create helper calls DelegateProxies.Create for the release callback and passes it to hb_blob_create — the callback fires when HarfBuzz destroys the blob. The release callback disposes MemoryStream, not the byte array, so the array is never freed or unpinned by the callback.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKImage.cs",
        "lines": "30",
        "finding": "SKImage uses Marshal.AllocCoTaskMem for a similar pattern of passing pointer to native code — the correct approach already used elsewhere in the codebase.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Manually allocate unmanaged memory with Marshal.AllocHGlobal or Marshal.AllocCoTaskMem, copy data into it, then construct Blob directly with: var ptr = Marshal.AllocCoTaskMem(data.Length); Marshal.Copy(data, 0, ptr, data.Length); var blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));",
      "Pin the managed array for the Blob lifetime using GCHandle: var gch = GCHandle.Alloc(data, GCHandleType.Pinned); var blob = new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());"
    ],
    "nextQuestions": [
      "Does the same GC-safety issue affect any other FromStream or similar methods in HarfBuzzSharp or SkiaSharp that pass fixed pointers to native objects outliving the fixed scope?",
      "The reporter's suggested fix uses stream.Length which fails for non-seekable streams — should the fix use MemoryStream+copy or GCHandle.Alloc approach instead?"
    ],
    "resolution": {
      "hypothesis": "Replace the `fixed` pin with unmanaged memory allocation so the pointer remains valid for the entire Blob lifetime, or use GCHandle.Alloc(Pinned) and free it in the release callback.",
      "proposals": [
        {
          "title": "Use Marshal.AllocCoTaskMem for unmanaged copy",
          "description": "Replace fixed block with Marshal.AllocCoTaskMem, copy bytes into unmanaged memory, pass to Blob, free in release callback. This is safe for non-seekable streams and matches the pattern used in SKImage.cs.",
          "category": "fix",
          "codeSnippet": "public static unsafe Blob FromStream(Stream stream)\n{\n    using var ms = new MemoryStream();\n    stream.CopyTo(ms);\n    var data = ms.ToArray();\n    var ptr = Marshal.AllocCoTaskMem(data.Length);\n    Marshal.Copy(data, 0, ptr, data.Length);\n    return new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));\n}",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use GCHandle.Alloc(Pinned) to pin managed array",
          "description": "Allocate a GCHandle to pin the managed array for the Blob's lifetime and free it in the release callback. Avoids extra copy but keeps managed memory alive longer.",
          "category": "fix",
          "codeSnippet": "public static unsafe Blob FromStream(Stream stream)\n{\n    using var ms = new MemoryStream();\n    stream.CopyTo(ms);\n    var data = ms.ToArray();\n    var gch = GCHandle.Alloc(data, GCHandleType.Pinned);\n    return new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());\n}",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Use Marshal.AllocCoTaskMem for unmanaged copy",
      "recommendedReason": "Matches the existing pattern in SKImage.cs, avoids long-term GC heap fragmentation from pinned managed arrays, and works correctly for all stream types including non-seekable ones."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "Root cause is confirmed by code inspection, fix is clear and low-effort, workaround exists for users. Related issue #2323 has been open since 2022 without a fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: fix area to HarfBuzzSharp, remove incorrect os/Windows-Classic (cross-platform bug), keep type/bug and tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/HarfBuzzSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "link-related",
        "description": "Cross-reference prior report of the same bug",
        "risk": "low",
        "confidence": 0.95,
        "linkedIssue": 2323
      },
      {
        "type": "add-comment",
        "description": "Confirm the bug, provide workaround, and indicate the fix path",
        "risk": "medium",
        "confidence": 0.92,
        "comment": "Thanks for the detailed analysis! This is a confirmed GC-safety bug — the `fixed` pin in `FromStream` is scoped to the block and is released when the method returns, but HarfBuzz retains the pointer for the blob's lifetime. Once unpinned, the GC can relocate the managed array, corrupting the blob data (also reported in #2323).\n\n**Workaround (until fixed):** Allocate unmanaged memory yourself and construct `Blob` directly:\n```csharp\nusing var ms = new MemoryStream();\nstream.CopyTo(ms);\nvar data = ms.ToArray();\nvar ptr = Marshal.AllocCoTaskMem(data.Length);\nMarshal.Copy(data, 0, ptr, data.Length);\nvar blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeCoTaskMem(ptr));\n```\n\nThe fix in `FromStream` should use `Marshal.AllocCoTaskMem` + copy instead of the `fixed` block, matching the existing pattern in `SKImage.cs`. We'll also update the TODO comment about avoiding the second copy."
      }
    ]
  }
}
```

</details>
