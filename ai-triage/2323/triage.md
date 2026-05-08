# Issue Triage Report — #2323

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T17:00:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/HarfBuzzSharp (0.98 (98%)) |
| Suggested action | ready-to-fix (0.95 (95%)) |

**Issue Summary:** HarfBuzzSharp.Blob.FromStream passes a fixed pointer to native HarfBuzz after the fixed block exits, leaving the byte array unpinned and allowing the GC to relocate it, causing data corruption or AccessViolationException.

**Analysis:** FromStream pins a byte array with fixed to obtain a pointer, passes that pointer to the native hb_blob_create, then exits the fixed block — at which point the byte array is no longer pinned. The GC is free to relocate the array, invalidating the pointer held by native HarfBuzz. A community commenter confirmed AccessViolationException and corrupted GlyphInfos. The fix is to use GCHandle.Alloc(data, GCHandleType.Pinned) and free it in the release delegate.

**Recommendations:** **ready-to-fix** — Root cause is clear (fixed pointer escapes fixed block), fix path is well-understood (GCHandle.Pinned or AllocHGlobal), and a community commenter already confirmed the bug.

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

## Evidence

### Reproduction

1. Create a HarfBuzzSharp Blob using Blob.FromStream(stream)
2. Use the Blob to shape text via a Face and Font
3. Observe that Buffer.GlyphInfos codepoints are all 0, or an AccessViolationException is thrown

**Environment:** Cross-platform; affects all runtimes where GC can move heap memory

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | AccessViolationException; or all codepoints in Buffer.GlyphInfos become 0 |
| Repro quality | partial |
| Target frameworks | — |

## Analysis

### Technical Summary

FromStream pins a byte array with fixed to obtain a pointer, passes that pointer to the native hb_blob_create, then exits the fixed block — at which point the byte array is no longer pinned. The GC is free to relocate the array, invalidating the pointer held by native HarfBuzz. A community commenter confirmed AccessViolationException and corrupted GlyphInfos. The fix is to use GCHandle.Alloc(data, GCHandleType.Pinned) and free it in the release delegate.

### Rationale

Although filed as a question, a community comment confirmed the bug reproduces as AccessViolationException and corrupted glyph data. The code clearly passes a fixed pointer to native memory that outlives the fixed block — a classic unsafe memory management mistake documented in CLAUDE.md anti-patterns.

### Key Signals

