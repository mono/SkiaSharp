# Issue Triage Report — #3612

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-07-11T05:20:00Z |
| Type | type/question (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User reports DllNotFoundException for libSkiaSharp on RHEL9-based Docker container due to missing libfontconfig.so.1 dependency; root cause is that SkiaSharp.NativeAssets.Linux (fontconfig-dependent variant) is being loaded instead of the NoDependencies variant, which is a known deployment pattern documented in packages.md.

**Analysis:** The DllNotFoundException is caused by libSkiaSharp.so being found but its fontconfig dependency missing in the minimal RHEL9 container. The error pattern (libfontconfig.so.1 as first failure) confirms the fontconfig-dependent variant of libSkiaSharp is being loaded, not the NoDependencies variant. The user reports that adding SkiaSharp.NativeAssets.Linux.NoDependencies 'did not make a difference', which is a known issue when NativeAssets packages are added to a library project instead of the executable (application) project.

**Recommendations:** **close-as-not-a-bug** — This is a known deployment pattern documented in packages.md. The libfontconfig error confirms the fontconfig-dependent binary is being loaded. NoDependencies 'not helping' is a classic sign of wrong project placement. Both workarounds are well-documented. No SkiaSharp bug exists.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/compatibility |
| Perf | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. Create a .NET application referencing SkiaSharp 3.116.0 using LiveCharts2
2. Deploy to a RHEL9-based Docker container (ubi9/dotnet-100)
3. Execute code that creates SKBitmap or otherwise initializes SKImageInfo
4. Observe DllNotFoundException with libfontconfig.so.1 as the first missing dependency

**Environment:** RHEL9 Docker (registry.access.redhat.com/ubi9/dotnet-100:9.7-1771893416), SkiaSharp 3.116.0, net10.0

**Related issues:** #2653

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | Unable to load shared library 'libSkiaSharp' or one of its dependencies. libfontconfig.so.1: cannot open shared object file: No such file or directory |
| Repro quality | complete |
| Target frameworks | net10.0 |

**Stack trace:**

```text
System.TypeInitializationException -> System.DllNotFoundException at SkiaSharp.SkiaApi.sk_colortype_get_default_8888() -> SKImageInfo..cctor()
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The NativeAssets.Linux deployment pattern and fontconfig dependency have not changed across recent releases. |

## Analysis

### Technical Summary

The DllNotFoundException is caused by libSkiaSharp.so being found but its fontconfig dependency missing in the minimal RHEL9 container. The error pattern (libfontconfig.so.1 as first failure) confirms the fontconfig-dependent variant of libSkiaSharp is being loaded, not the NoDependencies variant. The user reports that adding SkiaSharp.NativeAssets.Linux.NoDependencies 'did not make a difference', which is a known issue when NativeAssets packages are added to a library project instead of the executable (application) project.

### Rationale

The first error in the DllNotFoundException output is 'libfontconfig.so.1: cannot open shared object file' — this means libSkiaSharp.so was located but its dependency fontconfig is missing. This is the exact pattern documented in packages.md Troubleshooting section for SkiaSharp.NativeAssets.Linux (fontconfig variant). The NoDependencies package not helping suggests it was added to a library project rather than the executable project, causing the runtime to still pick up the fontconfig-dependent binary from the core SkiaSharp transitive dependency. The user found a valid workaround (installing fontconfig) which confirms the diagnosis.

### Key Signals

- "libfontconfig.so.1: cannot open shared object file: No such file or directory" — **issue body - stack trace** (First error in DllNotFoundException — libSkiaSharp.so was found but depends on fontconfig. This is the NativeAssets.Linux (not NoDependencies) binary being loaded.)
- "I have tried adding the Nuget Packages: SkiaSharp.NativeAssets.Linux, SkiaSharp.NativeAssets.Linux.NoDependencies. But those did not make a difference." — **issue body** (NoDependencies not helping is a classic sign the package was added to a library project, not the executable project, so the correct binary is not deployed.)
- "microdnf install -y fontconfig freetype freetype-devel libpng expat" — **issue body - workaround** (Installing fontconfig fixed the problem, confirming the fontconfig-dependent binary is being used. The correct solution for containers is NoDependencies in the executable project.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj` | — | direct | SkiaSharp.NativeAssets.Linux packages binaries from output/native/linux/{arch} which require fontconfig. These are placed under runtimes/linux-x64/native/ — if this package is transitively included, it wins over NoDependencies. |
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj` | — | direct | NoDependencies packages binaries from output/native/linuxnodeps/{arch} with no fontconfig dependency. However, if this package is added to a library project rather than the executable project, the runtime won't find it and will fall back to the fontconfig-dependent binary. |
| `documentation/dev/packages.md` | 175-200 | context | The Troubleshooting section explicitly documents: 'libfontconfig.so.1 error means .so was found but has a dependency not installed — switch to NoDependencies'. It also documents that NativeAssets must be in the executable project, not a library project. |

### Workarounds

- Add SkiaSharp.NativeAssets.Linux.NoDependencies to the executable (application) project — not a library project — to ensure the no-dependency binary is deployed
- If fontconfig is acceptable, install it in the container: microdnf install -y fontconfig (as the reporter discovered)
- Check for transitive SkiaSharp.NativeAssets.Linux references in library projects that might conflict with NoDependencies

### Next Questions

- Was SkiaSharp.NativeAssets.Linux.NoDependencies added to the application's .csproj directly, or only to a library project?
- Is there a transitive reference to SkiaSharp.NativeAssets.Linux from LiveCharts2 or another library?

### Resolution Proposals

**Hypothesis:** The fontconfig-dependent NativeAssets.Linux binary is deployed because NoDependencies is only referenced in a library project (not the executable), and/or LiveCharts2 has a transitive reference to NativeAssets.Linux.

1. **Add NoDependencies to executable project** — workaround, confidence 0.92 (92%), cost/xs, validated=untested
   - Add SkiaSharp.NativeAssets.Linux.NoDependencies as a PackageReference in the application project (the .csproj that produces the executable), not in any library project. This ensures the no-dependency binary is deployed to the output directory.
2. **Install fontconfig in container** — workaround, confidence 0.98 (98%), cost/xs, validated=yes
   - Add 'RUN microdnf install -y fontconfig' to the Dockerfile (or equivalent for your base image). This satisfies the fontconfig dependency of SkiaSharp.NativeAssets.Linux. Reporter confirmed this works.

**Recommended proposal:** Add NoDependencies to executable project

**Why:** Preferred for containers — zero system dependencies beyond glibc. Adding NoDependencies to the correct project is the clean solution. Installing fontconfig works but adds system package overhead.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | This is a known deployment pattern documented in packages.md. The libfontconfig error confirms the fontconfig-dependent binary is being loaded. NoDependencies 'not helping' is a classic sign of wrong project placement. Both workarounds are well-documented. No SkiaSharp bug exists. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Reclassify from type/bug to type/question; add area and platform labels | labels=type/question, area/libSkiaSharp.native, os/Linux, tenet/compatibility |
| add-comment | medium | 0.90 (90%) | Explain the deployment pattern and provide the NoDependencies fix | — |
| close-issue | medium | 0.82 (82%) | Close as answered — known deployment pattern with documented fix | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and for sharing your workaround!

The error `libfontconfig.so.1: cannot open shared object file` as the **first** failure is the key signal: `libSkiaSharp.so` was found, but it's the **fontconfig-dependent** variant (`SkiaSharp.NativeAssets.Linux`), not the no-dependencies variant. That's why installing fontconfig via `microdnf` fixed it.

The reason `SkiaSharp.NativeAssets.Linux.NoDependencies` didn't help is almost certainly one of these:

1. **Wrong project** — NativeAssets packages must be referenced in the **executable (application) project**, not in a library project. If it's only in a library, the runtime won't deploy that binary to the output directory.
2. **Conflicting transitive reference** — LiveCharts2 (or another dependency) may transitively reference `SkiaSharp.NativeAssets.Linux`, which overrides `NoDependencies`.

**Recommended fix for Docker containers:**
```xml
<!-- In your application .csproj (the one that builds the executable) -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.116.0" />
```

This variant has **zero** third-party dependencies (only libc/libm/libpthread/libdl) and works in minimal RHEL9 containers without installing fontconfig. You can verify the correct binary is deployed by running `ldd` on `libSkiaSharp.so` in your publish output — the NoDependencies variant shows only glibc entries.

See the [Linux Package Selection Guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) and [Troubleshooting: Native Loading](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#troubleshooting-native-loading) in our docs for details.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3612,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-07-11T05:20:00Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "User reports DllNotFoundException for libSkiaSharp on RHEL9-based Docker container due to missing libfontconfig.so.1 dependency; root cause is that SkiaSharp.NativeAssets.Linux (fontconfig-dependent variant) is being loaded instead of the NoDependencies variant, which is a known deployment pattern documented in packages.md.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unable to load shared library 'libSkiaSharp' or one of its dependencies. libfontconfig.so.1: cannot open shared object file: No such file or directory",
      "stackTrace": "System.TypeInitializationException -> System.DllNotFoundException at SkiaSharp.SkiaApi.sk_colortype_get_default_8888() -> SKImageInfo..cctor()",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net10.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET application referencing SkiaSharp 3.116.0 using LiveCharts2",
        "Deploy to a RHEL9-based Docker container (ubi9/dotnet-100)",
        "Execute code that creates SKBitmap or otherwise initializes SKImageInfo",
        "Observe DllNotFoundException with libfontconfig.so.1 as the first missing dependency"
      ],
      "codeSnippet": "using var bitmap = new SkiaSharp.SKBitmap(width, height);\nusing var canvas = new SkiaSharp.SKCanvas(bitmap);\nusing var image = SkiaSharp.SKImage.FromBitmap(bitmap);\nusing var data = image.Encode(SkiaSharp.SKEncodedImageFormat.Png, 100);\nreturn Convert.ToBase64String(data.ToArray());",
      "environmentDetails": "RHEL9 Docker (registry.access.redhat.com/ubi9/dotnet-100:9.7-1771893416), SkiaSharp 3.116.0, net10.0",
      "relatedIssues": [
        2653
      ],
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The NativeAssets.Linux deployment pattern and fontconfig dependency have not changed across recent releases."
    }
  },
  "analysis": {
    "summary": "The DllNotFoundException is caused by libSkiaSharp.so being found but its fontconfig dependency missing in the minimal RHEL9 container. The error pattern (libfontconfig.so.1 as first failure) confirms the fontconfig-dependent variant of libSkiaSharp is being loaded, not the NoDependencies variant. The user reports that adding SkiaSharp.NativeAssets.Linux.NoDependencies 'did not make a difference', which is a known issue when NativeAssets packages are added to a library project instead of the executable (application) project.",
    "rationale": "The first error in the DllNotFoundException output is 'libfontconfig.so.1: cannot open shared object file' — this means libSkiaSharp.so was located but its dependency fontconfig is missing. This is the exact pattern documented in packages.md Troubleshooting section for SkiaSharp.NativeAssets.Linux (fontconfig variant). The NoDependencies package not helping suggests it was added to a library project rather than the executable project, causing the runtime to still pick up the fontconfig-dependent binary from the core SkiaSharp transitive dependency. The user found a valid workaround (installing fontconfig) which confirms the diagnosis.",
    "keySignals": [
      {
        "text": "libfontconfig.so.1: cannot open shared object file: No such file or directory",
        "source": "issue body - stack trace",
        "interpretation": "First error in DllNotFoundException — libSkiaSharp.so was found but depends on fontconfig. This is the NativeAssets.Linux (not NoDependencies) binary being loaded."
      },
      {
        "text": "I have tried adding the Nuget Packages: SkiaSharp.NativeAssets.Linux, SkiaSharp.NativeAssets.Linux.NoDependencies. But those did not make a difference.",
        "source": "issue body",
        "interpretation": "NoDependencies not helping is a classic sign the package was added to a library project, not the executable project, so the correct binary is not deployed."
      },
      {
        "text": "microdnf install -y fontconfig freetype freetype-devel libpng expat",
        "source": "issue body - workaround",
        "interpretation": "Installing fontconfig fixed the problem, confirming the fontconfig-dependent binary is being used. The correct solution for containers is NoDependencies in the executable project."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj",
        "finding": "SkiaSharp.NativeAssets.Linux packages binaries from output/native/linux/{arch} which require fontconfig. These are placed under runtimes/linux-x64/native/ — if this package is transitively included, it wins over NoDependencies.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/SkiaSharp.NativeAssets.Linux.NoDependencies.csproj",
        "finding": "NoDependencies packages binaries from output/native/linuxnodeps/{arch} with no fontconfig dependency. However, if this package is added to a library project rather than the executable project, the runtime won't find it and will fall back to the fontconfig-dependent binary.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "175-200",
        "finding": "The Troubleshooting section explicitly documents: 'libfontconfig.so.1 error means .so was found but has a dependency not installed — switch to NoDependencies'. It also documents that NativeAssets must be in the executable project, not a library project.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "Add SkiaSharp.NativeAssets.Linux.NoDependencies to the executable (application) project — not a library project — to ensure the no-dependency binary is deployed",
      "If fontconfig is acceptable, install it in the container: microdnf install -y fontconfig (as the reporter discovered)",
      "Check for transitive SkiaSharp.NativeAssets.Linux references in library projects that might conflict with NoDependencies"
    ],
    "nextQuestions": [
      "Was SkiaSharp.NativeAssets.Linux.NoDependencies added to the application's .csproj directly, or only to a library project?",
      "Is there a transitive reference to SkiaSharp.NativeAssets.Linux from LiveCharts2 or another library?"
    ],
    "resolution": {
      "hypothesis": "The fontconfig-dependent NativeAssets.Linux binary is deployed because NoDependencies is only referenced in a library project (not the executable), and/or LiveCharts2 has a transitive reference to NativeAssets.Linux.",
      "proposals": [
        {
          "title": "Add NoDependencies to executable project",
          "description": "Add SkiaSharp.NativeAssets.Linux.NoDependencies as a PackageReference in the application project (the .csproj that produces the executable), not in any library project. This ensures the no-dependency binary is deployed to the output directory.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Install fontconfig in container",
          "description": "Add 'RUN microdnf install -y fontconfig' to the Dockerfile (or equivalent for your base image). This satisfies the fontconfig dependency of SkiaSharp.NativeAssets.Linux. Reporter confirmed this works.",
          "category": "workaround",
          "confidence": 0.98,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add NoDependencies to executable project",
      "recommendedReason": "Preferred for containers — zero system dependencies beyond glibc. Adding NoDependencies to the correct project is the clean solution. Installing fontconfig works but adds system package overhead."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "This is a known deployment pattern documented in packages.md. The libfontconfig error confirms the fontconfig-dependent binary is being loaded. NoDependencies 'not helping' is a classic sign of wrong project placement. Both workarounds are well-documented. No SkiaSharp bug exists.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Reclassify from type/bug to type/question; add area and platform labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/compatibility"
        ]
      },
      {
        "type": "add-comment",
        "description": "Explain the deployment pattern and provide the NoDependencies fix",
        "risk": "medium",
        "confidence": 0.9,
        "comment": "Thanks for the detailed report and for sharing your workaround!\n\nThe error `libfontconfig.so.1: cannot open shared object file` as the **first** failure is the key signal: `libSkiaSharp.so` was found, but it's the **fontconfig-dependent** variant (`SkiaSharp.NativeAssets.Linux`), not the no-dependencies variant. That's why installing fontconfig via `microdnf` fixed it.\n\nThe reason `SkiaSharp.NativeAssets.Linux.NoDependencies` didn't help is almost certainly one of these:\n\n1. **Wrong project** — NativeAssets packages must be referenced in the **executable (application) project**, not in a library project. If it's only in a library, the runtime won't deploy that binary to the output directory.\n2. **Conflicting transitive reference** — LiveCharts2 (or another dependency) may transitively reference `SkiaSharp.NativeAssets.Linux`, which overrides `NoDependencies`.\n\n**Recommended fix for Docker containers:**\n```xml\n<!-- In your application .csproj (the one that builds the executable) -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.116.0\" />\n```\n\nThis variant has **zero** third-party dependencies (only libc/libm/libpthread/libdl) and works in minimal RHEL9 containers without installing fontconfig. You can verify the correct binary is deployed by running `ldd` on `libSkiaSharp.so` in your publish output — the NoDependencies variant shows only glibc entries.\n\nSee the [Linux Package Selection Guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) and [Troubleshooting: Native Loading](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#troubleshooting-native-loading) in our docs for details."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — known deployment pattern with documented fix",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
