# Issue Triage Report — #3278

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:35:58Z |
| Type | type/question (0.88 (88%)) |
| Area | area/Build (0.92 (92%)) |
| Suggested action | close-as-not-a-bug (0.85 (85%)) |

**Issue Summary:** User asking how to manually build libSkiaSharp.so for Android on Linux using gn/ninja, encountering a ninja build error due to missing zlib third-party dependency file (adler32_simd.c).

**Analysis:** The build failure is caused by two root issues: (1) the user did not run 'python3 tools/git-sync-deps' (or the Cake 'git-sync-deps' task), which downloads zlib and other third-party Skia dependencies before building; and (2) the Android native build in SkiaSharp's Cake script explicitly requires macOS or Windows as the build host and will not run on Linux. The correct approach is to use 'dotnet cake --target=externals-android --arch=arm64' on a macOS or Windows machine.

**Recommendations:** **close-as-not-a-bug** — This is a how-to question — the error is caused by missing git-sync-deps and building on an unsupported host (Linux) for Android targets. The correct build process is documented and uses the Cake build system.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | os/Android, os/Linux |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | type/bug |

## Evidence

### Reproduction

1. On Linux, clone SkiaSharp and checkout externals/skia at version 2.88.3
2. Run gn gen with Android target settings without first running python3 tools/git-sync-deps
3. Run ninja -C out/android/arm64 SkiaSharp
4. Observe: ninja error: '../../../third_party/externals/zlib/adler32_simd.c', needed by 'obj/third_party/externals/zlib/zlib_adler32_simd.adler32_simd.o', missing and no known rule to make it

**Environment:** Linux, Visual Studio Code, SkiaSharp 3.116.0 / Skia 2.88.3, Android NDK r21, target arm64

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 3.116.0, 2.88.9, 2.88.3 |
| Worked in | 2.88.9 |
| Broke in | 3.116.0 |
| Current relevance | likely |
| Relevance reason | The user is attempting a manual Skia 2.88.3 build against SkiaSharp 3.116.0 which expects a newer Skia milestone; and is missing the git-sync-deps step that downloads third-party dependencies including zlib. |

## Analysis

### Technical Summary

The build failure is caused by two root issues: (1) the user did not run 'python3 tools/git-sync-deps' (or the Cake 'git-sync-deps' task), which downloads zlib and other third-party Skia dependencies before building; and (2) the Android native build in SkiaSharp's Cake script explicitly requires macOS or Windows as the build host and will not run on Linux. The correct approach is to use 'dotnet cake --target=externals-android --arch=arm64' on a macOS or Windows machine.

### Rationale

The title 'how to build' and the content show this is a how-to question: the user is following an outdated wiki approach. Code investigation confirms the build.cake requires macOS/Windows for Android and depends on git-sync-deps. The missing file is a third-party dependency that is only present after running git-sync-deps.

### Key Signals

- "ninja: error: '../../../third_party/externals/zlib/adler32_simd.c'... missing and no known rule to make it" — **issue body** (zlib was not synced from DEPS — user skipped python3 tools/git-sync-deps step.)
- "I followed the Wiki here https://github.com/mono/SkiaSharp/wiki/Building-on-Linux, skia version is 2.88.3" — **issue body** (User is following an outdated wiki and using Skia 2.88.3 which is incompatible with SkiaSharp 3.116.0.)
- "Version of SkiaSharp: 3.116.0 (Current), Last Known Good Version: 2.88.9 (Previous)" — **issue body** (User previously built with 2.88.9 successfully but is now attempting 3.116.0 with a different build approach.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `native/android/build.cake` | 13-16 | direct | Task('libSkiaSharp') has .IsDependentOn('git-sync-deps') and .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows()) — Android builds require macOS or Windows and must run git-sync-deps first to fetch zlib and other DEPS-managed third-party sources. |
| `scripts/cake/native-shared.cake` | 22-43 | direct | Task('git-sync-deps') runs python3 tools/git-sync-deps in externals/skia, which downloads third-party externals including zlib to third_party/externals/. Without this step, adler32_simd.c and other files are absent. |
| `documentation/dev/building-linux.md` | 40-46 | related | Manual build instructions show 'git submodule update --init --recursive' and 'python3 tools/git-sync-deps' as required steps before gn gen. The user skipped these. This guide is for Linux (host) targets, not Android. |
| `documentation/dev/building.md` | 151-153 | context | Documented Android cake target: 'dotnet cake --target=externals-android --arch=arm64' — must be run on macOS or Windows per build.cake WithCriteria. |

