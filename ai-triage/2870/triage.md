# Issue Triage Report — #2870

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:50:00Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/SkiaSharp (0.95 (95%)) |
| Suggested action | ready-to-fix (0.93 (93%)) |

**Issue Summary:** On .NET Framework 4.7.2 (x64), the native libSkiaSharp.dll fails to load when the application path contains non-ASCII Unicode characters (e.g., 'ő') because the Win32 LoadLibrary P/Invoke uses CharSet.Ansi instead of CharSet.Unicode.

**Analysis:** The root cause is in binding/Binding.Shared/LibraryLoader.cs: the Win32 inner class declares LoadLibrary with [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)], which resolves to LoadLibraryA (the ANSI variant). On Windows, LoadLibraryA cannot handle non-ASCII Unicode path characters — paths containing characters like 'ő' are silently truncated or mangled, causing the load to fail. The fix is to use LoadLibraryW (the wide/Unicode variant) by adding EntryPoint = "LoadLibraryW" and CharSet = CharSet.Unicode to the DllImport. This code path is active for net4x targets (controlled by USE_DELEGATES define). The .NET 5+ code path uses NativeLibrary.Load which handles Unicode correctly.

**Recommendations:** **ready-to-fix** — Root cause is confirmed in source code (CharSet.Ansi on Win32.LoadLibrary). The fix is a one-line change. Two users reproduced it independently. The correct pattern (LoadLibraryW) is already used elsewhere in the codebase.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/SkiaSharp |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | tenet/compatibility, tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Windows-Classic, area/SkiaSharp, tenet/compatibility, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a .NET Framework 4.7.2 x64 application that uses SkiaSharp 2.88.8
2. Place the application in a directory whose path contains a non-ASCII Unicode character (e.g., 'ő', 'á', 'ü')
3. Run the application
4. Observe DllNotFoundException when SkiaSharp attempts to load libSkiaSharp.dll

**Environment:** Windows 11 23H2 x64, .NET Framework 4.7.2, SkiaSharp 2.88.8, Visual Studio

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2870#issuecomment-2134445094 — Reporter self-identified root cause: LoadLibrary DllImport uses CharSet.Ansi instead of CharSet.Unicode

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | DllNotFoundException: Unable to load library 'libSkiaSharp' |
| Repro quality | complete |
| Target frameworks | net472 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3, 2.88.8 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The Win32.LoadLibrary CharSet.Ansi code in LibraryLoader.cs is present in the current codebase and has not been changed. |

## Analysis

### Technical Summary

The root cause is in binding/Binding.Shared/LibraryLoader.cs: the Win32 inner class declares LoadLibrary with [DllImport("Kernel32.dll", CharSet = CharSet.Ansi)], which resolves to LoadLibraryA (the ANSI variant). On Windows, LoadLibraryA cannot handle non-ASCII Unicode path characters — paths containing characters like 'ő' are silently truncated or mangled, causing the load to fail. The fix is to use LoadLibraryW (the wide/Unicode variant) by adding EntryPoint = "LoadLibraryW" and CharSet = CharSet.Unicode to the DllImport. This code path is active for net4x targets (controlled by USE_DELEGATES define). The .NET 5+ code path uses NativeLibrary.Load which handles Unicode correctly.

### Rationale

Reporter correctly identified the root cause: CharSet.Ansi on LoadLibrary DllImport means Windows calls LoadLibraryA which only accepts ANSI/MBCS paths. Paths with characters outside the current ANSI code page (e.g., Hungarian 'ő' not in Windows-1252 Western European) fail silently or are corrupted. A second user confirmed the issue. The fix is straightforward: use LoadLibraryW with CharSet.Unicode. This is a genuine bug affecting any user whose install path contains non-ASCII characters on Windows with .NET Framework.

### Key Signals

