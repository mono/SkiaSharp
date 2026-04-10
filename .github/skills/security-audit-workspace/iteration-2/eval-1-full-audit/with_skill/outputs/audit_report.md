# Security Audit Report

**Date:** 2026-04-10
**Audited:** mono/SkiaSharp native dependencies (including Skia core)

## Summary

| Status | Count |
|--------|-------|
| 🔴 Needs attention | 4 |
| 🆕 Undiscovered | 4 |
| ⚪ False positive | 4 |
| ✅ Already fixed | 8 |

## Verification Summary

### Skia Submodule Verification

| Field | Source of Truth | Verified Value | cgmanifest.json | Match? |
|-------|----------------|----------------|-----------------|--------|
| Submodule commit | `git submodule status` | `8c99e432ff06e61c42cf99aa8f2cbe248d301b9a` | `8c99e432ff06e61c42cf99aa8f2cbe248d301b9a` | ✅ |
| Milestone | `SkMilestone.h` | 132 (`#define SK_MILESTONE 132`) | `chrome_milestone: 132` | ✅ |
| Upstream merge commit | `git fetch upstream chrome/m132` → FETCH_HEAD | `9ab7c2064b2b1ab22f856a7f0a8c3b3ae4cb89c7` | `upstream_merge_commit: 9ab7c20...` | ✅ |
| Upstream ancestry | `git merge-base --is-ancestor FETCH_HEAD 03228c57a8` | **VERIFIED** | N/A | ✅ |

**Upstream verification detail:**
- Merge commit `03228c57a8` ("Merge skiasharp into chrome/m132 with conflict resolution") has parents:
  - `9ab7c2064b2b` (upstream chrome/m132 tip) — independently fetched from `google/skia`
  - `9065669a615d` (SkiaSharp fork commits)
- `git merge-base --is-ancestor FETCH_HEAD 03228c57a8` → **VERIFIED**

### Third-Party Dependency Version Verification

All versions verified from actual DEPS commit hashes against Chromium mirror header files.

| Dependency | DEPS Commit | Verified Version (from header) | cgmanifest.json | Match? |
|------------|-------------|-------------------------------|-----------------|--------|
| libpng | `02f2b4f4...` | 1.6.54 (png.h) | 1.6.54 | ✅ |
| zlib | `e432200a...` | 1.3.0.1-motley (zlib.h) | 1.3.0.1 | ✅ |
| freetype | `83af801b...` | 2.13.3 (freetype.h) | 2.13.3 | ✅ |
| harfbuzz | `2b3631a8...` | 8.3.1 (hb-version.h) | 8.3.1 | ✅ |
| libexpat | `4575e52f...` | 2.7.3 (expat.h) | 2.7.3 | ✅ |
| brotli | `028fb5a2...` | 1.2.0 (version.h) | 1.2.0 | ✅ |
| libjpeg-turbo | `927aabfc...` | 2.1.5.1 (README.chromium) | 2.1.5.1 | ✅ |
| libwebp | `4fa21912...` | 1.6.0 (NEWS) | 1.6.0 | ✅ |

**No mismatches found.** All cgmanifest.json entries match the actual verified versions.

---

## Detailed Findings

### 1. 🔴 skia — Needs attention

| Field | Value |
|-------|-------|
| Issues | #2716 (closed), Discussion #3432 |
| CVEs | 5 potentially affected, 5 manual review needed |
| Current | chrome/m132 (verified from SkMilestone.h + upstream) |
| Min fix | m133 (for CVE-2025-0444), m146 (for all HIGH CVEs) |
| NVD query | `keywordSearch=Skia` — 115 total, 21 recent (2023+), 5 affected, 11 fixed, 5 no-version |

**Chrome-versioned CVEs (fix milestone > m132):**

| CVE | Fixed In | Severity | Description |
|-----|----------|----------|-------------|
| CVE-2026-3538 | m145 | HIGH 8.8 | Integer overflow in Skia |
| CVE-2026-3931 | m146 | HIGH 8.8 | Heap buffer overflow in Skia |
| CVE-2026-3909 | m146 | HIGH 8.8 | Out of bounds write in Skia |
| CVE-2026-4460 | m146 | HIGH 8.8 | Out of bounds read in Skia |
| CVE-2025-0444 | m133 | MEDIUM 6.3 | Use after free in Skia |

**Non-Chrome CVEs (manual review needed):**

