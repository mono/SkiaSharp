# Issue Triage Report — #3040

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:36:49Z |
| Type | type/question (0.78 (78%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-info (0.80 (80%)) |

**Issue Summary:** User is calling C# SkiaSharp code from Python via pythonnet (clr), getting DllNotFoundException for libSkiaSharp because the native Win32 DLL is not on Windows' DLL search path when running under Python — a deployment/configuration issue specific to the pythonnet scenario.

**Analysis:** The user is loading SkiaSharp managed assemblies from the .nuget packages cache using pythonnet, but the native libSkiaSharp.dll is not in the lib/netstandard2.0 folder — it lives under runtimes/win-x64/native/ in the NuGet package. When running through Python, Windows DLL search order applies (process directory, PATH, current directory, system dirs), and libSkiaSharp.dll must be in one of those locations. The user tried placing the DLL next to their managed DLLs but failed; this likely means an architecture mismatch (32-bit Python loading 64-bit DLL) or VC++ runtime missing, or incorrect directory.

**Recommendations:** **needs-info** — The maintainer's guidance was correct but the user still fails after placing the DLL. To diagnose further we need to know the Python architecture (32-bit vs 64-bit) and exactly where the DLL was placed. The architecture mismatch is the most likely cause of continued failure.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug, type/question, os/Windows-Classic, area/libSkiaSharp.native, status/needs-attention, triage/triaged |

## Evidence

### Reproduction

1. Create a .NET Standard 2.0 library using SkiaSharp 2.88.8
2. Load managed DLLs from .nuget packages folder via pythonnet clr.AddReference()
3. Attempt to call SkiaSharp API from Python
4. DllNotFoundException is thrown for libSkiaSharp

**Environment:** Windows, Python 3.9 + pythonnet (clr), SkiaSharp 2.88.8, .NET Standard 2.0, Visual Studio Code

**Repository links:**
- https://github.com/user-attachments/files/17703961/PythonWrapper.zip — Zip file with .NET application and Python code uploaded by reporter

**Code snippets:**

```csharp
clr.AddReference(skiasharp_dll)  # loads SkiaSharp.dll from lib/netstandard2.0 - no native binary
```

```csharp
DllNotFoundException: Unable to load DLL 'libSkiaSharp': The specified module could not be found. (Exception from HRESULT: 0x8007007E)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.8, 6.88.6 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The issue is about native asset deployment, not a SkiaSharp API change. The 6.88.6 'Last Known Good' is deprecated and likely worked by coincidence — possibly Python's environment happened to have libSkiaSharp.dll on its PATH from another installation. |

## Analysis

### Technical Summary

The user is loading SkiaSharp managed assemblies from the .nuget packages cache using pythonnet, but the native libSkiaSharp.dll is not in the lib/netstandard2.0 folder — it lives under runtimes/win-x64/native/ in the NuGet package. When running through Python, Windows DLL search order applies (process directory, PATH, current directory, system dirs), and libSkiaSharp.dll must be in one of those locations. The user tried placing the DLL next to their managed DLLs but failed; this likely means an architecture mismatch (32-bit Python loading 64-bit DLL) or VC++ runtime missing, or incorrect directory.

### Rationale

This is a usage/configuration question — SkiaSharp itself is not broken. P/Invoke on Windows uses Windows DLL search order, which doesn't automatically discover files inside NuGet package subfolders. Pythonnet bypasses the normal .NET publish pipeline that would copy native assets. Classified as type/question because the behavior is by-design; the root cause is an environment deployment gap in the pythonnet workflow.

### Key Signals

- "Unable to load DLL 'libSkiaSharp': The specified module could not be found. (Exception from HRESULT: 0x8007007E)" — **issue body — error log** (Windows error 0x8007007E (ERROR_MOD_NOT_FOUND) means the DLL file was not found on any of the Windows DLL search paths. This is a deployment issue, not a SkiaSharp defect.)
- "You are loading the managed (.NET) dlls, but have not made sure the native parts are able to be found. You can't reference them directly via the CLR, but if they are in the same folder as the managed dlls, they should be found" — **maintainer comment (mattleibow)** (Maintainer correctly identified the root cause: managed DLLs load fine but native libSkiaSharp.dll is missing from discovery paths.)
- "We are still getting the exception after including the native libSkiaSharp.dll" — **comment by DharanyaSakthivel-SF4210** (User placed the native DLL but it still fails. Possible causes: architecture mismatch (Python x86 vs Win32 x64 DLL), wrong directory, or missing VC++ redistributable.)
- "Last Known Good Version of SkiaSharp: 6.88.6 (Deprecated)" — **issue body** (Not a true regression — 6.88.6 is very old and likely worked due to libSkiaSharp.dll being present on PATH from prior installation or different binary naming.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 12 | direct | DllImport constant is 'libSkiaSharp' which on Windows resolves to libSkiaSharp.dll via Windows DLL search order (process directory, PATH, current directory, system dirs). pythonnet does not alter this search path, so the DLL must be explicitly placed in a discoverable location. |
| `binding/SkiaSharp/SkiaSharpVersion.cs` | 22-30 | direct | sk_version_get_milestone() is called in SkiaSharpVersion.Native property, which is invoked from SKObject..cctor() — matches the stack trace exactly (SKObject type initializer fails with DllNotFoundException on the first P/Invoke call). |

### Workarounds

- Publish the .NET app with dotnet publish and use all DLLs from the publish output folder; this copies libSkiaSharp.dll alongside managed assemblies
- Copy libSkiaSharp.dll (from SkiaSharp.NativeAssets.Win32 NuGet, runtimes/win-x64/native/ subfolder) to the same directory as the Python script
- Add the directory containing libSkiaSharp.dll to the Windows PATH environment variable before running Python
- Use Python 64-bit if copying the x64 DLL; or use the x86 DLL if using 32-bit Python (architecture must match)

### Next Questions

- What is the architecture of the Python installation (32-bit vs 64-bit)?
- Exactly which directory did the user copy libSkiaSharp.dll to when it still failed?
- Is the VC++ 2019/2022 redistributable installed on the Windows machine?
- What is the Python working directory when running the script?

### Resolution Proposals

**Hypothesis:** The native libSkiaSharp.dll cannot be found by Windows DLL loader because: (a) it's not in the Python process directory or PATH, OR (b) architecture mismatch between Python bitness and the DLL bitness, OR (c) VC++ redistributable is missing.

1. **Copy DLL to Python script directory (architecture-matched)** — workaround, confidence 0.80 (80%), cost/xs, validated=untested
   - From SkiaSharp.NativeAssets.Win32 NuGet package, copy the correct libSkiaSharp.dll (x64 for 64-bit Python, x86 for 32-bit Python) to the same directory as the Python script. The DLL is under runtimes/win-x64/native/libSkiaSharp.dll (for x64) or runtimes/win-x86/native/libSkiaSharp.dll (for x86).
2. **Use dotnet publish and load from publish output** — fix, confidence 0.85 (85%), cost/s, validated=untested
   - Create a .NET 8.0 (not netstandard2.0) console or class library project, add SkiaSharp with proper NativeAssets reference, publish with dotnet publish, then point pythonnet to the published output directory. All native assets will be co-located.

**Recommended proposal:** Copy DLL to Python script directory (architecture-matched)

**Why:** Simplest immediate fix. Requires verifying Python bitness matches DLL architecture, which is also the most likely reason it failed after the user's first attempt.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.80 (80%) |
| Reason | The maintainer's guidance was correct but the user still fails after placing the DLL. To diagnose further we need to know the Python architecture (32-bit vs 64-bit) and exactly where the DLL was placed. The architecture mismatch is the most likely cause of continued failure. |
| Suggested repro platform | windows |

### Missing Info

- Python installation architecture: is the Python interpreter 32-bit (x86) or 64-bit (x64)?
- Exact directory path where libSkiaSharp.dll was copied when the failure persisted
- Whether VC++ 2019 or 2022 redistributable is installed
- Which subfolder of the NuGet package was the DLL taken from (runtimes/win-x64/ vs runtimes/win-x86/)

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.78 (78%) | Correct labels: remove duplicate type/bug (this is a question/usage issue), keep type/question and area/libSkiaSharp.native and os/Windows-Classic | labels=type/question, area/libSkiaSharp.native, os/Windows-Classic |
| add-comment | medium | 0.80 (80%) | Ask reporter for Python architecture details and explain architecture mismatch as likely cause | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for providing the sample zip! To help diagnose why it still fails after placing `libSkiaSharp.dll`, we need a few more details:

1. **Python architecture**: Are you running 32-bit (x86) or 64-bit (x64) Python? You can check with: `python -c "import struct; print(struct.calcsize('P') * 8, 'bit')"`
2. **Which DLL did you copy**: From the `SkiaSharp.NativeAssets.Win32` NuGet package, there are architecture-specific DLLs:
   - `runtimes/win-x64/native/libSkiaSharp.dll` — for 64-bit Python
   - `runtimes/win-x86/native/libSkiaSharp.dll` — for 32-bit Python
3. **Where did you copy it**: The DLL must be in the same directory as your Python script, or in a directory listed in your system `PATH`.

The most common cause of continued failure after placing the DLL is an **architecture mismatch** — 64-bit Python cannot load a 32-bit DLL and vice versa. Make sure the DLL matches your Python bitness.

Also, make sure the [VC++ 2019 or 2022 Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) is installed, as `libSkiaSharp.dll` depends on it.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3040,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:36:49Z",
    "currentLabels": [
      "type/bug",
      "type/question",
      "os/Windows-Classic",
      "area/libSkiaSharp.native",
      "status/needs-attention",
      "triage/triaged"
    ]
  },
  "summary": "User is calling C# SkiaSharp code from Python via pythonnet (clr), getting DllNotFoundException for libSkiaSharp because the native Win32 DLL is not on Windows' DLL search path when running under Python — a deployment/configuration issue specific to the pythonnet scenario.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.78
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Standard 2.0 library using SkiaSharp 2.88.8",
        "Load managed DLLs from .nuget packages folder via pythonnet clr.AddReference()",
        "Attempt to call SkiaSharp API from Python",
        "DllNotFoundException is thrown for libSkiaSharp"
      ],
      "codeSnippets": [
        "clr.AddReference(skiasharp_dll)  # loads SkiaSharp.dll from lib/netstandard2.0 - no native binary",
        "DllNotFoundException: Unable to load DLL 'libSkiaSharp': The specified module could not be found. (Exception from HRESULT: 0x8007007E)"
      ],
      "environmentDetails": "Windows, Python 3.9 + pythonnet (clr), SkiaSharp 2.88.8, .NET Standard 2.0, Visual Studio Code",
      "repoLinks": [
        {
          "url": "https://github.com/user-attachments/files/17703961/PythonWrapper.zip",
          "description": "Zip file with .NET application and Python code uploaded by reporter"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.8",
        "6.88.6"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The issue is about native asset deployment, not a SkiaSharp API change. The 6.88.6 'Last Known Good' is deprecated and likely worked by coincidence — possibly Python's environment happened to have libSkiaSharp.dll on its PATH from another installation."
    }
  },
  "analysis": {
    "summary": "The user is loading SkiaSharp managed assemblies from the .nuget packages cache using pythonnet, but the native libSkiaSharp.dll is not in the lib/netstandard2.0 folder — it lives under runtimes/win-x64/native/ in the NuGet package. When running through Python, Windows DLL search order applies (process directory, PATH, current directory, system dirs), and libSkiaSharp.dll must be in one of those locations. The user tried placing the DLL next to their managed DLLs but failed; this likely means an architecture mismatch (32-bit Python loading 64-bit DLL) or VC++ runtime missing, or incorrect directory.",
    "rationale": "This is a usage/configuration question — SkiaSharp itself is not broken. P/Invoke on Windows uses Windows DLL search order, which doesn't automatically discover files inside NuGet package subfolders. Pythonnet bypasses the normal .NET publish pipeline that would copy native assets. Classified as type/question because the behavior is by-design; the root cause is an environment deployment gap in the pythonnet workflow.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "12",
        "finding": "DllImport constant is 'libSkiaSharp' which on Windows resolves to libSkiaSharp.dll via Windows DLL search order (process directory, PATH, current directory, system dirs). pythonnet does not alter this search path, so the DLL must be explicitly placed in a discoverable location.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SkiaSharpVersion.cs",
        "lines": "22-30",
        "finding": "sk_version_get_milestone() is called in SkiaSharpVersion.Native property, which is invoked from SKObject..cctor() — matches the stack trace exactly (SKObject type initializer fails with DllNotFoundException on the first P/Invoke call).",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Unable to load DLL 'libSkiaSharp': The specified module could not be found. (Exception from HRESULT: 0x8007007E)",
        "source": "issue body — error log",
        "interpretation": "Windows error 0x8007007E (ERROR_MOD_NOT_FOUND) means the DLL file was not found on any of the Windows DLL search paths. This is a deployment issue, not a SkiaSharp defect."
      },
      {
        "text": "You are loading the managed (.NET) dlls, but have not made sure the native parts are able to be found. You can't reference them directly via the CLR, but if they are in the same folder as the managed dlls, they should be found",
        "source": "maintainer comment (mattleibow)",
        "interpretation": "Maintainer correctly identified the root cause: managed DLLs load fine but native libSkiaSharp.dll is missing from discovery paths."
      },
      {
        "text": "We are still getting the exception after including the native libSkiaSharp.dll",
        "source": "comment by DharanyaSakthivel-SF4210",
        "interpretation": "User placed the native DLL but it still fails. Possible causes: architecture mismatch (Python x86 vs Win32 x64 DLL), wrong directory, or missing VC++ redistributable."
      },
      {
        "text": "Last Known Good Version of SkiaSharp: 6.88.6 (Deprecated)",
        "source": "issue body",
        "interpretation": "Not a true regression — 6.88.6 is very old and likely worked due to libSkiaSharp.dll being present on PATH from prior installation or different binary naming."
      }
    ],
    "workarounds": [
      "Publish the .NET app with dotnet publish and use all DLLs from the publish output folder; this copies libSkiaSharp.dll alongside managed assemblies",
      "Copy libSkiaSharp.dll (from SkiaSharp.NativeAssets.Win32 NuGet, runtimes/win-x64/native/ subfolder) to the same directory as the Python script",
      "Add the directory containing libSkiaSharp.dll to the Windows PATH environment variable before running Python",
      "Use Python 64-bit if copying the x64 DLL; or use the x86 DLL if using 32-bit Python (architecture must match)"
    ],
    "nextQuestions": [
      "What is the architecture of the Python installation (32-bit vs 64-bit)?",
      "Exactly which directory did the user copy libSkiaSharp.dll to when it still failed?",
      "Is the VC++ 2019/2022 redistributable installed on the Windows machine?",
      "What is the Python working directory when running the script?"
    ],
    "resolution": {
      "hypothesis": "The native libSkiaSharp.dll cannot be found by Windows DLL loader because: (a) it's not in the Python process directory or PATH, OR (b) architecture mismatch between Python bitness and the DLL bitness, OR (c) VC++ redistributable is missing.",
      "proposals": [
        {
          "title": "Copy DLL to Python script directory (architecture-matched)",
          "description": "From SkiaSharp.NativeAssets.Win32 NuGet package, copy the correct libSkiaSharp.dll (x64 for 64-bit Python, x86 for 32-bit Python) to the same directory as the Python script. The DLL is under runtimes/win-x64/native/libSkiaSharp.dll (for x64) or runtimes/win-x86/native/libSkiaSharp.dll (for x86).",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use dotnet publish and load from publish output",
          "description": "Create a .NET 8.0 (not netstandard2.0) console or class library project, add SkiaSharp with proper NativeAssets reference, publish with dotnet publish, then point pythonnet to the published output directory. All native assets will be co-located.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Copy DLL to Python script directory (architecture-matched)",
      "recommendedReason": "Simplest immediate fix. Requires verifying Python bitness matches DLL architecture, which is also the most likely reason it failed after the user's first attempt."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.8,
      "reason": "The maintainer's guidance was correct but the user still fails after placing the DLL. To diagnose further we need to know the Python architecture (32-bit vs 64-bit) and exactly where the DLL was placed. The architecture mismatch is the most likely cause of continued failure.",
      "suggestedReproPlatform": "windows"
    },
    "missingInfo": [
      "Python installation architecture: is the Python interpreter 32-bit (x86) or 64-bit (x64)?",
      "Exact directory path where libSkiaSharp.dll was copied when the failure persisted",
      "Whether VC++ 2019 or 2022 redistributable is installed",
      "Which subfolder of the NuGet package was the DLL taken from (runtimes/win-x64/ vs runtimes/win-x86/)"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct labels: remove duplicate type/bug (this is a question/usage issue), keep type/question and area/libSkiaSharp.native and os/Windows-Classic",
        "risk": "low",
        "confidence": 0.78,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Ask reporter for Python architecture details and explain architecture mismatch as likely cause",
        "risk": "medium",
        "confidence": 0.8,
        "comment": "Thanks for providing the sample zip! To help diagnose why it still fails after placing `libSkiaSharp.dll`, we need a few more details:\n\n1. **Python architecture**: Are you running 32-bit (x86) or 64-bit (x64) Python? You can check with: `python -c \"import struct; print(struct.calcsize('P') * 8, 'bit')\"`\n2. **Which DLL did you copy**: From the `SkiaSharp.NativeAssets.Win32` NuGet package, there are architecture-specific DLLs:\n   - `runtimes/win-x64/native/libSkiaSharp.dll` — for 64-bit Python\n   - `runtimes/win-x86/native/libSkiaSharp.dll` — for 32-bit Python\n3. **Where did you copy it**: The DLL must be in the same directory as your Python script, or in a directory listed in your system `PATH`.\n\nThe most common cause of continued failure after placing the DLL is an **architecture mismatch** — 64-bit Python cannot load a 32-bit DLL and vice versa. Make sure the DLL matches your Python bitness.\n\nAlso, make sure the [VC++ 2019 or 2022 Redistributable](https://learn.microsoft.com/en-us/cpp/windows/latest-supported-vc-redist) is installed, as `libSkiaSharp.dll` depends on it."
      }
    ]
  }
}
```

</details>
