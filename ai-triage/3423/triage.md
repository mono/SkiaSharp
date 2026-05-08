# Issue Triage Report — #3423

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T12:47:42Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** DllNotFoundException for 'libSkiaSharp' when publishing a .NET Framework 4.7.2 WinForms app via ClickOnce because the native DLLs included via the NativeAssets.Win32 targets as None items are not picked up by ClickOnce's deployment manifest.

**Analysis:** SkiaSharp's NativeAssets.Win32 targets file includes libSkiaSharp.dll as a `None` MSBuild item with CopyToOutputDirectory=PreserveNewest. ClickOnce does not automatically include `None` items in its deployment manifest, so the DLL is present locally but absent from the ClickOnce package, causing DllNotFoundException at runtime.

**Recommendations:** **needs-investigation** — Root cause identified in NativeAssets.Win32 net4 targets (None vs Content). Needs investigation to confirm the targets change won't break other scenarios and to check if the same fix is needed for HarfBuzzSharp.

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
| Current labels | type/bug, os/Windows-Classic, area/libSkiaSharp.native, tenet/reliability |

## Evidence

### Reproduction

1. Create a .NET Framework 4.7.2 WinForms app
2. Add SkiaSharp (3.116.0+) and SkiaSharp.NativeAssets.Win32 NuGet packages
3. Call SKBitmap.Decode() or any SkiaSharp API
4. Publish app via ClickOnce
5. Install and run the ClickOnce-published app

**Environment:** SkiaSharp 3.116.0/3.119.1, .NET Framework 4.7.2, WinForms, Windows 10/11, ClickOnce publish from Visual Studio

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3423#issuecomment-3859787091 — Community workaround: add libSkiaSharp.dll manually as Content item

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | System.DllNotFoundException: Unable to load library 'libSkiaSharp' |
| Repro quality | partial |
| Target frameworks | net472 |

**Stack trace:**

```text
at SkiaSharp.LibraryLoader.LoadLocalLibrary[T](String libraryName)
at System.Lazy`1.CreateValue()
at SkiaSharp.SkiaApi.sk_data_new_empty()
at SkiaSharp.SKData..cctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.1 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The NativeAssets.Win32 targets file uses None items which do not appear in ClickOnce Application Files — this behavior has not changed. |

## Analysis

### Technical Summary

SkiaSharp's NativeAssets.Win32 targets file includes libSkiaSharp.dll as a `None` MSBuild item with CopyToOutputDirectory=PreserveNewest. ClickOnce does not automatically include `None` items in its deployment manifest, so the DLL is present locally but absent from the ClickOnce package, causing DllNotFoundException at runtime.

### Rationale

The stack trace clearly shows LibraryLoader.LoadLocalLibrary failing to find libSkiaSharp, and the reporter observed that 'Application Files' dialog lists the DLL as 'not referenced'. This correlates exactly with how SkiaSharp targets include native assets as `None` items — ClickOnce's manifest builder does not track `None` items. The MSIX version works because MSIX uses a different packaging model that includes all output files. A community workaround (manually adding DLL as Content) was confirmed by two users.

### Key Signals

