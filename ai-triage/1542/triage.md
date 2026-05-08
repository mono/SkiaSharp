# Issue Triage Report — #1542

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-26T10:45:00Z |
| Type | type/question (0.90 (90%)) |
| Area | area/Build (0.85 (85%)) |
| Suggested action | close-as-not-a-bug (0.90 (90%)) |

**Issue Summary:** Reporter gets NuGet NU1108 cycle error when installing SkiaSharp 2.80.2; root cause confirmed to be a user project named 'SkiaSharp', which confuses NuGet into seeing a self-referential dependency.

**Analysis:** NuGet error NU1108 ('Cycle detected: SkiaSharp -> SkiaSharp') was triggered because the reporter's project was named 'SkiaSharp', causing NuGet to detect a circular dependency between the project and the NuGet package of the same name. This is not a bug in SkiaSharp.

**Recommendations:** **close-as-not-a-bug** — Reporter confirmed the issue was caused by naming their project 'SkiaSharp', which conflicts with the NuGet package name. No bug exists in SkiaSharp. The issue was self-resolved once the root cause was understood.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/question |
| Area | area/Build |
| Platforms | — |
| Backends | — |
| Tenets | tenet/compatibility |
| Partner | — |

## Evidence

### Reproduction

1. Create a .NET Core project named 'SkiaSharp' in Visual Studio 2019
2. Attempt to install SkiaSharp NuGet package via NuGet Manager
3. Observe NU1108 error: 'Cycle detected. SkiaSharp -> SkiaSharp (>= 2.80.2)'

**Environment:** Visual Studio 2019, .NET Core project, SkiaSharp 2.80.2

### Version Analysis

| Field | Value |
|-------|-------|
| Mentioned versions | 2.80.2 |
| Worked in | — |
| Broke in | — |
| Current relevance | unlikely |
| Relevance reason | The issue is not in SkiaSharp itself — it was caused by the reporter's project being named 'SkiaSharp', which NuGet treats as the same identity as the package. The reporter confirmed this in the comments. |

## Analysis

### Technical Summary

NuGet error NU1108 ('Cycle detected: SkiaSharp -> SkiaSharp') was triggered because the reporter's project was named 'SkiaSharp', causing NuGet to detect a circular dependency between the project and the NuGet package of the same name. This is not a bug in SkiaSharp.

### Rationale

The reporter confirmed in a follow-up comment that naming their project 'SkiaSharp' was the root cause. The maintainer correctly identified this as a NuGet/Visual Studio behavior rather than a SkiaSharp defect. The SkiaSharp.csproj has no self-referential dependency — no cycles exist in the published package.

### Key Signals

