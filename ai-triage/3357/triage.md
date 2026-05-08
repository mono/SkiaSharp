# Issue Triage Report — #3357

| Field | Value |
|-------|-------|
| Repository | mono/SkiaSharp |
| Analyzed | 2026-04-21T18:36:17Z |
| Type | type/documentation (0.98 (98%)) |
| Area | area/Docs (0.98 (98%)) |
| Suggested action | needs-investigation (0.82 (82%)) |

**Issue Summary:** Maintainer task issue requesting AI agents to migrate API documentation from the docs folder into inline C# XML triple-slash doc comments, working through a migration checklist.

**Analysis:** This is a maintainer-authored task issue instructing AI agents to migrate existing API documentation from the docs/ folder into inline C# XML documentation comments across the binding/SkiaSharp/ source files. The docs/ directory is currently empty in the checkout, and the referenced migration guide/checklist (~/API.Docs.Migration.Guide.md, ~/API.Docs.Migration.Checklist.md) are expected to be provisioned externally by the workflow environment. Of 85 C# binding files, only 5 currently contain XML triple-slash doc comments.

**Recommendations:** **needs-investigation** — The migration source files (~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md) are not present in the current environment, and the docs/ directory is empty. The task is valid but requires the referenced files to be available before execution can proceed.

---

## Classification

| Field | Value |
|-------|-------|
| Type | type/documentation |
| Area | area/Docs |
| Platforms | — |
| Backends | — |
| Tenets | — |
| Partner | — |
| Current labels | area/Docs |

## Evidence

### Reproduction

**Environment:** Created by maintainer mattleibow on 2025-09-04. References ~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md.

## Analysis

### Technical Summary

This is a maintainer-authored task issue instructing AI agents to migrate existing API documentation from the docs/ folder into inline C# XML documentation comments across the binding/SkiaSharp/ source files. The docs/ directory is currently empty in the checkout, and the referenced migration guide/checklist (~/API.Docs.Migration.Guide.md, ~/API.Docs.Migration.Checklist.md) are expected to be provisioned externally by the workflow environment. Of 85 C# binding files, only 5 currently contain XML triple-slash doc comments.

### Rationale

The issue is unambiguously a documentation task created by the maintainer. No bug is reported, no behavior is wrong — this is a work assignment to migrate docs into C# source as XML triple-slash comments. Classification as type/documentation with area/Docs is definitive.

### Key Signals

- "Can you help migrate my docs from the docs folder into the c# source." — **issue body** (Explicit documentation migration task. No bug, no feature request.)
- "There is a guide here: ~/API.Docs.Migration.Guide.md · This is the checklist of APIs to migrate: ~/API.Docs.Migration.Checklist.md" — **issue body** (References workflow-provisioned files not committed to the repository.)
- "Each type should be migrated and committed separately." — **issue body** (Structured batch migration task with atomic per-type commits.)

### Code Investigation

| File | Lines | Relevance | Finding |
|------|-------|-----------|---------|
| `binding/SkiaSharp/` | — | direct | 85 C# source files exist in the binding directory. Only 5 files contain any /// XML doc comments (SKCanvas.cs, SKObject.cs, SKTypeface.cs, HandleDictionary.cs, and a few others). The vast majority of public API surface lacks triple-slash documentation. |
| `docs/` | — | direct | The docs/ directory is empty in the current checkout (0 files). The source documentation that was to be migrated is not present, likely populated by a separate workflow step before the agent runs. |
| `binding/SkiaSharp/SKObject.cs` | 337-362 | context | A few internal interface types (ISKReferenceCounted, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration) have XML summary comments showing the intended doc format for the codebase. |

### Next Questions

- Are ~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md provisioned by a separate workflow job before this issue is processed?
- Is there a prior docs/ folder populated from a different branch or artifact that should be checked out?

## Recommendations

### Actionability

| Field | Value |
|-------|-------|
| Suggested action | needs-investigation |
| Confidence | 0.82 (82%) |
| Reason | The migration source files (~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md) are not present in the current environment, and the docs/ directory is empty. The task is valid but requires the referenced files to be available before execution can proceed. |
| Suggested repro platform | linux |

### Automatable Actions

| Type | Risk | Confidence | Description | Details |
|------|------|------------|-------------|---------|
| update-labels | low | 0.98 (98%) | Apply type/documentation and area/Docs labels (area/Docs already present) | labels=type/documentation, area/Docs |
| add-comment | medium | 0.82 (82%) | Note that migration source files are not found in current environment | — |

**Comment draft for `add-comment`:**

```markdown
This documentation migration task references `~/API.Docs.Migration.Guide.md` and `~/API.Docs.Migration.Checklist.md`. In the current environment the `docs/` directory is empty and those files are not present at `~/`. The migration can proceed once the workflow provisions those files. The binding folder currently has 85 C# files with sparse XML doc coverage — only ~5 files have any triple-slash comments.
```

