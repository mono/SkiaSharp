# Issue Triage Report — #3229

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-24T14:56:22Z |
| Type | type/bug (0.95 (95%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-investigation (0.85 (85%)) |

**Issue Summary:** libSkiaSharp.so on Linux arm64 and riscv64 fails to load with 'undefined symbol: FT_Get_BDF_Property' because the bundled fontconfig was built against a newer FreeType version that exports this BDF symbol, but the system's installed FreeType lacks it (e.g., Debian Bookworm FreeType 2.12.x compiled without BDF support).

**Analysis:** libSkiaSharp.so for Linux (arm64 / riscv64) is dynamically linked against the system libfontconfig. The build Docker image uses fontconfig 2.15.0-2.3 for riscv64 and 2.13.1-2 for other arches. Both fontconfig versions reference FT_Get_BDF_Property from libfreetype. On end-user systems running Debian Bookworm (FreeType 2.12.x) or any distro where FreeType was compiled without BDF support (TT_CONFIG_OPTION_BDF), the symbol is absent at runtime and the binary fails to load.

**Recommendations:** **needs-investigation** — Clear bug with real-world impact on ARM64/RISC-V Linux devices. Root cause (fontconfig/freetype version mismatch at runtime) is understood; the fix path (static fontconfig linkage or aligned freetype version) needs engineering investigation. Workarounds exist and should be communicated.

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
| Current labels | type/bug, os/Linux, area/libSkiaSharp.native, tenet/reliability, triage/triaged |

## Evidence

### Reproduction

1. Run a .NET 9.0 application referencing SkiaSharp 3.119.0-preview on Linux RISC-V 64 or ARM64
2. The process immediately exits with 'symbol lookup error: libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property'

**Environment:** RISC-V 64 (AOSC OS 12.1 / QEMU), also confirmed on ARM64 (Raspberry Pi Debian Bookworm, Pi 5), SkiaSharp 3.119.0-preview.1.2 / 3.119.0, .NET 8 and 9

**Related issues:** #3272, #3436

**Repository links:**
- https://github.com/mono/SkiaSharp/pull/3221 — PR referenced in issue thread (possibly related fix)
- https://github.com/unoplatform/uno/issues/20429 — Same error on RPi CM4 via Uno Platform
- https://github.com/linuxserver/docker-jellyfin/issues/298 — Same error reported in Jellyfin on Pi5 arm64

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | high |
| Regression claimed | False |
| Error type | crash |
| Error message | symbol lookup error: libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property |
| Repro quality | complete |
| Target frameworks | net9.0 |

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.119.0-preview.1.2, 3.119.0, 3.116.0 |
| Worked in | 3.116.0 |
| Broke in | 3.119.0-preview.1.2 |
| Current relevance | likely |
| Relevance reason | Issue is still open and multiple users confirm it on SkiaSharp 3.119.0 final; the Dockerfile build matrix shows the fontconfig version divergence remains. |

## Analysis

### Technical Summary

libSkiaSharp.so for Linux (arm64 / riscv64) is dynamically linked against the system libfontconfig. The build Docker image uses fontconfig 2.15.0-2.3 for riscv64 and 2.13.1-2 for other arches. Both fontconfig versions reference FT_Get_BDF_Property from libfreetype. On end-user systems running Debian Bookworm (FreeType 2.12.x) or any distro where FreeType was compiled without BDF support (TT_CONFIG_OPTION_BDF), the symbol is absent at runtime and the binary fails to load.

### Rationale

This is a genuine native packaging bug. The NativeAssets.Linux package dynamically links fontconfig, which has a transitive runtime dependency on a specific FreeType version with BDF support. Users on Debian Bookworm (FreeType 2.12.x) and RPi/ARM64 devices hit this. The NoDependencies variant avoids it entirely. The bug is in the mismatch between what FreeType version fontconfig expects vs. what is available on common distro versions.

### Key Signals

- "symbol lookup error: /home/gamma/Downloads/WattToolkit/native/linux-riscv64/libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property" — **issue body** (libSkiaSharp.so loaded successfully but a symbol it (or its transitive dependency fontconfig) requires is missing from the system libfreetype.)
- "This issue also happens on linux-arm64" — **comment by jeromelaban** (Not riscv64-specific — affects any Linux arch where the system freetype lacks FT_Get_BDF_Property (BDF support not compiled in, or version too old).)
- "it's not specific to riscv64. you will find the same issue with x86_64 etc. with same (new) version of fontconfig" — **comment by kasperk81** (The root issue is the fontconfig version on the system, not the architecture. Any system with a fontconfig that references FT_Get_BDF_Property from a FreeType version that doesn't have it will fail.)
- "export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 / dotnet run" — **comment by YuriiYa** (Workaround confirmed: forcing the loader to use the correctly built system FreeType (one with BDF support) resolves the error.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `scripts/Docker/debian/10/Dockerfile` | 59-61 | direct | riscv64 is built against fontconfig 2.15.0-2.3 (from Debian trixie), while arm, arm64, x86, x64 use fontconfig 2.13.1-2 (from Debian buster archive). Both versions use FT_Get_BDF_Property from libfreetype at runtime. |
| `native/linux/build.cake` | 108 | direct | skia_use_system_freetype2=false — Skia compiles its own freetype sources, but fontconfig is NOT listed as a system-override, so the SkiaSharp .so dynamically links against the system libfontconfig at runtime, which in turn expects a compatible libfreetype from the OS. |
| `documentation/dev/packages.md` | 183-195 | related | The Troubleshooting section already documents this error pattern: 'A .so was found and loaded, but it has a dependency that isn't installed'. The fix documented is to switch to SkiaSharp.NativeAssets.Linux.NoDependencies. However, no mention of the FT_Get_BDF_Property symptom or specific FreeType version requirements. |
| `scripts/Docker/debian/10/Dockerfile` | 96-99 | related | Bionic variant uses skia_use_fontconfig=false to disable fontconfig entirely — this is the same approach that NoDependencies uses to avoid the fontconfig/freetype runtime dependency chain. |

### Workarounds

- Use SkiaSharp.NativeAssets.Linux.NoDependencies instead of SkiaSharp.NativeAssets.Linux — this variant has no fontconfig/freetype runtime deps
- Install a FreeType with BDF support and force-load it: sudo apt-get install -y libfreetype6 libfontconfig1 && export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6
- For systemd services, add Environment=LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 to the [Service] section

### Next Questions

- What exact FreeType version and compile flags are present on the failing systems? (ldd /usr/lib/aarch64-linux-gnu/libfreetype.so.6)
- Would switching to static fontconfig linkage or providing a statically-bundled freetype (similar to how harfbuzz is handled) resolve this for the standard Linux package?
- Is the FT_Get_BDF_Property requirement coming from fontconfig itself or another Skia dependency?
- Should the docs update to list the required libfreetype version per architecture?

### Resolution Proposals

**Hypothesis:** fontconfig (dynamically linked into libSkiaSharp) uses FT_Get_BDF_Property from libfreetype at runtime. When the system FreeType lacks BDF support or is too old, the symbol is missing.

1. **Switch to NoDependencies package** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Use SkiaSharp.NativeAssets.Linux.NoDependencies instead of SkiaSharp.NativeAssets.Linux. This build has no fontconfig/freetype runtime deps and avoids the issue entirely. Fonts must be loaded explicitly (no system font enumeration).
2. **LD_PRELOAD compatible libfreetype** — workaround, confidence 0.85 (85%), cost/xs, validated=yes
   - Install libfreetype6 and libfontconfig1, then set LD_PRELOAD to point to the installed libfreetype.so.6. This forces the dynamic linker to use a FreeType with BDF support.

```csharp
sudo apt-get install -y libfreetype6 libfontconfig1
export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6
dotnet run
```
3. **Investigate static fontconfig linkage in native build** — fix, confidence 0.65 (65%), cost/l, validated=untested
   - Explore whether libfontconfig can be statically linked into libSkiaSharp (like libstdc++ is today) to avoid the runtime FreeType dependency chain, or whether the fontconfig version used in the Dockerfile should be aligned with the system's FreeType version.

**Recommended proposal:** Switch to NoDependencies package

**Why:** Zero-risk workaround available immediately. NoDependencies is already documented as the recommended package for containers and minimal environments. Users who need system font enumeration should use the LD_PRELOAD workaround until the native build is fixed.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.85 (85%) |
| Reason | Clear bug with real-world impact on ARM64/RISC-V Linux devices. Root cause (fontconfig/freetype version mismatch at runtime) is understood; the fix path (static fontconfig linkage or aligned freetype version) needs engineering investigation. Workarounds exist and should be communicated. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Post workarounds and root cause analysis | — |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report, and to everyone who added context in the comments.

**Root cause:** `libSkiaSharp.so` (standard Linux package) is dynamically linked against the system `libfontconfig`, which in turn requires `FT_Get_BDF_Property` from `libfreetype`. On Debian Bookworm, Raspberry Pi OS, and similar distros, the installed FreeType may be compiled without BDF support, causing the symbol lookup failure at runtime.

**Workaround 1 (recommended for containers / minimal environments):** Switch to the `NoDependencies` package, which has no fontconfig or FreeType runtime dependencies:
```xml
<!-- Remove this -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="3.119.0" />
<!-- Add this -->
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="3.119.0" />
```
Note: with `NoDependencies`, you must load fonts explicitly (system font enumeration via `SKFontManager` won't return system fonts).

**Workaround 2 (for standard Linux, keep fontconfig):** Install the required FreeType and force-load it:
```bash
sudo apt-get install -y libfreetype6 libfontconfig1
export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6
dotnet run
```
For systemd services, add to the `[Service]` section:
```
Environment=LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6
```

We're investigating a longer-term fix in the native build to eliminate the runtime FreeType version sensitivity.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3229,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-24T14:56:22Z",
    "currentLabels": [
      "type/bug",
      "os/Linux",
      "area/libSkiaSharp.native",
      "tenet/reliability",
      "triage/triaged"
    ]
  },
  "summary": "libSkiaSharp.so on Linux arm64 and riscv64 fails to load with 'undefined symbol: FT_Get_BDF_Property' because the bundled fontconfig was built against a newer FreeType version that exports this BDF symbol, but the system's installed FreeType lacks it (e.g., Debian Bookworm FreeType 2.12.x compiled without BDF support).",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.95
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
      "errorType": "crash",
      "errorMessage": "symbol lookup error: libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property",
      "reproQuality": "complete",
      "targetFrameworks": [
        "net9.0"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Run a .NET 9.0 application referencing SkiaSharp 3.119.0-preview on Linux RISC-V 64 or ARM64",
        "The process immediately exits with 'symbol lookup error: libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property'"
      ],
      "environmentDetails": "RISC-V 64 (AOSC OS 12.1 / QEMU), also confirmed on ARM64 (Raspberry Pi Debian Bookworm, Pi 5), SkiaSharp 3.119.0-preview.1.2 / 3.119.0, .NET 8 and 9",
      "relatedIssues": [
        3272,
        3436
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/pull/3221",
          "description": "PR referenced in issue thread (possibly related fix)"
        },
        {
          "url": "https://github.com/unoplatform/uno/issues/20429",
          "description": "Same error on RPi CM4 via Uno Platform"
        },
        {
          "url": "https://github.com/linuxserver/docker-jellyfin/issues/298",
          "description": "Same error reported in Jellyfin on Pi5 arm64"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.119.0-preview.1.2",
        "3.119.0",
        "3.116.0"
      ],
      "workedIn": "3.116.0",
      "brokeIn": "3.119.0-preview.1.2",
      "currentRelevance": "likely",
      "relevanceReason": "Issue is still open and multiple users confirm it on SkiaSharp 3.119.0 final; the Dockerfile build matrix shows the fontconfig version divergence remains."
    }
  },
  "analysis": {
    "summary": "libSkiaSharp.so for Linux (arm64 / riscv64) is dynamically linked against the system libfontconfig. The build Docker image uses fontconfig 2.15.0-2.3 for riscv64 and 2.13.1-2 for other arches. Both fontconfig versions reference FT_Get_BDF_Property from libfreetype. On end-user systems running Debian Bookworm (FreeType 2.12.x) or any distro where FreeType was compiled without BDF support (TT_CONFIG_OPTION_BDF), the symbol is absent at runtime and the binary fails to load.",
    "codeInvestigation": [
      {
        "file": "scripts/Docker/debian/10/Dockerfile",
        "lines": "59-61",
        "finding": "riscv64 is built against fontconfig 2.15.0-2.3 (from Debian trixie), while arm, arm64, x86, x64 use fontconfig 2.13.1-2 (from Debian buster archive). Both versions use FT_Get_BDF_Property from libfreetype at runtime.",
        "relevance": "direct"
      },
      {
        "file": "native/linux/build.cake",
        "lines": "108",
        "finding": "skia_use_system_freetype2=false — Skia compiles its own freetype sources, but fontconfig is NOT listed as a system-override, so the SkiaSharp .so dynamically links against the system libfontconfig at runtime, which in turn expects a compatible libfreetype from the OS.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "183-195",
        "finding": "The Troubleshooting section already documents this error pattern: 'A .so was found and loaded, but it has a dependency that isn't installed'. The fix documented is to switch to SkiaSharp.NativeAssets.Linux.NoDependencies. However, no mention of the FT_Get_BDF_Property symptom or specific FreeType version requirements.",
        "relevance": "related"
      },
      {
        "file": "scripts/Docker/debian/10/Dockerfile",
        "lines": "96-99",
        "finding": "Bionic variant uses skia_use_fontconfig=false to disable fontconfig entirely — this is the same approach that NoDependencies uses to avoid the fontconfig/freetype runtime dependency chain.",
        "relevance": "related"
      }
    ],
    "keySignals": [
      {
        "text": "symbol lookup error: /home/gamma/Downloads/WattToolkit/native/linux-riscv64/libSkiaSharp.so: undefined symbol: FT_Get_BDF_Property",
        "source": "issue body",
        "interpretation": "libSkiaSharp.so loaded successfully but a symbol it (or its transitive dependency fontconfig) requires is missing from the system libfreetype."
      },
      {
        "text": "This issue also happens on linux-arm64",
        "source": "comment by jeromelaban",
        "interpretation": "Not riscv64-specific — affects any Linux arch where the system freetype lacks FT_Get_BDF_Property (BDF support not compiled in, or version too old)."
      },
      {
        "text": "it's not specific to riscv64. you will find the same issue with x86_64 etc. with same (new) version of fontconfig",
        "source": "comment by kasperk81",
        "interpretation": "The root issue is the fontconfig version on the system, not the architecture. Any system with a fontconfig that references FT_Get_BDF_Property from a FreeType version that doesn't have it will fail."
      },
      {
        "text": "export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 / dotnet run",
        "source": "comment by YuriiYa",
        "interpretation": "Workaround confirmed: forcing the loader to use the correctly built system FreeType (one with BDF support) resolves the error."
      }
    ],
    "rationale": "This is a genuine native packaging bug. The NativeAssets.Linux package dynamically links fontconfig, which has a transitive runtime dependency on a specific FreeType version with BDF support. Users on Debian Bookworm (FreeType 2.12.x) and RPi/ARM64 devices hit this. The NoDependencies variant avoids it entirely. The bug is in the mismatch between what FreeType version fontconfig expects vs. what is available on common distro versions.",
    "workarounds": [
      "Use SkiaSharp.NativeAssets.Linux.NoDependencies instead of SkiaSharp.NativeAssets.Linux — this variant has no fontconfig/freetype runtime deps",
      "Install a FreeType with BDF support and force-load it: sudo apt-get install -y libfreetype6 libfontconfig1 && export LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6",
      "For systemd services, add Environment=LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6 to the [Service] section"
    ],
    "nextQuestions": [
      "What exact FreeType version and compile flags are present on the failing systems? (ldd /usr/lib/aarch64-linux-gnu/libfreetype.so.6)",
      "Would switching to static fontconfig linkage or providing a statically-bundled freetype (similar to how harfbuzz is handled) resolve this for the standard Linux package?",
      "Is the FT_Get_BDF_Property requirement coming from fontconfig itself or another Skia dependency?",
      "Should the docs update to list the required libfreetype version per architecture?"
    ],
    "resolution": {
      "hypothesis": "fontconfig (dynamically linked into libSkiaSharp) uses FT_Get_BDF_Property from libfreetype at runtime. When the system FreeType lacks BDF support or is too old, the symbol is missing.",
      "proposals": [
        {
          "title": "Switch to NoDependencies package",
          "description": "Use SkiaSharp.NativeAssets.Linux.NoDependencies instead of SkiaSharp.NativeAssets.Linux. This build has no fontconfig/freetype runtime deps and avoids the issue entirely. Fonts must be loaded explicitly (no system font enumeration).",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "LD_PRELOAD compatible libfreetype",
          "description": "Install libfreetype6 and libfontconfig1, then set LD_PRELOAD to point to the installed libfreetype.so.6. This forces the dynamic linker to use a FreeType with BDF support.",
          "codeSnippet": "sudo apt-get install -y libfreetype6 libfontconfig1\nexport LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6\ndotnet run",
          "category": "workaround",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Investigate static fontconfig linkage in native build",
          "description": "Explore whether libfontconfig can be statically linked into libSkiaSharp (like libstdc++ is today) to avoid the runtime FreeType dependency chain, or whether the fontconfig version used in the Dockerfile should be aligned with the system's FreeType version.",
          "category": "fix",
          "confidence": 0.65,
          "effort": "cost/l",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Switch to NoDependencies package",
      "recommendedReason": "Zero-risk workaround available immediately. NoDependencies is already documented as the recommended package for containers and minimal environments. Users who need system font enumeration should use the LD_PRELOAD workaround until the native build is fixed."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.85,
      "reason": "Clear bug with real-world impact on ARM64/RISC-V Linux devices. Root cause (fontconfig/freetype version mismatch at runtime) is understood; the fix path (static fontconfig linkage or aligned freetype version) needs engineering investigation. Workarounds exist and should be communicated.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability labels",
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
        "description": "Post workarounds and root cause analysis",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report, and to everyone who added context in the comments.\n\n**Root cause:** `libSkiaSharp.so` (standard Linux package) is dynamically linked against the system `libfontconfig`, which in turn requires `FT_Get_BDF_Property` from `libfreetype`. On Debian Bookworm, Raspberry Pi OS, and similar distros, the installed FreeType may be compiled without BDF support, causing the symbol lookup failure at runtime.\n\n**Workaround 1 (recommended for containers / minimal environments):** Switch to the `NoDependencies` package, which has no fontconfig or FreeType runtime dependencies:\n```xml\n<!-- Remove this -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux\" Version=\"3.119.0\" />\n<!-- Add this -->\n<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"3.119.0\" />\n```\nNote: with `NoDependencies`, you must load fonts explicitly (system font enumeration via `SKFontManager` won't return system fonts).\n\n**Workaround 2 (for standard Linux, keep fontconfig):** Install the required FreeType and force-load it:\n```bash\nsudo apt-get install -y libfreetype6 libfontconfig1\nexport LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6\ndotnet run\n```\nFor systemd services, add to the `[Service]` section:\n```\nEnvironment=LD_PRELOAD=/usr/lib/aarch64-linux-gnu/libfreetype.so.6\n```\n\nWe're investigating a longer-term fix in the native build to eliminate the runtime FreeType version sensitivity."
      }
    ]
  }
}
```

</details>
