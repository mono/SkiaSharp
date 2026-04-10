# Security Audit Transcript

**Date:** 2026-04-10
**Task:** Full security audit of SkiaSharp native dependencies

---

## Step 1: Search Issues & PRs

### mono/SkiaSharp Issues
- **Discussion #3432** (2025-12-18): User asks about CVE-2023-45853 (zlib/MiniZip) in SkiaSharp 2.88.9
- **Issue #2716** (2024-01-15, closed): CVE-2023-6345 reported against SkiaSharp.NativeAssets
- **Issue #2336** (2022-12-09, closed): Blackduck scan flagged old expat, zlib, libjpeg-turbo CVEs
- **PR #3166** (2025-02-14, merged): Addressed CVE-2024-30105 (System.Text.Json, not native dep)

### mono/skia PRs
- GitHub code search returned skill/dependency docs, no open security PRs found

---

## Step 2a: Verify Skia Milestone and Upstream Commit

### Submodule Status
```
$ git submodule status externals/skia
8c99e432ff06e61c42cf99aa8f2cbe248d301b9a externals/skia (v2.88.4-preview.95-17103-g8c99e432ff)
```

### SkMilestone.h
```
#define SK_MILESTONE 132
```

### Merge History
```
$ cd externals/skia && git log --oneline --merges --grep="chrome/m" -5 HEAD
e2260f39c4 Merge pull request #171 from ramezgerges/xamarin-m132
03228c57a8 Merge skiasharp into chrome/m132 with conflict resolution
251a663707 Merge remote-tracking branch 'upstream/chrome/m119' into dev/update-skia-119
bb35d345cc Merge remote-tracking branch 'upstream/chrome/m118' into dev/update-skia-m118
37d8cf1c0a Merge remote-tracking branch 'upstream/chrome/m117' into dev/update-skia-m117
```

### MANDATORY: Add Upstream Remote and Fetch
```
$ git remote add upstream https://github.com/google/skia.git 2>$null
$ git fetch upstream chrome/m132 --depth=1
From https://github.com/google/skia
 * branch chrome/m132 -> FETCH_HEAD

$ git log --format="%H %s" -1 FETCH_HEAD
9ab7c2064b2b1ab22f856a7f0a8c3b3ae4cb89c7 Remove CQ for unsupported branch chrome/m132
```

### MANDATORY: Verify Merge Parents
```
$ git log --format="%H %P" -1 03228c57a8
03228c57a86629dbb76b3e769509fe65786af0b5 9ab7c2064b2b1ab22f856a7f0a8c3b3ae4cb89c7 9065669a615daa6493594ada3255833fe34466b3
```
- Parent 1: `9ab7c2064b2b...` = upstream chrome/m132 tip (matches FETCH_HEAD ✅)
- Parent 2: `9065669a615d...` = SkiaSharp fork branch

### MANDATORY: Verify Upstream Ancestry
```
$ git merge-base --is-ancestor FETCH_HEAD 03228c57a8
VERIFIED: upstream chrome/m132 is ancestor of merge commit
```

### cgmanifest.json Comparison
| Field | Verified | cgmanifest.json | Match |
|-------|----------|-----------------|-------|
| Milestone | 132 (SkMilestone.h) | chrome_milestone: 132 | ✅ |
| Fork commit | 8c99e432ff... | commitHash: 8c99e432ff... | ✅ |
| Upstream commit | 9ab7c2064b2b... (FETCH_HEAD) | upstream_merge_commit: 9ab7c20... | ✅ |

---

## Step 2b: Verify Third-Party Dependency Versions

### DEPS Commit Hashes Extracted
| Dependency | DEPS Commit |
|------------|-------------|
| libpng | `02f2b4f4699f0ef9111a6534f093b53732df4452` |
| zlib | `e432200a7931de1b0b1b50201aac8a46d9380cb2` |
| freetype | `83af801b552111e37d9466a887e1783a0fb5f196` |
| harfbuzz | `2b3631a866b3077d9d675caa4ec9010b342b5a7c` |
| expat | `4575e52f83e0d6d7bd24939eab8952bbc7bc358f` |
| brotli | `028fb5a23661f123017c060daa546b55cf4bde29` |
| libjpeg-turbo | `927aabfcd26897abb9776ecf2a6c38ea5bb52ab6` |
| libwebp | `4fa21912338357f89e4fd51cf2368325b59e9bd9` |

### Version Verification via Googlesource Mirror Headers
Fetched base64-encoded headers from `{host}/{path}/+/{commit}/{file}?format=TEXT` and decoded.

