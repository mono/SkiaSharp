# Release Notes Analysis Instructions

Read this entire file before starting your analysis. It contains the methodology for extracting,
categorizing, and classifying changes between two SkiaSharp git refs.

---

## Part 1: Diff Analysis Methodology

### C API Changes

Diff paths: `externals/skia/include/c/` and `externals/skia/src/c/`

Look for:
- **New functions**: Lines starting with `+` that contain function declarations (pattern: `sk_*`, `gr_*`)
- **Removed functions**: Lines starting with `-` with function declarations
- **Changed signatures**: Same function name but different parameters or return type
- **New types/enums**: New `typedef`, `enum`, or `struct` definitions

For each C API change, record:
- Function name and signature
- The header file it's in
- Whether it's new, changed, or removed

### C# Binding Changes

Diff path: `binding/SkiaSharp/`

Focus on **hand-written files** (not `*.generated.cs`). For each file changed:
- Look for new `public` classes, methods, properties, events
- Look for `[Obsolete]` attributes (new deprecations)
- Look for removed public members
- Look for changed method signatures

For generated files (`*.generated.cs`), just note the overall scope of changes (how many new
P/Invoke entries, new enum values, etc.) rather than listing each one.

### Git Log Analysis

For `git log REF1..REF2`:
- Extract PR numbers from commit messages (pattern: `(#NNNN)`)
- Classify each commit by its message prefix/content:
  - "Fix", "fix", "resolve", "close" → bug fix
  - "Add", "Implement", "Enable" → feature
  - "Update", "Bump", "Upgrade" → dependency
  - "doc", "README" → docs
  - Merge commits → extract the PR title
- Skip version bump commits ("Bump the version to...")
- Skip merge-only commits without meaningful content

### Dependency Changes

For `externals/skia/DEPS`:
- Parse the diff to extract old and new version/commit SHAs for each dependency
- Common deps: libpng, freetype, harfbuzz, libwebp, zlib, libexpat, libjpeg-turbo,
  vulkanmemoryallocator, libavif
- Note if a dependency was added or removed entirely

For `cgmanifest.json`:
- Similar to DEPS but tracks the cgmanifest component versions

### Native Build Changes

Diff path: `native/`

Look for:
- New platform targets (new `.cake` files, new build scripts)
- New architecture support (arm64, riscv64, loongarch64)
- Security flags (CFG, Spectre mitigation, buffer security check)
- Compiler/linker changes
- New build configurations

---

## Part 2: Classification Rules

### changeType

| Value | Criteria |
|-------|----------|
| `added` | New public API (class, method, property), new capability |
| `changed` | Existing public API modified in behavior or signature |
| `fixed` | Commit/PR explicitly fixes a bug (check message for "fix", "resolve", "close", linked issues) |
| `deprecated` | New `[Obsolete]` attribute added |
| `removed` | Public API or feature removed |
| `dependency` | Third-party library version bumped |
| `platform` | New platform/arch support, build flag changes, security hardening |
| `upstream` | Benefit from Skia engine upgrade — perf, rendering quality, codec improvement, reliability |

### importance

| Value | Criteria |
|-------|----------|
| `breaking` | Any change that could cause existing code to fail to compile or behave differently |
| `major` | New user-facing capability, new platform support, significant new API surface |
| `minor` | Quality improvement, small new API, non-breaking enhancement |
| `patch` | Bug fix, internal refactoring, CI/build changes, version bumps |

### Labels

Apply these freeform labels where relevant:
- `security` — Security fix, CVE patch, hardening
- `skia-upstream` — Change came from Skia submodule bump
- `breaking-change` — Always apply alongside `importance: "breaking"`
- `new-platform` — New architecture or platform support
- `performance` — Speed or memory improvement
- `deprecation` — API deprecated with `[Obsolete]`

---

## Part 3: Extracting Upstream Skia Benefits

When a Skia milestone bump is part of the diff, read Skia's RELEASE_NOTES.md for the milestone
range and extract "invisible benefits" — things users get automatically from the engine upgrade
without any SkiaSharp API changes.

**Categories to look for:**

