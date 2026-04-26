---
name: release-notes-writer
description: >
  Generate release notes for SkiaSharp by diffing two git refs (tags, SHAs, or branches). Produces
  both marketing-style slide bullets and a detailed technical changelog, backed by structured JSON
  with schema validation and an interactive HTML viewer. Use this skill whenever the user asks to
  "write release notes", "generate changelog", "what changed between versions", "diff two tags",
  "what shipped in this release", "summarize changes since v3.x", "prepare release announcement",
  "what's new in SkiaSharp", "changelog for v3.x", or any request to compare two SkiaSharp versions.
  Also use proactively when the user mentions a release branch, finishes a release cycle, or asks
  what went into a specific version. This is the companion to skia-feature-scout — that skill looks
  at what SkiaSharp is *missing* from upstream Skia; this skill looks at what SkiaSharp actually
  *shipped* between two refs.
---

# Release Notes Writer

You generate release notes for SkiaSharp by analyzing the git diff between two refs (tags, SHAs, or
branch names). You produce two output formats — marketing slides for conferences/announcements and a
detailed changelog for developers — backed by structured JSON.

## Why This Matters

When SkiaSharp ships a new version, users and maintainers need to know what changed. The git log has
all the information but it's scattered across commits, PRs, and submodule bumps. This skill
synthesizes everything into coherent release notes that serve two audiences:
1. **Marketing** — conference slides, blog posts, announcements (user value, not implementation)
2. **Developer** — exact API changes, migration guides, dependency bumps (technical precision)

## Key References

- **[references/schema-cheatsheet.md](references/schema-cheatsheet.md)** — Human-readable JSON output schema
- **[references/release-notes-schema.json](references/release-notes-schema.json)** — JSON Schema (Draft 2020-12) for machine validation
- **[references/analysis-instructions.md](references/analysis-instructions.md)** — Detailed instructions for diff analysis agents

## Workflow

```
Phase 1: Setup (resolve refs, establish diff range)
Phase 2: Gather data (parallel agents analyze different layers)
  ├─ Agent 1: C API diff + C# binding diff
  ├─ Agent 2: Git log, PRs, issues
  └─ Agent 3: Native build / dependency / submodule changes
Phase 3: Cross-reference with Skia milestones (if submodule bumped)
Phase 4: Synthesize — merge, categorize, deduplicate
Phase 5: Generate outputs (JSON → validate → HTML → markdown)
Phase 6: Present results
```

### Phase 1: Setup

**1a. Resolve refs**

The user provides two refs. These can be tags (`v3.116.1`, `v3.119.0`), SHAs, or branch names.
Validate both refs exist:

```bash
git rev-parse --verify "$REF1" >/dev/null 2>&1 || echo "❌ Invalid ref: $REF1"
git rev-parse --verify "$REF2" >/dev/null 2>&1 || echo "❌ Invalid ref: $REF2"
```

If the user only provides one ref, ask what to diff against. Common patterns:
- "What changed in v3.119.0?" → diff `v3.118.0..v3.119.0` (previous stable tag)
- "What's new since the last release?" → diff last tag to HEAD
- "Summarize this PR" → diff the PR's base..head

**1b. Establish context**

```bash
# Count commits in range
git log "$REF1..$REF2" --oneline | wc -l

# Check if submodule was bumped
git diff "$REF1..$REF2" -- externals/skia 2>/dev/null | head -5

# Get date range
git log "$REF1" -1 --format=%ci
git log "$REF2" -1 --format=%ci
```

### Phase 2: Gather Data

Launch **three** parallel background agents to analyze different layers of changes. Each agent
should read [references/analysis-instructions.md](references/analysis-instructions.md) first.

**Agent 1: API Changes** (general-purpose, background)

```
Analyze the C API and C# binding changes between $REF1 and $REF2 in the SkiaSharp repo.

FIRST: Read .agents/skills/release-notes-writer/references/analysis-instructions.md

THEN:
1. C API diff: git diff $REF1..$REF2 -- externals/skia/include/c/ externals/skia/src/c/
   - New functions (added lines with function signatures)
   - Changed functions (modified signatures or implementations)
   - Removed functions
2. C# binding diff: git diff $REF1..$REF2 -- binding/SkiaSharp/
   - New public classes, methods, properties
   - Changed signatures or behavior
   - Removed or deprecated APIs ([Obsolete] markers)
   - Exclude *.generated.cs — note them but focus on hand-written code
3. Generated binding changes: git diff $REF1..$REF2 -- binding/SkiaSharp/SkiaApi.generated.cs
   - New P/Invoke entries (new native functions exposed)

Save findings as JSON to /tmp/release-notes-api-changes.json
```

**Agent 2: Git Log & PRs** (general-purpose, background)

