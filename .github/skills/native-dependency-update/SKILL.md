---
name: native-dependency-update
description: >
  Update native dependencies (libpng, libexpat, zlib, libwebp, harfbuzz, freetype, libjpeg-turbo, etc.)
  in SkiaSharp's Skia fork. Handles security CVE fixes, bug fixes, and version bumps.
  
  Use when user asks to:
  - Bump/update a native dependency (libpng, zlib, expat, webp, etc.)
  - Fix a CVE or security vulnerability in a native library
  - Update Skia's DEPS file
  - Check what version of a dependency is currently used
  - Analyze breaking changes between dependency versions
  
  Triggers: "bump libpng", "update zlib", "fix CVE in expat", "update native deps",
  "what version of libpng", "check for breaking changes", "update DEPS".
---

# Native Dependency Update Skill

Update native dependencies in SkiaSharp's Skia fork (mono/skia).

## Architecture

```
SkiaSharp repo
└── externals/skia (mono/skia fork, submodule)
    ├── DEPS (pins dependencies to commit hashes)
    └── third_party/
        ├── {dep}/BUILD.gn (build config)
        └── externals/{dep}/ (source checkout)
```

**Key insight:** Changes go to mono/skia first, then SkiaSharp updates its submodule.

## Workflow

### 1. Find Current Version

Check `externals/skia/DEPS` for the dependency. Format:
```
"third_party/externals/{name}": "{mirror_url}@{commit_hash}",
```

Navigate to `externals/skia/third_party/externals/{name}` and inspect the commit to determine the current version. Each project has different tagging conventions—explore before assuming.

### 2. Find Target Version

- **For CVE fixes:** Check the advisory for the fixed version
- **For bug fixes:** Check upstream changelog/releases
- **For general updates:** Check what upstream Google Skia uses, or use latest stable

**Getting the commit hash:** Use `git rev-parse {tag}^{commit}` (not just `git rev-parse {tag}`) to handle annotated tags correctly. The DEPS file requires full 40-character commit hashes.

### 3. Analyze Breaking Changes

Compare current and target versions for:
- **Changelog entries** mentioning breaking changes or API changes
- **New source files** that may need adding to BUILD.gn
- **Deleted source files** that would break the build
- **API changes** in headers (added/removed/changed functions)

Risk assessment:
- Security-only releases → usually safe, no breaking changes
- Patch versions (1.2.3 → 1.2.4) → typically safe
- Minor versions (1.2 → 1.3) → read changelog carefully
- Major versions (1.x → 2.x) → expect breaking changes

👉 **See [references/breaking-changes.md](references/breaking-changes.md)** for detailed analysis guidance.

### 4. Update and Build

1. Edit `externals/skia/DEPS` with the new commit hash

2. If needed, update `externals/skia/third_party/{dep}/BUILD.gn` (rare—only when new source files are added to the library)

3. Build one platform/arch to verify (from repo root):
   ```bash
   dotnet cake --target=externals-macos --arch=arm64   # or x64
   dotnet cake --target=externals-windows --arch=x64  
   dotnet cake --target=externals-linux --arch=x64
   ```

4. Run tests via Cake (handles platform-specific skip traits correctly):
   ```bash
   dotnet cake --target=tests-netcore --skipExternals=all
   ```

**Tip:** Use `--arch` to build only one architecture (arm64, x64, arm, x86). Always use `dotnet cake --target=tests-netcore` for testing—it properly filters tests that should be skipped on each platform. Running `dotnet test` directly will cause crashes from tests that throw exceptions expecting to be skipped.

**If build fails:** Check compiler errors for missing symbols or files. Usually means BUILD.gn needs updating (new source files) or an API changed. See [references/breaking-changes.md](references/breaking-changes.md) for guidance.

### 5. Create PRs

**Both PRs must be created together** - CI requires both to exist for validation. All three (issue + both PRs) should be cross-linked.

#### Step 1: Find related issues

Search for existing issues in SkiaSharp related to the dependency update:
```bash
gh search issues --repo mono/SkiaSharp "{dependency name}"
```

#### Step 2: Create mono/skia PR

Create branch and PR in `externals/skia`:
```bash
cd externals/skia
git checkout -b dev/update-{dep}
git add DEPS
git commit -m "Update {dep} to {version}"
git push origin dev/update-{dep}
gh pr create --repo mono/skia --base skiasharp --head dev/update-{dep} \
  --title "Update {dep} to {version}"
```

Fill out the PR template:
- **SkiaSharp Issue:** Link to the issue (e.g., `Related to https://github.com/mono/SkiaSharp/issues/NNNN`)
- **Required SkiaSharp PR:** Leave as placeholder initially, update after creating SkiaSharp PR

#### Step 3: Create SkiaSharp PR

Create branch and PR in main repo:
```bash
cd ../..  # back to SkiaSharp root
git checkout -b dev/update-{dep}
git add cgmanifest.json
git commit -m "Update {dep} to {version}"
git push origin dev/update-{dep}
gh pr create --repo mono/SkiaSharp --base main --head dev/update-{dep} \
  --title "Update {dep} to {version}"
```

Fill out the PR template:
- **Bugs Fixed:** Link with "Fixes" to auto-close (e.g., `- Fixes https://github.com/mono/SkiaSharp/issues/NNNN`)
- **Required skia PR:** Link to mono/skia PR (e.g., `https://github.com/mono/skia/pull/NNN`)

#### Step 4: Update mono/skia PR

Go back and edit the mono/skia PR to add the SkiaSharp PR link:
```bash
gh pr edit {skia-pr-number} --repo mono/skia --body "..."
```

Update **Required SkiaSharp PR** to link to the SkiaSharp PR.

#### Result

All three are now cross-linked:
- Issue → closed by SkiaSharp PR on merge
- mono/skia PR → links to issue and SkiaSharp PR
- SkiaSharp PR → links to issue and mono/skia PR

**Merge order:** Merge mono/skia PR first, then the SkiaSharp PR.

## Common Dependencies

| Dependency | DEPS Key | Notes |
|------------|----------|-------|
| libpng | `third_party/externals/libpng` | PNG images |
| libexpat | `third_party/externals/expat` | XML/SVG parsing |
| zlib | `third_party/externals/zlib` | Compression |
| libwebp | `third_party/externals/libwebp` | WebP images |
| harfbuzz | `third_party/externals/harfbuzz` | Text shaping |
| freetype | `third_party/externals/freetype` | Font rendering |
| libjpeg-turbo | `third_party/externals/libjpeg-turbo` | JPEG images |

Dependencies use Google-hosted mirrors (URLs visible in DEPS file).

## Security Update Checklist

- [ ] Identify CVE numbers and affected dependency
- [ ] Find fixed version from advisory
- [ ] Verify fix is in target commit
- [ ] Check for additional CVEs fixed in newer versions
- [ ] Analyze for breaking changes (usually none)
- [ ] Update DEPS, build, test
- [ ] Document CVEs in PR description
- [ ] Update `cgmanifest.json` for compliance scanning

## References

- [references/breaking-changes.md](references/breaking-changes.md) - Detailed breaking change analysis guidance
- [documentation/building.md](../../../documentation/building.md) - Full build prerequisites and setup
- [documentation/maintaining.md](../../../documentation/maintaining.md) - Maintainer processes and update cadence
- [CONTRIBUTING.md](../../../CONTRIBUTING.md) - PR guidelines and code standards
