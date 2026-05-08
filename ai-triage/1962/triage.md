# Issue Triage Report — #1962

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T07:31:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.88 (88%)) |

**Issue Summary:** SKManagedStream.OnReadManagedStream uses Stream.Read() which may return fewer bytes than requested even when not at end-of-stream, causing image decoding failures (e.g., SKCodec.Create returns null) for streams that deliver data in chunks, introduced as a regression in 2.80.3.

**Analysis:** The SKManagedStream.OnReadManagedStream method performs a single stream.Read() call which, per the Stream.Read() contract, is permitted to return fewer bytes than requested even when not at end-of-stream. The previous implementation used BinaryReader.ReadBytes() which only returns fewer bytes at EOF. Additionally, the isAsEnd heuristic on line 92 uses `len <= (int)size` which is always true (can never read more than requested), incorrectly marking non-seekable streams as at-end after any partial read.

**Recommendations:** **ready-to-fix** — Root cause is clear, precisely located, and the fix is a small change in OnReadManagedStream. Complete repro is provided. Two independent reports confirm real-world impact.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | — |
| Backends | — |
| Tenets | tenet/reliability, tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a custom MemoryStream subclass that limits each Read() call to BLOCK_SIZE=10 bytes
2. Load a PNG image into this BlockMemoryStream
3. Call SKCodec.Create(blockMemoryStream)
4. Observe that codec is null in 2.80.3 but non-null in 2.80.2

**Environment:** SkiaSharp 2.80.3 vs 2.80.2. Also reported with Azure Blob stream.

**Repository links:**
- https://github.com/mono/SkiaSharp/files/8148503/SkiaSharpBlockStreamDemo.zip — Minimal repro project demonstrating the regression with BlockMemoryStream
- https://github.com/mono/SkiaSharp/pull/1510 — PR #1510 — the change that introduced the regression (switched from BinaryReader.ReadBytes to stream.Read)

**Code snippets:**

```csharp
var codec = SKCodec.Create(blockMemoryStream); // returns null in 2.80.3
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | wrong-output |
| Error message | SKCodec.Create returns null; NullReferenceException when accessing codec.Info.Size |
| Repro quality | complete |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2, 2.80.3 |
| Worked in | 2.80.2 |
| Broke in | 2.80.3 |
| Current relevance | likely |
| Relevance reason | The OnReadManagedStream code in binding/SkiaSharp/SKManagedStream.cs still uses a single stream.Read() call without looping. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.97 (97%) |
| Reason | Reporter explicitly identifies the introducing commit (61b71d6e4893) in PR #1510 and shows the exact API change that broke behavior. Code inspection confirms the single stream.Read() call is still present. |
| Worked in version | 2.80.2 |
| Broke in version | 2.80.3 |

## Analysis

### Technical Summary

The SKManagedStream.OnReadManagedStream method performs a single stream.Read() call which, per the Stream.Read() contract, is permitted to return fewer bytes than requested even when not at end-of-stream. The previous implementation used BinaryReader.ReadBytes() which only returns fewer bytes at EOF. Additionally, the isAsEnd heuristic on line 92 uses `len <= (int)size` which is always true (can never read more than requested), incorrectly marking non-seekable streams as at-end after any partial read.

### Rationale

Classified as type/bug because the behavioral change between 2.80.2 and 2.80.3 is clearly documented, the root cause is identified (single stream.Read() without a retry loop), a complete reproduction is provided, and the behavior is a regression from a known-good state. The area is area/SkiaSharp because SKManagedStream is a core SkiaSharp binding class. Severity is high because it causes complete decoding failure (null codec) for any non-standard stream that delivers data in chunks — a class of streams that includes real-world data sources like Azure Blob storage.

### Key Signals

- "The current code uses stream.Read(...) — An implementation is free to return fewer bytes than requested even if the end of the stream has not been reached." — **issue body** (Reporter correctly identifies the root cause: Stream.Read() contract vs BinaryReader.ReadBytes() contract.)
- "SkiaSharp 2.80.3 -> codec is null -> NullReferenceException" — **issue body** (Skia interprets the partial read as EOF, abandons stream parsing, returns null codec.)
- "Just believe that I bounced into this bug when using Azure Blob stream" — **comment by bang75** (Real-world impact: Azure Blob streams are non-seekable and deliver data in variable-sized chunks, confirming this is not just a synthetic test case.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKManagedStream.cs` | 73-96 | direct | OnReadManagedStream uses `stream.Read(managedBuffer.Array, 0, managedBuffer.Length)` at line 83. Stream.Read() may return fewer bytes than requested even mid-stream. No retry loop is present. The isAsEnd heuristic at line 92 checks `len <= (int)size` which is always true, so any read on a non-seekable stream incorrectly sets isAsEnd=true. |
| `binding/SkiaSharp/SKManagedStream.cs` | 114-120 | direct | OnIsAtEnd() returns isAsEnd for non-seekable streams. If isAsEnd is incorrectly set to true after a partial read, all subsequent OnIsAtEnd() calls will return true, causing Skia to stop reading the stream prematurely — explaining why SKCodec.Create returns null. |
| `binding/SkiaSharp/SKFrontBufferedManagedStream.cs` | 64-124 | related | SKFrontBufferedManagedStream.OnRead uses stream.Read() from native SKStream (not managed Stream), which has different semantics. The same partial-read pattern is present in step 3 (line 111) but it wraps an SKStream whose Read() delegates to Skia's native read which may loop internally. |