| Category | What to extract | Example |
|----------|----------------|---------|
| **Rendering quality** | Better visual output from the same API calls | "Perlin noise shaders now properly rotate when transformed" (m124) |
| **Performance** | Faster rendering, lower memory usage | "Raster performance of Perlin noise significantly improved" (m124), "Canvas save/restore preallocation tunable" (m133) |
| **Color accuracy** | More correct color handling | "kRec709 transfer function corrected to match BT.1886 pure gamma 2.4" (m142) |
| **Codec improvements** | Better image decode/encode | "HDR metadata support in PNG" (m142), "MaxDecodeMemory protection" (m145) |
| **Reliability** | Crash fixes, error handling | "Vulkan device-lost callback" (m123), "SkSL reserved keyword rejection" (m128) |
| **HDR pipeline** | End-to-end HDR improvements | "skhdr::Metadata" (m142), "AGTM tone mapping" (m145) |

**What to SKIP:**
- New Skia APIs that SkiaSharp already wraps (those appear as `added` findings from the API diff)
- Graphite-only changes (SkiaSharp uses Ganesh/GL/Metal/Vulkan)
- Dawn backend changes
- Internal header reorganization
- Build system (GN flags, defines)
- Changes to APIs SkiaSharp doesn't expose

**How to write upstream findings:**
- `changeType`: always `"upstream"`
- `importance`: usually `"minor"` unless it's a behavior change that could affect output (`"major"`)
- `labels`: always include `"skia-upstream"`, add `"performance"` for perf items
- `skiaMilestone`: the milestone where the improvement was introduced
- `skiaFeature`: the upstream Skia API or feature name
- `description`: Focus on USER VALUE. Don't say "Skia changed X". Say "Apps that use X will
  see Y improvement. No code changes needed."
- `slideBullet`: Include for items that are exciting enough for a conference slide

---

## Part 3: Writing Marketing Slide Bullets

Each finding that's `major` importance or higher should have a `slideBullet` field. Guidelines:

- Start with a thematic emoji (🎨🖼️⚡🔒🌐🔧🐛📦🚀)
- **Bold** the feature name (max 5 words)
- Follow with a dash and one sentence of user value (max 20 words)
- Focus on what the user gains, not what we did internally

**Good:**
- 🌐 **LoongArch64 Support** — Build and run SkiaSharp natively on LoongArch64 Linux systems
- 🔒 **Spectre Mitigation** — Native DLLs hardened against speculative execution attacks
- ⚡ **Direct3D Backend** — Windows apps can render through Direct3D alongside OpenGL

**Bad:**
- Updated build scripts for loongarch64 target (too technical)
- Fix #3231 (no context)
- Enable CFG flag in MSVC (implementation detail)

---

## Part 4: Writing Migration Guides

For breaking changes, the `migrationGuide` field should contain markdown with:

1. Brief explanation of what changed and why
2. **Before** code block showing the old way
3. **After** code block showing the new way
4. Any edge cases or gotchas

Example:
```markdown
`SKPaint.TextSize` has been removed. Use `SKFont` for all text shaping.

### Before
\```csharp
var paint = new SKPaint { TextSize = 24 };
canvas.DrawText("Hello", 10, 50, paint);
\```

### After
\```csharp
var font = new SKFont(SKTypeface.Default, 24);
var paint = new SKPaint();
canvas.DrawText("Hello", 10, 50, font, paint);
\```
```

---

## Part 5: Output Format

Each agent should output a JSON file with this structure:

```json
{
  "agent": "api-changes|git-log|build-changes",
  "refFrom": "v3.116.1",
  "refTo": "v3.119.0",
  "findings": [
    {
      "name": "...",
      "changeType": "added|changed|fixed|...",
      "importance": "breaking|major|minor|patch",
      "description": "...",
      // ... other fields as applicable
    }
  ],
  "rawData": {
    // Agent-specific raw data for the synthesizer to use
  }
}
```

The synthesizer (Phase 4) will merge all agent outputs into the final unified report.

---

## Part 6: Tips for Accurate Analysis

- **Don't count version bump commits as features.** "Bump the version to X" is infrastructure.
- **Don't double-count.** A PR that adds a C API function AND a C# wrapper is ONE finding.
- **Submodule bumps are rich.** A single "Update externals" commit may contain dozens of upstream
  Skia changes. Unpack them.
- **Read PR descriptions.** They often contain context that commit messages lack.
- **Check for reverts.** If a feature was added then reverted in the same range, skip it.
- **Security changes matter.** CFG, Spectre, buffer security — these are user-facing even though
  they don't change the API surface.
- **Platform expansion is major.** New architecture support (riscv64, loongarch64) is a big deal
  for the affected communities.
- **Look for the actual diff, not just the message.** A commit message saying "Update externals"
  could contain significant API changes in the submodule.
