# Issue Triage Report — #2377

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-05-04T01:25:00Z |
| Type | type/question (0.85 (85%)) |
| Area | area/libSkiaSharp.native (0.90 (90%)) |
| Suggested action | needs-info (0.82 (82%)) |

**Issue Summary:** User's .NET 6 application on Ubuntu 20.04 throws DllNotFoundException for libSkiaSharp despite having SkiaSharp.NativeAssets.Linux.NoDependencies installed; ldd shows the library's own dependencies are satisfied, pointing to a deployment configuration issue.

**Analysis:** The DllNotFoundException is caused by libSkiaSharp.so not being in the expected runtime path at deployment time. The 'liblibSkiaSharp' string in the error is a .NET runtime fallback path (it prepends 'lib' to the DLL name as a search heuristic), not the primary failure. The ldd output confirms the library's own dependencies are satisfied when the file is found — the file is simply not in the path .NET searches. This is a deployment configuration issue: the NativeAssets package must be referenced in the executable project (not a library), and the publish must target the correct RID so the .so lands in the runtime output directory.

**Recommendations:** **needs-info** — This is a deployment configuration question. We need to know whether the NativeAssets package is in the executable project and what the publish output structure looks like before providing a definitive answer.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/libSkiaSharp.native |
| Platforms | os/Linux |
| Backends | — |
| Tenets | tenet/reliability |
| Partner | — |

## Evidence

### Reproduction

1. Build a .NET 6 web application using SkiaSharp 2.88.3 on Windows
2. Add SkiaSharp.NativeAssets.Linux.NoDependencies package reference
3. Publish and deploy to Ubuntu 20.04 x64 server running Apache
4. Trigger any code path that uses SkiaSharp (e.g., SKBitmap.Decode)

**Environment:** SkiaSharp 2.88.3, .NET 6, Ubuntu 20.04 x64, Apache web server, Windows 11 development machine

**Related issues:** #2385, #2653

**Repository links:**
- https://github.com/mono/SkiaSharp/issues/2385 — Related question: Why is SkiaSharp trying to load liblibSkiaSharp?
- https://github.com/mono/SkiaSharp/issues/2653 — Similar bug report: SKiaSharp on Linux can't find liblibSkiaSharp.so

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.88.3 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Linux native loading behavior has not fundamentally changed since 2.88.3; the deployment pattern guidance still applies. |

## Analysis

### Technical Summary

The DllNotFoundException is caused by libSkiaSharp.so not being in the expected runtime path at deployment time. The 'liblibSkiaSharp' string in the error is a .NET runtime fallback path (it prepends 'lib' to the DLL name as a search heuristic), not the primary failure. The ldd output confirms the library's own dependencies are satisfied when the file is found — the file is simply not in the path .NET searches. This is a deployment configuration issue: the NativeAssets package must be referenced in the executable project (not a library), and the publish must target the correct RID so the .so lands in the runtime output directory.

### Rationale

The issue is a deployment/configuration question, not a SkiaSharp code bug. The SKData static initializer is the first P/Invoke that fails when libSkiaSharp.so is not in the .NET runtime library search path. The ldd confirms the .so dependencies are satisfied when the file is located, which rules out a missing-dependency root cause. The most probable causes are: (1) NativeAssets.Linux.NoDependencies referenced in a library project instead of the executable, (2) publish output not containing the linux-x64 binary in the expected runtime path, or (3) deployment that only copies some files instead of the full publish output.

### Key Signals

