# Issue Triage Report — #1105

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T13:15:15Z |
| Type | type/bug (0.90 (90%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | keep-open (0.80 (80%)) |

**Issue Summary:** SKTypeface.FromFile, FromFamilyName, and FromStream all return null in a Windows Nano Server Docker container because Nano Server lacks dwrite.dll and usp10.dll, which the Skia Windows font manager requires.

**Analysis:** The Windows Nano Server native library (libSkiaSharp.dll for NanoServer) uses DirectWrite (dwrite.dll) for font management, but Windows Nano Server does not ship with dwrite.dll or usp10.dll. When LoadLibraryW('dwrite.dll') returns null, the font manager fails to initialize, causing all SKTypeface factory methods to return null. The fix would require building a separate Nano Server variant of libSkiaSharp that uses FreeType + a directory-based font manager instead of DirectWrite.

**Recommendations:** **keep-open** — Root cause is clear (DirectWrite missing from Nano Server) and well-documented by maintainer, but a proper fix (FreeType-based NanoServer native asset) requires significant native build work that has not been attempted. The issue has ongoing community demand.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Nano-Server |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a .NET application that references SkiaSharp 1.68.1.1 and SkiaSharp.NativeAssets.NanoServer 1.68.1.1
2. Run in a Windows Nano Server Docker container
3. Call SKTypeface.FromFamilyName, FromFile, FromStream, or CreateDefault()
4. Observe: all factory methods return null or return a typeface with an empty FamilyName

**Environment:** SkiaSharp 1.68.1.1, SkiaSharp.NativeAssets.NanoServer 1.68.1.1, Windows Nano Server Docker container

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/591 — Related earlier issue: SkiaSharp does not work in Docker Windows Container (closed as completed for Server Core)
- https://github.com/mono/skia/blob/xamarin-mobile-bindings/src/utils/win/SkDWrite.cpp#L31 — Skia source: DWriteCreateFactory load point that fails when dwrite.dll is missing
- https://github.com/google/skia/blob/master/src/core/SkFontMgr.cpp#L80 — Skia upstream: SkFontMgr returns nullptr when no font manager implementation is available
- https://github.com/google/skia/blob/master/BUILD.gn#L274 — Skia upstream: FreeType build option that could enable font management without DirectWrite

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | missing-output |
| Error message | SKTypeface.FromFamilyName / FromFile / FromStream returns null; SKTypeface.CreateDefault().FamilyName is empty string |
| Repro quality | complete |
| Target frameworks | netcoreapp |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 1.68.1.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | As of 2024, contributors confirm the issue still exists. The native library for Nano Server has not been updated to use FreeType instead of DirectWrite. |

## Analysis

### Technical Summary

The Windows Nano Server native library (libSkiaSharp.dll for NanoServer) uses DirectWrite (dwrite.dll) for font management, but Windows Nano Server does not ship with dwrite.dll or usp10.dll. When LoadLibraryW('dwrite.dll') returns null, the font manager fails to initialize, causing all SKTypeface factory methods to return null. The fix would require building a separate Nano Server variant of libSkiaSharp that uses FreeType + a directory-based font manager instead of DirectWrite.

### Rationale

This is a real bug in the SkiaSharp.NativeAssets.NanoServer package: it ships a native library that silently fails to create typefaces due to missing platform DLLs. The root cause (DirectWrite missing from Nano Server) was confirmed by maintainer @mattleibow. A fix path (FreeType on Windows) was identified but never implemented. The issue remains open and affects multiple users as of 2024.

### Key Signals

- "SKFontManager.Default.FontFamilyCount returns zero and SKFontManager.Default.FontFamilies is empty" — **comment #575143845** (Font manager has no registered font families — confirming the system font manager failed to initialize on Nano Server.)
- "It is because this returns null LoadLibraryW(L"dwrite.dll")" — **comment #575948098 (mattleibow)** (Root cause confirmed by maintainer: dwrite.dll is missing from Nano Server, preventing DirectWrite initialization.)
- "GDI font manager tried locally but usp10.dll is missing on Nano Server too" — **comment #577431058 (mattleibow)** (Both DirectWrite and GDI (Uniscribe) font managers are unavailable, leaving no font management path on Nano Server.)
- "There is nothing to fix here. Limitation of the platform." — **comment #1973082415 (Gillibald)** (Contributor assessment that Nano Server simply lacks the required DLLs; workaround or platform change needed.)
- "It is possible to copy the DWrite.dll to the final build of windows nano server container and then even the text rendering is working" — **comment #1804166427 (SevcikMichal)** (Community workaround: manually copying dwrite.dll into the container resolves the issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj` | — | direct | The NanoServer native asset package ships output/native/nanoserver/x64/libSkiaSharp* — the same Win32 DLL with only the XPS subsetting call removed. It does not use a FreeType-based build and still depends on DirectWrite. |
| `binding/SkiaSharp/SKTypeface.cs` | 50-72 | direct | SKTypeface.FromFamilyName calls sk_typeface_create_from_name via P/Invoke; the C API returns nullptr if the underlying Skia font manager fails. GetObject returns null when the handle is IntPtr.Zero — matching the reporter's observation. |
| `binding/SkiaSharp/SKFontManager.cs` | 14-40 | direct | SKFontManager.Default is initialized via sk_fontmgr_ref_default. If Skia's default font manager (DirectWrite-based on Windows) fails to initialize, FontFamilyCount returns 0 and all font lookups fail. |

### Workarounds

- Switch to a Linux-based Docker container (recommended by maintainer @mattleibow): Linux containers are smaller, run on the same host, and have full FreeType font support.
- Switch to Windows Server Core instead of Nano Server: Server Core includes dwrite.dll and full font APIs.
- Copy dwrite.dll from a Windows Server Core image into the Nano Server container's System32 directory — community workaround reported by @SevcikMichal that enables text rendering.

### Next Questions

- Has a FreeType-based Windows build ever been attempted since the m68 blocker was identified?
- Is there interest in maintaining a separate Nano Server variant with FreeType, given that Microsoft has been deprioritizing Nano Server?

### Resolution Proposals

**Hypothesis:** The NanoServer native asset needs a dedicated build using FreeType + a directory-based font manager (SkFontMgr_New_Custom_Directory) instead of DirectWrite, to work within Nano Server's limited Win32 API surface.

1. **Switch to Linux container** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Use a Linux-based Docker container with the standard SkiaSharp Linux native assets. Linux containers support FreeType-based font management out of the box, are smaller than Windows containers, and can coexist on the same Docker host.
2. **Switch to Windows Server Core** — workaround, confidence 0.95 (95%), cost/xs, validated=untested
   - Use mcr.microsoft.com/windows/servercore as the base image instead of Nano Server. Server Core includes dwrite.dll and full font support, allowing the standard SkiaSharp.NativeAssets.Win native package to work.
3. **Copy dwrite.dll into Nano Server container** — workaround, confidence 0.70 (70%), cost/s, validated=untested
   - In the Dockerfile, copy dwrite.dll from a Windows Server Core stage into the Nano Server container's System32. This enables DirectWrite on Nano Server.
4. **Build a FreeType-based NanoServer native asset** — fix, confidence 0.75 (75%), cost/xl, validated=untested
   - Build libSkiaSharp for Windows using FreeType and SkFontMgr_New_Custom_Directory instead of DirectWrite. This would be a new build target (e.g., externals-windows-nanoserver-freetype) and a separate NuGet package variant. Skia already supports this configuration on Linux/Android.

**Recommended proposal:** Switch to Linux container

**Why:** Zero SkiaSharp changes required, immediately resolves the issue for most users, and is the solution already suggested by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | keep-open |
| Confidence | 0.80 (80%) |
| Reason | Root cause is clear (DirectWrite missing from Nano Server) and well-documented by maintainer, but a proper fix (FreeType-based NanoServer native asset) requires significant native build work that has not been attempted. The issue has ongoing community demand. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels already set correctly: type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability | labels=type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post comment summarizing root cause and available workarounds | — |

**Comment draft for `add-comment`:**

```markdown
## Status Update

This issue is a known platform limitation of Windows Nano Server: Nano Server does not ship `dwrite.dll` (DirectWrite) or `usp10.dll` (Uniscribe/GDI), which are the two font APIs that Skia uses on Windows. When `LoadLibraryW("dwrite.dll")` returns null, the Skia font manager fails to initialize, causing all `SKTypeface` factory methods to return null.

### Workarounds available today

1. **Switch to a Linux container** (recommended): Linux containers use FreeType for font management and have full SkiaSharp text support. They're also smaller than Windows containers.

2. **Switch to Windows Server Core**: Server Core includes `dwrite.dll` and full font API support.

3. **Copy `dwrite.dll` into your Nano Server container**: Copy `C:\Windows\System32\dwrite.dll` from a Server Core layer into your Nano Server image. [Reported to work](https://github.com/mono/SkiaSharp/issues/1105#issuecomment-1804166427) by the community.

### Long-term fix

A proper fix would require building a separate `SkiaSharp.NativeAssets.NanoServer` variant that uses FreeType + a directory-based font manager instead of DirectWrite. This is technically feasible (Skia supports this on Linux/Android), but requires significant native build infrastructure work. We're tracking this as a future enhancement.

If this is critical to your use case, a community contribution adding a FreeType-based Windows build target would be very welcome.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1105,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T13:15:15Z",
    "currentLabels": [
      "type/bug",
      "area/libSkiaSharp.native",
      "os/Windows-Nano-Server",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "SKTypeface.FromFile, FromFamilyName, and FromStream all return null in a Windows Nano Server Docker container because Nano Server lacks dwrite.dll and usp10.dll, which the Skia Windows font manager requires.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.9
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Nano-Server"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "missing-output",
      "errorMessage": "SKTypeface.FromFamilyName / FromFile / FromStream returns null; SKTypeface.CreateDefault().FamilyName is empty string",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netcoreapp"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET application that references SkiaSharp 1.68.1.1 and SkiaSharp.NativeAssets.NanoServer 1.68.1.1",
        "Run in a Windows Nano Server Docker container",
        "Call SKTypeface.FromFamilyName, FromFile, FromStream, or CreateDefault()",
        "Observe: all factory methods return null or return a typeface with an empty FamilyName"
      ],
      "environmentDetails": "SkiaSharp 1.68.1.1, SkiaSharp.NativeAssets.NanoServer 1.68.1.1, Windows Nano Server Docker container",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/591",
          "description": "Related earlier issue: SkiaSharp does not work in Docker Windows Container (closed as completed for Server Core)"
        },
        {
          "url": "https://github.com/mono/skia/blob/xamarin-mobile-bindings/src/utils/win/SkDWrite.cpp#L31",
          "description": "Skia source: DWriteCreateFactory load point that fails when dwrite.dll is missing"
        },
        {
          "url": "https://github.com/google/skia/blob/master/src/core/SkFontMgr.cpp#L80",
          "description": "Skia upstream: SkFontMgr returns nullptr when no font manager implementation is available"
        },
        {
          "url": "https://github.com/google/skia/blob/master/BUILD.gn#L274",
          "description": "Skia upstream: FreeType build option that could enable font management without DirectWrite"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "1.68.1.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "As of 2024, contributors confirm the issue still exists. The native library for Nano Server has not been updated to use FreeType instead of DirectWrite."
    }
  },
  "analysis": {
    "summary": "The Windows Nano Server native library (libSkiaSharp.dll for NanoServer) uses DirectWrite (dwrite.dll) for font management, but Windows Nano Server does not ship with dwrite.dll or usp10.dll. When LoadLibraryW('dwrite.dll') returns null, the font manager fails to initialize, causing all SKTypeface factory methods to return null. The fix would require building a separate Nano Server variant of libSkiaSharp that uses FreeType + a directory-based font manager instead of DirectWrite.",
    "rationale": "This is a real bug in the SkiaSharp.NativeAssets.NanoServer package: it ships a native library that silently fails to create typefaces due to missing platform DLLs. The root cause (DirectWrite missing from Nano Server) was confirmed by maintainer @mattleibow. A fix path (FreeType on Windows) was identified but never implemented. The issue remains open and affects multiple users as of 2024.",
    "keySignals": [
      {
        "text": "SKFontManager.Default.FontFamilyCount returns zero and SKFontManager.Default.FontFamilies is empty",
        "source": "comment #575143845",
        "interpretation": "Font manager has no registered font families — confirming the system font manager failed to initialize on Nano Server."
      },
      {
        "text": "It is because this returns null LoadLibraryW(L\"dwrite.dll\")",
        "source": "comment #575948098 (mattleibow)",
        "interpretation": "Root cause confirmed by maintainer: dwrite.dll is missing from Nano Server, preventing DirectWrite initialization."
      },
      {
        "text": "GDI font manager tried locally but usp10.dll is missing on Nano Server too",
        "source": "comment #577431058 (mattleibow)",
        "interpretation": "Both DirectWrite and GDI (Uniscribe) font managers are unavailable, leaving no font management path on Nano Server."
      },
      {
        "text": "There is nothing to fix here. Limitation of the platform.",
        "source": "comment #1973082415 (Gillibald)",
        "interpretation": "Contributor assessment that Nano Server simply lacks the required DLLs; workaround or platform change needed."
      },
      {
        "text": "It is possible to copy the DWrite.dll to the final build of windows nano server container and then even the text rendering is working",
        "source": "comment #1804166427 (SevcikMichal)",
        "interpretation": "Community workaround: manually copying dwrite.dll into the container resolves the issue."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.NanoServer/SkiaSharp.NativeAssets.NanoServer.csproj",
        "finding": "The NanoServer native asset package ships output/native/nanoserver/x64/libSkiaSharp* — the same Win32 DLL with only the XPS subsetting call removed. It does not use a FreeType-based build and still depends on DirectWrite.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKTypeface.cs",
        "lines": "50-72",
        "finding": "SKTypeface.FromFamilyName calls sk_typeface_create_from_name via P/Invoke; the C API returns nullptr if the underlying Skia font manager fails. GetObject returns null when the handle is IntPtr.Zero — matching the reporter's observation.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKFontManager.cs",
        "lines": "14-40",
        "finding": "SKFontManager.Default is initialized via sk_fontmgr_ref_default. If Skia's default font manager (DirectWrite-based on Windows) fails to initialize, FontFamilyCount returns 0 and all font lookups fail.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Switch to a Linux-based Docker container (recommended by maintainer @mattleibow): Linux containers are smaller, run on the same host, and have full FreeType font support.",
      "Switch to Windows Server Core instead of Nano Server: Server Core includes dwrite.dll and full font APIs.",
      "Copy dwrite.dll from a Windows Server Core image into the Nano Server container's System32 directory — community workaround reported by @SevcikMichal that enables text rendering."
    ],
    "nextQuestions": [
      "Has a FreeType-based Windows build ever been attempted since the m68 blocker was identified?",
      "Is there interest in maintaining a separate Nano Server variant with FreeType, given that Microsoft has been deprioritizing Nano Server?"
    ],
    "resolution": {
      "hypothesis": "The NanoServer native asset needs a dedicated build using FreeType + a directory-based font manager (SkFontMgr_New_Custom_Directory) instead of DirectWrite, to work within Nano Server's limited Win32 API surface.",
      "proposals": [
        {
          "title": "Switch to Linux container",
          "description": "Use a Linux-based Docker container with the standard SkiaSharp Linux native assets. Linux containers support FreeType-based font management out of the box, are smaller than Windows containers, and can coexist on the same Docker host.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Switch to Windows Server Core",
          "description": "Use mcr.microsoft.com/windows/servercore as the base image instead of Nano Server. Server Core includes dwrite.dll and full font support, allowing the standard SkiaSharp.NativeAssets.Win native package to work.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Copy dwrite.dll into Nano Server container",
          "description": "In the Dockerfile, copy dwrite.dll from a Windows Server Core stage into the Nano Server container's System32. This enables DirectWrite on Nano Server.",
          "category": "workaround",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Build a FreeType-based NanoServer native asset",
          "description": "Build libSkiaSharp for Windows using FreeType and SkFontMgr_New_Custom_Directory instead of DirectWrite. This would be a new build target (e.g., externals-windows-nanoserver-freetype) and a separate NuGet package variant. Skia already supports this configuration on Linux/Android.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/xl",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to Linux container",
      "recommendedReason": "Zero SkiaSharp changes required, immediately resolves the issue for most users, and is the solution already suggested by the maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "keep-open",
      "confidence": 0.8,
      "reason": "Root cause is clear (DirectWrite missing from Nano Server) and well-documented by maintainer, but a proper fix (FreeType-based NanoServer native asset) requires significant native build work that has not been attempted. The issue has ongoing community demand.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already set correctly: type/bug, area/libSkiaSharp.native, os/Windows-Nano-Server, tenet/reliability",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Nano-Server",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post comment summarizing root cause and available workarounds",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "## Status Update\n\nThis issue is a known platform limitation of Windows Nano Server: Nano Server does not ship `dwrite.dll` (DirectWrite) or `usp10.dll` (Uniscribe/GDI), which are the two font APIs that Skia uses on Windows. When `LoadLibraryW(\"dwrite.dll\")` returns null, the Skia font manager fails to initialize, causing all `SKTypeface` factory methods to return null.\n\n### Workarounds available today\n\n1. **Switch to a Linux container** (recommended): Linux containers use FreeType for font management and have full SkiaSharp text support. They're also smaller than Windows containers.\n\n2. **Switch to Windows Server Core**: Server Core includes `dwrite.dll` and full font API support.\n\n3. **Copy `dwrite.dll` into your Nano Server container**: Copy `C:\\Windows\\System32\\dwrite.dll` from a Server Core layer into your Nano Server image. [Reported to work](https://github.com/mono/SkiaSharp/issues/1105#issuecomment-1804166427) by the community.\n\n### Long-term fix\n\nA proper fix would require building a separate `SkiaSharp.NativeAssets.NanoServer` variant that uses FreeType + a directory-based font manager instead of DirectWrite. This is technically feasible (Skia supports this on Linux/Android), but requires significant native build infrastructure work. We're tracking this as a future enhancement.\n\nIf this is critical to your use case, a community contribution adding a FreeType-based Windows build target would be very welcome."
      }
    ]
  }
}
```

</details>
