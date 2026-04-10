# Security Audit Transcript

**Date:** 2026-04-10
**Task:** Full security audit of SkiaSharp native dependencies
**Skill used:** `security-audit` (from `.github/skills/security-audit/SKILL.md`)

---

## Step 1: Search Issues & PRs

### Tools called:
1. **mihubot-search_dotnet_repos** — Searched for "SkiaSharp CVE security vulnerability" across dotnet repos
2. **GitHub API search** — Searched `repo:mono/SkiaSharp state:open` for CVE/security/vulnerability
3. **GitHub API search** — Searched `repo:mono/skia is:pr state:open` for dependency updates

### Findings:
- **Discussion #3432** (open): User asking about zlib CVE-2023-45853 in SkiaSharp 2.88.9 → False positive (MiniZip not compiled)
- **Issue #2716** (closed): CVE-2023-6345 reported against SkiaSharp → Already fixed in current milestone
- **Issue #2336** (closed): Blackduck scan findings for older versions → Historical, already resolved
- **PR #3166** (merged): Addressed CVE-2024-30105 (System.Text.Json) → Already fixed
- **PR #3656** (open): Fix cgmanifest.json version drift → In progress
- **mono/skia PR #157** (open): Update harfbuzz to 8.4.0 → Pending merge

---

## Step 2a: Verify Skia Milestone

### Tools called:
1. **powershell** — `git submodule status externals/skia` → Got commit `8c99e432ff06e61c42cf99aa8f2cbe248d301b9a`
2. **github-mcp-server-get_file_contents** — Read `include/core/SkMilestone.h` at submodule commit from mono/skia repo
3. **view** — Read `cgmanifest.json` from working tree

### Verification results:
| Field | Source of Truth | Actual Value | cgmanifest.json | Match |
|-------|----------------|-------------|-----------------|-------|
| Milestone | SkMilestone.h | `#define SK_MILESTONE 132` | chrome_milestone: 132 | ✅ |
| Fork commit | git submodule status | 8c99e432ff... | commitHash: 8c99e432ff... | ✅ |
| Upstream commit | cgmanifest.json | 9ab7c2064b... | upstream_merge_commit: 9ab7c2064b... | ✅ (not independently verified - would require `git fetch upstream`) |

**Note:** The upstream_merge_commit verification would require adding the google/skia remote and fetching chrome/m132, which was not performed in this read-only audit to avoid modifying the repo state.

---

## Step 2b: Verify Third-Party Dependency Versions

### Tools called:
1. **github-mcp-server-get_file_contents** — Read `DEPS` file at submodule commit `8c99e432ff...` from mono/skia
2. **web_fetch** (×8) — Fetched version headers from googlesource.com mirrors at DEPS commit hashes:
   - `https://skia.googlesource.com/third_party/libpng/+/{sha}/png.h?format=TEXT`
   - `https://chromium.googlesource.com/.../zlib/+/{sha}/zlib.h?format=TEXT`
   - `https://chromium.googlesource.com/.../freetype2/+/{sha}/include/freetype/freetype.h?format=TEXT`
   - `https://chromium.googlesource.com/.../harfbuzz/+/{sha}/src/hb-version.h?format=TEXT`
   - `https://chromium.googlesource.com/.../libexpat/+/{sha}/expat/lib/expat.h?format=TEXT`
   - `https://skia.googlesource.com/.../brotli/+/{sha}/c/common/version.h?format=TEXT`
   - `https://chromium.googlesource.com/.../libjpeg_turbo/+/{sha}/README.chromium?format=TEXT`
   - `https://chromium.googlesource.com/.../libwebp/+/{sha}/NEWS?format=TEXT`
3. **powershell** (×3) — Decoded base64 responses and extracted version strings

### DEPS commit hashes extracted:
| Dependency | DEPS Commit Hash | Source URL |
|-----------|-----------------|------------|
| libpng | 02f2b4f4699f0ef9111a6534f093b53732df4452 | skia.googlesource.com/third_party/libpng |
| zlib | e432200a7931de1b0b1b50201aac8a46d9380cb2 | chromium.googlesource.com/.../zlib |
| freetype | 83af801b552111e37d9466a887e1783a0fb5f196 | chromium.googlesource.com/.../freetype2 |
| harfbuzz | 2b3631a866b3077d9d675caa4ec9010b342b5a7c | chromium.googlesource.com/.../harfbuzz |
| expat | 4575e52f83e0d6d7bd24939eab8952bbc7bc358f | chromium.googlesource.com/.../libexpat |
| brotli | 028fb5a23661f123017c060daa546b55cf4bde29 | skia.googlesource.com/.../brotli |
| libjpeg-turbo | 927aabfcd26897abb9776ecf2a6c38ea5bb52ab6 | chromium.googlesource.com/.../libjpeg_turbo |
| libwebp | 4fa21912338357f89e4fd51cf2368325b59e9bd9 | chromium.googlesource.com/.../libwebp |

### Verified versions (from header files):
| Dependency | Version String Found | Method |
|-----------|---------------------|--------|
| libpng | `libpng version 1.6.54` | png.h line 3 |
| zlib | `ZLIB_VERSION "1.3.0.1-motley"` | zlib.h #define |
| freetype | `FREETYPE_MAJOR 2`, `FREETYPE_MINOR 13`, `FREETYPE_PATCH 3` | freetype.h #defines |
| harfbuzz | `HB_VERSION_MAJOR 8`, `HB_VERSION_MINOR 3`, `HB_VERSION_MICRO 1` | hb-version.h #defines |
| libexpat | `XML_MAJOR_VERSION 2`, `XML_MINOR_VERSION 7`, `XML_MICRO_VERSION 3` | expat.h #defines |
| brotli | `BROTLI_VERSION_MAJOR 1`, `BROTLI_VERSION_MINOR 2`, `BROTLI_VERSION_PATCH 0` | version.h #defines |
| libjpeg-turbo | `Version: 2.1.5.1` | README.chromium |
| libwebp | `6/30/2025 version 1.6.0` | NEWS line 1 |

