# Release Guide

How to release SkiaSharp: create branch → wait for CI → test → publish → tag.

## ⚠️ NO UNDO WARNING

**Tags and releases cannot be deleted.** Once a tag is pushed or a release is published, it's permanent. Each skill confirms before destructive operations - always review carefully before proceeding.

- Wrong tag pushed → Cannot delete, must create new release
- Wrong version published to NuGet.org → Cannot unpublish, must release new version
- Branch deleted prematurely → May lose CI artifacts

## Skills

The release process is handled by three skills in order:

| Step | Skill | Purpose | Trigger |
|------|-------|---------|---------|
| 1 | [release-branch](../.github/skills/release-branch/SKILL.md) | Create release branch, trigger CI | "release now", "release X.Y.Z" |
| 2 | [release-testing](../.github/skills/release-testing/SKILL.md) | Test packages before publishing | "test the release", "continue" |
| 3 | [release-publish](../.github/skills/release-publish/SKILL.md) | Publish to NuGet.org, tag, finalize | "publish X.Y.Z", "finalize" |

Each skill confirms with `ask_user` before executing destructive operations.

---

## Reference Tables

### Version Patterns

| Release Type | Version Format | Branch | NuGet Pattern | Tag |
|--------------|----------------|--------|---------------|-----|
| Preview | `X.Y.Z-preview.N` | `release/X.Y.Z-preview.N` | `X.Y.Z-preview.N.{build}` | `vX.Y.Z-preview.N.{build}` |
| Stable | `X.Y.Z` | `release/X.Y.Z` | `X.Y.Z-stable.{build}` | `vX.Y.Z` |
| Hotfix Preview | `X.Y.Z.F-preview.N` | `release/X.Y.Z.F-preview.N` | `X.Y.Z.F-preview.N.{build}` | `vX.Y.Z.F-preview.N.{build}` |
| Hotfix Stable | `X.Y.Z.F` | `release/X.Y.Z.F` | `X.Y.Z.F-stable.{build}` | `vX.Y.Z.F` |

The `{build}` number is auto-assigned by CI.

### Release Type → Base Branch

| Type | Base | PREVIEW_LABEL |
|------|------|---------------|
| Preview | `main` | `preview.N` |
| Stable | `release/X.Y.Z-preview.{latest}` | `stable` |
| Hotfix Preview | tag `vX.Y.Z` | `preview.N` |
| Hotfix Stable | `release/X.Y.Z.F-preview.{latest}` | `stable` |

### HarfBuzzSharp Versioning

HarfBuzzSharp uses 4-digit versions: `X.Y.Z.N`

| Digits | Meaning |
|--------|---------|
| X.Y.Z | Native HarfBuzz version (e.g., `8.3.1`) |
| N | Incremented with each SkiaSharp release |

**Why 4 digits?** HarfBuzzSharp packages are released with SkiaSharp even when there are no HarfBuzz changes. The 4th digit keeps them in sync.

**When native HarfBuzz upgrades:** Reset to 3-digit version (e.g., `8.3.1.4` → `8.4.0`).

### Feeds

| Feed | URL | Purpose |
|------|-----|---------|
| Preview | `https://aka.ms/skiasharp-eap/index.json` | CI builds, testing |
| Stable | NuGet.org | Public releases |

### Pipelines