### Workarounds

- On macOS or Windows: use 'dotnet cake --target=externals-android --arch=arm64' — this handles git-sync-deps, gn, and ninja automatically.
- If manual gn/ninja build is required: first run 'git submodule update --init --recursive' then 'cd externals/skia && python3 tools/git-sync-deps' before running gn gen.
- Use the Skia version from the SkiaSharp submodule (externals/skia), not a separately checked-out 2.88.3 — SkiaSharp 3.116.0 requires a specific newer Skia milestone.

### Next Questions

- Is the user building on Linux to cross-compile for Android, or building on a machine where macOS/Windows is available?
- What is the user's goal — distributing a custom build, or just using SkiaSharp for Android in an app?

### Resolution Proposals

**Hypothesis:** The error is caused by the user skipping the git-sync-deps step (which fetches zlib and other DEPS-managed sources) and attempting to build Android on Linux (which is not supported by the Cake Android build target). The user should use the Cake build system on macOS or Windows.

1. **Use the Cake build system on macOS or Windows** — fix, confidence 0.95 (95%), cost/xs, validated=untested
   - Run 'dotnet cake --target=externals-android --arch=arm64' on macOS or Windows. This automates git-sync-deps, the correct gn args, and ninja invocation. Ensure ANDROID_NDK_ROOT or ANDROID_NDK_HOME is set.
2. **Run git-sync-deps before manual build** — workaround, confidence 0.80 (80%), cost/s, validated=untested
   - If manual control is needed: git submodule update --init --recursive, then cd externals/skia && python3 tools/git-sync-deps, then proceed with gn gen using the SkiaSharp submodule's Skia (not a separate 2.88.3 checkout). Note that Android builds are only supported on macOS/Windows hosts in the official build system.

**Recommended proposal:** Use the Cake build system on macOS or Windows

**Why:** Simplest and most reliable — handles all dependencies automatically and uses the correct Skia version pinned to SkiaSharp 3.116.0.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.85 (85%) |
| Reason | This is a how-to question — the error is caused by missing git-sync-deps and building on an unsupported host (Linux) for Android targets. The correct build process is documented and uses the Cake build system. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.88 (88%) | Correct the label from type/bug to type/question, add area/Build and platform labels | labels=type/question, area/Build, os/Android, os/Linux |
| add-comment | high | 0.85 (85%) | Post answer explaining the correct build process and why the error occurs | — |
| close-issue | medium | 0.80 (80%) | Close as answered — this is a usage/how-to question with a clear documented answer | stateReason=completed |

**Comment draft for `add-comment`:**

```markdown
Thanks for the report! The build error is caused by two issues:

1. **Missing third-party dependency sync**: Before running `gn` and `ninja`, you must first sync Skia's third-party dependencies (including zlib). The `adler32_simd.c` file lives in `third_party/externals/zlib` which is only populated after running:
   ```bash
   git submodule update --init --recursive
   cd externals/skia
   python3 tools/git-sync-deps
   ```

2. **Android builds require macOS or Windows**: SkiaSharp's Android native build target (`externals-android`) only runs on macOS or Windows — Linux is not a supported host for Android cross-compilation in this build system.

**Recommended approach** — on macOS or Windows, use the Cake build script:
```bash
# Set Android NDK path first
export ANDROID_NDK_HOME=/path/to/android-ndk

