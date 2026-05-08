# Issue Triage Report — #1790

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-05T11:27:12Z |
| Type | type/bug (0.65 (65%)) |
| Area | area/SkiaSharp (0.90 (90%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** Reporter sees AccessViolationException when calling DrawCircle with a private SKPaint field, but not when using a public field assigned from the same object.

**Analysis:** The reported behavior—private field crashes while public field (referencing the same object) succeeds—is logically inconsistent under normal C# object semantics, suggesting that the reproduction is incomplete or the reporter has additional code that may reassign or dispose _blueP. SKPaint.DisposeNative calls sk_compatpaint_delete; if the handle is freed and reused this would explain an AVE. The pasted code contains 'miss code' placeholders, meaning the actual reproduction has undisclosed logic that could include disposal or reassignment of the private field.

**Recommendations:** **needs-info** — The reproduction code is explicitly incomplete ('miss code' comments). The described behavior—private field crashes, public field (same reference) works—is logically inconsistent without additional disposal/reassignment logic. Cannot reproduce or investigate further without the full code and SkiaSharp version.

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

1. Create a class with private SKPaint _blueP and public SKPaint Paint = _blueP.
2. Initialize _blueP with new SKPaint() and assign Paint = _blueP.
3. In Draw(), call canvas.DrawCircle using _blueP — AVE occurs; same call using Paint succeeds.

**Code snippets:**

```csharp
canvas.DrawCircle(P0, 5, _blueP); // raises System.AccessViolationException
canvas.DrawCircle(P0, 5, Paint);  // does not crash (Paint = _blueP)
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.AccessViolationException |
| Repro quality | partial |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | — |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | No SkiaSharp version specified. Issue filed 2021 against .NET Framework 4.7.2 WPF. |

## Analysis

### Technical Summary

The reported behavior—private field crashes while public field (referencing the same object) succeeds—is logically inconsistent under normal C# object semantics, suggesting that the reproduction is incomplete or the reporter has additional code that may reassign or dispose _blueP. SKPaint.DisposeNative calls sk_compatpaint_delete; if the handle is freed and reused this would explain an AVE. The pasted code contains 'miss code' placeholders, meaning the actual reproduction has undisclosed logic that could include disposal or reassignment of the private field.

### Rationale

Classified type/bug at moderate confidence because AVE is always a serious signal, but confidence is reduced because the stated behavior (private vs public field on the same object causing different outcomes) is not reproducible via the provided snippet alone. The removed code may contain disposal or reassignment of _blueP. Platform os/Windows-Classic applies because the reporter specifies WPF/.NET Framework 4.7.2. Severity is medium: AVE is serious but likely a user-side disposal or threading issue rather than a SkiaSharp internals bug.

### Key Signals

- "canvas.DrawCircle(P0, 5, _blueP); // This raise System.AccessViolationException" — **issue body** (AVE when passing private field directly to native interop call.)
- "canvas.DrawCircle(P0, 5, Paint); // This not crash" — **issue body** (No crash when passing public field that was assigned from _blueP — both reference the same object, making this inconsistency suspicious.)
- "// miss code ... (I removed all not unnecessary code to reproduce)" — **issue body** (Reproduction is acknowledged to be incomplete, making root cause determination impossible without the full code.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPaint.cs` | 38-89 | direct | SKPaint extends SKObject, ISKSkipObjectRegistration. DisposeNative calls sk_compatpaint_delete(Handle). Both _blueP and Paint reference the same managed object; if it were GC-collected or disposed, both would crash. |
| `binding/SkiaSharp/SKPaint.cs` | 64-70 | direct | SKPaint constructor calls sk_compatpaint_new_with_font(defaultFont.Handle) and throws if handle is zero, confirming creation succeeds. No evidence of a bug in SKPaint construction itself. |

### Workarounds

- Assign _blueP to a local variable before use: var p = _blueP; canvas.DrawCircle(P0, 5, p);
- Ensure _blueP is not disposed before Draw() is called.

### Next Questions

- What is the full, unredacted Sk0CubicBezier implementation? Specifically: is _blueP disposed or reassigned anywhere?
- What version of SkiaSharp is being used?
- Does the issue reproduce in a fresh minimal WPF project with only the code shown?
- Is the Draw() method called from the UI thread or a background thread?

### Resolution Proposals

**Hypothesis:** The omitted code in Sk0CubicBezier likely disposes or reassigns _blueP after construction, leaving a dangling handle. Accessing the private field directly in Draw() then causes AVE. The public Paint field may incidentally survive because of a separate code path.

1. **Request full reproduction** — investigation, cost/xs, validated=untested
   - Ask the reporter to provide the full Sk0CubicBezier class without redactions, a SkiaSharp version, and confirmation that no disposal of _blueP occurs before Draw().

**Recommended proposal:** p1

**Why:** Cannot determine root cause without the missing code sections.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The reproduction code is explicitly incomplete ('miss code' comments). The described behavior—private field crashes, public field (same reference) works—is logically inconsistent without additional disposal/reassignment logic. Cannot reproduce or investigate further without the full code and SkiaSharp version. |
| Suggested repro platform | windows |

### Missing Info

- Complete, unredacted source of Sk0CubicBezier (especially any Dispose implementations).
- SkiaSharp NuGet package version.
- Whether the crash is deterministic or intermittent.
- Whether Draw() is called from the UI thread.

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.85 (85%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Ask reporter for full code, SkiaSharp version, and threading context. | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this!

The behavior you describe — where `_blueP` crashes but `Paint` (which is assigned `= _blueP`) works — is very unusual because both fields should reference the exact same managed object and native handle.

To investigate further, could you please provide:

1. **Full source of `Sk0CubicBezier`** — particularly any `Dispose()` implementation and any place where `_blueP` might be reassigned or nulled out.
2. **SkiaSharp NuGet version** (check your `.csproj` or NuGet package manager).
3. **Is `Draw()` called from the UI thread**, or from a background/render thread?
4. **Is the crash deterministic** (every run) or intermittent?

One common cause of `AccessViolationException` with `SKPaint` is disposing the object (or the parent object) before the draw call. If `_blueP.Dispose()` or the enclosing class's `Dispose()` is called before `Draw()`, the private field's handle becomes invalid.

A quick test: does the crash still occur if you add `GC.KeepAlive(_blueP);` at the end of `Draw()`?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1790,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-05T11:27:12Z"
  },
  "summary": "Reporter sees AccessViolationException when calling DrawCircle with a private SKPaint field, but not when using a public field assigned from the same object.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.65
    },
    "area": {
      "value": "area/SkiaSharp",
      "confidence": 0.9
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
      "errorType": "exception",
      "errorMessage": "System.AccessViolationException",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [
        "canvas.DrawCircle(P0, 5, _blueP); // raises System.AccessViolationException\ncanvas.DrawCircle(P0, 5, Paint);  // does not crash (Paint = _blueP)"
      ],
      "stepsToReproduce": [
        "Create a class with private SKPaint _blueP and public SKPaint Paint = _blueP.",
        "Initialize _blueP with new SKPaint() and assign Paint = _blueP.",
        "In Draw(), call canvas.DrawCircle using _blueP — AVE occurs; same call using Paint succeeds."
      ]
    },
    "versionAnalysis": {
      "currentRelevance": "unknown",
      "relevanceReason": "No SkiaSharp version specified. Issue filed 2021 against .NET Framework 4.7.2 WPF."
    }
  },
  "analysis": {
    "summary": "The reported behavior—private field crashes while public field (referencing the same object) succeeds—is logically inconsistent under normal C# object semantics, suggesting that the reproduction is incomplete or the reporter has additional code that may reassign or dispose _blueP. SKPaint.DisposeNative calls sk_compatpaint_delete; if the handle is freed and reused this would explain an AVE. The pasted code contains 'miss code' placeholders, meaning the actual reproduction has undisclosed logic that could include disposal or reassignment of the private field.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint extends SKObject, ISKSkipObjectRegistration. DisposeNative calls sk_compatpaint_delete(Handle). Both _blueP and Paint reference the same managed object; if it were GC-collected or disposed, both would crash.",
        "relevance": "direct",
        "lines": "38-89"
      },
      {
        "file": "binding/SkiaSharp/SKPaint.cs",
        "finding": "SKPaint constructor calls sk_compatpaint_new_with_font(defaultFont.Handle) and throws if handle is zero, confirming creation succeeds. No evidence of a bug in SKPaint construction itself.",
        "relevance": "direct",
        "lines": "64-70"
      }
    ],
    "keySignals": [
      {
        "text": "canvas.DrawCircle(P0, 5, _blueP); // This raise System.AccessViolationException",
        "source": "issue body",
        "interpretation": "AVE when passing private field directly to native interop call."
      },
      {
        "text": "canvas.DrawCircle(P0, 5, Paint); // This not crash",
        "source": "issue body",
        "interpretation": "No crash when passing public field that was assigned from _blueP — both reference the same object, making this inconsistency suspicious."
      },
      {
        "text": "// miss code ... (I removed all not unnecessary code to reproduce)",
        "source": "issue body",
        "interpretation": "Reproduction is acknowledged to be incomplete, making root cause determination impossible without the full code."
      }
    ],
    "rationale": "Classified type/bug at moderate confidence because AVE is always a serious signal, but confidence is reduced because the stated behavior (private vs public field on the same object causing different outcomes) is not reproducible via the provided snippet alone. The removed code may contain disposal or reassignment of _blueP. Platform os/Windows-Classic applies because the reporter specifies WPF/.NET Framework 4.7.2. Severity is medium: AVE is serious but likely a user-side disposal or threading issue rather than a SkiaSharp internals bug.",
    "nextQuestions": [
      "What is the full, unredacted Sk0CubicBezier implementation? Specifically: is _blueP disposed or reassigned anywhere?",
      "What version of SkiaSharp is being used?",
      "Does the issue reproduce in a fresh minimal WPF project with only the code shown?",
      "Is the Draw() method called from the UI thread or a background thread?"
    ],
    "workarounds": [
      "Assign _blueP to a local variable before use: var p = _blueP; canvas.DrawCircle(P0, 5, p);",
      "Ensure _blueP is not disposed before Draw() is called."
    ],
    "resolution": {
      "hypothesis": "The omitted code in Sk0CubicBezier likely disposes or reassigns _blueP after construction, leaving a dangling handle. Accessing the private field directly in Draw() then causes AVE. The public Paint field may incidentally survive because of a separate code path.",
      "proposals": [
        {
          "category": "investigation",
          "title": "Request full reproduction",
          "description": "Ask the reporter to provide the full Sk0CubicBezier class without redactions, a SkiaSharp version, and confirmation that no disposal of _blueP occurs before Draw().",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "p1",
      "recommendedReason": "Cannot determine root cause without the missing code sections."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The reproduction code is explicitly incomplete ('miss code' comments). The described behavior—private field crashes, public field (same reference) works—is logically inconsistent without additional disposal/reassignment logic. Cannot reproduce or investigate further without the full code and SkiaSharp version.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Complete, unredacted source of Sk0CubicBezier (especially any Dispose implementations).",
      "SkiaSharp NuGet package version.",
      "Whether the crash is deterministic or intermittent.",
      "Whether Draw() is called from the UI thread."
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/reliability",
        "risk": "low",
        "confidence": 0.85,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for full code, SkiaSharp version, and threading context.",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for reporting this!\n\nThe behavior you describe — where `_blueP` crashes but `Paint` (which is assigned `= _blueP`) works — is very unusual because both fields should reference the exact same managed object and native handle.\n\nTo investigate further, could you please provide:\n\n1. **Full source of `Sk0CubicBezier`** — particularly any `Dispose()` implementation and any place where `_blueP` might be reassigned or nulled out.\n2. **SkiaSharp NuGet version** (check your `.csproj` or NuGet package manager).\n3. **Is `Draw()` called from the UI thread**, or from a background/render thread?\n4. **Is the crash deterministic** (every run) or intermittent?\n\nOne common cause of `AccessViolationException` with `SKPaint` is disposing the object (or the parent object) before the draw call. If `_blueP.Dispose()` or the enclosing class's `Dispose()` is called before `Draw()`, the private field's handle becomes invalid.\n\nA quick test: does the crash still occur if you add `GC.KeepAlive(_blueP);` at the end of `Draw()`?"
      }
    ]
  }
}
```

</details>
