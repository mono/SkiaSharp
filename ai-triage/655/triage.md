# Issue Triage Report — #655

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-28T07:16:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** SKPath.Op() with SKPathOp.Intersect returns a result path with 0 points for specific coordinate values and mixed fill types (Winding + EvenOdd), while the same operation works correctly in the Skia C++ fiddle.

**Analysis:** The SkiaSharp C# wrapper correctly delegates path boolean operations to the native sk_pathop_op C API. The bug is in Skia's SkPathOps C++ implementation, which fails to compute the intersection of two rectangles when they use different fill types (Winding vs EvenOdd) and involve near-zero floating-point coordinate values.

**Recommendations:** **needs-investigation** — The bug is in the native Skia pathops library. The issue is 7+ years old with a status/skia-update-required label. Need to verify if still reproducible with current SkiaSharp before deciding to close or escalate.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, status/skia-update-required, area/libSkiaSharp.native |

## Evidence

### Reproduction

1. Create path_a with Winding fill type using hex float coordinates forming a rectangle
2. Create path_b with EvenOdd fill type using hex float coordinates forming a nearly-identical rectangle
3. Call path_a.Op(path_b, SKPathOp.Intersect)
4. Check path_r.PointCount — result is 0 instead of expected intersection points

**Environment:** SkiaSharp 1.60.3, Visual Studio 2017, .NET Core 2.1, Windows 10

**Repository links:**
- https://fiddle.skia.org/c/cde611c02e7cfb956670649610eb40d3 — Skia C++ fiddle demonstrating correct intersection result
- https://groups.google.com/g/skia-discuss/c/7kp0d5dcQq4/m/oGKRxz_xAgAJ — Skia discussion group thread about path operations (linked in comment June 2025)

**Code snippets:**

```csharp
SKPath path_r = path_a.Op(path_b, SKPathOp.Intersect);
Console.WriteLine(path_r.PointCount); // prints 0
```

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | wrong-output |
| Error message | path_r.PointCount == 0 (expected non-zero intersection) |
| Repro quality | complete |
| Target frameworks | net-core-2.1 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.60.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unknown |
| Relevance reason | Issue was filed with 1.60.3 in 2018 and labeled status/skia-update-required. The 2025 comment suggests ongoing community interest; whether Skia's pathops has been fixed in subsequent updates is unknown without running the repro. |

## Analysis

### Technical Summary

The SkiaSharp C# wrapper correctly delegates path boolean operations to the native sk_pathop_op C API. The bug is in Skia's SkPathOps C++ implementation, which fails to compute the intersection of two rectangles when they use different fill types (Winding vs EvenOdd) and involve near-zero floating-point coordinate values.

### Rationale

The C# wrapper at SKPath.Op() is structurally sound — it validates arguments and delegates to sk_pathop_op. The P/Invoke binding is generated and correct. The discrepancy (C++ fiddle works, C# returns 0 points) isolates the bug to the native Skia pathops library. The existing label status/skia-update-required confirms prior maintainer assessment. The issue is medium severity: it produces wrong output but only for specific coordinate combinations; many path operations work correctly.

### Key Signals