| Dependency | File | Verified Version | cgmanifest.json | Match |
|------------|------|-----------------|-----------------|-------|
| libpng | png.h: "libpng version 1.6.54" | 1.6.54 | 1.6.54 | ✅ |
| zlib | zlib.h: ZLIB_VERSION "1.3.0.1-motley" | 1.3.0.1 | 1.3.0.1 | ✅ |
| freetype | freetype.h: MAJOR=2, MINOR=13, PATCH=3 | 2.13.3 | 2.13.3 | ✅ |
| harfbuzz | hb-version.h: HB_VERSION_STRING "8.3.1" | 8.3.1 | 8.3.1 | ✅ |
| expat | expat.h: XML_*_VERSION 2.7.3 | 2.7.3 | 2.7.3 | ✅ |
| brotli | version.h: BROTLI_VERSION 1.2.0 | 1.2.0 | 1.2.0 | ✅ |
| libjpeg-turbo | README.chromium: Version: 2.1.5.1 | 2.1.5.1 | 2.1.5.1 | ✅ |
| libwebp | NEWS line 1: "version 1.6.0" | 1.6.0 | 1.6.0 | ✅ |

**Result:** Zero mismatches found between verified versions and cgmanifest.json.

---

## Step 3: Query CVE Databases

### Skia Core — NVD API
```
GET https://services.nvd.nist.gov/rest/json/cves/2.0?keywordSearch=Skia&resultsPerPage=200
Total results: 115
Recent (2023+): 21
```

**Classification using verified milestone m132:**
- Potentially affected (fix > m132): 5 CVEs
- Already fixed (fix ≤ m132): 11 CVEs
- No Chrome version (manual review): 5 CVEs

### Third-Party Dependencies — Web Search Results

**libpng:** 3 new CVEs affecting 1.6.54 (fixed in 1.6.55-1.6.56)
- CVE-2026-25646: heap buffer overflow in png_set_quantize (HIGH ~8.3)
- CVE-2026-33636: use-after-free (HIGH)
- CVE-2026-33416: OOB read/write in AArch64 palette (HIGH)

**freetype:** 1 new CVE affecting 2.13.3 (fixed in 2.14.2)
- CVE-2026-23865: integer overflow in tt_var_load_item_variation_store (HIGH)
- CVE-2025-27363 (HIGH 8.1, actively exploited) → FIXED in 2.13.1, we have 2.13.3

**zlib:** 1 CVE potentially affecting 1.3.0.1
- CVE-2026-27171: CPU exhaustion DoS (MEDIUM 2.9), fixed in 1.3.2
- CVE-2026-22184: untgz utility only → FALSE POSITIVE

**libexpat:** 2 CVEs affecting 2.7.3 (fixed in 2.7.4)
- CVE-2026-25210: integer overflow in doContent (MEDIUM)
- CVE-2026-24515: encoding handler user data issue (LOW)

**libwebp:** No new CVEs since 2023. CVE-2023-4863 fixed in 1.3.2; we have 1.6.0.

**libjpeg-turbo:** No new CVEs for core library. CVE-2026-24797 affects cupoch fork only.

**harfbuzz:** CVE-2024-56732 affects 8.5.0-10.0.1; we have 8.3.1 (NOT affected). Also disabled in SkiaSharp.

**brotli:** CVE-2025-6176 fixed in 1.2.0; we have 1.2.0. Clean.

---

## Step 4: Verify Fix Commits

### FreeType CVE-2025-27363
- Claimed affected: ≤ 2.13.0
- Fix: 2.13.1+
- Our version: 2.13.3
- **Status: FIXED** ✅

### Brotli CVE-2025-6176
- Fix: brotli 1.2.0
- Our version: 1.2.0
- **Status: FIXED** ✅

### libwebp CVE-2023-4863
- Fix: libwebp 1.3.2
- Our version: 1.6.0
- **Status: FIXED** ✅

### libexpat CVE-2024-8176, CVE-2025-59375
- Fix: 2.7.0 (improved in 2.7.3)
- Our version: 2.7.3
- **Status: FIXED** ✅

---

## Step 5: Check False Positives

### CVE-2023-45853 (zlib MiniZip)
- MiniZip is in `zlib/contrib/minizip/` but Skia's BUILD.gn excludes it
- No MiniZip includes found in Skia source
- **Status: FALSE POSITIVE** ⚪

### CVE-2026-22184 (zlib untgz utility)
- Affects the `untgz` demo tool, not core zlib
- Not compiled by Skia
- **Status: FALSE POSITIVE** ⚪

### CVE-2025-48630 (SkiaRenderEngine)
- Affects Android's SkiaRenderEngine.cpp
- Not part of the Skia graphics library or SkiaSharp
- **Status: FALSE POSITIVE** ⚪

### CVE-2026-24797 (cupoch libjpeg-turbo)
- Affects cupoch's bundled fork of libjpeg-turbo
- Not the upstream library pinned by Skia
- **Status: FALSE POSITIVE** ⚪

### CVE-2024-56732 (harfbuzz)
- Affects versions 8.5.0-10.0.1
- Our version is 8.3.1 (below vulnerable range)
- HarfBuzz also disabled in SkiaSharp builds
- **Status: NOT AFFECTED** ✅

---

## Step 6: Report Generated

See `audit_report.md` for the full formatted report following report-template.md format.
