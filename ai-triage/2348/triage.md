# Issue Triage Report — #2348

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-23T23:31:53Z |
| Type | type/question (0.92 (92%)) |
| Area | area/Build (0.90 (90%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** Developer asks how to run SkiaSharpGenerator to produce new bindings; gets 'Unable to load DLL libclang' on Windows, which is caused by missing native libclang dependencies that come with the Visual Studio C++ workload.

**Analysis:** The SkiaSharpGenerator tool depends on CppAst (v0.21.4), which uses native libclang binaries bundled via NuGet. On Windows, these native DLLs require the MSVC C++ runtime, provided by Visual Studio's C++ workload. The reporter also used --skia which is not a valid flag (the correct flag is --root), though the libclang error would occur before argument validation. The wiki page for Creating-Bindings does not document this prerequisite.

**Recommendations:** **close-as-not-a-bug** — Developer tooling setup question with a known answer (install VS C++ workload). The generator itself works correctly; the issue is missing native runtime on Windows. Maintainer already commented the fix.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Windows-Classic |
| Backends | — |
| Tenets | — |
| Partner | — |

## Evidence

### Reproduction

1. Run `dotnet cake --target=externals-download`
2. Run `dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.Skottie.json --skia externals/skia --output binding/SkiaSharp.Skottie/SkottieApi.generated.cs`
3. Observe error: Unable to load DLL 'libclang' or one of its dependencies (0x8007007E)

**Environment:** Windows, SkiaSharp dev setup, dotnet run on SkiaSharpGenerator

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2348#issuecomment-1370342636 — Maintainer comment: installing VS C++ workload resolved the issue
- https://github.com/mono/SkiaSharp/issues/2348#issuecomment-3239663823 — 2025 community comment with detailed Linux workaround instructions

## Analysis

### Technical Summary

The SkiaSharpGenerator tool depends on CppAst (v0.21.4), which uses native libclang binaries bundled via NuGet. On Windows, these native DLLs require the MSVC C++ runtime, provided by Visual Studio's C++ workload. The reporter also used --skia which is not a valid flag (the correct flag is --root), though the libclang error would occur before argument validation. The wiki page for Creating-Bindings does not document this prerequisite.

### Rationale

The title prefix [QUESTION] and issue body clearly ask how to set up the binding generator. The error is a missing native dependency (libclang) not a SkiaSharp code bug. The maintainer already provided the correct fix in a comment (install VS C++ workload). This is a developer tooling setup question with a known answer that should be documented.

### Key Signals

- "Unable to load DLL 'libclang' or one of its dependencies: The specified module could not be found. (0x8007007E)" — **issue body** (0x8007007E is Windows ERROR_MOD_NOT_FOUND — a native DLL dependency of libclang is missing. Typically the MSVC C++ runtime from Visual Studio C++ workload.)
- "Not sure what it really wants but I think I found that I just had to install the C++ workload in VS and then it started working" — **maintainer comment #issuecomment-1370342636** (Confirmed root cause: missing MSVC native runtime. Workaround is to install Visual Studio C++ workload.)
- "The wiki entry https://github.com/mono/SkiaSharp/wiki/Creating-Bindings makes no mention of how to actually generate those bindings." — **reporter follow-up #issuecomment-1370043967** (Documentation gap: the wiki does not explain the setup prerequisites for running the generator.)
- "I was able to get the generator process to run on linux, but it just spits out a almost empty file" — **community comment #issuecomment-3239663823** (Linux also has difficulties — NuGet ClangSharp.Interop artifacts not publishing for Linux. Separate issue from the Windows libclang error.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj` | 16 | direct | Generator depends on CppAst 0.21.4 NuGet package, which bundles native libclang binaries. On Windows, the MSVC C++ runtime is required for these binaries, supplied by the Visual Studio C++ workload. |
| `utils/SkiaSharpGenerator/Generate/GenerateCommand.cs` | 21-24 | related | The valid flag is --root (or -r), not --skia as used by the reporter. However the libclang error occurs during assembly load before argument parsing. |
| `utils/generate.ps1` | 1-54 | context | The generate.ps1 script shows correct usage with --root parameter and covers all binding configs (libSkiaSharp.json, Skottie, SceneGraph, Resources, HarfBuzz). No documentation of setup prerequisites. |

### Workarounds

- On Windows: Install Visual Studio C++ workload (provides native MSVC runtime that libclang.dll depends on)
- Use the correct --root flag instead of --skia when running the generator
- Use utils/generate.ps1 script instead of running the generator manually — it uses the correct parameters

### Resolution Proposals

**Hypothesis:** On Windows, CppAst's bundled libclang.dll requires the Visual C++ Runtime which is not installed by default without Visual Studio's C++ workload. Adding documentation to the wiki and/or generate.ps1 about this prerequisite would prevent this question.

1. **Install Visual Studio C++ workload** — workaround, confidence 0.92 (92%), cost/xs, validated=untested
   - Install the 'Desktop development with C++' workload in Visual Studio Installer. This provides the MSVC C++ runtime that libclang.dll depends on. After installing, running the generator should work.
2. **Use generate.ps1 for correct invocation** — alternative, confidence 0.90 (90%), cost/xs, validated=untested
   - Instead of running dotnet run manually, use `pwsh ./utils/generate.ps1` which uses the correct --root parameter and covers all binding configs. Reporter used --skia which is not a valid parameter.
3. **Document libclang prerequisites in wiki** — fix, confidence 0.88 (88%), cost/s, validated=untested
   - Update https://github.com/mono/SkiaSharp/wiki/Creating-Bindings to document: (1) install Visual Studio C++ workload on Windows, (2) correct generate.ps1 invocation, (3) Linux LLVM/clang system package requirements.

**Recommended proposal:** Install Visual Studio C++ workload

**Why:** Immediate fix the reporter can apply now. Already confirmed by the maintainer.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | Developer tooling setup question with a known answer (install VS C++ workload). The generator itself works correctly; the issue is missing native runtime on Windows. Maintainer already commented the fix. |
| Suggested repro platform | windows |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.92 (92%) | Apply question and build area labels | labels=type/question, area/Build, os/Windows-Classic |
| add-comment | medium | 0.88 (88%) | Post answer explaining how to set up libclang and use the generator correctly | — |
| close-issue | medium | 0.80 (80%) | Close as answered — developer tooling setup question with known fix | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The `SkiaSharpGenerator` tool uses [CppAst](https://github.com/xoofx/CppAst.NET) which requires native libclang binaries. On **Windows**, these depend on the MSVC C++ runtime:

**Fix:** Install the **Desktop development with C++** workload in [Visual Studio Installer](https://visualstudio.microsoft.com/downloads/). This provides the native runtime that libclang.dll requires.

Also note that the correct parameter is `--root` (not `--skia`), or you can use the provided script which handles the correct invocation:

```powershell
pwsh ./utils/generate.ps1
```

See `utils/generate.ps1` for all the available config targets (libSkiaSharp.json, Skottie, SceneGraph, Resources, HarfBuzz).

For **Linux**, you'll need system `clang`/`llvm` packages installed (e.g., `apt install clang` on Debian/Ubuntu). The NuGet-provided ClangSharp native binaries may not be available for all Linux distributions, in which case building from source as described in the later comments may be needed.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2348,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-23T23:31:53Z"
  },
  "summary": "Developer asks how to run SkiaSharpGenerator to produce new bindings; gets 'Unable to load DLL libclang' on Windows, which is caused by missing native libclang dependencies that come with the Visual Studio C++ workload.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.92
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.9
    },
    "platforms": [
      "os/Windows-Classic"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Run `dotnet cake --target=externals-download`",
        "Run `dotnet run --project=utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj -- generate --config binding/libSkiaSharp.Skottie.json --skia externals/skia --output binding/SkiaSharp.Skottie/SkottieApi.generated.cs`",
        "Observe error: Unable to load DLL 'libclang' or one of its dependencies (0x8007007E)"
      ],
      "environmentDetails": "Windows, SkiaSharp dev setup, dotnet run on SkiaSharpGenerator",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2348#issuecomment-1370342636",
          "description": "Maintainer comment: installing VS C++ workload resolved the issue"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2348#issuecomment-3239663823",
          "description": "2025 community comment with detailed Linux workaround instructions"
        }
      ]
    }
  },
  "analysis": {
    "summary": "The SkiaSharpGenerator tool depends on CppAst (v0.21.4), which uses native libclang binaries bundled via NuGet. On Windows, these native DLLs require the MSVC C++ runtime, provided by Visual Studio's C++ workload. The reporter also used --skia which is not a valid flag (the correct flag is --root), though the libclang error would occur before argument validation. The wiki page for Creating-Bindings does not document this prerequisite.",
    "rationale": "The title prefix [QUESTION] and issue body clearly ask how to set up the binding generator. The error is a missing native dependency (libclang) not a SkiaSharp code bug. The maintainer already provided the correct fix in a comment (install VS C++ workload). This is a developer tooling setup question with a known answer that should be documented.",
    "codeInvestigation": [
      {
        "file": "utils/SkiaSharpGenerator/SkiaSharpGenerator.csproj",
        "lines": "16",
        "finding": "Generator depends on CppAst 0.21.4 NuGet package, which bundles native libclang binaries. On Windows, the MSVC C++ runtime is required for these binaries, supplied by the Visual Studio C++ workload.",
        "relevance": "direct"
      },
      {
        "file": "utils/SkiaSharpGenerator/Generate/GenerateCommand.cs",
        "lines": "21-24",
        "finding": "The valid flag is --root (or -r), not --skia as used by the reporter. However the libclang error occurs during assembly load before argument parsing.",
        "relevance": "related"
      },
      {
        "file": "utils/generate.ps1",
        "lines": "1-54",
        "finding": "The generate.ps1 script shows correct usage with --root parameter and covers all binding configs (libSkiaSharp.json, Skottie, SceneGraph, Resources, HarfBuzz). No documentation of setup prerequisites.",
        "relevance": "context"
      }
    ],
    "keySignals": [
      {
        "text": "Unable to load DLL 'libclang' or one of its dependencies: The specified module could not be found. (0x8007007E)",
        "source": "issue body",
        "interpretation": "0x8007007E is Windows ERROR_MOD_NOT_FOUND — a native DLL dependency of libclang is missing. Typically the MSVC C++ runtime from Visual Studio C++ workload."
      },
      {
        "text": "Not sure what it really wants but I think I found that I just had to install the C++ workload in VS and then it started working",
        "source": "maintainer comment #issuecomment-1370342636",
        "interpretation": "Confirmed root cause: missing MSVC native runtime. Workaround is to install Visual Studio C++ workload."
      },
      {
        "text": "The wiki entry https://github.com/mono/SkiaSharp/wiki/Creating-Bindings makes no mention of how to actually generate those bindings.",
        "source": "reporter follow-up #issuecomment-1370043967",
        "interpretation": "Documentation gap: the wiki does not explain the setup prerequisites for running the generator."
      },
      {
        "text": "I was able to get the generator process to run on linux, but it just spits out a almost empty file",
        "source": "community comment #issuecomment-3239663823",
        "interpretation": "Linux also has difficulties — NuGet ClangSharp.Interop artifacts not publishing for Linux. Separate issue from the Windows libclang error."
      }
    ],
    "workarounds": [
      "On Windows: Install Visual Studio C++ workload (provides native MSVC runtime that libclang.dll depends on)",
      "Use the correct --root flag instead of --skia when running the generator",
      "Use utils/generate.ps1 script instead of running the generator manually — it uses the correct parameters"
    ],
    "resolution": {
      "hypothesis": "On Windows, CppAst's bundled libclang.dll requires the Visual C++ Runtime which is not installed by default without Visual Studio's C++ workload. Adding documentation to the wiki and/or generate.ps1 about this prerequisite would prevent this question.",
      "proposals": [
        {
          "title": "Install Visual Studio C++ workload",
          "description": "Install the 'Desktop development with C++' workload in Visual Studio Installer. This provides the MSVC C++ runtime that libclang.dll depends on. After installing, running the generator should work.",
          "category": "workaround",
          "confidence": 0.92,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Use generate.ps1 for correct invocation",
          "description": "Instead of running dotnet run manually, use `pwsh ./utils/generate.ps1` which uses the correct --root parameter and covers all binding configs. Reporter used --skia which is not a valid parameter.",
          "category": "alternative",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Document libclang prerequisites in wiki",
          "description": "Update https://github.com/mono/SkiaSharp/wiki/Creating-Bindings to document: (1) install Visual Studio C++ workload on Windows, (2) correct generate.ps1 invocation, (3) Linux LLVM/clang system package requirements.",
          "category": "fix",
          "confidence": 0.88,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Install Visual Studio C++ workload",
      "recommendedReason": "Immediate fix the reporter can apply now. Already confirmed by the maintainer."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "Developer tooling setup question with a known answer (install VS C++ workload). The generator itself works correctly; the issue is missing native runtime on Windows. Maintainer already commented the fix.",
      "suggestedReproPlatform": "windows"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and build area labels",
        "risk": "low",
        "confidence": 0.92,
        "labels": [
          "type/question",
          "area/Build",
          "os/Windows-Classic"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining how to set up libclang and use the generator correctly",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the report! The `SkiaSharpGenerator` tool uses [CppAst](https://github.com/xoofx/CppAst.NET) which requires native libclang binaries. On **Windows**, these depend on the MSVC C++ runtime:\n\n**Fix:** Install the **Desktop development with C++** workload in [Visual Studio Installer](https://visualstudio.microsoft.com/downloads/). This provides the native runtime that libclang.dll requires.\n\nAlso note that the correct parameter is `--root` (not `--skia`), or you can use the provided script which handles the correct invocation:\n\n```powershell\npwsh ./utils/generate.ps1\n```\n\nSee `utils/generate.ps1` for all the available config targets (libSkiaSharp.json, Skottie, SceneGraph, Resources, HarfBuzz).\n\nFor **Linux**, you'll need system `clang`/`llvm` packages installed (e.g., `apt install clang` on Debian/Ubuntu). The NuGet-provided ClangSharp native binaries may not be available for all Linux distributions, in which case building from source as described in the later comments may be needed."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — developer tooling setup question with known fix",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
