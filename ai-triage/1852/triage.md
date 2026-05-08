# Issue Triage Report — #1852

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T22:44:51Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/Build (0.92 (92%)) |
| Suggested action | close-as-fixed (0.82 (82%)) |

**Issue Summary:** SkiaSharpGenerator fails on macOS with 'stdint.h' file not found when parsing Skia C headers via CppAst, because the system include paths for the macOS SDK and Clang headers were not configured.

**Analysis:** The SkiaSharpGenerator developer tool failed on macOS because CppAst (the C/C++ header parser) could not find standard C system headers such as stdint.h. In 2021 the generator only configured include paths for Windows. The current codebase (BaseTool.cs lines 53–116) adds macOS-specific logic: it discovers the versioned Clang include directory under CommandLineTools and uses xcrun to find the active macOS SDK include path. This fully addresses the reported root cause.

**Recommendations:** **close-as-fixed** — The current BaseTool.cs contains macOS-specific Clang and SDK include path detection that directly resolves the reported stdint.h error. The issue was filed in 2021 and the code has since been updated. The reporter's workaround (using Windows) is no longer necessary.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/macOS |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Clone SkiaSharp on macOS
2. Run: dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.json --skia externals/skia --output binding/Binding/SkiaApi.generated.cs
3. Observe error: 'stdint.h' file not found

**Environment:** macOS (Apple Silicon or Intel), Xcode/CommandLineTools installed, SkiaSharp source checkout (2021-11-01)

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | low |
| Regression claimed | False |
| Error type | build-error |
| Error message | ERROR: 'stdint.h' file not found at externals/skia/./include/c/sk_types.h(13, 10) |
| Repro quality | complete |
| Target frameworks | net10.0 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.88 (88%) |
| Reason | BaseTool.cs now contains macOS-specific include path detection: it searches /Library/Developer/CommandLineTools/usr/lib/clang/ for Clang headers and calls 'xcrun --show-sdk-path' to find the macOS SDK usr/include path. The original issue was caused by the absence of these include paths. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

The SkiaSharpGenerator developer tool failed on macOS because CppAst (the C/C++ header parser) could not find standard C system headers such as stdint.h. In 2021 the generator only configured include paths for Windows. The current codebase (BaseTool.cs lines 53–116) adds macOS-specific logic: it discovers the versioned Clang include directory under CommandLineTools and uses xcrun to find the active macOS SDK include path. This fully addresses the reported root cause.

### Rationale

The error 'stdint.h' file not found is a classic symptom of missing system include paths when invoking a C parser. The maintainer confirmed in 2021 the tool only worked on Windows. The current BaseTool.cs now includes macOS SDK path discovery, strongly indicating the issue has been fixed in the interim. The fix quality warrants close-as-fixed rather than needs-investigation.

### Key Signals