- "libSkiaSharp.dylib is not being included in the ClickOnce publish, even though I have it configured to (the Application Files dialog says its not referenced)" — **issue body** (ClickOnce's Application Files dialog shows native DLL is not tracked — confirms the None item metadata doesn't register with ClickOnce.)
- "That file IS included in the MSIX package however" — **issue body** (MSIX packages all output files; ClickOnce selectively includes only tracked items. Confirms this is deployment-metadata, not native loading logic.)
- "add the dll as Content with Copy to Output Directory = Copy if newer, then it appears in Application Files with Action = Include" — **comment by ERPCG** (Confirmed workaround: Content items ARE included in ClickOnce manifest. None items are NOT. Root cause is in the .targets file.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets` | 23-27 | direct | Native DLLs are included as `None` items with CopyToOutputDirectory=PreserveNewest. ClickOnce only tracks Content items in its Application Files manifest; None items are invisible to the ClickOnce manifest builder, causing them to be excluded from the published package. |
| `binding/Binding.Shared/LibraryLoader.cs` | 34-86 | related | LoadLocalLibrary searches for the DLL alongside the assembly, in current directory, and AppDomain paths. In a ClickOnce app, these paths all point to the ClickOnce cache, where libSkiaSharp.dll was never deployed — the search fails and throws DllNotFoundException. |

### Workarounds

- Manually add libSkiaSharp.dll from the NuGet cache (e.g. packages/SkiaSharp.NativeAssets.Win32.3.119.1/runtimes/win-x64/native/libSkiaSharp.dll) as an Existing Item in the project with Build Action = Content and Copy to Output Directory = Copy if newer. In Publish → Application Files, verify the DLL appears with Action = Include.
- Use MSIX packaging instead of ClickOnce (already works per reporter).

### Next Questions

- Could the net4 targets file be updated to use Content items (or add ClickOnce-specific metadata like IsClickOnceFile=true) instead of None to make native DLLs automatically deployable via ClickOnce?
- Does the same ClickOnce exclusion affect HarfBuzzSharp.NativeAssets.Win32?

### Resolution Proposals

**Hypothesis:** The `None` MSBuild item type used in the net4 targets file is not tracked by ClickOnce. Changing it to `Content` or adding ClickOnce-specific metadata would fix the issue.

1. **Update NativeAssets.Win32 net4 targets to use Content items** — fix, confidence 0.75 (75%), cost/s, validated=untested
   - Change the `None` item type to `Content` in binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets so ClickOnce includes the native DLLs in its manifest automatically.
2. **Manual workaround: add DLL as Content item** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Add libSkiaSharp.dll from NuGet cache as an Existing Item with Build Action=Content and Copy if newer. Confirmed working by community.

**Recommended proposal:** Update NativeAssets.Win32 net4 targets to use Content items

**Why:** Fixes the root cause for all users without requiring manual steps per project. The community workaround is confirmed, but automated fix via targets is preferred.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | Root cause identified in NativeAssets.Win32 net4 targets (None vs Content). Needs investigation to confirm the targets change won't break other scenarios and to check if the same fix is needed for HarfBuzzSharp. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Labels already present are correct; no changes needed | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Share root cause analysis and workaround | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report. The root cause is in SkiaSharp's MSBuild targets for .NET Framework: native DLLs are included as `None` items, which ClickOnce does not track in its deployment manifest. The MSIX packager includes all output files, which is why that path works.

**Workaround (confirmed by the community):**
1. Find `libSkiaSharp.dll` in your NuGet cache: `packages\SkiaSharp.NativeAssets.Win32.{version}\runtimes\win-x64\native\` (or `win-x86` for 32-bit)
2. In Visual Studio, right-click your project → **Add → Existing Item** and select the DLL
3. Select the added file, open **Properties**, set **Build Action = Content** and **Copy to Output Directory = Copy if newer**
4. In **Project → Properties → Publish → Application Files**, verify `libSkiaSharp.dll` appears with **Action = Include**

We are tracking a proper fix to the NativeAssets.Win32 targets file so this works automatically.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3423,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T12:47:42Z",
    "currentLabels": [
      "type/bug",
      "os/Windows-Classic",
      "area/libSkiaSharp.native",
      "tenet/reliability"
    ]
  },
  "summary": "DllNotFoundException for 'libSkiaSharp' when publishing a .NET Framework 4.7.2 WinForms app via ClickOnce because the native DLLs included via the NativeAssets.Win32 targets as None items are not picked up by ClickOnce's deployment manifest.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
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
      "errorType": "exception",
      "errorMessage": "System.DllNotFoundException: Unable to load library 'libSkiaSharp'",
      "stackTrace": "at SkiaSharp.LibraryLoader.LoadLocalLibrary[T](String libraryName)\nat System.Lazy`1.CreateValue()\nat SkiaSharp.SkiaApi.sk_data_new_empty()\nat SkiaSharp.SKData..cctor()",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net472"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Framework 4.7.2 WinForms app",
        "Add SkiaSharp (3.116.0+) and SkiaSharp.NativeAssets.Win32 NuGet packages",
        "Call SKBitmap.Decode() or any SkiaSharp API",
        "Publish app via ClickOnce",
        "Install and run the ClickOnce-published app"
      ],
      "environmentDetails": "SkiaSharp 3.116.0/3.119.1, .NET Framework 4.7.2, WinForms, Windows 10/11, ClickOnce publish from Visual Studio",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3423#issuecomment-3859787091",
          "description": "Community workaround: add libSkiaSharp.dll manually as Content item"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.1"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The NativeAssets.Win32 targets file uses None items which do not appear in ClickOnce Application Files — this behavior has not changed."
    }
  },
  "analysis": {
    "summary": "SkiaSharp's NativeAssets.Win32 targets file includes libSkiaSharp.dll as a `None` MSBuild item with CopyToOutputDirectory=PreserveNewest. ClickOnce does not automatically include `None` items in its deployment manifest, so the DLL is present locally but absent from the ClickOnce package, causing DllNotFoundException at runtime.",
    "rationale": "The stack trace clearly shows LibraryLoader.LoadLocalLibrary failing to find libSkiaSharp, and the reporter observed that 'Application Files' dialog lists the DLL as 'not referenced'. This correlates exactly with how SkiaSharp targets include native assets as `None` items — ClickOnce's manifest builder does not track `None` items. The MSIX version works because MSIX uses a different packaging model that includes all output files. A community workaround (manually adding DLL as Content) was confirmed by two users.",
    "keySignals": [
      {
        "text": "libSkiaSharp.dylib is not being included in the ClickOnce publish, even though I have it configured to (the Application Files dialog says its not referenced)",
        "source": "issue body",
        "interpretation": "ClickOnce's Application Files dialog shows native DLL is not tracked — confirms the None item metadata doesn't register with ClickOnce."
      },
      {
        "text": "That file IS included in the MSIX package however",
        "source": "issue body",
        "interpretation": "MSIX packages all output files; ClickOnce selectively includes only tracked items. Confirms this is deployment-metadata, not native loading logic."
      },
      {
        "text": "add the dll as Content with Copy to Output Directory = Copy if newer, then it appears in Application Files with Action = Include",
        "source": "comment by ERPCG",
        "interpretation": "Confirmed workaround: Content items ARE included in ClickOnce manifest. None items are NOT. Root cause is in the .targets file."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets",
        "lines": "23-27",
        "finding": "Native DLLs are included as `None` items with CopyToOutputDirectory=PreserveNewest. ClickOnce only tracks Content items in its Application Files manifest; None items are invisible to the ClickOnce manifest builder, causing them to be excluded from the published package.",
        "relevance": "direct"
      },
      {
        "file": "binding/Binding.Shared/LibraryLoader.cs",
        "lines": "34-86",
        "finding": "LoadLocalLibrary searches for the DLL alongside the assembly, in current directory, and AppDomain paths. In a ClickOnce app, these paths all point to the ClickOnce cache, where libSkiaSharp.dll was never deployed — the search fails and throws DllNotFoundException.",
        "relevance": "related"
      }
    ],
    "workarounds": [
      "Manually add libSkiaSharp.dll from the NuGet cache (e.g. packages/SkiaSharp.NativeAssets.Win32.3.119.1/runtimes/win-x64/native/libSkiaSharp.dll) as an Existing Item in the project with Build Action = Content and Copy to Output Directory = Copy if newer. In Publish → Application Files, verify the DLL appears with Action = Include.",
      "Use MSIX packaging instead of ClickOnce (already works per reporter)."
    ],
    "nextQuestions": [
      "Could the net4 targets file be updated to use Content items (or add ClickOnce-specific metadata like IsClickOnceFile=true) instead of None to make native DLLs automatically deployable via ClickOnce?",
      "Does the same ClickOnce exclusion affect HarfBuzzSharp.NativeAssets.Win32?"
    ],
    "resolution": {
      "hypothesis": "The `None` MSBuild item type used in the net4 targets file is not tracked by ClickOnce. Changing it to `Content` or adding ClickOnce-specific metadata would fix the issue.",
      "proposals": [
        {
          "title": "Update NativeAssets.Win32 net4 targets to use Content items",
          "description": "Change the `None` item type to `Content` in binding/SkiaSharp.NativeAssets.Win32/buildTransitive/net4/SkiaSharp.targets so ClickOnce includes the native DLLs in its manifest automatically.",
          "category": "fix",
          "confidence": 0.75,
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Manual workaround: add DLL as Content item",
          "description": "Add libSkiaSharp.dll from NuGet cache as an Existing Item with Build Action=Content and Copy if newer. Confirmed working by community.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Update NativeAssets.Win32 net4 targets to use Content items",
      "recommendedReason": "Fixes the root cause for all users without requiring manual steps per project. The community workaround is confirmed, but automated fix via targets is preferred."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "Root cause identified in NativeAssets.Win32 net4 targets (None vs Content). Needs investigation to confirm the targets change won't break other scenarios and to check if the same fix is needed for HarfBuzzSharp.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Labels already present are correct; no changes needed",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Share root cause analysis and workaround",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the detailed report. The root cause is in SkiaSharp's MSBuild targets for .NET Framework: native DLLs are included as `None` items, which ClickOnce does not track in its deployment manifest. The MSIX packager includes all output files, which is why that path works.\n\n**Workaround (confirmed by the community):**\n1. Find `libSkiaSharp.dll` in your NuGet cache: `packages\\SkiaSharp.NativeAssets.Win32.{version}\\runtimes\\win-x64\\native\\` (or `win-x86` for 32-bit)\n2. In Visual Studio, right-click your project → **Add → Existing Item** and select the DLL\n3. Select the added file, open **Properties**, set **Build Action = Content** and **Copy to Output Directory = Copy if newer**\n4. In **Project → Properties → Publish → Application Files**, verify `libSkiaSharp.dll` appears with **Action = Include**\n\nWe are tracking a proper fix to the NativeAssets.Win32 targets file so this works automatically."
      }
    ]
  }
}
```

</details>
