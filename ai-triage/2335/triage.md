# Issue Triage Report — #2335

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T11:10:00Z |
| Type | type/bug (0.93 (93%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | close-as-fixed (0.78 (78%)) |

**Issue Summary:** NuGet restore fails with NU1202 when a net6.0 unit test project transitively references SkiaSharp.Views.Forms 2.88.3, because that package pulled in OpenTK 3.0.1 and OpenTK.GLControl 3.0.1 which only support .NET Framework (net20).

**Analysis:** OpenTK 3.0.1 was unconditionally referenced in SkiaSharp.Views.Forms 2.88.3 (via the Windows desktop GL view). OpenTK 3.0.1 only targets net20, so NuGet refuses to install it into net6.0 test projects. The current 3.x codebase resolves this by gating the OpenTK 3.x reference on net4.x only and using OpenTK 4.x for modern TFMs.

**Recommendations:** **close-as-fixed** — The root cause (unconditional OpenTK 3.x reference in SkiaSharp.Views.WindowsForms) is fixed in 3.x via TFM conditions. For 2.88.x users still on Xamarin.Forms, a community workaround exists and is documented in the thread. The issue is 2+ years old with no maintainer engagement and no sign the 2.88.x branch will receive a fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | backend/OpenGL |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a Xamarin.Forms solution using SkiaSharp 2.88.3 and SkiaSharp.Views.Forms 2.88.3
2. Add a unit test project targeting net6.0 that references the shared Xamarin.Forms library
3. Run nuget restore on the Azure DevOps pipeline (macOS runner)
4. Observe NU1202 errors for OpenTK 3.0.1 and OpenTK.GLControl 3.0.1

**Environment:** SkiaSharp 2.88.3, SkiaSharp.Views.Forms 2.88.3, net6.0 unit test project, Azure DevOps macOS runner

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2335#issuecomment-1640343543 — Community workaround: add IncludeAssets=None for OpenTK and OpenTK.GLControl packages

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | build-error |
| Error message | NU1202: Package OpenTK.GLControl 3.0.1 is not compatible with net6.0 (.NETCoreApp,Version=v6.0). Package OpenTK.GLControl 3.0.1 supports: net20 (.NETFramework,Version=v2.0) |
| Repro quality | partial |
| Target frameworks | net6.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | In the current 3.x codebase, SkiaSharp.Views.WindowsForms.csproj conditionally uses OpenTK 3.1.0 only for net4.x targets and OpenTK 4.8.2 for modern TFMs (net6.0+). The 2.88.x-era unconditional OpenTK 3.0.1 dependency is not present in 3.x packages. |

### Fix Status

| Field | Value |
|-------|-------|
| Likely fixed | True |
| Confidence | 0.85 (85%) |
| Reason | The current SkiaSharp.Views.WindowsForms.csproj uses TFM-conditional OpenTK references: net4.x targets get OpenTK 3.1.0 (NoWarn NU1701), non-net4 targets get OpenTK 4.8.2. This prevents the NU1202 error for net6.0+ consumers. |
| Related PRs | — |
| Related commits | — |
| Fixed in version | — |

## Analysis

### Technical Summary

OpenTK 3.0.1 was unconditionally referenced in SkiaSharp.Views.Forms 2.88.3 (via the Windows desktop GL view). OpenTK 3.0.1 only targets net20, so NuGet refuses to install it into net6.0 test projects. The current 3.x codebase resolves this by gating the OpenTK 3.x reference on net4.x only and using OpenTK 4.x for modern TFMs.

### Rationale

Clear build-error bug caused by an incompatible transitive NuGet dependency. The NU1202 error is deterministic and reproducible. Three separate users independently hit the same issue. A community workaround exists. The root cause (unconditional OpenTK 3.0.1 reference without TFM guard) has been structurally fixed in 3.x where SkiaSharp.Views.WindowsForms.csproj uses `Condition="$(TargetFramework.StartsWith('net4'))"` to scope the OpenTK 3.x dependency.

### Key Signals

- "NU1202: Package OpenTK.GLControl 3.0.1 is not compatible with net6.0 (.NETCoreApp,Version=v6.0). Package OpenTK.GLControl 3.0.1 supports: net20 (.NETFramework,Version=v2.0)" — **issue body** (OpenTK 3.0.1 was a Windows Forms GL dependency in 2.88.3. It only targets .NET Framework (net20), not .NET Core or .NET 6+. Restoring it into a net6.0 project fails with NU1202.)
- "I have the same problem with incompatibility with net7.0 after upgrading to it from .netcore3.1" — **comment by fverdou** (Issue also affects net7.0, confirming this is a TFM-version problem not specific to net6.0.)
- "just add packages to your test project with IncludeAssets="None"" — **comment by albyrock87** (Community-validated workaround: explicitly reference OpenTK 3.0.1 with IncludeAssets=None to prevent its assets from being restored into the test project.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SkiaSharp.Views.WindowsForms.csproj` | 12-19 | direct | Current code uses two separate ItemGroups: net4.x gets OpenTK 3.1.0 with NoWarn NU1701; non-net4 (net6.0+) gets OpenTK 4.8.2. This TFM condition is the structural fix that prevents NU1202 in modern projects. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj` | 12-19 | direct | Same pattern: net4.x gets OpenTK 3.3.1 + OpenTK.GLWpfControl 3.3.0 (NoWarn NU1701); non-net4 gets OpenTK 4.3.0 + OpenTK.GLWpfControl 4.2.3. Confirms the fix is applied to both Desktop GL packages. |
| `source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs` | 4-9 | related | SKGLControl.cs imports OpenTK, OpenTK.GLControl, OpenTK.Graphics, OpenTK.Graphics.ES20 — confirming OpenTK is a required runtime dependency for the GL control, not just a build tool. |

### Workarounds

- Add OpenTK 3.0.1 and OpenTK.GLControl 3.0.1 references with IncludeAssets="None" to the net6.0+ test project to suppress the incompatible dependency error.
- Upgrade to SkiaSharp 3.x (SkiaSharp.Views.Maui) where OpenTK 3.x is conditioned on net4.x targets only.

### Resolution Proposals

**Hypothesis:** In 2.88.3, OpenTK 3.0.1 was included unconditionally in the Windows Desktop GL view package, causing NuGet restore to fail for net6.0+ consumer projects. The fix (TFM conditions) is already in 3.x.

1. **Workaround: suppress OpenTK in test project** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - Add OpenTK and OpenTK.GLControl 3.0.1 explicitly to the test .csproj with IncludeAssets="None" to prevent NuGet from trying to restore the incompatible assets.

```csharp
<!-- SkiaSharp OpenTK workaround: https://github.com/mono/SkiaSharp/issues/2335 -->
<PackageReference Include="OpenTK" Version="3.0.1" IncludeAssets="None" GeneratePathProperty="true" />
<PackageReference Include="OpenTK.GLControl" Version="3.0.1" IncludeAssets="None" GeneratePathProperty="true" />
```
2. **Upgrade to SkiaSharp 3.x + SkiaSharp.Views.Maui** — alternative, confidence 0.80 (80%), cost/l, validated=untested
   - Migrate from Xamarin.Forms + SkiaSharp.Views.Forms 2.88.3 to MAUI + SkiaSharp.Views.Maui 3.x. The 3.x packages use TFM-conditional OpenTK references that do not affect net6.0+ test projects.

**Recommended proposal:** Workaround: suppress OpenTK in test project

**Why:** Immediately actionable with no migration cost. Community-validated (1 hooray reaction). The upgrade path to 3.x is a large effort for Xamarin.Forms users.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-fixed |
| Confidence | 0.78 (78%) |
| Reason | The root cause (unconditional OpenTK 3.x reference in SkiaSharp.Views.WindowsForms) is fixed in 3.x via TFM conditions. For 2.88.x users still on Xamarin.Forms, a community workaround exists and is documented in the thread. The issue is 2+ years old with no maintainer engagement and no sign the 2.88.x branch will receive a fix. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply type/bug, area/Build, os/Windows-Classic, backend/OpenGL, tenet/compatibility | labels=type/bug, area/Build, os/Windows-Classic, backend/OpenGL, tenet/compatibility |
| add-comment | medium | 0.85 (85%) | Post diagnosis and workaround with note about fix in 3.x | — |
| close-issue | medium | 0.78 (78%) | Close as fixed in 3.x; workaround documented in comment | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for reporting this. The root cause is that `SkiaSharp.Views.Forms 2.88.3` transitively included `OpenTK 3.0.1` without a target-framework condition, and that version of OpenTK only targets `.NET Framework 2.0 (net20)`, making it incompatible with net6.0+ projects.

**Workaround for 2.88.x users:** Add the following to your net6.0 test project's `.csproj` to suppress the incompatible dependency:

```xml
<!-- SkiaSharp OpenTK workaround: https://github.com/mono/SkiaSharp/issues/2335 -->
<PackageReference Include="OpenTK" Version="3.0.1" IncludeAssets="None" GeneratePathProperty="true" />
<PackageReference Include="OpenTK.GLControl" Version="3.0.1" IncludeAssets="None" GeneratePathProperty="true" />
```

**Permanent fix:** This issue is resolved in SkiaSharp 3.x (`SkiaSharp.Views.Maui`). The current `SkiaSharp.Views.WindowsForms` package now gates the OpenTK 3.x dependency exclusively on `net4.x` targets and uses `OpenTK 4.8.2` for net6.0+, preventing the NU1202 error entirely. If you can migrate from Xamarin.Forms to .NET MAUI, upgrading to SkiaSharp 3.x eliminates this issue permanently.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2335,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T11:10:00Z"
  },
  "summary": "NuGet restore fails with NU1202 when a net6.0 unit test project transitively references SkiaSharp.Views.Forms 2.88.3, because that package pulled in OpenTK 3.0.1 and OpenTK.GLControl 3.0.1 which only support .NET Framework (net20).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.93
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ],
    "backends": [
      "backend/OpenGL"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "build-error",
      "errorMessage": "NU1202: Package OpenTK.GLControl 3.0.1 is not compatible with net6.0 (.NETCoreApp,Version=v6.0). Package OpenTK.GLControl 3.0.1 supports: net20 (.NETFramework,Version=v2.0)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net6.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a Xamarin.Forms solution using SkiaSharp 2.88.3 and SkiaSharp.Views.Forms 2.88.3",
        "Add a unit test project targeting net6.0 that references the shared Xamarin.Forms library",
        "Run nuget restore on the Azure DevOps pipeline (macOS runner)",
        "Observe NU1202 errors for OpenTK 3.0.1 and OpenTK.GLControl 3.0.1"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, SkiaSharp.Views.Forms 2.88.3, net6.0 unit test project, Azure DevOps macOS runner",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2335#issuecomment-1640343543",
          "description": "Community workaround: add IncludeAssets=None for OpenTK and OpenTK.GLControl packages"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "In the current 3.x codebase, SkiaSharp.Views.WindowsForms.csproj conditionally uses OpenTK 3.1.0 only for net4.x targets and OpenTK 4.8.2 for modern TFMs (net6.0+). The 2.88.x-era unconditional OpenTK 3.0.1 dependency is not present in 3.x packages."
    },
    "fixStatus": {
      "likelyFixed": true,
      "confidence": 0.85,
      "reason": "The current SkiaSharp.Views.WindowsForms.csproj uses TFM-conditional OpenTK references: net4.x targets get OpenTK 3.1.0 (NoWarn NU1701), non-net4 targets get OpenTK 4.8.2. This prevents the NU1202 error for net6.0+ consumers.",
      "relatedPRs": []
    }
  },
  "analysis": {
    "summary": "OpenTK 3.0.1 was unconditionally referenced in SkiaSharp.Views.Forms 2.88.3 (via the Windows desktop GL view). OpenTK 3.0.1 only targets net20, so NuGet refuses to install it into net6.0 test projects. The current 3.x codebase resolves this by gating the OpenTK 3.x reference on net4.x only and using OpenTK 4.x for modern TFMs.",
    "rationale": "Clear build-error bug caused by an incompatible transitive NuGet dependency. The NU1202 error is deterministic and reproducible. Three separate users independently hit the same issue. A community workaround exists. The root cause (unconditional OpenTK 3.0.1 reference without TFM guard) has been structurally fixed in 3.x where SkiaSharp.Views.WindowsForms.csproj uses `Condition=\"$(TargetFramework.StartsWith('net4'))\"` to scope the OpenTK 3.x dependency.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SkiaSharp.Views.WindowsForms.csproj",
        "lines": "12-19",
        "finding": "Current code uses two separate ItemGroups: net4.x gets OpenTK 3.1.0 with NoWarn NU1701; non-net4 (net6.0+) gets OpenTK 4.8.2. This TFM condition is the structural fix that prevents NU1202 in modern projects.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WPF/SkiaSharp.Views.WPF.csproj",
        "lines": "12-19",
        "finding": "Same pattern: net4.x gets OpenTK 3.3.1 + OpenTK.GLWpfControl 3.3.0 (NoWarn NU1701); non-net4 gets OpenTK 4.3.0 + OpenTK.GLWpfControl 4.2.3. Confirms the fix is applied to both Desktop GL packages.",
        "relevance": "direct"
      },
      {
        "file": "source/SkiaSharp.Views/SkiaSharp.Views.WindowsForms/SKGLControl.cs",
        "lines": "4-9",
        "finding": "SKGLControl.cs imports OpenTK, OpenTK.GLControl, OpenTK.Graphics, OpenTK.Graphics.ES20 — confirming OpenTK is a required runtime dependency for the GL control, not just a build tool.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "NU1202: Package OpenTK.GLControl 3.0.1 is not compatible with net6.0 (.NETCoreApp,Version=v6.0). Package OpenTK.GLControl 3.0.1 supports: net20 (.NETFramework,Version=v2.0)",
        "source": "issue body",
        "interpretation": "OpenTK 3.0.1 was a Windows Forms GL dependency in 2.88.3. It only targets .NET Framework (net20), not .NET Core or .NET 6+. Restoring it into a net6.0 project fails with NU1202."
      },
      {
        "text": "I have the same problem with incompatibility with net7.0 after upgrading to it from .netcore3.1",
        "source": "comment by fverdou",
        "interpretation": "Issue also affects net7.0, confirming this is a TFM-version problem not specific to net6.0."
      },
      {
        "text": "just add packages to your test project with IncludeAssets=\"None\"",
        "source": "comment by albyrock87",
        "interpretation": "Community-validated workaround: explicitly reference OpenTK 3.0.1 with IncludeAssets=None to prevent its assets from being restored into the test project."
      }
    ],
    "workarounds": [
      "Add OpenTK 3.0.1 and OpenTK.GLControl 3.0.1 references with IncludeAssets=\"None\" to the net6.0+ test project to suppress the incompatible dependency error.",
      "Upgrade to SkiaSharp 3.x (SkiaSharp.Views.Maui) where OpenTK 3.x is conditioned on net4.x targets only."
    ],
    "resolution": {
      "hypothesis": "In 2.88.3, OpenTK 3.0.1 was included unconditionally in the Windows Desktop GL view package, causing NuGet restore to fail for net6.0+ consumer projects. The fix (TFM conditions) is already in 3.x.",
      "proposals": [
        {
          "title": "Workaround: suppress OpenTK in test project",
          "description": "Add OpenTK and OpenTK.GLControl 3.0.1 explicitly to the test .csproj with IncludeAssets=\"None\" to prevent NuGet from trying to restore the incompatible assets.",
          "category": "workaround",
          "codeSnippet": "<!-- SkiaSharp OpenTK workaround: https://github.com/mono/SkiaSharp/issues/2335 -->\n<PackageReference Include=\"OpenTK\" Version=\"3.0.1\" IncludeAssets=\"None\" GeneratePathProperty=\"true\" />\n<PackageReference Include=\"OpenTK.GLControl\" Version=\"3.0.1\" IncludeAssets=\"None\" GeneratePathProperty=\"true\" />",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Upgrade to SkiaSharp 3.x + SkiaSharp.Views.Maui",
          "description": "Migrate from Xamarin.Forms + SkiaSharp.Views.Forms 2.88.3 to MAUI + SkiaSharp.Views.Maui 3.x. The 3.x packages use TFM-conditional OpenTK references that do not affect net6.0+ test projects.",
          "category": "alternative",
          "confidence": 0.8,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Workaround: suppress OpenTK in test project",
      "recommendedReason": "Immediately actionable with no migration cost. Community-validated (1 hooray reaction). The upgrade path to 3.x is a large effort for Xamarin.Forms users."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-fixed",
      "confidence": 0.78,
      "reason": "The root cause (unconditional OpenTK 3.x reference in SkiaSharp.Views.WindowsForms) is fixed in 3.x via TFM conditions. For 2.88.x users still on Xamarin.Forms, a community workaround exists and is documented in the thread. The issue is 2+ years old with no maintainer engagement and no sign the 2.88.x branch will receive a fix.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/Build, os/Windows-Classic, backend/OpenGL, tenet/compatibility",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/bug",
          "area/Build",
          "os/Windows-Classic",
          "backend/OpenGL",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post diagnosis and workaround with note about fix in 3.x",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for reporting this. The root cause is that `SkiaSharp.Views.Forms 2.88.3` transitively included `OpenTK 3.0.1` without a target-framework condition, and that version of OpenTK only targets `.NET Framework 2.0 (net20)`, making it incompatible with net6.0+ projects.\n\n**Workaround for 2.88.x users:** Add the following to your net6.0 test project's `.csproj` to suppress the incompatible dependency:\n\n```xml\n<!-- SkiaSharp OpenTK workaround: https://github.com/mono/SkiaSharp/issues/2335 -->\n<PackageReference Include=\"OpenTK\" Version=\"3.0.1\" IncludeAssets=\"None\" GeneratePathProperty=\"true\" />\n<PackageReference Include=\"OpenTK.GLControl\" Version=\"3.0.1\" IncludeAssets=\"None\" GeneratePathProperty=\"true\" />\n```\n\n**Permanent fix:** This issue is resolved in SkiaSharp 3.x (`SkiaSharp.Views.Maui`). The current `SkiaSharp.Views.WindowsForms` package now gates the OpenTK 3.x dependency exclusively on `net4.x` targets and uses `OpenTK 4.8.2` for net6.0+, preventing the NU1202 error entirely. If you can migrate from Xamarin.Forms to .NET MAUI, upgrading to SkiaSharp 3.x eliminates this issue permanently."
      },
      {
        "type": "close-issue",
        "description": "Close as fixed in 3.x; workaround documented in comment",
        "risk": "medium",
        "confidence": 0.78,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