# Build Android ARM64 native library
dotnet cake --target=externals-android --arch=arm64
```

Also note that SkiaSharp 3.116.0 expects a specific Skia milestone — use the `externals/skia` submodule from the SkiaSharp repository rather than a separately downloaded Skia 2.88.3.

See [Building SkiaSharp](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/building.md) for full instructions.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3278,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:35:58Z",
    "currentLabels": [
      "type/bug"
    ]
  },
  "summary": "User asking how to manually build libSkiaSharp.so for Android on Linux using gn/ninja, encountering a ninja build error due to missing zlib third-party dependency file (adler32_simd.c).",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.88
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.92
    },
    "platforms": [
      "os/Android",
      "os/Linux"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "On Linux, clone SkiaSharp and checkout externals/skia at version 2.88.3",
        "Run gn gen with Android target settings without first running python3 tools/git-sync-deps",
        "Run ninja -C out/android/arm64 SkiaSharp",
        "Observe: ninja error: '../../../third_party/externals/zlib/adler32_simd.c', needed by 'obj/third_party/externals/zlib/zlib_adler32_simd.adler32_simd.o', missing and no known rule to make it"
      ],
      "environmentDetails": "Linux, Visual Studio Code, SkiaSharp 3.116.0 / Skia 2.88.3, Android NDK r21, target arm64"
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "3.116.0",
        "2.88.9",
        "2.88.3"
      ],
      "workedIn": "2.88.9",
      "brokeIn": "3.116.0",
      "currentRelevance": "likely",
      "relevanceReason": "The user is attempting a manual Skia 2.88.3 build against SkiaSharp 3.116.0 which expects a newer Skia milestone; and is missing the git-sync-deps step that downloads third-party dependencies including zlib."
    }
  },
  "analysis": {
    "summary": "The build failure is caused by two root issues: (1) the user did not run 'python3 tools/git-sync-deps' (or the Cake 'git-sync-deps' task), which downloads zlib and other third-party Skia dependencies before building; and (2) the Android native build in SkiaSharp's Cake script explicitly requires macOS or Windows as the build host and will not run on Linux. The correct approach is to use 'dotnet cake --target=externals-android --arch=arm64' on a macOS or Windows machine.",
    "rationale": "The title 'how to build' and the content show this is a how-to question: the user is following an outdated wiki approach. Code investigation confirms the build.cake requires macOS/Windows for Android and depends on git-sync-deps. The missing file is a third-party dependency that is only present after running git-sync-deps.",
    "keySignals": [
      {
        "text": "ninja: error: '../../../third_party/externals/zlib/adler32_simd.c'... missing and no known rule to make it",
        "source": "issue body",
        "interpretation": "zlib was not synced from DEPS — user skipped python3 tools/git-sync-deps step."
      },
      {
        "text": "I followed the Wiki here https://github.com/mono/SkiaSharp/wiki/Building-on-Linux, skia version is 2.88.3",
        "source": "issue body",
        "interpretation": "User is following an outdated wiki and using Skia 2.88.3 which is incompatible with SkiaSharp 3.116.0."
      },
      {
        "text": "Version of SkiaSharp: 3.116.0 (Current), Last Known Good Version: 2.88.9 (Previous)",
        "source": "issue body",
        "interpretation": "User previously built with 2.88.9 successfully but is now attempting 3.116.0 with a different build approach."
      }
    ],
    "codeInvestigation": [
      {
        "file": "native/android/build.cake",
        "lines": "13-16",
        "finding": "Task('libSkiaSharp') has .IsDependentOn('git-sync-deps') and .WithCriteria(IsRunningOnMacOs() || IsRunningOnWindows()) — Android builds require macOS or Windows and must run git-sync-deps first to fetch zlib and other DEPS-managed third-party sources.",
        "relevance": "direct"
      },
      {
        "file": "scripts/cake/native-shared.cake",
        "lines": "22-43",
        "finding": "Task('git-sync-deps') runs python3 tools/git-sync-deps in externals/skia, which downloads third-party externals including zlib to third_party/externals/. Without this step, adler32_simd.c and other files are absent.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/building-linux.md",
        "lines": "40-46",
        "finding": "Manual build instructions show 'git submodule update --init --recursive' and 'python3 tools/git-sync-deps' as required steps before gn gen. The user skipped these. This guide is for Linux (host) targets, not Android.",
        "relevance": "related"
      },
      {
        "file": "documentation/dev/building.md",
        "lines": "151-153",
        "finding": "Documented Android cake target: 'dotnet cake --target=externals-android --arch=arm64' — must be run on macOS or Windows per build.cake WithCriteria.",
        "relevance": "context"
      }
    ],
    "workarounds": [
      "On macOS or Windows: use 'dotnet cake --target=externals-android --arch=arm64' — this handles git-sync-deps, gn, and ninja automatically.",
      "If manual gn/ninja build is required: first run 'git submodule update --init --recursive' then 'cd externals/skia && python3 tools/git-sync-deps' before running gn gen.",
      "Use the Skia version from the SkiaSharp submodule (externals/skia), not a separately checked-out 2.88.3 — SkiaSharp 3.116.0 requires a specific newer Skia milestone."
    ],
    "nextQuestions": [
      "Is the user building on Linux to cross-compile for Android, or building on a machine where macOS/Windows is available?",
      "What is the user's goal — distributing a custom build, or just using SkiaSharp for Android in an app?"
    ],
    "resolution": {
      "hypothesis": "The error is caused by the user skipping the git-sync-deps step (which fetches zlib and other DEPS-managed sources) and attempting to build Android on Linux (which is not supported by the Cake Android build target). The user should use the Cake build system on macOS or Windows.",
      "proposals": [
        {
          "title": "Use the Cake build system on macOS or Windows",
          "description": "Run 'dotnet cake --target=externals-android --arch=arm64' on macOS or Windows. This automates git-sync-deps, the correct gn args, and ninja invocation. Ensure ANDROID_NDK_ROOT or ANDROID_NDK_HOME is set.",
          "category": "fix",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Run git-sync-deps before manual build",
          "description": "If manual control is needed: git submodule update --init --recursive, then cd externals/skia && python3 tools/git-sync-deps, then proceed with gn gen using the SkiaSharp submodule's Skia (not a separate 2.88.3 checkout). Note that Android builds are only supported on macOS/Windows hosts in the official build system.",
          "category": "workaround",
          "confidence": 0.8,
          "effort": "cost/s",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Use the Cake build system on macOS or Windows",
      "recommendedReason": "Simplest and most reliable — handles all dependencies automatically and uses the correct Skia version pinned to SkiaSharp 3.116.0."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.85,
      "reason": "This is a how-to question — the error is caused by missing git-sync-deps and building on an unsupported host (Linux) for Android targets. The correct build process is documented and uses the Cake build system.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Correct the label from type/bug to type/question, add area/Build and platform labels",
        "risk": "low",
        "confidence": 0.88,
        "labels": [
          "type/question",
          "area/Build",
          "os/Android",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post answer explaining the correct build process and why the error occurs",
        "risk": "high",
        "confidence": 0.85,
        "comment": "Thanks for the report! The build error is caused by two issues:\n\n1. **Missing third-party dependency sync**: Before running `gn` and `ninja`, you must first sync Skia's third-party dependencies (including zlib). The `adler32_simd.c` file lives in `third_party/externals/zlib` which is only populated after running:\n   ```bash\n   git submodule update --init --recursive\n   cd externals/skia\n   python3 tools/git-sync-deps\n   ```\n\n2. **Android builds require macOS or Windows**: SkiaSharp's Android native build target (`externals-android`) only runs on macOS or Windows — Linux is not a supported host for Android cross-compilation in this build system.\n\n**Recommended approach** — on macOS or Windows, use the Cake build script:\n```bash\n# Set Android NDK path first\nexport ANDROID_NDK_HOME=/path/to/android-ndk\n\n# Build Android ARM64 native library\ndotnet cake --target=externals-android --arch=arm64\n```\n\nAlso note that SkiaSharp 3.116.0 expects a specific Skia milestone — use the `externals/skia` submodule from the SkiaSharp repository rather than a separately downloaded Skia 2.88.3.\n\nSee [Building SkiaSharp](https://github.com/mono/SkiaSharp/blob/main/documentation/dev/building.md) for full instructions."
      },
      {
        "type": "close-issue",
        "description": "Close as answered — this is a usage/how-to question with a clear documented answer",
        "risk": "medium",
        "confidence": 0.8,
        "stateReason": "completed"
      }
    ]
  }
}
```

</details>
