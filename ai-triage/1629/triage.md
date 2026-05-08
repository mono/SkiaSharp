# Issue Triage Report — #1629

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-25T03:43:39Z |
| Type | type/bug (0.75 (75%)) |
| Area | area/libSkiaSharp.native (0.95 (95%)) |
| Suggested action | needs-info (0.85 (85%)) |

**Issue Summary:** DllNotFoundException for libSkiaSharp on AWS Lambda (.NET Core 3.1) when SkiaSharp.NativeAssets.Linux is not correctly installed in the executable project; the native library fails to load because it was not deployed to the Lambda function output.

**Analysis:** The native libSkiaSharp.so file was not deployed to the AWS Lambda function output. Root cause is either: (a) the reporter used a misspelled package name 'SkiaSharp.NativeAssests.Linux' (typo — no such package) instead of 'SkiaSharp.NativeAssets.Linux', or (b) the NativeAssets package was referenced in a library project rather than the executable project, preventing the .so from being copied to the publish output. Linux NativeAssets are not auto-included by SkiaSharp core and must be manually added to the app project.

**Recommendations:** **needs-info** — High likelihood this is a user configuration issue (typo in package name or wrong project placement), but need confirmation. Community workaround exists and is documented.

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

## Evidence

### Reproduction

1. Create empty AWS Lambda Project (.NET Core - C#) in Visual Studio
2. Add SkiaSharp drawing code in FunctionHandler
3. Install SkiaSharp v2.80.2 and SkiaSharp.NativeAssets.Linux v2.80.2
4. Publish to AWS Lambda
5. Invoke function via ASP.NET Core app

**Environment:** SkiaSharp v2.80.2, ASP.NET Core 3.1 AWS Lambda, Visual Studio 2019 16.8.4

**Repository links:**
- https://github.com/mono/SkiaSharp/files/5971480/IssueReproduceSample.zip — Attached reproduction zip provided by reporter
- https://github.com/mono/SkiaSharp/issues/288 — Earlier similar issue: Unable to load DLL libSkiaSharp on Linux (AWS Lambda) — closed completed
- https://github.com/mono/SkiaSharp/issues/2961 — Similar issue: libSkiaSharp DllNotFoundException on AWS Lambda .NET 8 — closed
- https://github.com/mono/SkiaSharp/issues/3235 — Same reporter, same error in AWS Elastic Beanstalk .NET 6.0 — open

### Bug Signals

| Field | Value |
|-------|-------|
| Severity | medium |
| Regression claimed | False |
| Error type | exception |
| Error message | Unable to load shared library 'libSkiaSharp' or one of its dependencies. liblibSkiaSharp: cannot open shared object file: No such file or directory |
| Repro quality | complete |
| Target frameworks | netcoreapp3.1 |

**Stack trace:**

```text
at SkiaSharp.SkiaApi.sk_colortype_get_default_8888()
at SkiaSharp.SKImageInfo..cctor()
at SimpleAWS.Function.FunctionHandler(String input, ILambdaContext context)
```

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | likely |
| Relevance reason | Linux NativeAssets must still be added manually in the executable project; this pattern has not changed across versions. |

## Analysis

### Technical Summary

The native libSkiaSharp.so file was not deployed to the AWS Lambda function output. Root cause is either: (a) the reporter used a misspelled package name 'SkiaSharp.NativeAssests.Linux' (typo — no such package) instead of 'SkiaSharp.NativeAssets.Linux', or (b) the NativeAssets package was referenced in a library project rather than the executable project, preventing the .so from being copied to the publish output. Linux NativeAssets are not auto-included by SkiaSharp core and must be manually added to the app project.

### Rationale

Error is DllNotFoundException wrapping TypeInitializationException — a canonical 'native .so not found at runtime' failure pattern on Linux. The reporter says they installed 'SkiaSharp.NativeAssests.Linux' (note the typo 'Assests' vs 'Assets'). Multiple community members have reported the same symptom and confirmed the fix is correctly installing SkiaSharp.NativeAssets.Linux. Issue #288 and #2961 cover identical scenarios. Documentation in packages.md explicitly states Linux NativeAssets must be added manually to the executable project.

### Key Signals

- "Install SkiaSharp v2.80.2, SkiaSharp.NativeAssests.Linux v2.80.2" — **issue body** (Reporter has a typo: 'NativeAssests' instead of 'NativeAssets'. No such NuGet package exists with the misspelled name, so the native binary was never installed.)
- "liblibSkiaSharp: cannot open shared object file: No such file or directory" — **issue body stack trace** (Classic Linux DllNotFoundException: the libSkiaSharp.so file is simply not present in the deployment output directory.)
- "This issue not occur in ASP.NET Core 2.1 AWS Lambda application" — **issue body** (Possibly the 2.1 version of the application did not use SkiaSharp, or used it differently. SkiaSharp NativeAssets behave the same across these versions.)
- "It turns out AWS lambdas use AWS Linux and to solve the issue, we need to install the NuGet package SkiaSharp.NativeAssets.Linux." — **comment by derekdkim** (Community-confirmed fix: installing the correctly-named SkiaSharp.NativeAssets.Linux package resolves the error.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj` | — | direct | Ships libSkiaSharp*.so under runtimes/linux-x64/native/ (and other RIDs). Must be referenced in the executable project for the .so to be copied to publish output. If only referenced in a library project, the runtime directory is not propagated to the app output. |
| `documentation/dev/packages.md` | 86, 139-141, 179-184 | direct | Confirms Linux NativeAssets must be added manually and must be in the executable project. Documents the exact error pattern (liblibSkiaSharp cannot open shared object file) and its root cause: NativeAssets package missing or placed in a library-only project. |

### Workarounds

- Add `SkiaSharp.NativeAssets.Linux` (correct spelling, not 'NativeAssests') to the executable/Lambda project — not a library project.
- Alternatively, use `SkiaSharp.NativeAssets.Linux.NoDependencies` for minimal Lambda containers with no fontconfig dependency.

### Next Questions

- Did the reporter actually install 'SkiaSharp.NativeAssets.Linux' (correct) or 'SkiaSharp.NativeAssests.Linux' (typo)?
- Was the NativeAssets package referenced in the Lambda executable project or in a shared library project?
- What is the publish mode (framework-dependent vs self-contained) used for the Lambda deployment?

### Resolution Proposals

**Hypothesis:** The libSkiaSharp.so was not deployed because the reporter used a misspelled package name or placed the NativeAssets reference in the wrong project. Correct package name in the executable project will fix it.

1. **Install correct NativeAssets package in executable project** — fix, confidence 0.90 (90%), cost/xs, validated=yes
   - Add SkiaSharp.NativeAssets.Linux (or SkiaSharp.NativeAssets.Linux.NoDependencies for containers) as a PackageReference in the AWS Lambda function project (the executable), not any shared library.

```csharp
<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.9" />
```

**Recommended proposal:** Install correct NativeAssets package in executable project

**Why:** Community confirmed and matches documented behavior in packages.md. NoDependencies variant recommended for Lambda/container environments.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-info |
| Confidence | 0.85 (85%) |
| Reason | High likelihood this is a user configuration issue (typo in package name or wrong project placement), but need confirmation. Community workaround exists and is documented. |
| Suggested repro platform | linux |

### Missing Info

- Confirm the exact NuGet package name installed — was it 'SkiaSharp.NativeAssets.Linux' (correct) or 'SkiaSharp.NativeAssests.Linux' (typo)?
- Confirm whether the NativeAssets package is referenced in the Lambda function project (executable) and not only in a library project.
- Confirm the publish mode (self-contained vs framework-dependent) used for AWS Lambda deployment.

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.95 (95%) | Apply native library, Linux, and reliability labels | labels=type/bug, area/libSkiaSharp.native, os/Linux, tenet/reliability |
| add-comment | medium | 0.88 (88%) | Provide workaround guidance and request confirmation of package name and project placement | — |
| link-related | low | 0.90 (90%) | Cross-reference related issues with same root cause | linkedIssue=#288 |

**Comment draft for `add-comment`:**

```markdown
Thanks for the detailed report and reproduction sample!

The error `liblibSkiaSharp: cannot open shared object file: No such file or directory` indicates that `libSkiaSharp.so` was not deployed to the Lambda output directory.

A few things to check:

1. **Package name spelling** — The issue mentions `SkiaSharp.NativeAssests.Linux` (note *Assests*). The correct package name is **`SkiaSharp.NativeAssets.Linux`** (note *Assets*). No NuGet package with the misspelled name exists, so if that was used the native binary was never installed.

2. **Reference in the executable project** — The NativeAssets package must be added to the **Lambda function project** (the executable), not to any shared library or class library project. If it's only in a transitive library the .so won't be copied to the publish output.

3. **Recommended package for Lambda/containers** — For AWS Lambda (minimal Linux environment), use `SkiaSharp.NativeAssets.Linux.NoDependencies` which has zero external dependencies (no fontconfig required):
   ```xml
   <PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="2.88.9" />
   ```

Could you confirm which exact package name you installed, and whether it's in the Lambda function project?
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1629,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-25T03:43:39Z"
  },
  "summary": "DllNotFoundException for libSkiaSharp on AWS Lambda (.NET Core 3.1) when SkiaSharp.NativeAssets.Linux is not correctly installed in the executable project; the native library fails to load because it was not deployed to the Lambda function output.",
  "classification": {
    "type": {
      "value": "type/bug",
      "confidence": 0.75
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
      "severity": "medium",
      "regressionClaimed": false,
      "errorType": "exception",
      "errorMessage": "Unable to load shared library 'libSkiaSharp' or one of its dependencies. liblibSkiaSharp: cannot open shared object file: No such file or directory",
      "stackTrace": "at SkiaSharp.SkiaApi.sk_colortype_get_default_8888()\nat SkiaSharp.SKImageInfo..cctor()\nat SimpleAWS.Function.FunctionHandler(String input, ILambdaContext context)",
      "reproQuality": "complete",
      "targetFrameworks": [
        "netcoreapp3.1"
      ]
    },
    "reproEvidence": {
      "stepsToReproduce": [
        "Create empty AWS Lambda Project (.NET Core - C#) in Visual Studio",
        "Add SkiaSharp drawing code in FunctionHandler",
        "Install SkiaSharp v2.80.2 and SkiaSharp.NativeAssets.Linux v2.80.2",
        "Publish to AWS Lambda",
        "Invoke function via ASP.NET Core app"
      ],
      "environmentDetails": "SkiaSharp v2.80.2, ASP.NET Core 3.1 AWS Lambda, Visual Studio 2019 16.8.4",
      "repoLinks": [
        {
          "url": "https://github.com/mono/SkiaSharp/files/5971480/IssueReproduceSample.zip",
          "description": "Attached reproduction zip provided by reporter"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/288",
          "description": "Earlier similar issue: Unable to load DLL libSkiaSharp on Linux (AWS Lambda) — closed completed"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/2961",
          "description": "Similar issue: libSkiaSharp DllNotFoundException on AWS Lambda .NET 8 — closed"
        },
        {
          "url": "https://github.com/mono/SkiaSharp/issues/3235",
          "description": "Same reporter, same error in AWS Elastic Beanstalk .NET 6.0 — open"
        }
      ]
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "likely",
      "relevanceReason": "Linux NativeAssets must still be added manually in the executable project; this pattern has not changed across versions."
    }
  },
  "analysis": {
    "summary": "The native libSkiaSharp.so file was not deployed to the AWS Lambda function output. Root cause is either: (a) the reporter used a misspelled package name 'SkiaSharp.NativeAssests.Linux' (typo — no such package) instead of 'SkiaSharp.NativeAssets.Linux', or (b) the NativeAssets package was referenced in a library project rather than the executable project, preventing the .so from being copied to the publish output. Linux NativeAssets are not auto-included by SkiaSharp core and must be manually added to the app project.",
    "rationale": "Error is DllNotFoundException wrapping TypeInitializationException — a canonical 'native .so not found at runtime' failure pattern on Linux. The reporter says they installed 'SkiaSharp.NativeAssests.Linux' (note the typo 'Assests' vs 'Assets'). Multiple community members have reported the same symptom and confirmed the fix is correctly installing SkiaSharp.NativeAssets.Linux. Issue #288 and #2961 cover identical scenarios. Documentation in packages.md explicitly states Linux NativeAssets must be added manually to the executable project.",
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp.NativeAssets.Linux/SkiaSharp.NativeAssets.Linux.csproj",
        "finding": "Ships libSkiaSharp*.so under runtimes/linux-x64/native/ (and other RIDs). Must be referenced in the executable project for the .so to be copied to publish output. If only referenced in a library project, the runtime directory is not propagated to the app output.",
        "relevance": "direct"
      },
      {
        "file": "documentation/dev/packages.md",
        "lines": "86, 139-141, 179-184",
        "finding": "Confirms Linux NativeAssets must be added manually and must be in the executable project. Documents the exact error pattern (liblibSkiaSharp cannot open shared object file) and its root cause: NativeAssets package missing or placed in a library-only project.",
        "relevance": "direct"
      }
    ],
    "keySignals": [
      {
        "text": "Install SkiaSharp v2.80.2, SkiaSharp.NativeAssests.Linux v2.80.2",
        "source": "issue body",
        "interpretation": "Reporter has a typo: 'NativeAssests' instead of 'NativeAssets'. No such NuGet package exists with the misspelled name, so the native binary was never installed."
      },
      {
        "text": "liblibSkiaSharp: cannot open shared object file: No such file or directory",
        "source": "issue body stack trace",
        "interpretation": "Classic Linux DllNotFoundException: the libSkiaSharp.so file is simply not present in the deployment output directory."
      },
      {
        "text": "This issue not occur in ASP.NET Core 2.1 AWS Lambda application",
        "source": "issue body",
        "interpretation": "Possibly the 2.1 version of the application did not use SkiaSharp, or used it differently. SkiaSharp NativeAssets behave the same across these versions."
      },
      {
        "text": "It turns out AWS lambdas use AWS Linux and to solve the issue, we need to install the NuGet package SkiaSharp.NativeAssets.Linux.",
        "source": "comment by derekdkim",
        "interpretation": "Community-confirmed fix: installing the correctly-named SkiaSharp.NativeAssets.Linux package resolves the error."
      }
    ],
    "workarounds": [
      "Add `SkiaSharp.NativeAssets.Linux` (correct spelling, not 'NativeAssests') to the executable/Lambda project — not a library project.",
      "Alternatively, use `SkiaSharp.NativeAssets.Linux.NoDependencies` for minimal Lambda containers with no fontconfig dependency."
    ],
    "nextQuestions": [
      "Did the reporter actually install 'SkiaSharp.NativeAssets.Linux' (correct) or 'SkiaSharp.NativeAssests.Linux' (typo)?",
      "Was the NativeAssets package referenced in the Lambda executable project or in a shared library project?",
      "What is the publish mode (framework-dependent vs self-contained) used for the Lambda deployment?"
    ],
    "resolution": {
      "hypothesis": "The libSkiaSharp.so was not deployed because the reporter used a misspelled package name or placed the NativeAssets reference in the wrong project. Correct package name in the executable project will fix it.",
      "proposals": [
        {
          "title": "Install correct NativeAssets package in executable project",
          "description": "Add SkiaSharp.NativeAssets.Linux (or SkiaSharp.NativeAssets.Linux.NoDependencies for containers) as a PackageReference in the AWS Lambda function project (the executable), not any shared library.",
          "codeSnippet": "<PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.9\" />",
          "category": "fix",
          "confidence": 0.9,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Install correct NativeAssets package in executable project",
      "recommendedReason": "Community confirmed and matches documented behavior in packages.md. NoDependencies variant recommended for Lambda/container environments."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-info",
      "confidence": 0.85,
      "reason": "High likelihood this is a user configuration issue (typo in package name or wrong project placement), but need confirmation. Community workaround exists and is documented.",
      "suggestedReproPlatform": "linux"
    },
    "missingInfo": [
      "Confirm the exact NuGet package name installed — was it 'SkiaSharp.NativeAssets.Linux' (correct) or 'SkiaSharp.NativeAssests.Linux' (typo)?",
      "Confirm whether the NativeAssets package is referenced in the Lambda function project (executable) and not only in a library project.",
      "Confirm the publish mode (self-contained vs framework-dependent) used for AWS Lambda deployment."
    ],
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply native library, Linux, and reliability labels",
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
        "description": "Provide workaround guidance and request confirmation of package name and project placement",
        "risk": "medium",
        "confidence": 0.88,
        "comment": "Thanks for the detailed report and reproduction sample!\n\nThe error `liblibSkiaSharp: cannot open shared object file: No such file or directory` indicates that `libSkiaSharp.so` was not deployed to the Lambda output directory.\n\nA few things to check:\n\n1. **Package name spelling** — The issue mentions `SkiaSharp.NativeAssests.Linux` (note *Assests*). The correct package name is **`SkiaSharp.NativeAssets.Linux`** (note *Assets*). No NuGet package with the misspelled name exists, so if that was used the native binary was never installed.\n\n2. **Reference in the executable project** — The NativeAssets package must be added to the **Lambda function project** (the executable), not to any shared library or class library project. If it's only in a transitive library the .so won't be copied to the publish output.\n\n3. **Recommended package for Lambda/containers** — For AWS Lambda (minimal Linux environment), use `SkiaSharp.NativeAssets.Linux.NoDependencies` which has zero external dependencies (no fontconfig required):\n   ```xml\n   <PackageReference Include=\"SkiaSharp.NativeAssets.Linux.NoDependencies\" Version=\"2.88.9\" />\n   ```\n\nCould you confirm which exact package name you installed, and whether it's in the Lambda function project?"
      },
      {
        "type": "link-related",
        "description": "Cross-reference related issues with same root cause",
        "risk": "low",
        "confidence": 0.9,
        "linkedIssue": 288
      }
    ]
  }
}
```

</details>