| CVE | Severity | Description | Assessment |
|-----|----------|-------------|------------|
| CVE-2026-5870 | N/A (new) | Integer overflow in Skia (Chrome prior to 147.0.7727.55) | 🔴 Affected (m147, NVD not yet enriched with CPE) |
| CVE-2025-32318 | HIGH 8.8 | Out of bounds write due to heap buffer overflow | ⚠️ Needs review — check if code path reachable via C API |
| CVE-2025-54627 | HIGH 8.8 | Out-of-bounds write in skia module | ⚠️ Needs review — may be Android-specific |
| CVE-2024-43768 | HIGH 7.8 | SkDeflate.cpp out of bounds write due to integer overflow | ⚠️ Needs review — deflate code is used by SkiaSharp |
| CVE-2025-48630 | HIGH 7.4 | SkiaRenderEngine.cpp GPU cache access | ⚪ Android-only — Not applicable to SkiaSharp |

**Action:** Merge newer upstream milestone (minimum m133 for use-after-free fix, m146+ for all HIGH fixes). CVE-2026-5870 requires m147.

---

### 2. 🔴 libpng — Needs attention

| Field | Value |
|-------|-------|
| Issues | None open |
| CVEs | CVE-2026-25646 (HIGH ~8.3), CVE-2026-33636 (HIGH), CVE-2026-33416 (HIGH) |
| Current | 1.6.54 (verified) |
| Min fix | 1.6.55 (for CVE-2026-25646), 1.6.56 (for all) |
| Latest | 1.6.56 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fixed In | Description |
|-----|----------|----------|-------------|
| CVE-2026-25646 | HIGH ~8.3 | 1.6.55 | Heap buffer overflow in png_set_quantize() — 30-year-old code |
| CVE-2026-33636 | HIGH | 1.6.56 | Use-after-free in palette/transparency expansion |
| CVE-2026-33416 | HIGH | 1.6.56 | Out-of-bounds read/write in AArch64 palette routines |

**Already fixed in 1.6.54:** CVE-2026-22801, CVE-2026-22695 (integer truncation, heap buffer over-read)

**Action:** Create issue and PR to update libpng to 1.6.56

---

### 3. 🔴 freetype — Needs attention

| Field | Value |
|-------|-------|
| Issues | None open |
| CVEs | CVE-2026-23865 (HIGH) |
| Current | 2.13.3 (verified) |
| Min fix | 2.14.2 |
| Latest | 2.14.2 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fixed In | Description |
|-----|----------|----------|-------------|
| CVE-2026-23865 | HIGH | 2.14.2 | Integer overflow in tt_var_load_item_variation_store with OpenType variable fonts |

**Already fixed:** CVE-2025-27363 (HIGH 8.1, actively exploited in the wild) — fixed in 2.13.1, we have 2.13.3

**Action:** Create issue and PR to update freetype to 2.14.2

---

### 4. 🆕 zlib — Undiscovered CVE

| Field | Value |
|-------|-------|
| Issues | Discussion #3432 (MiniZip question) |
| CVEs | CVE-2026-27171 (MEDIUM ~2.9) |
| Current | 1.3.0.1-motley (verified, Chromium fork) |
| Min fix | 1.3.2 |
| Latest | 1.3.2 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fixed In | Description |
|-----|----------|----------|-------------|
| CVE-2026-27171 | MEDIUM 2.9 | 1.3.2 | CPU exhaustion via crc32_combine64 infinite loop (DoS) |

**Note:** Skia uses Chromium's zlib fork ("motley" variant). The fix applicability should be verified against Chromium's fork.

**Action:** Create issue, investigate if Chromium's fork is affected

---

### 5. 🆕 libexpat — Undiscovered CVEs

| Field | Value |
|-------|-------|
| Issues | None open |
| CVEs | CVE-2026-24515 (LOW), CVE-2026-25210 (MEDIUM) |
| Current | 2.7.3 (verified) |
| Min fix | 2.7.4 |
| Latest | 2.7.4 |
| PR | None |

**CVE Details:**

| CVE | Severity | Fixed In | Description |
|-----|----------|----------|-------------|
| CVE-2026-25210 | MEDIUM | 2.7.4 | Integer overflow in doContent buffer size check |
| CVE-2026-24515 | LOW | 2.7.4 | Improper copying of encoding handler user data |

**Already fixed in 2.7.3:** CVE-2024-8176 (HIGH 7.5, stack overflow), CVE-2025-59375 (unrestricted memory allocation)

**Action:** Create issue and PR to update expat to 2.7.4

---

### 6. ✅ brotli — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None affecting current version |
| Current | 1.2.0 (verified) |
| Status | Up to date |

**Already fixed:** CVE-2025-6176 (HIGH 7.5, decompression bomb DoS) — fixed in 1.2.0, we have 1.2.0

**Action:** None required

---

### 7. ✅ libwebp — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None since 2023 |
| Current | 1.6.0 (verified) |
| Status | Up to date |

**Already fixed:** CVE-2023-4863 (CRITICAL, heap buffer overflow) — fixed in 1.3.2, we have 1.6.0

**Action:** None required

---