- "fixed (byte* dataPtr = data) { return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ()); }" — **binding/HarfBuzzSharp/Blob.cs lines 79-81** (The fixed block ends immediately after the constructor returns, un-pinning data while native HarfBuzz still holds the pointer.)
- "Can confirm this is actually a problem. At best all codepoints in Buffer.GlyphInfos becomes 0, at worst AccessViolationException will be thrown." — **issue comment by TJYSunset** (Independent confirmation of the bug with two concrete failure modes.)
- "a workaround is to allocate the blob's memory yourself with Marshal.AllocHGlobal, then instantiate the Blob with the IntPtr constructor" — **issue comment by TJYSunset** (Commenter already identified the root cause and a valid workaround.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/HarfBuzzSharp/Blob.cs` | 71-82 | direct | FromStream pins data with fixed, passes the pointer to the Blob constructor (which calls hb_blob_create), then exits fixed — leaving the native blob holding a pointer to now-unpinned managed memory. |
| `binding/HarfBuzzSharp/Blob.cs` | 84-89 | direct | The Create helper passes the raw IntPtr directly to hb_blob_create without any mechanism to keep the managed byte array alive or pinned for the blob's lifetime. |
| `binding/HarfBuzzSharp/NativeObject.cs` | 1-78 | related | NativeObject lifecycle — Dispose calls DisposeHandler which calls hb_blob_destroy, releasing the native blob. The release delegate is invoked by HarfBuzz on destroy, but no GCHandle lifetime is tied to the blob. |

**Error fingerprint:** `HarfBuzzSharp.Blob.FromStream:fixed-pointer-escapes-fixed-block`

### Workarounds

- Allocate unmanaged memory with Marshal.AllocHGlobal, copy data into it, and use the Blob(IntPtr, int, MemoryMode, ReleaseDelegate) constructor with a release delegate that calls Marshal.FreeHGlobal.
- Use GCHandle.Alloc(data, GCHandleType.Pinned) before creating the Blob, and free the GCHandle in the release delegate.

### Next Questions

- Does Blob.FromFile have the same issue, or does HarfBuzz take ownership via hb_blob_create_from_file?
- Should the fix use GCHandle.Pinned (avoids copy) or Marshal.AllocHGlobal+copy (avoids GC pressure from large pinned arrays)?

### Resolution Proposals

**Hypothesis:** The byte array from ms.ToArray() must remain pinned for the entire lifetime of the native HarfBuzz blob. Using GCHandle.Alloc(data, GCHandleType.Pinned) and freeing the handle in the release delegate is the minimal, correct fix.

1. **Pin with GCHandle for blob lifetime** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - Replace the fixed block with GCHandle.Alloc(data, GCHandleType.Pinned). Pass gch.AddrOfPinnedObject() to the Blob constructor. Free with gch.Free() in the release delegate.

```csharp
var data = ms.ToArray();
var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
return new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => { gch.Free(); ms.Dispose(); });
```
2. **Use Marshal.AllocHGlobal** — alternative, confidence 0.90 (90%), cost/xs, validated=yes
   - Copy the stream bytes into unmanaged memory via Marshal.AllocHGlobal and Marshal.Copy. Free in the release delegate. Avoids pinning managed heap.

```csharp
var data = ms.ToArray();
var ptr = Marshal.AllocHGlobal(data.Length);
Marshal.Copy(data, 0, ptr, data.Length);
return new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => { Marshal.FreeHGlobal(ptr); ms.Dispose(); });
```

**Recommended proposal:** Pin with GCHandle for blob lifetime

**Why:** Avoids extra copy of potentially large font data and requires minimal code change. GCHandle.Free in the release delegate cleanly ties memory lifetime to the native blob.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.95 (95%) |
| Reason | Root cause is clear (fixed pointer escapes fixed block), fix path is well-understood (GCHandle.Pinned or AllocHGlobal), and a community commenter already confirmed the bug. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply bug, HarfBuzzSharp, reliability labels | labels=type/bug, area/HarfBuzzSharp, tenet/reliability |
| add-comment | medium | 0.95 (95%) | Confirm the bug, explain root cause, and provide workaround and fix guidance | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this — you are correct, this is a real bug.

The `fixed` statement only pins the `data` array for the duration of the `fixed` block. Once `FromStream` returns, the `fixed` block has exited and the array is no longer pinned. The GC is free to move it at any time, invalidating the pointer already handed to native HarfBuzz. This causes the symptoms reported in the comments: corrupted `GlyphInfos` (codepoints become 0) or an `AccessViolationException`.

**Workaround** (until fixed):

Allocate unmanaged memory yourself:
```csharp
var stream = ...;
using var ms = new MemoryStream();
stream.CopyTo(ms);
var data = ms.ToArray();
var ptr = Marshal.AllocHGlobal(data.Length);
Marshal.Copy(data, 0, ptr, data.Length);
var blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(ptr));
```

Or use a pinned GCHandle:
```csharp
var gch = GCHandle.Alloc(data, GCHandleType.Pinned);
var blob = new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());
```

**Fix**: In `Blob.FromStream`, replace the `fixed` block with `GCHandle.Alloc(data, GCHandleType.Pinned)` and free it in the release delegate so the array remains pinned for the entire native blob lifetime.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2323,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T17:00:00Z"
  },
  "summary": "HarfBuzzSharp.Blob.FromStream passes a fixed pointer to native HarfBuzz after the fixed block exits, leaving the byte array unpinned and allowing the GC to relocate it, causing data corruption or AccessViolationException.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/HarfBuzzSharp",
      "confidence": 0.98
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
      "errorMessage": "AccessViolationException; or all codepoints in Buffer.GlyphInfos become 0",
      "reproQuality": "partial",
      "targetFrameworks": []
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a HarfBuzzSharp Blob using Blob.FromStream(stream)",
        "Use the Blob to shape text via a Face and Font",
        "Observe that Buffer.GlyphInfos codepoints are all 0, or an AccessViolationException is thrown"
      ],
      "environmentDetails": "Cross-platform; affects all runtimes where GC can move heap memory",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "FromStream pins a byte array with fixed to obtain a pointer, passes that pointer to the native hb_blob_create, then exits the fixed block — at which point the byte array is no longer pinned. The GC is free to relocate the array, invalidating the pointer held by native HarfBuzz. A community commenter confirmed AccessViolationException and corrupted GlyphInfos. The fix is to use GCHandle.Alloc(data, GCHandleType.Pinned) and free it in the release delegate.",
    "rationale": "Although filed as a question, a community comment confirmed the bug reproduces as AccessViolationException and corrupted glyph data. The code clearly passes a fixed pointer to native memory that outlives the fixed block — a classic unsafe memory management mistake documented in CLAUDE.md anti-patterns.",
    "keySignals": [
      {
        "text": "fixed (byte* dataPtr = data) { return new Blob ((IntPtr)dataPtr, data.Length, MemoryMode.ReadOnly, () => ms.Dispose ()); }",
        "source": "binding/HarfBuzzSharp/Blob.cs lines 79-81",
        "interpretation": "The fixed block ends immediately after the constructor returns, un-pinning data while native HarfBuzz still holds the pointer."
      },
      {
        "text": "Can confirm this is actually a problem. At best all codepoints in Buffer.GlyphInfos becomes 0, at worst AccessViolationException will be thrown.",
        "source": "issue comment by TJYSunset",
        "interpretation": "Independent confirmation of the bug with two concrete failure modes."
      },
      {
        "text": "a workaround is to allocate the blob's memory yourself with Marshal.AllocHGlobal, then instantiate the Blob with the IntPtr constructor",
        "source": "issue comment by TJYSunset",
        "interpretation": "Commenter already identified the root cause and a valid workaround."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "71-82",
        "finding": "FromStream pins data with fixed, passes the pointer to the Blob constructor (which calls hb_blob_create), then exits fixed — leaving the native blob holding a pointer to now-unpinned managed memory.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/Blob.cs",
        "lines": "84-89",
        "finding": "The Create helper passes the raw IntPtr directly to hb_blob_create without any mechanism to keep the managed byte array alive or pinned for the blob's lifetime.",
        "relevance": "direct"
      },
      {
        "file": "binding/HarfBuzzSharp/NativeObject.cs",
        "lines": "1-78",
        "finding": "NativeObject lifecycle — Dispose calls DisposeHandler which calls hb_blob_destroy, releasing the native blob. The release delegate is invoked by HarfBuzz on destroy, but no GCHandle lifetime is tied to the blob.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Allocate unmanaged memory with Marshal.AllocHGlobal, copy data into it, and use the Blob(IntPtr, int, MemoryMode, ReleaseDelegate) constructor with a release delegate that calls Marshal.FreeHGlobal.",
      "Use GCHandle.Alloc(data, GCHandleType.Pinned) before creating the Blob, and free the GCHandle in the release delegate."
    ],
    "nextQuestions": [
      "Does Blob.FromFile have the same issue, or does HarfBuzz take ownership via hb_blob_create_from_file?",
      "Should the fix use GCHandle.Pinned (avoids copy) or Marshal.AllocHGlobal+copy (avoids GC pressure from large pinned arrays)?"
    ],
    "errorFingerprint": "HarfBuzzSharp.Blob.FromStream:fixed-pointer-escapes-fixed-block",
    "resolution": {
      "hypothesis": "The byte array from ms.ToArray() must remain pinned for the entire lifetime of the native HarfBuzz blob. Using GCHandle.Alloc(data, GCHandleType.Pinned) and freeing the handle in the release delegate is the minimal, correct fix.",
      "proposals": [
        {
          "title": "Pin with GCHandle for blob lifetime",
          "description": "Replace the fixed block with GCHandle.Alloc(data, GCHandleType.Pinned). Pass gch.AddrOfPinnedObject() to the Blob constructor. Free with gch.Free() in the release delegate.",
          "category": "fix",
          "codeSnippet": "var data = ms.ToArray();\nvar gch = GCHandle.Alloc(data, GCHandleType.Pinned);\nreturn new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => { gch.Free(); ms.Dispose(); });",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Use Marshal.AllocHGlobal",
          "description": "Copy the stream bytes into unmanaged memory via Marshal.AllocHGlobal and Marshal.Copy. Free in the release delegate. Avoids pinning managed heap.",
          "category": "alternative",
          "codeSnippet": "var data = ms.ToArray();\nvar ptr = Marshal.AllocHGlobal(data.Length);\nMarshal.Copy(data, 0, ptr, data.Length);\nreturn new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => { Marshal.FreeHGlobal(ptr); ms.Dispose(); });",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Pin with GCHandle for blob lifetime",
      "recommendedReason": "Avoids extra copy of potentially large font data and requires minimal code change. GCHandle.Free in the release delegate cleanly ties memory lifetime to the native blob."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.95,
      "reason": "Root cause is clear (fixed pointer escapes fixed block), fix path is well-understood (GCHandle.Pinned or AllocHGlobal), and a community commenter already confirmed the bug.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, HarfBuzzSharp, reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/HarfBuzzSharp",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm the bug, explain root cause, and provide workaround and fix guidance",
        "risk": "medium",
        "confidence": 0.95,
        "comment": "Thanks for reporting this — you are correct, this is a real bug.\n\nThe `fixed` statement only pins the `data` array for the duration of the `fixed` block. Once `FromStream` returns, the `fixed` block has exited and the array is no longer pinned. The GC is free to move it at any time, invalidating the pointer already handed to native HarfBuzz. This causes the symptoms reported in the comments: corrupted `GlyphInfos` (codepoints become 0) or an `AccessViolationException`.\n\n**Workaround** (until fixed):\n\nAllocate unmanaged memory yourself:\n```csharp\nvar stream = ...;\nusing var ms = new MemoryStream();\nstream.CopyTo(ms);\nvar data = ms.ToArray();\nvar ptr = Marshal.AllocHGlobal(data.Length);\nMarshal.Copy(data, 0, ptr, data.Length);\nvar blob = new Blob(ptr, data.Length, MemoryMode.ReadOnly, () => Marshal.FreeHGlobal(ptr));\n```\n\nOr use a pinned GCHandle:\n```csharp\nvar gch = GCHandle.Alloc(data, GCHandleType.Pinned);\nvar blob = new Blob(gch.AddrOfPinnedObject(), data.Length, MemoryMode.ReadOnly, () => gch.Free());\n```\n\n**Fix**: In `Blob.FromStream`, replace the `fixed` block with `GCHandle.Alloc(data, GCHandleType.Pinned)` and free it in the release delegate so the array remains pinned for the entire native blob lifetime."
      }
    ]
  }
}
```

</details>