- "ERROR: 'stdint.h' file not found at externals/skia/./include/c/sk_types.h(13, 10)" — **issue body** (CppAst could not resolve the standard C integer type header — missing system include path for macOS SDK or Clang built-ins.)
- "I think I only got it to work on windows. You also need vs installed I think. I never actually used it on mac" — **comment by mattleibow** (Maintainer confirmed the macOS path was unsupported in 2021; acknowledged Clang path configuration was needed.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `utils/SkiaSharpGenerator/BaseTool.cs` | 53-116 | direct | macOS-specific block added: searches /Library/Developer/CommandLineTools/usr/lib/clang/ for versioned Clang include dirs and calls xcrun --show-sdk-path to locate the macOS SDK usr/include directory. Both are added to CppParserOptions.SystemIncludeFolders, directly resolving the stdint.h search failure. |
| `utils/SkiaSharpGenerator/BaseTool.cs` | 118-149 | related | Linux-specific block calls 'clang -print-resource-dir' and also adds /usr/include/x86_64-linux-gnu and /usr/include as fallbacks. The parallel structure confirms the macOS block is intentional parity work. |
| `utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj` | 1-22 | context | Project targets net10.0 and uses CppAst 0.21.4 for header parsing. No Windows-only constraints in the project file. |

### Next Questions

- Was there a specific PR or commit that added the macOS include path detection in BaseTool.cs?
- Does the generator now run successfully end-to-end on macOS with only Xcode Command Line Tools (no full Xcode.app)?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.82 (82%) |
| Reason | The current BaseTool.cs contains macOS-specific Clang and SDK include path detection that directly resolves the reported stdint.h error. The issue was filed in 2021 and the code has since been updated. The reporter's workaround (using Windows) is no longer necessary. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/Build, and os/macOS labels | labels=type/bug, area/Build, os/macOS |
| add-comment | medium | 0.82 (82%) | Notify reporter that macOS support was added and the issue is likely fixed | — |
| close-issue | medium | 0.82 (82%) | Close as fixed — root cause addressed in current codebase | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this! The `SkiaSharpGenerator` now includes macOS-specific include path detection in `BaseTool.cs`: it searches for the versioned Clang include directory under `/Library/Developer/CommandLineTools/usr/lib/clang/` and uses `xcrun --show-sdk-path` to locate the macOS SDK headers.

This should resolve the `'stdint.h' file not found` error. Please make sure you have Xcode Command Line Tools installed (`xcode-select --install`) and try running the generator again. If the issue persists with the latest code, please reopen with updated environment details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1852,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T22:44:51Z"
  },
  "summary": "SkiaSharpGenerator fails on macOS with 'stdint.h' file not found when parsing Skia C headers via CppAst, because the system include paths for the macOS SDK and Clang headers were not configured.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.92
    },
    "platforms": [
      "os/macOS"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "low",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "ERROR: 'stdint.h' file not found at externals/skia/./include/c/sk_types.h(13, 10)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Clone SkiaSharp on macOS",
        "Run: dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.json --skia externals/skia --output binding/Binding/SkiaApi.generated.cs",
        "Observe error: 'stdint.h' file not found"
      ],
      "environmentDetails": "macOS (Apple Silicon or Intel), Xcode/CommandLineTools installed, SkiaSharp source checkout (2021-11-01)"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.88,
      "reason": "BaseTool.cs now contains macOS-specific include path detection: it searches /Library/Developer/CommandLineTools/usr/lib/clang/ for Clang headers and calls 'xcrun --show-sdk-path' to find the macOS SDK usr/include path. The original issue was caused by the absence of these include paths.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "The SkiaSharpGenerator developer tool failed on macOS because CppAst (the C/C++ header parser) could not find standard C system headers such as stdint.h. In 2021 the generator only configured include paths for Windows. The current codebase (BaseTool.cs lines 53–116) adds macOS-specific logic: it discovers the versioned Clang include directory under CommandLineTools and uses xcrun to find the active macOS SDK include path. This fully addresses the reported root cause.",
    "rationale": "The error 'stdint.h' file not found is a classic symptom of missing system include paths when invoking a C parser. The maintainer confirmed in 2021 the tool only worked on Windows. The current BaseTool.cs now includes macOS SDK path discovery, strongly indicating the issue has been fixed in the interim. The fix quality warrants close-as-fixed rather than needs-investigation.",
    "codeInvestigation": [
      {
        "file": "utils/SkiaSharpGenerator/BaseTool.cs",
        "lines": "53-116",
        "finding": "macOS-specific block added: searches /Library/Developer/CommandLineTools/usr/lib/clang/ for versioned Clang include dirs and calls xcrun --show-sdk-path to locate the macOS SDK usr/include directory. Both are added to CppParserOptions.SystemIncludeFolders, directly resolving the stdint.h search failure.",
        "relevance": "direct"
      },
      {
        "file": "utils/SkiaSharpGenerator/BaseTool.cs",
        "lines": "118-149",
        "finding": "Linux-specific block calls 'clang -print-resource-dir' and also adds /usr/include/x86_64-linux-gnu and /usr/include as fallbacks. The parallel structure confirms the macOS block is intentional parity work.",
        "relevance": "related"
      },
      {
        "file": "utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj",
        "lines": "1-22",
        "finding": "Project targets net10.0 and uses CppAst 0.21.4 for header parsing. No Windows-only constraints in the project file.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "ERROR: 'stdint.h' file not found at externals/skia/./include/c/sk_types.h(13, 10)",
        "source": "issue body",
        "interpretation": "CppAst could not resolve the standard C integer type header — missing system include path for macOS SDK or Clang built-ins."
      },
      {
        "text": "I think I only got it to work on windows. You also need vs installed I think. I never actually used it on mac",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer confirmed the macOS path was unsupported in 2021; acknowledged Clang path configuration was needed."
      }
    ],
    "nextQuestions": [
      "Was there a specific PR or commit that added the macOS include path detection in BaseTool.cs?",
      "Does the generator now run successfully end-to-end on macOS with only Xcode Command Line Tools (no full Xcode.app)?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.82,
      "reason": "The current BaseTool.cs contains macOS-specific Clang and SDK include path detection that directly resolves the reported stdint.h error. The issue was filed in 2021 and the code has since been updated. The reporter's workaround (using Windows) is no longer necessary.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/Build, and os/macOS labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build",
          "os/macOS"
        ]
      },
      {
        "type": "add-comment",
        "description": "Notify reporter that macOS support was added and the issue is likely fixed",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for reporting this! The `SkiaSharpGenerator` now includes macOS-specific include path detection in `BaseTool.cs`: it searches for the versioned Clang include directory under `/Library/Developer/CommandLineTools/usr/lib/clang/` and uses `xcrun --show-sdk-path` to locate the macOS SDK headers.\n\nThis should resolve the `'stdint.h' file not found` error. Please make sure you have Xcode Command Line Tools installed (`xcode-select --install`) and try running the generator again. If the issue persists with the latest code, please reopen with updated environment details."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed — root cause addressed in current codebase",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
