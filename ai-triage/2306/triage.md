# Issue Triage Report — #2306

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T23:30:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/SkiaSharp (0.88 (88%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Hard crash (SIGSEGV on Linux, Fatal CLR Error on Windows) when calling CopyToAsync on a stream returned by SKData.AsStream(), due to the GC potentially freeing native SKData memory during async await suspension.

**Analysis:** SKData.AsStream() returns a private SKDataStream (UnmanagedMemoryStream) backed by native memory. In async methods, the GC may finalize the parent SKData object — freeing native memory — during an await suspension point, leaving the stream with a dangling pointer. Reading from that pointer in CopyToAsync causes the SIGSEGV / Fatal CLR Error. SKData.SaveTo() has an explicit GC.KeepAlive(this) guard for this exact risk, confirming it is known, but AsStream() lacks equivalent async-context protection.

**Recommendations:** **needs-investigation** — Real crash with detailed stack traces pointing to native memory use-after-free in SKData.AsStream() + async context. Code investigation confirms the vulnerable pattern. Needs a deterministic reproduction and a proper fix in SKDataStream.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Linux, os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Create SKImage from encoded data stream
2. Draw resized version onto SKSurface
3. Call surface.Snapshot() then ss.Encode() to produce SKData
4. Call thumbnailPng.AsStream() to get UnmanagedMemoryStream
5. Await CopyToAsync to write stream to FileStream
6. Observe intermittent SIGSEGV (Linux) or Fatal CLR Error 0x80131506 (Windows)

**Environment:** SkiaSharp 2.88.3, .NET 6.0, Linux Debian 11 + Windows 10 Pro 19044, server-side image processing on thread pool threads

**Repository links:**
- https://user-images.githubusercontent.com/33007665/200140248-9e31e43f-0f70-40f5-811e-81b105b0a52f.png — IDE crash screenshot showing exception at CopyToAsync call
- https://github.com/mono/SkiaSharp/issues/209 — Related issue: Thread Safety in SKBitmap.Decode — similar GC-timing/crash pattern in async server-side image processing

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | Fatal error. Internal CLR error. (0x80131506) at System.Buffer._Memmove |
| Repro quality | partial |
| Target frameworks | net6.0 |

**Stack trace:**

```text
UnmanagedMemoryStream.ReadCore -> Buffer.Memmove -> UnmanagedMemoryStream.Read -> UnmanagedMemoryStream.ReadAsync -> CopyToAsync
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | SKData.AsStream() and the private SKDataStream implementation have not materially changed since 2.88.3. The GC-timing vulnerability with async UnmanagedMemoryStream reads would still be present. |

## Analysis

### Technical Summary

SKData.AsStream() returns a private SKDataStream (UnmanagedMemoryStream) backed by native memory. In async methods, the GC may finalize the parent SKData object — freeing native memory — during an await suspension point, leaving the stream with a dangling pointer. Reading from that pointer in CopyToAsync causes the SIGSEGV / Fatal CLR Error. SKData.SaveTo() has an explicit GC.KeepAlive(this) guard for this exact risk, confirming it is known, but AsStream() lacks equivalent async-context protection.

### Rationale

This is a type/bug because it produces a hard crash (kills the .NET runtime) in structurally sound async code. The intermittent nature — crashing on some runs but not others on the same image — is the hallmark of a GC-timing race, not bad input. The Windows stack trace lands in UnmanagedMemoryStream.ReadCore → Buffer.Memmove, which is accessing freed native memory. The presence of GC.KeepAlive(this) in SaveTo() confirms the team is aware of the GC-timing pattern in SKData. The reporter's note that a minimal console app cannot reproduce it is consistent: higher server load triggers GC more aggressively, exposing the race.

### Key Signals

- "Fatal error. Internal CLR error. (0x80131506) at System.Buffer._Memmove ... UnmanagedMemoryStream.ReadCore" — **issue body (Windows crash output)** (Classic use-after-free: CopyToAsync is reading from native memory that has been freed by the GC/finalizer during the async suspension)
- "Inconsistent behaviour where the copy just doesn't work. Doesn't appear to be invalid input image data at all, as it can crash and succeed on the same image." — **issue body** (Non-deterministic failure tied to GC scheduling — crash occurs only when GC runs and finalizes SKData during the async await window)
- "I have been unable to reproduce this in a separate console app using essentially that sample code I provided" — **comment #1304645904** (Consistent with GC-timing: a simple console app rarely triggers GC within the short async window; a high-concurrency server does)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKData.cs` | 293-313 | direct | Private SKDataStream class (returned by AsStream()) inherits UnmanagedMemoryStream and passes the raw native pointer from host.Data at construction time. With disposeHost=false (default), the host field holds a managed reference to SKData, but in Release-mode async state machines the JIT may determine the stream variable is 'dead' before the CopyToAsync continuation finishes, allowing the host to be collected and its native memory freed. |
| `binding/SkiaSharp/SKData.cs` | 270-286 | direct | SaveTo() extracts a raw native pointer (var ptr = Data) and uses GC.KeepAlive(this) at the end to prevent SKData finalization while ptr is in use. This guard is absent in the async AsStream() path, confirming the team is aware of the pattern but has not addressed it for async consumers. |
| `binding/SkiaSharp/SKData.cs` | 255-259 | direct | AsStream() defaults streamDisposesData to false. SKDataStream stores host but the UnmanagedMemoryStream base only holds the raw unsafe byte* pointer — the GC does not track this pointer as a reference to the managed SKData object. |

**Error fingerprint:** `UnmanagedMemoryStream.ReadCore|Buffer.Memmove|CopyToAsync|SKData.AsStream`

### Workarounds

- Use SKData.ToArray() to copy data to a managed byte[] before any async operation, then wrap in MemoryStream — eliminates all native pointer access during await
- Add GC.KeepAlive(thumbnailPng) after the await CopyToAsync call to hint the GC not to collect the SKData prematurely
- Use thumbnailPng.AsStream(streamDisposesData: true) so the stream owns the SKData lifetime — though this ties SKData disposal to stream disposal and may not fully prevent GC-timing in all scenarios

### Next Questions

- Can a forced GC.Collect() inside the CopyToAsync await reproduce the crash deterministically?
- Does adding GC.KeepAlive(thumbnailPng) after the await prevent the crash?
- Does the crash occur only in Release mode (JIT optimizes away references) or also in Debug mode?

### Resolution Proposals

**Hypothesis:** SKData.AsStream() returns an UnmanagedMemoryStream backed by native SKData memory. In async methods, the GC may collect the SKData during an await suspension, freeing native memory while the stream still holds a raw pointer to it. Reading from the freed pointer in CopyToAsync causes the crash.

1. **Copy to managed memory before async operation** — workaround, confidence 0.75 (75%), cost/xs, validated=yes
   - Use SKData.ToArray() to copy to a managed byte[] before any async operation, then wrap in MemoryStream. This eliminates all native pointer access during await.

```csharp
using SKImage? ss = surface.Snapshot();
if (ss is null) return;
using SKData? thumbnailPng = ss.Encode();
byte[]? bytes = thumbnailPng?.ToArray();
if (bytes is not null)
{
    using var ms = new MemoryStream(bytes);
    await ms.CopyToAsync(fileHandle);
}
```

Note: null-check ss and thumbnailPng before use as factory methods can return null on failure.
2. **Fix SKDataStream to guard GC in async scenarios** — fix, confidence 0.70 (70%), cost/s, validated=untested
   - Add a GCHandle.Alloc(Pinned) or override ReadAsync in SKDataStream to call GC.KeepAlive(host) after each read, mirroring the SaveTo() pattern. This fixes the root cause without requiring callers to change their code.

**Recommended proposal:** Copy to managed memory before async operation

**Why:** Immediate safe workaround requiring minimal code change. Zero risk of native memory issues. The fix proposal needs deeper investigation to handle all async code paths correctly.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Real crash with detailed stack traces pointing to native memory use-after-free in SKData.AsStream() + async context. Code investigation confirms the vulnerable pattern. Needs a deterministic reproduction and a proper fix in SKDataStream. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply bug, core library, platform, and reliability labels | labels=type/bug, area/SkiaSharp, os/Linux, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.82 (82%) | Provide root cause analysis and safe workaround using managed memory copy | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report including GDB output and Windows stack trace!

This looks like a **GC timing issue** with `SKData.AsStream()` in async methods. `AsStream()` returns an `UnmanagedMemoryStream` backed by native memory owned by the `SKData` object. When used with `await`, the GC may collect and finalize the `SKData` object during the `await` suspension point — freeing the native memory while the stream still holds a raw pointer to it. Reading from that pointer in `CopyToAsync` causes the SIGSEGV / Fatal CLR Error.

Note: `SKData.SaveTo()` has an explicit `GC.KeepAlive(this)` guard for exactly this reason, but `AsStream()` lacks equivalent async-context protection.

The fact that it cannot reproduce in a minimal console app (your comment) and only happens on the server is consistent with a GC-timing race: higher concurrency triggers GC more aggressively during async suspension windows.

**Workaround:** Copy to managed memory before the async operation:

```csharp
using SKImage? ss = surface.Snapshot();
if (ss is null) return;
using SKData? thumbnailPng = ss.Encode();
byte[]? bytes = thumbnailPng?.ToArray();
if (bytes is not null)
{
    using var ms = new MemoryStream(bytes);
    await ms.CopyToAsync(fileHandle);
}
```

`ToArray()` copies to managed memory so the async operation never touches native pointers — GC timing no longer matters.

We'll investigate fixing `SKDataStream` to properly protect against GC during async reads.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2306,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T23:30:00Z"
  },
  "summary": "Hard crash (SIGSEGV on Linux, Fatal CLR Error on Windows) when calling CopyToAsync on a stream returned by SKData.AsStream(), due to the GC potentially freeing native SKData memory during async await suspension.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.88
    },
    "platforms": [
      "os/Linux",
      "os/Windows-Classic"
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
      "errorMessage": "Fatal error. Internal CLR error. (0x80131506) at System.Buffer._Memmove",
      "stackTrace": "UnmanagedMemoryStream.ReadCore -> Buffer.Memmove -> UnmanagedMemoryStream.Read -> UnmanagedMemoryStream.ReadAsync -> CopyToAsync",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create SKImage from encoded data stream",
        "Draw resized version onto SKSurface",
        "Call surface.Snapshot() then ss.Encode() to produce SKData",
        "Call thumbnailPng.AsStream() to get UnmanagedMemoryStream",
        "Await CopyToAsync to write stream to FileStream",
        "Observe intermittent SIGSEGV (Linux) or Fatal CLR Error 0x80131506 (Windows)"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET 6.0, Linux Debian 11 + Windows 10 Pro 19044, server-side image processing on thread pool threads",
      "repoLinks": [
        {
          "url": "https://user-images.githubusercontent.com/33007665/200140248-9e31e43f-0f70-40f5-811e-81b105b0a52f.png",
          "description": "IDE crash screenshot showing exception at CopyToAsync call"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/209",
          "description": "Related issue: Thread Safety in SKBitmap.Decode — similar GC-timing/crash pattern in async server-side image processing"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "SKData.AsStream() and the private SKDataStream implementation have not materially changed since 2.88.3. The GC-timing vulnerability with async UnmanagedMemoryStream reads would still be present."
    }
  },
  "analysis": {
    "summary": "SKData.AsStream() returns a private SKDataStream (UnmanagedMemoryStream) backed by native memory. In async methods, the GC may finalize the parent SKData object — freeing native memory — during an await suspension point, leaving the stream with a dangling pointer. Reading from that pointer in CopyToAsync causes the SIGSEGV / Fatal CLR Error. SKData.SaveTo() has an explicit GC.KeepAlive(this) guard for this exact risk, confirming it is known, but AsStream() lacks equivalent async-context protection.",
    "rationale": "This is a type/bug because it produces a hard crash (kills the .NET runtime) in structurally sound async code. The intermittent nature — crashing on some runs but not others on the same image — is the hallmark of a GC-timing race, not bad input. The Windows stack trace lands in UnmanagedMemoryStream.ReadCore → Buffer.Memmove, which is accessing freed native memory. The presence of GC.KeepAlive(this) in SaveTo() confirms the team is aware of the GC-timing pattern in SKData. The reporter's note that a minimal console app cannot reproduce it is consistent: higher server load triggers GC more aggressively, exposing the race.",
    "keySignals": [
      {
        "text": "Fatal error. Internal CLR error. (0x80131506) at System.Buffer._Memmove ... UnmanagedMemoryStream.ReadCore",
        "source": "issue body (Windows crash output)",
        "interpretation": "Classic use-after-free: CopyToAsync is reading from native memory that has been freed by the GC/finalizer during the async suspension"
      },
      {
        "text": "Inconsistent behaviour where the copy just doesn't work. Doesn't appear to be invalid input image data at all, as it can crash and succeed on the same image.",
        "source": "issue body",
        "interpretation": "Non-deterministic failure tied to GC scheduling — crash occurs only when GC runs and finalizes SKData during the async await window"
      },
      {
        "text": "I have been unable to reproduce this in a separate console app using essentially that sample code I provided",
        "source": "comment #1304645904",
        "interpretation": "Consistent with GC-timing: a simple console app rarely triggers GC within the short async window; a high-concurrency server does"
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "293-313",
        "finding": "Private SKDataStream class (returned by AsStream()) inherits UnmanagedMemoryStream and passes the raw native pointer from host.Data at construction time. With disposeHost=false (default), the host field holds a managed reference to SKData, but in Release-mode async state machines the JIT may determine the stream variable is 'dead' before the CopyToAsync continuation finishes, allowing the host to be collected and its native memory freed.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "270-286",
        "finding": "SaveTo() extracts a raw native pointer (var ptr = Data) and uses GC.KeepAlive(this) at the end to prevent SKData finalization while ptr is in use. This guard is absent in the async AsStream() path, confirming the team is aware of the pattern but has not addressed it for async consumers.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "255-259",
        "finding": "AsStream() defaults streamDisposesData to false. SKDataStream stores host but the UnmanagedMemoryStream base only holds the raw unsafe byte* pointer — the GC does not track this pointer as a reference to the managed SKData object.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Can a forced GC.Collect() inside the CopyToAsync await reproduce the crash deterministically?",
      "Does adding GC.KeepAlive(thumbnailPng) after the await prevent the crash?",
      "Does the crash occur only in Release mode (JIT optimizes away references) or also in Debug mode?"
    ],
    "workarounds": [
      "Use SKData.ToArray() to copy data to a managed byte[] before any async operation, then wrap in MemoryStream — eliminates all native pointer access during await",
      "Add GC.KeepAlive(thumbnailPng) after the await CopyToAsync call to hint the GC not to collect the SKData prematurely",
      "Use thumbnailPng.AsStream(streamDisposesData: true) so the stream owns the SKData lifetime — though this ties SKData disposal to stream disposal and may not fully prevent GC-timing in all scenarios"
    ],
    "errorFingerprint": "UnmanagedMemoryStream.ReadCore|Buffer.Memmove|CopyToAsync|SKData.AsStream",
    "resolution": {
      "hypothesis": "SKData.AsStream() returns an UnmanagedMemoryStream backed by native SKData memory. In async methods, the GC may collect the SKData during an await suspension, freeing native memory while the stream still holds a raw pointer to it. Reading from the freed pointer in CopyToAsync causes the crash.",
      "proposals": [
        {
          "title": "Copy to managed memory before async operation",
          "description": "Use SKData.ToArray() to copy to a managed byte[] before any async operation, then wrap in MemoryStream. This eliminates all native pointer access during await.\n\n```csharp\nusing SKImage? ss = surface.Snapshot();\nif (ss is null) return;\nusing SKData? thumbnailPng = ss.Encode();\nbyte[]? bytes = thumbnailPng?.ToArray();\nif (bytes is not null)\n{\n    using var ms = new MemoryStream(bytes);\n    await ms.CopyToAsync(fileHandle);\n}\n```\n\nNote: null-check ss and thumbnailPng before use as factory methods can return null on failure.",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Fix SKDataStream to guard GC in async scenarios",
          "description": "Add a GCHandle.Alloc(Pinned) or override ReadAsync in SKDataStream to call GC.KeepAlive(host) after each read, mirroring the SaveTo() pattern. This fixes the root cause without requiring callers to change their code.",
          "category": "fix",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Copy to managed memory before async operation",
      "recommendedReason": "Immediate safe workaround requiring minimal code change. Zero risk of native memory issues. The fix proposal needs deeper investigation to handle all async code paths correctly."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Real crash with detailed stack traces pointing to native memory use-after-free in SKData.AsStream() + async context. Code investigation confirms the vulnerable pattern. Needs a deterministic reproduction and a proper fix in SKDataStream.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, core library, platform, and reliability labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Linux",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide root cause analysis and safe workaround using managed memory copy",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report including GDB output and Windows stack trace!\n\nThis looks like a **GC timing issue** with `SKData.AsStream()` in async methods. `AsStream()` returns an `UnmanagedMemoryStream` backed by native memory owned by the `SKData` object. When used with `await`, the GC may collect and finalize the `SKData` object during the `await` suspension point — freeing the native memory while the stream still holds a raw pointer to it. Reading from that pointer in `CopyToAsync` causes the SIGSEGV / Fatal CLR Error.\n\nNote: `SKData.SaveTo()` has an explicit `GC.KeepAlive(this)` guard for exactly this reason, but `AsStream()` lacks equivalent async-context protection.\n\nThe fact that it cannot reproduce in a minimal console app (your comment) and only happens on the server is consistent with a GC-timing race: higher concurrency triggers GC more aggressively during async suspension windows.\n\n**Workaround:** Copy to managed memory before the async operation:\n\n```csharp\nusing SKImage? ss = surface.Snapshot();\nif (ss is null) return;\nusing SKData? thumbnailPng = ss.Encode();\nbyte[]? bytes = thumbnailPng?.ToArray();\nif (bytes is not null)\n{\n    using var ms = new MemoryStream(bytes);\n    await ms.CopyToAsync(fileHandle);\n}\n```\n\n`ToArray()` copies to managed memory so the async operation never touches native pointers — GC timing no longer matters.\n\nWe'll investigate fixing `SKDataStream` to properly protect against GC during async reads."
      }
    ]
  }
}
```

</details>