### Workarounds

- Copy the stream into a MemoryStream before passing to SKCodec.Create: `var ms = new MemoryStream(); stream.CopyTo(ms); ms.Position = 0; var codec = SKCodec.Create(ms);`
- Use SKData.Create(stream) to buffer the entire stream into native memory first, then pass the SKData to SKCodec.Create: `using var data = SKData.Create(stream); var codec = SKCodec.Create(data);`
- Wrap the stream in a buffering adapter that internally retries Read() until the requested count is satisfied or the stream ends.

### Next Questions

- Does SKFrontBufferedManagedStream have the same issue when wrapping a managed Stream directly?
- Is the isAsEnd fix needed or can the condition be simplified to `len == 0` (true EOF sentinel)?
- Are there other callers of OnRead in SKAbstractManagedStream subclasses that have the same partial-read bug?

### Resolution Proposals

**Hypothesis:** The fix is to replace the single stream.Read() call with a loop that retries until the requested number of bytes have been read or 0 bytes are returned (true EOF). The isAsEnd heuristic should also be corrected to only set true when 0 bytes are returned on a non-zero-size request.

1. **Fix OnReadManagedStream to loop until full read or EOF** — fix, confidence 0.88 (88%), cost/xs, validated=untested
   - Replace the single stream.Read() call with a loop. Update isAsEnd to only fire when stream returns 0 bytes.

```csharp
int totalRead = 0;
while (totalRead < (int)size)
{
    int bytesRead = stream.Read(managedBuffer.Array, totalRead, (int)size - totalRead);
    if (bytesRead == 0) break;
    totalRead += bytesRead;
}
var len = totalRead;

// Only mark as end when stream truly returned 0 bytes on a non-zero request
if (!stream.CanSeek && (int)size > 0 && len < (int)size && totalRead == 0)
    isAsEnd = true;
```
2. **Workaround: buffer the stream into MemoryStream before use** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Users can copy their stream into a MemoryStream which always returns the full requested count, avoiding the issue until a fix is released.

```csharp
// Option A: via MemoryStream
var ms = new MemoryStream();
yourStream.CopyTo(ms);
ms.Position = 0;
using var codec = SKCodec.Create(ms);

// Option B: via SKData (more efficient, avoids double-copy)
using var data = SKData.Create(yourStream);
using var codec = SKCodec.Create(data);
```

**Recommended proposal:** Fix OnReadManagedStream to loop until full read or EOF

**Why:** The fix is small, well-understood, and directly addresses the regression. The workaround requires callers to know about the bug and change their code.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.88 (88%) |
| Reason | Root cause is clear, precisely located, and the fix is a small change in OnReadManagedStream. Complete repro is provided. Two independent reports confirm real-world impact. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp, tenet/reliability, tenet/compatibility | labels=type/bug, area/SkiaSharp, tenet/reliability, tenet/compatibility |
| add-comment | medium | 0.88 (88%) | Acknowledge the regression, confirm root cause, provide workaround, signal intent to fix | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed analysis — you've correctly identified the root cause.

**Root cause:** `SKManagedStream.OnReadManagedStream` switched from `BinaryReader.ReadBytes()` to `stream.Read()` in PR #1510. The `Stream.Read()` contract allows returning fewer bytes than requested even when not at end-of-stream, while `BinaryReader.ReadBytes()` only returns fewer bytes at EOF. This causes Skia's codec to receive a short read that it interprets as EOF, so `SKCodec.Create` returns `null`.

There is also a secondary issue: the `isAsEnd` heuristic uses `len <= (int)size` which is always true, incorrectly marking non-seekable streams as at-end after any partial read.

**Workaround until a fix is released:**

```csharp
// Option A — buffer into MemoryStream first
var ms = new MemoryStream();
yourStream.CopyTo(ms);
ms.Position = 0;
using var codec = SKCodec.Create(ms);

// Option B — use SKData (avoids double buffering)
using var data = SKData.Create(yourStream);
using var codec = SKCodec.Create(data);
```