<details>
<summary>Raw JSON</summary>

```json
{
  "meta": {
    "schemaVersion": "1.0",
    "number": 3357,
    "repo": "mono/SkiaSharp",
    "analyzedAt": "2026-04-21T18:36:17Z",
    "currentLabels": [
      "area/Docs"
    ]
  },
  "summary": "Maintainer task issue requesting AI agents to migrate API documentation from the docs folder into inline C# XML triple-slash doc comments, working through a migration checklist.",
  "classification": {
    "type": {
      "value": "type/documentation",
      "confidence": 0.98
    },
    "area": {
      "value": "area/Docs",
      "confidence": 0.98
    }
  },
  "evidence": {
    "reproEvidence": {
      "environmentDetails": "Created by maintainer mattleibow on 2025-09-04. References ~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md.",
      "repoLinks": []
    }
  },
  "analysis": {
    "summary": "This is a maintainer-authored task issue instructing AI agents to migrate existing API documentation from the docs/ folder into inline C# XML documentation comments across the binding/SkiaSharp/ source files. The docs/ directory is currently empty in the checkout, and the referenced migration guide/checklist (~/API.Docs.Migration.Guide.md, ~/API.Docs.Migration.Checklist.md) are expected to be provisioned externally by the workflow environment. Of 85 C# binding files, only 5 currently contain XML triple-slash doc comments.",
    "rationale": "The issue is unambiguously a documentation task created by the maintainer. No bug is reported, no behavior is wrong — this is a work assignment to migrate docs into C# source as XML triple-slash comments. Classification as type/documentation with area/Docs is definitive.",
    "keySignals": [
      {
        "text": "Can you help migrate my docs from the docs folder into the c# source.",
        "source": "issue body",
        "interpretation": "Explicit documentation migration task. No bug, no feature request."
      },
      {
        "text": "There is a guide here: ~/API.Docs.Migration.Guide.md · This is the checklist of APIs to migrate: ~/API.Docs.Migration.Checklist.md",
        "source": "issue body",
        "interpretation": "References workflow-provisioned files not committed to the repository."
      },
      {
        "text": "Each type should be migrated and committed separately.",
        "source": "issue body",
        "interpretation": "Structured batch migration task with atomic per-type commits."
      }
    ],
    "codeInvestigation": [
      {
        "file": "binding/SkiaSharp/",
        "finding": "85 C# source files exist in the binding directory. Only 5 files contain any /// XML doc comments (SKCanvas.cs, SKObject.cs, SKTypeface.cs, HandleDictionary.cs, and a few others). The vast majority of public API surface lacks triple-slash documentation.",
        "relevance": "direct"
      },
      {
        "file": "docs/",
        "finding": "The docs/ directory is empty in the current checkout (0 files). The source documentation that was to be migrated is not present, likely populated by a separate workflow step before the agent runs.",
        "relevance": "direct"
      },
      {
        "file": "binding/SkiaSharp/SKObject.cs",
        "lines": "337-362",
        "finding": "A few internal interface types (ISKReferenceCounted, ISKNonVirtualReferenceCounted, ISKSkipObjectRegistration) have XML summary comments showing the intended doc format for the codebase.",
        "relevance": "context"
      }
    ],
    "nextQuestions": [
      "Are ~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md provisioned by a separate workflow job before this issue is processed?",
      "Is there a prior docs/ folder populated from a different branch or artifact that should be checked out?"
    ]
  },
  "output": {
    "actionability": {
      "suggestedAction": "needs-investigation",
      "confidence": 0.82,
      "reason": "The migration source files (~/API.Docs.Migration.Guide.md and ~/API.Docs.Migration.Checklist.md) are not present in the current environment, and the docs/ directory is empty. The task is valid but requires the referenced files to be available before execution can proceed.",
      "suggestedReproPlatform": "linux"
    },
    "actions": [
      {
        "type": "update-labels",
        "description": "Apply type/documentation and area/Docs labels (area/Docs already present)",
        "risk": "low",
        "confidence": 0.98,
        "labels": [
          "type/documentation",
          "area/Docs"
        ]
      },
      {
        "type": "add-comment",
        "description": "Note that migration source files are not found in current environment",
        "risk": "medium",
        "confidence": 0.82,
        "comment": "This documentation migration task references `~/API.Docs.Migration.Guide.md` and `~/API.Docs.Migration.Checklist.md`. In the current environment the `docs/` directory is empty and those files are not present at `~/`. The migration can proceed once the workflow provisions those files. The binding folder currently has 85 C# files with sparse XML doc coverage — only ~5 files have any triple-slash comments."
      }
    ]
  }
}
```

</details>