- "liblibSkiaSharp: cannot open shared object file: No such file or directory" — **issue body — DllNotFoundException message** (The 'liblibSkiaSharp' name is a .NET fallback path with double 'lib' prefix, indicating the runtime searched multiple paths and none contained the file.)
- "I initially published the application with Target-Runtime = Portable but that didn't work." — **issue body** (Portable publish does not include linux-x64 native binaries; a linux-x64 RID-specific publish is required.)
- "I've run the ldd command and it appears to have all of the necessary dependencies for libSkiaSharp.so." — **issue body** (The file is present somewhere on the machine, but .NET cannot find it in its search paths. The library's own deps are not the problem.)
- "LD_DEBUG=libs libSkiaSharp.so … libSkiaSharp.so: command not found" — **issue body — LD_DEBUG output** (User ran LD_DEBUG incorrectly by trying to execute the .so as a shell command. This output is not meaningful for the actual .NET runtime loading issue.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SKData.cs` | 20-27 | direct | SKData static constructor calls SkiaApi.sk_data_new_empty() — the very first P/Invoke into libSkiaSharp. If the native library is not found, this throws TypeInitializationException wrapping DllNotFoundException, exactly matching the stack trace in the issue. |
| `documentation/dev/packages.md` | 179-215 | direct | Packages documentation explains the 'liblibSkiaSharp' error as a .NET runtime fallback path. Also documents that NativeAssets packages must be referenced in the executable project, not a library project, and that publishing with 'Portable' RID does not copy the linux-x64 binary to the expected runtime path. |

### Workarounds

- Ensure SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the executable (application) project, not a class library.
- Publish with --runtime linux-x64 --self-contained false to ensure the native binary is copied to runtimes/linux-x64/native/ in the output.
- Copy the entire publish output directory to the server, not just selected files.
- Set the environment variable LD_LIBRARY_PATH to include the directory containing libSkiaSharp.so as a temporary workaround.

### Next Questions

- Is SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the executable project or in a class library?
- What does the publish output directory structure look like? Is libSkiaSharp.so present under runtimes/linux-x64/native/?
- How is the application deployed to the Apache server — full publish output or only selected files?

### Resolution Proposals

**Hypothesis:** The libSkiaSharp.so binary is either not included in the deployment (NativeAssets in wrong project or partial file copy) or not in a path that .NET searches at runtime.

1. **Move NativeAssets reference to executable project** — fix, confidence 0.85 (85%), cost/xs, validated=untested
   - Ensure SkiaSharp.NativeAssets.Linux.NoDependencies is in the .csproj of the web application (the executable), not a referenced class library. Then republish with --runtime linux-x64.
2. **Set LD_LIBRARY_PATH as temporary workaround** — workaround, confidence 0.75 (75%), cost/xs, validated=untested
   - If libSkiaSharp.so is already present in the deployment directory, set LD_LIBRARY_PATH to include that directory before launching dotnet. E.g.: export LD_LIBRARY_PATH=/var/www/mytest.com/public_html:$LD_LIBRARY_PATH

**Recommended proposal:** Move NativeAssets reference to executable project

**Why:** The most common cause of this pattern is the NativeAssets package being in the wrong project. Republishing with the correct RID ensures the binary is in the expected runtime path.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.82 (82%) |
| Reason | This is a deployment configuration question. We need to know whether the NativeAssets package is in the executable project and what the publish output structure looks like before providing a definitive answer. |
| Suggested repro platform | linux |

### Missing Info

- Is SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the .csproj of the web application (executable) or in a class library project?
- What is the full publish command used (dotnet publish arguments, especially --runtime)?
- Is libSkiaSharp.so present in the deployed directory? If so, what is its exact path?

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question, linux native, and Linux platform labels | labels=type/question, area/libSkiaSharp.native, os/Linux |
| add-comment | medium | 0.82 (82%) | Provide deployment troubleshooting guidance and request missing info | — |
| link-related | low | 0.85 (85%) | Cross-reference related liblibSkiaSharp question | linkedIssue=#2385 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report! The `liblibSkiaSharp` string in the error is a .NET runtime fallback path (it prepends `lib` to the DLL name as a search heuristic) — this is normal when the library is not found in any search path.

The most common causes for this error are:

1. **NativeAssets in wrong project** — `SkiaSharp.NativeAssets.Linux.NoDependencies` must be referenced in the **application** (executable) project `.csproj`, not in a class library. If it's only in a library, the native binary won't be copied to the output.

2. **Partial deployment** — The entire publish output must be deployed, not just selected files. The native binary lives at `runtimes/linux-x64/native/libSkiaSharp.so` relative to the publish output root.

3. **Portable publish** — Publishing with `Target-Runtime = Portable` (no RID) does not copy the linux-x64 native binary. You need `--runtime linux-x64` (or `linux-x64` in the project settings).

To diagnose, can you confirm:
- Which `.csproj` file contains the `SkiaSharp.NativeAssets.Linux.NoDependencies` reference?
- Is `libSkiaSharp.so` present somewhere in your deployed directory? If yes, what is the exact path?
- What is the exact `dotnet publish` command you used?

As a quick workaround while investigating, you can set:
```bash
export LD_LIBRARY_PATH=/path/to/directory/containing/libSkiaSharp.so:$LD_LIBRARY_PATH
```
before starting your application.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 2377,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-05-04T01:25:00Z"
  },
  "summary": "User's .NET 6 application on Ubuntu 20.04 throws DllNotFoundException for libSkiaSharp despite having SkiaSharp.NativeAssets.Linux.NoDependencies installed; ldd shows the library's own dependencies are satisfied, pointing to a deployment configuration issue.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.85
    },
    "area": {
      "value": "area/libSkiaSharp.native",
      "confidence": 0.9
    },
    "platforms": [
      "os/Linux"
    ],
    "tenets": [
      "tenet/reliability"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Build a .NET 6 web application using SkiaSharp 2.88.3 on Windows",
        "Add SkiaSharp.NativeAssets.Linux.NoDependencies package reference",
        "Publish and deploy to Ubuntu 20.04 x64 server running Apache",
        "Trigger any code path that uses SkiaSharp (e.g., SKBitmap.Decode)"
      ],
      "environmentDetails": "SkiaSharp 2.88.3, .NET 6, Ubuntu 20.04 x64, Apache web server, Windows 11 development machine",
      "relatedIssues": [
        2385,
        2653
      ],
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2385",
          "description": "Related question: Why is SkiaSharp trying to load liblibSkiaSharp?"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2653",
          "description": "Similar bug report: SKiaSharp on Linux can't find liblibSkiaSharp.so"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.88.3"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Linux native loading behavior has not fundamentally changed since 2.88.3; the deployment pattern guidance still applies."
    }
  },
  "analysis": {
    "summary": "The DllNotFoundException is caused by libSkiaSharp.so not being in the expected runtime path at deployment time. The 'liblibSkiaSharp' string in the error is a .NET runtime fallback path (it prepends 'lib' to the DLL name as a search heuristic), not the primary failure. The ldd output confirms the library's own dependencies are satisfied when the file is found — the file is simply not in the path .NET searches. This is a deployment configuration issue: the NativeAssets package must be referenced in the executable project (not a library), and the publish must target the correct RID so the .so lands in the runtime output directory.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SKData.cs",
        "lines": "20-27",
        "finding": "SKData static constructor calls SkiaApi.sk_data_new_empty() — the very first P/Invoke into libSkiaSharp. If the native library is not found, this throws TypeInitializationException wrapping DllNotFoundException, exactly matching the stack trace in the issue.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "179-215",
        "finding": "Packages documentation explains the 'liblibSkiaSharp' error as a .NET runtime fallback path. Also documents that NativeAssets packages must be referenced in the executable project, not a library project, and that publishing with 'Portable' RID does not copy the linux-x64 binary to the expected runtime path.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "liblibSkiaSharp: cannot open shared object file: No such file or directory",
        "source": "issue body — DllNotFoundException message",
        "interpretation": "The 'liblibSkiaSharp' name is a .NET fallback path with double 'lib' prefix, indicating the runtime searched multiple paths and none contained the file."
      },
      {
        "text": "I initially published the application with Target-Runtime = Portable but that didn't work.",
        "source": "issue body",
        "interpretation": "Portable publish does not include linux-x64 native binaries; a linux-x64 RID-specific publish is required."
      },
      {
        "text": "I've run the ldd command and it appears to have all of the necessary dependencies for libSkiaSharp.so.",
        "source": "issue body",
        "interpretation": "The file is present somewhere on the machine, but .NET cannot find it in its search paths. The library's own deps are not the problem."
      },
      {
        "text": "LD_DEBUG=libs libSkiaSharp.so … libSkiaSharp.so: command not found",
        "source": "issue body — LD_DEBUG output",
        "interpretation": "User ran LD_DEBUG incorrectly by trying to execute the .so as a shell command. This output is not meaningful for the actual .NET runtime loading issue."
      }
    ],
    "rationale": "The issue is a deployment/configuration question, not a SkiaSharp code bug. The SKData static initializer is the first P/Invoke that fails when libSkiaSharp.so is not in the .NET runtime library search path. The ldd confirms the .so dependencies are satisfied when the file is located, which rules out a missing-dependency root cause. The most probable causes are: (1) NativeAssets.Linux.NoDependencies referenced in a library project instead of the executable, (2) publish output not containing the linux-x64 binary in the expected runtime path, or (3) deployment that only copies some files instead of the full publish output.",
    "workarounds": [
      "Ensure SkiaSharp.NativeAssets.Linux.NoDependencies is referenced in the executable (application) project, not a class library.",
      "Publish with --runtime linux-x64 --self-contained false to ensure the native binary is copied to runtimes/linux-x64/native/ in the output.",
      "Copy the entire publish output directory to the server, not just selected files.",
      "Set the environment variable LD_LIBRARY_PATH to include the directory containing libSkiaSharp.so as a temporary workaround."
    ],
    "nextQuestions": [
      "Is SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the executable project or in a class library?",
      "What does the publish output directory structure look like? Is libSkiaSharp.so present under runtimes/linux-x64/native/?",
      "How is the application deployed to the Apache server — full publish output or only selected files?"
    ],
    "resolution": {
      "hypothesis": "The libSkiaSharp.so binary is either not included in the deployment (NativeAssets in wrong project or partial file copy) or not in a path that .NET searches at runtime.",
      "proposals": [
        {
          "title": "Move NativeAssets reference to executable project",
          "description": "Ensure SkiaSharp.NativeAssets.Linux.NoDependencies is in the .csproj of the web application (the executable), not a referenced class library. Then republish with --runtime linux-x64.",
          "category": "fix",
          "confidence": 0.85,
          "effort": "cost/xs",
          "validated": "untested"
        },
        {
          "title": "Set LD_LIBRARY_PATH as temporary workaround",
          "description": "If libSkiaSharp.so is already present in the deployment directory, set LD_LIBRARY_PATH to include that directory before launching dotnet. E.g.: export LD_LIBRARY_PATH=/var/www/mytest.com/public_html:$LD_LIBRARY_PATH",
          "category": "workaround",
          "confidence": 0.75,
          "effort": "cost/xs",
          "validated": "untested"
        }
      ],
      "recommendedProposal": "Move NativeAssets reference to executable project",
      "recommendedReason": "The most common cause of this pattern is the NativeAssets package being in the wrong project. Republishing with the correct RID ensures the binary is in the expected runtime path."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.82,
      "reason": "This is a deployment configuration question. We need to know whether the NativeAssets package is in the executable project and what the publish output structure looks like before providing a definitive answer.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Is SkiaSharp.NativeAssets.Linux.NoDependencies referenced in the .csproj of the web application (executable) or in a class library project?",
      "What is the full publish command used (dotnet publish arguments, especially --runtime)?",
      "Is libSkiaSharp.so present in the deployed directory? If so, what is its exact path?"
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question, linux native, and Linux platform labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/libSkiaSharp.native",
          "os/Linux"
        ]
      },
      {
        "type": "add-comment",
        "description": "Provide deployment troubleshooting guidance and request missing info",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "Thanks for the detailed report! The `liblibSkiaSharp` string in the error is a .NET runtime fallback path (it prepends `lib` to the DLL name as a search heuristic) — this is normal when the library is not found in any search path.\n\nThe most common causes for this error are:\n\n1. **NativeAssets in wrong project** — `SkiaSharp.NativeAssets.Linux.NoDependencies` must be referenced in the **application** (executable) project `.csproj`, not in a class library. If it's only in a library, the native binary won't be copied to the output.\n\n2. **Partial deployment** — The entire publish output must be deployed, not just selected files. The native binary lives at `runtimes/linux-x64/native/libSkiaSharp.so` relative to the publish output root.\n\n3. **Portable publish** — Publishing with `Target-Runtime = Portable` (no RID) does not copy the linux-x64 native binary. You need `--runtime linux-x64` (or `linux-x64` in the project settings).\n\nTo diagnose, can you confirm:\n- Which `.csproj` file contains the `SkiaSharp.NativeAssets.Linux.NoDependencies` reference?\n- Is `libSkiaSharp.so` present somewhere in your deployed directory? If yes, what is the exact path?\n- What is the exact `dotnet publish` command you used?\n\nAs a quick workaround while investigating, you can set:\n```bash\nexport LD_LIBRARY_PATH=/path/to/directory/containing/libSkiaSharp.so:$LD_LIBRARY_PATH\n```\nbefore starting your application."
      },
      {
        "type": "link-related",
        "description": "Cross-reference related liblibSkiaSharp question",
        "risk": "low",
        "confidence": 0.85,
        "linkedIssue": 2385
      }
    ]
  }
}
```

</details>