- "Cycle detected. SkiaSharp -> SkiaSharp (>= 2.80.2)" — **issue body** (NuGet sees a project and a package with the same name 'SkiaSharp', creating a self-referential dependency.)
- "Unless you have a project in your solution named SkiaSharp? Maybe VS is getting confused?" — **comment by mattleibow** (Maintainer immediately identified the likely cause.)
- "I had called the project I reported to you SkiaSharp." — **reporter follow-up comment** (Reporter confirms the project name was 'SkiaSharp', fully explaining the NU1108 cycle detection.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/SkiaSharp.csproj` | — | direct | No self-referential PackageReference or circular ProjectReference exists in the SkiaSharp binding project. The package definitions are clean — confirming NU1108 was not caused by the SkiaSharp package itself. |

### Workarounds

- Rename the user project to something other than 'SkiaSharp' to avoid the NuGet name collision
- Clear NuGet cache with 'nuget locals all -clear' if a corrupted cache is suspected

### Resolution Proposals

**Hypothesis:** User named their project 'SkiaSharp', which causes NuGet to detect a cyclic dependency when adding the SkiaSharp NuGet package to that project. Renaming the project resolves the issue.

1. **Rename the user project** — workaround, confidence 0.95 (95%), cost/xs, validated=yes
   - Rename the consumer project to any name other than 'SkiaSharp'. NuGet matches project identity by name, so having a project and package with the same name triggers NU1108.

**Recommended proposal:** Rename the user project

**Why:** Root cause confirmed by reporter; trivial fix on the user's side.

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | close-as-not-a-bug |
| Confidence | 0.90 (90%) |
| Reason | Reporter confirmed the issue was caused by naming their project 'SkiaSharp', which conflicts with the NuGet package name. No bug exists in SkiaSharp. The issue was self-resolved once the root cause was understood. |
| Suggested repro platform | — |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.90 (90%) | Apply question and build labels | labels=type/question, area/Build |
| add-comment | high | 0.90 (90%) | Post a brief closing comment explaining the root cause and close | — |
| close-issue | medium | 0.88 (88%) | Close as not a bug — user error confirmed | stateReason=not_planned |

**Comment draft for `add-comment`:**

```markdown
Thanks for following up! As confirmed in the thread, the NU1108 cycle error occurs because your project was named **SkiaSharp** — NuGet treats the project and the package as the same identity and reports a circular dependency.

The fix is to rename your project to any other name. This behavior is by-design in NuGet (not specific to SkiaSharp) and has been reported to Microsoft.

Closing this as not a bug in SkiaSharp. Feel free to reopen if you encounter a different issue.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 1542,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-26T10:45:00Z"
  },
  "summary": "Reporter gets NuGet NU1108 cycle error when installing SkiaSharp 2.80.2; root cause confirmed to be a user project named 'SkiaSharp', which confuses NuGet into seeing a self-referential dependency.",
  "classification": {
    "type": {
      "value": "type/question",
      "confidence": 0.9
    },
    "area": {
      "value": "area/Build",
      "confidence": 0.85
    },
    "tenets": [
      "tenet/compatibility"
    ]
  },
  "evidence": {
    "reproEvidence": {
      "stepsToReproduce": [
        "Create a .NET Core project named 'SkiaSharp' in Visual Studio 2019",
        "Attempt to install SkiaSharp NuGet package via NuGet Manager",
        "Observe NU1108 error: 'Cycle detected. SkiaSharp -> SkiaSharp (>= 2.80.2)'"
      ],
      "environmentDetails": "Visual Studio 2019, .NET Core project, SkiaSharp 2.80.2",
      "repoLinks": []
    },
    "versionAnalysis": {
      "mentionedVersions": [
        "2.80.2"
      ],
      "currentRelevance": "unlikely",
      "relevanceReason": "The issue is not in SkiaSharp itself — it was caused by the reporter's project being named 'SkiaSharp', which NuGet treats as the same identity as the package. The reporter confirmed this in the comments."
    }
  },
  "analysis": {
    "summary": "NuGet error NU1108 ('Cycle detected: SkiaSharp -> SkiaSharp') was triggered because the reporter's project was named 'SkiaSharp', causing NuGet to detect a circular dependency between the project and the NuGet package of the same name. This is not a bug in SkiaSharp.",
    "rationale": "The reporter confirmed in a follow-up comment that naming their project 'SkiaSharp' was the root cause. The maintainer correctly identified this as a NuGet/Visual Studio behavior rather than a SkiaSharp defect. The SkiaSharp.csproj has no self-referential dependency — no cycles exist in the published package.",
    "keySignals": [
      {
        "text": "Cycle detected. SkiaSharp -> SkiaSharp (>= 2.80.2)",
        "source": "issue body",
        "interpretation": "NuGet sees a project and a package with the same name 'SkiaSharp', creating a self-referential dependency."
      },
      {
        "text": "Unless you have a project in your solution named SkiaSharp? Maybe VS is getting confused?",
        "source": "comment by mattleibow",
        "interpretation": "Maintainer immediately identified the likely cause."
      },
      {
        "text": "I had called the project I reported to you SkiaSharp.",
        "source": "reporter follow-up comment",
        "interpretation": "Reporter confirms the project name was 'SkiaSharp', fully explaining the NU1108 cycle detection."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/SkiaSharp.csproj",
        "finding": "No self-referential PackageReference or circular ProjectReference exists in the SkiaSharp binding project. The package definitions are clean — confirming NU1108 was not caused by the SkiaSharp package itself.",
        "relevance": "direct"
      }
    ],
    "workarounds": [
      "Rename the user project to something other than 'SkiaSharp' to avoid the NuGet name collision",
      "Clear NuGet cache with 'nuget locals all -clear' if a corrupted cache is suspected"
    ],
    "resolution": {
      "hypothesis": "User named their project 'SkiaSharp', which causes NuGet to detect a cyclic dependency when adding the SkiaSharp NuGet package to that project. Renaming the project resolves the issue.",
      "proposals": [
        {
          "title": "Rename the user project",
          "description": "Rename the consumer project to any name other than 'SkiaSharp'. NuGet matches project identity by name, so having a project and package with the same name triggers NU1108.",
          "category": "workaround",
          "confidence": 0.95,
          "effort": "cost/xs",
          "validated": "yes"
        }
      ],
      "recommendedProposal": "Rename the user project",
      "recommendedReason": "Root cause confirmed by reporter; trivial fix on the user's side."
    }
  },
  "output": {
    "actionability": {
      "suggestedAction": "close-as-not-a-bug",
      "confidence": 0.9,
      "reason": "Reporter confirmed the issue was caused by naming their project 'SkiaSharp', which conflicts with the NuGet package name. No bug exists in SkiaSharp. The issue was self-resolved once the root cause was understood."
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply question and build labels",
        "risk": "low",
        "confidence": 0.9,
        "labels": [
          "type/question",
          "area/Build"
        ]
      },
      {
        "type": "add-comment",
        "description": "Post a brief closing comment explaining the root cause and close",
        "risk": "high",
        "confidence": 0.9,
        "comment": "Thanks for following up! As confirmed in the thread, the NU1108 cycle error occurs because your project was named **SkiaSharp** — NuGet treats the project and the package as the same identity and reports a circular dependency.\n\nThe fix is to rename your project to any other name. This behavior is by-design in NuGet (not specific to SkiaSharp) and has been reported to Microsoft.\n\nClosing this as not a bug in SkiaSharp. Feel free to reopen if you encounter a different issue."
      },
      {
        "type": "close-issue",
        "description": "Close as not a bug — user error confirmed",
        "risk": "medium",
        "confidence": 0.88,
        "stateReason": "not_planned"
      }
    ]
  }
}
```

</details>
