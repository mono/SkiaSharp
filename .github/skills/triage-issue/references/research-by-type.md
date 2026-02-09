# Research by Issue Type

Shared and type-specific research guidance. Read after classifying `type` in Phase 3.

## Shared Research (All Types)

Always do these regardless of type:

1. **Search for duplicates** — `$CACHE/ai-triage/` for existing triages; search GitHub issues for similar titles/symptoms
2. **Check issue age and activity** — Time since last comment, whether OP responded to requests, whether issue is abandoned
3. **Extract all evidence** — Screenshots, code snippets, stack traces, environment details, attachments, repo links
4. **Identify SkiaSharp version** — Check for version mentions, NuGet package references, csproj contents in repro code
5. **Check for member/maintainer comments** — These often contain diagnosis or workarounds already

## Bug Research

**Focus:** Root cause hypothesis and workarounds.

**Where to search:**
- Stack traces → map to SkiaSharp source (`binding/SkiaSharp/`, `externals/skia/src/c/`)
- [documentation/packages.md](../../documentation/packages.md) → deployment/loading issues (DllNotFoundException, container, publishing)
- [references/skia-patterns.md](skia-patterns.md) → known platform quirks, common traps, diagnostic heuristics
- `.docs/docs/docs/` → if bug involves API misuse vs actual defect
- Similar closed issues → check if there's an existing fix or documented workaround

**Proposal archetypes:**
1. Workaround the user can apply now
2. Fix in SkiaSharp code (with steps and effort estimate)
3. Alternative approach that avoids the problem entirely

**Key risks:** Don't confuse "user is using API wrong" with "API is broken". Check docs first.

## Question Research

**Focus:** Find the answer. The goal is a complete, ready-to-post response.

**Where to search:**
- `docs/SkiaSharpAPI/*.xml` → API reference for the types/methods involved
- `.docs/docs/docs/` → tutorials, guides, samples that answer the question
- [documentation/packages.md](../../documentation/packages.md) → if about packages, deployment, or platform selection
- `mslearn`/`microsoft_docs_search` → if about non-SkiaSharp tech (MAUI, Blazor, WPF, ASP.NET)
- Existing closed issues → often the same question was answered before

**Proposal archetypes:**
1. Direct answer with code example
2. Alternative approach if the "obvious" answer has caveats
3. Pointer to existing documentation/sample if comprehensive answer exists

**Key risks:** Don't just say "check the docs" — find the specific answer and include it.

## Feature / Enhancement Research

**Focus:** Existing alternatives and feasibility assessment.

**Where to search:**
- SkiaSharp API surface → does a partial solution already exist?
- Upstream Skia → does the C++ API support this? (`externals/skia/include/`)
- [documentation/architecture.md](../../documentation/architecture.md) → understand which layer would need changes
- Similar issues/PRs → has this been requested or attempted before?

**Proposal archetypes:**
1. Existing workaround using current API
2. What an implementation would look like (scope, effort, which layers)
3. Alternative library/approach if out of SkiaSharp's scope

**Key risks:** Don't promise features. Frame proposals as "here's what would be involved" not "we'll add this."

## Documentation Research

**Focus:** Draft the missing documentation or find what exists.

**Where to search:**
- `docs/SkiaSharpAPI/*.xml` → current API docs (may have "To be added" placeholders)
- `.docs/docs/docs/` → existing tutorials and guides
- `documentation/` → project-level docs
- Source code → if docs are missing, read the implementation to draft accurate docs

**Proposal archetypes:**
1. Draft documentation the maintainer can use directly
2. Code sample that demonstrates the undocumented feature
3. Pointer to where the docs gap exists and what should be added

**Key risks:** Don't generate inaccurate API docs. Verify against source code if unsure.