- "The same example in C++ is working well. You can try it in the next fiddle record." — **issue body** (The C# binding is correctly passing through to native code. Since C++ works in Skia directly, the issue is in how SkiaSharp's bundled Skia version handles this pathop, not the C# wrapper.)
- "Result path contains no points. It writes 0 on console." — **issue body** (The Op() call succeeds (returns non-null) but the result path has 0 points — wrong-output rather than a crash or null return.)
- "path_b.FillType = SKPathFillType.EvenOdd" — **issue body** (Mixed fill types (Winding for path_a, EvenOdd for path_b) combined with near-zero float coordinates (e.g., 0xB6D27CA9 ≈ -1.6e-8) may trigger a degenerate case in Skia's boolean ops.)
- "status/skia-update-required label already applied" — **existing labels** (Maintainers previously identified this as a Skia upstream bug awaiting a fix in a Skia update.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKPath.cs` | 276-295 | direct | Op() method validates non-null arguments and delegates to SkiaApi.sk_pathop_op(Handle, other.Handle, op, result.Handle). The wrapper is correct — there is no bug here. |
| `binding/SkiaSharp/SkiaApi.generated.cs` | 10209-10228 | direct | sk_pathop_op is a generated P/Invoke binding to the native sk_pathop_op C function. The signature matches (sk_path_t* one, sk_path_t* two, sk_pathop_t op, sk_path_t* result). No issues with the binding. |

### Next Questions

- Is this still reproducible with the current SkiaSharp (3.x) version?
- Did Skia's SkPathOps fix the mixed fill type intersection issue in a subsequent milestone?
- Does the issue reproduce on Linux/macOS or only Windows?

### Resolution Proposals

**Hypothesis:** Skia's SkPathOps algorithm has a degenerate case with near-zero coordinates combined with mixed Winding/EvenOdd fill types. This was a known upstream Skia bug (status/skia-update-required). Whether it has been fixed in more recent Skia milestones needs verification.

1. **Verify repro with current SkiaSharp version** — investigation, confidence 0.85 (85%), cost/xs, validated=untested
   - Run the reporter's code against the current SkiaSharp release. If it now works, close as fixed. If still failing, escalate to Skia upstream.
2. **Workaround: convert both paths to Winding before intersection** — workaround, confidence 0.65 (65%), cost/xs, validated=untested
   - Call path_b.ToWinding() to normalize fill types before performing the intersection operation. Mixing fill types in boolean ops can trigger degenerate cases in older Skia versions.

**Recommended proposal:** Verify repro with current SkiaSharp version

**Why:** Fastest path to resolution. Issue is 7+ years old and labeled as requiring a Skia update — the fix may already be in shipping versions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The bug is in the native Skia pathops library. The issue is 7+ years old with a status/skia-update-required label. Need to verify if still reproducible with current SkiaSharp before deciding to close or escalate. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Confirm existing labels are correct (type/bug, area/libSkiaSharp.native); add tenet/reliability | labels=type/bug, area/libSkiaSharp.native, tenet/reliability |
| add-comment | medium | 0.80 (80%) | Request confirmation whether this is still reproducible with current SkiaSharp, and suggest fill-type normalization workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed repro case. This was previously labeled as `status/skia-update-required` — the bug is in Skia's SkPathOps algorithm rather than the C# wrapper.

Could you try this with a current SkiaSharp release (3.x)? Skia's path operations have been updated significantly since 1.60.3.

As a potential workaround while investigating, try normalizing both paths to the same fill type before the operation:

```csharp
// Normalize both paths to Winding before intersection
var path_b_winding = path_b.ToWinding();
var path_r = path_a.Op(path_b_winding ?? path_b, SKPathOp.Intersect);
```

Mixing `Winding` and `EvenOdd` fill types in boolean operations can trigger degenerate cases in the pathops algorithm. If normalizing fill types resolves the issue, that confirms the root cause.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 655,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-28T07:16:00Z",
    "currentLabels": [
      "type/bug",
      "status/skia-update-required",
      "area/libSkiaSharp.native"
    ]
  },
  "summary": "SKPath.Op() with SKPathOp.Intersect returns a result path with 0 points for specific coordinate values and mixed fill types (Winding + EvenOdd), while the same operation works correctly in the Skia C++ fiddle.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.97
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
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
      "errorType": "wrong-output",
      "errorMessage": "path_r.PointCount == 0 (expected non-zero intersection)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net-core-2.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create path_a with Winding fill type using hex float coordinates forming a rectangle",
        "Create path_b with EvenOdd fill type using hex float coordinates forming a nearly-identical rectangle",
        "Call path_a.Op(path_b, SKPathOp.Intersect)",
        "Check path_r.PointCount — result is 0 instead of expected intersection points"
      ],
      "environmentDetails": "SkiaSharp 1.60.3, Visual Studio 2017, .NET Core 2.1, Windows 10",
      "repoLinks": [
        {
          "url": "https://fiddle.skia.org/c/cde611c02e7cfb956670649610eb40d3",
          "description": "Skia C++ fiddle demonstrating correct intersection result"
        },
        {
          "url": "https://groups.google.com/g/skia-discuss/c/7kp0d5dcQq4/m/oGKRxz_xAgAJ",
          "description": "Skia discussion group thread about path operations (linked in comment June 2025)"
        }
      ],
      "codeSnippets": [
        "SKPath path_r = path_a.Op(path_b, SKPathOp.Intersect);\nConsole.WriteLine(path_r.PointCount); // prints 0"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.60.3"
      ],
      "currentRelevance": "unknown",
      "relevanceReason": "Issue was filed with 1.60.3 in 2018 and labeled status/skia-update-required. The 2025 comment suggests ongoing community interest; whether Skia's pathops has been fixed in subsequent updates is unknown without running the repro."
    }
  },
  "analysis": {
    "summary": "The SkiaSharp C# wrapper correctly delegates path boolean operations to the native sk_pathop_op C API. The bug is in Skia's SkPathOps C++ implementation, which fails to compute the intersection of two rectangles when they use different fill types (Winding vs EvenOdd) and involve near-zero floating-point coordinate values.",
    "rationale": "The C# wrapper at SKPath.Op() is structurally sound — it validates arguments and delegates to sk_pathop_op. The P/Invoke binding is generated and correct. The discrepancy (C++ fiddle works, C# returns 0 points) isolates the bug to the native Skia pathops library. The existing label status/skia-update-required confirms prior maintainer assessment. The issue is medium severity: it produces wrong output but only for specific coordinate combinations; many path operations work correctly.",
    "keySignals": [
      {
        "text": "The same example in C++ is working well. You can try it in the next fiddle record.",
        "source": "issue body",
        "interpretation": "The C# binding is correctly passing through to native code. Since C++ works in Skia directly, the issue is in how SkiaSharp's bundled Skia version handles this pathop, not the C# wrapper."
      },
      {
        "text": "Result path contains no points. It writes 0 on console.",
        "source": "issue body",
        "interpretation": "The Op() call succeeds (returns non-null) but the result path has 0 points — wrong-output rather than a crash or null return."
      },
      {
        "text": "path_b.FillType = SKPathFillType.EvenOdd",
        "source": "issue body",
        "interpretation": "Mixed fill types (Winding for path_a, EvenOdd for path_b) combined with near-zero float coordinates (e.g., 0xB6D27CA9 ≈ -1.6e-8) may trigger a degenerate case in Skia's boolean ops."
      },
      {
        "text": "status/skia-update-required label already applied",
        "source": "existing labels",
        "interpretation": "Maintainers previously identified this as a Skia upstream bug awaiting a fix in a Skia update."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKPath.cs",
        "lines": "276-295",
        "finding": "Op() method validates non-null arguments and delegates to SkiaApi.sk_pathop_op(Handle, other.Handle, op, result.Handle). The wrapper is correct — there is no bug here.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaApi.generated.cs",
        "lines": "10209-10228",
        "finding": "sk_pathop_op is a generated P/Invoke binding to the native sk_pathop_op C function. The signature matches (sk_path_t* one, sk_path_t* two, sk_pathop_t op, sk_path_t* result). No issues with the binding.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Is this still reproducible with the current SkiaSharp (3.x) version?",
      "Did Skia's SkPathOps fix the mixed fill type intersection issue in a subsequent milestone?",
      "Does the issue reproduce on Linux/macOS or only Windows?"
    ],
    "resolution": {
      "hypothesis": "Skia's SkPathOps algorithm has a degenerate case with near-zero coordinates combined with mixed Winding/EvenOdd fill types. This was a known upstream Skia bug (status/skia-update-required). Whether it has been fixed in more recent Skia milestones needs verification.",
      "proposals": [
        {
          "title": "Verify repro with current SkiaSharp version",
          "description": "Run the reporter's code against the current SkiaSharp release. If it now works, close as fixed. If still failing, escalate to Skia upstream.",
          "category": "investigation",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Workaround: convert both paths to Winding before intersection",
          "description": "Call path_b.ToWinding() to normalize fill types before performing the intersection operation. Mixing fill types in boolean ops can trigger degenerate cases in older Skia versions.",
          "category": "workaround",
          "confidence": 0.65,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Verify repro with current SkiaSharp version",
      "recommendedReason": "Fastest path to resolution. Issue is 7+ years old and labeled as requiring a Skia update — the fix may already be in shipping versions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The bug is in the native Skia pathops library. The issue is 7+ years old with a status/skia-update-required label. Need to verify if still reproducible with current SkiaSharp before deciding to close or escalate.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Confirm existing labels are correct (type/bug, area/libSkiaSharp.native); add tenet/reliability",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Request confirmation whether this is still reproducible with current SkiaSharp, and suggest fill-type normalization workaround",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for the detailed repro case. This was previously labeled as `status/skia-update-required` — the bug is in Skia's SkPathOps algorithm rather than the C# wrapper.\n\nCould you try this with a current SkiaSharp release (3.x)? Skia's path operations have been updated significantly since 1.60.3.\n\nAs a potential workaround while investigating, try normalizing both paths to the same fill type before the operation:\n\n```csharp\n// Normalize both paths to Winding before intersection\nvar path_b_winding = path_b.ToWinding();\nvar path_r = path_a.Op(path_b_winding ?? path_b, SKPathOp.Intersect);\n```\n\nMixing `Winding` and `EvenOdd` fill types in boolean operations can trigger degenerate cases in the pathops algorithm. If normalizing fill types resolves the issue, that confirms the root cause."
      }
    ]
  }
}
```

</details>
