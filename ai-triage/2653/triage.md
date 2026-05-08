# Issue Triage Report — #2653

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:20:00Z |
| Type | type/bug (0.92 (92%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-investigation (0.80 (80%)) |

**Issue Summary:** On Alpine Linux (musl) in a Docker container, SkiaSharp 2.88.6 fails to load libSkiaSharp.so at runtime with a confusing 'liblibSkiaSharp' error; multiple users across versions (including a potential regression in 3.116.0 where linux-musl-x64 is missing from publish output) report the same failure.

**Analysis:** Native library loading fails on Alpine Linux because the NativeAssets.Linux package (with fontconfig dependency) is used instead of NativeAssets.Linux.NoDependencies, or fontconfig is not installed in the container. The confusing 'liblibSkiaSharp' error is a .NET runtime diagnostic artifact — when dlopen searches fallback paths it constructs the filename as 'lib' + 'libSkiaSharp', producing 'liblibSkiaSharp'. For 3.116.0+ there is a separate reported regression where dotnet publish omits linux-musl-x64 from the output directory.

**Recommendations:** **needs-investigation** — The 2.88.x case has clear workarounds (NoDependencies or fontconfig), but the 3.116.0 report of missing linux-musl-x64 publish output is a potential packaging regression that needs verification. Issue has 44 comments and 9 upvotes indicating broad impact.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/bug |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |
| Current labels | type/bug, os/Linux, area/libSkiaSharp.native, status/needs-attention, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Create a .NET Core 7.0 app using SkiaSharp.NativeAssets.Linux 2.88.6
2. Run the app in an Alpine Linux Docker container (linux-musl-x64)
3. Call SKBitmap.Decode(stream) or any SkiaSharp API that loads the native library
4. Observe DllNotFoundException with 'liblibSkiaSharp' in the error output

**Environment:** .NET 7.0, Alpine Linux (musl), Docker, SkiaSharp 2.88.6, SkiaSharp.NativeAssets.Linux

**Related issues:** #2215, #453

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2215 — Related: Alpine Linux DllNotFoundException with NoDependencies 2.88.0 (closed)
- https://github.com/mono/SkiaSharp/issues/2653#issuecomment-2561032188 — 3.116.0 regression: linux-musl-x64 not published in dotnet publish output

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | exception |
| Error message | Unable to load shared library 'libSkiaSharp' or one of its dependencies. Error loading shared library liblibSkiaSharp: No such file or directory |
| Repro quality | partial |
| Target frameworks | net7.0 |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_managedstream_set_procs(SKManagedStreamDelegates procs)
at SkiaSharp.SKAbstractManagedStream..cctor()
at SkiaSharp.SKAbstractManagedStream..ctor(Boolean owns)
at SkiaSharp.SKManagedStream..ctor(Stream managedStream, Boolean disposeManagedStream)
at SkiaSharp.SKBitmap.Decode(Stream stream)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.6, 2.88.8, 2.88.9, 3.116.0 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Issue is still open as of April 2026 with recent comments about 3.116.0 regression; multiple versions affected |

## Analysis

### Technical Summary

Native library loading fails on Alpine Linux because the NativeAssets.Linux package (with fontconfig dependency) is used instead of NativeAssets.Linux.NoDependencies, or fontconfig is not installed in the container. The confusing 'liblibSkiaSharp' error is a .NET runtime diagnostic artifact — when dlopen searches fallback paths it constructs the filename as 'lib' + 'libSkiaSharp', producing 'liblibSkiaSharp'. For 3.116.0+ there is a separate reported regression where dotnet publish omits linux-musl-x64 from the output directory.

### Rationale

The stack trace and error log clearly show DllNotFoundException — the native .so was not found at runtime. The 'liblibSkiaSharp' pattern is documented in packages.md as a fallback-path artifact. The root cause for 2.88.x is almost always wrong package selection (Linux vs NoDependencies) or missing fontconfig. The 3.116.0 regression (missing musl publish output) is a separate potential packaging issue that warrants investigation.

### Key Signals

- "Error loading shared library /app/runtimes/linux-musl-x64/native/liblibSkiaSharp: No such file or directory" — **issue body — log output** (.NET fallback probe generated 'liblibSkiaSharp' as filename; the actual file libSkiaSharp.so is in that directory but is not being found — indicates the runtime cannot load it (likely wrong package or missing fontconfig dependency))
- "I've installed the package SkiaSharp.NativeAssets.Linux in my project" — **issue body** (Reporter used NativeAssets.Linux (requires fontconfig) not NoDependencies — on Alpine without fontconfig this will fail to load)
- "SkiaSharp.NativeAssets.Linux.NoDependencies version 2.88.6, along with the base skiasharp 2.88.6 worked fine for me" — **comment #1804674889 by hsellentin** (Confirms switching to NoDependencies resolves the issue for the 2.88.6 case on Alpine)
- "no runtime libraries for linux-musl-x64 are published [in 3.116.0]" — **comment #2561032188 by Ghostbird** (Potential packaging regression in 3.116.0 where linux-musl-x64 native assets are omitted from dotnet publish output)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 12 | direct | P/Invoke constant SKIA = 'libSkiaSharp'. On Linux, when .NET's native library resolver probes fallback paths, it constructs 'lib' + 'libSkiaSharp' = 'liblibSkiaSharp' as the filename — explaining the confusing error message. This is expected .NET runtime behavior, not a SkiaSharp naming bug. |
| `binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets` | 32-50 | direct | NoDependencies targets include linux-musl-x64 runtime files (libSkiaSharp*.so). If the reporter used NativeAssets.Linux (not NoDependencies), fontconfig is required on Alpine. The targets file confirms both packages include musl variants for 2.88.x. |
| `documentation/dev/packages.md` | 177-215 | context | The packages.md troubleshooting section documents this exact error pattern: 'liblibSkiaSharp' is a known fallback-path artifact from the .NET runtime loader. Fix is always NativeAssets.Linux.NoDependencies for minimal/Alpine containers, or installing fontconfig for the full variant. |

### Workarounds

- Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies in the executable project's PackageReference
- If sticking with NativeAssets.Linux, install fontconfig: RUN apt-get install -y libfontconfig1 (Debian/Ubuntu) or apk add fontconfig (Alpine)
- Ensure NativeAssets package is in the executable project (not only a library project)
- Use 'dotnet publish -r linux-musl-x64' to force correct RID selection on Alpine

### Next Questions

- Is the 3.116.0 regression (missing linux-musl-x64 in publish output) reproducible with a minimal Dockerfile?
- Does Ghostbird's issue affect NoDependencies as well or only NativeAssets.Linux?
- What base image is being used — is it using glibc or musl (Alpine)?

### Resolution Proposals

**Hypothesis:** For 2.88.x: reporter used NativeAssets.Linux on Alpine without fontconfig installed — switching to NoDependencies or adding fontconfig resolves it. For 3.116.0+: potential packaging regression where linux-musl-x64 native binaries are not published by dotnet publish.

1. **Switch to NoDependencies package** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. This package has no external dependencies and includes both glibc and musl variants — designed for Alpine and minimal containers.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.6" />
```
2. **Install fontconfig in Dockerfile** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Keep NativeAssets.Linux but add fontconfig to the container. Useful when system font enumeration is needed.

```csharp
RUN apk add --no-cache fontconfig
```
3. **Investigate 3.116.0 musl publish regression** — investigation, confidence 0.70 (70%), cost/s, validated=untested
   - Reproduce Ghostbird's report: create a minimal Dockerfile using dotnet publish targeting linux-musl-x64 with SkiaSharp 3.116.0 and verify whether linux-musl-x64 native assets are included in publish output.

**Recommended proposal:** Switch to NoDependencies package

**Why:** Addresses the root cause for the majority of reporters (Alpine/musl containers) with minimal effort. Documented as the correct package for this scenario in packages.md.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.80 (80%) |
| Reason | The 2.88.x case has clear workarounds (NoDependencies or fontconfig), but the 3.116.0 report of missing linux-musl-x64 publish output is a potential packaging regression that needs verification. Issue has 44 comments and 9 upvotes indicating broad impact. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply correct labels — type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability (already present) | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.85 (85%) | Post a comment explaining the liblibSkiaSharp error pattern, the fix (NoDependencies), and ask for a repro for the 3.116 regression | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report and to everyone who has added context in the comments!

The `liblibSkiaSharp` error is a diagnostic artifact from the .NET runtime loader — when it probes fallback paths it constructs the filename as `lib` + `libSkiaSharp`, producing `liblibSkiaSharp`. The underlying issue is that the native `libSkiaSharp.so` cannot be loaded.

**For most Alpine/musl Docker containers (2.88.x):**
Switch from `SkiaSharp.NativeAssets.Linux` to `SkiaSharp.NativeAssets.Linux.NoDependencies`. The `NoDependencies` variant has zero external dependencies and includes musl variants:
```xml
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.6" />
```

If you need system font enumeration, keep `NativeAssets.Linux` but install fontconfig:
```dockerfile
RUN apk add --no-cache fontconfig   # Alpine
# or: RUN apt-get install -y libfontconfig1   # Debian/Ubuntu
```
Also ensure the NativeAssets package is referenced in the **executable** project, not only a library project.

**For 3.116.0 (linux-musl-x64 missing from publish output):**
@Ghostbird — could you share a minimal Dockerfile or reproduction repository? This looks like it may be a separate packaging regression and I want to verify it.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2653,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:20:00Z",
    "currentLabels": [
      "type/bug",
      "os/Linux",
      "area/libSkiaSharp.native",
      "status/needs-attention",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "On Alpine Linux (musl) in a Docker container, SkiaSharp 2.88.6 fails to load libSkiaSharp.so at runtime with a confusing 'liblibSkiaSharp' error; multiple users across versions (including a potential regression in 3.116.0 where linux-musl-x64 is missing from publish output) report the same failure.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.92
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.95
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "bugSignals": {
      "severity": "high",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unable to load shared library 'libSkiaSharp' or one of its dependencies. Error loading shared library liblibSkiaSharp: No such file or directory",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_managedstream_set_procs(SKManagedStreamDelegates procs)\nat SkiaSharp.SKAbstractManagedStream..cctor()\nat SkiaSharp.SKAbstractManagedStream..ctor(Boolean owns)\nat SkiaSharp.SKManagedStream..ctor(Stream managedStream, Boolean disposeManagedStream)\nat SkiaSharp.SKBitmap.Decode(Stream stream)",
      "reproQuality": "partial",
      "targetFrameworks": [
        "net7.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Core 7.0 app using SkiaSharp.NativeAssets.Linux 2.88.6",
        "Run the app in an Alpine Linux Docker container (linux-musl-x64)",
        "Call SKBitmap.Decode(stream) or any SkiaSharp API that loads the native library",
        "Observe DllNotFoundException with 'liblibSkiaSharp' in the error output"
      ],
      "environmentDetails": ".NET 7.0, Alpine Linux (musl), Docker, SkiaSharp 2.88.6, SkiaSharp.NativeAssets.Linux",
      "relatedIssues": [
        2215,
        453
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2215",
          "description": "Related: Alpine Linux DllNotFoundException with NoDependencies 2.88.0 (closed)"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2653#issuecomment-2561032188",
          "description": "3.116.0 regression: linux-musl-x64 not published in dotnet publish output"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.6",
        "2.88.8",
        "2.88.9",
        "3.116.0"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Issue is still open as of April 2026 with recent comments about 3.116.0 regression; multiple versions affected"
    }
  },
  "analysis": {
    "summary": "Native library loading fails on Alpine Linux because the NativeAssets.Linux package (with fontconfig dependency) is used instead of NativeAssets.Linux.NoDependencies, or fontconfig is not installed in the container. The confusing 'liblibSkiaSharp' error is a .NET runtime diagnostic artifact — when dlopen searches fallback paths it constructs the filename as 'lib' + 'libSkiaSharp', producing 'liblibSkiaSharp'. For 3.116.0+ there is a separate reported regression where dotnet publish omits linux-musl-x64 from the output directory.",
    "rationale": "The stack trace and error log clearly show DllNotFoundException — the native .so was not found at runtime. The 'liblibSkiaSharp' pattern is documented in packages.md as a fallback-path artifact. The root cause for 2.88.x is almost always wrong package selection (Linux vs NoDependencies) or missing fontconfig. The 3.116.0 regression (missing musl publish output) is a separate potential packaging issue that warrants investigation.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "12",
        "finding": "P/Invoke constant SKIA = 'libSkiaSharp'. On Linux, when .NET's native library resolver probes fallback paths, it constructs 'lib' + 'libSkiaSharp' = 'liblibSkiaSharp' as the filename — explaining the confusing error message. This is expected .NET runtime behavior, not a SkiaSharp naming bug.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux.NoDependencies/buildTransitive/net4/SkiaSharp.targets",
        "lines": "32-50",
        "finding": "NoDependencies targets include linux-musl-x64 runtime files (libSkiaSharp*.so). If the reporter used NativeAssets.Linux (not NoDependencies), fontconfig is required on Alpine. The targets file confirms both packages include musl variants for 2.88.x.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "177-215",
        "finding": "The packages.md troubleshooting section documents this exact error pattern: 'liblibSkiaSharp' is a known fallback-path artifact from the .NET runtime loader. Fix is always NativeAssets.Linux.NoDependencies for minimal/Alpine containers, or installing fontconfig for the full variant.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Error loading shared library /app/runtimes/linux-musl-x64/native/liblibSkiaSharp: No such file or directory",
        "source": "issue body — log output",
        "interpretation": ".NET fallback probe generated 'liblibSkiaSharp' as filename; the actual file libSkiaSharp.so is in that directory but is not being found — indicates the runtime cannot load it (likely wrong package or missing fontconfig dependency)"
      },
      {
        "text": "I've installed the package SkiaSharp.NativeAssets.Linux in my project",
        "source": "issue body",
        "interpretation": "Reporter used NativeAssets.Linux (requires fontconfig) not NoDependencies — on Alpine without fontconfig this will fail to load"
      },
      {
        "text": "SkiaSharp.NativeAssets.Linux.NoDependencies version 2.88.6, along with the base skiasharp 2.88.6 worked fine for me",
        "source": "comment #1804674889 by hsellentin",
        "interpretation": "Confirms switching to NoDependencies resolves the issue for the 2.88.6 case on Alpine"
      },
      {
        "text": "no runtime libraries for linux-musl-x64 are published [in 3.116.0]",
        "source": "comment #2561032188 by Ghostbird",
        "interpretation": "Potential packaging regression in 3.116.0 where linux-musl-x64 native assets are omitted from dotnet publish output"
      }
    ],
    "workarounds": [
      "Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies in the executable project's PackageReference",
      "If sticking with NativeAssets.Linux, install fontconfig: RUN apt-get install -y libfontconfig1 (Debian/Ubuntu) or apk add fontconfig (Alpine)",
      "Ensure NativeAssets package is in the executable project (not only a library project)",
      "Use 'dotnet publish -r linux-musl-x64' to force correct RID selection on Alpine"
    ],
    "nextQuestions": [
      "Is the 3.116.0 regression (missing linux-musl-x64 in publish output) reproducible with a minimal Dockerfile?",
      "Does Ghostbird's issue affect NoDependencies as well or only NativeAssets.Linux?",
      "What base image is being used — is it using glibc or musl (Alpine)?"
    ],
    "resolution": {
      "hypothesis": "For 2.88.x: reporter used NativeAssets.Linux on Alpine without fontconfig installed — switching to NoDependencies or adding fontconfig resolves it. For 3.116.0+: potential packaging regression where linux-musl-x64 native binaries are not published by dotnet publish.",
      "proposals": [
        {
          "title": "Switch to NoDependencies package",
          "description": "Replace SkiaSharp.NativeAssets.Linux with SkiaSharp.NativeAssets.Linux.NoDependencies in the application project. This package has no external dependencies and includes both glibc and musl variants — designed for Alpine and minimal containers.",
          "category": "workaround",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.6\" />",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Install fontconfig in Dockerfile",
          "description": "Keep NativeAssets.Linux but add fontconfig to the container. Useful when system font enumeration is needed.",
          "category": "workaround",
          "codeSnippet": "RUN apk add --no-cache fontconfig",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate 3.116.0 musl publish regression",
          "description": "Reproduce Ghostbird's report: create a minimal Dockerfile using dotnet publish targeting linux-musl-x64 with SkiaSharp 3.116.0 and verify whether linux-musl-x64 native assets are included in publish output.",
          "category": "investigation",
          "confidence": 0.7,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to NoDependencies package",
      "recommendedReason": "Addresses the root cause for the majority of reporters (Alpine/musl containers) with minimal effort. Documented as the correct package for this scenario in packages.md."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.8,
      "reason": "The 2.88.x case has clear workarounds (NoDependencies or fontconfig), but the 3.116.0 report of missing linux-musl-x64 publish output is a potential packaging regression that needs verification. Issue has 44 comments and 9 upvotes indicating broad impact.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply correct labels — type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability (already present)",
        "risk": "low",
        "confidence": 0.95,
        "labels": [
          "type/bug",
          "area/libSkiaSharp.native",
          "os/Linux",
          "tenet/reliability"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post a comment explaining the liblibSkiaSharp error pattern, the fix (NoDependencies), and ask for a repro for the 3.116 regression",
        "risk": "medium",
        "confidence": 0.85,
        "comment": "Thanks for the report and to everyone who has added context in the comments!\n\nThe `liblibSkiaSharp` error is a diagnostic artifact from the .NET runtime loader — when it probes fallback paths it constructs the filename as `lib` + `libSkiaSharp`, producing `liblibSkiaSharp`. The underlying issue is that the native `libSkiaSharp.so` cannot be loaded.\n\n**For most Alpine/musl Docker containers (2.88.x):**\nSwitch from `SkiaSharp.NativeAssets.Linux` to `SkiaSharp.NativeAssets.Linux.NoDependencies`. The `NoDependencies` variant has zero external dependencies and includes musl variants:\n```xml\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.6\" />\n```\n\nIf you need system font enumeration, keep `NativeAssets.Linux` but install fontconfig:\n```dockerfile\nRUN apk add --no-cache fontconfig   # Alpine\n# or: RUN apt-get install -y libfontconfig1   # Debian/Ubuntu\n```\nAlso ensure the NativeAssets package is referenced in the **executable** project, not only a library project.\n\n**For 3.116.0 (linux-musl-x64 missing from publish output):**\n@Ghostbird — could you share a minimal Dockerfile or reproduction repository? This looks like it may be a separate packaging regression and I want to verify it."
      }
    ]
  }
}
```

</details>