**All versions match cgmanifest.json. No mismatches detected.**

---

## Step 3: Query CVE Databases

### Skia Core CVEs

**Tool:** powershell `Invoke-RestMethod` to NVD API
- URL: `https://services.nvd.nist.gov/rest/json/cves/2.0?keywordSearch=Skia&resultsPerPage=200`
- Total results: 115
- CVEs since 2023: 21
- Classification using verified milestone m132:
  - Potentially affected (fix > m132): 5 CVEs
  - Already fixed (fix ≤ m132): 11 CVEs
  - Manual review needed: 4 CVEs
  - Not applicable (Android RenderEngine): 1 CVE

### Third-Party Dependency CVEs

**Tool:** web_search (×6) for each dependency
- `libpng CVE 2024 2025 2026` → Found CVE-2026-25646, CVE-2026-33416, CVE-2026-33636
- `zlib CVE 2024 2025 2026` → Found CVE-2026-27171, CVE-2026-22184 (contrib only)
- `freetype CVE 2024 2025 2026` → Found CVE-2025-27363 (already fixed in 2.13.1), CVE-2026-23865
- `harfbuzz CVE 2024 2025 2026` → Found CVE-2024-56732 (not affected, < 8.5.0), CVE-2026-22693
- `libexpat CVE 2024 2025 2026` → Found CVE-2026-25210, CVE-2026-24515 (older CVEs already fixed)
- `libjpeg-turbo CVE 2024 2025 2026` → Found CVE-2023-2804 (12-bit JPEG)
- `libwebp CVE 2024 2025` → No new CVEs since CVE-2023-4863 (already fixed)
- `brotli CVE 2024 2025 2026` → Found CVE-2025-6176 (fixed in 1.2.0 = our version)

---

## Step 4: Verify Fix Commits

Fix commit verification was performed where possible:

| CVE | Method | Result |
|-----|--------|--------|
| CVE-2025-27363 (FreeType) | Version comparison: fix in 2.13.1, our version 2.13.3 | ✅ Already fixed |
| CVE-2024-56732 (HarfBuzz) | Version range: affects 8.5.0–10.0.1, our version 8.3.1 | ✅ Not affected |
| CVE-2025-6176 (Brotli) | Version comparison: fix in 1.2.0, our version 1.2.0 | ✅ Fixed |
| CVE-2023-4863 (libwebp) | Version comparison: fix in 1.3.2, our version 1.6.0 | ✅ Fixed |
| CVE-2024-8176 (expat) | Version comparison: fix in 2.7.0, our version 2.7.3 | ✅ Fixed |
| Skia CVEs | Milestone comparison: m132 vs fix milestones | See report |

**Note:** `git merge-base --is-ancestor` was not used for individual commit verification as the third-party submodules are not initialized in this worktree. Version-based comparison was used instead.

---

## Step 5: Check False Positives

### Verified false positives:

1. **CVE-2023-45853 (zlib/MiniZip)** — MiniZip is in `zlib/contrib/minizip/` but Skia's `BUILD.gn` excludes it. The vulnerable code (`unzip.c`, `zip.c`, `ioapi.c`) is NOT compiled or linked.

2. **CVE-2025-48630 (SkiaRenderEngine)** — Affects `SkiaRenderEngine.cpp` in Android's graphics framework (`platform/frameworks`). This is Android OS infrastructure, not part of Skia core or SkiaSharp.

3. **CVE-2024-43768 (SkDeflate.cpp Android)** — References `platform/frameworks` code path. Not part of the Skia library as consumed by SkiaSharp.

4. **CVE-2026-22184 (zlib contrib/untgz)** — Affects the `untgz` utility in `contrib/`. Not compiled by Skia.

### Notes on potential false positives needing more investigation:

- **CVE-2025-32318, CVE-2025-54627** — Skia CVEs without Chrome milestone info. Need manual review of whether the affected code paths are reachable through SkiaSharp's C API surface.

---

## Step 6: Generate Report

Report generated using the template from `.github/skills/security-audit/references/report-template.md`.

**Sorting applied:**
1. 🔴 Needs attention (skia, libpng, zlib, freetype, libexpat) — sorted by severity within priority
2. 🆕 Undiscovered (harfbuzz, libjpeg-turbo)
3. ⚪ False positives (zlib/MiniZip, SkiaRenderEngine, Android SkDeflate)
4. ✅ Clean (libwebp, brotli, wuffs, dng_sdk, GPU deps)

**Output saved to:** `audit_report.md`

---

## Summary of Tool Usage

| Tool | Count | Purpose |
|------|-------|---------|
| powershell | 6 | Git submodule status, NVD API query, base64 decoding, GitHub API searches |
| github-mcp-server-get_file_contents | 2 | Read SkMilestone.h and DEPS from mono/skia at submodule commit |
| web_fetch | 8 | Fetch version headers from googlesource.com mirrors |
| web_search | 6 | Search CVE databases for each dependency |
| mihubot-search_dotnet_repos | 1 | Search SkiaSharp issues for CVE/security keywords |
| github-mcp-server-search_code | 2 | Search for CVE references in repo code |
| view | 3 | Read SKILL.md, report-template.md, dependencies.md, cgmanifest.json |
