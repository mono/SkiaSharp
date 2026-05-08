# Issue Triage Report — #1830

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-01T02:10:00Z |
| Type | type/bug (0.88 (88%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | needs-investigation (0.75 (75%)) |

**Issue Summary:** Packing a class library that references SkiaSharp.Views.Forms 2.80.x via `msbuild -t:pack` fails because a `Native.*.manifest` file is expected but not generated on disk.

**Analysis:** When packing a Xamarin.iOS class library that references SkiaSharp.Views.Forms 2.80.x, MSBuild expects a 'Native.*.manifest' file to be present as a pack output, but this file is never generated because the NativeReference handling in Xamarin.iOS does not produce the manifest during a pack-only build. This is a downstream effect of the upstream MSBuild issue #4584: native references from NuGet packages cause the pack target to expect a manifest file that only gets created during a full build, not during a pack operation.

**Recommendations:** **needs-investigation** — Complete reproduction provided, confirmed regression from 1.68.3 to 2.80.x. Root cause points to MSBuild/Xamarin tooling but SkiaSharp may need to provide a mitigation in its build targets. Needs investigation into whether this is still reproducible and whether Xamarin.Forms support is still active.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/Build |
| Platforms | os/iOS |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a class library referencing SkiaSharp.Views.Forms 2.80.x targeting Xamarin.iOS
2. Run `msbuild -t:pack`
3. Observe error about missing Native.*.manifest file

**Repository links:**
- https://github.com/dotnet/msbuild/issues/4584 — Root-cause MSBuild issue: NativeReference manifest not generated when target is pack (referenced by contributor maxkatz6)

**Attachments:**
- SkiaSharpPack (reproduction repository) — https://github.com/samtun/SkiaSharpPack

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | True |
| Error type | build-error |
| Error message | The file '.../bin/iPhoneSimulator/Debug/Native.ClassLibraryiOS.manifest' to be packed was not found on disk. |
| Repro quality | complete |
| Target frameworks | Xamarin.iOS |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.x, 1.68.3 |
| Worked in | 1.68.3 |
| Broke in | 2.80.x |
| Current relevance | unknown |
| Relevance reason | Issue filed in 2021 against 2.80.x. No fix confirmation. Xamarin.Forms support may be deprecated in current versions. |

### Regression

| Field | Value |
|-------|-------|
| Is regression | True |
| Confidence | 0.85 (85%) |
| Reason | Reporter explicitly states it worked in 1.68.3 and broke in 2.80.x |
| Worked in version | 1.68.3 |
| Broke in version | 2.80.x |

## Analysis

### Technical Summary

When packing a Xamarin.iOS class library that references SkiaSharp.Views.Forms 2.80.x, MSBuild expects a 'Native.*.manifest' file to be present as a pack output, but this file is never generated because the NativeReference handling in Xamarin.iOS does not produce the manifest during a pack-only build. This is a downstream effect of the upstream MSBuild issue #4584: native references from NuGet packages cause the pack target to expect a manifest file that only gets created during a full build, not during a pack operation.

### Rationale

Classified as type/bug area/Build because packing class libraries that reference SkiaSharp.Views.Forms broke between 1.68.3 and 2.80.x. The root cause is external (MSBuild + Xamarin NativeReference handling), but SkiaSharp could potentially provide a mitigation. Marked medium severity as a workaround exists (manually creating the manifest) and it's a Xamarin.Forms/Xamarin.iOS-specific scenario that may affect fewer users given MAUI migration trends.

### Key Signals

- "The file '.../bin/iPhoneSimulator/Debug/Native.ClassLibraryiOS.manifest' to be packed was not found on disk." — **Issue body - build error output** (MSBuild pack target requires the manifest file that Xamarin.iOS NativeReference handling generates. The file is only created during a regular build, not pack-only.)
- "Original issue is this https://github.com/dotnet/msbuild/issues/4584" — **Comment by maxkatz6** (This is a known upstream MSBuild issue, not strictly a SkiaSharp bug. SkiaSharp could potentially work around it by adjusting how it declares NativeReferences.)
- "Creating the missing manifest file manually in the expected location fixes the process" — **Issue body** (Workaround exists - the manifest file just needs to exist. SkiaSharp could generate/include a stub manifest to prevent the error.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `source/SkiaSharp.NuGet.targets` | — | related | SkiaSharp defines custom NuGet packing targets but does not include any NativeReference or manifest generation logic. The manifest file is expected by Xamarin.iOS build tooling when packing transitive NativeReferences from NuGet packages. |
| `source/SkiaSharp.Views (SkiaSharp.Views.Forms not found in current codebase)` | — | direct | SkiaSharp.Views.Forms is not present in the current repository structure. The source/SkiaSharp.Views directory only contains Blazor, WinUI, WindowsForms (classic), and Uno variants. Xamarin.Forms support was likely removed or deprecated in later versions. |

### Resolution Proposals

**Hypothesis:** SkiaSharp.Views.Forms 2.80.x introduced NativeReferences that trigger the Xamarin.iOS pack manifest requirement. The fix would be for SkiaSharp's build targets (or Xamarin tooling) to ensure the manifest is generated during pack operations, or for SkiaSharp to include a stub manifest.

1. **Investigate workaround: create empty manifest in build targets** — investigation, cost/s, validated=untested
   - Add a custom MSBuild target in SkiaSharp.Views.Forms NuGet package that generates an empty `Native.$(AssemblyName).manifest` file before the `GenerateNuspec` target runs, similar to the workaround described in dotnet/msbuild#4584.
2. **Workaround for users: manually create manifest before packing** — workaround, cost/xs, validated=untested
   - Until fixed, users can manually create the missing manifest file at the expected path before running `msbuild -t:pack`. The path follows: `bin/{Platform}/{Config}/Native.{AssemblyName}.manifest`.

**Recommended proposal:** Investigate workaround: create empty manifest in build targets

**Why:** Investigating a build target fix addresses the root cause; the workaround can be shared with the reporter immediately.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.75 (75%) |
| Reason | Complete reproduction provided, confirmed regression from 1.68.3 to 2.80.x. Root cause points to MSBuild/Xamarin tooling but SkiaSharp may need to provide a mitigation in its build targets. Needs investigation into whether this is still reproducible and whether Xamarin.Forms support is still active. |
| Suggested repro platform | macos |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply triage classification labels | labels=type/bug, area/Build, os/iOS, tenet/compatibility |
| add-comment | medium | 0.75 (75%) | Acknowledge the issue and share workaround while investigating | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and reproduction repository!

This appears to be related to the upstream MSBuild issue [dotnet/msbuild#4584](https://github.com/dotnet/msbuild/issues/4584), where NativeReference items from NuGet packages cause the pack target to expect a `Native.*.manifest` file that is only generated during a regular build — not during a `pack`-only operation.

**Workaround (while this is investigated):**
Before running `msbuild -t:pack`, do a regular build first so the manifest file is generated:
```shell
msbuild -t:build
msbuild -t:pack
```
Or manually create an empty manifest file at the expected location:
```
bin/iPhoneSimulator/Debug/Native.{YourAssemblyName}.manifest
```

Note: SkiaSharp.Views.Forms targets Xamarin.Forms which is now deprecated in favor of .NET MAUI. If you're able to migrate to MAUI, [SkiaSharp.Views.Maui](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls) is the actively maintained successor.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1830,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-01T02:10:00Z"
  },
  "summary": "Packing a class library that references SkiaSharp.Views.Forms 2.80.x via `msbuild -t:pack` fails because a `Native.*.manifest` file is expected but not generated on disk.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.88
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "platforms": [
      "os/iOS"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": true,
      "errorType": "build-error",
      "errorMessage": "The file '.../bin/iPhoneSimulator/Debug/Native.ClassLibraryiOS.manifest' to be packed was not found on disk.",
      "reproQuality": "complete",
      "targetFrameworks": [
        "Xamarin.iOS"
      ]
    },
    "reproEvidence": {
      "codeSnippets": [],
      "attachments": [
        {
          "url": "https://github.com/samtun/SkiaSharpPack",
          "filename": "SkiaSharpPack (reproduction repository)"
        }
      ],
      "repoLinks": [
        {
          "url": "https://github.com/dotnet/msbuild/issues/4584",
          "description": "Root-cause MSBuild issue: NativeReference manifest not generated when target is pack (referenced by contributor maxkatz6)"
        }
      ],
      "stepsToReproduce": [
        "Create a class library referencing SkiaSharp.Views.Forms 2.80.x targeting Xamarin.iOS",
        "Run `msbuild -t:pack`",
        "Observe error about missing Native.*.manifest file"
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.x",
        "1.68.3"
      ],
      "workedIn": "1.68.3",
      "brokeIn": "2.80.x",
      "currentRelevance": "unknown",
      "relevanceReason": "Issue filed in 2021 against 2.80.x. No fix confirmation. Xamarin.Forms support may be deprecated in current versions."
    },
    "regression": {
      "isRegression": true,
      "confidence": 0.85,
      "reason": "Reporter explicitly states it worked in 1.68.3 and broke in 2.80.x",
      "workedInVersion": "1.68.3",
      "brokeInVersion": "2.80.x"
    }
  },
  "analysis": {
    "summary": "When packing a Xamarin.iOS class library that references SkiaSharp.Views.Forms 2.80.x, MSBuild expects a 'Native.*.manifest' file to be present as a pack output, but this file is never generated because the NativeReference handling in Xamarin.iOS does not produce the manifest during a pack-only build. This is a downstream effect of the upstream MSBuild issue #4584: native references from NuGet packages cause the pack target to expect a manifest file that only gets created during a full build, not during a pack operation.",
    "codeInvestigation": [
      {
        "file": "source/SkiaSharp.NuGet.targets",
        "finding": "SkiaSharp defines custom NuGet packing targets but does not include any NativeReference or manifest generation logic. The manifest file is expected by Xamarin.iOS build tooling when packing transitive NativeReferences from NuGet packages.",
        "relevance": "related"
      },
      {
        "file": "source/SkiaSharp.Views (SkiaSharp.Views.Forms not found in current codebase)",
        "finding": "SkiaSharp.Views.Forms is not present in the current repository structure. The source/SkiaSharp.Views directory only contains Blazor, WinUI, WindowsForms (classic), and Uno variants. Xamarin.Forms support was likely removed or deprecated in later versions.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "The file '.../bin/iPhoneSimulator/Debug/Native.ClassLibraryiOS.manifest' to be packed was not found on disk.",
        "source": "Issue body - build error output",
        "interpretation": "MSBuild pack target requires the manifest file that Xamarin.iOS NativeReference handling generates. The file is only created during a regular build, not pack-only."
      },
      {
        "text": "Original issue is this https://github.com/dotnet/msbuild/issues/4584",
        "source": "Comment by maxkatz6",
        "interpretation": "This is a known upstream MSBuild issue, not strictly a SkiaSharp bug. SkiaSharp could potentially work around it by adjusting how it declares NativeReferences."
      },
      {
        "text": "Creating the missing manifest file manually in the expected location fixes the process",
        "source": "Issue body",
        "interpretation": "Workaround exists - the manifest file just needs to exist. SkiaSharp could generate/include a stub manifest to prevent the error."
      }
    ],
    "rationale": "Classified as type/bug area/Build because packing class libraries that reference SkiaSharp.Views.Forms broke between 1.68.3 and 2.80.x. The root cause is external (MSBuild + Xamarin NativeReference handling), but SkiaSharp could potentially provide a mitigation. Marked medium severity as a workaround exists (manually creating the manifest) and it's a Xamarin.Forms/Xamarin.iOS-specific scenario that may affect fewer users given MAUI migration trends.",
    "resolution": {
      "hypothesis": "SkiaSharp.Views.Forms 2.80.x introduced NativeReferences that trigger the Xamarin.iOS pack manifest requirement. The fix would be for SkiaSharp's build targets (or Xamarin tooling) to ensure the manifest is generated during pack operations, or for SkiaSharp to include a stub manifest.",
      "proposals": [
        {
          "title": "Investigate workaround: create empty manifest in build targets",
          "description": "Add a custom MSBuild target in SkiaSharp.Views.Forms NuGet package that generates an empty `Native.$(AssemblyName).manifest` file before the `GenerateNuspec` target runs, similar to the workaround described in dotnet/msbuild#4584.",
          "category": "investigation",
          "effort": "cost/s",
          "validated": "untested"
        },
        {
          "title": "Workaround for users: manually create manifest before packing",
          "description": "Until fixed, users can manually create the missing manifest file at the expected path before running `msbuild -t:pack`. The path follows: `bin/{Platform}/{Config}/Native.{AssemblyName}.manifest`.",
          "category": "workaround",
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Investigate workaround: create empty manifest in build targets",
      "recommendedReason": "Investigating a build target fix addresses the root cause; the workaround can be shared with the reporter immediately."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.75,
      "reason": "Complete reproduction provided, confirmed regression from 1.68.3 to 2.80.x. Root cause points to MSBuild/Xamarin tooling but SkiaSharp may need to provide a mitigation in its build targets. Needs investigation into whether this is still reproducible and whether Xamarin.Forms support is still active.",
      "suggestedReproPlatform": "macos"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply triage classification labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/bug",
          "area/Build",
          "os/iOS",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Acknowledge the issue and share workaround while investigating",
        "risk": "medium",
        "confidence": 0.75,
        "comment": "Thanks for the detailed report and reproduction repository!\n\nThis appears to be related to the upstream MSBuild issue [dotnet/msbuild#4584](https://github.com/dotnet/msbuild/issues/4584), where NativeReference items from NuGet packages cause the pack target to expect a `Native.*.manifest` file that is only generated during a regular build — not during a `pack`-only operation.\n\n**Workaround (while this is investigated):**\nBefore running `msbuild -t:pack`, do a regular build first so the manifest file is generated:\n```shell\nmsbuild -t:build\nmsbuild -t:pack\n```\nOr manually create an empty manifest file at the expected location:\n```\nbin/iPhoneSimulator/Debug/Native.{YourAssemblyName}.manifest\n```\n\nNote: SkiaSharp.Views.Forms targets Xamarin.Forms which is now deprecated in favor of .NET MAUI. If you're able to migrate to MAUI, [SkiaSharp.Views.Maui](https://www.nuget.org/packages/SkiaSharp.Views.Maui.Controls) is the actively maintained successor."
      }
    ]
  }
}
```

</details>