- "[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]
public static extern IntPtr LoadLibrary (string lpFileName);" — **binding/Binding.Shared/LibraryLoader.cs line 261-262** (Uses LoadLibraryA (ANSI) — cannot handle Unicode paths. Should use LoadLibraryW with CharSet.Unicode.)
- "Is there anything wrong with not having LoadLibraryW and CharSet = CharSet.Unicode implemented here?" — **issue comment by reporter** (Reporter self-diagnosed the root cause correctly.)
- "Confirming an issue. Any non-unicode character path on Windows causes loading fails." — **comment by EugeneZaitsev** (Second user independently reproduced the issue.)
- "<DefineConstants>$(DefineConstants);USE_DELEGATES</DefineConstants> for net4x" — **binding/SkiaSharp/SkiaSharp.csproj line 13** (The LibraryLoader Win32 code path is active only for .NET Framework (net4x targets), explaining why only .NET fx users are affected.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/Binding.Shared/LibraryLoader.cs` | 257-269 | direct | Win32 inner class defines LoadLibrary, GetProcAddress, FreeLibrary all with CharSet = CharSet.Ansi. LoadLibrary(CharSet.Ansi) resolves to kernel32!LoadLibraryA which rejects non-ANSI Unicode paths. GRGlInterface.cs already uses EntryPoint="LoadLibraryW" for its own library loading, showing the correct pattern exists in the codebase. |
| `binding/SkiaSharp/SkiaSharp.csproj` | 12-14 | direct | USE_DELEGATES is defined for net4x target frameworks, which activates the LibraryLoader Win32 code path. Modern .NET (net5+) does not use this path. |
| `binding/SkiaSharp/GRGlInterface.cs` | 141-142 | related | For .NET 7+ builds, GRGlInterface already uses LibraryImport with EntryPoint="LoadLibraryW" and StringMarshalling.Utf16 for loading ANGLE libraries — confirms that the Unicode-aware approach is known and used elsewhere. |
| `binding/Binding.Shared/LibraryLoader.cs` | 138-139 | direct | LoadLibrary method calls Win32.LoadLibrary(libraryPath) on Windows — this is the exact call site that fails for Unicode paths on .NET fx. |

### Workarounds

- Place the application in a directory with only ASCII characters in the path
- Use a short path (8.3 format) for the application directory on Windows

### Next Questions

- Does the same issue affect HarfBuzzSharp on .NET fx (it shares LibraryLoader.cs via #if HARFBUZZ)?
- Should GetProcAddress also be changed (though symbol names are ASCII-only, so less critical)?

### Resolution Proposals

**Hypothesis:** Changing CharSet.Ansi to CharSet.Unicode and adding EntryPoint = "LoadLibraryW" in the Win32.LoadLibrary DllImport in LibraryLoader.cs will fix the issue for .NET fx targets.

1. **Change LoadLibrary to use CharSet.Unicode / LoadLibraryW** — fix, confidence 0.95 (95%), cost/xs, validated=yes
   - In binding/Binding.Shared/LibraryLoader.cs, change the Win32.LoadLibrary DllImport from CharSet.Ansi to CharSet.Unicode and add EntryPoint = "LoadLibraryW" so that Windows uses the wide-character variant that supports full Unicode paths.

```csharp
[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW")]
public static extern IntPtr LoadLibrary (string lpFileName);
```

**Recommended proposal:** Change LoadLibrary to use CharSet.Unicode / LoadLibraryW

**Why:** One-line fix with very high confidence. Directly resolves the ANSI/Unicode mismatch. No side effects — LoadLibraryW is available on all supported Windows versions.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | ready-to-fix |
| Confidence | 0.93 (93%) |
| Reason | Root cause is confirmed in source code (CharSet.Ansi on Win32.LoadLibrary). The fix is a one-line change. Two users reproduced it independently. The correct pattern (LoadLibraryW) is already used elsewhere in the codebase. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.97 (97%) | Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility, tenet/reliability labels | labels=type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility, tenet/reliability |
| add-comment | medium | 0.93 (93%) | Confirm root cause and provide fix details | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the clear diagnosis! You've identified the root cause correctly.

In `binding/Binding.Shared/LibraryLoader.cs`, the `Win32` class calls `LoadLibrary` via P/Invoke with `CharSet = CharSet.Ansi`, which resolves to `kernel32!LoadLibraryA`. The ANSI variant cannot handle paths containing non-ASCII Unicode characters like `ő`.

The fix is a one-line change:
```csharp
[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = "LoadLibraryW")]
public static extern IntPtr LoadLibrary (string lpFileName);
```

This code path is only active for `.NET Framework` builds (`net4x` — controlled by `USE_DELEGATES`). .NET 5+ uses `NativeLibrary.Load` which handles Unicode correctly.

**Workaround in the meantime:** place your application in a directory with only ASCII characters in the path.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2870,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:50:00Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/SkiaSharp",
      "tenet/compatibility",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "On .NET Framework 4.7.2 (x64), the native libSkiaSharp.dll fails to load when the application path contains non-ASCII Unicode characters (e.g., 'ő') because the Win32 LoadLibrary P/Invoke uses CharSet.Ansi instead of CharSet.Unicode.",
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
    "tenets": [
      "tenet/compatibility",
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "DllNotFoundException: Unable to load library 'libSkiaSharp'",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Framework 4.7.2 x64 application that uses SkiaSharp 2.88.8",
        "Place the application in a directory whose path contains a non-ASCII Unicode character (e.g., 'ő', 'á', 'ü')",
        "Run the application",
        "Observe DllNotFoundException when SkiaSharp attempts to load libSkiaSharp.dll"
      ],
      "environmentDetails": "Windows 11 23H2 x64, .NET Framework 4.7.2, SkiaSharp 2.88.8, Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2870#issuecomment-2134445094",
          "description": "Reporter self-identified root cause: LoadLibrary DllImport uses CharSet.Ansi instead of CharSet.Unicode"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3",
        "2.88.8"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The Win32.LoadLibrary CharSet.Ansi code in LibraryLoader.cs is present in the current codebase and has not been changed."
    }
  },
  "analysis": {
    "summary": "The root cause is in binding/Binding.Shared/LibraryLoader.cs: the Win32 inner class declares LoadLibrary with [DllImport(\"Kernel32.dll\", CharSet = CharSet.Ansi)], which resolves to LoadLibraryA (the ANSI variant). On Windows, LoadLibraryA cannot handle non-ASCII Unicode path characters — paths containing characters like 'ő' are silently truncated or mangled, causing the load to fail. The fix is to use LoadLibraryW (the wide/Unicode variant) by adding EntryPoint = \"LoadLibraryW\" and CharSet = CharSet.Unicode to the DllImport. This code path is active for net4x targets (controlled by USE_DELEGATES define). The .NET 5+ code path uses NativeLibrary.Load which handles Unicode correctly.",
    "rationale": "Reporter correctly identified the root cause: CharSet.Ansi on LoadLibrary DllImport means Windows calls LoadLibraryA which only accepts ANSI/MBCS paths. Paths with characters outside the current ANSI code page (e.g., Hungarian 'ő' not in Windows-1252 Western European) fail silently or are corrupted. A second user confirmed the issue. The fix is straightforward: use LoadLibraryW with CharSet.Unicode. This is a genuine bug affecting any user whose install path contains non-ASCII characters on Windows with .NET Framework.",
    "keySignals": [
      {
        "text": "[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Ansi)]\npublic static extern IntPtr LoadLibrary (string lpFileName);",
        "source": "binding/Binding.Shared/LibraryLoader.cs line 261-262",
        "interpretation": "Uses LoadLibraryA (ANSI) — cannot handle Unicode paths. Should use LoadLibraryW with CharSet.Unicode."
      },
      {
        "text": "Is there anything wrong with not having LoadLibraryW and CharSet = CharSet.Unicode implemented here?",
        "source": "issue comment by reporter",
        "interpretation": "Reporter self-diagnosed the root cause correctly."
      },
      {
        "text": "Confirming an issue. Any non-unicode character path on Windows causes loading fails.",
        "source": "comment by EugeneZaitsev",
        "interpretation": "Second user independently reproduced the issue."
      },
      {
        "text": "<DefineConstants>$(DefineConstants);USE_DELEGATES</DefineConstants> for net4x",
        "source": "binding/SkiaSharp/SkiaSharp.csproj line 13",
        "interpretation": "The LibraryLoader Win32 code path is active only for .NET Framework (net4x targets), explaining why only .NET fx users are affected."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/Binding.Shared/LibraryLoader.cs",
        "lines": "257-269",
        "finding": "Win32 inner class defines LoadLibrary, GetProcAddress, FreeLibrary all with CharSet = CharSet.Ansi. LoadLibrary(CharSet.Ansi) resolves to kernel32!LoadLibraryA which rejects non-ANSI Unicode paths. GRGlInterface.cs already uses EntryPoint=\"LoadLibraryW\" for its own library loading, showing the correct pattern exists in the codebase.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "lines": "12-14",
        "finding": "USE_DELEGATES is defined for net4x target frameworks, which activates the LibraryLoader Win32 code path. Modern .NET (net5+) does not use this path.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRGlInterface.cs",
        "lines": "141-142",
        "finding": "For .NET 7+ builds, GRGlInterface already uses LibraryImport with EntryPoint=\"LoadLibraryW\" and StringMarshalling.Utf16 for loading ANGLE libraries — confirms that the Unicode-aware approach is known and used elsewhere.",
        "relevance": "related"
      },
      {
        "file": "binding/Binding.Shared/LibraryLoader.cs",
        "lines": "138-139",
        "finding": "LoadLibrary method calls Win32.LoadLibrary(libraryPath) on Windows — this is the exact call site that fails for Unicode paths on .NET fx.",
        "relevance": "direct"
      }
    ],
    "nextQuestions": [
      "Does the same issue affect HarfBuzzSharp on .NET fx (it shares LibraryLoader.cs via #if HARFBUZZ)?",
      "Should GetProcAddress also be changed (though symbol names are ASCII-only, so less critical)?"
    ],
    "workarounds": [
      "Place the application in a directory with only ASCII characters in the path",
      "Use a short path (8.3 format) for the application directory on Windows"
    ],
    "resolution": {
      "hypothesis": "Changing CharSet.Ansi to CharSet.Unicode and adding EntryPoint = \"LoadLibraryW\" in the Win32.LoadLibrary DllImport in LibraryLoader.cs will fix the issue for .NET fx targets.",
      "proposals": [
        {
          "title": "Change LoadLibrary to use CharSet.Unicode / LoadLibraryW",
          "description": "In binding/Binding.Shared/LibraryLoader.cs, change the Win32.LoadLibrary DllImport from CharSet.Ansi to CharSet.Unicode and add EntryPoint = \"LoadLibraryW\" so that Windows uses the wide-character variant that supports full Unicode paths.",
          "category": "fix",
          "codeSnippet": "[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = \"LoadLibraryW\")]\npublic static extern IntPtr LoadLibrary (string lpFileName);",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Change LoadLibrary to use CharSet.Unicode / LoadLibraryW",
      "recommendedReason": "One-line fix with very high confidence. Directly resolves the ANSI/Unicode mismatch. No side effects — LoadLibraryW is available on all supported Windows versions."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "ready-to-fix",
      "confidence": 0.93,
      "reason": "Root cause is confirmed in source code (CharSet.Ansi on Win32.LoadLibrary). The fix is a one-line change. Two users reproduced it independently. The correct pattern (LoadLibraryW) is already used elsewhere in the codebase.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/SkiaSharp, os/Windows-Classic, tenet/compatibility, tenet/reliability labels",
        "risk": "low",
        "confidence": 0.97,
        "labels": [
          "type/bug",
          "area/SkiaSharp",
          "os/Windows-Classic",
          "tenet/compatibility",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Confirm root cause and provide fix details",
        "risk": "medium",
        "confidence": 0.93,
        "comment": "Thanks for the clear diagnosis! You've identified the root cause correctly.\n\nIn `binding/Binding.Shared/LibraryLoader.cs`, the `Win32` class calls `LoadLibrary` via P/Invoke with `CharSet = CharSet.Ansi`, which resolves to `kernel32!LoadLibraryA`. The ANSI variant cannot handle paths containing non-ASCII Unicode characters like `ő`.\n\nThe fix is a one-line change:\n```csharp\n[DllImport (SystemLibrary, SetLastError = true, CharSet = CharSet.Unicode, EntryPoint = \"LoadLibraryW\")]\npublic static extern IntPtr LoadLibrary (string lpFileName);\n```\n\nThis code path is only active for `.NET Framework` builds (`net4x` — controlled by `USE_DELEGATES`). .NET 5+ uses `NativeLibrary.Load` which handles Unicode correctly.\n\n**Workaround in the meantime:** place your application in a directory with only ASCII characters in the path."
      }
    ]
  }
}
```

</details>
