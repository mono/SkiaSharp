# Issue Triage Report — #3300

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T23:44:26Z |
| Type | type/bug (0.97 (97%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-duplicate (0.90 (90%)) |

**Issue Summary:** DllNotFoundException for libSkiaSharp on Windows Server 2012 R2 after upgrading from 3.116 to 3.119, caused by a hard link-time dependency on d3d12.dll introduced in 3.119 — the same root cause as #3267.

**Analysis:** libSkiaSharp 3.119 introduced a hard link-time dependency on d3d12.dll via the D3D12 GPU backend. Windows Server 2012 R2 does not ship d3d12.dll, so the DLL loader fails to load libSkiaSharp.dll itself, producing DllNotFoundException before any SkiaSharp code runs. The fix (using /DELAYLOAD linker flags to make d3d12.dll optional at load time) has been applied in the current codebase and #3267 was closed as completed.

**Recommendations:** **close-as-duplicate** — This issue is a duplicate of #3267 (D3D12 hard dependency crash on legacy Windows). Same root cause, same error pattern, same fix path. #3267 was closed as completed with the DELAYLOAD fix applied.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Windows-Classic |
| Backends | backend/Direct3D |
| Tenets | tenet/compatibility |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Install SkiaSharp 3.119 in a .NET 9 app
2. Deploy to a Windows Server 2012 R2 machine
3. Run the app and invoke any SkiaSharp code
4. Observe DllNotFoundException: Unable to load DLL 'libSkiaSharp' or one of its dependencies

**Environment:** Windows Server 2012 R2, .NET 9, SkiaSharp 3.119; works with SkiaSharp 3.116.0

**Related issues:** #3267, #3365

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/3267 — Original report: libSkiaSharp.dll Requires d3d12.dll in 3.119.0, Causing Crashes on Win7 — closed as completed
- https://github.com/mono/SkiaSharp/issues/3365 — Duplicate report: libSkiaSharp.dll Requires d3d12.dll causing crashes on systems without DX12 — 0.1% crash rate across 1M installs

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | True |
| Error type | crash |
| Error message | DllNotFoundException Unable to load DLL 'libSkiaSharp' or one of its dependencies |
| Repro quality | partial |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 3.119.0, 2.88.9 |
| Worked in | 3.116.0 |
| Broke in | 3.119.0 |
| Current relevance | unlikely |
| Relevance reason | DELAYLOAD fix (/DELAYLOAD:d3d12.dll /DELAYLOAD:dxgi.dll /DELAYLOAD:D3DCOMPILER_47.dll) added to native/windows/build.cake; #3267 closed as completed 2026-04-03. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.95 (95%) |
| Reason | Reporter confirms 3.116.0 works and 3.119 fails. D3D12 support (and its hard link-time dependency) was introduced in 3.119. |
| Worked in version | 3.116.0 |
| Broke in version | 3.119.0 |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | DELAYLOAD linker flags for d3d12.dll, dxgi.dll, and D3DCOMPILER_47.dll are present in native/windows/build.cake (line 80), converting the hard link-time dependency to a runtime-optional one. Related issue #3267 was closed as 'completed' on 2026-04-03. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

libSkiaSharp 3.119 introduced a hard link-time dependency on d3d12.dll via the D3D12 GPU backend. Windows Server 2012 R2 does not ship d3d12.dll, so the DLL loader fails to load libSkiaSharp.dll itself, producing DllNotFoundException before any SkiaSharp code runs. The fix (using /DELAYLOAD linker flags to make d3d12.dll optional at load time) has been applied in the current codebase and #3267 was closed as completed.

### Rationale

This is a duplicate of #3267 (Win7 D3D12 crash) with the same root cause: hard link-time dependency on d3d12.dll in 3.119. Windows Server 2012 R2 doesn't ship d3d12.dll. The DELAYLOAD fix is confirmed in native/windows/build.cake and #3267 was closed as completed. Close as duplicate of #3267.

### Key Signals

- "DllNotFoundException Unable to load DLL 'libSkiaSharp' or one of its dependencies" — **issue body** (libSkiaSharp.dll itself fails to load because d3d12.dll (a link-time dependency added in 3.119) is not present on Windows Server 2012 R2.)
- "upgraded the version from 3.116 to 3.119 ... If i regress to 3.116 everything work again" — **issue body** (Clear regression in 3.119; 3.116 predates the D3D12 backend addition.)
- "This is probably because Windows Server 2012 doesn't support DirectX 12, which 3.119 is built against. Probably a duplicate of #3267" — **comment by molesmoke** (Community immediately identified the root cause and the duplicate — Windows Server 2012 R2 lacks d3d12.dll.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/windows/build.cake` | 15,80 | direct | SUPPORT_DIRECT3D controls D3D12 compilation; when true, DELAYLOAD flags '/DELAYLOAD:d3d12.dll /DELAYLOAD:dxgi.dll /DELAYLOAD:D3DCOMPILER_47.dll' are passed to the linker — making d3d12.dll a runtime-optional dependency instead of a hard load-time requirement. |
| `binding/SkiaSharp/GRContext.cs` | 66-70 | related | GRContext.CreateDirect3D() is the explicit entry point for D3D12 GPU context creation. With DELAYLOAD, d3d12.dll is only resolved when this method is actually called — systems without d3d12.dll can still use SkiaSharp for CPU rasterization. |

### Workarounds

- Downgrade to SkiaSharp 3.116.x (no D3D12 dependency); reporter already confirmed this works.
- Upgrade to a SkiaSharp release that includes the DELAYLOAD fix (built after 2026-04-03 close of #3267).
- If D3D12 rendering is not needed, upgrading should be safe — the DELAYLOAD fix makes d3d12.dll optional so it is only loaded when GRContext.CreateDirect3D() is explicitly called.

### Resolution Proposals

**Hypothesis:** d3d12.dll is a hard link-time dependency in libSkiaSharp 3.119+ on Windows. Windows Server 2012 R2 does not ship this DLL, preventing the library from loading. The fix (DELAYLOAD) is already in the codebase.

1. **Upgrade to fixed SkiaSharp release** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - The DELAYLOAD fix converting d3d12.dll from a hard to a runtime-optional dependency is in native/windows/build.cake. Upgrading to any SkiaSharp release built after the fix (post-#3267 resolution on 2026-04-03) should resolve the issue on Windows Server 2012 R2.
2. **Downgrade to SkiaSharp 3.116.x** — workaround, confidence 0.98 (98%), cost/xs, validated=yes
   - 3.116.x does not include the D3D12 backend and has no d3d12.dll dependency. The reporter confirmed this workaround resolves the crash.

**Recommended proposal:** Upgrade to fixed SkiaSharp release

**Why:** The DELAYLOAD fix is in the codebase and #3267 was closed as completed — upgrading is the proper long-term solution.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-duplicate |
| Confidence | 0.90 (90%) |
| Reason | This issue is a duplicate of #3267 (D3D12 hard dependency crash on legacy Windows). Same root cause, same error pattern, same fix path. #3267 was closed as completed with the DELAYLOAD fix applied. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply bug, native, Windows, Direct3D, and compatibility labels | labels=type/bug, area/libSkiaSharp.native, os/Windows-Classic, backend/Direct3D, tenet/compatibility |
| link-duplicate | medium | 0.90 (90%) | Mark as duplicate of #3267 — same D3D12 hard dependency root cause | linkedIssue=#3267 |
| add-comment | medium | 0.88 (88%) | Explain root cause and workarounds; note the fix in #3267 | — |
| close-issue | medium | 0.90 (90%) | Close as duplicate of #3267 | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report!

This crash is caused by a hard link-time dependency on `d3d12.dll` that was introduced with the Direct3D 12 GPU backend in SkiaSharp 3.119. Windows Server 2012 R2 does not ship `d3d12.dll`, so `libSkiaSharp.dll` fails to load entirely — before any of your code runs.

This is a duplicate of #3267 (same root cause on Windows 7). The fix — using `/DELAYLOAD:d3d12.dll` linker flags to make `d3d12.dll` a runtime-optional dependency — has been applied to the build system. `d3d12.dll` will only be loaded if you explicitly call `GRContext.CreateDirect3D()`; CPU rasterization is unaffected.

**Workarounds:**
- **Immediate:** Downgrade to SkiaSharp 3.116.x (no D3D12 dependency) — you already confirmed this works.
- **Long-term:** Upgrade to a SkiaSharp release built after the #3267 fix (2026-04-03), which includes the DELAYLOAD fix.

Closing as a duplicate of #3267.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3300,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T23:44:26Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "DllNotFoundException for libSkiaSharp on Windows Server 2012 R2 after upgrading from 3.116 to 3.119, caused by a hard link-time dependency on d3d12.dll introduced in 3.119 — the same root cause as #3267.",
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
    "backends": [
      "backend/Direct3D"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": true,
      "errorType": "crash",
      "errorMessage": "DllNotFoundException Unable to load DLL 'libSkiaSharp' or one of its dependencies",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Install SkiaSharp 3.119 in a .NET 9 app",
        "Deploy to a Windows Server 2012 R2 machine",
        "Run the app and invoke any SkiaSharp code",
        "Observe DllNotFoundException: Unable to load DLL 'libSkiaSharp' or one of its dependencies"
      ],
      "environmentDetails": "Windows Server 2012 R2, .NET 9, SkiaSharp 3.119; works with SkiaSharp 3.116.0",
      "relatedIssues": [
        3267,
        3365
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3267",
          "description": "Original report: libSkiaSharp.dll Requires d3d12.dll in 3.119.0, Causing Crashes on Win7 — closed as completed"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3365",
          "description": "Duplicate report: libSkiaSharp.dll Requires d3d12.dll causing crashes on systems without DX12 — 0.1% crash rate across 1M installs"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "3.119.0",
        "2.88.9"
      ],
      "workedIn": "3.116.0",
      "brokeIn": "3.119.0",
      "currentRelevance": "unlikely",
      "relevanceReason": "DELAYLOAD fix (/DELAYLOAD:d3d12.dll /DELAYLOAD:dxgi.dll /DELAYLOAD:D3DCOMPILER_47.dll) added to native/windows/build.cake; #3267 closed as completed 2026-04-03."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.95,
      "reason": "Reporter confirms 3.116.0 works and 3.119 fails. D3D12 support (and its hard link-time dependency) was introduced in 3.119.",
      "workedInVersion": "3.116.0",
      "brokeInVersion": "3.119.0"
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "DELAYLOAD linker flags for d3d12.dll, dxgi.dll, and D3DCOMPILER_47.dll are present in native/windows/build.cake (line 80), converting the hard link-time dependency to a runtime-optional one. Related issue #3267 was closed as 'completed' on 2026-04-03.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "libSkiaSharp 3.119 introduced a hard link-time dependency on d3d12.dll via the D3D12 GPU backend. Windows Server 2012 R2 does not ship d3d12.dll, so the DLL loader fails to load libSkiaSharp.dll itself, producing DllNotFoundException before any SkiaSharp code runs. The fix (using /DELAYLOAD linker flags to make d3d12.dll optional at load time) has been applied in the current codebase and #3267 was closed as completed.",
    "codeInvestigation": [
      {
        "file": "native/windows/build.cake",
        "lines": "15,80",
        "finding": "SUPPORT_DIRECT3D controls D3D12 compilation; when true, DELAYLOAD flags '/DELAYLOAD:d3d12.dll /DELAYLOAD:dxgi.dll /DELAYLOAD:D3DCOMPILER_47.dll' are passed to the linker — making d3d12.dll a runtime-optional dependency instead of a hard load-time requirement.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/GRContext.cs",
        "lines": "66-70",
        "finding": "GRContext.CreateDirect3D() is the explicit entry point for D3D12 GPU context creation. With DELAYLOAD, d3d12.dll is only resolved when this method is actually called — systems without d3d12.dll can still use SkiaSharp for CPU rasterization.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "DllNotFoundException Unable to load DLL 'libSkiaSharp' or one of its dependencies",
        "source": "issue body",
        "interpretation": "libSkiaSharp.dll itself fails to load because d3d12.dll (a link-time dependency added in 3.119) is not present on Windows Server 2012 R2."
      },
      {
        "text": "upgraded the version from 3.116 to 3.119 ... If i regress to 3.116 everything work again",
        "source": "issue body",
        "interpretation": "Clear regression in 3.119; 3.116 predates the D3D12 backend addition."
      },
      {
        "text": "This is probably because Windows Server 2012 doesn't support DirectX 12, which 3.119 is built against. Probably a duplicate of #3267",
        "source": "comment by molesmoke",
        "interpretation": "Community immediately identified the root cause and the duplicate — Windows Server 2012 R2 lacks d3d12.dll."
      }
    ],
    "rationale": "This is a duplicate of #3267 (Win7 D3D12 crash) with the same root cause: hard link-time dependency on d3d12.dll in 3.119. Windows Server 2012 R2 doesn't ship d3d12.dll. The DELAYLOAD fix is confirmed in native/windows/build.cake and #3267 was closed as completed. Close as duplicate of #3267.",
    "workarounds": [
      "Downgrade to SkiaSharp 3.116.x (no D3D12 dependency); reporter already confirmed this works.",
      "Upgrade to a SkiaSharp release that includes the DELAYLOAD fix (built after 2026-04-03 close of #3267).",
      "If D3D12 rendering is not needed, upgrading should be safe — the DELAYLOAD fix makes d3d12.dll optional so it is only loaded when GRContext.CreateDirect3D() is explicitly called."
    ],
    "resolution": {
      "hypothesis": "d3d12.dll is a hard link-time dependency in libSkiaSharp 3.119+ on Windows. Windows Server 2012 R2 does not ship this DLL, preventing the library from loading. The fix (DELAYLOAD) is already in the codebase.",
      "proposals": [
        {
          "title": "Upgrade to fixed SkiaSharp release",
          "description": "The DELAYLOAD fix converting d3d12.dll from a hard to a runtime-optional dependency is in native/windows/build.cake. Upgrading to any SkiaSharp release built after the fix (post-#3267 resolution on 2026-04-03) should resolve the issue on Windows Server 2012 R2.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Downgrade to SkiaSharp 3.116.x",
          "description": "3.116.x does not include the D3D12 backend and has no d3d12.dll dependency. The reporter confirmed this workaround resolves the crash.",
          "category": "workaround",
          "confidence": 0.98,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Upgrade to fixed SkiaSharp release",
      "recommendedReason": "The DELAYLOAD fix is in the codebase and #3267 was closed as completed — upgrading is the proper long-term solution."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-duplicate",
      "confidence": 0.9,
      "reason": "This issue is a duplicate of #3267 (D3D12 hard dependency crash on legacy Windows). Same root cause, same error pattern, same fix path. #3267 was closed as completed with the DELAYLOAD fix applied.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply bug, native, Windows, Direct3D, and compatibility labels",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Windows-Classic",
          "backend/Direct3D",
          "tenet/compatibility"
        ]
      },
      {
        "type": "link-duplicate",
        "description": "Mark as duplicate of #3267 — same D3D12 hard dependency root cause",
        "risk": "medium",
        "confidence": 0.9,
        "linkedIssue": 3267
      },
      {
        "type": "add-comment",
        "description": "Explain root cause and workarounds; note the fix in #3267",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report!\n\nThis crash is caused by a hard link-time dependency on `d3d12.dll` that was introduced with the Direct3D 12 GPU backend in SkiaSharp 3.119. Windows Server 2012 R2 does not ship `d3d12.dll`, so `libSkiaSharp.dll` fails to load entirely — before any of your code runs.\n\nThis is a duplicate of #3267 (same root cause on Windows 7). The fix — using `/DELAYLOAD:d3d12.dll` linker flags to make `d3d12.dll` a runtime-optional dependency — has been applied to the build system. `d3d12.dll` will only be loaded if you explicitly call `GRContext.CreateDirect3D()`; CPU rasterization is unaffected.\n\n**Workarounds:**\n- **Immediate:** Downgrade to SkiaSharp 3.116.x (no D3D12 dependency) — you already confirmed this works.\n- **Long-term:** Upgrade to a SkiaSharp release built after the #3267 fix (2026-04-03), which includes the DELAYLOAD fix.\n\nClosing as a duplicate of #3267."
      },
      {
        "type": "close-issue",
        "description": "Close as duplicate of #3267",
        "risk": "medium",
        "confidence": 0.9,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