### 8. ✅ libjpeg-turbo — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None affecting our version |
| Current | 2.1.5.1 (verified) |
| Status | No new CVEs for core library in 2024-2026 |

**Note:** CVE-2026-24797 affects cupoch's bundled fork, not the upstream libjpeg-turbo library.

**Action:** None required

---

### 9. ✅ harfbuzz — Clean

| Field | Value |
|-------|-------|
| Issues | None |
| CVEs | None affecting current version |
| Current | 8.3.1 (verified) |
| Status | Clean |

**Not affected:** CVE-2024-56732 (affects 8.5.0-10.0.1, we have 8.3.1 — below vulnerable range)

**Note:** HarfBuzz is disabled in SkiaSharp builds, further reducing exposure.

**Action:** None required

---

### 10. ⚪ zlib (MiniZip) — False positive

| Field | Value |
|-------|-------|
| Issue | Discussion #3432 |
| CVE | CVE-2023-45853 |
| Status | Not vulnerable |

**Reason:** CVE affects MiniZip only (`zlib/contrib/minizip/`). Skia's BUILD.gn excludes MiniZip sources. `grep -r "minizip\|unzip\.h" externals/skia/src/` returns nothing.

**Action:** Respond to discussion #3432 explaining MiniZip is not compiled or linked

---

### 11. ⚪ zlib (untgz) — False positive

| Field | Value |
|-------|-------|
| Issue | None |
| CVE | CVE-2026-22184 |
| Status | Not vulnerable |

**Reason:** CVE affects the `untgz` demonstration utility, not the core zlib compression library. This tool is not compiled by Skia.

**Action:** None required

---

### 12. ⚪ skia (SkiaRenderEngine) — False positive

| Field | Value |
|-------|-------|
| Issue | None |
| CVE | CVE-2025-48630 |
| Status | Not applicable |

**Reason:** CVE affects `SkiaRenderEngine.cpp` which is part of Android's platform rendering engine, not the Skia graphics library used by SkiaSharp.

**Action:** None required

---

### 13. ⚪ libjpeg-turbo (cupoch) — False positive

| Field | Value |
|-------|-------|
| Issue | None |
| CVE | CVE-2026-24797 |
| Status | Not applicable |

**Reason:** CVE affects cupoch's bundled fork of libjpeg-turbo (tjbench.C), not the upstream library used by Skia.

**Action:** None required

---

## Skia CVE Milestone Summary

| CVE | Fix Milestone | Our Milestone | Gap | Severity |
|-----|--------------|---------------|-----|----------|
| CVE-2025-0444 | m133 | m132 | 1 | MEDIUM 6.3 |
| CVE-2026-3538 | m145 | m132 | 13 | HIGH 8.8 |
| CVE-2026-3931 | m146 | m132 | 14 | HIGH 8.8 |
| CVE-2026-3909 | m146 | m132 | 14 | HIGH 8.8 |
| CVE-2026-4460 | m146 | m132 | 14 | HIGH 8.8 |
| CVE-2026-5870 | m147* | m132 | 15 | N/A (new) |

*CVE-2026-5870 not yet enriched with CPE data in NVD; milestone inferred from description ("Chrome prior to 147.0.7727.55")

---

## Next Steps

1. **🔴 Merge upstream Skia m146+** — Fixes 4 HIGH Skia CVEs (CVE-2026-3538, CVE-2026-3931, CVE-2026-3909, CVE-2026-4460) plus 1 MEDIUM (CVE-2025-0444). Ideally merge m147 to also cover CVE-2026-5870. This is the highest-priority action.

2. **🔴 Update libpng to 1.6.56** — Fixes CVE-2026-25646 (heap buffer overflow in png_set_quantize, HIGH), CVE-2026-33636 and CVE-2026-33416. Run: `bump libpng to 1.6.56`

3. **🔴 Update freetype to 2.14.2** — Fixes CVE-2026-23865 (integer overflow with OpenType variable fonts, HIGH). Run: `bump freetype to 2.14.2`

4. **🆕 Update libexpat to 2.7.4** — Fixes CVE-2026-25210 (MEDIUM) and CVE-2026-24515 (LOW). Run: `bump expat to 2.7.4`

5. **🆕 Investigate zlib CVE-2026-27171** — CPU exhaustion DoS (MEDIUM 2.9). Verify if Chromium's zlib fork is affected before updating.

6. **⚠️ Review non-Chrome Skia CVEs** — CVE-2024-43768 (SkDeflate.cpp), CVE-2025-32318, CVE-2025-54627 need manual assessment of reachability through SkiaSharp's C API.

7. **⚪ Respond to Discussion #3432** — Explain that CVE-2023-45853 (MiniZip) does not affect SkiaSharp since MiniZip is not compiled or linked.