| Pipeline | Purpose |
|----------|---------|
| [Main Build](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=27373) | Builds + auto-publishes to preview feed |
| [NuGet.org Publish](https://dev.azure.com/devdiv/DevDiv/_build?definitionId=25298) | Publishes to NuGet.org (manual trigger) |

---

## Workflow Diagrams

### Stage 1: Preparation (release-branch skill)

```mermaid
flowchart TB
    START([User requests release]) --> PROVIDED{Version provided?}
    
    PROVIDED -->|Yes| PARSE
    PROVIDED -->|No| AUTO
    
    AUTO["Auto-detect Version
    ∙ Read SKIASHARP_VERSION from main
    ∙ Check existing release branches
    ∙ Calculate next preview number"]
    
    AUTO --> CONFIRM{User confirms?}
    CONFIRM -->|No| ABORT([Abort])
    CONFIRM -->|Yes| PARSE
    
    PARSE["Parse Version String"] --> TYPE{Release type?}
    
    TYPE -->|Preview| BASE_MAIN["Base: main"]
    TYPE -->|Stable| BASE_PREVIEW["Base: preview branch"]
    TYPE -->|Hotfix Preview| BASE_TAG["Base: tag"]
    TYPE -->|Hotfix Stable| BASE_HOTFIX["Base: hotfix preview"]
    
    BASE_PREVIEW --> EXISTS{Base exists?}
    BASE_TAG --> EXISTS
    BASE_HOTFIX --> EXISTS
    EXISTS -->|No| ERROR([Error])
    
    BASE_MAIN --> CREATE
    EXISTS -->|Yes| CREATE
    
    CREATE["Create Branch
    ∙ Checkout base
    ∙ Create release branch
    ∙ Set PREVIEW_LABEL
    ∙ Commit and push"]
    
    CREATE --> CI([CI Build Started])
    
    CI --> IS_PREVIEW{Preview from main?}
    IS_PREVIEW -->|No| DONE([Done - wait 2-4 hours])
    IS_PREVIEW -->|Yes| BUMP
    
    BUMP["Bump Main Version
    ∙ Edit SKIASHARP_VERSION
    ∙ Edit VERSIONS.txt
    ∙ Increment HarfBuzzSharp
    ∙ Create and merge PR"]
    
    BUMP --> DONE

    classDef error fill:#ffebee,stroke:#c62828
    classDef endpoint fill:#f3e5f5,stroke:#7b1fa2
    class ABORT,ERROR error
    class START,CI,DONE endpoint
```

### Stage 2: Testing (release-testing skill)

```mermaid
flowchart TB
    START([CI Build Complete]) --> RESOLVE
    
    RESOLVE["Resolve Package Versions
    ∙ Fetch release branch
    ∙ Read VERSIONS.txt (both packages)
    ∙ Search preview feed
    ∙ Pick latest build"]
    
    RESOLVE --> FOUND{Packages found?}
    FOUND -->|No| WAIT([Wait - CI not done])
    FOUND -->|Yes| REPORT[Report versions to user]
    
    REPORT --> STABLE{Stable release?}
    STABLE -->|No| TESTS
    STABLE -->|Yes| SOURCE{Test source?}
    
    SOURCE -->|Preview feed| TESTS
    SOURCE -->|Local artifacts| SETUP
    
    SETUP["Setup Local Testing
    ∙ Create local nuget.config
    ∙ Clear NuGet cache"]
    
    SETUP --> TESTS
    
    TESTS["Run Integration Tests
    ∙ Console, Blazor, MAUI
    ∙ iOS, Android, Mac, Windows"]
    
    TESTS --> RESULT{All pass?}
    RESULT -->|No| FIX([Fix and retest])
    RESULT -->|Yes| CHECK{Stable release?}
    
    CHECK -->|No| READY([Ready for publish])
    CHECK -->|Yes| CHECKLIST
    
    CHECKLIST["Stable Checklist
    ∙ Verify packages
    ∙ Check native assets
    ∙ Validate metadata"]
    
    CHECKLIST --> OK{Passes?}
    OK -->|No| FIX2([Fix issues])
    OK -->|Yes| READY

    classDef error fill:#ffebee,stroke:#c62828
    classDef endpoint fill:#f3e5f5,stroke:#7b1fa2
    class WAIT,FIX,FIX2 error
    class START,READY endpoint
```

### Stage 3: Publishing (release-publish skill)

```mermaid
flowchart TB
    START([Tests Passed]) --> REQ{Publish to NuGet.org?}
    
    REQ -->|Stable - Required| PUBLISH
    REQ -->|Preview| SKIP{Skip NuGet.org?}
    SKIP -->|Yes| TAG
    SKIP -->|No| PUBLISH
    
    PUBLISH["Publish to NuGet.org
    ∙ Trigger publish pipeline
    ∙ Wait for completion
    ∙ Verify packages visible"]
    
    PUBLISH --> STATUS{Success?}
    STATUS -->|No| FAILED([Fix and retry])
    STATUS -->|Yes| TAG
    
    TAG["Create Git Tag
    ∙ Preview: vX.Y.Z-preview.N.build
    ∙ Stable: vX.Y.Z
    ∙ Push tag to origin"]
    
    TAG --> RELEASE
    
    RELEASE["Create GitHub Release
    ∙ Set title and notes
    ∙ Mark pre-release if preview
    ∙ Attach samples if stable"]
    
    RELEASE --> FINAL{Stable release?}
    FINAL -->|No| DONE([Complete])
    FINAL -->|Yes| MILESTONE[Close GitHub milestone]
    MILESTONE --> DONE

    classDef error fill:#ffebee,stroke:#c62828
    classDef endpoint fill:#f3e5f5,stroke:#7b1fa2
    class FAILED error
    class START,DONE endpoint
```

---

## Related Documentation

- [Versioning](versioning.md) — Version numbering scheme explanation