```
Analyze the git log between $REF1 and $REF2 in the SkiaSharp repo.

FIRST: Read .agents/skills/release-notes-writer/references/analysis-instructions.md

THEN:
1. Full commit log: git log $REF1..$REF2 --oneline --no-merges
2. Merge commits (PRs): git log $REF1..$REF2 --merges --oneline
3. For each PR number found in commit messages (#NNNN):
   - Get title and labels: gh pr view NNNN --repo mono/SkiaSharp --json title,labels,body
4. Search for linked issues in PR bodies
5. Categorize commits: bug fix, feature, build/CI, docs, dependency update

Save findings as JSON to /tmp/release-notes-git-log.json
```

**Agent 3: Build & Dependencies** (general-purpose, background)

```
Analyze native build and dependency changes between $REF1 and $REF2 in the SkiaSharp repo.

FIRST: Read .agents/skills/release-notes-writer/references/analysis-instructions.md

THEN:
1. DEPS changes: git diff $REF1..$REF2 -- externals/skia/DEPS
   - Identify dependency version bumps (libpng, freetype, harfbuzz, etc.)
2. cgmanifest.json changes: git diff $REF1..$REF2 -- cgmanifest.json
3. Build flag changes: git diff $REF1..$REF2 -- native/
   - New platforms, architecture support
   - Security flags (CFG, Spectre, etc.)
4. Skia submodule bump: git diff $REF1..$REF2 -- externals/skia
   - If bumped, identify old and new Skia commits
   - Check Skia RELEASE_NOTES.md for milestone features that landed

Save findings as JSON to /tmp/release-notes-build-changes.json
```

### Phase 3: Extract Skia Upstream Benefits

This phase is **critical** whenever the Skia submodule was bumped. A Skia milestone bump is often
the most valuable part of a release — even when SkiaSharp adds zero new APIs, the engine upgrade
delivers rendering improvements, codec fixes, performance wins, and reliability gains that users
get automatically.

**The key insight:** SkiaSharp controls the *API surface*. Skia controls the *engine*. When Skia
improves gradient rendering, fixes a PNG decoder bug, or optimizes path rasterization, SkiaSharp
users benefit without any managed code changes. These "invisible benefits" are the main reason to
bump Skia, and they deserve prominent placement in release notes.

If the Skia submodule was bumped between the two refs:

1. Identify the old and new Skia milestone numbers from commit messages or SkMilestone.h
2. Fetch Skia's RELEASE_NOTES.md:
   ```
   https://raw.githubusercontent.com/google/skia/main/RELEASE_NOTES.md
   ```
3. Read **every milestone** in the range (e.g., m120 through m133)
4. For each milestone, extract items in these categories — these are the things users get
   "for free" from the engine upgrade:

**What to look for (and examples from real Skia release notes):**

| Category | What it means for users | Examples |
|----------|------------------------|----------|
| **Rendering quality** | Visual output is better | Perlin noise now rotates properly (m124), mipmap sharpening on by default (m131), gradient interpolation with wide-gamut color spaces like P3/Rec2020 (m134) |
| **Color accuracy** | Colors are more correct | kRec709 transfer function corrected to BT.1886 (m142), kHLG/kPQ use new skcms representations (m141) |
| **Codec/format improvements** | Image decode/encode is better | Rust PNG encoder/decoder promoted to official API (m142), HDR metadata in PNG (m142), hasHighBitDepthEncodedData (m132), MaxDecodeMemory protection (m145), Exif orientation respected in getImage (m123) |
| **Performance** | Faster rendering or lower memory | Perlin noise raster perf "significantly improved" (m124), canvas save/restore preallocation tuning (m133), pipeline precompilation improvements (m133-135) |
| **Reliability/safety** | Fewer crashes, better error handling | Vulkan device-lost callback (m123), SkSL reserved keyword rejection (m128), codec memory limit protection (m145) |
| **HDR pipeline** | Better HDR support end-to-end | skhdr::Metadata (m142), AGTM tone mapping (m145), HDR PNG metadata (m142), CICP color spaces (m133) |

**What to skip:**
- API additions that SkiaSharp already exposes (those are in the API diff findings)
- Graphite-only changes (SkiaSharp uses Ganesh)
- Dawn backend changes
- Internal refactoring (header moves, GN flag changes)
- Build system changes

5. For each extracted benefit, create a finding with `changeType: "upstream"` and connect
   it to user value — don't say "Skia changed kRec709", say "Colors using the Rec.709
   transfer function are now more accurate, matching the BT.1886 standard. Apps displaying
   video-sourced content may see subtle color improvements."

6. Separate the "upstream" findings clearly — these go in their own section in both the
   slides and changelog. The narrative is: "By upgrading to Skia m133, you automatically
   get these improvements without changing any code."

### Phase 4: Synthesize

Merge findings from all three agents into a unified findings list. This is the quality gate.