The fix is to replace the single `stream.Read()` call with a loop that retries until the full count is satisfied or 0 bytes are returned (true EOF).
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1962,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T07:31:00Z"
  },
  "summary": "SKManagedStream.OnReadManagedStream uses Stream.Read() which may return fewer bytes than requested even when not at end-of-stream, causing image decoding failures (e.g., SKCodec.Create returns null) for streams that deliver data in chunks, introduced as a regression in 2.80.3.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.95
    },
    "tenets": [
      "tenet/reliability",
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "wrong-output",
      "errorMessage": "SKCodec.Create returns null; NullReferenceException when accessing codec.Info.Size",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a custom MemoryStream subclass that limits each Read() call to BLOCK_SIZE=10 bytes",
        "Load a PNG image into this BlockMemoryStream",
        "Call SKCodec.Create(blockMemoryStream)",
        "Observe that codec is null in 2.80.3 but non-null in 2.80.2"
      ],
      "codeSnippets": [
        "var codec = SKCodec.Create(blockMemoryStream); // returns null in 2.80.3"
      ],
      "environmentDetails": "SkiaSharp 2.80.3 vs 2.80.2. Also reported with Azure Blob stream.",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/8148503/SkiaSharpBlockStreamDemo.zip",
          "description": "Minimal repro project demonstrating the regression with BlockMemoryStream"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/pull/1510",
          "description": "PR #1510 — the change that introduced the regression (switched from BinaryReader.ReadBytes to stream.Read)"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2",
        "2.80.3"
      ],
      "workedIn": "2.80.2",
      "brokeIn": "2.80.3",
      "currentRelevance": "likely",
      "relevanceReason": "The OnReadManagedStream code in binding/SkiaSharp/SKManagedStream.cs still uses a single stream.Read() call without looping."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.97,
      "reason": "Reporter explicitly identifies the introducing commit (61b71d6e4893) in PR #1510 and shows the exact API change that broke behavior. Code inspection confirms the single stream.Read() call is still present.",
      "workedInVersion": "2.80.2",
      "brokeInVersion": "2.80.3"
    }
  },
  "analysis": {
    "summary": "The SKManagedStream.OnReadManagedStream method performs a single stream.Read() call which, per the Stream.Read() contract, is permitted to return fewer bytes than requested even when not at end-of-stream. The previous implementation used BinaryReader.ReadBytes() which only returns fewer bytes at EOF. Additionally, the isAsEnd heuristic on line 92 uses `len <= (int)size` which is always true (can never read more than requested), incorrectly marking non-seekable streams as at-end after any partial read.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "73-96",
        "finding": "OnReadManagedStream uses `stream.Read(managedBuffer.Array, 0, managedBuffer.Length)` at line 83. Stream.Read() may return fewer bytes than requested even mid-stream. No retry loop is present. The isAsEnd heuristic at line 92 checks `len <= (int)size` which is always true, so any read on a non-seekable stream incorrectly sets isAsEnd=true.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKManagedStream.cs",
        "lines": "114-120",
        "finding": "OnIsAtEnd() returns isAsEnd for non-seekable streams. If isAsEnd is incorrectly set to true after a partial read, all subsequent OnIsAtEnd() calls will return true, causing Skia to stop reading the stream prematurely — explaining why SKCodec.Create returns null.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFrontBufferedManagedStream.cs",
        "lines": "64-124",
        "finding": "SKFrontBufferedManagedStream.OnRead uses stream.Read() from native SKStream (not managed Stream), which has different semantics. The same partial-read pattern is present in step 3 (line 111) but it wraps an SKStream whose Read() delegates to Skia's native read which may loop internally.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "The current code uses stream.Read(...) — An implementation is free to return fewer bytes than requested even if the end of the stream has not been reached.",
        "source": "issue body",
        "interpretation": "Reporter correctly identifies the root cause: Stream.Read() contract vs BinaryReader.ReadBytes() contract."
      },
      {
        "text": "SkiaSharp 2.80.3 -> codec is null -> NullReferenceException",
        "source": "issue body",
        "interpretation": "Skia interprets the partial read as EOF, abandons stream parsing, returns null codec."
      },
      {
        "text": "Just believe that I bounced into this bug when using Azure Blob stream",
        "source": "comment by bang75",
        "interpretation": "Real-world impact: Azure Blob streams are non-seekable and deliver data in variable-sized chunks, confirming this is not just a synthetic test case."
      }
    ],
    "rationale": "Classified as type/bug because the behavioral change between 2.80.2 and 2.80.3 is clearly documented, the root cause is identified (single stream.Read() without a retry loop), a complete reproduction is provided, and the behavior is a regression from a known-good state. The area is area/SkiaSharp because SKManagedStream is a core SkiaSharp binding class. Severity is high because it causes complete decoding failure (null codec) for any non-standard stream that delivers data in chunks — a class of streams that includes real-world data sources like Azure Blob storage.",
    "workarounds": [
      "Copy the stream into a MemoryStream before passing to SKCodec.Create: `var ms = new MemoryStream(); stream.CopyTo(ms); ms.Position = 0; var codec = SKCodec.Create(ms);`",
      "Use SKData.Create(stream) to buffer the entire stream into native memory first, then pass the SKData to SKCodec.Create: `using var data = SKData.Create(stream); var codec = SKCodec.Create(data);`",
      "Wrap the stream in a buffering adapter that internally retries Read() until the requested count is satisfied or the stream ends."
    ],
    "nextQuestions": [
      "Does SKFrontBufferedManagedStream have the same issue when wrapping a managed Stream directly?",
      "Is the isAsEnd fix needed or can the condition be simplified to `len == 0` (true EOF sentinel)?",
      "Are there other callers of OnRead in SKAbstractManagedStream subclasses that have the same partial-read bug?"
    ],
    "resolution": {
      "hypothesis": "The fix is to replace the single stream.Read() call with a loop that retries until the requested number of bytes have been read or 0 bytes are returned (true EOF). The isAsEnd heuristic should also be corrected to only set true when 0 bytes are returned on a non-zero-size request.",
      "proposals": [
        {
          "title": "Fix OnReadManagedStream to loop until full read or EOF",
          "description": "Replace the single stream.Read() call with a loop. Update isAsEnd to only fire when stream returns 0 bytes.",
          "category": "fix",
          "codeSnippet": "int totalRead = 0;\nwhile (totalRead < (int)size)\n{\n    int bytesRead = stream.Read(managedBuffer.Array, totalRead, (int)size - totalRead);\n    if (bytesRead == 0) break;\n    totalRead += bytesRead;\n}\nvar len = totalRead;\n\n// Only mark as end when stream truly returned 0 bytes on a non-zero request\nif (!stream.CanSeek && (int)size > 0 && len < (int)size && totalRead == 0)\n    isAsEnd = true;",
          "confidence": 0.88,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: buffer the stream into MemoryStream before use",
          "description": "Users can copy their stream into a MemoryStream which always returns the full requested count, avoiding the issue until a fix is released.",
          "category": "workaround",
          "codeSnippet": "// Option A: via MemoryStream\nvar ms = new MemoryStream();\nyourStream.CopyTo(ms);\nms.Position = 0;\nusing var codec = SKCodec.Create(ms);\n\n// Option B: via SKData (more efficient, avoids double-copy)\nusing var data = SKData.Create(yourStream);\nusing var codec = SKCodec.Create(data);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Fix OnReadManagedStream to loop until full read or EOF",
      "recommendedReason": "The fix is small, well-understood, and directly addresses the regression. The workaround requires callers to know about the bug and change their code."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.88,
      "reason": "Root cause is clear, precisely located, and the fix is a small change in OnReadManagedStream. Complete repro is provided. Two independent reports confirm real-world impact.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, tenet/reliability, tenet/compatibility",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "tenet/reliability",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the regression, confirm root cause, provide workaround, signal intent to fix",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed analysis — you've correctly identified the root cause.\n\n**Root cause:** `SKManagedStream.OnReadManagedStream` switched from `BinaryReader.ReadBytes()` to `stream.Read()` in PR #1510. The `Stream.Read()` contract allows returning fewer bytes than requested even when not at end-of-stream, while `BinaryReader.ReadBytes()` only returns fewer bytes at EOF. This causes Skia's codec to receive a short read that it interprets as EOF, so `SKCodec.Create` returns `null`.\n\nThere is also a secondary issue: the `isAsEnd` heuristic uses `len <= (int)size` which is always true, incorrectly marking non-seekable streams as at-end after any partial read.\n\n**Workaround until a fix is released:**\n\n```csharp\n// Option A — buffer into MemoryStream first\nvar ms = new MemoryStream();\nyourStream.CopyTo(ms);\nms.Position = 0;\nusing var codec = SKCodec.Create(ms);\n\n// Option B — use SKData (avoids double buffering)\nusing var data = SKData.Create(yourStream);\nusing var codec = SKCodec.Create(data);\n```\n\nThe fix is to replace the single `stream.Read()` call with a loop that retries until the full count is satisfied or 0 bytes are returned (true EOF)."
      }
    ]
  }
}
```

</details>
