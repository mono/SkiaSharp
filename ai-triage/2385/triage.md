# Issue Triage Report — #2385

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-22T18:37:00Z |
| Type | type/question (0.88 (88%)) |
| Area | area/libSkiaSharp.native (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User asks why the .NET runtime searches for 'liblibSkiaSharp.so' on Linux when the library is named 'libSkiaSharp.so'; multiple commenters also report DllNotFoundException when deploying to NixOS, WSL, and Docker containers.

**Analysis:** The 'liblibSkiaSharp.so' probing is standard .NET runtime behavior: when DllImport specifies 'libSkiaSharp', the Linux runtime probes multiple variants including adding the standard lib prefix (liblibSkiaSharp.so). This is not a SkiaSharp bug. The actual deployment problem for many commenters is a missing NativeAssets package or missing fontconfig dependency.

**Recommendations:** **close-as-not-a-bug** — The 'liblibSkiaSharp' probing is by-design .NET runtime behavior. The maintainer already explained this in 2023. The packages.md documentation explicitly covers this pattern. Closing with a pointer to docs and a code snippet is appropriate.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Deploy an Avalonia or other .NET app that uses SkiaSharp to NixOS (or Docker/WSL)
2. Run with LD_DEBUG=libs or observe DllNotFoundException output
3. Observe the runtime probing for 'liblibSkiaSharp.so' in addition to 'libSkiaSharp.so'

**Environment:** NixOS with manually placed libSkiaSharp.so; also reproduced on WSL running Uno and Docker containers

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2385#issuecomment-1518986437 — Maintainer explanation of runtime probe variants

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.x (implied) |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | The DllImport name 'libSkiaSharp' is unchanged and the .NET runtime probing behavior is a stable runtime feature, so the question remains relevant. |

## Analysis

### Technical Summary

The 'liblibSkiaSharp.so' probing is standard .NET runtime behavior: when DllImport specifies 'libSkiaSharp', the Linux runtime probes multiple variants including adding the standard lib prefix (liblibSkiaSharp.so). This is not a SkiaSharp bug. The actual deployment problem for many commenters is a missing NativeAssets package or missing fontconfig dependency.

### Rationale

The original post is a 'why does this happen?' question, not a bug report. The maintainer already answered: the .NET runtime automatically appends 'lib' prefix on Unix for native library names. The behavior is correct by design and is also documented in documentation/dev/packages.md under 'Reading .NET Native Loading Errors'. Several commenters experience actual DllNotFoundException which is a separate deployment issue (missing NativeAssets package or missing fontconfig), also addressed in the same docs.

### Key Signals

- "find library=liblibSkiaSharp.so [0]; searching" — **issue body (LD_DEBUG output)** (.NET runtime probing variant — it prepends 'lib' to the already-lib-prefixed name 'libSkiaSharp'. This is expected runtime behavior.)
- "I think the lookup versions are based on the fact that unix uses a lib prefix and windows does not." — **comment by mattleibow (maintainer)** (Authoritative explanation: by-design .NET runtime probing. No SkiaSharp fix needed.)
- "/app/runtimes/linux-x64/native/liblibSkiaSharp.so: cannot open shared object file: No such file or directory" — **comment by avojacek** (Fallback probe path, not the actual error. The real issue was missing fontconfig (shown earlier in the same error log).)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaApi.cs` | 12 | direct | DllImport constant is 'libSkiaSharp' (without .so extension). The .NET runtime on Linux probes: libSkiaSharp.so, libSkiaSharp.so.1, liblibSkiaSharp.so, liblibSkiaSharp.so.1 — causing the 'liblibSkiaSharp' search seen in LD_DEBUG output. |
| `documentation/dev/packages.md` | 199-206 | direct | The packages.md 'Reading .NET Native Loading Errors' section explicitly documents the liblibSkiaSharp.so probing pattern, explains that the first error is the root cause, and lists workarounds. |

### Workarounds

- Add 'SkiaSharp.NativeAssets.Linux' or 'SkiaSharp.NativeAssets.Linux.NoDependencies' as a PackageReference in the executable project
- For containers or minimal environments without fontconfig, use 'SkiaSharp.NativeAssets.Linux.NoDependencies'
- If fontconfig is missing, install it (e.g. apt-get install libfontconfig1) or switch to NoDependencies variant
- For NixOS, place libSkiaSharp.so in a path on LD_LIBRARY_PATH; the liblibSkiaSharp probes will fail silently once the real library is found

### Next Questions

- Did the original reporter get the library working after placing it manually?
- Does the NixOS Nix flake for SkiaSharp correctly set the library name?

### Resolution Proposals

**Hypothesis:** The 'liblibSkiaSharp' probing is the .NET runtime's cross-platform fallback probing for DllImport names. It is not a bug. The actual failure for most reporters is a missing NativeAssets package, missing fontconfig, or the library placed in the wrong location.

1. **Add NativeAssets package to executable project** — workaround, confidence 0.92 (92%), cost/xs, validated=yes
   - Add the appropriate NativeAssets package as a PackageReference in the executable (not library) project. Use NoDependencies for containers and minimal environments.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.*" />
```
2. **Install missing fontconfig for full NativeAssets.Linux** — workaround, confidence 0.90 (90%), cost/xs, validated=yes
   - If using SkiaSharp.NativeAssets.Linux (not NoDependencies), install fontconfig on the host or container image.

```csharp
# Debian/Ubuntu Dockerfile
RUN apt-get update && apt-get install -y libfontconfig1
```

**Recommended proposal:** Add NativeAssets package to executable project

**Why:** Most reporters are missing the NativeAssets package entirely. The NoDependencies variant works in all Linux environments without extra system libraries.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | The 'liblibSkiaSharp' probing is by-design .NET runtime behavior. The maintainer already explained this in 2023. The packages.md documentation explicitly covers this pattern. Closing with a pointer to docs and a code snippet is appropriate. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question, native, and linux labels | labels=type/question, area/libSkiaSharp.native, os/Linux |
| add-comment | high | 0.85 (85%) | Post explanation of runtime probing behavior and direct fix | — |
| close-issue | medium | 0.82 (82%) | Close as not a bug — behavior is by design | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed LD_DEBUG output!

The `liblibSkiaSharp.so` searches are expected — they are the .NET runtime's standard probing behavior on Linux. When SkiaSharp declares `[DllImport("libSkiaSharp")]`, the runtime probes multiple filename variants on Linux (adding the standard `lib` prefix, trying `.so`, `.so.1`, etc.), which results in attempts for both `libSkiaSharp.so` and `liblibSkiaSharp.so`. The probing failures for `liblibSkiaSharp` can be safely ignored.

The **real issue** is usually one of:

1. **Missing NativeAssets package** — Add the correct package to your **executable** project (not a library project):
   ```xml
   <!-- For containers / NixOS / minimal environments: -->
   <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.*" />
   
   <!-- For standard Linux with system font support: -->
   <PackageReference Include="SkiaSharp.NativeAssets.Linux" Version="2.88.*" />
   ```

2. **Missing fontconfig** (if using `NativeAssets.Linux`) — Install it in your environment:
   ```bash
   # Debian/Ubuntu
   apt-get install -y libfontconfig1
   ```

For NixOS specifically, you'll need to either use the packaged `SkiaSharp.NativeAssets.Linux.NoDependencies` or provide the correct `libSkiaSharp.so` path via `LD_LIBRARY_PATH`.

See the [Linux deployment guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for the full decision matrix.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2385,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-22T18:37:00Z"
  },
  "summary": "User asks why the .NET runtime searches for 'liblibSkiaSharp.so' on Linux when the library is named 'libSkiaSharp.so'; multiple commenters also report DllNotFoundException when deploying to NixOS, WSL, and Docker containers.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.92
    },
    "platforms": [
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Deploy an Avalonia or other .NET app that uses SkiaSharp to NixOS (or Docker/WSL)",
        "Run with LD_DEBUG=libs or observe DllNotFoundException output",
        "Observe the runtime probing for 'liblibSkiaSharp.so' in addition to 'libSkiaSharp.so'"
      ],
      "environmentDetails": "NixOS with manually placed libSkiaSharp.so; also reproduced on WSL running Uno and Docker containers",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2385#issuecomment-1518986437",
          "description": "Maintainer explanation of runtime probe variants"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.x (implied)"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "The DllImport name 'libSkiaSharp' is unchanged and the .NET runtime probing behavior is a stable runtime feature, so the question remains relevant."
    }
  },
  "analysis": {
    "summary": "The 'liblibSkiaSharp.so' probing is standard .NET runtime behavior: when DllImport specifies 'libSkiaSharp', the Linux runtime probes multiple variants including adding the standard lib prefix (liblibSkiaSharp.so). This is not a SkiaSharp bug. The actual deployment problem for many commenters is a missing NativeAssets package or missing fontconfig dependency.",
    "rationale": "The original post is a 'why does this happen?' question, not a bug report. The maintainer already answered: the .NET runtime automatically appends 'lib' prefix on Unix for native library names. The behavior is correct by design and is also documented in documentation/dev/packages.md under 'Reading .NET Native Loading Errors'. Several commenters experience actual DllNotFoundException which is a separate deployment issue (missing NativeAssets package or missing fontconfig), also addressed in the same docs.",
    "keySignals": [
      {
        "text": "find library=liblibSkiaSharp.so [0]; searching",
        "source": "issue body (LD_DEBUG output)",
        "interpretation": ".NET runtime probing variant — it prepends 'lib' to the already-lib-prefixed name 'libSkiaSharp'. This is expected runtime behavior."
      },
      {
        "text": "I think the lookup versions are based on the fact that unix uses a lib prefix and windows does not.",
        "source": "comment by mattleibow (maintainer)",
        "interpretation": "Authoritative explanation: by-design .NET runtime probing. No SkiaSharp fix needed."
      },
      {
        "text": "/app/runtimes/linux-x64/native/liblibSkiaSharp.so: cannot open shared object file: No such file or directory",
        "source": "comment by avojacek",
        "interpretation": "Fallback probe path, not the actual error. The real issue was missing fontconfig (shown earlier in the same error log)."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaApi.cs",
        "lines": "12",
        "finding": "DllImport constant is 'libSkiaSharp' (without .so extension). The .NET runtime on Linux probes: libSkiaSharp.so, libSkiaSharp.so.1, liblibSkiaSharp.so, liblibSkiaSharp.so.1 — causing the 'liblibSkiaSharp' search seen in LD_DEBUG output.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "199-206",
        "finding": "The packages.md 'Reading .NET Native Loading Errors' section explicitly documents the liblibSkiaSharp.so probing pattern, explains that the first error is the root cause, and lists workarounds.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Add 'SkiaSharp.NativeAssets.Linux' or 'SkiaSharp.NativeAssets.Linux.NoDependencies' as a PackageReference in the executable project",
      "For containers or minimal environments without fontconfig, use 'SkiaSharp.NativeAssets.Linux.NoDependencies'",
      "If fontconfig is missing, install it (e.g. apt-get install libfontconfig1) or switch to NoDependencies variant",
      "For NixOS, place libSkiaSharp.so in a path on LD_LIBRARY_PATH; the liblibSkiaSharp probes will fail silently once the real library is found"
    ],
    "nextQuestions": [
      "Did the original reporter get the library working after placing it manually?",
      "Does the NixOS Nix flake for SkiaSharp correctly set the library name?"
    ],
    "resolution": {
      "hypothesis": "The 'liblibSkiaSharp' probing is the .NET runtime's cross-platform fallback probing for DllImport names. It is not a bug. The actual failure for most reporters is a missing NativeAssets package, missing fontconfig, or the library placed in the wrong location.",
      "proposals": [
        {
          "title": "Add NativeAssets package to executable project",
          "description": "Add the appropriate NativeAssets package as a PackageReference in the executable (not library) project. Use NoDependencies for containers and minimal environments.",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.*\" />",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "yes"
        },
        {
          "title": "Install missing fontconfig for full NativeAssets.Linux",
          "description": "If using SkiaSharp.NativeAssets.Linux (not NoDependencies), install fontconfig on the host or container image.",
          "codeSnippet": "# Debian/Ubuntu Dockerfile\nRUN apt-get update && apt-get install -y libfontconfig1",
          "category": "workaround",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Add NativeAssets package to executable project",
      "recommendedReason": "Most reporters are missing the NativeAssets package entirely. The NoDependencies variant works in all Linux environments without extra system libraries."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "The 'liblibSkiaSharp' probing is by-design .NET runtime behavior. The maintainer already explained this in 2023. The packages.md documentation explicitly covers this pattern. Closing with a pointer to docs and a code snippet is appropriate.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, native, and linux labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post explanation of runtime probing behavior and direct fix",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the detailed LD_DEBUG output!\n\nThe `liblibSkiaSharp.so` searches are expected — they are the .NET runtime's standard probing behavior on Linux. When SkiaSharp declares `[DllImport(\"libSkiaSharp\")]`, the runtime probes multiple filename variants on Linux (adding the standard `lib` prefix, trying `.so`, `.so.1`, etc.), which results in attempts for both `libSkiaSharp.so` and `liblibSkiaSharp.so`. The probing failures for `liblibSkiaSharp` can be safely ignored.\n\nThe **real issue** is usually one of:\n\n1. **Missing NativeAssets package** — Add the correct package to your **executable** project (not a library project):\n   ```xml\n   <!-- For containers / NixOS / minimal environments: -->\n   <PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.*\" />\n   \n   <!-- For standard Linux with system font support: -->\n   <PackageReference Include=\"SkiaSharp.NativeAssets.Linux\" Version=\"2.88.*\" />\n   ```\n\n2. **Missing fontconfig** (if using `NativeAssets.Linux`) — Install it in your environment:\n   ```bash\n   # Debian/Ubuntu\n   apt-get install -y libfontconfig1\n   ```\n\nFor NixOS specifically, you'll need to either use the packaged `SkiaSharp.NativeAssets.Linux.NoDependencies` or provide the correct `libSkiaSharp.so` path via `LD_LIBRARY_PATH`.\n\nSee the [Linux deployment guide](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/packages.md#linux-package-selection-guide) for the full decision matrix."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — behavior is by design",
        "risk": "medium",
        "confidence": 0.82,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