1. **Deduplicate** — the same change may appear in the API diff and the git log. Merge them,
   keeping the richer data from each.
2. **Categorize** each finding using `changeType`:
   - `added` — new API, class, method, or capability
   - `changed` — modified behavior or signature
   - `fixed` — bug fix (look for "fix", "resolve", "close" in commit messages)
   - `deprecated` — new `[Obsolete]` markers
   - `removed` — removed APIs or features
   - `dependency` — dependency version bumps
   - `platform` — new platform support, build flag changes
   - `upstream` — benefit from Skia engine upgrade (perf, quality, codec, reliability)
3. **Assign importance**: `breaking`, `major`, `minor`, `patch`
   - `breaking` — removed APIs, changed signatures, behavior changes that affect existing code
   - `major` — new APIs, significant new capabilities
   - `minor` — enhancements, quality improvements
   - `patch` — bug fixes, internal improvements
4. **Link everything** — every finding should reference at least one PR or commit SHA
5. **Breaking changes get special treatment**: for each breaking change, write a migration
   guide showing before/after code

### Phase 5: Generate Outputs

**5a. Generate JSON Report**

Produce JSON following the schema in [references/schema-cheatsheet.md](references/schema-cheatsheet.md).
Save to the artifacts directory as `release-notes-REF1-REF2.json` (using short ref names).

The JSON includes:
- `meta` — refs, dates, commit count, repo
- `summary` — counts by changeType and importance
- `findings` — every change with full details
- `slides` — marketing-format bullet points (pre-rendered)
- `changelog` — detailed changelog markdown (pre-rendered)

**5b. Validate JSON Report**

> 🛑 **MANDATORY:** Always validate before rendering.

```bash
python3 .agents/skills/release-notes-writer/scripts/validate-release-notes.py <path-to-json>
```

Exit codes: 0=valid, 1=fixable (regenerate), 2=fatal.

**5c. Render HTML Report**

> 🛑 **MANDATORY:** Always generate the HTML report.

```bash
python3 .agents/skills/release-notes-writer/scripts/render-release-notes.py <path-to-json>
```

This produces a self-contained HTML file with the interactive viewer.

**5d. Persist**

```bash
python3 .agents/skills/release-notes-writer/scripts/persist-release-notes.py <path-to-json>
```

This copies the validated JSON to `output/ai/repos/mono-SkiaSharp/ai-release-notes/` and
generates sidecar `.md` and `.html` files.

### Phase 6: Present Results

Show the user both formats inline:

**Marketing Slides:**
```
## What's New in SkiaSharp v3.119.0

🎨 **Custom Mesh Drawing** — Draw vertex meshes with custom SkSL shaders for particles, ...
🖼️ **HDR Image Support** — Read and write HDR metadata in PNG images ...
⚡ **30% faster gradient rendering** — Perlin noise and gradient shaders optimized ...
🔒 **Security hardening** — Control Flow Guard and Spectre mitigations enabled ...
```

**Changelog Preview** (abbreviated, full version in .md file):
```
### ⚠️ Breaking Changes
- None in this release

### ✨ New APIs
- `SKCanvas.DrawMesh()` — Draw custom vertex meshes (#3123)
  - New types: SKMesh, SKMeshSpecification
  ...

### 🐛 Bug Fixes
- Fix SKImage.FromPicture implementation (#3231)
  ...
```

Then offer next steps:
1. "Want me to refine the marketing bullets for a specific audience?"
2. "Should I create a GitHub release draft with these notes?"
3. "Want to see more detail on any specific change?"

---

## Slide Writing Guidelines

Marketing slides should be:
- **Short** — one line per bullet, 15 words max for the bold part
- **Exciting** — use emoji, active voice, user-facing value
- **Grouped by theme** — not by PR number or commit
- **Non-technical** — "faster rendering" not "optimized shader pipeline dispatch"

Group by themes like:
- 🎨 Rendering & Drawing
- 🖼️ Image & Codec
- ⚡ Performance
- 🔒 Security
- 🌐 Platform Support
- 🔧 Developer Experience
- 🚀 Engine Improvements (what you get from the Skia upgrade, no code changes needed)

## Changelog Writing Guidelines

The detailed changelog should be:
- **Precise** — exact method names, PR numbers, affected types
- **Grouped by change type** — Breaking Changes first (always, even if empty)
- **Migration-ready** — breaking changes include before/after code
- **Complete** — every public-facing change mentioned

Ordering within the changelog:
1. ⚠️ Breaking Changes (always show, even if "None")
2. ✨ New APIs
3. 🐛 Bug Fixes
4. ⚡ Performance
5. 🔒 Security
6. 🚀 Engine Improvements (upstream Skia benefits — rendering, codecs, color, reliability)
7. ⚠️ Deprecations
8. 📦 Dependencies
9. 🌐 Platform Changes
10. 📝 Other
